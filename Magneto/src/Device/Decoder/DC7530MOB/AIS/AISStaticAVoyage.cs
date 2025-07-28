using System;
using System.Text;
using Contract.AIS;
using Magneto.Contract.AIS.Interface;

namespace Magneto.Device.DC7530MOB.AIS;

/// <summary>
///     船舶静态与航行数据相关
/// </summary>
public class AisStaticAVoyage : IAisStaticAVoyage, IAisDecodable, ICloneable
{
    /// <summary>
    ///     呼号
    /// </summary>
    private string _callSign;

    //船舶长度尺寸1
    //船舶长度尺寸2
    //船舶宽度尺寸1
    //船舶宽度尺寸2
    //预计到达时间
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
        DimensionA = dimensionA;
        DimensionB = dimensionB;
        DimensionC = dimensionC;
        DimensionD = dimensionD;
        _eta = eta;
        Draught = draught;
        Destination = destination;
        CountryId = countryId;
        MsgSrc = msgSrc;
    }

    /// <summary>
    ///     解析编码数据
    ///     <para>消息类型：5</para>
    ///     <para>返回船舶静态与航行相关数据</para>
    ///     <para>消息总长424位，占用两个AVIDM句子</para>
    /// </summary>
    /// <param name="decBytes">待解析的二进制字符串</param>
    /// <returns></returns>
    public IAisMessage Decode(string decBytes) //throws AISParseException
    {
        if (decBytes.Length < 421)
            //DCMS.Server.BasicDefine.Notepad.WriteError("数据长度不符合该类型消息的长度！\r\n消息类型：5");
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
        CountryId = AisCommonMethods.GetCountryId(_mmsi);
        //GPS天线位置距离船艏、船尾、左舷、右舷的距离 bits 240-269 - 30 bits
        DimensionA = AisDecoder.GetDecValueByBinStr(decBytes.Substring(240, 9), false);
        DimensionB = AisDecoder.GetDecValueByBinStr(decBytes.Substring(249, 9), false);
        DimensionC = AisDecoder.GetDecValueByBinStr(decBytes.Substring(258, 6), false);
        DimensionD = AisDecoder.GetDecValueByBinStr(decBytes.Substring(264, 6), false);
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
            if (etaMonth is < 1 or > 12 || etaDay is < 1 or > 31 || etaHour is < 0 or > 23 ||
                etaMinute is < 0 or > 59)
                etaTime = DateTime.MinValue;
            else
                etaTime = new DateTime(year, etaMonth, etaDay, etaHour, etaMinute, 0);
        }
        catch
        {
        }

        _eta = etaTime;
        //最大静态吃水深度 bits 294-301 - 8 bits
        var draughtInt = AisDecoder.GetDecValueByBinStr(decBytes.Substring(294, 8), false);
        Draught = draughtInt / 10.0;
        //目的地 bits 302-421 - 120 bits
        Destination = AisDecoder.GetDecStringFrom6BitStr(decBytes.Substring(302, 120));
        return this;
    }

    //最大静态吃水深度
    //目的地
    //船籍对应ID
    //消息源ID
    /// <summary>
    ///     消息ID
    /// </summary>
    /// <returns></returns>
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
    /// <returns></returns>
    public int Imo => _imo;

    /// <summary>
    ///     船舶或者货物类型
    /// </summary>
    public ShipType ShipType => _shipType;

    /// <summary>
    ///     GPS天线距离船艏的距离
    /// </summary>
    /// <returns></returns>
    public int DimensionA { get; private set; }

    /// <summary>
    ///     GPS天线距离船尾的距离
    /// </summary>
    /// <returns></returns>
    public int DimensionB { get; private set; }

    /// <summary>
    ///     GPS天线距离左舷的距离
    /// </summary>
    /// <returns></returns>
    public int DimensionC { get; private set; }

    /// <summary>
    ///     GPS天线距离右舷的距离
    /// </summary>
    /// <returns></returns>
    public int DimensionD { get; private set; }

    /// <summary>
    ///     船籍ID
    /// </summary>
    public CountryId CountryId { get; private set; }

    /// <summary>
    ///     消息源
    /// </summary>
    /// <returns></returns>
    public int MsgSrc { get; }

    /// <summary>
    ///     最大静态吃水深度
    /// </summary>
    /// <returns></returns>
    public double Draught { get; private set; }

    /// <summary>
    ///     预计到达时间
    /// </summary>
    /// <returns></returns>
    public DateTime Eta => _eta;

    /// <summary>
    ///     目的地
    /// </summary>
    /// <returns></returns>
    public string Destination { get; private set; }

    public object Clone()
    {
        return (AisStaticAVoyage)MemberwiseClone();
    }

    public override bool Equals(object o)
    {
        if (o == null)
            return false;
        if (o == this)
            return true;
        if (o is not AisStaticAVoyage that)
            return false;
        var same = Destination.Equals(that.Destination)
                   && _callSign.Equals(that._callSign)
                   && CountryId == that.CountryId
                   && DimensionA == that.DimensionA
                   && DimensionB == that.DimensionB
                   && DimensionC == that.DimensionC
                   && DimensionD == that.DimensionD
                   && _msgId == that._msgId
                   && _imo == that._imo
                   && _mmsi == that._mmsi
                   && MsgSrc == that.MsgSrc
                   && _shipName.Equals(that._shipName)
                   && _shipType == that._shipType;
        return same;
    }

    public override string ToString()
    {
        var sbuilder = new StringBuilder();
        sbuilder.Append("AISStaticAVoyage\n").Append('[').Append("MSGID:" + _msgId + "\n")
            .Append("MMSI:" + _mmsi + "\n")
            .Append("IMO:" + _imo + "\n").Append("CallSign:" + _callSign + "\n").Append("BoatName:" + _shipName + "\n")
            .Append("ShipType:" + _shipType + "\n").Append("DimensionA:" + DimensionA + "\n")
            .Append("DimensionB:" + DimensionB + "\n").Append("DimensionC:" + DimensionC + "\n")
            .Append("DimensionD:" + DimensionD + "\n").Append("ETA:" + _eta + "\n").Append("Draught:" + Draught + "\n")
            .Append("Destination:" + Destination + "\n")
            .Append("Country Name:" + CountryId.ToString().Replace("_", " ") + "\n").Append("MsgSrc:" + MsgSrc)
            .Append(']');
        return sbuilder.ToString();
    }

    public override int GetHashCode()
    {
        const int prime = 31;
        var result = 1;
        long temp = Draught.GetHashCode(); // Double.doubleToLongBits(draught);
        result = prime + (int)(temp ^ (temp >> 32));
        result = prime * result + _msgId.GetHashCode();
        result = prime * result + _imo;
        result = prime * result + _mmsi;
        result = prime * result + _shipType.GetHashCode();
        result = prime * result + (_eta == default ? 0 : _eta.GetHashCode());
        result = prime * result + (_callSign == null ? 0 : _callSign.GetHashCode());
        result = prime * result + (_shipName == null ? 0 : _shipName.GetHashCode());
        result = prime * result + (Destination == null ? 0 : Destination.GetHashCode());
        result = prime * result + CountryId.GetHashCode();
        result = prime * result + MsgSrc;
        result = prime * result + DimensionA;
        result = prime * result + DimensionB;
        result = prime * result + DimensionC;
        result = prime * result + DimensionD;
        return result;
    }
}