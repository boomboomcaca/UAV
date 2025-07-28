using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.SerialAntController;

public partial class SerialAntController
{
    /// <summary>
    ///     初始化串口
    /// </summary>
    private void InitSerialPort()
    {
        _serialPort = new SerialPort(Com, BaudRate);
        _serialPort.Open();
    }

    /// <summary>
    ///     初始化心跳线程
    /// </summary>
    private void InitHeartBeat()
    {
        if (_heartBeat?.IsCompleted == false) return;
        _tokenSource = new CancellationTokenSource();
        _heartBeat = new Task(KeepAlive, _tokenSource.Token);
        _heartBeat.Start();
    }

    /// <summary>
    ///     心跳线程方法
    /// </summary>
    private void KeepAlive()
    {
        try
        {
            while (!_tokenSource.IsCancellationRequested)
            {
                if (_serialPort?.IsOpen == true)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                throw new IOException("串口未打开或已关闭");
            }
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception ex)
        {
#if DEBUG
            Trace.WriteLine(ex.ToString());
#endif
            SendMessage(new SDataMessage
            {
                LogType = LogType.Error,
                Description = "设备重连异常"
            });
        }
    }

    /// <summary>
    ///     停止线程
    /// </summary>
    private void ReleaseHeartBeat()
    {
        if (_heartBeat == null || _tokenSource == null) return;
        try
        {
            if (!_heartBeat.IsCompleted) _tokenSource.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }
        catch (AggregateException)
        {
        }
        finally
        {
            _tokenSource.Dispose();
        }
    }

    /// <summary>
    ///     关闭串口
    /// </summary>
    private void ReleaseSerialPort()
    {
        if (_serialPort == null) return;
        try
        {
            _serialPort.Close();
        }
        catch (IOException e)
        {
#if DEBUG
            Trace.WriteLine(e.ToString());
#endif
        }
        finally
        {
            _serialPort = null;
        }
    }
}