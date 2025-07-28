namespace Magneto.Contract.AIS.Interface;

public interface IAisDecodable
{
    IAisMessage Decode(string decBytes);
}