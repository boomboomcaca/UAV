using System;
using System.Text;
using Contract.AIS;
using Magneto.Contract.AIS.Interface;

namespace Magneto.Device.Parser;

/// <summary>
///     AISData 扩展的B类船舶 消息ID=19
/// </summary>
public class AisPositionExtB : AisPositionB
{
    /// <summary>
    ///     扩展B类船舶数据
    /// </summary>
    public AisPositionExtB()
    {
    }

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="msgId">消息ID</param>
    /// <param name="mmsi">船舶MMSI</param>
    /// <param name="sog">对地航速</param>
    /// <param name="longitude">位置经度</param>
    /// <param name="latitude">位置纬度</param>
    /// <param name="cog">对地航向</param>
    /// <param name="trueHeading">船艏真航向</param>
    /// <param name="name">船名</param>
    /// <param name="shipType">船舶类型</param>
    /// <param name="dimensionA">GPS天线距离船首距离</param>
    /// <param name="dimensionB">GPS天线距离船尾距离</param>
    /// <param name="dimensionC">GPS天线距离左舷距离</param>
    /// <param name="dimensionD">GPS天线距离右舷距离</param>
    /// <param name="countryId">船籍ID</param>
    /// <param name="msgTimestamp">消息时间戳</param>
    public AisPositionExtB(AisMessageType msgId, int mmsi, double sog, double longitude, double latitude,
        double cog, int trueHeading, string name, ShipType shipType, int dimensionA, int dimensionB,
        int dimensionC, int dimensionD, CountryId countryId, DateTime msgTimestamp)
        : base(msgId, mmsi, sog, longitude, latitude, cog, trueHeading, msgTimestamp)
    {
        ShipName = name;
        ShipType = shipType;
        DimensionA = dimensionA;
        DimensionB = dimensionB;
        DimensionC = dimensionC;
        DimensionD = dimensionD;
        CountryId = countryId;
    }

    /// <summary>
    ///     船籍ID
    /// </summary>
    public CountryId CountryId { get; set; }

    /// <summary>
    ///     GPS天线距离船艏距离
    /// </summary>
    public int DimensionA { get; set; }

    /// <summary>
    ///     GPS天线距离船尾距离
    /// </summary>
    public int DimensionB { get; set; }

    /// <summary>
    ///     GPS天线距离左右距离
    /// </summary>
    public int DimensionC { get; set; }

    /// <summary>
    ///     GPS天线距离右舷距离
    /// </summary>
    public int DimensionD { get; set; }

    /// <summary>
    ///     消息来自方
    /// </summary>
    public int MsgSrc { get; set; }

    /// <summary>
    ///     船舶名称
    /// </summary>
    public string ShipName { get; set; }

    /// <summary>
    ///     船舶类型
    /// </summary>
    public ShipType ShipType { get; set; }

