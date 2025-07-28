using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

#pragma warning disable VSTHRD002
namespace Magneto.Device.FTAS3500;

public partial class Ftas3500 : DeviceBase, IDataPort
{
    #region 构造函数

    public Ftas3500(Guid deviceId) : base(deviceId)
    {
    }

    #endregion

    public Guid TaskId => Guid.Empty;

    #region 重写父类方法

    public override bool Initialized(ModuleInfo moduleInfo)
    {
        try
        {
            var result = base.Initialized(moduleInfo);
            if (result)
            {
                InitMiscs();
                InitNetworks();
                InitWorks();
                SetHeartBeat(_ctrlChannel);
            }

            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            ReleaseResources();
            return false;
        }
    }

    #endregion

    #region 成员变量

    private Socket _ctrlChannel;
    private Socket _localDataChannel;
    private Socket _remoteDataChannel;
    private Task[] _taskArray;
    private CancellationTokenSource _cancelTokenSource;
    private MQueue<byte[]> _buffer;
    private MQueue<SDataIq> _sendIqCache;

    #endregion

    #region 初始化方法

    private void InitMiscs()
    {
        _buffer = new MQueue<byte[]>();
        _sendIqCache = new MQueue<SDataIq>();
        _cancelTokenSource = new CancellationTokenSource();
    }

    private void InitNetworks()
    {
        _ctrlChannel = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _ctrlChannel.Connect(Ip, Port);
        _ctrlChannel.NoDelay = true;
        if (_ctrlChannel.LocalEndPoint is not IPEndPoint localEndPoint) throw new Exception("无可用网络地址");
        _localDataChannel = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _localDataChannel.Bind(new IPEndPoint(localEndPoint.Address, LocalUdpPort));
        _localDataChannel.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _localDataChannel.Connect(Ip, 0);
        _remoteDataChannel = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _remoteDataChannel.Connect(Ip, RemoteUdpPort);
    }

    private void InitWorks()
    {
        var token = _cancelTokenSource.Token;
        _taskArray = new[]
        {
            UploadIqAsync(token),
            CaptureDataAsync(token),
            DispatchDataAsync(token)
        };
    }

    public override void Dispose()
    {
        ReleaseResources();
        base.Dispose();
    }

    #endregion

    #region 释放资源

    private void ReleaseResources()
    {
        ReleaseWorks();
        ReleaseQueues();
        ReleaseNetworks();
    }

    private void ReleaseWorks()
    {
        try
        {
            _cancelTokenSource?.Cancel();
        }
        catch
        {
        }

        try
        {
            Task.Run(async () => await Task.WhenAll(_taskArray)).GetAwaiter().GetResult();
        }
        catch (AggregateException ex)
        {
            foreach (var ie in ex.Flatten().InnerExceptions) Console.WriteLine(ie.ToString());
        }
        catch
        {
        }
        finally
        {
            _cancelTokenSource?.Dispose();
        }
    }

    private void ReleaseQueues()
    {
        _buffer?.Clear();
        _sendIqCache?.Clear();
    }

    private void ReleaseNetworks()
    {
        _ctrlChannel?.Close();
        _localDataChannel?.Close();
        _remoteDataChannel?.Close();
    }

    #endregion

    #region 数据处理

    private async Task UploadIqAsync(CancellationToken token)
    {
        await Task.Run(() =>
        {
            while (!token.IsCancellationRequested)
            {
                var iq = _sendIqCache.DeQueue(100);
                if (iq == null || (uint)(iq.SamplingRate * 1000) != 320000) continue;
                byte[] dataToSend = null;
                if (iq.Data32 != null)
                    dataToSend = WrappedIq.Wrap((ulong)(iq.Frequency * 1000000), (uint)(iq.Bandwidth * 1000),
                        iq.Data32);
                else if (iq.Data16 != null)
                    dataToSend = WrappedIq.Wrap((ulong)(iq.Frequency * 1000000), (uint)(iq.Bandwidth * 1000),
                        iq.Data16);
                try
                {
                    if (dataToSend != null) _remoteDataChannel.Send(dataToSend, dataToSend.Length, SocketFlags.None);
                }
                catch (Exception ex)
                {
                    if (ex is SocketException)
                    {
                        Console.WriteLine(ex.Message);
                        Thread.Sleep(1000);
                    }
                }
            }
        }, token);
    }

