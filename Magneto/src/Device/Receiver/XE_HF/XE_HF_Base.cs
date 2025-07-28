using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Device.XE_HF.Common;
using Magneto.Device.XE_HF.Protocols;
using Magneto.Device.XE_HF.Protocols.Data;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF;

public partial class XeHf
{
    #region 初始化

    private void InitNetwork()
    {
        //发送Telnet指令
        _telnetSocket ??= new TelnetSocket();
        if (!_telnetSocket.Connected)
        {
            _telnetSocket.Connect(Ip, TelnetPort);
            Trace.WriteLine($"{DeviceInfo?.DisplayName}({Ip})接收机正在初始化,请耐心等待!");
        }

        //2.1.3 Wait 1s until the WBAT software is initialized.
        Thread.Sleep(1000);
        //建立TCP连接
        var connected = false;
        var attempt = 0;
        do
        {
            try
            {
                _cmdSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                    { NoDelay = true };
                _cmdSocket.Connect(Ip, Port);
                connected = true;
            }
            catch (SocketException)
            {
                attempt++;
            }

            if (connected) break;
            Thread.Sleep(1000);
        } while (attempt < 10);

        if (!connected) throw new Exception("设备初始化网络失败");
        //本地用于连接设备所使用的IP
        var localIp = (_cmdSocket.LocalEndPoint as IPEndPoint)?.Address.ToString();
        //初始化接收机，获取版本号、硬件配置等信息
        InitReceiver();
        //建立UDP数据通道
        InitUdpSocket(localIp, out _udpDfSocket, ref _udpDfPort);
        InitUdpSocket(localIp, out _udpBbfftSocket, ref _udpBbfftPort);
        InitUdpSocket(localIp, out _udpNbfftSocket, ref _udpNbfftPort);
        InitUdpSocket(localIp, out _udpAudioSocket, ref _udpAudioPort);
        InitUdpSocket(localIp, out _udpNbituSocket, ref _udpNbituPort);
    }

    private static void InitUdpSocket(string localIp, out Socket udpSocket, ref int localPort)
    {
        var ep = new IPEndPoint(IPAddress.Parse(localIp), localPort);
        udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        udpSocket.Bind(ep);
        localPort = ((IPEndPoint)udpSocket.LocalEndPoint)!.Port;
    }

    private void InitTasks()
    {
        _udpBbfftCaptrueCts = new CancellationTokenSource();
        _udpBbfftCaptrueTask = new Task(UdpBbfftDataCaptrueProc, _udpBbfftCaptrueCts.Token);
        _udpBbfftCaptrueTask.Start();
        _udpNbfftCaptrueCts = new CancellationTokenSource();
        _udpNbfftCaptrueTask = new Task(UdpNbfftDataCaptrueProc, _udpNbfftCaptrueCts.Token);
        _udpNbfftCaptrueTask.Start();
        _udpDfCaptrueCts = new CancellationTokenSource();
        _udpDfCaptrueTask = new Task(UdpDfDataCaptrueProc, _udpDfCaptrueCts.Token);
        _udpDfCaptrueTask.Start();
        _udpAudioCaptrueCts = new CancellationTokenSource();
        _udpAudioCaptrueTask = new Task(UdpAudioDataCaptrueProc, _udpAudioCaptrueCts.Token);
        _udpAudioCaptrueTask.Start();
        _udpNbituCaptrueCts = new CancellationTokenSource();
        _udpNbituCaptrueTask = new Task(UdpItuDataCaptrueProc, _udpNbituCaptrueCts.Token);
        _udpNbituCaptrueTask.Start();
        //数据处理线程
        _udpDataProcessCts = new CancellationTokenSource();
        _udpDataProcessTask = new Task(UdpDataProcessProc, _udpDataProcessCts.Token);
        _udpDataProcessTask.Start();
    }

