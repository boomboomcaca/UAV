using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Driver.PCOMS;

[DriverDescription(FeatureType = FeatureType.PCOMS,
    Name = "公众通信管控",
    Category = ModuleCategory.RadioSuppressing,
    Version = "1.1.0",
    Model = "PCOMS",
    MaxInstance = 1,
    Description = "联合监测设备实现引导或独立的公众通信管制功能")]
public partial class Pcoms
{
    private FreqBandTemplate[] _freqBands;

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.Suppressor)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("压制机")]
    [Module(NeedModule = ModuleCategory.RadioSuppressing,
        NeedFeature = FeatureType.PCOMS,
        NeedEquip = true,
        IsPrimaryDevice = true
    )]
    [Description("设置无线电管制的设备")]
    [PropertyOrder(0)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice Suppressor { get; set; }

    [Name("decoder")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("解码设备")]
    [Module(NeedEquip = true,
        NeedModule = ModuleCategory.Decoder,
        IsPrimaryDevice = true,
        NeedFeature = FeatureType.BsDecoding)]
    [Description("提供基站解码设备")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice Decoder { get; set; }

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.SwitchArray)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("开关矩阵")]
    [Module(NeedModule = ModuleCategory.SwitchArray,
        NeedEquip = true)]
    [Description("切换监测与管制的开关矩阵")]
    [PropertyOrder(3)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice SwitchArray { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("decodeDataSendInterval")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("发送基站解码数据时间间隔")]
    [ValueRange(20, 30000)]
    [DefaultValue(500)]
    [Description("发送基站解码数据间隔时间，到一定时间再向客户端更新数据，单位毫秒。")]
    [Style(DisplayStyle.Slider)]
    public int DecodeDataSendInterval { get; set; } = 1000;

    [Parameter(AbilitySupport = FeatureType.PCOMS, Template = typeof(FreqBandTemplate))]
    [Name("bandList")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("频段列表")]
    [Description("频段列表，用于频率筛选。")]
    [Browsable(false)]
    [Style(DisplayStyle.Default)]
    public Dictionary<string, object>[] BandList
    {
        get => null;
        set
        {
            if (value == null) return;
            _freqBands = Array.ConvertAll(value, item => (FreqBandTemplate)item);
        }
    }
}