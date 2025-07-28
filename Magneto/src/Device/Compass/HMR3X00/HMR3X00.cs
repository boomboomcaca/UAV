/*********************************************************************************************
 *
 * 文件名称:		..\Tracker800 V9\Server\Source\Device\Compass\HMR3X00\HMR3X00.cs
 *
 * 作    者:		陈鹏
 *
 * 创作日期:		2017-09-07
 *
 * 修    改:		无
 *
 * 备    注:		HMR3X00罗盘设备
 *
 *********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

#pragma warning disable 1591
namespace Magneto.Device.HMR3X00;

public partial class Hmr3X00 : DeviceBase
{
    #region 构造函数

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="id">模块编号</param>
    public Hmr3X00(Guid id) : base(id)
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

    // 文本流读写对象
    private TextReader _textReader;
    private TextWriter _textWriter;
    private Task _captureTask;
    private Task _dispatcherTask;
    private CancellationTokenSource _captureTokenSource;

    private CancellationTokenSource _dispatcherTokenSource;

    // 为保证数据完整性，使用队列对收到的数据进行操作
    private MQueue<SDataCompass> _queue;

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
            InitCapture();
            InitDispatcher();
            InitCalibration();
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
        _queue = new MQueue<SDataCompass>();
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
            throw new Exception("HMR3X00安装参数格式有误");
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
        _textReader = new StreamReader(_stream);
        _textWriter = new StreamWriter(_stream);
        _textWriter.NewLine = "\r\n";
    }

    private void InitSerialPort(string serialPort)
    {
        var spValue = serialPort.Split(new[] { ',' });
        _serialPort = new SerialPort();
        _serialPort.PortName = spValue[0].Trim().ToUpper();
        _serialPort.BaudRate = spValue.Length >= 2 ? Convert.ToInt32(spValue[1].Trim()) : 19200;
        _serialPort.Parity = Parity.None;
        _serialPort.DataBits = 8;
        _serialPort.StopBits = StopBits.One;
        _serialPort.ReadTimeout = 10000;
        _serialPort.Open();
        _stream = _serialPort.BaseStream;
        _textReader = new StreamReader(_stream);
        _textWriter = new StreamWriter(_stream);
        _textWriter.NewLine = "\r\n";
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

    private void InitCalibration()
    {
        if (!Calibrating) // 是否启用罗盘校准
            return;
        _textWriter.WriteLine("*S");
        Thread.Sleep(500);
        _textWriter.WriteLine("#Dev={0}", OffsetAngle * 10);
        Thread.Sleep(500);
        _textWriter.WriteLine("#Var={0}", Declination * 10);
        Thread.Sleep(500);
        _textWriter.WriteLine("*S");
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

            if (_socket != null)
            {
                _socket.Close();
                _socket = null;
            }

            if (_textReader != null)
            {
                _textReader.Close();
                _textReader = null;
            }

            if (_textWriter != null)
            {
                _textWriter.Close();
                _textWriter = null;
            }

            if (_stream != null)
            {
                _stream.Close();
                _stream = null;
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
            _queue.Dispose();
            _queue = null;
        }
    }

    #endregion

    #region 收发数据

    private void CaptureData()
    {
        var data = string.Empty;
        while (!_captureTokenSource.IsCancellationRequested)
            try
            {
                data = _textReader.ReadLine();
#if DEBUG1
					Debug.WriteLine(data);
#endif
                var angle = 0.0f;
                angle = Convert.ToSingle(data != null && data.IndexOf(",", StringComparison.Ordinal) > 0
                    ? data.Substring(0, data.IndexOf(",", StringComparison.Ordinal))
                    : data);
                var compass = new SDataCompass { Heading = angle };
                _queue.EnQueue(compass);
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
        var epsilon = 0.00001d;
        while (!_dispatcherTokenSource.IsCancellationRequested)
        {
            if (_queue.Count > _maxQueueBuffer)
            {
                _queue.Clear();
                return;
            }

            var data = _queue.DeQueue();
            if (ReportingDirection)
            {
                if (data == null)
                {
                    Thread.Sleep(1);
                    continue;
                }

                if (Math.Abs(data.Heading - _data.Heading) > epsilon)
                {
#if DEBUG
                    Debug.WriteLine("{0}", data.Heading);
#endif
                    SendMessageData(new List<object> { data });
                    _data = data;
                }
            }
        }
    }

    #endregion
}