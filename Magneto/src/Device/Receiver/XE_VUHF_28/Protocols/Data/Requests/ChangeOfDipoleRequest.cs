using System.Collections.Generic;
using Magneto.Device.XE_VUHF_28.Protocols.Field;

namespace Magneto.Device.XE_VUHF_28.Protocols.Data;

//[0X17] – CHANGE OF DIPOLE REQUEST
//Message to change dipole on a DF antenna. Useful to visualize active spectrum on maintenance. For TRC6200 unit only.
internal class ChangeOfDipoleRequest
{
    //[0x2032] Channel number
    public UCharField ChannelNo;

    public MessageHeader Header;

    //[0x9001] Switching number
    public UCharField SwitchingNo;

    public ChangeOfDipoleRequest()
    {
        Header = new MessageHeader(MessageId.MreDemChangeDipole, 0);
        ChannelNo = new UCharField(0x2032);
        SwitchingNo = new UCharField(0x9001);
    }

    public byte[] GetBytes()
    {
        Header.ContentSize = UCharField.GetSize() + UCharField.GetSize();
        var bytes = new List<byte>();
        bytes.AddRange(Header.GetBytes());
        bytes.AddRange(ChannelNo.GetBytes());
        bytes.AddRange(SwitchingNo.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return MessageHeader.GetSize() + UCharField.GetSize() + UCharField.GetSize();
    }
}