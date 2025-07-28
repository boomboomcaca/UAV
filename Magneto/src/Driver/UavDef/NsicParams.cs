using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Magneto.Contract;
using Magneto.Driver.UavDef.NSIC;
using Magneto.Protocol.Define;
using Newtonsoft.Json;

namespace Magneto.Driver.UavDef;

public partial class UavDef
{
    [Parameter(AbilitySupport = FeatureType.UavDef)]
    [Name("functionSwitch")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|信号比对|模板采集")]
    [Resident]
    [DisplayName("功能切换")]
    [Description("切换采集模板与信号比对")]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    public bool FunctionSwitch { get; set; }

    [Parameter(AbilitySupport = FeatureType.UavDef)]
    [Name("setSimData")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|是|否")]
    [Resident]
    [DisplayName("模拟信号")]
    [Description("是否产生模拟信号")]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    public bool SetSimData { get; set; }

    [Parameter(AbilitySupport = FeatureType.UavDef)]
    [Name(ParameterNames.AutoThreshold)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|自动|手动")]
    [Resident]
    [DisplayName("自动门限")]
    [Description("切换自动门限与手动门限")]
    [DefaultValue(true)]
    [Style(DisplayStyle.Switch)]
    public bool AutoThreshold { get; set; }

    [Parameter(AbilitySupport = FeatureType.UavDef)]
    [Name(ParameterNames.ThresholdValue)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [Resident]
    [DisplayName("门限")]
    [Description("如果是手动门限则本参数为门限值;如果是自动门限则本参数为门限容差;如果为信号比对功能则本参数为截获阈值")]
    [ValueRange(0, 60)]
    [DefaultValue(6)]
    [Unit(UnitNames.DBuV)]
    [Style(DisplayStyle.Slider)]
    public double ThresholdValue { get; set; }

    [Parameter(AbilitySupport = FeatureType.UavDef)]
    [Name("templateID")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("模板ID")]
    [Description("新信号比对的模板ID")]
    [DefaultValue("6")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string TemplateId { get; set; }

    [Parameter(AbilitySupport = FeatureType.UavDef, Template = typeof(TemplateInfo))]
    [DisplayName("获取所有模板")]
    [Description("返回TemplateInfo序列化的Json字符串。注意：设置非null值，清空所有存储的模板")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    public static string[] GetComparisonTemplates
    {
        get
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.ComparisonTemplate);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var files = Directory.GetFiles(dir);
            return files.Select(File.ReadAllText).ToArray();
        }
        set
        {
            if (value is null) return;
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.ComparisonTemplate);
            if (Directory.Exists(dir)) Directory.Delete(dir);
        }
    }

    [Parameter(AbilitySupport = FeatureType.UavDef, Template = typeof(TemplateInfo))]
    [DisplayName("添加模板")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [Description("输入TemplateInfo序列化的Json字符串。")]
    public static string AddComparisonTemplate
    {
        get => string.Empty;
        set
        {
            if (string.IsNullOrEmpty(value)) return;
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.ComparisonTemplate);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var cur = JsonConvert.DeserializeObject<TemplateInfo>(value);
            var path = Path.Combine(dir, cur.Id);
            File.WriteAllText(path, value);
        }
    }

    [Parameter(AbilitySupport = FeatureType.UavDef)]
    [DisplayName("删除模板")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [Description("输入删除模板的Guid")]
    public static string DelComparisonTemplate
    {
        get => string.Empty;
        set
        {
            if (string.IsNullOrEmpty(value)) return;
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.ComparisonTemplate);
            var path = Path.Combine(dir, value);
            if (File.Exists(path)) File.Delete(path);
        }
    }
}