using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Magneto.Contract;
using Magneto.Contract.BaseClass;

namespace Core.Utils;

public static class TypesFactory
{
    private static readonly List<Type> _driverTypeCollection = new();
    private static readonly List<Type> _deviceTypeCollection = new();

    /// <summary>
    ///     静态构造函数
    /// </summary>
    static TypesFactory()
    {
        var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.PathDriver);
        foreach (var path in FindFiles(dir))
            if (Path.GetExtension(path) == ".dll")
                try
                {
                    var ass = Assembly.LoadFile(path);
                    foreach (var type in ass.GetTypes())
                        if (IsBaseTypeExists(type, typeof(DriverBase)))
                            _driverTypeCollection.Add(type);
                }
                catch (ReflectionTypeLoadException)
                {
                }
                catch (BadImageFormatException)
                {
                }
                catch (FileLoadException)
                {
                }

        dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.PathDevice);
        foreach (var path in FindFiles(dir))
            if (Path.GetExtension(path) == ".dll")
                try
                {
                    var ass = Assembly.LoadFile(path);
                    foreach (var type in ass.GetTypes())
                        if (IsBaseTypeExists(type, typeof(DeviceBase)))
                            _deviceTypeCollection.Add(type);
                }
                catch (ReflectionTypeLoadException)
                {
                }
                catch (BadImageFormatException)
                {
                }
                catch (FileLoadException)
                {
                }
    }

    public static Type GetDriverType(string className)
    {
        foreach (var type in _driverTypeCollection)
            if (type.FullName == className)
                return type;
        return null;
    }

    public static Type GetDeviceType(string className)
    {
        foreach (var type in _deviceTypeCollection)
            if (type.FullName == className)
                return type;
        return null;
    }

    public static List<Type> GetDeviceTypes()
    {
        return _deviceTypeCollection;
    }

    public static List<Type> GetDriverTypes()
    {
        return _driverTypeCollection;
    }

    /// <summary>
    ///     获取目录下的所有文件，包括子目录
    /// </summary>
    /// <param name="path"></param>
    public static List<string> FindFiles(string path)
    {
        var list = new List<string>();
        if (!Directory.Exists(path)) return list;
        var dirs = Directory.GetDirectories(path);
        if (dirs.Length > 0)
            foreach (var dir in dirs)
            {
                var arr = FindFiles(dir);
                list.AddRange(arr);
            }

        var files = Directory.GetFiles(path);
        list.AddRange(files);
        return list;
    }

    /// <summary>
    ///     查询类型是否继承自某个基类
    /// </summary>
    /// <param name="type">被查询的类型</param>
    /// <param name="baseType">基类的类型</param>
    public static bool IsBaseTypeExists(Type type, Type baseType)
    {
        // 由于类可能会继承多层，因此需要找到最顶层
        var bType = type.BaseType;
        if (bType == baseType) return true;
        while (bType != null)
        {
            bType = bType.BaseType;
            if (bType == baseType) return true;
        }

        return false;
    }
}