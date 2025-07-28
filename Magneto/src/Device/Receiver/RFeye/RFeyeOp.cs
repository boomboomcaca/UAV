using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Numerics;
using System.Threading;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.RFeye;

public partial class RFeye
{
    #region 数据采集和处理

    /// <summary>
    ///     数据接收线程
    /// </summary>
    private void DataCaptureProc()
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
                lock (_dataProcessLock)
                {
                    if (TaskState == TaskState.Start)
                    {
                        var packet = Packet.Parse(recvdata, 0);
                        listSendData = ParseMediaData(packet);
                    }
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
    ///     GPS数据收发线程
    /// </summary>
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
                        //SendMessage(MessageDomain.Network, MessageType.MonNodeGPSChange, dataGps);
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
        if (status == 0) return null;
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
            case ParamType.DataSigned8:
            case ParamType.DataUnsigned8:
                size = 1;
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
        var startFreqMiliHz = 0;
        var stopFreqMiliHz = 0;
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
                    startFreqMiliHz = GetParameterIntValue(param);
                    break;
                case PacketKey.SweepStopFreqMHz:
                    stopFreqMHz = GetParameterIntValue(param);
                    break;
                case PacketKey.SweepStopFreqMilliHz:
                    stopFreqMiliHz = GetParameterIntValue(param);
                    break;
                case PacketKey.SweepResBandwidthHz:
                    info.RealResBwHz = GetParameterIntValue(param);
                    break;
                case PacketKey.SweepPeakData:
                case PacketKey.SweepAverageData:
                    info.Data = ConvertData(param.Data, param.GetParamType());
                    break;
            }

