using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Device.AV3900A.Common;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.AV3900A;

public partial class Av3900A
{
    private const uint NumTransferSamples = 1024;
    private readonly ConcurrentQueue<AudioSegmentData> _audioDataCache = new();
    private readonly ConcurrentQueue<SpectrumSegmentData> _dataCache = new();
    private readonly ConcurrentQueue<IqSegmentData> _iqDataCache = new();
    private readonly List<SegmentInfo> _segmentInfos = new();
    private bool _iqTrigger = true;
    private Task _processDataTask;
    private CancellationTokenSource _processDataTokenSource;
    private Task _processDemodDataTask;
    private CancellationTokenSource _processDemodDataTokenSource;
    private Task _receiveDataTask;
    private CancellationTokenSource _receiveDataTokenSource;
    private Task _receiveDemodDataTask;
    private CancellationTokenSource _receiveDemodDataTokenSource;

    #region 单频测量

    private bool StartFfmTask()
    {
        _segmentInfos.Clear();
        var error = SalErrorType.SalErrNone;
        var times = (int)Math.Log2(MaxSpanHz / (IfBandwidth * 1e3));
        times = Math.Min(MaxDecimations, times);
        times = Math.Max(0, times);
        var sampleRate = Math.Floor(MaxSampleRateHz / Math.Pow(2, times));
        if (IqSwitch)
        {
            var tunerParams = new SalTunerParams
            {
                CenterFrequency = Frequency * 1e6,
                SampleRate = sampleRate,
                Antenna = GetAntennaType(),
                Preamp = Preamp,
                Attenuation = Attenuation
            };
            error = Driver.SetTuner(_sensorHandle, ref tunerParams);
            if (error != SalErrorType.SalErrNone)
            {
                Trace.WriteLine("设置解调电路参数失败");
                return false;
            }

            var timeDataParams = new SalTimeDataParms
            {
                CenterFrequency = Frequency * 1e6,
                SampleRate = sampleRate,
                DataType = 1,
                NumSamples = NumTransferSamples,
                NumTransferSamples = NumTransferSamples
            };
            var triggerParams = new SalTimeTrigParms
            {
                TimeTriggerType = SalTimeTrigType.TimeTrigReltime,
                TriggerTimeSecs = 0,
                TriggerTimeNSecs = (uint)(40 * 1e6),
                TriggerCount = uint.MaxValue
            };
            error = Driver.RequestTimeDataByTimeTrigger(ref _measHandle, _sensorHandle, ref timeDataParams,
                ref triggerParams);
            if (error != SalErrorType.SalErrNone)
            {
                Trace.WriteLine($"启动IQ触发测量失败，{error}");
                if (error == SalErrorType.SalErrInvalidRequest)
                {
                    _iqTrigger = false;
                    error = Driver.RequestTimeData(ref _measHandle, _sensorHandle, ref timeDataParams);
                    if (error != SalErrorType.SalErrNone)
                    {
                        Trace.WriteLine($"启动IQ测量失败，{error}");
                        Driver.Close(_measHandle);
                        _iqTrigger = true;
                    }
                    else
                    {
                        _iqTrigger = false;
                    }
                }
                else
                {
                    Driver.Close(_measHandle);
                    _iqTrigger = true;
                    return false;
                }
            }
            else
            {
                _iqTrigger = true;
            }

            Trace.WriteLine("启动IQ测量成功");
        }
        else
        {
            //var numFftPoints = NumFFTPoints / Math.Pow(2, times));
            var numFftPoints = NumFftPoints;
            var demodTimes = (int)Math.Log2(MaxDemodSampleRateHz / (FilterBandwidth * 1e3));
            demodTimes = Math.Min(MaxDecimations, demodTimes);
            demodTimes = Math.Max(0, demodTimes);
            var demodSampleRate = Math.Floor(MaxDemodSampleRateHz / Math.Pow(2, demodTimes));
            Trace.WriteLine($"调谐采样率：{sampleRate} 解调采样率：{demodSampleRate}");
            if (AudioSwitch)
            {
                if (demodSampleRate.CompareWith(sampleRate) >= 0)
                {
                    Trace.WriteLine("参数错误，错误原因：解调采样率大于或等于调谐采样率。");
                    return false;
                }

                var tunerParams = new SalTunerParams
                {
                    CenterFrequency = Frequency * 1e6,
                    SampleRate = sampleRate,
                    Antenna = GetAntennaType(),
                    Preamp = Preamp,
                    Attenuation = Attenuation
                };
                error = Driver.SetTuner(_sensorHandle, ref tunerParams);
                if (error != SalErrorType.SalErrNone)
                {
                    Trace.WriteLine("设置解调电路参数失败");
                    return false;
                }

                var demodParams = new SalDemodParms
                {
                    TunerCenterFrequency = Frequency * 1e6,
                    TunerSampleRate = sampleRate,
                    DemodCenterFrequency = Frequency * 1e6,
                    DemodSampleRate = demodSampleRate,
                    NumSamples = 0,
                    Demodulation = GetDemodType()
                };
                error = Driver.RequestDemodData(ref _demodHandle, _sensorHandle, ref demodParams);
                if (error != SalErrorType.SalErrNone)
                {
                    Trace.WriteLine("启动解调测量失败");
                    Driver.Close(_demodHandle);
                    return false;
                }
            }

            var span = IfBandwidth * 1e3;
            var centerFrequency = Frequency * 1e6;
            var numPoints = (uint)(numFftPoints * span / sampleRate);
            var firstPoint = (numFftPoints - numPoints) / 2;
            var sweepParams = new SalSweepParams
            {
                NumSegments = 1,
                SweepInterval = 40
                //MonitorMode = SalMonitorMode.MonitorModeOn,
                //MonitorInterval = 40
            };
            var segment = new SalFrequencySegment
            {
                NumFftPoints = numFftPoints,
                NumPoints = numPoints,
                FirstPoint = firstPoint,
                CenterFrequency = centerFrequency,
                SampleRate = sampleRate,
                OverlapMode = SalOverlapType.OverlapOff,
                Preamp = Preamp,
                Attenuation = Attenuation,
                AverageType = GetAverageType(),
                NumAverages = GetAverageNum(Detector, numFftPoints, sampleRate, MeasureTime),
                NoTunerChange = AudioSwitch ? 1 : 0
            };
            var segmentTable = new[] { segment };
            _segmentInfos.Add(new SegmentInfo
            {
                StartPoint = firstPoint,
                SampleRate = sampleRate,
                NumFftPoints = numFftPoints,
                NumPoints = numPoints
            });
            error = Driver.StartSweep(ref _measHandle, _sensorHandle, ref sweepParams, segmentTable, IntPtr.Zero);
            if (error != SalErrorType.SalErrNone)
            {
                Trace.WriteLine("启动测量失败");
                Driver.Close(_measHandle);
                return false;
            }

            Trace.WriteLine("启动测量成功");
        }

        _receiveDataTokenSource = new CancellationTokenSource();
        _receiveDataTask = new Task(ReceiveFfmData, _receiveDataTokenSource.Token);
        _receiveDataTask.Start();
        _processDataTokenSource = new CancellationTokenSource();
        _processDataTask = new Task(ProcessFfmData, _processDataTokenSource.Token);
        _processDataTask.Start();
        if (AudioSwitch && !IqSwitch)
        {
            _receiveDemodDataTokenSource = new CancellationTokenSource();
            _receiveDemodDataTask = new Task(ReceiveDemodData, _receiveDemodDataTokenSource.Token);
            _receiveDemodDataTask.Start();
            _processDemodDataTokenSource = new CancellationTokenSource();
            _processDemodDataTask = new Task(ProcessDemodData, _processDemodDataTokenSource.Token);
            _processDemodDataTask.Start();
        }

        return true;
    }

