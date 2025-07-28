using System;
using Magneto.Contract.AIS.Interface;
using Magneto.Protocol.Data;

namespace Magneto.Contract.AIS;

/// <summary>
///     船舶信息
/// </summary>
public class AisData
{
    /// <summary>
    ///     唯一标识符
    /// </summary>
    public int Mmsi { get; set; }

    /// <summary>
    ///     船舶静态或航行信息
    /// </summary>
    public IAisStaticData StaticData { get; set; }

    /// <summary>
    ///     船舶位置信息
    /// </summary>
    public IAisPosition PositionMessage { get; set; }

    public SDataAis ToAis()
    {
        var info = new AisInfo();
        if (StaticData == null) return null;
        if (PositionMessage == null) return null;
        info.Name = StaticData.ShipName;
        info.Mmsi = Mmsi.ToString();
        info.Callsign = StaticData.CallSign;
        info.Length = StaticData.DimensionA + StaticData.DimensionB;
        info.Width = StaticData.DimensionC + StaticData.DimensionD;
        info.Country = Utils.GetNameByDescription(StaticData.CountryId);
        if (StaticData is IAisStaticAVoyage av)
        {
            info.Imo = av.Imo.ToString();
            info.Category = Utils.GetNameByDescription(av.ShipType);
            info.Destination = av.Destination;
            info.Draught = av.Draught;
            info.ArrivalTime = Utils.GetTimestamp(av.Eta);
        }

        info.Latitude = PositionMessage.Latitude;
        info.Longitude = PositionMessage.Longitude;
        info.ShipHeader = PositionMessage.TrueHeading > 359 ? double.NaN : PositionMessage.TrueHeading;
        info.TrackHeader = PositionMessage.Cog;
        info.Speed = PositionMessage.Sog;
        if (info is { Latitude: 0, Longitude: 0 }) return null;
        if (PositionMessage is IAisPositionA pa) info.State = Utils.GetNameByDescription(pa.NavigationState);
        if (string.IsNullOrEmpty(info.Imo)) info.Imo = "未知";
        if (string.IsNullOrEmpty(info.Name)) info.Name = "未知";
        if (string.IsNullOrEmpty(info.Category)) info.Category = "未知";
        if (string.IsNullOrEmpty(info.Destination)) info.Destination = "未知";
        if (string.IsNullOrEmpty(info.State)) info.State = "未知";
        if (string.IsNullOrEmpty(info.Callsign)) info.Callsign = "未知";
        if (double.IsNaN(info.ShipHeader)) info.ShipHeader = 0;
        info.UpdateTime = Utils.GetNowTimestamp();
        Console.WriteLine(
            $"{DateTime.Now:HH:mm:ss.fff} AIS结果信息:{info.Mmsi},到达时间:{info.ArrivalTime},名称{info.Name},呼号:{info.Callsign},类型:{info.Category},海事组织编号:{info.Imo},宽:{info.Width},长:{info.Length},吃水深度:{info.Draught},Lat:{info.Latitude:0.000000},Lng:{info.Longitude:0.000000},航速:{info.Speed},航迹向:{info.TrackHeader},船首向:{info.ShipHeader}");
        return new SDataAis
        {
            Data = [info]
        };
    }
}