using System;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Magneto.Device.Nvt;

internal class WebSocketServer
{
    private bool _isRunning = true;
    private WebSocket _webSocket;

    ~WebSocketServer()
    {
        _isRunning = false;
    }

    private async Task StartWebSocketServerAsync()
    {
        var hostname = Dns.GetHostName();
        var addresses = await Dns.GetHostAddressesAsync(hostname);
        var listener = new HttpListener();
        listener.Prefixes.Add("http://127.0.0.1:10003/");
        Console.WriteLine("WebSocket Server is listening on 127.0.0.1");
        foreach (var address in addresses)
        {
            if (address.AddressFamily != AddressFamily.InterNetwork) continue;
            var uri = $"http://{address}:10003/";
            listener.Prefixes.Add(uri);
            Console.WriteLine("WebSocket Server is listening on " + uri);
        }

        listener.Start();

        while (_isRunning)
        {
            var context = await listener.GetContextAsync();
            if (context.Request.IsWebSocketRequest)
            {
                _ = ProcessWebSocketRequestAsync(context);
            }
            else
            {
                context.Response.StatusCode = 400;
                context.Response.Close();
            }
        }
    }

    private async Task ProcessWebSocketRequestAsync(HttpListenerContext context)
    {
        WebSocketContext webSocketContext;
        try
        {
            webSocketContext = await context.AcceptWebSocketAsync(null);
            Console.WriteLine("WebSocket connected from " + context.Request.RemoteEndPoint);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.Close();
            Console.WriteLine("WebSocket connection failed: " + ex.Message);
            return;
        }

        _webSocket = webSocketContext.WebSocket;
        try
        {
            var buffer = new byte[1024];
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine("WebSocket message received: " + message);

                // 在这里可以处理客户端发送的消息，并向客户端发送响应

                //result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            //await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            Console.WriteLine("WebSocket disconnected from " + context.Request.RemoteEndPoint);
        }
        catch (Exception ex)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, ex.Message, CancellationToken.None);
            Console.WriteLine("WebSocket communication error: " + ex.Message);
        }
    }

    public async Task StartAsync()
    {
        await StartWebSocketServerAsync();
    }

    public void SendData(byte[] data)
    {
        if (_webSocket != null)
            _ = _webSocket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true,
                CancellationToken.None);
    }
}