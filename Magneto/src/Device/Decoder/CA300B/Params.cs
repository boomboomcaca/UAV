using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.CA300B;

[DeviceDescription(
    Name = "CA300B",
    Model = "CA300B",
    DeviceCategory = ModuleCategory.Decoder,
    MaxInstance = 1,
    Manufacturer = "成都阿莱夫信息技术有限公司",
    Version = "1.1.2",
    Description = "CA300B电视分析仪",
    FeatureType = FeatureType.RTV)]
public partial class Ca300B
{
    #region 安装参数

    /// <summary>
    ///     IP地址
    /// </summary>
    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.IpAddress)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("IP地址")]
    [Description("CA300B设备IP地址")]
    [DefaultValue("192.168.1.33")]
    [Style(DisplayStyle.Input)]
    public string Ip { get; set; } = "192.168.1.33";

    [Parameter(IsInstallation = true)]
    [Name("dvbtBandwidth")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("DVB-T带宽")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|8MHz|6MHz",
        StandardValues = "|8000|6000")]
    [Description("CA300B设备DVBT制式带宽")]
    [DefaultValue(8000)]
    [Style(DisplayStyle.Radio)]
    [Unit(UnitNames.KHz)]
    public int DvbtBandwidth { get; set; } = 8000;

    #endregion

    #region 普通参数

    private int _volumeSize = 15;

    /// <summary>
    ///     音量大小
    /// </summary>
    [Parameter(AbilitySupport = FeatureType.RTV)]
    [Category(PropertyCategoryNames.Demodulation)]
    [Name("volume")]
    [DisplayName("调整音量")]
    [Description("设置音量大小，值越大声音越大。")]
    [ValueRange(0, 30)]
    [Browsable(false)]
    [DefaultValue(15)]
    [Style(DisplayStyle.Slider)]
    public int Volume
    {
        get => _volumeSize;
        set
        {
            if (value < 0) return;
            _volumeSize = value;
            // TODO: 现在暂时没有音量调整
            // SetVolume(_volumeSize);
        }
    }

    #endregion

    #region 高级参数

    private Dictionary<string, object>[] _searchProgram;

    [Parameter(AbilitySupport = FeatureType.RTV, Template = typeof(SearchProgramTemplate))]
    [Category(PropertyCategoryNames.Demodulation)]
    [Name("searchProgram")]
    [DisplayName("节目搜索")]
    [Description("按频段设置节目搜索")]
    [Browsable(false)]
    [Style(DisplayStyle.Default)]
    public Dictionary<string, object>[] SearchProgram
    {
        get => _searchProgram;
        set
        {
            if (value == null)
            {
                Console.WriteLine("节目搜索为空");
                return;
            }

            _searchProgram = value;
            Console.WriteLine($"发送搜索指令:{_searchProgram.Length}个");
            var cmd = new ProgramCmd
            {
                Cmd = Array.ConvertAll(_searchProgram, item => (SearchProgramTemplate)item),
                Type = ProgramType.Search
            };
            _cmdQueue.Enqueue(cmd);
        }
    }

    private bool _cancelSearch;

    [Name("cancelSearch")]
    [Parameter(AbilitySupport = FeatureType.RTV)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("停止搜索")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|停止|不停")]
    [Description("停止搜索节目。")]
    [Browsable(false)]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    public bool CancelSearch
    {
        get => _cancelSearch;
        set
        {
            Console.WriteLine($"发送停止搜索指令:{value}");
            // if (_cancelSearch == value)
            // {
            //     return;  // 防止多次调用
            // }
            _cancelSearch = value;
            _newProtocol.CancelSearch();
            // SendSearchResult();
        }
    }

    private string _playProgram = string.Empty;

    /// <summary>
    ///     播放节目
    /// </summary>
    [Parameter(AbilitySupport = FeatureType.RTV)]
    [Name("playProgram")]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("播放节目")]
    [Browsable(false)]
    [Description("播放节目，格式：制式|频率|节目编号|节目名称，如：DTMB|538|8|CCTV-1")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string PlayProgram
    {
        get => _playProgram;
        set
        {
            Console.WriteLine($"发送播放指令:{value}");
            _playProgram = value;
            var cmd = new ProgramCmd
            {
                Cmd = _playProgram,
                Type = ProgramType.Play
            };
            _cmdQueue.Enqueue(cmd);
        }
    }

    #endregion
}

