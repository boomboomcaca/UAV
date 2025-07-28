using System.Runtime.InteropServices;
using System.Text;

namespace Magneto.Device.ESMC.SDK;

public class Gpib
{
    /// <summary>
    ///     GPIB Bus Control Lines bit vector
    /// </summary>
    public enum BusControlLine
    {
        ValidDav = 0x01,
        ValidNdac = 0x02,
        ValidNrfd = 0x04,
        ValidIfc = 0x08,
        ValidRen = 0x10,
        ValidSrq = 0x20,
        ValidAtn = 0x40,
        ValidEoi = 0x80,
        ValidAll = 0xff,
        BusDav = 0x0100, /* DAV  line status bit */
        BusNdac = 0x0200, /* NDAC line status bit */
        BusNrfd = 0x0400, /* NRFD line status bit */
        BusIfc = 0x0800, /* IFC  line status bit */
        BusRen = 0x1000, /* REN  line status bit */
        BusSrq = 0x2000, /* SRQ  line status bit */
        BusAtn = 0x4000, /* ATN  line status bit */
        BusEoi = 0x8000 /* EOI  line status bit */
    }

    /// <summary>
    ///     End-of-string (EOS) modes for use with ibeos
    /// </summary>
    public enum EosFlags
    {
        EosMask = 0x1c00,
        Reos = 0x0400, /* Terminate reads on EOS	*/
        Xeos = 0x800, /* assert EOI when EOS char is sent */
        Bin = 0x1000 /* Do 8-bit compare on EOS	*/
    }

    /// <summary>
    ///     Timeout values and meanings
    /// </summary>
    public enum GpibTimeout
    {
        Tnone = 0, /* Infinite timeout (disabled)     */
        T10Us = 1, /* Timeout of 10 usec (ideal)      */
        T30Us = 2, /* Timeout of 30 usec (ideal)      */
        T100Us = 3, /* Timeout of 100 usec (ideal)     */
        T300Us = 4, /* Timeout of 300 usec (ideal)     */
        T1Ms = 5, /* Timeout of 1 msec (ideal)       */
        T3Ms = 6, /* Timeout of 3 msec (ideal)       */
        T10Ms = 7, /* Timeout of 10 msec (ideal)      */
        T30Ms = 8, /* Timeout of 30 msec (ideal)      */
        T100Ms = 9, /* Timeout of 100 msec (ideal)     */
        T300Ms = 10, /* Timeout of 300 msec (ideal)     */
        T1S = 11, /* Timeout of 1 sec (ideal)        */
        T3S = 12, /* Timeout of 3 sec (ideal)        */
        T10S = 13, /* Timeout of 10 sec (ideal)       */
        T30S = 14, /* Timeout of 30 sec (ideal)       */
        T100S = 15, /* Timeout of 100 sec (ideal)      */
        T300S = 16, /* Timeout of 300 sec (ideal)      */
        T1000S = 17 /* Timeout of 1000 sec (maximum)   */
    }

    public enum IbaskOption
    {
        IbaPad = 0x1,
        IbaSad = 0x2,
        IbaTmo = 0x3,
        IbaEot = 0x4,
        IbaPpc = 0x5, /* board only */
        IbaReaddr = 0x6, /* device only */
        IbaAutopoll = 0x7, /* board only */
        IbaCicprot = 0x8, /* board only */
        IbaIrq = 0x9, /* board only */
        IbaSc = 0xa, /* board only */
        IbaSre = 0xb, /* board only */
        IbaEoSrd = 0xc,
        IbaEoSwrt = 0xd,
        IbaEoScmp = 0xe,
        IbaEoSchar = 0xf,
        IbaPp2 = 0x10, /* board only */
        IbaTiming = 0x11, /* board only */
        IbaDma = 0x12, /* board only */
        IbaReadAdjust = 0x13,
        IbaWriteAdjust = 0x14,
        IbaEventQueue = 0x15, /* board only */
        IbaSPollBit = 0x16, /* board only */
        IbaSpollBit = 0x16, /* board only */
        IbaSendLlo = 0x17, /* board only */
        IbaSPollTime = 0x18, /* device only */
        IbaPPollTime = 0x19, /* board only */
        IbaEndBitIsNormal = 0x1a,
        IbaUnAddr = 0x1b, /* device only */
        IbaHsCableLength = 0x1f, /* board only */
        IbaIst = 0x20, /* board only */
        IbaRsv = 0x21, /* board only */
        IbaBna = 0x200, /* device only */
        IbaBaseAddr = 0x201 /* GPIB board's base I/O address.*/
    }

