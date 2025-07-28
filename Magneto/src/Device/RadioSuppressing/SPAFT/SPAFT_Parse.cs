using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Magneto.Device.SPAFT;

public partial class Spaft
{
    private void LowRangeReceiveData()
    {
        var buffer = new byte[4096];
        while (_lowReceiveCts?.IsCancellationRequested == false)
            try
            {
                if (_lowSocket == null) break;
                if (!_lowSocket.Connected)
                {
                    Thread.Sleep(100);
                    continue;
                }

                var count = _lowSocket.Receive(buffer, SocketFlags.None);
                if (count == 0)
                {
                    Task.Delay(10, _lowReceiveCts.Token).ConfigureAwait(false).GetAwaiter().GetResult();
                    continue;
                }

                for (var i = 0; i < count; i++) _lowDataQueue.Enqueue(buffer[i]);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode is SocketError.Disconnecting or SocketError.NotConnected or SocketError.Shutdown)
                {
                    Trace.WriteLine($"低端Socket异常，异常信息：{e.SocketErrorCode}");
                    SetDeviceError();
                    break;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"低端接收数据异常，异常信息：{ex}");
            }
    }

    private void HighRangeReceiveData()
    {
        var buffer = new byte[4096];
        while (_highReceiveCts?.IsCancellationRequested == false)
            try
            {
                if (_highSocket == null) break;
                if (!_highSocket.Connected)
                {
                    Thread.Sleep(100);
                    continue;
                }

                var count = _highSocket.Receive(buffer, SocketFlags.None);
                if (count == 0)
                {
                    Task.Delay(10, _highReceiveCts.Token).ConfigureAwait(false).GetAwaiter().GetResult();
                    continue;
                }

                for (var i = 0; i < count; i++) _highDataQueue.Enqueue(buffer[i]);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode is SocketError.Disconnecting or SocketError.NotConnected or SocketError.Shutdown)
                {
                    Trace.WriteLine($"高端Socket异常，异常信息：{e.SocketErrorCode}");
                    SetDeviceError();
                    break;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"高端接收数据异常，异常信息：{ex}");
            }
    }

    private void ParseLowRangeData()
    {
        var isData = false;
        var hasHead = false;
        var tempTag = new Queue<byte>();
        var tempData = new List<byte>();
        while (_lowParseCts?.IsCancellationRequested == false)
            try
            {
                var b = _lowDataQueue.TryDequeue(out var bt);
                if (!b)
                {
                    Task.Delay(10, _lowParseCts.Token).ConfigureAwait(false).GetAwaiter().GetResult();
                    continue;
                }

                while (tempTag.Count >= 4) tempTag.Dequeue();
                tempTag.Enqueue(bt);
                if (tempTag.Count < 4) continue;
                if (hasHead) tempData.Add(bt);
                var tag = BitConverter.ToUInt32(tempTag.ToArray());
                if (tag == Head)
                {
                    tempData.Clear();
                    tempData.AddRange(tempTag);
                    isData = false;
                    hasHead = true;
                }
                else if (tag == Tail)
                {
                    isData = hasHead;
                    hasHead = false;
                }

                if (isData)
                {
                    var legal = ParseData(tempData.ToArray(), out var cmdKey, out var content);
                    if (!legal || content == null) continue;
                    if (_isLowSync) _lowSyncData.Enqueue((cmdKey, content));
                    ParsePowerInfo(0, cmdKey, content);
                    isData = false;
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"低端解析数据异常，异常信息：{ex}");
            }
    }

    private void ParseHighRangeData()
    {
        var isData = false;
        var hasHead = false;
        var tempTag = new Queue<byte>();
        var tempData = new List<byte>();
        while (_highParseCts?.IsCancellationRequested == false)
            try
            {
                var b = _highDataQueue.TryDequeue(out var bt);
                if (!b)
                {
                    Task.Delay(10, _highParseCts.Token).ConfigureAwait(false).GetAwaiter().GetResult();
                    continue;
                }

                while (tempTag.Count >= 4) tempTag.Dequeue();
                tempTag.Enqueue(bt);
                if (tempTag.Count < 4) continue;
                if (hasHead) tempData.Add(bt);
                var tag = BitConverter.ToUInt32(tempTag.ToArray());
                if (tag == Head)
                {
                    tempData.Clear();
                    tempData.AddRange(tempTag);
                    isData = false;
                    hasHead = true;
                }
                else if (tag == Tail)
                {
                    isData = hasHead;
                    hasHead = false;
                }

                if (isData)
                {
                    var legal = ParseData(tempData.ToArray(), out var cmdKey, out var content);
                    if (!legal || content == null) continue;
                    if (_isHighSync) _highSyncData.Enqueue((cmdKey, content));
                    ParsePowerInfo(1, cmdKey, content);
                    isData = false;
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"高端解析数据异常，异常信息：{ex}");
            }
    }

    private static bool ParseData(byte[] data, out byte cmdKey, out byte[] content)
    {
        cmdKey = 0;
        content = null;
        if (data == null || data.Length < 14) return false;
        var offset = 0;
        var head = BitConverter.ToUInt32(data, offset);
        offset += 4;
        if (head != Head) return false;
        cmdKey = data[offset];
        offset += 1;
        var length = BitConverter.ToInt32(data, offset);
        offset += 4;
        if (offset + length + 1 + 4 > data.Length) return false;
        if (length > 0)
        {
            content = new byte[length];
            Buffer.BlockCopy(data, offset, content, 0, length);
            offset += length;
        }

        offset += 1; //保留字段 1字节
        var tail = BitConverter.ToUInt32(data, offset);
        if (tail != Tail) return false;
        return true;
    }

    private void ParsePowerInfo(int deviceNumber, byte cmdKey, byte[] content)
    {
        //Console.WriteLine($"L:{BitConverter.ToString(content)}");
        switch (cmdKey)
        {
            case 0x55:
                if (content.Length < 9) return;
                for (var index = 0; index < 3; ++index)
                {
                    var channel = deviceNumber * 3 + index;
                    var b = _channelStateMap.TryGetValue(channel, out var state);
                    if (!b || !state) continue;
                    b = _channelPowerMap.TryGetValue(channel, out var data);
                    if (!b) continue;
                    var power = BitConverter.ToInt16(content, index * 3 + 1) / 10.0f;
                    if (_powers != null && channel < _powers.Length)
                    {
                        var setPower = _powers[channel];
                        if (power > 0 && Math.Abs(power - setPower) <= 1)
                            data.Power = power;
                        else
                            data.Power = setPower;
                    }
                    else
                    {
                        data.Power = power;
                    }
                    //data.Power = BitConverter.ToInt16(content, index * 3 + 1) / 10.0f;
                    // Console.WriteLine($"通道{channel}功率： {data.Power}\t");
                }

                break;
            case 0x58:
                if (content.Length < 3) return;
                for (var index = 0; index < 3; ++index)
                {
                    var channel = deviceNumber * 3 + index;
                    var b = _channelPowerMap.TryGetValue(channel, out var data);
                    if (!b) continue;
                    data.Vsw = (content[index] & 0x01) == 0x01;
                    data.Warning = (content[index] & 0x02) == 0x02 ? "告警" : string.Empty;
                    //var powerEnabled = (content[index] & 0x04) == 0x04;
                    //if (powerEnabled)
                    //{
                    //    continue;
                    //}
                    //data.Power = -1;
                }

                break;
        }

        SendData(_channelPowerMap.Select(p => (object)p.Value).ToList());
    }

    #region 数据查询与获取

    private void QueryPowerInfo()
    {
        while (_queryPowerInfoCts?.IsCancellationRequested == false)
            try
            {
                RaiseDeviceQueryingOrSetting(ToQueryPowerStatusFrame);
                Thread.Sleep(50);
                RaiseDeviceQueryingOrSetting(ToQueryPowerValueFrame);
                Thread.Sleep(1000);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
    }

    private void SendAudioStream()
    {
        while (_sendAudioStreamCts?.IsCancellationRequested == false)
        {
            _audioSendingEvent.WaitOne();
            var audioBuffer = new byte[4096];
            try
            {
                lock (_audioLock)
                {
                    if (_audioIndex == -1)
                    {
                        Thread.Sleep(500);
                        Trace.WriteLine("没有音频文件");
                        continue;
                    }

                    if (_audioStream == null)
                    {
                        var fileName = _audioFiles[_audioIndex];
                        _audioStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    }

                    var length = _audioStream.Read(audioBuffer, 0, 2756);
                    if (length == 0)
                    {
                        _audioStream.Seek(0, SeekOrigin.Begin);
                        Thread.Sleep(80);
                        continue;
                    }

                    var bufferToSend = new byte[length];
                    Buffer.BlockCopy(audioBuffer, 0, bufferToSend, 0, length);
                    var frame = ToAudioFrame(bufferToSend);
                    SendCommand(0, frame);
                    SendCommand(1, frame);
                    Thread.Sleep(50); //警示语音一定要有间隔，大概50ms
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"发送音频流异常，异常信息：{ex}");
            }
        }
    }

    #endregion
}