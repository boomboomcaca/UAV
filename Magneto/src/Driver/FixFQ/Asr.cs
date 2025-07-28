using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.Audio;
using Magneto.Protocol.Data;

namespace FixFQ;

public class Asr
{
    #region 语音识别

    private Task _asrProcessTask;
    private CancellationTokenSource _asrCts;
    private readonly ConcurrentQueue<SDataAudio> _audioCache = new();
    private readonly ConcurrentQueue<byte> _asrAudioCache = new();
    private bool _running;

    private AudioProcess _audioProcess;

    // private AudioDataSave _audioSave = null;
    private AudioRecognition _audioRecog;
    private List<string> _keywords = new();
    public event EventHandler<SDataAudioRecognition> AudioRecogResultArrivedEvent;

    public void Start(int targetSampleRate, int targetBitPerSample, int targetChannels)
    {
        _audioProcess = new AudioProcess();
        _audioProcess.Start(targetBitPerSample, targetSampleRate, targetChannels);
        _ = Task.Run(async () =>
        {
            var data = await CloudClient.Instance.GetCloudDictionaryDataAsync("irmKeyword");
            if (data.DictionaryType != "irmKeyword") return;
            _keywords.Clear();
            _keywords = data.Data.Select(item => item.Value).ToList();
            Console.WriteLine($"关键字列表:{string.Join(",", _keywords)}");
        });
        // _audioSave = new();
        _audioRecog = new AudioRecognition();
        _audioRecog.Initialized(RunningInfo.AudioRecognitionAddress, RunningInfo.AudioRecognitionPort,
            RunningInfo.AudioRecognitionServerKey);
        _audioRecog.AudioRecogResultArrived += AudioRecogResultArrived;
        _running = true;
        _audioCache.Clear();
        _asrAudioCache.Clear();
        _asrCts = new CancellationTokenSource();
        _asrProcessTask = Task.Run(() => ProcessAsrAsync(_asrCts.Token));
    }

    public void Stop()
    {
        _running = false;
        _audioCache.Clear();
        _asrAudioCache.Clear();
        _asrCts?.Cancel();
        _audioProcess?.Stop();
        _audioRecog?.Close();
        // _audioSave.SaveComplete("audioTemp");
        // _audioSave.Dispose();
        try
        {
            _asrProcessTask?.Dispose();
        }
        catch
        {
            // ignored
        }
    }

    public void AddAudio(SDataAudio audio)
    {
        if (!_running) return;
        _audioCache.Enqueue(audio);
    }

    /// <summary>
    ///     语音识别线程
    /// </summary>
    /// <param name="obj"></param>
    private async Task ProcessAsrAsync(object obj)
    {
        if (obj is not CancellationToken token) return;
        while (!token.IsCancellationRequested)
        {
            if (!_audioCache.TryDequeue(out var audio))
            {
                await Task.Delay(1, token).ConfigureAwait(false);
                continue;
            }

            var data = audio.Data;
            var list = _audioProcess.Convert(data, audio.SamplingRate, audio.BitsPerSample, audio.Channels);
            // if (!_audioSave.Running)
            // {
            //     _audioSave.SaveStart(AppDomain.CurrentDomain.BaseDirectory, "AudioTemp", "testAudio", 100, 1, 16000, 16);
            // }
            // _audioSave.SaveData(buffer);
            list.ForEach(item => _asrAudioCache.Enqueue(item));
            List<byte[]> asrList = new();
            while (_asrAudioCache.Count > 5120)
            {
                var buffer = new byte[5120];
                for (var i = 0; i < 5120; i++)
                {
                    if (!_asrAudioCache.TryDequeue(out var bt)) continue;
                    buffer[i] = bt;
                }

                asrList.Add(buffer);
            }

            foreach (var buffer in asrList)
            {
                await _audioRecog.GetAuidoRecogMessageAsync(buffer, token);
                await Task.Delay(100, token).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    ///     上次发送音频识别完成的消息的时间
    ///     为了防止音频识别完成的那个完整句子刚出现就被后面的未完成词语刷掉，这里暂时添加1秒的延迟
    /// </summary>
    private DateTime _lastSendRecogResultTime = DateTime.MinValue;

    private void AudioRecogResultArrived(object sender, RecogResultEventArgs e)
    {
        try
        {
            if (e == null) return;
            if (e.ReusltType is RecogType.Error or RecogType.Info) return;
            var keys = new List<string>();
            foreach (var str in _keywords)
                if (e.Message.Contains(str))
                    keys.Add(str);
            if (e.ReusltType == RecogType.Middle &&
                DateTime.Now.Subtract(_lastSendRecogResultTime).TotalSeconds < 2) return;
            var msg = e.Message;
            if (e.ReusltType == RecogType.Finish)
            {
                _lastSendRecogResultTime = DateTime.Now;
                var audioData = new SDataAudioRecognition
                {
                    Timestamp = Utils.GetTimestamp(e.BeginTime),
                    Message = msg,
                    Keywords = keys
                };
                AudioRecogResultArrivedEvent?.Invoke(null, audioData);
            }
        }
        catch
        {
            // ignored
        }
    }

    #endregion
}