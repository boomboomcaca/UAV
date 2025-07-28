namespace Magneto.Contract.AIS.Interface;

public interface IAisMessage
{
    //Do not add any methods because this is a marker interface
    AisMessageType MsgId { get; }
    int Mmsi { get; }
}