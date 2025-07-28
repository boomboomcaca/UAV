using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Driver.GSMR;

[DriverDescription(FeatureType = FeatureType.GSMR,
    Name = "GSM-R专项监测",
    Category = ModuleCategory.Monitoring,
    Version = "1.0.0",
    Model = "GSMR",
    MaxInstance = 5,
    Description = "GSM-R专项监测")]
public partial class Gsmr
{
    private FreqBandTemplate[] _freqBands;

    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.GSMR)]
    [Name(ParameterNames.Receiver)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("接收机")]
    [Module(NeedEquip = true,
        NeedModule = ModuleCategory.Monitoring,
        IsPrimaryDevice = true,
        NeedFeature = FeatureType.SCAN)]
    [Description("提供监测数据的主设备")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice Receiver { get; set; }

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.AntennaController)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("天线控制器")]
    [Module(NeedModule = ModuleCategory.AntennaControl,
        NeedFeature = FeatureType.None,
        NeedEquip = true)]
    [Description("使用的天线控制器，实现天线的逻辑控制")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public new IDevice AntennaController
    {
        get => base.AntennaController;
        set => base.AntennaController = value;
    }

    [Name("decoder")]
    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.GSMR)]
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
    [Name("decodeDataSendInterval")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("发送基站解码数据时间间隔")]
    [ValueRange(20, 30000)]
    [DefaultValue(500)]
    [Description("发送基站解码数据间隔时间，到一定时间再向客户端更新数据，单位毫秒。")]
    [Style(DisplayStyle.Slider)]
    public int DecodeDataSendInterval { get; set; } = 1000;

    [Parameter(AbilitySupport = FeatureType.BsDecoding, Template = typeof(FreqBandTemplate))]
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