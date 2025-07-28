namespace Magneto.Device.Nvt.SDK;

internal static class DefineConstants
{
    public const int OptoPort = 9966;
    public const int OptoVersion = 9002;
    public const uint StartCode = 0x8A808988;
    public const uint StopCode = 0x8B8A8089;
    public const int UserContextSize = 128;

    /// <summary>
    ///     设备请求升级
    /// </summary>
    public const int AutoUpgradeCodeRequest = 0x01;

    /// <summary>
    ///     设备开始升级
    /// </summary>
    public const int AutoUpgradeCodeStart = 0x02;

    /// <summary>
    ///     设备成功升级
    /// </summary>
    public const int AutoUpgradeCodeSuccess = 0x03;

    /// <summary>
    ///     设备升级中
    /// </summary>
    public const int AutoUpgradeCodeState = 0x04;

    /// <summary>
    ///     设备和服务器网络不通
    /// </summary>
    public const int AutoUpgradeCodeNetWorError = 0x05;

    /// <summary>
    ///     设备请求连接时发生错误
    /// </summary>
    public const int AutoUpgradeCodeCantConnect = 0x06;

    /// <summary>
    ///     设备升级Firmware时发生错误
    /// </summary>
    public const int AutoUpgradeCodeWriteError = 0x07;

    /// <summary>
    ///     初始化成设备控制SDK句柄
    /// </summary>
    public const int TmccInitTypeControl = 0x00;

    /// <summary>
    ///     初始化成枚举SDK句柄
    /// </summary>
    public const int TmccInitTypeENum = 0x01;

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
    public const int TmccInitTypeRtpStream = 0x0C;

    /// <summary>
    ///     初始化为SDK句柄掩码
    /// </summary>
    public const int TmccInitTypeMask = 0xFF;

    public const int PutinStreamDataReset = 0x01;
    public const int PutinStreamDataVideo = 0x02;
    public const int PutinStreamDataNeedKeyFrame = 0x04;
    public const int IpLen = 24;
    public const int NameLen = 32;
    public const int SerialNoLen = 48;
    public const int MacAddrLen = 6;
    public const int MaxEthernet = 2;
    public const int PathNameLen = 128;
    public const int PassWdLen = 32;
    public const int MaxChanNum = 64;
    public const int MaxAlarmOut = 64;
    public const int MaxTimeSegment = 4;
    public const int MaxPreset = 256;
    public const int MaxPresetNameLen = 32;
    public const int MaxKeepWatch = 4;
    public const int MaxPresetVideoData = 32;
    public const int MaxDays = 7;
    public const int MaxAllTimeSegment = 28;
    public const int MaxWeekName = 20;
    public const int PhoneNumberLen = 32;
    public const int MaxDiskNum = 16;
    public const int MaxWindow = 16;
    public const int MaxVga = 1;
    public const int MaxUserNum = 16;
    public const int MaxExceptionNum = 16;
    public const int MaxLink = 6;
    public const int MaxAlarmIn = 64;
    public const int MaxIrAlarmIn = 8;
    public const int MaxIrLight = 8;
    public const int MaxVideoOut = 4;

    /// <summary>
    ///     DVS本地登陆名最大长度
    /// </summary>
    public const int MaxNameLen = 16;

    /// <summary>
    ///     权限
    /// </summary>
    public const int MaxRight = 64;

    /// <summary>
    ///     卡号长度
    /// </summary>
    public const int CardNumLen = 20;

    /// <summary>
    ///     解码器名称最大长度
    /// </summary>
    public const int DecoderNameLen = 20;

    /// <summary>
    ///     移动区域个数最大值
    /// </summary>
    public const int MaxMotionAreaNum = 5;

    /// <summary>
    ///     串口传输数据长度最大值
    /// </summary>
    public const int MaxSerialTransLen = 128;

    /// <summary>
    ///     循环路数最大值
    /// </summary>
    public const int MaxCircLeNum = 32;

    /// <summary>
    ///     NTP服务器的最大数量
    /// </summary>
    public const int MaxNtpServers = 3;

    /// <summary>
    ///     RS232端口数量
    /// </summary>
    public const int MaxRs232Num = 8;

    /// <summary>
    ///     RS485端口数量
    /// </summary>
    public const int MaxRs485Num = 8;

    /// <summary>
    ///     每个RS232端口下属设备的最大数量
    /// </summary>
    public const int MaxRs232SubNum = 64;

    /// <summary>
    ///     巡航最多支持的线路数
    /// </summary>
    public const int CruiseMaxLineNums = 8;

    /// <summary>
    ///     最大的报警设备数
    /// </summary>
    public const int MaxAlarmDevice = 6;

    /// <summary>
    ///     最大的循切数
    /// </summary>
    public const int MaxCircleConnectNum = 32;

    /// <summary>
    ///     最大解码通道数
    /// </summary>
    public const int MaxWindowNum = 16;

    /// <summary>
    ///     最大支持配置模式数
    /// </summary>
    public const int MaxSchedVideoInMode = 5;

    /// <summary>
    ///     最大支持帧率数
    /// </summary>
    public const int MaxFrameRateNum = 25;

    /// <summary>
    ///     颜色模式数
    /// </summary>
    public const int ColorModeNum = 16;

    /// <summary>
    ///     热像区域个数最大值
    /// </summary>
    public const int MaxFlirAreaNum = 16;

    /// <summary>
    ///     热像点数最大值
    /// </summary>
    public const int MaxFlirPointNum = 16;

    /// <summary>
    ///     热像线数最大值
    /// </summary>
    public const int MaxFlirLineNum = 16;

    /// <summary>
    ///     测温组的最大数量
    /// </summary>
    public const int MaxFlirGroupNum = 32;

    /// <summary>
    ///     测温组里的源的最大数量
    /// </summary>
    public const int MaxFlirGrpSrcNum = 16;

    /// <summary>
    ///     测温组里的人脸数据最大数量
    /// </summary>
    public const int MaxFlirFaceNum = 16;

    /// <summary>
    ///     RGB区域数量
    /// </summary>
    public const int MaxRgbNum = 16;

    /// <summary>
    ///     转发IP地址数量
    /// </summary>
    public const int MaxTransmitNum = 8;

    /// <summary>
    ///     自动选择网络接口
    /// </summary>
    public const int NetIfAuto = 0;

    /// <summary>
    ///     10M以太网
    /// </summary>
    public const int NetIf10MHalf = 1;

    /// <summary>
    ///     10M以太网全双工模式
    /// </summary>
    public const int NetIf10MFull = 2;

    /// <summary>
    ///     100M以太网
    /// </summary>
    public const int NetIf100MHalf = 3;

    /// <summary>
    ///     100M以太网全双工模式
    /// </summary>
    public const int NetIf100MFull = 4;

    /// <summary>
    ///     1000M以太网（半双工）
    /// </summary>
    public const int NetIf1000MHalf = 5;

    /// <summary>
    ///     1000M以太网（全双工）
    /// </summary>
    public const int NetIf1000MFull = 6;

    /// <summary>
    ///     视频服务器
    /// </summary>
    public const int Dvs = 3;

    /// <summary>
    ///     标清解码器
    /// </summary>
    public const int Dec = 4;

    /// <summary>
    ///     高清解码器
    /// </summary>
    public const int HdDec = 5;

    /// <summary>
    ///     高清NVR
    /// </summary>
    public const int HdNvr = 6;

    /// <summary>
    ///     存储服务器
    /// </summary>
    public const int HdStorage = 7;

    /// <summary>
    ///     转发器
    /// </summary>
    public const int HdTurn = 8;

    /// <summary>
    ///     电视墙解码器
    /// </summary>
    public const int HdTvWall = 9;

    /// <summary>
    ///     15/17编码模块
    /// </summary>
    public const int DvsIpCamera = 12;

    /// <summary>
    ///     1080p10摄像机
    /// </summary>
    public const int HdIpCamera = 13;

    /// <summary>
    ///     720p摄像机
    /// </summary>
    public const int HdIpCamera1 = 14;

    /// <summary>
    ///     D1摄像机
    /// </summary>
    public const int HdIpCamera2 = 15;

    /// <summary>
    ///     1080p30摄像机
    /// </summary>
    public const int HdIpCamera3 = 16;

    /// <summary>
    ///     保留
    /// </summary>
    public const int HdIpCamera4 = 17;

    /// <summary>
    ///     D1编码模块(BNC输入)
    /// </summary>
    public const int HdNvs = 21;

    /// <summary>
    ///     高清编码模块(YPbPr输入)
    /// </summary>
    public const int HdNvs1 = 22;

    /// <summary>
    ///     SC110编码模块
    /// </summary>
    public const int HdSc110Cam = 23;

    /// <summary>
    ///     2M数字一体机
    /// </summary>
    public const int HdDigitalCamera2M18 = 25;

    /// <summary>
    ///     模拟CAM
    /// </summary>
    public const int AnalogCam = 31;

    /// <summary>
    ///     1M网络摄像机
    /// </summary>
    public const int HdIpCamera1M = 32;

    /// <summary>
    ///     2M网络摄像机
    /// </summary>
    public const int HdIpCamera2M = 33;

    /// <summary>
    ///     3M网络摄像机
    /// </summary>
    public const int HdIpCamera3M = 34;

    /// <summary>
    ///     5M网络摄像机
    /// </summary>
    public const int HdIpCamera5M = 35;

    /// <summary>
    ///     2M18倍网络一体机
    /// </summary>
    public const int HdIpCamera2M18 = 36;

    /// <summary>
    ///     2M16倍网络一体机
    /// </summary>
    public const int HdIpCamera2M16 = 37;

    /// <summary>
    ///     2M22倍网络一体机
    /// </summary>
    public const int HdIpCamera2M22 = 38;

    /// <summary>
    ///     D1摄像机
    /// </summary>
    public const int HdIpCameraD1 = 39;

    /// <summary>
    ///     2M38x38模块
    /// </summary>
    public const int HdIp38X382M = 40;

    /// <summary>
    ///     1M38x38模块
    /// </summary>
    public const int HdIp38X381M = 41;

    /// <summary>
    ///     D138x38模块
    /// </summary>
    public const int HdIp38X38D1 = 42;

    /// <summary>
    ///     2M1M网络摄像机
    /// </summary>
    public const int HdIpCamera2M1M = 43;

    /// <summary>
    ///     2M118倍网络一体机
    /// </summary>
    public const int HdIpCamera2M1M16 = 44;

    /// <summary>
    ///     2M1M16倍网络一体机
    /// </summary>
    public const int HdIpCamera2M1M18 = 45;

    /// <summary>
    ///     2M1M22倍网络一体机
    /// </summary>
    public const int HdIpCamera2M1M22 = 46;

    /// <summary>
    ///     安全卫士系统
    /// </summary>
    public const int HdIpCameraSafe = 47;

    /// <summary>
    ///     2M智能枪机
    /// </summary>
    public const int HdIpCamera2MIntelligent = 48;

    /// <summary>
    ///     2M20倍网络一体机
    /// </summary>
    public const int HdIpCamera2M20 = 49;

    /// <summary>
    ///     2M30倍网络一体机
    /// </summary>
    public const int HdIpCamera2M36 = 50;

    /// <summary>
    ///     1.3M18倍网络一体机
    /// </summary>
    public const int HdIpCamera1D3M18 = 51;

    /// <summary>
    ///     1.3M16倍网络一体机
    /// </summary>
    public const int HdIpCamera1D3M16 = 52;

    /// <summary>
    ///     1M18倍网络一体机
    /// </summary>
    public const int HdIpCamera1M18 = 53;

    /// <summary>
    ///     1M16倍网络一体机
    /// </summary>
    public const int HdIpCamera1M16 = 54;

    /// <summary>
    ///     1M22倍网络一体机
    /// </summary>
    public const int HdIpCamera1M22 = 55;

    /// <summary>
    ///     1.3M22倍网络一体机
    /// </summary>
    public const int HdIpCamera1D3M22 = 56;

    /// <summary>
    ///     1M3倍网络一体机
    /// </summary>
    public const int HdIpCamera1M3 = 57;

    /// <summary>
    ///     1.3M3倍网络一体机
    /// </summary>
    public const int HdIpCamera1D3M3 = 58;

    /// <summary>
    ///     2M3倍网络一体机
    /// </summary>
    public const int HdIpCamera2M3 = 59;