    /// <summary>
    ///     1. 若XE已被其它服务端或LG319使用，此处可能收不到版本号信息等任何其它消息，等待其它连接关闭不再占用设备后会初始化成功
    ///     2. 偶尔会出现设备不返回质检结果的情况，为了能保证设备各功能运行正常，初始化时必须等到质检 结果的消息才能算是初始化成功
    /// </summary>
    private void InitReceiver()
    {
        _cmdSocket.ReceiveTimeout = 15000; //用于处理情况1
        var state = ClientConnectionState.TcpConnectionEstablished;
        var startTime = DateTime.Now; //用于处理情况2
        var recvBuffer = new byte[1024 * 1024];
        var headerBuffer = new byte[8];
        while (state != ClientConnectionState.ConnectionActive)
        {
            //校验情况2是否超时
            if ((DateTime.Now - startTime).TotalMilliseconds > 15000)
            {
                _telnetSocket.Dispose();
                //情况2需要重新telnet
                throw new Exception("质检消息超时");
            }

            var startIndex = 0;
            //读取消息头
            ReceiveData(recvBuffer, 0, 8);
            Array.Copy(recvBuffer, headerBuffer, 8);
            var header = new MessageHeader(headerBuffer, ref startIndex);
            if (header.ContentSize > 0)
            {
                //读取消息内容
                ReceiveData(recvBuffer, startIndex, header.ContentSize);
                switch (header.MessageId)
                {
                    case MessageId.MreInfoInterfaceVersion:
                        state = ClientConnectionState.ReceivedInterfaceVersion;
                        break;
                    case MessageId.MreResConfigmat:
                        state = ClientConnectionState.ReceivedHardwareConfigResult;
                        break;
                    case MessageId.MreResEtatDispo:
                        state = ClientConnectionState.ReceivedAvailableStatusResult;
                        break;
                    case MessageId.MreResConfiglog:
                        state = ClientConnectionState.ReceivedSoftwareConfigResult;
                        break;
                    case MessageId.MreResTest:
                        state = ClientConnectionState.ReceivedTestResult;
                        break;
                }

                if (header.MessageId == MessageId.MreInfoInterfaceVersion)
                    ParseVersionInfo(recvBuffer);
                else if (header.MessageId == MessageId.MreResConfigmat) ParseHardwareConfigInfo(recvBuffer);
                SetConnectionState(ref state);
            }
        }

        _cmdSocket.ReceiveTimeout = -1;
        _detectTime = DateTime.Now;
        //开启接收线程，接收其它消息
        _tcpDataProcessCts = new CancellationTokenSource();
        _tcpDataProcessTask = new Task(TcpDataProc, _tcpDataProcessCts.Token);
        _tcpDataProcessTask.Start();
        //设置测向质量门限，打开环境模式TODO:
        var dfQualityCmd = new DfQualityThresholdConfig();
        dfQualityCmd.QualityMask.Value = 0;
        SendCmd(dfQualityCmd.GetBytes(), 50);
        //删除所有手动通道
        var manualChannelCmd = new ManualChannelsDeletionRequest();
        manualChannelCmd.NumOfChannels.Value = 0;
        SendCmd(manualChannelCmd.GetBytes());
    }

    private void SetConnectionState(ref ClientConnectionState state)
    {
        switch (state)
        {
            case ClientConnectionState.ReceivedInterfaceVersion:
            {
                //发送硬件查询请求
                var cmd = new HardwareConfigRequest();
                SendCmd(cmd.GetBytes());
                state = ClientConnectionState.SentHardwareConfigRequest;
                break;
            }
            case ClientConnectionState.ReceivedHardwareConfigResult:
            {
                //发送状态查询指令
                var cmd = new AvailableStatusRequest();
                SendCmd(cmd.GetBytes());
                state = ClientConnectionState.SentAvailableStatusRequest;
                break;
            }
            case ClientConnectionState.ReceivedAvailableStatusResult:
            {
                //查询软件配置
                var cmd = new SoftwareConfigRequest();
                SendCmd(cmd.GetBytes());
                state = ClientConnectionState.SentSoftwareConfigRequest;
                break;
            }
            case ClientConnectionState.ReceivedSoftwareConfigResult:
            {
                //发送自检指令
                var cmd = new TestRequest();
                cmd.ForceTest.Value = 1;
                SendCmd(cmd.GetBytes());
                state = ClientConnectionState.SentTestRequest;
                Trace.WriteLine($"{DeviceInfo?.DisplayName}({DeviceId})正在自检,大约需要5秒,请耐心等待!");
                break;
            }
            case ClientConnectionState.ReceivedTestResult:
            {
                state = ClientConnectionState.ConnectionActive;
                break;
            }
        }
    }

    /// <summary>
    ///     解析接收机版本号，连接建立后设备会主动发送过来
    /// </summary>
    /// <param name="recvBuffer"></param>
    private void ParseVersionInfo(byte[] recvBuffer)
    {
        var startIndex = 0;
        var info = new InterfaceVersionInfo(recvBuffer, ref startIndex);
        //保存版本信息
        _version = info.Version.Value;
        Trace.WriteLine($"{DeviceInfo?.DisplayName}({DeviceId})协议版本号为{_version}");
    }