public class SearchProgramTemplate
{
    [PropertyOrder(0)]
    [Parameter(AbilitySupport = FeatureType.RTV)]
    [Name(ParameterNames.StartFrequency)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("起始频率")]
    [ValueRange(20.0d, 8000.0d, 6)]
    [DefaultValue(87.0d)]
    [Unit(UnitNames.MHz)]
    [Description("设置频段扫描起始频点，单位为MHz")]
    [Style(DisplayStyle.Input)]
    public double StartFrequency { get; set; } = 87.0d;

    [PropertyOrder(1)]
    [Parameter(AbilitySupport = FeatureType.RTV)]
    [Name(ParameterNames.StopFrequency)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("终止频率")]
    [ValueRange(20.0d, 8000.0d, 6)]
    [DefaultValue(108.0d)]
    [Unit(UnitNames.MHz)]
    [Description("设置扫描终止频率，单位MHz")]
    [Style(DisplayStyle.Input)]
    public double StopFrequency { get; set; } = 108.0d;

    [PropertyOrder(2)]
    [Parameter(AbilitySupport = FeatureType.RTV)]
    [Name(ParameterNames.StepFrequency)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("步进")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|Auto|8MHz|6MHz",
        StandardValues = "|0|8000|6000")]
    [DefaultValue(0d)]
    [Unit(UnitNames.KHz)]
    [Description("设置扫描步进，单位kHz")]
    [Style(DisplayStyle.Radio)]
    public double StepFrequency { get; set; } = 0d;

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.RTV)]
    [Name("standard")]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("节目制式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|ANAFM|ANATV|DVBC|DVBT|DVBT2|DTMB|CMMB",
        DisplayValues = "|模拟广播|模拟电视|DVBC|DVBT|DVBT2|DTMB|CMMB")]
    [DefaultValue(TvStandard.ANAFM)]
    [Description("设置频段扫描步进，单位kHz")]
    [Style(DisplayStyle.Dropdown)]
    public TvStandard Standard { get; set; } = TvStandard.ANAFM;

    public static explicit operator SearchProgramTemplate(Dictionary<string, object> dict)
    {
        if (dict == null) return null;
        var template = new SearchProgramTemplate();
        var type = template.GetType();
        try
        {
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var name =
                    Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                        ? property.Name
                        : nameAttribute.Name;
                if (dict.TryGetValue(name, out var value))
                {
                    if (property.PropertyType.IsEnum)
                    {
                        if (value is object[] list)
                        {
                            // Flag
                            long num = 0;
                            if (list.Length == 0)
                                foreach (var item in Enum.GetValues(property.PropertyType))
                                    num |= Convert.ToInt64(item);
                            else
                                foreach (var item in list)
                                {
                                    var enum1 = Utils.ConvertStringToEnum(item.ToString(), property.PropertyType);
                                    num |= Convert.ToInt64(enum1);
                                }

                            var objValue = Enum.ToObject(property.PropertyType, num);
                            property.SetValue(template, objValue, null);
                        }
                        else
                        {
                            var objValue = Utils.ConvertStringToEnum(value.ToString(), property.PropertyType);
                            property.SetValue(template, objValue, null);
                        }
                    }
                    else
                    {
                        property.SetValue(template, value, null);
                    }
                }
            }
        }
        catch
        {
            // 容错代码
        }

        return template;
    }

    public Dictionary<string, object> ToDictionary()
    {
        var dic = new Dictionary<string, object>();
        var type = GetType();
        try
        {
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                if (Attribute.GetCustomAttribute(property, typeof(ParameterAttribute)) is not ParameterAttribute)
                    continue;
                var name =
                    Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                        ? property.Name
                        : nameAttribute.Name;
                var value = property.GetValue(this);
                dic.Add(name, value);
            }
        }
        catch
        {
            // 容错代码
        }

        return dic;
    }
}