using System;
using System.Text;
using Contract.AIS;
using Magneto.Contract.AIS.Interface;

namespace Magneto.Device.Parser;

/// <summary>
///     船舶静态与航行数据相关
///     AISData 消息ID=5
/// </summary>
public class AisStaticAVoyage : IAisStaticAVoyage, IAisDecodable, ICloneable
{
    /// <summary>
    ///     消息源ID
    /// </summary>
    private readonly int _msgSrc;

    /// <summary>
    ///     呼号
    /// </summary>
    private string _callSign;

    /// <summary>
    ///     船籍对应ID
    /// </summary>
    private CountryId _countryId;

    /// <summary>
    ///     目的地
    /// </summary>
    private string _destination;

    /// <summary>
    ///     船舶长度尺寸1
    /// </summary>
    private int _dimensionA;

    /// <summary>
    ///     船舶长度尺寸2
    /// </summary>
    private int _dimensionB;

    /// <summary>
    ///     船舶宽度尺寸1
    /// </summary>
    private int _dimensionC;

    /// <summary>
    ///     船舶宽度尺寸2
    /// </summary>
    private int _dimensionD;

    /// <summary>
    ///     最大静态吃水深度
    /// </summary>
    private double _draught;

    /// <summary>
    ///     预计到达时间
    /// </summary>
    private DateTime _eta;

    /// <summary>
    ///     国际海事组织编号
    /// </summary>
    private int _imo;

    /// <summary>
    ///     船舶识别码
    /// </summary>
    private int _mmsi;

    /// <summary>
    ///     消息ID
    /// </summary>
    private AisMessageType _msgId;

    /// <summary>
    ///     船名
    /// </summary>
    private string _shipName;

    /// <summary>
    ///     船舶或者货物类型
    /// </summary>
    private ShipType _shipType;

    /// <summary>
    ///     静态航行数据
    /// </summary>
    public AisStaticAVoyage()
    {
    }

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="msgId">消息ID</param>
    /// <param name="imo">IMO</param>
    /// <param name="mmsi">船舶MMSI</param>
    /// <param name="callSign">船舶呼号</param>
    /// <param name="name">船舶名称</param>
    /// <param name="shipType">船舶类型</param>
    /// <param name="dimensionA">GPS天线距离船艏距离</param>
    /// <param name="dimensionB">GPS天线距离船尾距离</param>
    /// <param name="dimensionC">GPS天线距离左舷距离</param>
    /// <param name="dimensionD">GPS天线距离右舷距离</param>
    /// <param name="eta">预计到达时间</param>
    /// <param name="draught">静态吃水深度</param>
    /// <param name="destination">目的地</param>
    /// <param name="countryId">船籍ID</param>
    /// <param name="msgSrc">消息来源</param>
    public AisStaticAVoyage(AisMessageType msgId, int imo, int mmsi, string callSign, string name, ShipType shipType,
        int dimensionA, int dimensionB, int dimensionC, int dimensionD, DateTime eta,
        double draught, string destination, CountryId countryId, int msgSrc)
    {
        _msgId = msgId;
        _imo = imo;
        _mmsi = mmsi;
        _callSign = callSign;
        _shipName = name;
        _shipType = shipType;
        _dimensionA = dimensionA;
        _dimensionB = dimensionB;
        _dimensionC = dimensionC;
        _dimensionD = dimensionD;
        _eta = eta;
        _draught = draught;
        _destination = destination;
        _countryId = countryId;
        _msgSrc = msgSrc;
    }

