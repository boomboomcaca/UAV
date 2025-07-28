using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Magneto.Protocol.Define;
using MessagePack;
using Newtonsoft.Json;

namespace Magneto.Contract;

public static partial class Utils
{
    /// <summary>
    ///     获取经过时间补偿的当前时间
    ///     此时间为云端时间
    /// </summary>
    public static DateTime GetNowTime()
    {
        return DateTime.Now.AddMilliseconds(RunningInfo.TimeCompensation);
    }

    /// <summary>
    ///     获取经过时间补偿的当前时间的UTC时间戳
    ///     此时间为云端时间的时间戳
    /// </summary>
    public static ulong GetNowTimestamp()
    {
        var time = DateTime.Now.AddMilliseconds(RunningInfo.TimeCompensation);
        return GetTimestamp(time);
    }

    /// <summary>
    ///     将本地时间修改为云端时间
    /// </summary>
    /// <param name="time"></param>
    public static DateTime ConvertTimeToCloud(DateTime time)
    {
        return time.AddMilliseconds(RunningInfo.TimeCompensation);
    }

    /// <summary>
    ///     计算扫描的总点数
    /// </summary>
    /// <param name="startFrequency">起始频率 MHz</param>
    /// <param name="stopFrequency">结束频率 MHz</param>
    /// <param name="stepFrequency">步进 kHz</param>
    public static int GetTotalCount(double startFrequency, double stopFrequency, double stepFrequency)
    {
        var start = new decimal(startFrequency);
        var stop = new decimal(stopFrequency);
        var step = new decimal(stepFrequency / 1000.0d);
        var total = (int)Math.Round((stop - start) / step, 0) + 1;
        return total;
    }

    public static int GetTotalCount(double range, double stepFrequency)
    {
        var step = new decimal(stepFrequency / 1000.0d);
        var total = (int)Math.Round((decimal)range / step, 0) + 1;
        return total;
    }

    /// <summary>
    ///     返回相对于1970年的时间戳
    /// </summary>
    /// <param name="time">时间</param>
    /// <param name="isUtc">是否转换为UTC的时间戳</param>
    /// <returns>时间戳 单位：ns</returns>
    public static ulong GetTimestamp(DateTime time, bool isUtc = true)
    {
        var tmp = isUtc ? time.ToUniversalTime() : time.ToLocalTime();
        return (ulong)tmp.Subtract(new DateTime(1970, 1, 1)).Ticks * 100;
    }

    /// <summary>
    ///     返回时间戳代表的时间
    /// </summary>
    /// <param name="ticks">时间戳</param>
    /// <param name="isUtc">当前时间戳是否为UTC时间</param>
    public static DateTime GetTimeByTicks(ulong ticks, bool isUtc = true)
    {
        var kind = isUtc ? DateTimeKind.Utc : DateTimeKind.Local;
        return new DateTime(1970, 1, 1, 0, 0, 0, kind).AddTicks((long)ticks / 100);
    }

    /// <summary>
    ///     获取指定频率在扫描频点中的索引值
    /// </summary>
    /// <param name="currFrequency">要获取索引值的当前频率值 MHz</param>
    /// <param name="startFrequency">扫描的起始频率 MHz</param>
    /// <param name="stepFrequency">扫描的频率 kHz</param>
    public static int GetCurrIndex(double currFrequency, double startFrequency, double stepFrequency)
    {
        try
        {
            var curr = new decimal(currFrequency);
            var start = new decimal(startFrequency);
            var step = new decimal(stepFrequency / 1000.0d);
            var currIndex = decimal.ToInt32((curr - start) / step);
            return currIndex;
        }
        catch (OverflowException)
        {
            return -1;
        }
    }

    /// <summary>
    ///     将枚举值转换为KeyAttribute特性指定的string字符串
    /// </summary>
    /// <param name="value"></param>
    public static string ConvertEnumToString(object value)
    {
        var enumType = value.GetType();
        if (!enumType.IsEnum) return value.ToString();
        var field = enumType.GetField(value.ToString() ?? string.Empty);
        if (field == null) return value.ToString();
        return Attribute.GetCustomAttribute(field, typeof(KeyAttribute)) is not KeyAttribute { IntKey: null } attribute
            ? value.ToString()
            : attribute.StringKey;
    }