    private void ReceiveDemodData()
    {
        if (_sensorHandle == UIntPtr.Zero || _demodHandle == UIntPtr.Zero) return;
        while (_receiveDemodDataTokenSource is { IsCancellationRequested: false })
        {
            if (_sensorHandle == UIntPtr.Zero || _demodHandle == UIntPtr.Zero) break;
            Thread.Sleep(1);
            var data = new int[1024];
            var demodData = new SalDemodData();
            var error = Driver.GetDemodData(_demodHandle, ref demodData, data);
            if (error != SalErrorType.SalErrNone) continue;
            _audioDataCache.Enqueue(new AudioSegmentData
            {
                Header = demodData,
                Data = data
            });
        }
    }

    private void ProcessDemodData()
    {
        while (_processDemodDataTokenSource is { IsCancellationRequested: false })
        {
            Thread.Sleep(1);
            _audioDataCache.TryDequeue(out var data);
            if (data == null) continue;
            var demodData = data.Data;
            var dataBuffer = new byte[4096];
            for (var i = 0; i < demodData.Length; i++)
            {
                var demodValue = demodData[i];
                var bytes = BitConverter.GetBytes(demodValue);
                Array.Copy(bytes, 0, dataBuffer, i * sizeof(int), bytes.Length);
            }

            var audioSampleRate = 11025;
            var bitsPerSample = 32;
            var channels = 1;
            var blockAlign = channels * bitsPerSample / 8;
            var bytesPerSecond = blockAlign * audioSampleRate;
            var audioData = new SDataAudio
            {
                Format = AudioFormat.Pcm,
                BlockAlign = blockAlign,
                SamplingRate = audioSampleRate,
                BitsPerSample = bitsPerSample,
                BytesPerSecond = bytesPerSecond,
                Channels = channels,
                Data = dataBuffer
            };
            SendData(new List<object> { audioData });
        }
    }