    /// <summary>
    ///     解析编码数据
    ///     <para>消息类型:5</para>
    ///     <para>返回船舶静态与航行相关数据</para>
    ///     <para>消息总长424位,占用两个AVIDM句子</para>
    /// </summary>
    /// <param name="decBytes">待解析的二进制字符串</param>
    public IAisMessage Decode(string decBytes) //throws AISParseException
    {
        if (decBytes.Length < 421)
            //DCMS.Server.BasicDefine.Notepad.WriteError("数据长度不符合该类型消息的长度!\r\n消息类型:5");
            return null;
        //位置报告消息类型ID 1,2,3 bits 0-5
        var id = AisDecoder.GetDecValueByBinStr(decBytes[..6], false);
        _msgId = AisCommonMethods.GetAisMessageType(id);
        //用户ID mmsi bits 8-37
        _mmsi = AisDecoder.GetDecValueByBinStr(decBytes.Substring(8, 30), false);
        // AIS version indicator bits 38-39 - 2 bits
        // int navState = decBytes[6] & 0x3;
        // Debug.WriteLine("navState = "+navState);
        //IMO号码 bits 40-69 - 30 bits
        _imo = AisDecoder.GetDecValueByBinStr(decBytes.Substring(40, 30), false);
        //呼号 bits 70-111 - 42 bits
        _callSign = AisDecoder.GetDecStringFrom6BitStr(decBytes.Substring(70, 42));
        //船舶名称 bits 112-231 - 120 bits
        _shipName = AisDecoder.GetDecStringFrom6BitStr(decBytes.Substring(112, 120));
        //船舶或者货物类型 bits 232-239 - 8 bits
        var st = AisDecoder.GetDecValueByBinStr(decBytes.Substring(232, 8), false);
        _shipType = AisCommonMethods.GetShipType(st);
        _countryId = AisCommonMethods.GetCountryId(_mmsi);
        //GPS天线位置距离船艏、船尾、左舷、右舷的距离 bits 240-269 - 30 bits
        _dimensionA = AisDecoder.GetDecValueByBinStr(decBytes.Substring(240, 9), false);
        _dimensionB = AisDecoder.GetDecValueByBinStr(decBytes.Substring(249, 9), false);
        _dimensionC = AisDecoder.GetDecValueByBinStr(decBytes.Substring(258, 6), false);
        _dimensionD = AisDecoder.GetDecValueByBinStr(decBytes.Substring(264, 6), false);
        //电子定位装置类型 bits 270-273 - 4 bits
        AisDecoder.GetDecValueByBinStr(decBytes.Substring(270, 4), false);
        //预计到达时间 ETA bits 274-293 - 20 bits
        var etaMonth = AisDecoder.GetDecValueByBinStr(decBytes.Substring(274, 4), false);
        var etaDay = AisDecoder.GetDecValueByBinStr(decBytes.Substring(278, 5), false);
        var etaHour = AisDecoder.GetDecValueByBinStr(decBytes.Substring(283, 5), false);
        var etaMinute = AisDecoder.GetDecValueByBinStr(decBytes.Substring(288, 6), false);
        var year = DateTime.Now.Year;
        var etaTime = DateTime.MinValue;
        try
        {
            if (etaMonth is >= 1 and <= 12
                && etaDay is >= 1 and <= 31
                && etaHour is >= 0 and <= 23
                && etaMinute is >= 0 and <= 59)
                etaTime = new DateTime(year, etaMonth, etaDay, etaHour, etaMinute, 0, DateTimeKind.Utc);
            else
                etaTime = DateTime.MinValue;
        }
        catch
        {
        }

        _eta = etaTime;
        //最大静态吃水深度 bits 294-301 - 8 bits
        var draughtInt = AisDecoder.GetDecValueByBinStr(decBytes.Substring(294, 8), false);
        _draught = draughtInt / 10.0;
        //目的地 bits 302-421 - 120 bits
        _destination = AisDecoder.GetDecStringFrom6BitStr(decBytes.Substring(302, 120));
        return this;
    }

    /// <summary>
    ///     消息ID
    /// </summary>
    public AisMessageType MsgId => _msgId;

    /// <summary>
    ///     识别码
    /// </summary>
    public int Mmsi => _mmsi;

    /// <summary>
    ///     船舶名称
    /// </summary>
    public string ShipName => _shipName;

    /// <summary>
    ///     呼号
    /// </summary>
    public string CallSign => _callSign;

    /// <summary>
    ///     IMO（国际海事组织）
    /// </summary>
    public int Imo => _imo;

    /// <summary>
    ///     船舶或者货物类型
    /// </summary>
    public ShipType ShipType => _shipType;

    /// <summary>
    ///     GPS天线距离船艏的距离
    /// </summary>
    public int DimensionA => _dimensionA;

