using System;
using System.Runtime.InteropServices;

namespace Magneto.Device.Nvt.SDK;

/***************************************************************************
tmTransDefine.h
-------------------
begin                :  2005.12.21
copyright            : (C) 2005 by aipstar
***************************************************************************/
/***************************************************************************
 *  该文件为AS视频服务器，视频解码器客户端配置服务器结构定义文件		   *
 ***************************************************************************/
/*修改日志
 *2007-12-18
 *2009-05-19 添加设备的心跳包，这个心跳包支持我们的设备和所有模块
 *2010-04-44 在视频参数配置结构<tmPicPreviewCfg_t/tmPicModeScheduleInfo_t>中锐度变量
 *2011-11-11 支持超D1 960x576
 */
/* 以下宏定义不能随便修改 */
/* 网络接口定义 */
/*设备型号(DVS类型)*/
/* 用户信息定义，单个用户信息 */
public class TmUserInfoT
{
    public byte[] ByMacAddr = new byte[6]; // 允许访问的远程物理地址(为0时表示允许任何地址)
    public uint DwRemoteRightHi; // 权限
    public uint DwRemoteRightLo; // 权限
    public uint DwSize; // 该结构大小，可能由于版本不同而变化
    public uint DwUserIp; // 允许访问的远程IP地址(为0时表示允许任何地址)
    public string SzMotifyPassword = new(new char[32]); // 密码
    public string SzPassword = new(new char[32]); // 密码
    public string SzUserName = new(new char[32]); // 用户名
    public ushort WRemoteLevel; // 级别
    public ushort WRemoteLoginNum; // 允许同时登录数，0-为限制登录
}

/* 用户列表，列举用户时使用 */
public class TmUserT
{
    public uint DwSize; //该结构大小
    public TmUserInfoT[] StruUserInfo = Arrays.InitializeWithDefaultInstances<TmUserInfoT>(16); //用户信息数组
    public int UserCount; //可用用户个数，必须小于MAX_USERNUM
}

/* 用户加密数据TMCC_MINOR_CMD_GETENCRYPTDATA */
public class TmUserEncryptMd5CfgT
{
    public byte[] ByMd5Data = new byte[16]; //加密后的数据16个字节
    public byte[] ByUserData = new byte[128]; //需要加密的源数据
    public uint DwSize;
    public uint DwUserDataLen; //需要加密的数据长度
}

/*DVS设备参数*/
public class TmDeviceCfgT
{
    public byte ByAlarmInPortNum; //DVS报警输入个数, MAX_ALARMIN-byAlarmInPortNum为网络报警个数
    public byte ByAlarmOutPortNum; //DVS报警输出个数
    public byte ByChanNum; //DVS 通道个数
    public byte ByDecodeChs; //DVS 解码路数
    public byte ByDiskCtrlNum; //DVS 硬盘控制器个数
    public byte ByDiskNum; //DVS 硬盘个数
    public byte ByDvsType; //DVS类型,
    public byte ByFactory; //厂商代码，视频服务器的厂商ID,AIP公司定义
    public byte ByNetworkPortNum; //网络口个数
    public byte ByRs232Num; //DVS 232串口个数
    public byte ByRs485Num; //DVS 485串口个数
    public byte ByStartChan; //起始通道号,例如DVS-1,DVS - 1
    public byte ByUsbNum; //USB口的个数
    public byte ByVgaNum; //VGA口的个数
    public uint DwDspSoftwareBuildDate; //DSP软件生成日期,0xYYYYMMDD
    public uint DwDspSoftwareVersion; //DSP软件版本,高16位是主版本,低16位是次版本
    public uint DwDvsId; //DVS ID,用于遥控器
    public uint DwHardwareVersion; //硬件版本,高16位是主版本,低16位是次版本
    public uint DwMicrophone; //是否启用语音对讲, 0不启用,其它启用
    public uint DwModeType; //设备类型,比如A5DDR2 A5DDR3 S2 等,参见MODAL_ONLY_FLAG_A5
    public uint DwPanelVersion; //前面板版本,高16位是主版本,低16位是次版本
    public uint DwRecycleRecord; //是否循环录像,0:不是; 1:是
    public uint DwSize;
    public uint DwSoftwareBuildDate; //软件生成日期,0xYYYYMMDD
    public uint DwSoftwareVersion; //软件版本号,高16位是主版本,低16位是次版本
    public uint DwVideoStandard; //视频制式0:PAL 1:NTSC
    public string SDvsName = new(new char[32]); //DVS名称

    public string SDvsType = new(new char[16]); //设备类型名称

    /*以下不能更改*/
    public string SzSerialNumber = new(new char[48]); //序列号
    public ushort WDeviceExternType; //设备扩展类型：第一位表示是否支持红外，第二位表示是否支持双路报警输入(0x0001 -- 支持红外; 0x0002 -- 支持双路报警)
}

/*DVS设备扩展参数*/
public class TmDeviceExCfgT
{
    public byte By232TransDataCom; //完全透明传输串口0-没有，1-串口1
    public byte By232TransDataSubComId; //完全透明传输子串口ID
    public byte By511ACascNum; //511A级联数，默认1级
    public byte By511AVersion; //511A串口单片机版本信息,0-自动，1-强制老版本，2-强制新版本
    public byte ByAlarmOutTurnIn; //报警输出修改成报警输入(M38模组支持此功能)
    public byte ByConnect511AMode; //系统是否连接串口透传设备511A,0-没有连接，1-连接到系统debug串口，2-连接到默认485口
    public byte ByDisableTmccCfg; //是否禁止第三方协议(tmcc)配置参数, 0-否，1-是
    public byte ByEnableTemperature; //是否启用热像测温
    public byte ByEnableViscaCom; //启用单独的Visca串口，机型不一样串口可能不一样, 0-没有启用，1-连接到系统debug串口1，2-连接到默认485口2, 3-串口3
    public byte ByIrLightMode; //红外灯光控制模式0-默认有主板配置控制，1-主板IO控制, 2-内部激光控制, 3-云台辅助开关控制, 4-其它
    public byte ByLaserComMode; //系统控制激光的串口，0-没有，1-连接到系统debug串口1，2-连接到默认485口2, 3-串口3
    public byte ByLaserComSubId; //激光子串口ID
    public byte ByLaserRangefinder; //激光测距仪类型
    public byte ByLaserRangingCom; //激光测距串口0-没有，1-串口1
    public byte ByLaserRangingComSubId; //激光测距子串口ID
    public byte ByLaserReverse; //激光器是否反向，有些厂家笔筒型号激光器控制方法反的，这里需要标志
    public byte ByLaserType; //激光类型，0-奥利激光，1-3千米激光
    public byte ByRtpExtension; //是否启用rtp扩展数据，0-不启用，1-华为协议，2-人脸检测数据
    public byte[] ByTemp = new byte[8]; //保留
    public byte ByThermalCtrlCom; //热成像镜头控制串口0-没有，1-串口1
    public byte ByThermalCtrlComSubId; //热成像镜头控制子串口ID
    public byte ByThermalLensCom; //热成像FV数据传输串口0-没有，1-串口1
    public byte ByThermalLensComSubId; //热成像FV子串口ID
    public byte ByThermalLensType; //热像探测器类型，1-core41h
    public byte ByTransProtocol; //选用的协议0-默认内置协议,1-ONVIF协议,2-国标协议,3-互信互通协议
    public byte ByViscaComSubId; //Visca子串口ID
    public uint DwSize; //本结构大小
}

/* 网络接口参数 */
public class TmEthernetT
{
    public byte ByClientConnectType; //要求客户端的连接模式
    public byte ByEnable; //是否启用:0永远启用，1根据配置
    public byte ByInitNetInterface; //当前网络类型
    public byte[] ByMacAddr = new byte[6]; //服务器的物理地址
    public byte[] ByTemp = new byte[3];
    public uint DwNetInterface; //网络接口 1-10MBase-T 2-10MBase-T全双工 3-100MBase-TX 4-100M全双工 5-10M/100M自适应
    public string SDvsip = new(new char[25]); //服务器IP地址
    public string SDvsipMask = new(new char[25]); //服务器IP地址掩码
    public ushort WDhcp; //1启用动态IP获取,0指定IP
    public ushort WDvsPort; //端口号
    public ushort WHttpPort; //Http服务器端口号
}

/* DDNS服务器信息参数 */
public class TmDdnsT
{
    public uint DwSize;
    public string SDdnsName = new(new char[32]); //DDNS名称
    public string SDdnsPassword = new(new char[32]); //DDNS密码
    public string SDdnsServerName = new(new char[32]); //DDNS服务器名称
    public string SDdnsServerName2 = new(new char[32]); //备用DDNS服务器名称
    public string SDdnsUser = new(new char[32]); //DDNS用户名
}

/* 网络配置结构 */
public class TmNetCfgT
{
    public byte ByEnableNfs; //是否启用NFS，启用后摄像机将向指定目录写文件
    public byte ByMultiCastIpMode; //多播地址组成模式:0-使用设置的地址, 1-地址xxx.aaa.aaa.aaa(其中aaa与本机地址一致,xxx使用设定的)
    public byte ByMultiCastPortMode; //多播端口组成模式:0-各码流端口相同,1-各码流端口递增，多个通道也根据通道数递增
    public byte ByMultiCastTemp;
    public byte[] ByTemp = new byte[12]; //保留

    public uint DwDdns; //0-不启用,1-启用，如果是局域网登录，登录DDNS的地址为网关地址

    /*RTP是否启用0-不启用，1-启用*/
    public uint DwEnableRtsp;
    public uint DwManageHost; //远程管理主0-不启用,1-启用
    public uint DwMultiCastPort; //多播组端口，第2码流+1,第3码流+2,第4码流+3
    public uint DwPppoe; //0-不启用,1-启用
    public uint DwSize;
    public uint DwSyncTimeFromManageHost; //从管理中心同步时间
    public string SDnsip = new(new char[25]); //DNS服务器地址

    public string SDnsip2 = new(new char[25]); //备用DNS服务器地址

    /*RTSP从码流的访问标识，默认为bs3*/
    public string SFourthStream = new(new char[32]);

    public string SGatewayIp = new(new char[25]); //网关地址

    /*RTSP主码流的访问标识，默认为bs0*/
    public string SMajorStream = new(new char[32]);
    public string SManaeHostPassword = new(new char[32]); //登陆管理中心密码
    public string SManaeHostUser = new(new char[32]); //登陆管理中心用户名

    public string SManageHostIp = new(new char[26]); //远程管理主机地址

    /*RTSP从码流的访问标识，默认为bs1*/
    public string SMinorStream = new(new char[32]);
    public string SMultiCastIp = new(new char[18]); //多播组地址
    public string SNfsDirectory = new(new char[130]); //NAS目录
    public string SNfsip = new(new char[25]); //NAS主机IP地址
    public string SPpPoEip = new(new char[16]); //PPPoE IP地址(只读)
    public string SPpPoEPassword = new(new char[32]); //PPPoE密码

    public string SPpPoEUser = new(new char[32]); //PPPoE用户名

    /*RTSP主码流的访问标识，默认为bs2*/
    public string SThirdStream = new(new char[32]);
    public TmDdnsT StruDdns = new(); //DDNS
    public TmEthernetT[] StruEtherNet = Arrays.InitializeWithDefaultInstances<TmEthernetT>(2); // 以太网口

    public ushort WManageHostPort; //远程管理主机端口号

    /*RTP/RTSP传输模式0-VLC兼容模式,1-QT兼容模式*/
    public ushort WRtspMode;

    /*RTP/RTSP监听端口*/
    public ushort WRtspPort;
}

/*TMCC_MINOR_CMD_STREAMSENDSTATUS*/
public class TmCheckStreamSendStatusCfgT
{
    public byte ByCheckTime; //单位秒最小为5秒 最大4分钟，应该是够了
    public byte ByEnable; //是否启用，当有视频传输时，立即上报正常，当出现没有发送流时，每2秒上报一次
    public byte ByHandleWith; //0:不处理;1:重启;2: 存储
    public byte ByModuleId; //0:主程序 1:CGI 2:tmcc 3:RTSP STREAM_STAUTS_MODULE_SDK
    public byte[] ByTemp = new byte[4]; //保留
    public uint DwSize;
}

public class TmRtspMutiCastStreamCfgT
{
    public byte ByEnableAudio; //使能音频组播发送
    public byte ByEnableVideo; //使能视频组播发送
    public byte[] ByReserve = new byte[2];
    public byte[] ByReserve2 = new byte[8];
    public uint DwSize;
    public string SMultiCastIpAduio = new(new char[20]); //音频多播组地址
    public string SMultiCastIpVideo = new(new char[20]); //视频多播组地址
    public ushort WMultiCastPortAudio; //音频组播端口
    public ushort WMultiCastPortVideo; //视频组播端口
}

/*RTSP组播发送配置参数*/
public class TmRtspMutiCastCfgT //TMCC_MINOR_CMD_RTSPMUTICASTCFG
{
    public byte ByCurStreamNum;
    public byte[] ByReserve = new byte[3];
    public uint DwSize;
    public TmRtspMutiCastStreamCfgT[] RtspMuticast = Arrays.InitializeWithDefaultInstances<TmRtspMutiCastStreamCfgT>(8);
}

/* 无线网络配置结构 */
public class TmWifiCfgT
{
    public byte ByDhcp; //无线地址方式,1启用动态IP获取,0指定IP
    public byte ByEnable; //无线是否启用
    public byte ByKeyFormat; //密码格式0-字符串,1-16数
    public byte ByKeyIndex; //密码索引
    public byte ByKeyMgmt; //网络身份验证0-开放,1-共享,2-WPA,3-WPA-PSK,4-WPA2,5-WPA2-PSK
    public byte ByKeyType; //数据加密0-禁用，1-WEP，2-TKIP,3-AES
    public byte[] ByMacAddr = new byte[6]; //WifiMac地址
    public byte ByScanSsid; //即使没有广播也连接
    public byte[] ByTemp = new byte[3];
    public uint DwSize; //本结构大小
    public uint DwWpaPtkRekey; //更换密钥时间
    public string SDnsip1 = new(new char[16]); //WifiDNS1地址
    public string SDnsip2 = new(new char[16]); //WifiDNS2地址
    public string SGatewayIp = new(new char[16]); //Wifi网关地址
    public string SIpAddr = new(new char[16]); //Wifi地址
    public string SIpMaskAddr = new(new char[16]); //Wifi地址掩码
    public string SKey = new(new char[48]); //密钥限制32
    public string SSsid = new(new char[48]); //服务器的SSID限制32
}

/*2010-09-39 add by stone*/
public class TmDdnsCfgT
{
    public byte ByDdnsEnable;
    public byte[] ByTemp = new byte[3];
    public uint DwSize;
    public string SDdnsName = new(new char[32]); //DDNS名称
    public string SDdnsPassword = new(new char[32]); //DDNS密码
    public string SDdnsServerName = new(new char[32]); //DDNS服务器名称
    public string SDdnsServerName2 = new(new char[32]); //备用DDNS服务器名称
    public string SDdnsUser = new(new char[32]); //DDNS用户名
}

/*2007/11/29 frankxia add for Simpe Network Time Protocol Server*/
public class TmNtpCfgT
{
    /*实际的NTP服务器个数 1<=dwCount<= NTPSERVER_MAX*/
    public uint DwCount;

    /*正在使用的服务器索引,0<=dwIdx<MAX_NTP_SERVERS*/
    public uint DwIdx;

    public uint DwSize;

    /*最大允许的NTP服务器的个数*/
    public char[][] StrNtpServer =
        RectangularArrays.RectangularCharArray(DefineConstants.MaxNtpServers, DefineConstants.PathNameLen);
}

public class TmNtpCfgExT
{
    /*是否启用*/
    public uint DwEnableNtp;

    /*同步时间间隔(单位:分钟)*/
    public uint DwNtpTime;

    public uint DwSize;

    /*最大允许的NTP服务器的个数*/
    public string StrNtpServer = new(new char[128]);
}

/*2007/12/26 frankxia add for ftp file recorder*/
/*FTP服务器设置，定义了*/
public class TmFtpCfgT
{
    /*是否启用指定目录存放*/
    public byte ByEnableDirectory;

    /*录像格式，暂时未用*/
    public byte ByRecordFormat;

    /*FTP录像码流ID号*/
    public byte ByRecordStreamId;

    /*是否启用FTP服务器*/
    public byte ByUseFtp;

    /*记录文件的时间*/
    public uint DwFtpRecordFileSize;

    /*服务器的端口，ftp默认21*/
    public uint DwServerPort;

    /*本结构大小*/
    public uint DwSize;

    /*指定的存放目录*/
    public string StrDirectoryName = new(new char[128]);

    /*FTP服务器用户名*/
    public string StrFtpCliUserName = new(new char[16]);

    /*FTP服务器用户密码*/
    public string StrFtpCliUserPass = new(new char[16]);

    /*允许最大128字节*/
    public string StrFtpServerIpAddr = new(new char[128]);
}

/*2007/12/26 frankxia add for sendmail*/
public class TmSmtpCfgT
{
    /*字节对其保留*/
    public byte[] ByReserves = new byte[3];

    /*是否启用邮件服务器*/
    public byte ByUseSmtp;

    public uint DwSize;

    /*邮件服务器用户名*/
    public string StrSmtpCliUserName = new(new char[16]);

    /*邮件服务器用户口令*/
    public string StrSmtpCliUserPass = new(new char[16]);

    /*邮件服务器地址*/
    public string StrSmtpServerIpAddr = new(new char[128]); //允许最大128字节
}

/*2016-11-25 zzt add for RTMP*/
public class TmRtmpCfgT
{
    /*是否启用RTMP协议*/
    public byte ByEnable;

    /*端口*/
    public uint DwPort;

    public uint DwSize;

    /*保留*/
    public int[] Reverse = new int[4];

    /*密码*/
    public string SzPass = new(new char[32]);

    /*服务器IP*/
    public string SzServerIp = new(new char[64]);

    /*url标识*/
    public string SzUrl = new(new char[64]);

    /*用户名*/
    public string SzUser = new(new char[32]);

    /*设备UUID*/
    public string SzUuid = new(new char[128]);
}

/*设备心跳配置定义*/
public class TmLiveHeartCfgT
{
    public byte ByEnableLiveHeart; //是否启用心跳
    public byte ByEnableRecord; //是否启用断网自动录像
    public byte ByLiveHeartMode; //心跳包发送模式0-UDP,1-TCP/IP
    public byte[] ByTem = new byte[3];
    public uint DwServerPort; //远程管理主机端口号
    public uint DwSize; //该结构的大小，必须填写
    public string SServerAddress = new(new char[32]); //远程管理主机地址
    public ushort WLiveTime; //发送心跳间隔时间(单位:秒)
}

/*设备数据流主动上传服务器信息*/
public class TmStreamUpToServerCfgT
{
    public byte ByEnable; //是否启用自动上传
    public byte ByTemp; //保留
    public ushort DwSize; //该结构的大小，必须填写
    public string SDeviceNumber = new(new char[32]); //设备序列号
    public string SServerAddress = new(new char[64]); //远程管理主机地址
    public ushort WControlPort; //远程管理主机端口号
    public ushort WStreamPort; //远程管理主机端口号
    public ushort WTalkPort; //远程管理主机端口号
    public ushort WTemp; //保留
}

/* 时间配置，从视服中得到时间，设置时给系统时间 */
public class TmTimeCfgT
{
    public byte[] ByTemp = new byte[2];
    public int DayLightTime; //夏令时时间偏差，秒
    public uint DwSize;
    public ushort WDay; //日
    public ushort WDayOfWeek; //日
    public ushort WHour; //时
    public ushort WMinute; //分
    public ushort WMonth; //月
    public ushort WSecond; //秒
    public short WTimeZone; //时区
    public ushort WYear; //年
    public ushort WZoneIndex; //时区名称索引,先由wTimeZone确定时区，再由wZoneIndex确定是哪个地区，解决相同时区不同地区的问题
}

/*时间定义*/
public class TmTimeInfoT
{
    public byte ByDay; //日
    public byte ByHour; //时
    public byte ByMinute; //分
    public byte ByMonth; //月
    public byte BySecond; //秒
    public byte ByTemp; //保留
    public uint DwMicroSecond; //豪秒
    public ushort WYear; //年
}

/* 时间范围 */
public class TmSchedTimeT
{
    //C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
    //	union
    //	{
    //		uint dwSize;
    //		uint u32Data;
    //	};
    /*开始时间*/
    public byte ByStartHour;

    public byte ByStartMin;

    /*结束时间*/
    public byte ByStopHour;
    public byte ByStopMin;
}

/* 报警处理方式*/
public class TmHandleExceptionT
{
    public byte[] ByRelAlarmOut = new byte[4]; //报警触发的输出通道,报警触发的输出,为1表示触发该输出
    public byte[] ByRelAlarmOutEnable = new byte[4]; // 标明报警输出有效byRelAlarmOutEnable[0]为12则报警输出0-3
    public uint DwHandleType; //处理方式,处理方式的"或"结果
}

/* 报警处理动作 */
public class TmTransFerT
{
    public byte ByChannel; // 通道号0-MAX_CHANNUM
    public byte ByCruiseNo; // 巡航
    public byte ByEnableCaptureChan; // 报警触发的抓图通道，为1表示触发该通道
    public byte ByEnableCruise; // 是否调用巡航
    public byte ByEnablePreset; // 是否调用预置点
    public byte ByEnablePtzTrack; // 是否调用轨迹
    public byte ByEnableRelRecordChan; // 报警触发的录象通道,为1表示触发该通道
    public byte ByPresetNo; // 调用的云台预置点序号,一个报警输入可以调用多个通道的云台预置点
    public byte ByPtzTrack; // 调用的云台的轨迹序号

    public byte[] ByTemp = new byte[2]; // 保留

    /*取消结构大小定义*/
    public byte ByTransFerType; // 处理联动类型12为tmTransFer_t为通道数组0-16, 13为通道由byChannel决定
}

/* 区域偏移定义*/
public class TmAreaOffsetT
{
    public short WBottomOff; // 区域的下边偏移
    public short WLeftOff; // 区域的左边偏移
    public short WRightOff; // 区域的右边偏移
    public short WTopOff; // 区域的上边偏移
}

/* 通道图象结构, 区域定义，该区域PAL-704*576, NTSC-704x480 */
public class TmAreaScopeT
{
    public ushort WAreaHeight; // 区域的高
    public ushort WAreaTopLeftX; // 区域的x坐标
    public ushort WAreaTopLeftY; // 区域的y坐标
    public ushort WAreaWidth; // 区域的宽
}

/* 移动侦测 */
public class TmMotionT
{
    /*置越小越灵敏*/
    public byte ByEnableHandleMotion; //是否处理移动侦测
    public byte ByMotionScopeNum; //侦测区域个数，必须小于或等于5
    public byte ByMotionSensitive; //移动侦测灵敏度, 0 - 5,越高越灵敏
    public byte ByMotionThreshold; //移动检测阀值，为设置移动区域的宏块总数百分比，0-100，当为0时只要有一个宏块移动就算移动
    public uint DwSize;
    public TmHandleExceptionT StrMotionHandleType = new(); //处理方式
    public TmSchedTimeT[][] StruAlarmTime = RectangularArrays.RectangularTmSchedTime_tArray(7, 4); //布防时间
    public TmTransFerT[] StruAlarmTransFer = Arrays.InitializeWithDefaultInstances<TmTransFerT>(16); //触发通道
    public TmAreaScopeT[] StruMotionScope = Arrays.InitializeWithDefaultInstances<TmAreaScopeT>(5); //侦测区域704*576
}

/* 移动侦测 */
public class TmVideoMotionCfgT
{
    public byte ByEnableHandleMotion; //是否处理移动侦测
    public byte ByMotionScopeNum; //侦测区域个数，必须小于或等于5
    public byte ByMotionSensitive; //移动侦测灵敏度, 0 - 5,越高越灵敏
    public byte ByMotionThreshold; //置越小越灵敏
    public uint DwHandleMinTime; //处理报警的最小时间，单位毫秒(在此时间内有对此报警只处理一次)
    public uint DwSize;
    public TmHandleExceptionT StrMotionHandleType = new(); //处理方式
    public TmSchedTimeT[][] StruAlarmTime = RectangularArrays.RectangularTmSchedTime_tArray(7, 4); //布防时间
    public TmTransFerT[] StruAlarmTransFer = Arrays.InitializeWithDefaultInstances<TmTransFerT>(16); //触发通道
    public TmAreaScopeT[] StruMotionScope = Arrays.InitializeWithDefaultInstances<TmAreaScopeT>(5); //侦测区域704*576
}

/*双镜头图像合成配置*/
public class TmVideoFusionCfgT
{
    /*处理图像合成配置*/
    public byte ByFusionLimit;

    /*主码流合成视频模式,0-不合成，1-vin1在vin0上显示,2-合成图像在vin0上显示*/
    public byte ByFusionViewMode;
    public byte[] ByTemp = new byte[3];

    public byte[] ByViewTemp = new byte[3];

    /*动态图像合成限制*/
    public byte ByYtjFusionLimit;
    public byte[] ByYtjTemp = new byte[3];
    public uint DwSize;
    public TmAreaScopeT StruRcView = new();
    public TmAreaOffsetT[] StruRcVin = Arrays.InitializeWithDefaultInstances<TmAreaOffsetT>(8);
    public TmAreaOffsetT[] StruRcVinTele = Arrays.InitializeWithDefaultInstances<TmAreaOffsetT>(8);
    public TmAreaOffsetT[] StruRcVinWide = Arrays.InitializeWithDefaultInstances<TmAreaOffsetT>(8);
}

/*add by zzt: 测温区域扩展,支持异形区域*/
/*顶点坐标信息*/
public class TmPointT
{
    public int X;
    public int Y;
}

public class TmSizeT
{
    public int Cx;
    public int Cy;
}

public class TmRectT
{
    //C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
    //	union
    //	{
    //		int iLeft;
    //		int left;
    //	};
    //C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
    //	union
    //	{
    //		int iTop;
    //		int top;
    //	};
    //C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
    //	union
    //	{
    //		int iRight;
    //		int right;
    //	};
    //C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
    //	union
    //	{
    //		int iBottom;
    //		int bottom;
    //	};
}

//测温区域定义
public class TmFlirAreaScopeT
{
    public byte ByAlarm;
    public byte ByAlarmDelay; //时域差温报警持续时间,单位s
    public byte ByCheckTempMode; // 检查温度模式，0-检查平均温度，1-检查最低温度，2-检查最高温度， 0xFF-不检测
    public byte ByOutTempEnable; //超温报警使能
    public byte ByPointNum; //用到顶点个数
    public byte ByUserData; // 用户设定的数据，在tmFlirTempInfo_t对应名称
    public byte ByViewType; //名称显示方式，0-显示szName, 1-显示byUserData
    public ushort Color; //区域边框颜色
    public TmPointT[] StPointList = Arrays.InitializeWithDefaultInstances<TmPointT>(DefineConstants.MaxPointNum);
    public string SzName = new(new char[16]); // 名称标识
    public byte[] SzTemp = new byte[15]; //保留
    public short WMaxTemperature; // 报警最高摄氏温度*10
    public short WMinTemperature; // 报警最低摄氏温度*10
    public ushort WSpatDiffAlarmThresh; //空域差温报警阀值*10, 0无效
    public ushort WTimeDiffAlarmThresh; //时域差温报警阀值*10, 0无效
}

