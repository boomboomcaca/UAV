using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.StreamStorage;

public partial class StreamStorage : DeviceBase
{
    private CancellationTokenSource _cts;
    private string _dataPath;
    private string _fileName = string.Empty;
    private NamedPipeClientStream _pipeClient = null!;
    private NamedPipeServerStream _pipeStream;
    private StreamStorageMode _preMode = StreamStorageMode.None;
    private Task _recvDataTask;
    private Stream _recvStream;
    private Stream _sendStream;
    private SimDevice _simDev;
    private Socket _socket;

    /// <summary>
    ///     开始记录时间
    /// </summary>
    private ulong _startTime;

    public StreamStorage(Guid deviceId) : base(deviceId)
    {
    }

    public override bool Initialized(ModuleInfo device)
    {
        if (!base.Initialized(device)) return false;
        try
        {
            if (IsSim)
            {
                ConnectMode = false;
                _simDev = new SimDevice();
                _simDev.Start(Address);
            }

            _cts = new CancellationTokenSource();
            _recvDataTask = new Task(p => RecvDataMsgAsync(p).ConfigureAwait(false), _cts.Token);
            _recvDataTask.Start();
            if (!ConnectMode)
            {
                var server = $"{Address}_Reply";
                _pipeStream = new NamedPipeServerStream(server, PipeDirection.InOut, 1);
                _ = _pipeStream.WaitForConnectionAsync();
                _recvStream = _pipeStream;
                var client = Address;
                _pipeClient = new NamedPipeClientStream(client);
                _pipeClient.Connect(1000);
                _sendStream = _pipeClient;
            }
            else
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var ep = new IPEndPoint(IPAddress.Parse(Address), Port);
                _socket.Connect(ep);
                _sendStream = new NetworkStream(_socket);
                _recvStream = _sendStream;
            }

            return true;
        }
        catch
        {
            ReleaseSource();
            return false;
        }
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        CheckSsMode();
    }

    public override void SetParameter(string name, object value)
    {
        base.SetParameter(name, value);
        if (TaskState == TaskState.Start)
        {
            if (name.Equals("ssmode", StringComparison.OrdinalIgnoreCase)) CheckSsMode();
            if (name.Equals("recordId", StringComparison.OrdinalIgnoreCase)) CheckSsMode();
            if (name.Equals("progressIndex", StringComparison.OrdinalIgnoreCase))
                if (_ssMode == StreamStorageMode.Playback)
                {
                    var cmd = $"progress:playback:{ProgressIndex}";
                    SendCmd(cmd);
                }
        }
    }

    public override void Stop()
    {
        base.Stop();
        if (_ssMode != StreamStorageMode.None)
        {
            _ssMode = StreamStorageMode.None;
            RecordId = string.Empty;
            CheckSsMode();
        }
    }

    public override void Dispose()
    {
        Stop();
        ReleaseSource();
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    private void ReleaseSource()
    {
        try
        {
            _cts?.Cancel();
            _recvDataTask?.Dispose();
        }
        catch
        {
        }

        try
        {
            _simDev?.Stop();
        }
        catch
        {
        }

        try
        {
            _sendStream?.Dispose();
            _recvStream?.Dispose();
        }
        catch
        {
        }

        try
        {
            _socket?.Dispose();
        }
        catch
        {
        }

        try
        {
            _pipeStream?.Dispose();
            _pipeClient?.Dispose();
        }
        catch
        {
        }

        try
        {
            _simDev?.Stop();
        }
        catch
        {
        }
    }

    private async Task RecvDataMsgAsync(object obj)
    {
        if (obj is not CancellationToken token) return;
        var buffer = new byte[1024];
        var connected = false;
        while (!token.IsCancellationRequested)
            try
            {
                if (!ConnectMode)
                {
                    if (_pipeStream?.IsConnected != true)
                    {
                        await Task.Yield();
                        if (connected)
                        {
                            connected = false;
                            Console.WriteLine("应答管道连接中断");
                        }

                        continue;
                    }

                    if (!connected)
                    {
                        connected = true;
                        Console.WriteLine("应答管道已连接");
                    }
                }

                var count = await _recvStream!.ReadAsync(buffer);
                if (count == 0)
                {
                    await Task.Yield();
                    continue;
                }

                var str = Encoding.ASCII.GetString(buffer, 0, count);
                var cmds = str.Split(';');
                Console.WriteLine($"收到消息:{str}");
                foreach (var cmd in cmds)
                {
                    if (string.IsNullOrEmpty(cmd)) continue;
                    var array = cmd.Split(":");
                    if (array.Length < 2) continue;
                    switch (array[0].ToLower())
                    {
                        case "start":
                        {
                            if (array.Length < 3) break;
                            if (!array[1].Equals("record", StringComparison.OrdinalIgnoreCase)) break;
                            var datapath = array[2];
                            if (datapath.Equals("false", StringComparison.OrdinalIgnoreCase)) break;
                            _startTime = Utils.GetNowTimestamp();
                            _dataPath = datapath;
                            var notify = new FileSavedNotification
                            {
                                NotificationType = FileNotificationType.Created,
                                TaskId = DataPort?.TaskId.ToString(),
                                RootPath = datapath.Replace("\\", "/"),
                                ComputerId = RunningInfo.ComputerId,
                                FileName = _fileName,
                                DataType = FileDataType.Ssd,
                                Parameters = "",
                                BeginRecordTime = _startTime,
                                EndRecordTime = 0,
                                LastModifiedTime = _startTime,
                                RecordCount = 0,
                                PluginId = "",
                                RelativePath = "",
                                Size = 0
                            };
                            if (DataPort is DriverBase driver) notify.DriverId = driver.Id.ToString();
                            SendData(new List<object> { notify });
                        }
                            break;
                        case "record":
                            if (array.Length < 2) break;
                            if (array[1].Equals("complete", StringComparison.OrdinalIgnoreCase))
                            {
                                if (array.Length < 4) break;
                                if (!long.TryParse(array[2], out var dataCnt)
                                    || !long.TryParse(array[3], out var size))
                                    break;
                                var notify = new FileSavedNotification
                                {
                                    NotificationType = FileNotificationType.Modified,
                                    TaskId = DataPort?.TaskId.ToString(),
                                    RootPath = _dataPath.Replace("\\", "/"),
                                    ComputerId = RunningInfo.ComputerId,
                                    FileName = _fileName,
                                    DataType = FileDataType.Ssd,
                                    Parameters = "",
                                    BeginRecordTime = _startTime,
                                    EndRecordTime = Utils.GetNowTimestamp(),
                                    LastModifiedTime = Utils.GetNowTimestamp(),
                                    RecordCount = dataCnt,
                                    Size = size
                                };
                                if (DataPort is DriverBase driver) notify.DriverId = driver.Id.ToString();
                                SendData(new List<object> { notify });
                            }
                            else if (array[1].Equals("drop", StringComparison.OrdinalIgnoreCase))
                            {
                                var notify = new FileSavedNotification
                                {
                                    NotificationType = FileNotificationType.Delete,
                                    TaskId = DataPort?.TaskId.ToString(),
                                    RootPath = _dataPath.Replace("\\", "/"),
                                    ComputerId = RunningInfo.ComputerId,
                                    FileName = _fileName,
                                    DataType = FileDataType.Ssd,
                                    Parameters = "",
                                    BeginRecordTime = _startTime,
                                    EndRecordTime = Utils.GetNowTimestamp(),
                                    LastModifiedTime = Utils.GetNowTimestamp(),
                                    RecordCount = 0,
                                    Size = 0
                                };
                                if (DataPort is DriverBase driver) notify.DriverId = driver.Id.ToString();
                                SendData(new List<object> { notify });
                            }

                            break;
                        case "progress":
                            if (array.Length < 2) break;
                            if (!int.TryParse(array[1], out var progress)) break;
                            var sp = new SDataPlaybackProgress
                            {
                                Progress = progress
                            };
                            SendData(new List<object> { sp });
                            break;
                    }
                }
            }
            catch
            {
            }
    }

    private void CheckSsMode()
    {
        if (!string.IsNullOrEmpty(RecordId) || _preMode != StreamStorageMode.None)
        {
            if (_preMode == _ssMode) return;
            if (_preMode == StreamStorageMode.Record && _ssMode == StreamStorageMode.Playback) return;
            if (_preMode == StreamStorageMode.Playback && _ssMode == StreamStorageMode.Record) return;
        }

        switch (_ssMode)
        {
            case StreamStorageMode.None:
                if (!string.IsNullOrEmpty(RecordId))
                {
                    // 暂停回放
                    var cmd = "pause:playback";
                    SendCmd(cmd);
                }
                else
                {
                    string cmd;
                    if (_preMode == StreamStorageMode.Record)
                        cmd = "stop:record";
                    else
                        cmd = "stop:playback";
                    SendCmd(cmd);
                }

                break;
            case StreamStorageMode.Record:
            {
                _fileName = Guid.NewGuid().ToString();
                var cmd = $"start:record:{_fileName}";
                SendCmd(cmd);
            }
                break;
            case StreamStorageMode.Playback:
            {
                _fileName = RecordId;
                var cmd = $"start:playback:{_fileName}";
                SendCmd(cmd);
            }
                break;
            case StreamStorageMode.Drop:
            {
                var cmd = "cancel:record";
                SendCmd(cmd);
            }
                break;
        }

        _preMode = _ssMode;
    }

    private void SendCmd(string cmd)
    {
        if (!cmd.EndsWith(';')) cmd += ";";
        var buffer = Encoding.ASCII.GetBytes(cmd);
        _sendStream.Write(buffer);
    }
}