using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;

namespace Magneto.Device.EBD100;

public partial class Ebd100
{
    #region 释放资源

    /// <summary>
    ///     释放非托管资源
    /// </summary>
    private void ReleaseResources()
    {
        if (_serialPort is { IsOpen: true })
        {
            _serialPort.DataReceived -= SerialPort_DataReceived;
            lock (_lockComport)
            {
                _serialPort.Close();
            }
        }

        Utils.CancelTask(_thdScanCompassTask, _thdScanCompassTokenSource);
        Utils.CancelTask(_thdHeartBeatTask, _thdHeartBeatTokenSource);
    }

    #endregion

    #region 初始化

    /// <summary>
    ///     初始化串口连接
    /// </summary>
    private bool InitSerialPort()
    {
        var port = "COM" + Port;
        _lastGetDataTime = DateTime.Now;
        _serialPort = new SerialPort(port, 9600, Parity.None, 8, StopBits.One);
        _serialPort.Open();
        _serialPort.ReceivedBytesThreshold = 1;
        _serialPort.DiscardInBuffer();
        _serialPort.DataReceived += SerialPort_DataReceived;
        return true;
    }

    /// <summary>
    ///     初始化设备
    /// </summary>
    private void InitDevice()
    {
        SetIf(SplitIf(If));
    }

    /// <summary>
    ///     初始化线程
    /// </summary>
    private void InitThread()
    {
        _thdScanCompassTokenSource = new CancellationTokenSource();
        _thdScanCompassTask = new Task(ScanCompass, _thdScanCompassTokenSource.Token);
        _thdScanCompassTask.Start();
        // 心跳线程
        _thdHeartBeatTokenSource = new CancellationTokenSource();
        _thdHeartBeatTask = new Task(KeepAlive, _thdHeartBeatTokenSource.Token);
        _thdHeartBeatTask.Start();
    }

    #endregion 初始化

    #region 命令下发

    /// <summary>
    ///     发送命令
    /// </summary>
    /// <param name="cmd">命令字符</param>
    private void SendCmd(string cmd)
    {
        if (_serialPort == null) return;
        if (!_serialPort.IsOpen) return;
        cmd += "\r\n";
        var buffer = Encoding.ASCII.GetBytes(cmd);
        lock (_lockComport)
        {
            _serialPort.Write(buffer, 0, buffer.Length);
        }
    }

    #region 程序内部设置/安装参数设置

    /// <summary>
    ///     设置罗盘取几次的平均值
    /// </summary>
    /// <param name="value">平均数</param>
    private void SetCompassAvers(byte value)
    {
        SendCmd(Ebd100Command.CompassAvgCommand + value);
    }

    /// <summary>
    ///     格式化测向机返回数据命令
    ///     有以下6种
    ///     S0 返回:A123 其中123为示向度
    ///     S1 返回:A123,43 其中43为测向质量
    ///     S2 返回:A123,43,500 其中500为测向时效
    ///     S3 返回:A123,43,500,55 其中55为电平
    ///     S4 返回:A123,43,500,55,184312.3 其中184312.3为时间:18:43:12.3h
    ///     S5 返回:A123,43,500,55,184312.3,1234 其中1234为频率为:1234MHz
    /// </summary>
    /// <param name="value">格式化参数</param>
    private void SetDataFormat(byte value)
    {
        SendCmd(Ebd100Command.FormatReturnDataCommand + value);
    }

    /// <summary>
    ///     设置测向机测向模式
    ///     有三种测向模式;NORMAL,CONTINUOUS,GATE,格式为:Mx
    ///     NORMAL:高于电平门限的进行测向  x=0
    ///     CONTINUOUS:连续测向，这时电平门限无效 x=1
    ///     GATE:和NORMAL方式一样，不同的是测向停止后平均值缓冲区并不清空,将参与继续积分 x=2
    /// </summary>
    /// <param name="value">测向模式参数</param>
    private void SetDdfMode(byte value)
    {
        SendCmd(Ebd100Command.DdfModeCommand + value);
    }

    /// <summary>
    ///     设置中频,格式:Zx
    ///     x=0  IF=10.7Mhz
    ///     x=1  IF=21.4MHz
    /// </summary>
    /// <param name="value"></param>
    private void SetIf(byte value)
    {
        SendCmd(Ebd100Command.SetIfCommand + value);
    }

    /// <summary>
    ///     程序控制模式
    /// </summary>
    private void GotoRemote()
    {
        SendCmd(Ebd100Command.SetRemoteCommand);
    }

    /// <summary>
    ///     本地控制模式
    /// </summary>
    private void GotoLocal()
    {
        SendCmd(Ebd100Command.SetLocalCommand);
    }

    #endregion 程序内部设置/安装参数设置

    #region 运行参数设置

    /// <summary>
    ///     设置测向机频率
    /// </summary>
    /// <param name="value">频率</param>
    private void SetFreqs(double value)
    {
        SendCmd(Ebd100Command.SetFreqCommand + value);
    }

    /// <summary>
    ///     设置测向机测向带宽
    ///     VHF和UHF:x=0 1KHz; x=1 2.5KHz;x=2 8KHz; x=3 15KHz; x=4 100KHz
    ///     HF:x=5 250Hz;x=6 500Hz;x=7 1KHz; x=8 3KHz;x=9 5KHz;
    ///     x=10 SDS
    /// </summary>
    /// <param name="value">带宽参数</param>
    private void SetBandWidth(byte value)
    {
        SendCmd(Ebd100Command.SetBwCommand + value);
    }

    /// <summary>
    ///     设置测向机测向时效
    ///     x=0 100ms;x=1 200ms;x=2 500ms;x=3 1s;x=4 2s;x=5 5s;
    /// </summary>
    /// <param name="value">测向时效参数</param>
    private void SetInterTime(byte value)
    {
        SendCmd(Ebd100Command.SetInterTimeCommand + value);
    }

    /// <summary>
    ///     设置测向机测向门限
    /// </summary>
    /// <param name="value"></param>
    private void SetSetSquelch(int value)
    {
        SendCmd(Ebd100Command.SetSquelchCommand + value);
    }

    #endregion 运行参数设置

    #endregion 命令下发
}