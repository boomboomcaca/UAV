using System;
using System.Runtime.InteropServices;

namespace Magneto.Device.Nvt.SDK;

/***************************************************************************
                                tmControlClient.h
                             -------------------
    begin                :  2005.12.21
    copyright            : (C) 2005 by aip
 ***************************************************************************/
/***************************************************************************
 *  该文件为AIP视频服务器客户端配置服务器的接口定义文件					   *
 ***************************************************************************/
/// <summary>
///     连接用户信息结构定义
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TmConnectInfoT
{
    /// <summary>
    ///     该结构的大小，必须填写
    /// </summary>
    public uint DwSize;

    /// <summary>
    ///     连接服务器的IP地址
    /// </summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string PIp;

    /// <summary>
    ///     服务器连接的端口
    /// </summary>
    public int Port;

    /// <summary>
    ///     登录用户名
    /// </summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string SzUser;

    /// <summary>
    ///     登录用户口令
    /// </summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string SzPass;

    /// <summary>
    ///     登录用户级别，主要用户DVS的一些互斥访问资源
    /// </summary>
    public int UserLevel;

    /// <summary>
    ///     用户自定义数据
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = DefineConstants.UserContextSize)]
    public byte[] PUserContext;
}

/// <summary>
///     服务器命令包，输入命令以及命令具体内容
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TmCommandInfoT
{
    /// <summary>
    ///     该结构的大小，必须填写为sizeof(tmCommandInfo_t)
    /// </summary>
    public uint DwSize;

    /// <summary>
    ///     主消息数据命令即数据类型
    /// </summary>
    public uint DwMajorCommand;

    /// <summary>
    ///     次消息数据命令即数据类型
    /// </summary>
    public uint DwMinorCommand;

    /// <summary>
    ///     通道号，该通道号要根据dwMajorCommand来判断是否有效
    /// </summary>
    public ushort Channel;

    /// <summary>
    ///     子通道号，该通道号要根据dwMajorCommand来判断是否有效
    /// </summary>
    public ushort Stream;

    /// <summary>
    ///     消息数据缓冲大小
    /// </summary>
    public int CommandBufferLen;

    /// <summary>
    ///     消息数据实际大小
    /// </summary>
    public int CommandDataLen;

    /// <summary>
    ///     消息控制返回结果
    /// </summary>
    public uint DwResult;

    /// <summary>
    ///     消息数据缓冲
    /// </summary>
    public object PCommandBuffer;
}

/// <summary>
///     服务器系统升级进度信息
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TmProgressInfoT
{
    /// <summary>
    ///     接口控制句柄
    /// </summary>
    public IntPtr HTmcc;

    /// <summary>
    ///     模块ID号
    /// </summary>
    public uint DwModalId;

    /// <summary>
    ///     模块数据大小
    /// </summary>
    public uint DwModalSize;

    /// <summary>
    ///     数据传输当前位置
    /// </summary>
    public uint DwModalPos;
}

/// <summary>
///     服务器自动升级配置结构
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TmUpgradeConfigT
{
    /// <summary>
    ///     本结构大小
    /// </summary>
    public uint DwSize;

    /// <summary>
    ///     服务器监听端口，设备将主动连接此端口
    /// </summary>
    public int ListenPort;

    /// <summary>
    ///     服务器列表数，当为0时请求网络所有机器升级
    /// </summary>
    public int ServerListCount;

    /// <summary>
    ///     需要升级的设备列表，由服务器信息组成列表
    /// </summary>
    public TmServerInfoExT PServerInfoList;

    /// <summary>
    ///     升级的包类型0-升级文件,1-IE升级包,2-内核升级包
    /// </summary>
    public int FileType;

    /// <summary>
    ///     升级的文件包，或IE升级目录(260为windows api定义最长文件名)
    /// </summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
    public string SzFileName;

    /// <summary>
    ///     升级完成后自动重启
    /// </summary>
    public bool BAutoReboot;
}

/// <summary>
///     服务器自动升级信息结构
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TmUpgradeInfoT
{
    /// <summary>
    ///     本结构大小
    /// </summary>
    public uint DwSize;

    /// <summary>
    ///     设备信息
    /// </summary>
    public TmServerInfoExT PServerInfo;

    /// <summary>
    ///     模块升级代码,请参考摄像机自动升级输出代码定义说明
    /// </summary>
    public uint DwResultCode;

    /// <summary>
    ///     升级进度0-100
    /// </summary>
    public int UpgradeProgress;
}

