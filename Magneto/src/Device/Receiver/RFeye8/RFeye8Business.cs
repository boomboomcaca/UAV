using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Magneto.Protocol.Define;

namespace Magneto.Device.RFeye8;

public partial class RFeye8
{
    #region 设备消息

    /// <summary>
    ///     与设备建立认证关系
    /// </summary>
    /// <param name="client"></param>
    private void CreateAuthentication(Socket client)
    {
        client.ReceiveTimeout = 5000;
        var state = ClientConnectionState.TcpConnectionEstablished;
        var recvBuffer = new byte[1024 * 1024];
        while (state != ClientConnectionState.ConnectionActive)
        {
            ReceiveData(recvBuffer, 0, DataSize.HeaderSize << 2, client);
            var header = Header.Parse(recvBuffer, 0);
            if (header.HeaderCode != 0xAABBCCDD) continue;
            ReceiveData(recvBuffer, DataSize.HeaderSize << 2, ((int)header.PacketSize - DataSize.HeaderSize) << 2,
                client);
            var footerCode = BitConverter.ToUInt32(recvBuffer, ((int)header.PacketSize - 1) << 2);
            if (footerCode != 0xDDCCBBAA) continue;
            if (header.PacketType == (int)PacketType.Link)
            {
                var packet = Packet.Parse(recvBuffer, 0);
                foreach (var field in packet.ListFieldInfo)
                {
                    switch (field.FieldName)
                    {
                        case PacketKey.LinkFieldServerGreeting:
                            state = ClientConnectionState.ReceivedGreeting;
                            break;
                        case PacketKey.LinkFieldServerAuthReq:
                            state = ClientConnectionState.ReceivedAuthenticationRequest;
                            break;
                        case PacketKey.LinkFieldServerCobfirm:
                            state = ClientConnectionState.ReceivedAuthenticationOk;
                            break;
                    }

                    SetConnectionState(ref state, client);
                }
            }
        }

        client.ReceiveTimeout = -1;
    }

    private void SetConnectionState(ref ClientConnectionState recvState, Socket client)
    {
        switch (recvState)
        {
            case ClientConnectionState.ReceivedGreeting:
            {
                var linkPacket = new Packet();
                linkPacket.BeginPacket(PacketType.Link, -1);
                linkPacket.AddField(PacketKey.LinkFieldClientConnReq, 0);
                linkPacket.AddParamString(PacketKey.LinkParamClientId, Dns.GetHostName());
                linkPacket.EndPacket();
                SendPacket(linkPacket, client);
                recvState = ClientConnectionState.SentClientConnectionRequest;
                break;
            }
            case ClientConnectionState.ReceivedAuthenticationRequest:
            {
                var linkPacket = new Packet();
                linkPacket.BeginPacket(PacketType.Link, -1);
                linkPacket.AddField(PacketKey.LinkFieldClientAuthResp, 0);
                linkPacket.AddParamString(PacketKey.LinkParamClientAuth, "Client Authentication Word");
                linkPacket.EndPacket();
                SendPacket(linkPacket, client);
                recvState = ClientConnectionState.SentAuthenticationResponse;
                break;
            }
            case ClientConnectionState.ReceivedAuthenticationOk:
                recvState = ClientConnectionState.ConnectionActive;
                break;
        }
    }

