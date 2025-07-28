using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Magneto.Contract;
using Magneto.Device.XE_HF.Protocols;
using Magneto.Device.XE_HF.Protocols.Data;
using Magneto.Device.XE_HF.Protocols.Field;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.XE_HF;

public partial class XeHf
{
    /// <summary>
    ///     解析消息头，但不改变原有buffer中的数据
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    private static MessageHeader ParseMessageHeader(byte[] buffer)
    {
        var startIndex = 0;
        var headerBuffer = new byte[8];
        Array.Copy(buffer, headerBuffer, 8);
        var header = new MessageHeader(headerBuffer, ref startIndex);
        return header;
    }

    /// <summary>
    ///     查找正确测向结果，或者最接近的（误差小于分辨率 1562.5Hz)
    /// </summary>
    /// <param name="arrAzi">设备返回的测向结果</param>
    /// <param name="centerFreq">当前中心频率，单位Hz（//TODO:20180628,修改此参数为double，若为原来的uint则在和CentreFrequency.Value进行相减时得到的负数会变成一个极大的正数并非预期）</param>
    /// <returns></returns>
    private static DfInfo GetDstAzimuth(DfInfo[] arrAzi, double centerFreq)
    {
        var tempArrAzi = arrAzi.OrderBy(x => x.CentreFrequency.Value).ToArray();
        var arrFreq = tempArrAzi.Select(x => x.CentreFrequency.Value).ToArray();
        var index = Array.BinarySearch(arrFreq, (uint)centerFreq);
        if (index >= 0) return tempArrAzi[index];
        //未找到
        DfInfo azi = null;
        if (index == -1)
        {
            azi = tempArrAzi[0];
        }
        else if (Math.Abs(index) == tempArrAzi.Length + 1)
        {
            azi = tempArrAzi[^1];
        }
        else
        {
            var pos = Math.Abs(index) - 1;
            azi = Math.Abs(tempArrAzi[pos - 1].CentreFrequency.Value - centerFreq) <=
                  Math.Abs(tempArrAzi[pos].CentreFrequency.Value - centerFreq)
                ? tempArrAzi[pos - 1]
                : tempArrAzi[pos];
        }

        if (azi != null && Math.Abs(azi.CentreFrequency.Value - centerFreq) < 1562.5) return azi;
        return null;
    }

    #region 数据采集和处理

