using Magneto.Device.DT1000AS.Driver.Base;

namespace Magneto.Device.DT1000AS.Driver;

public partial class Receiver
{
    private const byte DspHeader = 0xf7;

    private const int Message0Len = 18;

    //private const int MaxCellNum = 128;
    private const int RssiOffset = 140;

    //private const int RfNum = 2;
    private const int MaxLadpdmLen = 512;

    //private const int MaxLapdmByte = 230;
    private const int MaxSdcchTask = 8;
    private const int RrImmAss = 0x3f;

    private const int RrImmAssExt = 0x39;

    //private const int RrImmAssRej = 0x3a;
    //private const int RrAssCmd = 0x2e;
    //private const int RrHandoCmd = 0x2b;
    //private const int RrHandoCompl = 0x2c;
    //private const int RrChanRel = 0x0d;
    private const int RrPagReq1 = 0x21;
    private const int RrPagReq2 = 0x22;
    private const int RrPagReq3 = 0x24;

    private readonly byte[] _byte23 = new byte[23];

    //private const int RrSysinfo8 = 0x18;
    //private const int RrSysinfo1 = 0x19;
    //private const int RrSysinfo2 = 0x1a;
    //private const int RrSysinfo3 = 0x1b;
    //private const int RrSysinfo4 = 0x1c;
    //private const int RrSysinfo5 = 0x1d;
    //private const int RrSysinfo6 = 0x1e;
    //private const int RrSysinfo7 = 0x1f;
    //private const int RrSysinfo2Bis = 0x02;
    //private const int RrSysinfo2Ter = 0x03;
    //private const int RrSysinfo5Bis = 0x05;
    //private const int RrSysinfo5Ter = 0x06;
    //private const int RrSysinfo9 = 0x04;
    //private const int RrSysinfo13 = 0x00;
    //private const int RrSysinfo16 = 0x3d;
    //private const int RrSysinfo17 = 0x3e;
    //private const int CcAlerting = 0x01;
    //private const int CcCallConf = 0x08;
    //private const int CcCallProc = 0x02;
    //private const int CcConnect = 0x07;
    //private const int CcSetup = 0x05;
    //private const int CcDisconnect = 0x25;
    //private const int CcRelease = 0x2d;
    //private const int CcReleaseCompl = 0x2a;
    private readonly int[] _c2LWarning = new int[2];
    private readonly short[,] _ccchDataBuffer = new short[2, 16 * 114];
    private readonly char _iAPageType = default;
    private readonly SmsStruct[] _l3Message = new SmsStruct[MaxSdcchTask + 4];
    private readonly int[,] _lacList = new int[2, 256];
    private readonly byte[,] _messageHeader = new byte[12, 64];
    private readonly int[] _messageHeaderIdx = new int[12];

    private readonly short[,] _rxBuffer = new short[2, 16 * 114];

    //private byte _rfState;
    private readonly int[] _sDcch4ImmAssFn = new int[4];

    //private uint _targetImsi;
    private readonly ChannelStruct[] _taskSdcch4 = new ChannelStruct[4];
    private readonly ChannelStruct[] _taskSdcch8 = new ChannelStruct[MaxSdcchTask];
    private ushort _bcchC0;
    private BcchDataStruct _bcchData = new() { CA = new ushort[65], BA = new ushort[65], extBA = new ushort[65] };

    private byte _bsCcchans = 4;

    //private int _cellAll;
    private int _cellSearchFlag;
    private ChannelRequestType _channelRequestType;
    private CountStruct _count = new();

    private int _countAbnormal1;

    //private int _countAbnormal2;
    //private int _countCell;
    //private int _countCell2;
    //private int _countImedPage;
    //private byte _dspCheck;
    //private string _dspSoftId;
    //private ushort _equipmentId;
    //private int _fakebsC0;
    //private int _fakebsRssi;
    private int _fn;
    private int _fnSlot;
    private int _frameNum;

    private byte _locationUpdate;
    //private int _newCellCount;

    ///// <summary>
    /////     该标志是在小区搜索时，新小区使用前一个小区的数据
    ///// </summary>
    //private bool _newCellDataFlag = false;

    ///// <summary>
    /////     新的SIB3 寻呼等数据包来到会置 1
    ///// </summary>
    //private bool _newStateFlag;

    //private int _oldCellCount;
    private byte _originatingCall;
    private byte _pageCall;
    private ChannelStruct _phyicalChannel = new() { MobileIdentity = new byte[20], Phone_number = new byte[20] };

    private ChannelStruct _phyicalChannel2 = new() { MobileIdentity = new byte[20], Phone_number = new byte[20] };

    //private readonly byte[,] _sDcch4SmsBuffer = new byte[4, 40];
    //private readonly byte[] _sDcch4SmsFlag = new byte[4];
    private CellSearchRequest _searchRequest;

    private int _sms;

    //private byte _smsSdcchType;
    private int _smsShow;

    private int _speech;

    //private byte _stm8Check;
    private uint _tAcount;
    private sbyte _targetIAslot;
    private double _tAsum;
    private TimeoutCountStruct _timeoutCount;
    private int _timerForOneCell;
    private byte _tnSlot;

    private void CellSearchTimer_Elapsed()
    {
        //CellSearchForSystemInfo();
        _timerForOneCell++;
    }

    private void NoRxDatatimer_Elapsed()
    {
        //if (!_actionFlag && _cellSearchFlag == 0)
        //{
        //    OnBnClickedButtonrx();
        //}
        //if (_cellSearchFlag == 1 && !_actionFlag)
        //{
        //    CellSearch();
        //}
        //CellSearch();
        //_actionFlag = false;
    }
}