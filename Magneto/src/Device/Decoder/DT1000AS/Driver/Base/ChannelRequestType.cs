namespace Magneto.Device.DT1000AS.Driver.Base;

public struct ChannelRequestType
{
    public byte PagingCall;
    public byte PagingSms;
    public byte OriginateCall;
    public byte OriginateSms;
    public byte LocationUpdate;
}