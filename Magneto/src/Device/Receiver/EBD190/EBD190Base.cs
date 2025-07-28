using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Device.EBD190.IO;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.EBD190;

public partial class Ebd190
{
    #region 释放资源

    private void ReleaseResources()
    {
        ReleaseTasks();
        _client?.Close();
    }

    private void ReleaseTasks()
    {
        Utils.CancelTask(_thdScanCompassTask, _thdScanCompassTokenSource);
        Utils.CancelTask(_thdHeartBeatTask, _thdHeartBeatTokenSource);
    }

    #endregion

    #region 初始化

    /// <summary>
    ///     创建客户端对象
    /// </summary>
    /// <returns>Client客户端对象</returns>
    private IClient GetClient()
    {
        IClient client = null;
        switch (NetType)
        {
            case "TCP":
                client = new TcpClient(Ip, Port);
                break;
            case "串口":
                client = new SerialPortClient(Com, Baudrate);
                break;
        }

        return client;
    }

    /// <summary>
    ///     初始化线程
    /// </summary>
    private void InitTasks()
    {
        if (_thdScanCompassTask?.IsCompleted == false) return;
        _thdScanCompassTokenSource = new CancellationTokenSource();
        _thdScanCompassTask = new Task(ScanCompass, _thdScanCompassTokenSource.Token);
        _thdScanCompassTask.Start();
        if (_thdHeartBeatTask?.IsCompleted == false) return;
        _thdHeartBeatTokenSource = new CancellationTokenSource();
        _thdHeartBeatTask = new Task(KeepAlive, _thdHeartBeatTokenSource.Token);
        _thdHeartBeatTask.Start();
    }

    /// <summary>
    ///     初始化设备
    /// </summary>
    private void InitDevice()
    {
        try
        {
            SetIf(SplitIf(If));
        }
        catch
        {
        }
    }

    #endregion

    #region 命令下发

    #region 程序内部设置/安装参数设置

    /// <summary>
    ///     设置罗盘取几次的平均值
    /// </summary>
    /// <param name="value">平均数</param>
    private void SetCompassAvers(byte value)
    {
        _client.SendCmd(Ebd190Command.CompassAverCommand + value);
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
        _client.SendCmd(Ebd190Command.FormatReturnDataCommand + value);
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
        _client.SendCmd(Ebd190Command.DdfModeCommand + value);
    }

    /// <summary>
    ///     设置中频,格式:Zx
    ///     x=0  IF=10.7Mhz
    ///     x=1  IF=21.4MHz
    /// </summary>
    /// <param name="value"></param>
    private void SetIf(byte value)
    {
        _client.SendCmd(Ebd190Command.SetIfCommand + value);
    }

    /// <summary>
    ///     程序控制模式
    /// </summary>
    private void GotoRemote()
    {
        _client.SendCmd(Ebd190Command.SetRemoteCommand);
    }

    /// <summary>
    ///     本地控制模式
    /// </summary>
    private void GotoLocal()
    {
        _client.SendCmd(Ebd190Command.SetLocalCommand);
    }

    #endregion 程序内部设置/安装参数设置

    #region 运行参数设置

    /// <summary>
    ///     设置测向机频率
    /// </summary>
    /// <param name="value">频率</param>
    private void SetFreqs(double value)
    {
        try
        {
            //System.Threading.Thread.Sleep(200);
            _client.SendCmd(Ebd190Command.SetFreqCommand + value);
        }
        catch
        {
        }
    }

    /// <summary>
    ///     设置测向机测向带宽
    ///     VHF和UHF:x=0 1KHz; x=1 2.5KHz;x=2 8KHz; x=3 15KHz; x=4 100KHz
    ///     HF:x=5 250Hz;x=6 500Hz;x=7 1KHz; x=8 3KHz;x=9 5KHz;
    ///     x=10 SDS
    /// </summary>
    /// <param name="value">带宽参数</param>
    private void SetBoundWidth(byte value)
    {
        try
        {
            //System.Threading.Thread.Sleep(200);
            _client.SendCmd(Ebd190Command.SetBwCommand + value);
        }
        catch
        {
        }
    }

    /// <summary>
    ///     设置测向机测向时效
    ///     x=0 100ms;x=1 200ms;x=2 500ms;x=3 1s;x=4 2s;x=5 5s;
    /// </summary>
    /// <param name="value">测向时效参数</param>
    private void SetInterTime(byte value)
    {
        try
        {
            //System.Threading.Thread.Sleep(200);
            _client.SendCmd(Ebd190Command.SetInterTimeCommand + value);
        }
        catch
        {
        }
    }

    /// <summary>
    ///     设置测向机测向门限
    /// </summary>
    /// <param name="value"></param>
    private void SetSetSquelch(int value)
    {
        try
        {
            //System.Threading.Thread.Sleep(200);
            _client.SendCmd(Ebd190Command.SetSquelchCommand + value);
        }
        catch
        {
        }
    }

    #endregion 运行参数设置

    #endregion 命令下发

    #region 数值转换

    private byte SplitIf(double value)
    {
        byte i = 0;
        switch (value)
        {
            case 10.7:
                i = 0;
                break;
            case 21.4:
                i = 1;
                break;
            default:
                i = 0;
                break;
        }

        return i;
    }

    private byte SplitBoundWidth(double value)
    {
        byte i = 0;
        switch (value)
        {
            case 1:
                i = 0;
                break;
            case 2.5:
                i = 1;
                break;
            case 8:
                i = 2;
                break;
            case 15:
                i = 3;
                break;
            case 100:
                i = 4;
                break;
            default:
                i = 3;
                break;
        }

        return i;
    }