/// <summary>
///     捕获数据结构定义
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TmAvRenderConfigT
{
    //音频
    public uint DwSize;

    /// <summary>
    ///     是否带视频
    /// </summary>
    public bool BVideo;

    /// <summary>
    ///     视频宽
    /// </summary>
    public int Width;

    /// <summary>
    ///     视频高
    /// </summary>
    public int Height;

    /// <summary>
    ///     帧率*1000
    /// </summary>
    public int FrameRate;

    /// <summary>
    ///     是否带音频
    /// </summary>
    public bool BAudio;

    //视频
    /// <summary>
    ///     音频采样位数
    /// </summary>
    public byte ByBitsPerSample;

    /// <summary>
    ///     音频的声道数
    /// </summary>
    public byte ByChannels;

    /// <summary>
    ///     音频采样率
    /// </summary>
    public int SamplesPerSec;
}

/// <summary>
///     解码数据结构定义
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TmAvDecoderConfigT
{
    //音频
    public uint DwSize;

    /// <summary>
    ///     是否解码单关键帧，FALSE为解码数据流
    /// </summary>
    public bool BSingleFrame;

    /// <summary>
    ///     是否带视频
    /// </summary>
    public bool BVideo;

    /// <summary>
    ///     流类型Tag
    /// </summary>
    public uint DwVideoTag;

    /// <summary>
    ///     流ID
    /// </summary>
    public uint DwVideoId;

    /// <summary>
    ///     视频宽
    /// </summary>
    public int Width;

    /// <summary>
    ///     视频高
    /// </summary>
    public int Height;

    /// <summary>
    ///     是否带音频
    /// </summary>
    public bool BAudio;

    /// <summary>
    ///     音频流类型Tag
    /// </summary>
    public uint DwAudioTag;

    /// <summary>
    ///     音频流ID
    /// </summary>
    public uint DwAudioId;

    //视频
    /// <summary>
    ///     音频采样位数
    /// </summary>
    public byte ByBitsPerSample;

    /// <summary>
    ///     音频的声道数
    /// </summary>
    public byte ByChannels;

    /// <summary>
    ///     音频采样率
    /// </summary>
    public int SamplesPerSec;
}

/// <summary>
///     语音对讲数据头
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TmVoiceHeadT
{
    public uint DwSize;

    /// <summary>
    ///     音频编码格式0-MP2,1-AAC,2-S16LE,3-MULAW/G.711,4-ALAW/G.711,5-G.722,6-G.723,7-G.726,8-G.729
    /// </summary>
    public byte ByCompressFormat;

    /// <summary>
    ///     保留
    /// </summary>
    public byte ByTemp;

    /// <summary>
    ///     音频采样位数
    /// </summary>
    public byte ByBitsPerSample;

    /// <summary>
    ///     音频的声道数
    /// </summary>
    public byte ByChannels;

    /// <summary>
    ///     音频采样率
    /// </summary>
    public int SamplesPerSec;

    /// <summary>
    ///     音频码流大小
    /// </summary>
    public int AudioBitRate;
}

/// <summary>
///     捕获数据结构定义
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TmVoiceDataT
{
    /// <summary>
    ///     音频数据缓冲大小
    /// </summary>
    public int DataSize;

    /// <summary>
    ///     音频数据类型0是头，1是数据包
    /// </summary>
    public byte ByDataType;

    /// <summary>
    ///     保留
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public byte[] ByTemp;

    /// <summary>
    ///     音频数据缓冲，没有包头，一包完整数据
    /// </summary>
    public object PData;

    /// <summary>
    ///     本结构大小
    /// </summary>
    public uint DwSize;

    /// <summary>
    ///     音频包时间大小，毫秒
    /// </summary>
    public uint DwTimeStamp;
}

/// <summary>
///     数据流头信息
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TmStreamHeadInfoT
{
    public uint DwSize;

    /// <summary>
    ///     是否带视频
    /// </summary>
    public bool BVideo;

    /// <summary>
    ///     流类型Tag
    /// </summary>
    public uint DwVideoTag;

    /// <summary>
    ///     流ID
    /// </summary>
    public uint DwVideoId;

    /// <summary>
    ///     视频宽
    /// </summary>
    public int Width;

    /// <summary>
    ///     视频高
    /// </summary>
    public int Height;

    /// <summary>
    ///     显示比例*1000
    /// </summary>
    public int DisplayScale;

    /// <summary>
    ///     帧率*1000
    /// </summary>
    public int FrameRate;

    /// <summary>
    ///     此数据流的码流大小
    /// </summary>
    public int VideoBitRate;

    /// <summary>
    ///     视频是Interlacer
    /// </summary>
    public bool BInterlacer;

    /// <summary>
    ///     是否采集音频
    /// </summary>
    public bool BAudio;

    /// <summary>
    ///     音频流类型Tag
    /// </summary>
    public uint DwAudioTag;

    /// <summary>
    ///     音频流ID
    /// </summary>
    public uint DwAudioId;

    //音频
    /// <summary>
    ///     音频采样位数
    /// </summary>
    public int BitsPerSample;

    //视频
    /// <summary>
    ///     音频的声道数
    /// </summary>
    public int Channels;

    /// <summary>
    ///     音频采样率
    /// </summary>
    public int SamplesPerSec;

    /// <summary>
    ///     此音频流的码流大小
    /// </summary>
    public int AudioBitRate;

    /// <summary>
    ///     一个音频包包含多少帧
    /// </summary>
    public uint DwSampleSize;
}

