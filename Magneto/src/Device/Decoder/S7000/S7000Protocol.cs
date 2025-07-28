using System;
using System.Collections.Generic;
using System.Text;

namespace Magneto.Device;

/// <summary>
///     S7000指令协议
///     命令格式： xxxx \r\n
///     命令名称与参数之间，命令参数之间 空格隔开。
/// </summary>
internal static class S7000Protocol
{
    /// <summary>
    ///     S7000 端口
    /// </summary>
    public static readonly int Port = 1234;

    /// <summary>
    ///     获取稳定的信号结果的包数
    /// </summary>
    public static readonly int StableNum = 4;

    #region 通用协议

    /// <summary>
    ///     退出通信状态 无应答无参数
    ///     参数:无
    ///     应答	:无
    /// </summary>
    public static readonly string Reset = "*RST";

    /// <summary>
    ///     锁定键盘
    ///     参数1：锁定标志（ON 、OFF）
    ///     参数2：	是否需要应答（0,1）
    ///     应答：	根据参数2设定。
    ///     SYSTem:KLOCk:KBD ON 锁定键盘
    ///     SYSTem:KLOCk:KBD OFF 1 解锁键盘，需要应答
    ///     确认返回成功 SYSTem:KLOCk:KBD OFF
    /// </summary>
    public static readonly string LockKeyBoard = "SYSTem:KLOCk:KBD";

    ///// <summary>
    /////     读取仪器序列号
    /////     参数:无
    /////     应答	:返回仪器序列号（6位）
    ///// </summary>
    //public static string GetSerialNum = "*SYSTem:SerialNum:Get?";

    ///// <summary>
    /////     读取系统时间
    /////     参数:无
    /////     应答	:系统时间，格式如下： 年 月 日 时 分 秒
    /////     SYSTem:Time:Get?
    /////     2012年12月5日13:32:45
    ///// </summary>
    //public static string GetTime = "SYSTem:Time:Get?";

    ///// <summary>
    /////     设置系统时间
    /////     参数:系统时间，格式如下： 年 月 日 时 分 秒
    /////     应答	:无
    /////     SYSTem:Time:Set 2012 12 5 13 32 45
    ///// </summary>
    //public static string SetTime = "SYSTem:Time:Set";

    ///// <summary>
    /////     读取硬件版本
    /////     参数:无
    /////     应答	:格式如下： 120140 表示主板版本1.20 通道板版本1.40
    /////     SYSTem:HWVersion:Get?
    /////     SYSTem:Time:Set 2012 12 5 13 32 45
    ///// </summary>
    //public static string GetHwVersion = "SYSTem:HWVersion:Get?";

    ///// <summary>
    /////     关机
    /////     参数:无
    /////     应答	:无
    ///// </summary>
    //public static string Shutdown = "SYSTem:SHUTDOWN";

    /// <summary>
    ///     重启
    ///     参数:无
    ///     应答	:无
    /// </summary>
    public static readonly string Reboot = "SYSTem:REBOOT";

    ///// <summary>
    /////     读取IP
    /////     参数:无
    /////     应答	:无
    ///// </summary>
    //public static string GetIp = "Get:NET:IP?";

    ///// <summary>
    /////     设置仪器IP
    /////     参数:IP,IP2 IP3 IP4
    /////     应答	:无
    /////     Set:NET:IP 192 168 63 1
    ///// </summary>
    //public static string SetIp = "Set:NET:IP";

    #endregion

    #region 地面系统测试协议

    ///// <summary>
    /////     切换系统
    /////     参数:0 表示地面系统
    /////     应答	:switch system 0 status=0x80007fff ok!
    ///// </summary>
    //public static string SetSwitchSystem = "SYSTem:SwitchSystem";

    /// <summary>
    ///     设置频率
    ///     参数:频率（单位Hz）
    ///     应答	:"Set Freq ok!" or "out of range!"
    ///     Set:FreqHz 500000000
    /// </summary>
    public static readonly string SetFreq = "Set:FreqHz";

    /// <summary>
    ///     设置带宽
    ///     参数:频率（单位Hz）
    ///     应答	:"Set Bw ok!" or "out of range!"  or "wrong signaltype!"
    ///     Set: BwHz 8000000
    /// </summary>
    public static readonly string SetBw = "Set:BwHz";

    /// <summary>
    ///     设置信号类型
    ///     参数:0---ANAFM,1---ANATV,2---DVB-C,3---DVB-T,4---DVB-T2,5---DTMB
    ///     应答	:"Set SignalType ok!" or "error"
    ///     Set:SignalType 2
    /// </summary>
    public static readonly string SetSignalType = "Set:SignalType";

    ///// <summary>
    /////     终止测量
    /////     参数:无
    /////     应答	:stop test!
    ///// </summary>
    //public static string Stop = "*STOP";

    /// <summary>
    ///     读取当前频率模拟电平
    ///     参数:无
    ///     应答	:电平（dBuV）当前频率（Hz）
    ///     Set:FreqHz 500000000
    ///     Get:Level?
    ///     80.5dBuV 500000000Hz
    /// </summary>
    public static readonly string GetLevel = "Get:Level?";

    ///// <summary>
    /////     读取当前频率功率电平，需要设置带宽
    /////     参数:无
    /////     应答	:电平（dBuV）当前频率（Hz）
    /////     Set:FreqHz 500000000
    /////     Set:BwHz 8000000
    /////     Get:Power?
    /////     电平=80.6dBuV，频率=500MHz
    /////     Get:Power?
    /////     电平=80.6dBuV，频率=500MHz
    ///// </summary>
    //public static string GetPower = "Get:Power?";

