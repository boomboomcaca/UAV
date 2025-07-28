/***************************************************************
 *
 * 类    名: Client
 * 作    者: 侯华
 * 创作日期: 2020-9-24 9:59:47
 * 功能概述：
 *
 * --------------修改记录------------
 * 修改时间：xxxx.xx.xx    修改者：xxx
 * 修改说明：
 *
 ***************************************************************/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Protocol.Data;

namespace Magneto.Device;

/// <summary>
///     设备连接客户端类
/// </summary>
internal class Client : IDisposable
{
/*
    /// <summary>
    ///     超时时间 1s
    /// </summary>
    private const int ClearTime = 1;
*/

    private readonly string _deviceIp;
    private readonly int _devicePort;
    private readonly Dictionary<string, FlightInfo> _flightCache = new();
    private readonly int _interval;
    private readonly ConcurrentQueue<string> _queue;
    private CancellationTokenSource _cts;
    private Ping _ping;
    private DateTime _preSendTime = DateTime.Now;
    private Task[] _taskArray;
    public Action<SDataAdsB> OnData;

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="deviceIp">设备IP地址</param>
    /// <param name="devicePort">设备数据端口</param>
    /// <param name="interval">发送间隔</param>
    public Client(string deviceIp, int devicePort, int interval)
    {
        _interval = interval;
        _deviceIp = deviceIp;
        _devicePort = devicePort;
        _queue = new ConcurrentQueue<string>();
        _ping = new Ping();
    }

    public Socket Sokcet { get; private set; }

    /// <summary>
    ///     释放资源
    /// </summary>
    public void Dispose()
    {
        Stop();
        if (Sokcet != null)
        {
            Sokcet.Close();
            Sokcet.Dispose();
            Sokcet = null;
        }

        if (_ping != null)
        {
            _ping.Dispose();
            _ping = null;
        }
    }

    /// <summary>
    ///     Ping设备网络是否连通
    /// </summary>
    /// <returns>true=网络正常；false=网络未连通</returns>
    public bool PingDevice()
    {
        var reply = _ping.Send(_deviceIp);
        return reply?.Status == IPStatus.Success;
    }