        info.RealStartFrequency = startFreqMHz + startFreqMiliHz / 1000000000d;
        info.RealStopFrequency = stopFreqMHz + stopFreqMiliHz / 1000000000d;
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
        var dcOffsetI = 0;
        var dcOffsetQ = 0;
        var agcAtten = 0;
        var radioGain = 0;
        byte[] dataIq = null;
        var invert = 0; //IQ是否交换,0为不交换
        var nDecimation = 0;
        var nCenterFreqMHz = 0;
        var nDdsOffsetHz = 0;
        foreach (var param in field.ListParameter)
            switch (param.ParameterName)
            {
                case PacketKey.TimeRadioGain:
                    radioGain = GetParameterIntValue(param);
                    break;
                case PacketKey.TimeAgcAtten:
                    agcAtten = GetParameterIntValue(param);
                    break;
                case PacketKey.TimeDcOffsetI:
                    dcOffsetI = GetParameterIntValue(param);
                    break;
                case PacketKey.TimeDcOffsetQ:
                    dcOffsetQ = GetParameterIntValue(param);
                    break;
                case PacketKey.TimeDecimation:
                    nDecimation = GetParameterIntValue(param);
                    break;
                case PacketKey.TimeIqData:
                    dataIq = param.Data;
                    break;
                case PacketKey.TimeFreqPlanInvert:
                    invert = GetParameterIntValue(param);
                    break;
                case PacketKey.TimeCenterFreqMHz:
                    nCenterFreqMHz = GetParameterIntValue(param);
                    break;
                case PacketKey.TimeDdsOffsetHz:
                    nDdsOffsetHz = GetParameterIntValue(param);
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
        var lingain = Math.Pow(10, (double)radioGain / 16 / 20);
        var agc = 1e6;
        if (agcAtten != 0) agc = Math.Pow(10, (double)agcAtten / 20) * 1e6;
        //计算电平转换参数
        info.Scale = agc / (65536 * lingain);
        info.AgcAtten = agcAtten;
        //计算采样率
        info.Samplerate = nDecimation == 0 ? 40 : 40d / nDecimation;
        //计算实际中心频率
        info.RealFrequency = GetFrequency(nCenterFreqMHz, nDdsOffsetHz);
        //拆分IQ数据
        var len = dataIq.Length / 4;
        info.DataI = new double[len];
        info.DataQ = new double[len];
        //注意IQ是否倒置
        for (var i = 0; i < len; i++)
            if (invert == 0)
            {
                info.DataI[i] = (BitConverter.ToInt16(dataIq, 4 * i) - dcOffsetI) * info.Scale;
                info.DataQ[i] = (BitConverter.ToInt16(dataIq, 4 * i + 2) - dcOffsetQ) * info.Scale;
            }
            else
            {
                info.DataQ[i] = (BitConverter.ToInt16(dataIq, 4 * i) - dcOffsetI) * info.Scale;
                info.DataI[i] = (BitConverter.ToInt16(dataIq, 4 * i + 2) - dcOffsetQ) * info.Scale;
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
        var nDecimation = 0;
        var nDataDecimation = 0;
        foreach (var param in field.ListParameter)
            switch (param.ParameterName)
            {
                case PacketKey.DemodRadioTuneFreqMHz:
                    info.RadioTuneFreqMHz = GetParameterIntValue(param);
                    break;
                case PacketKey.DemodDdsFreqKHz:
                    info.DdsFreqKHz = GetParameterIntValue(param);
                    break;
                case PacketKey.DemodData:
                    info.DataAudio = param.Data;
                    break;
                case PacketKey.DemodSpectrumData:
                    info.DataSpec = ConvertData(param.Data, param.GetParamType());
                    break;
                case PacketKey.DemodRfDecimation:
                    nDecimation = GetParameterIntValue(param);
                    break;
                case PacketKey.DemodDataDecimation:
                    nDataDecimation = GetParameterIntValue(param);
                    break;
            }

        if (info.DataAudio == null && info.DataSpec == null) return null;
        //输出音频的采样率(MHz) = 40 / DECI / DDEC 
        info.Samplerate = 40.0 / nDecimation / nDataDecimation;
        return info;
    }

    private List<object> ParseFixFqData(object info)
    {
        var datas = new List<object>();
        if (info is TimeDataInfo timeDataInfo)
        {
            if ((_media & MediaType.Level) > 0)
            {
                var dataLevel = ToLevel(timeDataInfo);
                if (dataLevel != null)
                {
                    datas.Add(dataLevel);
                    //保存最新的电平值，用于实现静噪门限
                    _currLevel = (dataLevel as SDataLevel)?.Data ?? 0;
                }
            }

            if ((_media & MediaType.Iq) > 0) datas.Add(ToIq(timeDataInfo));
            if ((_media & MediaType.Itu) > 0) datas.Add(ToItu(timeDataInfo));
        }
        else if (info is SweepDataInfo sweepDataInfo)
        {
            if ((_media & MediaType.Spectrum) > 0)
                datas.Add(ToSpectrum(sweepDataInfo, Frequency, IfBandwidth, ResBandWidthHz));
        }
        else if (info is DemodDataInfo demodDataInfo)
        {
            if ((_media & MediaType.Audio) > 0 && _currLevel >= SquelchThreshold)
                datas.Add(ToAudio(demodDataInfo, Frequency));
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
            if ((_media & MediaType.Tdoa) > 0) datas.Add(ToTdoa(timeDataInfo));
            if ((_media & MediaType.Spectrum) > 0) datas.Add(ToSpectrum(timeDataInfo));
            if ((_media & MediaType.Level) > 0) datas.Add(ToLevel(timeDataInfo));
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
        var isTimeData = false;
        if (info is TimeDataInfo timeDataInfo)
        {
            if ((_media & MediaType.Scan) > 0)
            {
                var dataScan = ToScan(timeDataInfo);
                if (dataScan is SDataScan scan)
                {
                    datas.Add(dataScan);
                    _bReceivedScan = true;
                    isTimeData = true;
                    _currLevel = 0;
                    if (scan.Data?.Length > 0) _currLevel = scan.Data[0] / 10f;
                }
            }
        }
        else if (info is SweepDataInfo sweepDataInfo)
        {
            if ((_media & MediaType.Spectrum) > 0)
            {
                var dataSpec = ToSpectrum(sweepDataInfo, _scanFreqs[_scanFreqIndex].Frequency,
                    _scanFreqs[_scanFreqIndex].IfBandWidth,
                    GetResBw(CurFeature, _scanFreqs[_scanFreqIndex].IfBandWidth));
                if (dataSpec != null)
                {
                    _bReceivedSpec = true;
                    if (_watcherDwellTime.ElapsedMilliseconds > 0)
                        //TODO:进入驻留才返回频谱，即等待时间内不返回频谱数据
                        datas.Add(dataSpec);
                }
            }
        }
        else if (info is DemodDataInfo demodDataInfo)
        {
            if ((_media & MediaType.Audio) > 0)
            {
                var dataAudio = ToAudio(demodDataInfo, _scanFreqs[_scanFreqIndex].Frequency);
                if (dataAudio != null)
                {
                    _bReceivedAudio = true;
                    if (_watcherDwellTime.ElapsedMilliseconds > 0 && _currLevel >= SquelchThreshold)
                        //TODO:进入驻留且电平高于静噪门限才返回音频
                        datas.Add(dataAudio);
                }
            }
        }

        //移除空的数据项
        datas.RemoveAll(x => x == null);
        //如果本包是TimedataInfo则检查是否可以切换到下一个频点
        if (isTimeData) NextFrequency(_currLevel);
        return datas;
    }

    /// <summary>
    ///     可能是FixFQ/FSCNE/MSCNE中的频谱数据
    /// </summary>
    /// <param name="info"></param>
    /// <param name="setFrequency"></param>
    /// <param name="setSpan"></param>
    /// <param name="setResBwHz"></param>
    private object ToSpectrum(SweepDataInfo info, double setFrequency, double setSpan, int setResBwHz)
    {
        //校验起始频率,结束频率,分辨率带宽保证频谱的正确性
        //向设备设置的频率
        var setStartFreq = setFrequency - setSpan / 1000 / 2;
        var setStopFreq = setFrequency + setSpan / 1000 / 2;
        if (info.RealStartFrequency > setStartFreq || info.RealStopFrequency < setStopFreq ||
            info.RealResBwHz != setResBwHz) return null;
        var step = (info.RealStopFrequency - info.RealStartFrequency) / info.Data.Length;
        var nStartPos = (int)((setStartFreq - info.RealStartFrequency) / step);
        var nStopPos = (int)((setStopFreq - info.RealStartFrequency) / step);
        var count = nStopPos - nStartPos + 1;
        if (info.Data.Length - nStartPos < count) count = info.Data.Length - nStartPos;
        if (count <= 0) return null;
        var arrSpec = new short[count];
        for (int i = nStartPos, j = 0; i < nStartPos + count; ++i, ++j)
            arrSpec[j] = (short)((info.Data[i] / 2.0f - 127.5 + info.RefLevel + 107) * 10);
        var dataSpec = new SDataSpectrum
        {
            Frequency = setFrequency,
            Span = setSpan,
            Data = arrSpec
        };
        return dataSpec;
    }

    /// <summary>
    ///     SCAN中的扫描数据
    /// </summary>
    /// <param name="info"></param>
    private object ToScan(SweepDataInfo info)
    {
        //TODO:25K步进处理
        //近似步进扫描
        if (info.RealStartFrequency > StartFrequency || info.RealStopFrequency < StopFrequency ||
            info.RealResBwHz != ResBandWidthHz) return null;
        var realStep = GetStep(info.RealResBwHz);
        var totalcount = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
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
            arrSpec[i] = (short)(10 * (Math.Abs(decimalPart - 0.5d) < 0.000001d ? Math.Max(levelBefore, levelAfter) :
                decimalPart < 0.5 ? levelBefore : levelAfter));
        }

        var dataScan = new SDataScan
        {
            StartFrequency = StartFrequency,
            StopFrequency = StopFrequency,
            StepFrequency = StepFrequency,
            Total = totalcount,
            Offset = 0,
            Data = arrSpec
        };
        return dataScan;
    }

    /// <summary>
    ///     FixFQ/TDOA电平数据
    /// </summary>
    /// <param name="info"></param>
    private object ToLevel(TimeDataInfo info)
    {
        //取平均值
        if (Math.Abs(info.RealFrequency - Frequency) > 1e-9) return null;
        var nLevelCount = 0;
        double fLevelSum = 0;
        for (var i = 0; i < info.DataI.Length; ++i)
            if (info.DataI[i] != 0 || info.DataQ[i] != 0)
            {
                var dataI = info.DataI[i];
                var dataQ = info.DataQ[i];
                var fLevel = dataI * dataI + dataQ * dataQ;
                fLevelSum += Math.Log10(fLevel);
                nLevelCount++;
            }

        var dataLevel = new SDataLevel
        {
            Frequency = Frequency,
            Bandwidth = IfBandwidth,
            Data = fLevelSum == 0 ? 0 : (float)(10 * fLevelSum / nLevelCount)
        };
        return dataLevel;
    }

    /// <summary>
    ///     IQ
    /// </summary>
    /// <param name="info"></param>
    private object ToIq(TimeDataInfo info)
    {
        var dataIq = new SDataIq
        {
            Frequency = Frequency,
            Bandwidth = FilterBandwidth, //IQ数据由FieldTime提供
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
    ///     TDOA
    /// </summary>
    /// <param name="info"></param>
    private object ToTdoa(TimeDataInfo info)
    {
        var dataTdoa = new SDataIq
        {
            Bandwidth = FilterBandwidth,
            Frequency = Frequency,
            Attenuation = info.AgcAtten,
            SamplingRate = info.Samplerate * 1000.0d, //(2*_spectrumSpan/1000d);//TODO:
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
    ///     TDOA功能扫描数据
    /// </summary>
    /// <param name="info"></param>
    private object ToSpectrum(TimeDataInfo info)
    {
        var arrIq = new Complex[info.DataI.Length];
        for (var i = 0; i < info.DataI.Length; ++i) arrIq[i] = new Complex(info.DataI[i], info.DataQ[i]);
        //////////////////////////////////////////
        //float[] arrWin = new float[arrIQ.Length];
        //float fWinCorr = Window(ref arrWin, arrIQ.Length, FilterWindow.Hanning);
        //Complex[] arrIQTemp = new Complex[arrIQ.Length];
        //EnFrame(ref arrIQTemp, arrIQ, arrWin, arrIQ.Length);
        //////////////////////////////////////////
        var arrIqTemp = arrIq;
        Fft(ref arrIqTemp);
        var arrSpec = new double[arrIqTemp.Length];
        for (var i = 0; i < arrIqTemp.Length; ++i)
        {
            var dataI = arrIqTemp[i].Real;
            var dataQ = arrIqTemp[i].Imaginary;
            arrSpec[i] = 10 * Math.Log10(dataI * dataI + dataQ * dataQ) - 20 * Math.Log10(arrIqTemp.Length);
        }

        //采样率是实际带宽的两倍，此处取一半的IQ来画频谱
        var nSpecPoint = arrSpec.Length / 2;
        var nSpecIndex = arrSpec.Length - nSpecPoint / 2;
        var dataSpec = new SDataSpectrum
        {
            Span = IfBandwidth, //TODO:TDOA中只暴露了中频带宽，此处赋中频带宽即可
            Frequency = Frequency,
            Data = new short[nSpecPoint]
        };
        for (var i = 0; i < nSpecPoint; ++i)
            dataSpec.Data[i] = (short)(arrSpec[(nSpecIndex + i) % arrSpec.Length] * 10);
        return dataSpec;
    }

    /// <summary>
    ///     FixFQ
    /// </summary>
    /// <param name="info"></param>
    private object ToItu(TimeDataInfo info)
    {
        var arrIq = new short[info.DataI.Length * 2];
        for (var i = 0; i < info.DataI.Length; ++i)
        {
            arrIq[2 * i] = (short)info.DataI[i];
            arrIq[2 * i + 1] = (short)info.DataQ[i];
        }

        var ituResult = new ItuResult();
        if (!_itu.MeasureItu(_id, Frequency * 1000000, (int)(info.Samplerate * 1000000), XdB, 1, 5000, arrIq,
                ref ituResult)) return null;
        var dAmDepth = ituResult.FAmMod * 100;
        var dFmDev = ituResult.FFmMod > FilterBandwidth * 1000 * 2 ? double.MinValue : ituResult.FFmMod / 1000d;
        var dFmDevPos = ituResult.FFmPos > FilterBandwidth * 1000 * 2 ? double.MinValue : ituResult.FFmPos / 1000d;
        var dFmDevNeg = ituResult.FFmNeg > FilterBandwidth * 1000 * 2
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
    ///     FSCNE/MSCAN/MSCNE的扫描数据
    /// </summary>
    /// <param name="info"></param>
    private object ToScan(TimeDataInfo info)
    {
        var setFrequency = _scanFreqs[_scanFreqIndex].Frequency;
        if (Math.Abs(setFrequency - info.RealFrequency) > 1e-9) return null;
        var nLevelCount = 0;
        double fLevelSum = 0;
        for (var i = 0; i < info.DataI.Length; ++i)
            if (info.DataI[i] != 0 || info.DataQ[i] != 0)
            {
                var dataI = info.DataI[i];
                var dataQ = info.DataQ[i];
                var fLevel = dataI * dataI + dataQ * dataQ;
                fLevelSum += Math.Log10(fLevel);
                nLevelCount++;
            }

        var dataScan = new SDataScan();
        if (CurFeature == FeatureType.FScne)
        {
            dataScan.StartFrequency = StartFrequency;
            dataScan.StopFrequency = StopFrequency;
            dataScan.StepFrequency = StepFrequency;
        }

        dataScan.Total = _scanFreqs.Count;
        dataScan.Offset = _scanFreqIndex;
        dataScan.Data = new[] { fLevelSum == 0 ? (short)0 : (short)(10 * 10 * fLevelSum / nLevelCount) };
        return dataScan;
    }

    /// <summary>
    ///     FixFQ/IFFQ/FSCNE/MSCNE的音频数据
    /// </summary>
    /// <param name="info"></param>
    /// <param name="setFrequency"></param>
    private object ToAudio(DemodDataInfo info, double setFrequency)
    {
        var nRadioTuneFreqMHz = (int)setFrequency;
        var nDdsFreqKHz = (int)((setFrequency - nRadioTuneFreqMHz) * 1000);
        if (info.RadioTuneFreqMHz != nRadioTuneFreqMHz || info.DdsFreqKHz != nDdsFreqKHz) return null;
        var audioSampleRateHz = (int)(info.Samplerate * 1000000);
        return new SDataAudio
        {
            Format = AudioFormat.Pcmono,
            Channels = 1,
            SamplingRate = audioSampleRateHz,
            BitsPerSample = 16,
            BlockAlign = 2,
            BytesPerSecond = 22050 * 2,
            Data = info.DataAudio
        };
    }

    #endregion
}