using System.Reflection;
using System.Runtime.InteropServices;
using Magneto.Contract;
using Magneto.Device.DT1000AS.Driver.Base;

namespace Magneto.Device.DT1000AS;

internal class DllInvoker
{
    internal const string LibPath = "GSMDll";
    internal const CallingConvention DriverCallingConvention = CallingConvention.StdCall;

    static DllInvoker()
    {
        Utils.ResolveDllImport(Assembly.GetExecutingAssembly(), "DT1000AS", new[] { LibPath });
    }

    [DllImport(LibPath, EntryPoint = "?Byte18Decoder@GSM@@QAEXQAEQAF0PAE22PAH@Z",
        CallingConvention = DriverCallingConvention)]
    public static extern void Byte18Decoder(
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 18)]
        byte[] byteIn,
        short[] tsBits114,
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 2)]
        byte[] stealBits2,
        ref byte errorFlag,
        ref byte rfIndex,
        ref byte tn,
        ref int fn
    );

    [DllImport(LibPath, EntryPoint = "?SCH_Decoder@GSM@@QAEEQAFPAHPAE22@Z",
        CallingConvention = DriverCallingConvention)]
    public static extern byte SCH_Decoder(short[] relib, ref int fn, ref byte tsn, ref byte plmn, ref byte errorBits);

    [DllImport(LibPath, EntryPoint = "?Lapdm_ByteReverse@GSM@@QAEXQACQAE@Z",
        CallingConvention = DriverCallingConvention)]
    public static extern void Lapdm_ByteReverse(
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 184)]
        byte[] lapDm184,
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 23)]
        byte[] lapDm23);

    [DllImport(LibPath, EntryPoint = "?Decoder_456@GSM@@QAEGQAFQAEFPAE@Z",
        CallingConvention = DriverCallingConvention)]
    public static extern ushort Decoder_456(ref short inBuffer,
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 184)]
        byte[] decoderOut, short channelType,
        ref byte decoderFlag);

    [DllImport(LibPath, EntryPoint = "?BCCH_Info@GSM@@QAEEQACQAEPAUBCCH_DataStruct@@PAUChannelStruct@@3@Z",
        CallingConvention = DriverCallingConvention)]
    public static extern byte BCCH_Info(
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 184)]
        byte[] lapDm184,
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 23)]
        byte[] lapDm23,
        ref BcchDataStruct bcchData,
        ref ChannelStruct phyicalCh1,
        ref ChannelStruct phyicalCh2);

    [DllImport(LibPath, EntryPoint = "?Return_SDCCH8_BlkIdx@GSM@@QAEFHE@Z",
        CallingConvention = DriverCallingConvention)]
    public static extern short Return_SDCCH8_BlkIdx(int fn, byte linktype);

    [DllImport(LibPath, EntryPoint = "?Mobile_Identity@GSM@@QAEEQAE0PAE1@Z",
        CallingConvention = DriverCallingConvention)]
    public static extern byte Mobile_Identity(ref byte byteIn,
        ref byte identity, ref byte bcdLen, ref byte identityType);

    [DllImport(LibPath, EntryPoint = "?ImmAssignPageType@GSM@@QAEEE@Z", CallingConvention = DriverCallingConvention)]
    public static extern byte ImmAssignPageType(byte bt);

    [DllImport(LibPath, EntryPoint = "?Byte23ToFormatBLAPDm@GSM@@QAEEQAE0PAG@Z",
        CallingConvention = DriverCallingConvention)]
    public static extern byte Byte23ToFormatBLAPDm([MarshalAs(UnmanagedType.LPArray, SizeConst = 23)] byte[] byte23,
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 512)]
        byte[] lapDm, ref ushort index);

    [DllImport(LibPath, EntryPoint = "?L3MessageType@GSM@@QAEEQAE@Z", CallingConvention = DriverCallingConvention)]
    public static extern byte L3MessageType([MarshalAs(UnmanagedType.LPArray, SizeConst = 512)] byte[] lapDm23);

    [DllImport(LibPath, EntryPoint = "?Page_Request@GSM@@QAEHQAE@Z", CallingConvention = DriverCallingConvention)]
    public static extern int Page_Request([MarshalAs(UnmanagedType.LPArray, SizeConst = 23)] byte[] lapdm23);

    [DllImport(LibPath, EntryPoint = "?SMS_Decode_FaksBS@GSM@@QAEFQAEF00QAG@Z",
        CallingConvention = DriverCallingConvention)]
    public static extern short SMS_Decode_FaksBS([MarshalAs(UnmanagedType.LPArray, SizeConst = 512)] byte[] byteIn,
        short dataLen,
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 64)]
        byte[] messageHeader,
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 22)]
        byte[] phoneId,
        ushort[] chineseSmSdata);
}