    #endregion

    #region DTMB频道测试

    /// <summary>
    ///     初始化DTMB模块，启动DTMB测试，设置相关参数
    ///     参数:参数1-----CONS      0----无星座图数据 1----250个点星座图数据
    ///     参数2-----频道参数   0----不获取频道参数 1----获取频道参数
    ///     应答	:Set:Test Ok!
    ///     Set:FreqHz 500000000
    ///     Set:BwHz 8000000
    ///     Get:Power?
    ///     电平=80.6dBuV，频率=500MHz
    ///     Get:Power?
    ///     电平=80.6dBuV，频率=500MHz
    /// </summary>
    public static readonly string SetDtmbTest = "Set:DTMB:Test";

    /// <summary>
    ///     无需设置参数，直接读取DTMB测试结果
    ///     参数:参数1-----CONS      0----无星座图数据 1----250个点星座图数据
    ///     参数2-----频道参数   0----不获取频道参数 1----获取频道参数
    ///     应答	:格式如下：
    ///     序号	参数名称
    ///     1	锁定标志 1-lock，0-unlock
    ///     2	功率（dBuV）
    ///     3	MER（dB）
    ///     4	误码数
    ///     5	码总数
    ///     6	频道参数有效标志：
    ///     0-无效
    ///     1-表示以下频道参数[6~10]有效
    ///     7	CARR Mode(0-多载波，1-单载波)
    ///     8	调制类型：锁定有效
    ///     4QAM=0
    ///     16QAM=1
    ///     32QAM=2
    ///     64QAM=3
    ///     4QAM_NR=4
    ///     9	 交织深度：锁定有效
    ///     TD_240N=0,
    ///     TD_240I=1
    ///     TD_720I=2
    ///     10	编码码率：锁定有效
    ///     CR_0.4=0
    ///     CR_0.6=1
    ///     CR_0.8=2
    ///     11	保护间隔：锁定有效
    ///     GI_420=0
    ///     GI_595=1
    ///     GI_945=2
    ///     12~N	星座图数据：参数1没有参数或参数为0时，没有下面的数据
    ///     星座图坐标X0
    ///     星座图坐标Y0
    ///     …
    ///     星座图坐标X249
    ///     星座图坐标Y249
    ///     Set:SignalType 5 频道类型DTMB
    ///     Set:FreqHz 500000000 设置频率500MHz
    ///     Set:BwHz 8000000 设置带宽8MHz
    ///     Set:DTMB:Test 1 1 初始化DTMB模块，启动测试 Set:Test ok!
    ///     Get:DTMB:Result? 读取指标
    ///     电平=80.5dBuV，MER=31.0dB等 80.5dBuV 31.0dB 0 10000…
    ///     Get:DTMB:Result? 读取指标
    ///     电平=80.5dBuV，MER=30.5dB等
    ///     1 78.3dBuV 41.2dB 0 100000 1 0 2 1 2 0 -70 11 67 43 42 -44 72 14 72 17 69 18 71 14 -72 -15 -14 -17 16 -45 -10 -45
    ///     43 -43 -15 -16 40 -14 44 -13 -43 13 12 12 72 12 14 -12 -16 41 -42 -45 -16 11 -43 -45 -44 -17 15 -42 -15 -42 -14 -43
    ///     -43 -17 14 44 40 73 43 -71 14 -15 -42 -44 -72 -16 -14 46 44 -71 41 72 -16 44 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
    ///     0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
    ///     0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
    ///     0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
    ///     0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
    ///     0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
    ///     0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
    ///     0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
    ///     解析如下：
    ///     1 ---Lock
    ///     78.3dBuv ---功率
    ///     41.2dB ---MER
    ///     0 ---错误码统计
    ///     100000 ---总码数
    ///     1 --- 频道参数有效
    ///     0 --- 多载波
    ///     2 --- 32QAM
    ///     1 --- 240I
    ///     2 --- CR0.8
    ///     0 --- GI420
    ///     -70 --- Point[1].x
    ///     -11 --- Point[1].y............
    ///     注意：由于解调硬件特性问题，可能读取到全零星座点，显示时，可以进行过滤。
    ///     …	…
    ///     *STOP 终止测量确认终止 stop test!
    /// </summary>
    public static readonly string GetDtmbResult = "Get:DTMB:Result?";

    #endregion

    #region DVBC频道测试

    ///// <summary>
    /////     初始化DTMB模块，启动DTMB测试，设置相关参数
    /////     参数:参数1-----STD  （0-J83.A，1-J83.B，2-J83.C 3-J83.D）
    /////     参数2-----QAM    2-16QAM 3-32QAM   4-64QAM 5-128QAM  6-256QAM   7-8VSB  8-16VSB
    /////     参数3---- SR      （4000KS/S-7000KS/s）
    /////     应答	:Set:Test Ok!
    /////     Set:FreqHz 500000000
    /////     Set:BwHz 8000000
    /////     Get:Power?
    /////     电平=80.6dBuV，频率=500MHz
    /////     Get:Power?
    /////     电平=80.6dBuV，频率=500MHz
    ///// </summary>
    //public static string SetDvbcTest = "Set:DVBC:Test";