    /// <summary>
    ///     解析接收机硬件配置信息，留作备用
    /// </summary>
    /// <param name="recvBuffer"></param>
    private void ParseHardwareConfigInfo(byte[] recvBuffer)
    {
        var startIndex = 0;
        var info = new HardwareConfigResult(recvBuffer, ref startIndex, _version);
        _antennaSubRanges ??= new Dictionary<int, XeAntennaSubRange>();
        //记录硬件配置信息
        var note = new StringBuilder();
        note.AppendLine("硬件信息");
        var equipmentName = info.EquipmentName.ToString();
        if (equipmentName != "MRE_DHF")
        {
            Trace.WriteLine($"该动态库只支持短波设备(MRE_DHF)，当前连接设备为{equipmentName}");
            throw new Exception("设备型号与动态库不匹配，请检查！");
        }

        note.AppendLine($"设备名称：{equipmentName}");
        for (var i = 0; i < info.NumOfBlocks.Value; ++i)
        {
            note.AppendLine($"单元号：{i}");
            var block = info.Blocks[i];
            for (var j = 0; j < block.NumOfChannels.Value; ++j)
            {
                note.AppendLine($"通道号：{j}");
                var channel = block.Channels[j];
                for (var k = 0; k < channel.NumOfAntennas.Value; ++k)
                {
                    var antenna = channel.Antennas[k];
                    note.AppendLine(
                        $"天线名称：{antenna.AntennaName.ToString()}，天线标识：{antenna.Identification.ToString()}，方向性：{(antenna.Directivity.Value == 1 ? "全向" : "定向")}，极化方式：{(antenna.Polarisation.Value == 1 ? "水平" : "垂直")}，天线分段数：{(int)antenna.NumOfSubRanges.Value}");
                    for (var m = 0; m < antenna.NumOfSubRanges.Value; ++m)
                    {
                        var subrange = antenna.SubRanges[m];
                        note.AppendLine(
                            $"天线分段名：{subrange.Name.ToString()}，最小频率：{subrange.FMin.Value}，最大频率：{subrange.FMax.Value}");
                        if (j == 0)
                        {
                            //TODO: 保留天线分段信息备用
                            var subrangeName = subrange.Name.ToString();
                            var jnumer = int.Parse(subrangeName?.Substring(subrangeName.Length - 1) ?? string.Empty);
                            _antennaSubRanges[jnumer] = new XeAntennaSubRange
                                { Jnumer = jnumer, Fmin = subrange.FMin.Value, Fmax = subrange.FMax.Value };
                        }
                    }
                }
            }
        }

        _antennaSubRanges = _antennaSubRanges.OrderBy(x => x.Key).ToDictionary(k => k.Key, v => v.Value);
        Trace.WriteLine($"{DeviceInfo?.DisplayName}模块（{DeviceId}）：{note}");
    }

    #endregion

    #region 指令发送和数据接收

    /// <summary>
    ///     读取指定长度的数据到数组
    /// </summary>
    /// <param name="recvBuffer">接收数据缓冲区</param>
    /// <param name="offset">缓冲区的偏移</param>
    /// <param name="bytesToRead">要读取的字节数</param>
    private void ReceiveData(byte[] recvBuffer, int offset, int bytesToRead)
    {
        //当前已接收到的字节数
        var totalRecvLen = 0;
        //循环接收数据，确保接收完指定字节数
        while (totalRecvLen < bytesToRead)
        {
            var recvLen = _cmdSocket.Receive(recvBuffer, offset + totalRecvLen, bytesToRead - totalRecvLen,
                SocketFlags.None);
            totalRecvLen += recvLen;
        }
    }

    /// <summary>
    ///     发送指令
    /// </summary>
    /// <param name="sendBuffer">待发送的数据</param>
    /// <param name="interval">发送指令后休眠时间</param>
    public void SendCmd(byte[] sendBuffer, int interval = 0)
    {
        //待发送的数据长度
        var bytesToSend = sendBuffer.Length;
        //总共已发送的字节数
        var totalSentLen = 0;
        //循环发送，确保数据全部发送完毕
        while (totalSentLen < bytesToSend)
        {
            var sentLen = _cmdSocket.Send(sendBuffer, totalSentLen, bytesToSend - totalSentLen, SocketFlags.None);
            totalSentLen += sentLen;
        }

        if (interval > 0) Thread.Sleep(interval);
    }

    #endregion

    #region 资源释放

    private void StopTasks()
    {
        _watcherIntegrationTime?.Stop();
        Utils.CancelTask(_udpBbfftCaptrueTask, _udpBbfftCaptrueCts);
        Utils.CancelTask(_udpNbfftCaptrueTask, _udpNbfftCaptrueCts);
        Utils.CancelTask(_udpAudioCaptrueTask, _udpAudioCaptrueCts);
        Utils.CancelTask(_udpDfCaptrueTask, _udpDfCaptrueCts);
        Utils.CancelTask(_udpNbituCaptrueTask, _udpNbituCaptrueCts);
        Utils.CancelTask(_udpDataProcessTask, _udpDataProcessCts);
        Utils.CancelTask(_tcpDataProcessTask, _tcpDataProcessCts);
    }

    private void ReleaseResource()
    {
        StopTasks();
        Utils.CloseSocket(_udpBbfftSocket);
        Utils.CloseSocket(_udpNbfftSocket);
        Utils.CloseSocket(_udpAudioSocket);
        Utils.CloseSocket(_udpDfSocket);
        Utils.CloseSocket(_udpNbituSocket);
        Utils.CloseSocket(_cmdSocket);
        _telnetSocket?.Dispose();
    }

    #endregion
}