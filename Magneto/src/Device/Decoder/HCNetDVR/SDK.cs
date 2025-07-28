/*********************************************************************************************
 *
 * 文件名称:    ..\Tracker800\Server\Source\Device\Decoder\HCNetDVR\SDK.cs
 *
 * 作    者:	王 喜 进
 *
 * 创作日期:    2018/07/13
 *
 * 修    改:
 *
 * 备    注:	录播盒驱动模块，实现SDK调用。
 *
 *********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.HCNetDVR;

public partial class HcNetDvr
{
    /// <summary>
    ///     字符串对象
    /// </summary>
    private string _str;

    /// <summary>
    ///     发送操作结果信息
    /// </summary>
    /// <param name="operateType"></param>
    /// <param name="result"></param>
    /// <param name="msg"></param>
    /// <param name="frequency"></param>
    /// <param name="standard"></param>
    /// <param name="programNumber"></param>
    /// <param name="programName"></param>
    private void SendControlResult(OperateType operateType, bool result, string msg, double frequency = 0d,
        string standard = "", int programNumber = 0, string programName = "")
    {
        var pr = new SDataPlayResult
        {
            OperateType = operateType,
            Frequency = frequency,
            Standard = standard,
            ProgramNumber = programNumber,
            ProgramName = programName,
            Result = result,
            Uri = msg
        };
        // 发送文件信息到客户端
        SendData(new List<object> { pr });
    }

    /// <summary>
    ///     异步判断路径是否创建了m3u8文件(前端没有此文件会播放失败)
    /// </summary>
    private async Task<bool> CheckFileCreateAsync()
    {
        var path = Path.Combine(_pushUri, "index.m3u8");
        var isCreated = false;
        var count = 0;
        while (!isCreated)
        {
            await Task.Delay(100).ConfigureAwait(false);
            isCreated = File.Exists(path);
            count++;
            if (count > 150) break;
        }

        await Task.Delay(2000).ConfigureAwait(false);
        return isCreated;
    }

    #region 实时预览

    private async Task StartRealPlayAsync()
    {
        StopPlayBackDvr();
        if (!StopReceiveData())
        {
            SendControlResult(OperateType.RealPlay, false, "停止上次的播放失败", _currentFrequency, _currentStandard.ToString(),
                _currentNumber, _currentProgramName);
            return;
        }

        _currentUriId = Guid.NewGuid().ToString("N");
        _pushUri = Path.Combine(RunningInfo.VideoDir, _currentUriId);
        if (!Directory.Exists(_pushUri)) Directory.CreateDirectory(_pushUri);
        _pushStream.Initialized(_pushUri, UserName, Password, Ip);
        _pushStream.StartPush();
        var time = DateTime.Now;
        var isFileCreated = await CheckFileCreateAsync().ConfigureAwait(false);
        var uri = $"{_videoUriHeader}/{_currentUriId}/index.m3u8";
        var span = DateTime.Now.Subtract(time).TotalSeconds;
        Trace.WriteLine($"等待创建播放文件的时间:{span}秒");
        if (!isFileCreated)
        {
            StopReceiveData();
            uri = "读取播放文件失败,请重试！";
        }

        _avPlaying = true;
        _playType = PlayTypeEnum.RealTime;
        SendControlResult(OperateType.RealPlay, isFileCreated, uri, _currentFrequency, _currentStandard.ToString(),
            _currentNumber, _currentProgramName);
    }

    /// <summary>
    ///     停止接收数据
    /// </summary>
    private bool StopReceiveData()
    {
        Trace.WriteLine("停止实时视频预览...");
        ResetCacheData();
        _playType = PlayTypeEnum.None;
        ClearVideoDir();
        return true;
    }

    #endregion

    #region 录像

    /// <summary>
    ///     启动、停止录像
    /// </summary>
    /// <param name="record"></param>
    private async Task StartStopDvrRecordAsync(bool record)
    {
        if (record)
            // 播放的画面比录播盒晚6秒，因此这里要做补偿
            _startDvrTime = DateTime.Now.AddSeconds(-6);
        else
            _stopDvrTime = DateTime.Now.AddSeconds(-6);
        await AddDvrFileToCloudAsync(record, true, _str).ConfigureAwait(false);
    }

    private async Task AddDvrFileToCloudAsync(bool record, bool dvrResult, string msg)
    {
        if (dvrResult && !record)
        {
            var name = $"{_startDvrTime:yyyyMMddHHmmss}-{_stopDvrTime:yyyyMMddHHmmss}";
            var file = new DvrFileInfo
            {
                EdgeId = RunningInfo.EdgeId,
                FileName = name,
                ProgramName = _currentProgramName, // 这里临时使用节目号
                Frequency = _currentFrequency,
                Standard = _currentStandard.ToString(),
                StartTime = Utils.GetTimestamp(_startDvrTime),
                StopTime = Utils.GetTimestamp(_stopDvrTime)
            };
            await CloudClient.Instance.AddDvrFileToCloudAsync(file).ConfigureAwait(false);
        }

        var operate = record ? OperateType.RecordStart : OperateType.RecordStop;
        SendControlResult(operate, dvrResult, msg, _currentFrequency, _currentStandard.ToString(), _currentNumber);
    }

    #endregion

    #region 录像回放

    private DateTime _preSendPlayTime = DateTime.Now;
    private DvrFileInfo _currentDvr;

    /// <summary>
    ///     当前回放的文件的总时长 单位 秒
    /// </summary>
    private int _totalDvrTime;

    /// <summary>
    ///     开始回放时间
    ///     进行回放进度控制使用
    /// </summary>
    private DateTime _startPlaybackTime = DateTime.Now;

    /// <summary>
    ///     录像的起始时间
    /// </summary>
    private DateTime _fileBeginTime = DateTime.Now;

    /// <summary>
    ///     等待回放的时间
    ///     这个时间需要加到音频识别上去
    /// </summary>
    private double _waitReplayTime;

    /// <summary>
    ///     录像回放
    ///     这里暂时改为按时间回放
    /// </summary>
    private async Task PlayBackByNameAsync()
    {
        // 如果已经正在回放，先停止回放
        StopPlayBackDvr();
        Trace.WriteLine($"开始回放{_playBackFileName}");
        _totalDvrTime = 0;
        DateTime start;
        DateTime stop;
        if (!_playBackFileName.Contains("|"))
        {
            _currentDvr = _dvrFilesCache.Find(item => item.FileName == _playBackFileName);
            if (_currentDvr.FileName != _playBackFileName)
            {
                _str = $"按文件名回放失败，找不到录像文件{_playBackFileName}";
                SendControlResult(OperateType.Playback, false, _str);
                return;
            }

            start = Utils.GetTimeByTicks(_currentDvr.StartTime);
            stop = Utils.GetTimeByTicks(_currentDvr.StopTime);
            _totalDvrTime = (int)stop.Subtract(start).TotalSeconds;
        }
        else
        {
            var split = _playBackFileName.Split('|');
            if (!ulong.TryParse(split[0], out var num1) || !ulong.TryParse(split[1], out var num2))
            {
                _str = $"回放失败，转换时间失败{_playBackFileName}";
                SendControlResult(OperateType.Playback, false, _str);
                return;
            }

            start = Utils.GetTimeByTicks(num1).ToLocalTime();
            stop = Utils.GetTimeByTicks(num2).ToLocalTime();
            _totalDvrTime = (int)stop.Subtract(start).TotalSeconds;
        }

        _fileBeginTime = start;
        _currentUriId = Guid.NewGuid().ToString("N");
        _pushUri = Path.Combine(RunningInfo.VideoDir, _currentUriId);
        if (!Directory.Exists(_pushUri)) Directory.CreateDirectory(_pushUri);
        _pushStream.Initialized(_pushUri, UserName, Password, Ip, start, stop);
        _pushStream.StartPush();
        var time = DateTime.Now;
        var isFileCreated = await CheckFileCreateAsync().ConfigureAwait(false);
        var uri = $"{_videoUriHeader}/{_currentUriId}/index.m3u8";
        // var uri = $"{_videoUriHeader}/index.m3u8";
        var span = DateTime.Now.Subtract(time).TotalSeconds;
        _waitReplayTime = span;
        Trace.WriteLine($"等待创建播放文件的时间:{span}秒");
        if (!isFileCreated)
        {
            StopReceiveData();
            uri = "读取播放文件失败,请重试！";
        }

        _startPlaybackTime = DateTime.Now;
        _avPlaying = true;
        _playType = PlayTypeEnum.Playback;
        SendControlResult(OperateType.Playback, isFileCreated, uri);
    }

    private int GetCurrentTime(int pos)
    {
        return (int)(_totalDvrTime * pos / 100d);
    }

    /// <summary>
    ///     停止录像回放
    /// </summary>
    private void StopPlayBackDvr()
    {
        Trace.WriteLine("停止回放视频预览...");
        ResetCacheData();
        // 停止回放之后，切换回实时预览模式
        _playType = PlayTypeEnum.None;
        ClearVideoDir();
    }

    #endregion
}