    ///// <summary>
    /////     无需设置参数，直接读取DTMB测试结果
    /////     参数:参数1-----CONS      0----无星座图数据 1----250个点星座图数据
    /////     参数2-----频道参数   0----不获取频道参数 1----获取频道参数
    /////     应答	:格式如下：
    /////     序号	参数名称
    /////     LOCK标志（1 Lock，0-Unlock）
    /////     电平（dBuV）
    /////     MER（dB）
    /////     纠错前误码数
    /////     纠错前码总数
    /////     纠错后误码数
    /////     纠错后码总数
    /////     Set:SignalType 2 频道类型DVBC
    /////     Set:FreqHz 500000000 设置频率500MHz
    /////     Set:BwHz 8000000 设置带宽8MHz
    /////     Set:DVBC:Test 0 4 6875 J83.A,64QAM,6.875MS/s Set:Test Ok!
    /////     Get:DVBC:Result?
    /////     读取指标
    /////     电平=80.5dBuV，MER=31.0dB等
    /////     80.5dBuV 31.0dB 0 10000…
    /////     Get:DVBC:Result?
    /////     返回值==80.5dBuV 30.5dB 5 100000000 0 100000000
    /////     纠错前误码数=5；
    /////     纠错前码总数=100000000；
    /////     纠错后误码数=0；
    /////     纠错后码总数=100000000；
    /////     preBER=纠错前误码数/纠错前码总数
    /////     postBER=纠错后误码数/纠错后码总数
    /////     *STOP 终止测量确认终止 stop test!
    ///// </summary>
    //public static string GetDvbcResult = "Get:DVBC:Result?";

    #endregion

    #region DVBT频道测试

    /// <summary>
    ///     作用	初始化DVBT模块，启动DVBT测试，设置相关参数
    ///     参数 参数1-----CONS
    ///     0----无星座图数据
    ///     1----250个点星座图数据
    ///     参数2-----频道参数
    ///     0----不获取频道参数
    ///     1----获取频道参数
    /// </summary>
    public static readonly string SetDvbtTest = "Set:DVBT:Test";

    /// <summary>
    ///     无需设置参数，直接读取DVBT测试结果
    /// </summary>
    public static readonly string GetDvbtResult = "Get:DVBT:Result?";

    #endregion

    #region DVBT2频道测试

    /// <summary>
    ///     作用:初始化DVBT2模块，启动DVBT2测试，设置相关参数，参数格式如下：
    ///     参数1-----PLPID  （0-255）
    ///     参数2-----CONS   0----无星座图数据，1----250个点星座图数据
    ///     参数3-----频道参数   0----不获取频道参数，1----获取频道参数
    ///     应答  格式如下：Set:Test Ok!
    /// </summary>
    public static readonly string SetDvbt2Test = "Set:DVBT2:Test";

    /// <summary>
    ///     无需设置参数，直接读取DVBT2测试结果
    /// </summary>
    public static readonly string GetDvbt2Result = "Get:DVBT2:Result?";

    #endregion

    #region DTV载噪比测试

    ///// <summary>
    /////     初始化CNR模块，启动DTVCNR测试，设置相关参数
    /////     参数:无
    /////     应答	:Set:Test Ok!
    ///// </summary>
    //public static string SetDtvcnrTest = "Set:DTV:CNRTest";

    ///// <summary>
    /////     无需设置参数，直接读取CNR测试结果
    /////     参数:无
    /////     应答	:CNR（dB）
    /////     Set:SignalType 4 频道类型DVBT2
    /////     Set:FreqHz 500000000 设置频率500MHz
    /////     Set:DTV:CNRTest 575 设置噪声带宽5.75MHz
    /////     Set:Test Ok!
    /////     Get:DTV:CNR? 读取指标 CNR=45.5dB
    ///// </summary>
    //public static string GetDtvcnr = "Get:DTV:CNR?";

    #endregion

    #region CATV载噪比测试

    /// <summary>
    ///     初始化CNR模块，启动CNR测试，设置相关参数
    ///     参数:NoiseBW(10KHz)  1MHz-8MHz
    ///     常用设置 575---5.75MHz对应8M带宽信号
    ///     常用设置 475---4.75MHz对应7M带宽信号
    ///     常用设置 400---4.00MHz对应6M带宽信号
    ///     应答	:Set:Test Ok!
    /// </summary>
    public static readonly string SetCatvcnrTest = "Set:CATV:CNRTest";

    /// <summary>
    ///     无需设置参数，直接读取CNR测试结果
    ///     参数:无
    ///     应答	:CNR（dB）
    ///     Set:SignalType 1 频道类型ANA TV
    ///     Set:FreqHz 500000000 设置频率500MHz
    ///     Set:CATV:CNRTest 575 设置噪声带宽5.75MHz
    ///     Set:Test Ok!
    ///     Get:CATV:CNR? 读取指标 CNR=45.5dB
    /// </summary>
    public static readonly string GetCatvcnr = "Get:CATV:CNR?";

    #endregion

    #region CATVHUM测试 哼调测试

    /// <summary>
    ///     初始化HUM 模块，启动HUM测试，设置相关参数
    ///     参数:Type (0-50Hz,1-60Hz)
    ///     应答	:Set:Test Ok!
    /// </summary>
    public static readonly string SetCatvhumTest = "Set:CATV:HUMTest";

    /// <summary>
    ///     无需设置参数，直接读取HUM测试结果
    ///     参数:无
    ///     应答	:格式如下：
    ///     HUM_LPF,
    ///     HUM_50Hz,
    ///     HUM_100Hz,
    ///     HUM_150Hz,
    ///     HUM_200Hz,
    ///     Set:SignalType 1 频道类型ANA TV
    ///     Set:FreqHz 500000000 设置频率500MHz
    ///     Set:CATV:HUMTest 0 启动系统HUM侧四模块
    ///     Set:Test Ok!
    ///     Get:CATV:HUM? 读取指标 HUM=2.3%
    /// </summary>
    public static readonly string GetCatvhum = "Get:CATV:HUM?";

    #endregion

    #region CATV Modulate测试

