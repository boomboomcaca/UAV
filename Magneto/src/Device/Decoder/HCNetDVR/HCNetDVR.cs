using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.Audio;
using Magneto.Contract.BaseClass;
using Magneto.Contract.FFMpeg.PipePushStream;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.HCNetDVR;

/// <summary>
///     录播盒驱动模块
/// </summary>
public partial class HcNetDvr : DeviceBase
{
    private readonly Dictionary<DateTime, string> _messageCache = new();

    /// <summary>
    ///     上次发送音频识别完成的消息的时间
    ///     为了防止音频识别完成的那个完整句子刚出现就被后面的未完成词语刷掉，这里暂时添加1秒的延迟
    /// </summary>
    private DateTime _lastSendRecogResultTime = DateTime.MinValue;

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="id">模块ID</param>
    public HcNetDvr(Guid id)
        : base(id)
    {
    }

    private void ResetCacheData()
    {
        _avPlaying = false;
        _pushStream?.Stop();
        _audioDataCache.Clear();
        _messageCache.Clear();
    }

    private static void ClearVideoDir()
    {
        var path = RunningInfo.VideoDir;
        var info = new DirectoryInfo(path);
        try
        {
            foreach (var dir in info.GetDirectories()) dir.Delete(true);
        }
        catch
        {
            // ignored
        }
    }

    private void FfMpegExeExited(object sender, EventArgs e)
    {
        if (_playType != PlayTypeEnum.Playback) return;
        if (_messageCache.Count == 0) return;
        var message = "";
        foreach (var pair in _messageCache)
            // message += $"[{time:HH:mm:ss}] {pair.Value}|";
            message += $"{pair.Value}|";

        _messageCache.Clear();
        var keys = new List<string>();
        foreach (var str in _keywords)
            if (message.Contains(str))
                keys.Add(str);
        var audioData = new SDataAudioRecognition
        {
            Timestamp = Utils.GetTimestamp(_startPlaybackTime),
            Message = message,
            Keywords = keys
        };
        SendData(new List<object> { audioData });
    }

    private void AudioDataReceived(object sender, byte[] e)
    {
        if (!_avPlaying) return;
        _audioDataCache.Enqueue(e);
    }