    /// <summary>
    ///     1.3M网络摄像机
    /// </summary>
    public const int HdIpCamera1D3M = 60;

    /// <summary>
    ///     1.3M38x38模块
    /// </summary>
    public const int HdIp38X381D3M = 61;

    /// <summary>
    ///     1M二代半球
    /// </summary>
    public const int HdIp2Ball1M = 62;

    /// <summary>
    ///     1.3M二代半球
    /// </summary>
    public const int HdIp2Ball1D3M = 63;

    /// <summary>
    ///     2M二代半球
    /// </summary>
    public const int HdIp2Ball2M = 64;

    /// <summary>
    ///     1M一体机
    /// </summary>
    public const int HdIpCamera1M30 = 65;

    /// <summary>
    ///     1.3M一体机
    /// </summary>
    public const int HdIpCamera1D3M30 = 66;

    /// <summary>
    ///     2M一体机
    /// </summary>
    public const int HdIpCamera2M30 = 67;

    /// <summary>
    ///     2M4码流枪机
    /// </summary>
    public const int HdIpCamera2M4Ch = 68;

    /// <summary>
    ///     2M4码流M38模组
    /// </summary>
    public const int HdIp38X382M4Ch = 69;

    /// <summary>
    ///     4M网络摄像机
    /// </summary>
    public const int HdIpCamera4M = 70;

    /// <summary>
    ///     2M60帧网络摄像机
    /// </summary>
    public const int HdIpCamera2M60Frame = 71;

    /// <summary>
    ///     3M60帧网络摄像机
    /// </summary>
    public const int HdIpCamera3M60Frame = 72;

    /// <summary>
    ///     6M网络摄像机
    /// </summary>
    public const int HdIpCamera6M = 73;

    /// <summary>
    ///     8M网络摄像机
    /// </summary>
    public const int HdIpCamera8M = 74;

    /// <summary>
    ///     12M网络摄像机
    /// </summary>
    public const int HdIpCamera12M = 75;

    /// <summary>
    ///     3M双板M38模组
    /// </summary>
    public const int HdIp38X383M = 76;

    /// <summary>
    ///     全景摄像机
    /// </summary>
    public const int HdIpCameraPanorama = 77;

    /// <summary>
    ///     1M双板M42模组
    /// </summary>
    public const int HdIp38X384M = 78;

    /// <summary>
    ///     1M双板M38x42模组
    /// </summary>
    public const int HdIp38X421M = 79;

    /// <summary>
    ///     1.3M双板M38x42模组
    /// </summary>
    public const int HdIp38X421D3M = 80;

    /// <summary>
    ///     2M双板M38x42模组
    /// </summary>
    public const int HdIp38X422M = 81;

    /// <summary>
    ///     3M双板M38x42模组
    /// </summary>
    public const int HdIp38X423M = 82;

    /// <summary>
    ///     4M双板M38x42模组
    /// </summary>
    public const int HdIp38X424M = 83;

    /// <summary>
    ///     1M双板M42模组
    /// </summary>
    public const int HdIp42X421M = 84;

    /// <summary>
    ///     1.3M双板M42模组
    /// </summary>
    public const int HdIp42X421D3M = 85;

    /// <summary>
    ///     2M双板M42模组
    /// </summary>
    public const int HdIp42X422M = 86;

    /// <summary>
    ///     3M双板M42模组
    /// </summary>
    public const int HdIp42X423M = 87;

    /// <summary>
    ///     4M双板M42模组
    /// </summary>
    public const int HdIp42X424M = 88;

    /// <summary>
    ///     5M双板M42模组
    /// </summary>
    public const int HdIp42X425M = 89;

    /// <summary>
    ///     1M倍网络一体机
    /// </summary>
    public const int HdIpCameraYtj1M = 90;

    /// <summary>
    ///     1.3M倍网络一体机
    /// </summary>
    public const int HdIpCameraYtj1D3M = 91;

    /// <summary>
    ///     2M倍网络一体机
    /// </summary>
    public const int HdIpCameraYtj2M = 92;

    /// <summary>
    ///     3M倍网络一体机
    /// </summary>
    public const int HdIpCameraYtj3M = 93;

    /// <summary>
    ///     4M倍网络一体机
    /// </summary>
    public const int HdIpCameraYtj4M = 94;

    /// <summary>
    ///     5M倍网络一体机
    /// </summary>
    public const int HdIpCameraYtj5M = 95;

    /// <summary>
    ///     3M二代半球
    /// </summary>
    public const int HdIp2Ball3M = 100;

    /// <summary>
    ///     5M二代半球
    /// </summary>
    public const int HdIp2Ball5M = 101;

    /// <summary>
    ///     6M二代半球
    /// </summary>
    public const int HdIp2Ball6M = 102;

    /// <summary>
    ///     ATM1.3M设备
    /// </summary>
    public const int HdIpAtm1D3M = 105;

    /// <summary>
    ///     ATM2M设备
    /// </summary>
    public const int HdIpAtm2M = 106;

    /// <summary>
    ///     ATM3M设备
    /// </summary>
    public const int HdIpAtm3M = 107;

    /// <summary>
    ///     ATM4M设备
    /// </summary>
    public const int HdIpAtm4M = 108;

    /// <summary>
    ///     电梯设备
    /// </summary>
    public const int HdIpElevator2M = 108;

    /// <summary>
    ///     无响应
    /// </summary>
    public const int NoAction = 0x00;

    /// <summary>
    ///     监视器上警告
    /// </summary>
    public const int WarNonMonitor = 0x01;

    /// <summary>
    ///     声音警告
    /// </summary>
    public const int WarNonAudioOut = 0x02;

    /// <summary>
    ///     上传中心
    /// </summary>
    public const int UpToCenter = 0x04;

    /// <summary>
    ///     触发报警输出
    /// </summary>
    public const int TriggerAlarmOut = 0x08;

    /// <summary>
    ///     上传控制客户端
    /// </summary>
    public const int UpToCtrlClient = 0x10;

    /// <summary>
    ///     上传音视频客户端
    /// </summary>
    public const int UpToAvClient = 0x20;

    /// <summary>
    ///     启动跟踪
    /// </summary>
    public const int EnableTrace = 0x40;

    public const int MaxPointNum = 16;
    public const int FlirCheckModeAve = 0x00;
    public const int FlirCheckModeMin = 0x01;
    public const int FlirCheckModeMax = 0x02;
    public const int FlirCheckModeNo = 0xFF;
    public const int FlirAlarmModeMin = 0x01;
    public const int FlirAlarmModeMax = 0x02;
    public const int FlirAlarmModeDif = 0x04;
    public const int FlirAlarmTypeOut = 0x00;
    public const int FlirAlarmTypeTime = 0x01;
    public const int FlirAlarmTypeSpat = 0x02;
    public const int TmccAudioEncTypeMp2 = 0x00;
    public const int TmccAudioEncTypeAac = 0x01;
    public const int TmccAudioEncTypeMulAw = 0x02;
    public const int TmccAudioEncTypeAlAw = 0x03;
    public const int TmccAudioEncTypeG721 = 0x04;
    public const int TmccAudioEncTypeG722 = 0x05;
    public const int TmccAudioEncTypeG72324 = 0x06;
    public const int TmccAudioEncTypeG72340 = 0x07;
    public const int TmccAudioEncTypeG726 = 0x08;
    public const int TmccAudioEncTypeG729 = 0x09;
    public const int TmccAudioEncTypeS16Le = 0x0A;
    public const int TmccAudioEncTypeMp3 = 0x0B;
    public const int CaptureImageModeSave = 0x00;
    public const int CaptureImageModeFtp = 0x01;
    public const int CaptureImageModeServer = 0x02;
    public const int CaptureImageModeSaveAndFtp = 0x03;
    public const int CaptureImageModeSaveAndServer = 0x04;
    public const int PtzRealTrans = 0;

    /// <summary>
    ///     设置控制模式
    /// </summary>
    public const int PtzSetCmdMode = 1;

    /// <summary>
    ///     接通灯光电源 1
    /// </summary>
    public const int PtzLightPwRon = 2;

    /// <summary>
    ///     接通雨刷开关 2
    /// </summary>
    public const int PtzWiperPwRon = 3;

    /// <summary>
    ///     接通风扇开关 3
    /// </summary>
    public const int PtzFanPwRon = 4;

    /// <summary>
    ///     接通加热器开关 4
    /// </summary>
    public const int PtzHeaterPwRon = 5;

    /// <summary>
    ///     接通辅助设备开关 5 5
    /// </summary>
    public const int PtzAuxPwRon = 6;

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

    public const int PtzFocusNear = 13;

    /// <summary>
    ///     焦点以速度SS后调
    /// </summary>
    public const int PtzFocusFar = 14;

    /// <summary>
    ///     光圈以速度SS扩大
    /// </summary>
    public const int PtzIrisEnlarge = 15;

    /// <summary>
    ///     光圈以速度SS缩小
    /// </summary>
    public const int PtzIrisShrink = 16;

    /// <summary>
    ///     焦距以速度SS单步变大(倍率变大)
    /// </summary>
    public const int PtzZoomStepIn = 17;

    /// <summary>
    ///     焦距以速度SS单步变小(倍率变小)
    /// </summary>
    public const int PtzZoomStepOut = 18;

    /// <summary>
    ///     云台以SS的速度上仰
    /// </summary>
    public const int PtzUp = 21;

    /// <summary>
    ///     云台以SS的速度下俯
    /// </summary>
    public const int PtzDown = 22;

    /// <summary>
    ///     云台以SS的速度左转
    /// </summary>
    public const int PtzLeft = 23;

    /// <summary>
    ///     云台以SS的速度右转
    /// </summary>
    public const int PtzRight = 24;

    /// <summary>
    ///     云台以SS的速度右上仰
    /// </summary>
    public const int PtzRightUp = 25;

    /// <summary>
    ///     云台以SS的速度右下仰
    /// </summary>
    public const int PtzRightDown = 26;

    /// <summary>
    ///     云台以SS的速度左上仰
    /// </summary>
    public const int PtzLeftUp = 27;

    /// <summary>
    ///     云台以SS的速度左下仰
    /// </summary>
    public const int PtzLeftDown = 28;

    /// <summary>
    ///     云台以SS的速度左右自动扫描
    /// </summary>
    public const int PtzAuto = 29;

    /// <summary>
    ///     保存云台信息
    /// </summary>
    public const int PtzSaveLastInfo = 30;

    /// <summary>
    ///     485接收数据输入
    /// </summary>
    public const int Ptz485Input = 31;

    /// <summary>
    ///     485发送数据输出
    /// </summary>
    public const int Ptz485Output = 32;

    /// <summary>
    ///     保存守望点
    /// </summary>
    public const int PtzSetKeepWatch = 33;

    /// <summary>
    ///     调用守望点
    /// </summary>
    public const int PtzGotoKeepWatch = 34;

    /// <summary>
    ///     关闭看守卫
    /// </summary>
    public const int PtzLockKeepWatch = 35;

    /// <summary>
    ///     关闭看守卫
    /// </summary>
    public const int PtzCloseKeepWatch = 35;

    /// <summary>
    ///     打开看守卫
    /// </summary>
    public const int PtzUnlockKeepWatch = 36;

    /// <summary>
    ///     打开看守卫
    /// </summary>
    public const int PtzOpenKeepWatch = 36;

    /// <summary>
    ///     转到指定倍数,默认基数10开始为一倍，小于10为特殊位置
    /// </summary>
    public const int PtzGotoZoomBs = 37;

    /// <summary>
    ///     转到指定倍数,默认基数10开始为一倍，小于10为特殊位置
    /// </summary>
    public const int PtzDZoomGoToBs = 39;

    /// <summary>
    ///     数字变倍以速度SS变大(倍率变大)
    /// </summary>
    public const int PtzDZoomIn = 40;

    /// <summary>
    ///     数字变倍以速度SS变小(倍率变小)
    /// </summary>
    public const int PtzDZoomOut = 41;

    /// <summary>
    ///     云台以SS的速度上仰
    /// </summary>
    public const int PtzDZoomUp = 42;

    /// <summary>
    ///     云台以SS的速度下俯
    /// </summary>
    public const int PtzDZoomDown = 43;

    /// <summary>
    ///     云台以SS的速度左转
    /// </summary>
    public const int PtzDZoomLeft = 44;

