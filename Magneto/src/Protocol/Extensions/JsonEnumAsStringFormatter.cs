// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="JsonEnumAsStringFormatter.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using MessagePack;
using Newtonsoft.Json;

namespace Magneto.Protocol.Extensions;

/// <summary>
///     Json自定义枚举序列化类
/// </summary>
/// <typeparam name="T"></typeparam>
public class JsonEnumAsStringFormatter<T> : JsonConverter<T> where T : Enum
{
    /// <summary>
    ///     The int values
    /// </summary>
    private readonly List<long> _intValues = new();

    /// <summary>
    ///     The is has flag
    /// </summary>
    private readonly bool _isHasFlag;

    /// <summary>
    ///     The names
    /// </summary>
    private readonly List<string> _names = new();

    ///// <summary>
    /////     The values
    ///// </summary>
    //private readonly List<T> _values = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="JsonEnumAsStringFormatter{T}" /> class.
    /// </summary>
    public JsonEnumAsStringFormatter()
    {
        var type = typeof(T);
        var attr = Attribute.GetCustomAttribute(type, typeof(FlagsAttribute));
        if (attr != null) _isHasFlag = true;
        foreach (T tValue in Enum.GetValues(typeof(T)))
        {
            var field = tValue.GetType().GetField(tValue.ToString());
            if (field != null &&
                Attribute.GetCustomAttribute(field, typeof(KeyAttribute)) is KeyAttribute { IntKey: null } key)
                _names.Add(key.StringKey);
            else
                _names.Add(tValue.ToString());
            _intValues.Add(Convert.ToInt64(tValue));
        }
    }

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
        if (_isHasFlag)
        {
            try
            {
                if (reader.TokenType == JsonToken.StartArray)
                {
                    long num = 0;
                    var str = reader.ReadAsString();
                    while (reader.TokenType == JsonToken.String)
                    {
                        var index = _names.IndexOf(str);
                        if (index >= 0)
                        {
                            var value = _intValues[index];
                            num |= value;
                        }

                        str = reader.ReadAsString();
                    }

                    return (T)Enum.ToObject(objectType, num);
                }

                if (reader.TokenType == JsonToken.String)
                {
                    var str = reader.Value?.ToString();
                    var index = _names.IndexOf(str);
                    if (index >= 0)
                    {
                        var value = _intValues[index];
                        return (T)Enum.ToObject(objectType, value);
                    }
                }
            }
            catch
            {
                return default;
            }
        }
        else
        {
            var str = reader.Value?.ToString();
            var index = _names.IndexOf(str);
            if (index >= 0)
            {
                var value = _intValues[index];
                return (T)Enum.ToObject(objectType, value);
            }
        }

        return default;
    }

    /// <summary>
    ///     Writes the json.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The serializer.</param>
    public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
    {
        var num = Convert.ToInt64(value);
        if (_isHasFlag)
        {
            writer.WriteStartArray();
            foreach (var enumValue in Enum.GetValues(value.GetType()))
            {
                var enumValueTemp = Convert.ToInt64(enumValue);
                if ((enumValueTemp & num) > 0)
                {
                    var index = _intValues.IndexOf(enumValueTemp);
                    if (index >= 0)
                    {
                        var str = _names[index];
                        writer.WriteValue(str);
                    }
                }
            }

            writer.WriteEndArray();
        }
        else
        {
            var index = _intValues.IndexOf(num);
            if (index >= 0)
            {
                var str = _names[index];
                writer.WriteValue(str);
            }
        }
    }
}