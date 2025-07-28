using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Magneto.Contract.FFMpeg.PipePushStream;

public class RtspPushVideoServer
{
    /// <summary>
    ///     回放切片长度 5秒
    /// </summary>
    private const int SectionLength = 5;

    private string _ffmpegPath;
    private Process _ffmpegProcess;
    private FileSystemWatcher _fileWatcher;

    /// <summary>
    ///     新文件标记
    /// </summary>
    private bool _isNew;

    // private bool _isRunning = false;
    private string _m3U8Dir;

    /// <summary>
    ///     回放时长
    /// </summary>
    private double _totalReplayLength;

    /// <summary>
    ///     初始化实时推送
    /// </summary>
    /// <param name="uri">要推送到的地址</param>
    /// <param name="user"></param>
    /// <param name="password"></param>
    /// <param name="ip"></param>
    /// <param name="port">rtsp推流端口，默认为554</param>
    public void Initialized(string uri, string user, string password, string ip, int port = 554)
    {
        _ffmpegPath = GetFFmpegPath();
        var pushPath = Path.Combine(uri, "index.m3u8");
        Console.WriteLine($"ffmpeg路径:{_ffmpegPath},推送路径:{pushPath}");
        _m3U8Dir = uri;
        if (!Directory.Exists(_m3U8Dir)) Directory.CreateDirectory(_m3U8Dir);
        _pipeNameRecvAudio = "pushPipeAudio";
        // 新格式：
        // 实时 rtsp://username:password@<address>:<port>/Streaming/Channels/<id>(?parm1=value1&parm2-=value2…)
        // 回放 rtsp://username:password@<address>:<port>/Streaming/tracks/<id>(?parm1=value1&parm2-=value2…)
        var rtsp = $"rtsp://{user}:{password}@{ip}:{port}/Streaming/Channels/101?transportmode=unicast";
        // var arguments = $"-i {rtsp} -g 1 -keyint_min 1 -hls_time 1 -hls_list_size 5 -hls_wrap 5 -f hls {pushPath} -acodec pcm_s16le -f s16le -ac 1 -ar 16000 -y //./pipe/{_pipeNameRecvAudio} -loglevel error";
        var pipePath = @$"\\.\pipe\{_pipeNameRecvAudio}";
        var arguments =
            $"-i {rtsp} -g 1 -keyint_min 1 -hls_time 1 -hls_list_size 5 -hls_wrap 5 -f hls {pushPath} -acodec pcm_s16le -f s16le -ac 1 -ar 16000 -y {pipePath} -loglevel error";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            pipePath = $"/tmp/CoreFxPipe_{_pipeNameRecvAudio}=";
            arguments =
                $"-i {rtsp} -g 1 -keyint_min 1 -hls_time 1 -hls_list_size 5 -hls_delete_threshold 5 -f hls {pushPath} -acodec pcm_s16le -f s16le -ac 1 -ar 16000 -y {pipePath} -loglevel error";
        }

        #region 音频提取

        try
        {
            _pipeStreamRecvAudio = new NamedPipeServerStream(_pipeNameRecvAudio, PipeDirection.InOut, 1);
            _ = _pipeStreamRecvAudio.WaitForConnectionAsync().ConfigureAwait(false);
            Console.WriteLine($"创建管道名称:{_pipeNameRecvAudio}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"创建管道失败:{ex}");
        }

        #endregion

        _ffmpegProcess = ExecuteCmd(_ffmpegPath, arguments, AppDomain.CurrentDomain.BaseDirectory);
        _ffmpegProcess.Exited += Process_Exited;
    }

