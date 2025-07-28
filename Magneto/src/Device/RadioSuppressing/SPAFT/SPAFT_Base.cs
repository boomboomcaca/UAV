using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.SPAFT;

public partial class Spaft
{
    private void SendCmd(Socket socket, byte[] cmdBytes)
    {
        var head = BitConverter.GetBytes(Head);
        var tail = BitConverter.GetBytes(Tail);
        var reserved = new byte[] { 0x00 };
        var offset = 0;
        var dataToSend = new byte[head.Length + cmdBytes.Length + reserved.Length + tail.Length];
        Buffer.BlockCopy(head, 0, dataToSend, offset, head.Length);
        offset += head.Length;
        Buffer.BlockCopy(cmdBytes, 0, dataToSend, offset, cmdBytes.Length);
        offset += cmdBytes.Length;
        Buffer.BlockCopy(reserved, 0, dataToSend, offset, reserved.Length);
        offset += reserved.Length;
        Buffer.BlockCopy(tail, 0, dataToSend, offset, tail.Length);
        var totalLength = dataToSend.Length;
        var sentLength = 0;
        var sentOffset = 0;
        try
        {
            while (sentLength < totalLength)
            {
                var sentBufferLength = socket.Send(dataToSend, sentOffset, totalLength - sentLength, SocketFlags.None);
                sentOffset += sentBufferLength;
                sentLength += sentBufferLength;
            }
        }
        catch
        {
        }
    }

    private void SendCmdToLowRange(byte[] cmdBytes, bool isPrint = false)
    {
        if (_lowSocket?.Connected != true)
        {
            SetDeviceError();
            return;
        }

        try
        {
            _lowResetEvent.WaitOne();
            if (isPrint) Trace.WriteLine($"低端-->{BitConverter.ToString(cmdBytes)}");
            SendCmd(_lowSocket, cmdBytes);
        }
        finally
        {
            _lowResetEvent.Set();
        }
    }

    private void SendCmdToHighRange(byte[] cmdBytes, bool isPrint = false)
    {
        if (_highSocket?.Connected != true)
        {
            SetDeviceError();
            return;
        }

        try
        {
            _highResetEvent.WaitOne();
            if (isPrint) Trace.WriteLine($"高端-->{BitConverter.ToString(cmdBytes)}");
            SendCmd(_highSocket, cmdBytes);
        }
        finally
        {
            _highResetEvent.Set();
        }
    }

    private bool SendCmdToLowRange(byte[] cmdBytes, byte[] successBytes, int timeout = 3000)
    {
        if (cmdBytes == null || cmdBytes.Length < 1) return false;
        if (_lowSocket?.Connected != true)
        {
            SetDeviceError();
            return false;
        }

        var cmdKey = cmdBytes[0];
        _isLowSync = true;
        _lowSyncData.Clear();
        var flag = false;
        _lowResetEvent.WaitOne();
        var stopWatch = new Stopwatch();
        try
        {
            SendCmd(_lowSocket, cmdBytes);
            stopWatch.Start();
            while (stopWatch.ElapsedMilliseconds < timeout)
            {
                var temp = _lowSyncData.ToList().FirstOrDefault(p => p.cmdKey == cmdKey);
                if (temp.cmdKey == cmdKey)
                {
                    flag = temp.content.SequenceEqual(successBytes);
                    break;
                }

                Thread.Sleep(10);
            }

            stopWatch.Stop();
        }
        finally
        {
            stopWatch.Reset();
            _isLowSync = false;
            _lowResetEvent.Set();
        }

        return flag;
    }

    private bool SendCmdToHighRange(byte[] cmdBytes, byte[] successBytes, int timeout = 3000)
    {
        if (cmdBytes == null || cmdBytes.Length < 1) return false;
        if (_highSocket?.Connected != true)
        {
            SetDeviceError();
            return false;
        }

        var cmdKey = cmdBytes[0];
        _isHighSync = true;
        _highSyncData.Clear();
        var stopWatch = new Stopwatch();
        var flag = false;
        _highResetEvent.WaitOne();
        try
        {
            SendCmd(_highSocket, cmdBytes);
            stopWatch.Start();
            while (stopWatch.ElapsedMilliseconds < timeout)
            {
                var temp = _highSyncData.ToList().FirstOrDefault(p => p.cmdKey == cmdKey);
                if (temp.cmdKey == cmdKey)
                {
                    flag = temp.content.SequenceEqual(successBytes);
                    break;
                }

                Thread.Sleep(10);
            }

            stopWatch.Stop();
        }
        finally
        {
            stopWatch.Reset();
            _isHighSync = false;
            _highResetEvent.Set();
        }

        return flag;
    }

    private void UpdatePowerMap(List<SDataRadioSuppressing> datas)
    {
        Thread.Sleep(1000);
        if (datas?.Any() == true) SendData(datas.Cast<object>().ToList());
    }

    private void SetDeviceError()
    {
        var info = new SDataMessage
        {
            LogType = LogType.Warning,
            ErrorCode = (int)InternalMessageType.DeviceRestart,
            Description = DeviceId.ToString(),
            Detail = DeviceInfo.DisplayName
        };
        SendMessage(info);
    }

    #region 初始化