/*热像测温黑体信息配置*/
public class TmFlirBlackBodyCfgT
{
    public byte ByAlarmThresh; //报警阈值(实际测试黑体的温度与设置的黑体温度相差超过该阈值，则认为黑体被遮挡或无效)
    public byte[] ByReverse = new byte[3]; //保留
    public uint DwSize;
    public uint DwTemp; //黑体温度 *100
    public TmRectT StRect = new(); //黑体坐标位置(参考704*576)
}

/*热像在可见光中的视场偏移*/
public class TmFlirOffsetCfgT
{
    public uint DwSize;
    public TmRectT StRect = new(); //偏移的矩形坐标(参考704*576)
}

/*测温系统的工作模式*/
public class TmFlirWorkModeCfgT
{
    public byte ByEnableFaceCheck; //启用人脸测温
    public byte ByEnableTempLimit; //使用温度限制，超过限制范围视为非法温度，不拉入统计
    public byte[] ByTemp = new byte[2];
    public int DwMaxTemp; //测温的最高温度*10
    public int DwMinTemp; //测温的最低温度*10
    public uint DwSize;
    public int WorkMode; //0-普通模式1--人体测测温模式3--使用指定范围
}

//测温全屏定义
public class TmFlirFullScopeT
{
    public byte ByAlarm;
    public byte ByAlarmDelay; //差温报警持续时间,单位s
    public byte ByCheckTempMode; // 检查温度模式，0-检查平均温度，1-检查最低温度，2-检查最高温度， 0xFF-不检测
    public byte ByDiffAlarmThresh; //差温报警阀值*10
    public byte ByOutTempEnable; //超温报警使能
    public byte ByUserData; // 用户设定的数据，在tmFlirTempInfo_t对应名称
    public short WMaxTemperature; // 全屏幕最高摄氏温度*10
    public ushort WMaxTempX; // 最高温度的x坐标
    public ushort WMaxTempY; // 最高温度的y坐标
    public short WMinTemperature; // 全屏幕最低摄氏温度*10
    public ushort WMinTempX; // 最低温度的x坐标
    public ushort WMinTempY; // 最低温度的y坐标
}

//测温点定义
public class TmFlirSpoltScopeT
{
    public byte ByAlarm;
    public byte ByAlarmDelay; //时域差温报警持续时间,单位s
    public byte ByCheckTempMode; // 检查温度模式，0-检查平均温度，1-检查最低温度，2-检查最高温度， 0xFF-不检测
    public byte ByOutTempEnable; //超温报警使能
    public byte[] ByTemp = new byte[12]; //保留， 和先前的结构定义大小保持一致
    public byte ByUserData;
    public byte ByViewType; //名称显示方式，0-显示szName, 1-显示byUserData
    public uint DwSize;
    public string SzName = new(new char[16]); // 名称标识
    public short WMaxTemperature; // 报警最高摄氏温度*10
    public short WMinTemperature; // 报警最低摄氏温度*10
    public ushort WSpatDiffAlarmThresh; //空域差温报警阀值*10, 0无效
    public ushort WSpoltX; //单点测温X坐标
    public ushort WSpoltY; //单点测温Y坐标
    public short WTemperature; //当前点的摄氏温度*10，只读
    public ushort WTimeDiffAlarmThresh; //时域差温报警阀值*10 0无效
}

public class TmFlirLineT
{
    public byte ByAlarm;
    public byte ByAlarmDelay; //时域差温报警持续时间,单位s
    public byte ByCheckTempMode; // 检查温度模式，0-检查平均温度，1-检查最低温度，2-检查最高温度， 0xFF-不检测
    public byte ByOutTempEnable; //超温报警使能
    public byte ByUserData;
    public byte ByViewType; //名称显示方式，0-显示szName, 1-显示byUserData
    public uint DwSize;
    public TmPointT StStartPt = new();
    public TmPointT StStopPt = new();
    public string SzName = new(new char[16]); // 名称标识
    public byte[] SzTemp = new byte[14]; //保留 和先前的结构定义大小保持一致
    public short WMaxTemperature; // 报警最高摄氏温度*10
    public short WMinTemperature; // 报警最低摄氏温度*10
    public ushort WSpatDiffAlarmThresh; //空域差温报警阀值*10, 0无效
    public ushort WTimeDiffAlarmThresh; //时域差温报警阀值*10，0无效
}

/* 热成像视频区域测温 */
public class TmVideoFlirAreaCfgT
{
    public byte ByCheckMode; //测温类型:按位取: 第一位:区域测温，第二位:-全屏测温，第三位:-点测温 第四位:-线测温
    public byte ByEnablePreset; //是否关联到预置点
    public byte ByLineNum;
    public byte ByPointNum;
    public byte ByPresetNo; //预置点ID -1 ,1-MAX_PRESET, 0-为默认
    public byte ByScopeNum; //侦测区域个数，必须小于或等于MAX_FLIRAREANUM
    public byte[] ByTemp = new byte[2];
    public uint DwSize;
    public TmFlirFullScopeT StruFullScope = new(); //全屏信息
    public TmFlirLineT[] StruLine = Arrays.InitializeWithDefaultInstances<TmFlirLineT>(DefineConstants.MaxFlirLineNum);

    public TmFlirAreaScopeT[] StruScope =
        Arrays.InitializeWithDefaultInstances<TmFlirAreaScopeT>(DefineConstants.MaxFlirAreaNum); //区域信息

    public TmFlirSpoltScopeT[] StruSpoltScope =
        Arrays.InitializeWithDefaultInstances<TmFlirSpoltScopeT>(DefineConstants.MaxFlirPointNum); //单点信息
}

//临时测温
public class TmVideoFlirAreaCfgTempT
{
    public byte ByCheckMode;
    public byte ByTimeOut;
    public uint DwSize;
    public TmFlirFullScopeT StruFullScope = new();
    public TmFlirLineT StruLine = new(); //侦测区域个数，必须小于或等于MAX_FLIRAREANUM
    public TmFlirAreaScopeT StruScope = new(); //区域信息
    public TmFlirSpoltScopeT StruSpoltScope = new(); //单点信息
    public byte[] SzTemp = new byte[8];
}

/*测温源*/
public class TmFlirSourceT
{
    public byte ById; //测温区域的索引
    public byte ByPresetNo; //预置点ID -1 ,1-MAX_PRESET, 0-为默认
    public byte[] ByTemp = new byte[4];
    public byte ByType; //测温源的类型:0--区域测温,1--线测温 3--点测温
    public byte ByUserData;
}

/*测温区域分组信息*/
public class TmFlirAreaGroupInfoT
{
    public byte ByAlarmDelay; //差温报警检测间隔时间,单位s
    public byte ByCheckMode; //检测报警类型 0-检查平均温度，1-检查最低温度，2-检查最高温度
    public byte ByEnable; //是否启用
    public byte ByOutTempEnable; //超温报警使能
    public byte[] ByReserve = new byte[6]; //保留字段
    public byte BySourceNum; //测温源数量
    public byte ByTemp;
    public uint DwSize;

    public TmFlirSourceT[] StSource =
        Arrays.InitializeWithDefaultInstances<TmFlirSourceT>(DefineConstants.MaxFlirGrpSrcNum); //测温源

    public string SzName = new(new char[32]); //组名称
    public short WMaxTemperature; // 超温报警最高摄氏温度*10
    public short WMinTemperature; // 超温报警最低摄氏温度*10
    public ushort WSpatDiffAlarmThresh; //空域差温报警阀值*10, 0 无效
    public ushort WTimeDiffAlarmThresh; //时域差温报警阀值*10, 0 无效
}

public class TmFlirAreaGroupCfgT
{
    public uint DwSize;

    public TmFlirAreaGroupInfoT[] StGroupInfo =
        Arrays.InitializeWithDefaultInstances<TmFlirAreaGroupInfoT>(DefineConstants.MaxFlirGroupNum);
}

/* 热成像视频点测温 */
public class TmVideoFlirSpoltCfgT
{
    public byte ByEnableSpolt;
    public byte ByTemp;
    public uint DwSize;
    public ushort WSpoltX; //单点测温X坐标
    public ushort WSpoltY; //单点测温Y坐标
    public short WTemperature; //当前点的摄氏温度*10，只读
}

/* 热成像视频区域测温 */
public class TmVideoFlirCfgT
{
    public byte ByAvgTempLevel; //平均温测温级别
    public byte ByCheckShutterMode; //切换挡板模式*， 0-默认，1-不切换， 2-定时切换
    public byte ByCheckTempMode; //检查温度模式，0-检查平均温度，1-检查最低温度，2-检查最高温度
    public byte ByClearAlarmCaptureTime; //前端人脸测温报警抓图清空时间，单位分钟， 0-不清空，1-
    public byte ByDrawColor; //在可见光上显示信息, 0-否， 1-只显示视场框， 2-只显示测温区域，3-全部显示
    public byte ByEnableAlarmThreshold; //显示区域的报警阀值
    public byte ByEnableAvgTemp; //是否启用平均温测温，0-否， 1-是
    public byte ByEnableCount; //是否启用人数统计，0-否， 1-是
    public byte ByEnableHandleFlir; //是否处理测温布防报警, 0-不布防, 1-按时间布防, 2-全天布防
    public byte ByEnablePresetFlir; //是否启用预置点热像测温配置参数
    public byte ByEnableResetFlirScope; //是否启用区域检查参数回复设置
    public byte ByFahrenheit; //温度类型，0-摄氏度， 1-华氏度， 2-开尔文
    public byte ByOutTempEnable; //是否启用超温报警，区域中没有想用配置使用此配置
    public byte ByRefreshThreshold; //温度刷新阀值判断0-255, 值越大处理越慢
    public byte ByResetFlirScopeTime; //预置点调用后延时检查时间，主要体现在预置点到位的时间(单位分钟)
    public byte BySampleRefreshTime; //样本刷新时间，单位分钟
    public string ByTemp = new(new char[1]);
    public byte ByVarietyThreshold; //温度变化阀值判断0-255, 值越大处理越慢
    public byte ByViewFlirInfo; //显示测温信息
    public byte ByViewInfoType; //温度信息显示类型，0-默认，1-只显示高温，2-只显示低温, 3-全部显示
    public uint DwAutoFocusTempOffset; //自动聚焦的温度偏差(摄氏温度*10)
    public uint DwCheckDelayTime; //设置后延时检查时间，主要体现在预置点到位的时间(单位秒)
    public uint DwHandleMinTime; //处理报警的最小时间，单位毫秒(在此时间内有对此报警只处理一次)
    public uint DwMaxSample; //平均温测温样本最大数量
    public ushort DwMinSample; //平均温测温样本最小数量
    public uint DwSize;
    public ushort DwUptoCenterTime; //主动上传服务器的间隔时间，单位秒, 0不上传
    public TmAreaOffsetT StCheckOffset = new(); //温度数据四周数据很可能不正常，现在需要排除周围一圈
    public TmAreaOffsetT StFaceOffset = new(); //人脸测温区域偏移， 按百分比计算
    public TmHandleExceptionT StrHandleType = new(); //处理方式
    public TmSchedTimeT[][] StruAlarmTime = RectangularArrays.RectangularTmSchedTime_tArray(7, 4); //布防时间
    public TmTransFerT[] StruAlarmTransFer = Arrays.InitializeWithDefaultInstances<TmTransFerT>(16); //触发通道
    public int TempOffset; //温度偏差(摄氏温度*10)
    public short WAlarmMaxTemperature; //高温(摄氏温度*10)报警值
    public short WAlarmMinTemperature; //低温(摄氏温度*10)报警值
    public ushort WCheckShutterTime; //定时切换挡板时间，单位秒，56-65536
    public ushort WTempStreamInterval; //传输原始温度数据的时间间隔:毫秒(1-65536), 0-实时上传
}

/* 热成像视频区域温度信息 */
public class TmFlirTempInfoT
{
    public byte ByAlarm; //0-无报警 第1位为1表示低温报警 第2位表示超温报警,第3位表示差温报警
    public byte ByUserData; //用户定义数据
    public string SzName = new(new char[32]); //名称标识
    public ushort WAreaHeight; // 区域的高
    public ushort WAreaTopLeftX; // 区域的x坐标
    public ushort WAreaTopLeftY; // 区域的y坐标
    public ushort WAreaWidth; // 区域的宽
    public short WAverageTemperature; //当前点的平均摄氏温度*10，只读
    public short WMaxTemperature; //当前点的最高摄氏温度*10，只读
    public ushort WMaxTempX; // 最高温度的x坐标
    public ushort WMaxTempY; // 最高温度的y坐标
    public short WMinTemperature; //当前点的最低摄氏温度*10，只读
    public ushort WMinTempX; // 最低温度的x坐标
    public ushort WMinTempY; // 最低温度的y坐标
}

/* 扩展,支持异形区域, 热成像视频区域温度信息 */
public class TmFlirTempInfoExT
{
    public byte ByAlarm; //0-无报警 第1位为1表示低温报警 第2位表示超温报警,第3位表示差温报警
    public byte ByUserData; //用户定义数据
    public int PointNum; // 顶点数量

    public TmPointT[]
        StPointList = Arrays.InitializeWithDefaultInstances<TmPointT>(DefineConstants.MaxPointNum); // 区域坐标

    public string SzName = new(new char[32]); // 名称标识
    public short WAverageTemperature; //当前点的平均摄氏温度*10，只读
    public short WMaxTemperature; //当前点的最高摄氏温度*10，只读
    public ushort WMaxTempX; // 最高温度的x坐标
    public ushort WMaxTempY; // 最高温度的y坐标
    public short WMinTemperature; //当前点的最低摄氏温度*10，只读
    public ushort WMinTempX; // 最低温度的x坐标
    public ushort WMinTempY; // 最低温度的y坐标
}

/*热成像上报温度信息定义*/
public class TmToManagerFlirInfoT
{
    public byte ByCheckMode; //测温类型:按位取: 第一位:区域测温，第二位:-全屏测温，第三位:-点测温 第四位:-线测温, 第五位:人脸测温
    public byte ByEnablePreset; //是否关联到预置点
    public byte ByFaceNum;
    public byte ByLineNum;
    public byte[] ByMacAddr = new byte[6]; //远程物理地址
    public byte ByPointNum;
    public byte ByPresetNo; //预置点ID,0-MAX_PRESET-1
    public byte ByScopeNum; //侦测区域个数，必须小于或等于MAX_FLIRAREANUM

    public byte ByTemp;

    /*本结构定义*/
    public uint DwSize;

    public TmFlirTempInfoT[] StruFace =
        Arrays.InitializeWithDefaultInstances<TmFlirTempInfoT>(DefineConstants.MaxFlirFaceNum); //人脸温度信息

    public TmFlirTempInfoT StruFullScope = new(); //全屏信息

    public TmFlirTempInfoT[] StruLine =
        Arrays.InitializeWithDefaultInstances<TmFlirTempInfoT>(DefineConstants.MaxFlirLineNum);

    public TmFlirTempInfoExT[] StruScope =
        Arrays.InitializeWithDefaultInstances<TmFlirTempInfoExT>(DefineConstants.MaxFlirAreaNum); //区域信息

    public TmFlirTempInfoT[] StruSpoltScope =
        Arrays.InitializeWithDefaultInstances<TmFlirTempInfoT>(DefineConstants.MaxFlirPointNum); //单点信息

    public TmTimeInfoT StruTime = new(); //摄像机时间
    public string SzDeviceName = new(new char[32]); //DVS名称
    public string SzPresetName = new(new char[128]); //预置点名称
    public byte[] SzSerialNumber = new byte[48]; //服务器序列号
    public string SzServerIp = new(new char[16]); //服务器地址
    public short WMaxTemperature; //高温(摄氏温度*10)报警值
    public short WMinTemperature; //低温(摄氏温度*10)报警值
}

/*扩展,支持异形区域*/
/*测温分组上报信息*/
public class TmToManagerFlirGroupInfoT
{
    public byte ByAlarmType; //报警类型:0--超温报警; 1--时域差温报警; 2--空域差温报警
    public byte ByGroupId; //组号
    public byte[] ByMacAddr = new byte[6]; //远程物理地址

    public byte[] ByTemp = new byte[2];

    /*本结构定义*/
    public uint DwSize;
    public TmTimeInfoT StruTime = new(); //摄像机时间
    public TmFlirSourceT StSource = new(); //报警源
    public string SzDeviceName = new(new char[32]); //DVS名称
    public byte[] SzSerialNumber = new byte[48]; //服务器序列号
    public string SzServerIp = new(new char[16]); //服务器地址
}

public class TmFlirAlarmTempInfoT
{
    public short WAverageTemperature; //当前点的平均摄氏温度*10，只读
    public short WMaxTemperature; //当前点的最高摄氏温度*10，只读
    public short WMinTemperature; //当前点的最低摄氏温度*10，只读
}

/*测温区域报警上报信息*/
public class TmToManagerFlirAlarmInfoT
{
    public byte ByAlarmType; //0-低温报警， 1-高温报警, 2 -差温报警
    public byte ByCheckTempMode; // 检查温度模式，0-检查平均温度，1-检查最低温度，2-检查最高温度， 0xFF-不检测
    public byte ByEnablePreset; //是否关联到预置点
    public byte ByPresetNo; //报警是设置的预置点号0～MAX_PRESET-1
    public string ByTemp = new(new char[4]);
    public byte ByUserData;
    public uint DwSize;
    public TmTimeInfoT StruTime = new(); //摄像机时间

    public TmFlirAlarmTempInfoT[]
        StTempInfo = Arrays.InitializeWithDefaultInstances<TmFlirAlarmTempInfoT>(2); //0-当前温度，1-之前温度

    public ushort WDiffAlarmThresh; //差温报警阀值
    public short WMaxTemper; // 报警最高摄氏温度*10
    public short WMinTemper; // 报警最低摄氏温度*10
}

/*测温分组报警上报信息*/
public class TmToManagerFlirGroupAlarmInfoT
{
    public byte ByAlarmType; //报警类型:0--超温报警; 1--时域差温报警; 2--空域差温报警
    public byte ByCheckTempMode; // 检查温度模式，0-检查平均温度，1-检查最低温度，2-检查最高温度， 0xFF-不检测
    public byte ByGroupId; //组号
    public string ByTemp = new(new char[4]);
    public uint DwSize;
    public TmTimeInfoT StruTime = new(); //摄像机时间
    public TmFlirSourceT[] StSource = Arrays.InitializeWithDefaultInstances<TmFlirSourceT>(2); //报警源

    public TmFlirAlarmTempInfoT[]
        StTempInfo = Arrays.InitializeWithDefaultInstances<TmFlirAlarmTempInfoT>(2); //时域报警:0-当前温度，1-之前温度；空域报警:与报警源一一对应

    public short WMaxTemper; // 报警最高摄氏温度*10
    public short WMinTemper; // 报警最低摄氏温度*10
    public ushort WSpatDiffAlarmThresh; //空域差温报警阀值
    public ushort WTimeDiffAlarmThresh; //时域差温报警阀值
}

/*rgb参数配置 TMCC_MAJOR_CMD_VIDEOINCFG/TMCC_MINOR_CMD_RGBCFG */
public class TmRgbCfgT
{
    public byte ByDrawCaption; // 是否显示rgb文字信息
    public byte[] ByTemp = new byte[6];
    public byte ByViewInfoType; // rgb信息显示方式， 0-默认，1-白框，2-边框，3-填充
    public uint DwSize;
    public uint DwUptoCenterTime; // 上传中心间隔时间，秒
}

/*rgb区域配置 TMCC_MAJOR_CMD_VIDEOINCFG/TMCC_MINOR_CMD_RGBAREACFG */
public class TmRgbAreaCfgT
{
    public uint DwSize;
    public int RectNum;

    public TmRectT[]
        StRect = Arrays.InitializeWithDefaultInstances<TmRectT>(DefineConstants.MaxRgbNum); //坐标位置(参考704*576)
}

/* 遮挡报警区域为704*576 */
public class TmHideAlarmT
{
    public byte ByEnableHandleHideAlarm; // 是否处理信号丢失报警
    public byte ByHideSensitive; // 遮挡灵敏度 ,0 - 5,越高越灵敏
    public uint DwSize;
    public TmHandleExceptionT StrHideAlarmHandleType = new(); // 处理方式
    public TmSchedTimeT[][] StruAlarmTime = RectangularArrays.RectangularTmSchedTime_tArray(7, 4); // 布防时间
    public TmAreaScopeT StruHideAlarmArea = new(); // 遮挡区域
}

/* 遮挡报警区域扩展为704*576 */
public class TmVideoHideCfgT
{
    public byte ByEnableHandleHideAlarm; // 是否处理信号丢失报警
    public byte ByHideScopeNum; //遮挡区域个数，必须小于或等于5
    public byte ByHideSensitive; // 遮挡灵敏度 ,0 - 5,越高越灵敏
    public byte[] ByTemp = new byte[1];
    public uint DwHandleMinTime; // 处理报警的最小时间，单位毫秒(在此时间内有对此报警只处理一次)
    public uint DwSize;
    public TmHandleExceptionT StrHideAlarmHandleType = new(); // 处理方式
    public TmSchedTimeT[][] StruAlarmTime = RectangularArrays.RectangularTmSchedTime_tArray(7, 4); // 布防时间
    public TmAreaScopeT StruHideAlarmArea = new(); // 遮挡区域

    public TmAreaScopeT[]
        StruHideAlarmAreaEx = Arrays.InitializeWithDefaultInstances<TmAreaScopeT>(4); // 报警区域扩展，为了程序兼容只能新增加4个到最后,总共5个
}

/* 信号丢失报警 */
public class TmViLostT
{
    public byte ByEnableHandleViLost; // 是否处理信号丢失报警
    public uint DwSize;
    public TmSchedTimeT[][] StruAlarmTime = RectangularArrays.RectangularTmSchedTime_tArray(7, 4); // 布防时间
    public TmHandleExceptionT StrViLostHandleType = new(); // 处理方式
}

/* 信号丢失报警扩展 */
public class TmVideoLostCfgT
{
    public byte ByEnableHandle; // 是否处理信号丢失报警
    public byte[] ByTemp = new byte[3];
    public uint DwSize;
    public TmHandleExceptionT StrHandleType = new(); // 处理方式
    public TmSchedTimeT[][] StruAlarmTime = RectangularArrays.RectangularTmSchedTime_tArray(7, 4); // 布防时间
}

/* 视频遮挡配置 */
public class TmVideoMaskCfgT
{
    public byte ByEnableMask; // 是否启动遮挡 ,0-否,1-是 区域为704*576
    public byte[] ByTemp = new byte[3];
    public uint DwSize;
    public TmAreaScopeT[] StruHideArea = Arrays.InitializeWithDefaultInstances<TmAreaScopeT>(4); //遮挡区域
}

/* 图像预览参数 */
public class TmPicPreviewCfgT
{
    public short ByAcutance; //锐度,0-255
    public byte ByAcutanceA; //锐度A,0-255
    public short ByBrightness; //亮度,0-255
    public short ByContrast; //对比度,0-255
    public byte ByEnableAcutanceA; //是否启用0-不单独使用，根据byAcutance设置，1-单独使用
    public byte ByGamma; //伽玛,0-255
    public short ByHue; //色调,0-255
    public short BySaturation; //饱和度,0-255
    public uint DwSize;
    public byte DwVideoFormat; //只读：视频制式 0-PAL 1-NTSC
}

/* 图像预览参数 */
public class TmPicPreviewInfoT
{
    /*亮度,0-255*/
    public byte ByBrightness;

    /*对比度,0-255*/
    public byte ByContrast;

    /*色调,0-255*/
    public byte ByHue;

    /*饱和度,0-255 */
    public byte BySaturation;
}

/*图像模式预设计划*/
public class TagPicModeScheduleInfoT
{
    /*扩展的锐度(0-255)*/
    public byte ByAcutance;

    /*是否启用标志: 0-不启用; 1-启用*/
    public byte ByEnable;

    /*保留*/
    public byte ByGamma;

    /*视频模式索引: 0-白天模式; 1-强光模式; 2-傍晚模式; 3-夜间模式*/
    public byte ByMode;

    /*时间段信息，开始时*/
    public byte ByStartHour;
    public byte ByStartMin;
    public byte ByStopHour;
    public byte ByStopMin;
}

public class TagPicModeScheduleCfgT
{
    public uint DwSize;

    /*图像模式:0-白天模式; 1-强光模式; 2-傍晚模式; 3-夜间模式*/
    public TmPicPreviewInfoT[] StruMode = Arrays.InitializeWithDefaultInstances<TmPicPreviewInfoT>(4);

    /*索引0，1，2.3分别代表四个时间段*/
    public TagPicModeScheduleInfoT[] StruModeSched = Arrays.InitializeWithDefaultInstances<TagPicModeScheduleInfoT>(4);
}

/*　图像采集偏移微调定义 */
public class TmVideoOffsetCfgT
{
    public uint DwSize;
    public short OffsetBottom;
    public short OffsetLeft;
    public short OffsetRight;
    public short OffsetTop;
}

/* 图像OSD参数配置 */
public class TmVideoOsdCfgT
{
    public byte ByAlignMode; // 字符串中如果带\n将隔行显示，头顶对齐方式,0-右对齐，1-中间，2-左对齐，0xFF-自动

    /* 0: 半透明 */
    /* 1: 不透明 */
    /* 当最高位为1时，低7位为透明度0~127*/
    public byte ByFontMode; // OSD的字体模式通道名大小
    public byte ByFontMode1; // OSD的字体模式时间

    public byte ByFontMode2; // OSD的字体模式扩展字符

    /* 0-相对左上角，1-相对右上角，2-相对左下角，3-相对右下角*/
    public byte ByFontMode4; // OSD的字体模式第4个OSD区域
    public byte ByFontThickNess; //OSD的字体粗细模式0-默认
    public byte ByNameColorMode; // 童道名字体颜色方案0~15
    public byte ByNameCoordinateMode; // 坐标模式通道模式
    public byte ByOsdAttrib; // OSD属性:
    public byte ByShowChanName; // 预览的图象上是否显示通道名称,0-不显示,1-显示 区域为704*576
    public byte ByShowText; // 预览的图象上是否显示扩展的标题,0-不显示,1-显示 区域为704*576
    public byte ByShowText4; // 预览的图象上是否显示第4个OSD,0-不显示,1-显示 区域为704*576

    public byte ByShowTime; // 预览的图象上是否显示OSD,0-不显示,1-显示

    /* 0: xxxx/xx/xx xx:xx:xx 年/月/日 时分秒*/
    /* 1: xx/xx/xxxx xx:xx:xx 月/日/年 时分秒*/
    /* 2: xx:xx:xx 时分秒*/
    /* 3: xxxx-xx-xx xx:xx:xx 年-月-日 时分秒*/
    /* 4: xx-xx-xxxx xx:xx:xx 月-日-年 时分秒*/
    /* 5: xx-xx-xxxx xx:xx:xx 日-月-年 时分秒 wangjun*/
    /* 6: xx/xx/xxxx xx:xx:xx 日/月/年 时分秒 wangjun*/
    /* 7: xxxx年xx月xx日 xx时xx分xx秒 wangjun*/
    /* 8: xxxx年xx月xx日 xx:xx:xx */
    public byte ByShowWeek; // 是否显示星期
    public byte[] ByTemp = new byte[3];
    public byte ByText4ColorMode; // 扩展字符字体颜色方案0~15
    public byte ByText4CoordinateMode; // 坐标模式
    public byte ByTextBackGroundColor; // 背景透明度
    public byte ByTextBackGroundMode; // 扩展OSD背景大小模式 0-以高为准，1-以宽为准，2-自动按图像比例处理, 3-指定大小, 4-全屏
    public byte ByTextColorMode; // 扩展字符字体颜色方案0~15

