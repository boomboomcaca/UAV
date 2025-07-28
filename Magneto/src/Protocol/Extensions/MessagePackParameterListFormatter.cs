// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="MessagePackParameterListFormatter.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Collections.Generic;
using Magneto.Protocol.Define;
using MessagePack;
using MessagePack.Formatters;
using Newtonsoft.Json.Linq;

namespace Magneto.Protocol.Extensions;

/// <summary>
///     MessagePack子参数集合自定义序列化类
/// </summary>
/// <typeparam name="T"></typeparam>
public class MessagePackParameterListFormatter<T> : IMessagePackFormatter<T> where T : List<object>
{
    /// <summary>
    ///     Deserializes a value.
    /// </summary>
    /// <param name="reader">The reader to deserialize from.</param>
    /// <param name="options">
    ///     The serialization settings to use, including the resolver to use to obtain formatters for types
    ///     that make up the composite type <typeparamref name="T" />.
    /// </param>
    /// <returns>The deserialized value.</returns>
    public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var count = reader.ReadArrayHeader();
        var list = new List<object>();
        for (var i = 0; i < count; i++)
            //if (reader.TryReadArrayHeader(out int length))
        {
            var dic = MessagePackSerializer.Deserialize<Dictionary<string, object>>(ref reader);
            list.Add(dic);
            // List<Parameter> parameters = new List<Parameter>();
            // for (int j = 0; j < length; j++)
            // {
            //     Parameter parameter = MessagePackSerializer.Deserialize<Parameter>(ref reader);
            //     parameters.Add(parameter);
            // }
            // list.Add(parameters);
        }

        // else
        // {
        //     Parameter parameter = MessagePackSerializer.Deserialize<Parameter>(ref reader);
        //     list.Add(parameter);
        // }
        return (T)list;
    }

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="value">The value.</param>
    /// <param name="options">The options.</param>
    public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options)
    {
        try
        {
            if (value is not List<object> list) return;
            var count = list.Count;
            writer.WriteArrayHeader(count);
            foreach (var para in list)
                if (para is List<Parameter>)
                    MessagePackSerializer.Serialize(typeof(List<Parameter>), ref writer, para);
                else if (para is Parameter)
                    MessagePackSerializer.Serialize(typeof(Parameter), ref writer, para);
                else if (para is JArray)
                    MessagePackSerializer.Serialize(typeof(JArray), ref writer, para);
                else if (para is Dictionary<string, object>)
                    MessagePackSerializer.Serialize(typeof(Dictionary<string, object>), ref writer, para);
        }
        catch
        {
            //
        }
    }
}