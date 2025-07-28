/*********************************************************************************************
 *
 * 文件名称:		..\Tracker800 V9\Server\Source\Device\Compass\SEC345\SEC345.cs
 *
 * 作    者:		陈鹏
 *
 * 创作日期:		2017-11-03
 *
 * 修    改:		无
 *
 * 备    注:		SEC345罗盘设备
 *
 *********************************************************************************************/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

#pragma warning disable 1591
namespace Magneto.Device.SEC345;

public partial class Sec345 : DeviceBase
{
    #region 构造函数

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="id">模块编号</param>
    public Sec345(Guid id) : base(id)
    {
    }

    #endregion

    #region 成员变量

    /// <summary>
    ///     最大队列缓存长度
    /// </summary>
    private readonly int _maxQueueBuffer = 500;

    /// <summary>
    ///     匹配串口通信的正则表达式，如：COM1 或 COM1 或 以“/”开关，如：/dev/ttyUSB0, 9600 或 COM1 , 9600等
    /// </summary>
    private readonly string _serialPortReg =
        @"^((COM\d+)|\/\S+)(\s*,\s*(110|300|600|1200|2400|4800|9600|14400|19200|38400|56000|57600|115200|128000|256000))?$";

    /// <summary>
    ///     匹配网口通信的正则表达式，如tcp://192.168.1.100:1000或192.168.1.100：1000，匹配项3对应tcp，4对应192.168.1.100，12对应1000
    /// </summary>
    private readonly string _networkReg =
        @"^(((tcp):\/\/){0,1})(((([1-9]?|1\d)\d|2([0-4]\d|5[0-5]))\.){3}(([1-9]?|1\d)\d|2([0-4]\d|5[0-5])))\:(\d{1,5})$";

    /// <summary>
    ///     缓存的最新罗盘数据，主要用于与后续收到的数据进行比对
    /// </summary>
    private SDataCompass _data;

    private SerialPort _serialPort;
    private Socket _socket;

    /// <summary>
    ///     控制流
    /// </summary>
    private Stream _stream;

    private Thread _capture;
    private Thread _dispatcher;

    /// <summary>
    ///     为保证数据完整性，使用队列对收到的数据进行操作
    /// </summary>
    private ConcurrentQueue<SDataCompass> _queue;

    private bool _isRunning;

    #endregion

    #region 重写父类方法

    public override bool Initialized(ModuleInfo mi)
    {
        try
        {
            ReleaseResources();
            var result = base.Initialized(mi);
            _isRunning = true;
            InitMember();
            InitConnection();
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
        var serialPort = Connection; //string.Empty;
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
    }

    private void InitSerialPort(string serialPort)
    {
        try
        {
            var spValue = serialPort.Split(new[] { ',' });
            _serialPort = new SerialPort
            {
                PortName = spValue[0].Trim(),
                BaudRate = spValue.Length >= 2 ? Convert.ToInt32(spValue[1].Trim()) : 115200,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                ReadTimeout = 10000
            };
            _serialPort.Open();
            _stream = _serialPort.BaseStream;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"SEC345 init error:{ex.Message}");
            throw;
        }
    }

    private void InitCapture()
    {
        if (_capture?.IsAlive != true)
        {
            _capture = new Thread(CaptureData)
            {
                Name = "hmr3x00_direction_capture",
                IsBackground = true
            };
            _capture.Start();
        }
    }

    private void InitDispatcher()
    {
        if (_dispatcher?.IsAlive != true)
        {
            _dispatcher = new Thread(DispatchData)
            {
                Name = "hmr3x00_direction_dispatcher",
                IsBackground = true
            };
            _dispatcher.Start();
        }
    }

    #endregion

    #region 释放资源

    private void ReleaseResources()
    {
        _isRunning = false;
        ReleaseThreads();
        ReleaseConnection();
        ReleaseMisc();
    }

    private void ReleaseThreads()
    {
        if (_capture?.IsAlive == true)
        {
            _capture.Join();
            _capture = null;
        }

        if (_dispatcher?.IsAlive == true)
        {
            _dispatcher.Join();
            _dispatcher = null;
        }
    }

