using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.HCS;

public partial class Hcs : DeviceBase
{
    #region 构造函数

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="id">模块编号</param>
    public Hcs(Guid id) : base(id)
    {
    }

    #endregion

    #region 成员变量

    // 最大队列缓存长度
    private readonly int _maxQueueBuffer = 500;

    // 匹配串口通信的正则表达式，如：COM1 或 COM1, 9600 或 COM1 , 9600等
    private readonly string _serialPortReg =
        @"^COM\d+(\s*,\s*(110|300|600|1200|2400|4800|9600|14400|19200|38400|56000|57600|115200|128000|256000))?$";

    // 匹配网口通信的正则表达式，如tcp://192.168.1.100:1000或192.168.1.100：1000，匹配项3对应tcp，4对应192.168.1.100，12对应1000
    private readonly string _networkReg =
        @"^(((tcp):\/\/){0,1})(((([1-9]?|1\d)\d|2([0-4]\d|5[0-5]))\.){3}(([1-9]?|1\d)\d|2([0-4]\d|5[0-5])))\:(\d{1,5})$";

    // 缓存的最新罗盘数据，主要用于与后续收到的数据进行比对
    private SDataCompass _data;
    private SerialPort _serialPort;

    private Socket _socket;

    // 控制流
    private Stream _stream;
    private StreamReader _streamReader;
    private Task _captureTask;
    private Task _dispatcherTask;
    private CancellationTokenSource _captureTokenSource;

    private CancellationTokenSource _dispatcherTokenSource;

    // 为保证数据完整性，使用队列对收到的数据进行操作
    private ConcurrentQueue<SDataCompass> _queue;

    #endregion

    #region 重写父类方法

    public override bool Initialized(ModuleInfo mi)
    {
        try
        {
            ReleaseResources();
            var result = base.Initialized(mi);
            InitMember();
            InitConnection();
            InitDevice();
            InitCapture();
            InitDispatcher();
            return result;
        }
        catch
        {
            ReleaseResources();
            throw;
        }
    }

    public override void Dispose()
    {
        ReleaseResources();
        base.Dispose();
    }

    #endregion

    #region 初始化

    private void InitMember()
    {
        _data = new SDataCompass();
        _queue = new ConcurrentQueue<SDataCompass>();
    }

