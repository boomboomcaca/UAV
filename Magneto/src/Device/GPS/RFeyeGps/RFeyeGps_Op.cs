using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Magneto.Protocol.Data;

namespace Magneto.Device.RFeyeGps;

public partial class RFeyeGps
{
    private void DataProc()
    {
        //保存最新收到的经纬度信息
        double lastLatitude = 0;
        double lastLongitude = 0;
        //记录最新发送GPS的时间
        var lastSendTime = DateTime.MinValue;
        while (_dataCts?.IsCancellationRequested == false)
            try
            {
                //发送Gps数据请求
                var requestPacket = new Packet();
                requestPacket.BeginPacket(PacketType.Node, -1);
                requestPacket.AddField(PacketKey.FieldGps, -1);
                requestPacket.EndPacket();
                SendPacket(requestPacket, _socket);
                //接收Gps数据(返回的数据可能还有心跳包,此处意为在有数据的情况下直到接收到GPS数据再发送下一次请求)
                Field gpsField = null;
                while (_socket.Available > 0)
                {
                    var responsePacket = ReceivePacket(_socket);
                    if (responsePacket != null)
                    {
                        gpsField = responsePacket.ListFieldInfo.FirstOrDefault(x => x.FieldName == PacketKey.FieldGps);
                        if (gpsField != null) break;
                    }
                }

                if (gpsField == null) continue;
                //解析Gps数据并发送(距离大于2米或者时间大于10秒)
                var dataGps = ParseGpsData(gpsField);
                if (dataGps != null)
                {
                    var distance = GetDistance(lastLatitude, lastLongitude, dataGps.Latitude, dataGps.Longitude);
                    var currTime = DateTime.Now;
                    var timeSpan = currTime - lastSendTime;
                    if (distance > 2 || timeSpan.TotalMilliseconds > 10000)
                    {
                        SendMessageData(new List<object> { dataGps });
                        //记录最新的发送时间
                        lastSendTime = currTime;
                    }

                    //保存最新的经纬度信息
                    lastLatitude = dataGps.Latitude;
                    lastLongitude = dataGps.Longitude;
                }

                Thread.Sleep(1000);
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
                Trace.WriteLine($"{DeviceInfo.DisplayName}数据处理异常，异常信息：{ex}");
            }
    }

    private static SDataGps ParseGpsData(Field gpsField)
    {
        var altitude = GetParameterIntValue(gpsField, PacketKey.GpsAltitude);
        var heading = GetParameterIntValue(gpsField, PacketKey.GpsHeading);
        var latitude = GetParameterIntValue(gpsField, PacketKey.GpsLatitude);
        var longtitude = GetParameterIntValue(gpsField, PacketKey.GpsLongitude);
        var speed = GetParameterIntValue(gpsField, PacketKey.GpsSpeed);
        var satellites = GetParameterIntValue(gpsField, PacketKey.GpsSatellites);
        var status = GetParameterIntValue(gpsField, PacketKey.GpsStatus);
        if (status == 0) return null;
        var dataGps = new SDataGps
        {
            Altitude = (float)altitude / 1000,
            Heading = (ushort)(heading / 100),
            Latitude = (double)latitude / 1000000,
            Longitude = (double)longtitude / 1000000,
            Speed = (ushort)(speed / 1000),
            Satellites = (byte)satellites
        };
        return dataGps;
    }

    /// <summary>
    ///     获取两个点的距离，单位米
    /// </summary>
    /// <param name="lantitude1"></param>
    /// <param name="longitude1"></param>
    /// <param name="lantitude2"></param>
    /// <param name="longitude2"></param>
    /// <returns></returns>
    private static double GetDistance(double lantitude1, double longitude1, double lantitude2, double longitude2)
    {
        const double axis = 6378.137; //地球半径，km
        var dLat1InRad = lantitude1 * (Math.PI / 180);
        var dLong1InRad = longitude1 * (Math.PI / 180);
        var dLat2InRad = lantitude2 * (Math.PI / 180);
        var dLong2InRad = longitude2 * (Math.PI / 180);
        var dLongitude = dLong2InRad - dLong1InRad;
        var dLatitude = dLat2InRad - dLat1InRad;
        var a = Math.Pow(Math.Sin(dLatitude / 2), 2) +
                Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) * Math.Pow(Math.Sin(dLongitude / 2), 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var dDistance = axis * c * 1000;
        return dDistance;
    }

    /// <summary>
    ///     获取Field中参数名为paramKey的参数值
    /// </summary>
    /// <param name="field"></param>
    /// <param name="paramKey">参数名</param>
    /// <returns>参数值</returns>
    private static int GetParameterIntValue(Field field, int paramKey)
    {
        var value = ~0; //表示无效
        var param = field.ListParameter.FirstOrDefault(x => x.ParameterName == paramKey);
        try
        {
            if (param != null && param.Data.Length == 4) value = BitConverter.ToInt32(param.Data, 0);
        }
        catch (ArgumentException)
        {
        }

        return value;
    }
}