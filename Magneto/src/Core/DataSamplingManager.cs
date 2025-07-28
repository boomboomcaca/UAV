using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Magneto.Protocol.Define;

namespace Core;

internal class DataSamplingManager
{
    private static readonly Dictionary<SDataType, (bool sendDirectly, bool needSampling, bool canDrop)> _map;

    static DataSamplingManager()
    {
        _map = new Dictionary<SDataType, (bool sendDirectly, bool needSampling, bool canDrop)>();
        _map.Clear();
        var type = typeof(SDataType);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
        if (fields.Any() != true) return;
        foreach (var field in fields)
        {
            var name = field.Name;
            if (!Enum.TryParse<SDataType>(name, out var value)) continue;
            try
            {
                var attribute = field.GetCustomAttribute<DataTypeAttribute>();
                _map.Add(value,
                    attribute == null
                        ? (false, false, true)
                        : (attribute.SendDirectly, attribute.NeedSampling, attribute.CanDrop));
            }
            catch
            {
                // ignored
            }
        }
    }

    public static (bool sendDirectly, bool needSampling, bool canDrop) Get(SDataType dataType)
    {
        if (_map.TryGetValue(dataType, out var value))
        {
            var (sendDirectly, needSampling, canDrop) = value;
            return (sendDirectly, needSampling, canDrop);
        }

        return (false, false, true);
    }
}