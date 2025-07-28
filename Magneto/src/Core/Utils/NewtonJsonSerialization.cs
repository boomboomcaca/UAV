using Newtonsoft.Json;

namespace Core.Utils;

/// <summary>
///     使用Netonsoft.Json进行json的序列化与反序列化
/// </summary>
public static class NewtonJsonSerialization
{
    public static string Serialize(object value, bool isIndented)
    {
        var formatting = isIndented ? Formatting.Indented : Formatting.None;
        return JsonConvert.SerializeObject(value, formatting);
    }

    public static T Deserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }
}