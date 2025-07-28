using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device;

public partial class VirtualAis : DeviceBase
{
    private readonly List<BsInfo> _bsInfos = new();
    private readonly Random _random = new(DateTime.Now.Millisecond);
    private CancellationTokenSource _bsDataCts;
    private Task _bsDataTask;
    private CancellationTokenSource _cts;
    private Task _processTask;

    public VirtualAis(Guid deviceId) : base(deviceId)
    {
    }

    public override bool Initialized(ModuleInfo device)
    {
        var res = base.Initialized(device);
        if (!res) return false;
        Init();
        return true;
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        if (feature != FeatureType.BsDecoding || !_bsInfos.Any()) return;
        _bsDataCts = new CancellationTokenSource();
        _bsDataTask = new Task(BsDataGenerate, _bsDataCts.Token);
        _bsDataTask.Start();
    }

    public override void Stop()
    {
        base.Stop();
        Utils.CancelTask(_bsDataTask, _bsDataCts);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Utils.CancelTask(_bsDataTask, _bsDataCts);
        Utils.CancelTask(_processTask, _cts);
    }

    private void Init()
    {
        _bsInfos.Clear();
        var fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bsdec_station_decode.json");
        if (File.Exists(fileName))
        {
            var str = File.ReadAllText(fileName);
            if (!string.IsNullOrWhiteSpace(str))
                try
                {
                    var list = Utils.ConvertFromJson<List<BsInfo>>(str.Trim());
                    _bsInfos.AddRange(list);
                }
                catch
                {
                }
        }

        var gps = RunningInfo.BufGpsData;
        var lng = gps.Longitude;
        var lat = gps.Latitude;

        #region ADS_B

        _adslngStart = lng - 1;
        _adslngStop = lng + 1;
        _adslatStart = lat - 1;
        _adslatStop = lat + 1;
        // 规定飞机初始飞行角度
        _flightAngle = _flightDic.ToDictionary(p => p.Key, _ => _random.NextSingle() * 359.9f);
        _flightCache = _flightDic.ToDictionary(p => p.Key, p => new FlightInfo
        {
            PlaneAddress = p.Key,
            FlightNumber = p.Value,
            TransponderCode = $"{p.Value}test",
            Longitude = _random.NextSingle() * 2 + _adslngStart,
            Latitude = _random.NextSingle() * 2 + _adslatStart,
            Altitude = 8000 + _random.NextSingle() * 400,
            HorizontalSpeed = 750 + _random.NextSingle() * 100,
            VerticalSpeed = _random.NextSingle() * 10,
            Azimuth = _flightAngle[p.Key]
        });
        var ads = new SDataAdsB
        {
            Data = _flightCache.Values.ToList()
        };

        #endregion

        #region AIS

        _aislngStart = lng - 1;
        _aislngStop = lng + 1;
        _aislatStart = lat - 1;
        _aislatStop = lat + 1;
        // 规定飞机初始飞行角度
        _aisAngle = _aisDic.ToDictionary(p => p.Key, _ => _random.NextSingle() * 359.9f);
        _aisCache = _aisDic.ToDictionary(p => p.Key, p => new AisInfo
        {
            Mmsi = p.Key,
            Name = p.Value,
            Callsign = $"{p.Key}Callsign",
            Imo = $"{p.Key}IMO",
            Longitude = _random.NextSingle() * 2 + _adslngStart,
            Latitude = _random.NextSingle() * 2 + _adslatStart,
            Speed = _random.NextSingle() * 5,
            ShipHeader = _aisAngle[p.Key],
            TrackHeader = _aisAngle[p.Key],
            Category = "测试船舶",
            Country = "中华人民共和国",
            Length = 12,
            Width = 4,
            Destination = "测试地址",
            State = "航行中",
            Draught = 3,
            ArrivalTime = Utils.GetTimestamp(Utils.GetNowTime().AddHours(1)),
            UpdateTime = Utils.GetNowTimestamp()
        });
        var ais = new SDataAis
        {
            Data = _aisCache.Values.ToList()
        };

        #endregion

        SendMessageData(new List<object> { ads, ais });
        _cts = new CancellationTokenSource();
        _processTask = new Task(_ => DataGenerate(), _cts.Token);
        _processTask.Start();
    }

