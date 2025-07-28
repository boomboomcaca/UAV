using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

//[0X79] – AGC MEASUREMENT REQUEST
//Message for selectively sending a request for AGC to the sensor.
internal class AgcMeasurementRequest
{
    public MessageHeader Header;

    public AgcMeasurementRequest()
    {
        Header = new MessageHeader(MessageId.MreDemMesureCag, 0);
    }

    public byte[] GetBytes()
    {
        Header.ContentSize = 0;
        return Header.GetBytes();
    }

    public int GetSize()
    {
        return MessageHeader.GetSize();
    }
}