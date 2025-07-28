using System;
using System.Text;
using Contract.AIS;
using Magneto.Contract.AIS.Interface;

namespace Magneto.Device.Parser;

/// <summary>
///     B类船舶位置数据
///     AISData 消息ID=18
/// </summary>
public class AisPositionB : IAisPositionB, IAisDecodable, ICloneable
{
    /// <summary>
    ///     标准B类船舶数据
    /// </summary>
    public AisPositionB()
    {
    }

    /// <summary>
    ///     B类船舶位置数据
    /// </summary>
    /// <param name="msgId">消息类型</param>
    /// <param name="mmsi">船舶识别码</param>
    /// <param name="sog">对地航速</param>
    /// <param name="longitude">经度</param>
    /// <param name="latitude">纬度</param>
    /// <param name="cog">对地航向</param>
    /// <param name="trueHeading">船艏真航向</param>
    /// <param name="msgTimestamp">消息时戳</param>
    public AisPositionB(AisMessageType msgId, int mmsi, double sog, double longitude, double latitude, double cog,
        int trueHeading, DateTime msgTimestamp)
    {
        MsgId = msgId;
        Mmsi = mmsi;
        Sog = sog;
        Longitude = longitude;
        Latitude = latitude;
        Cog = cog;
        TrueHeading = trueHeading;
        MsgTimestamp = msgTimestamp;
    }

    /// <summary>
    ///     解析编码数据
    ///     <para>消息类型:18</para>
    ///     <para>返回B类船舶位置数据（通常由小型船只发送）</para>
    /// </summary>
    /// <param name="decBytes">待解析的二进制字符串</param>
    public virtual IAisMessage Decode(string decBytes)
    {
        if (decBytes.Length < 134)
            //DCMS.Server.BasicDefine.Notepad.WriteError("数据长度不符合该类型消息的长度!\r\n消息类型:18");
            return null;
        //位置报告消息类型 18 bits 0-5-6bits
        var id = AisDecoder.GetDecValueByBinStr(decBytes[..6], false);
        MsgId = AisCommonMethods.GetAisMessageType(id);
        //船舶识别码 bits 8-37-30bits
        Mmsi = AisDecoder.GetDecValueByBinStr(decBytes.Substring(8, 30), false);
        //对地航速 bits 46-55 - 10 bits
        Sog = AisDecoder.GetDecValueByBinStr(decBytes.Substring(46, 10), false) / 10.0;
        //位置经度 bits 56-56-1bit
        _ = AisDecoder.GetDecValueByBinStr(decBytes.Substring(56, 1), false);
        //经度 bits 57-84-28bits 
        var longitudeHour = AisDecoder.GetDecValueByBinStr(decBytes.Substring(57, 28), true);
        Longitude = longitudeHour / 600000.0;
        if (Longitude is > 180.0 or < -180.0) Longitude = 181;
        //throw new AISParseException(AISParseException.LONGITUDE_OUT_OF_RANGE + " " + longitude);
        //纬度 bits 85-111-27bits
        var latitudeHour = AisDecoder.GetDecValueByBinStr(decBytes.Substring(85, 27), true);
        Latitude = latitudeHour / 600000.0;
        if (Latitude is > 90.0 or < -90.0) Latitude = 91;
        //对地航向 bits 112-123-12bits 
        Cog = AisDecoder.GetDecValueByBinStr(decBytes.Substring(112, 12), false) / 10.0;
        //实际航向 bits 124-132-9bits
        TrueHeading = AisDecoder.GetDecValueByBinStr(decBytes.Substring(124, 9), false);
        //消息时间戳:秒 bits 138-143-6bits
        //int timeStamp = AISDecoder.GetDecValueByBinStr(decBytes.Substring(133, 6), false);
        // TODO: impelemt the rest bits
        // Calendar cal = Calendar.getInstance(TimeZone.getTimeZone("UTC"));
        MsgTimestamp = DateTime.Now; //new DateTime(cal.getTimeInMillis());
        return this;
    }

    /// <summary>
    ///     对地航向
    /// </summary>
    public double Cog { get; set; }

    /// <summary>
    ///     消息ID
    /// </summary>
    public AisMessageType MsgId { get; set; }

    /// <summary>
    ///     纬度
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    ///     经度
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    ///     消息时间戳
    /// </summary>
    public DateTime MsgTimestamp { get; set; }

    /// <summary>
    ///     对地航速
    /// </summary>
    public double Sog { get; set; }

    /// <summary>
    ///     船艏真航向
    /// </summary>
    public int TrueHeading { get; set; }

    /// <summary>
    ///     船舶识别码
    /// </summary>
    public int Mmsi { get; set; }

    public object Clone()
    {
        return (AisPositionB)MemberwiseClone();
    }

    public override string ToString()
    {
        var sbuider = new StringBuilder();
        sbuider.Append("AISPositionB\n")
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
            .Append("Longitude:")
            .Append(Longitude)
            .Append('\n')
            .Append("Latitude:")
            .Append(Latitude)
            .Append('\n')
            .Append("COG:")
            .Append(Cog)
            .Append('\n')
            .Append("Heading:")
            .Append(TrueHeading)
            .Append('\n')
            .Append("MstTimeStamp:")
            .Append(MsgTimestamp)
            .Append("]");
        return sbuider.ToString();
    }

    public override bool Equals(object o)
    {
        if (o == null) return false;
        if (o == this) return true;
        if (o is not AisPositionB that) return false;
        var same = Math.Abs(Cog - that.Cog) < 1e-9 && MsgId == that.MsgId
                                                   && Math.Abs(Latitude - that.Latitude) < 1e-9
                                                   && Math.Abs(Longitude - that.Longitude) < 1e-9
                                                   && MsgTimestamp.Equals(that.MsgTimestamp)
                                                   && Math.Abs(Cog - that.Cog) < 1e-9 && TrueHeading == that.TrueHeading
                                                   && MsgId == that.MsgId;
        return same;
    }

    public override int GetHashCode()
    {
        const int prime = 31;
        var result = 1;
        long temp = Cog.GetHashCode(); // Double.doubleToLongBits(cog);
        result = prime + (int)(temp ^ (temp >> 32));
        temp = Latitude.GetHashCode(); // Double.doubleToLongBits(latitude);
        result = prime * result + (int)(temp ^ (temp >> 32));
        temp = Longitude.GetHashCode(); // Double.doubleToLongBits(longitude);
        result = prime * result + (int)(temp ^ (temp >> 32));
        result = prime * result + Mmsi;
        result = prime * result + MsgId.GetHashCode();
        result = prime * result
                 + (MsgTimestamp == default ? 0 : MsgTimestamp.GetHashCode());
        temp = Sog.GetHashCode(); // Double.doubleToLongBits(sog);
        result = prime * result + (int)(temp ^ (temp >> 32));
        result = prime * result + TrueHeading;
        return result;
    }
}