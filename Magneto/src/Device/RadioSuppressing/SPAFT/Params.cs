using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.SPAFT;

[DeviceDescription(Name = "SPAFT",
    DeviceCategory = ModuleCategory.RadioSuppressing,
    Manufacturer = "成都阿莱夫信息技术有限公司",
    Description = "适用于20MHz ~ 6000MHz范围内分段多通道管制",
    Model = "SPAFT",
    Version = "1.2.3",
    MaxInstance = 1,
    FeatureType = FeatureType.FBANDS)]
public partial class Spaft
{
    #region 常规参数

    private RftxSegmentsTemplate[] _rftxSegments;

    [Parameter(AbilitySupport = FeatureType.FBANDS, Template = typeof(RftxSegmentsTemplate))]
    [Name(ParameterNames.RftxSegments)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("射频管控")]
    [Description("设置管制载波信道信息，包含频率模式，干扰制式等信息")]
    [PropertyOrder(0)]
    [Style(DisplayStyle.Default)]
    public Dictionary<string, object>[] RftxSegments
    {
        set
        {
            if (value == null)
            {
                _rftxSegments = null;
                return;
            }

            var templates = Array.ConvertAll(value, item => (RftxSegmentsTemplate)item).ToList();
            if (templates.Any() != true)
            {
                _rftxSegments = null;
                return;
            }

            templates.RemoveAll(p => p is not { RftxSwitch: true });
            var groups = from p in templates group p by p.PhysicalChannelNumber;
            var combTemplates = new List<RftxSegmentsTemplate>();
            foreach (var group in groups)
            {
                var temp = group.FirstOrDefault(p => p is
                    { RftxSwitch: true, RftxFrequencyMode: 3, StartFrequency: > 0, StopFrequency: > 0 });
                if (temp != null) combTemplates.Add(temp);
            }

            templates.RemoveAll(p => combTemplates.Any(q => q.PhysicalChannelNumber == p.PhysicalChannelNumber));
            foreach (var combTemplate in combTemplates)
            {
                var startFrequency = combTemplate.StartFrequency;
                var stopFrequency = combTemplate.StopFrequency;
                var count = 0;
                if (_rftxBands != null)
                    count = Array.Find(_rftxBands, p => p.PhysicalChannelNumber == combTemplate.PhysicalChannelNumber)
                        ?.LogicalChannelCount ?? 0;
                else
                    count = combTemplate.PhysicalChannelNumber < 3 ? 10 : 8;
                if (count < 1) continue;
                var step = count == 1 ? stopFrequency - startFrequency : (stopFrequency - startFrequency) / (count - 1);
                for (var i = 0; i < count; i++)
                    templates.Add(new RftxSegmentsTemplate
                    {
                        PhysicalChannelNumber = combTemplate.PhysicalChannelNumber,
                        LogicalChannelNumber = i,
                        Modulation = Modulation.Cw,
                        ModulationSource = 0,
                        RftxFrequencyMode = 0,
                        Frequency = startFrequency + i * step,
                        Bandwidth = 200,
                        Baudrate = 200,
                        RftxSwitch = true,
                        StartFrequency = startFrequency,
                        StopFrequency = stopFrequency
                    });
            }

            _rftxSegments = templates.ToArray();
            Trace.WriteLine(Utils.ConvertToJson(templates));
            SetChannelFrequencyEnds(_rftxSegments); // 必须按通道编号设置好每个通道的频率信息
            RaiseChannelParameterSetting(ToLocalOscillatorFrame, _rftxSegments); // 设置通道本振
            Thread.Sleep(LockMs); // 按厂商手册和源代码说明，为了确保锁相环锁定
            RaiseChannelParameterSetting(ToFrequencyListFrame, _rftxSegments); // 设置通道频率相关的参数信息
        }
    }

    private float[] _powers = { 0, 0, 0, 0, 0, 0 };

    [Parameter(AbilitySupport = FeatureType.FBANDS)]
    [Name(ParameterNames.Powers)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("信道功率")]
    [Description("用于描述通道使用的功率值")]
    [DefaultValue(new float[] { 0, 0, 0, 0, 0, 0 })]
    [ValueRange(0.0f, 100.0f)]
    [PropertyOrder(1)]
    [Style(DisplayStyle.Slider)]
    public float[] Powers
    {
        get => _powers;
        set
        {
            if (value == null) return;
            const int powersArrayLen = 6;
            _powers = new float[powersArrayLen];
            //_powers前端是按照通道号从小到大的顺序
            for (var i = 0; i < powersArrayLen && i < value.Length; i++) _powers[i] = value[i];
            Trace.WriteLine(Utils.ConvertToJson(_powers));
            RaiseChannelParameterSetting(ToPowerListFrame, _powers);
        }
    }

    private int _audioIndex = -1;

