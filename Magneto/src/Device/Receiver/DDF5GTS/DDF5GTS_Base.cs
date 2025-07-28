using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.DDF5GTS;

public partial class Ddf5Gts
{
    #region 私有函数

    /// <summary>
    ///     释放资源
    /// </summary>
    private void ReleaseResource()
    {
        Utils.CancelTask(_dataProcTask, _dataProcCts);
        Utils.CancelTask(_dataProcIqTask, _dataProcIqCts);
        Utils.CancelTask(_dataGpsCompassTask, _dataGpsCompassCts);
        Utils.CancelTask(_dataReceiveTask, _dataReceiveCts);
        Utils.CancelTask(_dataReceiveIqTask, _dataReceiveIqCts);
        Utils.CloseSocket(_cmdSocket);
        Utils.CloseSocket(_oldSocket);
        Utils.CloseSocket(_dataReceiveSocket);
        Utils.CloseSocket(_dataReceiveIqSocket);
    }

    /// <summary>
    ///     初始化设备
    /// </summary>
    private void InitDevice()
    {
        QueryCommand("Options");
        SetModeRx();
        HwRefresh();
        SendCommand("Volume", "iDemodVolume", "100");
        SendCommand("Volume", "iMainVolume", "100");
        var list = GetHwInfo();
        var compass = "";
        foreach (var item in list)
        {
            if (item is { HwType: EhwType.HwDfAntenna, HwStatus: EhwStatus.HwStatusOk })
            {
                var property = GetAntennaProperties(item.Name);
                var info = GetAntennaSetup(property.Name);
                if (info.AntennaName == property.Name)
                {
                    var sign = false;
                    if (info.FreqBegin < property.FreqBegin)
                    {
                        sign = true;
                        info.FreqBegin = property.FreqBegin;
                    }

                    if (info.FreqEnd > property.FreqEnd)
                    {
                        sign = true;
                        info.FreqEnd = property.FreqEnd;
                    }

                    if (Math.Abs(info.FreqBegin - info.FreqEnd) < _epsilon)
                    {
                        sign = true;
                        info.FreqBegin = property.FreqBegin;
                        info.FreqEnd = property.FreqEnd;
                    }

                    if (info.CompassName != "Software") compass = info.CompassName;
                    if (sign) AntennaSetup(info);
                }

                if (!_antennaName.Contains(info.AntennaName)) _antennaName.Add(info.AntennaName);
            }

            if (item is { HwType: EhwType.HwCompass, HwStatus: EhwStatus.HwStatusOk })
                if (item is { Port: >= 0, Version: { MainVersion: > 0, SubVersion: > 0 } } && item.Name != "Software" &&
                    (string.IsNullOrEmpty(compass) ||
                     string.Equals(item.Name, compass, StringComparison.OrdinalIgnoreCase)))
                    _compassName = item.Name;
        }
    }

    /// <summary>
    ///     初始化网络套接字
    /// </summary>
    private void InitSocket()
    {
        // 通过Xml控制设备的端口固定为设备端口(默认5555)+8;
        // 设备进行大数据返回的端口固定为设备端口(默认5555)+10;        
        var oldIp = Ip;
        var oldPort = Port;
        InitTcpSocket(Ip, Port, "SCPI通道", out _oldSocket, ref oldIp, ref oldPort);
        // Xml协议命令下发与接收返回值
        // Xml数据端口为5555+8
        var cmdIp = Ip;
        var cmdPort = Port + 8;
        InitTcpSocket(Ip, Port + 8, "Xml通道", out _cmdSocket, ref cmdIp, ref cmdPort);
        // 从设备接收大数据
        // 接收数据的端口为5555+10
        InitTcpSocket(Ip, Port + 10, "普通数据通道", out _dataReceiveSocket, ref _localIp, ref _localPort);
        // 从设备接收大数据
        // 接收数据的端口为5555+10
        InitTcpSocket(Ip, Port + 10, "IQ数据通道", out _dataReceiveIqSocket, ref _localIqIp, ref _localIqPort);
    }

