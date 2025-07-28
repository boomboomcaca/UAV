using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.HCNetDVR;

[DeviceDescription(
    Name = "HCNetDVR",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.Decoder,
    FeatureType = FeatureType.AVProcess,
    Version = "1.2.0",
    MaxInstance = 1,
    Description = "电视播放，传输视频和音频信号")]
public partial class HcNetDvr
{
    #region 安装参数

    [PropertyOrder(0)]
    [Name(ParameterNames.IpAddress)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("IP地址")]
    [Description("设置连接设备的网络地址，IPv4格式[x.x.x.x]")]
    [DefaultValue("192.168.151.93")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Ip { get; set; } = "192.168.151.93";

    [PropertyOrder(1)]
    [Name(ParameterNames.Port)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("端口")]
    [DefaultValue(8000)]
    [Description("设置连接并控制设备的网络端口号")]
    [ValueRange(1024, 65535, 0)]
    [Style(DisplayStyle.Input)]
    public int Port { get; set; } = 8000;

    //private int _channelId = 0;

    //[PropertyOrder(2)]
    //[Parameter(AbilitySupport = SpecificAbility.AudioVideoDecoding)]
    //[Category(PropertyCategoryNames.Normal)]
    //[DisplayName("预览通道")]
    //[StandardValues(IsSelectOnly = true,
    //    StandardValues = "|0|1|2|3",
    //    DisplayValues = "|通道1|通道2|通道3|通道4")]
    //[DefaultValue(0)]
    //[Description("设置播放播放音视频通道号")]
    //public int ChannelID
    //{
    //    get { return _channelID; }
    //    set { _channelID = value; }
    //}
    [PropertyOrder(3)]
    [Name("userName")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("用户名")]
    [Description("设置连接到设备的用户名")]
    [DefaultValue("admin")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string UserName { get; set; } = "admin";

    [PropertyOrder(4)]
    [Name("password")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("密码")]
    [Description("设置连接到设备的密码")]
    [DefaultValue("dc123456")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Password { get; set; } = "dc123456";

    #endregion

    #region 高级参数

    private string _playProgram = string.Empty;

    /// <summary>
    ///     当前切换的节目
    /// </summary>
    [Name("playProgram")]
    [Parameter(AbilitySupport = FeatureType.AVProcess)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("当前切换的节目")]
    [Browsable(false)]
    [Description("设置当前切换的节目，格式:制式|频率|节目编号|节目名称，如:DTMB|538|8|CCTV-1")]
    public string PlayProgram
    {
        get => _playProgram;
        set
        {
            if (string.IsNullOrEmpty(value)) return;
            _playProgram = value;
            _playType = PlayTypeEnum.RealTime;
            // 制式|频率|节目编号|节目名称
            var split = _playProgram.Split('|');
            if (split.Length < 4)
            {
                Trace.WriteLine("播放的节目格式错误!");
                return;
            }

            _ = Enum.TryParse(split[0], out _currentStandard);
            _ = double.TryParse(split[1], out _currentFrequency);
            var number = split[2].Replace(split[1], "");
            _ = int.TryParse(number, out _currentNumber);
            _currentProgramName = split[3];
        }
    }

    private bool _startRealPlay;

    /// <summary>
    ///     启动播放
    /// </summary>
    [Name("startRealPlay")]
    [Parameter(AbilitySupport = FeatureType.None)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("当前切换的节目")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|启动|停止")]
    [Browsable(false)]
    [Description("开始播放节目")]
    [Style(DisplayStyle.Switch)]
    public bool StartRealPlay
    {
        get => _startRealPlay;
        set
        {
            _startRealPlay = value;
            if (_startRealPlay)
            {
                var task = new Task(() => StartRealPlayAsync().ConfigureAwait(false));
                task.Start();
            }
            else
            {
                var task = new Task(() => StopReceiveData());
                task.Start();
            }
        }
    }

    private bool _dvrRecord;

    [Name("dvrRecord")]
    [Parameter(AbilitySupport = FeatureType.AVProcess)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("录像")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|启动|停止")]
    [Description("启动、停止录像。")]
    [Browsable(false)]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    public bool DvrRecord
    {
        get => _dvrRecord;
        set
        {
            if (_dvrRecord == value) return; // 防止多次调用
            _dvrRecord = value;
            // 启动、停止录像
            _ = Task.Run(() => StartStopDvrRecordAsync(_dvrRecord));
        }
    }

    /// <summary>
    ///     录像文件查询  查询近一个月的文件
    /// </summary>
    [Name("queryRecord")]
    [Parameter(AbilitySupport = FeatureType.AVProcess)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("录像文件查询")]
    [Browsable(false)]
    [Description("查询录像文件")]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    public bool QueryRecord { get; set; }

    #region 录像文件回放和控制

    private string _playBackFileName = string.Empty;

    /// <summary>
    ///     录像回放（开始）
    ///     这里的格式暂时改为 起始时间的时间戳|结束时间的时间戳
    /// </summary>
    [Name("playBackFileName")]
    [Parameter(AbilitySupport = FeatureType.AVProcess)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("录像回放")]
    [Browsable(false)]
    [Description("下发需要回放的录像文件名称")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string PlayBackFileName
    {
        get => _playBackFileName;
        set
        {
            if (string.IsNullOrEmpty(value)) return;
            if (value.Length <= 2)
                // 客户端会下发无效参数“-1”
                return;
            _playBackFileName = value;
            // TODO : 尝试解析文件名
            // 回放录像
            _ = Task.Run(PlayBackByNameAsync);
        }
    }

    /// <summary>
    ///     暂停
    /// </summary>
    [Name("playControl")]
    [Parameter(AbilitySupport = FeatureType.AVProcess)]
    [Category(PropertyCategoryNames.Demodulation)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|realPlay|realStop|realPause|playbackPlay|playbackStop|playbackPause",
        DisplayValues = "|实时播放|实时停止|实时暂停|回放播放|回放停止|回放暂停"
    )]
    [DisplayName("播放控制")]
    [Browsable(false)]
    [DefaultValue(PlayControlMode.RealPlay)]
    [Description("暂停、播放。0(实时播放)、1(实时停止)、2(实时暂停)、3(回放播放)、4(回放停止)、5(回放暂停)")]
    [Style(DisplayStyle.Dropdown)]
    public PlayControlMode PlayControl
    {
        get;
        set;
        // 播放、暂停回放
        // PlayOrPause_DVR();
    } = PlayControlMode.RealPlay;

    /// <summary>
    ///     快放 慢放 恢复正常速度
    /// </summary>
    [Name("speedControl")]
    [Parameter(AbilitySupport = FeatureType.AVProcess)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("速度控制")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|slow|fast|single|normal",
        DisplayValues = "|慢放|快放|单帧|正常")]
    [Browsable(false)]
    [DefaultValue(SpeedControlMode.Normal)]
    [Description("慢放、快放、单帧、正常速度。")]
    [Style(DisplayStyle.Radio)]
    public SpeedControlMode PlaySpeed
    {
        get;
        set;
        // ChangePlaySpeed();
    } = SpeedControlMode.Normal;

    /// <summary>
    ///     正放 倒放
    /// </summary>
    [Name("forward")]
    [Parameter(AbilitySupport = FeatureType.AVProcess)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("方向控制")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|正放|倒放")]
    [Browsable(false)]
    [DefaultValue(true)]
    [Description("正放、倒放。")]
    [Style(DisplayStyle.Switch)]
    public bool ForwardReverse
    {
        get;
        set;
        // PlayForwardReverse();
    } = true;

    /// <summary>
    ///     停止
    /// </summary>
    private bool _stopPlayBack;

    [Name("stopPlayback")]
    [Parameter(AbilitySupport = FeatureType.AVProcess)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("回放控制")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|停止|不停")]
    [Browsable(false)]
    [DefaultValue(false)]
    [Description("停止录像回放。")]
    [Style(DisplayStyle.Switch)]
    public bool StopPlayBack
    {
        get => _stopPlayBack;
        set
        {
            if (!value) return;
            _stopPlayBack = true;
            StopPlayBackDvr();
        }
    }

    #endregion

    // 硬盘管理
    // private string _formatDisk = string.Empty;
    // [Parameter(AbilitySupport = FeatureType.RTV)]
    // [Category(PropertyCategoryNames.Demodulation)]
    // [DisplayName("格式化")]
    // [Browsable(false)]
    // [Description("格式化设备硬盘。")]
    // public string FormatDisk
    // {
    //     get { return _formatDisk; }
    //     set
    //     {
    //         if (string.IsNullOrEmpty(value))
    //         {
    //             return;
    //         }
    //         _formatDisk = value;
    //         // 格式化硬盘
    //         FormatDiskDVR();
    //     }
    // }

    #endregion
}