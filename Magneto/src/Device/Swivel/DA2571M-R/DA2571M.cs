using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.DA2571M;

public partial class Da2571M : DeviceBase
{
    #region 构造函数

    public Da2571M(Guid id) : base(id)
    {
    }

    #endregion

    #region Helper

    private void SendCommand(string cmd)
    {
        var sentBuffer = Encoding.Default.GetBytes(cmd + "\r\n");
        var bytesToSend = sentBuffer.Length;
        var offset = 0;
        var total = 0;
        try
        {
            while (total < bytesToSend)
            {
                var sentBytes = _ctrlChannel.Send(sentBuffer, offset, bytesToSend - total, SocketFlags.None);
                offset += sentBytes;
                total += sentBytes;
            }
        }
        catch
        {
            // ignored
        }
    }

    #endregion

    #region 成员变量

    private Socket _ctrlChannel;
    private Task[] _taskArray;
    private CancellationTokenSource _cts;
    private ConcurrentQueue<SDataAngle> _queue;

    #endregion

    #region 重写父类方法

    public override bool Initialized(ModuleInfo device)
    {
        try
        {
            var result = base.Initialized(device);
            if (result)
            {
                InitMisc();
                InitNetworks();
                InitDevice();
                InitWorks();
                SetHeartBeat(_ctrlChannel);
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            ReleaseResources();
            return false;
        }
    }

    public override void Dispose()
    {
        SendCommand("*RESET");
        ReleaseResources();
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        _queue.Clear();
        SendCommand($"*SPEED_{_dfindMode}");
    }

    public override void Stop()
    {
        SendCommand("*RESET");
        _queue.Clear();
        base.Stop();
    }

    #endregion

    #region 初始化

    private void InitMisc()
    {
        _cts ??= new CancellationTokenSource();
        _queue ??= new ConcurrentQueue<SDataAngle>();
        _queue.Clear();
    }

    private void InitNetworks()
    {
        _ctrlChannel ??= new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true,
            ReceiveTimeout = 5000
        };
        _ctrlChannel.Connect(Ip, Port);
    }

    private void InitDevice()
    {
        SendCommand("*RESET");
    }

    private void InitWorks()
    {
        var token = _cts.Token;
        _taskArray = new[] { CaptureDataAsync(token), DispatchDataAsync(token) };
    }

    #endregion

    #region 释放资源

    private void ReleaseResources()
    {
        ReleaseWorks();
        ReleaseNetworks();
        ReleaseQueues();
    }

    private void ReleaseWorks()
    {
        try
        {
            _cts?.Cancel();
            if (_taskArray != null)
                Task.Run(async () => await Task.WhenAll(_taskArray)).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        catch (AggregateException ex)
        {
            foreach (var ie in ex.Flatten().InnerExceptions) Console.WriteLine(ie.ToString());
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

    private void ReleaseNetworks()
    {
        _ctrlChannel?.Close();
    }

    private void ReleaseQueues()
    {
        _queue?.Clear();
    }

    #endregion

    #region 数据接收与处理

    private async Task CaptureDataAsync(CancellationToken token)
    {
        await Task.Factory.StartNew(() =>
        {
            using var reader = new StreamReader(new NetworkStream(_ctrlChannel));
            var epsilon = 1.0e-6;
            while (!token.IsCancellationRequested)
                try
                {
                    var angleString = reader.ReadLine();
                    if (float.TryParse(angleString, out var angle) && Math.Abs(angle - 0) <= epsilon)
                        _queue.Enqueue(new SDataAngle { Azimuth = angle });
                }
                catch (Exception ex)
                {
                    if (ex is SocketException)
                    {
                        Console.WriteLine(ex.Message);
                        Thread.Sleep(1000);
                    }
                }
        }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    private async Task DispatchDataAsync(CancellationToken token)
    {
        await Task.Factory.StartNew(() =>
        {
            while (!token.IsCancellationRequested)
            {
                var value = _queue.TryDequeue(out var angle);
                if (!value)
                {
                    Thread.Sleep(10);
                    continue;
                }

                if (TaskState == TaskState.Start) SendData(new List<object> { angle });
            }
        }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    #endregion
}