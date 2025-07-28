using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Magneto.Protocol.Define;

namespace Magneto.Contract;

/// <summary>
///     功能描述
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class DriverDescriptionAttribute : Attribute
{
    public string Name { get; set; }

    /// <summary>
    ///     功能类型
    /// </summary>
    public FeatureType FeatureType { get; set; } = FeatureType.None;

    /// <summary>
    ///     独占任务，true表示所有设备为独占状态，其他任务不能使用
    /// </summary>
    public bool IsMonopoly { get; set; }

    /// <summary>
    ///     功能支持返回的数据类型
    /// </summary>
    public MediaType MediaType { get; set; } = MediaType.None;

    /// <summary>
    ///     MaxInstance,当前模块支持的最大实例数，超过则抛异常
    /// </summary>
    public int MaxInstance { get; set; } = 8;

    /// <summary>
    ///     设备信息描述
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///     功能的分类
    /// </summary>
    public ModuleCategory Category { get; set; }

    public string Model { get; set; }

    /// <summary>
    ///     驱动版本
    /// </summary>
    public string Version { get; set; }
}

[AttributeUsage(AttributeTargets.Class)]
public class RepeatedParameterAttribute : Attribute
{
}

/// <summary>
///     用于功能模块描述子模块成员的特性
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ModuleAttribute : Attribute
{
    /// <summary>
    ///     模块需要支持的功能
    /// </summary>
    public FeatureType NeedFeature { get; set; } = FeatureType.None;

    /// <summary>
    ///     模块的主类型
    /// </summary>
    public ModuleCategory NeedModule { get; set; } = ModuleCategory.Monitoring;

    /// <summary>
    ///     表明该属性需要的模块是否必须配置，默认是true
    /// </summary>
    public bool NeedEquip { get; set; } = true;

    /// <summary>
    ///     表明该设备是否是该功能使用的主设备
    /// </summary>
    public bool IsPrimaryDevice { get; set; } = false;

    /// <summary>
    ///     表明该设备是否是该功能使用的主设备
    /// </summary>
    public Type[] NeedInterface { get; set; }

    #region 构造函数

    /// <summary>
    ///     无参数构造函数
    /// </summary>
    public ModuleAttribute()
    {
    }

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="category">模块类型</param>
    public ModuleAttribute(ModuleCategory category)
    {
        NeedModule = category;
    }

    #endregion
}

[AttributeUsage(AttributeTargets.Field)]
public class EnumValueAttribute(string displayName) : Attribute
{
    public string DisplayName { get; } = displayName;
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ApplicableAttribute(bool value = true) : Attribute
{
    public bool Value { get; } = value;
}

public class EnumValueConverter(Type type) : EnumConverter(type)
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        return sourceType == typeof(string) ||
               TypeDescriptor.GetConverter(typeof(Enum)).CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (value is string x) return GetEnumValue(EnumType, x);
        if (value is Enum y) return GetEnumDescription(y);
        return base.ConvertFrom(context, culture, value);
    }

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
        Type destinationType)
    {
        if (value is Enum e && destinationType == typeof(string))
            return GetEnumDescription(e);
        if (value is string s && destinationType == typeof(string))
            return GetEnumDescription(EnumType, s);
        return base.ConvertTo(context, culture, value, destinationType);
    }

    public static string GetEnumDescription(Enum value)
    {
        var fieldInfo = value.GetType().GetField(value.ToString());
        var attributes = (EnumValueAttribute[])fieldInfo?.GetCustomAttributes(typeof(EnumValueAttribute), false);
        return attributes is { Length: > 0 } ? attributes[0].DisplayName : value.ToString();
    }

    public static string GetEnumDescription(Type value, string name)
    {
        var fieldInfo = value.GetField(name);
        var attributes = (EnumValueAttribute[])fieldInfo?.GetCustomAttributes(typeof(EnumValueAttribute), false);
        return attributes is { Length: > 0 } ? attributes[0].DisplayName : name;
    }

    public static object GetEnumValue(Type value, string displayName)
    {
        var fields = value.GetFields();
        foreach (var fieldInfo in fields)
        {
            var attributes = (EnumValueAttribute[])fieldInfo.GetCustomAttributes(typeof(EnumValueAttribute), false);
            if (attributes.Length > 0 && attributes[0].DisplayName == displayName)
                return fieldInfo.GetValue(fieldInfo.Name);
            if (fieldInfo.Name == displayName) return fieldInfo.GetValue(fieldInfo.Name);
        }

        return displayName;
    }
}

/// <summary>
///     数据类型特性标识 序列化与反序列化时使用
///     必须要显式知道其类型 故以此特性进行标识
///     使用方法 为类添加特性：[GlobalData] 即可
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
public class GlobalDataAttribute : Attribute
{
    // 仅做标识使用，故不需要实现任何成员
}

/// <summary>
///     标记是否是常驻参数
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ResidentAttribute : Attribute
{
    // 仅做标识使用，故不需要实现任何成员
}

/// <summary>
///     类及其属性绑定到PropertyGrid的排序处理规则 使用方法：
///     为属性类添加类特性：[TypeConverter(typeof(PropertySorter))]
///     如果必要再添加类特性：[DefaultProperty("属性名称")]
///     为属性添加属性特性：[PropertyOrder(顺序号)]
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class PropertyOrderAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}

/// <summary>
///     设置参数的默认子参数集合
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ParametersDefaultAttribute : Attribute
{
    /// <summary>
    ///     构造
    /// </summary>
    /// <param name="names">参数名集合</param>
    /// <param name="defaults">参数值集合，支持设置多个集合，每个参数值集合的数量必须与参数名一致</param>
    public ParametersDefaultAttribute(string[] names, params object[] defaults)
    {
        if (names == null || names.Length == 0) return;
        if (defaults == null || defaults.Length == 0) return;
        DefaultParameters = new object[defaults.Length];
        for (var i = 0; i < defaults.Length; i++)
        {
            if (defaults[i] is not object[] array) throw new ArgumentException($"值的类型{defaults[i].GetType()}不受支持");
            // continue;
            if (array == null || array.Length != names.Length) throw new ArgumentException("值的数量与参数名的个数不匹配");
            //continue;
            var dic = new Dictionary<string, object>();
            for (var j = 0; j < array.Length; j++) dic.Add(names[j], array[j]);
            DefaultParameters[i] = dic;
        }
    }

    // 使用示例
    // [ParametersDefault(
    //     new[] { ParameterNames.StartFrequency, ParameterNames.StopFrequency, ParameterNames.StepFrequency }, // 参数名集合
    //     new object[] { 88d, 108d, 25d },      // 第一个参数值集合
    //     new object[] { 1000d, 2000d, 25d },   // 第二个参数值集合
    //     new object[] { 2000d, 3000d, 25d }    // 第三个参数值集合
    // )]
    /// <summary>
    ///     默认子参数集合
    /// </summary>
    public object[] DefaultParameters { get; }
}

/// <summary>
///     关联参数特性
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ChildrenAttribute : Attribute
{
    public ChildrenAttribute(string children, params object[] values)
    {
        var arr = children[1..].Split(children[0]);
        Children = arr.ToList();
        Values = values.ToList();
    }

    public List<string> Children { get; }
    public List<object> Values { get; set; }
}