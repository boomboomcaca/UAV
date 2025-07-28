using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace Magneto.Contract.FFMpeg.PipePushStream;

public class PipeServer
{
    private readonly ConcurrentQueue<byte[]> _dataCache = new();
    private CancellationTokenSource _cts;
    private string _ffmpegPath;
    private Process _ffmpegProcess;
    private bool _isRunning;
    private string _m3U8Dir;
    private string _pipeName = "pushVideoDataPipe";
    private NamedPipeServerStream _pipeStream;
    private Task _sendDataTask;

    /// <summary>
    ///     初始化推送
    /// </summary>
    /// <param name="uri">要推送到的地址</param>
    /// <param name="isPlayback">测试标记</param>
    public void Initialized(string uri, bool isPlayback = false)
    {
        var dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "library", "ffmpeg");
        if (!Directory.Exists(dllPath)) throw new Exception("未找到ffmpeg安装路径!");
        var pushPath = Path.Combine(uri, "index.m3u8");
        _m3U8Dir = uri;
        if (!Directory.Exists(_m3U8Dir)) Directory.CreateDirectory(_m3U8Dir);
        var arguments =
            $"-i //./pipe/{_pipeName} -profile:v baseline -level 3.0 -start_number 0 -hls_time 1 -hls_list_size 3 -hls_wrap 3 -f hls {pushPath}";
        // var arguments = $"-re -i //./pipe/{_pipeName} -codec:v libx264 -map 0 -g 25 -keyint_min 25 -hls_time 1 -hls_list_size 3 -hls_wrap 3 -f hls {pushPath}";
        // var arguments = $"-re -i //./pipe/{_pipeName} -g 10 -keyint_min 10 -sc_threshold 0 -hls_time 1 -hls_list_size 3 -hls_wrap 3 -f hls {pushPath}";
        // var arguments = $"-i //./pipe/{_pipeName} -force_key_frames \"expr: gte(t, n_forced * 1)\" -hls_time 1 -hls_list_size 3 -hls_wrap 3 -f hls {pushPath}";
        // var arguments = $"-i //./pipe/{_pipeName} -profile:v baseline -level 3.0 -start_number 0 -hls_time 1 -hls_list_size 8 -hls_wrap 8 -f hls {pushPath}";
        if (isPlayback)
        {
            _pipeName = "pushPlaybackVideoDataPipe";
            arguments =
                $"-i //./pipe/{_pipeName} -g 1 -keyint_min 3 -hls_time 3 -hls_list_size 0 -hls_wrap 0 -f hls {pushPath}";
        }

        _pipeStream = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, 1);
        _ = Task.Run(() => _pipeStream.WaitForConnectionAsync());
        _ffmpegPath = Path.Combine(dllPath, "ffmpeg.exe");
        _ffmpegProcess = ExecuteCmd(_ffmpegPath, arguments, AppDomain.CurrentDomain.BaseDirectory);
    }

    public void StartPush()
    {
        _cts = new CancellationTokenSource();
        _sendDataTask = Task.Run(() => SendDataAsync(_cts.Token));
        _isRunning = true;
    }

    public void Stop()
    {
        _isRunning = false;
        try
        {
            _cts?.Cancel();
            _sendDataTask?.Dispose();
        }
        catch
        {
            // ignored
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
        }

        try
        {
            _ffmpegProcess?.Kill();
            _ffmpegProcess?.Close();
            _ffmpegProcess?.Dispose();
        }
        catch
        {
            // ignored
        }

        Task.Delay(100).ConfigureAwait(false).GetAwaiter().GetResult();
        try
        {
            _pipeStream?.Close();
            _pipeStream?.Dispose();
        }
        catch
        {
            // ignored
        }

        Task.Delay(100).ConfigureAwait(false).GetAwaiter().GetResult();
        // try
        // {
        //     ClearData();
        // }
        // catch
        // {
        // }
    }

    public void AddData(byte[] data)
    {
        if (!_isRunning) return;
        _dataCache.Enqueue(data);
    }

    private async Task SendDataAsync(object obj)
    {
        var time = DateTime.Now;
        var token = (CancellationToken)obj;
        while (!token.IsCancellationRequested)
            // await Task.Delay(0).ConfigureAwait(false);
            try
            {
                if (_dataCache.IsEmpty)
                {
                    await Task.Delay(0, token).ConfigureAwait(false);
                    continue;
                }

                // var list = new List<byte>();
                // while (_dataCache.TryDequeue(out byte[] buffer))
                // {
                //     list.AddRange(buffer);
                // }
                if (!_dataCache.TryDequeue(out var data))
                {
                    await Task.Delay(0, token).ConfigureAwait(false);
                    continue;
                }

                if (!_pipeStream.IsConnected)
                {
                    await Task.Delay(0, token).ConfigureAwait(false);
                    continue;
                    // PushPipeClosed?.Invoke(this, null);
                    // _isRunning = false;
                    // break;
                }

                if (DateTime.Now.Subtract(time).TotalSeconds > 5)
                {
                    Console.WriteLine($"当前缓存数量{_dataCache.Count}");
                    time = DateTime.Now;
                }

                if (data.Length == 20) continue;
                // {
                //     try
                //     {
                //         using var fs = new FileStream("video.dat", FileMode.Append, FileAccess.Write);
                //         await fs.WriteAsync(data, 0, data.Length);
                //         await fs.FlushAsync();
                //         fs.Close();
                //     }
                //     catch
                //     {
                //     }
                // }
                // var data = list.ToArray();
                await _pipeStream.WriteAsync(data, token).ConfigureAwait(false);
            }
            catch
            {
                // ignored
            }
    }

    private static Process ExecuteCmd(string fileName, string arguments, string workingDirectory = "")
    {
        var p = new Process();
        p.StartInfo.FileName = fileName;
        p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        p.StartInfo.Arguments = arguments;
        p.StartInfo.CreateNoWindow = false;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.WorkingDirectory = workingDirectory;
        p.StartInfo.ErrorDialog = true;
        // p.OutputDataReceived += OutputDataReceived;
        p.EnableRaisingEvents = false;
        p.Start();
        // p.BeginOutputReadLine();
        return p;
    }
}