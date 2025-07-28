using System;

namespace Core.Utils;

/// <summary>
///     模块实例化类
/// </summary>
public static class ModuleFactory
{
    /// <summary>
    ///     实例化模块
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="args"></param>
    public static T CreateInstance<T>(params object[] args) where T : class
    {
        var type = typeof(T);
        try
        {
            var obj = Activator.CreateInstance(type, args);
            return (T)obj;
        }
        catch
        {
            return null;
        }
    }

    public static T CreateInstance<T>(Type type, params object[] args) where T : class
    {
        try
        {
            var obj = Activator.CreateInstance(type, args);
            return (T)obj;
        }
        catch
        {
            return null;
        }
    }
}