    [Parameter(AbilitySupport = FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [Name("audioIndex")]
    [DisplayName("音频序号")]
    [Description("设置当FM调制源模式下的音频文件序号，文件来自指定音频文件目录")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|自动|音频1|音频2|音频3|音频4|音频5",
        StandardValues = "|-1|0|1|2|3|4")]
    [DefaultValue(-1)]
    [PropertyOrder(2)]
    [Style(DisplayStyle.Dropdown)]
    public int AudioIndex
    {
        get => _audioIndex;
        set
        {
            _audioIndex = value;
            if (_audioFiles == null || _audioFiles.Length == 0)
                _audioIndex = -1;
            else
                _audioIndex = _audioIndex == -1
                    ? new Random().Next(_audioFiles.Length)
                    : _audioIndex % _audioFiles.Length;
            if (TaskState == TaskState.Start) RaiseAudioSetting(true, _audioIndex);
        }
    }

    #endregion

    #region 配置参数

    /// <summary>
    ///     频段索引与通道号对应字典， 键为管制频段索引，值为频段物理通道号。
    /// </summary>
    private readonly Dictionary<int, int> _indexChannelNumberMap = new();

    private RftxBandsTemplate[] _rftxBands;

    [Parameter(Template = typeof(RftxBandsTemplate), AbilitySupport = FeatureType.FBANDS)]
    [Name(ParameterNames.RftxBands)]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("管制频段")]
    [Description("表示管制通道包含的基本信息，包含适用于频段范围")]
    [ParametersDefault(
        new[]
        {
            ParameterNames.PhysicalChannelNumber,
            ParameterNames.StartFrequency,
            ParameterNames.StopFrequency,
            ParameterNames.LogicalChannelCount
        }, new object[] { 0, 20.0d, 90.0d, 10 }, new object[] { 3, 90.0d, 130.0d, 8 },
        new object[] { 1, 130.0d, 600.0d, 10 }, new object[] { 2, 600.0d, 1000.0d, 10 },
        new object[] { 4, 1000.0d, 3200.0d, 8 }, new object[] { 5, 3200.0d, 6000.0d, 8 })]
    [Style(DisplayStyle.Default)]
    public Dictionary<string, object>[] RftxBands
    {
        set
        {
            if (value == null) return;
            _indexChannelNumberMap.Clear();
            var list = new List<RftxBandsTemplate>();
            for (var i = 0; i < value.Length; i++)
            {
                var item = (RftxBandsTemplate)value[i];
                list.Add(item);
                _indexChannelNumberMap.Add(i, item.PhysicalChannelNumber);
            }

            _rftxBands = list.OrderBy(p => p.PhysicalChannelNumber).ToArray();
        }
    }

    #endregion

    #region 安装参数

    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("低端地址")]
    [Description("设置低频段压制设备TCP地址，涉及20MHz-90MHz，130MHz-600MHz，600MHz-1000MHz三个通道")]
    [DefaultValue("192.168.1.2")]
    [PropertyOrder(0)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string LowRangeIp { get; set; }

    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("低端端口")]
    [Description("设置低频段压制设备控制端口，涉及20MHz-90MHz，130MHz-600MHz，600MHz-1000MHz三个通道")]
    [DefaultValue(7000)]
    [PropertyOrder(1)]
    [ValueRange(1024, 65535, 0)]
    [Style(DisplayStyle.Input)]
    public int LowRangePort { get; set; }

    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("高端地址")]
    [Description("设置高频段压制设备IP地址，涉及90MHz-130MHz，1000MHz-3200MHz，3200MHz-60000MHz三个通道")]
    [DefaultValue("192.168.1.3")]
    [PropertyOrder(2)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string HighRangeIp { get; set; }

    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("高端端口")]
    [Description("设置高频段压制设备控制端口，涉及90MHz-130MHz，1000MHz-3200MHz，3200MHz-60000MHz三个通道")]
    [DefaultValue(7000)]
    [PropertyOrder(3)]
    [ValueRange(1024, 65535, 0)]
    [Style(DisplayStyle.Input)]
    public int HighRangePort { get; set; }

    private string _audioDirectory;

    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("音频目录")]
    [Description("设置外部音频源所在目录，当调制源设置为网络音频时，将从此目录选择相应文件作为调制对象")]
    [DefaultValue(".")]
    [PropertyOrder(4)]
    [ValueRange(double.NaN, double.NaN, 255)]
    [Style(DisplayStyle.Input)]
    public string AuidoDirectory
    {
        get => _audioDirectory;
        set
        {
            _audioDirectory = value;
            if (string.IsNullOrWhiteSpace(value) || string.Equals(_audioDirectory, "."))
                _audioDirectory = AppDomain.CurrentDomain.BaseDirectory;
        }
    }

    #endregion
}