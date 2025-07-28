using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Magneto.Contract;
using Magneto.Protocol.Data;

namespace Magneto.Device.ADS_B_PS;

public partial class AdsBPs
{
    private void ReadData()
    {
        // 一条飞机数据长度大概是800多，一次有可能会发送多条飞机数据
        var buffer = new byte[1024 * 10];
        EndPoint ep = new IPEndPoint(IPAddress.Parse(LocalIp), LocalPort);
        while (_readDataCts?.IsCancellationRequested == false)
            try
            {
                var length = _socket.ReceiveFrom(buffer, SocketFlags.None, ref ep);
                if (length > 0)
                {
                    var recvStr = Encoding.ASCII.GetString(buffer, 0, length);
                    _dataCache.Enqueue(recvStr);
                }
                else
                {
                    Thread.Sleep(5);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
            }
    }

    private void ParseData()
    {
        while (_parseDataCts?.IsCancellationRequested == false)
            try
            {
                var b = _dataCache.TryDequeue(out var recvStr);
                if (!b || string.IsNullOrWhiteSpace(recvStr)) continue;
                if (recvStr.Contains("\"aircraft\""))
                {
                    // 飞机信息
                    var aircraftObj = Utils.DeserializeFromJson<DataAircraft>(recvStr);
                    OnData(aircraftObj.Aircraft);
                }
                //else if (recvStr.Contains("\"status\""))
                //{
                //    // 状态信息
                //    DataStatus statusObj = Utils.DeserializeFromJson<DataStatus>(recvStr);
                //    if (statusObj != null && statusObj.Status != null)
                //    {
                //        OnData(statusObj.Status);
                //    }
                //}
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
            }
    }

    private void OnData(List<Aircraft> data)
    {
        if (data?.Any() != true) return;
        var flights = new List<FlightInfo>();
        foreach (var aircraft in data)
        {
            if (string.IsNullOrWhiteSpace(aircraft.Callsign)) continue;
            var flightInfo = new FlightInfo
            {
                FlightNumber = aircraft.Callsign,
                PlaneAddress = aircraft.IcaoAddress,
                Altitude = aircraft.AltitudeMm / 1e3,
                Azimuth = aircraft.HeadingDe2,
                HorizontalSpeed = aircraft.HorVelocityCms * 3600 / 1e3,
                Latitude = aircraft.LatDd,
                Longitude = aircraft.LonDd,
                Country = "",
                Age = 0,
                Model = "",
                TransponderCode = aircraft.Squawk.ToString(),
                UpdateTime = Utils.GetNowTimestamp(),
                VerticalSpeed = aircraft.VerVelocityCms * 3600 / 1e3
            };
            flights.Add(flightInfo);
        }

        var adsBData = new SDataAdsB
        {
            Data = flights
        };
        SendData(new List<object> { adsBData });
    }
}