    public enum IbconfigOption
    {
        IbcPad = 0x1,
        IbcSad = 0x2,
        IbcTmo = 0x3,
        IbcEot = 0x4,
        IbcPpc = 0x5, /* board only */
        IbcReaddr = 0x6, /* device only */
        IbcAutopoll = 0x7, /* board only */
        IbcCicprot = 0x8, /* board only */
        IbcIrq = 0x9, /* board only */
        IbcSc = 0xa, /* board only */
        IbcSre = 0xb, /* board only */
        IbcEoSrd = 0xc,
        IbcEoSwrt = 0xd,
        IbcEoScmp = 0xe,
        IbcEoSchar = 0xf,
        IbcPp2 = 0x10, /* board only */
        IbcTiming = 0x11, /* board only */
        IbcDma = 0x12, /* board only */
        IbcReadAdjust = 0x13,
        IbcWriteAdjust = 0x14,
        IbcEventQueue = 0x15, /* board only */
        IbcSPollBit = 0x16, /* board only */
        IbcSpollBit = 0x16, /* board only */
        IbcSendLlo = 0x17, /* board only */
        IbcSPollTime = 0x18, /* device only */
        IbcPPollTime = 0x19, /* board only */
        IbcEndBitIsNormal = 0x1a,
        IbcUnAddr = 0x1b, /* device only */
        IbcHsCableLength = 0x1f, /* board only */
        IbcIst = 0x20, /* board only */
        IbcRsv = 0x21, /* board only */
        IbcLon = 0x22,
        IbcBna = 0x200 /* device only */
    }

    /// <summary>
    ///     IBERR error codes
    /// </summary>
    public enum IberrCode
    {
        Edvr = 0, /* system error */
        Ecic = 1, /* not CIC */
        Enol = 2, /* no listeners */
        Eadr = 3, /* CIC and not addressed before I/O */
        Earg = 4, /* bad argument to function call */
        Esac = 5, /* not SAC */
        Eabo = 6, /* I/O operation was aborted */
        Eneb = 7, /* non-existent board (GPIB interface offline) */
        Edma = 8, /* DMA hardware error detected */
        Eoip = 10, /* new I/O attempted with old I/O in progress  */
        Ecap = 11, /* no capability for intended opeation */
        Efso = 12, /* file system operation error */
        Ebus = 14, /* bus error */
        Estb = 15, /* lost serial poll bytes */
        Esrq = 16, /* SRQ stuck on */
        Etab = 20 /* Table Overflow */
    }

    public enum IbstaBitNumbers
    {
        DcasNum = 0,
        DtasNum = 1,
        LacsNum = 2,
        TacsNum = 3,
        AtnNum = 4,
        CicNum = 5,
        RemNum = 6,
        LokNum = 7,
        CmplNum = 8,
        EventNum = 9,
        SpollNum = 10,
        RqsNum = 11,
        SrqiNum = 12,
        EndNum = 13,
        TimoNum = 14,
        ErrNum = 15
    }

    /// <summary>
    ///     IBSTA status bits (returned by all functions)
    /// </summary>
    public enum IbstaBits
    {
        Dcas = 1 << IbstaBitNumbers.DcasNum, /* device clear state */
        Dtas = 1 << IbstaBitNumbers.DtasNum, /* device trigger state */
        Lacs = 1 << IbstaBitNumbers.LacsNum, /* GPIB interface is addressed as Listener */
        Tacs = 1 << IbstaBitNumbers.TacsNum, /* GPIB interface is addressed as Talker */
        Atn = 1 << IbstaBitNumbers.AtnNum, /* Attention is asserted */
        Cic = 1 << IbstaBitNumbers.CicNum, /* GPIB interface is Controller-in-Charge */
        Rem = 1 << IbstaBitNumbers.RemNum, /* remote state */
        Lok = 1 << IbstaBitNumbers.LokNum, /* lockout state */
        Cmpl = 1 << IbstaBitNumbers.CmplNum, /* I/O is complete  */
        Event = 1 << IbstaBitNumbers.EventNum, /* DCAS, DTAS, or IFC has occurred */
        Spoll = 1 << IbstaBitNumbers.SpollNum, /* board serial polled by busmaster */
        Rqs = 1 << IbstaBitNumbers.RqsNum, /* Device requesting service  */
        Srqi = 1 << IbstaBitNumbers.SrqiNum, /* SRQ is asserted */
        End = 1 << IbstaBitNumbers.EndNum, /* EOI or EOS encountered */
        Timo = 1 << IbstaBitNumbers.TimoNum, /* Time limit on I/O or wait function exceeded */
        Err = 1 << IbstaBitNumbers.ErrNum /* Function call terminated on error */
    }