    public byte ByTextCoordinateMode; // 坐标模式

    /* 0-相对左上角，1-相对右上角，2-相对左下角，3-相对右下角*/
    public byte ByTextExpandMode; // 是否启用扩展标题的菜单模式
    public byte ByTimeColorMode; // 时间字体颜色方案0~15
    public byte ByTimeCoordinateMode; // 坐标模式时间模式
    public byte ByTimeType; // OSD类型(主要是年月日格式)
    public byte ByWeekType; // 星期类型0-中文，1-英文
    public uint DwSize;

    public string SChanName = new(new char[64]); // 通道名

    /*扩展显示的标题*/
    public string SShowText = new(new char[256]); // 扩展的标题

    /*第4个OSD区域*/
    public string SText4 = new(new char[256]); // 第4个OSD
    public ushort WShowNameTopLeftX; // 通道名称显示位置的x坐标
    public ushort WShowNameTopLeftY; // 通道名称显示位置的y坐标
    public ushort WShowTextTopLeftX; // 扩展的标题显示位置的x坐标

    public ushort WShowTextTopLeftY; // 扩展的标题显示位置的y坐标

    /* 0-相对左上角，1-相对右上角，2-相对左下角，3-相对右下角*/
    public ushort WShowTimeTopLeftX; // OSD的x坐标
    public ushort WShowTimeTopLeftY; // OSD的y坐标
    public ushort WText4TopLeftX; // 第4个OSD显示位置的x坐标
    public ushort WText4TopLeftY; // 第4个OSD显示位置的y坐标
    public ushort WTextBackGroundHeight; // 扩展背景高度，相对于704*576
    public ushort WTextBackGroundWidth; // 扩展背景宽度，相对于704*576
}

/*OSD颜色方案配置*/
public class TmVideoOsdColorInfoT
{
    public byte ByBackAlpha;
    public byte ByBackB;
    public byte ByBackG;
    public byte ByBackR;
    public byte ByEdgeB;
    public byte ByEdgeG;

    public byte ByEdgeR;

    /*字体主题颜色RGB*/
    public byte ByEnable;

    /*字体背景颜色RGB*/
    public byte ByEnableBack;

    /*字体边缘颜色RGB*/
    public byte ByEnableEdge;
    public byte ByFocusB;

    public byte ByFocusG;

    /*焦点颜色*/
    public byte ByFocusR;
    public byte ByFontB;
    public byte ByFontG;
    public byte ByFontR;
}

public class TmVideoOsdColorCfgT
{
    public uint DwSize;

    public TmVideoOsdColorInfoT[] StruColorMode =
        Arrays.InitializeWithDefaultInstances<TmVideoOsdColorInfoT>(DefineConstants.ColorModeNum);
}

/*安霸需要*/
public class TmVideoExpandOsdInfoT
{
    /*0-相对左上角，1-相对右上角，2-相对左下角，3-相对右下角*/
    public byte CoordinateMode;

    /*模式0-字符串，1-画框, 2-点, 3-画十字， 21-字符串和框，22字符串和点, 23字符串和十字*/
    public byte Mode;

    /*位置和大小信息，相对于704*576*/
    public TmAreaScopeT Pos = new();

    public byte Save;

    /*字体大小*/
    public byte Size;

    /*字体颜色RGB24{r,g,b}*/
    //C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
    //	union
    //	{
    //		struct
    //		{
    //			byte r;
    //			byte g;
    //			byte b;
    //			byte t;
    //		}
    //		rgb;
    //		uint c;
    //	}
    //C++ TO C# CONVERTER NOTE: Access declarations are not available in C#:
    //	color;
    //C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
    //	union
    //	{
    //		struct
    //		{
    //			byte r;
    //			byte g;
    //			byte b;
    //			byte t;
    //		}
    //		rgb;
    //		uint c;
    //	}
    //C++ TO C# CONVERTER NOTE: Access declarations are not available in C#:
    //	border_color;
    /*字符信息*/
    public string SzInfo = new(new char[256]);
}

public class TmVideoExpandOsdCfgT
{
    public byte ByTemp;
    public byte ByType;
    public ushort Count;
    public uint DwSize;
    public TmVideoExpandOsdInfoT[] StruOsdInfo = Arrays.InitializeWithDefaultInstances<TmVideoExpandOsdInfoT>(1);
}

/*安霸需要*/
/*扩展字符参数配置*/
public class TmExpandOsdCfgT
{
    public byte ByBackColor; //背景色ID, 0-默认
    public byte ByBackHeight; //菜单高度，为原始高度1%-100%
    public byte ByBackWidth; //菜单宽度，为原始高度1%-100%
    public ushort ByBackX; //菜单左上角坐标，为原始高度0%-100%
    public ushort ByBackY; //菜单左上角坐标，为原始高度0%-100%
    public byte ByBgAlpha; //背景透明度，0-默认,透明，255-不透明
    public byte ByCaptionColor; //标题色ID, 0-默认
    public byte ByEnableCaption; //是否使用标题
    public byte ByEnableXy; //启用左上角坐标
    public byte ByFgAlpha; //字体透明度，0-默认,透明，255-不透明
    public byte ByFocusColor; //焦点色ID, 0-默认
    public byte ByFontColor; //字体色ID, 0-默认
    public byte[] ByTemp = new byte[2];
    public uint DwSize;
}

/*扩展字符颜色设置*/
public class TmExpandOsdAttrCfgT
{
    public byte ByColorMode;
    public byte ByLine;
    public uint DwSize;
    public ushort NHorPosition;
}

/*扩展字符字符串设置*/
public class TmExpandOsdStringCfgT
{
    public byte ByLine; //操所行0-第一行，0xFF-所有行
    public byte[] ByTemp = new byte[3];
    public uint DwSize;
    public string SzInfo = new(new char[256]);
}

/*扩展字符显示设置*/
public class TmExpandOsdDisplayCfgT
{
    public byte ByDisplay;
    public byte ByLine;
    public byte[] ByTemp = new byte[2];
    public uint DwSize;
}

/*扩展字符清空设置*/
public class TmExpandOsdClearCfgT
{
    public byte ByLine;
    public byte[] ByTemp = new byte[3];
    public uint DwSize;
}

/* 图像参数 */
public class TmPicCfgT
{
    /* 0: xxxx/xx/xx xx:xx:xx 年月日 时分秒*/
    /* 1: xx/xx/xxxx xx:xx:xx 月日年 时分秒*/
    /* 2: xx:xx:xx 时分秒*/
    public byte ByDispWeek; // 是否显示星期
    public byte ByEnableStreamNum; //是否支持双码流，0-单码流，1-双码流，此参数还必须根据NVS硬件决定
    public byte ByOsdAttrib; // OSD属性:透明
    public byte ByOsdType; // OSD类型(主要是年月日格式)

    public byte ByShowNamAttrib; // 名称属性:透明

    /*遮挡*/
    public byte DwEnableHide; // 是否启动遮挡 ,0-否,1-是 区域为704*576

    /*显示通道名*/
    public uint DwShowChanName; // 预览的图象上是否显示通道名称,0-不显示,1-显示 区域为704*576
    public uint DwShowOsd; // 预览的图象上是否显示OSD,0-不显示,1-显示
    public uint DwSize;
    public uint DwVideoFormat; // 只读：视频制式 0-PAL 1-NTSC

    public string SChanName = new(new char[32]);

    /*遮挡报警*/
    public TmHideAlarmT StruHideAlarm = new();

    public TmAreaScopeT StruHideArea = new(); //遮挡区域

    /*移动侦测*/
    public TmMotionT StruMotion = new();

    /*信号丢失报警*/
    public TmViLostT StruViLost = new();

    /* 0: 不透明 */
    /* 1: 半透明 */
    public char[][] SWeekName = RectangularArrays.RectangularCharArray(7, 20); //星期名称，0为星期一，6为星期日
    public ushort WOsdTopLeftX; // OSD的x坐标
    public ushort WOsdTopLeftY; // OSD的y坐标
    public ushort WShowNameTopLeftX; // 通道名称显示位置的x坐标
    public ushort WShowNameTopLeftY; // 通道名称显示位置的y坐标
}

/*压缩参数*/
public class TmCompressionT
{
    /*码率类型0:定码率, 1:变码率, 2:限码流*/
    public byte ByBitrateType;

    /*此结构值修改为编码格式，为了保持一致此值不能为16*/
    /*unsigned int	dwSize;*/
    /*视频编码格式，0-BKMPEG4,1-H264,2-MJPEG,3-H265,4-temperature,10-YPbPr,11-cvbs,12-hdmi,13-Digital*/
    public byte ByCompressFormat;

    /*设置当全帧时显示的帧率*/
    public byte ByDisplayFrame;

    /*是否启用图像质量控制*/
    public byte ByEnableQuality;

    /*关键帧间隔 : 0x00 表示使用默认；0xFF 表示全关键帧;其他值表示关键帧帧间隔*/
    public byte ByKeyFrameInterval;

    /*图象质量 0-最好 1-次好 2-较好 3-一般 4-较差 5-差 6-最差*/
    public byte ByPicQuality;

    /*H26x编码级别<10-默认, 10-Baseline, 11-MainProfile, 12-HighProfile*/
    public byte ByProfileLevel;

    /*分辨率,最高位表示此变量是否生效，低6位为编码格式
    0-CIF(352x288) 1-QCIF(176x144), 2-2CIF(704x288), 3-4CIF(704x576),
    4-QQCIF(88x72), 5-DCIF(512x384), 6-VGA(640x480), 7-QDICF(256x192), 8-QVGA(320x240),
    9-800x600, 10-1024x768 11-1280x720, 12-D1(720x576),
    13-1600x1200, 14-1440x1080, 15-1920x1080, 16-1280x1024, 17-1280x960
    18-1920x1200, 19-2048x1152, 20-2048x1536, 21-2560x1440, 22-2592x1944
    23-960x576, 24-2304x1296, 25-3072x2048, 26-3840x2160, 27-4096x2160, 28-4000x3000
    29-1280x1280, 30-2880x720, 0x7F-指定大小扩展信息中定义
    */
    public byte ByResolution;

    /*码流类型0-视频流,1-复合流*/
    public byte ByStreamType;
    public byte ByTemp1;

    public byte ByVideoFrameRate;

    /*视频码率 0-保留 1-保留 2-32K 3-48k 4-64K 5-80K 6-96K 7-128K 8-160k 9-192K 10-224K 11-256K 12-320K
    13-384K 14-448K 15-512K 16-640K 17-768K 18-896K 19-1024K 20-1280K 21-1536K 22-1792K 23-2048K
    最高位(32位)置成1表示是自定义码流, 0-31位表示码流值(MIN-16K MAX-8192K)。
    */
    public uint DwVideoBitrate;

    /*帧率 0-全部; 1-1/16; 2-1/8; 3-1/4; 4-1/2; 5-1; 6-2; 7-4; 8-6; 9-8; 10-10; 11-12; 12-16; 13-20;
    扩展 14-1/15; 15-1/14: 16-1/13; 17-1/12; 18-1/11; 19-1/10; 20-1/9; 21-1/7; 22-1/6; 23-1/5; 24-1/3;
    25-使用byVideoFrameRate的实际帧率
    */
    public byte DwVideoFrameRate;
}

/*压缩参数*/
public class TmCompressionExT
{
    public uint[] DwTemp = new uint[9];

    public ushort NImageHeight;

    /*图像的大小，要求必须4对齐*/
    public ushort NImageWidth;
}

public class TmCompressionCfgT
{
    /*组合编码格式，如果不是用新的编码方式，请设置为0*/
    public byte ByFormatId;

    /*0位标示D1使用的分辨率(0-720x576,1-704x576)*/
    public byte ByResolutionCfg;

    /*码流数*/
    public byte ByStreamCount;

    /*网络数据流是加密模式0-不加密，1-加密*/
    public byte ByStreamEncryptMode;

    public uint DwSize;

    //压缩的扩展参数
    public TmCompressionExT[] StruCompressionEx = Arrays.InitializeWithDefaultInstances<TmCompressionExT>(8);

    /*第4子码流参数*/
    public TmCompressionT StruFourthPara = new();

    /*网络传输子码流参数*/
    public TmCompressionT StruNetPara = new();

    /*第5678子码流参数*/
    public TmCompressionT[] StruOtherPara = Arrays.InitializeWithDefaultInstances<TmCompressionT>(4);

    /*录像主流参数*/
    public TmCompressionT StruRecordPara = new();

    /*第3子码流参数*/
    public TmCompressionT StruThirdPara = new();
}

/* 区域编码参数，针对码流的信息 */
public class TmVideoEnjoyInfoT
{
    public byte ByEnable; //是否启用5
    public byte[] ByResTemp = new byte[12];
    public byte ByScopeNum; //区域数
    public byte[] ByTemp = new byte[2]; //保留

    public AnonymousClass[] Scope = Arrays.InitializeWithDefaultInstances<AnonymousClass>(4);

    //C++ TO C# CONVERTER NOTE: Classes must be named in C#, so the following class has been named by the converter:
    public class AnonymousClass
    {
        public byte ByLevel; //区域的编码级别
        public byte ByQuality; //区域的质量
        public byte[] ByTemp = new byte[6];
        public TmAreaScopeT StruScope = new(); //侦测区域704*576
    }
}

/* 区域编码参数配置 */
public class TmVideoEnjoyCfgT
{
    public uint DwSize;
    public TmVideoEnjoyInfoT[] StruEnjoy = Arrays.InitializeWithDefaultInstances<TmVideoEnjoyInfoT>(8);
}

/*码流信息*/
public class TmStreamDescriptionT
{
    public byte ByFormat; //编码格式
    public byte ByFrameRateNum; //帧率列表数
    public byte[] ByFramesRateList = new byte[DefineConstants.MaxFrameRateNum]; //帧率列表
    public byte ByMaxFrameRate; //最大帧率限制
    public byte ByResolution; //图像大小格式索引
    public byte[] ByTemp = new byte[2]; //保留
    public byte ByType; //码流类型0-压缩编码，1-数字或模拟输出，2-抓图码流
    public uint DwDisplayRatio; //显示的正确比例*1000
    public uint DwMaxBitRate; //码流上限
    public ushort NHeight; //图像高
    public ushort NWidth; //图像宽
}

/*编码能力查询*/
public class TmCompressCapabilityT
{
    public byte ByFormatId; //编码格式
    public byte ByStreamCount; //码流数
    public byte[] ByTemp = new byte[2];
    public string SName = new(new char[64]); //编码格式名称,如1080P(H264) + D1(H264)

    public TmStreamDescriptionT[]
        StruStreamFormat = Arrays.InitializeWithDefaultInstances<TmStreamDescriptionT>(4); //码流的编码格式
}

public class TmCompressCapabilityCfgT
{
    public uint DwCount; //支持的压缩格式数
    public uint DwSize; //本结构大小

    public TmCompressCapabilityT[]
        StruCapability = Arrays.InitializeWithDefaultInstances<TmCompressCapabilityT>(1); //组合编码列表，这里象征性定义一个
}

/*扩展编码能力查询*/
public class TmCompressCapabilityExT
{
    public byte ByFormatId; //编码格式
    public byte ByStreamCount; //码流数
    public byte[] ByTemp = new byte[2];
    public string SName = new(new char[128]); //编码格式名称,如1080P(H264) + D1(H264)

    public TmStreamDescriptionT[]
        StruStreamFormat = Arrays.InitializeWithDefaultInstances<TmStreamDescriptionT>(8); //码流的编码格式
}

public class TmCompressCapabilityExCfgT
{
    public uint DwCount; //支持的压缩格式数
    public uint DwSize; //本结构大小

    public TmCompressCapabilityExT[]
        StruCapability = Arrays.InitializeWithDefaultInstances<TmCompressCapabilityExT>(1); //组合编码列表，这里象征性定义一个
}

/*音频压缩的配置结构，这个结果的设置是针对系统的所有通道*/
public class TmAudioCfgT
{
    /*音频降噪功能，0-关闭，1-100开启*/
    public byte ByAudioDenoise;

    /*输入选择0-选择音频输入，1-选择MIC输入*/
    public byte ByAudioInMode;

    /*音频输出功能，0-外部输出，1-内置喇叭输出*/
    public byte ByAudioOutMode;

    /*声音输出0-AOUT,1-HDMI*/
    public byte ByAudioOutToHdmi;

    /*音量输出大小0-100, 0最小*/
    public byte ByAudioOutVolume;

    /*音频压缩码流默认0-默认,1-16K,2-24K,3-32K,4-40K,5-48K,6-56K,7-64K,8-128K,9-256K*/
    public byte ByBitRate;

    /*要所音频声道数0-左声道，1-右声道，2-立体声*/
    public byte ByChannelMode;

    /*音频编码格式，参考音频编码格式列表*/
    public byte ByCompressFormat;

    /*音频输入模式，0-拾音器，1-线性输入*/
    public byte ByLineInMode;

    /*音频采样率0-默认, 1-8000Hz, 2-16000Hz, 3-22050Hz, 4-44100Hz, 5-48000Hz*/
    public byte BySamplesRate;

    public byte[] ByTemp = new byte[2];

    /*音频放大倍数*100,如1表示0.01,100表示1*/
    public uint DwLampFactor;

    /*本结构大小*/
    public uint DwSize;
}

/*音频编码格式列表*/
/*摄像机抓拍定义*/
public class TmCaptureImageCfgT
{
    /*报警抓图时，如果一直有报警，那么每次开始抓图的时间间隔，单位秒，默认10秒,0为开始报警才抓图*/
    /*注意当报警停掉，间隔10秒后再报警为另外一次报警，否则还是前一次报警*/
    public byte ByAlarmIntervalCapture;

    /*每次报警抓图数目0-默认，1~5*/
    public byte ByCaptureNum;

    /*抓图码流为从码流0-主码流，1-从码流，2-第3码流，3-第4码流，0xFF使用默认码流根据默认配置选项处理*/
    public byte ByCaptureStream;

    /*抓取vin号，0-默认(全部抓图), 从低到高位表示1,2,3,4,5,6,7,8*/
    public byte ByCaptureVin;

    /*默认抓图设置0-自动选择最佳抓图(首先选择最大的MJPEG，后选择主码流)，1-主码流抓图，2-2码流抓图，3-3码流抓图，4-4码流抓图*/
    public byte ByDefaultCaptureMode;

    /*是否启用指定目录存放*/
    public byte ByEnableDirectory;

    /*图像格式，0-BMP, 1-JPEG, 2-H264, 3-YUV, 4-H265目前保留只能是1*/
    public byte ByFormat;

    /*抓图大小0-默认(抓取主码流大小)， 1-抓取vin的最大,2-指定*/
    public byte ByMaxVinImage;

    /*图象质量 0-最好 1-次好 2-较好 3-一般 4-较差 5-差 6-最差*/
    public byte ByPicQuality;

    /*抓图通道是否叠加和主码流一样的OSD信息，这里只针对有单独的抓图通道，如果是其它码流按设置处理*/
    public byte ByRenderOsdInfo;

    //抓图的大小,与编码信息的参数byResolution一致
    public byte ByResolution;

    /*抓图存放位置0-保存到本地硬盘，1-上传到FTP服务器,2-通过报警通道上传*/
    /*3-本地保存同时上传FTP,4-本地保存同时上传报警通道*/
    public byte BySaveMode;

    /*自动抓图的间隔时间毫秒*/
    public uint DwCaptureInterval;

    /*服务器的端口，ftp默认21*/
    public uint DwFtpServerPort;

    /*本结构大小*/
    public uint DwSize;

    /*抓图的存放目录*/
    public string SDirectoryName = new(new char[128]);

    /*FTP服务器用户名*/
    public string SFtpCliUserName = new(new char[16]);

    /*FTP服务器用户密码*/
    public string SFtpCliUserPass = new(new char[16]);

    /*抓图FTP服务器设置*/
    /*允许最大128字节*/
    public string SFtpServerIpAddr = new(new char[128]);
}

/*自动抓图的布防时间*/
public class TmCaptureSchedCfgT
{
    /*是否启用布防抓图*/
    public byte ByEnableCapture;

    /*保留*/
    public byte ByReserves;

    /*保留*/
    public byte[] ByTemp = new byte[2];

    /*每组抓图动作间隔时间，单位秒,默认10秒*/
    public uint DwCaptureTime;

    /*本结构大小*/
    public uint DwSize;

    /*抓图时间段*/
    public TmSchedTimeT[][] StruCaptureSched =
        RectangularArrays.RectangularTmSchedTime_tArray(DefineConstants.MaxDays, DefineConstants.MaxTimeSegment);
}

/*摄像机抓拍定义*/
public class TmManualCaptureCfgT
{
    /*抓图数目1~5*/
    public byte ByCaptureNum;

    /*图像格式，0-BMP, 1-JPEG, 2-H264, 3-YUV, 4-H265目前保留只能是1*/
    public byte ByFormat;

    /*抓图*/
    /*抓图的间隔时间毫秒*/
    public uint DwCaptureInterval;

    /*本结构大小*/
    public uint DwSize;
}

/*----------------------------------------------------------------------*/
/*解码器*/
public class TmDecoderCfgT
{
    public byte ByBindRs232Id; //绑定的串口号0-默认，1-第一个，2-第二个...，0xFF关闭
    public byte ByDataBit; // 数据有几位 0－5位，1－6位，2－7位，3－8位;
    public byte ByDeviceAddress; //本设备485地址
    public byte ByFlowcontrol; // 0－无，1－软流控,2-硬流控
    public byte ByMode; //控制模式，0-默认，1-485, 2-422
    public byte ByParity; // 校验 0－无校验，1－奇校验，2－偶校验;
    public byte ByStopBit; // 停止位 0－1位，1－1.5位，2-2;
    public byte[] ByTemp = new byte[49];

    public byte
        ByTransBaudRate; // 透明传输的第三方波特率0－50，1－75，2－110，3－150，4－300，5－600，6－1200，7－2400，8－4800，9－9600，10－19200， 11－38400，12－57600，13－76800，14－115.2k;

    public uint DwBaudRate; //波特率(bps)直接表示
    public uint DwSize;
    public string SzDecoderName = new(new char[20]); //解码器名称
    public ushort WDecoderAddress; //解码器地址:0 - 255
}

public class TmDecoderInfoT
{
    public uint DwSize;
    public string SzDecoderName = new(new char[20]); //解码器名称
}

/*
 *预置点新结构定义
 */
public class TmPresetInfoT
{
    public byte ByEnable; // 预置点是否设置
    public byte[] ByTemp = new byte[3]; // 保留
    public string SzPresetName = new(new char[128]); // 预置点名称
}

public class TmPresetCfgT
{
    public uint DwSize; //本结构大小

    public TmPresetInfoT[]
        PPresetList = Arrays.InitializeWithDefaultInstances<TmPresetInfoT>(DefineConstants.MaxPreset); //预置点列表
}

/*
巡航的点信息
*/
public class TmCruisePointT
{
    public byte ByCruiseSpeed; //巡航速度
    public byte ByEnable; //是否启用0,1
    public byte ByPresetNo; //预置点号0-MAX_PRESET-1
    public byte ByTemp; //保留
    public uint DwStopTime; //停留时间，单位秒
}

public class TmCruiseInfoT
{
    public byte ByCruiseLineMerge; //N条线合并为一条线
    public byte ByEnableThisCruise; //是否启用本巡航轨迹
    public byte[] ByTemp = new byte[2];
    public TmCruisePointT[] StruCruise = Arrays.InitializeWithDefaultInstances<TmCruisePointT>(16); //预置点信息
}

/*巡航定义*/
public class TmCruiseCfgT
{
    public uint DwSize; //本结构大小

    public TmCruiseInfoT[] StruCruiseLine =
        Arrays.InitializeWithDefaultInstances<TmCruiseInfoT>(DefineConstants.CruiseMaxLineNums);
}

public class TmPtzSchedTimeT
{
    public byte ByRunLine; //运行的线,0xFF为所有线
    public byte ByRunMode; //0位-是否回到看守卫
    public byte ByRunNum; //运行次数, 0-无限制，其它为次数
    public byte ByRunType; //运行类型0-巡航,1-轨迹,2-线扫,3-Z字形扫描,4-看守位
    public TmSchedTimeT StruSchedTime = new();
}

/*云台配置信息*/
public class TmPtzCfgT
{
    public byte By3DProtocal; //云台3D协议，0-不启用,1-hb1,2-hb2,3-sx
    public byte By3DZoomMaxBs; //每次调用3DZoom的最大倍数
    public byte ByAlarmOutByAux; //报警输出通过辅助开关命令输出0-不输出，其它-辅助开关x
    public byte ByAuxLinkAlarmOut; //zzt add  辅助开关联动报警输出
    public byte ByCruiseCaptureImage; //预置点巡航是否抓图，具体抓图信息依据tmCaptureImageCfg_t的配置信息
    public byte ByDisableMenu; //是否不启用菜单
    public byte ByDisAutoSend485; //是否自动发送485数据，0-默认发送， 1-不发送
    public byte ByDisplayMode; //信息显示参考位置，参考OSD设置
    public byte ByDisplayPtzInfo; //当操作云台是是否显示信息(zoom倍数，角度)
    public byte ByDisplayX; //信息显示X坐标
    public byte ByDisplayY; //信息显示Y坐标
    public byte ByFocusTrace; //调用预置点是否跟焦
    public byte ByIrCutMode; //IRCut光敏控制模式，0-光敏优先，1-摄像机agc优先，需要禁止光敏控制
    public byte ByLanguageId; //显示语言，包括Osd和菜单，0-中文， 1-英文
    public byte ByLimitMaxSpeed; //Zoom的最大限速
    public byte ByLimitMinSpeed; //Zoom的最小限速
    public byte[] ByLimitTemp = new byte[40]; //保留
    public byte ByManualControlPtzTime; //手动控制PTZ多少秒后，自动PTZ才能生效，默认30秒
    public byte ByPresetCruiseTime; //预置点停留时间默认值(默认为10秒)，可设置5~100秒
    public byte ByPtzHorControlMode; //云台水平控制模式，0-向右水平角度变大，1-向左水平角度变大
    public byte ByPtzLimitPresetMax; //不能使用的预置点结束号(包含)，1标示第一个,0表示不启用
    public byte ByPtzLimitPresetMin; //不能使用的预置点开始号(包含)，1标示第一个,0表示不启用
    public byte ByPtzMaxSpeedCtl; //设置球机速度缩放比例 0 无效 1-200 最大速度的百分比
    public byte ByPtzSpeedCtl; //wangjun add 球机变倍后速度控制参数，因为球机的速度不是线行的，所以这里只是一个大概值
    public byte ByPtzTransPresetMax; //球机需要的特殊预置点结束号(包含)，1标示第一个,0表示不启用
    public byte ByPtzTransPresetMin; //球机需要的特殊预置点开始号(包含)，1标示第一个,0表示不启用
    public byte ByPtzUsePresetMin; //球机的预置点号可以使用开始号，共需要9个,0表示不启用
    public byte ByPtzZoomMode; //云台动作和机芯动作先后模式，0-同时动作，1-先云台后机芯，2-先机芯后云台
    public byte ByRefuse485; //是否从接收485数据0-接收默认，1-拒绝接收
    public byte ByRotate; //原始图像旋转状态,用于修正云台控制，0-正常，1-水平镜像，2-垂直镜像，3-180°
    public byte BySendSpecialtiesZoomBs; //发送特别倍速0-不发送，其它为特定倍数，默认3
    public byte BySendZoomBsMode; //发送变倍倍数到球机的模式0-不发送，1-发送所有倍数，2-发送整数倍数，3-变倍结束后发送倍数
    public byte[] ByTemp0 = new byte[340];
    public byte[] ByTemp1 = new byte[3];
    public byte[] ByTmp = new byte[1];
    public ushort ByVerticallyMaxAngle; //云台垂直最大限制度*10
    public byte ByViewZoomBsMode; //显示倍数模式0-安光学倍数显示，1-安线性倍数显示
    public uint DwSize; //本结构大小
}

