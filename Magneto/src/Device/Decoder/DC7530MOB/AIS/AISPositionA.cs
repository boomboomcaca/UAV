using System;
using System.Text;
using Contract.AIS;
using Magneto.Contract.AIS.Interface;

namespace Magneto.Device.DC7530MOB.AIS;

/// <summary>
///     A类船舶位置报告
///     <para>接收到的船舶位置数据</para>
/// </summary>
public class AisPositionA : IAisMessage, IAisDecodable, ICloneable
{
    //消息类型
    //消息转发标识符
    //船舶识别码
    //航行状态
    //转向率 +-127
    //对地航速0~102.2节（102.2节代表102.2节或者更高）
    //经度 +-180° 东经-正，西经-负
    //纬度 +-90° 北纬-正，南纬-负
    //对地航向（0-359.9°）
    //船艏真航向（0-359，511表示不可用）
    //消息时戳（UTC）
    private DateTime _msgTimestamp;

    /// <summary>
    ///     A类船舶数据
    /// </summary>
    public AisPositionA()
    {
    }

    /// <summary>
    ///     A类船舶位置
    /// </summary>
    /// <param name="msgId">消息ID</param>
    /// <param name="repeatIndicator">消息重复标志</param>
    /// <param name="mmsi">船舶MMSI</param>
    /// <param name="navState">航向状态</param>
    /// <param name="rot">转向率</param>
    /// <param name="sog">对地航速</param>
    /// <param name="longitude">船舶位置经度</param>
    /// <param name="latitude">船舶位置纬度</param>
    /// <param name="cog">对地航向</param>
    /// <param name="trueHeading">船艏真航向</param>
    /// <param name="msgTimestamp">消息时间</param>
    public AisPositionA(AisMessageType msgId, int repeatIndicator, int mmsi, NavigationState navState, double rot,
        double sog, double longitude, double latitude, double cog, int trueHeading, DateTime msgTimestamp)
    {
        MsgId = msgId;
        RepeatIndicator = repeatIndicator;
        Mmsi = mmsi;
        NavigationState = navState;
        Rot = rot;
        Sog = sog;
        Longitude = longitude;
        Latitude = latitude;
        Cog = cog;
        TrueHeading = trueHeading;
        _msgTimestamp = msgTimestamp;
    }

    /// <summary>
    ///     对地航向
    /// </summary>
    public double Cog { get; private set; }

    /// <summary>
    ///     消息重复标志
    /// </summary>
    public int RepeatIndicator { get; private set; }

    /// <summary>
    ///     船舶纬度
    /// </summary>
    public double Latitude { get; private set; }

    /// <summary>
    ///     船舶经度
    /// </summary>
    public double Longitude { get; private set; }

    /// <summary>
    ///     消息时间
    /// </summary>
    public DateTime MsgTimestamp => _msgTimestamp;

    /// <summary>
    ///     航行状态
    /// </summary>
    public NavigationState NavigationState { get; private set; }

    /// <summary>
    ///     转向率
    /// </summary>
    public double Rot { get; private set; }

    /// <summary>
    ///     对地航速
    /// </summary>
    public double Sog { get; private set; }

    /// <summary>
    ///     真航向
    /// </summary>
    public int TrueHeading { get; private set; }

    /// <summary>
    ///     解析编码数据
    ///     <para>消息类型：1，2，3</para>
    ///     <para>返回A类船舶位置数据</para>
    /// </summary>
    /// <param name="decBytes">编码的六位二进制字符串</param>
    /// <returns>解析后的A类船舶位置数据</returns>
    public IAisMessage Decode(string decBytes)
    {
        if (decBytes.Length < 144)
            //DCMS.Server.BasicDefine.Notepad.WriteError("数据长度不符合当前的消息类型！\r\n消息类型：1,2,3");
            return null;
        /* Possition Reports Message ID 1,2,3 bits 0-5 */
        var id = AisDecoder.GetDecValueByBinStr(decBytes[..6], false);
        MsgId = AisCommonMethods.GetAisMessageType(id);
        /* repeat indicator, bits 6-7 */
        RepeatIndicator = AisDecoder.GetDecValueByBinStr(decBytes.Substring(6, 2), false);
        /* user id mmsi bits 8-37 */
        Mmsi = AisDecoder.GetDecValueByBinStr(decBytes.Substring(8, 30), false);
        /* navigational status bits 38-41 - 4 bits */
        var state = AisDecoder.GetDecValueByBinStr(decBytes.Substring(38, 4), false);
        NavigationState = AisCommonMethods.GetNavigationState(state);
        //转向率 bits 42-49 - 8 bits
        Rot = AisDecoder.GetDecValueByBinStr(decBytes.Substring(42, 8), true);
        //对地航速 bits 50-59 - 10 bits
        Sog = AisDecoder.GetDecValueByBinStr(decBytes.Substring(50, 10), false) / 10.0;
        //定位精度 bit 60
        AisDecoder.GetDecValueByBinStr(decBytes.Substring(78, 1), false);
        //经度 bits 61-88
        var longitudeHour = AisDecoder.GetDecValueByBinStr(decBytes.Substring(61, 28), true);
        Longitude = longitudeHour / 600000.0;
        if (Longitude is > 180.0 or < -180.0) Longitude = 181;
        //纬度 bits 89-115
        var latitudeHour = AisDecoder.GetDecValueByBinStr(decBytes.Substring(89, 27), true);
        Latitude = latitudeHour / 600000.0;
        if (Latitude is > 90.0 or < -90.0) Latitude = 91;
        //对地航向 bits 117-127
        Cog = AisDecoder.GetDecValueByBinStr(decBytes.Substring(116, 12), false) / 10.0;
        //船艏真航向 bits 128-136 
        TrueHeading = AisDecoder.GetDecValueByBinStr(decBytes.Substring(128, 9), false);
        //UTC时间的秒
        AisDecoder.GetDecValueByBinStr(decBytes.Substring(137, 6), false);
        //int syncState = AISDecoder.GetDecValueByBinStr(decBytes.Substring(149, 2), false);
        //int slotTimeOut = AISDecoder.GetDecValueByBinStr(decBytes.Substring(151, 3), false);
        //int subMessage = AISDecoder.GetDecValueByBinStr(decBytes.Substring(154, 14), false);
        //string commState = AISDecoder.GetDecStringFrom6BitStr(decBytes.Substring(149, 19));
        //int year = cal.Year;
        //int month = cal.Month;
        //int day = cal.Day;
        //int hour = cal.Hour;
        //int minute = cal.Minute;
        //this.msgTimestamp = new DateTime(year, month, day, hour, minute, timeStamp);
        //Calendar cal = Calendar.getInstance(TimeZone.getTimeZone("UTC"));
        _msgTimestamp = DateTime.Now; // new DateTime(cal.getTimeInMillis());
        // TODO: implement the rest bits
        return this;
    }

