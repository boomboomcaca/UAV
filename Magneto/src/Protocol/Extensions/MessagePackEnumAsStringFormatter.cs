// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="MessagePackEnumAsStringFormatter.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;

namespace Magneto.Protocol.Extensions;

/// <summary>
///     MessagePack自定义枚举序列化类
///     1. MessagePack自带的EnumAsStringFormatter将带Flags特性的枚举序列化为形如："A,B,C,D"的字符串，这不符合我们的协议要求
///     因此自己继承了IMessagePackFormatter，将带Flags特性的枚举序列化为形如：["A","B","C","D"]的json数组
///     2. MessagePack自带的EnumAsStringFormatter将普通的枚举序列化时，无法读取KeyAttribute特性的值，
///     我们的需求是将类似 AbcdEfg这样的枚举项序列化为abcdEfg，但是自带的序列化器无法做到
/// </summary>
/// <typeparam name="T"></typeparam>
public class MessagePackEnumAsStringFormatter<T> : IMessagePackFormatter<T> where T : Enum
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

    /// <summary>
    ///     The values
    /// </summary>
    private readonly List<T> _values = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="MessagePackEnumAsStringFormatter{T}" /> class.
    /// </summary>
    public MessagePackEnumAsStringFormatter()
    {
        var type = typeof(T);
        var attr = Attribute.GetCustomAttribute(type, typeof(FlagsAttribute));
        if (attr != null) _isHasFlag = true;
        foreach (T tValue in Enum.GetValues(typeof(T)))
        {
            _values.Add(tValue);
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
        if (_isHasFlag)
        {
            if (reader.NextMessagePackType == MessagePackType.Array)
            {
                long num = 0;
                var count = reader.ReadArrayHeader();
                for (var i = 0; i < count; i++)
                {
                    var name = reader.ReadString();
                    var index = _names.IndexOf(name);
                    if (index < 0) continue;
                    var tValue = _values[index];
                    num |= Convert.ToInt64(tValue);
                }

                var value = (T)Enum.ToObject(typeof(T), num);
                return value;
            }

            if (reader.NextMessagePackType == MessagePackType.String)
            {
                var name = reader.ReadString();
                var index = _names.IndexOf(name);
                var tValue = _values[index];
                return tValue;
            }

            return default;
        }

        {
            var name = reader.ReadString();
            var index = _names.IndexOf(name);
            var value = _values[index];
            return value;
        }
    }

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="value">The value.</param>
    /// <param name="options">The options.</param>
    public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options)
    {
        if (_isHasFlag)
        {
            var list = new List<string>();
            var numEnum = Convert.ToInt64(value);
            for (var i = 0; i < _intValues.Count; i++)
            {
                var num = _intValues[i];
                if ((numEnum & num) > 0)
                {
                    var str = _names[i];
                    list.Add(str);
                }
            }

            writer.WriteArrayHeader(list.Count);
            foreach (var str in list) writer.Write(str);
        }
        else
        {
            //writer.Write(value.ToString());
            var num = Convert.ToInt64(value);
            var index = _intValues.IndexOf(num);
            if (index >= 0)
            {
                var str = _names[index];
                writer.Write(str);
            }
        }
    }
}