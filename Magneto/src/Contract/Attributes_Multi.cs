using System;
using System.Text.RegularExpressions;
using Magneto.Protocol.Define;

namespace Magneto.Contract;

public interface IMultipleAttribute
{
    string RegexPattern { get; set; }
    string Key { get; }

    /// <summary>
    /// </summary>
    short Level { get; }

    bool IsMatch(string pattern);
}

public abstract class MultipleAttribute : Attribute, IMultipleAttribute
{
    public string Key { get; set; }
    public short Level { get; protected set; } = -1;
    public string RegexPattern { get; set; }

    public bool IsMatch(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        if (!string.IsNullOrWhiteSpace(Key))
        {
            if (string.Equals(Key, name, StringComparison.OrdinalIgnoreCase))
            {
                Level = 2;
                return true;
            }
        }
        else if (!string.IsNullOrWhiteSpace(RegexPattern))
        {
            if (Regex.IsMatch(name, RegexPattern, RegexOptions.IgnoreCase))
            {
                Level = 1;
                return true;
            }
        }
        else
        {
            Level = 0;
            return true;
        }

        return false;
    }
}

/// <summary>
///     设备描述属性
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DeviceDescriptionAttribute : Attribute
{
    /// <summary>
    ///     名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     设备类型
    /// </summary>
    public ModuleCategory DeviceCategory { get; set; }

    /// <summary>
    ///     设备驱动版本
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    ///     设备序列号
    /// </summary>
    public string Sn { get; set; }

    /// <summary>
    ///     设备厂商
    /// </summary>
    public string Manufacturer { get; set; }

    public string Model { get; set; }

    /// <summary>
    ///     模块支持的具体功能
    /// </summary>
    public FeatureType FeatureType { get; set; } = FeatureType.None;

    /// <summary>
    ///     模块支持的站点能力
    /// </summary>
    public EdgeCapacity Capacity { get; set; } = EdgeCapacity.None;

    public string Description { get; set; }

    /// <summary>
    ///     MaxInstance,当前模块支持的最大实例数，超过则抛异常
    /// </summary>
    public int MaxInstance { get; set; } = 8;

    /// <summary>
    ///     设备能力
    /// </summary>
    public string DeviceCapability { get; set; }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class ParameterAttribute : MultipleAttribute
{
    /// <summary>
    ///     是否是安装参数，默认为false
    /// </summary>
    public bool IsInstallation { get; set; }

    /// <summary>
    ///     该参数与哪些业务功能相关
    /// </summary>
    public FeatureType AbilitySupport { get; set; } = FeatureType.None;

    public Type Template { get; set; }
}

/// <summary>
///     描述参数名
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class NameAttribute : MultipleAttribute
{
    public NameAttribute()
    {
    }

    public NameAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}

/// <summary>
///     用于描述参数的取值列表
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class StandardValuesAttribute : MultipleAttribute
{
    #region 变量/属性

    /// <summary>
    ///     指定该参数是否只能选择，不可输入
    /// </summary>
    public bool IsSelectOnly { get; set; } = true;

    /// <summary>
    ///     枚举字符串，用首字符分割，如 \"|FM|AM|IQ\" 或 \",FM,AM,IQ\"。
    /// </summary>
    public string StandardValues { get; set; } = string.Empty;

    /// <summary>
    ///     获取/设置显示用的枚举字符串，各个枚举值之间用首字符分割，如调制模式："|调频|调幅|原始IQ" 或 ",调频,调幅,原始IQ"。
    /// </summary>
    public string DisplayValues { get; set; } = string.Empty;

    public object DefaultValue { get; set; }

    #endregion

    #region 构造函数

    /// <summary>
    ///     无参数构造函数
    /// </summary>
    public StandardValuesAttribute()
    {
    }

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="standardValues">标准值</param>
    /// <param name="displayValues">显示值</param>
    /// <param name="defaultValue"></param>
    public StandardValuesAttribute(string standardValues, string displayValues, object defaultValue = null)
    {
        StandardValues = standardValues;
        DisplayValues = displayValues;
        DefaultValue = defaultValue;
    }

    #endregion
}

/// <summary>
///     用于描述数值类型的参数取值范围
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class ValueRangeAttribute : MultipleAttribute
{
    /// <summary>
    ///     范围最小值
    /// </summary>
    public double? Minimum { get; set; }

    /// <summary>
    ///     范围最大值
    /// </summary>
    public double? Maximum { get; set; }

    public double Step { get; set; } = 1;

    #region 构造函数

    /// <summary>
    ///     无参数构造函数
    /// </summary>
    public ValueRangeAttribute()
    {
    }

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="min">最小值</param>
    /// <param name="max">最大值</param>
    /// <param name="step">步进；如果是<see cref="DisplayStyle.Input" />参数，这里代表小数位数；如果是<see cref="DisplayStyle.Slider" />型参数，这里代表滑动步进</param>
    /// <param name="key"></param>
    /// <param name="regexPattern"></param>
    public ValueRangeAttribute(double min, double max, double step = 1, string key = null, string regexPattern = null)
    {
        if (double.IsFinite(min)) Minimum = min;
        if (double.IsFinite(max)) Maximum = max;
        Step = step;
        Key = key;
        RegexPattern = regexPattern;
    }

    #endregion
}

/// <summary>
///     单位特性
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class UnitAttribute : MultipleAttribute
{
    public UnitAttribute()
    {
    }

    public UnitAttribute(string unit)
    {
        Unit = unit;
    }

    public string Unit { get; }
}

/// <summary>
///     参数样式特性
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class StyleAttribute : MultipleAttribute
{
    public StyleAttribute()
    {
    }

    public StyleAttribute(DisplayStyle style)
    {
        Style = style;
    }

    public DisplayStyle Style { get; }
}