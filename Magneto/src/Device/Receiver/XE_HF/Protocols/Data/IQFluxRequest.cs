using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

//[0X0C] – IQ FLUX REQUEST
//Broadband and narrow band IQ flux requests have the same format. Homing request to have DF results in listening mode.
internal class IqFluxRequest
{
    //[0x2032] Number of the channel from which the IQ flux is extracted. If equal to 0, the numbers of the channels
    //contained in each track which are used to manage the multiple-acquisition recordings
    public UCharField ChannelNo;

    //[0x8302] Recording duration in ms. Infinite if it is equal to 0
    public UInt32Field Duration;

    public MessageHeader Header;

    //bg--25版本添加参数
    //[0x204B] Id of the narrow band channel for Homing (0 : No Homing)
    public UCharField HomingIdChannel;

    //[0x6005] Track identifier used in the results (direction-finding)
    public UInt32Field HomingIdentify;

    //[0x450F] Homing threshold value (absolute, -174 to +20 dBm)
    public ShortField HomingThreshold;

    //---ed
    //[0x831B] [1,4] Number of tracks
    public UCharField NumOfTracks;

    //List of recording tracks
    public IqFluxTrack[] Tracks;

    //[0x8304] Type of trigger :
    //0 : internal trigger, 1 : external trigger
    public UCharField Trigger;

    public IqFluxRequest()
    {
        Header = new MessageHeader(MessageId.MreDemFluxIq, 0);
        ChannelNo = new UCharField(0x2032);
        Duration = new UInt32Field(0x8302);
        Trigger = new UCharField(0x8304);
        //bg--25版本添加参数
        HomingIdChannel = new UCharField(0x204B);
        HomingIdentify = new UInt32Field(0x6005);
        HomingThreshold = new ShortField(0x450F);
        //---ed
        NumOfTracks = new UCharField(0x831B);
        Tracks = null;
    }

    public byte[] GetBytes(uint version)
    {
        NumOfTracks.Value = (byte)(Tracks == null ? 0 : Tracks.Length);
        Header.ContentSize = GetSize() - Header.GetSize();
        if (version != 25)
            Header.ContentSize -= HomingIdChannel.GetSize() + HomingIdentify.GetSize() + HomingThreshold.GetSize();
        var bytes = new List<byte>();
        bytes.AddRange(Header.GetBytes());
        bytes.AddRange(ChannelNo.GetBytes());
        bytes.AddRange(Duration.GetBytes());
        bytes.AddRange(Trigger.GetBytes());
        if (version == 25)
        {
            bytes.AddRange(HomingIdChannel.GetBytes());
            bytes.AddRange(HomingIdentify.GetBytes());
            bytes.AddRange(HomingThreshold.GetBytes());
        }

        bytes.AddRange(NumOfTracks.GetBytes());
        if (Tracks != null)
            foreach (var track in Tracks)
                bytes.AddRange(track.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        var totalSize = Header.GetSize() + ChannelNo.GetSize() + Duration.GetSize()
                        + Trigger.GetSize() + HomingIdChannel.GetSize() + HomingIdentify.GetSize()
                        + HomingThreshold.GetSize() + NumOfTracks.GetSize();
        if (Tracks != null)
            foreach (var track in Tracks)
                totalSize += track.GetSize();
        return totalSize;
    }
}