    /// <summary>
    ///     初始化Mod 模块，启动Mod测试，设置相关参数
    ///     参数:无
    ///     应答	:Set:Test Ok!
    /// </summary>
    public static readonly string SetCatvmodUlateTest = "Set:CATV:MODUlateTest";

    /// <summary>
    ///     无需设置参数，直接读取mod测试结果
    ///     参数:无
    ///     应答	:格式如下：调制度
    ///     Set:SignalType 1 频道类型ANA TV
    ///     Set:FreqHz 500000000 设置频率500MHz
    ///     Set:CATV:MODUlateTest  启动调制度测试模块
    ///     Set:Test Ok!
    ///     Get:CATV:MODUlate? 读取指标 Mod=99%
    /// </summary>
    public static readonly string GetCatvmodUlate = "Get:CATV:MODUlate?";

    #endregion

    #region 码流分析协议

    /// <summary>
    ///     开启码流分析
    ///     参数:参数1：  输入选择，0---ASI输入，1---射频输入
    ///     注：如果是射频输入，首先进行解调，保证信号锁定
    ///     参数2：  标准选择，0---ATSC，   1---DVB
    ///     应答	:开启码流分析有几种情况应答，如下：
    ///     1、没有码流输入时应答： "no ts input, can't start parse thread"
    ///     2、有码流输入但没有找到同步字节： "no sync bytes, can't start parse thread"
    ///     3、或许系统某种原因无法开启分析线程： "start parse thread failed"
    ///     4、如果参数1为0，本机没有开启ASI选件 “ASI function is disable.”
    ///     5、码流分析已经开启未关闭时 TS analyzer has been already started.”
    ///     6、码流分析正常开启： "start parse thread ok"
    /// </summary>
    public static readonly string SetTsStart = "Set:TS:Start";

    /// <summary>
    ///     关闭码流分析
    ///     参数:无
    ///     应答	:无
    /// </summary>
    public static readonly string SetTsStop = "Set:TS:Stop";

    ///// <summary>
    /////     获取码流状态
    /////     参数:无
    /////     应答	:ts_status=0 没有码流  ts_status=1 有码流
    ///// </summary>
    //public static string GetTsStatus = "Get:TS:Status?";

    ///// <summary>
    /////     获取节目个数
    /////     参数:无
    /////     应答	:“PMT Quant=11，PMT Real Quant=10”
    /////     其中PMT Quant是指PAT表中指出的PMT的个数，
    /////     PMT Real Quant是PMT实际解析到的节目个数，有时PMT Real Quant和PMT Quant可能不一样，但大多数码流编码时是一样的
    ///// </summary>
    //public static string GetTsProgSum = "Get:TS:ProgSum?";

    /// <summary>
    ///     获取节目列表信息
    ///     参数:无
    ///     应答	:“[No.]=0,[PNum]=100,[CA]=UnEncryped,[SerName]=cctv-1,[ProName]=CTV,[SerType]=DIG
    ///     TV,[Resolution]=704*480,[No.]=1,[PNum]=101,[CA]=UnEncryped,[SerName]=cctv-2,[ProName]=CTV,[SerType]=DIG
    ///     TV,[Resolution]=704*480”
    ///     说明： 这个命令是一次性返回所有实际存在的PMT的节目列表信息，每一个PMT节目对应有有以下标签，每个标签中用逗号隔开，每个标签用”[***]=”标示，具体如下
    ///     [No.]=0,[PNum]=100,[CA]=UnEncryped,[SerName]=cctv-1,[ProName]=CTV,[SerType]=DIG TV,[Resolution]=704*480
    /// </summary>
    public static readonly string GetTsProgList = "Get:TS:ProgList?";

    ///// <summary>
    /////     获取节目音视频信息
    /////     参数:无
    /////     应答
    /////     :[No.]=0,[VPid]=1001,[VType]=MPEG-2,[Resolution]=704*480,[Bitrate]=15000000,[Profile]=main,[Level]=main,[Aspect]=4:3,[Chroma]=4:2:0,[APid1]=1002,[AType1]=MPEG-2,[Mode]=stereo,[Layer]=II,[Bitrate]=128
    /////     Kbps,[Sampling]=48
    /////     KHz,[CHConfig]=---,[Speaker]=---,[APid2]=1003,[AType2]=MPEG-2,[Mode]=stereo,[Layer]=II,[Bitrate]=128
    /////     Kbps,[Sampling]=48
    /////     KHz,[CHConfig]=---,[Speaker]=---,[No.]1,[VPid]=1011,[VType]=MPEG-2,[Resolution]=704*480,[Bitrate]=15000000,[Profile]=main,[Level]=main,[Aspect]=4:3,[Chroma]=4:2:0,[APid1]=1012,[AType1]=MPEG-2,[Mode]=stereo,[Layer]=II,[Bitrate]=128
    /////     Kbps,[Sampling]=48 KHz,[CHConfig]=---,[Speaker]=---,
    /////     说明： 该命令会一次性返回所有节目的音视频编码信息，先视频信息，然后是音频信息，如果一个节目有多个音频时，最多返回2个音频信息
    /////     视频信息包括：
    /////     [VPid]=1001         视频PID
    /////     [VType]=MPEG-2       视频类型
    /////     [Resolution]=704*480 视频分辨率，水平像素数*垂直像素数
    /////     [Bitrate]=15000000  比特率（帧/秒）
    /////     [Profile]=main      类
    /////     [Level]=main        等级
    /////     [Aspect]=4:3       宽高比
    /////     [Chroma]=4:2:0     色度格式
    /////     音频信息：
    /////     根据音频类型不同，音频信息也不同，具体如下：
    /////     音频类型是AAC时：
    /////     [APid1]=1002,                       音频PID
    /////     [AType1]=MPEG-2,                    音频类型
    /////     [Mode]=stereo,                      音频模式
    /////     [Bitrate]=128 Kbps,                 比特率
    /////     [Sampling]=48 KHz,                  采样率
    /////     [CHConfig]=---,                     通道个数
    /////     [Speaker]=---,                      扬声器
    /////     音频类型是AC3时：
    /////     [APid1]=1002,                       音频PID
    /////     [AType1]=MPEG-2,                    音频类型
    /////     [Mode]=stereo,                      音频模式
    /////     [Layer]=II,                         压缩层
    /////     [Bitrate]=128 Kbps,                 比特率
    /////     [Sampling]=48 KHz,                 采样率
    /////     [Profile]=---,                    类
    /////     [CHConfig]=---,                  通道个数
    /////     音频类型是DRA时：
    /////     [APid1]=1002,                       音频PID
    /////     [AType1]=MPEG-2,                    音频类型
    /////     [Sampling]=48 KHz,                  采样率
    /////     其他：
    /////     [APid1]=1002,                       音频PID
    /////     [AType1]=MPEG-2,                    音频类型
    /////     [Mode]=stereo,                      音频模式
    /////     [Layer]=II,                      压缩层
    /////     [Bitrate]=128 Kbps,              比特率
    /////     [Sampling]=48 KHz,               采样率
    /////     [CHConfig]=---,                  通道个数
    /////     [Speaker]=---,                   扬声器
    ///// </summary>
    //public static string GetTsProgInfo = "Get:TS:ProgInfo?";

