using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using StreamJsonRpc;

namespace CCC;

public class JsonRpcMiddleware
{
    /// <summary>
    ///     创建链接
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="n"></param>
    /// <returns>Task</returns>
    private static async Task AcceptorAsync(HttpContext httpContext, Func<Task> n)
    {
        //Trace.WriteLine(httpContext.Request.Headers);
        if (!httpContext.WebSockets.IsWebSocketRequest) return;
        // 解析连接信息并缓存
        // WebSocket连接
        var socket = await httpContext.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
        // 远程IP地址
        var ip = httpContext.Connection.RemoteIpAddress?.ToString();
        // 远程端口
        var port = httpContext.Connection.RemotePort;
        // 本地IP地址
        var localIp = httpContext.Connection.LocalIpAddress?.ToString();
        // 本地端口
        var localPort = httpContext.Connection.LocalPort;
        // 连接ID
        var sessionId = httpContext.Connection.Id;
        // client访问的map
        string map = httpContext.Request.PathBase;
        // 解析任务ID（如果有）
        var taskId = httpContext.Request.Path.HasValue ? httpContext.Request.Path.Value?[1..] : "";
        // 创建连接信息类
        var client = new ClientInfo(localIp, localPort)
        {
            IpAddress = ip,
            Port = port,
            Map = map,
            SessionId = sessionId,
            TaskId = taskId,
            Socket = socket,
            ActiveTime = DateTime.Now
        };
        var server = Server.UpdateServer(localPort, client);
        if (server == null) return;
        if (!server.ServerMap.ContainsKey(map)) return;
        Trace.WriteLine(
            $"{DateTime.Now:HH:mm:ss.fff}    New Connect:{ip}:{port},SessionId:{sessionId},Map:{map},TaskId:{taskId}");
        var formatter = Utils.CreateFormatterInstance(server.FormatterType, client);
        var handler = new WebSocketMessageHandler(socket, formatter);
        // 创建JsonRpc的实例
        using var jsonRpc = new JsonRpc(handler);
        // 设置最小ID为255
        //jsonRpc.SetMaxServerId(0);
        jsonRpc.SynchronizationContext = null;
        object instance;
        if (server.IsSingle)
        {
            instance = server.ServerInstanceMap[map];
        }
        else
        {
            var serverType = server.ServerMap[map];
            instance = Utils.CreateServerInstance(serverType, client);
        }

        if (instance == null)
        {
            Server.ReleaseClient(localPort, sessionId);
            return;
        }

        client.Instance = instance as IDisposable;
        // 将实例化的服务承载到StreamJsonRpc上
        jsonRpc.AddLocalRpcTarget(instance);
        jsonRpc.StartListening();
        client.RpcServer = jsonRpc;
        try
        {
            // 阻塞线程，监听连接断开
            await jsonRpc.Completion.ConfigureAwait(false);
        }
        catch
        {
            // catch到错误也代表连接断开了
        }

        // 连接断开以后需要清理连接
        Server.ReleaseClient(localPort, sessionId);
    }

    /// <summary>
    ///     路由绑定处理
    /// </summary>
    /// <param name="app"></param>
    public static void Map(IApplicationBuilder app)
    {
        app.UseWebSockets();
        app.Use(AcceptorAsync);
    }
}