namespace Magneto.Device.DT1000AS.Driver.Base;

public class CountStruct
{
    /// <summary>
    ///     普通突发统计
    /// </summary>
    public int NbCount { get; set; }

    /// <summary>
    ///     the number of SCH burst
    /// </summary>
    public int SbCount { get; set; }

    /// <summary>
    ///     the fail of SB decoding
    /// </summary>
    public int SbCrcError { get; set; }

    /// <summary>
    ///     the fail of BCCH  decoding
    /// </summary>
    public int BcchError { get; set; }

    /// <summary>
    ///     the number of buffers
    /// </summary>
    public int BufferNum { get; set; }

    /// <summary>
    ///     统计立即指配的个数
    /// </summary>
    public int ImmAssign { get; set; }

    public int IaOriginatingCall { get; set; }
    public int IaPacket { get; set; }
    public int Calls { get; set; }
    public int IaLocationUpdate { get; set; }
    public int PageReq { get; set; }
    public int Lapdm184Count { get; set; }
    public int RxMessageResponse { get; set; }
    public int Sdcch4 { get; set; }
    public int Conflict { get; set; }
    public int ThreadNum { get; set; }
    public int SmsFlagCount { get; set; }
    public int Sms { get; set; }
    public double RxBits { get; set; }
    public double RxErrorBits { get; set; }
    public int CrcFailExit { get; set; }
    public int ImmAssignExt { get; set; }
    public int TaskSendtoDsp { get; set; }
    public int SpeechCount { get; set; }
    public int[] SacchPacket { get; set; } = new int[2];
    public int[] SdcchPacket { get; set; } = new int[2];
    public int[] FacchPacket { get; set; } = new int[2];
    public int[] SpeechPacket { get; set; } = new int[2];
    public int[] SidFirstPackt { get; set; } = new int[2];
    public int[] SidUpdatePackt { get; set; } = new int[2];
    public int[] CorrectBurst { get; set; } = new int[2];
    public int[] SuccesiveErrorPacket { get; set; } = new int[2];
    public short[,] Sdcch8 { get; set; } = new short[2, 8];
    public short[,] Sdcch8SuccesiveError { get; set; } = new short[2, 8];
    public int ValidImmAssign { get; set; }
    public ushort TmsiCatch { get; set; }
    public int LocationUpdateRequest { get; set; }
    public int LocationUpdateAccept { get; set; }
    public int LocationUpdateReject { get; set; }
    public int ImsiRequest { get; set; }
}