    /// <summary>
    ///     初始化连接
    /// </summary>
    /// <returns>true=成功；false=失败</returns>
    public bool InitConnect()
    {
        if (!PingDevice()) return false;
        if (Sokcet != null)
        {
            Sokcet.Close();
            Sokcet.Dispose();
            Sokcet = null;
        }

        try
        {
            var point = new IPEndPoint(IPAddress.Parse(_deviceIp), _devicePort);
            Sokcet = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                ReceiveTimeout = 2000
            };
            Sokcet.Connect(point);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     开始
    /// </summary>
    /// <returns>true=成功；false=失败</returns>
    public bool Start()
    {
        StopThread();
        _cts = new CancellationTokenSource();
        _taskArray = new[]
        {
            ReceiveDataAsync(_cts.Token),
            ParseDataAsync(_cts.Token)
        };
        return true;
    }

    /// <summary>
    ///     停止
    /// </summary>
    /// <returns>true=成功；false=失败</returns>
    public void Stop()
    {
        StopThread();
        _queue.Clear();
    }

    /// <summary>
    ///     停止线程
    /// </summary>
    /// <returns>true=成功；false=失败</returns>
    private void StopThread()
    {
        _cts?.Cancel();
        try
        {
            if (_taskArray != null)
                Task.Run(async () => await Task.WhenAll(_taskArray)).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        catch (AggregateException ex)
        {
            foreach (var ie in ex.Flatten().InnerExceptions) Console.WriteLine(ie.ToString());
        }
        finally
        {
            _cts?.Dispose();
        }
    }

    /// <summary>
    ///     接收数据
    /// </summary>
    /// <param name="obj"></param>
    private async Task ReceiveDataAsync(object obj)
    {
        var token = (CancellationToken)obj;
        // 一条飞机数据长度大概是800多，一次有可能会发送多条飞机数据
        var buffer = new byte[1024 * 10];
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(10).ConfigureAwait(false);
            try
            {
                if (Sokcet == null) continue;
                var bytes = Sokcet.Receive(buffer, buffer.Length, 0);
                var page = Encoding.ASCII.GetString(buffer, 0, bytes);
                if (_queue == null) continue;
                _queue.Enqueue(page);
            }
            catch (Exception e)
            {
                Console.WriteLine($"接收数据出错:{e.Message}");
                if (Sokcet?.Connected != true) InitConnect();
            }
        }
    }

    /// <summary>
    ///     解析数据
    /// </summary>
    /// <param name="obj"></param>
    private async Task ParseDataAsync(object obj)
    {
        var token = (CancellationToken)obj;
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(10).ConfigureAwait(false);
            try
            {
                if (_queue.IsEmpty) continue;
                if (!_queue.TryDequeue(out var str)) continue;
                if (string.IsNullOrEmpty(str)) continue;
                if (!str.StartsWith("#A:") && !str.StartsWith("#S:")) continue;
                var list = new List<FlightInfo>();
                foreach (var sp in str.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                    if (sp.StartsWith("#A:"))
                    {
                        //Console.WriteLine($"收到航班信息航班:{sp}");
                        var datum = ParseAircraft(sp);
                        if (string.IsNullOrEmpty(datum.PlaneAddress)) continue;
                        if (string.IsNullOrEmpty(datum.FlightNumber)) continue;
                        if (datum is { Latitude: 0d, Longitude: 0d }) continue;
#if DEBUG
                        Console.WriteLine(
                            $"    ICAO:{datum.PlaneAddress}   呼号:{datum.FlightNumber}   应答机编码:{datum.TransponderCode}  位置:{datum.Longitude}E,{datum.Latitude}N   水平速度:{datum.HorizontalSpeed}km/h    垂直速度:{datum.VerticalSpeed}m/s   方向:{datum.Azimuth}    海拔:{datum.Altitude}m");
#endif
                        list.Add(datum);
                    }
                    //统计
                    else if (sp.StartsWith("#S:"))
                    {
                    }

                if (list.Count > 0) list.ForEach(item => _flightCache[item.PlaneAddress] = item);
                if (DateTime.Now.Subtract(_preSendTime).TotalMilliseconds > _interval)
                {
                    if (_flightCache.Count > 0)
                    {
                        var data = new SDataAdsB
                        {
                            Data = _flightCache.Values.ToList()
                        };
                        _flightCache.Clear();
                        OnData?.Invoke(data);
                    }

                    _preSendTime = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理数据出错:{ex.Message}");
            }
        }
    }

    private static FlightInfo ParseAircraft(string str)
    {
        if (!str.StartsWith("#A:")) return default;
        str = str.Remove(0, 3);
        var craftSplit = str.Split(',');
        if (craftSplit.Length < 15) return default;
        // #A:781C09,, CCA4063,1500,30.68742,104.00337,6450,55,261,1920,783,100,0, 100, ,  3, 8A2F
        //    0      1 2       3    4        5         6    7  8   9    10  11  12 13   14 15 16
        /*
            Index   #A          Aircraft message start indicator            Example value   Remark
            0       ICAO        ICAO number of aircraft (3 bytes)           3C65AC          航空公司代码+?
            1       FLAGS       Flags bitfield, see table 7                 1               标记
            2       CALL        Callsign of aircraft                        N61ZP           呼号
            3       SQ          SQUAWK of aircraft                          7232            应答机编码
            4       LAT         Latitude, in degrees                        57.57634        纬度
            5       LON         Longitude, in degrees                       17.59554        经度
            6       ALT_BARO    Barometric altitude, in feets               5000            海拔(英尺)
            7       TRACK       Track of aircraft, in degrees [0,360)       35              航向(°)
            8       VELH        Horizontal velocity of aircraft, in knots   464             水平速度(节)
            9       VELV        Vertical velocity of aircraft, in ft/min ­   1344            垂直速度(英尺/分)
            10      SIGS        Signal strength, in mV                      840             信号强度(mV)
            11      SIGQ        Signal quality, in mV                       72              信号质量(mV)
            12      FPS         Number of raw MODE­S frames received from    5               最后一秒从飞机接收到的原始MODE-S帧数
                                aircraft during last second
            13      NICNAC      NIC/NAC bitfield, see table 8 (v2.6.0+)     31B
            14      ALT_GEO     Geometric altitude, in feets (v2.6.0+)      5000
            15      ECAT        Emitter category, see table 9 (v2.7.0+)     14
            16      CRC         CRC16 (described in CRC section)            2D3E
        */
        /*
            FM–上海航空公司 774　 　CA–国际航空公司 999
            CJ–北方航空公司 782　 　SZ–西南航空公司 785
            CZ–南方航空公司 784　　 WH–西北航空公司 783
            3U–四川航空公司 C10　 　SC–山东航空公司 C07
            MU–东方航空公司 781　 　4G–深圳航空公司 C09
            MF–厦门航空公司 731　 　HU–海南航空公司 880
            F6–中国航空股份有限公司　 　XO–新疆航空公司 651
            G8–长城航空公司　 　G4–贵州省航空公司
            8C–山西航空公司　 　KA–港龙航空公司
            PO–邮政航空公司　 　NX–澳门航空公司
            IV–福建航空公司 791　 　2Z–长安航空公司
            WU–武汉航空公司 C12　 　3Q–云南航空公司 592
            XW–中国新华航空公司 779
        */
        // TODO : 需要对照协议
        // int.TryParse(craftSplit[3], out int sq); // 这个
        _ = double.TryParse(craftSplit[4], out var latitude);
        _ = double.TryParse(craftSplit[5], out var longitude);
        _ = double.TryParse(craftSplit[6], out var altitude); // 海拔高度，单位英尺
        // var alt = altitude / 0.0032808;
        // var altmm = alt / 100 * 100;
        // var altM = altmm / 1000;
        var altM = altitude * 0.3048; // 1英尺=0.3048米
        _ = int.TryParse(craftSplit[7], out var track);
        _ = int.TryParse(craftSplit[8], out var horVelocity);
        _ = int.TryParse(craftSplit[9], out var verVelocity);
        return new FlightInfo
        {
            PlaneAddress = craftSplit[0],
            Altitude = Math.Round(altM, 3),
            TransponderCode = craftSplit[3], // SQUAWK of aircraft 为应答机编码
            Latitude = latitude,
            Longitude = longitude,
            Azimuth = track,
            HorizontalSpeed = Math.Round(horVelocity * 1.852, 3), // 1节=1.852km/h
            VerticalSpeed = Math.Round(verVelocity * 0.3048 / 60, 3), // 1英尺=0.3038米 1英尺/分= 0.3048/60 米/秒
            UpdateTime = Utils.GetNowTimestamp(),
            Model = string.Empty,
            Age = 0,
            Country = "",
            FlightNumber = craftSplit[2]
        };
    }
}