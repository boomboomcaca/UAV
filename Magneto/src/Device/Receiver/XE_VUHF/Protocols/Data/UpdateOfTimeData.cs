using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

//[0X66] – TIME/GEO DATA UPDATING
//This message is sent spontaneously by the sensor.
internal class UpdateOfTimeData
{
    //[0x2052] GPS Heading in tenths of a degree (in case of problem of compass : 9999 value is sent)
    public ShortField GpsHeading;

    public MessageHeader Header;

    //[0x202B] Heading in tenths of a degree
    public ShortField Heading;

    //[0x2009] Latitude in ten-millionths of a degree (in case of problem of GPS: 0x7fffffff value is sent)
    public Int32Field Latitude;

    //[0x2008] Longitude in ten-millionths of a degree (in case of problem of GPS : 0x7fffffff value is sent)
    public Int32Field Longitude;

    //[0x2051] Pitch in tenths of a degree
    public ShortField Pitch;

    //[0x2050] Roll in tenths of a degree
    public ShortField Roll;

    //[0x2054] Number of satellites detected by GPS
    public UShortField Satellites;

    //[0x2053] Speed in tenth of a knot
    public UShortField Speed;

    //[0x2004] Current time in ns (see § 2.3.2.2)
    public UInt64Field Time;

    public UpdateOfTimeData(byte[] value, ref int startIndex, uint version)
    {
        Header = new MessageHeader(value, ref startIndex);
        Time = new UInt64Field(value, ref startIndex);
        Heading = new ShortField(value, ref startIndex);
        Latitude = new Int32Field(value, ref startIndex);
        Longitude = new Int32Field(value, ref startIndex);
        if (version >= 25)
        {
            //目前21版本的无以下几个字段
            Roll = new ShortField(value, ref startIndex);
            Pitch = new ShortField(value, ref startIndex);
            GpsHeading = new ShortField(value, ref startIndex);
            Speed = new UShortField(value, ref startIndex);
            Satellites = new UShortField(value, ref startIndex);
        }
    }
}