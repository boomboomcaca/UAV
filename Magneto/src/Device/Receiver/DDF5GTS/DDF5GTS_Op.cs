using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.DDF5GTS;

public partial class Ddf5Gts
{
    /// <summary>
    ///     数据处理
    /// </summary>
    private void DataProc()
    {
        while (_dataReceiveCts?.IsCancellationRequested == false)
            try
            {
                if (_dataQueue.IsEmpty)
                {
                    Thread.Sleep(1);
                    continue;
                }

                var b = _dataQueue.TryDequeue(out var buffer);
                if (!b || buffer == null)
                {
                    Thread.Sleep(1);
                    continue;
                }

                var packet = RawPacket.Parse(buffer, 0);
                if (packet == null || packet.DataCollection.Count == 0)
                {
                    Thread.Sleep(1);
                    continue;
                }

                ForwardPacket(packet);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (SocketException)
            {
                break;
            }
            catch (Exception)
            {
            }
    }

    private void ForwardPacket(RawPacket packet)
    {
        if (packet == null) return;
        var result = new List<object>();
        object obj = null;
        foreach (var data in packet.DataCollection)
        {
            switch ((DataType)data.Tag)
            {
                case DataType.Audio:
                    obj = ToAudio(data as RawAudio);
                    break;
                case DataType.Ifpan:
                    obj = ToSpectrum(data as RawIfPan);
                    break;
                case DataType.DfpScan:
                    obj = ToDfpScan(data as RawDfPscan);
                    break;
                case DataType.Cw:
                    obj = ToCw(data as RawCw);
                    break;
                case DataType.Pscan:
                    obj = ToPScan(data as RawPScan);
                    break;
                case DataType.GpsCompass:
                    obj = ToGpsCompass(data as RawGpsCompass);
                    break;
            }

            if (obj != null)
            {
                if (obj is List<object> list)
                    result.AddRange(list);
                else
                    result.Add(obj);
            }
        }

        result = result.Where(item => item != null).ToList();
        if (result.Count > 0 && TaskState == TaskState.Start) SendData(result);
    }

    /// <summary>
    ///     IQ数据处理
    /// </summary>
    private void DataProcIq()
    {
        while (_dataReceiveIqCts?.IsCancellationRequested == false)
            try
            {
                if (_dataIqQueue.IsEmpty)
                {
                    Thread.Sleep(1);
                    continue;
                }

                var b = _dataIqQueue.TryDequeue(out var buffer);
                if (!b || buffer == null)
                {
                    Thread.Sleep(1);
                    continue;
                }

                var packet = RawPacket.Parse(buffer, 0);
                if (packet == null || packet.DataCollection.Count == 0)
                {
                    Thread.Sleep(1);
                    continue;
                }

                ForwardIqPacket(packet);
                // 由于IQ数据量大，会造成队列数据不断积压增大，导致内存不足，所以只处理一帧后就清空该队列
                _dataIqQueue.Clear();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (SocketException)
            {
                break;
            }
            catch (Exception)
            {
            }
            finally
            {
                _dataIqQueue.Clear();
            }
    }

    private void ForwardIqPacket(RawPacket packet)
    {
        if (packet == null) return;
        var result = new List<object>();
        object obj = null;
        foreach (var data in packet.DataCollection)
        {
            switch ((DataType)data.Tag)
            {
                case DataType.If:
                    obj = ToIq(data as RawIf);
                    break;
            }

            if (obj != null)
            {
                if (obj is List<object> list)
                    result.AddRange(list);
                else
                    result.Add(obj);
            }
        }

        result = result.Where(item => item != null).ToList();
        if (result.Count > 0 && TaskState == TaskState.Start)
            //result.Add(_deviceId);
            SendData(result);
    }

    private void GpsCompassProc()
    {
        while (_dataGpsCompassCts?.IsCancellationRequested == false)
            try
            {
                Thread.Sleep(1000);
                if (UseCompass)
                {
                    var rep = QueryCommand(CmdCompassheading, "zCompassName", _compassName);
                    if (rep.Command is { Params.Count: > 0 })
                    {
                        var heading = 0f;
                        foreach (var item in rep.Command.Params)
                            if (item.Name == "iHeading")
                            {
                                heading = float.Parse(item.Value) / 10f;
                                _heading = heading;
                            }

                        var compass = new SDataCompass
                        {
                            Heading = heading
                        };
                        SendMessageData(new List<object> { compass });
                    }
                }

                // 获取GPS数据信息
                if (UseGps)
                {
                    var result = SendSyncCmd("SYSTem:GPS:DATA?");
                    var gpsInfos = result.Split(',');
                    var dataGps = new SDataGps
                    {
                        Latitude = double.Parse(gpsInfos[6]) + double.Parse(gpsInfos[7]) / 60 +
                                   double.Parse(gpsInfos[8]) / 3600,
                        Longitude = double.Parse(gpsInfos[10]) + double.Parse(gpsInfos[11]) / 60 +
                                    double.Parse(gpsInfos[12]) / 3600
                    };
                    //_dataGPS.Heading = float.Parse(GPSInfos[21]);
                    SendMessageData(new List<object> { dataGps });
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (SocketException)
            {
                break;
            }
            catch (Exception)
            {
            }
    }

    /// <summary>
    ///     根据IQ数据计算电平值
    /// </summary>
    /// <returns></returns>
    private static float GetLevelByIq(short[] iq)
    {
        if (iq.Length <= 0) return float.MinValue;
        var count = iq.Length / 2;
        var arrDataI = new double[count];
        var arrDataQ = new double[count];
        // 分离实部和虚部用于计算ITU数据
        for (var i = 0; i < count; i++)
        {
            arrDataI[i] = iq[2 * i];
            arrDataQ[i] = iq[2 * i + 1];
        }

        var fLevel = 0d;
        for (var i = 0; i < count; ++i) fLevel += arrDataI[i] * arrDataI[i] + arrDataQ[i] * arrDataQ[i];
        fLevel = fLevel / count;
        var fLevelSum = 10 * Math.Log10(fLevel);
        var level = (float)fLevelSum;
        return level;
    }

    /// <summary>
    ///     根据DFStatus判断是否是最后一包数据
    /// </summary>
    /// <param name="dfStatus"></param>
    /// <returns></returns>
    private static bool IsLastHop(int dfStatus)
    {
        if ((dfStatus & 0x10) > 0)
            return true;
        return false;
    }

    /// <summary>
    ///     根据DFStatus判断测向数据是否进行了方位校正
    /// </summary>
    /// <param name="dfStatus"></param>
    /// <returns></returns>
    private static bool IsCorrectionData(int dfStatus)
    {
        if (((dfStatus >> 20) & 0x01) > 0)
            return true;
        return false;
    }

    /// <summary>
    ///     通过5555端口发送命令并检查设置命令结果
    /// </summary>
    /// <param name="cmd"></param>
    /// <returns></returns>
    private string SendSyncCmd(string cmd)
    {
        lock (this)
        {
            var buffer = Encoding.ASCII.GetBytes(cmd + "\n");
            _oldSocket.Send(buffer);
            var result = RecvResult('\n');
            return result;
        }
    }

    private string RecvResult(int endflag)
    {
        var total = 0;
        var buffer = new byte[1024 * 1024];
        while (_oldSocket.Receive(buffer, total, 1, SocketFlags.None) > 0)
            if (buffer[total++] == endflag)
                break;
        return Encoding.ASCII.GetString(buffer, 0, total);
    }

    #region 解析数据(EB200)

    /// <summary>
    ///     解析CW数据(单频测量任何时候都要订阅这个数据,需要解析电平数据)
    /// </summary>
    /// <returns></returns>
    private object ToCw(RawCw data)
    {
        var datas = new List<object>();
        if (ItuSwitch)
        {
            var frequency = _frequency;
            if (data.FreqOffset[0] >= -1000000000f) frequency = _frequency + data.FmDev[0] / 1000000f;
            var amDepth = data.AmDepth[0] < 0 || data.AmDepth[0] > 100 ? double.MinValue : data.AmDepth[0] / 10f;
            var fmDev = data.FmDev[0] < -1000000000f ? double.MinValue : data.FmDev[0] / 1000f;
            var fmDevPos = data.FmDevPos[0] < -1000000000f ? double.MinValue : data.FmDevPos[0] / 1000f;
            var fmDevNeg = data.FmDevNeg[0] < -1000000000f ? double.MinValue : data.FmDevNeg[0] / 1000f;
            var pmDepth = data.PmDepth[0] == short.MinValue ? double.MinValue : data.PmDepth[0] / 100f;
            double xdBBw;
            double betaBw;
            if (_bandMeasureMode == "XDB")
            {
                xdBBw = data.BandWidth[0] < -1000000000f ? double.MinValue : data.BandWidth[0] / 1000f;
                betaBw = double.MinValue;
            }
            else
            {
                betaBw = data.BandWidth[0] < -1000000000f ? double.MinValue : data.BandWidth[0] / 1000f;
                xdBBw = double.MinValue;
            }

            var sDataItu = new SDataItu
            {
                Modulation = Modulation.Cw,
                Frequency = frequency,
                Misc = new Dictionary<string, object>
                {
                    { ParameterNames.ItuAmDepth, amDepth },
                    { ParameterNames.ItuFmDev, fmDev },
                    { ParameterNames.ItuFmDevPos, fmDevPos },
                    { ParameterNames.ItuFmDevNeg, fmDevNeg },
                    { ParameterNames.ItuPmDepth, pmDepth },
                    { ParameterNames.ItuBeta, betaBw },
                    { ParameterNames.ItuXdb, xdBBw }
                }
            };
            datas.Add(sDataItu);
        }

        // 计算电平值
        var sDataLevel = new SDataLevel
        {
            Frequency = _frequency,
            Bandwidth = _ifbw,
            Data = data.Level[0] / 10f
        };
        datas.Add(sDataLevel);
        return datas;
    }

    /// <summary>
    ///     解析PScan数据
    /// </summary>
    /// <returns></returns>
    private object ToPScan(RawPScan data)
    {
        var currIndex = Utils.GetCurrIndex(data.FreqOfFirstStep / 1000000d, StartFrequency, StepFrequency);
        if (currIndex < 0) return null;
        var levels = new List<short>();
        levels.AddRange(data.DataCollection);
        var total = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
        //表示本次扫描结束,此处仅补齐缺少的点
        if (Math.Abs(levels.Last() - 200f) < 1e-9)
        {
            levels.RemoveAt(levels.Count - 1);
            for (var i = currIndex + levels.Count; i < total; ++i) levels.Add(0);
        }

        var scan = new SDataScan
        {
            StartFrequency = StartFrequency,
            StopFrequency = StopFrequency,
            StepFrequency = StepFrequency,
            Offset = currIndex,
            Total = total,
            Data = levels.ToArray()
        };
        return scan;
    }

    /// <summary>
    ///     解析测向数据
    /// </summary>
    private object ToDfpScan(RawDfPscan data)
    {
        var selectorFlags = data.SelectorFlags;
        var dfStatus = new DfStatus(data.DfStatus);
        // 当前包是否是最后一包
        var isLast = IsLastHop(data.DfStatus);
        // 测向数据是否经过了方位校正
        var isCorrection = IsCorrectionData(data.DfStatus);
        // 判断总点数与发来的数据是否匹配
        var total = (uint)data.LogChannel + (uint)data.NumberOfTraceItems;
        if (CurFeature == FeatureType.ScanDF)
            total = (uint)Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
        var offset = 0;
        var dFpScanData = new DfpScanData((uint)data.NumberOfTraceItems, data.DataCollection, ref offset, selectorFlags,
            (uint)data.LogChannel, total, isLast, isCorrection, AngleOffset);
        var sr = new SRdfData();
        if (dfStatus.DfMethod == 0x03 && data.SrSelectorflags > 0)
            sr = new SRdfData((uint)data.NumberOfTraceItems, data.NumberOfEigenvalues, data.SrWaveCount,
                data.DataCollection, ref offset, data.SrSelectorflags);
        var freq = (long)data.Frequency;
        var freqIndex = (int)((_frequency * 1000000 - freq) /
                              ((double)data.FrequencyStepNumerator / data.FrequencyStepDenominator));
        if (freqIndex > data.NumberOfTraceItems) freqIndex = 0;
        if ((_mediaType & (MediaType.Dfind | MediaType.Dfpan | MediaType.Scan)) > 0)
        {
            var result = new List<object>();
            if ((_mediaType & MediaType.Dfind) > 0)
            {
                var azimuth = dFpScanData.AzimuthF[freqIndex];
                var quality = dFpScanData.QualityF[freqIndex];
                var dataDFind = new SDataDfind
                {
                    Frequency = _frequency,
                    BandWidth = _dfBandWidth,
                    Azimuth = azimuth,
                    Quality = quality
                };
                if (dataDFind.Quality > QualityThreshold) result.Add(dataDFind);
            }

            if ((_mediaType & MediaType.Dfpan) > 0)
            {
                if (CurFeature != FeatureType.SSE)
                {
                    var dataWbdf = new SDataDfpan
                    {
                        Frequency = _frequency,
                        Span = _ifBandwidth,
                        Azimuths = dFpScanData.AzimuthF,
                        Qualities = dFpScanData.QualityF
                    };
                    result.Add(dataWbdf);
                }
                else
                {
                    var eigen = new float[sr.EigenCnt];
                    var level = new float[sr.WaveCnt];
                    var azimuth = new float[sr.WaveCnt];
                    var quality = new float[sr.WaveCnt];
                    var fs = new float[sr.WaveCnt];
                    var ele = new float[sr.WaveCnt];
                    for (var i = 0; i < sr.EigenCnt; i++) eigen[i] = sr.Eigenvalue[i, freqIndex];
                    for (var i = 0; i < sr.WaveCnt; i++)
                    {
                        level[i] = sr.Level[i, freqIndex];
                        azimuth[i] = sr.Azimuth[i, freqIndex];
                        quality[i] = sr.Quality[i, freqIndex];
                        fs[i] = sr.Fstrength[i, freqIndex];
                        ele[i] = sr.Elevation[i, freqIndex];
                    }

                    var sse = new SDataSse
                    {
                        Frequency = _frequency,
                        Bandwidth = _dfBandWidth
                    };
                    var sseData = new float[3600];
                    for (var i = 0; i < sr.WaveCnt; i++)
                        if (level[i] < 199 && azimuth[i] < 32700)
                        {
                            var azi = (int)(azimuth[i] * 10);
                            azi = (azi + 3600) % 3600;
                            sseData[azi] = quality[i];
                        }

                    for (var i = 0; i < sseData.Length; i++)
                        if (sseData[i] < 3)
                            sseData[i] = 3;
                    sse.Data = sseData;
                    result.Add(sse);
                }
            }

            if ((_mediaType & MediaType.Scan) > 0)
            {
                var dataScanDf = new SDataDfScan
                {
                    StartFrequency = StartFrequency,
                    StopFrequency = StopFrequency,
                    StepFrequency = StepFrequency
                };
                var indexs = new int[data.NumberOfTraceItems];
                if (isLast && data.NumberOfTraceItems + data.LogChannel != total)
                    indexs = new int[total - data.LogChannel];
                var count = isLast ? (int)total - data.LogChannel : data.NumberOfTraceItems;
                for (var i = 0; i < count; i++) indexs[i] = i + data.LogChannel;
                dataScanDf.Offset = indexs[0];
                dataScanDf.Count = indexs.Length;
                dataScanDf.Indices = indexs;
                dataScanDf.Azimuths = dFpScanData.AzimuthF;
                dataScanDf.Qualities = dFpScanData.QualityF;
                result.Add(dataScanDf);
                var sDataScan = new SDataScan
                {
                    StartFrequency = StartFrequency,
                    StopFrequency = StopFrequency,
                    StepFrequency = StepFrequency,
                    Offset = data.LogChannel,
                    Total = data.ChannelsInScanRange,
                    SegmentOffset = 0,
                    Data = dFpScanData.LevelF
                };
                result.Add(sDataScan);
            }

            if (result.Count > 0)
                return result;
            return null;
        }

        return null;
    }

    /// <summary>
    ///     解析IQ数据
    /// </summary>
    private object ToIq(RawIf data)
    {
        var rtnFreq = ((data.FrequencyHigh == 0 ? 0 : (long)data.FrequencyHigh << 32) + data.FrequencyLow) / 1000000d;
        // 校验数据有效性，以及判断当前IQ数据返回的中心频率是否与当前设置一致，不一致则丢弃该IQ数据
        if (data.NumberOfTraceItems <= 0 || !rtnFreq.Equals(Frequency)) return null;
        if (_iqSwitch)
        {
            var dataIq = new SDataIq
            {
                Frequency = rtnFreq,
                Bandwidth = data.Bandwidth / 1000d,
                SamplingRate = data.Samplerate / 1000000d,
                Attenuation = data.RxAtt
            };
            if (data.Mode == 1)
                dataIq.Data16 = data.DataCollection16;
            else if (data.Mode == 2) dataIq.Data32 = data.DataCollection32;
            return dataIq;
        }

        return null;
    }

    /// <summary>
    ///     解析频谱数据
    /// </summary>
    private static object ToSpectrum(RawIfPan data)
    {
        //校验数据有效性
        if (data.NumberOfTraceItems <= 0) return null;
        var datas = new List<object>();
        var dataSpec = new SDataSpectrum
        {
            Frequency = ((data.FrequencyHigh == 0 ? 0 : (long)data.FrequencyHigh << 32) + data.FrequencyLow) / 1000000d,
            Span = data.SpanFrequency / 1000d,
            Data = data.DataCollection
        };
        datas.Add(dataSpec);
        return datas;
    }

    /// <summary>
    ///     解析音频数据
    /// </summary>
    private static object ToAudio(RawAudio data)
    {
        if (data == null) return null;
        //校验数据有效性,最高频率到26.5G时 FrequencyHigh = 6,当大于6时，表明该包数据无效
        if (data.FrequencyHigh > 6) return null;
        Consts.AfModes.TryGetValue(data.AudioMode, out var afMode);
        if (afMode == null) return null;
        var audio = new SDataAudio
        {
            Format = AudioFormat.Pcm,
            SamplingRate = afMode.SamplingRate * 1000,
            BytesPerSecond = afMode.DataRate * 1000,
            BitsPerSample = afMode.BitsPerSample,
            BlockAlign = afMode.LengthPerFrame,
            Channels = afMode.Channels,
            Data = data.DataCollection
        };
        return audio;
    }

    /// <summary>
    ///     解析GPS
    /// </summary>
    private static object ToGpsCompass(RawGpsCompass data)
    {
        if (data.GpsValid != 1) return null;
        var gps = new SDataGps
        {
            Longitude = data.LonMin / 60 + data.LonDeg,
            Latitude = data.LatMin / 60 + data.LatDeg,
            Altitude = data.Altitude / 100f
        };
        if (data.NoOfSatInView == -1)
            gps.Satellites = 0x00;
        else
            gps.Satellites = (byte)data.NoOfSatInView;
        gps.Speed = (ushort)(data.SpeedOverGround / 10f * 3600 / 1000);
        gps.Heading = (ushort)(data.CompassHeading / 10f);
        return gps;
    }

    #endregion 解析数据(EB200)

    #region 数据转换 将参数转换为DDF5GTS的Xml协议可以识别的枚举

    /// <summary>
    ///     检波方式转换
    /// </summary>
    /// <param name="mode"></param>
    /// <returns></returns>
    private static ELevelIndicatir ConvertDetect(DetectMode mode)
    {
        var cmd = ELevelIndicatir.LevelIndicatorFast;
        switch (mode)
        {
            case DetectMode.Fast:
                cmd = ELevelIndicatir.LevelIndicatorFast;
                break;
            case DetectMode.Pos:
                cmd = ELevelIndicatir.LevelIndicatorPeak;
                break;
            case DetectMode.Avg:
                cmd = ELevelIndicatir.LevelIndicatorAvg;
                break;
            case DetectMode.Rms:
                cmd = ELevelIndicatir.LevelIndicatorRms;
                break;
        }

        return cmd;
    }

    /// <summary>
    ///     带宽测量方式转换
    /// </summary>
    /// <param name="mode"></param>
    /// <returns></returns>
    private static EMeasureMode ConvertMeasureMode(string mode)
    {
        EMeasureMode eMeasureMode;
        if (mode == "XDB")
            eMeasureMode = EMeasureMode.MeasuremodeXdb;
        else
            eMeasureMode = EMeasureMode.MeasuremodeBeta;
        return eMeasureMode;
    }

    /// <summary>
    ///     测向模式转换
    /// </summary>
    /// <param name="mode"></param>
    /// <returns></returns>
    private static EAverageMode ConvertDFindMode(DFindMode mode)
    {
        return mode switch
        {
            DFindMode.Feebleness => EAverageMode.DfsquOff,
            DFindMode.Gate => EAverageMode.DfsquGate,
            _ => EAverageMode.DfsquNorm
        };
    }

    /// <summary>
    ///     测向取值方式转换
    /// </summary>
    /// <param name="mode"></param>
    /// <returns></returns>
    private static EBlockAveragingSelect ConvertDfSelectMode(string mode)
    {
        if (mode == "TIME")
            return EBlockAveragingSelect.BlockAveragingSelectTime;
        return EBlockAveragingSelect.BlockAveragingSelectCycles;
    }

    /// <summary>
    ///     FFT模式转换
    /// </summary>
    /// <param name="mode"></param>
    /// <returns></returns>
    private static EifPanMode ConvertIfPanMode(string mode)
    {
        switch (mode)
        {
            case "MIN":
                return EifPanMode.IfpanModeMinhold;
            case "MAX":
                return EifPanMode.IfpanModeMaxhold;
            case "SCALar":
                return EifPanMode.IfpanModeAverage;
            case "OFF":
                return EifPanMode.IfpanModeClrwrite;
        }

        return EifPanMode.IfpanModeClrwrite;
    }

    #endregion 数据转换 将参数转换为DDF5GTS的Xml协议可以识别的枚举
}