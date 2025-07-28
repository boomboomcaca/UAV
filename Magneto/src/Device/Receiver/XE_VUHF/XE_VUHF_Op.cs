using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Magneto.Contract;
using Magneto.Device.XE_VUHF.Protocols;
using Magneto.Device.XE_VUHF.Protocols.Data;
using Magneto.Device.XE_VUHF.Protocols.Field;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.XE_VUHF;

public partial class XeVuhf
{
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
                        case MessageId.IdMsgVcyEmissionBeginning:
                        {
                            ParseMScanEmissionBegin(recvBuffer);
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

    private void UdpExtractionDataCaptrueProc()
    {
        var dataSocket = _udpExtractionSocket;
        if (dataSocket == null) return;
        EndPoint remote = new IPEndPoint(IPAddress.Any, 0);
        var recvBuffer = new byte[1024 * 1024];
        //TODO:udp数据通常整包发整包收，此处暂不校验
        //TODO:呵呵...结果证实在窄带带宽为100Hz时，XE并未对 > 65000个字节的音频数据包进行分包发送，造成协议层拆分成了两个包，
        while (_udpExtractionCaptrueCts?.IsCancellationRequested == false)
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
        ScanningCycleInfo info = new(buffer, ref startIndex);
        var duration = info.Duration.Value / 1000d;
        if (duration > 5 && CurFeature == FeatureType.SCAN && TaskState == TaskState.Start)
            Trace.WriteLine($"由于点数过多，完整一帧数据返回需要{duration}秒左右，请耐心等待或者重新设置参数。");
    }

    private void ParseMScanEmissionBegin(byte[] buffer)
    {
        var startIndex = 0;
        MScanEmissionBegin begin = new(buffer, ref startIndex);
        var curVal = (int)begin.Identifier.Value;
        if (_mscanIndex != curVal) _mscanIndex = (int)begin.Identifier.Value;
    }

    private List<object> ParseMediaData(byte[] recvBuffer)
    {
        List<object> listData = null;
        var header = ParseMessageHeader(recvBuffer);
        if (recvBuffer.Length < MessageHeader.GetSize() + header.ContentSize)
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
            case MessageId.MreResExtract:
                info = ParseExtractionData(recvBuffer);
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
                case FeatureType.MScan:
                    listData = ParseMscanData(info);
                    break;
                case FeatureType.WBDF:
                    listData = ParseWbdfData(info);
                    break;
            }

        return listData;
    }

    private static object ParseFftData(byte[] recvBuffer)
    {
        var startIndex = 0;
        FftResults info = new(recvBuffer, ref startIndex);
        return info;
    }

    private static object ParseDfData(byte[] recvBuffer)
    {
        var startIndex = 0;
        DirectionFindingResults info = new(recvBuffer, ref startIndex);
        return info;
    }

    private static object ParseAudioData(byte[] recvBuffer)
    {
        var startIndex = 0;
        AudioResult info = new(recvBuffer, ref startIndex);
        return info;
    }

    private static object ParseItuData(byte[] recvBuffer)
    {
        var startIndex = 0;
        ItuMeasurementsResult info = new(recvBuffer, ref startIndex);
        return info;
    }

    private static object ParseExtractionData(byte[] recvBuffer)
    {
        var startIndex = 0;
        ExtractionResults info = new(recvBuffer, ref startIndex);
        return info;
    }