    private async Task CaptureDataAsync(CancellationToken token)
    {
        await Task.Run(() =>
        {
            var buffer = new byte[1024 * 1024];
            while (!token.IsCancellationRequested)
            {
                var receivedCount = _localDataChannel.Receive(buffer);
                if (receivedCount <= 0)
                {
                    Thread.Sleep(1);
                    continue;
                }

                var receivedBuffer = new byte[receivedCount];
                Buffer.BlockCopy(buffer, 0, receivedBuffer, 0, receivedCount);
                _buffer.EnQueue(receivedBuffer);
            }
        }, token);
    }

    private async Task DispatchDataAsync(CancellationToken token)
    {
        await Task.Run(() =>
        {
            while (!token.IsCancellationRequested)
            {
                var buffer = _buffer.DeQueue(100);
                if (buffer == null) continue;
                try
                {
                    var packet = RawPacket.Parse(buffer, 0);
                    if (packet?.DataCollection.Count == 0) continue;
                    ForwardData(packet);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }, token);
    }

    private void ForwardData(RawPacket packet)
    {
        foreach (var data in packet.DataCollection)
            if (data.SignalType >= 23)
                ProcessFraud(data);
            else
                switch (data.SignalType)
                {
                    case 11:
                        ProcessFm(data);
                        break;
                    case 12:
                    case 13:
                        ProcessDmrpdt(data);
                        break;
                    case 14:
                    case 18:
                        ProcessDpmr(data);
                        break;
                    case 15:
                        ProcessTetra(data);
                        break;
                    case 16:
                    case 17:
                        ProcessNxdn(data);
                        break;
                    case 19:
                        ProcessTetraDmo(data);
                        break;
                    default:
                        ProcessOthers(data);
                        break;
                }
    }

    private void ProcessFraud(RawData data)
    {
        if (data is not RawFraud fraud) return;
        var system = RawData.GetProtocolByCode(fraud.SignalType);
        var result = new List<object>
        {
            new SDataEseResult
            {
                Frequency = fraud.Frequency / 1000000.0d,
                Result = true,
                Decoder = fraud.Message,
                System = system
            }
        };
        SendData(result);
    }

    private void ProcessFm(RawData data)
    {
        if (data is not RawFm fm) return;
        var system = RawData.GetProtocolByCode(fm.SignalType);
        var result = new List<object>
        {
            new SDataEseResult
            {
                Frequency = fm.Frequency / 1000000.0d,
                Result = true,
                Decoder = "",
                System = system
            }
        };
        if (fm.Audio != null)
            result.Add(new SDataAudio
            {
                Format = AudioFormat.Pcm,
                Channels = 1,
                SamplingRate = 8000,
                BytesPerSecond = 8000 * 2,
                BlockAlign = 2,
                BitsPerSample = 16,
                Data = fm.Audio
            });
        SendData(result);
    }

    private void ProcessDmrpdt(RawData data)
    {
        if (data is not RawDmrpdt dmrPdt) return;
        var system = RawData.GetProtocolByCode(dmrPdt.SignalType);
        var result = new List<object>
        {
            new SDataEseResult
            {
                Frequency = dmrPdt.Frequency / 1000000.0d,
                Result = true,
                Decoder = dmrPdt.Message,
                System = system
            }
        };
        if (dmrPdt.Audio != null)
            result.Add(new SDataAudio
            {
                Format = AudioFormat.Pcm,
                Channels = 1,
                SamplingRate = 8000,
                BytesPerSecond = 8000 * 2,
                BlockAlign = 2,
                BitsPerSample = 16,
                Data = dmrPdt.Audio
            });
        SendData(result);
    }

    private void ProcessDpmr(RawData data)
    {
        if (data is not RawDpmr dpmr) return;
        var system = RawData.GetProtocolByCode(dpmr.SignalType);
        var result = new List<object>
        {
            new SDataEseResult
            {
                Frequency = dpmr.Frequency / 1000000.0d,
                Result = true,
                Decoder = dpmr.Message,
                System = system
            }
        };
        if (dpmr.Audio != null)
            result.Add(new SDataAudio
            {
                Format = AudioFormat.Pcm,
                Channels = 1,
                SamplingRate = 8000,
                BytesPerSecond = 8000 * 2,
                BlockAlign = 2,
                BitsPerSample = 16,
                Data = dpmr.Audio
            });
        SendData(result);
    }

    private void ProcessTetra(RawData data)
    {
        if (data is not RawTetra tetra) return;
        var system = RawData.GetProtocolByCode(tetra.SignalType);
        var result = new List<object>
        {
            new SDataEseResult
            {
                Frequency = tetra.Frequency / 1000000.0d,
                Result = true,
                Decoder = "",
                System = system
            }
        };
        if (tetra.Audio != null)
            result.Add(new SDataAudio
            {
                Format = AudioFormat.Pcm,
                Channels = 1,
                SamplingRate = 8000,
                BytesPerSecond = 8000 * 2,
                BlockAlign = 2,
                BitsPerSample = 16,
                Data = tetra.Audio
            });
        SendData(result);
    }

    private void ProcessNxdn(RawData data)
    {
        if (data is not RawNxdn nxdn) return;
        var system = RawData.GetProtocolByCode(nxdn.SignalType);
        var result = new List<object>
        {
            new SDataEseResult
            {
                Frequency = nxdn.Frequency / 1000000.0d,
                Result = true,
                Decoder = nxdn.Message,
                System = system
            }
        };
        if (nxdn.Audio != null)
            result.Add(new SDataAudio
            {
                Format = AudioFormat.Pcm,
                Channels = 1,
                SamplingRate = 8000,
                BytesPerSecond = 8000 * 2,
                BlockAlign = 2,
                BitsPerSample = 16,
                Data = nxdn.Audio
            });
        SendData(result);
    }

    private void ProcessTetraDmo(RawData data)
    {
        if (data is not RawTetraDmo tetraDmo) return;
        var system = RawData.GetProtocolByCode(tetraDmo.SignalType);
        var result = new List<object>
        {
            new SDataEseResult
            {
                Frequency = tetraDmo.Frequency / 1000000.0d,
                Result = true,
                Decoder = "",
                System = system
            }
        };
        if (tetraDmo.Audio != null)
            result.Add(new SDataAudio
            {
                Format = AudioFormat.Pcm,
                Channels = 1,
                SamplingRate = 8000,
                BytesPerSecond = 8000 * 2,
                BlockAlign = 2,
                BitsPerSample = 16,
                Data = tetraDmo.Audio
            });
        SendData(result);
    }

    private void ProcessOthers(RawData data)
    {
        if (data is not RawOthers tetraDmo) return;
        var system = RawData.GetProtocolByCode(tetraDmo.SignalType);
        var result = new List<object>
        {
            new SDataEseResult
            {
                Frequency = tetraDmo.Frequency / 1000000.0d,
                Result = true,
                Decoder = "",
                System = system
            }
        };
        SendData(result);
    }

    #endregion

    #region IDataPort

    public void OnData(List<object> data)
    {
        if (data?.Find(x => x is SDataIq) is SDataIq iq) _sendIqCache.EnQueue(iq);
    }

    public void OnMessage(SDataMessage message)
    {
        throw new NotImplementedException();
    }

    #endregion
}