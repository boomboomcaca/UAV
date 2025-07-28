using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Protocol.Data;
using StreamJsonRpc;

namespace Magneto.Contract.SignalDemod;

public sealed class AnalysisService : IDisposable
{
    private static readonly Lazy<AnalysisService> _instance = new(() => new AnalysisService());
    private ClientWebSocket _clientWebSocket;
    private bool _disposed;
    private JsonRpc _jsonRpc;

    private AnalysisService()
    {
    }

    public static AnalysisService InstanceAnalysisService => _instance.Value;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public event EventHandler<AnalysisResult> AnalysisCompleted;
    public event EventHandler<EventArgs> Disconnected;

    ~AnalysisService()
    {
        Dispose(false);
    }

    public async Task<bool> InitAsync(string ip, int port, CancellationToken token)
    {
        try
        {
            _clientWebSocket = new ClientWebSocket();
            await _clientWebSocket.ConnectAsync(new Uri($"ws://{ip}:{port}/socket"), token).ConfigureAwait(false);
            //var formatter = new JsonMessageFormatter();
            //formatter.JsonSerializer.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            var formatter = new MessagePackFormatter();
            _jsonRpc = new JsonRpc(new WebSocketMessageHandler(_clientWebSocket, formatter));
            _jsonRpc.AddLocalRpcMethod("AnalysisCompleted",
                new Action<AnalysisResult>(args => AnalysisCompleted?.Invoke(this, args)));
            _jsonRpc.Disconnected += JsonRpc_Disconnected;
            _jsonRpc.StartListening();
            _ = _jsonRpc.Completion; // Start listening without wrapping in Task.Run
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> IsAvaliableAsync()
    {
        try
        {
            var result =
                await InvokeAsync<ResponseModel<bool>>("CheckServiceAvailable", new RequestModel { Secret = "xmen" })
                    .ConfigureAwait(false);
            return result?.Result == true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> StartAnalysisCoreAsync()
    {
        try
        {
            var result =
                await InvokeAsync<ResponseModel<bool>>("RestartAnalysisCore", new RequestModel { Secret = "xmen" })
                    .ConfigureAwait(false);
            return result?.Result == true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> AnalysisDataAsync(string fileName, bool calculateSymbolRate, Guid id, string edgeId,
        string deviceId)
    {
        try
        {
            if (!File.Exists(fileName)) return false;
            var name = Path.GetFileName(fileName);
            await using var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            var buffer = new byte[fs.Length];
            _ = await fs.ReadAsync(buffer);
            var result = await InvokeAsync<ResponseModel<bool>>("AnalyseSignalData",
                new RequestModel<AnalysisDataInfo>
                {
                    Secret = "xmen",
                    Data = new AnalysisDataInfo
                    {
                        Id = id,
                        DeviceId = deviceId,
                        EdgeId = edgeId,
                        FileName = name,
                        CalculateSymbolRate = calculateSymbolRate,
                        Data = buffer
                    }
                }).ConfigureAwait(false);
            return result?.Data == true;
        }
        catch
        {
            return false;
        }
    }

    public void Close()
    {
        if (_jsonRpc != null)
        {
            _jsonRpc.Disconnected -= JsonRpc_Disconnected;
            _jsonRpc.Dispose();
        }

        _clientWebSocket?.Dispose();
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
        }

        Close();
        _disposed = true;
    }

    private async Task<T> InvokeAsync<T>(string targetName, params object[] arguments)
    {
        if (arguments == null || arguments.Length == 0)
            return await _jsonRpc.InvokeWithParameterObjectAsync<T>(targetName);
        return await _jsonRpc.InvokeAsync<T>(targetName, arguments);
    }

    private void JsonRpc_Disconnected(object sender, JsonRpcDisconnectedEventArgs e)
    {
        Disconnected?.Invoke(sender, e);
    }
}