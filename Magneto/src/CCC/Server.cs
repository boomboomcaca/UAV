using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CCC;

public static class Server
{
    private static readonly ConcurrentDictionary<int, ServerInfo> _serverDic = new();
    public static int Port { get; set; }
    public static List<string> Maps => _serverDic.First().Value.ServerMap.Keys.ToList();

    public static WebApplicationBuilder CreateHostBuilder(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Warning);
        builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
        builder.WebHost.UseUrls($"http://0.0.0.0:{RunningInfo.Port}");
        builder.Services.AddControllers();
        builder.Services.AddLogging();
        builder.Services.AddEndpointsApiExplorer();
        //前端资源在Magneto.dll中，需要加载
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var externalDllPath = Path.Combine(baseDir, "Magneto.dll");
        var assembly = Assembly.LoadFrom(externalDllPath);
        builder.Services.AddMvc()
            .AddApplicationPart(assembly)
            .AddRazorPagesOptions(options => { options.RootDirectory = "/Pages"; });

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>
            {
                //登录路径：这是当用户试图访问资源但未经过身份验证时，程序将会将请求重定向到这个相对路径
                o.LoginPath = new PathString("/Login");
                //禁止访问路径：当用户试图访问资源时，但未通过该资源的任何授权策略，请求将被重定向到这个相对路径。
                o.AccessDeniedPath = new PathString("/Login");
            });
        builder.Services.AddCors(options =>
        {
            // 1.解决前端访问本地的视频源时报错"No 'Access-Control-Allow-Origin'"的问题
            options.AddPolicy("any", builderSetting =>
            {
                //builder.AllowAnyOrigin(); //允许任何来源的主机访问
                builderSetting
                    .WithOrigins("http://*.*.*.*")
                    //.SetIsOriginAllowedToAllowWildcardSubdomains()//设置允许访问的域
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowAnyOrigin();
                // .AllowCredentials();//
            });
        });
        builder.Services.AddSwaggerGen();
        var curPath = Environment.CurrentDirectory;
        var basePath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
        var isRunWinService = curPath != basePath;
        if (isRunWinService && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Trace.WriteLine("当前运行在windows服务模式");
            builder.Host.UseWindowsService();
        }
        else
        {
            Trace.WriteLine("当前运行在普通模式");
        }

        return builder;
    }

    /// <summary>
    ///     绑定外部服务
    ///     如果要启动服务请自行调用host的启动命令
    /// </summary>
    /// <param name="host"></param>
    /// <param name="ipAddr"></param>
    /// <param name="port"></param>
    /// <param name="formatterType"></param>
    /// <param name="isSingle"></param>
    public static void CreateJsonRpcServer(WebApplication host, string ipAddr, int port, FormatterType formatterType,
        bool isSingle = false)
    {
        var server = new ServerInfo
        {
            IpAddress = ipAddr,
            Port = port,
            App = host,
            FormatterType = Utils.GetFormatter(formatterType),
            IsSingle = isSingle
        };
        Port = port;
        _serverDic.TryAdd(port, server);
    }

    /// <summary>
    ///     阻塞运行服务
    /// </summary>
    /// <param name="port"></param>
    public static void RunServer(int port)
    {
        if (!_serverDic.TryGetValue(port, out var server)) return;
        server.App.UseSwagger();
        server.App.UseSwaggerUI();
        // 2.解决前端访问本地的视频源时报错"No 'Access-Control-Allow-Origin'"的问题
        server.App.UseCors("any");
        // app.UseMiddleware<CorsMiddleware>();
        // if (env.IsDevelopment())
        server.App.UseDeveloperExceptionPage();
        // else
        // {
        //     app.UseExceptionHandler("/Error");
        //     // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        //     app.UseHsts();
        // }
        //#if DEBUG
        //        if (!Debugger.IsAttached) server.App.UseHttpsRedirection();
        //#else
        //            server.App.UseHttpsRedirection();
        //#endif
        server.App.UseStaticFiles();
        var path = RunningInfo.VideoDir;
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        var provider = new FileExtensionContentTypeProvider
        {
            Mappings =
            {
                [".m3u8"] = "application/x-mpegURL", //m3u8的MIME
                [".ts"] = "video/MP2TL" //.ts的MIME
            }
        };
        server.App.UseDirectoryBrowser(new DirectoryBrowserOptions
        {
            FileProvider = new PhysicalFileProvider(path),
            RequestPath = $"/{PublicDefine.PathVideo}"
        });
        var savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);
        server.App.UseDirectoryBrowser(new DirectoryBrowserOptions
        {
            FileProvider = new PhysicalFileProvider(savePath),
            RequestPath = "/saveData"
        });
        server.App.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(path),
            ContentTypeProvider = provider,
            RequestPath = new PathString($"/{PublicDefine.PathVideo}")
        });
        server.App.UseRouting();
        server.App.UseAuthorization();
        server.App.MapControllers();
        server.App.MapRazorPages();
        var maps = Maps;
        maps.ForEach(m => server.App.Map(m, JsonRpcMiddleware.Map));
        server.App.Run();
    }

    /// <summary>
    ///     异步运行服务
    /// </summary>
    /// <param name="port"></param>
    public static Task RunServerAsync(int port)
    {
        if (!_serverDic.TryGetValue(port, out var server)) return Task.CompletedTask;
        return server.App.RunAsync();
    }

    /// <summary>
    ///     设置服务映射
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="port">开启的WebSoc服务端口</param>
    /// <param name="map">服务要映射的map</param>
    public static void MapServer<T>(int port, string map) where T : IDisposable
    {
        // if (map != Define.MAP_TASKS && map != Define.MAP_CONTROL)
        // {
        //     throw new Exception("map格式不对！只能为\"/task\"、\"/control\"中的一个");
        // }
        if (!_serverDic.TryGetValue(port, out var server)) return;
        if (server.ServerMap.ContainsKey(map))
        {
            server.ServerMap.Remove(map);
            if (server.IsSingle)
            {
                (server.ServerInstanceMap[map] as IDisposable)?.Dispose();
                server.ServerInstanceMap.Remove(map);
            }
        }

        var type = typeof(T);
        server.ServerMap.Add(map, type);
        if (!server.IsSingle) return;
        var instance = Utils.CreateServerInstance(type);
        server.ServerInstanceMap.Add(map, instance);
    }

    /// <summary>
    ///     关闭服务
    /// </summary>
    /// <param name="port"></param>
    public static async Task CloseServerAsync(int port)
    {
        if (!_serverDic.TryGetValue(port, out var server)) return;
        //server.Host.WaitForShutdown();
        await server.App.DisposeAsync();
    }

    /// <summary>
    ///     关闭服务下的所有连接
    /// </summary>
    /// <param name="serverPort"></param>
    public static async Task CloseClientAsync(int serverPort)
    {
        if (!_serverDic.TryGetValue(serverPort, out var server)) return;
        foreach (var pair in server.ClientDic)
        {
            server.ClientDic.TryRemove(pair.Key, out var client);
            if (client == null) continue;
            await client.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None)
                .ConfigureAwait(false);
            client.Socket.Abort();
        }
    }

    /// <summary>
    ///     根据TaskId获取连接信息
    /// </summary>
    /// <param name="serverPort"></param>
    /// <param name="taskId"></param>
    public static ClientInfo GetClient(int serverPort, string taskId)
    {
        if (!_serverDic.TryGetValue(serverPort, out var server)) return null;
        foreach (var pair in server.ClientDic)
            if (pair.Value.TaskId.Equals(taskId))
                return pair.Value;
        return null;
    }

    /// <summary>
    ///     关闭连接
    /// </summary>
    /// <param name="serverPort"></param>
    /// <param name="sessionId"></param>
    public static void ReleaseClient(int serverPort, string sessionId)
    {
        if (!_serverDic.TryGetValue(serverPort, out var server)) return;
        //string key = $"{clientIp}:{clientPort}";
        var key = sessionId;
        if (!server.ClientDic.TryRemove(key, out var client)) return;
        try
        {
            Trace.WriteLine(
                $"{DateTime.Now:HH:mm:ss.fff}    关闭连接:{client.SessionId},{client.IpAddress}:{client.Port}，当前连接数:{server.ClientDic.Count}");
            var cancellation = CancellationToken.None;
            // 异步关闭,不等待关闭结果，否则如果前端阻塞可能会造成一直在这里等待
            _ = Task.Run(() =>
                client.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "服务端关闭连接", cancellation)
                    .ConfigureAwait(false), cancellation);
            client.Socket.Abort();
            if (!server.IsSingle)
                // 如果服务不是单实例的，则在连接断开的时候需要将服务也回收
                client.Instance?.Dispose();
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"{DateTime.Now:HH:mm:ss.fff}    关闭连接{client.SessionId}失败:{ex.Message}");
        }
    }

    /// <summary>
    ///     有新连接到来的时候更新服务，将新的连接加入相应服务的集合中
    /// </summary>
    /// <param name="port">服务端口号</param>
    /// <param name="client">新增的连接</param>
    internal static ServerInfo UpdateServer(int port, ClientInfo client)
    {
        if (!_serverDic.TryGetValue(port, out var info)) return null;
        //string key = $"{client.IpAddress}:{client.Port}";
        var key = client.SessionId;
        info.ClientDic.AddOrUpdate(key, client, (_, _) => client);
        Trace.WriteLine(
            $"{DateTime.Now:HH:mm:ss.fff}    新连接:{client.SessionId},{client.IpAddress}:{client.Port}，当前连接数:{info.ClientDic.Count}");
        return info;
    }

    /// <summary>
    ///     获取当前的连接信息
    /// </summary>
    public static List<ClientInfo> GetClients()
    {
        return _serverDic?.FirstOrDefault().Value?.ClientDic?.Values.Select(i => i.Clone()).ToList();
    }
}