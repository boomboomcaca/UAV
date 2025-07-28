using System;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Magneto.Device.StreamStorage;

public class SimDevice
{
    private CancellationTokenSource _cts;
    private int _currentProgress;
    private bool _isPause;
    private bool _isPlayback;
    private NamedPipeClientStream _pipeClient = null!;
    private string _pipeName;
    private NamedPipeServerStream _pipeStream;
    private Task _pipeTask;
    private double _preSpan;
    private DateTime _preTime = DateTime.Now;
    private Task _simDataTask;
    private long _totalSize;

    public void Start(string pipeName)
    {
        _pipeName = pipeName;
        _cts = new CancellationTokenSource();
        _pipeTask = new Task(p => RecvPipeMsgAsync(p).ConfigureAwait(false), _cts.Token);
        _pipeTask.Start();
        _simDataTask = new Task(p => SimDataAsync(p).ConfigureAwait(false), _cts.Token);
        _simDataTask.Start();
        var server = _pipeName;
        _pipeStream = new NamedPipeServerStream(server, PipeDirection.InOut, 1);
        _ = _pipeStream.WaitForConnectionAsync();
    }

    public void Stop()
    {
        try
        {
            _cts?.Cancel();
        }
        catch
        {
        }

        try
        {
            _pipeTask?.Dispose();
            _simDataTask?.Dispose();
        }
        catch
        {
        }

        try
        {
            _pipeClient?.Dispose();
            _pipeStream?.Dispose();
        }
        catch
        {
        }
    }

    private async Task RecvPipeMsgAsync(object obj)
    {
        if (obj is not CancellationToken token) return;
        var buffer = new byte[1024];
        var connected = false;
        while (!token.IsCancellationRequested)
            try
            {
                if (_pipeStream?.IsConnected != true)
                {
                    await Task.Yield();
                    if (connected)
                    {
                        connected = false;
                        Console.WriteLine("SIM:命令管道连接中断");
                    }

                    continue;
                }

                if (!connected)
                {
                    connected = true;
                    var client = $"{_pipeName}_Reply";
                    _pipeClient = new NamedPipeClientStream(client);
                    await _pipeClient.ConnectAsync(1000, token);
                    Console.WriteLine("SIM:命令管道已连接");
                }

                var count = await _pipeStream!.ReadAsync(buffer);
                if (count == 0)
                {
                    await Task.Yield();
                    continue;
                }

                var str = Encoding.ASCII.GetString(buffer, 0, count);
                var cmds = str.Split(';');
                Console.WriteLine($"SIM:命令管道收到消息:{str}");
                foreach (var cmd in cmds)
                {
                    if (string.IsNullOrEmpty(cmd)) continue;
                    var array = cmd.Split(":");
                    if (array.Length < 2) continue;
                    switch (array[0].ToLower())
                    {
                        // 任务启动
                        case "start":
                        {
                            if (array.Length < 3)
                            {
                                await SendCmdAsync("error:format;", token);
                                break;
                            }

                            var res = "";
                            switch (array[1])
                            {
                                case "record":
                                    res = "testDataPathForSim";
                                    break;
                                case "playback":
                                    res = "testDataPathForSim";
                                    if (!long.TryParse(array[2], out var len)) break;
                                    _totalSize = len;
                                    _preTime = DateTime.Now;
                                    if (_isPlayback && _isPause)
                                    {
                                        _isPause = false;
                                    }
                                    else
                                    {
                                        _preSpan = 0;
                                        _currentProgress = 0;
                                        _isPlayback = true;
                                        _isPause = false;
                                    }

                                    break;
                            }

                            var reply = $"start:{array[1]}:{res};";
                            await SendCmdAsync(reply, token);
                        }
                            break;
                        // 任务停止
                        case "stop":
                        {
                            if (array.Length < 2)
                            {
                                await SendCmdAsync("error:format;", token);
                                break;
                            }

                            _isPause = false;
                            _isPlayback = false;
                            _currentProgress = 0;
                            if (array[1].Equals("record", StringComparison.OrdinalIgnoreCase))
                            {
                                var rp = "record:complete:100:10000";
                                await SendCmdAsync(rp, token);
                            }

                            await SendCmdAsync($"stop:{array[1]}:OK;", token);
                        }
                            break;
                        // 取消保存
                        case "cancel":
                        {
                            if (array.Length < 2)
                            {
                                await SendCmdAsync("error:format;", token);
                                break;
                            }

                            if (!array[1].Equals("record", StringComparison.OrdinalIgnoreCase))
                            {
                                await SendCmdAsync("error:parameter;", token);
                                break;
                            }

                            await SendCmdAsync("record:drop;", token);
                        }
                            break;
                        // 暂停发送
                        case "pause":
                        {
                            if (array.Length < 2)
                            {
                                await SendCmdAsync("error:format;", token);
                                break;
                            }

                            if (!array[1].Equals("playback", StringComparison.OrdinalIgnoreCase))
                            {
                                await SendCmdAsync("error:parameter;", token);
                                break;
                            }

                            if (!_isPlayback)
                            {
                                await SendCmdAsync("error:parameter;", token);
                                break;
                            }

                            _isPause = true;
                            _preTime = DateTime.Now;
                            await SendCmdAsync("pause:playback:OK;", token);
                        }
                            break;
                        // 跳转回放
                        case "progress":
                        {
                            if (array.Length < 3)
                            {
                                await SendCmdAsync("error:format;", token);
                                break;
                            }

                            if (!array[1].Equals("playback", StringComparison.OrdinalIgnoreCase))
                            {
                                await SendCmdAsync("error:parameter;", token);
                                break;
                            }

                            if (!int.TryParse(array[2], out var progress))
                            {
                                await SendCmdAsync("error:parameter;", token);
                                break;
                            }

                            _preTime = DateTime.Now;
                            _preSpan = _totalSize * progress / 10000d;
                            _currentProgress = progress;
                            await SendCmdAsync("progress:playback:OK;", token);
                        }
                            break;
                    }
                }
            }
            catch
            {
            }
    }

    private async Task SimDataAsync(object obj)
    {
        if (obj is not CancellationToken token) return;
        while (!token.IsCancellationRequested)
            try
            {
                await Task.Delay(100).ConfigureAwait(false);
                if (!_isPlayback)
                {
                    _currentProgress = 0;
                    continue;
                }

                if (_isPause)
                {
                    _preTime = DateTime.Now;
                    continue;
                }

                var span = DateTime.Now.Subtract(_preTime).TotalMilliseconds;
                _preTime = DateTime.Now;
                _preSpan += span;
                var progress = _preSpan / _totalSize;
                _currentProgress = (int)(progress * 10000);
                if (_currentProgress > 10000)
                {
                    _currentProgress = 10000;
                    _isPlayback = false;
                }

                var cmd = $"progress:{_currentProgress}";
                await SendCmdAsync(cmd, token);
            }
            catch
            {
            }
    }

    private async Task<bool> SendCmdAsync(string cmd, CancellationToken token)
    {
        if (!cmd.EndsWith(';')) cmd += ";";
        if (_pipeClient?.IsConnected != true) return false;
        var data = Encoding.ASCII.GetBytes(cmd);
        await _pipeClient.WriteAsync(data, token);
        return true;
    }
}