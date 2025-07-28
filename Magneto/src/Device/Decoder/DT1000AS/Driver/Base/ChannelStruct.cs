using System.Runtime.InteropServices;

namespace Magneto.Device.DT1000AS.Driver.Base;

[StructLayout(LayoutKind.Sequential)]
public struct ChannelStruct
{
    /// <summary>
    ///     信道类型
    /// </summary>
    public readonly short ChannelType;

    /// <summary>
    ///     时隙号
    /// </summary>
    public readonly byte TN;

    /// <summary>
    ///     用以指示SDCCH等用的是哪个数据块，若为8则为指配到TCH
    /// </summary>
    public readonly short BlkIndex;

    public readonly short BlkEndFN;
    public readonly short MAIO;
    public readonly short HSN;

    /// <summary>
    ///     训练序列号
    /// </summary>
    public readonly byte TSC;

    public readonly short Release_Flag;

    /// <summary>
    ///     指示使用的lapdm的block
    /// </summary>
    public readonly short lapdm_index;

    /// <summary>
    ///     物理频道数
    /// </summary>
    public readonly short RFNum;

    /// <summary>
    ///     B_SDCCH8_D, B_SDCCH4_D,B_TCH_FACCH_D, B_TCH_FACCH_H_D
    /// </summary>
    public readonly short AssChannelType;

    /// <summary>
    ///     64个频道号。
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public readonly ushort[] RFList;

    public short C0;
    public readonly byte Speech_Version;
    public readonly byte AMR_Config;
    public readonly short uplinkSDCCHPacket;
    public short downlinkSDCCHPacket;
    public short sdcch8succesiveError;
    public ushort downlinkTimeSlotSuccesiveError;
    public readonly short voidFrameNum;

    /// <summary>
    ///     record the FN for the immediate assignment
    /// </summary>
    public int ImmAssFN;

    public readonly int SMS_FN;
    public readonly int TaskIndex;

    /// <summary>
    ///     记录上一次接收数据的帧号，若检测长时间无数据，则终止
    /// </summary>
    public int last_RX_Data_FN;

    public byte ImmAssType;
    public byte IAslot;
    public byte PageType;
    public readonly byte CiphingFlag;
    public readonly byte SetupFlag;
    public byte serviceType;
    public byte connectType;

    /// <summary>
    ///     TMSI, IMSI, IMEI, NONE
    /// </summary>
    public byte MobileIdentityType;

    /// <summary>
    ///     the first value is the length
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public byte[] MobileIdentity;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public byte[] Phone_number;

    public readonly byte tmsi_match_flag;
    public byte tmsi_get;
    public readonly byte AuthRequest;
    public byte PageResponseInd;
    public byte CMServiceInd;
    public readonly byte PendingTaskInd;
    public byte pageResponseByte;
    public readonly int uplinkRSSI;
    public readonly short uplinkRSSICount;
    public readonly byte TA;
    public readonly short void_frame_count;
    public readonly byte request_RA;
    public readonly int request_FN;
    public byte location_update_request;
    public byte location_update_reject;
    public byte location_update_accept;
    public byte imsi_request;
}