    private void ReleaseConnection()
    {
        try
        {
            if (_serialPort?.IsOpen == true)
            {
                _serialPort.Close();
                _serialPort = null;
            }

            if (_socket != null)
            {
                _socket.Close();
                _socket = null;
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
            _queue.Clear();
            _queue = null;
        }
    }

    #endregion

    #region 收发数据

    private void CaptureData()
    {
        while (_isRunning)
            try
            {
                var array = ReadArray();
                if (IsValidInput(array))
                {
                    var data = ParseData(array);
                    if (data != null) _queue.Enqueue(data);
                }
                else
                {
                    Thread.Sleep(5);
                }
            }
            catch (Exception ex)
            {
                if (ex is IOException)
                {
                    Trace.WriteLine($"设备{DeviceInfo.DisplayName}连接故障");
                    var info = new SDataMessage
                    {
                        LogType = LogType.Warning,
                        ErrorCode = (int)InternalMessageType.DeviceRestart,
                        Description = DeviceId.ToString(),
                        Detail = DeviceInfo.DisplayName
                    };
                    SendMessage(info);
                    break;
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
        const double epsilon = 0.00001d;
        while (_isRunning)
        {
            if (_queue.Count > _maxQueueBuffer)
            {
                _queue.Clear();
                continue;
            }

            if (_queue.IsEmpty)
            {
                Thread.Sleep(10);
                continue;
            }

            _queue.TryDequeue(out var data);
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

    #region Helper

    /// <summary>
    ///     读取字节数组
    ///     解析字节数组为罗盘数据
    /// </summary>
    /// <param name="data"></param>
    private SDataCompass ParseData(byte[] data)
    {
        if (data == null || data.Length < 5) return null;
        var heading = new byte[3];
        // 只关心命令字为0x14, 0x83, 0x84的值
        switch (data[2])
        {
            case 0x14:
                // 输出所有值的情况下，只取方位角 
                Array.Copy(data, 9, heading, 0, 3);
                break;
            case 0x83:
                // 只输出方位角
                Array.Copy(data, 3, heading, 0, 3);
                break;
            case 0x84:
                // 一组角度的情况下，只取方位角
                Array.Copy(data, 9, heading, 0, 3);
                break;
        }

        // 方位角是以6位十六进制值表示
        var stringValue = $"{heading[0]:x2}{heading[1]:x2}.{heading[2]:x2}";
        // 第一位是符号位
        stringValue = stringValue[1..];
        if (!float.TryParse(stringValue, out var value)) return null;
        var angle = heading[0] >= 0x10 ? -value : value;
        // 加补偿角度值
        angle += ExtraAngle;
        // 保证范围在0~360之间
        angle = (angle % 360 + 360) % 360;
        return new SDataCompass
        {
            Heading = angle
        };
    }

    private byte[] ReadArray()
    {
        if (_stream == null) return null;
        // 读取头
        var header = _stream.ReadByte();
        if (header != 0x77) return null;
        // 读取有效数据的长度
        var total = _stream.ReadByte();
        if (total <= 0) return null;
        // 读取数据
        var data = new byte[total];
        data[0] = (byte)total;
        // 总共的数据长度是包含了当前表示数据长度的字节，因此后续还要收的数据长度应该减1
        total--;
        var offset = 1;
        var received = 0;
        while (total > 0)
        {
            received = _stream.Read(data, offset, total);
            offset += received;
            total -= received;
        }

        return data;
    }

    /// <summary>
    ///     校验字节数组是否为有效的罗盘数据
    /// </summary>
    /// <param name="input"></param>
    private bool IsValidInput(byte[] input)
    {
        if (input == null || input.Length < 2) return false;
        var checkedValue = 0;
        // 计算校验和
        for (var index = 0; index < input.Length - 1; ++index) checkedValue += input[index];
        // 与校验和进行比较，判断数据是否有效
        return (byte)checkedValue == input[^1];
    }

    #endregion
}