    private List<object> ParseFixFqData(object info)
    {
        List<object> datas = new();
        if (info is ItuMeasurementsResult ituResult)
        {
            if ((_media & MediaType.Level) > 0) datas.Add(ToLevel(ituResult, _frequency, _ifBandWidth));
            if ((_media & MediaType.Itu) > 0) datas.Add(ToItu(ituResult, _ifBandWidth));
        }
        else if (info is FftResults fftResult)
        {
            if ((_media & MediaType.Spectrum) > 0)
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
        List<object> datas = new();
        if (info is FftResults fftResult)
        {
            var dataScan = ToScan(fftResult, _startFrequency, _stopFrequency, _stepFrequency);
            if (dataScan != null) datas.Add(dataScan);
        }

        return datas;
    }

    private List<object> ParseFixDfData(object info)
    {
        List<object> datas = new();
        if (info is DirectionFindingResults dfResult)
        {
            var dataDFind = ToDFind(dfResult, _dfBandWidth);
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
        List<object> datas = new();
        if (info is DirectionFindingResults dfResult)
            datas.Add(ToScanDf(dfResult, _startFrequency, _stopFrequency, _stepFrequency));
        else if (info is FftResults fftResult)
            datas.Add(ToScan(fftResult, _startFrequency, _stopFrequency, _stepFrequency));

        datas.RemoveAll(x => x == null);
        return datas;
    }

    private List<object> ParseWbdfData(object info)
    {
        List<object> datas = new();
        if (info is DirectionFindingResults dfResult)
            datas.Add(ToWbdf(dfResult, _frequency, SpectrumSpan, _channelBandWidth));
        else if (info is FftResults fftResult) datas.Add(ToSpectrum(fftResult, _frequency, _dfBandWidth));

        datas.RemoveAll(x => x == null);
        return datas;
    }

    private List<object> ParseIfmch(object info)
    {
        List<object> datas = new();
        if (info is FftResults fftResult)
            datas.Add(ToMch(fftResult));
        else if (info is AudioResult audioResult) datas.Add(ToMch(audioResult));

        datas.RemoveAll(x => x == null);
        return datas;
    }

    private List<object> ParseMscanData(object info)
    {
        List<object> datas = new();
        if (info is ItuMeasurementsResult ituResult)
        {
            //if (_mscanIndex == ituResult.Identifier.Value)
            //{
            var data = ToMScan(ituResult, (int)ituResult.Identifier.Value);
            if (data != null) datas.Add(data);
            //}
        }

        return datas;
    }

    //IFMCH
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
                    ChannelNumber = channelNumber,
                    Data //设备取值为[1,4]，Tracker800中为[0,3]
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

    //IFMCH
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

    //FIXFQ,IFMCH,FIXDF频谱（窄带频谱）或者宽带测向频谱
    private object ToSpectrum(FftResults fftResult, double setFrequency, double setBw)
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

        if ((_media & MediaType.Dfpan) > 0)
            //TODO: 保证宽带测向奇数个点否则客户端界面上会少一个点造成频率不对齐
            if (levels.Count % 2 == 0)
                levels.Add(0);
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
        //Average antenna level in dBm, -174 if wrong measure
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
        var dataItu = new SDataItu
        {
            Frequency = ituResult.CentreFrequency.Value / 1000000d,
            Misc = new Dictionary<string, object>
            {
                { ParameterNames.ItuBeta, betaBw },
                { ParameterNames.ItuXdb, xdBbw },
                { ParameterNames.ItuAmDepth, amDepth },
                { ParameterNames.ItuFmDev, fmDev },
                { ParameterNames.ItuFmDevPos, fmDevPos },
                { ParameterNames.ItuFmDevNeg, fmDevNeg },
                { ParameterNames.ItuPmDepth, pmDepth }
            }
        };
        return dataItu;
    }

    //FIXIFQ, FIXDF音频数据
    private static object ToAudio(AudioResult audioResult)
    {
        if (audioResult?.AudioSamples.Value == null || audioResult.NumOfSamples.Value == 0) return null;
        var bytes = new byte[audioResult.AudioSamples.Value.Length * 2];
        Buffer.BlockCopy(audioResult.AudioSamples.Value, 0, bytes, 0, bytes.Length);
        var dataAudio = new SDataAudio
        {
            Format = AudioFormat.Pcm,
            SamplingRate = 44100,
            Channels = 1,
            BitsPerSample = 16,
            BlockAlign = 2,
            Data = AudioFilter.Filter(audioResult.AudioSamples.Value)
        };
        return dataAudio;
    }

    //SCAN频段扫描数据或者SCANDF扫描测向功能中的扫描数据
    private object ToScan(FftResults fftResult, double start, double stop, double step)
    {
        //首先校验数据是否与当前参数是否匹配
        if (fftResult.NumOfFft.Value <= 0) return null;
        var realFmin = fftResult.FfTs[0].FMin.Value / 1000000d;
        var realStep = fftResult.Resolution.Value / 1000d;
        if (realFmin < start - step / 1000d || realFmin > stop || Math.Abs(realStep - step) > 1e-9) return null;
        var currIndex = Utils.GetCurrIndex(realFmin, start, step);
        if (currIndex < 0) return null;
        var total = Utils.GetTotalCount(start, stop, step);
        List<short> levels = new();
        for (ushort i = 0; i < fftResult.NumOfFft.Value; ++i)
        {
            var flag = false;
            for (uint j = 0; j < fftResult.FfTs[i].NumOfBins.Value; ++j)
                if (fftResult.FfTs[i].Levels.Value[j] == -128)
                {
                    //-128(sbyte)即128(0x80,byte)表示本帧数据结束,按框架要求补齐缺少的点
                    for (var k = currIndex + levels.Count; k < total; ++k) levels.Add(0);
                    //频段扫描的多频段是再功能层控制的，对于设备来说该功能永远只有一个频段，因此此处理论上只会在
                    //fftResult.FFTs的最后一个FFT数据中出现-128(sbyte)即128(0x80,byte)，通常这个数组是定长的，
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
        //频段扫描下检波方式处理
        if (CurFeature == FeatureType.SCAN)
        {
            switch (Detector)
            {
                case DetectMode.Pos:
                    if (0 == _realNumLoops)
                        Array.Copy(levels.ToArray(), 0, _arrScanBuffer, currIndex, levels.Count);
                    else
                        for (var i = 0; i < levels.Count; ++i)
                            if (levels[i] > _arrScanBuffer[currIndex + i])
                                _arrScanBuffer[currIndex + i] = levels[i];
                    break;
                case DetectMode.Avg:
                    for (var i = 0; i < levels.Count; ++i) _arrScanBuffer[currIndex + i] += levels[i];
                    break;
                default: //DetectMode.FAST
                    Array.Copy(levels.ToArray(), 0, _arrScanBuffer, currIndex, levels.Count);
                    break;
            }

            var dataScan = new SDataScan
            {
                StartFrequency = start,
                StopFrequency = stop,
                StepFrequency = step,
                Total = total,
                Offset = currIndex
            };
            if (Detector != DetectMode.Fast && _realNumLoops < SweepNumLoops - 1)
            {
                dataScan = null;
            }
            else
            {
                dataScan.Data = new short[levels.Count];
                if (Detector == DetectMode.Avg)
                    for (var i = 0; i < levels.Count; ++i)
                        dataScan.Data[i] = (short)(_arrScanBuffer[currIndex + i] / SweepNumLoops);
                else
                    Array.Copy(_arrScanBuffer, currIndex, dataScan.Data, 0, levels.Count);
            }

            if (currIndex + levels.Count == total)
            {
                _realNumLoops++;
                if (_realNumLoops == SweepNumLoops)
                    //清理缓存
                    Array.Clear(_arrScanBuffer, 0, _arrScanBuffer.Length);
                _realNumLoops %= SweepNumLoops;
            }

            return dataScan;
        }
        else
        {
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
    }

    //FIXDF测向数据,电平数据
    private List<object> ToDFind(DirectionFindingResults dfResult, double setBw)
    {
        if (dfResult.NumOfValidDf.Value != 1) return null;
        var tempAzi = dfResult.Azimuths[0];
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

        List<object> datas = new();
        var dataDFind = new SDataDfind
        {
            BandWidth = tempAzi.Width.Value / 1000d, //TODO：确认是否和设置的带宽一样？
            Frequency = tempAzi.CentreFrequency.Value / 1000000d,
            Quality = (tempAzi.QualityMask.Value + 1) * 10,
            Azimuth = (tempAzi.Azimuth.Value / 10f - _compass) % 360f //TODO：
        };
        if (dataDFind.Azimuth < 0) dataDFind.Azimuth += 360;
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

    private object ToWbdf(DirectionFindingResults dfResult, double setFrequency, double setBw, double resolutionBw)
    {
        var dataWbdf = new SDataDfpan
        {
            Azimuths = new float[(int)(setBw / resolutionBw) + 1], //TODO:暂时加上1否则界面上展示时会少一个点造成频率不对齐，V10再考虑修改
            Frequency = setFrequency,
            Span = setBw
        };
        dataWbdf.Qualities = new float[dataWbdf.Azimuths.Length];
        for (var i = 0; i < dataWbdf.Azimuths.Length; ++i) dataWbdf.Azimuths[i] = float.MinValue;
        for (var i = 0; i < dfResult.NumOfValidDf.Value; ++i)
        {
            var tempAzi = dfResult.Azimuths[i];
            var index = (int)((tempAzi.CentreFrequency.Value - (setFrequency * 1000000 - setBw * 1000 / 2)) /
                              (resolutionBw * 1000));
            if (index >= 0 && index < dataWbdf.Azimuths.Length)
            {
                var azi = (tempAzi.Azimuth.Value / 10f - _compass) % 360f;
                if (azi < 0) azi += 360;
                dataWbdf.Azimuths[index] = azi;
                dataWbdf.Qualities[index] = (tempAzi.QualityMask.Value + 1) * 10;
            }
        }

        return dataWbdf;
    }

    //离散扫描数据
    private object ToMScan(ItuMeasurementsResult ituResult, int index)
    {
        //Average antenna level in dBm, -174 if wrong measure
        if (Math.Abs(ituResult.AverageAntennaLevel.Value - -174d) < 1e-9) return null;
        //当前频点电平
        var level = (short)(((float)ituResult.AverageAntennaLevel.Value + 107) * 10);
        //未高过门限没有返回数据的频点用理论门限填充,为防止都使用低噪时一样再加一个+-2db的随机
        var noise = GetBackgroundNoise(25000);
        Random radom = new();
        //离散扫描总点数
        var total = MscanPoints.Length;
        //待发送的数据
        short[] levels = null;
        //待发送数据的开始索引
        var currIndex = _dstIndex;
        //本包数据的有效索引起点
        var validIndex = _dstIndex == 0 ? total - 1 : _dstIndex - 1;
        SDataScan dataScan = new()
        {
            Total = total
        };
        //未返回数据的频点个数
        int lostPoint;
        if (index == validIndex || index == _dstIndex)
        {
            levels = new short[1];
            levels[0] = level;
            currIndex = index;
        }
        else if (index > _dstIndex)
        {
            lostPoint = index - _dstIndex;
            levels = new short[lostPoint + 1];
            for (var i = 0; i < lostPoint; ++i) levels[i] = (short)(noise * 10 + radom.Next(-20, 20));
            levels[lostPoint] = level;
            currIndex = _dstIndex;
        }
        else if (index < _dstIndex)
        {
            //先补充上一帧未返回数据的频点发送
            var tempLevels = new short[total - _dstIndex];
            for (var i = 0; i < tempLevels.Length; ++i) tempLevels[i] = (short)(noise * 10 + radom.Next(-20, 20));
            var tempDataScan = new SDataScan
            {
                Offset = _dstIndex,
                Total = total,
                Data = null
            };
            SendData(new List<object> { tempDataScan });
            //当前帧数据
            _dstIndex = 0;
            lostPoint = index - _dstIndex;
            levels = new short[lostPoint + 1];
            for (var i = 0; i < lostPoint; ++i) levels[i] = (short)(noise * 10 + radom.Next(-20, 20));
            levels[lostPoint] = level;
            currIndex = _dstIndex;
        }

        dataScan.Offset = currIndex;
        dataScan.Data = levels;
        //保存最新索引
        if (levels != null)
            _dstIndex = currIndex + levels.Length == total ? 0 : currIndex + levels.Length;
        return dataScan;
    }

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
        MessageHeader header = new(headerBuffer, ref startIndex);
        return header;
    }

    /// <summary>
    ///     用于离散扫描时计算理论噪声
    /// </summary>
    /// <param name="resolution">单位Hz</param>
    /// <returns></returns>
    private static float GetBackgroundNoise(double resolution)
    {
        var noise = -67 + 10 * Math.Log10(resolution);
        return (int)(noise * 10) / 10f;
    }
}