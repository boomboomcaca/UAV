using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Device.AV3900A.Common;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.AV3900A;

public partial class Av3900A
{
    public const double MaxSampleRateHz = 28e6;
    public const double MaxDemodSampleRateHz = 875e3;
    public const int MaxDecimations = 15;
    public const double MaxSpanHz = 20e6;
    public const uint NumFftPoints = 1024;
    public const double SampleRateToSpanRatio = 1.28;
    private static UIntPtr _sensorHandle;
    private static UIntPtr _measHandle;
    private static UIntPtr _demodHandle;
    private readonly object _opLocker = new();
    private Task _receiveGpsTask;
    private CancellationTokenSource _receiveGpsTokenSource;

    private bool InitResources()
    {
        lock (_opLocker)
        {
            _sensorHandle = UIntPtr.Zero;
            var error = Driver.ConnectSensor(ref _sensorHandle, UIntPtr.Zero, Ip, " ", 0);
            if (error != SalErrorType.SalErrNone)
            {
                Console.WriteLine("设备AV3900A连接失败");
                return false;
            }

            Console.WriteLine("设备AV3900A连接成功");
            Driver.AbortAll(_sensorHandle);
            if (UseGps)
            {
                _receiveGpsTokenSource = new CancellationTokenSource();
                _receiveGpsTask = new Task(ReceiveGps, _receiveGpsTokenSource.Token);
                _receiveGpsTask.Start();
            }
        }

        return true;
    }

    private void ReceiveGps()
    {
        while (_receiveGpsTokenSource is { IsCancellationRequested: false })
        {
            if (_sensorHandle == UIntPtr.Zero) break;
            Thread.Sleep(1);
            uint timeAlarms = 0;
            var error = Driver.GetGPSStatus(_sensorHandle, ref timeAlarms);
            if (error != SalErrorType.SalErrNone)
            {
                Thread.Sleep(3000);
                continue;
            }

            var gpsStatusInfo = new GpsStatusInfo(timeAlarms);
            if (!gpsStatusInfo.IsGpsValid(out var warnings))
            {
                Console.WriteLine($"GPS不可用，原因：{warnings}");
                Thread.Sleep(3000);
                continue;
            }

            var location = new SalLocation();
            error = Driver.GetSensorLocation(_sensorHandle, ref location);
            if (error != SalErrorType.SalErrNone)
            {
                Thread.Sleep(3000);
                continue;
            }

            var lng = location.Longitude / Math.PI * 180;
            var lat = location.Latitude / Math.PI * 180;
            var dataGps = new SDataGps
            {
                Longitude = lng,
                Latitude = lat,
                Altitude = (float)location.Altitude
            };
            SendMessageData(new List<object> { dataGps });
            Thread.Sleep(3000);
        }
    }

    private void ReleaseResources()
    {
        lock (_opLocker)
        {
            if (UseGps) Utils.CancelTask(_receiveGpsTask, _receiveGpsTokenSource);
            if (_sensorHandle == UIntPtr.Zero) return;
            Driver.Close(_sensorHandle);
            _sensorHandle = UIntPtr.Zero;
        }
    }

    private void StartTask(FeatureType feature)
    {
        lock (_opLocker)
        {
            Driver.AbortAll(_sensorHandle);
            bool success;
            ClearCache();
            switch (feature)
            {
                case FeatureType.FFM:
                    success = StartFfmTask();
                    break;
                case FeatureType.SCAN:
                    success = StartScanTask();
                    break;
                case FeatureType.TDOA:
                    success = StartTdoaTask();
                    break;
                default:
                    throw new Exception("当前仪器不支持该功能");
            }

            if (!success)
            {
                _demodHandle = UIntPtr.Zero;
                _measHandle = UIntPtr.Zero;
            }
        }
    }

