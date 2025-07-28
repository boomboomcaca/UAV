using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Protocol.Extensions;
using MessagePack;
using Newtonsoft.Json;

namespace Magneto.Contract.Audio;

/// <summary>
///     音频识别类型
/// </summary>
public enum RecogType
{
    /// <summary>
    ///     识别过程中
    /// </summary>
    Middle,

    /// <summary>
    ///     识别完成
    /// </summary>
    Finish,

    /// <summary>
    ///     识别错误
    /// </summary>
    Error,

    /// <summary>
    ///     其他信息
    /// </summary>
    Info
}

public class RecogResultEventArgs
{
    /// <summary>
    ///     本条信息开始解析的时间
    /// </summary>
    public DateTime BeginTime { get; set; }

    /// <summary>
    ///     本条信息解析完毕的时间
    /// </summary>
    public DateTime EndTime { get; set; }

    public RecogType ReusltType { get; set; }
    public string Message { get; set; }
}

public class AudioRecognition
{
    private ClientWebSocket _client;
    private CancellationTokenSource _cts;
    private string _ipAddress = "192.168.102.191";

    /// <summary>
    ///     语音识别服务是否准备好标记
    /// </summary>
    private bool _isRecogServerConnected;

    private int _port = 11011;
    private string _serverKey = "decentest_1234567890";
    private DateTime _startRecogTime = DateTime.Now;
    public event EventHandler<RecogResultEventArgs> AudioRecogResultArrived;

    public void Initialized(string ipAddress, int port, string key)
    {
        _ipAddress = ipAddress;
        _port = port;
        _serverKey = key;
        _cts = new CancellationTokenSource();
        _ = Task.Run(() => RecvRecogAsync(_cts.Token));
        _client = new ClientWebSocket();
        _client.Options.SetRequestHeader("key", _serverKey);
        _ = _client.ConnectAsync(new Uri($"ws://{_ipAddress}:{_port}"), CancellationToken.None).ConfigureAwait(false);
        Trace.WriteLine("开始连接语音识别服务...");
    }

    public void Close()
    {
        _client.Dispose();
        try
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
        catch
        {
            // ignored
        }
        finally
        {
            _cts = null;
        }
    }

    public async Task GetAuidoRecogMessageAsync(byte[] data, CancellationToken token)
    {
        if (_client == null || (_client.State != WebSocketState.Open && _client.State != WebSocketState.Connecting))
        {
            _client?.Dispose();
            _client = new ClientWebSocket();
            _client.Options.SetRequestHeader("key", _serverKey);
            await _client.ConnectAsync(new Uri($"ws://{_ipAddress}:{_port}"), token).ConfigureAwait(false);
        }

        if (!_isRecogServerConnected) return;
        try
        {
            var dic = new Dictionary<string, object>
            {
                { "type", "STREAM" },
                { "data", data.Select(item => (int)item).ToArray() }
            };
            var json = JsonConvert.SerializeObject(dic);
            var arr = Encoding.ASCII.GetBytes(json);
            // Console.WriteLine($"发送数据:{data.Length}");
            await _client.SendAsync(arr, WebSocketMessageType.Text, true, token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"音频识别错误:{ex.Message}");
        }
    }

    private async Task RecvRecogAsync(object obj)
    {
        var token = (CancellationToken)obj;
        var lastTime = DateTime.MinValue;
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(1, token).ConfigureAwait(false);
            if (_client is not { State: WebSocketState.Open })
            {
                await Task.Delay(1, token).ConfigureAwait(false);
                continue;
            }

            try
            {
                var buffer = new byte[1024 * 1024];
                var res = await _client.ReceiveAsync(buffer, CancellationToken.None);
                if (res.MessageType != WebSocketMessageType.Close && res.Count > 0)
                {
                    var str = Encoding.UTF8.GetString(buffer, 0, res.Count);
                    Console.WriteLine($"{DateTime.Now} 解析识别数据:{str}");
                    var reply = JsonConvert.DeserializeObject<AudioRecogResult>(str);
                    if (reply.Code == ResultCode.Connected)
                    {
                        if (!_isRecogServerConnected) Trace.WriteLine("语音识别服务连接成功！");
                        _isRecogServerConnected = true;
                        _startRecogTime = Utils.GetNowTime();
                        Console.WriteLine($"重置起始时间为:{_startRecogTime}");
                        lastTime = DateTime.MinValue;
                    }
                    else if (reply.Code == ResultCode.Failure)
                    {
                        _isRecogServerConnected = false;
                        lastTime = DateTime.MinValue;
                    }
                    else if (reply.Code == ResultCode.Success)
                    {
                        if (lastTime == DateTime.MinValue) lastTime = Utils.GetNowTime();
                        // {"code":"SUCCESS","type":"MID_TEXT","result":"做好出去的杨玉环遇难一年后唐明皇重返重返1700却送回了九负一"}
                        // {"code":"SUCCESS","type":"FIN_TEXT","result":"做好出去的杨玉环遇难，一年后，唐明皇重返重返1700，却送回了酒 。","startTime":0,"endTime":18790}
                        DateTime time;
                        if (reply.Type == ResultType.Final)
                        {
                            // _startRecogTime = DateTime.Now.AddMilliseconds(-reply.EndTime);
                            lastTime = _startRecogTime.AddMilliseconds(reply.EndTime);
                            time = _startRecogTime.AddMilliseconds(reply.StartTime);
                        }
                        else
                        {
                            time = lastTime;
                        }

                        var arg = new RecogResultEventArgs
                        {
                            ReusltType = reply.Type == ResultType.Final ? RecogType.Finish : RecogType.Middle,
                            Message = reply.Result,
                            BeginTime = time
                        };
                        AudioRecogResultArrived?.Invoke(this, arg);
                    }
                }
            }
            catch
            {
                // ignored
            }
        }
    }

    private class AudioRecogResult
    {
        [JsonProperty("code")] public ResultCode Code { get; set; }

        [JsonProperty("type")] public ResultType Type { get; set; }

        [JsonProperty("result")] public string Result { get; set; }

        /// <summary>
        ///     相对于第一包解析的毫秒数
        /// </summary>
        [JsonProperty("startTime")]
        public int StartTime { get; set; }

        /// <summary>
        ///     相对于第一包的毫秒数
        /// </summary>
        [JsonProperty("endTime")]
        public int EndTime { get; set; }

        [JsonProperty("err")] public string Error { get; set; }
    }

    [JsonConverter(typeof(JsonEnumAsStringFormatter<ResultCode>))]
    private enum ResultCode
    {
        [Key("CONNECTED")] Connected,
        [Key("SUCCESS")] Success,
        [Key("FAILURE")] Failure,
        [Key("HEARTBEAT")] Heartbeat
    }

    [JsonConverter(typeof(JsonEnumAsStringFormatter<ResultType>))]
    private enum ResultType
    {
        [Key("MID_TEXT")] Middle,
        [Key("FIN_TEXT")] Final
    }
}