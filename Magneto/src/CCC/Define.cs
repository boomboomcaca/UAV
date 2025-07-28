using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Builder;
using StreamJsonRpc;

namespace CCC;

/// <summary>
///     连接信息
/// </summary>
public class ClientInfo
{
    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="serverIp"></param>
    /// <param name="serverPort"></param>
    public ClientInfo(string serverIp, int serverPort)
    {
        ServerIp = serverIp;
        ServerPort = serverPort;
    }

    /// <summary>
    ///     连接访问的服务的IP地址
    /// </summary>
    public string ServerIp { get; set; }

    /// <summary>
    ///     连接访问的服务的端口号
    /// </summary>
    public int ServerPort { get; }

    /// <summary>
    ///     连接的IP地址
    /// </summary>
    public string IpAddress { get; set; }

    /// <summary>
    ///     连接的远程端口号
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    ///     连接的SessionId
    /// </summary>
    public string SessionId { get; set; }

    /// <summary>
    ///     此连接访问的服务Map
    /// </summary>
    public string Map { get; set; }

    /// <summary>
    ///     连接启动的任务ID
    /// </summary>
    public string TaskId { get; set; }

    /// <summary>
    ///     清除数据标记
    /// </summary>
    public bool ClearDataSign { get; set; }

    /// <summary>
    ///     是否观察者模式
    /// </summary>
    public bool IsObserver { get; set; }

    /// <summary>
    ///     连接的WebSocket实例
    /// </summary>
    public WebSocket Socket { get; set; }

    /// <summary>
    ///     服务实例
    /// </summary>
    public IDisposable Instance { get; set; }

    /// <summary>
    ///     RPC服务实例
    /// </summary>
    public JsonRpc RpcServer { get; set; }

    /// <summary>
    ///     最近的活跃时间
    /// </summary>
    public DateTime ActiveTime { get; set; }

    public ClientInfo Clone()
    {
        return MemberwiseClone() as ClientInfo;
    }
}

/// <summary>
///     服务信息类
/// </summary>
public class ServerInfo
{
    /// <summary>
    ///     服务的IP地址
    /// </summary>
    public string IpAddress { get; set; }

    /// <summary>
    ///     服务开启的端口号
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    ///     承载服务的实例
    /// </summary>
    public WebApplication App { get; set; }

    /// <summary>
    ///     序列化器的类型
    /// </summary>
    public Type FormatterType { get; set; }

    /// <summary>
    ///     是否是单实例服务
    /// </summary>
    public bool IsSingle { get; set; }

    /// <summary>
    ///     服务路径-服务类型对照表
    /// </summary>
    public Dictionary<string, Type> ServerMap { get; set; } = new();

    /// <summary>
    ///     服务路径-服务实例对照表
    /// </summary>
    public Dictionary<string, object> ServerInstanceMap { get; set; } = new();

    /// <summary>
    ///     本服务的连接集合
    /// </summary>
    public ConcurrentDictionary<string, ClientInfo> ClientDic { get; set; } = new();
}