    public enum T1Delays
    {
        T1Delay2000Ns = 1,
        T1Delay500Ns = 2,
        T1Delay350Ns = 3
    }

    /// <summary>
    ///     max address for primary/secondary gpib addresses
    /// </summary>
    private const int GpibAddrMax = 30;

    public const ushort Noaddr = 0xffff;

    #region P/Invoke

    private const string ApiLibName = "library\\ESMC\\x86\\Gpib-32.dll";

    /*  IEEE 488 Function Prototypes  */
    [DllImport(ApiLibName)]
    public static extern int ibask(int ud, int option, ref int value);

    [DllImport(ApiLibName)]
    public static extern int ibbna(int ud, string boardName);

    [DllImport(ApiLibName)]
    public static extern int ibcac(int ud, int synchronous);

    [DllImport(ApiLibName)]
    public static extern int ibclr(int ud);

    [DllImport(ApiLibName)]
    public static extern int ibcmd(int ud, string cmd, long cnt);

    [DllImport(ApiLibName)]
    public static extern int ibcmda(int ud, string cmd, long cnt);

    [DllImport(ApiLibName)]
    public static extern int ibconfig(int ud, int option, int value);

    [DllImport(ApiLibName)]
    public static extern int ibdev(int boardIndex, int pad, int sad, int timo, int sendEoi, int eosmode);

    [DllImport(ApiLibName)]
    public static extern int ibdma(int ud, int v);

    [DllImport(ApiLibName)]
    public static extern int ibeot(int ud, int v);

    [DllImport(ApiLibName)]
    public static extern int ibeos(int ud, int v);

    [DllImport(ApiLibName)]
    public static extern int ibfind(string dev);

    [DllImport(ApiLibName)]
    public static extern int ibgts(int ud, int shadowHandshake);

    [DllImport(ApiLibName)]
    public static extern int ibist(int ud, int ist);

    [DllImport(ApiLibName)]
    public static extern int iblines(int ud, short lineStatus);

    [DllImport(ApiLibName)]
    public static extern int ibln(int ud, int pad, int sad, short foundListener);

    [DllImport(ApiLibName)]
    public static extern int ibloc(int ud);

    [DllImport(ApiLibName)]
    public static extern int ibonl(int ud, int onl);

    [DllImport(ApiLibName)]
    public static extern int ibpad(int ud, int v);

    [DllImport(ApiLibName)]
    public static extern int ibpct(int ud);

    [DllImport(ApiLibName)]
    public static extern int ibppc(int ud, int v);

    [DllImport(ApiLibName)]
    public static extern int ibrd(int ud, StringBuilder buf, long count);

    [DllImport(ApiLibName)]
    public static extern int ibrda(int ud, StringBuilder buf, long count);

    [DllImport(ApiLibName)]
    public static extern int ibrdf(int ud, char filePath);

    [DllImport(ApiLibName)]
    public static extern int ibrpp(int ud, char ppr);

    [DllImport(ApiLibName)]
    public static extern int ibrsc(int ud, int v);

    [DllImport(ApiLibName)]
    public static extern int ibrsp(int ud, StringBuilder spr);

    [DllImport(ApiLibName)]
    public static extern int ibrsv(int ud, int v);

    [DllImport(ApiLibName)]
    public static extern int ibsad(int ud, int v);

    [DllImport(ApiLibName)]
    public static extern int ibsic(int ud);

    [DllImport(ApiLibName)]
    public static extern int ibsre(int ud, int v);

    [DllImport(ApiLibName)]
    public static extern int ibstop(int ud);

    [DllImport(ApiLibName)]
    public static extern int ibtmo(int ud, int v);