    private void BsDataGenerate()
    {
        var random = new Random();
        while (_bsDataCts?.IsCancellationRequested == false)
            try
            {
                var datas = new List<object>();
                var index = random.Next(0, _bsInfos.Count);
                var bsInfo = _bsInfos[index];
                if (bsInfo == null) return;
                var gps = RunningInfo.BufGpsData;
                var bsGps = bsInfo.StationLat == null || bsInfo.StationLng == null
                    ? null
                    : new GpsDatum
                    {
                        Longitude = bsInfo.StationLng.Value,
                        Latitude = bsInfo.StationLat.Value
                    };
                var data = new SDataCellular
                {
                    Bandwidth = bsInfo.Bandwidth,
                    FieldStrength = bsInfo.FieldStrength,
                    Frequency = bsInfo.Frequency,
                    Channel = bsInfo.Channel,
                    Ci = bsInfo.Ci,
                    Lac = bsInfo.Lac,
                    Mcc = bsInfo.Mcc,
                    Mnc = bsInfo.Mnc,
                    DuplexMode = Utils.ConvertStringToEnum<DuplexMode>(bsInfo.DuplexMode),
                    RxPower = bsInfo.RxPower,
                    Location = new GpsDatum
                    {
                        Longitude = gps.Longitude,
                        Latitude = gps.Latitude,
                        Altitude = gps.Altitude,
                        Heading = gps.Heading,
                        Speed = gps.Speed
                    },
                    Timestamp = Utils.GetNowTimestamp(),
                    BsGps = bsGps
                };
                try
                {
                    data.ExInfos = Utils.ConvertFromJson<Dictionary<string, ExtendedInfo>>(bsInfo.ExInfos);
                }
                catch
                {
                }

                datas.Add(data);
                SendData(datas);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
            }
    }

