using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Driver.BSDEC;

[DriverDescription(
    FeatureType = FeatureType.BsDecoding,
    Name = "单站基站搜索",
    Category = ModuleCategory.Decoder,
    Version = "1.2.0",
    Model = "BSDEC",
    MaxInstance = 1,
    IsMonopoly = true,
    Description = "单站基站搜索功能")]
public partial class Bsdec
{
    private FreqBandTemplate[] _freqBands;

    [Name("decoder")]
    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.BsDecoding)]
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