/// <summary>
///     设备主动登录信息结构定义
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TmDeviceLoginInfoT
{
    public uint DwSize;
    public uint DwDeviceIp;
    public uint DwFactoryId;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string SDeviceId;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 48)]
    public string SDeviceSn;
}

/// <summary>
///     设备主动数据上传信息结构定义
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TmDeviceStreamInfoT
{
    /// <summary>
    ///     本结构大小
    /// </summary>
    public uint DwSize;

    /// <summary>
    ///     设备地址
    /// </summary>
    public uint DwDeviceIp;

    /// <summary>
    ///     设备的厂商号
    /// </summary>
    public uint DwFactoryId;

    /// <summary>
    ///     设备登录ID号
    /// </summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string SDeviceId;

    /// <summary>
    ///     设备唯一序列号
    /// </summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 48)]
    public string SDeviceSn;

    /// <summary>
    ///     上传的通道
    /// </summary>
    public byte ByChannel;

    /// <summary>
    ///     上传的码流
    /// </summary>
    public byte ByStream;
}

/// <summary>
///     设备主动语音对讲信息结构定义
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TmDeviceTalkInfoT
{
    public uint DwSize;
    public uint DwDeviceIp;
    public uint DwFactoryId;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string SDeviceId;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 48)]
    public string SDeviceSn;
}

//摄像机自动升级输出代码说明定义
//初始化类型
/*
 * 接口函数TMCC_PutInStream的参数nData定义
 */
/* 操作回调函数定义 */
//调式回调函数，只有在开发模式下才有效
//ORIGINAL LINE: typedef void(CALLBACK* TMCC_LOG_CALLBACK)(System.IntPtr hTmCC, uint code, const char* info, object* context);
public delegate void TmccLogCallback(IntPtr hTmCc, uint code, string info, object context);

//连接消息回调函数，通过它可以得到异步方式和断开连接状态
//ORIGINAL LINE: typedef bool(CALLBACK* TMCC_CONNECT_CALLBACK)(System.IntPtr hTmCC, bool bConnect, uint dwResult, object* context);
public delegate bool TmccConnectCallback(IntPtr hTmCc, bool bConnect, uint dwResult, object context);

//消息读取回调函数，通过它可以得到服务器消息，同时异步方式的服务器数据获得也是通过该回调
//ORIGINAL LINE: typedef int(CALLBACK* TMCC_DATAREAD_CALLBACK)(System.IntPtr hTmcc, tmCommandInfo_t* pCmd, object* context);
public delegate int TmccDatareadCallback(IntPtr hTmcc, TmCommandInfoT pCmd, object context);

//服务器列举回调函数定义
//ORIGINAL LINE: typedef bool(CALLBACK* TMCC_ENUMSERVER_CALLBACK)(struct tmServerInfo_t* pEnum, object* context);
public delegate bool TmccEnumserverCallback(TmServerInfoT pEnum, object context);

//服务器连接回调函数，通过它可以得到服务器登陆，或者服务器无连接的报警信息
//ORIGINAL LINE: typedef bool(CALLBACK* TMCC_SERVERINFO_CALLBACK)(tmCommandInfo_t* pCmd, object* context);
public delegate bool TmccServerinfoCallback(TmCommandInfoT pCmd, object context);

//服务器模块升级进度回调函数
//ORIGINAL LINE: typedef bool(CALLBACK* TMCC_PROGRESS_CALLBACK)(tmProgressInfo_t* pInfo, object* context);
public delegate bool TmccProgressCallback(TmProgressInfoT[] pInfo, object context);

//通明通道数据回调函数
//ORIGINAL LINE: typedef int(CALLBACK* TMCC_SERIALDATA_CALLBACK)(System.IntPtr hTmCC, char *pRecvDataBuffer, int iBufSize, object* context);
public delegate int TmccSerialdataCallback(IntPtr hTmCc, ref string pRecvDataBuffer, int iBufSize, object context);

