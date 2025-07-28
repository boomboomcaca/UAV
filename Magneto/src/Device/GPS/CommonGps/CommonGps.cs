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

namespace Magneto.Device.CommonGps;

public partial class CommonGps : DeviceBase
{
    #region 构造函数

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="id">模块编号</param>
    public CommonGps(Guid id) : base(id)
    {
    }

    #endregion

    #region 变量

    // 最大队列缓存长度
    private readonly int _maxQueueBuffer = 500;

    // 匹配串口通信的正则表达式，如：COM1 或 COM1, 9600 或 COM1 , 9600等
    private readonly string _serialPortReg =
        @"^COM\d+(\s*,\s*(110|300|600|1200|2400|4800|9600|14400|19200|38400|56000|57600|115200|128000|256000))?$";

    // 匹配网口通信的正则表达式，如tcp://192.168.1.100:1000或192.168.1.100：1000，匹配项3对应tcp，4对应192.168.1.100，12对应1000
    private readonly string _networkReg =
        @"^(((tcp):\/\/){0,1})(((([1-9]?|1\d)\d|2([0-4]\d|5[0-5]))\.){3}(([1-9]?|1\d)\d|2([0-4]\d|5[0-5])))\:(\d{1,5})$";

    // 缓存的最新GPS数据，主要用于与后续收到的数据进行比对
    private SDataGps _data;
    private SerialPort _serialPort;

    private Socket _socket;

    // 控制/数据流
    private Stream _stream;

    // 文本(GPS字符串)流读取对象
    private TextReader _textReader;
    private Task _captureTask;
    private Task _dispatcherTask;
    private CancellationTokenSource _captureTokenSource;

    private CancellationTokenSource _dispatcherTokenSource;

    // 为保证数据完整性，使用队列对收到的数据进行操作
    private MQueue<SDataGps> _queue;

    #endregion

    #region 重写父类方法

