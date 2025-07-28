using System.Collections.Generic;
using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

internal class ChannelProgramming
{
    //[0x2039] Type of automatic gain control :
    //1 : manual (bit 1)
    //2 : auto (bit 2)
    //5 : expert (bit 3 indicates if the MMI is in the expert mode of gain control + bit 1 that indicates manual)
    public UCharField AgcType;

    /*[0x2014] Amplification configuration :
    bit 0/1 : RF head (1 normal, 2 high sensibility) for IRS compatibility
    bit 2 : DF ampli
    bit 3 : Listening ampli
    bit 5 : Sub-Range 3 ampli (DF VUHF antenna)
     bit 6 : Sub-Range 1 ampli (DF VUHF antenna)*/
    public UCharField AmpliConfig;

    //[0x2016] Current antenna selected, len: 32
    public MultiBytesField Antenna;

    //[0x8702] Channel information
    public GroupField Channel;

    //[0x2032] Channel number
    public UCharField ChannelNo;

    //[0x2010] Maximum frequency in Hz, not used in direction Operation  WBAT
    public UInt32Field FMax;

    //[0x2015] Positioning of the broadcast filter : 2 : inactive 1 : active
    public UCharField FmFilter;

    //[0x200F] Minimum frequency in Hz, not used in direction Operation  WBAT
    public UInt32Field FMin;

    //[0x2013] Position of the IF attenuator
    public UCharField IfAttenuator;

    //[0x2038] Level units returned : 1 : dBm 2 : dBμV (not used)
    public UCharField LevelUnits;

    //[0x2012] Position of the RF attenuator
    public UCharField RfAttenuator;

    public ChannelProgramming(byte[] value, ref int startIndex)
    {
        Channel = new GroupField(value, ref startIndex);
        ChannelNo = new UCharField(value, ref startIndex);
        FMin = new UInt32Field(value, ref startIndex);
        FMax = new UInt32Field(value, ref startIndex);
        RfAttenuator = new UCharField(value, ref startIndex);
        IfAttenuator = new UCharField(value, ref startIndex);
        AmpliConfig = new UCharField(value, ref startIndex);
        Antenna = new MultiBytesField(value, ref startIndex);
        FmFilter = new UCharField(value, ref startIndex);
        LevelUnits = new UCharField(value, ref startIndex);
        AgcType = new UCharField(value, ref startIndex);
    }

    public ChannelProgramming()
    {
        Channel = new GroupField(0x8702, 0);
        ChannelNo = new UCharField(0x2032);
        FMin = new UInt32Field(0x200F);
        FMax = new UInt32Field(0x2010);
        RfAttenuator = new UCharField(0x2012);
        IfAttenuator = new UCharField(0x2013);
        AmpliConfig = new UCharField(0x2014);
        Antenna = new MultiBytesField(0x2016, 32);
        FmFilter = new UCharField(0x2015);
        LevelUnits = new UCharField(0x2038);
        AgcType = new UCharField(0x2039);
    }

    public byte[] GetBytes()
    {
        Channel.DataSize = GetSize() - GroupField.GetSize();
        List<byte> bytes = new();
        bytes.AddRange(Channel.GetBytes());
        bytes.AddRange(ChannelNo.GetBytes());
        bytes.AddRange(FMin.GetBytes());
        bytes.AddRange(FMax.GetBytes());
        bytes.AddRange(RfAttenuator.GetBytes());
        bytes.AddRange(IfAttenuator.GetBytes());
        bytes.AddRange(AmpliConfig.GetBytes());
        bytes.AddRange(Antenna.GetBytes());
        bytes.AddRange(FmFilter.GetBytes());
        bytes.AddRange(LevelUnits.GetBytes());
        bytes.AddRange(AgcType.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return GroupField.GetSize() + UCharField.GetSize() + UInt32Field.GetSize() + UInt32Field.GetSize() +
               UCharField.GetSize() + UCharField.GetSize() + UCharField.GetSize() + Antenna.GetSize() +
               UCharField.GetSize() + UCharField.GetSize() + UCharField.GetSize();
    }
}