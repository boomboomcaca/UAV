using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

internal class FhChannelInfo
{
    //[0x8413] Centre frequency of the channel in Hz (N*ULONG)(4*N)
    public UserField<uint> ChannelList;

    public FhChannelInfo(byte[] value, ref int startIndex)
    {
        ChannelList = new UserField<uint>(value, ref startIndex);
    }
}