    private void ReceiveFfmData()
    {
        if (_sensorHandle == UIntPtr.Zero || _measHandle == UIntPtr.Zero) return;
        while (_receiveDataTokenSource is { IsCancellationRequested: false })
        {
            if (_sensorHandle == UIntPtr.Zero || _measHandle == UIntPtr.Zero) break;
            Thread.Sleep(1);
            SalErrorType error;
            if (IqSwitch)
            {
                var iqData = new short[NumTransferSamples * 2];
                var dataHdr = new SalTimeData();
                error = Driver.GetTimeData(_measHandle, ref dataHdr, iqData, NumTransferSamples * 2 * sizeof(short));
                if (!_iqTrigger)
                {
                    Thread.Sleep(40);
                    Driver.ContinueAcquire(_measHandle);
                }

                if (error != SalErrorType.SalErrNone) continue;
                _iqDataCache.Enqueue(new IqSegmentData
                {
                    Header = dataHdr,
                    Data = iqData
                });
            }
            else
            {
                var num = _segmentInfos[0].NumPoints;
                var size = sizeof(float) * num;
                var segmentData = new SalSegmentData();
                var amplitudes = new float[num];
                error = Driver.GetSegmentData(_measHandle, ref segmentData, amplitudes, size);
                if (error != SalErrorType.SalErrNone) break;
                _dataCache.Enqueue(new SpectrumSegmentData
                {
                    Header = segmentData,
                    Data = amplitudes
                });
            }
        }
    }

