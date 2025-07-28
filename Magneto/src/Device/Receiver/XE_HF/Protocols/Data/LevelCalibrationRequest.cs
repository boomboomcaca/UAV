using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

//[0X74] – LEVEL CALIBRATION REQUEST
//Activates the feedback of level calibration results.
internal class LevelCalibrationRequest
{
    //[0x8B00] Type of calibration :
    //0 : stop calibration
    //1 : start calibration
    //2 : request for internal calibration
    public CharField Calibration;

    public MessageHeader Header;

    //[0x8B01] UDP reception port for the level calibration results
    public UShortField Port;

    public LevelCalibrationRequest()
    {
        Header = new MessageHeader(MessageId.MreDemCalibration, 0);
        Calibration = new CharField(0x8B00);
        Port = new UShortField(0x8B01);
    }

    public byte[] GetBytes()
    {
        Header.ContentSize = GetSize() - Header.GetSize();
        var bytes = new List<byte>();
        bytes.AddRange(Header.GetBytes());
        bytes.AddRange(Calibration.GetBytes());
        bytes.AddRange(Port.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return Header.GetSize() + Calibration.GetSize() + Port.GetSize();
    }
}