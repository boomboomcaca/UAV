using System.Collections.Generic;
using Magneto.Device.XE_VUHF_28.Protocols.Field;

namespace Magneto.Device.XE_VUHF_28.Protocols.Data;

//[0X99] – ANTENNA MODIFICATION REQUEST
//This message is used for modifying the antenna parameters.
internal class AntennaModificationRequest
{
    //TODO: 抓包过程中发现数据多出了8个字节,并且按协议的格式会将接收机搞死
    public readonly byte[] Unknown;

    //[0x2016] Name of the antenna to modify If the name is empty, parameters have to be apply to all the antennas
    public MultiBytesField Antenna;

    public MessageHeader Header;

    //[0x8419] Polarization: 0 : Right, 1 : Left, 2 : Alternating
    public CharField Polarization;

    public AntennaModificationRequest()
    {
        Header = new MessageHeader(MessageId.MreDemAntennaModification, 0);
        Antenna = new MultiBytesField(0x2016, 32);
        Polarization = new CharField(0x8419);
        Unknown = new byte[8];
    }

    public byte[] GetBytes()
    {
        Header.ContentSize = GetSize() - MessageHeader.GetSize();
        var bytes = new List<byte>();
        bytes.AddRange(Header.GetBytes());
        bytes.AddRange(Antenna.GetBytes());
        bytes.AddRange(Polarization.GetBytes());
        bytes.AddRange(Unknown);
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return MessageHeader.GetSize() + Antenna.GetSize() + CharField.GetSize() + Unknown.Length;
    }
}