    private void ProcessFfmData()
    {
        while (_processDataTokenSource is { IsCancellationRequested: false })
        {
            Thread.Sleep(1);
            if (IqSwitch)
            {
                _iqDataCache.TryDequeue(out var data);
                if (data == null) continue;
                var dataHdr = data.Header;
                var iqs = data.Data;
                var iqData = new SDataIq
                {
                    SamplingRate = dataHdr.SampleRate / 1e3,
                    Attenuation = (int)dataHdr.Attenuation,
                    Bandwidth = IfBandwidth,
                    Data16 = iqs,
                    Frequency = Frequency,
                    Timestamp = (long)GetTimestamp(dataHdr.TimestampSeconds, dataHdr.TimestampNSeconds)
                };
                var spectrumData = new SDataSpectrum
                {
                    Frequency = Frequency,
                    Span = IfBandwidth,
                    Data = ToSpectrumByIq(iqs, IfBandwidth, dataHdr.SampleRate / 1e3, dataHdr.Attenuation)
                };
                var levelData = new SDataLevel
                {
                    Frequency = Frequency,
                    Bandwidth = IfBandwidth,
                    Data = ToLevelByIq(iqs, dataHdr.Attenuation)
                };
                SendData(new List<object> { iqData, spectrumData, levelData });
            }
            else
            {
                _dataCache.TryDequeue(out var data);
                if (data == null) continue;
                var dataHdr = data.Header;
                var amplitudes = data.Data;
                var temp = new float[dataHdr.NumPoints];
                var length = Math.Min(temp.Length, amplitudes.Length);
                Array.Copy(amplitudes, temp, length);
                var levels = Array.ConvertAll(temp, p => (short)((p + 107f) * 10));
                var spanHz = IfBandwidth * 1e3;
                var centerFrequencyHz = Frequency * 1e6;
                var spectrumData = new SDataSpectrum
                {
                    Frequency = centerFrequencyHz / 1e6,
                    Span = spanHz / 1e3,
                    Data = levels
                };
                var levelData = new SDataLevel
                {
                    Frequency = Frequency,
                    Bandwidth = IfBandwidth,
                    Data = levels[levels.Length / 2] / 10f
                };
                SendData(new List<object> { spectrumData, levelData });
            }
        }
    }

    #endregion

    #region 频段扫描

    private bool StartScanTask()
    {
        _segmentInfos.Clear();
        double interval = 40;
        var sweepParms = new SalSweepParams
        {
            NumSweeps = 0,
            SweepInterval = interval
        };
        var results = new SalSweepComputationResults();
        var rbw = StepFrequency * 1e3;
        var computationParms = new SalSweepComputationParams
        {
            StartFrequency = StartFrequency * 1e6,
            StopFrequency = StopFrequency * 1e6,
            Rbw = rbw
        };
        // 计算频谱扫描的分段数目
        var error = Driver.ComputeFftSegmentTableSize(ref computationParms, ref sweepParms, ref results);
        if (error != SalErrorType.SalErrNone) return false;
        // 根据频谱分段数目设置段数组大小
        var segments = new SalFrequencySegment[sweepParms.NumSegments];
        // 要扫描设置的默认段参数值
        var exampleSegments = new SalFrequencySegment
        {
            Antenna = GetAntennaType(),
            Attenuation = Attenuation,
            AverageType = GetAverageType(),
            OverlapMode = SalOverlapType.OverlapOff
        };
        // 初始化频谱扫描段参数
        error = Driver.InitializeFftSegmentTable(ref computationParms, ref sweepParms, ref exampleSegments, segments,
            ref results);
        if (error != SalErrorType.SalErrNone) return false;
        Array.ForEach(segments, segment =>
        {
            segment.NumAverages = GetAverageNum(Detector, segment.NumFftPoints, segment.SampleRate, MeasureTime);
            _segmentInfos.Add(new SegmentInfo
            {
                NumFftPoints = segment.NumFftPoints,
                NumPoints = segment.NumPoints,
                StartPoint = segment.FirstPoint,
                SampleRate = segment.SampleRate
            });
        });
        // 启动频谱扫描
        error = Driver.StartSweep(ref _measHandle, _sensorHandle, ref sweepParms, segments, IntPtr.Zero);
        if (error != SalErrorType.SalErrNone)
        {
            Trace.WriteLine("启动测量失败");
            Driver.Close(_measHandle);
            return false;
        }

        Trace.WriteLine("启动测量成功");
        _receiveDataTokenSource = new CancellationTokenSource();
        _receiveDataTask = new Task(ReceiveScanData, _receiveDataTokenSource.Token);
        _receiveDataTask.Start();
        _processDataTokenSource = new CancellationTokenSource();
        _processDataTask = new Task(ProcessScanData, _processDataTokenSource.Token);
        _processDataTask.Start();
        return true;
    }

