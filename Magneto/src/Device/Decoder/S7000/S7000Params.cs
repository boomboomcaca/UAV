using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device;

[DeviceDescription(
    Name = "S7000",
    Model = "S7000",
    DeviceCategory = ModuleCategory.Decoder,
    MaxInstance = 1,
    Manufacturer = "成都阿莱夫信息技术有限公司",
    Version = "1.1.0",
    Description = "S7000电视分析仪",
    FeatureType = FeatureType.RTV)]
public partial class S7000
{
    #region 安装参数

    /// <summary>
    ///     IP地址
    /// </summary>
    [Parameter(IsInstallation = true)]
    [PropertyOrder(30)]
    [Name(ParameterNames.IpAddress)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("IP地址")]
    [Description("S7000设备的IP地址。")]
    [DefaultValue("192.168.151.92")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Ip { get; set; } = "192.168.151.92";

    #endregion

    #region 普通参数

    /// <summary>
    ///     模拟电视自动搜索载噪比门限
    /// </summary>
    private int _cNrThreshold = 5;

    [PropertyOrder(0)]
    [Name("cnrThreshold")]
    [Parameter(AbilitySupport = FeatureType.RTV)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("载噪比门限")]
    [Description("设置模拟电视自动搜索载噪比门限，此门限和电平门限同时满足才能作为稳定的电视信号。")]
    [ValueRange(5, 20)]
    [Browsable(false)]
    [Style(DisplayStyle.Slider)]
    public int CnrThreshold
    {
        get => _cNrThreshold;
        set
        {
            if (_audioValue < 0) return;
            _cNrThreshold = value;
            _programSearchAnatv.Cnr = _cNrThreshold;
        }
    }

    /// <summary>
    ///     模拟电视自动搜索电平门限
    /// </summary>
    private int _levelThreshold = 30;

    [PropertyOrder(1)]
    [Name(ParameterNames.LevelThreshold)]
    [Parameter(AbilitySupport = FeatureType.RTV)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("电平门限")]
    [Description("设置模拟电视自动搜索电平门限，此门限和载噪比门限同时满足才能作为稳定的电视信号。")]
    [ValueRange(30, 100)]
    [DefaultValue(30)]
    [Browsable(false)]
    [Style(DisplayStyle.Slider)]
    public int LevelThreshold
    {
        get => _levelThreshold;
        set
        {
            if (_audioValue < 0) return;
            _levelThreshold = value;
            _programSearchAnatv.Level = _levelThreshold;
        }
    }

    /// <summary>
    ///     设置音量大小
    /// </summary>
    private int _audioValue = 15;

    [PropertyOrder(2)]
    [Name("volume")]
    [Parameter(AbilitySupport = FeatureType.RTV)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("调整音量")]
    [Description("设置音量大小，值越大声音越大。")]
    [ValueRange(0, 30)]
    [Browsable(false)]
    [DefaultValue(15)]
    [Style(DisplayStyle.Slider)]
    public int AudioValue
    {
        get => _audioValue;
        set
        {
            if (value < 0) return;
            _audioValue = value;
            _buffer = S7000Protocol.SetTsVolumeVal.GetOrder(_audioValue);
            _client.Send(_buffer);
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
            _searchProgram = value;
            Console.WriteLine($"发送搜索指令:{_searchProgram?.Length}个");
            if (_searchProgram != null)
            {
                var cmd = new ProgramCmd
                {
                    Cmd = Array.ConvertAll(_searchProgram, item => (SearchProgramTemplate)item),
                    Type = ProgramType.Search
                };
                _cmdQueue.Enqueue(cmd);
            }
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
            if (_cancelSearch == value) return; // 防止多次调用
            _cancelSearch = value;
            SendSearchResult();
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
            if (string.IsNullOrEmpty(value)) return;
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