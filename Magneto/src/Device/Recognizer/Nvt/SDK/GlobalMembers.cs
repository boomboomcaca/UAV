#pragma warning disable CA2101 // Specify marshaling for P/Invoke string arguments

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Magneto.Contract;

namespace Magneto.Device.Nvt.SDK;

public static class GlobalMembers
{
    /// <summary>
    ///     解码输出回调
    /// </summary>
    /// <param name="hTmCc"></param>
    /// <param name="pStreamInfo"></param>
    /// <param name="context"></param>
    public delegate void AvFrameCallback(IntPtr hTmCc, ref TmAvImageInfoT pStreamInfo, IntPtr context);

    /// <summary>
    ///     连接状态回调
    /// </summary>
    /// <param name="hTmCc"></param>
    /// <param name="connect"></param>
    /// <param name="dwResult"></param>
    /// <param name="context"></param>
    public delegate void ConnectCallback(IntPtr hTmCc, bool connect, uint dwResult, IntPtr context);

    /// <summary>
    ///     报警信息等设备数据回调
    /// </summary>
    /// <param name="hTmCc"></param>
    /// <param name="pCommandInfo"></param>
    /// <param name="context"></param>
    public delegate void DeviceDataCallback(IntPtr hTmCc, ref TmCommandInfoT pCommandInfo, IntPtr context);

    /// <summary>
    ///     实时流回调
    /// </summary>
    /// <param name="hTmCc"></param>
    /// <param name="pStreamInfo"></param>
    /// <param name="context"></param>
    public delegate void StreamCallback(IntPtr hTmCc, ref TmRealStreamInfoT pStreamInfo, IntPtr context);

    /// <summary>
    ///     绘制回调
    /// </summary>
    /// <param name="hTmCc"></param>
    /// <param name="hDc"></param>
    /// <param name="pRect"></param>
    /// <param name="iRegionNum"></param>
    /// <param name="context"></param>
    public delegate void TmccDrawCallback(IntPtr hTmCc, IntPtr hDc, ref Rect pRect, int iRegionNum, IntPtr context);

    private const string LibPath = "NvtSdk";

    /*初始化类型*/
    /// <summary>
    ///     初始化成设备控制SDK句柄
    /// </summary>
    public const int TmccInitTypeControl = 0x00;

    /// <summary>
    ///     初始化成枚举SDK句柄
    /// </summary>
    public const int TmccInitTypeEnum = 0x01;

    /// <summary>
    ///     初始化成升级SDK句柄
    /// </summary>
    public const int TmccInitTypeUpgrade = 0x02;

    /// <summary>
    ///     初始化成语音对讲SDK句柄
    /// </summary>
    public const int TmccInitTypeTalk = 0x03;

    /// <summary>
    ///     初始化成播放数据流SDK句柄
    /// </summary>
    public const int TmccInitTypeStream = 0x04;

    /// <summary>
    ///     初始化成播放实时数据流SDK句柄
    /// </summary>
    public const int TmccInitTypeRealStream = 0x05;

    /// <summary>
    ///     初始化为状态接收和报警接收SDK句柄
    /// </summary>
    public const int TmccInitTypeListen = 0x06;

    /// <summary>
    ///     初始化为视频显示SDK句柄
    /// </summary>
    public const int TmccInitTypeVideoRender = 0x07;

    /// <summary>
    ///     初始化为语音对讲数据解码SDK句柄
    /// </summary>
    public const int TmccInitTypeVoiceRender = 0x08;

    /// <summary>
    ///     初始化为主动监听设备上传SDK句柄
    /// </summary>
    public const int TmccInitTypeListenDevice = 0x09;

    /// <summary>
    ///     初始化为音视频解码SDK句柄
    /// </summary>
    public const int TmccInitTypeAvDecoder = 0x0A;

    /// <summary>
    ///     初始化为播放远程文件SDK句柄(通过回调读取数据)
    /// </summary>
    public const int TmccInitTypePlayRemoteFile = 0x0B;

    /// <summary>
    ///     初始化为播放RTP包的SDK句柄
    /// </summary>
    public const int TmccInitTypeRtpStream = 0x0C;

    /// <summary>
    ///     初始化为图像处理SDK句柄
    /// </summary>
    public const int TmccInitTypeImageProcess = 0x0D;

    /// <summary>
    ///     初始化为TFTP升级SDK句柄
    /// </summary>
    public const int TmccInitTypeTftpUpgrade = 0x0E;

    /// <summary>
    ///     初始化为远程文件控制SDK句柄
    /// </summary>
    public const int TmccInitTypeRemoteControlFile = 0x20;

    /// <summary>
    ///     初始化为远程文件读取SDK句柄
    /// </summary>
    public const int TmccInitTypeRemoteReadFile = 0x21;

    /// <summary>
    ///     初始化为播放远程文件SDK句柄
    /// </summary>
    public const int TmccInitTypeRemoteFile = 0x22;

    /// <summary>
    ///     初始化为播放远程文件SDK句柄
    /// </summary>
    public const int TmccInitTypeRemotePlay = 0x23;

    /// <summary>
    ///     初始化为远程老版本文件读取SDK句柄
    /// </summary>
    public const int TmccInitTypeRemoteTransFile = 0x24;

    /// <summary>
    ///     初始化为本地文件播放SDK句柄
    /// </summary>
    public const int TmccInitTypeLocalFile = 0x25;

    /// <summary>
    ///     操作成功
    /// </summary>
    public const int TmccErrSuccess = 0;

    /// <summary>
    ///     远程服务器控制
    /// </summary>
    public const int TmccMajorCmdServerControl = 0x111;

    /// <summary>
    ///     远程手动抓图传到本地
    /// </summary>
    public const int TmccMinorCmdManualCapture = 0x10;

    /// <summary>
    ///     视频输入配置
    /// </summary>
    public const int TmccMajorCmdVideoInCfg = 0x116;

    /// <summary>
    ///     输入配置
    /// </summary>
    public const int TmccMinorCmdVideoIn = 0x00;

    /// <summary>
    ///     服务器消息
    /// </summary>
    public const int TmccMajorCmdServerMessage = 0x10F;

    /// <summary>
    ///     报警上传管理中心具体信息看tmToManagerAlarmInfo_t
    /// </summary>
    public const int TmccMinorCmdServerAlarm = 0x04;

    /*手动跟踪相关*/
    /// <summary>
    ///     主命令：跟踪器配置
    /// </summary>
    public const int TmccMajorCmdTracerCfg = 0x127;

    /// <summary>
    ///     子命令：选择目标
    /// </summary>
    public const int TmccMinorCmdSelectObject = 0x00;

    /// <summary>
    ///     子命令：智能分析配置
    /// </summary>
    public const int TmccMinorCmdAiVideoCfg = 0x01;

    /// <summary>
    ///     子命令：停止跟踪
    /// </summary>
    public const int TmccMinorCmdStopTrace = 0x02;

    /// <summary>
    ///     子命令：跟踪属性参数配置
    /// </summary>
    public const int TmccMinorCmdParameter = 0x04;

    /*云台控制相关定义*/
    /// <summary>
    ///     透明云台数据传输
    /// </summary>
    public const int PtzRealTrans = 0;

    /// <summary>
    ///     设置控制模式
    /// </summary>
    public const int PtzSetCmdMode = 1;

    /// <summary>
    ///     接通灯光电源 1
    /// </summary>
    public const int PtzLightPwrOn = 2;

    /// <summary>
    ///     接通雨刷开关 2
    /// </summary>
    public const int PtzWiperPwrOn = 3;

    /// <summary>
    ///     接通风扇开关 3
    /// </summary>
    public const int PtzFanPwrOn = 4;

    /// <summary>
    ///     接通加热器开关 4
    /// </summary>
    public const int PtzHeaterPwrOn = 5;

    /// <summary>
    ///     接通辅助设备开关 5
    /// </summary>
    public const int PtzAuxPwrOn = 6;

    /// <summary>
    ///     自动聚焦
    /// </summary>
    public const int PtzFocusAuto = 7;

    /// <summary>
    ///     焦距以速度SS变大(倍率变大)
    /// </summary>
    public const int PtzZoomIn = 11;

    /// <summary>
    ///     焦距以速度SS变小(倍率变小)
    /// </summary>
    public const int PtzZoomOut = 12;

    /// <summary>
    ///     焦点以速度SS前调
    /// </summary>
    public const int PtzFocusNear = 13;

    /// <summary>
    ///     焦点以速度SS后调
    /// </summary>
    public const int PtzFocusFar = 14;

    /// <summary>
    ///     云台向上
    /// </summary>
    public const int PtzUp = 21;

    /// <summary>
    ///     云台向下
    /// </summary>
    public const int PtzDown = 22;

    /// <summary>
    ///     云台向左
    /// </summary>
    public const int PtzLeft = 23;

    /// <summary>
    ///     云台向右
    /// </summary>
    public const int PtzRight = 24;

    /// <summary>
    ///     云台向右上
    /// </summary>
    public const int PtzRightUp = 25;

    /// <summary>
    ///     云台向右下
    /// </summary>
    public const int PtzRightDown = 26;

    /// <summary>
    ///     云台向左上
    /// </summary>
    public const int PtzLeftUp = 27;

    /// <summary>
    ///     云台向左下
    /// </summary>
    public const int PtzLeftDown = 28;

    /*播放控制*/
    /// <summary>
    ///     播放，以iPlayData作为播放参数(0-保留当前设置,1-回复默认)
    /// </summary>
    public const int PlayControlPlay = 0;

    /// <summary>
    ///     停止
    /// </summary>
    public const int PlayControlStop = 1;

    /// <summary>
    ///     暂停,注意停止直接调用相关关闭函数即可
    /// </summary>
    public const int PlayControlPause = 2;

    /// <summary>
    ///     快放，以iSpeed作为速度
    /// </summary>
    public const int PlayControlFast = 3;

    /// <summary>
    ///     慢放，以iSpeed作为速度
    /// </summary>
    public const int PlayControlSlow = 4;

    /// <summary>
    ///     seek，以iCurrentPosition作为位置
    /// </summary>
    public const int PlayControlSeekPos = 5;

    /// <summary>
    ///     seek，以dwCurrentTime作为时间
    /// </summary>
    public const int PlayControlSeekTime = 6;

    /// <summary>
    ///     sfp，单帧播放
    /// </summary>
    public const int PlayControlSfp = 7;

    /// <summary>
    ///     切换文件，以szFileName作为文件名/或strucTime时间
    /// </summary>
    public const int PlayControlSwitch = 8;

    /// <summary>
    ///     音频开关，以iEnableAudio作为开关
    /// </summary>
    public const int PlayControlMute = 9;

    /// <summary>
    ///     倒放
    /// </summary>
    public const int PlayControlUpend = 10;

    /// <summary>
    ///     得到本地文件的索引
    /// </summary>
    public const int PlayControlGetAvIndex = 11;

    /// <summary>
    ///     设置播放文件的索引
    /// </summary>
    public const int PlayControlSetAvIndex = 12;

    /// <summary>
    ///     设置是否自动调节缓冲时间
    /// </summary>
    public const int PlayControlAutoResetBufTime = 13;

    /// <summary>
    ///     seek，以strucTime作为时间，绝对时间
    /// </summary>
    public const int PlayControlSeekTimeNew = 14;

    /// <summary>
    ///     修复文件索引
    /// </summary>
    public const int PlayControlRepairFile = 15;

    /************************************************************YOLOV3相关定义****************************************************************/
    /*YOLOV3定义*/
    internal static string[][] GYolov3Class80 =
    [
        ["person"],
        ["bicycle"],
        ["car"],
        ["motorbike"],
        ["aeroplane"],
        ["bus"],
        ["train"],
        ["truck"],
        ["boat"],
        ["traffic light"],
        ["fire hydrant"],
        ["stop sign"],
        ["parking meter"],
        ["bench"],
        ["bird"],
        ["cat"],
        ["dog"],
        ["horse"],
        ["sheep"],
        ["cow"],
        ["elephant"],
        ["bear"],
        ["zebra"],
        ["giraffe"],
        ["backpack"],
        ["umbrella"],
        ["handbag"],
        ["tie"],
        ["suitcase"],
        ["frisbee"],
        ["skis"],
        ["snowboard"],
        ["sports ball"],
        ["kite"],
        ["baseball bat"],
        ["baseball glove"],
        ["skateboard"],
        ["surfboard"],
        ["tennis racket"],
        ["bottle"],
        ["wine glass"],
        ["cup"],
        ["fork"],
        ["knife"],
        ["spoon"],
        ["bowl"],
        ["banana"],
        ["apple"],
        ["sandwich"],
        ["orange"],
        ["broccoli"],
        ["carrot"],
        ["hot dog"],
        ["pizza"],
        ["donut"],
        ["cake"],
        ["chair"],
        ["sofa"],
        ["pottedplant"],
        ["bed"],
        ["diningtable"],
        ["toilet"],
        ["tvmonitor"],
        ["laptop"],
        ["mouse"],
        ["remote"],
        ["keyboard"],
        ["cell phone"],
        ["microwave"],
        ["oven"],
        ["toaster"],
        ["sink"],
        ["refrigerator"],
        ["book"],
        ["clock"],
        ["vase"],
        ["scissors"],
        ["teddy bear"],
        ["hair drier"],
        ["toothbrush"]
    ];

    internal static string[][] GUavClass3 =
    [
        ["uav"],
        ["airplane"],
        ["fixwing"]
    ];

    internal static string[][] GMobilenetYolov3 =
    [
        ["background"],
        ["aeroplane"],
        ["bicycle"],
        ["bird"],
        ["boat"],
        ["bottle"],
        ["bus"],
        ["car"],
        ["cat"],
        ["chair"],
        ["cow"],
        ["diningtable"],
        ["dog"],
        ["horse"],
        ["motorbike"],
        ["person"],
        ["pottedplant"],
        ["sheep"],
        ["sofa"],
        ["train"],
        ["tvmonitor"]
    ];

    internal static string[][] GYolov4Class20 =
    [
        ["airplane"],
        ["uav"],
        ["fixwing"],
        ["smoke"],
        ["fire"],
        ["person"],
        ["bicycle"],
        ["motorbike"],
        ["car"],
        ["truck"],
        ["bus"],
        ["train"],
        ["ship"],
        ["kite"],
        ["bird"],
        ["animal"],
        ["surfboard"],
        ["tvmonitor"],
        ["laptop"],
        ["chair"]
    ];

    static GlobalMembers()
    {
        Utils.ResolveDllImport(Assembly.GetExecutingAssembly(), "Nvt", [LibPath]);
    }

    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr TMCC_Init(uint flag);

    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SetAutoReConnect(IntPtr ptr, bool bShow);

    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SetDisplayShow(IntPtr ptr, bool bShow);

    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SetStreamBufferTime(IntPtr ptr, uint dwTime);

    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_Connect(IntPtr ptr, ref TmConnectInfoT connectInfo, bool bSync);

    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_GetConfig(IntPtr ptr, ref TmCommandInfoT cmdInfo);

    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SetConfig(IntPtr ptr, ref TmCommandInfoT cmdInfo);

    /*ptz相关*/
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_PtzOpen(IntPtr ptr, int iChannel, bool block = false);

    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_PtzLock(IntPtr ptr, int iChannel);

    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_PtzControl(IntPtr ptr, uint dwPtzCmd, uint dwControl, uint dwSpeed);

    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_PtzClose(IntPtr ptr);

    /*PTZ 控制相关结束*/
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SaveConfig(IntPtr ptr);

    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_ConnectStream(IntPtr ptr, ref TmPlayRealStreamCfgT info, IntPtr playHandler);

    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_CloseStream(IntPtr ptr);

    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_ClearDisplay(IntPtr ptr);

    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_DisConnect(IntPtr flag);

    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_Done(IntPtr flag);

    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_CapturePictureToFile(IntPtr hTmCc,
        string pFileName, string pFmt);

    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SetOtherParam(IntPtr hTmCc, uint dwFlags, IntPtr buf, ref int iLen);

    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_RegisterConnectCallBack(IntPtr hTmCc, ConnectCallback pCallBack, IntPtr context);

    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_RegisterStreamCallBack(IntPtr ptr, StreamCallback back, IntPtr context);

    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_RegisterDataReadCallBack(IntPtr ptr, DeviceDataCallback back, IntPtr context);

    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_RegisterAVFrameCallBack(IntPtr ptr, AvFrameCallback back, IntPtr context);

    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SetImageOutFmt(IntPtr ptr, uint iOutFmt);

    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_GetLastError();

    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void TMCC_RegisterDrawCallBack(IntPtr ptr, TmccDrawCallback back, IntPtr context);