    private void ReceiveScanData()
    {
        if (_sensorHandle == UIntPtr.Zero || _measHandle == UIntPtr.Zero) return;
        var num = _segmentInfos[0].NumPoints;
        var size = sizeof(float) * num;
        var segmentData = new SalSegmentData();
        var amplitudes = new float[num];
        while (_receiveDataTokenSource is { IsCancellationRequested: false })
        {
            if (_sensorHandle == UIntPtr.Zero || _measHandle == UIntPtr.Zero) break;
            Thread.Sleep(1);
            var error = Driver.GetSegmentData(_measHandle, ref segmentData, amplitudes, size);
            if (error != SalErrorType.SalErrNone) break;
            _dataCache.Enqueue(new SpectrumSegmentData
            {
                Header = segmentData,
                Data = amplitudes
            });
        }
    }

    private void ProcessScanData()
    {
        while (_processDataTokenSource is { IsCancellationRequested: false })
        {
            Thread.Sleep(1);
            _dataCache.TryDequeue(out var data);
            if (data == null) continue;
            var dataHdr = data.Header;
            var amplitudes = data.Data;
            var temp = new float[dataHdr.NumPoints];
            var length = Math.Min(temp.Length, amplitudes.Length);
            Array.Copy(amplitudes, temp, length);
            var levels = Array.ConvertAll(temp, p => p + 107f);
            var total = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
            try
            {
                var scanData = GetScanData(levels, StartFrequency * 1e6, StopFrequency * 1e6, StepFrequency * 1e3,
                    dataHdr.StartFrequency, dataHdr.FrequencyStep, out var offset);
                var scan = new SDataScan
                {
                    StartFrequency = StartFrequency,
                    StopFrequency = StopFrequency,
                    StepFrequency = StepFrequency,
                    SegmentOffset = (int)dataHdr.SweepIndex,
                    Offset = offset,
                    Total = total,
                    Data = Array.ConvertAll(scanData, item => (short)(item * 10))
                };
                SendData(new List<object> { scan });
            }
            catch (Exception)
            {
            }
        }
    }

    #endregion

    #region TDOA

