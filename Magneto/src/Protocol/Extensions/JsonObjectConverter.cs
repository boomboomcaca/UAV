// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="JsonObjectConverter.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using Newtonsoft.Json;

namespace Magneto.Protocol.Extensions;

/// <summary>
///     参数模板序列化器
///     原生的解析会将数组解析为JArray，因此需要写自定义解析将Array解析为object[]
/// </summary>
/// <typeparam name="T"></typeparam>
public class JsonObjectConverter<T> : JsonConverter<T>
{
    /// <summary>
    ///     Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
    /// <param name="objectType">Type of the object.</param>
    /// <param name="existingValue">
    ///     The existing value of object being read. If there is no existing value then <c>null</c>
    ///     will be used.
    /// </param>
    /// <param name="hasExistingValue">The existing value has a value.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <returns>The object value.</returns>
    public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        try
        {
            var value = reader.Value;
            if (reader.ValueType == typeof(string))
            {
                var json = value?.ToString();
                if (json != null) return JsonConvert.DeserializeObject<T>(json);
            }

            return serializer.Deserialize<T>(reader);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    ///     Writes the json.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The serializer.</param>
    public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value, typeof(T));
    }
}