    /// <summary>
    ///     向设备发送数据请求，根据功能不同来构建请求包
    /// </summary>
    private void SendMediaRequest()
    {
        try
        {
            var requestPacket = new Packet();
            requestPacket.BeginPacket(PacketType.DspLoop, -1);
            switch (CurFeature)
            {
                case FeatureType.FFM:
                    InitFixFqRequestPacket(requestPacket);
                    break;
                case FeatureType.SCAN:
                    InitScanRequestPacket(requestPacket);
                    break;
                case FeatureType.TDOA:
                    InitTdoaRequestPacket(requestPacket);
                    break;
                case FeatureType.MScan:
                case FeatureType.MScne:
                case FeatureType.FScne:
                    InitFreqScanRequestPacket(requestPacket);
                    break;
            }

            requestPacket.EndPacket();
            SendPacket(requestPacket, _tcpSocket);
        }
        catch (ArgumentException e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    /// <summary>
    ///     构造单频测量功能数据请求包
    /// </summary>
    /// <param name="requestPacket"></param>
    private void InitFixFqRequestPacket(Packet requestPacket)
    {
        //TODO: 目前单频测量中的所有数据都通过IQ数据计算得到
        GetMHzAndMilliHz(Frequency, out var nCenterFreqMHz, out var nCenterFreqMilliHz);
        requestPacket.AddField(PacketKey.FieldTime, -1);
        requestPacket.AddParamInt(PacketKey.TimeCenterFreqMHz, nCenterFreqMHz);
        requestPacket.AddParamInt(PacketKey.TimeCenterFreqMilliHz, nCenterFreqMilliHz);
        //计算ITU数据至少需要8192个点，为保证音频连续性，此处统一每次采集8192个点
        const int timeNumSamples = 8192; //(_media & MediaType.ITU) > 0 ? 8192 : 2048;
        requestPacket.AddParamInt(PacketKey.TimeNumSamples, timeNumSamples);
        if (IfBandwidth < 1000)
        {
            requestPacket.AddParamInt(PacketKey.TimeRealTimeBandwidthMHz, 0);
            requestPacket.AddParamInt(PacketKey.TimeRealTimeBandwidthMilliHz, (int)(IfBandwidth * 1000000));
        }
        else
        {
            requestPacket.AddParamInt(PacketKey.TimeRealTimeBandwidthMHz, (int)(IfBandwidth / 1000));
        }

        if (Attenuation != -1) requestPacket.AddParamInt(PacketKey.TimeManualAtten, Attenuation);
        requestPacket.AddParamInt(PacketKey.TimeAntennaUid, _antennaCode);
    }

    /// <summary>
    ///     构造频段扫描功能数据请求包
    /// </summary>
    /// <param name="requestPacket"></param>
    private void InitScanRequestPacket(Packet requestPacket)
    {
        GetMHzAndMilliHz(StartFrequency, out var nStartFreqMHz, out var nStartFreqMilliHz);
        GetMHzAndMilliHz(StopFrequency, out var nStopFreqMHz, out var nStopFreqMilliHz);
        ResBandWidthHz = GetRbwHz(CurFeature, StepFrequency);
        requestPacket.AddField(PacketKey.FieldSweep, -1);
        requestPacket.AddParamInt(PacketKey.SweepStartFreqMHz, nStartFreqMHz);
        requestPacket.AddParamInt(PacketKey.SweepStartFreqMilliHz, nStartFreqMilliHz);
        requestPacket.AddParamInt(PacketKey.SweepStopFreqMHz, nStopFreqMHz);
        requestPacket.AddParamInt(PacketKey.SweepStopFreqMilliHz, nStopFreqMilliHz);
        requestPacket.AddParamInt(PacketKey.SweepResBandwidthHz, ResBandWidthHz);
        requestPacket.AddParamInt(PacketKey.SweepInput, _antennaCode);
        requestPacket.AddParamInt(PacketKey.SweepRefLevel, SweepRefLevel);
        requestPacket.AddParamInt(PacketKey.SweepNumLoops, Detector == DetectMode.Fast ? 1 : SweepNumLoops);
        if (Attenuation != -1) requestPacket.AddParamInt(PacketKey.SweepManualAtten, Attenuation);
        requestPacket.AddParamInt(
            Detector == DetectMode.Pos ? PacketKey.SweepGetPeakData : PacketKey.SweepGetAverageData, 1);
    }

    /// <summary>
    ///     构造TDOA数据请求包
    /// </summary>
    /// <param name="requestPacket"></param>
    private void InitTdoaRequestPacket(Packet requestPacket)
    {
        GetMHzAndMilliHz(Frequency, out var nCenterFreqMHz, out var nCenterFreqMilliHz);
        requestPacket.AddField(PacketKey.FieldTime, -1);
        requestPacket.AddParamInt(PacketKey.TimeCenterFreqMHz, nCenterFreqMHz);
        requestPacket.AddParamInt(PacketKey.TimeCenterFreqMilliHz, nCenterFreqMilliHz);
        requestPacket.AddParamInt(PacketKey.TimeNumSamples, TimeNumSamples);
        if (IfBandwidth < 1000)
        {
            requestPacket.AddParamInt(PacketKey.TimeRealTimeBandwidthMHz, 0);
            requestPacket.AddParamInt(PacketKey.TimeRealTimeBandwidthMilliHz, (int)(IfBandwidth * 1000000));
        }
        else
        {
            requestPacket.AddParamInt(PacketKey.TimeRealTimeBandwidthMHz, (int)(IfBandwidth / 1000));
        }

        if (Attenuation != -1) requestPacket.AddParamInt(PacketKey.TimeManualAtten, Attenuation);
        requestPacket.AddParamInt(PacketKey.TimeAntennaUid, _antennaCode);
        //设置触发模式
        requestPacket.AddParamInt(PacketKey.TimeTrigModeGps, 1);
    }

    /// <summary>
    ///     构造频率点扫描数据请求包（包括FSCNE, MSCAN, MSCNE）
    /// </summary>
    /// <param name="requestPacket"></param>
    private void InitFreqScanRequestPacket(Packet requestPacket)
    {
        //当前的频点信息
        var frequency = _scanFreqs[_scanFreqIndex].Frequency;
        var ifBw = _scanFreqs[_scanFreqIndex].IfBandWidth;
        //同单频测量，所有数据均由IQ计算得到
        GetMHzAndMilliHz(frequency, out var nCenterFreqMHz, out var nCenterFreqMilliHz);
        requestPacket.AddField(PacketKey.FieldTime, -1);
        requestPacket.AddParamInt(PacketKey.TimeCenterFreqMHz, nCenterFreqMHz);
        requestPacket.AddParamInt(PacketKey.TimeCenterFreqMilliHz, nCenterFreqMilliHz);
        //计算ITU数据至少需要8192个点
        const int timeNumSamples = 8192; //(_media & MediaType.ITU) > 0 ? 8192 : 2048;
        requestPacket.AddParamInt(PacketKey.TimeNumSamples, timeNumSamples);
        if (ifBw < 1000)
        {
            requestPacket.AddParamInt(PacketKey.TimeRealTimeBandwidthMHz, 0);
            requestPacket.AddParamInt(PacketKey.TimeRealTimeBandwidthMilliHz, (int)(ifBw * 1000000));
        }
        else
        {
            requestPacket.AddParamInt(PacketKey.TimeRealTimeBandwidthMHz, (int)(ifBw / 1000));
        }

        if (Attenuation != -1) requestPacket.AddParamInt(PacketKey.TimeManualAtten, Attenuation);
        requestPacket.AddParamInt(PacketKey.TimeAntennaUid, _antennaCode);
    }

    #endregion

    #region 请求发送与数据接收

    /// <summary>
    ///     发送数据请求
    /// </summary>
    /// <param name="packet">数据请求</param>
    /// <param name="socket">要发送数据的套接字</param>
    private void SendPacket(Packet packet, Socket socket)
    {
        try
        {
            var packetBuffer = packet.GetBytes();
            //当前待发送数据请求包的长度（字节数）
            var packetLen = packetBuffer.Length;
            //总共已发送的字节数
            var totalSentLen = 0;
            //循环发送，确保数据全部发送完毕
            while (totalSentLen < packetLen)
            {
                var sentLen = socket.Send(packetBuffer, totalSentLen, packetLen - totalSentLen, SocketFlags.None);
                totalSentLen += sentLen;
            }
        }
        catch (Exception)
        {
        }
    }

    /// <summary>
    ///     接收数据包
    /// </summary>
    /// <param name="socket"></param>
    /// <returns>可能为null(如收到无效数据等)</returns>
    private Packet ReceivePacket(Socket socket)
    {
        var recvBuffer = new byte[4];
        //收取数据头并校验
        ReceiveData(recvBuffer, 0, 4, socket);
        var headerCode = BitConverter.ToUInt32(recvBuffer, 0);
        if (headerCode != 0xAABBCCDD) return null;
        //收取包头并解析
        Array.Resize(ref recvBuffer, DataSize.HeaderSize << 2);
        ReceiveData(recvBuffer, 4, (DataSize.HeaderSize - 1) << 2, socket);
        var header = Header.Parse(recvBuffer, 0);
        if (header.PacketSize <= DataSize.HeaderSize) return null;
        //收取剩下的数据包,并校验包尾
        Array.Resize(ref recvBuffer, (int)header.PacketSize << 2);
        ReceiveData(recvBuffer, DataSize.HeaderSize << 2, ((int)header.PacketSize - DataSize.HeaderSize) << 2, socket);
        var footerCode = BitConverter.ToUInt32(recvBuffer, recvBuffer.Length - 4);
        if (footerCode != 0xDDCCBBAA) return null;
        //解析数据包并返回
        return Packet.Parse(recvBuffer, 0);
    }

    /// <summary>
    ///     读取指定长度的数据到数组
    /// </summary>
    /// <param name="recvBuffer">接收数据缓冲区</param>
    /// <param name="offset">缓冲区的偏移</param>
    /// <param name="bytesToRead">要读取的字节数</param>
    /// <param name="socket">要接收数据的套接字</param>
    private void ReceiveData(byte[] recvBuffer, int offset, int bytesToRead, Socket socket)
    {
        //当前已接收到的字节数
        var totalRecvLen = 0;
        //循环接收数据，确保接收完指定字节数
        while (totalRecvLen < bytesToRead)
        {
            var recvLen = socket.Receive(recvBuffer, offset + totalRecvLen, bytesToRead - totalRecvLen,
                SocketFlags.None);
            if (recvLen <= 0)
                //远程主机使用close或shutdown关闭连接，并且所有数据已被接收的时候此处不会抛异常而是立即返回0，
                //为避免出现此情况将导致该函数死循环，此处直接抛SocketException异常
                //10054:远程主机强迫关闭了一个现有连接
                throw new SocketException(10054);
            totalRecvLen += recvLen;
        }
    }

    #endregion

    #region FSCNE/MSCAN/MSCNE

    private void FreqsSwitchProc()
    {
        InitScanFreqs();
        ResetReceivedFlags();
        _switchEvent = new AutoResetEvent(false);
        while (!_stopSwitch && _freqsSwitchTokenSource?.IsCancellationRequested == false)
            try
            {
                SendMediaRequest();
                _watcherHoldTime.Start();
                _switchEvent.WaitOne();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
            }

        _switchEvent.Close();
        _switchEvent = null;
    }

    private void InitScanFreqs()
    {
        _scanFreqIndex = 0;
        _scanFreqs.Clear();
        if (CurFeature == FeatureType.FScne)
        {
            var freq = StartFrequency;
            var i = 0;
            while (freq <= StopFrequency)
            {
                _scanFreqs.Add(new ScanFreqInfo(freq, IfBandwidth, DemMode));
                freq = decimal.ToDouble(new decimal(StartFrequency + ++i * StepFrequency / 1000d)); //保证频率精度
            }
        }
        else if ((CurFeature & (FeatureType.MScan | FeatureType.MScne)) > 0)
        {
            for (var i = 0; i < MscanPoints.Length; ++i)
            {
                var dic = MscanPoints[i];
                var template = (MscanTemplate)dic;
                var freq = template.Frequency;
                var ifBw = template.FilterBandwidth;
                var demMode = template.DemMode;
                _scanFreqs.Add(new ScanFreqInfo(freq, ifBw, demMode));
            }
        }
    }

    private void NextFrequency(float level)
    {
        bool result;
        if (CurFeature == FeatureType.MScan)
        {
            result = _bReceivedScan;
        }
        else
        {
            if (level >= SquelchThreshold && _watcherDwellTime.ElapsedMilliseconds == 0)
            {
                _watcherDwellTime.Start();
                _watcherHoldTime.Reset();
            }

            result = _bReceivedScan && (_watcherHoldTime.ElapsedMilliseconds > HoldTime * 1000f ||
                                        _watcherDwellTime.ElapsedMilliseconds > DwellTime * 1000f);
        }

        if (!result) return;
        //清理未处理数据，重置标志位
        _scanFreqIndex = ++_scanFreqIndex % _scanFreqs.Count;
        _dataQueue.Clear();
        ResetReceivedFlags();
        ClearBuffer();
        //释放信号量，切换频点
        _switchEvent?.Set();
    }

    /// <summary>
    ///     重置频点、离散、驻留离散扫描中的各个标志
    /// </summary>
    private void ResetReceivedFlags()
    {
        _bReceivedScan = false;
        _watcherHoldTime.Reset();
        _watcherDwellTime.Reset();
    }

    /// <summary>
    ///     清理用于实现检波方式的缓存变量
    /// </summary>
    private void ClearBuffer()
    {
        Array.Clear(_arrSpecBuffer, 0, _arrSpecBuffer.Length);
        _specCount = 0;
        _levelSum = 0;
        _levelCount = 0;
        Array.Clear(_arrAudioBuffer, 0, _arrAudioBuffer.Length);
        _audioCount = 0;
    }

    #endregion
}