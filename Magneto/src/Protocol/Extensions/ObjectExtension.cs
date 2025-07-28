// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="ObjectExtension.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using MessagePack;

namespace Magneto.Protocol.Extensions;

/// <summary>
///     Class ObjectExtension.
/// </summary>
public static class ObjectExtension
{
    /// <summary>
    ///     将类型转换为字典集合的扩展方法
    /// </summary>
    /// <param name="obj">The object.</param>
    /// <returns>Dictionary&lt;System.String, System.Object&gt;.</returns>
    public static Dictionary<string, object> ToDictionary(this object obj)
    {
        var dic = new Dictionary<string, object>();
        try
        {
            var type = obj.GetType();
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var name = property.Name;
                if (Attribute.GetCustomAttribute(property, typeof(KeyAttribute)) is KeyAttribute
                    {
                        IntKey: null
                    } attribute) name = attribute.StringKey;
                var value = property.GetValue(obj);
                if (value == null) continue;
                if (name != null)
                    dic.Add(name, value);
            }

            return dic;
        }
        catch
        {
            return null;
        }
    }
}