    private void InitMisc()
    {
        _audioFiles =
            Directory.GetFiles(
                Directory.Exists(_audioDirectory) ? _audioDirectory : AppDomain.CurrentDomain.BaseDirectory, "*.wav",
                SearchOption.TopDirectoryOnly);
        if (_audioFiles.Length == 0)
            _audioIndex = -1;
        else
            _audioIndex = _audioIndex == -1
                ? new Random().Next(_audioFiles.Length)
                : _audioIndex % _audioFiles.Length;
        _audioSendingEvent = new ManualResetEvent(false);
        _lowResetEvent = new AutoResetEvent(true);
        _highResetEvent = new AutoResetEvent(true);
    }

    private void InitNetworks()
    {
        _lowSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true,
            SendTimeout = 3000,
            ReceiveTimeout = 3000,
            LingerState = new LingerOption(true, 1)
        };
        _lowSocket.Connect(LowRangeIp, LowRangePort);
        _highSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true,
            SendTimeout = 3000,
            ReceiveTimeout = 3000,
            LingerState = new LingerOption(true, 1)
        };
        _highSocket.Connect(HighRangeIp, HighRangePort);
    }

    private void InitWorks()
    {
        _queryPowerInfoCts = new CancellationTokenSource();
        _queryPowerInfoTask = new Task(QueryPowerInfo, _queryPowerInfoCts.Token);
        _queryPowerInfoTask.Start();
        _lowReceiveCts = new CancellationTokenSource();
        _lowReceiveTask = new Task(LowRangeReceiveData, _lowReceiveCts.Token);
        _lowReceiveTask.Start();
        _highReceiveCts = new CancellationTokenSource();
        _highReceiveTask = new Task(HighRangeReceiveData, _highReceiveCts.Token);
        _highReceiveTask.Start();
        _lowParseCts = new CancellationTokenSource();
        _lowParseTask = new Task(ParseLowRangeData, _lowParseCts.Token);
        _lowParseTask.Start();
        _highParseCts = new CancellationTokenSource();
        _highParseTask = new Task(ParseHighRangeData, _highParseCts.Token);
        _highParseTask.Start();
    }

    #endregion

    #region 资源释放

    private void ReleaseResources()
    {
        ReleaseWorks();
        ReleaseNetworks();
        _lowResetEvent?.Dispose();
        _highResetEvent?.Dispose();
        _audioSendingEvent?.Dispose();
        _audioStream?.Dispose();
    }

    private void ReleaseNetworks()
    {
        Utils.CloseSocket(_lowSocket);
        Utils.CloseSocket(_highSocket);
    }

    private void ReleaseWorks()
    {
        Utils.CancelTask(_sendAudioStreamTask, _sendAudioStreamCts);
        Utils.CancelTask(_queryPowerInfoTask, _queryPowerInfoCts);
        Utils.CancelTask(_lowParseTask, _lowParseCts);
        Utils.CancelTask(_highParseTask, _highParseCts);
        Utils.CancelTask(_lowReceiveTask, _lowReceiveCts);
        Utils.CancelTask(_highReceiveTask, _highReceiveCts);
    }

    #endregion

    #region Helper

    private void SetChannelFrequencyEnds(RftxSegmentsTemplate[] frequencies)
    {
        if (frequencies == null || !frequencies.Any()) return;
        var frequencyKeyValueCollections = from frequency in frequencies
            where frequency.MinFrequency > 0 && frequency.MaxFrequency > 0
            select new
            {
                frequency,
                frequency.PhysicalChannelNumber,
                CheckedMinFrequency = (long)(frequency.MinFrequency * 1000000L),
                CheckedMaxFrequency = (long)(frequency.MaxFrequency * 1000000L),
                MinFrequency = frequency.Modulation is Modulation.Fm or Modulation._2FSK or Modulation._4FSK
                    ? (long)(frequency.MinFrequency * 1000000L) - (long)(frequency.Bandwidth * 1000 / 2)
                    : (long)(frequency.MinFrequency * 1000000L),
                MaxFrequency = frequency.Modulation is Modulation.Fm or Modulation._2FSK or Modulation._4FSK
                    ? (long)(frequency.MaxFrequency * 1000000L) + (long)(frequency.Bandwidth * 1000 / 2)
                    : (long)(frequency.MaxFrequency * 1000000L)
            }
            into channelFrequencyEnds
            group channelFrequencyEnds by channelFrequencyEnds.PhysicalChannelNumber
            into groupedChannelFrequencyEnds
            select new KeyValuePair<int, Tuple<long, long, long, long>>(
                groupedChannelFrequencyEnds.Key,
                new Tuple<long, long, long, long>(
                    groupedChannelFrequencyEnds.Min(item => item.CheckedMinFrequency),
                    groupedChannelFrequencyEnds.Max(item => item.CheckedMaxFrequency),
                    groupedChannelFrequencyEnds.Min(item => item.MinFrequency),
                    groupedChannelFrequencyEnds.Max(item => item.MaxFrequency)
                )
            );
        _channelFrequencyEnds = new Dictionary<int, Tuple<long, long, long, long>>();
        foreach (var keyValue in frequencyKeyValueCollections) _channelFrequencyEnds.Add(keyValue.Key, keyValue.Value);
    }

    private void SendCommand(int deviceNumber, byte[] cmd, bool isPrint = false)
    {
        if (cmd == null || cmd.Length == 0) return;
        if (deviceNumber == 0)
            SendCmdToLowRange(cmd, isPrint);
        else if (deviceNumber == 1) SendCmdToHighRange(cmd, isPrint);
    }

    #endregion
}