using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Contract.UavDef;
using Magneto.Device.RxDrone.Sdk;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.RxDrone;

public partial class RxDrone : DeviceBase
{
    private const int FixedScanDataLen = 11520;
    private const string LibPath = GlobalMembers.LibPath;

    private static bool _firstInitializedFailed = true;
    private static bool _isInitialized;
    private DroneCbFn _droneCbFn;
    private SpectrumCbFn _spectrumCbFn;
    private string[] _whiteLists;

    public RxDrone(Guid deviceId) : base(deviceId)
    {
        if (_isInitialized) return;
        _isInitialized = true;
        GlobalMembers.Init();
    }

    [LibraryImport(LibPath)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void config_ip([MarshalAs(UnmanagedType.LPWStr)] string ipString);

    [LibraryImport(LibPath)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int GetDeviceNum();

    [LibraryImport(LibPath)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int OpenDevice(int antenna);

    [LibraryImport(LibPath)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void start_drone_scan2(long freq, int gain, int sampleRate, DroneCbFn droneCb,
        SpectrumCbFn cbPsd);

    [LibraryImport(LibPath)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void stop_device();

    [LibraryImport(LibPath)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void get_state(out float x);

    #region Private Functions

    private void SpectrumCallback(float[] spectrum, long startFreq, int length, float resolution)
    {
        //const int maxGetSpectrumLen = 65536;
        //while (_isRunning)
        //{
        //    // 速度
        //    Thread.Sleep(500);
        //    var freq = 0L;
        //    var spectrumData = new float[maxGetSpectrumLen];
        //    var len = get_spectrum(spectrumData, maxGetSpectrumLen, ref freq);
        //    if (freq is 0L) continue;
        //    if (len <= 0) continue;
        //    spectrumData = spectrumData.Take(len).ToArray();
        //    var scan = new SDataScan
        //    {
        //        StartFrequency = freq / 1e6,
        //        StopFrequency = StartFrequency + 0.001d * spectrumData.Length,
        //        StepFrequency = 1d,
        //        Data = spectrumData.Select(s => (short)((s + 107) * 10)).ToArray()
        //    };
        //    SetSegmentParam(ref scan);
        //    var data = new List<object> { scan };
        //    SendData(data);
        //}
    }

    private static void SetSegmentParam(ref SDataScan scan)
    {
        var freq = scan.StartFrequency.ToString(CultureInfo.InvariantCulture);
        switch (freq[..3])
        {
            case "240":
                scan.SegmentOffset = 0;
                scan.Offset = 0;
                break;
            case "242":
                scan.SegmentOffset = 0;
                scan.Offset = FixedScanDataLen * 1;
                break;
            case "243":
                scan.SegmentOffset = 0;
                scan.Offset = FixedScanDataLen * 2;
                break;
            case "245":
                scan.SegmentOffset = 0;
                scan.Offset = FixedScanDataLen * 3;
                break;
            default:
                {
                    switch (freq[..2])
                    {
                        case "57":
                            scan.SegmentOffset = 1;
                            scan.Offset = 0;
                            break;
                        case "58":
                            scan.SegmentOffset = 1;
                            scan.Offset = FixedScanDataLen * 1;
                            break;
                    }

                    break;
                }
        }
    }

    private void DroneCallback(DjiFlightInfoStr message)
    {
        if (message.PacketType.Equals(21))
        {
            Console.WriteLine("Encrypt DJI drone.");
            return;
        }
        if (message.DroneLatitude.Equals(0d) || message.DroneLongitude.Equals(0d)) return;
        if (message.DroneLatitude < -85 || message.DroneLatitude > 85 || message.DroneLongitude < -180 ||
            message.DroneLongitude > 180) return;
        Console.WriteLine(DateTime.Now.ToString(CultureInfo.CurrentCulture) + "!" + message.DroneSerialNum + "|" +
                          message.Height + "|" +
                          message.Altitude + "|" +
                          message.PilotLongitude + "|" +
                          message.PilotLatitude + "|" +
                          message.DroneLongitude + "|" +
                          message.DroneLatitude);

        _ = Task.Run(() =>
        {
            var data = new List<object>
            {
                new AlarmMessage
                {
                    Description = "Test",
                    Message = "",
                    Severity = "",
                    Source = "",
                    Status = AlarmStatus.Confirmed,
                    Timestamp = DateTime.Now,
                    Details =
                    [
                        new SDataDjiFlightInfoStr
                        {
                            Altitude = message.Altitude,
                            DroneLatitude = message.DroneLatitude,
                            DroneLongitude = message.DroneLongitude,
                            DroneSerialNum = message.DroneSerialNum,
                            EastSpeed = message.EastSpeed,
                            GpsTime = message.GpsTime,
                            Height = message.Height,
                            HomeLatitude = message.HomeLatitude,
                            HomeLongitude = message.HomeLongitude,
                            License = message.License,
                            NorthSpeed = message.NorthSpeed,
                            PacketType = message.PacketType,
                            PilotLatitude = message.PilotLatitude,
                            PilotLongitude = message.PilotLongitude,
                            PitchAngle = message.PitchAngle,
                            ProductType = message.ProductType,
                            ProductTypeStr = message.ProductTypeStr.TrimEnd('\0'),
                            RollAngle = message.RollAngle,
                            SeqNum = message.SeqNum,
                            StateInfo = message.StateInfo,
                            UpSpeed = message.UpSpeed,
                            Uuid = message.Uuid.ToString(),
                            UuidLength = message.UuidLength,
                            YawAngle = message.YawAngle,
                            IsWhite = _whiteLists.Contains(message.DroneSerialNum)
                        }
                    ]
                }
            };
            SendData(data);
        });
    }

    #endregion

    #region Override Functions

    public override bool Initialized(ModuleInfo device)
    {
        var result = base.Initialized(device);
        if (IsDemo) return result;
        config_ip(IpAddress);
        var deviceNum = GetDeviceNum();
        _ = OpenDevice(1);
        if (deviceNum != -1)
        {
            Console.WriteLine($"{deviceNum}设备已连接。{result}");
            _droneCbFn = DroneCallback;
            _spectrumCbFn = SpectrumCallback;
            start_drone_scan2(0, Gain, 13440000, _droneCbFn, _spectrumCbFn);
            result = true;
        }
        else
        {
            if (_firstInitializedFailed) Console.WriteLine($"无人机设备设备连接失败。{result}");
            result = false;
        }

        _firstInitializedFailed = false;
        _whiteLists = UavDefDataBase.Select<WhiteList>(typeof(WhiteList), string.Empty, new DynamicParameters())
            .Select(s => s.DroneSerialNum).ToArray();
        return result;
    }

    public override void Dispose()
    {
        if (IsDemo) return;
        base.Dispose();
        stop_device();
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Demo

    private DateTime _dateTimeDemo = DateTime.Now;
    private Task _getSpectrumTask;
    private bool _isRunning;

    #region Private Functions

    private readonly double[,] _uavPath =
    {
        { 104.00734405417381, 30.473104108707904 },
        { 104.3173440517381, 30.272104108707904 },
        { 104.52734405417381, 30.07410410707904 }
    };

    private int _uavIndex;

    private void GetSpectrumToScanData()
    {
        while (_isRunning)
        {
            // 速度
            if (DateTime.Now - _dateTimeDemo < TimeSpan.FromMinutes(5))
            {
                if (_uavIndex > 2) _uavIndex = 0;
                if ((DateTime.Now.Second % 5).Equals(0))
                    DroneCallbackDemo(new DjiFlightInfoStr
                    {
                        // 所有变量初始化值
                        Altitude = 0,
                        DroneSerialNum = "122342134",
                        DroneLatitude = _uavPath[_uavIndex, 1],
                        DroneLongitude = _uavPath[_uavIndex, 0],
                        EastSpeed = 10,
                        GpsTime = 1000,
                        Height = 500,
                        HomeLatitude = 30.71679727281d,
                        HomeLongitude = 104.094510129794d,
                        License = Encoding.Default.GetBytes("12"),
                        NorthSpeed = 140,
                        PacketType = 0,
                        PilotLatitude = 30.7167981d,
                        PilotLongitude = 104.094129794d,
                        PitchAngle = 210,
                        ProductType = 1,
                        ProductTypeStr = "Dji3",
                        RollAngle = 0,
                        SeqNum = 12,
                        StateInfo = 0,
                        UpSpeed = 12,
                        Uuid = Guid.Empty.ToByteArray(),
                        UuidLength = 0,
                        YawAngle = 60
                    });
                if ((DateTime.Now.Second % 2).Equals(0))
                    DroneCallbackDemo(new DjiFlightInfoStr
                    {
                        // 所有变量初始化值
                        Altitude = 0,
                        DroneSerialNum = "22222222",
                        DroneLatitude = _uavPath[_uavIndex, 1] - 0.1d,
                        DroneLongitude = _uavPath[_uavIndex, 0] + 0.1d,
                        EastSpeed = 10,
                        GpsTime = 1000,
                        Height = 500,
                        HomeLatitude = 30.71679d,
                        HomeLongitude = 104.09451d,
                        License = Encoding.Default.GetBytes("12"),
                        NorthSpeed = 140,
                        PacketType = 0,
                        PilotLatitude = 30.716989d,
                        PilotLongitude = 104.03429d,
                        PitchAngle = 210,
                        ProductType = 1,
                        ProductTypeStr = "Dji4",
                        RollAngle = 0,
                        SeqNum = 13,
                        StateInfo = 0,
                        UpSpeed = 13,
                        Uuid = Guid.Empty.ToByteArray(),
                        UuidLength = 0,
                        YawAngle = 60
                    });
                _uavIndex++;
            }
            else if (DateTime.Now - _dateTimeDemo > TimeSpan.FromMinutes(30))
            {
                _dateTimeDemo = DateTime.Now;
            }

            Thread.Sleep(1000);

            //double[] startFrequencies = { 2408.74, 2423.74, 2438.74, 2453.74, 5750.74, 5810.74 };
            //for (var i = 0; i < 6; i++)
            //{
            //    StartFrequency = startFrequencies[i];
            //    StopFrequency = StartFrequency + 288d;
            //    StepFrequency = 25d;
            //    var scanData = GetScan();
            //    SetSegmentParam(ref scanData);
            //    SendData(new List<object> { scanData });
            //}
        }
    }

    private void DroneCallbackDemo(DjiFlightInfoStr message)
    {
        _ = Task.Run(() =>
        {
            var data = new List<object>
            {
                new AlarmMessage
                {
                    Description = "Test",
                    Message = "",
                    Severity = "",
                    Source = "",
                    Status = AlarmStatus.Confirmed,
                    Timestamp = DateTime.Now,
                    Details = new List<object>
                    {
                        new SDataDjiFlightInfoStr
                        {
                            Altitude = message.Altitude,
                            DroneLatitude = message.DroneLatitude,
                            DroneLongitude = message.DroneLongitude,
                            DroneSerialNum = message.DroneSerialNum,
                            EastSpeed = message.EastSpeed,
                            GpsTime = message.GpsTime,
                            Height = message.Height,
                            HomeLatitude = message.HomeLatitude,
                            HomeLongitude = message.HomeLongitude,
                            License = message.License,
                            NorthSpeed = message.NorthSpeed,
                            PacketType = message.PacketType,
                            PilotLatitude = message.PilotLatitude,
                            PilotLongitude = message.PilotLongitude,
                            PitchAngle = message.PitchAngle,
                            ProductType = message.ProductType,
                            ProductTypeStr = message.ProductTypeStr.TrimEnd('\0'),
                            RollAngle = message.RollAngle,
                            SeqNum = message.SeqNum,
                            StateInfo = message.StateInfo,
                            UpSpeed = message.UpSpeed,
                            Uuid = message.Uuid.ToString(),
                            UuidLength = message.UuidLength,
                            YawAngle = message.YawAngle,
                            IsWhite = message.DroneSerialNum.Equals("22222222")
        }
                    }
}
            };
            SendData(data);
        });
    }

    #endregion

    #region Override Functions

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        if (!IsDemo) return;
        _dateTimeDemo = DateTime.Now;
        base.Start(feature, dataPort);
        _isRunning = true;
        _getSpectrumTask = Task.Run(GetSpectrumToScanData);
    }

    public override void Stop()
    {
        if (!IsDemo) return;
        try
        {
            base.Stop();
            _isRunning = false;
            _getSpectrumTask?.Wait(10000);
        }
        catch
        {
            // ignored
        }
    }

    #endregion

    #endregion
}