    /// <summary>
    ///     将string字符串转换为枚举值
    /// </summary>
    /// <param name="str"></param>
    /// <param name="enumType"></param>
    public static object ConvertStringToEnum(string str, Type enumType)
    {
        if (!enumType.IsEnum) return null;
        if (string.IsNullOrEmpty(str)) return null;
        foreach (var value in Enum.GetValues(enumType))
        {
            var field = enumType.GetField(value.ToString() ?? string.Empty);
            if (field != null)
                if (Attribute.GetCustomAttribute(field, typeof(KeyAttribute)) is KeyAttribute { IntKey: null } attribute
                    && str.Equals(attribute.StringKey))
                    return value;
            if (str.Equals(value.ToString(), StringComparison.OrdinalIgnoreCase)) return value;
        }

        return null;
    }

    /// <summary>
    ///     将string字符串转换为枚举值
    /// </summary>
    /// <param name="str"></param>
    /// <typeparam name="T"></typeparam>
    public static T ConvertStringToEnum<T>(string str)
    {
        if (string.IsNullOrEmpty(str)) return default;
        var enumType = typeof(T);
        if (!enumType.IsEnum) return default;
        foreach (var value in Enum.GetValues(enumType))
        {
            var field = enumType.GetField(value.ToString() ?? string.Empty);
            if (field != null)
                if (Attribute.GetCustomAttribute(field, typeof(KeyAttribute)) is KeyAttribute { IntKey: null } attribute
                    && str.Equals(attribute.StringKey))
                    return (T)value;
            if (str.Equals(value.ToString())) return (T)value;
        }

        return default;
    }

    public static PropertyInfo FindPropertyByName(string name, Type type)
    {
        foreach (var property in type.GetProperties())
        {
            if (Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is NameAttribute nameAttribute
                && name.Equals(nameAttribute.Name))
                return property;
            if (name.Equals(property.Name)) return property;
        }

        return null;
    }

    public static Tuple<string, object> GetPropertyNameValue(string name, object obj)
    {
        var property = FindPropertyByName(name, obj.GetType());
        if (property == null) return null;
        return new Tuple<string, object>(property.Name, property.GetValue(obj, null));
    }

    /// <summary>
    ///     根据参数名获取参数的Json名
    /// </summary>
    /// <param name="name">参数名</param>
    /// <param name="type"></param>
    public static string FindJsonNameByPropertyName(string name, Type type)
    {
        var info = type.GetProperty(name);
        if (info == null) return string.Empty;
        if (Attribute.GetCustomAttribute(info, typeof(NameAttribute)) is NameAttribute nameAttribute)
            return nameAttribute.Name;
        return string.Empty;
    }

    /// <summary>
    ///     更新参数的拥有者
    /// </summary>
    /// <param name="parameter">参数</param>
    /// <param name="moduleInfo">拥有者</param>
    public static void UpdateParameterOwners(ref Parameter parameter, ModuleInfo moduleInfo)
    {
        var name = parameter.Name;
        if (moduleInfo.Parameters.All(item => item.Name != name)) return;
        parameter.Owners ??= new List<string>();
        if (parameter.Owners.Contains(moduleInfo.Id.ToString())) return;
        parameter.Owners.Add(moduleInfo.Id.ToString());
    }

    /// <summary>
    ///     将类型转换为Json字符串
    /// </summary>
    /// <param name="obj"></param>
    public static string ConvertToJson(object obj)
    {
        return JsonConvert.SerializeObject(obj);
    }

    public static T DeserializeFromJson<T>(string str)
    {
        return JsonConvert.DeserializeObject<T>(str);
    }

    /// <summary>
    ///     将类型通过MessagePack转换为Json字符串
    /// </summary>
    /// <param name="obj"></param>
    public static string ConvertToMessagePackJson(object obj)
    {
        return MessagePackSerializer.SerializeToJson(obj);
    }

    public static T DeserializeFromMessagePackJson<T>(string str)
    {
        return MessagePackSerializer.Deserialize<T>(Encoding.UTF8.GetBytes(str).AsMemory());
    }

