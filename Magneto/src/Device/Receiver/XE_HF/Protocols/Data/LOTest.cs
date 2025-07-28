using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

internal class LoTest
{
    //[0x9008] Frequency used in this LO test in Hz (N*DOUBLE)
    public UserField<double> FreqList1; //Hz

    //[0x9008] Frequency used in this LO test in Hz (N*DOUBLE)
    public UserField<double> FreqList2; //Hz

    //[0x9006] DF LO Test or [0x9007] Listening LO Test
    public GroupField GroupLoTest;

    //[0x202E] Name of the antenna used in this LO test
    public MultiBytesField Name;

    public LoTest(byte[] value, ref int startIndex)
    {
        GroupLoTest = new GroupField(value, ref startIndex);
        Name = new MultiBytesField(value, ref startIndex);
        FreqList1 = new UserField<double>(value, ref startIndex);
        FreqList2 = new UserField<double>(value, ref startIndex);
    }
}