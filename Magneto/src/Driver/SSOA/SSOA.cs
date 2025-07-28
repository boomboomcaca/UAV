using System;
using System.Collections.Generic;
using System.IO;
using Magneto.Contract;
using Magneto.Contract.Algorithm;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver.SSOA;

public partial class Ssoa : DriverBase
{
    /// <summary>
    ///     地球半径，地球半径根据坐标系不同，数值也不同
    /// </summary>
    private const long EarthRadius = 6378137;

    private readonly FieldStrengthLocateHandle _fsLocateHandle = new();
    private double _bandwidth;
    private short _factor;
    private double _frequency;
    private SFieldStrengthTrackData _preGpsData;

    public Ssoa(Guid functionId) : base(functionId)
    {
    }

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        if (!base.Start(dataPort, mediaType)) return false;
        _fsLocateHandle.Clear();
        _fsLocateHandle.Start();
        _fsLocateHandle.UpdateSsoaDataEvent += UpdateSsoaDataArrived;
        (Receiver as DeviceBase)?.Start(FeatureType.FFM, this);
        // System.Threading.Tasks.Task.Run(DataProcess);
        return true;
    }

    public override bool Stop()
    {
        (Receiver as DeviceBase)?.Stop();
        _fsLocateHandle.Stop();
        _fsLocateHandle.Clear();
        return base.Stop();
    }

    public override void SetParameter(string name, object value)
    {
        base.SetParameter(name, value);
        if (name == ParameterNames.Frequency && AntennaController is IAntennaController antennaController
                                             && double.TryParse(value.ToString(), out var freq))
            antennaController.Frequency = freq;
        if (name == "resetSSOA")
            if (ResetSsoa)
                _fsLocateHandle?.Clear();
    }

    public override void OnData(List<object> data)
    {
        SendData(data);
        var level = (SDataLevel)data.Find(item => item is SDataLevel);
        if (level != null)
        {
            if (!Utils.IsNumberEquals(_frequency, level.Frequency)
                && AntennaController is IAntennaController antennaController)
            {
                _fsLocateHandle.Clear();
                _frequency = level.Frequency;
                _bandwidth = level.Bandwidth;
                _factor = antennaController.GetFactor(_frequency);
                var factor = new SDataFactor
                {
                    Data = new short[1]
                };
                factor.Data[0] = _factor;
                data.Insert(0, factor);
            }

            var gps = RunningInfo.BufGpsData;
            var tempData = new SFieldStrengthTrackData
            {
                Latitude = gps.Latitude,
                Longitude = gps.Longitude,
                FieldStrength = (float)Math.Round(level.Data + _factor / 10f, 1)
            };
            var distance = GetDistance(_preGpsData.Latitude, _preGpsData.Longitude, tempData.Latitude,
                tempData.Longitude);
            if (distance < 1)
                // 小于1米不计算
                return;
            _preGpsData = tempData;
            _fsLocateHandle.AddData(tempData);
            if (SaveLevelData)
            {
                var str = $"{DateTime.UtcNow:HH:mm:ss.fff},{gps.Longitude},{gps.Latitude},{level.Data}";
                var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ssoa.dat");
                using var fs = new FileStream(file, FileMode.Append, FileAccess.Write);
                using var sw = new StreamWriter(fs);
                sw.WriteLine(str);
                sw.Flush();
                sw.Close();
                sw.Dispose();
                fs.Close();
                fs.Dispose();
            }
        }
    }

    private void UpdateSsoaDataArrived(object sender, SsoaBitmapData e)
    {
        if (e == null) return;
        var data = new SDataSsoa
        {
            Frequency = _frequency,
            Bandwidth = _bandwidth,
            LeftTopLongitude = e.LeftTopLongitude,
            LeftTopLatitude = e.LeftTopLatitude,
            RightBottomLongitude = e.RightBottomLongitude,
            RightBottomLatitude = e.RightBottomLatitude,
            MaxLongitude = e.MaxLongitude,
            MaxLatitude = e.MaxLatitude,
            Data = e.Data
        };
        SendData(new List<object> { data });
    }

    /// <summary>
    ///     计算经纬度距离 单位：米
    /// </summary>
    /// <param name="latA"></param>
    /// <param name="lngA"></param>
    /// <param name="latB"></param>
    /// <param name="lngB"></param>
    private double GetDistance(double latA, double lngA, double latB, double lngB)
    {
        var dLat1InRad = latA * (Math.PI / 180);
        var dLong1InRad = lngA * (Math.PI / 180);
        var dLat2InRad = latB * (Math.PI / 180);
        var dLong2InRad = lngB * (Math.PI / 180);
        var dLongitude = dLong2InRad - dLong1InRad;
        var dLatitude = dLat2InRad - dLat1InRad;
        var a = Math.Pow(Math.Sin(dLatitude / 2), 2) +
                Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) * Math.Pow(Math.Sin(dLongitude / 2), 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadius * c;
    }
}