    /// <summary>
    ///     云台以SS的速度右转
    /// </summary>
    public const int PtzDZoomRight = 45;

    /// <summary>
    ///     云台以SS的速度右上仰
    /// </summary>
    public const int PtzDZoomRightUp = 46;

    /// <summary>
    ///     云台以SS的速度右下仰
    /// </summary>
    public const int PtzDZoomLeftDown = 49;

    /// <summary>
    ///     激光控制
    /// </summary>
    public const int PtzLaserConfig = 50;

    /// <summary>
    ///     设置预置点
    /// </summary>
    public const int PtzSetPreset = 100;

    /// <summary>
    ///     清除预置点
    /// </summary>
    public const int PtzClePreset = 101;

    /// <summary>
    ///     转到预置点
    /// </summary>
    public const int PtzGotoPreset = 102;

    /// <summary>
    ///     开始录制轨迹
    /// </summary>
    public const int PtzStartRecTrack = 110;

    /// <summary>
    ///     停止录制轨迹
    /// </summary>
    public const int PtzStopRecTrack = 111;

    /// <summary>
    ///     运行轨迹
    /// </summary>
    public const int PtzStarTRunTrack = 112;

    /// <summary>
    ///     停止轨迹
    /// </summary>
    public const int PtzStopRunTrack = 113;

    /// <summary>
    ///     启用巡航布防
    /// </summary>
    public const int PtzStartArmCruise = 118;

    /// <summary>
    ///     关闭巡航布防
    /// </summary>
    public const int PtzStopArmCruise = 119;

    /// <summary>
    ///     开始录制巡航
    /// </summary>
    public const int PtzStartRecCruise = 120;

    /// <summary>
    ///     停止录制巡航
    /// </summary>
    public const int PtzStopRecCruise = 121;

    /// <summary>
    ///     运行巡航
    /// </summary>
    public const int PtzStarTRunCruise = 122;

    /// <summary>
    ///     停止巡航
    /// </summary>
    public const int PtzStopRunCruise = 123;

    /// <summary>
    ///     云台综合控制，同时控制旋转和变倍等
    /// </summary>
    public const int PtzIntegRateControl = 124;

    /// <summary>
    ///     设置当前位置为坐标原始点
    /// </summary>
    public const int PtzSetOriginalityPt = 125;

    /// <summary>
    ///     开始录制线扫
    /// </summary>
    public const int PtzStartRecLine = 130;

    /// <summary>
    ///     停止录制线扫
    /// </summary>
    public const int PtzStopRecLine = 131;

    /// <summary>
    ///     设置线扫点
    /// </summary>
    public const int PtzSetLinePoint = 132;

    /// <summary>
    ///     运行巡航
    /// </summary>
    public const int PtzStartRunLine = 133;

    /// <summary>
    ///     停止巡航
    /// </summary>
    public const int PtzStopRunLine = 134;

    /// <summary>
    ///     开始z扫描
    /// </summary>
    public const int PtzStarTRunScan = 135;

    /// <summary>
    ///     停止z扫描
    /// </summary>
    public const int PtzStopRunScan = 136;

    /// <summary>
    ///     设置z扫描区域
    /// </summary>
    public const int PtzSetScan = 137;

    /// <summary>
    ///     设置热成像的伪彩
    /// </summary>
    public const int PtzSetColor = 140;

    /// <summary>
    ///     云台自检
    /// </summary>
    public const int PtzSelfCheck = 255;

    public const int Rs485RecvProcDef = 0x00;
    public const int Rs485RecvProcPm25 = 0x01;
    public const int Rs485RecvProcOsd = 0x02;
    public const int Rs485RecvProcVdm = 0x03;
    public const int Rs485RecvProcVisCa = 0x04;
    public const int Rs485RecvProc511A = 0x05;
    public const int Rs485RecvProcLaser = 0x06;
    public const int Rs485RecvProcAiTrace = 0x07;
    public const int Rs485RecvProcThermal = 0x08;
    public const int Rs485RecvProc485Trans = 0x09;
    public const int Rs485RecvProc232Trans = 0x0A;
    public const int AlarmTypeAlarmIn = 0x00;
    public const int AlarmTypeDiskFull = 0x01;
    public const int AlarmTypeVideoLost = 0x02;
    public const int AlarmTypeMotion = 0x03;
    public const int AlarmTypeDiskNotFormat = 0x04;
    public const int AlarmTypeDiskAccess = 0x05;
    public const int AlarmTypeCameraHide = 0x06;
    public const int AlarmTypeVideoStandard = 0x07;
    public const int AlarmTypeUnlawfulAccess = 0x08;

    /// <summary>
    ///     无存储计划报警
    /// </summary>
    public const int AlarmTypeWuCunChuJiHua = 9;

    /// <summary>
    ///     磁盘异常(不健康)
    /// </summary>
    public const int AlarmTypeDiskWarring = 10;

    /// <summary>
    ///     通道未录像
    /// </summary>
    public const int AlarmTypeTongDaoWeiLuXiang = 11;

    /// <summary>
    ///     前端信号量报警
    /// </summary>
    public const int AlarmTypeQianDuanXinHaoBaoJin = 12;

    /// <summary>
    ///     跨线报警
    /// </summary>
    public const int AlarmTypeAcrossLine = 13;

    /// <summary>
    ///     区域闯入报警
    /// </summary>
    public const int AlarmTypeInto = 14;

    /// <summary>
    ///     遗留物体报警
    /// </summary>
    public const int AlarmTypeAbandon = 15;

    public const int FlaFsPtzPos = 0x00000001;
    public const int FlaFsZoomUPos = 0x00000002;
    public const int FlaFsFocusPos = 0x00000004;
    public const int FlaFsDZoomPos = 0x00000008;
    public const int FlaFsZoomBs = 0x00000010;
    public const int TmccDriveUnknown = 0x00;
    public const int TmccDriveNoRootDir = 0x01;
    public const int TmccDriveRemovable = 0x02;
    public const int TmccDriveFixed = 0x03;
    public const int TmccDriveRemote = 0x04;
    public const int TmccDriveCdRom = 0x05;
    public const int TmccDriveRamDisk = 0x06;
    public const int TmccDriveSdRam = 0x07;
    public const int WdrModeHdr = 0x01;
    public const int WdrModeHiSo = 0x02;
    public const int WdrModeLowFrameRate = 0x04;
    public const int ExPoSureModeAutor = 0x00;
    public const int ExPoSureModeAutorEx = 0x01;
    public const int ExPoSureModeManualByShutter = 0x07;
    public const int ExPoSureModeManualByAperture = 0x08;
    public const int ExPoSureModeManualByAgc = 0x09;
    public const int ExPoSureModeAutor1 = 0x10;
    public const int ExPoSureModeAutor2 = 0x20;
    public const int ExPoSureModeAutor1Ex = 0x11;
    public const int ExPoSureModeAutor2Ex = 0x21;
    public const int ExPoSureModeAutor3Ex = 0x31;
    public const int IrShutModeAuto = 0x00;
    public const int IrShutModeSchedTime = 0x01;
    public const int IrShutModeAlarmIn = 0x02;
    public const int IrShutModeManual = 0xFF;
    public const int ZoomAutoFocus = 0x00;
    public const int ZoomThenFocus = 0x01;
    public const int ZoomManualFocus = 0x02;
    public const int ZoomCurveTypeNormal = 0x00;
    public const int ZoomCurveType850Nm = 0x01;
    public const int ZoomCurveType950Nm = 0x02;
    public const int DisCmdNone = 0x00;

    /// <summary>
    ///     设置当前的选中窗口号
    /// </summary>
    public const int DisCmdSetCurView = 0x01;

    /// <summary>
    ///     切换窗口数
    /// </summary>
    public const int DisCmdSwitchWin = 0x02;

    /// <summary>
    ///     当前显示模式下一屏
    /// </summary>
    public const int DisCmdNextScreen = 0x03;

    /// <summary>
    ///     设置音频是否输出
    /// </summary>
    public const int DisCmdEnableAudio = 0x04;

    /// <summary>
    ///     设置显示输出偏移
    /// </summary>
    public const int DisCmdViewOffset = 0x05;

    /// <summary>
    ///     设置显示输出画布大小
    /// </summary>
    public const int DisCmdViewScreen = 0x06;

    /// <summary>
    ///     设置显示位置模式，0-默认内部计算，1-外部计算
    /// </summary>
    public const int DisCmdRectMode = 0x07;

    /// <summary>
    ///     设置显示位置
    /// </summary>
    public const int DisCmdViewRect = 0x08;

    /// <summary>
    ///     交换两个窗口的通道
    /// </summary>
    public const int DisCmdSwitchChan = 0x09;

    /// <summary>
    ///     设置旋转角度
    /// </summary>
    public const int DisCmdRotate = 0x0A;

    /// <summary>
    ///     播放声音文件
    /// </summary>
    public const int DisCmdPlaySound = 0x0B;

    /// <summary>
    ///     播放数据流
    /// </summary>
    public const int DisCmdPlayAudioStream = 0x0C;

    /// <summary>
    ///     关闭声音文件
    /// </summary>
    public const int DisCmdStopSound = 0x0D;

    /// <summary>
    ///     打开播放文件
    /// </summary>
    public const int DisCmdPlayVideo = 0x0E;

    /// <summary>
    ///     停止播放文件
    /// </summary>
    public const int DisCmdStopVideo = 0x0F;

    /// <summary>
    ///     关闭声音文件
    /// </summary>
    public const int DisCmdStopAudioStream = 0x10;

    /// <summary>
    ///     列举音频文件名
    /// </summary>
    public const int DisCmdENumAudioFile = 0x21;

    /// <summary>
    ///     删除音频文件名
    /// </summary>
    public const int DisCmdDeleteAudioFile = 0x22;

    /// <summary>
    ///     列举视频文件名
    /// </summary>
    public const int DisCmdENumVideoFile = 0x23;

    /// <summary>
    ///     删除视频文件名
    /// </summary>
    public const int DisCmdDeleteVideoFile = 0x24;

    /// <summary>
    ///     设置其他显示区域
    /// </summary>
    public const int DisCmdOtherVideoRect = 0x30;

    /// <summary>
    ///     设置其他是否显示
    /// </summary>
    public const int DisCmdOtherVideoEnable = 0x31;

    /// <summary>
    ///     chunk is a 'LIST'
    /// </summary>
    public const int AviIfList = 0x00000001;

    /// <summary>
    ///     this frame is a key frame.
    /// </summary>
    public const int AviIfKeyframe = 0x00000010;

    /// <summary>
    ///     this frame doesn't take any Time
    /// </summary>
    public const int AviIfNoTime = 0x00000100;

    /// <summary>
    ///     these bits are for Compressor use
    /// </summary>
    public const int AviIfComPuSe = 0x0FFF0000;

    /// <summary>
    ///     解码显示播放，需要本地缓冲，此方式占用网络带快大
    /// </summary>
    public const int RemotePlayModeBufFile = 0x00;

    /// <summary>
    ///     解码显示，不带本地缓冲，此方式占用网络带宽与码流大小一致
    /// </summary>
    public const int RemotePlayModeNoBufFile = 0x01;

    /// <summary>
    ///     老方式播放文件，主要是a2的摄像机
    /// </summary>
    public const int RemotePlayModeOldFile = 0x02;

    /// <summary>
    ///     本地文件
    /// </summary>
    public const int RemotePlayModeLocalFile = 0x03;

    /// <summary>
    ///     不解码显示，为TMCC_ReadFile提供支持
    /// </summary>
    public const int RemotePlayModeReadFile = 0x04;

    /// <summary>
    ///     服务器控制视频的速率
    /// </summary>
    public const int RemotePlayModeControlFile = 0x05;

    /// <summary>
    ///     播放,以iPlayData作为播放参数(0-保留当前设置,1-回复默认)
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
    ///     快放,以iSpeed作为速度
    /// </summary>
    public const int PlayControlFast = 3;

    /// <summary>
    ///     慢放,以iSpeed作为速度
    /// </summary>
    public const int PlayControlSlow = 4;

    /// <summary>
    ///     seek,以iCurrentPosition
    /// </summary>
    public const int PlayControlSeekPos = 5;

    /// <summary>
    ///     seek,以dwCurrentTime作为时间
    /// </summary>
    public const int PlayControlSeekTime = 6;