    /// <summary>
    ///     解析编码数据
    ///     <para>消息类型:19</para>
    ///     <para>返回扩展的B类船舶位置信息</para>
    /// </summary>
    /// <param name="decBytes">待解析的二进制字符串</param>
    public override IAisMessage Decode(string decBytes)
    {
        if (decBytes.Length < 301)
            //DCMS.Server.BasicDefine.Notepad.WriteError("数据长度不符合该类型消息的长度!\r\n消息类型:19");
            return null;
        //位置报告消息类型19 bits 0-5
        var id = AisDecoder.GetDecValueByBinStr(decBytes[..6], false);
        MsgId = AisCommonMethods.GetAisMessageType(id);
        //用户ID mmsi bits 8-37
        Mmsi = AisDecoder.GetDecValueByBinStr(decBytes.Substring(8, 30), false);
        CountryId = AisCommonMethods.GetCountryId(Mmsi);
        //对地航速 bits 46-55 - 10 bits
        Sog = AisDecoder.GetDecValueByBinStr(decBytes.Substring(46, 10), false) / 10.0;
        //经度 bits 57-84
        var longitudeHour = AisDecoder.GetDecValueByBinStr(decBytes.Substring(57, 28), true);
        Longitude = longitudeHour / 600000.0;
        if (Longitude is > 180.0 or < -180.0) Longitude = 181;
        //纬度 bits 85-111
        var latitudeHour = AisDecoder.GetDecValueByBinStr(decBytes.Substring(85, 27), true);
        Latitude = latitudeHour / 600000.0;
        if (Latitude is > 90.0 or < -90.0) Latitude = 91;
        //对地航向 bits 112-123
        Cog = AisDecoder.GetDecValueByBinStr(decBytes.Substring(112, 12), false) / 10.0;
        //船艏真航向 bits 124-132
        TrueHeading = AisDecoder.GetDecValueByBinStr(decBytes.Substring(124, 9), false);
        //船舶名称 bits 143-262 - 120 bits
        ShipName = AisDecoder.GetDecStringFrom6BitStr(decBytes.Substring(143, 120));
        // 船舶或货物类型 bits 263-270 - 8 bits
        var st = AisDecoder.GetDecValueByBinStr(decBytes.Substring(263, 8), false);
        ShipType = AisCommonMethods.GetShipType(st);
        //GPS天线距离船艏、船尾、左舷、右舷的距离 bits 271-300 - 30 bits
        DimensionA = AisDecoder.GetDecValueByBinStr(decBytes.Substring(271, 9), false);
        DimensionB = AisDecoder.GetDecValueByBinStr(decBytes.Substring(280, 9), false);
        DimensionC = AisDecoder.GetDecValueByBinStr(decBytes.Substring(289, 6), false);
        DimensionD = AisDecoder.GetDecValueByBinStr(decBytes.Substring(295, 6), false);
        _ = DimensionA + DimensionB;
        _ = DimensionC + DimensionD;
        //GPS定位装置类型
        _ = AisDecoder.GetDecValueByBinStr(decBytes.Substring(301, 4), false);
        // TODO: impelemt the rest bits
        // TODO: implement vesselType, cargoType, countryId, msgSrc
        //Calendar cal = Calendar.getInstance(TimeZone.getTimeZone("UTC"));
        //this.msgTimestamp = new DateTime(cal.getTimeInMillis());
        MsgTimestamp = DateTime.Now;
        return this;
    }

    public override string ToString()
    {
        var sbuilder = new StringBuilder();
        sbuilder.Append("AISPositionExtB\n")
            .Append("[")
            .Append("MsgID:")
            .Append(MsgId)
            .Append('\n')
            .Append("MMSI:")
            .Append(Mmsi)
            .Append('\n')
            .Append("SOG:")
            .Append(Sog)
            .Append('\n')
            .Append("COG:")
            .Append(Cog)
            .Append('\n')
            .Append("Longitude:")
            .Append(Longitude)
            .Append('\n')
            .Append("Latitude:")
            .Append(Latitude)
            .Append('\n')
            .Append("Heading:")
            .Append(TrueHeading)
            .Append('\n')
            .Append("MstTimeStamp:")
            .Append(MsgTimestamp)
            .Append('\n')
            .Append("Ship Name:")
            .Append(ShipName)
            .Append('\n')
            .Append("DimensionA:")
            .Append(DimensionA)
            .Append('\n')
            .Append("DimensionB:")
            .Append(DimensionB)
            .Append('\n')
            .Append("DimensionC:")
            .Append(DimensionC)
            .Append('\n')
            .Append("DimensionD:")
            .Append(DimensionD)
            .Append('\n')
            .Append("Country Name:")
            .Append(CountryId.ToString().Replace("_", " "))
            .Append('\n')
            .Append("MsgSrc:")
            .Append(MsgSrc)
            .Append("]");
        return sbuilder.ToString();
    }

    public override bool Equals(object o)
    {
        if (o == null) return false;
        if (o == this) return true;
        if (o is not AisPositionExtB that) return false;
        var same = base.Equals(that)
                   && ShipType == that.ShipType
                   && DimensionA == that.DimensionA
                   && DimensionB == that.DimensionB
                   && DimensionC == that.DimensionC
                   && DimensionD == that.DimensionD
                   && CountryId == that.CountryId;
        return same;
    }

    public new object Clone()
    {
        return (AisPositionExtB)MemberwiseClone();
    }

    public override int GetHashCode()
    {
        var prime = base.GetHashCode();
        prime ^= Longitude.GetHashCode();
        prime ^= Latitude.GetHashCode();
        prime ^= Mmsi.GetHashCode();
        prime ^= ShipName.GetHashCode();
        return prime;
    }
}