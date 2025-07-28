using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Numerics;
using System.Threading;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.RFeye8;

public partial class RFeye8
{
    #region 数据采集和处理

    /// <summary>
    ///     数据接收线程
    /// </summary>
    private void DataCaptrueProc()
    {
        while (_dataCaptureTokenSource?.IsCancellationRequested == false)
            try
            {
                var recvBuffer = new byte[4];
                //收取数据头并校验
                ReceiveData(recvBuffer, 0, 4, _tcpSocket);
                var headerCode = BitConverter.ToUInt32(recvBuffer, 0);
                if (headerCode != 0xAABBCCDD) continue;
                //收取包头并解析
                Array.Resize(ref recvBuffer, DataSize.HeaderSize << 2);
                ReceiveData(recvBuffer, 4, recvBuffer.Length - 4, _tcpSocket);
                var header = Header.Parse(recvBuffer, 0);
                if (header.PacketSize <= DataSize.HeaderSize) continue;
                //收取剩下的数据包,并校验包尾
                Array.Resize(ref recvBuffer, (int)header.PacketSize << 2);
                ReceiveData(recvBuffer, DataSize.HeaderSize << 2, ((int)header.PacketSize - DataSize.HeaderSize) << 2,
                    _tcpSocket);
                var footerCode = BitConverter.ToUInt32(recvBuffer, recvBuffer.Length - 4);
                if (footerCode != 0xDDCCBBAA) continue;
                //校验包是否需要解析
                if (header.PacketType == (int)PacketType.Link ||
                    BitConverter.ToInt32(recvBuffer, recvBuffer.Length - 20) == PacketKey.AnyAcknowledgePacket ||
                    recvBuffer.Length == (DataSize.HeaderSize + DataSize.FooterSize) * 4)
                    continue;
                //放入队列待处理
                if (TaskState == TaskState.Start)
                {
                    if (_dataQueue.Count > 200)
                        //由于硬件原因可能会出现数据量过多，解析不过来的情况，此处直接丢弃
                        _dataQueue.Clear();
                    _dataQueue.Enqueue(recvBuffer);
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
            catch (Exception ex)
            {
#if DEBUG
                Trace.WriteLine(ex.ToString());
#endif
            }
    }

    /// <summary>
    ///     数据处理和发送线程
    /// </summary>
    private void DataProcessProc()
    {
        while (_dataProcessTokenSource?.IsCancellationRequested == false)
            try
            {
                _dataQueue.TryDequeue(out var recvdata);
                if (recvdata == null) continue;
                List<object> listSendData = null;
                if (TaskState == TaskState.Start)
                {
                    var packet = Packet.Parse(recvdata, 0);
                    listSendData = ParseMediaData(packet);
                }

                if (listSendData?.Count > 0 && TaskState == TaskState.Start)
                {
                    listSendData.Add(DeviceId);
                    SendData(listSendData);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
#if DEBUG
                Trace.WriteLine(ex.ToString());
#endif
            }
    }

    private void GpsProcessProc()
    {
        //保存最新收到的经纬度信息
        double lastLatitude = 0;
        double lastLongitude = 0;
        //记录最新发送GPS的时间
        var lastSendTime = DateTime.MinValue;
        while (_gpsProcessTokenSource?.IsCancellationRequested == false)
            try
            {
                //发送Gps数据请求
                var requestPacket = new Packet();
                requestPacket.BeginPacket(PacketType.Node, -1);
                requestPacket.AddField(PacketKey.FieldGps, -1);
                requestPacket.EndPacket();
                SendPacket(requestPacket, _gpsSocket);
                //接收Gps数据(返回的数据可能还有心跳包,此处意为在有数据的情况下直到接收到GPS数据再发送下一次请求)
                Field gpsField = null;
                while (_gpsSocket.Available > 0)
                {
                    var responsePacket = ReceivePacket(_gpsSocket);
                    if (responsePacket != null)
                    {
                        gpsField = responsePacket.ListFieldInfo.Find(x => x.FieldName == PacketKey.FieldGps);
                        if (gpsField != null) break;
                    }
                }

                if (gpsField == null) continue;
                //解析Gps数据并发送(距离大于2米或者时间大于10秒)
                var dataGps = ParseGpsData(gpsField);
                if (dataGps != null)
                {
                    var distance = GetDistance(lastLatitude, lastLongitude, dataGps.Latitude, dataGps.Longitude);
                    var currTime = DateTime.Now;
                    var timeSpan = currTime - lastSendTime;
                    if (distance > 2 || timeSpan.TotalMilliseconds > 10000)
                    {
                        SendMessageData(new List<object> { dataGps });
                        //记录最新的发送时间
                        lastSendTime = currTime;
                    }

                    //保存最新的经纬度信息
                    lastLatitude = dataGps.Latitude;
                    lastLongitude = dataGps.Longitude;
                }

                Thread.Sleep(1000);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (SocketException)
            {
                break;
            }
            catch (Exception ex)
            {
#if DEBUG
                Trace.WriteLine(ex.ToString());
#endif
            }
    }

    /// <summary>
    ///     解析GPS数据
    /// </summary>
    /// <param name="gpsField"></param>
    private SDataGps ParseGpsData(Field gpsField)
    {
        var altitude = GetParameterIntValue(gpsField, PacketKey.GpsAltitude);
        var heading = GetParameterIntValue(gpsField, PacketKey.GpsHeading);
        var latitude = GetParameterIntValue(gpsField, PacketKey.GpsLatitude);
        var longtitude = GetParameterIntValue(gpsField, PacketKey.GpsLongitude);
        var speed = GetParameterIntValue(gpsField, PacketKey.GpsSpeed);
        var satellites = GetParameterIntValue(gpsField, PacketKey.GpsSatellites);
        var status = GetParameterIntValue(gpsField, PacketKey.GpsStatus);
        if (status == 0 || satellites == 0) return null;
        return new SDataGps
        {
            Altitude = (float)altitude / 1000,
            Heading = (ushort)(heading / 100),
            Latitude = (double)latitude / 1000000,
            Longitude = (double)longtitude / 1000000,
            Speed = (ushort)(speed / 1000),
            Satellites = (byte)satellites
        };
    }

    /// <summary>
    ///     解析数据（将设备返回的数据转换成监测软件的数据结构）
    /// </summary>
    /// <param name="responsePacket">设备返回的数据包</param>
    private List<object> ParseMediaData(Packet responsePacket)
    {
        var listData = new List<object>();
        foreach (var field in responsePacket.ListFieldInfo)
        {
            object info = null;
            switch (field.FieldName)
            {
                case PacketKey.FieldSweep:
                    info = ParseFieldSweepData(field);
                    break;
                case PacketKey.FieldTime:
                    info = ParseFieldTimeData(field);
                    break;
                case PacketKey.FieldDemodulation:
                    info = ParseFieldDemodData(field);
                    break;
            }

            if (info != null)
            {
                List<object> tempListData = null;
                switch (CurFeature)
                {
                    case FeatureType.FFM:
                        tempListData = ParseFixFqData(info);
                        break;
                    case FeatureType.SCAN:
                        tempListData = ParseScanData(info);
                        break;
                    case FeatureType.TDOA:
                        tempListData = ParseTdoaData(info);
                        break;
                    case FeatureType.FScne:
                    case FeatureType.MScan:
                    case FeatureType.MScne:
                        tempListData = ParseFreqScanData(info);
                        break;
                }

                if (tempListData?.Count > 0) listData.AddRange(tempListData);
            }
        }

        return listData;
    }

    /// <summary>
    ///     获取Field中参数名为paramKey的参数值
    /// </summary>
    /// <param name="field"></param>
    /// <param name="paramKey">参数名</param>
    /// <returns>参数值</returns>
    private int GetParameterIntValue(Field field, int paramKey)
    {
        var value = ~0; //表示无效
        var param = field.ListParameter.Find(x => x.ParameterName == paramKey);
        try
        {
            if (param?.Data.Length == 4) value = BitConverter.ToInt32(param.Data, 0);
        }
        catch
        {
        }

        return value;
    }

    /// <summary>
    ///     获取int类型的参数值
    /// </summary>
    /// <param name="param"></param>
    /// <returns>返回~0表示无效</returns>
    private int GetParameterIntValue(Parameter param)
    {
        var value = ~0; //表示无效
        try
        {
            if (param?.Data.Length == 4) value = BitConverter.ToInt32(param.Data, 0);
        }
        catch
        {
        }

        return value;
    }

    /// <summary>
    ///     转换数据（注：此处为了兼容uint采用int64来转换数据）
    /// </summary>
    /// <param name="srcArrData">数据</param>
    /// <param name="type">数据格式</param>
    private long[] ConvertData(byte[] srcArrData, ParamType type)
    {
        int size;
        switch (type)
        {
            case ParamType.DataSigned32:
            case ParamType.DataUnsigned32:
                size = 4;
                break;
            case ParamType.DataSigned16:
            case ParamType.DataUnsigned16:
                size = 2;
                break;
            default:
                size = 1;
                break;
        }

        var arrLen = srcArrData.Length / size;
        if (arrLen == 0) return null;
        var dstArrData = new long[arrLen];
        for (int i = 0, j = 0; i < srcArrData.Length; i += size, ++j)
        {
            long value = 0;
            switch (size)
            {
                case 4:
                    value = (srcArrData[i + 3] << 24) + (srcArrData[i + 2] << 16) + (srcArrData[i + 1] << 8) +
                            srcArrData[i];
                    break;
                case 2:
                    value = (srcArrData[i + 1] << 8) + srcArrData[i];
                    break;
                case 1:
                    value = srcArrData[i];
                    break;
            }

            dstArrData[j] = value;
        }

        return dstArrData;
    }

    /// <summary>
    ///     解析设备扫描数据
    /// </summary>
    /// <param name="field">field.FieldName == PacketKey.FieldSweep的数据包</param>
    private object ParseFieldSweepData(Field field)
    {
        var info = new SweepDataInfo();
        var startFreqMHz = 0;
        var stopFreqMHz = 0;
        var startFreqMilliHz = 0;
        var stopFreqMilliHz = 0;
        foreach (var param in field.ListParameter)
            switch (param.ParameterName)
            {
                case PacketKey.SweepRefLevel:
                    info.RefLevel = GetParameterIntValue(param);
                    break;
                case PacketKey.SweepStartFreqMHz:
                    startFreqMHz = GetParameterIntValue(param);
                    break;
                case PacketKey.SweepStartFreqMilliHz:
                    startFreqMilliHz = GetParameterIntValue(param);
                    break;
                case PacketKey.SweepStopFreqMHz:
                    stopFreqMHz = GetParameterIntValue(param);
                    break;
                case PacketKey.SweepStopFreqMilliHz:
                    stopFreqMilliHz = GetParameterIntValue(param);
                    break;
                case PacketKey.SweepResBandwidthHz:
                    info.RealResBwHz = GetParameterIntValue(param);
                    break;
                case PacketKey.SweepPeakData:
                case PacketKey.SweepAverageData:
                    info.Data = ConvertData(param.Data, param.GetParamType());
                    break;
            }

        info.RealStartFrequency = GetFrequency(startFreqMHz, startFreqMilliHz);
        info.RealStopFrequency = GetFrequency(stopFreqMHz, stopFreqMilliHz);
        if (info.Data == null) return null;
        return info;
    }

    /// <summary>
    ///     解析设备实时IQ数据
    /// </summary>
    /// <param name="field">field.FieldName == PacketKey.FieldTime</param>
    private object ParseFieldTimeData(Field field)
    {
        var info = new TimeDataInfo();
        const int dcOffsetI = 0;
        const int dcOffsetQ = 0;
        var agcAtten = 0;
        var radioGain = 0;
        byte[] dataIq = null;
        //const int invert = 0;//IQ是否交换,0为不交换
        var nDecimation = 0;
        var nCenterFreqMHz = 0;
        var nCenterFreqMilliHz = 0;
        foreach (var param in field.ListParameter)
            //TODO: 50-8型号中, 原20-6中的参数TimeDCOffsetI, TimeDCOffsetQ, TimeFreqPlanInvert已取消， 
            //新增的参数TimeRealTimeBandwidthMHz, TimeRealTimeBandwidthMilliHz只用于参数设置，此
            //处不返回，只返回TimeDecimation用于计算采样率, 采样率 = 62.5MHz / TimeDecimation
            switch (param.ParameterName)
            {
                case PacketKey.TimeRadioGain:
                    radioGain = GetParameterIntValue(param);
                    break;
                case PacketKey.TimeAgcAtten:
                    agcAtten = GetParameterIntValue(param);
                    break;
                case PacketKey.TimeDecimation:
                    nDecimation = GetParameterIntValue(param);
                    break;
                case PacketKey.TimeIqData:
                    dataIq = param.Data;
                    break;
                case PacketKey.TimeCenterFreqMHz:
                    nCenterFreqMHz = GetParameterIntValue(param);
                    break;
                case PacketKey.TimeCenterFreqMilliHz:
                    nCenterFreqMilliHz = GetParameterIntValue(param);
                    break;
                case PacketKey.AnyDspRtcUnixTime:
                    info.Utim = GetParameterIntValue(param);
                    break;
                case PacketKey.AnyDspRtcNano:
                    info.Nano = GetParameterIntValue(param);
                    break;
                case PacketKey.AnyErrorCode:
                    info.ErrorCode = GetParameterIntValue(param);
                    break;
                case PacketKey.AnyWarningCode:
                    info.WarningCode = GetParameterIntValue(param);
                    break;
            }

        if (dataIq == null) return null;
        //TODO:20170720 SDK8.3准确性有待验证
        var gaindB = radioGain / 16d - agcAtten;
        info.Scale = Math.Pow(10.0, -gaindB / 20d) * 0.223606 * 1e6 / 32768d;
        info.AgcAtten = agcAtten;
        //计算采样率
        info.Samplerate = nDecimation == 0 ? 62.5 : 62.5 / nDecimation;
        //计算实际中心频率
        info.RealFrequency = GetFrequency(nCenterFreqMHz, nCenterFreqMilliHz);
        //拆分IQ数据
        var len = dataIq.Length / 4;
        info.DataI = new double[len];
        info.DataQ = new double[len];
        //解析IQ数据，注意IQ是否倒置
        for (var i = 0; i < len; i++)
        {
            info.DataI[i] = (BitConverter.ToInt16(dataIq, 4 * i) - dcOffsetI) * info.Scale;
            info.DataQ[i] = (BitConverter.ToInt16(dataIq, 4 * i + 2) - dcOffsetQ) * info.Scale;
        }

        return info;
    }

    /// <summary>
    ///     解析设备解调数据
    /// </summary>
    /// <param name="field">field.FieldName == PacketKey.FieldDemod的数据包</param>
    private object ParseFieldDemodData(Field field)
    {
        var info = new DemodDataInfo();
        //int nRadioTuneFreqMHz = 0, nRadioTuneFreqMilliHz = 0;
        foreach (var param in field.ListParameter)
            switch (param.ParameterName)
            {
                //TODO:50-8不再返回频率信息,也不再返回频谱
                //case PacketKey.DemodRadioTuneFreqMHz: nRadioTuneFreqMHz = GetParameterIntValue(param); break;
                //case PacketKey.DemodRadioTuneFreqMilliHz: nRadioTuneFreqMilliHz = GetParameterIntValue(param); break;
                case PacketKey.DemodData:
                    info.DataAudio = param.Data;
                    break;
                case PacketKey.DemodSampleRateHz:
                    info.SampleRateHz = GetParameterIntValue(param);
                    break;
                //case PacketKey.DemodSpectrumData: info.DataSpec = ConvertData(param.Data, param.GetParamType()); break;
            }

        if (info.DataAudio == null && info.DataSpec == null) return null;
        //info.RealFrequency = nRadioTuneFreqMHz + nRadioTuneFreqMilliHz / 1000000000d;
        info.RealFrequency = Frequency;
        return info;
    }

    private List<object> ParseFixFqData(object info)
    {
        var datas = new List<object>();
        if (info is TimeDataInfo timeDataInfo)
        {
            //统计次数加1
            _realNumLoops++;
            if ((_media & MediaType.Level) > 0) datas.Add(ToLevel(timeDataInfo, Frequency, IfBandwidth));
            if ((_media & MediaType.Iq) > 0) datas.Add(ToIq(timeDataInfo, Frequency, IfBandwidth));
            if ((_media & MediaType.Itu) > 0) datas.Add(ToItu(timeDataInfo, Frequency, IfBandwidth));
            if ((_media & MediaType.Spectrum) > 0) datas.Add(ToSpectrum(timeDataInfo, Frequency, IfBandwidth));
            if ((_media & MediaType.Audio) > 0 && IfBandwidth < 300d && _currLevel >= SquelchThreshold)
                datas.Add(ToAudio(timeDataInfo, Frequency, IfBandwidth));
            //开始下次检波统计
            _realNumLoops %= SweepNumLoops;
        }

        //移除空的数据项
        datas.RemoveAll(x => x == null);
        return datas;
    }

    private List<object> ParseScanData(object info)
    {
        var datas = new List<object>();
        if (info is SweepDataInfo sweepDataInfo)
        {
            var dataScan = ToScan(sweepDataInfo);
            if (dataScan != null) datas.Add(dataScan);
        }

        return datas;
    }

    private List<object> ParseTdoaData(object info)
    {
        var datas = new List<object>();
        if (info is TimeDataInfo timeDataInfo)
        {
            if ((_media & MediaType.Tdoa) > 0) datas.Add(ToTdoa(timeDataInfo, Frequency, IfBandwidth, TimeNumSamples));
            if ((_media & MediaType.Spectrum) > 0) datas.Add(ToSpectrum(timeDataInfo, Frequency, IfBandwidth));
            if ((_media & MediaType.Level) > 0) datas.Add(ToLevel(timeDataInfo, Frequency, IfBandwidth));
            //移除空的数据项
            datas.RemoveAll(x => x == null);
        }

        return datas;
    }

    /// <summary>
    ///     解析FSCNE/MSCAN/MSCNE功能
    /// </summary>
    /// <param name="info"></param>
    private List<object> ParseFreqScanData(object info)
    {
        var datas = new List<object>();
        var freqInfo = _scanFreqs[_scanFreqIndex];
        if (info is TimeDataInfo timeDataInfo)
        {
            if ((_media & MediaType.Scan) > 0)
            {
                var dataScan = ToScan(timeDataInfo, freqInfo.Frequency, freqInfo.IfBandWidth);
                if (dataScan != null)
                {
                    datas.Add(dataScan);
                    _bReceivedScan = true;
                }
            }

            if ((_media & MediaType.Spectrum) > 0)
                if (_bReceivedScan)
                    //TODO:此处频谱数据也是通过IQ数据转换，若本包IQ数据能有效转换成扫描数据同理也能转换成频谱数据，反之亦然
                    if (_watcherDwellTime.ElapsedMilliseconds > 0)
                        //TODO:进入驻留才返回频谱，即等待时间内不返回频谱数据
                        datas.Add(ToSpectrum(timeDataInfo, freqInfo.Frequency, freqInfo.IfBandWidth));
            if ((_media & MediaType.Audio) > 0)
                if (_bReceivedScan)
                    //TODO:此处音频数据也是通过IQ数据转换，若本包IQ数据能有效转换成扫描数据同理也能转换成音频数据，反之亦然
                    if (_watcherDwellTime.ElapsedMilliseconds > 0 && _currLevel >= SquelchThreshold &&
                        freqInfo.IfBandWidth <= 200)
                        //TODO:进入驻留且电平高于静噪门限才返回音频
                        datas.Add(ToAudio(timeDataInfo, freqInfo.Frequency, freqInfo.IfBandWidth));
        }

        //移除空的数据项
        datas.RemoveAll(x => x == null);
        //检查是否可以切换到下一个频点
        if (_bReceivedScan) NextFrequency(_currLevel);
        return datas;
    }

    /// <summary>
    ///     FixFQ/TDOA电平数据
    /// </summary>
    /// <param name="info"></param>
    /// <param name="setFrequency"></param>
    /// <param name="setBw"></param>
    private object ToLevel(TimeDataInfo info, double setFrequency, double setBw)
    {
        //校验数据的准确性
        if (Math.Abs(info.RealFrequency - setFrequency) > 1e-9 ||
            Math.Abs(info.Samplerate - GetSamplerate(setBw)) > 1e-9) return null;
        var level = GetAvgLevel(info.DataI, info.DataQ);
        //保存最新的电平值，用于实现静噪门限
        _currLevel = level;
        switch (Detector)
        {
            case DetectMode.Pos:
                if (level > _levelSum || _levelCount == 0)
                {
                    _levelSum = level;
                    _levelCount = 1;
                }

                break;
            case DetectMode.Avg:
                _levelSum += level;
                _levelCount++;
                break;
            default:
                _levelSum = level;
                _levelCount = 1;
                break;
        }

        if (Detector != DetectMode.Fast && _realNumLoops < SweepNumLoops) return null;
        var dataLevel = new SDataLevel
        {
            Frequency = setFrequency,
            Bandwidth = setBw,
            Data = _levelSum / _levelCount
        };
        _levelSum = 0;
        _levelCount = 0;
        return dataLevel;
    }

    /// <summary>
    ///     FixFQ中的IQ数据
    /// </summary>
    /// <param name="info"></param>
    /// <param name="setFrequency"></param>
    /// <param name="setBw"></param>
    private object ToIq(TimeDataInfo info, double setFrequency, double setBw)
    {
        if (Math.Abs(info.RealFrequency - setFrequency) > 1e-9 ||
            Math.Abs(info.Samplerate - GetSamplerate(setBw)) > 1e-9) return null;
        var dataIq = new SDataIq
        {
            Frequency = setFrequency,
            Bandwidth = setBw, //IQ数据由FieldTime提供
            Attenuation = info.AgcAtten,
            SamplingRate = info.Samplerate * 1000.0d,
            Data16 = new short[info.DataI.Length * 2]
        };
        for (var i = 0; i < info.DataI.Length; ++i)
        {
            dataIq.Data16[2 * i] = (short)info.DataI[i];
            dataIq.Data16[2 * i + 1] = (short)info.DataQ[i];
        }

        return dataIq;
    }

    /// <summary>
    ///     FixFQ中的ITU数据
    /// </summary>
    /// <param name="info"></param>
    /// <param name="setFrequency"></param>
    /// <param name="setBw"></param>
    private object ToItu(TimeDataInfo info, double setFrequency, double setBw)
    {
        if (Math.Abs(info.RealFrequency - setFrequency) > 1e-9 ||
            Math.Abs(info.Samplerate - GetSamplerate(setBw)) > 1e-9 ||
            info.DataI.Length < 8192) return null;
        var arrIq = new short[info.DataI.Length * 2];
        for (var i = 0; i < info.DataI.Length; ++i)
        {
            arrIq[2 * i] = (short)info.DataI[i];
            arrIq[2 * i + 1] = (short)info.DataQ[i];
        }

        var ituResult = new ItuResult();
        if (!_itu.MeasureItu(_id, setFrequency * 1000000, (int)(info.Samplerate * 1000000), XdB, 1, 5000, arrIq,
                ref ituResult)) return null;
        var dAmDepth = ituResult.FAmMod * 100;
        var dFmDev = ituResult.FFmMod > IfBandwidth * 1000 * 2 ? double.MinValue : ituResult.FFmMod / 1000d;
        var dFmDevPos = ituResult.FFmPos > IfBandwidth * 1000 * 2 ? double.MinValue : ituResult.FFmPos / 1000d;
        var dFmDevNeg = ituResult.FFmNeg > IfBandwidth * 1000 * 2
            ? double.MinValue
            : Math.Abs(ituResult.FFmNeg / 1000d);
        var dPmDepth = ituResult.FPmMod is <= -2 * Math.PI or >= 2 * Math.PI ? double.MinValue : ituResult.FPmMod;
        var itu = new SDataItu
        {
            Frequency = ituResult.FFreq / 1000000d, //_frequency;
            // Beta = ituResult.fBetaBw_psd >= FilterBandwidth * 1000 * 2 ? double.MinValue : ituResult.fBetaBw_psd / 1000d,
            // Xdb = ituResult.fXdBBw_psd >= FilterBandwidth * 1000 * 2 ? double.MinValue : ituResult.fXdBBw_psd / 1000d,
            Modulation = Modulation.Iq, //算法暂未提供
            Misc = new Dictionary<string, object>
            {
                { ParameterNames.ItuAmDepth, dAmDepth },
                { ParameterNames.ItuFmDev, dFmDev },
                { ParameterNames.ItuFmDevPos, dFmDevPos },
                { ParameterNames.ItuFmDevNeg, dFmDevNeg },
                { ParameterNames.ItuPmDepth, dPmDepth }
            }
        };
        if (ituResult.FBetaBwPsd < FilterBandwidth * 1000 * 2)
            itu.Misc[ParameterNames.ItuBeta] = ituResult.FBetaBwPsd / 1000.0d;
        if (ituResult.FXdBBwPsd < FilterBandwidth * 1000 * 2)
            itu.Misc[ParameterNames.ItuBeta] = ituResult.FXdBBwPsd / 1000.0d;
        return itu;
    }

    /// <summary>
    ///     FixFQ, TDOA, MSCNE, FSCNE功能中的频谱数据
    /// </summary>
    /// <param name="info"></param>
    /// <param name="setFrequency"></param>
    /// <param name="setBw"></param>
    private object ToSpectrum(TimeDataInfo info, double setFrequency, double setBw)
    {
        if (Math.Abs(info.RealFrequency - setFrequency) > 1e-9 ||
            Math.Abs(info.Samplerate - GetSamplerate(setBw)) > 1e-9) return null;
        var nPoint = info.DataI.Length > 2048 ? 2048 : info.DataI.Length;
        var arrIq = new Complex[nPoint];
        for (var i = 0; i < nPoint; ++i) arrIq[i] = new Complex(info.DataI[i], info.DataQ[i]);
        //////////////////////////////////////////
        //TODO:确认是否需要
        //float[] arrWin = new float[arrIQ.Length];
        //float fWinCorr = Window(ref arrWin, arrIQ.Length, FilterWindow.Hanning);
        //Complex[] arrIQTemp = new Complex[arrIQ.Length];
        //EnFrame(ref arrIQTemp, arrIQ, arrWin, arrIQ.Length);
        //////////////////////////////////////////
        var arrIqTemp = arrIq;
        Fft(ref arrIqTemp);
        switch (Detector)
        {
            case DetectMode.Pos:
                if (_specCount == 0)
                {
                    for (var i = 0; i < nPoint; ++i)
                    {
                        var dataI = arrIqTemp[i].Real;
                        var dataQ = arrIqTemp[i].Imaginary;
                        _arrSpecBuffer[i] = Math.Log10(dataI * dataI + dataQ * dataQ);
                    }

                    _specCount = 1;
                }
                else
                {
                    for (var i = 0; i < nPoint; ++i)
                    {
                        var dataI = arrIqTemp[i].Real;
                        var dataQ = arrIqTemp[i].Imaginary;
                        var tempValue = Math.Log10(dataI * dataI + dataQ * dataQ);
                        if (tempValue > _arrSpecBuffer[i]) _arrSpecBuffer[i] = tempValue;
                    }
                }

                break;
            case DetectMode.Avg:
                for (var i = 0; i < nPoint; ++i)
                {
                    var dataI = arrIqTemp[i].Real;
                    var dataQ = arrIqTemp[i].Imaginary;
                    _arrSpecBuffer[i] += Math.Log10(dataI * dataI + dataQ * dataQ);
                }

                _specCount++;
                break;
            default: //DetectMode.FAST
                for (var i = 0; i < nPoint; ++i)
                {
                    var dataI = arrIqTemp[i].Real;
                    var dataQ = arrIqTemp[i].Imaginary;
                    _arrSpecBuffer[i] = Math.Log10(dataI * dataI + dataQ * dataQ);
                }

                _specCount = 1;
                break;
        }

        if (Detector != DetectMode.Fast && _realNumLoops < SweepNumLoops) return null;
        var fCorr = -20 * Math.Log10(nPoint);
        //当前分辨率对应的理论最低噪声
        var fBackgroundNoise = GetBackgroundNoise(info.Samplerate, nPoint);
        for (var i = 0; i < nPoint; ++i)
        {
            _arrSpecBuffer[i] = 10 * _arrSpecBuffer[i] / _specCount + fCorr;
            if (_arrSpecBuffer[i] < fBackgroundNoise) _arrSpecBuffer[i] = fBackgroundNoise;
        }

        //计算频谱点数
        var nSpecPoint = (int)(nPoint * (setBw / 1000d) / info.Samplerate + 0.5); //arrSpec.Length;
        var nSpecIndex = nPoint - nSpecPoint / 2;
        var dataSpec = new SDataSpectrum
        {
            Span = IfBandwidth, //setBw,
            Frequency = setFrequency,
            Data = new short[nSpecPoint]
        };
        for (var i = 0; i < nSpecPoint; ++i) dataSpec.Data[i] = (short)(_arrSpecBuffer[(nSpecIndex + i) % nPoint] * 10);
        //清理缓存
        Array.Clear(_arrSpecBuffer, 0, _arrSpecBuffer.Length);
        _specCount = 0;
        return dataSpec;
    }

    /// <summary>
    ///     utim单位秒，nano单位纳秒
    /// </summary>
    /// <param name="info"></param>
    /// <param name="setFrequency"></param>
    /// <param name="setBw"></param>
    /// <param name="nPoint"></param>
    private object ToTdoa(TimeDataInfo info, double setFrequency, double setBw, int nPoint)
    {
        if (Math.Abs(info.RealFrequency - setFrequency) > 1e-9 ||
            Math.Abs(info.Samplerate - GetSamplerate(setBw)) > 1e-9 ||
            nPoint != info.DataI.Length) return null;
        var dataTdoa = new SDataIq
        {
            Bandwidth = setBw,
            Frequency = setFrequency,
            Attenuation = info.AgcAtten,
            SamplingRate = info.Samplerate * 1000.0d,
            Data16 = new short[info.DataI.Length * 2]
        };
        for (var i = 0; i < info.DataI.Length; ++i)
        {
            dataTdoa.Data16[2 * i] = (short)info.DataI[i];
            dataTdoa.Data16[2 * i + 1] = (short)info.DataQ[i];
        }

        dataTdoa.Timestamp = info.Utim * 1000000000L + info.Nano;
        return dataTdoa;
    }

    /// <summary>
    ///     近似步进
    ///     SCAN中的扫描数据
    /// </summary>
    /// <param name="info"></param>
    private object ToScan(SweepDataInfo info)
    {
        //20181016: modified by linxia 部分版本设备不满足该规律，返回的起始频率可能略小于设置的起始频率
        //校正频段信息
        if (info.RealResBwHz != ResBandWidthHz) return null;
        var realStep = GetStep(info.RealResBwHz);
        if ((info.RealStartFrequency > StartFrequency && (info.RealStartFrequency - StartFrequency) * 1000 > realStep)
            || (info.RealStopFrequency < StopFrequency && (StopFrequency - info.RealStopFrequency) * 1000 > realStep))
            return null;
        var totalcount = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
        if (totalcount <= 0) return null;
        var arrSpec = new short[totalcount];
        for (var i = 0; i < totalcount; ++i)
        {
            var freq = StartFrequency + i * StepFrequency / 1000d;
            var index = (freq - info.RealStartFrequency) * 1000 / realStep;
            var indexBefore = Math.Max((int)index, 0);
            var indexAfter = indexBefore + 1;
            if (indexBefore >= info.Data.Length)
            {
                arrSpec[i] = 0;
                continue;
            }

            if (indexAfter >= info.Data.Length) indexAfter = indexBefore;

            var levelBefore = (float)(info.Data[indexBefore] / 2.0f - 127.5 + info.RefLevel + 107);
            var levelAfter = (float)(info.Data[indexAfter] / 2.0f - 127.5 + info.RefLevel + 107);
            var decimalPart = index - indexBefore;
            //与前后两点距离相等取最大的，不等取最接近的
            var compareResult = decimalPart.CompareWith(0.5d, 1e-6);
            if (compareResult == 0)
                arrSpec[i] = (short)(10 * Math.Max(levelBefore, levelAfter));
            else if (compareResult > 0)
                arrSpec[i] = (short)(10 * levelAfter);
            else
                arrSpec[i] = (short)(10 * levelBefore);
        }

        return new SDataScan
        {
            StartFrequency = StartFrequency,
            StopFrequency = StopFrequency,
            StepFrequency = StepFrequency,
            Total = totalcount,
            Offset = 0,
            Data = arrSpec
        };
    }

    /// <summary>
    ///     FSCNE/MSCAN/MSCNE的扫描数据
    /// </summary>
    /// <param name="info"></param>
    /// <param name="setFrequency"></param>
    /// <param name="setBw"></param>
    private object ToScan(TimeDataInfo info, double setFrequency, double setBw)
    {
        if (Math.Abs(info.RealFrequency - setFrequency) > 1e-9 ||
            Math.Abs(info.Samplerate - GetSamplerate(setBw)) > 1e-9) return null;
        var level = GetAvgLevel(info.DataI, info.DataQ);
        //保存最新的电平值，用于实现静噪门限
        _currLevel = level;
        var dataScan = new SDataScan();
        if (CurFeature == FeatureType.FScne)
        {
            dataScan.StartFrequency = StartFrequency;
            dataScan.StopFrequency = StopFrequency;
            dataScan.StepFrequency = StepFrequency;
        }

        dataScan.Total = _scanFreqs.Count;
        dataScan.Offset = _scanFreqIndex;
        dataScan.Data = new[] { (short)(level * 10) };
        return dataScan;
    }

    /// <summary>
    ///     FixFQ/FSCNE/MSCNE中的音频数据
    /// </summary>
    /// <param name="info"></param>
    /// <param name="setFrequency"></param>
    /// <param name="setBw"></param>
    private object ToAudio(TimeDataInfo info, double setFrequency, double setBw)
    {
        if (Math.Abs(info.RealFrequency - setFrequency) > 1e-9 ||
            Math.Abs(info.Samplerate - GetSamplerate(setBw)) > 1e-9) return null;
        if (DemMode == Modulation.Am)
        {
            for (var i = 0; i < info.DataI.Length; ++i)
            {
                var dataI = info.DataI[i];
                var dataQ = info.DataQ[i];
                _arrAudioBuffer[_audioCount++] = (short)Math.Sqrt((dataI * dataI + dataQ * dataQ) * 256);
            }
        }
        else if (DemMode == Modulation.Fm)
        {
            var i0 = _i0;
            var q0 = _q0;
            for (var i = 0; i < info.DataI.Length; ++i)
            {
                var i1 = info.DataI[i];
                var q1 = info.DataQ[i];
                var x = (i0 * q1 - i1 * q0) / (i1 * i1 + q1 * q1 + 1);
                _arrAudioBuffer[_audioCount++] = (short)(x * 16384);
                i0 = i1;
                q0 = q1;
            }

            _i0 = i0;
            _q0 = q0;
        }

        //TODO: 增加voice_cn处理后，由于音频FFT窗口大小为512，每次处理平移半个窗口，测试后发现当取65536个点时，未能处理到的点最少
        if (_audioCount < 65536) return null;
        //滤波
        AudioFilter(_arrAudioBuffer, _audioCount);
        var audioSampleRateHz = (int)(info.Samplerate * 1000000);
        var bufferAudio = new byte[_audioCount * 2];
        for (var i = 0; i < _audioCount; ++i)
            Array.Copy(BitConverter.GetBytes(_arrAudioBuffer[i]), 0, bufferAudio, i * 2, 2);
        //清理缓存
        Array.Clear(_arrAudioBuffer, 0, _arrAudioBuffer.Length);
        _audioCount = 0;
        return new SDataAudio
        {
            Format = AudioFormat.Pcm,
            Channels = 1,
            SamplingRate = audioSampleRateHz,
            BitsPerSample = 16,
            BlockAlign = 2,
            BytesPerSecond = 8000 * 2,
            Data = bufferAudio
        };
    }

    #endregion
}