    /// <summary>
    ///     stemp,单帧播放
    /// </summary>
    public const int PlayControlStemp = 7;

    /// <summary>
    ///     切换文件,以szFileName作为文件名/或structTime时间
    /// </summary>
    public const int PlayControlSwitch = 8;

    /// <summary>
    ///     音频开关,以iEnableAudio作为开关
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
    ///     seek,以structTime作为时间  绝对时间
    /// </summary>
    public const int PlayControlSeekTimeNew = 14;

    /// <summary>
    ///     修复文件索引
    /// </summary>
    public const int PlayControlRepairFile = 15;

    /// <summary>
    ///     第三方分析缓存大小
    /// </summary>
    public const int ThirdAnalyseBufSize = 65536;

    /// <summary>
    ///     每个闯入区域的最大顶点个数
    /// </summary>
    public const int MaxPointNumInPolygon = 20;

    /// <summary>
    ///     跨线最大数
    /// </summary>
    public const int MaxCheckNum = 5;

    /// <summary>
    ///     左边跨到右边
    /// </summary>
    public const int MotionLineMethodLeftToRight = 0x01;

    /// <summary>
    ///     右边跨到左边
    /// </summary>
    public const int MotionLineMethodRightToLeft = 0x02;

    /// <summary>
    ///     上边跨到下边
    /// </summary>
    public const int MotionLineMethodTopToBottom = 0x04;

    /// <summary>
    ///     下边跨到上边
    /// </summary>
    public const int MotionLineMethodBottomToTop = 0x08;

    /// <summary>
    ///     右边为进
    /// </summary>
    public const int FlowLineActionRight = 0x04;

    /// <summary>
    ///     左边为进
    /// </summary>
    public const int FlowLineActionLeft = 0x03;

    /// <summary>
    ///     下边为进
    /// </summary>
    public const int FlowLineActionBottom = 0x02;

    /// <summary>
    ///     上边为进
    /// </summary>
    public const int FlowLineActionTop = 0x01;

    /// <summary>
    ///     用户操作相关命令信息
    /// </summary>
    public const int TmccMajorCmdUserCfg = 0x101;

    /// <summary>
    ///     修改系统默认用户信息
    /// </summary>
    public const int TmccMinorCmdUserModifyDefaultUser = 0x00;

    /// <summary>
    ///     所有用户信息
    /// </summary>
    public const int TmccMinorCmdUserInfo = 0x01;

    /// <summary>
    ///     单个用户信息
    /// </summary>
    public const int TmccMinorCmdUserSingleInfo = 0x02;

    /// <summary>
    ///     添加用户
    /// </summary>
    public const int TmccMinorCmdUserAdd = 0x03;

    /// <summary>
    ///     删除用户
    /// </summary>
    public const int TmccMinorCmdUserDelete = 0x04;

    /// <summary>
    ///     修改用户密码
    /// </summary>
    public const int TmccMinorCmdUserModifyPassword = 0x05;

    /// <summary>
    ///     修改用户权限
    /// </summary>
    public const int TmccMinorCmdUserModifyRight = 0x06;

    /// <summary>
    ///     验证用户
    /// </summary>
    public const int TmccMinorCmdUserVerify = 0x07;

    /// <summary>
    ///     摄像机扩展数据，可以有用户保留，摄像机只保留数据
    /// </summary>
    public const int TmccMinorCmdUserData = 0x08;

    /// <summary>
    ///     设备加密密钥设置
    /// </summary>
    public const int TmccMinorCmdEncryptKey = 0x09;

    /// <summary>
    ///     得到加密数据
    /// </summary>
    public const int TmccMinorCmdGetEncryptData = 0x0A;

    /// <summary>
    ///     onvif参数
    /// </summary>
    public const int TmccMinorCmdOnvifCfg = 0x0B;

    /// <summary>
    ///     设备参数
    /// </summary>
    public const int TmccMajorCmdDeviceCfg = 0x102;

    /// <summary>
    ///     设备参数
    /// </summary>
    public const int TmccMinorCmdDevice = 0x00;

    /// <summary>
    ///     扩展设备参数
    /// </summary>
    public const int TmccMinorCmdDeviceEx = 0x01;

    /// <summary>
    ///     网络参数
    /// </summary>
    public const int TmccMajorCmdNetCfg = 0x103;

    /// <summary>
    ///     本地有线网络参数
    /// </summary>
    public const int TmccMinorCmdLanNetCfg = 0x00;

    /// <summary>
    ///     本地无线网络参数
    /// </summary>
    public const int TmccMinorCmdWifiNetCfg = 0x01;

    /// <summary>
    ///     RTSP组播发送配置参数
    /// </summary>
    public const int TmccMinorCmdRtspMultiCastCfg = 0x02;

    /// <summary>
    ///     p2p代理模块配置信息
    /// </summary>
    public const int TmccMinorCmdP2PCfg = 0x03;

    /// <summary>
    ///     rtmp服务器配置
    /// </summary>
    public const int TmccMinorCmdRtmpCfg = 0x04;

    /// <summary>
    ///     取图象参数
    /// </summary>
    public const int TmccMajorCmdPicCfg = 0x104;

    /// <summary>
    ///     取图象参数(合成结构)
    /// </summary>
    public const int TmccMinorCmdPic = 0x00;

    /// <summary>
    ///     取图象OSD参数
    /// </summary>
    public const int TmccMinorCmdOsd = 0x01;

    /// <summary>
    ///     取图象遮盖参数
    /// </summary>
    public const int TmccMinorCmdMask = 0x02;

    /// <summary>
    ///     取图象视频丢失参数
    /// </summary>
    public const int TmccMinorCmdLost = 0x03;

    /// <summary>
    ///     取图象移动检测参数
    /// </summary>
    public const int TmccMinorCmdMotion = 0x04;

    /// <summary>
    ///     取图象遮挡报警参数
    /// </summary>
    public const int TmccMinorCmdHideAlarm = 0x05;

    /// <summary>
    ///     添加特殊OSD
    /// </summary>
    public const int TmccMinorCmdExpandOsdStr = 0x06;

    /// <summary>
    ///     添加特殊OSD
    /// </summary>
    public const int TmccMinorCmdAddExpandOsd = 0x06;

    /// <summary>
    ///     清除特殊OSD
    /// </summary>
    public const int TmccMinorCmdExpandOsdClear = 0x07;

    /// <summary>
    ///     清除特殊OSD
    /// </summary>
    public const int TmccMinorCmdClearExpandOsd = 0x07;

    /// <summary>
    ///     OSD颜色方案配置
    /// </summary>
    public const int TmccMinorCmdOsdColor = 0x08;

    /// <summary>
    ///     当前的特殊OSD
    /// </summary>
    public const int TmccMinorCmdExpandOsdDisplay = 0x12;

    /// <summary>
    ///     当前的特殊OSD
    /// </summary>
    public const int TmccMinorCmdExpandOsdAttr = 0x13;

    /// <summary>
    ///     设置特殊OSD参数
    /// </summary>
    public const int TmccMinorCmdExpandOsdReset = 0x14;

    /// <summary>
    ///     压缩参数
    /// </summary>
    public const int TmccMajorCmdCompressCfg = 0x105;

    /// <summary>
    ///     设置编码参数
    /// </summary>
    public const int TmccMinorCmdCompressCfg = 0x00;

    /// <summary>
    ///     查询编码能力
    /// </summary>
    public const int TmccMinorCmdQueryCompressCapability = 0x01;

    /// <summary>
    ///     扩展查询编码能力
    /// </summary>
    public const int TmccMinorCmdQueryCompressCapabilityEx = 0x02;

    /// <summary>
    ///     设置区域编码质量参数
    /// </summary>
    public const int TmccMinorCmdQproiCfg = 0x03;

    /// <summary>
    ///     录像时间参数
    /// </summary>
    public const int TmccMajorCmdRecordCfg = 0x106;

    /// <summary>
    ///     本地录像设置
    /// </summary>
    public const int TmccMinorCmdLocalRecordCfg = 0x00;

    /// <summary>
    ///     FTP录像设置
    /// </summary>
    public const int TmccMinorCmdFtpRecordCfg = 0x01;

    /// <summary>
    ///     巡航参数设置
    /// </summary>
    public const int TmccMajorCmdDecoderCfg = 0x107;

    /// <summary>
    ///     485参数设置
    /// </summary>
    public const int TmccMinorCmd485Cfg = 0x00;

    /// <summary>
    ///     巡航参数设置
    /// </summary>
    public const int TmccMinorCmdCruiseCfg = 0x01;

    /// <summary>
    ///     云台参数配置
    /// </summary>
    public const int TmccMinorCmdPtzCfg = 0x02;

    /// <summary>
    ///     预置点参数配置
    /// </summary>
    public const int TmccMinorCmdPresetCfg = 0x03;

    /// <summary>
    ///     一体机配置参数
    /// </summary>
    public const int TmccMinorCmdZoomCfg = 0x04;

    /// <summary>
    ///     轨迹参数配置
    /// </summary>
    public const int TmccMinorCmdTrackCfg = 0x05;

    /// <summary>
    ///     线扫参数配置
    /// </summary>
    public const int TmccMinorCmdLineCfg = 0x06;

    /// <summary>
    ///     辅助开关参数配置
    /// </summary>
    public const int TmccMinorCmdAuxCfg = 0x07;

    /// <summary>
    ///     光圈参数配置
    /// </summary>
    public const int TmccMinorCmdApertureCfg = 0x08;

    /// <summary>
    ///     PTZ模块扩展功能使能配置
    /// </summary>
    public const int TmccMinorCmdPtzCfgEx = 0x09;

    /// <summary>
    ///     3D修正配置
    /// </summary>
    public const int TmccMinorCmd3DAmendment = 0x0A;

    /// <summary>
    ///     云台或球机信息配置
    /// </summary>
    public const int TmccMinorCmdPtzInfo = 0x0B;

    /// <summary>
    ///     看守卫信息配置
    /// </summary>
    public const int TmccMinorCmdPtzSchedCfg = 0x0C;

    /// <summary>
    ///     232串口参数
    /// </summary>
    public const int TmccMajorCmdRs232Cfg = 0x108;

    /// <summary>
    ///     232串口参数
    /// </summary>
    public const int TmccMinorCmdRs232Cfg = 0x00;

    /// <summary>
    ///     报警输入参数
    /// </summary>
    public const int TmccMajorCmdAlarmInCfg = 0x109;

    /// <summary>
    ///     报警输入参数
    /// </summary>
    public const int TmccMinorCmdAlarmInCfg = 0x00;

    /// <summary>
    ///     IR报警输入参数
    /// </summary>
    public const int TmccMinorCmdIrAlarmInCfg = 0x01;

    /// <summary>
    ///     报警输出参数
    /// </summary>
    public const int TmccMajorCmdAlarmOutCfg = 0x10A;

    /// <summary>
    ///     DVR时间
    /// </summary>
    public const int TmccMajorCmdTimeCfg = 0x10B;

    /// <summary>
    ///     时间配置
    /// </summary>
    public const int TmccMinorCmdTimeCfg = 0x00;

    /// <summary>
    ///     时区配置
    /// </summary>
    public const int TmccMinorCmdZoneCfg = 0x01;

    /// <summary>
    ///     夏令时配置
    /// </summary>
    public const int TmccMinorCmdDayLightCfg = 0x02;

    /// <summary>
    ///     预览参数
    /// </summary>
    public const int TmccMajorCmdPreviewCfg = 0x10C;

    /// <summary>
    ///     颜色设置
    /// </summary>
    public const int TmccMinorCmdPreviewColorCfg = 0x00;

    /// <summary>
    ///     压缩偏移参数
    /// </summary>
    public const int TmccMinorCmdCompressOffsetCfg = 0x01;

    /// <summary>
    ///     预览偏移参数
    /// </summary>
    public const int TmccMinorCmdPreviewOffsetCfg = 0x02;

    /// <summary>
    ///     设置图像模式(add by zzt 2009-09-17)
    /// </summary>
    public const int TmccMinorCmdPicMode = 0x03;

    /// <summary>
    ///     临时预览色彩参数
    /// </summary>
    public const int TmccMinorCmdTempPreviewColorCfg = 0x04;

    /// <summary>
    ///     视频输出参数
    /// </summary>
    public const int TmccMajorCmdVideoOutCfg = 0x10D;

