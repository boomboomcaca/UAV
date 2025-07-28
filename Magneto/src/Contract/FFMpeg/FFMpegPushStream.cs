using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FFmpeg.AutoGen;
using Magneto.Contract.FFMpeg.PushStream;

namespace Magneto.Contract.FFMpeg;

/// <summary>
///     封装的使用FFmpeg.AutoGen进行推流的代码
///     鸣谢: https://blog.csdn.net/yang527062994/article/details/115622191
/// </summary>
public class FfMpegPushStream
{
    private const string IndexM3U8FileName = "index.m3u8";
    private readonly IoFillModel _fillModel = IoFillModel.ContinueWrite;
    private readonly PushStream.PushStream _pushStream = new();
    private readonly StreamItem _streamItem;
    private CancellationTokenSource _cts;
    private string _outputPath;
    private string _uri;
    public EventHandler<int> PushStopped;

    public FfMpegPushStream()
    {
        var dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "library", "ffmpeg");
        if (!Directory.Exists(dllPath)) throw new Exception("未找到ffmpeg安装路径！");
        ffmpeg.RootPath = dllPath;
        _streamItem = new StreamItem
        {
            TerminalNum = Guid.NewGuid().ToString(),
            Channel = 1,
            Tag = "FFmpeg"
        };
    }

    public void Initialized(string uri)
    {
        _uri = uri;
    }

    public void StartPush()
    {
        StartClearData();
        // 通过从读取内存数据，将流媒体数据推送到RTMP服务器
        _pushStream.AVIO_PushStreamToRmtp(_fillModel, _streamItem, _uri);
    }

    public Task StartPushAsync()
    {
        Trace.WriteLine("开始推流...");
        StartClearData();
        _outputPath = _uri;
        var path = Path.Combine(_uri, IndexM3U8FileName);
        // 通过从读取内存数据，将流媒体数据推送到RTMP服务器
        return Task.Run(() =>
        {
            int res;
            try
            {
                res = _pushStream.AVIO_PushStreamToRmtp(_fillModel, _streamItem, path, true);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"推流失败:{ex}");
                res = -1;
            }

            PushStopped?.Invoke(this, res);
        });
    }

    /// <summary>
    ///     现在暂时未实现通过ffmpeg保证文件夹下只有m3u8列表中的文件
    ///     因此在这里将不在列表中的文件删除
    /// </summary>
    public void ClearData()
    {
        if (!Directory.Exists(_outputPath)) return;
        var path = Path.Combine(_outputPath, IndexM3U8FileName);
        if (!File.Exists(path)) return;
        var info = new FileInfo(path);
        var lastTime = info.LastWriteTime;
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var sr = new StreamReader(fs);
        var files = new List<string>();
        while (!sr.EndOfStream)
        {
            var line = sr.ReadLine();
            if (line != null && line.StartsWith("#")) continue;
            if (line != null && !line.EndsWith(".ts"))
                // 防错
                continue;
            files.Add(line);
        }

        var dir = new DirectoryInfo(_outputPath);
        foreach (var file in dir.GetFiles())
        {
            if (files.Contains(file.Name)) continue;
            if (file.Extension == ".m3u8") continue;
            if (file.Extension == ".tmp")
                // 以防万一，tmp文件不删
                continue;
            if (file.LastWriteTime > lastTime)
                // 以防万一，时间比m3u8修改晚的文件也不删
                continue;
            try
            {
                Console.WriteLine($"删除文件:{file.Name}...");
                file.Delete();
            }
            catch
            {
                Console.WriteLine($"删除文件:{file.Name}失败");
            }
        }
    }

    public void Stop()
    {
        try
        {
            _cts?.Cancel();
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

        _pushStream.Stop();
    }

    public void AddData(byte[] buffer)
    {
        if (!_pushStream.IsRunning) return;
        var mdata = MDataCacheManager.Instance.GetMDataItem(_streamItem.TerminalNum, _streamItem.Channel);
        mdata.AddData(buffer);
    }

    private void StartClearData()
    {
        _cts = new CancellationTokenSource();
        _ = Task.Run(() => ClearOutputFileAsync(_cts.Token));
    }

    private async Task ClearOutputFileAsync(object obj)
    {
        if (obj is not CancellationToken token) return;
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(1000).ConfigureAwait(false);
            try
            {
                ClearData();
            }
            catch
            {
                // ignored
            }
        }
    }
}