    /// <summary>
    ///     GPS天线距离船尾的距离
    /// </summary>
    public int DimensionB => _dimensionB;

    /// <summary>
    ///     GPS天线距离左舷的距离
    /// </summary>
    public int DimensionC => _dimensionC;

    /// <summary>
    ///     GPS天线距离右舷的距离
    /// </summary>
    public int DimensionD => _dimensionD;

    /// <summary>
    ///     船籍ID
    /// </summary>
    public CountryId CountryId => _countryId;

    /// <summary>
    ///     消息源
    /// </summary>
    public int MsgSrc => _msgSrc;

    /// <summary>
    ///     最大静态吃水深度
    /// </summary>
    public double Draught => _draught;

    /// <summary>
    ///     预计到达时间
    /// </summary>
    public DateTime Eta => _eta;

    /// <summary>
    ///     目的地
    /// </summary>
    public string Destination => _destination;

    public object Clone()
    {
        return (AisStaticAVoyage)MemberwiseClone();
    }

    public override bool Equals(object o)
    {
        if (o == null) return false;
        if (o == this) return true;
        if (o is not AisStaticAVoyage that) return false;
        return _destination.Equals(that._destination)
               && _callSign.Equals(that._callSign)
               && _countryId == that._countryId
               && _dimensionA == that._dimensionA
               && _dimensionB == that._dimensionB
               && _dimensionC == that._dimensionC
               && _dimensionD == that._dimensionD
               && _msgId == that._msgId
               && _imo == that._imo
               && _mmsi == that._mmsi
               && _msgSrc == that._msgSrc
               && _shipName.Equals(that._shipName)
               && _shipType == that._shipType;
    }

    public override string ToString()
    {
        var sbuilder = new StringBuilder();
        sbuilder.Append("AISStaticAVoyage\n")
            .Append("[")
            .Append("MSGID:")
            .Append(_msgId)
            .Append('\n')
            .Append("MMSI:")
            .Append(_mmsi)
            .Append('\n')
            .Append("IMO:")
            .Append(_imo)
            .Append('\n')
            .Append("CallSign:")
            .Append(_callSign)
            .Append('\n')
            .Append("BoatName:")
            .Append(_shipName)
            .Append('\n')
            .Append("ShipType:")
            .Append(_shipType)
            .Append('\n')
            .Append("DimensionA:")
            .Append(_dimensionA)
            .Append('\n')
            .Append("DimensionB:")
            .Append(_dimensionB)
            .Append('\n')
            .Append("DimensionC:")
            .Append(_dimensionC)
            .Append('\n')
            .Append("DimensionD:")
            .Append(_dimensionD)
            .Append('\n')
            .Append("ETA:")
            .Append(_eta)
            .Append('\n')
            .Append("Draught:")
            .Append(_draught)
            .Append('\n')
            .Append("Destination:")
            .Append(_destination)
            .Append('\n')
            .Append("Country Name:")
            .Append(_countryId.ToString().Replace("_", " "))
            .Append('\n')
            .Append("MsgSrc:")
            .Append(_msgSrc)
            .Append("]");
        return sbuilder.ToString();
    }

    public override int GetHashCode()
    {
        const int prime = 31;
        var result = 1;
        long temp = _draught.GetHashCode(); // Double.doubleToLongBits(draught);
        result = prime + (int)(temp ^ (temp >> 32));
        result = prime * result + _msgId.GetHashCode();
        result = prime * result + _imo;
        result = prime * result + _mmsi;
        result = prime * result + _shipType.GetHashCode();
        result = prime * result + (_eta == default ? 0 : _eta.GetHashCode());
        result = prime * result + (_callSign?.GetHashCode() ?? 0);
        result = prime * result + (_shipName?.GetHashCode() ?? 0);
        result = prime * result + (_destination?.GetHashCode() ?? 0);
        result = prime * result + _countryId.GetHashCode();
        result = prime * result + _msgSrc;
        result = prime * result + _dimensionA;
        result = prime * result + _dimensionB;
        result = prime * result + _dimensionC;
        result = prime * result + _dimensionD;
        return result;
    }
}