    /*播放文件*/
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr TMCC_OpenFile(IntPtr hTmCc, ref TmPlayConditionCfgT pPlayInfo, IntPtr hPlayWnd);

    /*播放文件*/
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr TMCC_ControlFile(IntPtr hTmCc, ref TmPlayControlCfgT pPlayInfo);

    /*开始录像*/
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal static extern IntPtr TMCC_StartRecord(IntPtr hStream, string strFileName,
        string strFileType);

    /*停止录像*/
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr TMCC_StopRecord(IntPtr hStream);

    /*
     *函  数: TMCC_GetVersion
     *说  明: 获得本客户端库版本信息
     *参  数: pBuild为存放编译序号
     *返回值: 返回版本号，格式如<返回值为10000，则版本V1.0.0.00>
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint TMCC_GetVersion(ref uint pBuild);

    /*
     *函  数: TMCC_RegisterLogCallBack
     *说  明: 注册调试信息回调函数，只有在开发模式下才有效
     *参  数: pCallBack为回调函数指针，context为调用者自定义数据
     *返回值: 无
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void TMCC_RegisterLogCallBack(TmccLogCallback pCallBack, object context);

    /*
     *函  数: TMCC_RegisterServerInfoCallBack
     *说  明: 注册服务器消息信息回调函数，通过此回调可以得到服务器对管理中心的连接以及报警信息
     *参  数: pCallBack为回调函数指针，context为调用者自定义数据
     *返回值: 无
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void TMCC_RegisterServerInfoCallBack(TmccServerinfoCallback pCallBack, object context);

    /*
     *函  数: TMCC_RegisterConnectCallBack
     *说  明: 注册连接信息返回回调函数，异步连接成功通过该回调函数返回，发生错误断开连接也通过该回调
     *参  数: hTmCc为服务器控制句柄，pCallBack为回调函数指针，context为自定义数据
     *返回值: 无
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void TMCC_RegisterConnectCallBack(IntPtr hTmCc, TmccConnectCallback pCallBack,
        object context);

    /*
     *函  数: TMCC_RegisterDataReadCallBack
     *说  明: 注册服务器消息读取回调函数，异步方式通过它获得服务器消息，其它如报警消息也通过它得到
     *参  数: hTmCc为TMCC_Init返回的服务器控制句柄，pCallBack为回调函数指针，context为自定义数据
     *返回值: 无
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void TMCC_RegisterDataReadCallBack(IntPtr hTmCc, TmccDatareadCallback pCallBack,
        object context);

    /*
     *函  数: TMCC_RegisterProgressCallBack
     *说  明: 注册升级备份信息回调函数，只有在开发升级模式下才有效
     *参  数: pCallBack为回调函数指针，context为调用者自定义数据
     *返回值: 无
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void TMCC_RegisterProgressCallBack(IntPtr hTmCc, TmccProgressCallback pCallBack,
        object context);

    /*
     *函  数: TMCC_SetTimeOut
     *说  明: 设置与服务器通讯操作的超时时间，必须要在TMCC_Connect调用前设置才有效
     *参  数: hTmCc为TMCC_Init返回的服务器控制句柄，dwTime时间值单位为毫秒系统默认为2000毫秒
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SetTimeOut(IntPtr hTmCc, int dwTime);

    /*
     *函  数: TMCC_GetTimeOut
     *说  明: 获得与服务器通讯操作的超时时间，任何时候都能调用
     *参  数: hTmCc为TMCC_Init返回的服务器控制句柄
     *返回值: 成功返回时间值，错误返回0
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_GetTimeOut(IntPtr hTmCc);

    /*
     *函  数: TMCC_GetAutoReConnect
     *说  明: 获得发生错误后是否自动重新连接标志，任何时候都能调用
     *参  数: hTmCc为TMCC_Init返回的服务器控制句柄
     *返回值: 成功返回自动连接标志，错误返回FALSE
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern bool TMCC_GetAutoReConnect(IntPtr hTmCc);

    /*
     *函  数: TMCC_IsConnect
     *说  明: 获得当前是否正常连接服务器，任何时候都能调用
     *参  数: hTmCc为TMCC_Init返回的服务器控制句柄
     *返回值: 成功返回连接正常标志，错误返回FALSE
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern bool TMCC_IsConnect(IntPtr hTmCc);

    /*
     *函  数: TMCC_Connect
     *说  明: 连接服务器，即登录到服务器，调用该函数成功后必须要调用TMCC_DisConnect才能再一次调用，
              如果调用一异步方式则连接成功会通过连接回调函数反映，如没注册连接回调函数，异步连接会失败
     *参  数: hTmCc为TMCC_Init返回的服务器控制句柄，pConnectInfo为用户连接信息，bSync为连接方式<异步或同步>
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_Connect(IntPtr hTmCc, TmConnectInfoT pConnectInfo, bool bSync);

    /*
     *函  数: TMCC_SetConfig
     *说  明: 服务器参数配置，该函数所有配置的参数在服务器重新启后会丢失，除非调用TMCC_SaveConfig明确保存
     *参  数: hTmCc为TMCC_Init返回的服务器控制句柄；
               pCommandInfo命令消息结构指针，其中包含了所需要的命令，缓冲等
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SetConfig(IntPtr hTmCc, TmCommandInfoT pCommandInfo);

    /*
     *函  数: TMCC_GetConfig
     *说  明: 获取服务器参数
     *参  数: hTmCc为TMCC_Init返回的服务器控制句柄；
               pCommandInfo命令消息结构指针，其中包含了所需要的命令，缓冲等
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_GetConfig(IntPtr hTmCc, TmCommandInfoT pCommandInfo);

    /*
     *函  数: TMCC_RestoreConfig
     *说  明: 恢复服务器配置参数为系统默认值
     *参  数: hTmCc为TMCC_Init返回的服务器控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_RestoreConfig(IntPtr hTmCc);

    /*
     *函  数: TMCC_Reboot
     *说  明: 重新启动服务器，启动后客户端会自动连接它，重启期间所有设置均无效
     *参  数: hTmCc为TMCC_Init返回的服务器控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_Reboot(IntPtr hTmCc);

    /*
     *函  数: TMCC_ShutDown
     *说  明: 关闭服务器，关闭后与服务器的连接自动断开
     *参  数: hTmCc为TMCC_Init返回的服务器控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_ShutDown(IntPtr hTmCc);

    /*
     *函  数: TMCC_UpgradeSystem
     *说  明: 升级服务器系统，升级系统也要操作Flash，所以建议不要经常升级系统，
               该函数为阻塞调用，操作期间其它所有操作无效
     *参  数: hTmCc为TMCC_Init返回的服务器控制句柄，lpszFileName为系统镜像全路径名
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_UpgradeSystem(IntPtr hTmCc, string lpszFileName);

    /*
     *函  数: TMCC_BackupSystem
     *说  明: 保存服务器系统为镜像文件，该函数为阻塞调用，操作期间其它所有操作无效
     *参  数: hTmCc为TMCC_Init返回的服务器控制句柄，lpszFileName为系统镜像全路径名；
     *		  iModal为要备份的模块标志，分别以位表示，从最低为开始 :
              0位表示备份运行模块，1位表示备份升级的运行模块, 2位表示备份参数模块，3位表示备份PTZ模块，
              4位表示备份WEB模块，5位表示备份扩展模块
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_BackupSystem(IntPtr hTmCc, string lpszFileName,
        int iModal = 0x3E);

    /*
     *函  数: TMCC_EnumServer
     *说  明: 列举网络中的服务器，列举出来的设备仅能作为参考，可能有些不能列举到
     *参  数: pCallBack为回调函数指针；context为调用者设置的上下文指针
     *		  在函数pCallBack中返回TRUE表示继续列举，FALSE为停止列举
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_EnumServer(TmccEnumserverCallback pCallBack, object context,
        bool bRegisterCallBack = false);

    /*
     *函  数: TMCC_RefreshEnumServer
     *说  明: 刷新网络服务器列表
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_RefreshEnumServer();

    /*
     *函  数: TMCC_ConfigServer
     *说  明: 配置服务器的地址信息
     *参  数: dwFlags为配置类型，即通过什么方式配置，目前仅支持网络
     *		  pConfig为新的服务器信息
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_ConfigServer(TmServerCfgT pConfig, uint dwFlags = 0);

    //云台控制
    /*
     *函  数: TMCC_PtzGetDecoderList
     *说  明: 获得服务器内置解码器协议列表
     *参  数: pInfo为要填协议的列表，info_num为输入为列表最大个数，输出为当前协议个数
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_PtzGetDecoderList(IntPtr hTmCc, TmDecoderInfoT[] pInfo, ref int infoNum);

    /*
     *函  数: TMCC_PtzUnLock
     *说  明: 解锁云台控制
     *参  数: hTmCc为控制句柄, iChannel为云台号
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_PtzUnLock(IntPtr hTmCc, int iChannel);

    /*
     *函  数: TMCC_PtzTrans
     *说  明: 为云台控制直接输出，不用内置解码协议转换
     *参  数: hTmCc为控制句柄，pPTZCodeBuf为输出的数据缓冲，iBufSize为数据大小，最大值必须小于128
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_PtzTrans(IntPtr hTmCc, ref byte pPtzCodeBuf, int iBufSize);

    /*
     *函  数: TMCC_PtzPreset
     *说  明: 云台预置点调用
     *参  数: hTmCc为控制句柄，dwPTZPresetCmd为云台控制命令号，dwPresetIndex为预置点号，dwSpeed为速度
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_PtzPreset(IntPtr hTmCc, uint dwPtzPresetCmd, uint dwPresetIndex, uint dwSpeed);

    /*
     *函  数: TMCC_PtzIntegrate
     *说  明: 为云台综合控制，需要云台直接和一体机支持
     *参  数: hTmCc为控制句柄，pParam为控制的具体参数结构指针，iParamSize为结构大小大小，最大值必须小于128
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_PtzIntegrate(IntPtr hTmCc, TmPtzParameterT pParam, int iParamSize);

    //透明通道设置
    /*
     *函  数: TMCC_RegisterSerialDataReadCallBack
     *说  明: 注册服务器串口数据获得回调函数
     *参  数: hTmCc为控制句柄, , pCallBack为获取通道的回传数据，context为用户自定义数据
     *返回值: 无
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void TMCC_RegisterSerialDataReadCallBack(IntPtr hTmCc, TmccSerialdataCallback pCallBack,
        object context);

    /*
     *函  数: TMCC_SerialOpen
     *说  明: 打开与服务器间的透明通道传输
     *参  数: hTmCc为控制句柄, iSerialPort==0时打开232,==1打开485
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SerialOpen(IntPtr hTmCc, int iSerialPort);

    /*
     *函  数: TMCC_SerialSend
     *说  明: 向服务器发送通明数据，这些数据服务器将不作处理直接送出
     *参  数: hTmCc为控制句柄，iChannel为发送的通道号仅485有效
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SerialSend(IntPtr hTmCc, int iChannel,
        ref string pSendBuf, int iBufSize);

    /*
     *函  数: TMCC_SerialRecv
     *说  明: 从服务器读取通明数据，这些数据服务器将不作处理直接送出
     *参  数: hTmCc为控制句柄，iChannel为发送的通道号仅485有效
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SerialRecv(IntPtr hTmCc, int iChannel,
        ref string pRecvBuf, int iBufSize);

    /*
     *函  数: TMCC_SerialClose
     *说  明: 关闭与服务器的通明传输
     *参  数: hTmCc为控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SerialClose(IntPtr hTmCc);

    ///////////////////////////////////////////////////////////////////////
    ///////////////////////////////语音对讲////////////////////////////////
    ///////////////////////////////////////////////////////////////////////
    /*
     *函  数: TMCC_StartVoiceCom
     *说  明: 启动语音对讲
     *参  数: hTmCc为控制句柄，pConnectInfo为语音对象可以为NULL
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_StartVoiceCom(IntPtr hTmCc, TmConnectInfoT pConnectInfo);

    /*
     *函  数: TMCC_SetVoiceComClientVolume
     *说  明: 设置客户端语音播放硬件声音大小
     *参  数: hTmCc为控制句柄，iVolume为声音值-10000~0,-10000最小,0最大
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SetVoiceComClientVolume(IntPtr hTmCc, int iVolume);

    /*
     *函  数: TMCC_SetVoiceComClientVolumeZoom
     *说  明: 设置客户端语音播放软件放大值
     *参  数: hTmCc为控制句柄，fpmPerNum为声音放大值0 < fpmPerNum < 10.0
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SetVoiceComClientVolumeZoom(IntPtr hTmCc, float fpmPerNum);

    /*
     *函  数: TMCC_SetVoiceComClientMicZoom
     *说  明: 设置客户端语音采集，即麦克风声音大小
     *参  数: hTmCc为控制句柄，fpmPerNum为声音放大值0 < fpmPerNum < 10.0
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SetVoiceComClientMicZoom(IntPtr hTmCc, float fpmPerNum);

    /*
     *函  数: TMCC_StopVoiceCom
     *说  明: 关闭与服务器的语音队讲
     *参  数: hTmCc为控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_StopVoiceCom(IntPtr hTmCc);

    /*
     *函  数: TMCC_RegisterVoiceDataCallBack
     *说  明: 注册语音对讲数据获得回调，通过此回调可以得到服务器的语音数据
     *参  数: pCallBack为回调函数指针，context为调用者自定义数据
     *返回值: 无
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void TMCC_RegisterVoiceDataCallBack(IntPtr hTmCc, TmccVoicedataCallback pCallBack,
        object context);

    /*
     *函  数: TMCC_PutInVoiceData
     *说  明: 输入语音对讲数据
     *参  数: hTmCc为控制句柄，pVoiceData为音频数据结构
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_PutInVoiceData(IntPtr hTmCc, TmVoiceDataT pVoiceData);

    /*
     *函  数: TMCC_OpenVoice
     *说  明: 打开语音对讲数据解码和语音捕获
     *参  数: hTmCc为控制句柄，pVoiceHead为音频数据头结构
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_OpenVoice(IntPtr hTmCc, TmVoiceHeadT pVoiceInHead, TmVoiceHeadT pVoiceOutHead);

    /*
     *函  数: TMCC_CloseVoice
     *说  明: 关闭语音对讲数据解码和语音捕获
     *参  数: hTmCc为控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_CloseVoice(IntPtr hTmCc);

    //服务器状态
    /*
     *函  数: TMCC_GetServerWorkState
     *说  明: 得到服务器当前工作状态
     *参  数: hTmCc为控制句柄，lpWorkStatew为存放服务器状态信息的结构指针
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_GetServerWorkState(IntPtr hTmCc, TmWorkStateT lpWorkState);

    /*
     *函  数: TMCC_ConfigServerEx
     *说  明: 配置服务器的地址信息，1.0.0.09添加
     *参  数: dwFlags为配置类型，即通过什么方式配置，目前仅支持网络
     *		  pConfig为新的服务器信息
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_ConfigServerEx(IntPtr hTmCc, TmServerExCfgT pConfig, uint dwFlags);

    //////////////////////////////////////////////////////////////////////////
    //2007/07/16 franxkia 新加
    //设置播放的wave文件
    /*
     *函  数: TMCC_SetPlayWaveFile
     *说  明: PC播放WAVE文件,通过语音对讲通道传送到DVS,DVS输出
     *参  数: hTmCc为控制句柄，pFileName为文件名全路径,bPlayNum为要播放的次数
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SetPlayWaveFile(IntPtr hTmCc,
        ref string pFileName, byte bCirclePlay);

    /*
     *函  数: TMCC_SaveDefaultConfig
     *说  明: 保存服务器默认配置参数，服务器中Flash的察写次数一般有限，建议不要经常调用
     *参  数: hTmCc为TMCC_Init返回的服务器控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SaveDefaultConfig(IntPtr hTmCc);

    //////////////////////////////////////////////////////////////////////////////
    ///////////视频解码器直接操作函数接口定义 add by stone 20070930///////////////
    //////////////////////////////////////////////////////////////////////////////
    /*
     *函  数: TMCC_SetConnectInfo
     *说  明: 配置服务器的连接信息，1.0.0.20添加，如果是循环连接则加入，如果是非循环那就处理
     *参  数: pConfig为连接信息, iChannel为通道号(或窗口号)，size为pConfig的缓冲大小
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SetConnectInfo(IntPtr hTmCc, int iChannel, TmConnectCfgT pConfig, int size);

    /*
     *函  数: TMCC_GetConnectInfo
     *说  明: 得到当前的连接信息，1.0.0.20添加
     *参  数: pConfig为连接信息, iChannel为通道号(或窗口号)，size为pConfig的缓冲大小
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_GetConnectInfo(IntPtr hTmCc, int iChannel, TmConnectCfgT pConfig, int size);

    /*
     *函  数: TMCC_ClearConnectInfo
     *说  明: 清除当前的连接信息，如果是循环连接则清除指定循环序号，1.0.0.20添加
     *参  数: iChannel为通道号(或窗口号)，iIndex为为循环的序号为-1则为清除所有
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_ClearConnectInfo(IntPtr hTmCc, int iChannel, int iIndex);

    /*
     *函  数: TMCC_GetEnumConnectInfo
     *说  明: 枚举服务器的连接信息，通过回调输出，回调中可以反映通道号，是否是循环连接等信息，1.0.0.20添加
     *参  数: iChannel为通道号(或窗口号)，pCallBack为回调函数指针，context为用户定义的指针
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_EnumConnectInfo(IntPtr hTmCc, int iChannel, TmccEnumconnectCallback pCallBack,
        object context);

    /*
     *函  数: TMCC_SetWindowInfo
     *说  明: 设置显示的窗口信息，1.0.0.20添加
     *参  数: iChannel为通道号(或窗口号)，pConfig为窗口信息指针
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SetWindowInfo(IntPtr hTmCc, int iChannel, TmWindowsCfgT pConfig, int size);

    /*
     *函  数: TMCC_GetWindowInfo
     *说  明: 读取显示的窗口信息，1.0.0.20添加
     *参  数: iChannel为通道号(或窗口号)，pConfig为窗口信息指针
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_GetWindowInfo(IntPtr hTmCc, int iChannel, TmWindowsCfgT pConfig, int size);

    /*
     *函  数: TMCC_SetDisplayInfo
     *说  明: 设置解码器的显示信息，如单屏放大等,1.0.0.20添加
     *参  数: pConfig为显示信息指针，size为数据缓冲大小
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SetDisplayInfo(IntPtr hTmCc, TmDisplayCfgT pConfig, int size);

    /*
     *函  数: TMCC_GetDisplayInfo
     *说  明: 读取解码器的显示信息，如单屏放大等,1.0.0.20添加
     *参  数: pConfig为显示信息指针，size为数据缓冲大小
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_GetDisplayInfo(IntPtr hTmCc, TmDisplayCfgT pConfig, int size);

    /*
     *函  数: TMCC_SetLockInfo
     *说  明: 设置通道连接锁定信息，如单是否循环切换等, 注意循切的时间是在设置连接信息时配置,1.0.0.20添加
     *参  数: iChannel为通道号,pConfig为显示信息指针，size为数据缓冲大小
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SetLockInfo(IntPtr hTmCc, int iChannel, TmLockCfgT pConfig, int size);

    /*
     *函  数: TMCC_GetLockInfo
     *说  明: 读取通道连接锁定信息，如单是否循环切换等, 注意循切的时间是在设置连接信息时配置,1.0.0.20添加
     *参  数: iChannel为通道号,pConfig为显示信息指针，size为数据缓冲大小
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_GetLockInfo(IntPtr hTmCc, int iChannel, TmLockCfgT pConfig, int size);

    /*
     *函  数: TMCC_StartConnect
     *说  明: 使解码器打开连接,1.0.0.20添加
     *参  数: iChannel为通道号
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_StartConnect(IntPtr hTmCc, int iChannel);

    /*
     *函  数: TMCC_StopConnect
     *说  明: 使解码器关闭连接,1.0.0.20添加
     *参  数: iChannel为通道号
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_StopConnect(IntPtr hTmCc, int iChannel);

    /*
     *函  数: TMCC_SetSerialNumber
     *说  明: 设置连接的唯一厂商信息
     *参  数: hTmCc控制句柄, szSerialNumber为序列号
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SetSerialNumber(IntPtr hTmCc,
        string szSerialNumber);

    /*
     *函  数: TMCC_SetOtherParam
     *说  明: 设置一些扩展信息，已废弃
     *参  数: hTmCc为控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SetOtherParam(IntPtr hTmCc, uint dwFlags, object buf, ref int iLen);

    /*
     *函  数: TMCC_UpgradeWebPage
     *说  明: 升级服务器中的网页，网页可由用户自行设计，空间大小限制5M内
     *参  数: hTmCc为控制句柄, lpszPathName存放网页的目录
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_UpgradeWebPage(IntPtr hTmCc, string lpszPathName);

    /*
     *函  数: TMCC_UpgradeWebOcx
     *说  明: 升级服务器中的网页，网页可由用户自行设计，空间大小限制5M内
     *参  数: hTmCc为控制句柄, lpszNameOCX的全路劲名
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_UpgradeWebOcx(IntPtr hTmCc, string lpszName);

    /*
     *函  数: TMCC_UpgradeKernel
     *说  明: 升级服务器的Firmware
     *参  数: hTmCc为控制句柄, lpszName为Firmware名称
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_UpgradeKernel(IntPtr hTmCc, string lpszName);

    /*
     *函  数: TMCC_UpgradeTmccModule
     *说  明: 升级服务器的Tmcc扩展模块
     *参  数: hTmCc为控制句柄, lpszName为扩展模块目录名称
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_UpgradeTmccModule(IntPtr hTmCc, string lpszName);

    //////////////////////////////////////////////////////////////
    ///////////////////设备自动升级模块///////////////////////////
    //////////////////////////////////////////////////////////////
    /*
     *函  数: TMCC_ConfigUpgradeServer
     *说  明: 配置自动升级服务器
     *参  数: hTmCc为升级服务器控制句柄
     *返回值: 成功返回升级服务器控制句柄，失败返回NULL
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_ConfigUpgradeServer(IntPtr hTmCc, TmUpgradeConfigT pConfig);

    /*
     *函  数: TMCC_RegisterUpgradeCallBack
     *说  明: 注册升级服务器信息输出回调函数
     *参  数: hTmCc为升级服务器控制句柄
     *返回值: 成功返回升级服务器控制句柄，失败返回NULL
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void TMCC_RegisterUpgradeCallBack(IntPtr hTmCc, TmccUpgradeCallback pCallBack,
        object context);

    /*
     *函  数: TMCC_StartUpgrade
     *说  明: 启动升级服务器，开始升级
     *参  数: hTmCc为升级服务器控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_StartUpgrade(IntPtr hTmCc);

    /*
     *函  数: TMCC_StopUpgrade
     *说  明: 停止升级服务器
     *参  数: hTmCc为升级服务器控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_StopUpgrade(IntPtr hTmCc);

    /*
     *函  数: TMCC_SendUpgradeMessage
     *说  明: 向网络上发送升级消息
     *参  数: hTmCc为升级服务器控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SendUpgradeMessage(IntPtr hTmCc);

    //////////////////////////////////////////////////////////////
    ///////////////////设备自动服务器搜索/////////////////////////
    //////////////////////////////////////////////////////////////
    /*
     *函  数: TMCC_RegisterEnumServerCallBack
     *说  明: 注册枚举服务器信息回调函数
     *参  数: pCallBack为回调函数指针，context为调用者自定义数据
     *返回值: 无
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void TMCC_RegisterEnumServerCallBack(IntPtr hEnum, TmccEnumserverCallback pCallBack,
        object context);

    /*
     *函  数: TMCC_StartEnum
     *说  明: 启动搜索
     *参  数: hEnum为回枚举控制句柄，由TMCC_CreateEnumServer初始化返回句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_StartEnum(IntPtr hEnum);

    /*
     *函  数: TMCC_StopListen
     *说  明: 关闭枚举监听
     *参  数: hEnum为回枚举控制句柄，由TMCC_CreateEnumServer初始化返回句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_StopEnum(IntPtr hEnum);

    /*
     *函  数: TMCC_EnumServer
     *说  明: 列举网络中的服务器
     *参  数: hEnum为回枚举控制句柄，由TMCC_CreateEnumServer初始化返回句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_RefreshEnum(IntPtr hEnum);

    /*
     *函  数: TMCC_ConfigServerBuEnum
     *说  明: UDP方式配置服务器
     *参  数: hEnum为回枚举控制句柄，由TMCC_CreateEnumServer初始化返回句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_ConfigServerByMulti(IntPtr hEnum, TmMultiServerCfgT pConfig);

    ////////////////////////////////////////////////////////
    ///////////////////实时数据播放/////////////////////////
    ////////////////////////////////////////////////////////
    /*
     *函  数: TMCC_RegisterStreamCallBack
     *说  明: 注册实时数据输出回调
     *参  数: hClient为文件服务器句柄,pCallBack为回调函数指针,context为用户指针式
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void TMCC_RegisterStreamCallBack(IntPtr hClient, TmccStreamreadCallback pCallBack,
        object context);

    /*
     *函  数: TMCC_RegisterRtpStreamCallBack
     *说  明: 注册实时RTP数据输出回调
     *参  数: hClient为文件服务器句柄,pCallBack为回调函数指针,context为用户指针式
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void TMCC_RegisterRtpStreamCallBack(IntPtr hClient, TmccRtpstreamreadCallback pCallBack,
        object context, bool bTcpPacket);

    /*
     *函  数: TMCC_RegisterConnectMessage
     *说  明: 注册连接状态输出消息
     *参  数: hClient为服务器句柄,hWnd用户接收消息窗口,msgID消息ID
     *返回值: 无
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void TMCC_RegisterConnectMessage(IntPtr hClient, IntPtr hWnd, uint msgId);

    /*
     *函  数: TMCC_ConnectStream
     *说  明: 以指定条件打开服务器实时流
     *参  数: hClient为播放句柄,pPlayInfo为打开文件的条件
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_ConnectStream(IntPtr hClient, TmPlayRealStreamCfgT pPlayInfo, IntPtr hPlayWnd);

    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_ConnectStreamEx(IntPtr hClient, TmPlayRealStreamCfgT pPlayInfo, IntPtr hPlayWnd,
        int x, int y, int w, int h);

    /*
     *函  数: TMCC_MakeKeyFrame
     *说  明: 强制下一帧为关键帧，当句柄是连接视频的那么通道信息用连接中的信息
     *参  数: hClient为播放句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_MakeKeyFrame(IntPtr hClient);

    /*
     *函  数: TMCC_RefreshRealStreamBuffer
     *说  明: 刷新当前数据缓冲
     *参  数: hClient为播放句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_RefreshStreamBuffer(IntPtr hClient);

    /*
     *函  数: TMCC_SwitchRealStream
     *说  明: 切换数据流连接的码流
     *参  数: hClient为播放句柄,iStreamId为码流号
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SwitchRealStream(IntPtr hClient, int iStreamId);

    ////////////////////////////////////////////////////////
    /////////////////////数据流播放/////////////////////////
    ////////////////////////////////////////////////////////
    /*
     *函  数: TMCC_OpenStream
     *说  明: 以指定条件打开服务器文件
     *参  数: hTmFile为文件服务器句柄,pHeadBuf为打开文件头或者tmStreamHeadInfo_t头,hPlayWnd为显示视频的窗口
     *		  iPushBufferSize为缓冲大小，根据编码格式决定，一般1024*1024
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_OpenStream(IntPtr hStream, ref byte pHeadBuf, int iHeadSize, IntPtr hPlayWnd,
        int iPushBufferSize);

    /*
     *函  数: TMCC_ResetStream
     *说  明: 以指定条件复位数据流
     *参  数: hStream数据流控制句柄,pHeadBuf为数据流头缓冲，iHeadSize头缓冲大小
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_ResetStream(IntPtr hStream, ref byte pHeadBuf, int iHeadSize);

    /*
     * 函  数: TMCC_GetFileHead
     * 说  明: 读取文件的头，可以直接输入OpenStream
     * 参  数: pHeadBuf为数据流头存放缓冲,iBufSize缓冲长度
     * 返回值: 成功返回文件头长度，失败返回<=0
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_GetFileHead(string lpszFileName,
        ref byte pHeadBuf, int iBufSize);

    /*
     * 函  数: TMCC_CreateFileHead
     * 说  明: 读取文件的头，可以直接输入OpenStream
     * 参  数: pStreamHead为文件信息音视频信息，pHeadBuf为数据流头存放缓冲,iBufSize缓冲长度
     * 返回值: 成功返回文件头长度，失败返回<=0
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_CreateFileHead(TmStreamHeadInfoT pStreamHead, ref byte pHeadBuf, int iBufSize);

    /*
     * 函  数: TMCC_CreateStreamHead
     * 说  明: 转换文件头到数据流头
     * 参  数: pStreamHead为文件信息音视频信息，pHeadBuf为数据流头存放缓冲,iBufSize缓冲长度
     * 返回值: 成功返回文件头长度，失败返回<=0
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_CreateStreamHead(ref byte pHeadBuf, int iBufSize, TmStreamHeadInfoT pStreamHead);

    /*
     * 函  数: TMCC_GetFileIndex
     * 说  明: 读取文件的索引，可以直接输入文件播放，此函数可能很费时
     * 参  数: pHeadBuf为索引存放缓冲,iBufSize缓冲长度
     * 返回值: 成功返回文件索引长度，失败返回<=0
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_GetFileIndex(string lpszFileName,
        ref byte pHeadBuf, int iBufSize);

    /*
     *函  数: TMCC_PutInStream
     *说  明: 输入数据流播放
     *参  数: hStream为数据流播放句柄句柄,pStreamBuf为数据流缓冲,iStreamSize为数据流大小
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_PutInStream(IntPtr hStream, ref byte pStreamBuf, int iStreamSize, uint nData);

    ////////////////////////////////////////////////////////
    ///////////////////远程文件搜索/////////////////////////
    ////////////////////////////////////////////////////////
    /*
     *函  数: TMCC_FindFirst
     *说  明: 开始主动方式搜索服务器文件
     *参  数: hTmFile为文件服务器句柄，pFindCondition为搜索条件，lpFindFileData为输出文件信息
     *返回值: 成功返回文件搜索句柄，失败返回NULL
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr TMCC_FindFirstFile(IntPtr hTmCc, TmFindConditionCfgT pFindCondition,
        TmFindFileCfgT lpFindFileData);

    /*
     *函  数: TMCC_FindNextFile
     *说  明: 搜索下一个文件
     *参  数: hTmFile为文件服务器句柄，lpFindFileData为输出文件信息
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_FindNextFile(IntPtr hTmFile, TmFindFileCfgT lpFindFileData);

    /*
     *函  数: TMCC_FindCloseFile
     *说  明: 关闭搜索
     *参  数: hTmFile为文件服务器句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_FindCloseFile(IntPtr hTmFile);

    /*
     *函  数: TMCC_DownloadFile
     *说  明: 下载服务器文件
     *参  数: hTmFile为文件服务器句柄, pPlayInfo为下载条件,lpSaveFileName为保存的本地文件,bCancel下载中停止指针
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_DownloadFile(IntPtr hTmCc, TmPlayConditionCfgT pPlayInfo,
        string lpSaveFileName,
        ref bool bCancel, TmccDownloadprogressCallback pCallBack, object context);

    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_DownloadFileA(IntPtr hTmCc, TmDownloadFileCfgT pDownloadInfo, int iDownloadNum,
        ref bool bCancel, TmccDownloadprogressCallback pCallBack, object context);

    ////////////////////////////////////////////////////////
    //////////////////////文件播放//////////////////////////
    ////////////////////////////////////////////////////////
    /*
     *函  数: TMCC_OpenFile
     *说  明: 以指定条件打开服务器文件
     *参  数: hTmCc为文件服务器句柄,pPlayInfo为打开文件的条件,hPlayWnd为显示视频的窗口;
     *		  当hTmCc==NULL是为播放本地文件
     *返回值: 成功返回播放控制句柄,失败返回NULL
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr TMCC_OpenFile(IntPtr hTmCc, TmPlayConditionCfgT pPlayInfo, IntPtr hPlayWnd);

    /*
     *函  数: TMCC_OpenRemoteFile
     *说  明: 以指定条件打开服务器文件
     *参  数: hTmCc为文件服务器句柄,pPlayInfo为打开文件的条件,hPlayWnd为显示视频的窗口;
     *返回值: 成功返回播放控制句柄,失败返回NULL
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_OpenRemoteFile(IntPtr hTmCc, TmRemoteFileInfoT pPlayInfo, IntPtr hPlayWnd);

    /*
     *函  数: TMCC_CloseFile
     *说  明: 关闭打开的服务器文件
     *参  数: hTmFile为文件服务器句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_CloseFile(IntPtr hTmFile);

    /*
     *函  数: TMCC_GetFileInfo
     *说  明: 读取文件信息
     *参  数: hTmFile为文件服务器句柄,pPlayInfo为文件信息缓冲
     *返回值: 成功返回读取的大小，失败返回小于0
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_GetFileHeadInfo(IntPtr hTmFile, ref byte pHeadBuf, int iBufSize);

    /*
     *函  数: TMCC_ReadFile
     *说  明: 读取文件数据，可以任意读取，或读取完整一帧
     *参  数: hTmFile为文件服务器句柄,buf数据缓冲，iReadSize为读取大小
     *返回值: 成功返回读取的大小，失败返回小于0
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_ReadFile(IntPtr hTmFile, ref byte buf, int iReadSize);

    /*
     *函  数: TMCC_ControlFile
     *说  明: 控制文件播放
     *参  数: hTmFile为文件服务器句柄,pPlayControl为文件控制参数信息
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_ControlFile(IntPtr hTmFile, TmPlayControlCfgT pPlayControl);

    /*
     *函  数: TMCC_GetFilePlayState
     *说  明: 读取打开的文件位置
     *参  数: hTmFile为文件服务器句柄,tmTimeInfo_t为存放文件位置指针
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_GetFilePlayState(IntPtr hTmFile, TmPlayStateCfgT pState);

    /*
     *函  数: TMCC_RegisterFileCallBack
     *说  明: 注册文件访问回调函数
     *参  数: pCallBack为回调函数指针，context为调用者自定义数据
     *返回值: 无
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void TMCC_RegisterFileCallBack(IntPtr hTmFile, TmFileAccessInterfaceT pCallBack,
        object context);

    //////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////视频控制接口定义//////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////
    /*
     *函  数: TMCC_RegisterAVFrameCallBack
     *说  明: 注册解码视频输出回调
     *参  数: hTmCc为服务器句柄,pCallBack用户回调函数指针,context为用户自定义指针
     *返回值: 无
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void TMCC_RegisterAVFrameCallBack(IntPtr hTmCc, TmccAvframeCallback pCallBack,
        object context);

    /*
     *函  数: tmcc_registerDrawCallback
     *说  明: 注册解码视频自会输出回调
     *参  数: hTmCc为服务器句柄,pCallBack用户回调函数指针,context为用户自定义指针
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void TMCC_RegisterDrawCallBack(IntPtr hTmCc, TmccDrawCallback pCallBack, object context);

    /*
     *函  数: TMCC_SetVolume
     *说  明: 设置音频播放音量
     *参  数: hTmCc为控制句柄，iVolume为音量(-10000~0,-10000最小,0最大)
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SetVolume(IntPtr hTmCc, int iVolume);

    /*
     *函  数: TMCC_SetMute
     *说  明: 设置播放声音开关，只是本地解码后的声音
     *参  数: hTmCc为控制句柄，bMute为开关(FALSE打开声音，TRUE静音)
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SetMute(IntPtr hTmCc, bool bMute);

    /*
     *函  数: TMCC_GetDisplayRegion
     *说  明: 读取显示视频的显示位置
     *参  数: hTmCc为控制句柄，iRegionNum为显示号,pSrcRect为显示位置相对于原始图像,hDestWnd显示窗口,bEnable为是否显示
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_GetDisplayRegion(IntPtr hTmCc, int iRegionNum, Rect pSrcRect, IntPtr hDestWnd,
        ref bool bEnable);

    /*
     *函  数: TMCC_SetDisplayRegion
     *说  明: 读取显示视频的显示位置
     *参  数: hTmCc为控制句柄，iRegionNum为显示号,pSrcRect为显示位置相对于原始图像,hDestWnd显示窗口,bEnable为是否显示
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SetDisplayRegion(IntPtr hTmCc, int iRegionNum, Rect pSrcRect, IntPtr hDestWnd,
        bool bEnable);

    /*
     *函  数: TMCC_RefreshDisplay
     *说  明: 刷新当前显示
     *参  数: hTmCc为控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_RefreshDisplay(IntPtr hTmCc);

    /*
     *函  数: TMCC_GetImageSize
     *说  明: 读取视频大小
     *参  数: hTmCc为控制句柄，iWidth/iHeight为存放大小的指针
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_GetImageSize(IntPtr hTmCc, ref int iWidth, ref int iHeight);

    /*
     *函  数: TMCC_GetDisplayScale
     *说  明: 读取视频原始显示比例
     *参  数: hTmCc为控制句柄，iScale为存放显示比例的指针，注意此比例为宽/高*1000
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_GetDisplayScale(IntPtr hTmCc, ref int iScale);

    /*
     *函  数: TMCC_CapturePictureToBuffer
     *说  明: 当前显示视频抓图到指定缓冲
     *参  数: hTmCc为控制句柄,lpBuffer为存放图片缓冲,iBufferSize为缓冲大小(输入为缓冲大小，输出实际数据大小),pFmt为格式
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_CapturePictureToBuffer(IntPtr hTmCc, string pFmt,
        object lpBuffer,
        ref int iBufferSize);

    /*
     *函  数: TMCC_GetDiaReekCapability
     *说  明: 获取支持图像透雾的能力
     *参  数: hTmCc为控制句柄,piMode为透雾能力，0-不支持，其它为支持的能力
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_GetDiaReekCapability(IntPtr hTmCc, ref int piMode);

    /*
     *函  数: TMCC_SetDiaReekMode
     *说  明: 设置是否打开图像透雾功能
     *参  数: hTmCc为控制句柄,iMode为透雾开关，支持0-关，1-开
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SetDiaReekMode(IntPtr hTmCc, int iMode);

    /*
     *函  数: TMCC_SetImageProcessMode
     *说  明: 设置图像增强
     *参  数: hTmCc为控制句柄,iMode为增强模式，支持0和1
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SetImageProcessMode(IntPtr hTmCc, int iMode);

    /*
     *函  数: TMCC_OpenAvRender
     *说  明: 以指定条件打开音视频Render
     *参  数: pCfg为打开条件
     *返回值: 成功返回RENDER控制句柄,失败返回NULL
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_OpenAvRender(IntPtr hTmCc, TmAvRenderConfigT pCfg, IntPtr hPlayWnd);

    /*
     *函  数: TMCC_CloseAvRender
     *说  明: 关闭打开的Render
     *参  数: hTmFile为文件服务器句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_CloseAvRender(IntPtr hTmCc);

    /*
     *函  数: TMCC_PutinAvFrame
     *说  明: 输入音视频播放显示
     *参  数: hTmCc为控制句柄,pImage音视频帧
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_PutinAvFrame(IntPtr hTmCc, TmAvImageInfoT pImage);

    //////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////设备状态和报警监听/////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////
    /*
     *函  数: TMCC_StartListen
     *说  明: 开启状态和报警监听，以及报警图片上传
     *参  数: hTmCc为控制句柄,iPort为监听端口
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_StartListen(IntPtr hTmCc, int iPort);

    /*
     *函  数: TMCC_StopListen
     *说  明: 关闭状态和报警监听，以及报警图片上传
     *参  数: hTmCc为控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_StopListen(IntPtr hTmCc);

    //////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////监听设备主动上传///////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////
    /*
     *函  数: TMCC_RegisterDeviceLoginCallBack
     *说  明: 注册设备主动登录输出回调
     *参  数: hTmCc为服务器句柄,pCallBack用户回调函数指针,context为用户自定义指针
     *返回值: 无
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void TMCC_RegisterDeviceLoginCallBack(IntPtr hTmCc, TmccDeviceloginCallback pCallBack,
        object context);

    /*
     *函  数: TMCC_RegisterDeviceStreamCallBack
     *说  明: 注册设备数据上传输出回调
     *参  数: hTmCc为服务器句柄,pCallBack用户回调函数指针,context为用户自定义指针
     *返回值: 无
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void TMCC_RegisterDeviceStreamCallBack(IntPtr hTmCc, TmccDevicestreamCallback pCallBack,
        object context);

    /*
     *函  数: TMCC_RegisterDeviceTalkCallBack
     *说  明: 注册设备主动语音对讲输出回调
     *参  数: hTmCc为服务器句柄,pCallBack用户回调函数指针,context为用户自定义指针
     *返回值: 无
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void TMCC_RegisterDeviceTalkCallBack(IntPtr hTmCc, TmccDevicetalkCallback pCallBack,
        object context);

    /*
     *函  数: TMCC_StartListenDevice
     *说  明: 开启设备上传监听
     *参  数: hTmCc为控制句柄,iPort为监听端口
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_StartListenDevice(IntPtr hTmCc, int iPort);

    /*
     *函  数: TMCC_StopListenDevice
     *说  明: 关闭设备上传监听
     *参  数: hTmCc为控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_StopListenDevice(IntPtr hTmCc);

    //音视频解码
    /*
     *函  数: TMCC_OpenAvDecoder
     *说  明: 打开音视频解码
     *参  数: hTmCc为控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_OpenAvDecoder(IntPtr hTmCc, TmAvDecoderConfigT pCfg);

    /*
     *函  数: TMCC_CloseAvDecoder
     *说  明: 关闭音视频解码
     *参  数: hTmCc为控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_CloseAvDecoder(IntPtr hTmCc);

    /*
     *函  数: TMCC_PutInAvDecoderData
     *说  明: 输入原始数据解码成图片YUV
     *参  数: hTmCc为控制句柄,pImageIn为原始数据, pImageOut为解码输出的数据, iGetFrame放回是否解码完成
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_PutInAvDecoderData(IntPtr hTmCc, TmAvImageInfoT pImageIn, TmAvImageInfoT pImageOut,
        ref int iGetFrame);

    /*
     *函  数: TMCC_SetMD5EncryptParameter
     *说  明: 设置MD5的加密矩阵信息
     *参  数: hTmCc控制句柄, byData为矩阵数据
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_SetMD5EncryptParameter(IntPtr hTmCc, byte byData, int iDataSize);

    /*
     *函  数: TMCC_GetMD5EncryptData
     *说  明: 根据设置的MD5矩阵信息，加密数据
     *参  数: hTmCc控制句柄, byUserData为需要加密的数据,iUserDataSize为数据长度,byMD5Data为机密后的数据缓冲必须大于16
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_GetMD5EncryptData(IntPtr hTmCc, byte byUserData, int iUserDataSize,
        ref byte byMd5Data);

    /*
     *函  数: TMCC_GetFaceDetectResult
     *说  明: 读取当前显示帧的人脸检测结果
     *参  数: hTmCc为数据流控制句柄, pResult为存放结果的tmFaceDetectInfo_t指针
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_GetFaceDetectResult(IntPtr hTmCc, TmFaceDetectInfoT pResult);

    /// <summary>
    ///     时间定义
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TmTimeInfoT
    {
        /// <summary>
        ///     年
        /// </summary>
        public short wYear;

        /// <summary>
        ///     月
        /// </summary>
        public byte byMonth;

        /// <summary>
        ///     日
        /// </summary>
        public byte byDay;

        /// <summary>
        ///     时
        /// </summary>
        public byte byHour;

        /// <summary>
        ///     分
        /// </summary>
        public byte byMinute;

        /// <summary>
        ///     秒
        /// </summary>
        public byte bySecond;

        /// <summary>
        ///     保留
        /// </summary>
        public byte byTemp;

        /// <summary>
        ///     豪秒
        /// </summary>
        public uint dwMicroSecond;
    }

    /// <summary>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TmPlayControlCfgT
    {
        /// <summary>
        ///     本结构大小
        /// </summary>
        public uint dwSize;

        /// <summary>
        ///     控制命令
        /// </summary>
        public uint dwCommand;

        /// <summary>
        ///     文件的开始时间
        /// </summary>
        private readonly TmTimeInfoT strucTime;

        /// <summary>
        ///     播放参数
        /// </summary>
        public int iPlayData;

        /// <summary>
        ///     播放的速度
        /// </summary>
        public int iSpeed;

        /// <summary>
        ///     音频开关
        /// </summary>
        public int iEnableAudio;

        /// <summary>
        ///     新的播放位置(帧)
        /// </summary>
        public int iCurrentPosition;

        /// <summary>
        ///     新的播放位置(毫秒)
        /// </summary>
        public uint dwCurrentTime;

        /// <summary>
        ///     前进单帧
        /// </summary>
        public int bForward;

        /// <summary>
        ///     清空显示
        /// </summary>
        public int bClearDisplay;

        /// <summary>
        ///     是否自动调节缓冲
        /// </summary>
        public int bAutoResetBufTime;

        /// <summary>
        ///     是否自动生成索引
        /// </summary>
        public byte byAutoCreateIndex;

        /// <summary>
        ///     打开后是否自动播放
        /// </summary>
        public byte byAutoPlay;

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] byTemp;

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public char[] sFileName;
    }

    /// <summary>
    ///     播放相关
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TmPlayConditionCfgT
    {
        /// <summary>
        /// </summary>
        public uint dwSize;

        /// <summary>
        ///     厂商ID
        /// </summary>
        public ushort wFactoryId;

        /// <summary>
        ///     通道
        /// </summary>
        public byte byChannel;

        /// <summary>
        ///     是否操作图片
        /// </summary>
        public byte byPlayImage;

        /// <summary>
        ///     是否自动生成索引
        /// </summary>
        public byte byAutoCreateIndex;

        /// <summary>
        ///     打开后是否自动播放
        /// </summary>
        public byte byAutoPlay;

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] byTemp;

        /// <summary>
        ///     文件名
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public char[] sFileName;

        /// <summary>
        ///     文件访问回调函数
        /// </summary>
        public IntPtr pFileCallBack;

        /// <summary>
        ///     文件访问相关句柄
        /// </summary>
        public IntPtr pFileContext;

        /// <summary>
        ///     索引缓冲
        /// </summary>
        public IntPtr pAvIndex;

        /// <summary>
        ///     缓冲中的索引数
        /// </summary>
        public int iAvIndexCount;

        /// <summary>
        ///     开始播放是否需要缓冲数据
        /// </summary>
        public char byBufferBeforePlay;

        /// <summary>
        ///     是否启用网络参数
        /// </summary>
        public char byEnableServer;

        /// <summary>
        ///     播放方式
        /// </summary>
        public char byPlayType;

        /// <summary>
        ///     解码方式
        /// </summary>
        public char byDecoderType;

        /// <summary>
        ///     缓冲大小
        /// </summary>
        public int dwBufferSizeBeforePlay;

        /// <summary>
        ///     数据回调函数回调函数
        /// </summary>
        public IntPtr fnStreamReadCallBack;

        /// <summary>
        /// </summary>
        public IntPtr fnStreamReadContext;

        /// <summary>
        ///     多码流显示回调
        /// </summary>
        public IntPtr fnMultiStreamCallBack;

        /// <summary>
        /// </summary>
        public IntPtr fnMultiStreamContext;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public long left;
        public long top;
        public long right;
        public long bottom;
    }

    /// <summary>
    ///     连接信息结构体
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TmConnectInfoT
    {
        /// <summary>
        ///     该结构的大小，必须填写
        /// </summary>
        public uint dwSize;

        /// <summary>
        ///     连接服务器的IP地址
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] pIp;

        /// <summary>
        ///     服务器连接的端口
        /// </summary>
        public int iPort;

        /// <summary>
        ///     登录用户名
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] szUser;

        /// <summary>
        ///     登录用户口令
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] szPass;

        /// <summary>
        ///     登录用户级别，主要用户DVS的一些互斥访问资源
        /// </summary>
        public int iUserLevel;

        /// <summary>
        ///     用户自定义数据
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public byte[] pUserContext;
    }

    /// <summary>
    ///     实时流播放配置结构体
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TmPlayRealStreamCfgT
    {
        /// <summary>
        ///     结构体大小
        /// </summary>
        public uint dwSize;

        /// <summary>
        ///     连接服务器的IP地址
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] szAddress;

        /// <summary>
        ///     转发器地址
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] szTurnAddress;

        /// <summary>
        ///     服务器连接的端口
        /// </summary>
        public int iPort;

        /// <summary>
        ///     登录用户名
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] szUser;

        /// <summary>
        ///     登录用户口令
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] szPass;

        /// <summary>
        ///     通道
        /// </summary>
        public byte byChannel;

        /// <summary>
        ///     码流号
        /// </summary>
        public byte byStream;

        /// <summary>
        ///     传输类型
        /// </summary>
        public byte byTransType;

        /// <summary>
        ///     从连次数
        /// </summary>
        public byte byReConnectNum;

        /// <summary>
        ///     传输包大小
        /// </summary>
        public int iTransPackSize;

        /// <summary>
        ///     重连的时间间隔
        /// </summary>
        public int iReConnectTime;

        /// <summary>
        ///     传输协议0-内部自定,1-SONY,2-RTSP
        /// </summary>
        public byte byTransProtocol;

        /// <summary>
        /// </summary>
        public byte byForceDecode;

        /// <summary>
        ///     解码方式
        /// </summary>
        public byte byDecoderType;

        /// <summary>
        ///     登录用户口令
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public char[] ok;
    }

    /// <summary>
    ///     结构体：实时流信息
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TmRealStreamInfoT
    {
        public uint dwSize; //本结构大小
        public byte byFrameType; //帧类型0-视频，1-音频，2-数据流头
        public byte byNeedReset; //是否需要复位解码器
        public byte byKeyFrame; //是否关键帧
        public byte byTemp;
        public uint dwFactoryId; //厂家ID	
        public uint dwStreamTag; //流类型Tag

        public uint dwStreamId; //流ID

        //union
        //{
        //int	iWidth;		//视频宽
        public int iSamplesPerSec; //音频采样率

        //int	iHeight;	//视频高
        public int iBitsPerSample; //音频采样位数

        //};
        //union
        //{
        //    int	iFrameRate;	//帧率*1000
        public int iChannels; //音频的声道数

        //};
        //add by 2009-0429
        //union
        //{
        public uint nDisplayScale; //显示比例*1000

        //};
        public uint dwTimeStamp; //时间戳(单位毫秒)
        public uint dwPlayTime; //此帧播放时间(单位毫秒)
        public uint dwBitRate; //此数据流的码流大小	
        public IntPtr pBuffer; //数据缓冲
        public int iBufferSize; //数据大小
        public IntPtr pBuffer2; /*数据2缓冲*/
        public int iBuffer2Size; /*数据2大小*/
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TmAvImageInfoT
    {
        public byte video;
        public byte face;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] temp;