/*云台辅助开关布防设置，可以通过此设置定时打开辅助功能*/
/*从低到高第1表示PTZ_LIGHT_PWRON*/
/*从低到高第2表示PTZ_WIPER_PWRON*/
/*从低到高第3表示PTZ_FAN_PWRON*/
/*从低到高第4表示PTZ_HEATER_PWRON*/
/*从低到高第5表示PTZ_AUX_PWRON*/
public class TmPtzAuxTimeT
{
    public byte ByAuxEnable; //辅助开关使能
    public byte ByAuxState; //辅助开关状态
    public byte[] ByTemp = new byte[2];
    public TmSchedTimeT StruSchedTime = new(); //布防时间
}

public class TmPtzAuxCfgT
{
    public byte ByEnable; //辅助开关布防是否生效
    public byte[] ByTemp = new byte[3];
    public uint DwSize;

    public TmPtzAuxTimeT[][] StruSchedTime =
        RectangularArrays.RectangularTmPtzAuxTime_tArray(DefineConstants.MaxDays,
            DefineConstants.MaxTimeSegment); //布防的时间
}

/*看守卫布放配置*/
public class TmPtzSchedCfgT
{
    public byte ByEnableKeepWatch; //是否启用看守位0,1
    public byte ByEnableSchedTime; //是否启用时间布防
    public byte BySchedTimeMode; //布防时间模式0-每周布防模式，1-每天布防模式
    public byte ByTemp;
    public uint DwSize;
    public ushort WKeepWatchCheckTime; //(秒)停止云台控制一定时间后开始启用调用看守位或者在系统启动后调用看守位

    public ushort WRunLineInterval; //运行线停留时间(秒)
    //C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
    //	union
    //	{
    //		tmPtzSchedTime_t struSchedTime[DefineConstants.MAX_DAYS][DefineConstants.MAX_TIMESEGMENT]; //布防的时间
    //		tmPtzSchedTime_t struSchedDate[DefineConstants.MAX_ALLTIMESEGMENT];
    //	};
}

/*----------------------------------------------------------------------*/
/*RS232串口*/
public class TmPppCfgT
{
    public byte ByDataEncrypt; //数据加密,0-否,1-是
    public byte ByPppMode; //PPP模式, 0－主动，1－被动
    public byte ByRedial; //是否回拨:0-否,1-是
    public byte ByRedialMode; //回拨模式,0-由拨入者指定,1-预置回拨号码
    public byte ByTemp1;
    public byte ByTemp2;
    public byte ByTemp3;
    public byte ByTemp4;
    public uint DwMtu; //MTU
    public uint DwSize;
    public string SLocalIp = new(new char[24]); //本地IP地址
    public string SLocalIpMask = new(new char[24]); //本地IP地址掩码
    public string SRemoteIp = new(new char[24]); //远端IP地址
    public string STelephoneNumber = new(new char[32]); //电话号码
    public string SzPassword = new(new char[32]); // 密码
    public string SzUserName = new(new char[32]); // 用户名
}

public class TmRs2322CfgT
{
    public byte ByDataBit; // 数据有几位 0－5位，1－6位，2－7位，3－8位;
    public byte ByFlowcontrol; // 0－无,1－软流控,2-硬流控
    public byte ByParity; // 校验 0－无校验,1－奇校验，2－偶校验;
    public byte ByRecvProtocal; // 0-默认，1-PM2.5, 2-OSD叠加，3-VDM字符协议,4-VISCA8300协议,5-511A单片机协议,6-完全透传协议
    public byte ByStopBit; // 停止位 0－1位,1－2位;
    public byte[] ByTemp = new byte[186]; //保持和原来结构大小兼容
    public uint DwBaudRate; // 波特率(bps)
    public uint DwSize;
    public byte DwWorkMode; // 工作模式，0-默认, 1－窄带传输(232串口用于PPP拨号)，2－控制台(232串口用于参数控制)，3－透明通道
}

/*----------------------------------------------------------------------*/
/*IR报警输入*/
public class TmIrAlarmInCfgT
{
    public byte By511ACom2Baud; /*设备第二串口波特率
                                                                        0-默认, 1-2400, 2-4800, 3-9600, 4-14400, 5-19200, 6-38400, 7-56000, 8-57600, 9-115200, 10-128000, 11-256000,
                                                                        */

    public byte ByAdcCheckTime; //ADC检测报警时间延时0-默认，单位100毫秒

    /*511A设备信息*/
    public byte ByAdcWorkMode; //0-默认,1-串口设备自动IRCUT,2-串口设备报警连接报警输入
    public byte ByAlarmSource; //报警来源0-默认内部决定，1-球机的串口报警信息，2-511A设备的报警信息
    public byte ByAlarmType; //报警器类型,0：常开,1：常闭
    public byte[] ByTemp = new byte[1];
    public uint DwSize;
    public ushort WAdcCurrent; //读取报警有效的当前值(0~1000)，只读
    public ushort WAdcHigh; //读取报警有效的高值(0~1000)
    public ushort WAdcLow; //读取报警有效的低值(0~1000)
}

/*报警输入*/
public class TmAlarmInCfgT
{
    public byte ByAlarmInHandle; // 是否处理
    public byte ByAlarmSource; //报警来源0-默认内部决定，1-球机的串口报警信息，2-511A设备的报警信息
    public byte ByAlarmType; //报警器类型,0：常开,1：常闭
    public byte ByTemp;
    public uint DwHandleMinTime; // 处理报警的最小时间，单位毫秒(在此时间内有对此报警只处理一次)
    public uint DwSize;
    public string SAlarmInName = new(new char[32]); // 名称
    public TmHandleExceptionT StruAlarmHandleType = new(); // 处理方式
    public TmSchedTimeT[][] StruAlarmTime = RectangularArrays.RectangularTmSchedTime_tArray(7, 4); //布防时间

    public TmTransFerT[]
        StruAlarmTransFer = Arrays.InitializeWithDefaultInstances<TmTransFerT>(16); //触发的16个通道，在tmTransFer_t中定义通道号

    public ushort WAdcCurrent; //读取报警有效的当前值(0~1000)，只读
    public ushort WAdcHigh; //读取报警有效的高值(0~1000)
    public ushort WAdcLow; //读取报警有效的低值(0~1000)
}

/*DVR报警输出320*/
public class TmAlarmOutCfgT
{
    public byte ByAlarmObject; // 输出到0-默认，1-球机串口报警输出，2-511A设备的报警输出
    public byte ByAlarmType; // 报警器类型,0：常开,1：常闭
    public byte ByManualAlarmOutModle; // 0:由dwAlarmOutDelay决定关闭; 1:必须手动关闭
    public uint DwAlarmOutDelay; // 输出保持时间(-1为无限，手动关闭), 毫秒
    public byte DwSchedTimType; // 输出布防类型,0-用时间布防，1-默认开始所有布防，2-撤防
    public uint DwSize;
    public byte[] SAlarmOutName = new byte[32]; // 名称
    public TmSchedTimeT[][] StruAlarmTime = RectangularArrays.RectangularTmSchedTime_tArray(7, 4); // 报警输出激活时间段
}

/*DVR报警处理*/
public class TmAlarmCfgT
{
    /*报警上传中心类型，0客户端需要登录，1客户端不需要登录*/
    public byte ByAlarmToManagerType;

    /*是否允许服务器自动定时复位重启*/
    public byte ByAllowAutoReset;

    /*允许设置参数时蜂鸣器响和指示灯亮*/
    public byte ByAllowConfigBeep;

    /*允许配置连接超时检测*/
    public byte ByAllowConfigTinmeout;

    /*允许系统软复位,目前硬件不支持*/
    public byte ByAllowSoftReset;

    /*循环方式，0为按指定时间循环，此模式为系统启动开始计时参考时间；*/
    /*其它为指定天数中的指定时间复位(1-31)*/
    public byte ByAutoResetMode;

    /*是否允许SD卡录像*/
    public byte ByEnableSdCardRecord;

    /*是否启用UPNP服务*/
    public byte ByEnableUPnp;

    /*是否启用写文件状态灯提示*/
    public byte ByEnableWriteFileState;

    /*日志保留天数1-255,依据设备而定*/
    public byte ByLogRemainDays;

    /*是否启动自动录像，为0关闭(报警触发录像)，为1一般录像，如果此时有其他报警录像则启动其他录像*/
    public byte ByNormalRecoder;

    /*录像文件大小，以分钟计算0表示不录像*/
    public byte ByRecoderFileSize;

    /*是否上传文件操作状态到服务器，服务器需要监听UDP端口*/
    public byte ByUpFileStateToManager;

    /*报警声音延时时间	*/
    public uint DwAlarmAudioTimeout;

    /*byAutoResetMode==0时为循环时间，以秒为单位，必须大于等于60，以系统启动开始计数*/
    /*byAutoResetMode!=0为指定时刻自动复位重启的始参考时间，以秒为单位，即一天中的秒数(0-86400)*/
    public uint DwAutoResetCircleTime;

    /*自动复位标志，从低位开始，一位代表一个功能*/
    /*0位为1表示允许网络复位*/
    /*1位为1表示允许视频采集复位*/
    /*31位为1表示允许系统复位*/
    public uint DwAutoResetFlags;

    /*控制连接超时，必须大于5秒*/
    public uint DwConfigTimeout;

    /*向DDNS服务器注册的时间间隔，单位秒，必须大于60秒*/
    public uint DwDdnsLoginTimeOut;

    /*云台控制超时，必须大于5秒*/
    public uint DwPtzControlTimeout;

    /*控制台串口连接超时，必须大于5秒*/
    public uint DwSerialTimeout;

    /*本结构大小*/
    public uint DwSize;

    /*报警上传时间间隔(单位秒), 为0表示只要有连续报警，就连续输出*/
    /*仅对报警上传有效*/
    public uint DwUpToTime;

    /*RTSP网络传输大小,0为默认*/
    public ushort WRtpPacketSize;

    /*网络传输强制流方式,0为客户端制定，>0为指定大小传输，256-8192*/
    public ushort WTranstStreamSize;
}

/*----------------------------------------------------------------------*/
/*上传报警信息156*/
public class TmAlaramInfoT
{
    public byte ByAlarmSubChannel; //报警子端口
    public byte[] Bytemp = new byte[2];
    public byte ByTemp1;
    public byte ByTemp2;
    public byte DwAlarmChannel; //报警端口
    public uint[] DwAlarmOutputNumber = new uint[4]; //报警输入端口对应的输出端口，哪一位不为-1表示对应哪一个输出
    public uint[] DwAlarmRelateChannel = new uint[16]; //报警输入端口对应的录像通道，哪一位不为-1表示对应哪一路录像
    public uint[] DwDiskNumber = new uint[16]; //dwAlarmType为4时,哪一位不为-1表示哪个硬盘
    public uint DwSize;
    public byte WAlarmState; //报警状态0-结束报警，1-开始报警

    public byte WAlarmType; /*    0-信号量报警,    1-硬盘满,    2-信号丢失，3－移动侦测，
                                                4－硬盘未格式化,5-读写硬盘出错,6-遮挡报警,7-制式不匹配,
                                                8-非法访问 9-无存储计划 10-磁盘异常 11-通道未录像
                                                12-前端信号量报警 13-跨线报警 14-非法闯入 15-物品遗留/丢失
                                                16-低温报警，17-高温报警，18-差温报警,19,10保留热成像使用*/
}

/*图像输出回调*/
public class TmCaptureImageInfoT
{
    /*缓冲大小*/
    public int Buffer2Size;

    /*缓冲大小*/
    public int BufferSize;

    /*报警通道*/
    public byte ByAlarmChannel;

    /*抓图方法0-自动抓拍，1-报警抓拍，2-手动抓拍*/
    public byte ByCaptureFunc;

    /*通道号0表示第一通道*/
    public byte ByChannelId;

    /*图像格式，0-BMP, 1-JPEG, 2-H264, 3-YUV, 4-H265目前保留只能是1*/
    public byte ByImageFormat;

    /*当前的序号*/
    public int CurrentIndex;

    /*本结构大小必须填写为sizeof(struct tmCaptureImageInfo_t)*/
    public uint DwSize;

    /*图片大小高*/
    public short NHeight;

    /*抓图的用户ID,只有手动才有用*/
    public uint NUserId;

    /*图片大小宽*/
    public short NWidth;

    /*缓冲指针*/
    public byte[] PBuffer;

    /*缓冲指针*/
    //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
    //ORIGINAL LINE: byte* pBuffer2;
    public byte PBuffer2;

    /*时间戳*/
    public TmTimeInfoT StruTimeStamp = new();

    /*总共抓张数*/
    public int TotalCaptureNum;
}

/*上传管理中心报警信息*/
public class TmToManagerAlarmInfoT
{
    public byte[] ByMacAddr = new byte[6]; //远程物理地址
    public byte[] ByTemp = new byte[4];
    public uint DwSize;
    public TmAlaramInfoT StruAlarmInfo = new();
    public string SzDvsName = new(new char[32]); //DVS名称
    public byte[] SzSerialNumber = new byte[48]; //服务器序列号
    public byte[] SzServerGuid = new byte[16]; //服务器唯一GUID
    public string SzServerIp = new(new char[16]); //服务器地址
    public string SzServerMask = new(new char[24]); //服务器地址
}

/*上传管理中心的图片信息定义*/
public class TmToManagerImageInfoT
{
    public byte ByAlarmId; //报警通道

    public byte ByChannelId; //摄像机通道

    /*5-RGB32,6-YUV444,7-YUV422,8-YUV420,9-BKMPEG4,10-H264,11-H265*/
    public byte ByCount; //本次抓图共要抓取张数
    public byte ByImageFmt; //图片格式,0-JPEG,1-BMP,2-RGB555,3-RGB565,4-RGB24,
    public byte ByImageMode; //抓图的模式0-报警联动抓图, 1-移动联动抓图, 2-手动抓图, 3-刷卡抓图, 4-预置点巡航,0FF-自动抓图
    public byte ByIndex; //当前图片的序号
    public byte[] ByMacAddr = new byte[6]; //远程物理地址
    public byte[] ByOtherInfo = new byte[2]; //保留，如果是报警，那么0，1表示其它报警器状态
    public byte[] ByTemp1 = new byte[8];
    public byte[] ByTemp2 = new byte[2]; //保留
    public uint DwImageSize; //图像大小，也就是本结构后面数据大小
    public uint DwSize; //本结构大小
    public AnonymousClass2 Image = new();
    public string SzServerIp = new(new char[16]); //服务器地址

    public AnonymousClass3 Time = new();

    //C++ TO C# CONVERTER NOTE: Classes must be named in C#, so the following class has been named by the converter:
    public class AnonymousClass2
    {
        public byte ByBitCount; //图像位数
        public byte ByRevolving; //图像是否旋转
        public short NHeight; //图片大小高
        public short NPitch; //图片pitch
        public short NWidth; //图片大小宽
    }

    //C++ TO C# CONVERTER NOTE: Classes must be named in C#, so the following class has been named by the converter:
    public class AnonymousClass3
    {
        public byte NDay;
        public byte NDayOfWeek;
        public byte NHour;
        public byte NMinute;
        public byte NMonth;
        public byte NSecond;
        public short NYear;
    }
}

/*上传中心的心跳包*/
/*设备心跳包定义*/
public class TmToManagerLiveHeartInfoT
{
    /*本结构定义*/
    public uint DwSize;

    /*报警输入数*/
    public byte NAlarmInCount;

    /*报警输出个数*/
    public byte NAlarmOutCount;

    /*设备的通道数*/
    public byte NChannelCount;

    /*设备地址*/
    public uint NDeviceAddress;

    /*服务器唯一GUID*/
    public byte[] NDeviceGuid = new byte[16];

    /*设备名称*/
    public string NDeviceName = new(new char[32]);

    /*设备端口*/
    public ushort NDevicePort;

    /*设备具体类型如(D1,HDCMA)*/
    public byte NDeviceSubType;

    /*设备类型*/
    public ushort NDeviceType;

    /*硬盘数(分区数)*/
    public byte NDiskCount;

    /*设备的厂家ID*/
    public ushort NFactoryId;

    /*设备的网卡MARK地址，第一个主网卡*/
    public byte[] NMarkAddress = new byte[6];
}

/*安全卫士设备报警信息定义*/
public class TmToManagerSafeGuardInfoT
{
    /*设备的报警按钮按下0-没有按下，1-按下*/
    public byte ByAlarmState;

    /*设备的布防按钮按下0-没有按下，1-按下*/
    public byte ByBuFangState;

    /*设备有人刷卡*/
    public byte ByCardEnable;

    /*设备的撤防按状态0-没有按下，1-按下*/
    public byte ByCheFangState;

    /*设备的红外状态0-没有打开，1-打开*/
    public byte ByInfraredState;

    /*设备的通行灯状态0-禁止，1-通行*/
    public byte ByLightState;

    /*设备的对讲按钮按下1-对讲*/
    public byte ByTalkState;

    /*卡号*/
    public uint DwCardNo;

    /*本结构定义*/
    public uint DwSize;

    /*设备地址*/
    public uint NDeviceAddress;

    /*设备端口*/
    public ushort NDevicePort;

    /*设备的网卡MARK地址，第一个主网卡*/
    public byte[] NMarkAddress = new byte[6];
}

/*设备调用云台信息定义TMCC_MAJOR_CMD_SERVERMESSAGE/TMCC_MINOR_CMD_SERVERCALLPTZ*/
public class TmToManagerPtzInfoT
{
    /*预置点号*/
    public byte ByCallData;

    /*设备调用云台的类型0-调用预置点*/
    public byte ByCallType;

    public byte[] ByTemp = new byte[2];

    /*本结构定义*/
    public uint DwSize;

    /*设备地址*/
    public uint NDeviceAddress;

    /*设备端口*/
    public ushort NDevicePort;

    /*设备的网卡MARK地址，第一个主网卡*/
    public byte[] NMarkAddress = new byte[6];

    /*名称*/
    public string SzCallName = new(new char[32]);
}

/* 服务器设备信息 256*/
public class TmServerInfoT
{
    public byte BServerConnect; //服务器连接或是断开连接
    public byte ByAlarmInNum; //服务器输入报警个数
    public byte ByAlarmOutNum; //服务器输出报警个数
    public byte ByCenterManager; //是否启动管理中心
    public byte[] ByMacAddr = new byte[6]; //远程物理地址
    public uint DwFactoryNo; //服务器组厂商编号
    public uint DwGroupId; //服务器组号端口号
    public uint DwHardwareVersion; //硬件版本,高16位是主版本,低16位是次版本
    public uint DwPanelVersion; //前面板版本,高16位是主版本,低16位是次版本
    public uint DwServerType; //服务器类型
    public uint DwSize; //该结构的大小，必须填写
    public uint DwSoftwareVersion; //软件版本号,高16位是主版本,低16位是次版本
    public string SzCenterManagerIp = new(new char[25]); //管理中心地址
    public string SzDvsName = new(new char[32]); //DVS名称
    public byte[] SzSerialNumber = new byte[48]; //服务器序列号
    public byte[] SzServerGuid = new byte[16]; //服务器唯一GUID
    public string SzServerIp = new(new char[25]); //服务器地址
    public string SzServerMask = new(new char[25]); //服务器地址
    public ushort WChannelNum; //服务器通道数
    public ushort WDvsPort; //端口号
    public ushort WHttpPort; //HTTP服务器
}

public class TmServerInfoExT
{
    public byte BServerConnect; //服务器连接或是断开连接
    public byte ByAlarmInNum; //服务器输入报警个数
    public byte ByAlarmOutNum; //服务器输出报警个数
    public byte ByCenterManager; //是否启动管理中心
    public byte ByEnableDhcp; //DHCP启用
    public byte ByEtherNet; //网络ID0-第一个网络，1-第二个网络
    public byte[] ByMacAddr = new byte[6]; //远程物理地址
    public byte ByTemp; //保留
    public byte ByWifi; //是否是WIFI地址1为WIFI地址
    public uint DwCurrentServerIp; //当前工作IP
    public uint DwCurrentServerMask; //当前工作IP掩码
    public uint DwFactoryNo; //服务器组厂商编号
    public uint DwGroupId; //服务器组号端口号
    public uint DwHardwareVersion; //硬件版本,高16位是主版本,低16位是次版本
    public uint DwLastUpgradeTime; //设备最后一次升级时间
    public uint DwPanelVersion; //前面板版本,高16位是主版本,低16位是次版本

    public uint DwRunTime; //服务器已运行时间

    /*高清新加20090306*/
    public uint DwServerGateWay; //服务器网关
    public uint DwServerType; //服务器类型
    public uint DwSize; //该结构的大小，必须填写
    public uint DwSoftwareVersion; //软件版本号,高16位是主版本,低16位是次版本
    public string SzCenterManagerIp = new(new char[25]); //管理中心地址
    public string SzDvsName = new(new char[32]); //DVS名称
    public byte[] SzSerialNumber = new byte[48]; //服务器序列号
    public byte[] SzServerGuid = new byte[16]; //服务器唯一GUID
    public string SzServerIp = new(new char[25]); //服务器地址
    public string SzServerMask = new(new char[25]); //服务器地址
    public ushort WChannelNum; //服务器通道数
    public ushort WCurrentDvsPort; //当前工作端口号
    public ushort WCurrentHttpPort; //当前工作HTTP服务器
    public ushort WDvsPort; //端口号
    public ushort WHttpPort; //HTTP服务器
}

/*配置服务器地址结构定义*/
public class TmServerCfgT
{
    public int ConfigMode; //配置后动作，0表示无动作，1表示从新启动
    public uint DwMajorCommand; //服务器配置命令
    public uint DwMinorCommand; //服务器配置命令
    public uint DwResverse; //保留
    public uint DwResverse1; //保留
    public uint DwSize; //该结构的大小，必须填写
    public TmServerInfoT[] PInfo = Arrays.InitializeWithDefaultInstances<TmServerInfoT>(1); //服务器信息
    public string SzPassword = new(new char[32]); //配置的口令
    public string SzUserName = new(new char[32]); //配置用户名
}

/*配置服务器地址结构定义*/
public class TmServerExCfgT
{
    public byte BControlAll; //配置时是否配置所有设备
    public byte[] BTemp = new byte[2]; //保留
    public byte ConfigMode; //配置后动作，0表示无动作，1表示从新启动
    public uint DwMajorCommand; //服务器配置命令
    public uint DwMinorCommand; //服务器配置命令

    public uint DwResverse; //指示是否是tmServerCfg_t的扩展结构

    /*0表示使用tmServerInfo_t配置数据，1表示tmServerInfoEx_t配置数据*/
    /*其它值表示其它结构*/
    public uint DwResverse1; //保留
    public uint DwSize; //该结构的大小，必须填写
    public TmServerInfoT[] PInfo = Arrays.InitializeWithDefaultInstances<TmServerInfoT>(1); //服务器信息

    public TmServerInfoExT[]
        PInfoEx = Arrays.InitializeWithDefaultInstances<TmServerInfoExT>(1); //UPnp的配置信息，扩展修改只能修改tmServerInfoEx_t结构了

    public string SzPassword = new(new char[32]); //配置的口令
    public string SzUserName = new(new char[32]); //配置用户名
}

/*多播配置结构定义*/
public class TmMultiServerCfgT
{
    public uint DwCompareSerialNumber; //是否需要匹配序列号，0-不匹配，设置所有，1-匹配GUID设置
    public uint DwMajorCommand; //服务器配置命令
    public uint DwMinorCommand; //服务器配置命令
    public uint DwSize; //该结构的大小，必须填写
    public string SzPassword = new(new char[32]); //配置的口令
    public string SzSerialNumber = new(new char[48]); //服务器唯一序列号
    public string SzUserName = new(new char[32]); //配置用户名
}

/*设备状态*/
public class TmChannelStateT
{
    public byte ByHardwareStatic; //通道硬件状态,0-正常,1-异常,例如DSP死掉
    public byte ByRecordStatic; //通道是否在录像,0-不录像,1-录像
    public byte BySignalStatic; //连接的信号状态,0-正常,1-信号丢失
    public uint DwBitRate; //实际码率
    public uint[] DwClientIp = new uint[6]; //客户端的IP地址
    public uint DwLinkNum; //客户端连接的个数
    public uint DwSize;
    public byte ReservedData;
}

public class TmDiskStateT
{
    public uint DwFreeSpace; //硬盘的剩余空间
    public uint DwHardDiskStatic; //硬盘的状态,休眠,活动,不正常等
    public uint DwSize;
    public uint DwVolume; //硬盘的容量
}

public class TmUserStateT
{
    public uint[] DwLoginAddress = new uint[16]; //登录地址
    public uint DwLoginNum; //登录数, 如果为0表示无用户登录
    public uint DwSize;
    public string SzUserName = new(new char[32]); //登录用户名
}

public class TmWorkStateT
{
    public byte[] ByAlarmInStatic = new byte[16]; //报警端口的状态,0-没有报警,1-有报警

    public byte[] ByAlarmOutStatic = new byte[4]; //报警输出端口的状态,0-没有输出,1-有报警输出

    /*add by zzt:2018-11-29*/
    public byte ByIcrState; //当前ICR工作状态0--白天模式 1--晚上模式

    public byte[] ByTemp = new byte[3];

    /*add by stone 20090508*/
    public uint DwCpuClockFrequency; //CPU的频率
    public uint DwDeviceRunTime; //设备自上次启动来运行时间，单位秒
    public uint DwDeviceStatic; //设备的状态,0-正常,1-CPU占用率太高,超过85%,2-硬件错误,例如串口死掉
    public uint DwFreeMemorySize; //剩余内存大小
    public uint DwSize;
    public uint DwTotalMemorySize; //总共内存大小
    public TmChannelStateT[] StruChanStatic = Arrays.InitializeWithDefaultInstances<TmChannelStateT>(16); //通道的状态
    public TmDiskStateT[] StruHardDiskStatic = Arrays.InitializeWithDefaultInstances<TmDiskStateT>(16); //硬盘当前状态
    public TmUserStateT[] StruUserStatic = Arrays.InitializeWithDefaultInstances<TmUserStateT>(16); //登录用户的状态
}

/*云台绝对控制 TMCC_MAJOR_CMD_PTZCONTROL/TMCC_MINOR_CMD_PTZABSOLUTE*/
public class TmPtzAbsoluteT
{
    public byte[] ByTemp = new byte[2];
    public uint DwFlags; //控制方式，按位与；比如同时控制zoom和focus, dwFlags = FLAFS_ZOOMPOS | FLAFS_FOCUSPOS;
    public uint DwSize; //该结构的大小，必须填写
    public AnonymousClass4 Ptz = new();
    public ushort WZoomBs; //zoom倍率*100， FLAFS_ZOOMBS倍率控制模式下，机芯位置参数无效

    public AnonymousClass5 Zoom = new();