    private void InitConnection()
    {
        var serialPort = string.Empty;
        // 匹配串口
        var reg = new Regex(_serialPortReg, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        var match = reg.Match(Connection);
        if (match.Success) serialPort = match.Value;
        var socket = new string[3];
        // 匹配网口
        reg = new Regex(_networkReg, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        match = reg.Match(Connection);
        if (match.Success && match.Groups.Count > 12)
        {
            socket[0] = match.Groups[3].Value;
            socket[1] = match.Groups[4].Value;
            socket[2] = match.Groups[12].Value;
        }

        if (!string.IsNullOrEmpty(socket[1]) && !string.IsNullOrEmpty(socket[2]))
            InitNetwork(socket);
        else if (!string.IsNullOrEmpty(serialPort))
            InitSerialPort(serialPort);
        else
            throw new Exception("HCS安装参数格式有误");
    }

    private void InitNetwork(string[] socket)
    {
        var ip = socket[1];
        var port = Convert.ToInt32(socket[2]);
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _socket.Connect(ip, port);
        _socket.NoDelay = true;
        _socket.ReceiveTimeout = 10000;
        _stream = new NetworkStream(_socket);
        _streamReader = new StreamReader(_stream);
    }

    private void InitSerialPort(string serialPort)
    {
        var spValue = serialPort.Split(new[] { ',' });
        _serialPort = new SerialPort();
        _serialPort.PortName = spValue[0].Trim().ToUpper();
        _serialPort.BaudRate = spValue.Length >= 2 ? Convert.ToInt32(spValue[1].Trim()) : 115200;
        _serialPort.Parity = Parity.None;
        _serialPort.DataBits = 8;
        _serialPort.StopBits = StopBits.One;
        _serialPort.ReadTimeout = 10000;
        _serialPort.Open();
        _stream = _serialPort.BaseStream;
        _streamReader = new StreamReader(_stream);
    }

    private void InitCapture()
    {
        _captureTokenSource = new CancellationTokenSource();
        _captureTask = new Task(CaptureData, _captureTokenSource.Token);
        _captureTask.Start();
    }

    private void InitDispatcher()
    {
        _dispatcherTokenSource = new CancellationTokenSource();
        _dispatcherTask = new Task(DispatchData, _dispatcherTokenSource.Token);
        _dispatcherTask.Start();
    }

    /// <summary>
    ///     初始化设备
    /// </summary>
    private void InitDevice()
    {
        // 设置连续发送模式
        SendCommand("#FA0.3=1*26");
        // #BAD=nnnn*hh
        // 其中nnnn为600，hh为校验和
        // 设置每分钟发送600次（100ms一次）
        // SendCommand(string.Format("#BAD={0}*hh",600))
    }

    #endregion

    #region 释放资源

    private void ReleaseResources()
    {
        ReleaseThreads();
        ReleaseConnection();
        ReleaseMisc();
    }

    private void ReleaseThreads()
    {
        Utils.CancelTask(_captureTask, _captureTokenSource);
        Utils.CancelTask(_dispatcherTask, _dispatcherTokenSource);
    }

    private void ReleaseConnection()
    {
        try
        {
            if (_serialPort is { IsOpen: true })
            {
                _serialPort.Close();
                _serialPort = null;
            }
        }
        catch
        {
        }

        try
        {
            if (_socket != null)
            {
                _socket.Close();
                _socket = null;
            }
        }
        catch
        {
        }

        try
        {
            if (_stream != null)
            {
                _stream.Close();
                _stream = null;
            }
        }
        catch
        {
        }

        try
        {
            if (_streamReader != null)
            {
                _streamReader.Close();
                _streamReader = null;
            }
        }
        catch
        {
        }
    }

    private void ReleaseMisc()
    {
        if (_queue != null)
        {
            _queue.Clear();
            _queue = null;
        }
    }

    #endregion

    #region 收发数据

    private void CaptureData()
    {
        while (!_captureTokenSource.IsCancellationRequested)
            try
            {
                var str = _streamReader.ReadLine();
                if (str != null && IsValidInput(str) && str.Contains("PTNTHPR"))
                {
                    var data = ParseData(str);
                    if (data != null) _queue.Enqueue(data);
                }
                else
                {
                    Thread.Sleep(5);
                }
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException) return;
                if (ex is IOException)
                {
                    Trace.WriteLine($"设备{DeviceInfo.DisplayName}发生I/O异常");
                    var info = new SDataMessage
                    {
                        LogType = LogType.Warning,
                        ErrorCode = (int)InternalMessageType.DeviceRestart,
                        Description = DeviceId.ToString(),
                        Detail = DeviceInfo.DisplayName
                    };
                    SendMessage(info);
                }
                else
                {
                    Trace.WriteLine($"设备{DeviceInfo.DisplayName}发生异常，正尝试恢复...");
                    var info = new SDataMessage
                    {
                        LogType = LogType.Warning,
                        ErrorCode = (int)InternalMessageType.DeviceFault,
                        Description = DeviceId.ToString(),
                        Detail = DeviceInfo.DisplayName
                    };
                    SendMessage(info);
                }
            }
    }

    private void DispatchData()
    {
        var epsilon = 0.1d;
        var previouTimeStamp = DateTime.MinValue;
        while (!_dispatcherTokenSource.IsCancellationRequested)
        {
            if (_queue.Count > _maxQueueBuffer)
            {
                _queue.Clear();
                Thread.Sleep(1);
                continue;
            }

            if (!_queue.TryDequeue(out var data))
            {
                Thread.Sleep(1);
                continue;
            }

            if (ReportingDirection)
            {
                if (data == null)
                {
                    Thread.Sleep(2);
                    continue;
                }

                var currentTimeStamp = DateTime.Now;
                var timeSpan = currentTimeStamp - previouTimeStamp;
                if (Math.Abs(data.Heading - _data.Heading) > epsilon || timeSpan.TotalMilliseconds > 10000)
                {
#if DEBUG
                    Debug.WriteLine($"Compass: {data.Heading}");
#endif
                    previouTimeStamp = currentTimeStamp;
                    SendMessageData(new List<object> { data });
                    _data = data;
                }
            }
        }
    }

    #endregion

    #region Helper

    private void SendCommand(string cmd)
    {
        cmd += "\r\n";
        var buffer = Encoding.ASCII.GetBytes(cmd);
        _stream?.Write(buffer, 0, buffer.Length);
    }

    // 解析数据为罗盘数据
    private SDataCompass ParseData(string data)
    {
        // $PTNTHPR,85.9,N,-0.9,N,0.8,N*2C
        // 把罗盘的三个重要的测量结果和有用的状态信息结合在一起,数据依次代表：航向，磁场状态，俯仰，俯仰状态，横滚，横滚状态，角度单位为度。
        if (data == null || data.Length < 5) return null;
        if (!data.Contains(",")) return null;
        var arr = data.Split(',');
        if (arr.Length < 7) return null;
        if (!float.TryParse(arr[1], out var angle) // 航向角
            || !float.TryParse(arr[3], out var pitch) // 俯仰角
            || !float.TryParse(arr[5], out var roll)) // 横滚角
            return null;
        // 加补偿角度值
        angle += ExtraAngle;
        // 保证范围在0~360之间
        angle = (angle % 360 + 360) % 360;
        return new SDataCompass
        {
            Heading = angle,
            Rolling = roll,
            Pitch = pitch
        };
    }

    // 校验字节数组是否为有效的罗盘数据
    /*
        校验码计算方法
        将“#”或者“$”与“*”之间的，但不含“#”、“$”和“*”的信息的
        每个字符的 8 位 ASCII 码（不包括起始位和停止位）进行异或操作，结果的
        高四位位和剩下的低四位十六进制数字被转化为两个 ASCII 字符（0 – 9，A
        – F）进行发送。高四位字符发送在先。
    */
    private static bool IsValidInput(string input)
    {
        // $PTNTHPR,x.x,a,x.x,a,x.x,a*hh<CR><LF>
        if (string.IsNullOrEmpty(input) || input.Length < 2) return false;
        if (!input.Contains("$") && !input.Contains("#")) return false;
        if (!input.Contains("*")) return false;
        var checkedValue = 0;
        var sign = false;
        // 计算校验和
        for (var index = 0; index < input.Length - 1; ++index)
        {
            if (input[index] == '$' || input[index] == '#')
            {
                sign = true;
                continue;
            }

            if (input[index] == '*') sign = false;
            if (sign) checkedValue ^= input[index];
        }

        var high = (byte)((checkedValue & 0xF0) >> 4);
        var low = (byte)(checkedValue & 0x0F);
        var ascii = high.ToString("X") + low.ToString("X");
        // 与校验和进行比较，判断数据是否有效
        return ascii[0] == input[input.Length - 2] && ascii[1] == input[input.Length - 1];
    }

    #endregion
}