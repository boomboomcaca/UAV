using System;
using Contract.AIS;
using Magneto.Contract.AIS.Interface;

namespace Magneto.Device.DC7530MOB.AIS;

public class AisStaticDataReport : IAisMessage, IAisDecodable, ICloneable
{
    /// <summary>
    ///     静态数据报告
    /// </summary>
    public AisStaticDataReport()
    {
    }

    /// <summary>
    ///     船籍
    /// </summary>
    public CountryId CountryId { get; private set; }

    /// <summary>
    ///     消息属于哪一部分，0-A，1-B
    /// </summary>
    public int PartNo { get; private set; }

    /// <summary>
    ///     船舶名称
    /// </summary>
    public string ShipName { get; private set; }

    /// <summary>
    ///     船舶类型
    /// </summary>
    public ShipType ShipType { get; private set; }

    /// <summary>
    ///     供应商ID
    /// </summary>
    public string VendorId { get; private set; }

    /// <summary>
    ///     序列号
    /// </summary>
    public int SerialNo { get; private set; }

    /// <summary>
    ///     呼号
    /// </summary>
    public string CallSign { get; private set; }

    /// <summary>
    ///     GPS天线位置距离船艏距离，单位：米
    /// </summary>
    public int DimensionA { get; private set; }

    /// <summary>
    ///     GPS天线位置距离船尾距离，单位：米
    /// </summary>
    public int DimensionB { get; private set; }

    /// <summary>
    ///     GPS天线位置距离左舷距离，单位：米
    /// </summary>
    public int DimensionC { get; private set; }

    /// <summary>
    ///     GPS天线位置距离右舷距离，单位：米
    /// </summary>
    public int DimensionD { get; private set; }

    /// <summary>
    ///     母船MMSI
    /// </summary>
    public int MotherShipMmsi { get; private set; }

    /// <summary>
    ///     解析静态数据汇报A,B部分
    /// </summary>
    /// <param name="decBytes"></param>
    /// <returns></returns>
    public IAisMessage Decode(string decBytes)
    {
        if (decBytes.Length < 160)
            //DCMS.Server.BasicDefine.Notepad.WriteError("数据长度不符合该类型消息的长度！\r\n消息类型：24");
            return null;
        //静态数据报告消息类型 18 bits 0-5-6bits
        var id = AisDecoder.GetDecValueByBinStr(decBytes[..6], false);
        MsgId = AisCommonMethods.GetAisMessageType(id);
        //船舶识别码 bits 8-37-30bits
        Mmsi = AisDecoder.GetDecValueByBinStr(decBytes.Substring(8, 30), false);
        CountryId = AisCommonMethods.GetCountryId(Mmsi);
        //判断消息属于A部分还是B部分 bits 38-39-2bits
        PartNo = AisDecoder.GetDecValueByBinStr(decBytes.Substring(38, 2), false);
        //消息A包含船名信息，消息B不包含
        if (PartNo == 0)
        {
            ShipName = AisDecoder.GetDecStringFrom6BitStr(decBytes.Substring(40, 120));
            //A类数据只包含之前的信息，如果是A类数据直接在这返回，否则是B类数据继续后续解析
            ShipType = ShipType.UnKnown;
            return this;
        }

        ShipName = string.Empty;
        //船舶或者货物类型 bits 40-47 - 8 bits
        var st = AisDecoder.GetDecValueByBinStr(decBytes.Substring(40, 8), false);
        ShipType = AisCommonMethods.GetShipType(st);
        //船舶供应商ID bits 48-65-18bits
        VendorId = AisDecoder.GetDecStringFrom6BitStr(decBytes.Substring(48, 18));
        //船舶序列号 bits 70-89-20bits
        SerialNo = AisDecoder.GetDecValueByBinStr(decBytes.Substring(70, 20), false);
        //呼号  bits 90-131-42bits
        CallSign = AisDecoder.GetDecStringFrom6BitStr(decBytes.Substring(90, 42));
        //GPS天线位置A bits 132-140-9bits
        DimensionA = AisDecoder.GetDecValueByBinStr(decBytes.Substring(132, 9), false);
        //GPS天线位置B bits 141-149-9bits
        DimensionB = AisDecoder.GetDecValueByBinStr(decBytes.Substring(141, 9), false);
        //GPS天线位置C bits 150-155-6bits
        DimensionC = AisDecoder.GetDecValueByBinStr(decBytes.Substring(150, 6), false);
        //GPS天线位置D bits 156-161-6bits
        DimensionD = AisDecoder.GetDecValueByBinStr(decBytes.Substring(156, 6), false);
        //母船MMSI bits 132-161-30bits
        MotherShipMmsi = AisDecoder.GetDecValueByBinStr(decBytes.Substring(132, 30), false);
        return this;
    }

    /// <summary>
    ///     静态数据报告消息类型
    /// </summary>
    public AisMessageType MsgId { get; private set; }

    /// <summary>
    ///     船舶识别号码
    /// </summary>
    public int Mmsi { get; private set; }

    public object Clone()
    {
        return (AisStaticDataReport)MemberwiseClone();
    }

    public override bool Equals(object o)
    {
        if (o == null)
            return false;
        if (o == this)
            return true;
        if (o is not AisStaticDataReport that)
            return false;
        var same = MsgId == that.MsgId
                   && Mmsi == that.Mmsi
                   && CountryId == that.CountryId
                   && PartNo == that.PartNo
                   && ShipName == that.ShipName
                   && ShipType == that.ShipType
                   && VendorId == that.VendorId
                   && SerialNo == that.SerialNo
                   && CallSign == that.CallSign
                   && DimensionA == that.DimensionA
                   && DimensionB == that.DimensionB
                   && DimensionC == that.DimensionC
                   && DimensionD == that.DimensionD
                   && MotherShipMmsi == that.MotherShipMmsi;
        return same;
    }

    public override int GetHashCode()
    {
        var prime = GetType().GetHashCode();
        prime ^= MsgId.GetHashCode();
        prime ^= Mmsi.GetHashCode();
        prime ^= CountryId.GetHashCode();
        prime ^= PartNo.GetHashCode();
        prime ^= ShipName.GetHashCode();
        prime ^= ShipType.GetHashCode();
        prime ^= VendorId.GetHashCode();
        prime ^= SerialNo;
        prime ^= CallSign.GetHashCode();
        prime ^= DimensionA;
        prime ^= DimensionB;
        prime ^= DimensionC;
        prime ^= DimensionD;
        prime ^= MotherShipMmsi;
        return prime;
    }
}