    /*云台信息*/
    //C++ TO C# CONVERTER NOTE: Classes must be named in C#, so the following class has been named by the converter:
    public class AnonymousClass4
    {
        public int DwHorData; //水平角度*1000
        public int DwVerData; //垂直角度*1000
    }

    /*机芯位置*/
    //C++ TO C# CONVERTER NOTE: Classes must be named in C#, so the following class has been named by the converter:
    public class AnonymousClass5
    {
        public int FocusData; //当前focus_pos
        public int IdZoomData; //当前dzoom_pos
        public int ZoomData; //当前zoom_pos
    }
}

/*云台控制参数如控制多长时间，旋转角度等,大小必须小于128字节*/
/*当byControlMode==1时使用8192方法且都是相对位置，当byControlMode为其它值时使用几个方向的模式*/
[StructLayout(LayoutKind.Sequential)]
public struct TmPtzParameterT
{
    public uint AutoStopTime; /*自动断开时间，如光圈控制*/
    public int ZoomValue; /*变倍*/
    public int VerticallyAngle; /*垂直*/
    public int HorizontallyAngle; /*水平*/
    public byte ZoomMode; /*0-拉近,1-拉远,2-绝对,0xFF-不控制镜头*/
    public byte VerticallyMode; /*0-垂直向上,1-垂直向下,2-绝对*/
    public byte HorizontallyMode; /*0-水平向左(仅控制位置增减),1-水平向右,2-绝对*/
    public byte ControlMode; /*控制模式: 0-使用角度*1000,1-使用坐标计算方法*8192,3-使用步数,4-使用鱼眼模式(角度，倍数x1000)*no*/
    public byte HorSpeed; /*控制水平速度1~128*/
    public byte VerSpeed; /*控制垂直速度1~128*/
    public byte GoPathMode; /*控制水平方向走的路线0-最短路线走，1-水平向左，2-水平向右，3-直接到点位置(鱼眼)*/
    public byte RectRatio; /*为了让变倍更准确，算法:A=(框宽/图像宽*100)和B=(框高/图像高*100)，byRectRatio=(A>B)?A:(B|0x80)*/
}

/*
坐标计算方法：
1、以图像中心为参考原点，将图像划分成四个部分，相对于参考原点，分别为右上、左上、左下、右下，其中右、下为正，左、上为负。
2、得到当前坐标相对于原点的值，再除以单位长度，得到相对的比例值，此比例值已经与具体的分辨率无关
3、因为得到的相对比例值一定小于等于1，为了避免为小数，统一将得到的比例值乘以8192,将乘积后的值取整，作为参数填入到协议对应字节。
举例如下：
当前分辨率为1024＊768，则图像中心点坐标为(512,384)，如果目标位置坐标为(768,576),则可知目标位置处于右上，其比例值为：
水平：
（768-512）/512 = 0.5，再乘以8192，即为4096，因为是右边，所以为4096，填入对应字节即可。
垂直：
（576-384）/384 = 0.5，再乘以8192，即为4096，因为是上边，所以为-4096,填入对应字节即可。
*/
public class TmPtzCmdModeT
{
    public byte ByRegionId;
    public byte[] ByTemp = new byte[7];
    public uint DwHorPoint;
    public uint DwVerPoint;
}

/*云台控制结构160字节*/
public class TmPtzCommandCfgT
{
    public byte ByChannelId; //控制通道号

    public byte ByPtzControlMode; /*控制模式
                                                        0-原来的方法，
                                                        1-指定通道和码流
                                                        2-指定区域
                                                        3-采用指定的图像相对于左上偏移*1000,
                                                        */

    public byte[] BySpeedTemp = new byte[2];
    public byte ByStreamId; //控制码流
    public byte[] ByTemp = new byte[40];
    public byte ByTempCmd;
    public byte DwAddress;
    public uint DwPtzCommand; //云台控制命令
    public uint DwPtzControl; //控制操作，如果是预置点命令，为预置点号
    public uint DwSize; //该结构大小，此结构实际大小可能更大
    public byte DwSpeed;

    public int PtzCodeLen; //PTZ命令码长度

    //C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
    //	union
    //	{
    //		byte pPTZCodeBuf[200]; //具体命令码值，当码值大于1时，从第2个码值开始
    // /*的缓冲应紧跟此结构后*/
    //		tmPtzParameter_t struParameter; //云台参数
    //	};
    //控制模式占16个字节
    public TmPtzCmdModeT StruCmdMode = new();
}

/*云台当前信息对应命令TMCC_MAJOR_CMD_PTZCONTROL/TMCC_MINOR_CMD_GETPTZINFO*/
/*角度需要按注释乘以相应倍数，角度根据设备不同而不同水平360度，垂直90度*/
public class TmPtzIntegrateCfgT
{
    public byte ByControlMode; //控制模式0-角度
    public byte ByCurrentPreset; //当前运行在哪个预置点上，0标示没有在预置点上,1-标示1号预置点
    public byte ByFixMode; //摄像机安装模式 0-壁挂, 1-吸顶, 2-桌面
    public byte ByPtzMode; //云台信息0-没有云台信息，1-一般机械球机，2-鱼眼，3-全景，4-鱼眼云台信息
    public int DwHorizontallyMinStation; //最小水平位置 * 100
    public int DwHorizontallyStation; //最大水平位置 * 100
    public int DwHorizontallyVisualAngle; //当前可视水平位置 * 100
    public int DwImageHorVisualAngle; //当前倍数的图像视角水平大小 * 1000
    public int DwImageVerVisualAngle; //当前倍数的图像视角垂直大小 * 1000
    public int DwMaxZoomValue; //最大倍数 * 1000
    public uint DwSize; //该结构大小，此结构实际大小
    public int DwVerticallyMinStation; //最小垂直位置 * 100
    public int DwVerticallyStation; //最大垂直位置 * 100
    public int DwVerticallyVisualAngle; //当前可视垂直位置 * 100
    public int DwZoomValue; //当前的倍数* 1000
}

public class TmPtzIntegrateListCfgT
{
    public byte ByCount; //列表数
    public byte[] ByTemp = new byte[3]; //保留
    public uint DwSize; //该结构大小，此结构实际大小
    public TmPtzIntegrateCfgT[] StruList = Arrays.InitializeWithDefaultInstances<TmPtzIntegrateCfgT>(8); //列表
}

/*物理硬盘信息结构*/
public class TmDriveInfoT
{
    public byte ByFDisk; //硬盘是否按系统模式分区
    public byte ByInit; //硬盘是否初始化
    public byte ByReset; //是否再初始化硬盘
    public byte[] ByTemp = new byte[1]; //保留
    public uint DwDriveType; //硬盘类型
    public uint DwTotalSpace; //硬盘总空间
    public uint DwUsefullSpace; //可用空间大小
}

/*物理硬盘结构*/
public class TmDriveCfgT
{
    public uint DwDriveCount; //硬盘个数
    public uint DwSize; //该结构大小
    public TmDriveInfoT[] StruDisk = Arrays.InitializeWithDefaultInstances<TmDriveInfoT>(16); //硬盘信息
}

public class TmDriveAlarmCfgT
{
    public byte ByEnableExceptionAlarm; //开启磁盘异常报警
    public byte ByEnableFullSpaceAlarm; //开启磁盘满报警
    public byte[] ByTemp = new byte[2];
    public uint DwSize; //该结构大小
}

/*视频输入配置使用命令TMCC_MAJOR_CMD_VIDEOINCFG/TMCC_MINOR_CMD_VIDEOIN*/
public class TmVideoInCfgT
{
    public byte ByAeMeteringColumn;

    /***************以上是52个字节，总共556个字节******************/
    /*曝光区域*/
    public byte[] ByAeMeteringData = new byte[256];

    /*曝光模式0-spot, 1-center,2-average,3-custom,4-上边曝光，5-下边曝光，6-左边曝光，7-右边曝光*/
    public byte ByAeMeteringMode;

    /*曝光区域的大小*/
    public byte ByAeMeteringRow;

    /*AE速度0-默认，1~255*/
    public byte ByAeSpeed;

    /*自动曝光策略0-自动，1-高光区域优先，2-低光区域优先*/
    public byte ByAeStrategyMode;

    /*AGC增益0=42dB,32=36dB,64=30dB,96=24dB,128=18dB,160=12dB,192=6dB,224-自动,225-48dB,226-54dB,227-60dB, 0xFF-/使用nMinAgc和nMaxAgc*/
    public byte ByAgc;

    public byte ByAgcTransMax;

    /*黑白模式自动切换的增益阀值，只有在自动模式有效，同时也是慢快门*/
    public byte ByAgcTransMin;

    /*50/60Hz,0-auto, 1:50Hz,2-60Hz*/
    public byte ByAntiFlickerMode;

    //自动检测曝光补偿
    public byte ByAutoCheckExposureLevel;

    /*使用自动对比度, 0关闭， 1-自动, 2-手动*/
    public byte ByAutoContrast;

    /*AWB速度0-默认，1~255*/
    public byte ByAwbSpeed;

    //暗区0~255，0自动，默认128
    public byte ByBlackLevel;

    /*背光补偿0-关，1-开*/
    public byte ByBlc;

    //YUV输出类型，0-关闭YUV，1-默认， 2-主码流， 3-从码流, 从10开始参考tmCompression_t中byResolution定义-10
    public byte ByCaptureYuvResolution;

    public byte ByColorTransMax;

    /*黑白模式自动切换的亮度阀值，只有在自动模式有效*/
    public byte ByColorTransMin;

    /*彩转黑模式0-自动，1-彩色，2-黑白*/
    public byte ByColorTransMode;

    /*强制图像颜色不否切换滤光片0-默认，1-*/
    public byte ByColorTransNoIrCut;

    /*透雾强度0~255*/
    public byte ByDefogStrength;

    /*曝光补偿是否开启*/
    public byte ByExpCompEnable;

    /*光圈曝光0-默认，1-最大光圈;100-最小光圈, 0xFF-手动;设置光圈值时最大光圈值*/
    public byte ByExposure;

    /*曝光水平( 10 ~ 200 )*/
    public byte ByExposureLevel;

    /*是否强制降照,0/1,当为0是byNoiseFilter只是一个下限值，1-为一个固定值*/
    public byte ByForceNoiseFilter;

    /*曝光区域的权重0-自动*/
    public byte ByHistRatioSlope;

    /*滤光片切换时间默认500毫秒,0-500, 1-100, 2-200, 3-300*/
    public byte ByIrCutTime;

    /*切换滤光片是否控制报警输出控制红外灯(报警输出连接红外灯), 0-不联动，1-联动报警输出1，2-联动报警输出2*/
    public byte ByIrCutTriggerAlarmOut;

    /*滤光片类型0-红波，1-蓝波, 2-x, 3-x*/
    public byte ByIrFilterType;

    /*报警输入联动红外的报警号(当报警输入>1时生效)，默认0，内部决定的报警输入,1-第一个报警输入，2-第二个报警输入*/
    public byte ByIrShutAlarmIn;

    /*红外切换模式: 0.表示自动切换模式,
    1.表示定时切换，切换时间见下面定义
    2.表示使用报警输入切换,
    3.表示用设备发送IRCUT切换命令(这里可能是球机发送的或则511A设备发送的)
    4.表示用511A设备报警切换(此模式511A设备报警必须与摄像机报警输入连接)
    0xFF为手动控制模式,
    0xFE标示与IRCUT相关参数使用tmIRCutCfg_t
    */
    public byte ByIrShutMode;

    /*红外打开时间和关闭时间*/
    public byte ByIrStartHour;
    public byte ByIrStartMin;
    public byte ByIrStopHour;

    public byte ByIrStopMin;

    /*滤光片类型0-瞬间正向, 1-瞬间反向, 2-持续正向, 3-持续反向*/
    public byte ByIrType;

    /*强光抑制功能0-关闭，1-6级别*/
    public byte ByLightInhibitionEn;

    /*曝光区域影响程度0-自动*/
    public byte ByMaxHistOffset;

    /*3D降噪0~255*/
    public byte ByMctfStrength;

    /*0-自动, 1-快门优先, 2-自动增益优先, 3-自动光圈优先, 4-手动模式, 5-亮度控制(光圈+增益)*/
    public byte ByModeSwitch;

    /*降照级别0xFF为自动降照,0,1,2,3,4,5,6,7,8  (二级降噪)*/
    public byte ByNoiseFilter;

    /*视频输入旋转0-normal,1-180,2-90,3-270,4-vflip,5-hflip*/
    public byte ByRotaeAngle180;

    /*快门模式取值范围0=1/25,1=1/30,2=1/50,3=1/60,4=1/100,5=1/120,6=1/240,7=1/480,8=1/960,9=1/1024*/
    /*10=自动,11-1/4,12-1/8,13-1/15,14-1/180,15-1/2000,16-1/4000,17-1/10000, 0xFF-使用nMinShutterSpeed/nMaxShutterSpeed设置*/
    public byte ByShutterSpeed;

    /*慢快门的帧率0-读取宽门设置的值，其它为1/fps*/
    public byte BySlowShutterFpsDiv;

    /*慢快门模式0-自动判断，1-手动设置，2-按时间布放设置*/
    public byte BySlowShutterMode;

    /*红外打开时间和关闭时间*/
    public byte BySlowShutterStartHour;
    public byte BySlowShutterStartMin;
    public byte BySlowShutterStopHour;

    public byte BySlowShutterStopMin;

    /*保留*/
    public byte[] ByTemp = new byte[217];

    /*0:VIDEO_COLOR_STYLE_FOR_PC 1:VIDEO_COLOR_STYLE_FOR_TV;*/
    public byte ByVideoColorStyle;

    /*图像是否冻结*/
    public byte ByVideoFreeze;

    /*强制设置帧率
    0-自动, 1-1, 2-2, 3-3, 4-4, 5-5, 6-6, 7-10, 8-12, 9-13, 10-14, 11-15, 12-20, 13-24, 14-25, 15-30, 16-50, 17-60, 18-120
    19-29.75, 20-59.94, 21-23.976, 22-12.5, 23-6.25, 24-3.125, 25-7.5, 26-3.75
    */
    public byte ByVinFrameRate;

    //视频输入ID号，对多数字输入有效(如0-默认，1-表示SDI输入，2-为CVBS输入)
    public byte ByVinId;

    /*宽动态模式0-关,1-自动, 2-手动, byWdrStrength=级别*/
    public byte ByWdr;

    /*宽动态模式8bit，每bit标示一种模式, 全为0表示一般模式, 0位表示启用硬件宽动态, 1位表示启用HISO模式, 2位表示启用低帧率模式*/
    public byte ByWdrMode;

    /*宽动态级别*/
    public byte ByWdrStrength;

    /*白平衡控制0-关闭,1-自动,2-白炽灯,3-室内,4-ATW,5-日光,6-阴天,7-闪光灯,8-荧光的,9-荧光的H,10-水下,11-室外,0xFF-手动*/
    public byte ByWhiteBalance;

    //明区0~255，0自动，默认128
    public byte ByWhiteLevel;

    /*该结构大小*/
    public uint DwSize;

    /*曝光水平-127~127*/
    public int ExposureCompensation;

    /*AGC增益在byAgc基础上的增量，总的AGC=byAgc+byAgcAdd*/
    public ushort NAgcAdd;

    /*增益手动值*/
    public ushort NManualAgc;

    /*快门手动值*/
    public ushort NManualShutterSpeed;

    /*AGC*10增益上限, 默认0, 为了保持原来的兼容基数为1,也就是实际AGC+1，内部会自动把1转换成0*/
    public ushort NMaxAgc;

    /*快门取值最大值0-默认,其它单位为1/x*/
    public ushort NMaxShutterSpeed;

    /*AGC*10增益下限, 默认0， 为了保持原来的兼容基数为1,也就是实际AGC+1，内部会自动把1转换成0*/
    public ushort NMinAgc;

    /*快门取值最小值0-默认,其它单位为1/x*/
    public ushort NMinShutterSpeed;

    public ushort NWhiteBalanceB;

    /*白平衡手动值，RB(0~4080)*/
    public ushort NWhiteBalanceR;
}

/*AE 参数配置*/
public class TmAeParamT
{
    public uint DwAgcValue;
    public uint DwDgain;
    public uint DwShutterIndex;
    public uint DwSize;
}

/*手动白平衡参数*/
public class TmWbGainT
{
    public uint DwB;
    public uint DwDgain;
    public uint DwG;
    public uint DwR;
    public uint DwSize;
}

/*视频输入配置使用命令TMCC_MAJOR_CMD_VIDEOINCFG/TMCC_MINOR_CMD_VIDEOINEX*/
public class TmVideoInExCfgT
{
    public byte ByAeMeteringColumn;

    /*
     * 此处特别注意，在有些机型上byAeMeteringData的96个字节不过，但HDR的曝光参数由没用，
     * 所可以使用是扩展byAeMeteringData到496个
     */
    /*曝光区域*/
    public byte[] ByAeMeteringData = new byte[96];

    /*曝光区域*/
    public byte[][] ByAeMeteringDataHdr = RectangularArrays.RectangularByteArray(4, 96);

    /*曝光模式0-spot, 1-center,2-average,3-custom, 4-tile, 5-y_order, 6-histogram, 7-EXTERN_DESIGN, 8-HISTOGRRAM*/
    public byte ByAeMeteringMode;

    /*曝光区域的大小*/
    public byte ByAeMeteringRow;

    /*AGC增益0=42dB,32=36dB,64=30dB,96=24dB,128=18dB,160=12dB,192=6dB,224-自动,225-48dB,226-54dB,227-60dB*/
    public byte ByAgc;

    public byte ByAgcTransMax;

    /*黑白模式自动切换的增益阀值，只有在自动模式有效*/
    public byte ByAgcTransMin;

    /*50/60Hz,0-关闭, 1:50Hz,2-60Hz*/
    public byte ByAntiFlickerMode;

    //自动检测曝光补偿
    public byte ByAutoCheckExposureLevel;

    /*使用自动对比度, 0关闭， 1-自动， <256为具体强度*/
    public byte ByAutoContrast;

    //暗区0~255，0自动，默认128
    public byte ByBlackLevel;

    /*背光补偿0-关，1-开*/
    public byte ByBlc;

    //YUV输出类型，0-关闭YUV，1-默认， 2-主码流， 3-从码流, 从10开始参考tmCompression_t中byResolution定义-10
    public byte ByCaptureYuvResolution;

    public byte ByColorTransMax;

    /*黑白模式自动切换的亮度阀值，只有在自动模式有效*/
    public byte ByColorTransMin;

    /*彩转黑模式0-自动，1-彩色，2-黑白*/
    public byte ByColorTransMode;

    /*光圈曝光0-最大光圈;100-最小光圈, 0xFF-手动;设置光圈值时最大光圈值*/
    public byte ByExposure;

    /*曝光水平( 10 ~ 200 )*/
    public byte ByExposureLevel;

    /*HDR曝光水平( 10 ~ 200 )*/
    public byte[] ByExposureLevelHdr = new byte[4];

    /*是否强制降照,0/1,当为0是byNoiseFilter只是一个下限值，1-为一个固定值*/
    public byte ByForceNoiseFilter;

    /*滤光片切换时间默认500毫秒,0-500, 1-100, 2-200, 3-300*/
    public byte ByIrCutTime;

    /*切换滤光片是否控制报警输出控制红外灯(报警输出连接红外灯), 0-不联动，1-联动报警输出1，2-联动报警输出2*/
    public byte ByIrCutTriggerAlarmOut;

    /*滤光片类型0-红波，1-蓝波, 2-x, 3-x*/
    public byte ByIrFilterType;

    /*报警输入联动红外的报警号(当报警输入>1时生效)，默认0，内部决定的报警输入,1-第一个报警输入，2-第二个报警输入*/
    public byte ByIrShutAlarmIn;

    /*红外切换模式: 0表示自动切换模式,1表示定时切换，切换时间见下面定义*/
    /*2表示使用报警输入切换,0xFF为手动控制模式*/
    public byte ByIrShutMode;

    /*红外打开时间和关闭时间*/
    public byte ByIrStartHour;
    public byte ByIrStartMin;
    public byte ByIrStopHour;

    public byte ByIrStopMin;

    /*滤光片类型0-瞬间正向, 1-瞬间反向, 2-持续正向, 3-持续反向*/
    public byte ByIrType;

    /*强光抑制功能0-关闭，1-6级别*/
    public byte ByLightInhibitionEn;

    /*HDR最大增益*/
    public byte[] ByMaxAgcHdr = new byte[4];

    /*3D降噪0~255*/
    public byte ByMctfStrength;

    /*0-自动, 1-快门优先, 2-自动增益优先, 3-自动光圈优先, 4-手动模式*/
    public byte ByModeSwitch;

    /*降照级别0xFF为自动降照,0,1,2,3,4,5,6,7,8  (二级降噪)*/
    public byte ByNoiseFilter;

    /*视频输入旋转0-normal,1-180,2-90,3-270,4-vflip,5-hflip*/
    public byte ByRotaeAngle180;

    /*快门模式取值范围0=1/25,1=1/30,2=1/50,3=1/60,4=1/100,5=1/120,6=1/240,7=1/480,8=1/960,9=1/1024*/
    /*10=自动,11-1/4,12-1/8,13-1/15,14-1/180,15-1/2000,16-1/4000,17-1/10000*/
    public byte ByShutterSpeed;

    /*0:VIDEO_COLOR_STYLE_FOR_PC 1:VIDEO_COLOR_STYLE_FOR_TV;*/
    public byte ByVideoColorStyle;

    /*强制设置帧率
    0-自动, 1-1, 2-2, 3-3, 4-4, 5-5, 6-6, 7-10, 8-12, 9-13, 10-14, 11-15, 12-20, 13-24, 14-25, 15-30, 16-50, 17-60, 18-120
    19-29.75, 20-59.94, 21-23.976, 22-12.5, 23-6.25, 24-3.125, 25-7.5, 26-3.75
    */
    public byte ByVinFrameRate;

    /*宽动态模式0-关,1-自动,2-内部模式1X,3-内部模式2X,4-内部模式3X,5-内部模式4X, 0xFF-默认*/
    public byte ByWdr;

    /*宽动态模式8bit，每bit标示一种模式, 全为0表示一般模式, 0位表示启用硬件宽动态, 1位表示启用HISO模式, 2位表示启用低帧率模式*/
    public byte ByWdrMode;

    /*白平衡控制0-关闭,1-自动,2-白炽灯,3-D4000,4-D5000,5-日光,6-阴天,7-闪光灯,8-荧光的,9-荧光的H,10-水下,0xFF-手动*/
    public byte ByWhiteBalance;

    //明区0~255，0自动，默认128
    public byte ByWhiteLevel;

    /*该结构大小*/
    public uint DwSize;

    /*AGC增益在byAgc基础上的增量，总的AGC=byAgc+nMaxAgc*/
    public ushort NMaxAgc;

    /*快门取值最大值0-默认,其它单位为1/x*/
    public ushort NMaxShutterSpeed;

    /*HDR快门高限*/
    public ushort[] NMaxShutterSpeedHdr = new ushort[4];

    /*HDR快门低限*/
    public ushort[] NMinShutterSpeedHdr = new ushort[4];

    public ushort NWhiteBalanceB;

    /*白平衡手动值，RB(0~16383)*/
    public ushort NWhiteBalanceR;
}

public class TmVideoDisCfgT
{
    /*是否启用防抖功能*/
    public byte ByEnableDis;

    /*该结构大小*/
    public uint DwSize;
}

/*视频输入配置使用命令TMCC_MAJOR_CMD_VIDEOINCFG/TMCC_MINOR_CMD_VIDEOAEMETERING*/
public class TmVideoAeMeteringCfgT
{
    public byte ByAeMeteringColumn;

    /*曝光区域*/
    public byte[] ByAeMeteringData = new byte[256];

    /*曝光模式0-spot, 1-center,2-average,3-custom*/
    public byte ByAeMeteringMode;

    /*曝光区域的大小*/
    public byte ByAeMeteringRow;

    public byte[] ByTemp = new byte[9];

    /*该结构大小*/
    public uint DwSize;
}

public class TmVideoAwbCfgT
{
    public byte ByCurrentLuma;

    /*是否启用*/
    public byte ByEnableRepairAwb;

    /*原始色温是参考色温还是WB数据*/
    public byte ByRefColorTemp;

    public byte ByTemp;

    /*该结构大小*/
    public uint DwSize;
    public ushort WTargetHighBGainVal;
    public ushort WTargetHighRGainVal; //高色温

    public ushort WTargetLowBGainVal;

    /*自动白平衡的修正值，当tmVideoInCfg_t中byWhiteBalance=0/1时有效*/
    //C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
    //	union
    //	{
    //		struct
    //		{
    //			ushort wOrgLowRGainVal; //低色温
    //			ushort wOrgLowBGainVal;
    //		}
    //		low;
    //		uint dwRefLowColorTemp;
    //	};
    //C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
    //	union
    //	{
    //		struct
    //		{
    //			ushort wOrgHighRGainVal; //高色温
    //			ushort wOrgHighBGainVal;
    //		}
    //		high;
    //		uint dwRefHighColorTemp;
    //	};
    public ushort WTargetLowRGainVal; //低色温
}

/*视频输入配置使用命令TMCC_MAJOR_CMD_VIDEOINCFG/TMCC_MINOR_CMD_IRCUTVIDEOIN*/
/*IRCUT切换后使用该配置*/
public class TmIrCutVideoInCfgT
{
    /*是否启用*/
    public byte ByEnable;

    /*是否启用视频参数*/
    public byte ByEnablePicPreview;

    /*是否启用*/
    public byte ByEnableVideoIn;

    /*预览 标记*/
    public byte ByPreviewMode;

    /*该结构大小*/
    public uint DwSize;

    /*视频参数*/
    public TmPicPreviewCfgT StruPicPreview = new();

    /*视频输入参数*/
    public TmVideoInCfgT StruVideoIn = new();
}

/*视频输入配置使用命令TMCC_MAJOR_CMD_VIDEOINCFG/TMCC_MINOR_CMD_SCHEDVIDEOIN*/
public class TmSchedVideoInModeT
{
    /*是否启用*/
    public byte ByEnable;

    /*是否启用视频参数*/
    public byte ByEnablePicPreview;

    /*是否启用*/
    public byte ByEnableVideoIn;

    /*预览 标记*/
    public byte ByPreviewMode;

    /*该结构大小*/
    public uint DwSize;

    /*视频参数*/
    public TmPicPreviewCfgT StruPicPreview = new();

    /*布防时间*/
    public TmSchedTimeT StruSchedTime = new();

    /*视频输入参数*/
    public TmVideoInCfgT StruVideoIn = new();

    /*模式的名称*/
    public string SzModeName = new(new char[32]);
}

public class TmSchedVideoInCfgT
{
    /*该结构大小*/
    public uint DwSize;

    /*视频输入参数*/
    public TmSchedVideoInModeT[] StruVideoInMode =
        Arrays.InitializeWithDefaultInstances<TmSchedVideoInModeT>(DefineConstants.MaxSchedVideoInMode);
}

/*红外切换时间*/
public class TmIrCutCfgT
{
    public byte ByAgcTransMax;

    /*滤光片自动切换的增益阀值，只有在自动模式有效byIRShutMode=0*/
    public byte ByAgcTransMin;

    public byte ByColorTransMax;

    /*滤光片自动切换的亮度阀值，只有在自动模式有效byIRShutMode=0*/
    public byte ByColorTransMin;

