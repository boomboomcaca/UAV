using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using MessagePack;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using StreamJsonRpc;
using StreamJsonRpc.Protocol;

namespace CCC;

public static class Utils
{
    #region public

    /// <summary>
    ///     获取随机端口号
    /// </summary>
    public static int GetRandomPort()
    {
        var random = new Random();
        var randomPort = random.Next(10000, 65535);
        while (IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Any(p => p.Port == randomPort))
            randomPort = random.Next(10000, 65535);
        return randomPort;
    }

    #endregion

    #region internal

    internal class CustomMessagePackFormatter : MessagePackFormatter, IJsonRpcMessageFormatter
    {
        public new JsonRpcMessage Deserialize(ReadOnlySequence<byte> contentBuffer)
        {
            var str = MessagePackSerializer.ConvertToJson(contentBuffer.ToArray());
            if (!str.Contains("beat")
                && !str.Contains("\"id\":null")
                && !str.Contains("\"id\":0")
                && !str.Contains("edge.queryAllinfo"))
                Trace.WriteLine($"{DateTime.Now:HH:mm:sss.fff}    命令请求:{str}");
            return base.Deserialize(contentBuffer);
        }

        public new void Serialize(IBufferWriter<byte> bufferWriter, JsonRpcMessage message)
        {
            base.Serialize(bufferWriter, message);
        }
    }

    internal static object CreateServerInstance(Type serverType, params object[] args)
    {
        var parameters = args.Select(i => i.GetType()).ToArray();
        var p = serverType.GetConstructor(parameters);
        try
        {
            if (p != null) return Activator.CreateInstance(serverType, args);
            return Activator.CreateInstance(serverType);
        }
        catch
        {
            return null;
        }
    }

    internal static IJsonRpcMessageFormatter CreateFormatterInstance(Type formatterType, params object[] args)
    {
        var parameters = args.Select(i => i.GetType()).ToArray();
        var p = formatterType.GetConstructor(parameters);
        if (p != null) return (IJsonRpcMessageFormatter)Activator.CreateInstance(formatterType, args);
        return (IJsonRpcMessageFormatter)Activator.CreateInstance(formatterType);
    }

    internal static Type GetFormatter(FormatterType formatterType)
    {
        Type type = null;
        switch (formatterType)
        {
            case FormatterType.JsonMessageFormatter:
                type = typeof(JsonMessageFormatter);
                break;
            case FormatterType.MessagePackFormatter:
                type = typeof(MessagePackFormatter);
                break;
            case FormatterType.CustomMessagePackFormatter:
                type = typeof(CustomMessagePackFormatter);
                break;
        }

        return type;
    }

    internal static IHostBuilder CreateHostBuilder<T>(string ipAddr, int port) where T : class
    {
        var host = Host.CreateDefaultBuilder(null)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                var url = $"http://{ipAddr}:{port}";
                webBuilder.UseUrls(url);
                webBuilder.UseStartup<T>();
                webBuilder.UseKestrel();
            });
        var curPath = Environment.CurrentDirectory;
        var basePath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
        var isRunWinService = curPath != basePath;
        if (isRunWinService && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Trace.WriteLine("当前运行在windows服务模式");
            host.UseWindowsService();
        }
        else
        {
            Trace.WriteLine("当前运行在普通模式");
        }

        return host;
    }

    #endregion
}