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
using Magneto.Protocol.Define;

namespace Magneto.Device.DDF550;

public partial class Ddf550
{
    private readonly object _lockObject = new();
    private string _compassName;

    /// <summary>
    ///     释放资源
    /// </summary>
    private void ReleaseResource()
    {
        Utils.CancelTask(_dataProcTask, _dataProcCts);
        Utils.CancelTask(_dataProcIqTask, _dataProcIqCts);
        Utils.CancelTask(_dataReceiveTask, _dataReceiveCts);
        Utils.CancelTask(_dataReceiveIqTask, _dataReceiveIqCts);
        Utils.CloseSocket(_dataReceiveIqSocket);
        Utils.CloseSocket(_dataReceiveSocket);
        Utils.CloseSocket(_cmdSocket);
    }

    /// <summary>
    ///     根据IQ数据计算电平值
    /// </summary>
    /// <returns></returns>
    private float GetLevelByIq(short[] iq)
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
    private bool IsLastHop(int dfStatus)
    {
        return (dfStatus & 0x10) > 0;
    }

    /// <summary>
    ///     根据DFStatus判断测向数据是否进行了方位校正
    /// </summary>
    /// <param name="dfStatus"></param>
    private bool IsCorrectionData(int dfStatus)
    {
        return ((dfStatus >> 20) & 0x01) > 0;
    }

    #region 私有函数

    /// <summary>
    ///     初始化设备
    /// </summary>
    private void InitDevice()
    {
        SetModeRx();
        //HwRefresh(out msg);

        #region 初始化时下发的信息

        QueryCommand("DeviceLock");
        ScanRangeDeleteAll();
        SearchRangeDeleteAll();
        SendCommand("AntennaLevelSwitch", "eAntLevelSwitch", EAntLevelSwitch.AntlevelOff.ToString());
        TraceDeleteInactive(out _);
        SendCommand(CmdIfmode, "eIfMode", EifMode.If16Bit.ToString());
        SendCommand("TriggerSettings", "eTriggerMode", ETriggerMode.DftriggermodeDisabled.ToString());

        #endregion

        SendCommand("Volume", "iDemodVolume", "100");
        SendCommand("Volume", "iMainVolume", "100");
        var list = GetHwInfo();
        var compass = "";
        var compasses = list.Where(item => item.HwType == EhwType.HwCompass);
        _compassName = string.Empty;
        foreach (var item in compasses)
            if (item is { HwType: EhwType.HwCompass, HwStatus: EhwStatus.HwStatusOk } and
                    { Port: >= 0, Version: { MainVersion: > 0, SubVersion: > 0 } }
                && item.Name != "Software"
                && !item.Name.Contains("COG")
                && (string.IsNullOrEmpty(compass) ||
                    string.Equals(item.Name, compass, StringComparison.OrdinalIgnoreCase)))
                _compassName = item.Name;
        foreach (var item in list)
        {
            if (item.HwType != EhwType.HwDfAntenna || item.HwStatus != EhwStatus.HwStatusOk) continue;
            var property = GetAntennaProperties(item.Name);
            var info = GetAntennaSetup(property.Name);
            if (info.AntennaName != property.Name) continue;
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

            //if ((info.FreqBegin <= 20000001 || info.FreqEnd <= 1300000001)
            //    && info.RfInput != ERF_Input.RF_INPUT_VUSHF1)
            //{
            //    sign = true;
            //    info.RfInput = ERF_Input.RF_INPUT_VUSHF1;
            //}
            //if ((info.FreqBegin >= (1300000000 - 1))
            //   && info.RfInput != ERF_Input.RF_INPUT_VUSHF2)
            //{
            //    sign = true;
            //    info.RfInput = ERF_Input.RF_INPUT_VUSHF2;
            //}
            if (info.CompassName != "Software") compass = info.CompassName;
            if (UseCompass && !string.IsNullOrEmpty(_compassName))
            {
                sign = true;
                info.CompassName = _compassName;
            }

            if (sign) AntennaSetup(info);
        }

        if (UseGps || UseCompass)
            CheckGpsCompassSwitch(true);
        else
            CheckGpsCompassSwitch(false);
    }

    /// <summary>
    ///     初始化网络套接字
    /// </summary>
    private bool InitSocket()
    {
        // 通过Xml控制设备的端口固定为设备端口(默认5555)+8;
        // 设备进行大数据返回的端口固定为设备端口(默认5555)+10;
        try
        {
            // Xml协议命令下发与接收返回值
            // Xml数据端口为5555+8
            var localXmlIp = Ip;
            var localXmlPort = 0;
            InitTcpSocket(Ip, Port + 8, "XML通道", out _cmdSocket, ref localXmlIp, ref localXmlPort);
            // 从设备接收大数据
            // 接收数据的端口为5555+10
            InitTcpSocket(Ip, Port + 10, "数据通道", out _dataReceiveSocket, ref _localIp, ref _localPort);
            // 从设备接收大数据
            // 接收数据的端口为5555+10
            InitTcpSocket(Ip, Port + 10, "IQ数据通道", out _dataReceiveIqSocket, ref _localIqIp, ref _localIqPort);
            return true;
        }
        catch
        {
            return false;
        }
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
            Trace.WriteLine($"{name}==>{ip}:{(socket.LocalEndPoint as IPEndPoint)?.Port}");
            localIp = (socket.LocalEndPoint as IPEndPoint)?.Address.ToString();
            localPort = ((IPEndPoint)socket.LocalEndPoint)!.Port;
        }
        catch (SocketException)
        {
            Console.WriteLine($"创建{name}TCP连接失败");
            socket = null;
            throw;
        }
    }

    /// <summary>
    ///     初始化线程
    /// </summary>
    private void InitThread()
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
    ///     数据处理
    /// </summary>
    private void DataProc()
    {
        while (_dataProcCts?.IsCancellationRequested == false)
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
            catch (Exception e)
            {
#if DEBUG
                Trace.WriteLine(e.ToString());
#endif
            }
    }

    /// <summary>
    ///     IQ数据处理
    /// </summary>
    private void DataProcIq()
    {
        while (_dataProcIqCts?.IsCancellationRequested == false)
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
            catch (Exception ex)
            {
                Trace.WriteLine($"处理IQ数据异常，异常信息：{ex}");
                _dataIqQueue.Clear();
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
                    ToGpsCompass(data as RawGpsCompass);
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
        if (_cmdSocket == null)
        {
            Trace.WriteLine("命令通道未初始化");
            return null;
        }

        var result = string.Empty;
        if (_cmdReply != null) Array.Clear(_cmdReply, 0, _cmdReply.Length);
        lock (_lockObject)
        {
            // 发送
            _cmdSocket.Send(cmd, 0, cmd.Length, SocketFlags.None);
            try
            {
                while (true)
                {
                    var buffer = new byte[40960];
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
                        // Console.WriteLine("返回:" + result);
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

    /// <summary>
    ///     装包
    ///     Xml数据格式如下:
    ///     Xml包头+数据长度+截止符0x00+Xml包尾
    /// </summary>
    /// <param name="cmd"></param>
    /// <returns></returns>
    private byte[] PackedXmlCommand(byte[] cmd)
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
    private bool CheckXmlCommand(byte[] data)
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
}