    /// <summary>
    ///     视频输出参数
    /// </summary>
    public const int TmccMinorCmdVideoOut = 0x00;

    /// <summary>
    ///     配置连接信息
    /// </summary>
    public const int TmccMinorCmdConnectInfo = 0x01;

    /// <summary>
    ///     列举连接信息
    /// </summary>
    public const int TmccMinorCmdENumConnectInfo = 0x02;

    /// <summary>
    ///     窗口信息
    /// </summary>
    public const int TmccMinorCmdWindowsInfo = 0x03;

    /// <summary>
    ///     解码器显示信息
    /// </summary>
    public const int TmccMinorCmdDisPlayCfg = 0x04;

    /// <summary>
    ///     锁定当前连接，标示非循环连接
    /// </summary>
    public const int TmccMinorCmdLock = 0x05;

    /// <summary>
    ///     清除循环列表，保留当前连接
    /// </summary>
    public const int TmccMinorCmdClear = 0x06;

    /// <summary>
    ///     开始连接
    /// </summary>
    public const int TmccMinorCmdConnect = 0x07;

    /// <summary>
    ///     断开连接
    /// </summary>
    public const int TmccMinorCmdDisConnect = 0x08;

    /// <summary>
    ///     解码窗口描述
    /// </summary>
    public const int TmccMinorCmdWindowCapability = 0x09;

    /// <summary>
    ///     视频输出偏移
    /// </summary>
    public const int TmccMinorCmdVideoOutOffset = 0x0A;

    /// <summary>
    ///     解码器屏幕描述
    /// </summary>
    public const int TmccMinorCmdScreenInfo = 0x0B;

    /// <summary>
    ///     输出颜色设置
    /// </summary>
    public const int TmccMinorCmdCscColorCfg = 0x0C;

    ///<summary>输出控制</summary>
    public const int TmccMinorCmdVOutControl = 0x0D;

    /// <summary>
    ///     列举播放的文件
    /// </summary>
    public const int TmccMinorCmdENumVideoFile = 0x0E;

    /// <summary>
    ///     异常参数
    /// </summary>
    public const int TmccMajorCmdExceptionCfg = 0x10E;

    /// <summary>
    ///     服务器消息
    /// </summary>
    public const int TmccMajorCmdServerMessage = 0x10F;

    /// <summary>
    ///     服务器调试信息，调试版本有效
    /// </summary>
    public const int TmccMinorCmdDebugInfo = 0x01;

    /// <summary>
    ///     服务器启动
    /// </summary>
    public const int TmccMinorCmdServerStart = 0x02;

    /// <summary>
    ///     服务器退出
    /// </summary>
    public const int TmccMinorCmdServerStop = 0x03;

    /// <summary>
    ///     报警上传管理中心具体信息看tmToManagerAlarmInfo_t
    /// </summary>
    public const int TmccMinorCmdServerAlarm = 0x04;

    /// <summary>
    ///     服务器希望被同步时间
    /// </summary>
    public const int TmccMinorCmdServerWantSyncTime = 0x05;

    /// <summary>
    ///     服务器报警图片上传,具体信息看tmToManagerImageInfo_t
    /// </summary>
    public const int TmccMinorCmdServerImage = 0x06;

    /// <summary>
    ///     云台控制被其它用户关闭
    /// </summary>
    public const int TmccMinorCmdPtzCloseByOtherUser = 0x07;

    /// <summary>
    ///     云台控制时间超时关闭
    /// </summary>
    public const int TmccMinorCmdPtzCloseByTimeOut = 0x08;

    /// <summary>
    ///     服务器返回的串口数据
    /// </summary>
    public const int TmccMinorCmdSerialMessage = 0x09;

    /// <summary>
    ///     设备的心跳包
    /// </summary>
    public const int TmccMinorCmdServerLiveHeart = 0x0A;

    /// <summary>
    ///     光圈校正完毕
    /// </summary>
    public const int TmccMinorCmdApertureRepairComplete = 0x0B;

    /// <summary>
    ///     服务器坏点修复完毕
    /// </summary>
    public const int TmccMinorCmdBadPixRepairComplete = 0x0C;

    /// <summary>
    ///     安全卫士报警上传管理中心具体信息看tmToManagerSafeGuardInfo_t
    /// </summary>
    public const int TmccMinorCmdSafeguardInfo = 0x0D;

    ///<summary>设备倍焦效验完毕</summary>
    public const int TmccMinorCmdZoomRepairComplete = 0x0E;

    ///<summary>设备白平衡效验需求操作</summary>
    public const int TmccMinorCmdAwbRepairRequest = 0x0F;

    ///<summary>设备调用云台</summary>
    public const int TmccMinorCmdServerCallPtz = 0x10;

    ///<summary>设备镜头暗角修正需求操作</summary>
    public const int TmccMinorCmdLensRepairRequest = 0x11;

    ///<summary>云台设置的位置到位了</summary>
    public const int TmccMinorCmdArrivePtzPoint = 0x12;

    ///<summary>机芯设置的位置到位了</summary>
    public const int TmccMinorCmdArriveZoomPoint = 0x13;

    /// <summary>设备FPN修正需求操作</summary>
    public const int TmccMinorCmdFpnRepairRequest = 0x14;

    /// <summary>摄像机测温信息</summary>
    public const int TmccMinorCmdFlirInfo = 0x20;

    /// <summary>摄像机测温信息扩展</summary>
    public const int TmccMinorCmdFlirInfoEx = 0x21;

    ///<summary>摄像机测温分组消息</summary>
    public const int TmccMinorCmdFlirGroupInfo = 0x22;

    ///<summary>摄像机测温区域报警信息</summary>
    public const int TmccMinorCmdFlirAlarmInfo = 0x23;

    ///<summary>摄像机测温分组报警消息</summary>
    public const int TmccMinorCmdFlirGroupAlarmInfo = 0x24;

    ///<summary>服务器云台控制与串口控制</summary>
    public const int TmccMajorCmdPtzControl = 0x110;

    ///<summary>得到云台控制解码器协议</summary>
    public const int TmccMinorCmdPtzGetDecoderList = 0x00;

    ///<summary>打开云台控制</summary>
    public const int TmccMinorCmdPtzOpen = 0x01;

    /// <summary>打开云台控制和锁定云台</summary>
    public const int TmccMinorCmdPtzOpenAndLock = 0x02;

    ///<summary>关闭云台控制</summary>
    public const int TmccMinorCmdPtzClose = 0x03;

    ///<summary>发送云台控制命令</summary>
    public const int TmccMinorCmdPtzSendCmd = 0x04;

    ///<summary>发送透明传输数据</summary>
    public const int TmccMinorCmdPtzTrans = 0x05;

    ///<summary>打开串口通明通道</summary>
    public const int TmccMinorCmdSerialOpen = 0x06;

    ///<summary>关闭通明通道</summary>
    public const int TmccMinorCmdSerialClose = 0x07;

    ///<summary>发送通明通道数据</summary>
    public const int TmccMinorCmdSerialSend = 0x08;

    ///<summary>读取当前云台信息</summary>
    public const int TmccMinorCmdGetPtzInfo = 0x09;

    ///<summary>接收通明通道数据</summary>
    public const int TmccMinorCmdSerialRecv = 0x0A;

    ///<summary>升级球机程序</summary>
    public const int TmccMinorCmdUpgradePtz = 0x0B;

    ///<summary>获取云台版本信息</summary>
    public const int TmccMinorCmdGetPtzVersion = 0x0C;

    ///<summary>读取当前所有云台信息</summary>
    public const int TmccMinorCmdGetAllPtzInfo = 0x0D;

    ///<summary>获取Zoom所有可视角</summary>
    public const int TmccMinorCmdGetAllViewSize = 0x0E;

    ///<summary>调用直接到顶点</summary>
    public const int TmccMinorCmdGotoZoomTop = 0x0F;

    ///<summary>设置导出预置点的启始预置点号</summary>
    public const int TmccMinorCmdCfgPtzPresetPosNo = 0x10;

    ///<summary>导入导出云台预置点位置,当是GET时,预置点号自动增加</summary>
    public const int TmccMinorCmdPtzPresetInfo = 0x11;

    ///<summary>根据当前倍数的当前垂直角度下的水平可视角</summary>
    public const int TmccMinorCmdGetHorAngleByVer = 0x12;

    ///<summary>获取指定倍数的水平可视角和垂直可视角</summary>
    public const int TmccMinorCmdGetPzInfoByZoom = 0x13;

    ///<summary>获取镜头位置列表配置</summary>
    public const int TmccMinorCmdZoomPosListCfg = 0x14;

    ///<summary>修改镜头位置列表某项</summary>
    public const int TmccMinorCmdZoomPosListMod = 0x15;

    ///<summary>删除镜头位置列表某项</summary>
    public const int TmccMinorCmdZoomPosListDel = 0x16;

    ///<summary>ptz的绝对位置</summary>
    public const int TmccMinorCmdPtzAbsolute = 0x17;

    ///<summary>远程服务器控制</summary>
    public const int TmccMajorCmdServerControl = 0x111;

    ///<summary>报警信号上传服务器</summary>
    public const int TmccMinorCmdRemOntAlarmIn = 0x01;

    ///<summary>服务器状态灯开启</summary>
    public const int TmccMinorCmdOpenLight = 0x02;

    ///<summary>服务器状态灯关闭</summary>
    public const int TmccMinorCmdCloseLight = 0x03;

    ///<summary>服务器蜂鸣器开启</summary>
    public const int TmccMinorCmdOpenBuzzer = 0x04;

    ///<summary>服务器蜂鸣器关闭</summary>
    public const int TmccMinorCmdCloseBuzzer = 0x05;

    ///<summary>得到服务器工作状态</summary>
    public const int TmccMinorCmdGetWorkState = 0x06;

    ///<summary>手动关闭报警输出</summary>
    public const int TmccMinorCmdReMontCloseAlarmOut = 0x07;

    ///<summary>主动打开报警输出 </summary>
    public const int TmccMinorCmdReMontOpenAlarmOut = 0x08;

    ///<summary>系统复位</summary>
    public const int TmccMinorCmdReset = 0x09;

    ///<summary>视频移动报警信号</summary>
    public const int TmccMinorCmdReMontVideoMotion = 0x0A;

    ///<summary>视频丢失报警信号</summary>
    public const int TmccMinorCmdReMontVideoLost = 0x0B;

    ///<summary>视频遮挡报警信号</summary>
    public const int TmccMinorCmdReMontVideoHide = 0x0C;

    ///<summary>远请求升级</summary>
    public const int TmccMinorCmdRequestUpgrade = 0x0D;

    ///<summary>打开红外滤光片</summary>
    public const int TmccMinorCmdOpenIrCut = 0x0E;

    ///<summary>打开可见光滤光片</summary>
    public const int TmccMinorCmdCloseIrCut = 0x0F;

    ///<summary>远程手动抓图传到本地</summary>
    public const int TmccMinorCmdManualCapture = 0x10;

    ///<summary>自动查找Focus的值</summary>
    public const int TmccMinorCmdAutoApertureRepair = 0x11;

    ///<summary>升级后保留IP地址信息</summary>
    public const int TmccMinorCmdUpgradeKeepIp = 0x12;

    ///<summary>系统立即同步NTP服务器</summary>
    public const int TmccMinorCmdSyncNtpServer = 0x13;

    ///<summary>打开手动录像</summary>
    public const int TmccMinorCmdStartManualRecord = 0x14;

    ///<summary>关闭手动录像</summary>
    public const int TmccMinorCmdStopManualRecord = 0x15;

    ///<summary>命令</summary>
    public const int TmccMinorCmdHandle = 0x16;

    ///<summary>保存配置信息</summary>
    public const int TmccMinorCmdSaveConfigInfo = 0x17;

    ///<summary>控制安全卫士的状态灯</summary>
    public const int TmccMinorCmdControlSafeDoor = 0x18;

    ///<summary>保存当前Zoom位置的Focus值以备切换IrCut使用</summary>
    public const int TmccMinorCmdSaveIrCutFocus = 0x19;

    ///<summary>效验防抖传感器</summary>
    public const int TmccMinorCmdTestCameAtIngLe = 0x1A;

    ///<summary>强制临时关闭OSD</summary>
    public const int TmccMinorCmdCloseOsdView = 0x1B;

