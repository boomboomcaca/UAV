using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.DT1200AS;

[DeviceDescription(Name = "DT1200AS",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.Decoder,
    Version = "1.1.0",
    Model = "DT1200AS",
    MaxInstance = 1,
    FeatureType = FeatureType.BsDecoding,
    Description = "DT1200AS全制式基站扫频仪，支持2/3/4G基站信息解码。")]
public partial class Dt1200As
{
    private string _segmentType = "中国频段";

    [PropertyOrder(0)]
    [Parameter(AbilitySupport = FeatureType.BsDecoding)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("查询间隔")]
    [DefaultValue(1)]
    [Description("查询时间间隔，单位 秒")]
    [ValueRange(1, 100, 0)]
    [Style(DisplayStyle.Input)]
    public int Second { get; set; } = 1;

    [PropertyOrder(0)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Measurement)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|中国频段|边境频段",
        DisplayValues = "|中国频段|边境频段")]
    [DisplayName("频段类别")]
    [DefaultValue("中国频段")]
    [Description("按照频段类别切换扫频频段及模式")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string SegmentCategory
    {
        get => _segmentType;
        set
        {
            if (value.Equals(_segmentType)) return;
            _segmentType = value;
        }
    }
}