    private static void InitTcpSocket(string ip, int port, string name, out Socket socket, ref string localIp,
        ref int localPort)
    {
        try
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true
            };
            socket.Connect(ipEndPoint);
            Trace.WriteLine($"{name}==>{ip}:{((IPEndPoint)socket.LocalEndPoint)!.Port}");
            localIp = (socket.LocalEndPoint as IPEndPoint)?.Address.ToString();
            localPort = ((IPEndPoint)socket.LocalEndPoint).Port;
        }
        catch (SocketException)
        {
            socket = null;
        }
    }

    // 初始化天线
    private void InitAntennas()
    {
        var str = AntennaUsed();
        Console.WriteLine("当前打通的天线：" + str);
        //if (_antennas != null)
        //{
        //    foreach (var antenna in _antennas)
        //    {
        //        long freqBegin = (long)(antenna.StartFrequency * 1000000);
        //        long freqEnd = (long)(antenna.StopFrequency * 1000000);
        //        Enum.TryParse<ERF_Input>(antenna.RfInput, out ERF_Input rfInput);
        //        Enum.TryParse<EHF_Input>(antenna.HfInput, out EHF_Input hfInput);
        //        Enum.TryParse<ERx_Path>(antenna.RfRxPath, out ERx_Path rfRxPath);
        //        Enum.TryParse<ERx_Path>(antenna.HfRxPath, out ERx_Path hfRxPath);
        //        bool res = AntennaSetup(antenna.AntennaName,
        //                                freqBegin,
        //                                freqEnd,
        //                                antenna.CompassName,
        //                                antenna.NorthCorrection,
        //                                antenna.RollCorrection,
        //                                antenna.PitchCorrection,
        //                                rfInput,
        //                                hfInput,
        //                                rfRxPath,
        //                                hfRxPath,
        //                                antenna.CtrlPort,
        //                                antenna.GpsRead,
        //                                out string msg);
        //    }
        //}
    }

    /// <summary>
    ///     初始化线程
    /// </summary>
    private void InitTasks()
    {
        _dataReceiveCts = new CancellationTokenSource();
        _dataReceiveTask = new Task(DataReceive, _dataReceiveCts.Token);
        _dataReceiveTask.Start();
        _dataReceiveIqCts = new CancellationTokenSource();
        _dataReceiveIqTask = new Task(DataReceiveIq, _dataReceiveIqCts.Token);
        _dataReceiveIqTask.Start();
        _dataProcCts = new CancellationTokenSource();
        _dataProcTask = new Task(DataProc, _dataProcCts.Token);
        _dataProcTask.Start();
        _dataProcIqCts = new CancellationTokenSource();
        _dataProcIqTask = new Task(DataProcIq, _dataProcIqCts.Token);
        _dataProcIqTask.Start();
        _dataGpsCompassCts = new CancellationTokenSource();
        _dataGpsCompassTask = new Task(GpsCompassProc, _dataGpsCompassCts.Token);
        _dataGpsCompassTask.Start();
    }

    /// <summary>
    ///     数据接收
    /// </summary>
    private void DataReceive()
    {
        var tmp = new byte[4];
        var buffer = new byte[1024 * 1024];
        const int headLen = 16;
        while (_dataReceiveCts?.IsCancellationRequested == false)
            try
            {
                // 读取包头
                ReceiveData(buffer, 0, 4, _dataReceiveSocket);
                // 判断包头是否是000EB200
                if (buffer[0] == 0x00 && buffer[1] == 0x0E && buffer[2] == 0xB2 && buffer[3] == 0x00)
                {
                    // 读取头结构
                    ReceiveData(buffer, 4, headLen - 4, _dataReceiveSocket);
                    // 解析数据长度
                    Buffer.BlockCopy(buffer, headLen - 4, tmp, 0, 4);
                    Array.Reverse(tmp);
                    var totalLen = BitConverter.ToInt32(tmp, 0);
                    // 读取剩下的数据
                    ReceiveData(buffer, headLen, totalLen - headLen, _dataReceiveSocket);
                    //if (_taskState == TaskState.Start)
                    {
                        var data = new byte[totalLen];
                        Buffer.BlockCopy(buffer, 0, data, 0, totalLen);
                        _dataQueue.Enqueue(data);
                    }
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
    ///     IQ数据接收
    /// </summary>
    private void DataReceiveIq()
    {
        var tmp = new byte[4];
        var buffer = new byte[1024 * 1024];
        const int headLen = 16;
        while (_dataReceiveIqCts?.IsCancellationRequested == false)
            try
            {
                // 读取包头
                ReceiveData(buffer, 0, 4, _dataReceiveIqSocket);
                // 判断包头是否是000EB200
                if (buffer[0] == 0x00 && buffer[1] == 0x0E && buffer[2] == 0xB2 && buffer[3] == 0x00)
                {
                    // 读取头结构
                    ReceiveData(buffer, 4, headLen - 4, _dataReceiveIqSocket);
                    // 解析数据长度
                    Buffer.BlockCopy(buffer, headLen - 4, tmp, 0, 4);
                    Array.Reverse(tmp);
                    var totalLen = BitConverter.ToInt32(tmp, 0);
                    // 读取剩下的数据
                    ReceiveData(buffer, headLen, totalLen - headLen, _dataReceiveIqSocket);
                    if (TaskState == TaskState.Start)
                    {
                        var data = new byte[totalLen];
                        Buffer.BlockCopy(buffer, 0, data, 0, totalLen);
                        _dataIqQueue.Enqueue(data);
                    }
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

    /// <summary>
    ///     关闭所有开关
    /// </summary>
    private void SetAllSwitchOff()
    {
        _iqSwitch = false;
        _ituSwitch = false;
        _squelchSwitch = false;
        _spectrumSwitch = false;
        _audioSwitch = false;
        SendCommand(CmdItu, "bEnableMeasurement", _ituSwitch.ToString().ToLower());
        SendCommand(CmdDemodulationsettings, "bUseAfThreshold", _squelchSwitch.ToString().ToLower());
        _mediaType = MediaType.None;
    }

    #endregion

    #region Xml数据处理

    /// <summary>
    ///     发送命令并等待数据返回以后获取返回数据
    /// </summary>
    /// <param name="cmd"></param>
    /// <returns>返回应答信息</returns>
    private byte[] SendCmd(byte[] cmd)
    {
        try
        {
            var result = string.Empty;
            Array.Clear(_cmdReply, 0, _cmdReply.Length);
            lock (this)
            {
                // 发送
                _cmdSocket.Send(cmd, 0, cmd.Length, SocketFlags.None);
                try
                {
                    while (true)
                    {
                        var buffer = new byte[4096];
                        // 读取包头
                        ReceiveData(buffer, 0, 4, _cmdSocket);
                        if (buffer[0] == XmlStart[0] && buffer[1] == XmlStart[1] && buffer[2] == XmlStart[2] &&
                            buffer[3] == XmlStart[3])
                        {
                            // 读取头结构
                            ReceiveData(buffer, 4, 4, _cmdSocket);
                            var lenArr = new byte[4];
                            Buffer.BlockCopy(buffer, 4, lenArr, 0, 4);
                            Array.Reverse(lenArr);
                            var len = BitConverter.ToInt32(lenArr, 0);
                            // 读取剩下的数据
                            ReceiveData(buffer, 8, len + 4, _cmdSocket);
                            var data = new byte[len + 12];
                            Buffer.BlockCopy(buffer, 0, data, 0, data.Length);
                            if (!CheckXmlCommand(data)) return null;
                            // 从尾部查找0x00,Xml反序列化的时候不能正确识别截止符,因此需要将截止符去掉
                            for (var i = data.Length - 5; i >= 0; i--)
                                if (data[i] == 0x00)
                                    len--;
                                else
                                    break;
                            var xmlData = new byte[len];
                            Buffer.BlockCopy(data, 8, xmlData, 0, len);
                            result = Encoding.ASCII.GetString(xmlData).TrimEnd('\0');
                            Console.WriteLine("返回:" + result);
                            if (result.Substring(result.Length - 8, 8) == "</Reply>") return xmlData;
                        }
                    }
                }
                catch
                {
                    return null;
                }
            }
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     装包
    ///     Xml数据格式如下:
    ///     Xml包头+数据长度+截止符0x00+Xml包尾
    /// </summary>
    /// <param name="cmd"></param>
    /// <returns></returns>
    private static byte[] PackedXmlCommand(byte[] cmd)
    {
        var arr = new List<byte>();
        arr.AddRange(XmlStart);
        var len = cmd.Length + 1; //cmd长度+1位截止符0x00
        var lenArr = BitConverter.GetBytes(len);
        if (BitConverter.IsLittleEndian) Array.Reverse(lenArr);
        arr.AddRange(lenArr);
        arr.AddRange(cmd);
        arr.Add(0x00); //添加截止符\0
        arr.AddRange(XmlEnd);
        return arr.ToArray();
    }

    /// <summary>
    ///     数据校验（校验帧头帧尾和数据长度）
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private static bool CheckXmlCommand(byte[] data)
    {
        var lenData = new byte[4];
        if (data[0] != XmlStart[0]
            || data[1] != XmlStart[1]
            || data[2] != XmlStart[2]
            || data[3] != XmlStart[3])
            return false;
        if (data[data.Length - 4] != XmlEnd[0]
            || data[data.Length - 3] != XmlEnd[1]
            || data[data.Length - 2] != XmlEnd[2]
            || data[data.Length - 1] != XmlEnd[3])
            return false;
        Buffer.BlockCopy(data, 4, lenData, 0, 4);
        if (BitConverter.IsLittleEndian) Array.Reverse(lenData);
        var len = BitConverter.ToInt32(lenData, 0);
        return len == data.Length - 12;
    }

    #endregion Xml数据处理

    #region 数据开关

    /// <summary>
    ///     禁用所有数据的返回
    /// </summary>
    private void DisableAllSwitch()
    {
        //_dataQueue.Clear();
        TraceDisable(ETraceTag.TracetagSelCall, _localIp, _localPort);
        CheckIqSwitch(false);
        CheckSpectrumSwitch(false);
        CheckCwSwitch(false);
        CheckAudioSwitch(false);
        CheckDfSwitch(false);
        CheckPScanSwitch(false);
        //if ( _curFeature != FeatureType.FFM)
        // 所有测量停止的时候都需要将模式改为Rx
        // 下发参数的时候也需要改为Rx下发参数
        SetModeRx();
    }

    /// <summary>
    ///     检查开关状态,统一打开开关
    ///     DDF5GTS修改参数的时候需要先关闭所有的订阅数据,然后重新订阅-----在现场通过实际设备的测试发现,修改参数的时候不需要关闭数据,可以直接修改参数
    /// </summary>
    private void CheckAllSwitch()
    {
        // 开启任务的时候根据功能不同切换不同的DfMode
        if (CurFeature == FeatureType.FFM)
            SetModeRx();
        else if (CurFeature is FeatureType.FDF or FeatureType.WBDF or FeatureType.SSE)
            SetModeFfm();
        else if (CurFeature == FeatureType.ScanDF)
            SetModeScan();
        else if (CurFeature == FeatureType.SCAN) SetModeRxpscan();
        if (_iqSwitch) CheckIqSwitch(true);
        // 单频测量下CW数据永远返回(里面包含电平数据)
        if (CurFeature == FeatureType.FFM) CheckCwSwitch(true);
        if (_spectrumSwitch && CurFeature == FeatureType.FFM) CheckSpectrumSwitch(true);
        if (_audioSwitch) CheckAudioSwitch(true);
        // 测向功能返回的数据体为DFPScan,都需要开启DF数据
        if (CurFeature is FeatureType.FDF or FeatureType.WBDF or FeatureType.ScanDF or FeatureType.SSE)
        {
            CheckCwSwitch(true);
            CheckSpectrumSwitch(true);
            CheckDfSwitch(true);
        }

        if (CurFeature == FeatureType.SCAN) CheckPScanSwitch(true);
        if (CurFeature is FeatureType.FFM or FeatureType.SCAN) Initiate();
    }

    #endregion 数据开关
}