    private void DataGenerate()
    {
        while (_cts?.IsCancellationRequested == false)
            try
            {
                Thread.Sleep(100);
                if (AdsSwitch && DateTime.Now.Subtract(_preAdsTime).TotalMilliseconds > _adsInterval)
                {
                    _preAdsTime = DateTime.Now;
                    BuildAdsData();
                    var ads = new SDataAdsB
                    {
                        Data = _flightCache.Values.ToList()
                    };
                    SendMessageData(new List<object> { ads });
                }

                if (AisSwitch && DateTime.Now.Subtract(_preAisTime).TotalMilliseconds > _aisInterval)
                {
                    _preAisTime = DateTime.Now;
                    BuildAisData();
                    var ais = new SDataAis
                    {
                        Data = _aisCache.Values.ToList()
                    };
                    SendMessageData(new List<object> { ais });
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
            }
    }

    private void BuildAdsData()
    {
        const double span = 2d / 100d;
        foreach (var key in _flightDic.Keys)
        {
            if (!_flightCache.ContainsKey(key)) continue;
            var info = _flightCache[key];
            info.HorizontalSpeed = 750 + _random.NextSingle() * 100;
            info.VerticalSpeed = _random.NextSingle() * 10;
            info.Azimuth = info.Azimuth + _random.NextSingle() * 2 - 1;
            var sign = false;
            var newLng = 0d;
            var newLat = 0d;
            var newAngle = info.Azimuth;
            while (!sign)
            {
                var anglePi = newAngle * Math.PI / 180;
                newLng = Math.Sin(anglePi) * span + info.Longitude;
                newLat = Math.Cos(anglePi) * span + info.Latitude;
                sign = newLng >= _adslngStart
                       && newLng <= _adslngStop
                       && newLat >= _adslatStart
                       && newLat <= _adslatStop;
                if (!sign)
                    // if (info.PlaneAddress == "90001")
                    // {
                    //     Console.WriteLine("到达边界，重新规划行程");
                    // }
                    newAngle = _random.NextSingle() * 359.9f;
            }

            info.Latitude = newLat;
            info.Longitude = newLng;
            info.Azimuth = newAngle;
            info.UpdateTime = Utils.GetNowTimestamp();
            // if (info.PlaneAddress == "90001")
            // {
            //     Console.WriteLine($"{info.PlaneAddress},{info.FlightNumber},{info.Latitude},{info.Longitude},{info.Azimuth}");
            // }
            _flightCache[key] = info;
        }
    }

    private void BuildAisData()
    {
        const double span = 2d / 100d;
        var index = 0;
        foreach (var key in _aisDic.Keys)
        {
            if (!_aisCache.ContainsKey(key)) continue;
            index++;
            var info = _aisCache[key];
            info.Speed = _random.NextSingle() * 5;
            info.ShipHeader = info.ShipHeader + _random.NextSingle() * 2 - 1;
            info.TrackHeader = info.TrackHeader + _random.NextSingle() * 2 - 1;
            var sign = false;
            var newLng = info.Longitude;
            var newLat = info.Latitude;
            var newAngle = info.ShipHeader;
            if (index < 7)
            {
                while (!sign)
                {
                    var anglePi = newAngle * Math.PI / 180;
                    newLng = Math.Sin(anglePi) * span + info.Longitude;
                    newLat = Math.Cos(anglePi) * span + info.Latitude;
                    sign = newLng >= _aislngStart
                           && newLng <= _aislngStop
                           && newLat >= _aislatStart
                           && newLat <= _aislatStop;
                    if (!sign)
                        // if (info.PlaneAddress == "90001")
                        // {
                        //     Console.WriteLine("到达边界，重新规划行程");
                        // }
                        newAngle = _random.NextSingle() * 359.9f;
                }

                info.State = "航行中";
                info.Underway = true;
            }
            else
            {
                info.State = "锚泊";
                info.Underway = false;
            }

            info.Latitude = newLat;
            info.Longitude = newLng;
            info.ShipHeader = newAngle;
            info.TrackHeader = newAngle + _random.NextSingle() * 2 - 1;
            info.UpdateTime = Utils.GetNowTimestamp();
            // if (info.PlaneAddress == "90001")
            // {
            //     Console.WriteLine($"{info.PlaneAddress},{info.FlightNumber},{info.Latitude},{info.Longitude},{info.Azimuth}");
            // }
            _aisCache[key] = info;
        }
    }

    #region 航空相关定义

    private DateTime _preAdsTime = DateTime.Now;
    private Dictionary<string, FlightInfo> _flightCache = new();

    private readonly Dictionary<string, string> _flightDic = new()
    {
        { "90001", "UEA9001" },
        { "90002", "UEA9002" },
        { "90003", "UEA9003" },
        { "90004", "UEA9004" },
        { "90005", "UEA9005" },
        { "90006", "UEA9006" },
        { "90007", "UEA9007" },
        { "90008", "UEA9008" },
        { "90009", "UEA9009" },
        { "90010", "UEA9010" }
    };

    private Dictionary<string, float> _flightAngle = new();
    private double _adslngStart;
    private double _adslngStop;
    private double _adslatStart;
    private double _adslatStop;

    #endregion

    #region 水上相关定义

    private DateTime _preAisTime = DateTime.Now;
    private Dictionary<string, AisInfo> _aisCache = new();

    private readonly Dictionary<string, string> _aisDic = new()
    {
        { "80001", "测试船舶01" },
        { "80002", "测试船舶02" },
        { "80003", "测试船舶03" },
        { "80004", "测试船舶04" },
        { "80005", "测试船舶05" },
        { "80006", "测试船舶06" },
        { "80007", "测试船舶07" },
        { "80008", "测试船舶08" },
        { "80009", "测试船舶09" },
        { "80010", "测试船舶10" }
    };

    private Dictionary<string, float> _aisAngle = new();
    private double _aislngStart;
    private double _aislngStop;
    private double _aislatStart;
    private double _aislatStop;

    #endregion
}