    /*切换到红外模式的滤光片模式(打开激光使用滤光片)，0-默认，1-使用红外片，2-使用透雾片，3-使用可见光滤片*/
    /*此蚕食值争对通过串口打开滤片，和IE打开灯光有效，其它按通用配置*/
    public byte ByIrCurIdByLaserCtrl;

    /*滤光片切换时间默认500毫秒,0-500, 1-100, 2-200, 3-300*/
    public byte ByIrCutTime;

    /*切换滤光片是否控制报警输出控制红外灯(报警输出连接红外灯)*/
    public byte ByIrCutTriggerAlarmOut;

    /*滤光片类型0-红波，1-蓝波, 2-x, 3-x*/
    public byte ByIrFilterType;

    /*报警输入联动红外的报警号(当报警输入>1时生效)，默认0，内部决定的报警输入,1-第一个报警输入，2-第二个报警输入*/
    public byte ByIrShutAlarmIn;

    /*红外切换模式: 0表示自动切换模式,1表示定时切换，切换时间见上面定义*/
    /*2表示使用报警输入切换,0xFF为手动控制模式*/
    public byte ByIrShutMode;

    /*红外打开时间和关闭时间*/
    public byte ByIrStartHour;
    public byte ByIrStartMin;
    public byte ByIrStopHour;

    public byte ByIrStopMin;

    /*滤光片类型0-持续正向，1-持续反向，2-瞬间正向，3-瞬间反向*/
    public byte ByIrType;

    /*IRCUT切换时是否发送状态*/
    public byte BySendIrCutState;

    /*是否切换滤光片和彩转黑, 0-默认， 1-不切换,2-强制红外滤片,3-强制透雾滤片,4-强制可见光滤片*/
    public byte BySwitchIrFilter;

    public byte[] ByTemp = new byte[7];

    /*该结构大小*/
    public uint DwSize;
}

/*曝光模式*/
/*红外切换模式*/
/*镜头光圈参数配置*/
public class TmApertureCfgT
{
    /*光圈类型0-手动光圈, 1-自动光圈, 2-限制最大光圈*/
    public byte ByApertureType;

    /*光圈的可用范围(不同镜头不一样的)*/
    public byte ByDutyDelta;

    /*限制光圈曝光0-默认，1-最大光圈;100-最小光圈;设置光圈值时最大光圈值*/
    public byte ByExposure;

    /*手动光圈大小*/
    public byte ByManualAperture;

    /*光圈值0默认, 1~100*/
    public byte ByMaxAperture;

    public byte ByMinAperture;

    /*光圈高灵明度*/
    public byte BySensitivityMode;

    /*保留*/
    public byte[] ByTemp = new byte[1];

    /*光圈的平衡值(不同镜头不一样的)*/
    public uint DwDutyBalance;

    /*该结构大小*/
    public uint DwSize;
}

/*一体机工作模式配置*/
public class TmZoomCfgT
{
    /*聚焦环境*/
    public byte ByAutoFocusEnvironment;

    /*自动聚焦扩展模式, 当byFocusMode=AF是有效, 0-AF,1-ZF,2-TF,3-AF+TF,4-ZF+TF*/
    public byte ByAutoFocusMode;

    /*自动聚焦灵明度0-默认，1-255高*/
    public byte ByAutoFocusSensivity;

    /*聚焦区域模式0-默认，1-平均，2-中心点，3-指定*/
    public byte ByAutoFocusSpot;

    /*自动聚焦阀值0-默认1-高，255-低*/
    public byte ByAutoFocusStrength;

    /*定时聚焦时间间隔，单位分钟*/
    public byte ByAutoFocusTime;

    /*自动搜索扩大范围*/
    public byte ByAutoSearchOffset;

    /*调用预置点后是否需要主动聚焦*/
    public byte ByCallPresetAutoFocus;

    /*检查场景变化*/
    public byte ByCheckEnvChange;

    /*检查场景变化阀值0-默认,1~100，越小越灵敏*/
    public byte ByCheckEnvSensivity;

    /*检查场景变化强度值0-默认,1~255，越小越灵敏*/
    public byte ByCheckEnvStrength;

    /*检查场景变化时间0-关闭，1-255时间秒*/
    public byte ByCheckEnvTime;

    /*图像清晰度检查*/
    public byte ByCheckImageMang;

    //聚焦库用的串口信息
    public byte ByCom1Port;
    public byte ByCom1SubId;
    public byte ByCom2Port;

    public byte ByCom2SubId;

    /*红外类型0-850NM, 1-950NM*/
    public byte ByCurveType;

    /*默认聚焦曲线*/
    public byte ByDefaultFocusLine;

    /*系统支持的最大变倍倍数*/
    public byte ByDigitalZoomMaxBs;

    /*数字变倍的控制方式，如果系统支持数字变倍，0-综合控制，1-单独控制*/
    public byte ByDigitalZoomMode;

    /*是否启用数字变倍*/
    public byte ByEnableDigitalZoom;

    /*启用人脸检测，聚焦到人脸*/
    public byte ByEnableFocusFace;

    /*人脸大小限制0-默认，其它*/
    public byte ByFaceLimitSize;

    /*人脸消失从新聚焦等待时间0-默认,其它秒*/
    public byte ByFaceLostTime;

    /*人脸检测刷新时间0-默认,其它秒*/
    public byte ByFaceRefreshTime;

    public byte ByFarFocusLimitEnable;

    /*最小聚焦距离: 0-默认;1-10m;2-6m;3-3m;4-2m;5-1.5m;6-1m;7-0.5m;8-0.3m;9-0.1m;10-无限远*/
    public byte ByFocusLimit;

    /*最大聚焦距离: 0-默认;1-10m;2-6m;3-3m;4-2m;5-1.5m;6-1m;7-0.5m;8-0.3m;9-0.1m;10-无限远*/
    public byte ByFocusMaxLimit;

    /*聚焦搜索最小步长*/
    public byte ByFocusMinMoveStep;

    /*当前聚焦模式0-AF,1-ZF,2-MF*/
    public byte ByFocusMode;

    /*聚焦滤波器阀值0-关闭, 1-255为滤波器参数*/
    public byte ByFocusNrStrength;

    /*是否启用如果没有聚焦上，回到1x;0:不启用；1:启用，默认1*/
    public byte ByGoto1XEnable;

    /*检测参数5-200*/
    public byte ByGoto1XVal;

    /*move到指定倍数后是否需要主动聚焦单位秒，多少秒后主动聚焦一次*/
    public byte ByGotoBsAutoFocus;

    /*聚焦启用高亮度判断*/
    public byte ByHighLumaDisable;

    /*聚焦高亮度判断阀值0-默认*/
    public byte ByHighLumaTh;

    /*图像清晰度检查阀值0-默认，其它0xFFFF百分比*/
    public byte ByImageMangThr;

    /*切换滤光片时重新效验顶点0-默认不检测，1-检测*/
    //C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
    //	union
    //	{
    //		byte byIRCutSwicthNoAutoFocus;
    //		byte byIRCutSwicthCheckTop;
    //	};
    /*切换滤光片次数0-默认*/
    public byte ByIrCutSwicthNum;

    /*IR切换是否修正曲线0-不修正，1-需要修正*/
    public byte ByIrFocusCorrection;

    /*聚焦搜索限制FV的AGC,0-模式, (当AGC>此值，byMinFvLimit生效)*/
    public byte ByMinAgcLimit;

    /*图像焦点搜索时最小值级别0-默认，其它表示*/
    public byte ByMinFvLimit;

    /*聚焦FV检查下降强度0-默认,1-255强度*/
    public byte ByMinFvLimitByMaxAgc;

    /*聚焦FV检查下降强度0-默认,1-255强度*/
    public byte ByMinFvLimitByMinAgc;

    /*聚焦限制，动态参数*/
    public byte ByNearFocusLimitEnable;

    /*聚焦过程中收到新命令是否退出0-默认不退出,其它为个数*/
    public byte ByNewCmdExitCount;

    //每次启动不检查顶点位置
    public byte ByNoCheckPiPos;

    /*聚焦参数变化后保存参数超时时间(秒)*/
    public byte ByPowerOnDataSaveTimeOut;

    /*云台转动停止后是否自动聚焦(半自动和自动有效)*/
    public byte ByPtzStopAutoFocusEnable;

    /*云台转动停止后主动聚焦等待时间(秒)*/
    public byte ByPtzStopAutoFocusTime;

    /*图像聚焦深度阀值，值越大更能达到所要求聚焦的层次0-默认,其它*/
    public byte BySearchDepth;

    /*系统是否支持数字变倍，只读变量0-不支持，1-支持*/
    public byte BySupportDigitalZoom;

    public byte ByTemp;

    /*等待云台转的那个时间(秒)0-默认*/
    public byte ByWaitPtzStopTime;

    /*切换滤片后聚焦等待时间，秒*/
    public byte ByWaitTimeForSwicthIrCut;

    /*半自动聚焦变倍后聚焦次数，默认是1次*/
    public byte ByZfAutoFocusNum;

    /*半自动聚焦变倍后聚焦间隔时间(秒)*/
    public byte ByZfAutoFocusTime;

    /*变倍过程中自动跟焦0-自动，1-不跟焦*/
    public byte ByZoomAutoFocusTrack;

    /*最大速度模式*/
    public byte ByZoomMaxSpeedMode;

    /*当zoomout是是否聚焦，必须在自动和半自动模式下*/
    public byte ByZoomOutAutoFocus;

    /*力矩大小*/
    public byte ByZoomPowerIdle;

    public byte ByZoomPowerRun;

    /*变倍后主动聚焦等待时间(200毫秒)*/
    public byte ByZoomThenAutoFocusTime;

    /*该结构大小*/
    public uint DwSize;
    public int FarFocusLimit;
    public uint NCom1Baud;
    public uint NCom2Baud;

    public int NearFocusLimit;

    /*聚焦区域，不区分视频制式，都按704x576/704x480*/
    public TmAreaScopeT StruFocusArea = new();
}

/*聚焦模式*/
/*红外灯类型*/
/*镜头畸变校正*/
public class TmLensDeWarpCfgT
{
    /*是否启用*/
    public byte ByEnable;

    /*区域选择模式*/
    public byte ByLensWarpMode;

    /*子区域点表示模式,0-水平/垂直角度,1-中心点坐标*/
    public byte BySubregionMode;

    public byte ByTemp;

    /*全景视角度数<=180, x1000 */
    public uint DwHorPanorRange;

    /*视角大小度*1000*/
    public uint DwMaxFov;

    /*鱼眼半径*/
    public uint DwMaxRadius;

    /*缩放倍数*1000*/
    public uint DwMaxZoom;

    /*该结构大小*/
    public uint DwSize;
    public AnonymousClass8 StruNotrans = new();
    public AnonymousClass6 StruPantilt = new();

    public AnonymousClass7 StruRoi = new();

    /*区域水平/垂直角度 x1000*/
    //C++ TO C# CONVERTER NOTE: Classes must be named in C#, so the following class has been named by the converter:
    public class AnonymousClass6
    {
        public int Pan;
        public int Tilt;
    }

    /*区域中心点坐标 x1000*/
    //C++ TO C# CONVERTER NOTE: Classes must be named in C#, so the following class has been named by the converter:
    public class AnonymousClass7
    {
        public int X;
        public int Y;
    }

    /*区域选择*/
    //C++ TO C# CONVERTER NOTE: Classes must be named in C#, so the following class has been named by the converter:
    public class AnonymousClass8
    {
        public short Cx;
        public short Cy;
        public short X;
        public short Y;
    }
}

/*鱼眼安装模式查询*/
public class TmLensDeWarpCapabilityCfgT
{
    public uint DwCount; //支持的安装模式数
    public uint DwSize; //本结构大小

    public AnonymousClass9[] List = Arrays.InitializeWithDefaultInstances<AnonymousClass9>(1);

    //C++ TO C# CONVERTER NOTE: Classes must be named in C#, so the following class has been named by the converter:
    public class AnonymousClass9
    {
        public byte[] ByTemp = new byte[36];
        public string SName = new(new char[64]); //名称
    }
}

/*视频输出配置结构使用命令TMCC_MAJOR_CMD_VIDEOOUTCFG/TMCC_MINOR_CMD_VIDEOOUT
 *此结构配置改变需要复位VOUT设备
 */
public class TmVideoOutCfgT
{
    //使用外部控制打开手动VOUT功能是否启用，1-生效
    public byte ByEnableManualVideoOut;

    /*是否启用视频输出*/
    public byte ByEnableVideoOut;

    public byte ByImageMode;

    /*输入的区域，全景摄像机， 255为所有区域*/
    public byte ByRegionId;

    /*输出刷新率0-默认，1-50Hz,2-60Hz*/
    public byte ByRenovator;

    // 0-默认，其它视屏输出格式+1
    public byte ByResolution;

    //保留
    public byte ByTemp;

    //视频输出时钟反向
    public byte ByVideoOutClockReverse;

    /*视频输出模式现支持YPbPr*/
    /*0-默认*/
    /*1-YPbPr(720p output)*/
    /*2-YPbPr(576p)*/
    /*3-YPbPr(576i)*/
    /*4-CVBS(576i)*/
    /*10-YPbPr, 11-Cvbs, 12-HDMI, 13-DIGITAL*/
    public byte ByVideoOutMode;

    //输出帧率是否翻倍
    public byte ByVoutX2;

    /*该结构大小*/
    public uint DwSize;

    public ushort InputBottom;

    /*视频输入偏移,处理有些显示器显示不完整情况, 此配置只对界面有效*/
    public ushort InputLeft;
    public ushort InputRight;
    public ushort InputTop;
}

/*视频输出偏移配置结构使用命令TMCC_MAJOR_CMD_VIDEOOUTCFG/TMCC_MINOR_CMD_VIDEOOUTOFFSET
 *此结构参数可是实时修改
 */
public class TmVideoOutOffsetCfgT
{
    /*2个视频窗口显示模式0-默认,1-左右模式，2-上下模式*/
    public byte By2WindowsMode;

    /*视频输出是否全屏显示,0-按比例显示，1-全屏显示*/
    public byte ByFullScreen;

    /*旋转模式0-不旋转，1-90°，2-180°，3-270°*/
    public byte ByRotateMode;

    public byte[] ByTemp = new byte[11];

    /*输出通道*/
    public byte ByVinId;

    /*限制显示窗口数，0-默认根据通道自动吹，其它的显示窗口数*/
    public byte ByWindowsNum;

    /*该结构大小*/
    public uint DwSize;

    public ushort WOffsetBottom;

    /*视频输出偏移*/
    public ushort WOffsetLeft;
    public ushort WOffsetRight;
    public ushort WOffsetTop;
}

/*　图像采集偏移微调定义 */
public class TmVideoEptzCfgT
{
    public byte ByClearBottom;
    public byte ByClearLeft;
    public byte ByClearRight;
    public byte ByClearTop;
    public uint DwSize;
    public short OffsetCx;
    public short OffsetCy;
    public short OffsetX;
    public short OffsetY;
}

/*全天录像参数配置(子结构)*/
public class TmRecordDayT
{
    /*是否全天录像 0-否 1-是*/
    public byte ByAllDayRecord;

    /*0:定时录像，1:移动侦测，2:报警录像，3:动测|报警，4:动测&报警, 5:命令触发, 6: 智能录像, 7:手动录像*/
    public byte ByRecordType;

    /*保留*/
    public byte[] ByTemp = new byte[2];
}

/*时间段录像参数配置(子结构)*/
public class TmRecordSchedT
{
    /*0:定时录像，1:移动侦测，2:报警录像，3:动测|报警，4:动测&报警, 5:命令触发, 6: 智能录像, 7:手动录像*/
    public byte ByRecordType;

    /*保留*/
    public string ByTemp = new(new char[3]);

    /*录像时间*/
    public TmSchedTimeT StruRecordTime = new();
}

/*通道本地录像参数配置*/
public class TmRecordCfgT
{
    /*录像时复合流编码时是否记录音频数据*/
    public byte ByAudioRec;

    /*是否启用录像*/
    public byte ByEnableRecord;

    /*启用多码流录像，全景摄像机有效，第1位为第一码流等*/
    public byte ByMultiStreamRecord;

    /*录像码流为从码流0-主码流，1-从码流，2-第3码流，3-第4码流*/
    public byte ByRecordStream;

    /*是否冗余录像,重要数据双备份：0/1*/
    public byte ByRedundancyRec;

    /*保留*/
    public byte[] ByTemp = new byte[3];

    /*预录时间(单位:秒)*/
    public uint DwPreRecordTime;

    /*录像保存的最长时间(单位:天)*/
    public uint DwRecorderDuration;

    /*报警延时(单位:秒，>=10秒)，录像文件大小在tmAlarmCfg.byRecoderFileSize*/
    public uint DwRecordTime;

    /*本结构大小*/
    public uint DwSize;

    /*全天录像*/
    public TmRecordDayT[] StruRecordAllDay =
        Arrays.InitializeWithDefaultInstances<TmRecordDayT>(DefineConstants.MaxDays);

    /*录像时间段*/
    public TmRecordSchedT[][] StruRecordSched =
        RectangularArrays.RectangularTmRecordSched_tArray(DefineConstants.MaxDays, DefineConstants.MaxTimeSegment);
}

/*通道FTP录像参数配置，FTP设置参考tmFTPCfg_t结构定义*/
public class TmFtpRecordCfgT
{
    /*录像时复合流编码时是否记录音频数据*/
    public byte ByAudioRec;

    /*是否启用录像*/
    public byte ByEnableRecord;

    /*启用多码流录像，全景摄像机有效，第1位为第一码流等*/
    public byte ByMultiStreamRecord;

    /*录像码流为从码流*/
    public byte ByRecordStream;

    /*预录时间(单位:秒)*/
    public uint DwPreRecordTime;

    /*报警延时(单位:秒，>=10秒)，录像文件大小在tmAlarmCfg.byRecoderFileSize*/
    public uint DwRecordTime;

    /*本结构大小*/
    public uint DwSize;

    /*全天录像*/
    public TmRecordDayT[] StruRecordAllDay =
        Arrays.InitializeWithDefaultInstances<TmRecordDayT>(DefineConstants.MaxDays);

    /*录像时间段*/
    public TmRecordSchedT[][] StruRecordSched =
        RectangularArrays.RectangularTmRecordSched_tArray(DefineConstants.MaxDays, DefineConstants.MaxTimeSegment);
}

/*
 *解码器配置结构定义
 */
/*解码器的连接信息，控制通道在Command中给出*/
public class TmConnectCfgT
{
    public byte ByConnectMode; //连接的模式0-默认连接指定信息,1-指定现有的通道号(需要根据设备是否支持)
    public byte ByDisplay; // 是否立即处理该连接，当为循环连接时有效，其它则为立即处理
    public byte ByEnableTurnServer; // 是否启用转发器连接0-关闭，1-启用
    public byte ByStreamType; // 连接码流类型
    public byte ByTemp2;
    public byte[] ByTemp4 = new byte[2];
    public byte ByTransType; // 网络传输方式
    public uint DwSize; //该结构大小
    public string SRemoteIp = new(new char[24]); //远端IP地址
    public string STurnServerIp = new(new char[24]); //转发器IP地址
    public string SzPassword = new(new char[32]); // 密码
    public string SzUserName = new(new char[32]); // 用户名
    public ushort WChannelId; // 连接的通道号
    public ushort WConnectPort; // 连接端口
    public ushort WDelayTime; // 循环时间(秒)，必须大于等于10秒
    public ushort WStreamId; // 连接的码流号
}

/*解码通道列表信息*/
public class TmConnectListCfgT
{
    public uint DwCount; //报警设备数量
    public uint DwSize; //结构大小，需要反映实际大小sizeof(tmAlarmDeviceCfg_t)*dwCount+8

    public TmConnectCfgT[]
        PConnectList = Arrays.InitializeWithDefaultInstances<TmConnectCfgT>(1); //报警设备信息列表，象征性定义一个，需要根据dwCount判断
}

/*解码通道配置*/
public class TmWindowsCfgT
{
    public byte ByEnableAudio; // 是否播放声音
    public byte ByEnableState; // 是否显示状态
    public byte ByImageQuant; // 解码质量
    public byte ByResverse; // 保留
    public uint DwSize; // 该结构大小
}

/*播放的文件结构定义*/
public class TmPlayFileCfgT
{
    public byte ByCircleNum; // 循环播放次数
    public byte ByPlayMode;
    public byte BySleepTime; // 没循环中休息时间(秒)
    public byte[] ByTemp = new byte[1]; // 保留
    public uint DwSize; // 该结构大小
    public string SzName = new(new char[128]); // 播放的文件
}

public class TmPlayWaveCfgT
{
    public uint DwSize; // 该结构大小
    public uint NAvgBytesPerSec;
    public ushort NBlockAlign;
    public ushort NChannels;
    public uint NDelayTime;
    public uint NSamplesPerSec;
    public ushort WBitsPerSample;
    public ushort WFormatTag;
}

public class TmListHeadCfgT
{
    public uint DwListSize; // 后面列表大小
    public uint DwSize; // 本结构大小
    public uint U32Flags;
    public uint U32Num;
}

/*显示窗口配置*/
public class TmAVoutControlT
{
    public ushort U16Cmd; // 控制命令
    public ushort U16Size; //该结构大小
    public byte U8SubCmd; // 子命令
    public byte[] U8Temp = new byte[2];

    public byte U8VoutId; // 输出屏幕ID
    //C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
    //	union
    //	{
    //		struct
    //		{
    //			byte u8Data1;
    //			byte u8Data2;
    //			byte u8Data3;
    //			byte u8Data4;
    //		}
    //		data;
    //		uint u32CmdData; // 控制数据
    //		tmAreaOffset_t stAreaOff; // 显示区域偏移
    //		tmRect_t stScreenRect; // 显示位置
    //		tmRect_t stRectList[64]; // 显示窗口位置
    //		tmPlayFileCfg_t stPlayFile;
    //		tmPlayWaveCfg_t stPlayWave;
    //	};
}

public class TmDisplayCfgT
{
    public byte[] ByTemp = new byte[3];
    public byte ByVoutId; // 输出屏幕ID
    public ushort U16Cmd; // 控制命令

    public ushort U16Size; //该结构大小
    //C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
    //	union
    //	{
    //		uint u32CmdData; // 控制数据
    //		tmAreaOffset_t stAreaOff; // 显示区域偏移
    //		tmRect_t stScreenRect; // 显示位置
    //		tmRect_t stRectList[64];
    //	};
}

/*连接锁定信息*/
public class TmLockCfgT
{
    public byte ByLocked;
    public byte[] ByResverse = new byte[3];
    public uint DwSize;
}

/*解码器性能*/
public class TmWindowCapabilityCfgT
{
    /*每窗口支持寻切的摄像机数*/
    public byte ByCircleCount;

    /*窗口列表*/
    public byte[] BySwitchWindowList = new byte[32];

    /*本结构大小*/
    public uint DwSize;

    /*系统支持窗口切换数*/
    public int SwitchWindowNum;
}

/*NVR的通道设置结构定义*/
/*add by zzt 2010-6-25*/
/*定义NVR中使用的通道信息*/
/*报警输入输出设备信息*/
public class TmAlarmDeviceCfgT
{
    public byte ByEnable; //是否启用
    public byte BySourceType; //只读,通道模式0-本地，1-网络
    public uint DwFactoryId; //设备的ID
    public uint DwSize; //该结构大小
    public string SAddress = new(new char[32]); //远端IP地址可以是域名
    public string SName = new(new char[32]); //通道名
    public string SPassword = new(new char[32]); //密码
    public string SUserName = new(new char[32]); //用户名
    public ushort WAlarmInBase; //报警输入通道起始通道号
    public ushort WAlarmInNum; //报警输入通道数
    public ushort WAlarmOutBase; //报警输出通道起始号
    public ushort WAlarmOutNum; //报警输出通道数
    public ushort WPort; //连接端口
}

/*通道列表信息*/
public class TmAlarmDeviceListCfgT
{
    public uint DwCount; //报警设备数量
    public uint DwSize; //结构大小，需要反映实际大小sizeof(tmAlarmDeviceCfg_t)*dwCount+8

    public TmAlarmDeviceCfgT[]
        PAlarmList = Arrays.InitializeWithDefaultInstances<TmAlarmDeviceCfgT>(1); //报警设备信息列表，象征性定义一个，需要根据dwCount判断
}

/*单个通道信息*/
public class TmChannelCfgT
{
    public byte ByChannelId; //连接的通道号
    public byte ByEnable; //该通道是否启用
    public byte BySourceType; //只读,通道模式0-本地，1-网络
    public byte ByStreamType; //连接码流类型0-只处理视频流，1-处理音视频流
    public byte BySubStream; //是否带从码流
    public byte ByTemp; //保留
    public byte ByTransType; //网络传输方式0-内部默认协议，1-RTSP协议
    public byte ByTurnServer; //是否启用转发器连接0-关闭，1-启用
    public uint DwFactoryId; //设备的ID
    public uint DwSize; //该结构大小
    public string SAddress = new(new char[32]); //远端IP地址可以是域名
    public string SName = new(new char[32]); //通道名
    public string SPassword = new(new char[32]); //密码
    public string STurnAddress = new(new char[32]); //转发器地址
    public string SUserName = new(new char[32]); //用户名
    public ushort WPort; //连接端口
    public ushort WTurnPort; //转发器端口
}

/*通道列表信息*/
public class TmChannelListCfgT
{
    public uint DwCount; //通道数量
    public uint DwSize; //结构大小，需要反映实际大小sizeof(tmChannelCfg_t)*dwCount+8

    public TmChannelCfgT[]
        PChannelList = Arrays.InitializeWithDefaultInstances<TmChannelCfgT>(1); //通道信息列表，象征性定义一个，需要根据dwCount判断
}

/*RTSP配置通道信息*/
public class TmRtspChannelNameInfoT
{
    public byte ByEnable; //是否启用
    public byte[] ByTemp = new byte[3]; //保留
    public string SName1 = new(new char[32]); //第一码流连接名
    public string SName2 = new(new char[32]); //第二码流连接名
    public string SName3 = new(new char[32]); //第三码流连接名
    public string SName4 = new(new char[32]); //第四码流连接名
}

public class TmRtspChannelNameCfgT
{
    public uint DwCount; //通道数
    public uint DwSize; //本结构大小

    public TmRtspChannelNameInfoT[]
        StruRtspName = Arrays.InitializeWithDefaultInstances<TmRtspChannelNameInfoT>(1); //RTSP名称列表
}

/*日志信息*/
public class TmLogInfoT
{
    public byte ByMajorType; //主类型 0-系统, 1-报警; 2-异常; 3-操作; 0xff-全部
    public byte ByMinorType; //次类型 0-全部;
    public byte ByTemp; //保留
    public byte ByUserLoginType; //用户登录方式0-本地，1-网络
    public uint DwSize; //本结构大小
    public string SAddress = new(new char[16]); //远程主机地址
    public string SInfo = new(new char[4]); //日志类容，象征性定义4个字节
    public AnonymousClass10 StruLogTime = new(); //日志记录时间

    public string SUserName = new(new char[32]); //操作的用户名

    //C++ TO C# CONVERTER NOTE: Classes must be named in C#, so the following class has been named by the converter:
    public class AnonymousClass10
    {
        public byte ByDay; //日
        public byte ByHour; //时
        public byte ByMinute; //分
        public byte ByMonth; //月
        public byte BySecond; //秒
        public byte ByTemp; //保留
        public ushort WYear; //年
    }
}