    // 初始化模块
    public override bool Initialized(ModuleInfo moduleInfo)
    {
        try
        {
            ReleaseResources();
            var result = base.Initialized(moduleInfo);
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

    // 释放模块点用的资源
    public override void Dispose()
    {
        ReleaseResources();
        base.Dispose();
    }

    #endregion

    #region 初始化

    // 初始化成员
    private void InitMember()
    {
        _data = new SDataGps();
        _queue = new MQueue<SDataGps>();
    }

    // 初始化连接
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
            throw new Exception("安装参数格式有误");
    }

    // 初始化网络连接
    private void InitNetwork(string[] socket)
    {
        // 已通过正则表达式过滤
        var ip = socket[1];
        var port = Convert.ToInt32(socket[2]);
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _socket.Connect(ip, port);
        _socket.NoDelay = true;
        _socket.ReceiveTimeout = 10000;
        //_socket.ReceiveBufferSize = 4096;
        _stream = new NetworkStream(_socket);
        _textReader = new StreamReader(_stream);
    }

    // 初始化串口连接
    private void InitSerialPort(string serialPort)
    {
        var spValue = serialPort.Split(new[] { ',' });
        _serialPort = new SerialPort();
        _serialPort.PortName = spValue[0].Trim().ToUpper();
        _serialPort.BaudRate = spValue.Length >= 2 ? Convert.ToInt32(spValue[1].Trim()) : 9600; // 默认为9600
        _serialPort.Parity = Parity.None;
        _serialPort.DataBits = 8;
        _serialPort.StopBits = StopBits.One;
        _serialPort.ReadTimeout = 10000; // 设置超时，用于异常检测
        //_serialPort.ReadBufferSize = 4096;
        _serialPort.Open();
        _stream = _serialPort.BaseStream;
        _textReader = new StreamReader(_stream);
    }

    // 初始化采集数据线程
    private void InitCapture()
    {
        _captureTokenSource = new CancellationTokenSource();
        _captureTask = new Task(CaptureData, _captureTokenSource.Token);
        _captureTask.Start();
    }

    // 初始化发送数据线程
    private void InitDispatcher()
    {
        _dispatcherTokenSource = new CancellationTokenSource();
        _dispatcherTask = new Task(DispatchData, _dispatcherTokenSource.Token);
        _dispatcherTask.Start();
    }

    #endregion

    #region 释放资源

    // 释放当前模块点用的资源
    private void ReleaseResources()
    {
        ReleaseThreads();
        ReleaseConnection();
        ReleaseMisc();
    }

    // 释放连接
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

    // 释放线程资源
    private void ReleaseThreads()
    {
        Utils.CancelTask(_captureTask, _captureTokenSource);
        Utils.CancelTask(_dispatcherTask, _dispatcherTokenSource);
    }

    // 释放其它资源
    private void ReleaseMisc()
    {
        if (_queue != null)
        {
            _queue.Dispose();
            _queue = null;
        }
    }

    #endregion

    #region 接收和转发数据

    // 接收数据线程
    private void CaptureData()
    {
        var data = string.Empty;
        while (!_captureTokenSource.IsCancellationRequested)
            try
            {
                data = ReadLine();
#if DEBUG
                Debug.WriteLine(data);
#endif
                if (!IsValidInput(data)) continue;
                // 解析收到的数据
                ParseData(data);
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
                    return;
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

    // 发送数据线程
    private void DispatchData()
    {
        var previouTimeStamp = DateTime.MinValue;
        while (!_dispatcherTokenSource.IsCancellationRequested)
        {
            // 为保证不占用过度应用程序内存，为防止收数据速度快过发数据速度，当队列大小超过1000时，强行清空
            if (_queue.Count > _maxQueueBuffer)
            {
                _queue.Clear();
                Thread.Sleep(1);
                continue;
            }

            // 为保证收发一致性，无论数据是否用于报告位置信息，都必须将已有数据进行出队操作
            var data = _queue.DeQueue();
            if (ReportingPosition)
            {
                if (data == null)
                {
                    Thread.Sleep(2);
                    continue;
                }

                var distance = GetDistanceByPosition(_data.Latitude, _data.Longitude, data.Latitude, data.Longitude);
                var currentTimeStamp = DateTime.Now;
                var timeSpan = currentTimeStamp - previouTimeStamp;
                if (distance > 2 || timeSpan.TotalMilliseconds > 10000)
                {
#if DEBUG
                    Console.WriteLine($"GPS: {data.Latitude}, {data.Longitude}");
#endif
                    previouTimeStamp = currentTimeStamp;
                    SendMessageData(new List<object> { data });
                    _data = data;
                }
            }
        }
    }

    #endregion

    #region 解析数据

    // 解析收到的数据
    private void ParseData(string data)
    {
        // 数据是否合法由方法内部判断，
        // 传入值最多只能被一种方法解析
        ParseRmc(data);
        ParseGga(data);
        ParseGll(data);
    }

    // 解析GGA
    private void ParseGga(string data)
    {
        // 筛选GPGGA/BDGGA/GNGGA，分别表示GPS/北斗/GPS+北斗
        var reg = new Regex(@"^(\$\w{2}GGA)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!reg.IsMatch(data)) return;
#if DEBUG1
            Debug.WriteLine(data);
#endif
        // 如果当前GPS设备不作为位置信息公布设备，则不进行后续操作
        if (!ReportingPosition) return;
        var dataArray = data.Split(new[] { ',' });
        if (dataArray.Length < 8) return;
        // “1”为单点定位，“2”为伪距差分定位，其它为无效定位或未定位
        if (!dataArray[6].Equals("1") && !dataArray[6].Equals("2")) return;
        var lat = ToDegree(dataArray[2]);
        var lon = ToDegree(dataArray[4]);
        if (lat == null || lon == null) return;
        if (dataArray[3].ToLower().Equals("s")) lat *= -1;
        if (dataArray[5].ToLower().Equals("w")) lon *= -1;
        var gps = new SDataGps
        {
            Latitude = lat.Value,
            Longitude = lon.Value
        };
        _queue.EnQueue(gps);
    }

    // 解析GGL
    private void ParseGll(string data)
    {
        // 筛选GPGLL/BDGLL/GNGLL，分别表示GPS/北斗/GPS+北斗
        var reg = new Regex(@"^(\$\w{2}GLL)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!reg.IsMatch(data)) return;
#if DEBUG1
            Debug.WriteLine(data);
#endif
        // 如果当前GPS设备不作为位置信息公布设备，则不进行后续操作
        if (!ReportingPosition) return;
        var dataArray = data.Split(new[] { ',' });
        if (dataArray.Length < 7) return;
        // 未定位
        if (!dataArray[6].ToLower().Equals("a")) return;
        var lat = ToDegree(dataArray[1]);
        var lon = ToDegree(dataArray[3]);
        if (lat == null || lon == null) return;
        if (dataArray[2].ToLower().Equals("s")) lat *= -1;
        if (dataArray[4].ToLower().Equals("w")) lon *= -1;
        var gps = new SDataGps
        {
            Latitude = lat.Value,
            Longitude = lon.Value
        };
        _queue.EnQueue(gps);
    }

    // 解析GSA
    private void ParseGsa()
    {
        //TODO: 按实际需求编写
    }

    // 解析GSV
    private void ParseGsv()
    {
        //TODO: 按实际需求编写
    }

    // 解析RMC
    private void ParseRmc(string data)
    {
        // 筛选GPRMC/BDRMC/GNRMC，分别表示GPS/北斗/GPS+北斗
        var reg = new Regex(@"^(\$\w{2}RMC)", RegexOptions.IgnoreCase);
        if (!reg.IsMatch(data)) return;
#if DEBUG1
            Debug.WriteLine(data);
#endif
        var dataArray = data.Split(new[] { ',' });
        if (dataArray.Length < 12) return;
        // 未定位
        if (!dataArray[2].ToLower().Equals("a")) return;
        if (Timing) SynchronizeDateTime(dataArray[1], dataArray[9]);
        // 如果当前GPS设备不作为位置信息公布设备，则不进行后续操作
        if (!ReportingPosition) return;
        var lat = ToDegree(dataArray[3]);
        var lon = ToDegree(dataArray[5]);
        var dec = ToDeclination(dataArray[10]);
        if (lat == null || lon == null) return;
        dec ??= 0;
        if (dataArray[4].ToLower().Equals("s")) lat *= -1;
        if (dataArray[6].ToLower().Equals("w")) lon *= -1;
        if (dataArray[11].ToLower().Equals("w")) dec *= -1;
        var gps = new SDataGps
        {
            Latitude = lat.Value,
            Longitude = lon.Value
        };
        _queue.EnQueue(gps);
    }

    // 解析VTG
    private void ParseVtg()
    {
        //TODO: 按实际需求编写
    }

    // 解析ZDA
    private void ParseZda()
    {
        //TODO: 按实际需求编写
    }

    // 解析GST
    private void ParseGst()
    {
        //TODO: 按实际需求编写
    }

    #endregion

    #region Helper

    // 从数据流中读取一行字符
    private string ReadLine()
    {
        try
        {
            return _textReader.ReadLine();
        }
        catch (Exception ex)
        {
            if (ex is NullReferenceException) return string.Empty;
            throw;
        }
    }

    // 将字符串转换为浮点数度，原始格式为：ddmm.mmmmmm
    private double? ToDegree(string value)
    {
        try
        {
            var raw = decimal.Parse(value);
            raw = raw / 100;
            decimal deg = (int)raw;
            var min = (raw - deg) * 100;
            var result = (double)(deg + min / 60);
            return result;
        }
        catch
        {
            return null;
        }
    }

    // 转换为磁偏角
    private float? ToDeclination(string value)
    {
        try
        {
            var raw = float.Parse(value);
            raw = (short)(raw * 10);
            return raw;
        }
        catch
        {
            return null;
        }
    }

    // 校验数据是否正确
    private bool IsValidInput(string value)
    {
        // 不是'$'开头的数据不合法
        if (!value.StartsWith("$")) return false;
        // 不是*分隔的数据不合法
        var dataArray = value.Split(new[] { '*' });
        if (dataArray.Length < 2) return false;
        // 从*后面的为校验和的十六进制字符表达形式
        var checkSum = dataArray[1].Trim();
        int result;
        var i = 0;
        // 计算$到*之间（不包括$和*）数据的校验和
        for (result = value[1], i = 2; value[i] != '*'; ++i) result ^= value[i];
        // 将计算结果转换为16进制字符串形式，与输出的校验和进行比较，判断数据是否合法
        if (checkSum.Equals(Convert.ToString(result, 16), StringComparison.InvariantCultureIgnoreCase)) return true;
        return false;
    }

    // 同步时钟，从GPS获取到的时候为UTC时间
    private void SynchronizeDateTime(string time, string date)
    {
        var year = Convert.ToInt32("20" + date.Substring(4, 2));
        var month = Convert.ToInt32(date.Substring(2, 2));
        var day = Convert.ToInt32(date.Substring(0, 2));
        var hour = Convert.ToInt32(time.Substring(0, 2));
        var minute = Convert.ToInt32(time.Substring(2, 2));
        var second = Convert.ToInt32(time.Substring(4, 2));
        // 难道有时会没有毫秒???
        // int millisecond = Convert.ToInt32(time.Substring(7, 3));
        // DateTime dateTime = new DateTime(year, month, day, hour, minute, second, millisecond);
        var dateTime = new DateTime(year, month, day, hour, minute, second);
        var deltaSeconds = (dateTime.ToLocalTime() - DateTime.Now.ToLocalTime()).TotalSeconds;
        // 为避免频繁操作，当时间间隔超过预设值过后才同步时钟
        if (Math.Abs(deltaSeconds) >= 5) SetSystemDateTime(dateTime);
    }

    //获取两个点的距离，单位米
    private double GetDistanceByPosition(double lantitude1, double longitude1, double lantitude2, double longitude2)
    {
        var dLat1InRad = lantitude1 * (Math.PI / 180);
        var dLong1InRad = longitude1 * (Math.PI / 180);
        var dLat2InRad = lantitude2 * (Math.PI / 180);
        var dLong2InRad = longitude2 * (Math.PI / 180);
        var dLongitude = dLong2InRad - dLong1InRad;
        var dLatitude = dLat2InRad - dLat1InRad;
        var a = Math.Pow(Math.Sin(dLatitude / 2), 2) +
                Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) * Math.Pow(Math.Sin(dLongitude / 2), 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var dDistance = 6378.137 * c * 1000;
        return dDistance;
    }

    #endregion
}