    ///// <summary>
    /////     获取基本信息
    /////     参数:无
    /////     应答	:[Max]=24.1566 Mb/s,[Min]=9.9517 Mb/s,[Avg]=9.9534 Mb/s,[Current]=9.9529 Mb/s,[Video]=89.8 %,[Audio]=5.5
    /////     %,[Psisi]=2.1 %,[Empty]=1.6 %,[Other]=1.0
    /////     %,[TsLen]=188,[TsId]=24,[NetWorkId]=13092,[ProQuant]=4,[PidQuant]=20,[NetName]=CTVstar,
    /////     说明：该命令返回TS的基本信息
    /////     [Max]=24.1566 Mb/s,           最大速率
    /////     [Min]=9.9517 Mb/s,            最小速率
    /////     [Avg]=9.9534 Mb/s,            平均速率
    /////     [Current]=9.9529 Mb/s,        当前速率
    /////     [Video]=89.8 %,               视频白分比
    /////     [Audio]=5.5 %,                音频白分比
    /////     [Psisi]=2.1 %,                PSI白分比
    /////     [Empty]=1.6 %,               空包白分比
    /////     [Other]=1.0 %,              其他信息白分比
    /////     [TsLen]=188,                  TS包长度
    /////     [TsId]=24,                     TS ID
    /////     [NetWorkId]=13092,            网络ID
    /////     [ProQuant]=4,                节目个数
    /////     [PidQuant]=20,               PID个数
    /////     [NetName]=CTVstar,          网络名称
    ///// </summary>
    //public static string GetTsBasicInfo = "Get:TS:BasicInfo?";

    /// <summary>
    ///     开启视频解码
    ///     参数:参数1：PmtIndex Pmt序号
    ///     应答	:节目数为0时： "No Program"
    ///     设置的参数1数值大于等于节目总数时： "The selected program is greater than total programs"
    ///     解码设置成功时："Set program OK"
    /// </summary>
    public static readonly string SetTsOpenDecoder = "Set:TS:OpenDecoder";

    /// <summary>
    ///     关闭解码
    ///     参数:无
    ///     应答	:无
    /// </summary>
    public static readonly string SetTsCloseDecoder = "Set:TS:CloseDecoder";

    ///// <summary>
    /////     打开声音
    /////     参数:无
    /////     应答	:无
    ///// </summary>
    //public static string SetTsVolumeOn = "Set:TS:VolumeOn";

    ///// <summary>
    /////     关闭声音
    /////     参数:无
    /////     应答	:无
    ///// </summary>
    //public static string SetTsVolumeOff = "Set:TS:VolumeOff";

    /// <summary>
    ///     设置音量
    ///     参数:参数1：音量，范围是0~30  ，值越大声音越大
    ///     应答	:无
    /// </summary>
    public static readonly string SetTsVolumeVal = "Set:TS:VolumeVal";

    #endregion

    #region 模拟音视频解码

    ///// <summary>
    /////     开启模拟音视频解码
    /////     参数1：图像载波频率 （单位：hz）
    /////     参数2：音频载波频率 （单位：hz）
    /////     参数3：电视制式，取值范围如下：
    /////     typedef enum
    /////     {
    /////     PAL_DK = 0,
    /////     PAL_BG,
    /////     PAL_I,
    /////     PAL_M,
    /////     PAL_N,
    /////     NTSC_M,
    /////     NTSC_N,
    /////     NTSC_443,
    /////     SECAM_BG,
    /////     SECAM_DK,
    /////     SECAM_L,
    /////     TV_STANDARD_SUM
    /////     } SYS_TVStandard_Enum;*
    /////     <summary>
    /////         应答:
    /////         1、如果当前系统不是地面系统时，返回 "Please send cmd SYSTem:SwitchSystem to switch to terrestrial!"
    /////         2、音、视频频率设置正确时返回 Set Freq 232250000 hz ok!
    /////         3、音、视频频率设置不在有效范围内时返回 "The Freq 2322500000 hz is out of range!"
    /////         4、电视制式设置正确时返回 "set starderd to 0 Ok!"
    /////         5、电视制式不在有效范围内时返回 "starderd 12 is out of range!"
    /////         6、参数都设置正确时返回 "Set ATV decoder OK"
    /////         7、当前解码已经开启，没有关闭，再次收到命令时返回 Decoder is already started!”
    /////     </summary>
    public static readonly string SetAtvOpenDecoder = "Set:ATV:OpenDecoder";

