using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Device.DT2000AS.API;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.DT2000AS_E;

public partial class Dt2000AsE : DeviceBase
{
    private CancellationTokenSource _heartBeatCts;

    /// <summary>
    ///     心跳线程
    /// </summary>
    private Task _heartBeatTask;

    private CancellationTokenSource _readDataCts;
    private Task _readDataTask;

    public Dt2000AsE(Guid deviceId) : base(deviceId)
    {
    }

    public override bool Initialized(ModuleInfo device)
    {
        var ret = base.Initialized(device);
        if (ret)
        {
            var version = Rx3GInterface.RX3GInit();
            Rx3GInterface.ConfigIp(Ip);
            var deviceNum = Rx3GInterface.GetDeviceNum();
            ret = deviceNum != -1;
            if (!ret) throw new Exception("板卡未连接");
            deviceNum &= 0xffff;
            Trace.WriteLine($"设备{deviceNum}已连接，上位机动态库日期20{version & 0xffffff:X}");
            Rx3GInterface.SetDeviceGain(50);
            _heartBeatCts = new CancellationTokenSource();
            _heartBeatTask = new Task(HeartBeat, _heartBeatCts.Token);
            _heartBeatTask.Start();
        }

        return ret;
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        Rx3GInterface.ResetScanList();
        Thread.Sleep(50);
        Rx3GInterface.ScanFreqConfig(0, GetChar(Gsm), GetChar(Cdma1X), GetChar(Evdo), GetChar(Wcdma),
            GetChar(TdScdma), GetChar(Lte), GetChar(Lte), GetChar(Nr, true), GetChar(GlobalBand));
        if (Nr)
        {
            Add_5GScan_FreqBand((long)758e6, (long)798e6); // n28  广电700M 40M
            Add_5GScan_FreqBand((long)2110e6, (long)2170e6); // n1  电信联通
            Add_5GScan_FreqBand((long)2515e6, (long)2615e6); // n41 移动
            Add_5GScan_FreqBand((long)3400e6, (long)3600e6); // n78 电信联通
            Add_5GScan_FreqBand((long)4800e6, (long)4900e6); // n79   移动室内覆盖
        }

        Rx3GInterface.StartScan();
        _readDataCts = new CancellationTokenSource();
        _readDataTask = new Task(ProcessData, _readDataCts.Token);
        _readDataTask.Start();
    }

    public override void Stop()
    {
        base.Stop();
        Utils.CancelTask(_readDataTask, _readDataCts);
        Thread.Sleep(200);
        Rx3GInterface.StopScan();
    }

    /// <summary>
    ///     释放资源
    /// </summary>
    public override void Dispose()
    {
        Thread.Sleep(200);
        Utils.CancelTask(_readDataTask, _readDataCts);
        Utils.CancelTask(_heartBeatTask, _heartBeatCts);
        try
        {
            Rx3GInterface.CloseDevice();
        }
        catch (Exception)
        {
        }

        base.Dispose();
    }

    private void HeartBeat()
    {
        while (_heartBeatCts?.IsCancellationRequested == false)
            try
            {
                var b = IsSocketConnected();
                if (!b)
                {
                    var info = new SDataMessage
                    {
                        LogType = LogType.Warning,
                        ErrorCode = (int)InternalMessageType.DeviceRestart,
                        Description = DeviceId.ToString(),
                        Detail = DeviceInfo.DisplayName
                    };
                    SendMessage(info);
                    break;
                }

                Thread.Sleep(1000);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
            }
    }

    private bool IsSocketConnected(int maxRetry = 3)
    {
        if (maxRetry == 0) return false;
        try
        {
            using var ping = new Ping();
            var reply = ping.Send(IPAddress.Parse(Ip));
            if (reply is { Status: IPStatus.Success }) return true;
        }
        catch (PingException)
        {
            // 理论上此处不会抛出任何异常
            // 异常:
            // T:System.Net.NetworkInformation.NetworkInformationException: Win32 函数 GetTcpTable 失败。
        }

        Thread.Sleep(1000);
        return IsSocketConnected(--maxRetry);
    }

    private char GetChar(bool b, bool isNr = false)
    {
        if (isNr)
            return b ? NrBlindScan ? (char)2 : (char)1 : (char)0;
        return b ? (char)1 : (char)0;
    }

    private static void Add_5GScan_FreqBand(long lowf, long upperf)
    {
        Rx3GInterface.Add5GScanFreqBand(lowf, upperf);
    }
}