    ///<summary>强制临时打开OSD(是否显示看OSD的设置)</summary>
    public const int TmccMinorCmdOpenOsdView = 0x1C;

    ///<summary>暂停手动录像wang jun add</summary>
    public const int TmccMinorCmdPauseManualRecord = 0x1D;

    ///<summary>智能模块报警</summary>
    public const int TmccMinorCmdIntelligent = 0x1E;

    ///<summary>打开透雾滤光片</summary>
    public const int TmccMinorCmdOpenDefOgCut = 0x1F;

    ///<summary>远程扩展手动抓图传到本地</summary>
    public const int TmccMinorCmdManualCaptureEx = 0x20;

    ///<summary>开始FLIR透传</summary>
    public const int TmccMinorCmdStartFlirTrans = 0x21;

    ///<summary>关闭FLIR透传</summary>
    public const int TmccMinorCmdStopFlirTrans = 0x22;

    ///<summary>FLIR透传数据</summary>
    public const int TmccMinorCmdFlirTrans = 0x23;

    ///<summary>热源报警信号</summary>
    public const int TmccMinorCmdReMontVideoFlir = 0x24;

    ///<summary>使用指定结构信息抓图</summary>
    public const int TmccMinorCmdManualCaptureC = 0x25;

    ///<summary>合上镜头遮挡片</summary>
    public const int TmccMinorCmdOpenCover = 0x26;

    ///<summary>分开镜头遮挡片</summary>
    public const int TmccMinorCmdCloseCover = 0x27;

    ///<summary>打开鼠标操作</summary>
    public const int TmccMinorCmdMouseOpen = 0x28;

    ///<summary>关闭鼠标操作</summary>
    public const int TmccMinorCmdMouseClose = 0x29;

    ///<summary>鼠标操作命令 </summary>
    public const int TmccMinorCmdMouseCmd = 0x2A;

    ///<summary>报警处理参数</summary>
    public const int TmccMajorCmdAlarmCfg = 0x112;

    ///<summary>磁盘处理命令</summary>
    public const int TmccMajorCmdDiskControl = 0x113;

    ///<summary>得到物理硬盘信息</summary>
    public const int TmccMinorCmdDriveInfo = 0x01;

    ///<summary>初始化物理硬盘</summary>
    public const int TmccMajorCmdNetCfgEx = 0x114;

    public const int TmccMinorCmdNtpCfg = 0x00;
    public const int TmccMinorCmdFtpCfg = 0x01;
    public const int TmccMinorCmdSmtpCfg = 0x02;

    ///<summary>心跳包</summary>
    public const int TmccMinorCmdLiveHeartCfg = 0x03;

    ///<summary>DDns</summary>
    public const int TmccMinorCmdDDnsCfg = 0x04;

    ///<summary>设备主动上传配置</summary>
    public const int TmccMinorCmdUpCfg = 0x05;

    ///<summary>检测视频流发送状态</summary>
    public const int TmccMinorCmdStreamSendStatus = 0x06;

    ///<summary>扩展压缩参数</summary>
    public const int TmccMajorCmdCompressCfgA = 0x115;

    ///<summary>图像压缩参数</summary>
    public const int TmccMinorCmdCompressCfgA = 0x00;

    ///<summary>视频输入配置</summary>
    public const int TmccMajorCmdVideoInCfg = 0x116;

    ///<summary>输入配置</summary>
    public const int TmccMinorCmdVideoIn = 0x00;

    ///<summary>IrCut切换对应输入配置</summary>
    public const int TmccMinorCmdIrCutVideoIn = 0x01;

    ///<summary>按时间布防的输入配置</summary>
    public const int TmccMinorCmdSchedVideoIn = 0x02;

    ///<summary>临时预览VideoIn参数</summary>
    public const int TmccMinorCmdVideoInPreview = 0x03;

    ///<summary>白平衡参数设置</summary>
    public const int TmccMinorCmdAwbCfg = 0x04;

    ///<summary>强光抑制</summary>
    public const int TmccMinorCmdLightInHibition = 0x05;

    ///<summary>临时预览IRCutVin参数</summary>
    public const int TmccMinorCmdIrCutVideoInPreview = 0x06;

    ///<summary>临时预览SchedVideoIn参数</summary>
    public const int TmccMinorCmdSchedVideoInPreview = 0x07;

    ///<summary>镜头畸变校正参数参数</summary>
    public const int TmccMinorCmdLenSdeWarp = 0x08;

    ///<summary>镜头热像布防参数</summary>
    public const int TmccMinorCmdVideoFlir = 0x09;

    ///<summary>镜头热像区域测温参数(配置是iStream=0为默认配置，1-MAX_PRESET为对应预置点配置)</summary>
    public const int TmccMinorCmdVideoFlirArea = 0x0A;

    ///<summary>镜头热像点测温参数</summary>
    public const int TmccMinorCmdVideoFlirSpot = 0x0B;

    ///<summary>镜头热像点测温参数预置点绑定</summary>
    public const int TmccMinorCmdVideoFlirAreaToPreset = 0x0C;

    ///<summary>AE参数配置(agc, shutter,dGain)</summary>
    public const int TmccMinorCmdAeConfig = 0x0D;

    ///<summary>恢复AE控制模式</summary>
    public const int TmccMinorCmdRestoreAe = 0x0E;

    ///<summary>手动配置WB GAIN</summary>
    public const int TmccMinorCmdWbGain = 0x0F;

    ///<summary>镜头热像区域测温参数扩展(配置是iStream=0为默认配置，1-MAX_PRESET为对应预置点配置)</summary>
    public const int TmccMinorCmdVideoFlirAreaEx = 0x10;

    ///<summary>双光合成图像配置</summary>
    public const int TmccMinorCmdVideoFusion = 0x11;

    ///<summary>图像防抖功能</summary>
    public const int TmccMinorCmdVideoDis = 0x12;

    ///<summary>热像测温分组信息</summary>
    public const int TmccMinorCmdVideoFlirGroup = 0x13;

    ///<summary>临时测温配置</summary>
    public const int TmccMinorCmdVideoFlirAreaTemp = 0x14;

    ///<summary>黑体配置信息</summary>
    public const int TmccMinorCmdFlirBlackBody = 0x15;

    ///<summary>设置可将光视频视场偏移:用于双光显示位置的统一</summary>
    public const int TmccMinorCmdFlirViewOffset = 0x16;

    ///<summary>测温模式</summary>
    public const int TmccMinorCmdFlirWorMode = 0x17;

    ///<summary>rgb参数配置</summary>
    public const int TmccMinorCmdRgbCfg = 0x18;

    ///<summary>rgb区域配置</summary>
    public const int TmccMinorCmdRgbAreaCfg = 0x19;

    ///<summary></summary>
    public const int TmccMajorCmdShutterCfg = 0x117;

    ///<summary></summary>
    public const int TmccMajorCmdCaptureImageCfg = 0x118;

    ///<summary>抓图的配置</summary>
    public const int TmccMinorCmdCaptureImageCfg = 0x00;

    ///<summary>时间布防的配置</summary>
    public const int TmccMinorCmdCaptureSchedCfg = 0x01;

    public const int TmccMajorCmdAudioCfg = 0x119;
    public const int TmccMinorCmdAudioCfg = 0x00;
    public const int TmccMinorCmdENumAudioFile = 0x01;

    ///<summary>通道配置主命令</summary>
    public const int TmccMajorCmdChannelCfg = 0x11A;

    ///<summary>配置单个通道信息(设置/获取)</summary>
    public const int TmccMinorCmdSingleChannel = 0x00;

    ///<summary>配置所有通道信息(设置/获取)</summary>
    public const int TmccMinorCmdAllChannel = 0x01;

    ///<summary>配置单个通道RTSP名称(设置/获取)</summary>
    public const int TmccMinorCmdSingleRtspName = 0x02;

    ///<summary>配置所有通道RTSP名称(设置/获取)</summary>
    public const int TmccMinorCmdAllRtspName = 0x03;

    ///<summary>报警设备配置主命令</summary>
    public const int TmccMajorCmdAlarmDeviceCfg = 0x11B;

    ///<summary>配置单个报警设备信息(设置/获取)</summary>
    public const int TmccMinorCmdSingleAlarmDevice = 0x00;

    ///<summary>配置所有报警设备信息(设置/获取)</summary>
    public const int TmccMinorCmdAllAlarmDevice = 0x01;

    public const int TmccMajorCmdLogCfg = 0x11C;

    ///<summary>枚举所有日志</summary>
    public const int TmccMinorCmdENumLog = 0x00;

    ///<summary>清除日志</summary>
    public const int TmccMinorCmdClearLog = 0x01;

    public const int TmccMajorCmdConnectCfg = 0x11D;

    ///<summary>所有连接</summary>
    public const int TmccMinorCmdConnectListCfg = 0x00;

    ///<summary>清除解码连接</summary>
    public const int TmccMinorCmdClearConnect = 0x01;

    public const int TmccMajorCmdTheodoliteCfg = 0x11E;
    public const int TmccMajorCmdIrCutCfg = 0x11F;

    ///<summary>配置IrCut</summary>
    public const int TmccMinorCmdIrCutCfg = 0x00;

    public const int TmccMajorCmdUaRtReport = 0x120;

    ///<summary>存储状态上报</summary>
    public const int TmccMinorCmdStorageStatus = 0x00;

    ///<summary>人脸检测上报</summary>
    public const int TmccMinorCmdFaceCheck = 0x01;

    ///<summary>CIG消息</summary>
    public const int TmccMajorCmdCgiMsg = 0x123;

    public const int TmccMinorCmdCgiMsg = 0x00;

    ///<summary>设置调式或配置命令</summary>
    public const int TmccMajorCmdLocalSpecialCfg = 0x124;

    ///<summary>设置不同位数下红外灯的亮度</summary>
    public const int TmccMinorCmdPtzhongwailiangdu = 0x00;

    ///<summary>设置球机光敏阀值</summary>
    public const int TmccMinorCmdPtzguangminfazhi = 0x01;

    ///<summary>3D协议可以解决调式命令</summary>
    public const int TmccMinorCmd3DZoomAngle = 0x02;

    ///<summary>获取球当前光敏值</summary>
    public const int TmccMinorCmdGetPtzguangminzhi = 0x03;

    ///<summary>显示亮度、对比度、伽马、饱和度、色度等</summary>
    public const int TmccMajorCmdVideoParamCfg = 0x125;

    public const int TmccMinorCmdVideoParamCfg = 0x00;
    public const int TmccMajorCmdGraphicsCfg = 0x126;

    ///<summary>画布信息配置</summary>
    public const int TmccMinorCmdSurfaceCfg = 0x00;

    ///<summary>画线</summary>
    public const int TmccMinorCmdDrawLine = 0x01;

    ///<summary>画多边形</summary>
    public const int TmccMinorCmdDrawPoly = 0x02;

    ///<summary>画圆</summary>
    public const int TmccMinorCmdDrawCircle = 0x03;

    ///<summary>绘制矩形</summary>
    public const int TmccMinorCmdDrawRect = 0x04;

    ///<summary>绘制文字</summary>
    public const int TmccMinorCmdDrawText = 0x05;

    public const int TmccMajorCmdTracerCfg = 0x127;

    ///<summary>选择跟踪目标配置</summary>
    public const int TmccMinorCmdSelectObject = 0x00;

    ///<summary>智能分析配置</summary>
    public const int TmccMinorCmdAiVideoCfg = 0x01;

    ///<summary>停止跟踪</summary>
    public const int TmccMinorCmdStopTrace = 0x02;

    ///<summary>启动跟踪</summary>
    public const int TmccMinorCmdStartTrace = 0x03;

    ///<summary>跟踪属性参数配置</summary>
    public const int TmccMinorCmdParameter = 0x04;

    ///<summary>跟踪属性参数配置</summary>
    public const int TmccMinorCmdAiTraceCfg = 0x04;

    ///<summary>雷达联动信息配置</summary>
    public const int TmccMinorCmdRadarInfo = 0x05;

    ///<summary>智能识别回调设置</summary>
    public const int TmccMinorCmdAiRegisterCallback = 0x06;

    ///<summary>智能识别回调取消</summary>
    public const int TmccMinorCmdAiUnRegisterCallback = 0x07;

    ///<summary>智能识别参数设置(这个和目标跟踪目标不一样)</summary>
    public const int TmccMinorCmdAiObjectCfg = 0x08;