    [DllImport(ApiLibName)]
    public static extern int ibtrg(int ud);

    [DllImport(ApiLibName)]
    public static extern int ibwait(int ud, int mask);

    [DllImport(ApiLibName)]
    public static extern int ibwrt(int ud, string buf, long count);

    [DllImport(ApiLibName)]
    public static extern int ibwrta(int ud, string buf, long count);

    [DllImport(ApiLibName)]
    public static extern int ibwrtf(int ud, StringBuilder filePath);

    [DllImport(ApiLibName)]
    public static extern int gpib_get_globals(out int pibsta, out int piberr, out int pibcnt, out int pibcntl);

    /*  IEEE 488.2 Function Prototypes  */
    [DllImport(ApiLibName)]
    public static extern void AllSPoll(int boardDesc, ushort[] addressList, ushort[] resultList);

    [DllImport(ApiLibName)]
    public static extern void AllSpoll(int boardDesc, ushort[] addressLis, ushort[] resultList);

    [DllImport(ApiLibName)]
    public static extern void DevClear(int boardDesc, ushort address);

    [DllImport(ApiLibName)]
    public static extern void DevClearList(int boardDesc, ushort[] addressLis);

    [DllImport(ApiLibName)]
    public static extern void EnableLocal(int boardDesc, ushort[] addressLis);

    [DllImport(ApiLibName)]
    public static extern void EnableRemote(int boardDesc, ushort[] addressLis);

    [DllImport(ApiLibName)]
    public static extern void FindLstn(int boardDesc, ushort[] padList, ushort[] resultList, int maxNumResults);

    [DllImport(ApiLibName)]
    public static extern void FindRQS(int boardDesc, ushort[] addressList, out short result);

    [DllImport(ApiLibName)]
    public static extern void PassControl(int boardDesc, ushort address);

    [DllImport(ApiLibName)]
    public static extern void PPoll(int boardDesc, out short result);

    [DllImport(ApiLibName)]
    public static extern void PPollConfig(int boardDesc, ushort address, int dataLine, int lineSense);

    [DllImport(ApiLibName)]
    public static extern void PPollUnconfig(int boardDesc, ushort[] addressList);

    [DllImport(ApiLibName)]
    public static extern void RcvRespMsg(int boardDesc, char[] buffer, long count, int termination);

    [DllImport(ApiLibName)]
    public static extern void ReadStatusByte(int boardDesc, ushort address, out short result);

    [DllImport(ApiLibName)]
    public static extern void Receive(int boardDesc, ushort address, char[] buffer, long count, int termination);

    [DllImport(ApiLibName)]
    public static extern void ReceiveSetup(int boardDesc, ushort address);

    [DllImport(ApiLibName)]
    public static extern void ResetSys(int boardDesc, ushort[] addressList);

    [DllImport(ApiLibName)]
    public static extern void Send(int boardDesc, ushort address, char[] buffer, long count, int eotMode);

    [DllImport(ApiLibName)]
    public static extern void SendCmds(int boardDesc, char[] cmds, long count);

    [DllImport(ApiLibName)]
    public static extern void SendDataBytes(int boardDesc, char[] buffer, long count, int eotmode);

    [DllImport(ApiLibName)]
    public static extern void SendIFC(int boardDesc);

    [DllImport(ApiLibName)]
    public static extern void SendLLO(int boardDesc);

    [DllImport(ApiLibName)]
    public static extern void SendList(int boardDesc, ushort[] addressList, char[] buffer, long count, int eotmode);

    [DllImport(ApiLibName)]
    public static extern void SendSetup(int boardDesc, ushort[] addressList);

    [DllImport(ApiLibName)]
    public static extern void SetRWLS(int boardDesc, ushort[] addressList);

    [DllImport(ApiLibName)]
    public static extern void TestSRQ(int boardDesc, out short result);

    [DllImport(ApiLibName)]
    public static extern void TestSys(int boardDesc, ushort[] addrlist, short[] resultList);

    [DllImport(ApiLibName)]
    public static extern void Trigger(int boardDesc, ushort address);

    [DllImport(ApiLibName)]
    public static extern void TriggerList(int boardDesc, ushort[] addressList);

    [DllImport(ApiLibName)]
    public static extern void WaitSRQ(int boardDesc, out short result);

    #endregion
}