    private bool StartTdoaTask()
    {
        var times = (int)Math.Log2(MaxSpanHz / (IfBandwidth * 1e3));
        times = Math.Min(MaxDecimations, times);
        times = Math.Max(0, times);
        var sampleRate = Math.Floor(MaxSampleRateHz / Math.Pow(2, times));
        var tunerParams = new SalTunerParams
        {
            CenterFrequency = Frequency * 1e6,
            SampleRate = sampleRate,
            Antenna = GetAntennaType(),
            Preamp = Preamp,
            Attenuation = Attenuation
        };
        var error = Driver.SetTuner(_sensorHandle, ref tunerParams);
        if (error != SalErrorType.SalErrNone)
        {
            Trace.WriteLine("设置解调电路参数失败");
            return false;
        }

        var timeDataParams = new SalTimeDataParms
        {
            CenterFrequency = Frequency * 1e6,
            SampleRate = sampleRate,
            DataType = 1,
            NumSamples = NumTransferSamples,
            NumTransferSamples = NumTransferSamples
        };
        var secs = (uint)(DateTime.Now.AddSeconds(3).ToUniversalTime() -
                          new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        var timeInfo = new SalTimeInfo();
        error = Driver.GetSensorTime(_sensorHandle, ref timeInfo);
        if (error == SalErrorType.SalErrNone) secs = timeInfo.TimestampSeconds + 3;
        var triggerParams = new SalTimeTrigParms
        {
            TimeTriggerType = SalTimeTrigType.TimeTrigAbstime,
            TriggerTimeSecs = secs,
            TriggerTimeNSecs = 0,
            TriggerInterval = (uint)TdoaTriggerInterval,
            TriggerCount = uint.MaxValue
        };
        error = Driver.RequestTimeDataByTimeTrigger(ref _measHandle, _sensorHandle, ref timeDataParams,
            ref triggerParams);
        if (error != SalErrorType.SalErrNone)
        {
            Trace.WriteLine($"启动IQ触发测量失败，{error}");
            if (error == SalErrorType.SalErrInvalidRequest)
            {
                _iqTrigger = false;
                error = Driver.GetSensorTime(_sensorHandle, ref timeInfo);
                if (error != SalErrorType.SalErrNone)
                {
                    Trace.WriteLine($"启动IQ测量失败，失败原因：获取GPS时间失败，{error}");
                    Driver.Close(_measHandle);
                    return false;
                }

                var time1 = GetTimestamp(timeInfo.TimestampSeconds, timeInfo.TimestampNSeconds);
                var time2 = GetTimestamp(timeInfo.TimestampSeconds + 2, 0);
                var delta = time2 - time1;
                Thread.Sleep(new TimeSpan((long)(delta / 100)));
                error = Driver.RequestTimeData(ref _measHandle, _sensorHandle, ref timeDataParams);
                if (error != SalErrorType.SalErrNone)
                {
                    Trace.WriteLine($"启动IQ测量失败，{error}");
                    Driver.Close(_measHandle);
                    _iqTrigger = true;
                }
                else
                {
                    _iqTrigger = false;
                }
            }
            else
            {
                Driver.Close(_measHandle);
                _iqTrigger = true;
                return false;
            }
        }
        else
        {
            _iqTrigger = true;
        }

        Trace.WriteLine("启动IQ测量成功");
        _receiveDataTokenSource = new CancellationTokenSource();
        _receiveDataTask = new Task(ReceiveTdoaData, _receiveDataTokenSource.Token);
        _receiveDataTask.Start();
        _processDataTokenSource = new CancellationTokenSource();
        _processDataTask = new Task(ProcessTdoaData, _processDataTokenSource.Token);
        _processDataTask.Start();
        return true;
    }

    private void ReceiveTdoaData()
    {
        if (_sensorHandle == UIntPtr.Zero || _measHandle == UIntPtr.Zero) return;
        while (_receiveDataTokenSource is { IsCancellationRequested: false })
        {
            if (_sensorHandle == UIntPtr.Zero || _measHandle == UIntPtr.Zero) break;
            Thread.Sleep(1);
            var iqData = new short[NumTransferSamples * 2];
            var dataHdr = new SalTimeData();
            var error = Driver.GetTimeData(_measHandle, ref dataHdr, iqData, NumTransferSamples * 2 * sizeof(short));
            if (error != SalErrorType.SalErrNone) continue;
            if (!_iqTrigger)
            {
                Thread.Sleep(40);
                Driver.ContinueAcquire(_measHandle);
            }

            _iqDataCache.Enqueue(new IqSegmentData
            {
                Header = dataHdr,
                Data = iqData
            });
        }
    }

    private void ProcessTdoaData()
    {
        while (_processDataTokenSource is { IsCancellationRequested: false })
        {
            Thread.Sleep(1);
            _iqDataCache.TryDequeue(out var data);
            if (data == null) continue;
            var dataHdr = data.Header;
            var iqs = data.Data;
            var iqData = new SDataIq
            {
                SamplingRate = dataHdr.SampleRate / 1e3,
                Attenuation = (int)dataHdr.Attenuation,
                Bandwidth = IfBandwidth,
                Data16 = iqs,
                Frequency = Frequency,
                Timestamp = (long)GetTimestamp(dataHdr.TimestampSeconds, dataHdr.TimestampNSeconds)
            };
            var spectrumData = new SDataSpectrum
            {
                Frequency = Frequency,
                Span = IfBandwidth,
                Data = ToSpectrumByIq(iqs, IfBandwidth, dataHdr.SampleRate / 1e3, dataHdr.Attenuation)
            };
            var levelData = new SDataLevel
            {
                Frequency = Frequency,
                Bandwidth = IfBandwidth,
                Data = ToLevelByIq(iqs, dataHdr.Attenuation)
            };
            SendData(new List<object> { iqData, spectrumData, levelData });
        }
    }

    #endregion
}