    /// <summary>
    ///     关闭模拟音视频解码
    ///     参数：无
    ///     应答	:1、正常关闭解码时返回 " Close decoder ok! "
    ///     2、解码并没有开启时返回 "Decoder has not been started!"
    ///     CLIENT	SERVER
    ///     SYSTem:SwitchSystem 0 	将系统切换到地面系统
    ///     Set:TS:VolumeOn	打开音量
    ///     Set:TS:VolumeOff	关闭音量
    ///     Set:TS:VolumeVal 12	设置音量
    ///     Set:ATV:OpenDecoder 232250000 238750000 0	开启解码，图像载波频率为232.25MHz，音频载波频率为238.75HMz，电视制式为PAL DK
    ///     收到正确命令后，不管该频点是否有音视频节目都会开启解码，并且是全屏显示
    ///     Set:ATV:CloseDecoder	关闭解码，退出全屏显示，关闭音量
    /// </summary>
    public static readonly string SetAtvCloseDecoder = "Set:ATV:CloseDecoder";

    #endregion

    #region 扩展方法

    /// <summary>
    ///     获取指令的Byte数组
    /// </summary>
    /// <param name="s">指令串</param>
    public static byte[] GetOrder(this string s)
    {
        var sb = new StringBuilder();
        sb.Append(s);
        return GetOrderByte(sb);
    }

    /// <summary>
    ///     获取指令的Byte数组
    /// </summary>
    /// <typeparam name="T">参数类型</typeparam>
    /// <param name="s">指令串</param>
    /// <param name="t">需要追加的参数</param>
    public static byte[] GetOrder<T>(this string s, T t)
    {
        var sb = new StringBuilder();
        sb.Append(s).Append(' ');
        sb.Append(t);
        return GetOrderByte(sb);
    }

    /// <summary>
    ///     获取指令的Byte数组
    /// </summary>
    /// <typeparam name="T1">参数类型1</typeparam>
    /// <typeparam name="T2">参数类型2</typeparam>
    /// <param name="s">指令串</param>
    /// <param name="t1">需要追加的参数1</param>
    /// <param name="t2">需要追加的参数2</param>
    public static byte[] GetOrder<T1, T2>(this string s, T1 t1, T2 t2)
    {
        var sb = new StringBuilder();
        sb.Append(s).Append(' ');
        sb.Append(t1).Append(' ');
        sb.Append(t2);
        return GetOrderByte(sb);
    }

    public static byte[] GetOrder<T1, T2, T3>(this string s, T1 t1, T2 t2, T3 t3)
    {
        var sb = new StringBuilder();
        sb.Append(s).Append(' ');
        sb.Append(t1).Append(' ');
        sb.Append(t2).Append(' ');
        sb.Append(t3);
        return GetOrderByte(sb);
    }

    /// <summary>
    ///     获取功率值或者电平值
    /// </summary>
    /// <param name="s"></param>
    public static double GetPowerValue(this string s)
    {
        double power = 0;
        var index = s.IndexOf("dBuV", StringComparison.Ordinal);
        if (index == -1) index = s.IndexOf("dBmV", StringComparison.Ordinal);
        if (index == -1) index = s.IndexOf("dBm", StringComparison.Ordinal);
        if (index != -1 && s.Length - index > 0) double.TryParse(s[..index], out power);
        return power;
    }

    /// <summary>
    ///     获取MER值
    /// </summary>
    /// <param name="s"></param>
    public static double GetMer(this string s)
    {
        double mer = 0;
        var index = s.IndexOf("dB", StringComparison.Ordinal);
        if (index != -1 && s.Length - index > 0) double.TryParse(s[..^index], out mer);
        return mer;
    }

    /// <summary>
    ///     根据过滤器筛选出符合要求的子元素
    /// </summary>
    /// <param name="s">需要筛选的字符串</param>
    /// <param name="filter">筛选的条件字符串</param>
    public static List<string> GetSubElement(this string s, string filter)
    {
        List<string> result = null;
        filter = "," + filter;
        var index = s.IndexOf(filter, StringComparison.Ordinal);
        if (!s.Contains(filter)) //只有一个
            index = s.Contains("Not start TS Meas") ? -1 : s.Length - 1;
        var tempI = 0;
        while (index != -1)
        {
            (result ??= new List<string>()).Add(s.Substring(tempI, index));
            tempI += index + 1;
            var temp = s[tempI..];
            if (index + tempI == s.Length - 1 || index + tempI == s.Length) //最后一个输出,多一个逗号
            {
                result.Add(s.Substring(tempI, index));
                return result;
            }

            index = temp.IndexOf(filter, StringComparison.Ordinal);
        }

        return result;
    }

    /// <summary>
    ///     根据字符串获取字节数组
    /// </summary>
    /// <param name="sb"></param>
    private static byte[] GetOrderByte(StringBuilder sb)
    {
        sb.Append(" \r\n");
        var data = Encoding.UTF8.GetBytes(sb.ToString());
        sb = null;
        return data;
    }

    #endregion
}