    /// <summary>
    ///     初始化回放
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="user"></param>
    /// <param name="password"></param>
    /// <param name="ip"></param>
    /// <param name="startTime"></param>
    /// <param name="stopTime"></param>
    /// <param name="port"></param>
    public void Initialized(string uri, string user, string password, string ip, DateTime startTime, DateTime stopTime,
        int port = 554)
    {
        _ffmpegPath = GetFFmpegPath();
        var pushPath = Path.Combine(uri, "index.m3u8");
        _m3U8Dir = uri;
        if (!Directory.Exists(_m3U8Dir)) Directory.CreateDirectory(_m3U8Dir);
        _fileWatcher = new FileSystemWatcher(_m3U8Dir, "*.m3u8");
        _fileWatcher.Renamed += FileRenamed;
        _fileWatcher.EnableRaisingEvents = true;
        _isNew = true;
        _totalReplayLength = stopTime.Subtract(startTime).TotalSeconds;
        var span = stopTime - startTime;
        var start = $"{startTime:yyyyMMdd}t{startTime:HHmmss}z";
        var stop = $"{stopTime:yyyyMMdd}t{stopTime:HHmmss}z";
        var rtsp = $"rtsp://{user}:{password}@{ip}:{port}/Streaming/tracks/101?starttime={start}&stoptime={stop}";
        // 回放 rtsp://username:password@<address>:<port>/Streaming/tracks/<id>(?parm1=value1&parm2-=value2…)
        _pipeNameRecvAudio = $@"pushPipeAudio\{Guid.NewGuid():N}";
        // 将视频推送与音频提取合并成一条命令
        // 解决海康威视回放时Streaming/tracks无法支持多播而报错453 Not Enough Bandwidth的问题
        var pipePath = @$"\\.\pipe\{_pipeNameRecvAudio}";
        var arguments =
            $"-t {span:hh\\:mm\\:ss\\.fff} -i \"{rtsp}\"  -g 5 -keyint_min {SectionLength} -hls_time {SectionLength} -hls_list_size 0 -hls_wrap 0 -f hls {pushPath} -acodec pcm_s16le -f s16le -ac 1 -ar 16000 -y {pipePath} -loglevel error";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            pipePath = $"/tmp/CoreFxPipe_pushPipeAudio/{Guid.NewGuid():N}=";
            arguments =
                $"-t {span:hh\\:mm\\:ss\\.fff} -i \"{rtsp}\"  -g 5 -keyint_min {SectionLength} -hls_time {SectionLength} -hls_list_size 0 -hls_wrap 0 -f hls {pushPath} -acodec pcm_s16le -f s16le -ac 1 -ar 16000 -y {pipePath} -loglevel error";
        }

        #region 音频提取

        _pipeStreamRecvAudio = new NamedPipeServerStream(_pipeNameRecvAudio, PipeDirection.InOut, 1);
        _ = _pipeStreamRecvAudio.WaitForConnectionAsync().ConfigureAwait(false);

        #endregion

