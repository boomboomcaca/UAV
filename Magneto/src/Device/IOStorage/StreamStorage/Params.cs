using System;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.StreamStorage;

[DeviceDescription(Name = "流盘存储",
    DeviceCategory = ModuleCategory.IoStorage,
    Manufacturer = "Aleph",
    Version = "1.3.0",
    Model = "StreamStorage",
    Description = "流盘存储设备",
    MaxInstance = 1,
    FeatureType = FeatureType.IQRETRI)]
public partial class StreamStorage
{
    #region 运行参数

    private StreamStorageMode _ssMode = StreamStorageMode.None;

    [Parameter(AbilitySupport = FeatureType.IQRETRI)]
    [Name("ssMode")]
    [Category(PropertyCategoryNames.Command)]
    [DisplayName("流盘模式")]
    [Description("流盘数据模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|none|record|playback|drop",
        DisplayValues = "|关闭|记录|回放|取消")]
    [DefaultValue("none")]
    [Resident]
    [PropertyOrder(1)]
    [Browsable(false)]
    [Style(DisplayStyle.Radio)]
    public string SsMode
    {
        get => _ssMode.ToString().ToLower();
        set => Enum.TryParse(value, true, out _ssMode);
    }

    [Parameter(AbilitySupport = FeatureType.IQRETRI)]
    [Name("recordId")]
    [Category(PropertyCategoryNames.Command)]
    [DisplayName("流盘编号")]
    [Description("进行流盘回放时的编号")]
    [DefaultValue("")]
    [Resident]
    [PropertyOrder(1)]
    [Browsable(false)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string RecordId { get; set; } = "";

    [Parameter(AbilitySupport = FeatureType.IQRETRI)]
    [Name("progressIndex")]
    [Category(PropertyCategoryNames.Command)]
    [DisplayName("回放进度")]
    [Description("回访时跳转回放进度")]
    [ValueRange(0, 10000)]
    [DefaultValue(0)]
    [Resident]
    [PropertyOrder(1)]
    [Browsable(false)]
    [Style(DisplayStyle.Slider)]
    public int ProgressIndex { get; set; }

    #endregion

    #region 安装参数

    [Parameter(IsInstallation = true)]
    [Name("address")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("连接地址")]
    [Description("设备的管道或IP地址")]
    [DefaultValue("streamStorage")]
    [PropertyOrder(26)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Address { get; set; } = "streamStorage";

    [Parameter(IsInstallation = true)]
    [Name("port")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("连接方式")]
    [Description("连接流盘存储设备的方式")]
    [ValueRange(1000, 65535, 0)]
    [DefaultValue(9999)]
    [PropertyOrder(26)]
    [Style(DisplayStyle.Input)]
    public int Port { get; set; } = 9999;

    [Parameter(IsInstallation = true)]
    [Name("connectMode")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("连接方式")]
    [Description("连接流盘存储设备的方式(在虚拟模式下仅支持管道方式连接)")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|false|true",
        DisplayValues = "|管道|TCP")]
    [DefaultValue(false)]
    [PropertyOrder(26)]
    [Style(DisplayStyle.Switch)]
    public bool ConnectMode { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("isSim")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("虚拟模式")]
    [Description("使用虚拟模式进行测试")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|false|true",
        DisplayValues = "|禁用|启用")]
    [DefaultValue(false)]
    [PropertyOrder(26)]
    [Style(DisplayStyle.Switch)]
    public bool IsSim { get; set; }

    #endregion
}