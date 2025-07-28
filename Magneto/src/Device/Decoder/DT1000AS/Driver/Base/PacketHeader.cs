namespace Magneto.Device.DT1000AS.Driver.Base;

public enum PacketHeader
{
    PacketSlot = 1,
    PacketC2I,
    PacketId,
    PacketBcchRssi,
    PacketSdcchRssi,
    PacketTchRssi,
    PacketBcchArfcn,
    PacketHeartBeat
}