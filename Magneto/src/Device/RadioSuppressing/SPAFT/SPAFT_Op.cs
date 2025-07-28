using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Magneto.Device.SPAFT;

public partial class Spaft
{
    #region 触发与设置

    private void RaiseDeviceQueryingOrSetting(Func<byte[]> func)
    {
        var frame = func.Invoke();
        //高端和低端一起发的时候，先发高端的消息比较好
        SendCommand(1, frame);
        SendCommand(0, frame);
    }

    private void RaiseChannelParameterSetting<T>(Func<T, Dictionary<int, List<byte[]>>> func, T args)
    {
        var frames = func.Invoke(args);
        if (frames?.Any() != true) return;
        //高端和低端一起发的时候，先发高端的消息比较好
        var dic = frames.OrderByDescending(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
        foreach (var frame in dic)
            if (frame.Value != null)
                foreach (var item in frame.Value)
                {
                    SendCommand(frame.Key / 3, item, true);
                    Thread.Sleep(20);
                }
    }

    private void RaiseAudioSetting(bool enabled, int audioIndex)
    {
        if (!enabled || audioIndex == -1)
        {
            _audioSendingEvent.Reset();
        }
        else
        {
            _audioSendingEvent.Reset();
            lock (_audioLock)
            {
                _audioStream?.Close();
                var fileName = _audioFiles[_audioIndex];
                _audioStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            }

            _audioSendingEvent.Set();
        }
    }

    private void ResetChannelSetting()
    {
        if (_rftxSegments == null || _rftxSegments.Length == 0) return;
        foreach (var frequency in _rftxSegments) frequency.RftxSwitch = false;
        RaiseChannelParameterSetting(ToLocalOscillatorFrame, _rftxSegments);
        Thread.Sleep(LockMs);
        RaiseChannelParameterSetting(ToFrequencyListFrame, _rftxSegments);
    }

    #endregion
}