public class TmLogInfoListCfgT
{
    public uint DwCount; //日志条数
    public uint DwSize; //本结构大小
    public TmLogInfoT StruLogInfo = new(); //日志列表可变大小
}

/*日志操作*/
public class TmLogCfgT
{
    public uint DwSize; //本结构大小
    public TmTimeInfoT StruStartTime = new(); //日志开始时间
    public TmTimeInfoT StruStopTime = new(); //日志结束时间
}

/*摄像机经纬度配置结构配置命令TMCC_MAJOR_CMD_THEODOLITECFG*/
public class TmTheodoliteCfgT
{
    public int DwLatitude; //纬度*10000, 北纬为正数，南纬为负数(-900000~900000)
    public int DwLongitude; //经度*10000, 东经正数，西经为负数(-1800000~1800000)
    public uint DwSize; //本结构大小
}

/*摄像机设置的点信息*/
public class TmPointInfoT
{
    public ushort NxPos; //x水平方向坐标(PAL参考704*576, NTSC参考704*480)
    public ushort NyPos; //y水平方向坐标(PAL参考704*576, NTSC参考704*480)
}

/*摄像机跨线检测配置*/
public class TmSpanAlarmCfgT
{
    public uint DwSize; //本结构大小
}

//人脸信息
public class TmFaceDetectInfoT
{
    public uint DwSize; //本结构大小
    public int FaceNum;
    public int ImageHeight;
    public int ImageWidth;

    public AnonymousClass11[] List = Arrays.InitializeWithDefaultInstances<AnonymousClass11>(32);

    //C++ TO C# CONVERTER NOTE: Classes must be named in C#, so the following class has been named by the converter:
    public class AnonymousClass11
    {
        public ushort Cx;
        public ushort Cy;
        public ushort X;
        public ushort Y;
    }
}

/*数据流信息*/
public class TmRealStreamInfoT
{
    public byte BInterlacer;
    public int Buffer2Size; //数据2大小
    public int Buffer3Size; //数据3大小
    public int Buffer4Size; //数据4大小
    public int Buffer5Size; //数据5大小
    public int Buffer6Size; //数据6大小
    public int Buffer7Size; //数据7大小
    public int Buffer8Size; //数据8大小
    public int BufferSize; //数据大小
    public byte ByFrameType; //帧类型0-视频，1-音频，2-数据流头
    public byte ByKeyFrame; //是否关键帧
    public byte ByNeedReset; //是否需要复位解码器
    public byte ByStreamNo; //码流号
    public byte[] ByTemp = new byte[3];
    public uint DwBitRate; //此数据流的码流大小
    public uint DwFactoryId; //厂家ID
    public uint DwPlayTime; //此帧播放时间(单位毫秒)
    public uint DwSize; //本结构大小
    public uint DwStreamId; //流ID

    public uint DwStreamTag; //流类型Tag

    //C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
    //	union
    //	{
    //		int iWidth; //视频宽
    //		int iSamplesPerSec; //音频采样率
    //	};
    //C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
    //	union
    //	{
    //		int iHeight; //视频高
    //		int iBitsPerSample; //音频采样位数
    //	};
    //C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
    //	union
    //	{
    //		struct
    //		{
    //			short iFrameRate; //帧率*1000，如果<1000就是实际的帧率不用*1000
    //			short iDisplayFrameRate; //帧率*1000，如果<1000就是实际的帧率不用*1000
    //		}
    //		framerate;
    //		int iChannels; //音频的声道数
    //	};
    /*add by 2009-0429*/
    //C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
    //	union
    //	{
    //		uint nDisplayScale; //显示比例*1000
    //	};
    public uint DwTimeStamp; //时间戳(单位毫秒)
    public int MotionRectSize;

    public byte[] PBuffer; //数据缓冲

    //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
    //ORIGINAL LINE: byte* pBuffer2;
    public byte PBuffer2; //数据2缓冲

    //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
    //ORIGINAL LINE: byte* pBuffer3;
    public byte PBuffer3; //数据3缓冲

    //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
    //ORIGINAL LINE: byte* pBuffer4;
    public byte PBuffer4; //数据4缓冲

    //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
    //ORIGINAL LINE: byte* pBuffer5;
    public byte PBuffer5; //数据5缓冲

    //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
    //ORIGINAL LINE: byte* pBuffer6;
    public byte PBuffer6; //数据6缓冲

    //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
    //ORIGINAL LINE: byte* pBuffer7;
    public byte PBuffer7; //数据7缓冲

    //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
    //ORIGINAL LINE: byte* pBuffer8;
    public byte PBuffer8; //数据8缓冲

    public TmFaceDetectInfoT PFaceDetect; //人脸检测结果

    //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
    //ORIGINAL LINE: byte* pMotionRectList;
    public byte PMotionRectList; //移动侦测信息
}

/*数据流头信息*/
public class TmAvInfoT
{
    /*音频*/
    public byte ByAudio; //是否采集音频
    public byte ByAudioIndex; //码流序号
    public byte ByBitsPerSample; //音频采样位数
    public byte ByChannels; //音频的声道数
    public byte ByInterlacer; //视频是Interlacer

    public byte ByStreamId; //码流号

    /*视频*/
    public byte ByVideo; //是否带视频
    public byte ByVideoIndex; //码流序号
    public uint DwAudioBitRate; //此音频流的码流大小
    public uint DwAudioId; //音频流ID
    public uint DwAudioTag; //音频流类型Tag
    public uint DwFrameRate; //帧率*1000
    public uint DwSampleSize; //一个音频包包含多少帧
    public uint DwSamplesPerSec; //音频采样率
    public uint DwTempA;
    public uint DwVideoBitRate; //此数据流的码流大小
    public uint DwVideoId; //流ID
    public uint DwVideoTag; //流类型Tag
    public ushort NDisplayScale; //显示比例*1000
    public ushort NHeight; //视频高
    public ushort NTempV;
    public ushort NWidth; //视频宽
}

public class TmRealStreamHeadCfgT
{
    public uint DwSize; //本结构大小
    public ushort NFactoryId; //厂家ID
    public uint NFileTotalTime; //文件总时间
    public ushort NStreamNum; //码流数
    public TmAvInfoT[] StruStream = Arrays.InitializeWithDefaultInstances<TmAvInfoT>(8); //数据流音视频信息
}

/*
 *多码流数据结构定义
 */
public class TmMultiStreamInfoT
{
    public byte ByCurrentStream; //解码显示码流，默认是0
    public byte ByEnableMultiView; //启用多码流同时显示
    public byte ByStreamNum; //码流中的码流数
    public byte ByTemp; //保留
    public uint DwSize; //本结构大小
    public TmAvInfoT[] Stream = Arrays.InitializeWithDefaultInstances<TmAvInfoT>(8); //码流信息列表

    public AnonymousClass12[] View = Arrays.InitializeWithDefaultInstances<AnonymousClass12>(8);

    //C++ TO C# CONVERTER NOTE: Classes must be named in C#, so the following class has been named by the converter:
    public class AnonymousClass12
    {
        public byte ByAutoFitWindow; //显示的模式0-使用rcView显示，1-当窗口自适应模式，2-2窗口自适应模式

        public byte ByLockDisplayScale; //是否锁定显示比例

        /*3-4窗口自适应模式, 4*9窗口自适应模式*/
        public byte ByWindowIndex; //多窗口自适应模式的窗口id
        public uint HWnd; //显示窗口句柄
        public TmRectT RcView = new(); //显示相对于窗口的位置
    }
}

/*
 *文件播放
 */
/*文件搜索条件定义*/
public class TmFindConditionCfgT
{
    public byte ByBackupData; //是否搜索备份文件
    public byte ByChannel; //搜索的通道
    public byte ByEnableServer; //是否启用网络参数
    public byte ByFileType; //搜索类型 0xFF-全部，'N'-定时，'M'-移动，'A'-报警，'H'-手动，'O'-其它
    public byte ByOldServer; //是否是老设备
    public byte BySearchAllTime; //搜索所有时间文件
    public byte BySearchImage; //是否搜索图片
    public byte ByTemp;
    public uint DwServerPort; //服务器端口
    public uint DwSize; //本结构大小
    public string SServerAddress = new(new char[32]); //服务器地址
    public TmTimeInfoT StruStartTime = new(); //搜索的开始时间
    public TmTimeInfoT StruStopTime = new(); //搜索的结束时间
    public string SUserName = new(new char[32]); //用户名
    public string SUserPass = new(new char[32]); //用户密码
}

/*搜索到的录像文件信息*/
public class TmFindFileCfgT
{
    public byte ByBackupData; //是否是备份文件
    public byte ByChannel; //搜索的通道
    public byte ByDiskName; //所在磁盘
    public byte ByFileFormat; //文件格式
    public byte ByImage; //文件是否为图片
    public uint DwFileSize; //文件的大小(字节表示，所以录像文件不能太大)
    public uint DwFileTime; //文件时间，毫秒
    public uint DwSize;
    public string SFileName = new(new char[64]); //文件名
    public TmTimeInfoT StruStartTime = new(); //文件的开始时间

    public TmTimeInfoT StruStopTime = new(); //文件的结束时间

    //C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
    //	union
    //	{
    //		byte byAlarmType; //搜索类型 0xFF-全部，'N'-定时，'M'-移动，'A'-报警，'H'-手动，'O'-其它
    //		byte byFileType; //搜索类型 0xFF-全部，'N'-定时，'M'-移动，'A'-报警，'H'-手动，'C'-命令(断网), 'Z'-智能，'B'动测&报警,'O'-其它
    //	};
    public ushort WFactoryId; //厂商ID
}

/*文件播放回调接口定义*/
/*文件访问接口回调结构定义*/
public class TmFileAccessInterfaceT
{
    public delegate int CloseDelegate(IntPtr hFile);

    public delegate IntPtr OpenDelegate(string lpFileName, string lpMode, object context);

    public delegate int ReadDelegate(IntPtr hFile, object lpBuffer, int nRead);

    public delegate uint SeekDelegate(IntPtr hFile, int offset, int origin);

    public delegate uint SizeDelegate(IntPtr hFile);

    public delegate int WriteDelegate(IntPtr hFile, object lpBuffer, int nWrite);

    public CloseDelegate Close;
    public OpenDelegate Open;
    public ReadDelegate Read;
    public SeekDelegate Seek;
    public SizeDelegate Size;
    public WriteDelegate Write;
}

/*文件索引结构定义*/
public class TmAvIndexEntryT
{
    public uint Ckid;
    public uint DwChunkLength;
    public uint DwChunkOffset;
    public uint DwFlags;
}

/*索引中dwFlags值*/
/*远程文件访问接口回调结构定义*/
public class TmRemoteFileInfoT
{
    /*读取文件帧回调*/
    public delegate int ReadDelegate(IntPtr hObject, object lpBuffer, int nRead, ref uint dwCodeTag,
        ref int nNeedBufSize, object context);

    /*设置文件位置*/
    public delegate int SeekDelegate(IntPtr hObject, int offset, int origin, ref int iPosition, ref uint iTimeStamp,
        object context);

    /*解码方式*/
    public byte ByDecoderType;

    /*本结构大小*/
    public uint DwSize;
    public int HeadSize;

    public int IndexCount;

    /*回调关联指针*/
    public object PContext;

    /*打开的文件头*/
    //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
    //ORIGINAL LINE: byte* pHeadBuf;
    public byte PHeadBuf;

    /*索引*/
    public TmAvIndexEntryT PIndexBuf;
    public ReadDelegate Read;

    public SeekDelegate Seek;

    /*总帧率*/
    public int TotalFrames;

    /*总时间*/
    public int TotalTime;
}

/*文件播放条件定义*/
public class TmPlayConditionCfgT
{
    /*多码流显示回调*/
    //C++ TO C# CONVERTER TODO TASK: The original C++ function pointer contained an unconverted modifier:
    //ORIGINAL LINE: int(CALLBACK *fnMultiStreamCallBack)(System.IntPtr hTmCC, tmMultiStreamInfo_t* pMultiStream, object* context);
    public delegate int FnMultiStreamCallBackDelegate(IntPtr hTmCc, TmMultiStreamInfoT pMultiStream, object context);

    /*数据回调函数回调函数*/
    //C++ TO C# CONVERTER TODO TASK: The original C++ function pointer contained an unconverted modifier:
    //ORIGINAL LINE: int(CALLBACK *fnStreamReadCallBack)(System.IntPtr hTmCC, tmRealStreamInfo_t* pStreamInfo, object* context);
    public delegate int FnStreamReadCallBackDelegate(IntPtr hTmCc, TmRealStreamInfoT pStreamInfo, object context);

    //C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
    //	union
    //	{
    // /*服务器文件全是用此结构148*/
    //		struct
    //		{
    //			tmTimeInfo_t struStartTime; //文件的开始时间
    //			tmTimeInfo_t struStopTime; //文件的结束时间
    //			byte byCheckStopTime; //是否检测结束时间
    //			byte byAlarmType; //报警类型
    //			byte byFileFormat; //0-JPEG,1-JPEG2000,2-RGB555,3-RGB565,4-RGB24,
    // /*5-RGB32,6-YUV444,7-YUV422,8-YUV420,9-BKMPEG4,10-H264文件格式20-AVI,21-MKV*/
    //			byte byBackupData; //是否是备份文件
    //			byte byDiskName; //所在磁盘
    //			byte byConvertToJpeg; //非JPEG强制转换成JPEG
    //			char byReserves[18];
    //			char sServerAddress[32]; //服务器地址
    //			uint dwServerPort; //服务器端口
    //			char sUserName[32]; //用户名
    //			char sUserPass[32]; //用户密码
    //		}
    //		time;
    // /*本地文件是用此结构148*/
    //		struct
    //		{
    //			byte byAutoCreateIndex; //是否自动生成索引
    //			byte byAutoPlay; //打开后是否自动播放
    //			byte byTemp[2];
    //			char sFileName[128]; //文件名
    //			tmFileAccessInterface_t* pFileCallBack; //文件访问回调函数
    //			object* pFileContext; //文件访问相关句柄
    //			tmAvIndexEntry_t* pAvIndex; //索引缓冲
    //			int iAvIndexCount; //缓冲中的索引数
    //		}
    //		file;
    //	}
    //C++ TO C# CONVERTER NOTE: Access declarations are not available in C#:
    //	info;
    public byte ByBufferBeforePlay; //开始播放是否需要缓冲数据
    public byte ByChannel; //通道
    public byte ByDecoderType; //解码方式
    public byte ByEnableServer; //是否启用网络参数
    public byte ByPlayImage; //是否操作图片
    public byte ByPlayType; //播放方式
    public uint DwBufferSizeBeforePlay; //缓冲大小
    public uint DwSize;
    public FnMultiStreamCallBackDelegate FnMultiStreamCallBack;
    public object FnMultiStreamContext;
    public FnStreamReadCallBackDelegate FnStreamReadCallBack;
    public object FnStreamReadContext;
    public ushort WFactoryId; //厂商ID
}

/*远程文件播放方式*/
/*文件下载条件定义*/
public class TmDownloadFileCfgT
{
    public uint DwSize; //本结构大小
    public string SFileName = new(new char[128]); //本地文件名
    public TmPlayConditionCfgT StruCondition = new(); //文件条件
}

/*远程文件打开控制结构定义*/
public class TmPlayControlCfgT
{
    public uint DwCommand; //控制命令

    public uint DwSize; //本结构大小
    //C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
    //	union
    //	{
    //		tmTimeInfo_t struTime; //文件的开始时间
    //		int iPlayData; //播放参数
    //		int iSpeed; //播放的速度
    //		int iEnableAudio; //音频开关
    //		int iCurrentPosition; //新的播放位置(帧)
    //		uint dwCurrentTime; //新的播放位置(毫秒)
    //		bool bForward; //前进单帧
    //		bool bClearDisplay; //清空显示
    //		bool bAutoResetBufTime; //是否自动调节缓冲
    //		struct
    //		{
    //			byte byAutoCreateIndex; //是否自动生成索引
    //			byte byAutoPlay; //打开后是否自动播放
    //			byte byTemp[2];
    //			char sFileName[128]; //切换到文件名
    //		}
    //		file;
    //		struct
    //		{
    //			tmAvIndexEntry_t* pAvIndex; //索引缓冲
    //			int iAvIndexCount; //缓冲中的索引数
    //			int iAvIndexMaxCount; //缓冲的总索引数
    //		}
    //		index;
    //	}
    //C++ TO C# CONVERTER NOTE: Access declarations are not available in C#:
    //	control;
}

/*播放文件的当前信息*/
public class TmPlayStateCfgT
{
    public byte ByCurrentState; //当前播放状态
    public byte ByIndex; //当前文件下载数
    public byte ByResetFile; //需要复位时间戳
    public byte ByResetTime; //需要复位时间戳
    public uint DwCurrentFrame; //当前帧数
    public uint DwCurrentSize; //当前文件大小
    public uint DwCurrentTimes; //当前时间(毫秒)
    public uint DwSize; //本结构大小
    public uint DwTotalFrames; //总共帧数
    public uint DwTotalSize; //总文件大小
    public uint DwTotalTimes; //总时间(毫秒)
    public TmTimeInfoT StruStartTime = new(); //当前播放文件的开始时间
}

/*数据流播放参数结构定义*/
/*文件播放条件定义*/
public class TmPlayRealStreamCfgT
{
    public byte ByCacheStream; //zzt:连接缓冲码流
    public byte ByChannel; //通道
    public byte ByConnectType; //连接类型0-TcpIp,1-Udp,2-Dialing,3-TcpMulti,4-UdpMulti
    public byte ByDecoderType; //解码方式
    public byte ByForceDecode; //当现实窗口为空时是否需要强制解码输出
    public byte ByMultiStream; //多码流列表，第一位标示第1码流，第二位标示第2码流...
    public byte ByReConnectNum; //从连次数
    public byte ByStream; //码流号
    public byte ByStreamFlags; //连接移动码流0-默认,1-鱼眼的移动码流，2-温度数据, 3-缓冲码流,4-智能码流,5-vout的回写码流
    public byte ByTemp;
    public byte ByTransProtocol; //传输协议0-内部自定,1-SONY,2-RTSP
    public byte ByTranstType; //传输类型
    public uint DwSize;
    public int Port; //服务器连接的端口
    public int ReConnectTime; //重连的时间间隔
    public string SzAddress = new(new char[32]); //连接服务器的IP地址
    public string SzParameter = new(new char[128]); //连接参数
    public string SzPass = new(new char[32]); //登录用户口令
    public string SzTurnAddress = new(new char[32]); //转发器地址
    public string SzUser = new(new char[32]); //登录用户名
    public int TranstPackSize; //传输包大小
}

/*音视频原始帧信息*/
public class TmAvImageInfoT
{
    public byte Face; //是否有人脸检测结果，有那么yuv[3]是tmFaceDetectInfo_t指针

    //C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
    //	union
    //	{
    //		byte* yuv[4]; // 当输出格式为YUV420时，yuv[0] Y数据；yuv[1] U数据 yuv[2] V数据，当face==1时yuv[3]为人脸检测结果指针
    //		byte* buffer; //RGB格式使用这个指针，一行的字节数对应bufsize
    //	}
    //C++ TO C# CONVERTER NOTE: Access declarations are not available in C#:
    //	data;
    //C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
    //	union
    //	{
    //		int linesize[4]; //分别对应YUV一行数据的字节数，可能比图像宽度大，比如格式为YUV420，那么 linesize[0] Y的宽度；linesize[1] U的宽度 ；linesize[2] V的宽度
    //		int bufsize; //一行数据的字节数，比如格式为RGB24，那么这个值一般为width乘以3
    //	}
    //C++ TO C# CONVERTER NOTE: Access declarations are not available in C#:
    //	size;
    //C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
    //	union
    //	{
    //		struct
    //		{
    //			short width;
    //			short height;
    //			int framerate;
    //			byte format;
    //			byte temp[3];
    //		}
    //		video;
    //		struct
    //		{
    //			int samplespersec;
    //			byte channels;
    //			byte bitspersample;
    //		}
    //		audio;
    //		byte temp[16];
    //	}
    //C++ TO C# CONVERTER NOTE: Access declarations are not available in C#:
    //	format;
    public int KeyFrame;
    public byte[] Temp = new byte[2];
    public uint Timestamp;
    public byte Video; //是否是视频
}

public enum EnumRecordStatus
{
    RecordStatusStop = 0x00,
    RecordStatusPause,
    RecordStatusWorking,
    RecordStatusAbnormal
}

public class TmReportRecordStatusT //TMCC_MINOR_CMD_STORAGESTATUS
{
    public byte ByRecordType;
    public uint DwSize; //本结构大小
    public EnumRecordStatus EnRecordStatus;
}

public class TmPtzSoftVersionT //TMCC_MINOR_CMD_PTZ_VERSION
{
    public byte[] ByUnuse = new byte[2];
    public uint DwSize; //本结构大小
    public uint[] DwUnuse = new uint[2];
    public ushort WptzSoftVersion;
}

public class TmPtzExFuncCfgT //TMCC_MINOR_CMD_PTZ_CFG_EX
{
    public byte ByIrisCmdCtrlLaserDisEn; //关闭原有的光圈控制功能 0:正常IRIS控制 1:关闭正常控制 发送球机
    public byte ByLightReverse; //激光灯是否反向
    public byte ByOsdDisplayPresetEn; //0:不显示,1:在OSD上显示当前调用的预置点
    public byte ByPtzAutoAddSpeed; //上下左右命令自动加速 0:不加速,其它加速等级
    public byte ByQueryAlarmDisEn; //取消报警主动查询功能 0:主动查询 1:不主动查询

    public byte ByRecvProtocal; //当3D选择5时，pm2.5使能才有效

    /*485接收协议，0-原始控制协议，
    1-PM2.5, 2-OSD叠加，3-VDM字符协议,4-VISCA8300协议*/
    public byte ByRefuse485; //是否接收数据
    public byte BySendIrCutStatusDisEn; //强制关闭IRCUT上报使能 0:上报  1:不上报
    public byte ByUartAlarmDisEn; //串口模式报警使能       0:开启串口报警输入输出  1:关闭
    public byte[] ByUnuse = new byte[2];
    public byte ByYtjCmdSendToPanDisEn; //控制机芯命令发送到，0,发送，1不发送
    public uint DwPtzOrgH; //保存PTZ原始坐标位置信息
    public uint DwPtzOrgV; //保存PTZ原始坐标位置信息
    public uint DwSize; //本结构大小
}

/*add by 2014-05-27*/
/*#define TMCC_MINOR_CMD_SCREENINFO					0x0A		解码器屏幕描述*/
public class TmScreenInfoT
{
    public byte ByChanCount; //通道数
    public byte ByScreenId; //屏幕ID
    public byte ByScreenType; //0(4:3) 1(16:9)
    public byte ByStartNumber; //通道起始编号
}

public class TmScreenInfoCfgT
{
    public uint DwCount; //屏幕数
    public uint DwSize; //结构大小
    public TmScreenInfoT[] StruScreen = Arrays.InitializeWithDefaultInstances<TmScreenInfoT>(1);
}

/*
 * 命令：TMCC_MAJOR_CMD_VIDEOPARAMCFG //add by tzh
 * 用TMCC_SetConfig设置显示图像参数，句柄是正在播放视频的句柄，无需先TMCC_Connect
 **/
public class TagVideoParamCfgT
{
    public int Brightness; // 亮度            0-255
    public byte[] BUsed = new byte[4]; // bUsed[0]开关亮度、对比度、Gama调整；bUsed[1]开关饱和度、色度调整；bUsed[2]bUsed[3]保留
    public int Contrast; // 对比度        0-255
    public uint DwSize; // 本结构大小
    public float FGama; // gama调整        0.1-3.0
    public int Hue; // 色度            0-255
    public int Saturation; // 饱和度        0-255
    public int Threshold; // 对比度阈值    0-255
}

/*透雾参数配置  Add by TZH 2014-02-21*/
public class TagDefogParamsT
{
    public int BeRestoration; //启用默认值；0-可配置状态，1-恢复默认值，后面的参数无效；
    public double DEps; //图像中轮廓边缘处理程度0.001-1；默认0.005
    public double DGama; //亮度调整，0.01-5.0，=1亮度不变，<1图像变暗，>1图像变亮，默认值1.3
    public double DSubSample; //下采样率
    public double DThreshold; //主要用于远处或天空区域处理程度，值越大处理程度越小0.1-1.0；默认0.2
    public int ImRadius; //最小值滤波半径
    public int Level; //透雾处理程度1-100；默认82、90
    public int Radius; //滤波半径；10-200；默认40、90；
}

public class TagRoiT
{
    public int Height;
    public int Width;
    public int X;
    public int Y;
}

public class TagDefogRoisT
{
    public int RoiCounts; //区域数量,0停止区域处理,整体透雾
    public TagRoiT[] StuRois = Arrays.InitializeWithDefaultInstances<TagRoiT>(8); //透雾的区域 暂定支持8个子区域
}

//第三方智能参数
public class TagThirdAnalyseCfgT
{
    public byte[] AnalyseCfgbuf = new byte[DefineConstants.ThirdAnalyseBufSize];

    //本结构大小
    public uint DwSize;
}

/*导入导出预置点位置 TMCC_MINOR_CMD_PTZPRESETINFO*/
public class TagPtzPresetPosInfoT
{
    public byte ByEn;
    public byte ByNo;
    public byte[] ByTmp = new byte[1];

    public byte ByType; //0:3D方式预置点 其它为云台预置点，并指定tmPtzPreset->byValue的长度

    //本结构大小
    public uint DwSize;
    public byte[] SzPresetName = new byte[128];

    public AnonymousClass13 TmZoomPos = new();

    //C++ TO C# CONVERTER NOTE: Classes must be named in C#, so the following class has been named by the converter:
    public class AnonymousClass13
    {
        public int DigitalZoom; //当前数字放大倍数*100
        public int Focus; //当前后焦*100
        public int IrType; //新的对焦库,要求预置位同时保存IR状态
        public int Zoom; //当前放大倍数*100
    }
    //C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
    //	union
    //	{
    //		struct
    //		{
    //			int iPanPos;
    //			int iTiltPos;
    //		}
    //		tm3DPreset;
    //		struct
    //		{
    //			byte byValue[8];
    //		}
    //		tmPtzPreset;
    //	}
    //C++ TO C# CONVERTER NOTE: Access declarations are not available in C#:
    //	tmPtzPos;
}

/*ZOOM 位置列表*/
public class TmZoomPosInfoT
{
    public uint DwDistance; //距离
    public int FocusPos; //focus
    public int Temp; //温度信息
    public int ZoomBs; //倍数
    public int ZoomPos; //zoom位置
}

public class TmZoomPosCfgT
{
    public uint DwCount;
    public uint DwSize;
    public TmZoomPosInfoT[] PPosInfoList = Arrays.InitializeWithDefaultInstances<TmZoomPosInfoT>(1);
}

/********************************************************************
add by zzt: 2018-02-05
图形绘制相关
********************************************************************/
public class TmGsColorT
{
    public byte Ia; //alpha 保留
    public byte Ib;
    public byte Ig;
    public byte Ir;
}

/*划线数据结构*/
public class TmGsLineT
{
    public uint DwThick; //线宽
    public TmGsColorT StColor = new(); //颜色
    public TmPointT StStartPoint = new();
    public TmPointT StStopPoint = new();
}

/*画矩形数据结构*/
public class TmGsRectT
{
    public uint DwThick;
    public TmGsColorT StColor = new();
    public TmRectT StRect = new();
}

