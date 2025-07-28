// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="JsonParameterListConverter.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using Magneto.Protocol.Define;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Magneto.Protocol.Extensions;

/// <summary>
///     Json子参数集合自定义序列化类
/// </summary>
/// <typeparam name="T"></typeparam>
public class JsonParameterListConverter<T> : JsonConverter<T> where T : List<object>
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
        var list = new List<object>();
        while (reader.TokenType == JsonToken.StartArray)
        {
            _ = reader.Read();
            while (reader.TokenType == JsonToken.StartArray)
            {
                var parameters = new List<Parameter>();
                _ = reader.Read();
                while (reader.TokenType == JsonToken.StartObject)
                {
                    var parameter = serializer.Deserialize<Parameter>(reader);
                    parameters.Add(parameter);
                    _ = reader.Read();
                }

                list.Add(parameters);
                if (reader.TokenType == JsonToken.EndArray)
                {
                    _ = reader.Read();
                    break;
                }
            }

            while (reader.TokenType == JsonToken.StartObject)
            {
                //Parameter parameter = serializer.Deserialize<Parameter>(reader);
                var dic = serializer.Deserialize<Dictionary<string, object>>(reader);
                var temp = new Dictionary<string, object>();
                foreach (var pair in dic)
                    if (pair.Value is JArray array)
                    {
                        var objArr = array.ToObject<object[]>();
                        temp.Add(pair.Key, objArr);
                    }

                if (temp.Count > 0)
                    foreach (var pair in temp)
                        if (dic.ContainsKey(pair.Key))
                            dic[pair.Key] = pair.Value;
                list.Add(dic);
                _ = reader.Read();
            }

            if (reader.TokenType == JsonToken.EndArray) return (T)list;
        }

        return (T)list;
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
        // return;
        // if (value == null)
        // {
        //     writer.WriteNull();
        //     return;
        // }
        // List<object> list = value as List<object>;
        // writer.WriteStartArray();
        // foreach (var obj in list)
        // {
        //     if (obj is Parameter)
        //     {
        //         serializer.Serialize(writer, obj, typeof(Parameter));
        //     }
        //     else if (obj is List<Parameter> parameters)
        //     {
        //         writer.WriteStartArray();
        //         foreach (Parameter para in parameters)
        //         {
        //             serializer.Serialize(writer, para, typeof(Parameter));
        //         }
        //         writer.WriteEndArray();
        //     }
        //     else if (obj is Dictionary<string, object>)
        //     {
        //         // TODO: test!
        //         serializer.Serialize(writer, obj, typeof(Dictionary<string, object>));
        //     }
        // }
        // writer.WriteEndArray();
    }
}