///// <summary>
///// S7000音视频信息结构
///// </summary>
//struct S7000AudioAndVideo : IGetS7000Data
//{
//    /// <summary>
//    /// 序号，数值从0开始，最大值为PMT Real Quant-1
//    /// </summary>
//    public int No { get; set; }
//    //视频信息
//    /// <summary>
//    /// 视频PID
//    /// </summary>
//    public int VideoPid { get; set; }
//    /// <summary>
//    /// 视频类型 MPEG-2
//    /// </summary>
//    public string VideoType { get; set; }
//    /// <summary>
//    /// 视频比特率（帧/秒）
//    /// </summary>
//    public string VideoBitrate { get; set; }
//    /// <summary>
//    /// 视频分辨率，水平像素数*垂直像素数
//    /// </summary>
//    public string Resolution { get; set; }
//    //音频信息,如果一个节目有多个音频时，最多返回2个音频信息
//    /// <summary>
//    /// 音频PID
//    /// </summary>
//    public int AudioPid1 { get; set; }
//    /// <summary>
//    /// 音频模式 stereo双声道 single 单声道
//    /// </summary>
//    public string Mode1 { get; set; }
//    /// <summary>
//    /// 音频类型 MPEG-2
//    /// </summary>
//    public string AudioType1 { get; set; }
//    /// <summary>
//    /// 音频比特率128 Kbps
//    /// </summary>
//    public string AudioBitrate1 { get; set; }
//    /// <summary>
//    /// 音频PID
//    /// </summary>
//    public int AudioPid2 { get; set; }
//    /// <summary>
//    /// 音频模式 stereo双声道 single 单声道
//    /// </summary>
//    public string Mode2 { get; set; }
//    /// <summary>
//    /// 音频类型 MPEG-2
//    /// </summary>
//    public string AudioType2 { get; set; }
//    /// <summary>
//    /// 音频比特率128 Kbps
//    /// </summary>
//    public string AudioBitrate2 { get; set; }
//    int index;
//    public void GetData(string s)
//    {
//        string[] split = s.Split(',');
//        index = 0;
//        char filter = '=';
//        try
//        {
//            No = int.Parse(split[index].Split(filter)[1]);
//            index += 1;
//            VideoPid = int.Parse(split[index].Split(filter)[1]);
//            index += 1;
//            VideoType = split[index].Split(filter)[1];
//            index += 1;
//            Resolution = split[index].Split(filter)[1];
//            index += 1;
//            VideoBitrate = split[index].Split(filter)[1];
//            index += 4;
//            AudioPid1 = int.Parse(getVaule(split, "APid1"));
//            Mode1 = getVaule(split, "Mode");
//            AudioType1 = getVaule(split, "AType1");
//            AudioBitrate1 = getVaule(split, "Bitrate");
//            if (Mode1 == "single") return;
//            AudioPid2 = int.Parse(getVaule(split, "APid2"));
//            Mode2 = getVaule(split, "Mode");
//            AudioType2 = getVaule(split, "AType2");
//            AudioBitrate2 = getVaule(split, "Bitrate");
//        }
//        catch
//        {
//        }
//    }
//    /// <summary>
//    /// 在字符串数组中通过键获取值
//    /// </summary>
//    /// <param name="data">字符串数组</param>
//    /// <param name="key">键</param>
//    /// <returns></returns>
//    string getVaule(string[] data, string key)
//    {
//        string result = string.Empty;
//        key = String.Format("[{0}]", key);
//        for (int i = index; i < data.Length; i++)
//        {
//            if (data[i].Contains(key) && data[i].Contains("="))
//            {
//                result = data[i].Split('=')[1];
//                index++;
//                return result;
//            }
//        }
//        return result;
//    }
//}
///// <summary>
///// S7000 DTMB测试结果
///// </summary>
//struct S7000DTMBResult : IGetS7000Data
//{
//    /// <summary>
//    /// 锁定标识 1-lock，0-unlock
//    /// </summary>
//    public bool Locked { get; set; }
//    /// <summary>
//    /// 功率（dBuV）
//    /// </summary>
//    public double Power { get; set; }
//    /// <summary>
//    /// MER（dB）
//    /// </summary>
//    public double MER { get; set; }
//    /// <summary>
//    /// 误码数
//    /// </summary>
//    public int ErrorCode { get; set; }
//    /// <summary>
//    /// 码总数
//    /// </summary>
//    public int TotalCode { get; set; }
//    /// <summary>
//    /// 频道参数有效标志：
//    ///0-无效
//    ///1-表示以下频道参数[6~10]有效
//    /// </summary>
//    public bool ChannelParameterValidFlag { get; set; }
//    /// <summary>
//    /// CARR Mode(0-多载波，1-单载波)
//    /// </summary>
//    public CARRMode CARRMode { get; set; }
//    /// <summary>
//    /// 调制类型：锁定有效
//    ///4QAM=0
//    ///16QAM=1
//    ///32QAM=2
//    ///64QAM=3
//    ///4QAM_NR=4
//    /// </summary>
//    public DemType DemType { get; set; }
//    /// <summary>
//    /// 交织深度：锁定有效
//    ///TD_240N=0,
//    ///TD_240I=1
//    ///TD_720I=2
//    /// </summary>
//    public InterleavingDepth InterleavingDepth { get; set; }
//    /// <summary>
//    /// 编码码率：锁定有效
//    ///CR_0.4=0
//    ///CR_0.6=1
//    ///CR_0.8=2
//    /// </summary>
//    public CodeRate CodeRate { get; set; }
//    /// <summary>
//    /// 保护间隔：锁定有效
//    ///GI_420=0
//    ///GI_595=1
//    ///GI_945=2
//    /// </summary>
//    public GuardSpace GuardSpace { get; set; }
//    /// <summary>
//    /// 星座图坐标集合
//    /// </summary>
//    public List<ConstellationDiagramXY> XYCol { get; set; }
//    int index;
//    public void GetData(string s)
//    {
//        string[] split = s.Split(' ');
//        index = 0;
//        try
//        {
//            Locked = int.Parse(split[index]) == 1;
//            index += 1;
//            Power = split[index].GetPowerValue();
//            index += 1;
//            MER = split[index].GetMER();
//            index += 1;
//            ErrorCode = int.Parse(split[index]);
//            index += 1;
//            TotalCode = int.Parse(split[index]);
//            index += 1;
//            ChannelParameterValidFlag = int.Parse(split[index]) == 1;
//            if (!ChannelParameterValidFlag) return;
//            index += 1;
//            CARRMode = (CARRMode)Enum.Parse(typeof(CARRMode), split[index]);
//            index += 1;
//            DemType = (DemType)Enum.Parse(typeof(DemType), split[index]);
//            index += 1;
//            InterleavingDepth = (InterleavingDepth)Enum.Parse(typeof(InterleavingDepth), split[index]);
//            index += 1;
//            CodeRate = (CodeRate)Enum.Parse(typeof(CodeRate), split[index]);
//            index += 1;
//            GuardSpace = (GuardSpace)Enum.Parse(typeof(GuardSpace), split[index]);
//            index += 1;
//            XYCol = new List<ConstellationDiagramXY>();
//            for (int i = 0; i < 250; i++)
//            {
//                ConstellationDiagramXY xy = new ConstellationDiagramXY();
//                xy.X = int.Parse(split[index]);
//                index += 1;
//                xy.Y = int.Parse(split[index]);
//                index += 1;
//                if (xy.X != 0 && xy.Y != 0)
//                    XYCol.Add(xy);
//            }
//        }
//        catch
//        {
//        }
//    }
//}
///// <summary>
///// 星座图坐标
///// </summary>
//struct S7000ConstellationDiagramXY
//{
//    public int X { get; set; }
//    public int Y { get; set; }
//}
//enum CAType
//{
//    UnEncryped,
//    Encryped
//}
///// <summary>
///// 载波方式
///// </summary>
//enum CARRMode
//{
//    多载波,
//    单载波
//}
///// <summary>
///// 解调类型
///// </summary>
//enum DemType
//{
//    QAM_4,
//    QAM_16,
//    QAM_32,
//    QAM_64,
//    QAM_NR_4
//}
///// <summary>
///// 交织深度
///// </summary>
//enum InterleavingDepth
//{
//    TD_240N = 0,
//    TD_240I,
//    TD_720I
//}
///// <summary>
///// 编码码率 CR_0Point4=CR_0.4
///// </summary>
//enum CodeRate
//{
//    CR_0Point4,
//    CR_0Point6,
//    CR_0Point8,
//}
///// <summary>
///// 保护间隔
///// </summary>
//enum GuardSpace
//{
//    GI_420,
//    GI_595,
//    GI_945
//}
///// <summary>
///// 模拟电视制式
///// </summary>
//enum ATVStandard
//{
//    PAL_DK = 0,
//    PAL_BG,
//    PAL_I,
//    PAL_M,
//    PAL_N,
//    NTSC_M,
//    NTSC_N,
//    NTSC_443,
//    SECAM_BG,
//    SECAM_DK,
//    SECAM_L,
//    TV_STANDARD_SUM
//}
///// <summary>
///// 星座图坐标
///// </summary>
//struct ConstellationDiagramXY
//{
//    public int X { get; set; }
//    public int Y { get; set; }
//}