/*画多边形数据结构*/
public class TmGsPolyT
{
    public uint DwPointNum;
    public uint DwThick;
    public TmGsColorT StColor = new();
    public TmPointT[] StPoints = Arrays.InitializeWithDefaultInstances<TmPointT>(DefineConstants.MaxPointNum);
}

/*画圆数据结构*/
public class TmGsCircleT
{
    public uint DwThick;
    public int Radius; //半径
    public TmPointT StCenter = new();
    public TmGsColorT StColor = new();
}

/*画椭圆数据结构*/
public class TmGsEllipseT
{
    public int Angle;
    public uint DwThick;
    public int EndAngle;
    public int StartAngle;
    public TmPointT StCenter = new();
    public TmGsColorT StColor = new();
    public TmSizeT StSize = new(); //半径
}

/*文字绘制数据结构*/
public class TmGsTextT
{
    public byte ByBottomLeftOrigin;
    public byte ByScaleRation; //放大倍数 * 10
    public byte[] ByTemp = new byte[2];
    public uint DwThick;
    public int FontType;
    public TmGsColorT StColor = new();
    public TmPointT StPosition = new();
    public string SzText = new(new char[64]);
}

/*文字大小数据结构*/
public class TmGsTextSizeT
{
    public byte ByBottomLeftOrigin;
    public byte ByScaleRation; //放大倍数 * 10

    public byte[] ByTemp = new byte[2];

    /*本结构大小*/
    public uint DwSize;
    public uint DwThick;
    public int FontType;
    public TmSizeT StSize = new();
    public string SzText = new(new char[128]);
}

/*十字绘制数据结构*/
public class TmGsCrossT : TmGsTextT
{
    public byte ByCrpssThick;
    public byte ByCrpssType; //0=+, 1=
    public int Radius; //半径
    public TmPointT StCenter = new();
}

/*智能跟踪框绘制数据结构*/
public class TmGsTraceT
{
    public uint DwThick;
    public int Radius; //半径
    public TmPointT StCenter = new();
    public TmGsColorT StColor = new();
}

/*************************智能方面定义******************************/
/********************************************************************
add by zzt: 2019-01-21
智能、跟踪相关
********************************************************************/
/*智能视频源参数配置，这里同样对以前的跨线，遗留物等有效*/
public class TmAiVideoInCfgT
{
    /*视频分析方式,0-默认(内部智能分析)，1-外部智能分析,2-内部KCF跟踪,3-外部神经网络跟踪,4-内部神经网络跟踪*/
    public byte ByAiTracer;

    //C++ TO C# CONVERTER TODO TASK: C# does not allow bit fields:
    public byte ByComId;

    //与分析板通讯的串口号0-默认，1-第1个串口,2-第1个串口
    //C++ TO C# CONVERTER TODO TASK: C# does not allow bit fields:
    public byte ByComSubId;

    //分析者与本机通讯模式0-不通讯，1-串口通讯，2-网络通讯
    public byte ByCtrlMode;

    /*抓图中是否叠加智能分析数据*/
    public byte ByDrawAiToCapture;

    /*智能分析是否启用，总开关，目标识别，智能跟踪自己有启动标识*/
    public byte ByEnable;

    /*是否启用视频缓冲叠加信息*/
    public byte ByEnableImageBuffer;

    /*视频缓冲级别，值越大越同步，但是延时越大(0-32)*/
    public byte ByImageBufferThr;

    /*分析者需要的视频格式0-默认(YUV420),1-H264,2-MJPEG,3-H265,4-temperature,10-YPbPr,11-cvbs,12-hdmi,13-Digital*/
    public byte ByImageFormat;

    //视频的大小,+1与编码信息的参数byResolution一致，0-默认
    public byte ByResolution;

    public byte[] ByTemp = new byte[1];

    /*视频源,0-默认(摄像机自动选择), 1-可见光，2-热成像*/
    public byte ByVideoId;

    /*视频源,0-默认(本机Vin), 1-网络视频(需要解码HI3559支持)*/
    public byte ByVideoSource;

    /*本结构大小*/
    public uint DwSize;

    /*图片大小高0-默认*/
    public ushort NImageHeight;

    /*图片大小宽0-默认*/
    public ushort NImageWidth;

    /*识别区域*/
    public TmRectT StRect = new();
}

/* 人脸检测配置 */
public class TmFaceDetectCfgT
{
    public byte ByDrawResult; // 主码流显示人脸
    public byte ByEnable; // 是否启动 ,0-否,1-是
    public byte[] ByTemp = new byte[2];
    public uint DwSize;
}

//人员信息配置
public class TmAiPersonCfgT
{
    public byte ByImageNum; // 只读可以读取的人脸张数
    public byte[] ByTemp = new byte[2];
    public byte ByType; // 人员类型0-一般人员，1-白名单人员，2-黑名单人员
    public uint DwListSize; // 列表大小
    public uint DwSize;
    public string SzAudioName = new(new char[32]); // 识别出来声音提示名
    public string SzGuid = new(new char[32]); // 识别码(如身份证)
    public string SzName = new(new char[32]); // 人名这里定义32个字节，表示人名可能很长
}

public class TmAiPersonListCfgT
{
    public uint DwListSize; // 后面列表大小
    public uint DwSize; // 本结构大小
    public uint U32Flags;
    public uint U32Num;
}

public class TmAiFaceListCfgT : TmAiPersonCfgT
{
    public uint[] U32ImageIdList = new uint[1];
}

public class TmAiFaceCfgT
{
    public uint DwSize;
    public AnonymousClass14 StRect = new();
    public string SzGuid = new(new char[32]); // 识别码(如身份证)
    public string SzName = new(new char[32]); // 人名这里定义32个字节，表示人名可能很长
    public ushort U16ImageHeight;
    public ushort U16ImageWidth;
    public uint U32FaceSize; // 人脸特征数据大小(128*4)，具体数据在本结构后面
    public uint U32ImageId; // 图片唯一ID

    public uint U32ImageSize; // 图片数据大小，具体数据在本结构后面

    //C++ TO C# CONVERTER NOTE: Classes must be named in C#, so the following class has been named by the converter:
    public class AnonymousClass14
    {
        public ushort Cx;
        public ushort Cy;
        public ushort X;
        public ushort Y;
    }
}

/*车牌识别*/
public class TmPlateDetectCfgT
{
    public byte ByEnable; // 是否启动 ,0-否,1-是
    public byte[] ByTemp = new byte[3];
    public uint DwSize;
}

/* 球机智能跟踪，需要一体机机芯才生效 */
public class TmAptitudeScoutCfgT
{
    public byte ByEnable; //是否启用
    public byte[] ByTemp = new byte[2]; //保留
    public byte ByTimeOut; //跟踪超时时间 60s-200s
    public uint DwSize;
}

/**********跨线检测***********/
/*跨线检测的方式*/
/*线基本信息*/
public class TmLineInfoT
{
    public byte ByMotionSensitive; //跨线检测灵敏度, 0 - 100,越高越灵敏 灵敏度越高越小的物体跨线越容易被检测到
    public byte[] Bytmp = new byte[3];
    public int Method; //侦测方法
    public TmPointT PStarPostion = new(); //线的开始位置
    public TmPointT PStopPostion = new(); //线的结束位置
}

/*跨线检测的配置信息*/
public class TmMotionLineCfgT
{
    public bool BEnable; //是否启动侦测
    public byte[] ByTmp = new byte[3];
    public uint DwSize;
    public byte LineNum;
    public TmLineInfoT[] PLineList = Arrays.InitializeWithDefaultInstances<TmLineInfoT>(DefineConstants.MaxCheckNum);
    public TmHandleExceptionT StrMotionHandleType = new(); //处理方式
    public TmSchedTimeT[][] StruAlarmTime = RectangularArrays.RectangularTmSchedTime_tArray(7, 4); //布防时间
    public TmTransFerT[] StruAlarmTransFer = Arrays.InitializeWithDefaultInstances<TmTransFerT>(16); //触发通道
}

/*每个区域信息*/
public class TmPolygonScopeT
{
    public int ByPointNum; //用到顶点个数
    public byte ByPolygonSensitive; //区域检测的灵敏度， 0-100值越大，越小的物体闯入区域就越能被检测到
    public byte ByPolygonTime; //时间阈值: 时间阀值：表示目标进入警戒区域持续停留多久后产生报警,0-10s 如果为0表示闯入就会报警
    public byte[] ByTmp = new byte[2];

    public TmPointT[]
        PPointList =
            Arrays.InitializeWithDefaultInstances<TmPointT>(DefineConstants.MaxPointNumInPolygon); //坐标的范围为704*576；
}

/*区域检测的配置参数*/
public class TmPolygonCfgT
{
    public byte ByEnableAbandon; //是否处理区域侦测
    public byte ByPolygonScopeNum; //多边形区域个数，必须小于或等于5
    public byte[] ByTmp = new byte[2];
    public uint DwSize;

    public TmPolygonScopeT[] PPolygonScope =
        Arrays.InitializeWithDefaultInstances<TmPolygonScopeT>(DefineConstants.MaxCheckNum);

    public TmHandleExceptionT StrMotionHandleType = new(); //处理方式
    public TmSchedTimeT[][] StruAlarmTime = RectangularArrays.RectangularTmSchedTime_tArray(7, 4); //布防时间
    public TmTransFerT[] StruAlarmTransFer = Arrays.InitializeWithDefaultInstances<TmTransFerT>(16); //触发通道
}

/*****************流量统计的方向*************************/
//重新定义方向，与客户端保持一致
public class TmFlowLineT
{
    public byte ByMotionSensitive; //流量检测灵敏度, 0 - 100,越高越灵敏 灵敏度越高越小的物体越容易被检测到
    public byte[] ByTmp = new byte[3];
    public int EnterCount; //进量
    public int ExitCount; //出量
    public int Method; //统计方向
    public TmPointT PStarPostion = new(); //线的开始位置
    public TmPointT PStopPostion = new(); //线的结束位置
}

/*流量统计配置参数*/
public class TmFlowCountCfgT
{
    public byte ByEnable; //是否流量统计
    public byte[] ByTmp = new byte[2];
    public uint DwSize;
    public byte LineNum;
    public TmFlowLineT[] PFlowList = Arrays.InitializeWithDefaultInstances<TmFlowLineT>(DefineConstants.MaxCheckNum);
    public TmHandleExceptionT StrMotionHandleType = new(); //处理方式
    public TmSchedTimeT[][] StruAlarmTime = RectangularArrays.RectangularTmSchedTime_tArray(7, 4); //布防时间
    public TmTransFerT[] StruAlarmTransFer = Arrays.InitializeWithDefaultInstances<TmTransFerT>(16); //触发通道
}

public class TmAbandonInfoT
{
    public byte ByAbandonSensitive; //遗留检测灵敏度, 0 - 100,越高越灵敏 灵敏度越高越小的物体越容易被检测到
    public byte ByCheckTime; //物体持续10-120s在区域内遗留报警
    public byte[] ByTmp = new byte[2];
    public TmAreaScopeT StruAbandonScope = new(); //遗留区域704*576
}

/*遗留侦测*/
public class TmAbandonCfgT
{
    public byte ByAbandonScopeNum; //遗留区域个数，必须小于或等于5
    public byte ByEnableAbandon; //是否处理遗留侦测
    public byte[] ByTmp = new byte[2];
    public uint DwSize;

    public TmAbandonInfoT[] StrAbandonList =
        Arrays.InitializeWithDefaultInstances<TmAbandonInfoT>(DefineConstants.MaxCheckNum);

    public TmHandleExceptionT StrMotionHandleType = new(); //处理方式
    public TmSchedTimeT[][] StruAlarmTime = RectangularArrays.RectangularTmSchedTime_tArray(7, 4); //布防时间
    public TmTransFerT[] StruAlarmTransFer = Arrays.InitializeWithDefaultInstances<TmTransFerT>(16);
}

/*移动物体坐标信息*/
public class TagtmPostionResultT //移动物体的坐标和大小
{
    public ushort WHeight;
    public ushort WWidth;
    public ushort Wx;
    public ushort Wy;
}

public class TagtmMotionRectListT
{
    public uint DwSize; //加上所有pPointList的长度，方便以后好兼容

    public TagtmPostionResultT[]
        PPointList =
            Arrays.InitializeWithDefaultInstances<TagtmPostionResultT>(
                1); //有几个物体就有几个这种结构体,暂时只定义一个，这里的个数由 wImotion_num决定

    public ushort WImageHeight;
    public ushort WImageWidth;
    public ushort WImotionNum;
    public ushort WTemp;
}

/*智能模块产生报警信息*/
public class TmIntelligentAlarmT
{
    public int AlarmPostion; //报警发生的位置
    public int AlarmState; //报警状态
    public int AlarmType; //报警类型 1-跨线报警 2-区域闯入报警 3-遗留物报警
    public uint DwSize;
}

/*枪球联动中球机的配置信息*/
public class TmCameraItemInfoT
{
    public byte ByAlarmArea; //关联报警的具体区域
    public byte ByASsociateAlarm; //关联报警 1-跨线报警 2-区域闯入报警 3-遗留物报警
    public byte ByEnable; //是否启用此球机条目
    public byte ByTmp;
    public int HorBase; //球机水平位置
    public int Port; //端口
    public string SzIp = new(new char[32]); //球机ip地址
    public string SzPass = new(new char[32]); //密码
    public string SzUser = new(new char[32]); //用户
    public int VerBase; //球机垂直位置
    public int ZoomBase; //球机倍数
}

/*枪球联动配置信息 TMCC_MINOR_CMD_CAMTRACK*/
public class TmCameraTrackCfgT
{
    public byte[] ByTmp = new byte[4];
    public uint DwSize;
    public int HorViewAngle; //水平可视角
    public TmCameraItemInfoT[] PCamItemList = Arrays.InitializeWithDefaultInstances<TmCameraItemInfoT>(5); //关联球机的信息
    public int VerViewAngle; //垂直可视角
}

/*选定的智能识别目标*/
public class TmAiObjectCfgT
{
    //与分析板通讯的串口号0-默认，1-第1个串口,2-第1个串口
    public byte ByComId;

    //通讯串口活动信号检测时间(秒)0-不检测
    public byte ByComLiveHeartTime;

    //识别结果是否通过串口送出，0-不通讯，1-串口通讯
    public byte ByCtrlMode;

    /*启动目标识别*/
    public byte ByEnable;

    //是否启用抓图
    public byte ByEnableCapture;

    //是否启用人脸比对
    public byte ByEnableIdeFace;

    /*识别主类型*/
    public byte[] ByObjectType = new byte[8];

    //是否在图像上显示识别结果
    public byte ByViewResult;

    public uint DwSize;

    /*识别子类型*/
    public uint U32ObjectFlags;
}

/*选定的目标区域(跟踪目标):基于704X576*/
public class TmAiSelObjectT
{
    /*选择目标的模式0--手动1--自动选择*/
    public byte BySelMode;
    public byte[] ByTemp = new byte[2];
    public byte ByUseOrgSize; //是否使用原始尺寸
    public int DwBottom;

    public uint DwImageHeight;

    /*原始坐标图像大小=0为默认,704x576*/
    public uint DwImageWidth;

    /*目标的坐标信息,手动选择时生效*/
    public int DwLeft;
    public int DwRight;
    public uint DwSize;
    public int DwTop;
}

/*跟踪属性参数配置*/
public class TmTraceParmT
{
    public byte ByAiModuleId; //AI 模板ID: 用来动态切换AI模板 1--对天空 2--对地80分类
    public byte ByAiObjectType; //是否有外置目标识别器0-无,1-有
    public byte ByAngleType; //角度传感器类型
    public byte ByComId; //与分析板通讯的串口号0-默认，1-第1个串口,2-第1个串口
    public byte ByCtrlMode; //目标分析者与本机通讯模式0-内部通讯，1-串口通讯，2-网络通讯
    public byte ByDisableAutoZoom; //是否禁止自动变倍
    public byte ByEnableAngleSensor; //是否启用角度传感器
    public byte ByEnableAutoTrace; //是否自动跟踪，需要启动自动目标识别
    public byte ByEnableLaserSensor; //是否启用激光测距功能
    public byte ByFlirCorrection; //热像是否启用二次校正，0-不启用，1-启用
    public byte ByGotoFocus; //热像变倍后是否调用预置位focus
    public byte ByHideIdentifyRect; //是否隐藏识别框
    public byte ByHideTrackRect; //是否隐藏跟踪框
    public char ByIndexAdd; //雷达引导索引增量
    public byte ByLaserType; //激光测距类型
    public byte ByObjNoRule; //目标编号规则:0--识别目标小编号; 1--识别目标唯一长编号; 2--跟踪目标编号
    public byte[] ByTemp = new byte[3]; //保留
    public byte ByTracerType; //跟踪器类型,0-默认(内部智能分析)，1-外部智能分析
    public byte ByViewResult; //是否在图像上显示跟踪结果
    public byte ByWriteTraceLog; //是否记录引导跟踪日志
    public uint DwSize;

    public ushort UAutoZoomInPixesThresh; //自动变倍变大的像素大小阈值 / 10: 1表示10， 2 表示 20个像素

    /*change by zzt: 将下面4个字段合并成两个字段*/
    public ushort UAutoZoomOutPixesThresh; //自动变倍变小的像素大小阈值 / 10: 1表示10， 2 表示 20个像素
    public ushort WLaserInterval; //激光测距间隔时间, 单位ms
}

/*转发服务器*/
public class TmTransmitCfgT
{
    public byte ByEnable; //使能
    public byte[] ByTemp = new byte[3]; //保留
    public uint DwPort; //转发端口
    public string SzIp = new(new char[32]); //转发服务器IP
}

/*雷达联动相关信息*/
public class TmRadarCfgT
{
    public byte ByDeviceHorMode; //光电水平角度模式: 0 -- 顺时针递增 1--反时针递增
    public byte ByDeviceVerMode; //光电俯仰角起始位置: 0 --最上 1--水平中间 2--最下
    public byte ByEnableLongLat; //光电是否解算经纬度
    public byte ByRadarHorMode; //雷达水平角度模式: 0 -- 顺时针递增 1--反时针递增
    public byte ByRadarVerMode; //雷达俯仰角起始位置: 0 --最上 1--水平中间 2--最下
    public byte[] ByTemp = new byte[1];
    public byte[] ByTemp1 = new byte[2];
    public byte[] ByTemp3 = new byte[8];
    public uint DwDeviceHeight; //光电系统安装高度
    public uint DwDeviceMaxHorAngle; //光电设备最大水平角度 * 100
    public uint DwDeviceMaxVerAngle; //光电最大俯仰角度 * 100
    public uint DwDeviceNorthAngle; //光电偏北角*100
    public uint DwDeviceNumer; //光电编号
    public uint DwRadarLatitude; //雷达维度纬度*1000000, 北纬为正数，南纬为负数(-90*1000000~90*1000000)
    public uint DwRadarLongitude; //雷达经度*1000000, 东经正数，西经为负数(-180*1000000~180*1000000)
    public uint DwRadarMaxHorAngle; //雷达最大水平角度 * 100
    public uint DwRadarMaxVerAngle; //雷达最大俯仰角度 * 100
    public uint DwRadarNorthAngle; //雷达偏北角*100
    public uint DwServerPort; //雷达服务器端口
    public uint DwServerPort2; //雷达安装高度 dwRadarHeight-->dwServerPort2  第二指控服务器端口
    public uint DwSize;

    public TmTransmitCfgT[] StTransmit =
        Arrays.InitializeWithDefaultInstances<TmTransmitCfgT>(DefineConstants.MaxTransmitNum); //转发服务器

    public string SzPassword = new(new char[32]); //登录密码
    public string SzServerIp = new(new char[32]); //雷达服务器IP
    public string SzServerIp2 = new(new char[32]); //登录用户: 修改成第二指控服务器的IP
}

/*201感知系统相关配置*/
public class TmPerceptionCfgT
{
    public byte ByConfidence; //置信度
    public byte[] ByTemp = new byte[3];
    public uint DevicePort; //接收雷达数据
    public uint DwLeadTime; //接收雷达引导时间，ms
    public uint DwSize;
    public uint DwUptoMasterTime; //上传主控时间，ms
    public int HorOffset; //方位偏移
    public int HorOffset2; //方位偏移2
    public uint MasterPort; //发送数据给主控平台, 广播
    public uint RadarPort; //发送数据给雷达
    public string SzRadarIp = new(new char[32]); //雷达服务器IP
    public int VerOffset; //俯仰偏移
    public int VerOffset2; //俯仰偏移2
}

public class TmRadarOffsetT
{
    public uint DwDistance;
    public int HorOffset;
    public int VerOffset;
}

public class TmRadarOffsetCfgT
{
    public uint DwCount;
    public uint DwSize;
    public TmRadarOffsetT[] POffsetList = Arrays.InitializeWithDefaultInstances<TmRadarOffsetT>(1);
}

/*亿威尔相关配置*/
public class TmEwareCfgT
{
    public uint DwDevicePort; //设备端口
    public uint DwServerPort; //服务器端口
    public uint DwSize;
    public string SzServerIp = new(new char[32]); //服务器IP
}

/*cop15相关配置*/
public class TmCop15CfgT
{
    public byte ByCounter; //是否带反制设备
    public byte[] ByTemp = new byte[1]; //保留
    public uint DwServerPort; //服务器端口
    public uint DwSize;
    public string SzDeviceId = new(new char[16]); //设备编号
    public string SzServerIp = new(new char[32]); //服务器IP
    public uint WHeartbeatTime; //发送心跳间隔时间(单位:秒)
}

/*FK相关配置*/
public class TmFkCfgT
{
    public uint DwDevicePort; //设备端口
    public uint DwServerPort; //服务器端口
    public uint DwSize;
    public string SzServerIp = new(new char[32]); //服务器IP
}

/*其他厂家指控平台参数配置*/
public class TmOtherPlatformCfgT
{
    public byte ByFactoryNo; //厂商号
    public byte[] ByTemp = new byte[3];
    public uint DwSize;

    public string SzName = new(new char[64]);
    //C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
    //	union
    //	{
    //		tmPerceptionCfg_t stPerception;
    //		tmEwareCfg_t stEware;
    //		tmCop15Cfg_t stCop15;
    //		tmFkCfg_t stFk;
    //		byte pBuffer[1024];
    //	};
}

/*2020-12-10 zzt: 跟踪业务参数配置*/
public class TmTracerImplementCfgT
{
    public byte ByAutoFilter; //自动跟踪模式下，是否过滤
    public byte[] ByDisableTrack = new byte[128]; //是否对指定识别出来的目标禁止跟踪: 数组索引代表识别出来的目标索引，值0-不跟踪 1-跟踪
    public byte ByGuideMode; //引导工作模式:        0--布防区域触发      ； 1--雷达引导 2 --自主扫描
    public byte ByShowThresh; //显示的相似度阈值
    public byte[] ByTemp = new byte[2]; //保留
    public byte ByTimeOut; //持续跟踪时间, 单位s:0xFF-持续跟踪, 等
    public byte ByTrackThresh; //触发跟踪的目标相似度阈值，如60% 才触发跟踪
    public byte ByWatchMode; //看守模式，0-默认，1-看守位，2-自主扫描
    public uint DwSize;
}

/*光电自主扫描参数*/
public class TmAutoScanCfgT
{
    public byte ByCmd; //0-停止扫描，1-开始扫描
    public byte ByScanSpeed; //扫描速度 度/s
    public byte[] ByTemp = new byte[2];
    public uint DwRange; //防御范围:m
    public uint DwSize;
    public short UHorStart; //水平开始角度
    public short UHorStop; //水平结束角度
    public short UVerStart; //俯仰开始角度
    public short UVerStop; //俯仰结束角度
}

/*2020-12-10 zzt:  yolov 模型参数配置*/
public class TmYoloInfoCfgT
{
    public byte ByClassId; //分类类型1-对空 3分类 2--对地80分类 3--20 分类    ; 4--yolov4 20分类
    public byte[] ByTemp = new byte[3];
    public byte[] ByThresh = new byte[80]; //每个分类的判定阈值，索引为目标ID
    public uint DwSize;
}

/*PID参数配置*/
public class TmPidParamT
{
    public uint DwKd; //微分: x 1000000
    public uint DwKi; //积分: x 1000000
    public uint DwKp; //比列: x 1000000
    public uint DwMaxHorRation; //最大横向速度参数 * 100
    public uint DwMaxVerRation; //最大垂直速度参数* 100
    public uint DwSize;
    public uint DwSpeedHorRation; //横向加速参数* 100
    public uint DwSpeedVerRation; //垂直加速度参数* 100
    public uint DwVkd; //微分: x 1000000
    public uint DwVki; //积分: x 1000000
    public uint DwVkp; //比列: x 1000000
}

/*mqtt相关信息*/
public class TmMqttInfoT
{
    public byte[] ByTemp = new byte[4];
    public uint DwKeepLive; //心跳时间
    public uint DwServerPort; //mqtt服务器端口
    public uint DwSize;
    public string SzDeviceId = new(new char[32]); //设备ID
    public string SzPassword = new(new char[32]); //登录密码
    public string SzServerIp = new(new char[32]); //mqtt服务器IP
    public string SzUserName = new(new char[32]); //登录用户
}

/*鼠标键盘操作结构*/
public class TmEventCmdT
{
    public uint DwSize;
    public AnonymousClass15 Pt = new();
    public int S32Value;
    public ushort U16Code;
    public ushort U16Height;
    public ushort U16Type;

    public ushort U16Width;

    //C++ TO C# CONVERTER NOTE: Classes must be named in C#, so the following class has been named by the converter:
    public class AnonymousClass15
    {
        public ushort X;
        public ushort Y;
    }
}
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define EVENT_KEY (0x01)
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define EVENT_REL (0x02)
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define EVENT_ABS (0x03)
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define EVENT_MS_MOVE (0x01)
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define EVENT_MS_LBUTTONDOWN (0x02)
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define EVENT_MS_LBUTTONUP (0x03)
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define EVENT_MS_LBUTTONDBLCLK (0x04)
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define EVENT_MS_RBUTTONDOWN (0x05)
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define EVENT_MS_RBUTTONUP (0x06)
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define EVENT_MS_RBUTTONDBLCLK (0x07)
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define EVENT_MS_MBUTTONDOWN (0x08)
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define EVENT_MS_MBUTTONUP (0x09)
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define EVENT_MS_MBUTTONDBLCLK (0x0A)
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define EVENT_MS_WHEEL (0x0B)
/*命令代码定义*/
/*用户操作*/
/*服务器参数配置*/
/*云台解码器参数*/
/*解码器命令*/
/*服务器的消息*/
/*云台控制命令*/
/*服务器的基本控制*/
/*网络控制扩展命令*/
/*压缩参数扩展命令*/
/*抓图的命令*/
/*音频参数设置命令*/
/*NVR通道信息信息设置相关命令*/
/*NVR的虚拟报警设备*/
/*设备日志命令*/
/*解码器配置命令*/
/*摄像机经纬度配置命令对应tmTheodoliteCfg_t结构*/
/*IRCUT的配置命令*/
/*摄像机通过串口上报状态信息*/
/*CIG消息*/
/* 视频显示参数配置 add by tzh*/
/*
add by zzt: 2018
绘图配置
*/
/*
add by zzt: 2019-01
智能跟踪相关配置
*/
/*
 * 人脸识别参数配置
 */
/*热像探测器配置*/
/* 用户权限定义 */
/*播放文件的状态*/
/*解码视频输出格式*/
/*操作错误代码*/
/*错误代码*/