    ///<summary>设置智能分析方案</summary>
    public const int TmccMinorCmdAptItuDescOut = 0x09;

    ///<summary>设置人脸检测</summary>
    public const int TmccMinorCmdFaceDetect = 0x0A;

    ///<summary>设置越界检测</summary>
    public const int TmccMinorCmdLineDetect = 0x0B;

    ///<summary>设置区域闯入检测</summary>
    public const int TmccMinorCmdPolygonDetect = 0x0C;

    ///<summary>设置车流量检测</summary>
    public const int TmccMinorCmdFlowDetect = 0x0D;

    ///<summary>设置遗留物体检测</summary>
    public const int TmccMinorCmdAbandonDetect = 0x0E;

    ///<summary>获取智能分析模块移动区域</summary>
    public const int TmccMinorCmdMotionRect = 0x0F;

    ///<summary>设置枪球联动信息</summary>
    public const int TmccMinorCmdCamTrack = 0x10;

    ///<summary>第三方智能参数</summary>
    public const int TmccMinorCmdThirdAnalyse = 0x11;

    ///<summary>PID参数调校: 注意兼容早期版本</summary>
    public const int TmccMinorCmdPidParam = 0x12;

    ///<summary>开镜头标定模式</summary>
    public const int TmccMinorCmdOpenLensCheck = 0x13;

    ///<summary>关镜头标定模式</summary>
    public const int TmccMinorCmdCloseLensCheck = 0x14;

    ///<summary>获取跟踪模块状态</summary>
    public const int TmccMinorCmdGetTracerSoftSta = 0x15;

    ///<summary>干扰器开火</summary>
    public const int TmccMinorCmdOpenFire = 0x16;

    public const int TmccMinorCmdCloseFire = 0x17;

    ///<summary>是否上报PTZ当前位置</summary>
    public const int TmccMinorCmdReportPos = 0x18;

    ///<summary>mqtt相关信息</summary>
    public const int TmccMinorCmdMqttInfo = 0x19;

    ///<summary>跟踪业务参数配置</summary>
    public const int TmccMinorCmdTracerImplement = 0x20;

    ///<summary>校验水平角度</summary>
    public const int TmccMinorCmdCheckHorAngle = 0x21;

    ///<summary>201感知系统配置</summary>
    public const int TmccMinorCmdPerceptionCfg = 0x22;

    ///<summary>动态暂停分类检测</summary>
    public const int TmccMinorCmdPauseYolo = 0x23;

    ///<summary>开始检查雷达坐标偏移</summary>
    public const int TmccMinorCmdStartCheckOffset = 0x24;

    ///<summary>停止检查雷达坐标偏移</summary>
    public const int TmccMinorCmdStopCheckOffset = 0x25;

    ///<summary>自主扫描配置</summary>
    public const int TmccMinorCmdAutoScan = 0x26;

    ///<summary>雷达坐标偏移列表配置</summary>
    public const int TmccMinorCmdOffsetListCfg = 0x27;

    ///<summary>亿威尔相关配置</summary>
    public const int TmccMinorCmdEWareCfg = 0x28;

    ///<summary>cop15相关配置</summary>
    public const int TmccMinorCmdCop15Cfg = 0x29;

    ///<summary>FK相关配置</summary>
    public const int TmccMinorCmdFkCfg = 0x2A;

    ///<summary>其他平台相关配置</summary>
    public const int TmccMinorCmdOtherPlatformCfg = 0x2B;

    ///<summary>人脸比对配置</summary>
    public const int TmccMajorCmdFaceCompareCfg = 0x128;

    ///<summary>列举人员信息，这里不读原始人脸</summary>
    public const int TmccMinorCmdPersonNum = 0x01;

    ///<summary>人员录入</summary>
    public const int TmccMinorCmdPersonAdd = 0x02;

    ///<summary>人员删除</summary>
    public const int TmccMinorCmdPersonDelete = 0x03;

    ///<summary>人信息修改</summary>
    public const int TmccMinorCmdPersonCfg = 0x04;

    ///<summary>列举人脸</summary>
    public const int TmccMinorCmdFaceNum = 0x05;

    ///<summary>添加人脸</summary>
    public const int TmccMinorCmdFaceAdd = 0x06;

    ///<summary>删除人脸</summary>
    public const int TmccMinorCmdFaceDelete = 0x07;

    ///<summary>人脸修改</summary>
    public const int TmccMinorCmdFaceCfg = 0x08;

    ///<summary>人脸抓拍</summary>
    public const int TmccMinorCmdFaceCapture = 0x09;

    ///<summary>人脸比对，返回人脸信息，和相似度</summary>
    public const int TmccMinorCmdFaceCompare = 0x0A;

    ///<summary>人员声音提示音添加</summary>
    public const int TmccMinorCmdPersonAudioAdd = 0x0B;

    ///<summary>人员声音提示音删除</summary>
    public const int TmccMinorCmdPersonAudioDelete = 0x0C;

    ///<summary>热像探测器参数配置</summary>
    public const int TmccMajorCmdFlirSensorCfg = 0x130;

    ///<summary>单点校正</summary>
    public const int TmccMinorCmdSensorCheck = 0x01;

    ///<summary>位0: 远程控制云台</summary>
    public const int TmccUserRightControlPtz = 0x00000001;

    ///<summary>位1: 远程手动录象</summary>
    public const int TmccUserRightManualRec = 0x00000002;

    ///<summary>位2: 远程回放</summary>
    public const int TmccUserRightPlayback = 0x00000004;

    ///<summary>位3: 远程设置参数，参数保存</summary>
    public const int TmccUserRightSetup = 0x00000008;

    ///<summary>位4: 远程查看状态、日志</summary>
    public const int TmccUserRightLookStatus = 0x00000010;

    ///<summary>位5: 远程高级操作(升级，格式化，重启，关机)</summary>
    public const int TmccUserRightAdvance = 0x00000020;

    ///<summary>位6: 远程发起语音对讲</summary>
    public const int TmccUserRightTalk = 0x00000040;

    ///<summary>位7: 远程预览</summary>
    public const int TmccUserRightPreview = 0x00000080;

    ///<summary>位8: 远程请求报警上传、报警输出</summary>
    public const int TmccUserRightAlarm = 0x00000100;

    ///<summary>位9: 远程控制，本地输出</summary>
    public const int TmccUserRightControlLocal = 0x00000200;

    ///<summary>位10: 远程控制串口</summary>
    public const int TmccUserRightSerialPort = 0x00000400;

    ///<summary>位11: 远程修改用户</summary>
    public const int TmccUserRightModifyUser = 0x00000800;

    ///<summary>位12: 远程修改用户自己的密码</summary>
    public const int TmccUserRightModifySelfPassword = 0x00001000;

    ///<summary>位13: 远程RTSP链接</summary>
    public const int TmccUserRightRtspConnect = 0x00002000;

    ///<summary>停止播放</summary>
    public const int TmccPlayStateStop = 0;

    ///<summary>暂停播放</summary>
    public const int TmccPlayStatePause = 1;

    ///<summary>播放</summary>
    public const int TmccPlayStatePlay = 2;

    ///<summary>快放</summary>
    public const int TmccPlayStateFast = 3;

    ///<summary>慢放</summary>
    public const int TmccPlayStateSlow = 4;

    ///<summary>打开文件成功</summary>
    public const int TmccPlayStateOpen = 5;

    ///<summary>已经切换文件</summary>
    public const int TmccPlayStateSwitch = 6;

    ///<summary>文件全部播放完毕</summary>
    public const int TmccPlayStateFileEnd = 7;

    ///<summary>正在缓冲数据</summary>
    public const int TmccPlayStateStreamBuffering = 8;

    ///<summary>编码格式改变</summary>
    public const int TmccPlayStateEnCfMtChange = 9;

    public const int TmccImageOutFmt420P = 0;
    public const int TmccImageOutFmtRgb15 = 1;
    public const int TmccImageOutFmtRgb16 = 2;
    public const int TmccImageOutFmtRgb24 = 3;
    public const int TmccImageOutFmtRgb32 = 4;
    public const int TmccImageOutFmtYuy2 = 5;
    public const int TmccImageOutFmtYvyu = 6;
    public const int TmccImageOutFmtUyvy = 7;
    public const int TmccImageOutFmt420P2 = 8;
    public const int TmccImageOutFmt422P = 9;
    public const int TmccImageOutFmt422P2 = 10;
    public const int TmccImageOutFmtH264 = 11;
    public const int TmccImageOutFmtJpeg = 12;
    public const int TmccImageOutFmtH265 = 13;
    public const int TmccCaptureImageTypeBmp = 0x00;
    public const int TmccCaptureImageTypeJpeg = 0x01;
    public const int TmccCaptureImageTypeH264 = 0x02;
    public const int TmccCaptureImageTypeYuv = 0x03;
    public const int TmccCaptureImageTypeH265 = 0x04;
    public const int TmccCaptureImageTypeRaw = 0x05;
    public const int TmccCaptureImageTypeDef = 0xFF;
    public const int TmccCaptureImageFmtJpeg = 0x00;
    public const int TmccCaptureImageFmtBmp = 0x01;
    public const int TmccCaptureImageFmtRgb555 = 0x02;
    public const int TmccCaptureImageFmtRgb565 = 0x03;
    public const int TmccCaptureImageFmtRgb24 = 0x04;
    public const int TmccCaptureImageFmtRgb32 = 0x05;
    public const int TmccCaptureImageFmtYuv444 = 0x06;
    public const int TmccCaptureImageFmtYuv422 = 0x07;
    public const int TmccCaptureImageFmtYuv420 = 0x08;
    public const int TmccCaptureImageFmtBkMpeg4 = 0x09;
    public const int TmccCaptureImageFmtH264 = 0x0A;
    public const int TmccCaptureImageFmtH265 = 0x0B;
    public const int TmccCaptureImageFmtYUv420 = 0x0C;
    public const int TmccCaptureImageFmtYVu420 = 0x0D;
    public const int TmccCaptureImageFmtRaw = 0x0E;

    ///<summary>没有错误发生</summary>
    public const int TmccErrSuccess = 0x00000000;

    ///<summary>非法句柄</summary>
    public const uint TmccErrHandleInvalid = 0xC0000001;

    ///<summary>参数错误</summary>
    public const uint TmccErrParameterInvalid = 0xC0000002;

    ///<summary>内存益出错误</summary>
    public const uint TmccErrMemoryOut = 0xC0000003;

    ///<summary>线程生成错误</summary>
    public const uint TmccErrThreadCreate = 0xC0000004;

    ///<summary>启动线程错误</summary>
    public const uint TmccErrThreadStart = 0xC0000005;

    ///<summary>停止线程错误</summary>
    public const uint TmccErrThreadStop = 0xC0000006;

    ///<summary>套接字生成错误</summary>
    public const uint TmccErrSocketCreate = 0xC0000007;

    ///<summary>服务器没有初始化配置接口</summary>
    public const uint TmccErrServerNoConfig = 0xC0000008;

    ///<summary>服务器函数调用发生异常</summary>
    public const uint TmccErrServerException = 0xC0000009;

    ///<summary>该用户没有登录</summary>
    public const uint TmccErrUserNotLogin = 0xC000000A;

    ///<summary>不能设置</summary>
    public const uint TmccErrNotSetup = 0xC000000B;

    ///<summary>没有断开连接</summary>
    public const uint TmccErrNotDisConnect = 0xC000000C;

    ///<summary>连接服务器失败</summary>
    public const uint TmccErrNetWorFailConnect = 0xC000000D;

    ///<summary>向服务器发送失败</summary>
    public const uint TmccErrNetWorSendError = 0xC000000E;

    ///<summary>从服务器接收数据失败</summary>
    public const uint TmccErrNetWorRecvError = 0xC000000F;

    ///<summary>从服务器接收数据超时</summary>
    public const uint TmccErrNetWorRecvTimeOut = 0xC0000010;

    ///<summary>传送的数据有误</summary>
    public const uint TmccErrNetWorErrorData = 0xC0000011;

    ///<summary>服务器没有启动</summary>
    public const uint TmccErrNetWorServerNoStart = 0xC0000012;

    ///<summary>上次的操作还没有完成</summary>
    public const uint TmccErrOperaNotFinish = 0xC0000013;

    ///<summary>缓冲区太小</summary>
    public const uint TmccErrNoEnoughBuf = 0xC0000014;

