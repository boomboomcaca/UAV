using System;
using System.Text;
using Contract.AIS;
using Magneto.Contract.AIS.Interface;

namespace Magneto.Device.Parser;

/// <summary>
///     A类船舶位置报告
///     AISData 消息ID = 1,2,3
///     <para>接收到的船舶位置数据</para>
/// </summary>
public class AisPositionA : IAisPositionA, IAisDecodable, ICloneable
{
    /// <summary>
    ///     对地航向（0-359.9°）
    /// </summary>
    private double _cog;

    /// <summary>
    ///     纬度 +-90° 北纬-正，南纬-负
    /// </summary>
    private double _latitude;

    /// <summary>
    ///     经度 +-180° 东经-正，西经-负
    /// </summary>
    private double _longitude;

    /// <summary>
    ///     船舶识别码
    /// </summary>
    private int _mmsi;

    /// <summary>
    ///     消息类型
    /// </summary>
    private AisMessageType _msgId;

    /// <summary>
    ///     消息时戳（UTC）
    /// </summary>
    private DateTime _msgTimestamp;

    /// <summary>
    ///     航行状态
    /// </summary>
    private NavigationState _navState;

    /// <summary>
    ///     消息转发标识符
    /// </summary>
    private int _repeatIndicator;

    /// <summary>
    ///     转向率 +-127
    /// </summary>
    private double _rot;

    /// <summary>
    ///     对地航速0~102.2节（102.2节代表102.2节或者更高）
    /// </summary>
    private double _sog;

