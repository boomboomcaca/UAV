using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Magneto.Protocol.Data;

namespace Magneto.Contract;

public static partial class Utils
{
    private const int IndexSize = sizeof(long) + sizeof(ushort) + sizeof(int) + sizeof(ulong);

    /// <summary>
    ///     取消任务并释放资源
    /// </summary>
    /// <param name="task"></param>
    /// <param name="tokenSource"></param>
    public static void CancelTask(Task task, CancellationTokenSource tokenSource)
    {
        if (task == null || tokenSource == null) return;
        try
        {
            if (!task.IsCompleted) tokenSource.Cancel(true);
        }
        catch (AggregateException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        finally
        {
            tokenSource.Dispose();
        }
    }

    /// <summary>
    ///     关闭并释放Socket
    /// </summary>
    /// <param name="socket"></param>
    public static void CloseSocket(Socket socket)
    {
        try
        {
            if (socket == null) return;
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
        catch (SocketException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        finally
        {
            socket?.Dispose();
        }
    }

    /// <summary>
    ///     获取对象的真实值
    ///     适用于重写模板类的强制转换方法
    /// </summary>
    /// <param name="propType">类型</param>
    /// <param name="obj">对象</param>
    /// <returns>真实值</returns>
    public static object GetRealValue(Type propType, object obj)
    {
        if (propType.IsEnum) return ConvertStringToEnum(obj?.ToString(), propType);

        if (propType == typeof(Guid))
        {
            var b = Guid.TryParse(obj?.ToString(), out var value);
            if (!b) return Guid.Empty;
            return value;
        }

        if (typeof(IConvertible).IsAssignableFrom(propType))
        {
            var value = Convert.ChangeType(obj, propType);
            return value;
        }

        if (typeof(IList).IsAssignableFrom(propType))
        {
            var elementType = propType.IsArray ? propType.GetElementType() : propType.GetGenericArguments()[0];
            if (obj is not IList source) return null;
            var count = source.Count;
            var enu = source.GetEnumerator();
            using var unknown = enu as IDisposable;
            if (propType.IsArray)
            {
                if (elementType != null)
                {
                    var target = Array.CreateInstance(elementType, count);
                    for (var i = 0; i < count; i++)
                    {
                        enu.MoveNext();
                        target.SetValue(GetRealValue(elementType, enu.Current), i);
                    }

                    return target;
                }
            }
            else
            {
                var target = Activator.CreateInstance(propType);
                var list = target as IList;
                for (var i = 0; i < count; i++)
                {
                    enu.MoveNext();
                    list?.Add(GetRealValue(elementType, enu.Current));
                }

                return target;
            }
        }

        if (typeof(IDictionary).IsAssignableFrom(propType))
        {
            if (obj is not IDictionary source) return null;
            var count = source.Count;
            var enu = source.GetEnumerator();
            using var unknown = enu as IDisposable;
            var types = propType.GetGenericArguments();
            var target = Activator.CreateInstance(propType);
            var dic = target as IDictionary;
            for (var i = 0; i < count; i++)
            {
                enu.MoveNext();
                dic?.Add(GetRealValue(types[0], enu.Key), GetRealValue(types[1], enu.Value));
            }

            return target;
        }

        return obj;
    }

    /// <summary>
    ///     将对象序列化成xml
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="encoding"></param>
    /// <param name="newlineChars"></param>
    /// <param name="omitXmlDeclaration"></param>
    /// <returns>返回byte数组</returns>
    public static byte[] SerializeToXml(object obj, Encoding encoding = null, string newlineChars = "\r\n",
        bool omitXmlDeclaration = true)
    {
        encoding ??= Encoding.ASCII;
        var settings = new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = omitXmlDeclaration,
            NewLineChars = newlineChars,
            Encoding = encoding
        };
        using var ms = new MemoryStream();
        using var xmlWriter = XmlWriter.Create(ms, settings);
        var xmlSerializerNamespaces = new XmlSerializerNamespaces();
        xmlSerializerNamespaces.Add(string.Empty, string.Empty);
        var xs = new XmlSerializer(obj.GetType());
        xs.Serialize(xmlWriter, obj, xmlSerializerNamespaces);
        encoding.GetString(ms.ToArray());
        return ms.ToArray();
    }

    /// <summary>
    ///     将byte数组反序列化成对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    public static object DeserializeFromXml<T>(byte[] data)
    {
        var settings = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreWhitespace = true
        };
        using var ms = new MemoryStream(data);
        using var xmlReader = XmlReader.Create(ms, settings);
        var xs = new XmlSerializer(typeof(T));
        return (T)xs.Deserialize(xmlReader);
    }

    /// <summary>
    ///     解析C/C++库导入
    /// </summary>
    /// <param name="currentAssembly">已为其注册解析程序的程序集</param>
    /// <param name="libRootDirName">库文件文件夹</param>
    /// <param name="libNames">库名称集合</param>
    public static void ResolveDllImport(Assembly currentAssembly, string libRootDirName, string[] libNames)
    {
        NativeLibrary.SetDllImportResolver(currentAssembly, (libraryName, assembly, searchPath) =>
        {
            if (!libNames.Any(p => string.Equals(libraryName, p))) return IntPtr.Zero;
            var applicationRoot = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                switch (RuntimeInformation.ProcessArchitecture)
                {
                    case Architecture.X86:
                        return NativeLibrary.Load(
                            Path.Combine(applicationRoot, $"library/{libRootDirName}/win_x86/{libraryName}"), assembly,
                            searchPath);
                    case Architecture.X64:
                        return NativeLibrary.Load(
                            Path.Combine(applicationRoot, $"library/{libRootDirName}/win_x64/{libraryName}"), assembly,
                            searchPath);
                }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                switch (RuntimeInformation.ProcessArchitecture)
                {
                    case Architecture.X86:
                        return NativeLibrary.Load(
                            Path.Combine(applicationRoot, $"library/{libRootDirName}/linux_x86/{libraryName}"),
                            assembly, searchPath);
                    case Architecture.X64:
                        return NativeLibrary.Load(
                            Path.Combine(applicationRoot, $"library/{libRootDirName}/linux_x64/{libraryName}"),
                            assembly, searchPath);
                    case Architecture.Arm:
                        return NativeLibrary.Load(
                            Path.Combine(applicationRoot, $"library/{libRootDirName}/linux_arm/{libraryName}"),
                            assembly, searchPath);
                    case Architecture.Arm64:
                        return NativeLibrary.Load(
                            Path.Combine(applicationRoot, $"library/{libRootDirName}/linux_arm64/{libraryName}"),
                            assembly, searchPath);
                }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                switch (RuntimeInformation.ProcessArchitecture)
                {
                    case Architecture.X86:
                        return NativeLibrary.Load(
                            Path.Combine(applicationRoot, $"library/{libRootDirName}/osx_x86/{libraryName}"), assembly,
                            searchPath);
                    case Architecture.X64:
                        return NativeLibrary.Load(
                            Path.Combine(applicationRoot, $"library/{libRootDirName}/osx_x64/{libraryName}"), assembly,
                            searchPath);
                }

            return IntPtr.Zero;
        });
    }

    /// <summary>
    ///     原始数据存储时，序列化索引
    /// </summary>
    /// <param name="dataIndex">数据索引</param>
    /// <param name="fileIndex">文件索引</param>
    /// <param name="position">数据位置</param>
    /// <param name="timestamp">时间戳</param>
    public static byte[] CreateIndexBytes(long dataIndex, ushort fileIndex, int position, ulong timestamp)
    {
        var array = new byte[IndexSize];
        var dataIdx = BitConverter.GetBytes(dataIndex);
        var fileIdx = BitConverter.GetBytes(fileIndex);
        var pos = BitConverter.GetBytes(position);
        var ts = BitConverter.GetBytes(timestamp);
        var index = 0;
        Array.Copy(dataIdx, 0, array, index, dataIdx.Length);
        index += dataIdx.Length;
        Array.Copy(fileIdx, 0, array, index, fileIdx.Length);
        index += fileIdx.Length;
        Array.Copy(pos, 0, array, index, pos.Length);
        index += pos.Length;
        Array.Copy(ts, 0, array, index, ts.Length);
        return array;
    }

    /// <summary>
    ///     通用的信号提取算法
    ///     将有信号的频点合并为信号
    /// </summary>
    /// <param name="occupancy">占用度</param>
    /// <param name="snr">信噪比</param>
    /// <param name="startFrequency">起始频率</param>
    /// <param name="stepFrequency">步进</param>
    /// <param name="maxLevels">最大电平值</param>
    /// <param name="occupancyThreshold">占用度统计阈值</param>
    /// <param name="snrThreshold">信噪比阈值</param>
    /// <returns>返回不会为null</returns>
    public static List<SignalsResult> SignalExtract(double[] occupancy,
        float[] snr,
        double startFrequency,
        double stepFrequency,
        short[] maxLevels,
        double occupancyThreshold,
        double snrThreshold)
    {
        List<int> signalBuffer = new();
        for (var i = 0; i < occupancy.Length; i++)
            if (occupancy[i] >= occupancyThreshold && snr[i] >= snrThreshold)
                signalBuffer.Add(i);
        List<SignalsResult> singalUnite = new();
        //对有信号的频点进行排序
        var pointIndex = signalBuffer.ToArray();
        Array.Sort(pointIndex);
        //提取信号
        for (var i = 0; i < pointIndex.Length; i++)
        {
            int freqIndex;
            var s = i;
            var e = i + 1;
            //判断一个信号的起始索引和结束索引(连续超过门限的频点为一个信号)
            while (e < pointIndex.Length && pointIndex[e] - pointIndex[s] == 1)
            {
                s++;
                e++;
            }

            //计算信号中心频率
            var snrMaxIndex = pointIndex[i];
            var occMaxIndex = pointIndex[i];
            for (var v = i; v < e; v++)
            {
                //计算信噪比最高的频率索引
                if (snr[snrMaxIndex] < snr[pointIndex[v]]) snrMaxIndex = pointIndex[v];
                //计算占用度最高的频率索引
                if (occupancy[occMaxIndex] < occupancy[pointIndex[v]]) occMaxIndex = pointIndex[v];
            }

            //如果计算出的占用度最大频点和信噪比最大频点不同，并且两个点的占用度相差 > 1 %，则以占用度大的为中心频率
            if (snrMaxIndex != occMaxIndex && occupancy[occMaxIndex] - occupancy[snrMaxIndex] > 1)
                freqIndex = occMaxIndex;
            else
                freqIndex = snrMaxIndex;
            //计算估测带宽
            var bw = (e - i) * (float)stepFrequency;
            // bool findSkip = false;
            // //公众移动通讯频段不提取
            // if (_skipMobile)
            // {
            //     foreach (KeyValuePair<double, double> mItem in GeneralClass.MobileSegment)
            //     {
            //         if (FrequencyList[freqIndex] >= mItem.Key && FrequencyList[freqIndex] <= mItem.Value)
            //         {
            //             findSkip = true;
            //             break;
            //         }
            //     }
            //     if (findSkip) continue;
            // }
            //生成信号信息
            var freq = Math.Round(startFrequency + freqIndex * stepFrequency / 1000, 4);
            SignalsResult signal = new()
            {
                FrequencyIndex = freqIndex,
                Frequency = freq,
                Bandwidth = bw,
                MaxLevel = maxLevels[freqIndex] / 10f,
                LastTime = GetNowTimestamp(),
                FirstTime = GetNowTimestamp(), //背噪 = 实时值 - 信噪比
                Name = "新信号",
                Result = "新信号",
                Occupancy = occupancy[freqIndex]
            };
            singalUnite.Add(signal);
            i = e - 1;
        }

        return singalUnite;
    }

    /// <summary>
    ///     获取ffmpeg路径
    /// </summary>
    /// <param name="ffmpegPath"></param>
    public static bool GetFFmpegPath(out string ffmpegPath)
    {
        ffmpegPath = "";
        var dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.PathLibrary, "ffmpeg");
        if (!Directory.Exists(dllPath))
        {
            Trace.WriteLine("未找到ffmpeg安装路径！");
            return false;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var path = Path.Combine(dllPath, "linux", "ffmpeg");
            //设置ffmpeg权限
            var ret = ExecuteCmd("chmod", $"+x {path}", AppDomain.CurrentDomain.BaseDirectory);
            if (!ret)
            {
                Trace.WriteLine("设置ffmpeg执行权限失败！");
                return false;
            }

            ffmpegPath = path;
            return true;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            ffmpegPath = Path.Combine(dllPath, "windows", "ffmpeg.exe");
            return true;
        }

        Trace.WriteLine("获取ffmpeg路径失败,不受支持的系统");
        return false;
    }

    public static bool ExecuteCmd(string fileName, string arguments, string workingDirectory = "")
    {
        try
        {
            var p = new Process();
            var noError = true;
            p.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    noError = false;
                    Trace.WriteLine($"ffmpeg编码错误:{e.Data}");
                }
            };
            p.StartInfo.FileName = fileName;
            p.StartInfo.Arguments = arguments;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.WorkingDirectory = workingDirectory;
            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            p.WaitForExit();
            p.Close();
            return noError;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"执行命令错误:{ex.Message}");
            return false;
        }
    }
}