    private async Task AudioRecogAsync(object obj)
    {
        var token = (CancellationToken)obj;
        _ = new List<byte>();
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(0, token).ConfigureAwait(false);
            try
            {
                if (_audioDataCache.IsEmpty) continue;
                if (!_audioDataCache.TryDequeue(out var data)) continue;
                if (!_avPlaying) continue;
                Utils.GetTimestamp(DateTime.Now);
                // Console.WriteLine($"发送音频数据:{data.Length},间隔:{span},缓存数量:{_audioDataCache.Count}");
                await _audioRecog.GetAuidoRecogMessageAsync(data, token);
            }
            catch
            {
                // Console.WriteLine(ex.ToString());
            }
        }
    }

    private void AudioRecogResultArrived(object sender, RecogResultEventArgs e)
    {
        try
        {
            if (e == null) return;
            if (!_avPlaying || _playType == PlayTypeEnum.None) return;
            var audioData = new SDataAudioRecognition();
            if (e.ReusltType is RecogType.Error or RecogType.Info)
                // TODO : 错误信息是否要发送到前端
                return;
            var keys = new List<string>();
            foreach (var str in _keywords)
                if (e.Message.Contains(str))
                    keys.Add(str);
            if (e.ReusltType == RecogType.Middle &&
                DateTime.Now.Subtract(_lastSendRecogResultTime).TotalSeconds < 2) return;
            var time = e.BeginTime;
            var msg = e.Message;
            if (_playType == PlayTypeEnum.Playback)
            {
                // 回放时需要将时间转为真实的语音时间
                var span = e.BeginTime - _startPlaybackTime;
                time = _fileBeginTime + span;
                // msg = $"[{time:HH:mm:ss}] {e.Message}";
            }

            if (e.ReusltType == RecogType.Finish)
            {
                _lastSendRecogResultTime = DateTime.Now;
                _messageCache.Add(time, e.Message);
            }

            audioData.Timestamp = Utils.GetTimestamp(e.BeginTime);
            audioData.Message = msg;
            audioData.Keywords = keys;
            SendData(new List<object> { audioData });
        }
        catch
        {
            // ignored
        }
    }

    #region Field

    /// <summary>
    ///     录播盒的播放模式，默认为实时预览
    /// </summary>
    private PlayTypeEnum _playType = PlayTypeEnum.None;

    /// <summary>
    ///     当前播放的频率
    /// </summary>
    private double _currentFrequency;

    /// <summary>
    ///     当前播放的制式
    /// </summary>
    private TvStandard _currentStandard = TvStandard.ANAFM;

    /// <summary>
    ///     当前播放的节目号
    /// </summary>
    private int _currentNumber;

    private string _currentProgramName;

    #region 推流相关

    private RtspPushVideoServer _pushStream;
    private string _pushUri = string.Empty;
    private string _currentUriId = string.Empty;

    private readonly string _videoUriHeader =
        $"http://{RunningInfo.EdgeIp}:{RunningInfo.Port}/{PublicDefine.PathVideo}";

    #endregion

    #region 录制相关

    /// <summary>
    ///     录像文件缓存
    /// </summary>
    private readonly List<DvrFileInfo> _dvrFilesCache = new();

    /// <summary>
    ///     回放文件的开始时间
    /// </summary>
    private DateTime _startDvrTime = DateTime.Now;

    /// <summary>
    ///     回放文件的结束时间
    /// </summary>
    private DateTime _stopDvrTime = DateTime.Now;

    #endregion

    #region 音频解调

    private readonly ConcurrentQueue<byte[]> _audioDataCache = new();
    private CancellationTokenSource _cts;
    private Task _audioDataRecogTask;
    private List<string> _keywords = new();

    /// <summary>
    ///     正在播放或回放标记
    /// </summary>
    private bool _avPlaying;

    private AudioRecognition _audioRecog;

    #endregion

    //private Socket _socket = null;

    #endregion

    #region Implement DeviceBase

    /// <summary>
    ///     初始化
    /// </summary>
    /// <param name="moduleInfo">模块信息</param>
    /// <returns>true=成功；false=失败</returns>
    public override bool Initialized(ModuleInfo moduleInfo)
    {
        var result = base.Initialized(moduleInfo);
        if (!result) return false;
        _pushUri = RunningInfo.VideoDir;
        _pushStream = new RtspPushVideoServer();
        _pushStream.AudioDataReceived += AudioDataReceived;
        _pushStream.FfMpegExeExited += FfMpegExeExited;
        _audioRecog = new AudioRecognition();
        _audioRecog.Initialized(RunningInfo.AudioRecognitionAddress, RunningInfo.AudioRecognitionPort,
            RunningInfo.AudioRecognitionServerKey);
        _audioRecog.AudioRecogResultArrived += AudioRecogResultArrived;
        _cts = new CancellationTokenSource();
        _audioDataRecogTask = new Task(p => AudioRecogAsync(p).ConfigureAwait(false), _cts.Token);
        _audioDataRecogTask.Start();
        return true;
    }

    /// <summary>
    ///     开始
    /// </summary>
    /// <param name="feature"></param>
    /// <param name="dataPort"></param>
    /// <returns>true=成功；false=失败</returns>
    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        _ = Task.Run(async () =>
        {
            var data = await CloudClient.Instance.GetCloudDictionaryDataAsync("irmKeyword");
            if (data.DictionaryType != "irmKeyword") return;
            _keywords.Clear();
            _keywords = data.Data.Select(item => item.Value).ToList();
        });
        var path = RunningInfo.VideoDir;
        if (Directory.Exists(path)) Directory.Delete(path, true);
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
    }

    /// <summary>
    ///     停止
    /// </summary>
    /// <returns>true=成功；false=失败</returns>
    public override void Stop()
    {
        base.Stop();
        _pushStream.Stop();
        StopReceiveData();
        StopPlayBackDvr();
    }

    public override void SetParameter(string name, object value)
    {
        if (TaskState != TaskState.Start)
        {
            // 功能启动前禁止安装参数以外的其他参数设置
            var para = DeviceInfo?.Parameters?.Find(item => item.Name == name);
            if (para?.IsInstallation == false) return;
        }

        base.SetParameter(name, value);
    }

    /// <summary>
    ///     释放资源
    /// </summary>
    public override void Dispose()
    {
        // LoginOut();
        if (_pushStream is not null)
        {
            _pushStream.AudioDataReceived -= AudioDataReceived;
            _pushStream.FfMpegExeExited -= FfMpegExeExited;
            _pushStream.Stop();
        }

        try
        {
            _cts?.Cancel();
        }
        catch
        {
            // ignored
        }

        try
        {
            _audioDataRecogTask.Dispose();
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

        _audioRecog?.Close();
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion
}