    private void TcpDataProc()
    {
        var recvBuffer = new byte[1024 * 1024];
        while (_tcpDataProcessCts?.IsCancellationRequested == false)
            try
            {
                //读取消息头
                ReceiveData(recvBuffer, 0, 8);
                var startIndex = 0;
                var headerBuffer = new byte[8];
                Array.Copy(recvBuffer, headerBuffer, 8);
                var header = new MessageHeader(headerBuffer, ref startIndex);
                if (header.ContentSize > 0)
                {
                    //读取消息内容
                    ReceiveData(recvBuffer, startIndex, header.ContentSize);
                    switch (header.MessageId)
                    {
                        case MessageId.MreAcquittement:
                            break;
                        case MessageId.MreMajinfos:
                        {
                            _detectTime = DateTime.Now;
                            ParseGpsAndCompassInfo(recvBuffer);
                            break; //TODO：gps和compass信息
                        }
                        case MessageId.MreDemTypeDatation:
                            break;
                        case MessageId.MreInfoCycleBalayage:
                        {
                            ParseScanDuration(recvBuffer);
                            break;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
#if DEBUG
                Trace.WriteLine($"处理TCP消息异常，异常信息：{ex}");
#endif
            }
    }

    private void ParseGpsAndCompassInfo(byte[] buffer)
    {
        var startIndex = 0;
        var data = new UpdateOfTimeData(buffer, ref startIndex, _version);
        //以下0x7FFFFFFF和9999都为设备开发手册上定义的无效值
        if (GpsSwitch && data.Latitude.Value != 0x7FFFFFFF && data.Longitude.Value != 0x7FFFFFFF)
        {
            var dataGps = new SDataGps
            {
                Longitude = data.Longitude.Value / 10000000d,
                Latitude = data.Latitude.Value / 10000000d,
                Satellites = (byte)data.Satellites.Value,
                Speed = (ushort)(data.Speed.Value * 1.852 / 10)
            };
            SendMessageData(new List<object> { dataGps });
        }

        //21版本该消息长度为58,无GPSHeading等参数
        if (data.Heading.Value != 9999)
        {
            _compass = data.Heading.Value / 10f;
            if (CompassSwitch)
            {
                var dataCompass = new SDataCompass
                {
                    Heading = data.Heading.Value / 10f,
                    Pitch = (ushort)data.Pitch.Value,
                    Rolling = (ushort)data.Roll.Value
                };
                SendMessageData(new List<object> { dataCompass });
            }
        }
    }

    private void ParseScanDuration(byte[] buffer)
    {
        var startIndex = 0;
        var info = new ScanningCycleInfo(buffer, ref startIndex);
        var duration = info.Duration.Value / 1000d;
        if (duration > 5 && CurFeature == FeatureType.SCAN && TaskState == TaskState.Start)
            Trace.WriteLine($"由于点数过多，完整一帧数据返回需要{duration}秒左右，请耐心等待或者重新设置参数。");
    }

    private void UdpBbfftDataCaptrueProc()
    {
        var dataSocket = _udpBbfftSocket;
        if (dataSocket == null) return;
        EndPoint remote = new IPEndPoint(IPAddress.Any, 0);
        var recvBuffer = new byte[1024 * 1024];
        //TODO:udp数据通常整包发整包收，此处暂不校验
        //TODO:呵呵...结果证实在窄带带宽为100Hz时，XE并未对 > 65000个字节的音频数据包进行分包发送，造成协议层拆分成了两个包，
        while (_udpBbfftCaptrueCts?.IsCancellationRequested == false)
            try
            {
                //接收数据
                var recvBytes = dataSocket.ReceiveFrom(recvBuffer, ref remote);
                if (recvBytes <= 0) continue;
                if (recvBytes == 65000)
                    recvBytes += dataSocket.ReceiveFrom(recvBuffer, recvBytes, 65000, SocketFlags.None, ref remote);
                //放入待处理队列
                var data = new byte[recvBytes];
                Buffer.BlockCopy(recvBuffer, 0, data, 0, recvBytes);
                if (TaskState == TaskState.Start) _udpDataQueue.Enqueue(data);
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

    private void UdpNbfftDataCaptrueProc()
    {
        var dataSocket = _udpNbfftSocket;
        if (dataSocket == null) return;
        EndPoint remote = new IPEndPoint(IPAddress.Any, 0);
        var recvBuffer = new byte[1024 * 1024];
        //TODO:udp数据通常整包发整包收，此处暂不校验
        //TODO:呵呵...结果证实在窄带带宽为100Hz时，XE并未对 > 65000个字节的音频数据包进行分包发送，造成协议层拆分成了两个包，
        while (_udpNbfftCaptrueCts?.IsCancellationRequested == false)
            try
            {
                //接收数据
                var recvBytes = dataSocket.ReceiveFrom(recvBuffer, ref remote);
                if (recvBytes <= 0) continue;
                if (recvBytes == 65000)
                    recvBytes += dataSocket.ReceiveFrom(recvBuffer, recvBytes, 65000, SocketFlags.None, ref remote);
                //放入待处理队列
                var data = new byte[recvBytes];
                Buffer.BlockCopy(recvBuffer, 0, data, 0, recvBytes);
                if (TaskState == TaskState.Start) _udpDataQueue.Enqueue(data);
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

    private void UdpAudioDataCaptrueProc()
    {
        var dataSocket = _udpAudioSocket;
        if (dataSocket == null) return;
        EndPoint remote = new IPEndPoint(IPAddress.Any, 0);
        var recvBuffer = new byte[1024 * 1024];
        //TODO:udp数据通常整包发整包收，此处暂不校验
        //TODO:呵呵...结果证实在窄带带宽为100Hz时，XE并未对 > 65000个字节的音频数据包进行分包发送，造成协议层拆分成了两个包，
        while (_udpAudioCaptrueCts?.IsCancellationRequested == false)
            try
            {
                //接收数据
                var recvBytes = dataSocket.ReceiveFrom(recvBuffer, ref remote);
                if (recvBytes <= 0) continue;
                if (recvBytes == 65000)
                    recvBytes += dataSocket.ReceiveFrom(recvBuffer, recvBytes, 65000, SocketFlags.None, ref remote);
                //放入待处理队列
                var data = new byte[recvBytes];
                Buffer.BlockCopy(recvBuffer, 0, data, 0, recvBytes);
                if (TaskState == TaskState.Start) _udpDataQueue.Enqueue(data);
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

    private void UdpDfDataCaptrueProc()
    {
        var dataSocket = _udpDfSocket;
        if (dataSocket == null) return;
        EndPoint remote = new IPEndPoint(IPAddress.Any, 0);
        var recvBuffer = new byte[1024 * 1024];
        //TODO:udp数据通常整包发整包收，此处暂不校验
        //TODO:呵呵...结果证实在窄带带宽为100Hz时，XE并未对 > 65000个字节的音频数据包进行分包发送，造成协议层拆分成了两个包，
        while (_udpDfCaptrueCts?.IsCancellationRequested == false)
            try
            {
                //接收数据
                var recvBytes = dataSocket.ReceiveFrom(recvBuffer, ref remote);
                if (recvBytes <= 0) continue;
                if (recvBytes == 65000)
                    recvBytes += dataSocket.ReceiveFrom(recvBuffer, recvBytes, 65000, SocketFlags.None, ref remote);
                //放入待处理队列
                var data = new byte[recvBytes];
                Buffer.BlockCopy(recvBuffer, 0, data, 0, recvBytes);
                if (TaskState == TaskState.Start) _udpDataQueue.Enqueue(data);
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

    private void UdpItuDataCaptrueProc()
    {
        var dataSocket = _udpNbituSocket;
        if (dataSocket == null) return;
        EndPoint remote = new IPEndPoint(IPAddress.Any, 0);
        var recvBuffer = new byte[1024 * 1024];
        //TODO:udp数据通常整包发整包收，此处暂不校验
        //TODO:呵呵...结果证实在窄带带宽为100Hz时，XE并未对 > 65000个字节的音频数据包进行分包发送，造成协议层拆分成了两个包，
        while (_udpNbituCaptrueCts?.IsCancellationRequested == false)
            try
            {
                //接收数据
                var recvBytes = dataSocket.ReceiveFrom(recvBuffer, ref remote);
                if (recvBytes <= 0) continue;
                if (recvBytes == 65000)
                    recvBytes += dataSocket.ReceiveFrom(recvBuffer, recvBytes, 65000, SocketFlags.None, ref remote);
                //放入待处理队列
                var data = new byte[recvBytes];
                Buffer.BlockCopy(recvBuffer, 0, data, 0, recvBytes);
                if (TaskState == TaskState.Start) _udpDataQueue.Enqueue(data);
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

    private void UdpDataProcessProc()
    {
        while (_udpDataProcessCts?.IsCancellationRequested == false)
            try
            {
                var b = _udpDataQueue.TryDequeue(out var data);
                if (!b || data == null) continue;
                List<object> listSendData = null;
                if (TaskState == TaskState.Start) listSendData = ParseMediaData(data);
                if (listSendData is { Count: > 0 } && TaskState == TaskState.Start) SendData(listSendData);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
#if DEBUG
                Trace.WriteLine($"处理UDP消息异常，异常信息：{ex}");
#endif
            }
    }

    private List<object> ParseMediaData(byte[] recvBuffer)
    {
        List<object> listData = null;
        var header = ParseMessageHeader(recvBuffer);
        if (recvBuffer.Length < header.GetSize() + header.ContentSize)
            throw new Exception(string.Format(
                "无法解析的数据包，消息头信息为MessageID = {0:X}, ContentSize = {1}, 但当前数据包总长度为{2}, 与消息头不匹配。", header.MessageId,
                header.ContentSize, recvBuffer.Length));
        object info = null;
        switch (header.MessageId)
        {
            case MessageId.MreResFft:
                info = ParseFftData(recvBuffer);
                break;
            case MessageId.MreResGonio:
                info = ParseDfData(recvBuffer);
                break;
            case MessageId.MreResAudio:
                info = ParseAudioData(recvBuffer);
                break;
            case MessageId.MreResMesureuit:
                info = ParseItuData(recvBuffer);
                break;
        }

        if (info != null)
            switch (CurFeature)
            {
                case FeatureType.FFM:
                case FeatureType.ITUM:
                    listData = ParseFixFqData(info);
                    break;
                case FeatureType.SCAN:
                    listData = ParseScanData(info);
                    break;
                case FeatureType.FFDF:
                    listData = ParseFixDfData(info);
                    break;
                case FeatureType.ScanDF:
                    listData = ParseScanDfData(info);
                    break;
                case FeatureType.IFMCA:
                    listData = ParseIfmch(info);
                    break;
            }

        return listData;
    }

    private static object ParseFftData(byte[] recvBuffer)
    {
        var startIndex = 0;
        var info = new FftResults(recvBuffer, ref startIndex);
        return info;
    }

    private static object ParseDfData(byte[] recvBuffer)
    {
        var startIndex = 0;
        var info = new DirectionFindingResults(recvBuffer, ref startIndex);
        return info;
    }

    private static object ParseAudioData(byte[] recvBuffer)
    {
        var startIndex = 0;
        var info = new AudioResult(recvBuffer, ref startIndex);
        return info;
    }

    private static object ParseItuData(byte[] recvBuffer)
    {
        var startIndex = 0;
        var info = new ItuMeasurementsResult(recvBuffer, ref startIndex);
        return info;
    }

    private List<object> ParseFixFqData(object info)
    {
        var datas = new List<object>();
        if (info is ItuMeasurementsResult ituResult)
        {
            if ((_media & MediaType.Level) > 0) datas.Add(ToLevel(ituResult, _frequency, _ifBandWidth));
            if ((_media & MediaType.Itu) > 0) datas.Add(ToItu(ituResult, _ifBandWidth));
        }
        else if (info is FftResults fftResult)
        {
            if ( /*fftResult.PhaseNo.Value == (uint)ChannelType.NarrowBand &&*/ (_media & MediaType.Spectrum) > 0)
                //只用窄带频谱,fftResult.FFTs[0].FMin.Value == 0用于判断是否为窄带频谱
                if (fftResult.NumOfFft.Value > 0 && fftResult.FfTs[0].FMin.Value == 0)
                    datas.Add(ToSpectrum(fftResult, _frequency, _ifBandWidth));
        }
        else if (info is AudioResult audioResult)
        {
            if ((_media & MediaType.Audio) > 0) datas.Add(ToAudio(audioResult));
        }

        datas.RemoveAll(x => x == null);
        return datas;
    }

    private List<object> ParseScanData(object info)
    {
        var datas = new List<object>();
        if (info is FftResults fftResult)
        {
            var dataScan = ToScan(fftResult, _startFrequency, _stopFrequency, _stepFrequency);
            if (dataScan != null) datas.Add(dataScan);
        }

        return datas;
    }

    private List<object> ParseFixDfData(object info)
    {
        var datas = new List<object>();
        if (info is DirectionFindingResults dfResult)
        {
            var dataDFind = ToDFind(dfResult, _frequency, _dfBandWidth);
            if (dataDFind != null && dataDFind.Count != 0) datas.AddRange(dataDFind);
        }
        else if (info is FftResults fftResult)
        {
            if ((_media & MediaType.Spectrum) > 0)
                //fftResult.FFTs[0].FMin.Value == 0用于判断是否为窄带频谱
                if (fftResult.NumOfFft.Value > 0 && fftResult.FfTs[0].FMin.Value == 0)
                    datas.Add(ToSpectrum(fftResult, _frequency, _dfBandWidth));
        }
        else if (info is AudioResult audioResult)
        {
            if ((_media & MediaType.Audio) > 0) datas.Add(ToAudio(audioResult));
        }

        //移除空的数据项
        datas.RemoveAll(x => x == null);
        return datas;
    }

    private List<object> ParseScanDfData(object info)
    {
        var datas = new List<object>();
        if (info is DirectionFindingResults dfResult)
            datas.Add(ToScanDf(dfResult, _startFrequency, _stopFrequency, _stepFrequency));
        else if (info is FftResults fftResult)
            datas.Add(ToScan(fftResult, _startFrequency, _stopFrequency, _stepFrequency));

        datas.RemoveAll(x => x == null);
        return datas;
    }

    private List<object> ParseIfmch(object info)
    {
        var datas = new List<object>();
        if (info is FftResults fftResult)
            datas.Add(ToMch(fftResult));
        else if (info is AudioResult audioResult) datas.Add(ToMch(audioResult));

        datas.RemoveAll(x => x == null);
        return datas;
    }

    private object ToMch(FftResults fftResult)
    {
        if (fftResult.NumOfFft.Value <= 0) return null;
        //区别是宽带还是窄带
        if (fftResult.FfTs[0].FMin.Value != 0)
            return ToSpectrum(fftResult, _frequency, SpectrumSpan);
        if (fftResult.ChannelId.Value >= 1)
            if ((bool)_ddcChannels[fftResult.ChannelId.Value - 1][ParameterNames.IfSwitch])
            {
                var channelNumber = fftResult.ChannelId.Value - 1;
                var freq = (double)_ddcChannels[channelNumber][ParameterNames.Frequency];
                var bw = (double)_ddcChannels[channelNumber][ParameterNames.IfBandwidth];
                var dataMch = new SDataDdc
                {
                    ChannelNumber = channelNumber, Data //设备取值为[1,4]，Tracker800中为[0,3]
                        = new List<object>()
                };
                var dataSpec = ToSpectrum(fftResult, freq, bw);
                if (dataSpec != null)
                {
                    dataMch.Data.Add(dataSpec);
                    return dataMch;
                }
            }

        return null;
    }

    private object ToMch(AudioResult audioResult)
    {
        if (audioResult.ChannelId.Value < 1) return null;
        var index = audioResult.ChannelId.Value - 1;
        if ((bool)_ddcChannels[index][ParameterNames.AudioSwitch] == false ||
            (bool)_ddcChannels[index][ParameterNames.IfSwitch] == false) return null;
        var dataMch = new SDataDdc
        {
            ChannelNumber = index,
            Data = new List<object>()
        };
        var dataAudio = ToAudio(audioResult);
        if (dataAudio != null)
        {
            dataMch.Data.Add(dataAudio);
            return dataMch;
        }

        return null;
    }

    //FIXFQ,IFMCH,FIXDF频谱（窄带频谱）
    private static object ToSpectrum(FftResults fftResult, double setFrequency, double setBw)
    {
        if (fftResult.NumOfFft.Value <= 0) return null;
        var levels = new List<short>();
        for (ushort i = 0; i < fftResult.NumOfFft.Value; ++i)
        for (uint j = 0; j < fftResult.FfTs[i].NumOfBins.Value; ++j)
            if (fftResult.FfTs[i].Levels.Value[j] != -128) //TODO:
            {
                var level = (short)((fftResult.FfTs[i].Levels.Value[j] - (float)fftResult.Offset.Value + 107f) * 10);
                levels.Add(level);
            }

        var dataSpec = new SDataSpectrum
        {
            Frequency = setFrequency,
            Span = setBw,
            Data = levels.ToArray()
        };
        return dataSpec;
    }

    //FIXFQ电平
    private static object ToLevel(ItuMeasurementsResult ituResult, double setFrequency, double setBw)
    {
        if (Math.Abs(ituResult.AverageAntennaLevel.Value - -174d) < 1e-9) return null;
        var dataLevel = new SDataLevel
        {
            Frequency = setFrequency,
            Bandwidth = setBw,
            Data = (float)ituResult.AverageAntennaLevel.Value + 107
        };
        return dataLevel;
    }

    //FIXFQ ITU数据
    private static object ToItu(ItuMeasurementsResult ituResult, double setBw)
    {
        var dataItu = new SDataItu
        {
            Frequency = ituResult.CentreFrequency.Value / 1000000d,
            FieldStrength = ituResult.AverageFieldLevel.Value,
            Modulation = Modulation.Iq
        };
        var betaBw = ituResult.BetaBand.Value / 1000d;
        if (betaBw > 2 * setBw) betaBw = double.MinValue;
        var xdBbw = ituResult.XdB2Band.Value / 1000d;
        if (xdBbw > 2 * setBw) xdBbw = double.MinValue;
        var amDepth = ituResult.AmvqmIndex.Value is > 100 or < 0 ? double.MinValue : ituResult.AmvqmIndex.Value;
        var fmDev = ituResult.VqmFreqExcursion.Value / 1000d;
        if (fmDev > 2 * setBw) fmDev = double.MinValue;
        var fmDevPos = ituResult.PlusPeakFreqExcursion.Value / 1000d;
        if (fmDevPos > 2 * setBw) fmDevPos = double.MinValue;
        var fmDevNeg = ituResult.MinusPeakFreqExcursion.Value / 1000d;
        if (fmDevNeg > 2 * setBw) fmDevNeg = double.MinValue;
        var pmDepth = ituResult.VqmPhaseExcursion.Value;
        if (pmDepth is <= -2 * Math.PI or >= 2 * Math.PI) pmDepth = double.MinValue;
        dataItu.Misc = new Dictionary<string, object>
        {
            { ParameterNames.ItuBeta, betaBw },
            { ParameterNames.ItuXdb, xdBbw },
            { ParameterNames.ItuAmDepth, amDepth },
            { ParameterNames.ItuFmDev, fmDev },
            { ParameterNames.ItuFmDevPos, fmDevPos },
            { ParameterNames.ItuFmDevNeg, fmDevNeg },
            { ParameterNames.ItuPmDepth, pmDepth }
        };
        return dataItu;
    }

    //FIXIFQ, FIXDF音频数据
    private static object ToAudio(AudioResult audioResult)
    {
        if (audioResult.NumOfSamples.Value == 0) return null;
        var dataAudio = new SDataAudio
        {
            Format = AudioFormat.Pcm,
            Channels = 1,
            BlockAlign = 2,
            SamplingRate = (int)audioResult.NumOfSamples.Value,
            Data = AudioFilter.Filter(audioResult.AudioSamples.Value)
        };
        return dataAudio;
    }

    //SCAN频段扫描数据
    private static object ToScan(FftResults fftResult, double start, double stop, double step)
    {
        //首先校验数据是否与当前参数是否匹配
        if (fftResult.NumOfFft.Value <= 0) return null;
        var realFmin = fftResult.FfTs[0].FMin.Value / 1000000d;
        var realStep = fftResult.Resolution.Value / 1000d;
        if (realFmin < start - step / 1000d || realFmin > stop || Math.Abs(realStep - step) > 1e-9) return null;
        var currIndex = Utils.GetCurrIndex(realFmin, start, step);
        if (currIndex < 0) return null;
        var total = Utils.GetTotalCount(start, stop, step);
        var levels = new List<short>();
        for (ushort i = 0; i < fftResult.NumOfFft.Value; ++i)
        {
            var flag = false;
            for (uint j = 0; j < fftResult.FfTs[i].NumOfBins.Value; ++j)
                if (fftResult.FfTs[i].Levels.Value[j] == -128)
                {
                    //-128(sbyte)即128(0x80,byte)表示本帧数据结束,按框架要求补齐缺少的点
                    for (var k = currIndex + levels.Count; k < total; ++k) levels.Add(0);
                    //理论上只会在fftResult.FFTs的最后一个FFT数据中出现-128(sbyte)即128(0x80,byte)，通常这个数组是定长的，
                    //以-128(sbyte)即128(0x80,byte)来区别剩下的无效数据，此处为了避免作出多余的判断只要出现-128即表示
                    //后面也是无效所以被迫使用goto语句以跳出多层循环
                    //goto End;
                    flag = true;
                    break;
                }
                else
                {
                    var level = (short)((fftResult.FfTs[i].Levels.Value[j] - (float)fftResult.Offset.Value + 107f) *
                                        10);
                    levels.Add(level);
                }

            if (flag) break;
        }

        //End:
        //如果刚好是N个FFT则可能不会出现-128,如果只差一个点表示本帧数据已结束，在此处补0以避免框架在收到下一帧数据时认为丢包再补
        if (currIndex + levels.Count + 1 == total) levels.Add(0);
        var dataScan = new SDataScan
        {
            StartFrequency = start,
            StopFrequency = stop,
            StepFrequency = step,
            Total = total,
            Offset = currIndex,
            Data = levels.ToArray()
        };
        return dataScan;
    }

    //FIXDF测向数据,电平数据
    private List<object> ToDFind(DirectionFindingResults dfResult, double setFrequency, double setBw)
    {
        //测试发现短波设备除了连续测向都会返回很多频点，其中大多数时候中间那个点即为待测频点或最接近待测频点
        if (dfResult.NumOfValidDf.Value == 0) return null;
        var tempAzi = GetDstAzimuth(dfResult.Azimuths, setFrequency * 1000000);
        if (tempAzi == null) return null;
        if (_integralTime != 0)
        {
            //如果要积分则取积分时间内质量最高的示向度
            if (_watcherIntegrationTime.ElapsedMilliseconds < _integralTime)
            {
                if (_azi == null || _azi.QualityMask.Value <= tempAzi.QualityMask.Value) _azi = tempAzi;
                return null;
            }

            tempAzi = _azi;
            //重置计时器，开始下一次积分
            _watcherIntegrationTime.Reset();
            _watcherIntegrationTime.Start();
            _azi = null;
        }

        var datas = new List<object>();
        var dataDFind = new SDataDfind
        {
            Azimuth = (tempAzi.Azimuth.Value / 10f - _compass) % 360f
        };
        if (dataDFind.Azimuth < 0) dataDFind.Azimuth += 360;
        dataDFind.BandWidth = tempAzi.Width.Value / 1000d; //TODO：确认是否和设置的带宽一样？
        dataDFind.Frequency = tempAzi.CentreFrequency.Value / 1000000d;
        dataDFind.Quality = (tempAzi.QualityMask.Value + 1) * 10;
        datas.Add(dataDFind);
        var dataLevel = new SDataLevel
        {
            Frequency = dataDFind.Frequency,
            Bandwidth = setBw, //dataDFind.DFBandwidth;
            Data = tempAzi.Level.Value + 107
        };
        datas.Add(dataLevel);
        return datas;
    }

    //扫描测向数据
    private object ToScanDf(DirectionFindingResults dfResult, double start, double stop, double step)
    {
        if (dfResult.NumOfValidDf.Value == 0) return null;
        var total = Utils.GetTotalCount(start, stop, step);
        var listIndex = new int[total];
        var listAzimuth = new float[total];
        Array.Fill(listAzimuth, -1f);
        var listQuality = new float[total];
        Array.Fill(listQuality, -1f);
        var startHz = (long)(start * 1000000);
        var stopHz = (long)(stop * 1000000);
        var stepHz = (long)(step * 1000);
        foreach (var tempAzi in dfResult.Azimuths)
        {
            if (tempAzi.CentreFrequency.Value < startHz || tempAzi.CentreFrequency.Value > stopHz) continue;
            var index = (int)((tempAzi.CentreFrequency.Value - startHz) / stepHz);
            listIndex[index] = index;
            var azi = (tempAzi.Azimuth.Value / 10f - _compass) % 360f;
            if (azi < 0) azi += 360;
            listAzimuth[index] = azi;
            listQuality[index] = (tempAzi.QualityMask.Value + 1) * 10;
        }

        var dataScanDf = new SDataDfScan
        {
            StartFrequency = start,
            StopFrequency = stop,
            StepFrequency = step,
            Offset = listIndex[0],
            Count = listIndex.Length,
            Indices = listIndex,
            Azimuths = listAzimuth,
            Qualities = listQuality
        };
        return dataScanDf;
    }

    #endregion
}