#region 码流分析返回数据标签说明

//[No.]=0      		序号，数值从0开始，最大值为PMT Real Quant-1
//[PNum]=100     	节目号，等号后面是以10进制数据传送的
//[CA]=UnEncryped   	CA加密标志，UnEncryped为解扰，Encryped为加密
//[SerName]=cctv-1   节目名，表示节目名称为cctv-1
//[ProName]=CTV      节目提供商名称
//[SerType]=DIG TV   该节目的流类型
//[Resolution]=      视频分辨率
//以下以DTMB为例进行说明：
//Set:SignalType 5
//Set:FreqHz 474000000
//Set:BwHz 8000000
//Set:DTMB:Test 1 1 //获取频道参数，获取星座图
//Get:DTMB:Result?
//此处省略仪器返回，等待信号锁定后，可以读取MER，功率，星座图等数据。
//此条命令可以多次调用。
//待信号锁定后，才能进行码流分析。
//Set:TS:Start 1 1 //开启码流分析，选择射频入，按照DVB标准解析
//以下协议可以多次调用
//Get:TS:Status? // 获取TS状态，ts_status=1表示有码流输入
//Get:TS:MemStatus? // mem_status=0，码流分析缓冲区未满
//Get:TS: ChangeStatus? // change_status =0，码流正常
//Get:TS:BasicInfo?//
//Get:TS:ProgSum? //获取PMT数，PMT Quant=11，PMT Real Quant=10
//Get:TS:ProgList? //获取节目列表
//Get:TS:ProgInfo? //获取所有节目的音视频信息
//以下协议用于解码以及切换节目。
//Set:TS:OpenDecoder 1 //节目序号以Get:TS:ProgSum?获取的序号为准，从0开始排列。
//此例中参数为1，表示第二个频道.
//Set:TS:CloseDecoder //切换节目时，要关闭解码，然后再次开启新的解码。当码流分析过程终止时，                     需要首先终止解码器
//Set:TS:OpenDecoder 2
//Set:TS:Stop //码流分析终止时需要调用此协议，否则可能会影响下次开启。

#endregion