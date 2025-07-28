using System;
using System.Text;
using Contract.AIS;
using Magneto.Contract.AIS.Interface;

namespace Magneto.Device.Parser;

/// <summary>
///     AIS 固定位置基站信息周期报告
///     AISData 消息ID=4，11
/// </summary>
public class AisBaseStation : IAisMessage, IAisDecodable, ICloneable
{
    private DateTime _msgTimestamp;

    //经度 +-180° 东经-正 西经-负
    //纬度 +-90° 北纬-正 南纬-负
    // private GPSType gpsType;//暂不处理
    /// <summary>
    ///     固定基站位置报告
    /// </summary>
    public AisBaseStation()
    {
    }

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="msgId">消息ID</param>
    /// <param name="repeatIndicator">重复转发标识符</param>
    /// <param name="mmsi">船舶识别码</param>
    /// <param name="msgTimestamp">消息时间戳</param>
    /// <param name="longitude">经度</param>
    /// <param name="latitude">纬度</param>
    public AisBaseStation(AisMessageType msgId, int repeatIndicator, int mmsi,
            DateTime msgTimestamp, double longitude, double latitude)
        //GPSType deviceType)
    {
        MsgId = msgId;
        RepeatIndicator = repeatIndicator;
        Mmsi = mmsi;
        _msgTimestamp = msgTimestamp;
        Longitude = longitude;
        Latitude = latitude;
        // this.gpsType = deviceType;
    }

    /// <summary>
    ///     重复转发标志
    /// </summary>
    public int RepeatIndicator { get; private set; }

    /// <summary>
    ///     消息时间
    /// </summary>
    public DateTime Timestamp => _msgTimestamp;

    /// <summary>
    ///     船舶纬度
    /// </summary>
    public double Latitude { get; private set; }

    /// <summary>
    ///     船舶经度
    /// </summary>
    public double Longitude { get; private set; }

    //public GPSType GPSType { get { return gpsType; } }
    /// <summary>
    ///     解析编码数据
    ///     <para>消息类型：4</para>
    ///     <para>返回AIS 固定位置基站信息</para>
    /// </summary>
    /// <param name="decBytes">编码的二进制字符串</param>
    /// <returns>AIS 固定位置基站信息</returns>
    public IAisMessage Decode(string decBytes)
    {
        if (decBytes.Length < 135)
            //DCMS.Server.BasicDefine.Notepad.WriteError("数据长度不符合该类型消息的长度！\r\n消息类型：4");
            return null;
        /* Base Station Report message ID, bits 0-5 */
        var id = AisDecoder.GetDecValueByBinStr(decBytes[..6], false);
        MsgId = AisCommonMethods.GetAisMessageType(id);
        /* repeat indicator, bits 6-7 */
        RepeatIndicator = AisDecoder.GetDecValueByBinStr(decBytes.Substring(6, 2), false);
        /* mmsi, bits 8-37 */
        Mmsi = AisDecoder.GetDecValueByBinStr(decBytes.Substring(8, 30), false);
        /* year, bits 38-51 */
        var year = AisDecoder.GetDecValueByBinStr(decBytes.Substring(38, 14), false);
        /* month, bits 52-55 */
        var month = AisDecoder.GetDecValueByBinStr(decBytes.Substring(52, 4), false);
        /* day, bits 56-60 */
        var day = AisDecoder.GetDecValueByBinStr(decBytes.Substring(56, 5), false);
        /* hour, bits 61-65 */
        var hour = AisDecoder.GetDecValueByBinStr(decBytes.Substring(61, 5), false);
        /* minute, bits 66-71 */
        var minute = AisDecoder.GetDecValueByBinStr(decBytes.Substring(66, 6), false);
        /* second, bits 72-77 */
        var second = AisDecoder.GetDecValueByBinStr(decBytes.Substring(72, 6), false);
        var dt = DateTime.MinValue;
        try
        {
            if (month is < 1 or > 12 || day is < 1 or > 31 || hour is < 0 or > 23 ||
                minute is < 0 or > 59 || second is < 0 or > 59)
            {
                dt = DateTime.MinValue;
            }
            else
            {
                if (year < DateTime.Now.Year) year = DateTime.Now.Year;
                dt = new DateTime(year, month, day, hour, minute, second);
            }
        }
        catch
        {
        }

        _msgTimestamp = dt;
        // bit 78 - position accuracy won't be read.
        AisDecoder.GetDecValueByBinStr(decBytes.Substring(78, 1), false);
        /* longitude, bits 79-106 */
        var longitudeHour = AisDecoder.GetDecValueByBinStr(decBytes.Substring(79, 28), true);
        Longitude = longitudeHour / 600000.0;
        if (Longitude is > 180.0 or < -180.0) Longitude = 181;
        /* latitude, bits 107-133 */
        var latitudeHour = AisDecoder.GetDecValueByBinStr(decBytes.Substring(107, 27), true);
        Latitude = latitudeHour / 600000.0;
        if (Latitude is > 90.0 or < -90.0) Latitude = 91;
        //电子定位装置类型, bits 134-137
        //int gps = AISDecoder.GetDecValueByBinStr(decBytes.Substring(134, 4), false);
        //try
        //{
        //    Type gpsT = typeof(GPSType);
        //    this.gpsType = (GPSType)Enum.Parse(gpsT, Enum.GetName(gpsT, gps));
        //}
        //catch { this.gpsType = GPSType.NoUse; }
        //Debug.WriteLine("GPSType = " + gpsType);
        //通信状态
        //int syncState = AISDecoder.GetDecValueByBinStr(decBytes.Substring(149, 2), false);
        //int slotTimeOut = AISDecoder.GetDecValueByBinStr(decBytes.Substring(151, 3), false);
        //int subMessage = AISDecoder.GetDecValueByBinStr(decBytes.Substring(154, 14), false);
        //string commState = AISDecoder.GetDecStringFrom6BitStr(decBytes.Substring(149, 19));
        // TODO: implement the rest bits
        return this;
    }