    /// <summary>
    ///     船艏真航向（0-359，511表示不可用）
    /// </summary>
    private int _trueHeading;

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
        _msgId = msgId;
        _repeatIndicator = repeatIndicator;
        _mmsi = mmsi;
        _navState = navState;
        _rot = rot;
        _sog = sog;
        _longitude = longitude;
        _latitude = latitude;
        _cog = cog;
        _trueHeading = trueHeading;
        _msgTimestamp = msgTimestamp;
    }

    /// <summary>
    ///     解析编码数据
    ///     <para>消息类型:1,2,3</para>
    ///     <para>返回A类船舶位置数据</para>
    /// </summary>
    /// <param name="decBytes">编码的六位二进制字符串</param>
    /// <returns>解析后的A类船舶位置数据</returns>
    public IAisMessage Decode(string decBytes)
    {
        if (decBytes.Length < 144)
            //DCMS.Server.BasicDefine.Notepad.WriteError("数据长度不符合当前的消息类型!\r\n消息类型:1,2,3");
            return null;
        /* Possition Reports Message ID 1,2,3 bits 0-5 */
        var id = AisDecoder.GetDecValueByBinStr(decBytes[..6], false);
        _msgId = AisCommonMethods.GetAisMessageType(id);
        /* repeat indicator, bits 6-7 */
        _repeatIndicator = AisDecoder.GetDecValueByBinStr(decBytes.Substring(6, 2), false);
        /* user id mmsi bits 8-37 */
        _mmsi = AisDecoder.GetDecValueByBinStr(decBytes.Substring(8, 30), false);
        /* navigational status bits 38-41 - 4 bits */
        var state = AisDecoder.GetDecValueByBinStr(decBytes.Substring(38, 4), false);
        _navState = AisCommonMethods.GetNavigationState(state);
        //转向率 bits 42-49 - 8 bits
        _rot = AisDecoder.GetDecValueByBinStr(decBytes.Substring(42, 8), true);
        //对地航速 bits 50-59 - 10 bits
        _sog = AisDecoder.GetDecValueByBinStr(decBytes.Substring(50, 10), false) / 10.0;
        //定位精度 bit 60
        AisDecoder.GetDecValueByBinStr(decBytes.Substring(78, 1), false);
        //经度 bits 61-88
        var longitudeHour = AisDecoder.GetDecValueByBinStr(decBytes.Substring(61, 28), true);
        _longitude = longitudeHour / 600000.0;
        if (_longitude is > 180.0 or < -180.0) _longitude = 181;
        //纬度 bits 89-115
        var latitudeHour = AisDecoder.GetDecValueByBinStr(decBytes.Substring(89, 27), true);
        _latitude = latitudeHour / 600000.0;
        if (_latitude is > 90.0 or < -90.0) _latitude = 91;
        //对地航向 bits 117-127
        _cog = AisDecoder.GetDecValueByBinStr(decBytes.Substring(116, 12), false) / 10.0;
        //船艏真航向 bits 128-136 
        _trueHeading = AisDecoder.GetDecValueByBinStr(decBytes.Substring(128, 9), false);
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
    ///     对地航向
    /// </summary>
    public double Cog => _cog;

    /// <summary>
    ///     消息ID
    /// </summary>
    public AisMessageType MsgId => _msgId;

    /// <summary>
    ///     消息重复标志
    /// </summary>
    public int RepeatIndicator => _repeatIndicator;

    /// <summary>
    ///     船舶纬度
    /// </summary>
    public double Latitude => _latitude;

    /// <summary>
    ///     船舶经度
    /// </summary>
    public double Longitude => _longitude;

    /// <summary>
    ///     消息时间
    /// </summary>
    public DateTime MsgTimestamp => _msgTimestamp;

    /// <summary>
    ///     航行状态
    /// </summary>
    public NavigationState NavigationState => _navState;

    /// <summary>
    ///     转向率
    /// </summary>
    public double Rot => _rot;

    /// <summary>
    ///     对地航速
    /// </summary>
    public double Sog => _sog;

    /// <summary>
    ///     真航向
    /// </summary>
    public int TrueHeading => _trueHeading;

    /// <summary>
    ///     船舶MMSI
    /// </summary>
    public int Mmsi => _mmsi;

    public object Clone()
    {
        return (AisPositionA)MemberwiseClone();
    }

    public override string ToString()
    {
        var sbuilder = new StringBuilder();
        sbuilder.Append("AISPositionA\n").Append("[").Append("MsgID:").Append(_msgId).Append('\n')
            .Append("Repeat Indicator:")
            .Append(_repeatIndicator)
            .Append('\n')
            .Append("MMSI:")
            .Append(_mmsi)
            .Append('\n')
            .Append("Navigation State:")
            .Append(_navState)
            .Append('\n')
            .Append("ROT:")
            .Append(_rot)
            .Append('\n')
            .Append("Longitude:")
            .Append(_longitude)
            .Append('\n')
            .Append("Latitude:")
            .Append(_latitude)
            .Append('\n')
            .Append("SOG:")
            .Append(_sog)
            .Append('\n')
            .Append("COG:")
            .Append(_cog)
            .Append('\n')
            .Append("Heading:")
            .Append(_trueHeading)
            .Append('\n')
            .Append("MsgTimeStamp:")
            .Append(_msgTimestamp)
            .Append("]");
        return sbuilder.ToString();
    }

    public override int GetHashCode()
    {
        const int prime = 31;
        var result = 1;
        long temp = _cog.GetHashCode();
        result = prime + (int)(temp ^ (temp >> 32));
        temp = _latitude.GetHashCode(); //Double.doubleToLongBits(latitude);
        result = prime * result + (int)(temp ^ (temp >> 32));
        temp = _longitude.GetHashCode(); // Double.doubleToLongBits(longitude);
        result = prime * result + (int)(temp ^ (temp >> 32));
        result = prime * result + _mmsi;
        result = prime * result + _msgId.GetHashCode();
        result = prime * result + (_msgTimestamp == default ? 0 : _msgTimestamp.GetHashCode());
        result = prime * result + _navState.GetHashCode();
        result = prime * result + _repeatIndicator;
        temp = _rot.GetHashCode(); // Double.doubleToLongBits(rot);
        result = prime * result + (int)(temp ^ (temp >> 32));
        temp = _sog.GetHashCode(); // Double.doubleToLongBits(sog);
        result = prime * result + (int)(temp ^ (temp >> 32));
        result = prime * result + _trueHeading;
        return result;
    }

    public override bool Equals(object obj)
    {
        if (this == obj) return true;
        if (obj is not AisPositionA other) return false;
        if (Math.Abs(_cog - other._cog) > 1e-9) return false;
        if (Math.Abs(_latitude - other._latitude) > 1e-9) return false;
        if (Math.Abs(_longitude - other._longitude) > 1e-9) return false;
        if (_mmsi != other._mmsi) return false;
        if (_msgId != other._msgId) return false;
        if (_msgTimestamp == default)
        {
            if (other._msgTimestamp != default) return false;
        }
        else if (!_msgTimestamp.Equals(other._msgTimestamp))
        {
            return false;
        }

        if (_navState != other._navState) return false;
        if (_repeatIndicator != other._repeatIndicator) return false;
        if (Math.Abs(_rot - other._rot) > 1e-9) return false;
        if (Math.Abs(_sog - other._sog) > 1e-9) return false;
        if (_trueHeading != other._trueHeading) return false;
        return true;
    }
}