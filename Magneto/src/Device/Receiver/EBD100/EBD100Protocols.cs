namespace Magneto.Device.EBD100;

internal class Ebd100Command
{
    #region 常用命令

    /// <summary>
    ///     设置测向机频率
    /// </summary>
    internal const string SetFreqCommand = "F";

    /// <summary>
    ///     罗盘询问命令
    /// </summary>
    internal const string QueryCompass = "C?";

    /// <summary>
    ///     罗盘取几次的平均值,格式为:Kx
    ///     1&lt;=x&lt;=9
    /// </summary>
    internal const string CompassAvgCommand = "K";

    /// <summary>
    ///     格式化测向机返回数据命令
    ///     有以下6种
    ///     S0 返回:A123 其中123为示向度
    ///     S1 返回:A123,43 其中43为测向质量
    ///     S2 返回:A123,43,500 其中500为测向时效
    ///     S3 返回:A123,43,500,55 其中55为电平
    ///     S4 返回:A123,43,500,55,184312.3 其中184312.3为时间:18:43:12.3h
    ///     S5 返回:A123,43,500,55,184312.3,1234 其中1234为频率为:1234MHz
    /// </summary>
    internal const string FormatReturnDataCommand = "S";

    /// <summary>
    ///     开始测向
    /// </summary>
    internal const string StartDdfCommand = "D1";

    /// <summary>
    ///     停止测向
    /// </summary>
    internal const string StopDdfCommand = "D0";

    /// <summary>
    ///     有三种测向模式;NORMAL,CONTINUOUS,GATE,格式为:Mx
    ///     NORMAL:高于电平门限的进行测向  x=0
    ///     CONTINUOUS:连续测向，这时电平门限无效 x=1
    ///     GATE:和NORMAL方式一样，不同的是测向停止后平均值缓冲区并不清空,将参与继续积分 x=2
    /// </summary>
    internal const string DdfModeCommand = "M";

    /// <summary>
    ///     设置中频,格式:Zx
    ///     x=0  IF=10.7Mhz
    ///     x=1  IF=21.4MHz
    /// </summary>
    internal const string SetIfCommand = "Z";

    /// <summary>
    ///     设置测向带宽命令 格式:Wx
    ///     VHF和UHF:x=0 1KHz; x=1 2.5KHz;x=2 8KHz; x=3 15KHz; x=4 100KHz
    ///     HF:x=5 250Hz;x=6 500Hz;x=7 1KHz; x=8 3KHz;x=9 5KHz;
    ///     x=10 SDS
    /// </summary>
    internal const string SetBwCommand = "W";

    /// <summary>
    ///     设置测向时效,格式：Ix
    ///     x=0 100ms;x=1 200ms;x=2 500ms;x=3 1s;x=4 2s;x=5 5s;
    /// </summary>
    internal const string SetInterTimeCommand = "I";

    /// <summary>
    ///     设置测向门限,格式:Qx
    ///     0&lt;=x&lt;=100
    /// </summary>
    internal const string SetSquelchCommand = "Q";

    /// <summary>
    ///     程序控制模式
    /// </summary>
    internal const string SetRemoteCommand = "L0";

    /// <summary>
    ///     本地控制模式
    /// </summary>
    internal const string SetLocalCommand = "L1";

    #endregion
}