        public IntPtr buffer0;
        public IntPtr buffer1;
        public IntPtr buffer2;
        public IntPtr buffer3;
        public int bufSize0;
        public int bufSize1;
        public int bufSize2;
        public int bufSize3;
        private readonly StuVideo videoInfo;
        public int key_frame;
        public uint timestamp;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct StuVideo
    {
        public short width;
        public short height;
        public int frameRate;
        public byte format;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] temp;
    }

    /// <summary>
    ///     结构体，表示TmCommandInfoT命令信息
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TmCommandInfoT
    {
        /// <summary>
        ///     该结构的大小，必须填写为sizeof(tmCommandInfo_t)
        /// </summary>
        public uint dwSize;

        /// <summary>
        ///     主消息数据命令即数据类型
        /// </summary>
        public uint dwMajorCommand;

        /// <summary>
        ///     次消息数据命令即数据类型
        /// </summary>
        public uint dwMinorCommand;

        /// <summary>
        ///     通道号，该通道号要根据dwMajorCommand来判断是否有效
        /// </summary>
        public ushort iChannel;

        /// <summary>
        ///     子通道号，该通道号要根据dwMajorCommand来判断是否有效
        /// </summary>
        public ushort iStream;

        /// <summary>
        ///     消息数据缓冲
        /// </summary>
        public IntPtr pCommandBuffer;

        /// <summary>
        ///     消息数据缓冲大小
        /// </summary>
        public int iCommandBufferLen;

        /// <summary>
        ///     消息数据实际大小
        /// </summary>
        public int iCommandDataLen;

        /// <summary>
        ///     消息控制返回结果
        /// </summary>
        public uint dwResult;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TmToManagerImageInfoT
    {
        public uint dwSize;
        public StuImage image;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] byTemp;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] byMACAddr;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] byTemp2;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] szServerIP;

        public byte byImageFmt;
        public byte byCount;
        public byte byIndex;
        public byte byImageMode;
        public byte byAlarmId;
        public byte byChannelId;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] byOtherInfo;

        public StuTime time;
        public uint dwImageSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct StuImage
    {
        public short nWidth;
        public short nHeight;
        public byte byBitCount;
        public byte byRevolving;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] byTemp;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct StuTime
    {
        public short nYear;
        public byte nMonth;
        public byte nDay;
        public byte nDayOfWeek;
        public byte nHour;
        public byte nMinute;
        public byte nSecond;
    }

    /// <summary>
    ///     选定的目标区域(跟踪目标):基于704X576
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TmAiSelObjectT
    {
        public uint dwSize;

        /// <summary>
        ///     选择目标的模式0--手动1--自动选择
        /// </summary>
        public byte bySelMode;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] byTemp;

        /// <summary>
        ///     目标的坐标信息,手动选择时生效
        /// </summary>
        public int dwLeft;

        public int dwTop;
        public int dwRight;
        public int dwBottom;

        /// <summary>
        ///     原始坐标图像大小=0为默认,704x576
        /// </summary>
        public uint dwImageWidth;

        public uint dwImageHeight;
    }

    /// <summary>
    ///     视频跟踪源设置参数
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TmAiVideoInCfgT
    {
        /// <summary>
        ///     本结构大小
        /// </summary>
        public uint dwSize;

        /// <summary>
        ///     自能分析是否启用
        /// </summary>
        public byte byEnable;

        /// <summary>
        ///     视频源,0-默认(本机Vin), 1-网络视频(需要解码HI3559支持)
        /// </summary>
        public byte byVideoSource;

        /// <summary>
        ///     视频源,0-默认(摄像机自动选择), 1-可见光，2-热成像
        /// </summary>
        public byte byVideoId;

        /// <summary>
        ///     视频分析方式,0-默认(内部智能分析)，1-外部智能分析,2-内部KCF跟踪,3-外部神经网络跟踪,4-内部神经网络跟踪
        /// </summary>
        public byte byAITracer;

        // 分析者与本机通讯模式0-不通讯，1-串口通讯，2-网络通讯
        public byte byCtrlMode;

        // 与分析板通讯的串口号0-默认，1-第1个串口,2-第1个串口
        public byte byComSubId;

        /// <summary>
        ///     分析者需要的视频格式0-默认(YUV420),1-H264,2-MJPEG,3-H265,4-temperature,10-YPbPr,11-cvbs,12-hdmi,13-Digital
        /// </summary>
        public byte byImageFormat;

        // 视频的大小,+1与编码信息的参数byResolution一致，0-默认
        public byte byResolution;

        /// <summary>
        ///     图片大小宽0-默认
        /// </summary>
        public ushort nImageWidth;

        /// <summary>
        ///     图片大小高0-默认
        /// </summary>
        public ushort nImageHeight;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TmVideoInCfgT
    {
        public uint dwSize;
        public byte byAntiFlickerMode;
        public byte byVideoColorStyle;
        public byte byRotateAngle180;

        /// <summary>
        ///     彩转黑模式0-自动，1-彩色，2-黑白
        /// </summary>
        public byte byColorTransMode;

        public byte byShutterSpeed;
        public byte byAgc;
        public byte byIRShutMode;
        public byte byExposure;
        public byte byIRStartHour;
        public byte byIRStartMin;
        public byte byIRStopHour;
        public byte byIRStopMin;
        public byte byModeSwitch;
        public byte byWhiteBalance;
        public byte byWdr;
        public byte byBlc;
        public ushort nWhiteBalanceR;
        public ushort nWhiteBalanceB;
        public byte byMcTfStrength;
        public byte byIRType;
        public byte byIRCutTriggerAlarmOut;
        public byte byIRCutTime;
        public byte byExposureLevel;
        public byte byColorTransMin;
        public byte byColorTransMax;
        public byte byNoiseFilter;
        public byte byForceNoiseFilter;
        public byte byAeMeteringMode;
        public byte byWdrMode;
        public byte byIRShutAlarmIn;
        public byte byAutoContrast;
        public byte byLightInhibitionEn;
        public byte byVinFrameRate;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public byte[] byTemp;

        public byte byAgcTransMin;
        public byte byAgcTransMax;
        public ushort nMaxShutterSpeed;
        public ushort nMaxAgc;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 96)]
        public byte[] byAeMeteringData;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] byExposureLevelHdr;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public ushort[] nMinShutterSpeedHdr;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public ushort[] nMaxShutterSpeedHdr;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] byMaxAgcHdr;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4 * 96)]
        public byte[] byAeMeteringDataHdr;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TmAlarmInfoT
    {
        public uint dwSize;

        /// <summary>
        ///     报警类型，0-信号量报警,1-硬盘满,2-信号丢失，3－移动侦测，
        ///     4－硬盘未格式化,5-读写硬盘出错,6-遮挡报警,7-制式不匹配,
        /// </summary>
        public ushort wAlarmType;

        public ushort wAlarmState;
        public uint dwAlarmChannel;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] dwAlarmOutputNumber;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public uint[] dwAlarmRelateChannel;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public uint[] dwDiskNumber;
    }

    /// <summary>
    ///     跟踪配置参数
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TmTraceParamT
    {
        /// <summary>
        ///     本结构大小
        /// </summary>
        public uint dwSize;

        /// <summary>
        ///     是否启用重搜索功能
        /// </summary>
        public byte byEnableReSearch;

        /// <summary>
        ///     目标丢失启动重搜索超时时间: s
        /// </summary>
        public byte byLostTimeOut;

        /// <summary>
        ///     跟踪出错后启动重搜索超时时间:s
        /// </summary>
        public byte byErrTimeOut;

        /// <summary>
        ///     搜索目标的最小尺寸:pixel
        /// </summary>
        public byte byObjectMinSize;

        /// <summary>
        ///     搜索目标调整尺寸的阀值:pixel
        /// </summary>
        public byte byObjectAdThresh;

        /// <summary>
        ///     对搜索目标放大尺寸:pixel
        /// </summary>
        public byte byObjectAdSize;

        /// <summary>
        ///     搜索目标的阀值
        /// </summary>
        public byte bySearchThresh;

        /// <summary>
        ///     是否叠加雷达信息
        /// </summary>
        public byte byDisplayRadar;

        /// <summary>
        ///     是否显示调试信息
        /// </summary>
        public byte byDisplayDebug;

        /// <summary>
        ///     是否切换到调试模式
        /// </summary>
        public byte bySwitchDebugMode;

        /// <summary>
        ///     是否动态更新目标
        /// </summary>
        public byte byEnableUpdateObj;

        /// <summary>
        ///     动态更新目标方式:0--仅在云台移动后更新 1--实时更新
        /// </summary>
        public byte byUpdateMode;

        /// <summary>
        ///     跟踪器类型,0-默认(内部智能分析)，1-外部智能分析
        /// </summary>
        public byte byTracerType;

        /// <summary>
        ///     是否有外置目标识别器0-无,1-有
        /// </summary>
        public byte byAiObjectType;

        /// <summary>
        ///     目标分析者与本机通讯模式0-内部通讯，1-串口通讯，2-网络通讯
        /// </summary>
        public byte byCtrlMode;

        /// <summary>
        ///     与分析板通讯的串口号0-默认，1-第1个串口,2-第1个串口
        /// </summary>
        public byte byComId;

        /// <summary>
        ///     是否自动跟踪，需要启动自动目标识别
        /// </summary>
        public byte byEnableAutoTrace;

        /// <summary>
        ///     是否在图像上显示跟踪结果
        /// </summary>
        public byte byViewResult;

        /// <summary>
        ///     是否隐藏跟踪框
        /// </summary>
        public byte byHideTrackRect;

        /// <summary>
        ///     是否隐藏识别框
        /// </summary>
        public byte byHideIdentifyRect;
    }

    #region 智能识别、跟踪、指控平台引导联动相关接口 对应协议文档<<光电指控协议V2.0.docx>>

    /// <summary>
    ///     数据输出回调函数,具体数据类型根据返回的命令码确定
    /// </summary>
    /// <param name="iCmd">命令码</param>
    /// <param name="pData">数据指针</param>
    /// <param name="iDataLen">数据长度</param>
    /// <param name="context">上下文</param>
    public delegate void TmccTrackerDataCallback(uint iCmd, IntPtr pData, int iDataLen, IntPtr context);

    /// <summary>
    ///     初始化
    /// </summary>
    /// <param name="lpDeviceIp">设备IP</param>
    /// <param name="iPort">端口,默认9966</param>
    /// <returns>成功返回句柄，失败返回-1</returns>
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr TMCC_Tracker_Init(string lpDeviceIp, int iPort);

    /// <summary>
    ///     注册数据输出回调:光电的状态、目标识别的结果信息、跟踪的脱靶量等信息均从该接口输出
    /// </summary>
    /// <param name="hTracker">TMCC_Tracker_Init返回的句柄</param>
    /// <param name="pDataCallBack">回调函数</param>
    /// <param name="pContext">上下文</param>
    /// <returns>成功返回TMCC_ERR_SUCCESS，失败返回其它值</returns>
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_Tracker_RegisterDataCallBack(IntPtr hTracker, TmccTrackerDataCallback pDataCallBack,
        IntPtr pContext);

    /// <summary>
    ///     断开连接
    /// </summary>
    /// <param name="hTracker">TMCC_Tracker_Init返回的句柄</param>
    /// <param name="iCmd">命令码</param>
    /// <param name="pData">数据指针</param>
    /// <param name="iDataLen">数据长度</param>
    /// <returns>成功返回TMCC_ERR_SUCCESS，失败返回其它值</returns>
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int TMCC_Tracker_SendCommand(IntPtr hTracker, int iCmd, IntPtr pData, int iDataLen);

    /// <summary>
    ///     去初始化
    /// </summary>
    /// <param name="hTracker">TMCC_Tracker_Init()返回的句柄</param>
    /// <returns>无</returns>
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void TMCC_Tracker_Done(IntPtr hTracker);

    #endregion

    /*
     *函  数: TMCC_GetVersion
     *说  明: 获得本客户端库版本信息
     *参  数: pBulid为存放编译序号
     *返回值: 返回版本号，格式如<返回值为10000，则版本V1.0.0.00>
     */
    //TMCC_API uint TMCC_CALL TMCC_GetVersion(ref uint pBulid);
    /*
     *函  数: TMCC_RegisterLogCallBack
     *说  明: 注册调试信息回调函数，只有在开发模式下才有效
     *参  数: pCallBack为回调函数指针，context为调用者自定义数据
     *返回值: 无
     */
    //TMCC_API void TMCC_CALL TMCC_RegisterLogCallBack(TMCC_LOG_CALLBACK pCallBack, object context);
    /*
     *函  数: TMCC_RegisterServerInfoCallBack
     *说  明: 注册服务器消息信息回调函数，通过此回调可以得到服务器对管理中心的连接以及报警信息
     *参  数: pCallBack为回调函数指针，context为调用者自定义数据
     *返回值: 无
     */
    //TMCC_API void TMCC_CALL TMCC_RegisterServerInfoCallBack(TMCC_SERVERINFO_CALLBACK pCallBack, object context);
    /*
     *函  数: TMCC_GetLastError
     *说  明: 获得当前的错误码
     *参  数: 无
     *返回值: 返回错误码
     */
    //TMCC_API int TMCC_CALL TMCC_GetLastError();
    /////////////////////////////////////////////////////////////////////
    ///////////////////////////设备基本配置控制接口//////////////////////
    /////////////////////////////////////////////////////////////////////
    /*
     *函  数: TMCC_Init
     *说  明: 初始化并获得一个服务器控制句柄，其它所有接口函数都依赖此句柄访问服务器
     *参  数: dwFlags保留，必须设为0
     *返回值: 成功返回控制句柄，失败返回NULL
     */
    //TMCC_API System.IntPtr TMCC_CALL TMCC_Init(uint dwFlags);
    /*
     *函  数: TMCC_Done
     *说  明: 释放服务器控制句柄相关资源
     *参  数: hTmCC为TMCC_Init返回的服务器控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_Done(System.IntPtr hTmCC);
    /*
     *函  数: TMCC_RegisterConnectCallBack
     *说  明: 注册连接信息返回回调函数，异步连接成功通过该回调函数返回，发生错误断开连接也通过该回调
     *参  数: hTmCC为服务器控制句柄，pCallBack为回调函数指针，context为自定义数据
     *返回值: 无
     */
    //TMCC_API void TMCC_CALL TMCC_RegisterConnectCallBack(System.IntPtr hTmCC, TMCC_CONNECT_CALLBACK pCallBack, object context);
    /*
     *函  数: TMCC_RegisterDataReadCallBack
     *说  明: 注册服务器消息读取回调函数，异步方式通过它获得服务器消息，其它如报警消息也通过它得到
     *参  数: hTmCC为TMCC_Init返回的服务器控制句柄，pCallBack为回调函数指针，context为自定义数据
     *返回值: 无
     */
    //TMCC_API void TMCC_CALL TMCC_RegisterDataReadCallBack(System.IntPtr hTmCC, TMCC_DATAREAD_CALLBACK pCallBack, object context);
    /*
     *函  数: TMCC_RegisterProgressCallBack
     *说  明: 注册升级备份信息回调函数，只有在开发升级模式下才有效
     *参  数: pCallBack为回调函数指针，context为调用者自定义数据
     *返回值: 无
     */
    //TMCC_API void TMCC_CALL TMCC_RegisterProgressCallBack(System.IntPtr hTmCC, TMCC_PROGRESS_CALLBACK pCallBack, object context);
    /*
     *函  数: TMCC_SetTimeOut
     *说  明: 设置与服务器通讯操作的超时时间，必须要在TMCC_Connect调用前设置才有效
     *参  数: hTmCC为TMCC_Init返回的服务器控制句柄，dwTime时间值单位为毫秒系统默认为2000毫秒
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SetTimeOut(System.IntPtr hTmCC, int dwTime);
    /*
     *函  数: TMCC_GetTimeOut
     *说  明: 获得与服务器通讯操作的超时时间，任何时候都能调用
     *参  数: hTmCC为TMCC_Init返回的服务器控制句柄
     *返回值: 成功返回时间值，错误返回0
     */
    //TMCC_API int TMCC_CALL TMCC_GetTimeOut(System.IntPtr hTmCC);
    /*
     *函  数: TMCC_SetAutoReConnect
     *说  明: 设置发生错误后是否自动重新连接，注意必须要注册连接回调函数，且会根据回调函数返回值判断是否连接
     *参  数: hTmCC为TMCC_Init返回的服务器控制句柄，bAutoConnect为自动连接标志，系统默认为FALSE
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SetAutoReConnect(System.IntPtr hTmCC, bool bAutoConnect);
    /*
     *函  数: TMCC_GetAutoReConnect
     *说  明: 获得发生错误后是否自动重新连接标志，任何时候都能调用
     *参  数: hTmCC为TMCC_Init返回的服务器控制句柄
     *返回值: 成功返回自动连接标志，错误返回FALSE
     */
    //TMCC_API bool TMCC_CALL TMCC_GetAutoReConnect(System.IntPtr hTmCC);
    /*
     *函  数: TMCC_IsConnect
     *说  明: 获得当前是否正常连接服务器，任何时候都能调用
     *参  数: hTmCC为TMCC_Init返回的服务器控制句柄
     *返回值: 成功返回连接正常标志，错误返回FALSE
     */
    //TMCC_API bool TMCC_CALL TMCC_IsConnect(System.IntPtr hTmCC);
    /*
 *函  数: TMCC_Connect
 *说  明: 连接服务器，即登录到服务器，调用该函数成功后必须要调用TMCC_DisConnect才能再一次调用，
          如果调用一异步方式则连接成功会通过连接回调函数反映，如没注册连接回调函数，异步连接会失败
 *参  数: hTmCC为TMCC_Init返回的服务器控制句柄，pConnectInfo为用户连接信息，bSync为连接方式<异步或同步>
 *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
 */
    //TMCC_API int TMCC_CALL TMCC_Connect(System.IntPtr hTmCC, tmConnectInfo_t pConnectInfo, bool bSync);
    /*
     *函  数: TMCC_DisConnect
     *说  明: 断开到服务器的连接
     *参  数: hTmCC为TMCC_Init返回的服务器控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_DisConnect(System.IntPtr hTmC);
    /*
 *函  数: TMCC_SetConfig
 *说  明: 服务器参数配置，该函数所有配置的参数在服务器重新启后会丢失，除非调用TMCC_SaveConfig明确保存
 *参  数: hTmCC为TMCC_Init返回的服务器控制句柄；
           pCommandInfo命令消息结构指针，其中包含了所需要的命令，缓冲等
 *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
 */
    //TMCC_API int TMCC_CALL TMCC_SetConfig(System.IntPtr hTmCC, tmCommandInfo_t pCommandInfo);
    /*
 *函  数: TMCC_GetConfig
 *说  明: 获取服务器参数
 *参  数: hTmCC为TMCC_Init返回的服务器控制句柄；
           pCommandInfo命令消息结构指针，其中包含了所需要的命令，缓冲等
 *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
 */
    //TMCC_API int TMCC_CALL TMCC_GetConfig(System.IntPtr hTmCC, tmCommandInfo_t pCommandInfo);
    /*
     *函  数: TMCC_SaveConfig
     *说  明: 保存服务器配置参数，服务器中Flash的察写次数一般有限，建议不要经常调用
     *参  数: hTmCC为TMCC_Init返回的服务器控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SaveConfig(System.IntPtr hTmCC);
    /*
     *函  数: TMCC_RestoreConfig
     *说  明: 恢复服务器配置参数为系统默认值
     *参  数: hTmCC为TMCC_Init返回的服务器控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_RestoreConfig(System.IntPtr hTmCC);
    /*
     *函  数: TMCC_Reboot
     *说  明: 重新启动服务器，启动后客户端会自动连接它，重启期间所有设置均无效
     *参  数: hTmCC为TMCC_Init返回的服务器控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_Reboot(System.IntPtr hTmCC);
    /*
     *函  数: TMCC_ShutDown
     *说  明: 关闭服务器，关闭后与服务器的连接自动断开
     *参  数: hTmCC为TMCC_Init返回的服务器控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_ShutDown(System.IntPtr hTmCC);
    /*
 *函  数: TMCC_UpgradeSystem
 *说  明: 升级服务器系统，升级系统也要操作Flash，所以建议不要经常升级系统，
           该函数为阻塞调用，操作期间其它所有操作无效
 *参  数: hTmCC为TMCC_Init返回的服务器控制句柄，lpszFileName为系统镜像全路径名
 *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
 */
    //TMCC_API int TMCC_CALL TMCC_UpgradeSystem(System.IntPtr hTmCC, string lpszFileName);
    /*
 *函  数: TMCC_BackupSystem
 *说  明: 保存服务器系统为镜像文件，该函数为阻塞调用，操作期间其它所有操作无效
 *参  数: hTmCC为TMCC_Init返回的服务器控制句柄，lpszFileName为系统镜像全路径名；
 *		  iModal为要备份的模块标志，分别以位表示，从最低为开始 :
          0位表示备份运行模块，1位表示备份升级的运行模块, 2位表示备份参数模块，3位表示备份PTZ模块，
          4位表示备份WEB模块，5位表示备份扩展模块
 *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
 */
    //TMCC_API int TMCC_CALL TMCC_BackupSystem(System.IntPtr hTmCC, string lpszFileName, int iModal = 0x3E);
    /*
     *函  数: TMCC_EnumServer
     *说  明: 列举网络中的服务器，列举出来的设备仅能作为参考，可能有些不能列举到
     *参  数: pCallBack为回调函数指针；context为调用者设置的上下文指针
     *		  在函数pCallBack中返回TRUE表示继续列举，FALSE为停止列举
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_EnumServer(TMCC_ENUMSERVER_CALLBACK pCallBack, object context, bool bRegisterCallBack = FALSE);
    /*
     *函  数: TMCC_RefreshEnumServer
     *说  明: 刷新网络服务器列表
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_RefreshEnumServer();
    /*
     *函  数: TMCC_ConfigServer
     *说  明: 配置服务器的地址信息
     *参  数: dwFlags为配置类型，即通过什么方式配置，目前仅支持网络
     *		  pConfig为新的服务器信息
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_ConfigServer(tmServerCfg_t pConfig, uint dwFlags = 0);
    //云台控制
    /*
     *函  数: TMCC_PtzGetDecoderList
     *说  明: 获得服务器内置解码器协议列表
     *参  数: pInfo为要填协议的列表，info_num为输入为列表最大个数，输出为当前协议个数
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_PtzGetDecoderList(System.IntPtr hTmCC, tmDecoderInfo_t[] pInfo, ref int info_num);
    /*
     *函  数: TMCC_PtzOpen
     *说  明: 打开云台控制，要控制服务器云台，必须先打开对应台号
     *参  数: iChannel为云台号
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_PtzOpen(System.IntPtr hTmCC, int iChannel, bool bLock = FALSE);
    /*
     *函  数: TMCC_PtzClose
     *说  明: 关闭云台控制
     *参  数: hTmCC为控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_PtzClose(System.IntPtr hTmCC);
    /*
     *函  数: TMCC_PtzLock
     *说  明: 锁定云台控制，必须先打开对应台号
     *参  数: hTmCC为控制句柄, iChannel为云台号
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_PtzLock(System.IntPtr hTmCC, int iChannel);
    /*
     *函  数: TMCC_PtzUnLock
     *说  明: 解锁云台控制
     *参  数: hTmCC为控制句柄, iChannel为云台号
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_PtzUnLock(System.IntPtr hTmCC, int iChannel);
    /*
     *函  数: TMCC_PtzControl
     *说  明: 云台控制命令，这些命令得经过选用协议转换后输出，调用此功能必须选用正确解码协议
     *参  数: hTmCC为控制句柄，dwPTZCommand为云台控制命令，dwPTZControl为控制方法0-关闭1-开始，dwSpeed为运动速度
     *		  如果NVS中无正确的对应协议，可以用	TMCC_PtzTrans来由上层软件处理
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_PtzControl(System.IntPtr hTmCC, uint dwPTZCommand, uint dwPTZControl, uint dwSpeed);
    /*
     *函  数: TMCC_PtzTrans
     *说  明: 为云台控制直接输出，不用内置解码协议转换
     *参  数: hTmCC为控制句柄，pPTZCodeBuf为输出的数据缓冲，iBufSize为数据大小，最大值必须小于128
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_PtzTrans(System.IntPtr hTmCC, ref byte pPTZCodeBuf, int iBufSize);
    /*
     *函  数: TMCC_PtzPreset
     *说  明: 云台预置点调用
     *参  数: hTmCC为控制句柄，dwPTZPresetCmd为云台控制命令号，dwPresetIndex为预置点号，dwSpeed为速度
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_PtzPreset(System.IntPtr hTmCC, uint dwPTZPresetCmd, uint dwPresetIndex, uint dwSpeed);
    /*
     *函  数: TMCC_PtzIntegrate
     *说  明: 为云台综合控制，需要云台直接和一体机支持
     *参  数: hTmCC为控制句柄，pParam为控制的具体参数结构指针，iParamSize为结构大小大小，最大值必须小于128
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_PtzIntegrate(System.IntPtr hTmCC, tmPtzParameter_t pParam, int iParamSize);
    //透明通道设置
    /*
     *函  数: TMCC_RegisterSerialDataReadCallBack
     *说  明: 注册服务器串口数据获得回调函数
     *参  数: hTmCC为控制句柄, , pCallBack为获取通道的回传数据，context为用户自定义数据
     *返回值: 无
     */
    //TMCC_API void TMCC_CALL TMCC_RegisterSerialDataReadCallBack(System.IntPtr hTmCC, TMCC_SERIALDATA_CALLBACK pCallBack, object context);
    /*
     *函  数: TMCC_SerialOpen
     *说  明: 打开与服务器间的透明通道传输
     *参  数: hTmCC为控制句柄, iSerialPort==0时打开232,==1打开485
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SerialOpen(System.IntPtr hTmCC, int iSerialPort);
    /*
     *函  数: TMCC_SerialSend
     *说  明: 向服务器发送通明数据，这些数据服务器将不作处理直接送出
     *参  数: hTmCC为控制句柄，iChannel为发送的通道号仅485有效
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SerialSend(System.IntPtr hTmCC, int iChannel, ref string pSendBuf, int iBufSize);
    /*
     *函  数: TMCC_SerialRecv
     *说  明: 从服务器读取通明数据，这些数据服务器将不作处理直接送出
     *参  数: hTmCC为控制句柄，iChannel为发送的通道号仅485有效
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SerialRecv(System.IntPtr hTmCC, int iChannel, ref string pRecvBuf, int iBufSize);
    /*
     *函  数: TMCC_SerialClose
     *说  明: 关闭与服务器的通明传输
     *参  数: hTmCC为控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SerialClose(System.IntPtr hTmCC);
    ///////////////////////////////////////////////////////////////////////
    ///////////////////////////////语音对讲////////////////////////////////
    ///////////////////////////////////////////////////////////////////////
    /*
     *函  数: TMCC_StartVoiceCom
     *说  明: 启动语音对讲
     *参  数: hTmCC为控制句柄，pConnectInfo为语音对象可以为NULL
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_StartVoiceCom(System.IntPtr hTmCC, tmConnectInfo_t pConnectInfo);
    /*
     *函  数: TMCC_SetVoiceComClientVolume
     *说  明: 设置客户端语音播放硬件声音大小
     *参  数: hTmCC为控制句柄，iVolume为声音值-10000~0,-10000最小,0最大
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SetVoiceComClientVolume(System.IntPtr hTmCC, int iVolume);
    /*
     *函  数: TMCC_SetVoiceComClientVolumeZoom
     *说  明: 设置客户端语音播放软件放大值
     *参  数: hTmCC为控制句柄，fpmPerNum为声音放大值0 < fpmPerNum < 10.0
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SetVoiceComClientVolumeZoom(System.IntPtr hTmCC, float fpmPerNum);
    /*
     *函  数: TMCC_SetVoiceComClientMicZoom
     *说  明: 设置客户端语音采集，即麦克风声音大小
     *参  数: hTmCC为控制句柄，fpmPerNum为声音放大值0 < fpmPerNum < 10.0
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SetVoiceComClientMicZoom(System.IntPtr hTmCC, float fpmPerNum);
    /*
     *函  数: TMCC_StopVoiceCom
     *说  明: 关闭与服务器的语音队讲
     *参  数: hTmCC为控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_StopVoiceCom(System.IntPtr hTmCC);
    /*
     *函  数: TMCC_RegisterVoiceDataCallBack
     *说  明: 注册语音对讲数据获得回调，通过此回调可以得到服务器的语音数据
     *参  数: pCallBack为回调函数指针，context为调用者自定义数据
     *返回值: 无
     */
    //TMCC_API void TMCC_CALL TMCC_RegisterVoiceDataCallBack(System.IntPtr hTmCC, TMCC_VOICEDATA_CALLBACK pCallBack, object context);
    /*
     *函  数: TMCC_PutInVoiceData
     *说  明: 输入语音对讲数据
     *参  数: hTmCC为控制句柄，pVoiceData为音频数据结构
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_PutInVoiceData(System.IntPtr hTmCC, tmVoiceData_t pVoiceData);
    /*
     *函  数: TMCC_OpenVoice
     *说  明: 打开语音对讲数据解码和语音捕获
     *参  数: hTmCC为控制句柄，pVoiceHead为音频数据头结构
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_OpenVoice(System.IntPtr hTmCC, tmVoiceHead_t pVoiceInHead, tmVoiceHead_t pVoiceOutHead);
    /*
     *函  数: TMCC_CloseVoice
     *说  明: 关闭语音对讲数据解码和语音捕获
     *参  数: hTmCC为控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_CloseVoice(System.IntPtr hTmCC);
    //服务器状态
    /*
     *函  数: TMCC_GetServerWorkState
     *说  明: 得到服务器当前工作状态
     *参  数: hTmCC为控制句柄，lpWorkStatew为存放服务器状态信息的结构指针
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_GetServerWorkState(System.IntPtr hTmCC, tmWorkState_t lpWorkState);
    /*
     *函  数: TMCC_ConfigServerEx
     *说  明: 配置服务器的地址信息，1.0.0.09添加
     *参  数: dwFlags为配置类型，即通过什么方式配置，目前仅支持网络
     *		  pConfig为新的服务器信息
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_ConfigServerEx(System.IntPtr hTmCC, tmServerExCfg_t pConfig, uint dwFlags);
    //////////////////////////////////////////////////////////////////////////
    //2007/07/16 franxkia 新加
    //设置播放的wave文件
    /*
     *函  数: TMCC_SetPlayWaveFile
     *说  明: PC播放WAVE文件,通过语音对讲通道传送到DVS,DVS输出
     *参  数: hTmCC为控制句柄，pFileName为文件名全路径,bPlayNum为要播放的次数
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SetPlayWaveFile(System.IntPtr hTmCC, ref string pFileName, byte bCirclePlay);
    /*
     *函  数: TMCC_SaveDefaultConfig
     *说  明: 保存服务器默认配置参数，服务器中Flash的察写次数一般有限，建议不要经常调用
     *参  数: hTmCC为TMCC_Init返回的服务器控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SaveDefaultConfig(System.IntPtr hTmCC);
    //////////////////////////////////////////////////////////////////////////////
    ///////////视频解码器直接操作函数接口定义 add by stone 20070930///////////////
    //////////////////////////////////////////////////////////////////////////////
    /*
     *函  数: TMCC_SetConnectInfo
     *说  明: 配置服务器的连接信息，1.0.0.20添加，如果是循环连接则加入，如果是非循环那就处理
     *参  数: pConfig为连接信息, iChannel为通道号(或窗口号)，size为pConfig的缓冲大小
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SetConnectInfo(System.IntPtr hTmCC, int iChannel, tmConnectCfg_t pConfig, int size);
    /*
     *函  数: TMCC_GetConnectInfo
     *说  明: 得到当前的连接信息，1.0.0.20添加
     *参  数: pConfig为连接信息, iChannel为通道号(或窗口号)，size为pConfig的缓冲大小
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_GetConnectInfo(System.IntPtr hTmCC, int iChannel, tmConnectCfg_t pConfig, int size);
    /*
     *函  数: TMCC_ClearConnectInfo
     *说  明: 清除当前的连接信息，如果是循环连接则清除指定循环序号，1.0.0.20添加
     *参  数: iChannel为通道号(或窗口号)，iIndex为为循环的序号为-1则为清除所有
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_ClearConnectInfo(System.IntPtr hTmCC, int iChannel, int iIndex);
    /*
     *函  数: TMCC_GetEnumConnectInfo
     *说  明: 枚举服务器的连接信息，通过回调输出，回调中可以反映通道号，是否是循环连接等信息，1.0.0.20添加
     *参  数: iChannel为通道号(或窗口号)，pCallBack为回调函数指针，context为用户定义的指针
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_EnumConnectInfo(System.IntPtr hTmCC, int iChannel, TMCC_ENUMCONNECT_CALLBACK pCallBack, object context);
    /*
     *函  数: TMCC_SetWindowInfo
     *说  明: 设置显示的窗口信息，1.0.0.20添加
     *参  数: iChannel为通道号(或窗口号)，pConfig为窗口信息指针
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SetWindowInfo(System.IntPtr hTmCC, int iChannel, tmWindowsCfg_t pConfig, int size);
    /*
     *函  数: TMCC_GetWindowInfo
     *说  明: 读取显示的窗口信息，1.0.0.20添加
     *参  数: iChannel为通道号(或窗口号)，pConfig为窗口信息指针
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_GetWindowInfo(System.IntPtr hTmCC, int iChannel, tmWindowsCfg_t pConfig, int size);
    /*
     *函  数: TMCC_SetDisplayInfo
     *说  明: 设置解码器的显示信息，如单屏放大等,1.0.0.20添加
     *参  数: pConfig为显示信息指针，size为数据缓冲大小
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SetDisplayInfo(System.IntPtr hTmCC, tmDisplayCfg_t pConfig, int size);
    /*
     *函  数: TMCC_GetDisplayInfo
     *说  明: 读取解码器的显示信息，如单屏放大等,1.0.0.20添加
     *参  数: pConfig为显示信息指针，size为数据缓冲大小
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_GetDisplayInfo(System.IntPtr hTmCC, tmDisplayCfg_t pConfig, int size);
    /*
     *函  数: TMCC_SetLockInfo
     *说  明: 设置通道连接锁定信息，如单是否循环切换等, 注意循切的时间是在设置连接信息时配置,1.0.0.20添加
     *参  数: iChannel为通道号,pConfig为显示信息指针，size为数据缓冲大小
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SetLockInfo(System.IntPtr hTmCC, int iChannel, tmLockCfg_t pConfig, int size);
    /*
     *函  数: TMCC_GetLockInfo
     *说  明: 读取通道连接锁定信息，如单是否循环切换等, 注意循切的时间是在设置连接信息时配置,1.0.0.20添加
     *参  数: iChannel为通道号,pConfig为显示信息指针，size为数据缓冲大小
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_GetLockInfo(System.IntPtr hTmCC, int iChannel, tmLockCfg_t pConfig, int size);
    /*
     *函  数: TMCC_StartConnect
     *说  明: 使解码器打开连接,1.0.0.20添加
     *参  数: iChannel为通道号
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_StartConnect(System.IntPtr hTmCC, int iChannel);
    /*
     *函  数: TMCC_StopConnect
     *说  明: 使解码器关闭连接,1.0.0.20添加
     *参  数: iChannel为通道号
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_StopConnect(System.IntPtr hTmCC, int iChannel);
    /*
     *函  数: TMCC_SetSerialNumber
     *说  明: 设置连接的唯一厂商信息
     *参  数: hTmCC控制句柄, szSerialNumber为序列号
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SetSerialNumber(System.IntPtr hTmCC, string szSerialNumber);
    /*
     *函  数: TMCC_SetOtherParam
     *说  明: 设置一些扩展信息，已废弃
     *参  数: hTmCC为控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SetOtherParam(System.IntPtr hTmCC, uint dwFlags, object buf, ref int iLen);
    /*
     *函  数: TMCC_UpgradeWebPage
     *说  明: 升级服务器中的网页，网页可由用户自行设计，空间大小限制5M内
     *参  数: hTmCC为控制句柄, lpszPathName存放网页的目录
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_UpgradeWebPage(System.IntPtr hTmCC, string lpszPathName);
    /*
     *函  数: TMCC_UpgradeWebOcx
     *说  明: 升级服务器中的网页，网页可由用户自行设计，空间大小限制5M内
     *参  数: hTmCC为控制句柄, lpszNameOCX的全路劲名
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_UpgradeWebOcx(System.IntPtr hTmCC, string lpszName);
    /*
     *函  数: TMCC_UpgradeKernel
     *说  明: 升级服务器的Firmware
     *参  数: hTmCC为控制句柄, lpszName为Firmware名称
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_UpgradeKernel(System.IntPtr hTmCC, string lpszName);
    /*
     *函  数: TMCC_UpgradeTmccModule
     *说  明: 升级服务器的Tmcc扩展模块
     *参  数: hTmCC为控制句柄, lpszName为扩展模块目录名称
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_UpgradeTmccModule(System.IntPtr hTmCC, string lpszName);
    //////////////////////////////////////////////////////////////
    ///////////////////设备自动升级模块///////////////////////////
    //////////////////////////////////////////////////////////////
    /*
     *函  数: TMCC_ConfigUpgradeServer
     *说  明: 配置自动升级服务器
     *参  数: hTmCC为升级服务器控制句柄
     *返回值: 成功返回升级服务器控制句柄，失败返回NULL
     */
    //TMCC_API int TMCC_CALL TMCC_ConfigUpgradeServer(System.IntPtr hTmCC, tmUpgradeConfig_t pConfig);
    /*
     *函  数: TMCC_RegisterUpgradeCallBack
     *说  明: 注册升级服务器信息输出回调函数
     *参  数: hTmCC为升级服务器控制句柄
     *返回值: 成功返回升级服务器控制句柄，失败返回NULL
     */
    //TMCC_API void TMCC_CALL TMCC_RegisterUpgradeCallBack(System.IntPtr hTmCC, TMCC_UPGRADE_CALLBACK pCallBack, object context);
    /*
     *函  数: TMCC_StartUpgrade
     *说  明: 启动升级服务器，开始升级
     *参  数: hTmCC为升级服务器控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_StartUpgrade(System.IntPtr hTmCC);
    /*
     *函  数: TMCC_StopUpgrade
     *说  明: 停止升级服务器
     *参  数: hTmCC为升级服务器控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_StopUpgrade(System.IntPtr hTmCC);
    /*
     *函  数: TMCC_SendUpgradeMessage
     *说  明: 向网络上发送升级消息
     *参  数: hTmCC为升级服务器控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SendUpgradeMessage(System.IntPtr hTmCC);
    //////////////////////////////////////////////////////////////
    ///////////////////设备自动服务器搜索/////////////////////////
    //////////////////////////////////////////////////////////////
    /*
     *函  数: TMCC_RegisterEnumServerCallBack
     *说  明: 注册枚举服务器信息回调函数
     *参  数: pCallBack为回调函数指针，context为调用者自定义数据
     *返回值: 无
     */
    //TMCC_API void TMCC_CALL TMCC_RegisterEnumServerCallBack(System.IntPtr hEnum, TMCC_ENUMSERVER_CALLBACK pCallBack, object context);
    /*
     *函  数: TMCC_StartEnum
     *说  明: 启动搜索
     *参  数: hEnum为回枚举控制句柄，由TMCC_CreateEnumServer初始化返回句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_StartEnum(System.IntPtr hEnum);
    /*
     *函  数: TMCC_StopListen
     *说  明: 关闭枚举监听
     *参  数: hEnum为回枚举控制句柄，由TMCC_CreateEnumServer初始化返回句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_StopEnum(System.IntPtr hEnum);
    /*
     *函  数: TMCC_EnumServer
     *说  明: 列举网络中的服务器
     *参  数: hEnum为回枚举控制句柄，由TMCC_CreateEnumServer初始化返回句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_RefreshEnum(System.IntPtr hEnum);
    /*
     *函  数: TMCC_ConfigServerBuEnum
     *说  明: UDP方式配置服务器
     *参  数: hEnum为回枚举控制句柄，由TMCC_CreateEnumServer初始化返回句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_ConfigServerByMulti(System.IntPtr hEnum, tmMultiServerCfg_t pConfig);
    ////////////////////////////////////////////////////////
    ///////////////////实时数据播放/////////////////////////
    ////////////////////////////////////////////////////////
    /*
     *函  数: TMCC_RegisterStreamCallBack
     *说  明: 注册实时数据输出回调
     *参  数: hClient为文件服务器句柄,pCallBack为回调函数指针,context为用户指针式
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API void TMCC_CALL TMCC_RegisterStreamCallBack(System.IntPtr hClient, TMCC_STREAMREAD_CALLBACK pCallBack, object context);
    /*
     *函  数: TMCC_RegisterRtpStreamCallBack
     *说  明: 注册实时RTP数据输出回调
     *参  数: hClient为文件服务器句柄,pCallBack为回调函数指针,context为用户指针式
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API void TMCC_CALL TMCC_RegisterRtpStreamCallBack(System.IntPtr hClient, TMCC_RTPSTREAMREAD_CALLBACK pCallBack, object context, bool bTcpPacket);
    /*
     *函  数: TMCC_RegisterConnectMessage
     *说  明: 注册连接状态输出消息
     *参  数: hClient为服务器句柄,hWnd用户接收消息窗口,msgID消息ID
     *返回值: 无
     */
    //TMCC_API void TMCC_CALL TMCC_RegisterConnectMessage(System.IntPtr hClient, System.IntPtr hWnd, uint msgID);
    /*
     *函  数: TMCC_ConnectStream
     *说  明: 以指定条件打开服务器实时流
     *参  数: hClient为播放句柄,pPlayInfo为打开文件的条件
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_ConnectStream(System.IntPtr hClient, tmPlayRealStreamCfg_t pPlayInfo, System.IntPtr hPlayWnd);
    /*
     *函  数: TMCC_MakeKeyFrame
     *说  明: 强制下一帧为关键帧，当句柄是连接视频的那么通道信息用连接中的信息
     *参  数: hClient为播放句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_MakeKeyFrame(System.IntPtr hClient);
    /*
     *函  数: TMCC_SetRealStreamBufferTime
     *说  明: 设置缓冲时间大小
     *参  数: hClient为播放句柄,dwTime为缓冲时间(单位毫秒)
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SetStreamBufferTime(System.IntPtr hClient, uint dwTime);
    /*
     *函  数: TMCC_RefreshRealStreamBuffer
     *说  明: 刷新当前数据缓冲
     *参  数: hClient为播放句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_RefreshStreamBuffer(System.IntPtr hClient);
    /*
     *函  数: TMCC_SwicthRealStream
     *说  明: 切换数据流连接的码流
     *参  数: hClient为播放句柄,iStreamId为码流号
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SwicthRealStream(System.IntPtr hClient, int iStreamId);
    ////////////////////////////////////////////////////////
    /////////////////////数据流播放/////////////////////////
    ////////////////////////////////////////////////////////
    /*
     *函  数: TMCC_OpenStream
     *说  明: 以指定条件打开服务器文件
     *参  数: hTmFile为文件服务器句柄,pHeadBuf为打开文件头或者tmStreamHeadInfo_t头,hPlayWnd为显示视频的窗口
     *		  iPushBufferSize为缓冲大小，根据编码格式决定，一般1024*1024
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_OpenStream(System.IntPtr hStream, ref byte pHeadBuf, int iHeadSize, System.IntPtr hPlayWnd, int iPushBufferSize);
    /*
     *函  数: TMCC_ResetStream
     *说  明: 以指定条件复位数据流
     *参  数: hStream数据流控制句柄,pHeadBuf为数据流头缓冲，iHeadSize头缓冲大小
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_ResetStream(System.IntPtr hStream, ref byte pHeadBuf, int iHeadSize);
    /*
     *函  数: TMCC_CloseStream
     *说  明: 关闭数据流播放
     *参  数: hStream为文件服务器句柄,pPlayInfo为打开文件的条件,hPlayWnd为显示视频的窗口
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_CloseStream(System.IntPtr hStream);
    /*
     * 函  数: TMCC_GetFileHead
     * 说  明: 读取文件的头，可以直接输入OpenStream
     * 参  数: pHeadBuf为数据流头存放缓冲,iBufSize缓冲长度
     * 返回值: 成功返回文件头长度，失败返回<=0
     */
    //TMCC_API int TMCC_CALL TMCC_GetFileHead(string lpszFileName, ref byte pHeadBuf, int iBufSize);
    /*
     * 函  数: TMCC_CreateFileHead
     * 说  明: 读取文件的头，可以直接输入OpenStream
     * 参  数: pStreamHead为文件信息音视频信息，pHeadBuf为数据流头存放缓冲,iBufSize缓冲长度
     * 返回值: 成功返回文件头长度，失败返回<=0
     */
    //TMCC_API int TMCC_CALL TMCC_CreateFileHead(tmStreamHeadInfo_t pStreamHead, ref byte pHeadBuf, int iBufSize);
    /*
     * 函  数: TMCC_CreateStreamHead
     * 说  明: 转换文件头到数据流头
     * 参  数: pStreamHead为文件信息音视频信息，pHeadBuf为数据流头存放缓冲,iBufSize缓冲长度
     * 返回值: 成功返回文件头长度，失败返回<=0
     */
    //TMCC_API int TMCC_CALL TMCC_CreateStreamHead(ref byte pHeadBuf, int iBufSize, tmStreamHeadInfo_t pStreamHead);
    /*
     * 函  数: TMCC_GetFileIndex
     * 说  明: 读取文件的索引，可以直接输入文件播放，此函数可能很费时
     * 参  数: pHeadBuf为索引存放缓冲,iBufSize缓冲长度
     * 返回值: 成功返回文件索引长度，失败返回<=0
     */
    //TMCC_API int TMCC_CALL TMCC_GetFileIndex(string lpszFileName, ref byte pHeadBuf, int iBufSize);
    /*
     *函  数: TMCC_PutInStream
     *说  明: 输入数据流播放
     *参  数: hStream为数据流播放句柄句柄,pStreamBuf为数据流缓冲,iStreamSize为数据流大小
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_PutInStream(System.IntPtr hStream, ref byte pStreamBuf, int iStreamSize, uint nData);
    ////////////////////////////////////////////////////////
    ///////////////////远程文件搜索/////////////////////////
    ////////////////////////////////////////////////////////
    /*
     *函  数: TMCC_FindFirst
     *说  明: 开始主动方式搜索服务器文件
     *参  数: hTmFile为文件服务器句柄，pFindCondition为搜索条件，lpFindFileData为输出文件信息
     *返回值: 成功返回文件搜索句柄，失败返回NULL
     */
    //TMCC_API System.IntPtr TMCC_CALL TMCC_FindFirstFile(System.IntPtr hTmCC, tmFindConditionCfg_t pFindCondition, tmFindFileCfg_t lpFindFileData);
    /*
     *函  数: TMCC_FindNextFile
     *说  明: 搜索下一个文件
     *参  数: hTmFile为文件服务器句柄，lpFindFileData为输出文件信息
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_FindNextFile(System.IntPtr hTmFile, tmFindFileCfg_t lpFindFileData);
    /*
     *函  数: TMCC_FindCloseFile
     *说  明: 关闭搜索
     *参  数: hTmFile为文件服务器句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_FindCloseFile(System.IntPtr hTmFile);
    /*
     *函  数: TMCC_DownloadFile
     *说  明: 下载服务器文件
     *参  数: hTmFile为文件服务器句柄, pPlayInfo为下载条件,lpSaveFileName为保存的本地文件,bCancel下载中停止指针
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_DownloadFile(System.IntPtr hTmCC, tmPlayConditionCfg_t pPlayInfo, string lpSaveFileName, ref bool bCancel, TMCC_DOWNLOADPROGRESS_CALLBACK pCallBack, object context);
    //TMCC_API int TMCC_CALL TMCC_DownloadFileA(System.IntPtr hTmCC, tmDownloadFileCfg_t pDownloadInfo, int iDownloadNum, ref bool bCancel, TMCC_DOWNLOADPROGRESS_CALLBACK pCallBack, object context);
    ////////////////////////////////////////////////////////
    //////////////////////文件播放//////////////////////////
    ////////////////////////////////////////////////////////
    /*
     *函  数: TMCC_OpenFile
     *说  明: 以指定条件打开服务器文件
     *参  数: hTmCC为文件服务器句柄,pPlayInfo为打开文件的条件,hPlayWnd为显示视频的窗口;
     *		  当hTmCC==NULL是为播放本地文件
     *返回值: 成功返回播放控制句柄,失败返回NULL
     */
    //TMCC_API System.IntPtr TMCC_CALL TMCC_OpenFile(System.IntPtr hTmCC, tmPlayConditionCfg_t pPlayInfo, System.IntPtr hPlayWnd);
    /*
     *函  数: TMCC_OpenRemoteFile
     *说  明: 以指定条件打开服务器文件
     *参  数: hTmCC为文件服务器句柄,pPlayInfo为打开文件的条件,hPlayWnd为显示视频的窗口;
     *返回值: 成功返回播放控制句柄,失败返回NULL
     */
    //TMCC_API int TMCC_CALL TMCC_OpenRemoteFile(System.IntPtr hTmCC, tmRemoteFileInfo_t pPlayInfo, System.IntPtr hPlayWnd);
    /*
     *函  数: TMCC_CloseFile
     *说  明: 关闭打开的服务器文件
     *参  数: hTmFile为文件服务器句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_CloseFile(System.IntPtr hTmFile);
    /*
     *函  数: TMCC_GetFileInfo
     *说  明: 读取文件信息
     *参  数: hTmFile为文件服务器句柄,pPlayInfo为文件信息缓冲
     *返回值: 成功返回读取的大小，失败返回小于0
     */
    //TMCC_API int TMCC_CALL TMCC_GetFileHeadInfo(System.IntPtr hTmFile, ref byte pHeadBuf, int iBufSize);
    /*
     *函  数: TMCC_ReadFile
     *说  明: 读取文件数据，可以任意读取，或读取完整一帧
     *参  数: hTmFile为文件服务器句柄,buf数据缓冲，iReadSize为读取大小
     *返回值: 成功返回读取的大小，失败返回小于0
     */
    //TMCC_API int TMCC_CALL TMCC_ReadFile(System.IntPtr hTmFile, ref byte buf, int iReadSize);
    /*
     *函  数: TMCC_ControlFile
     *说  明: 控制文件播放
     *参  数: hTmFile为文件服务器句柄,pPlayControl为文件控制参数信息
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_ControlFile(System.IntPtr hTmFile, tmPlayControlCfg_t pPlayControl);
    /*
     *函  数: TMCC_GetFilePlayState
     *说  明: 读取打开的文件位置
     *参  数: hTmFile为文件服务器句柄,tmTimeInfo_t为存放文件位置指针
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_GetFilePlayState(System.IntPtr hTmFile, tmPlayStateCfg_t pState);
    /*
     *函  数: TMCC_RegisterFileCallBack
     *说  明: 注册文件访问回调函数
     *参  数: pCallBack为回调函数指针，context为调用者自定义数据
     *返回值: 无
     */
    //TMCC_API void TMCC_CALL TMCC_RegisterFileCallBack(System.IntPtr hTmFile, tmFileAccessInterface_t pCallBack, object context);
    //////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////视频控制接口定义//////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////
    /*
     *函  数: TMCC_RegisterAVFrameCallBack
     *说  明: 注册解码视频输出回调
     *参  数: hTmCC为服务器句柄,pCallBack用户回调函数指针,context为用户自定义指针
     *返回值: 无
     */
    //TMCC_API void TMCC_CALL TMCC_RegisterAVFrameCallBack(System.IntPtr hTmCC, TMCC_AVFRAME_CALLBACK pCallBack, object context);
    /*
     *函  数: TMCC_RegisterDrawCallBack
     *说  明: 注册解码视频自会输出回调
     *参  数: hTmCC为服务器句柄,pCallBack用户回调函数指针,context为用户自定义指针
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API void TMCC_CALL TMCC_RegisterDrawCallBack(System.IntPtr hTmCC, TMCC_DDRAW_CALLBACK pCallBack, object context);
    /*
     *函  数: TMCC_SetVolume
     *说  明: 设置音频播放音量
     *参  数: hTmCC为控制句柄，iVolume为音量(-10000~0,-10000最小,0最大)
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SetVolume(System.IntPtr hTmCC, int iVolume);
    /*
     *函  数: TMCC_SetMute
     *说  明: 设置播放声音开关，只是本地解码后的声音
     *参  数: hTmCC为控制句柄，bMute为开关(FALSE打开声音，TRUE静音)
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SetMute(System.IntPtr hTmCC, bool bMute);
    /*
     *函  数: TMCC_SetDisplayShow
     *说  明: 设置解码视频是否显示，默认打开
     *参  数: hTmCC为控制句柄，bShow为开关(FALSE不显示，TRUE显示)
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SetDisplayShow(System.IntPtr hTmCC, bool bShow);
    /*
     *函  数: TMCC_SetImageOutFmt
     *说  明: 设置解码视频输出格式(默认是YUV420)
     *参  数: hTmCC为控制句柄，iOutFmt为输出格式
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SetImageOutFmt(System.IntPtr hTmCC, uint iOutFmt);
    /*
     *函  数: TMCC_GetDisplayRegion
     *说  明: 读取显示视频的显示位置
     *参  数: hTmCC为控制句柄，iRegionNum为显示号,pSrcRect为显示位置相对于原始图像,hDestWnd显示窗口,bEnable为是否显示
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_GetDisplayRegion(System.IntPtr hTmCC, int iRegionNum, RECT pSrcRect, System.IntPtr hDestWnd, ref bool bEnable);
    /*
     *函  数: TMCC_SetDisplayRegion
     *说  明: 读取显示视频的显示位置
     *参  数: hTmCC为控制句柄，iRegionNum为显示号,pSrcRect为显示位置相对于原始图像,hDestWnd显示窗口,bEnable为是否显示
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SetDisplayRegion(System.IntPtr hTmCC, int iRegionNum, RECT pSrcRect, System.IntPtr hDestWnd, bool bEnable);
    /*
     *函  数: TMCC_RefreshDisplay
     *说  明: 刷新当前显示
     *参  数: hTmCC为控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_RefreshDisplay(System.IntPtr hTmCC);
    /*
     *函  数: TMCC_ClearDisplay
     *说  明: 清空当前显示
     *参  数: hTmCC为控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_ClearDisplay(System.IntPtr hTmCC);
    /*
     *函  数: TMCC_GetImageSize
     *说  明: 读取视频大小
     *参  数: hTmCC为控制句柄，iWidth/iHeight为存放大小的指针
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_GetImageSize(System.IntPtr hTmCC, ref int iWidth, ref int iHeight);
    /*
     *函  数: TMCC_GetDisplayScale
     *说  明: 读取视频原始显示比例
     *参  数: hTmCC为控制句柄，iScale为存放显示比例的指针，注意此比例为宽/高*1000
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_GetDisplayScale(System.IntPtr hTmCC, ref int iScale);
    /*
     *函  数: TMCC_CapturePictureToFile
     *说  明: 当前显示视频抓图到指定文件
     *参  数: hTmCC为控制句柄,pFileName为存放的文件,pFmt为格式
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_CapturePictureToFile(System.IntPtr hTmCC, string pFileName, string pFmt);
    /*
     *函  数: TMCC_CapturePictureToBuffer
     *说  明: 当前显示视频抓图到指定缓冲
     *参  数: hTmCC为控制句柄,lpBuffer为存放图片缓冲,iBufferSize为缓冲大小(输入为缓冲大小，输出实际数据大小),pFmt为格式
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_CapturePictureToBuffer(System.IntPtr hTmCC, string pFmt, object lpBuffer, ref int iBufferSize);
    /*
     *函  数: TMCC_GetDiareekCapability
     *说  明: 获取支持图像透雾的能力
     *参  数: hTmCC为控制句柄,piMode为透雾能力，0-不支持，其它为支持的能力
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_GetDiareekCapability(System.IntPtr hTmCC, ref int piMode);
    /*
     *函  数: TMCC_SetDiareekMode
     *说  明: 设置是否打开图像透雾功能
     *参  数: hTmCC为控制句柄,iMode为透雾开关，支持0-关，1-开
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SetDiareekMode(System.IntPtr hTmCC, int iMode);
    /*
     *函  数: TMCC_SetImageProcessMode
     *说  明: 设置图像增强
     *参  数: hTmCC为控制句柄,iMode为增强模式，支持0和1
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SetImageProcessMode(System.IntPtr hTmCC, int iMode);
    /*
     *函  数: TMCC_OpenAvRender
     *说  明: 以指定条件打开音视频Render
     *参  数: pCfg为打开条件
     *返回值: 成功返回RENDER控制句柄,失败返回NULL
     */
    //TMCC_API int TMCC_CALL TMCC_OpenAvRender(System.IntPtr hTmCC, tmAvRenderConfig_t pCfg, System.IntPtr hPlayWnd);
    /*
     *函  数: TMCC_CloseAvRender
     *说  明: 关闭打开的Render
     *参  数: hTmFile为文件服务器句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_CloseAvRender(System.IntPtr hTmCC);
    /*
     *函  数: TMCC_PutinAvFrame
     *说  明: 输入音视频播放显示
     *参  数: hTmCC为控制句柄,pImage音视频帧
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_PutinAvFrame(System.IntPtr hTmCC, tmAvImageInfo_t pImage);
    //////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////设备状态和报警监听/////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////
    /*
     *函  数: TMCC_StartListen
     *说  明: 开启状态和报警监听，以及报警图片上传
     *参  数: hTmCC为控制句柄,iPort为监听端口
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_StartListen(System.IntPtr hTmCC, int iPort);
    /*
     *函  数: TMCC_StopListen
     *说  明: 关闭状态和报警监听，以及报警图片上传
     *参  数: hTmCC为控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_StopListen(System.IntPtr hTmCC);
    //////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////监听设备主动上传///////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////
    /*
     *函  数: TMCC_RegisterDeviceLoginCallBack
     *说  明: 注册设备主动登录输出回调
     *参  数: hTmCC为服务器句柄,pCallBack用户回调函数指针,context为用户自定义指针
     *返回值: 无
     */
    //TMCC_API void TMCC_CALL TMCC_RegisterDeviceLoginCallBack(System.IntPtr hTmCC, TMCC_DEVICELOGIN_CALLBACK pCallBack, object context);
    /*
     *函  数: TMCC_RegisterDeviceStreamCallBack
     *说  明: 注册设备数据上传输出回调
     *参  数: hTmCC为服务器句柄,pCallBack用户回调函数指针,context为用户自定义指针
     *返回值: 无
     */
    //TMCC_API void TMCC_CALL TMCC_RegisterDeviceStreamCallBack(System.IntPtr hTmCC, TMCC_DEVICESTREAM_CALLBACK pCallBack, object context);
    /*
     *函  数: TMCC_RegisterDeviceTalkCallBack
     *说  明: 注册设备主动语音对讲输出回调
     *参  数: hTmCC为服务器句柄,pCallBack用户回调函数指针,context为用户自定义指针
     *返回值: 无
     */
    //TMCC_API void TMCC_CALL TMCC_RegisterDeviceTalkCallBack(System.IntPtr hTmCC, TMCC_DEVICETALK_CALLBACK pCallBack, object context);
    /*
     *函  数: TMCC_StartListenDevice
     *说  明: 开启设备上传监听
     *参  数: hTmCC为控制句柄,iPort为监听端口
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_StartListenDevice(System.IntPtr hTmCC, int iPort);
    /*
     *函  数: TMCC_StopListenDevice
     *说  明: 关闭设备上传监听
     *参  数: hTmCC为控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_StopListenDevice(System.IntPtr hTmCC);
    //音视频解码
    /*
     *函  数: TMCC_OpenAvDecoder
     *说  明: 打开音视频解码
     *参  数: hTmCC为控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_OpenAvDecoder(System.IntPtr hTmCC, tmAvDecoderConfig_t pCfg);
    /*
     *函  数: TMCC_CloseAvDecoder
     *说  明: 关闭音视频解码
     *参  数: hTmCC为控制句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_CloseAvDecoder(System.IntPtr hTmCC);
    /*
     *函  数: TMCC_PutInAvDecoderData
     *说  明: 输入原始数据解码成图片YUV
     *参  数: hTmCC为控制句柄,pImageIn为原始数据, pImageOut为解码输出的数据, iGetFrame放回是否解码完成
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_PutInAvDecoderData(System.IntPtr hTmCC, tmAvImageInfo_t pImageIn, tmAvImageInfo_t pImageOut, ref int iGetFrame);
    /*
     *函  数: TMCC_SetMD5EncryptParameter
     *说  明: 设置MD5的加密矩阵信息
     *参  数: hTmCC控制句柄, byData为矩阵数据
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_SetMD5EncryptParameter(System.IntPtr hTmCC, byte byData, int iDataSize);
    /*
     *函  数: TMCC_GetMD5EncryptData
     *说  明: 根据设置的MD5矩阵信息，加密数据
     *参  数: hTmCC控制句柄, byUserData为需要加密的数据,iUserDataSize为数据长度,byMD5Data为机密后的数据缓冲必须大于16
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_GetMD5EncryptData(System.IntPtr hTmCC, byte byUserData, int iUserDataSize, ref byte byMD5Data);
    /*
     *函  数: TMCC_GetFaceDetectResult
     *说  明: 读取当前显示帧的人脸检测结果
     *参  数: hTmCC为数据流控制句柄, pResult为存放结果的tmFaceDetectInfo_t指针
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_GetFaceDetectResult(System.IntPtr hTmCC, tmFaceDetectInfo_t pResult);
    /*
     *函  数: TMCC_StartRecord
     *说  明: 开始录像
     *参  数: hStream为实时视频流句柄， pFileName为需要保存的文件名;  pFileType为文件类型: "avi"为AVI， "mp4"为MP4，"mkv"为MKV, 目前仅支持avi和mkv两种格式
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回负数
     */
    //TMCC_API int TMCC_CALL TMCC_StartRecord(System.IntPtr hStream, string pFileName, string pFileType);
    /*
     *函  数: TMCC_StopRecord
     *说  明: 停止录像
     *参  数: hStream为实时视频流句柄
     *返回值: 成功返回TMCC_ERR_SUCCESS，失败返回其它值
     */
    //TMCC_API int TMCC_CALL TMCC_StopRecord(System.IntPtr hStream);
}

/*AVI文件头修复库*/
public sealed class Avi
{
    // [DllImport("RepairFile.dll", EntryPoint = "?Repair_ConvertFile@@YGHPBD00H@Z", CallingConvention = CallingConvention.StdCall)]
    [DllImport("RepairFile.dll", EntryPoint = "?Repair_ConvertFile@@YAHPEBD00H@Z",
        CallingConvention = CallingConvention.StdCall)]
    internal static extern int Repair_ConvertFile(string lpszSrcFile,
        string lpszDstFile, string lpszType,
        int bForce);
}