namespace Magneto.Device.XE_VUHF_28.Protocols.Field;

internal interface IField
{
    int Size { get; }
    byte[] GetBytes();
}