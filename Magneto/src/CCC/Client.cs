using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace CCC;

/// <summary>
///     WebSocket-StreamJsonRpc客户端类
/// </summary>
public class Client : IDisposable
{
    /// <summary>
    ///     WebSocket客户端
    /// </summary>
    private readonly ClientWebSocket _client = new();

    /// <summary>
    ///     构造函数
    /// </summary>
    public Client()
    {
    }

    public WebSocketState State
    {
        get
        {
            if (_client == null) return WebSocketState.None;
            return _client.State;
        }
    }

    public void Dispose()
    {
        _client?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     异步连接
    /// </summary>
    /// <param name="uri">服务端uri地址</param>
    /// <param name="formatterType">序列化类型</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task<ClientWebSocket> ConnectAsync(Uri uri, FormatterType formatterType,
        CancellationToken cancellationToken)
    {
        var type = Utils.GetFormatter(formatterType);
        Utils.CreateFormatterInstance(type);
        await _client.ConnectAsync(uri, cancellationToken).ConfigureAwait(false);
        return _client;
    }

    public void Abort()
    {
        _client?.Abort();
    }

    /// <summary>
    ///     关闭连接
    /// </summary>
    public async Task CloseAsync()
    {
        var cancellationToken = CancellationToken.None;
        await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "客户端关闭连接", cancellationToken)
            .ConfigureAwait(false);
    }

    public void Close()
    {
        var cancellationToken = CancellationToken.None;
        _client?.CloseAsync(WebSocketCloseStatus.NormalClosure, "客户端关闭连接", cancellationToken).ConfigureAwait(false)
            .GetAwaiter().GetResult();
    }
}