    /// <summary>
    ///     消息ID
    /// </summary>
    public AisMessageType MsgId { get; private set; }

    /// <summary>
    ///     船舶MMSI
    /// </summary>
    public int Mmsi { get; private set; }

    public object Clone()
    {
        return (AisPositionA)MemberwiseClone();
    }

    public override string ToString()
    {
        var sbuilder = new StringBuilder();
        sbuilder.Append("AISPositionA\n").Append("[").Append("MsgID:" + MsgId + "\n")
            .Append("Repeat Indicator:" + RepeatIndicator + "\n").Append("MMSI:" + Mmsi + "\n")
            .Append("Navigation State:" + NavigationState + "\n").Append("ROT:" + Rot + "\n")
            .Append("Longitude:" + Longitude + "\n").Append("Latitude:" + Latitude + "\n").Append("SOG:" + Sog + "\n")
            .Append("COG:" + Cog + "\n").Append("Heading:" + TrueHeading + "\n").Append("MsgTimeStamp:" + _msgTimestamp)
            .Append("]");
        return sbuilder.ToString();
    }

    public override int GetHashCode()
    {
        const int prime = 31;
        var result = 1;
        long temp = Cog.GetHashCode();
        result = prime + (int)(temp ^ (temp >> 32));
        temp = Latitude.GetHashCode(); //Double.doubleToLongBits(latitude);
        result = prime * result + (int)(temp ^ (temp >> 32));
        temp = Longitude.GetHashCode(); // Double.doubleToLongBits(longitude);
        result = prime * result + (int)(temp ^ (temp >> 32));
        result = prime * result + Mmsi;
        result = prime * result + MsgId.GetHashCode();
        result = prime * result + (_msgTimestamp == default ? 0 : _msgTimestamp.GetHashCode());
        result = prime * result + NavigationState.GetHashCode();
        result = prime * result + RepeatIndicator;
        temp = Rot.GetHashCode(); // Double.doubleToLongBits(rot);
        result = prime * result + (int)(temp ^ (temp >> 32));
        temp = Sog.GetHashCode(); // Double.doubleToLongBits(sog);
        result = prime * result + (int)(temp ^ (temp >> 32));
        result = prime * result + TrueHeading;
        return result;
    }

    public override bool Equals(object obj)
    {
        if (this == obj)
            return true;
        if (obj is not AisPositionA other)
            return false;
        if (Math.Abs(Cog - other.Cog) > 1e-9)
            return false;
        if (Math.Abs(Latitude - other.Latitude) > 1e-9)
            return false;
        if (Math.Abs(Longitude - other.Longitude) > 1e-9)
            return false;
        if (Mmsi != other.Mmsi)
            return false;
        if (MsgId != other.MsgId)
            return false;
        if (_msgTimestamp == default)
        {
            if (other._msgTimestamp != default)
                return false;
        }
        else if (!_msgTimestamp.Equals(other._msgTimestamp))
        {
            return false;
        }

        if (NavigationState != other.NavigationState)
            return false;
        if (RepeatIndicator != other.RepeatIndicator)
            return false;
        if (Math.Abs(Rot - other.Rot) > 1e-9)
            return false;
        if (Math.Abs(Sog - other.Sog) > 1e-9)
            return false;
        if (TrueHeading != other.TrueHeading)
            return false;
        return true;
    }
}