    private void StopTask()
    {
        lock (_opLocker)
        {
            Utils.CancelTask(_processDemodDataTask, _processDemodDataTokenSource);
            Utils.CancelTask(_processDataTask, _processDataTokenSource);
            Utils.CancelTask(_receiveDemodDataTask, _receiveDemodDataTokenSource);
            Utils.CancelTask(_receiveDataTask, _receiveDataTokenSource);
            switch (CurFeature)
            {
                case FeatureType.FFM:
                    if (IqSwitch)
                    {
                        StopTimeSweep();
                    }
                    else
                    {
                        StopSweep();
                        if (AudioSwitch) StopDemod();
                    }

                    break;
                case FeatureType.TDOA:
                    StopTimeSweep();
                    break;
                default:
                    StopSweep();
                    break;
            }

            ClearCache();
            if (_measHandle != UIntPtr.Zero)
            {
                Driver.Close(_measHandle);
                _measHandle = UIntPtr.Zero;
            }

            if (_demodHandle != UIntPtr.Zero)
            {
                Driver.Close(_demodHandle);
                _demodHandle = UIntPtr.Zero;
            }

            Driver.AbortAll(_sensorHandle);
            Driver.ForceSmsGarbageCollection(_sensorHandle);
        }
    }

    private void StopSweep()
    {
        if (_measHandle == UIntPtr.Zero) return;
        Driver.SendSweepCommand(_measHandle, SalSweepCommand.Abort);
    }

    private void StopTimeSweep()
    {
        if (_measHandle == UIntPtr.Zero) return;
        Driver.SendTimeDataCommand(_measHandle, SalTimeDataCmd.TimeDataCmdAbort);
    }

    private void StopDemod()
    {
        if (_demodHandle == UIntPtr.Zero) return;
        Driver.SendDemodCommand(_demodHandle, SalDemodCmd.DemodCmdAbort);
    }

    private void ClearCache()
    {
        _dataCache.Clear();
        _audioDataCache.Clear();
        _iqDataCache.Clear();
    }

    /// <summary>
    ///     获取天线类型
    /// </summary>
    /// <returns></returns>
    private SalAntennaType GetAntennaType()
    {
        if (!Enum.IsDefined(typeof(SalAntennaType), MonitorAntenna)) return SalAntennaType.Antenna1;
        return Enum.Parse<SalAntennaType>(MonitorAntenna.ToString());
    }

    /// <summary>
    ///     获取平均方式
    /// </summary>
    /// <returns></returns>
    private SalAverageType GetAverageType()
    {
        switch (Detector)
        {
            case DetectMode.Rms:
                return SalAverageType.Rms;
            case DetectMode.Pos:
                return SalAverageType.Peak;
            case DetectMode.Avg:
                return SalAverageType.Unknown;
            case DetectMode.Fast:
            default:
                return SalAverageType.Off;
        }
    }

    /// <summary>
    ///     获取解调模式
    /// </summary>
    /// <returns></returns>
    private SalDemodulation GetDemodType()
    {
        switch (DemMode)
        {
            case Modulation.Fm:
                return SalDemodulation.Fm;
            case Modulation.Am:
                return SalDemodulation.Am;
            default:
                return SalDemodulation.None;
        }
    }

    /// <summary>
    ///     根据测量时间推算平均数量
    /// </summary>
    /// <param name="detectMode">检波方式</param>
    /// <param name="numFftPoints">FFT点数</param>
    /// <param name="sampleRate">采样率</param>
    /// <param name="measureTime">测量时间</param>
    /// <returns>平均数量</returns>
    private uint GetAverageNum(DetectMode detectMode, uint numFftPoints, double sampleRate, int measureTime)
    {
        if (detectMode == DetectMode.Fast) return 0;
        var tAverage = measureTime / 1000d;
        var numAverages = (int)Math.Ceiling(tAverage * sampleRate / numFftPoints * 2) - 1;
        return (uint)numAverages;
    }

    /// <summary>
    ///     返回本系统标准时间戳
    /// </summary>
    /// <param name="timestampSeconds">Integer part of the timestamp (in UTC seconds since January 1, 1970).</param>
    /// <param name="timestampNSeconds">Fractional part of the timestamp (in Nanoseconds). </param>
    /// <returns>时间戳，单位为ns</returns>
    private ulong GetTimestamp(uint timestampSeconds, uint timestampNSeconds)
    {
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var temp = dateTime.AddSeconds(timestampSeconds + timestampNSeconds / 1e9);
        return Utils.GetTimestamp(temp);
    }
}