        _ffmpegProcess = ExecuteCmd(_ffmpegPath, arguments, AppDomain.CurrentDomain.BaseDirectory);
        _ffmpegProcess.Exited += Process_Exited;
    }

    public void StartPush()
    {
        _cts = new CancellationTokenSource();
        _recvDataTask = Task.Run(() => RecvDataAsync(_cts.Token));
        // _isRunning = true;
    }

    public void Stop()
    {
        // _isRunning = false;
        try
        {
            if (_pipeStreamRecvAudio?.IsConnected == true) _pipeStreamRecvAudio.Disconnect();
        }
        catch
        {
            // ignored
        }

        try
        {
            _pipeStreamRecvAudio?.Close();
            _pipeStreamRecvAudio?.Dispose();
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"关闭管道{_pipeNameRecvAudio}失败,{ex}");
        }

        Task.Delay(100).ConfigureAwait(false).GetAwaiter().GetResult();
        try
        {
            if (_ffmpegProcess != null)
            {
                _ffmpegProcess.Exited -= Process_Exited;
                _ffmpegProcess.Kill();
                _ffmpegProcess.Close();
                _ffmpegProcess.Dispose();
            }
        }
        catch
        {
            // ignored
        }

        // try
        // {
        //     if (_ffmpegAudioProcess != null)
        //     {
        //         _ffmpegAudioProcess.Kill();
        //         _ffmpegAudioProcess.Close();
        //         _ffmpegAudioProcess.Dispose();
        //     }
        // }
        // catch
        // {
        // }
        Task.Delay(100).ConfigureAwait(false).GetAwaiter().GetResult();
        if (_fileWatcher != null)
        {
            _fileWatcher.Renamed -= FileRenamed;
            _fileWatcher.Dispose();
        }

        try
        {
            _cts?.Cancel();
            _recvDataTask?.Dispose();
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
    }

    public void ClearData()
    {
        if (!Directory.Exists(_m3U8Dir)) return;
        // var dir = new DirectoryInfo(_m3u8Dir);
        // foreach (var file in dir.GetFiles())
        // {
        //     file.Delete();
        // }
        Directory.Delete(_m3U8Dir, true);
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
        p.EnableRaisingEvents = true;
        p.Start();
        // p.BeginOutputReadLine();
        return p;
    }

    private async Task RecvDataAsync(object obj)
    {
        var token = (CancellationToken)obj;
        var buffer = new byte[10240];
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(0, token).ConfigureAwait(false);
            try
            {
                if (!_pipeStreamRecvAudio.IsConnected) continue;
                var count = await _pipeStreamRecvAudio.ReadAsync(buffer, token).ConfigureAwait(false);
                if (count == 0) continue;
                // Console.WriteLine($"接收到{count}个数据,间隔{span}");
                var data = new byte[count];
                Buffer.BlockCopy(buffer, 0, data, 0, count);
                AudioDataReceived?.Invoke(this, data);
            }
            catch
            {
                // ignored
            }
        }
    }

    private void FileRenamed(object sender, RenamedEventArgs e)
    {
        try
        {
            if (!_isNew) return;
            if (e.Name != "index.m3u8") return;
            if (e.ChangeType == WatcherChangeTypes.Renamed)
            {
                Task.Delay(1).ConfigureAwait(false).GetAwaiter().GetResult();
                using var fs = new FileStream(e.FullPath, FileMode.Create, FileAccess.Write,
                    FileShare.Delete | FileShare.ReadWrite);
                using var sw = new StreamWriter(fs);
                var str = CreateM3U8PlayList();
                sw.WriteLine(str);
                sw.Flush();
                sw.Close();
                fs.Close();
                _isNew = false;
            }
        }
        catch
        {
            // ignored
        }
    }

    private string CreateM3U8PlayList()
    {
        var sb = new StringBuilder();
        sb.AppendLine("#EXTM3U");
        sb.AppendLine("#EXT-X-VERSION:3");
        sb.Append("#EXT-X-TARGETDURATION:").Append(SectionLength).AppendLine();
        sb.AppendLine("#EXT-X-MEDIA-SEQUENCE:0");
        var count = (int)_totalReplayLength / SectionLength;
        var remainder = _totalReplayLength - count * SectionLength;
        for (var i = 0; i < count; i++)
        {
            sb.Append("#EXTINF:").AppendFormat("{0:0.000000}", SectionLength).AppendLine(",");
            sb.Append("index").Append(i).AppendLine(".ts");
        }

        if (remainder > 0)
        {
            sb.Append("#EXTINF:").AppendFormat("{0:0.000000}", remainder).AppendLine(",");
            sb.Append("index").Append(count).AppendLine(".ts");
        }

        sb.AppendLine("#EXT-X-ENDLIST");
        return sb.ToString();
    }

    private void Process_Exited(object sender, EventArgs e)
    {
        Console.WriteLine("ffmpeg进程退出...");
        FfMpegExeExited?.Invoke(this, e);
    }

    private static string GetFFmpegPath()
    {
        var dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.PathLibrary, "ffmpeg");
        if (!Directory.Exists(dllPath)) throw new Exception("未找到ffmpeg安装路径！");
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return Path.Combine(dllPath, "linux", "ffmpeg");
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return Path.Combine(dllPath, "windows", "ffmpeg.exe");
        throw new Exception("不受支持的系统");
    }

    #region 音频提取

    private string _pipeNameRecvAudio = "pushPipeAudio";
    private NamedPipeServerStream _pipeStreamRecvAudio;
    private Task _recvDataTask;
    private CancellationTokenSource _cts;
    public event EventHandler<byte[]> AudioDataReceived;

    /// <summary>
    ///     回放用，ffmpeg进程退出事件
    /// </summary>
    public event EventHandler FfMpegExeExited;

    // private Process _ffmpegAudioProcess = null;

    #endregion
}