    ///<summary>非法命令</summary>
    public const uint TmccErrCommandInvalid = 0xC0000015;

    ///<summary>升级文件不能打开</summary>
    public const uint TmccErrUpgradeFileNotOpen = 0xC0000016;

    ///<summary>升级文件大小错误不能升级</summary>
    public const uint TmccErrUpgradeSizeLarge = 0xC0000017;

    ///<summary>升级文件效验错误不能升级</summary>
    public const uint TmccErrUpgradeParity = 0xC0000018;

    ///<summary>升级文件不能读取</summary>
    public const uint TmccErrUpgradeFileNotRead = 0xC0000019;

    ///<summary>结构中的大小错误</summary>
    public const uint TmccErrStructSize = 0xC000001A;

    ///<summary>数据CRC效验错误</summary>
    public const uint TmccErrDataCrc = 0xC000001B;

    ///<summary>用户已经存在</summary>
    public const uint TmccErrUserExist = 0xC000001D;

    ///<summary>用户达到最大在</summary>
    public const uint TmccErrUserMaxNum = 0xC000001E;

    ///<summary>没有初始化</summary>
    public const uint TmccErrNoInit = 0xC000001F;

    ///<summary>通道号错误</summary>
    public const uint TmccErrChannelError = 0xC0000020;

    ///<summary>连接到DVR的客户端个数超过最大</summary>
    public const uint TmccErrOverMaxLink = 0xC0000021;

    ///<summary>版本不匹配</summary>
    public const uint TmccErrVersionNoMatch = 0xC0000022;

    ///<summary>调用次序错误</summary>
    public const uint TmccErrOrderError = 0xC0000023;

    ///<summary>无此权限</summary>
    public const uint TmccErrOperaNoPermit = 0xC0000024;

    ///<summary>DVR命令执行超时</summary>
    public const uint TmccErrCommandTimeOut = 0xC0000025;

    ///<summary>串口号错误</summary>
    public const uint TmccErrErrorSerialPort = 0xC0000026;

    ///<summary>报警端口错误</summary>
    public const uint TmccErrErrorAlarmPort = 0xC0000027;

    ///<summary>参数错误</summary>
    public const uint TmccErrParameterError = 0xC0000028;

    ///<summary>服务器通道处于错误状态</summary>
    public const uint TmccErrChanException = 0xC0000029;

    ///<summary>没有硬盘</summary>
    public const uint TmccErrNoDisk = 0xC000002A;

    ///<summary>硬盘号错误</summary>
    public const uint TmccErrErrorDiskNum = 0xC000002B;

    ///<summary>服务器硬盘满</summary>
    public const uint TmccErrDiskFull = 0xC000002C;

    ///<summary>服务器硬盘出错</summary>
    public const uint TmccErrDiskError = 0xC000002D;

    ///<summary>服务器不支持</summary>
    public const uint TmccErrNoSupport = 0xC000002E;

    ///<summary>服务器忙</summary>
    public const uint TmccErrBusy = 0xC000002F;

    ///<summary>服务器修改不成功</summary>
    public const uint TmccErrModifyFail = 0xC0000030;

    ///<summary>密码输入格式不正确</summary>
    public const uint TmccErrPasswordFormatError = 0xC0000031;

    ///<summary>硬盘正在格式化，不能启动操作</summary>
    public const uint TmccErrDiskFormatting = 0xC0000032;

    ///<summary>DVR资源不足</summary>
    public const uint TmccErrDvrNoResource = 0xC0000033;

    ///<summary>DVR操作失败</summary>
    public const uint TmccErrDvrOpRateFailed = 0xC0000034;

    ///<summary>打开PC声音失败</summary>
    public const uint TmccErrOpenHostSoundFail = 0xC0000035;

    ///<summary>服务器语音对讲被占用</summary>
    public const uint TmccErrDvrVoiceOpened = 0xC0000036;

    ///<summary>时间输入不正确</summary>
    public const uint TmccErrTimeInputError = 0xC0000037;

    ///<summary>回放时服务器没有指定的文件</summary>
    public const uint TmccErrNoSpecFile = 0xC0000038;

    ///<summary>创建文件出错</summary>
    public const uint TmccErrCreateFileError = 0xC0000039;

    ///<summary>打开文件出错</summary>
    public const uint TmccErrFileOpenFail = 0xC000003A;

    ///<summary>获取当前播放的时间出错</summary>
    public const uint TmccErrGetPlayTimeFail = 0xC000003B;

    ///<summary>播放出错</summary>
    public const uint TmccErrPlayFail = 0xC000003C;

    ///<summary>文件格式不正确</summary>
    public const uint TmccErrFileFormatError = 0xC000003D;

    ///<summary>路径错误</summary>
    public const uint TmccErrDirError = 0xC000003E;

    ///<summary>资源分配错误</summary>
    public const uint TmccErrAllocResourceError = 0xC000003F;

    ///<summary>声卡模式错误</summary>
    public const uint TmccErrAudioModeError = 0xC0000040;

    ///<summary>创建SOCKET出错</summary>
    public const uint TmccErrCreateSocketError = 0xC0000042;

    ///<summary>设置SOCKET出错</summary>
    public const uint TmccErrSetSocketError = 0xC0000043;

    ///<summary>个数达到最大</summary>
    public const uint TmccErrMaxNum = 0xC0000044;

    ///<summary>用户不存在</summary>
    public const uint TmccErrUserNotExist = 0xC0000045;

    ///<summary>写FLASH出错</summary>
    public const uint TmccErrWriteFlashError = 0xC0000046;

    ///<summary>DVR升级失败</summary>
    public const uint TmccErrUpgradeFail = 0xC0000047;

    ///<summary>解码卡已经初始化过</summary>
    public const uint TmccErrCardHaveInit = 0xC0000048;

    ///<summary>播放器中错误</summary>
    public const uint TmccErrPlayerFailed = 0xC0000049;

    ///<summary>用户数达到最大</summary>
    public const uint TmccErrMaxUserNum = 0xC000004A;

    ///<summary>获得客户端的IP地址或物理地址失败</summary>
    public const uint TmccErrGetLocalIpAndMacFail = 0xC000004B;

    ///<summary>该通道没有编码</summary>
    public const uint TmccErrNoEncoding = 0xC000004C;

    ///<summary>IP地址不匹配</summary>
    public const uint TmccErrIpMisMatch = 0xC000004D;

    ///<summary>MAC地址不匹配</summary>
    public const uint TmccErrMacMisMatch = 0xC000004E;

    ///<summary>升级文件语言不匹配</summary>
    public const uint TmccErrUpgradeLangMisMatch = 0xC000004F;

    ///<summary>用户密码错误</summary>
    public const uint TmccErrPasswordVerify = 0xC0000050;

    ///<summary>用户已经登录</summary>
    public const uint TmccErrUserLogin = 0xC0000051;

    ///<summary>用户不能删除</summary>
    public const uint TmccErrCanNotDeleteUser = 0xC0000052;

    ///<summary>该版本的此模块不存在</summary>
    public const uint TmccErrModalNotExist = 0xC0000053;

    ///<summary>服务器正在启动中</summary>
    public const uint TmccErrServerStarting = 0xC0000054;

    ///<summary>打开控制云台的通道错误</summary>
    public const uint TmccErrChannelPtzOpen = 0xC0000055;

    ///<summary>云台控制没有打开</summary>
    public const uint TmccErrPtzControlNotOpen = 0xC0000056;

    ///<summary>不能得到云台控制信息</summary>
    public const uint TmccErrGetPtzControl = 0xC0000057;

    ///<summary>透明传输数据大小错误</summary>
    public const uint TmccErrSerialDataSize = 0xC0000058;

    ///<summary>云台已经被其它用户打开</summary>
    public const uint TmccErrPtzOpened = 0xC0000059;

    ///<summary>云台已经被其它用户锁定</summary>
    public const uint TmccErrPtzLocked = 0xC000005A;

    ///<summary>无语音对讲功能</summary>
    public const uint TmccErrNoVoiceFunction = 0xC000005B;

    ///<summary>语音对讲已经被占用</summary>
    public const uint TmccErrVoiceHaveConnect = 0xC000005C;

    ///<summary>语音对讲打开失败</summary>
    public const uint TmccErrVoiceNotOpen = 0xC000005D;

    ///<summary>处于升级模式不能打开</summary>
    public const uint TmccErrInUpgrade = 0xC000005E;

    ///<summary>网络报警通道错误</summary>
    public const uint TmccErrAlarmInChannel = 0xC000005F;

    ///<summary>通明通道已经打开</summary>
    public const uint TmccErrSerialHaveOpen = 0xC0000060;

    ///<summary>通明通道没有打开</summary>
    public const uint TmccErrSerialNotOpen = 0xC0000061;

    ///<summary>没有打开升级模式</summary>
    public const uint TmccErrNotOpenUpgradeModal = 0xC0000062;

    ///<summary>系统不支持的升级模块</summary>
    public const uint TmccErrUpgradeModalNoSupport = 0xC0000063;

    ///<summary>打开写模块错误</summary>
    public const uint TmccErrOpenUpgradeModalWrite = 0xC0000064;

    ///<summary>模块不能读错误</summary>
    public const uint TmccErrUpgradeModalNotRead = 0xC0000065;

    ///<summary>HTTP监听端口不能为6000</summary>
    public const uint TmccErrHttpPortIs6000 = 0xC0000066;

    ///<summary>设置的视频制式不支持</summary>
    public const uint TmccErrVideoStandard = 0xC0000067;

    ///<summary>当天日志不能删除</summary>
    public const uint TmccErrDeleteTodayLog = 0xC0000068;

    ///<summary>服务器中没有搜索的数据了</summary>
    public const uint TmccErrNoEnoughData = 0xC0000069;

    ///<summary>写文件错误</summary>
    public const uint TmccErrWriteFile = 0xC000006A;

    ///<summary>读文件错误</summary>
    public const uint TmccErrReadFile = 0xC000006B;

    ///<summary>读文件完毕</summary>
    public const uint TmccErrFileEnd = 0xC000006C;

    ///<summary>读文件索引错误</summary>
    public const uint TmccErrFileIndex = 0xC000006D;

    ///<summary>手动抓图失败</summary>
    public const uint TmccErrCaptureImage = 0xC000006E;

    ///<summary>内部缓冲满 请再次送入 </summary>
    public const uint TmccErrBufferFull = 0xC0000070;

    public const uint ErrorCodeBeginConnect = 0xC0001000;
    public const uint ErrorCodeConnectSuccess = 0xC0001001;
    public const uint ErrorCodeNetWor = 0xC0001002;
    public const uint ErrorCodeConnectError = 0xC0001003;
    public const uint ErrorCodeConnectError1000 = 0xC0001004;
    public const uint ErrorCodeServerStop = 0xC0001005;
    public const uint ErrorCodeServerStop1000 = 0xC0001006;
    public const uint ErrorCodeTimeOut = 0xC0001007;
    public const uint ErrorCodeTimeOut1000 = 0xC0001008;
    public const uint ErrorCodeSendData = 0xC0001009;
    public const uint ErrorCodeSendData1000 = 0xC000100A;
    public const uint ErrorCodeRecvData = 0xC000100B;
    public const uint ErrorCodeRecvData1000 = 0xC000100C;
    public const uint ErrorCodeCloseConnect = 0xC0010000;
    public const uint ErrorCodeServerNoStart = 0xC0010001;
    public const uint ErrorCodeServerError = 0xC0010002;
    public const uint ErrorCodeChannelLimit = 0xC0010003;
    public const uint ErrorCodeServerLimit = 0xC0010004;
    public const uint ErrorCodeServerRefuse = 0xC0010005;
    public const uint ErrorCodeIpLimit = 0xC0010006;
    public const uint ErrorCodePortLimit = 0xC0010007;
    public const uint ErrorCodeTypeError = 0xC0010008;
    public const uint ErrorCodeUserError = 0xC0010009;
    public const uint ErrorCodePasswordError = 0xC001000A;
    public const uint ErrorCodeDoNtTurn = 0xC001000B;
    public const uint ErrorCodeVersion = 0xC0100000;
    public const uint ErrorCodeFactory = 0xC0100001;
    public const uint ErrorCodeTransPacketSize = 0xC0100002;
    public const uint ErrorCodeDecodeVideoError = 0xC0100003;
}