    private byte SplitInterTime(string value)
    {
        byte i = 0;
        switch (value)
        {
            case "0.1":
                i = 0;
                break;
            case "0.2":
                i = 1;
                break;
            case "0.5":
                i = 2;
                break;
            case "1":
                i = 3;
                break;
            case "2":
                i = 4;
                break;
            case "5":
                i = 5;
                break;
            default:
                i = 1;
                break;
        }

        return i;
    }

    #endregion

    #region 线程

    // 心跳检测线程
    private void KeepAlive()
    {
        while (_thdHeartBeatTokenSource?.IsCancellationRequested == false)
        {
            // 心跳检测
            // 连上以后会自动发送数据过来，不需要主动发送信息
            if (DateTime.Now.Subtract(_client.LastGetDataTime).TotalMilliseconds > 20000) //20s超时
            {
                SendMessage(new SDataMessage
                {
                    LogType = LogType.Warning,
                    ErrorCode = (int)InternalMessageType.DeviceRestart,
                    Description = DeviceId.ToString(),
                    Detail = DeviceInfo.DisplayName
                });
                break;
            }

            Thread.Sleep(1000);
        }
    }

    // 罗盘询问线程-5s询问一次
    private void ScanCompass()
    {
        while (!_thdScanCompassTokenSource.IsCancellationRequested)
        {
            try
            {
                _client.SendCmd(Ebd190Command.QuaereCompass);
            }
            catch
            {
            }

            Thread.Sleep(5000);
        }
    }

    #endregion 线程

    #region 数据解析

    // 接收并组装数据
    private void ReceivedData(string data)
    {
        foreach (var t in data)
            if (t == '\r' || t == '\n')
            {
                if (_recvData is "" or "\r" or "\n")
                {
                    _recvData = t.ToString();
                }
                else
                {
                    AnalysisData(_recvData);
                    _recvData = "";
                }
            }
            else
            {
                if (_recvData is "" or "\r" or "\n")
                    _recvData = t.ToString();
                else
                    _recvData += t;
            }
    }

    /// <summary>
    ///     数据分析
    /// </summary>
    /// <param name="data">要分析的字符串</param>
    private void AnalysisData(string data)
    {
        //if (!_isRunning)
        //    return;
        // 发来的数据里有*号仍然处理
        //if (data.IndexOf("A*") > -1)
        //    return;
        var a = new[] { ',' };
        var arr = data.Split(a);
        if (arr.Length < 1) return;
        if (arr[0].Substring(0, 1).Trim().Equals("A")) DdfAnalysis(arr);
        if (arr[0].Substring(0, 1).Trim().Equals("C")) CompassAnalysis(arr);
    }

    /// <summary>
    ///     示向度分析
    /// </summary>
    /// <param name="data">要分析的字符串</param>
    private void DdfAnalysis(string[] data)
    {
        try
        {
            if (!_isRunning) return;
            // 将形如：A98,87,274,49的数据根据","分隔为字符串数组
            var len = data.Length;
            var ddf = data[0].Substring(1);
            var isHasDdf = false;
            var isHasLevel = false;
            if (short.TryParse(ddf, out var num))
            {
                _ddf = num;
                isHasDdf = true;
            }

            _quality = 0;
            _level = 0;
            switch (len)
            {
                case 2:
                    if (short.TryParse(data[1].Trim(), out num)) _quality = num; //取得测向机测向质量
                    break;
                case 4:
                case 5:
                case 6:
                    if (short.TryParse(data[1].Trim(), out num)) _quality = num;
                    if (short.TryParse(data[3].Trim(), out num))
                    {
                        _level = num; //测向机测向电平
                        isHasLevel = true;
                    }

                    break;
            }

            var datas = new List<object>();
            if (isHasDdf)
            {
                SDataDfind dfData = new()
                {
                    Frequency = _frequency,
                    BandWidth = _dfBandWidth,
                    Quality = _quality,
                    Azimuth = _ddf
                };
                datas.Add(dfData);
            }

            if ((_media & MediaType.Level) > 0 && isHasLevel)
            {
                var levelData = new SDataLevel
                {
                    Frequency = _frequency,
                    Bandwidth = _dfBandWidth,
                    Data = _level
                };
                datas.Add(levelData);
            }

            if (_isRunning) SendData(datas);
        }
        catch
        {
        }
    }

    /// <summary>
    ///     电子罗盘分析
    ///     罗盘是五秒获取一次数据，因此不需要缓存
    /// </summary>
    /// <param name="data">要分析的字符串</param>
    private void CompassAnalysis(string[] data)
    {
        var ePsilon = 0.00001d;
        try
        {
            var str = data[0].Substring(1);
            if (str == "999") str = "0";
            if (!HaveCompass) return;
            Encoding.ASCII.GetBytes(str);
            _compass = short.Parse(str);
            // 加补偿角度值
            _compass += (short)ExtraAngle;
            // 保证范围在0~360之间
            _compass = (short)((_compass % 360 + 360) % 360);
            if (ReportingDirection)
            {
                var compassData = new SDataCompass
                {
                    Heading = _compass
                };
                if (_data == null || Math.Abs(compassData.Heading - _data.Heading) > ePsilon)
                {
                    //SendMessage(MessageDomain.Network, MessageType.MonNodeCompassChange, compassData);
                    SendMessageData(new List<object> { compassData });
                    _data = compassData;
                }
            }
        }
        catch
        {
        }
    }

    #endregion 数据解析
}