    /// <summary>
    ///     将类型通过MessagePack转换为byte数组
    /// </summary>
    /// <param name="obj"></param>
    public static byte[] ConvertToMessagePackData(object obj)
    {
        return MessagePackSerializer.Serialize(obj);
    }

    /// <summary>
    ///     将json字符串反序列化为T对应的类型
    /// </summary>
    /// <param name="json"></param>
    /// <typeparam name="T"></typeparam>
    public static T ConvertFromJson<T>(string json)
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"反序列化失败，异常信息：{ex}");
            return default;
        }
    }

    /// <summary>
    ///     计算经纬度距离 单位：米
    /// </summary>
    /// <param name="latA"></param>
    /// <param name="lngA"></param>
    /// <param name="latB"></param>
    /// <param name="lngB"></param>
    public static double GetDistance(double latA, double lngA, double latB, double lngB)
    {
        const int earthRadius = 6378137;
        var dLat1InRad = latA * (Math.PI / 180);
        var dLong1InRad = lngA * (Math.PI / 180);
        var dLat2InRad = latB * (Math.PI / 180);
        var dLong2InRad = lngB * (Math.PI / 180);
        var dLongitude = dLong2InRad - dLong1InRad;
        var dLatitude = dLat2InRad - dLat1InRad;
        var a = Math.Pow(Math.Sin(dLatitude / 2), 2) +
                Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) * Math.Pow(Math.Sin(dLongitude / 2), 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadius * c;
    }

    /// <summary>
    ///     获取枚举的Description特性值
    /// </summary>
    /// <param name="value">枚举</param>
    public static string GetNameByDescription(Enum value)
    {
        var enumType = value.GetType();
        var field = enumType.GetField(value.ToString());
        if (field == null) return value.ToString();
        if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is not DescriptionAttribute attribute)
            return value.ToString();
        return attribute.Description;
    }

    /// <summary>
    ///     返回两个浮点数是否相等
    /// </summary>
    /// <param name="number1"></param>
    /// <param name="number2"></param>
    public static bool IsNumberEquals(double number1, double number2)
    {
        const double epsilon = 1.0E-7d;
        return Math.Abs(number1 - number2) <= epsilon;
    }

    /// <summary>
    ///     将数组转为支持中文的字符串
    /// </summary>
    /// <param name="data"></param>
    public static string GetGb2312String(byte[] data)
    {
        try
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return Encoding.GetEncoding("gb2312").GetString(data);
        }
        catch
        {
            return string.Empty;
        }
    }

    public static string GetEncodeString(byte[] data, string encode)
    {
        try
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var code = Encoding.GetEncoding(encode);
            return code.GetString(data);
        }
        catch
        {
            return string.Empty;
        }
    }

    public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue value)
    {
        if (dic == null) return;
        dic[key] = value;
    }

    /// <summary>
    ///     根据ITU的参数名获取相关的名称与单位
    /// </summary>
    /// <param name="name"></param>
    public static Tuple<string, string> ConvertNameAndUnit(string name)
    {
        return name switch
        {
            ParameterNames.ItuFrequency => new Tuple<string, string>("中心频率", "MHz"),
            ParameterNames.ItuBeta => new Tuple<string, string>("β带宽", "kHz"),
            ParameterNames.ItuXdb => new Tuple<string, string>("X dB带宽", "kHz"),
            ParameterNames.ItuFmDev => new Tuple<string, string>("FM频偏", "kHz"),
            ParameterNames.ItuFmDevPos => new Tuple<string, string>("FM正频偏", "kHz"),
            ParameterNames.ItuFmDevNeg => new Tuple<string, string>("FM负频偏", "kHz"),
            ParameterNames.ItuAmDepth => new Tuple<string, string>("AM调幅度", "%"),
            ParameterNames.ItuAmDepthPos => new Tuple<string, string>("AM正调幅度", "%"),
            ParameterNames.ItuAmDepthNeg => new Tuple<string, string>("AM负调幅度", "%"),
            ParameterNames.ItuLevel => new Tuple<string, string>("电平", "dBμV"),
            ParameterNames.ItuStrength => new Tuple<string, string>("场强", "dBμV/m"),
            ParameterNames.ItuPmDepth => new Tuple<string, string>("PM调制度", "rad"),
            _ => new Tuple<string, string>("", "")
        };
    }
}