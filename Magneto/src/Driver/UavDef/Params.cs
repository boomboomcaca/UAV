using System;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Driver.UavDef;

[DriverDescription(
    Name = "无人机防御",
    MaxInstance = 0,
    Category = ModuleCategory.Monitoring,
    Version = "1.10.1.0",
    FeatureType = FeatureType.UavDef,
    Description = "全面的无人机防御",
    IsMonopoly = false)]
public partial class UavDef
{
    private (Guid SignalId, int Segment, int StartIndex, int StopIndex) _markScanDf = (Guid.Empty, 0, 0, 0);

    private int _timeLength = 1;

    [Parameter(AbilitySupport = FeatureType.UavDef)]
    [Name("markScanDf")]
    [Category(PropertyCategoryNames.Scan)]
    [Browsable(false)]
    [DisplayName("示向度选择")]
    [DefaultValue("")]
    public string MarkScanDf
    {
        get => string.Join("|", _markScanDf.Segment, _markScanDf.StartIndex, _markScanDf.StopIndex);
        set
        {
            if (string.IsNullOrEmpty(value)) return;
            var splits = value.Split('|');
            if (!Guid.TryParse(splits[0], out _markScanDf.SignalId)) return;
            _markScanDf.Segment = int.Parse(splits[1]);
            _markScanDf.StartIndex = int.Parse(splits[2]);
            _markScanDf.StopIndex = int.Parse(splits[3]);
            _bearingStatistics.Clear();
        }
    }

    [Parameter(AbilitySupport = FeatureType.UavDef)]
    [DisplayName("测向积分时间")]
    [Description("设置宽带测向数据归一化后的优化时间窗")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DefaultValue(1)]
    [Style(DisplayStyle.Input)]
    public int TimeLength
    {
        get => _timeLength;
        set
        {
            _timeLength = value;
            _bearingStatistics.TimeLength = _timeLength;
        }
    }

    #region Install properies.

    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.UavDef)]
    [Name("receiver")]
    [PropertyOrder(1)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("解码器")]
    [Module(
        NeedEquip = true,
        NeedModule = ModuleCategory.Monitoring | ModuleCategory.UavDecoder,
        IsPrimaryDevice = true,
        NeedFeature = FeatureType.UavDef | FeatureType.Scan)]
    [Description("主要的无线电解码设备")]
    public IDevice Decoder { get; set; }

    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.UavDef)]
    [Name("receivers")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("接收机/测向机")]
    [Module(
        NeedEquip = false,
        NeedModule = ModuleCategory.Monitoring | ModuleCategory.DirectionFinding,
        IsPrimaryDevice = false,
        NeedFeature = FeatureType.UavDef | FeatureType.ScanDf)]
    [Description("除主设备外，所有的无线电监测测向设备")]
    public IDevice[] Receivers { get; set; }

    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.UavDef)]
    [Name("jammer")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("干扰机")]
    [Module(
        NeedModule = ModuleCategory.RadioSuppressing,
        NeedFeature = FeatureType.UavDef)]
    [Description("所有的无线电压制（干扰）设备")]
    public IDevice[] Jammers { get; set; }

    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.UavDef)]
    [Name("radar")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("雷达")]
    [Module(
        NeedModule = ModuleCategory.Radar,
        NeedFeature = FeatureType.UavDef)]
    [Description("所有的雷达设备")]
    public IDevice[] Radars { get; set; }

    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.UavDef)]
    [Name("imageRecognizer")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("图像识别机")]
    [Module(
        NeedModule = ModuleCategory.Recognizer,
        NeedFeature = FeatureType.UavDef)]
    [Description("所有的图像识别设备")]
    public IDevice[] ImageRecognizers { get; set; }

    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.UavDef)]
    [Name("httpFilesServerPort")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("录像文件HTTP服务端口号")]
    [Browsable(false)]
    [ValueRange(5001, 65535)]
    [DefaultValue(10001)]
    [Description("录像文件HTTP服务端口号")]
    public int HttpFilesServerPort { get; set; } = 10001;

    #endregion
}