//add by stone 20070930
//连接信息枚举回调定义
//ORIGINAL LINE: typedef int(CALLBACK* TMCC_ENUMCONNECT_CALLBACK)(System.IntPtr hTmCC, int iChannel, tmConnectCfg_t* pInfo, int iInfoSize, object* context);
public delegate int TmccEnumconnectCallback(IntPtr hTmCc, int iChannel, TmConnectCfgT[] pInfo, int iInfoSize,
    object context);

//服务器自动升级信息输出回调
//ORIGINAL LINE: typedef int(CALLBACK* TMCC_UPGRADE_CALLBACK)(System.IntPtr hTmCC, tmUpgradeInfo_t* pInfo, object* context);
public delegate int TmccUpgradeCallback(IntPtr hTmCc, TmUpgradeInfoT[] pInfo, object context);

//数据流回调
//ORIGINAL LINE: typedef int(CALLBACK* TMCC_STREAMREAD_CALLBACK)(System.IntPtr hTmCC, tmRealStreamInfo_t* pStreamInfo, object* context);
public delegate int TmccStreamreadCallback(IntPtr hTmCc, TmRealStreamInfoT pStreamInfo, object context);

//视频显示回调
//ORIGINAL LINE: typedef void(CALLBACK* TMCC_DDRAW_CALLBACK)(System.IntPtr hTmCC, System.IntPtr hDC, RECT* lpRect, int iRegionNum, object* context);
public delegate void TmccDdrawCallback(IntPtr hTmCc, IntPtr hDc, Rect lpRect, int iRegionNum, object context);

//解码帧输出回调
//ORIGINAL LINE: typedef int(CALLBACK* TMCC_AVFRAME_CALLBACK)(System.IntPtr hTmCC, tmAvImageInfo_t* pImage, object* context);
public delegate int TmccAvframeCallback(IntPtr hTmCc, TmAvImageInfoT pImage, object context);

//文件下载进度回调
//ORIGINAL LINE: typedef bool(CALLBACK* TMCC_DOWNLOADPROGRESS_CALLBACK)(System.IntPtr hTmCC, tmPlayStateCfg_t* pDownloadState, object* context);
public delegate bool TmccDownloadprogressCallback(IntPtr hTmCc, TmPlayStateCfgT pDownloadState, object context);

//对讲音频数据输出回调
//ORIGINAL LINE: typedef int(CALLBACK* TMCC_VOICEDATA_CALLBACK)(System.IntPtr hTmCC, tmVoiceData_t* pVoiceData, object* context);
public delegate int TmccVoicedataCallback(IntPtr hTmCc, TmVoiceDataT pVoiceData, object context);

//设备主动登录回调，注意hNewTmCC为新生成的句柄
//ORIGINAL LINE: typedef int(CALLBACK* TMCC_DEVICELOGIN_CALLBACK)(System.IntPtr hTmCC, System.IntPtr hNewTmCC, tmDeviceLoginInfo_t* pDeviceLogin, object* context);
public delegate int TmccDeviceloginCallback(IntPtr hTmCc, IntPtr hNewTmCc, TmDeviceLoginInfoT pDeviceLogin,
    object context);

//设备主动数据连接回调，注意hNewTmCC为新生成的句柄
//ORIGINAL LINE: typedef int(CALLBACK* TMCC_DEVICESTREAM_CALLBACK)(System.IntPtr hTmCC, System.IntPtr hNewTmCC, tmDeviceStreamInfo_t* pDeviceStream, object* context);
public delegate int TmccDevicestreamCallback(IntPtr hTmCc, IntPtr hNewTmCc, TmDeviceStreamInfoT pDeviceStream,
    object context);

//设备主动语音连接回调，注意hNewTmCC为新生成的句柄
//ORIGINAL LINE: typedef int(CALLBACK* TMCC_DEVICETALK_CALLBACK)(System.IntPtr hTmCC, System.IntPtr hNewTmCC, tmDeviceTalkInfo_t* pDeviceTalk, object* context);
public delegate int TmccDevicetalkCallback(IntPtr hTmCc, IntPtr hNewTmCc, TmDeviceTalkInfoT pDeviceTalk,
    object context);

//RTP包输出回调数据流回调
//ORIGINAL LINE: typedef int(CALLBACK* TMCC_RTPSTREAMREAD_CALLBACK)(System.IntPtr hTmCC, tmRealStreamInfo_t* pStreamInfo, object* context);
public delegate int TmccRtpstreamreadCallback(IntPtr hTmCc, TmRealStreamInfoT pStreamInfo, object context);