    /// <summary>
    ///     消息ID
    /// </summary>
    public AisMessageType MsgId { get; private set; }

    /// <summary>
    ///     水上移动通信业务标识码
    /// </summary>
    public int Mmsi { get; private set; }

    public object Clone()
    {
        return (AisBaseStation)MemberwiseClone();
    }

    public override string ToString()
    {
        var sbuilder = new StringBuilder();
        sbuilder.Append("AISBaseStation\n").Append("[")
            .Append("MsgID:")
            .Append(MsgId)
            .Append('\n')
            .Append("Repeat Indicator:")
            .Append(RepeatIndicator)
            .Append('\n')
            .Append("MMSI:")
            .Append(Mmsi)
            .Append('\n')
            .Append("UTC TIMESTAMP:")
            .Append(Timestamp)
            .Append('\n')
            .Append("Longitude:")
            .Append(Longitude)
            .Append('\n')
            .Append("Latitude:")
            .Append(Latitude)
            .Append('\n')
            .Append("]");
        return sbuilder.ToString();
    }

    public override bool Equals(object o)
    {
        if (o == null) return false;
        if (o == this) return true;
        if (o is not AisBaseStation that) return false;
        var same =
            MsgId == that.MsgId
            && RepeatIndicator == that.RepeatIndicator
            && Mmsi == that.Mmsi
            && _msgTimestamp.Equals(that._msgTimestamp)
            && Math.Abs(Latitude - that.Latitude) < 1e-9
            && Math.Abs(Longitude - that.Longitude) < 1e-9;
        // && this.gpsType == that.gpsType;
        return same;
    }

    public override int GetHashCode()
    {
        const int prime = 31;
        var result = 1;
        long temp = Latitude.GetHashCode();
        result = prime + (int)(temp ^ (temp >> 32));
        temp = Longitude.GetHashCode();
        result = prime * result + (int)(temp ^ (temp >> 32));
        result = prime * result + Mmsi;
        result = prime * result + MsgId.GetHashCode();
        result = prime * result + RepeatIndicator;
        result = prime * result
                 + (_msgTimestamp == default ? 0 : _msgTimestamp.GetHashCode());
        return result;
    }
}