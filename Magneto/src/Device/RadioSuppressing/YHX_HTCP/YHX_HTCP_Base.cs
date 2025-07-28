using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.YHX_HTCP;

public partial class YhxHtcp
{
    private void InitNet()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var endpoint = new IPEndPoint(IPAddress.Parse(Ip), Port);
        _socket.Connect(endpoint);
    }

    private void InitThreads()
    {
        _checkCts = new CancellationTokenSource();
        _checkTask = new Task(ThreadChkConn, _checkCts.Token);
        _checkTask.Start();
    }

    private void ReleaseResources()
    {
        Utils.CancelTask(_checkTask, _checkCts);
        Utils.CloseSocket(_socket);
    }

    private void ThreadChkConn()
    {
        var df = new DataFrame
        {
            ControlData = new ControlBlock(),
            ParamsData = new ParamsBlock
            {
                Function = Common.Htb,
                Params = "?"
            }
        };
        df.ControlData.SendControl = new SendControlBits(0, 1, 1, 0).DataBits;
        df.ControlData.ParamsLength = (short)df.ParamsData.GetBytes().Length;
        df.ControlData.DataLength = (short)(df.ControlData.GetBytes().Length + df.ParamsData.GetBytes().Length + 1);
        var sendata = df.GetBytes();
        while (_checkCts?.IsCancellationRequested == false)
            try
            {
                var result = SendCmd(sendata, true);
                if (VerifyResult(result))
                {
                    // 心跳检测 和设备端协议约定10s，设备端60s收不到则断开连接，释放端口重新等待设备端去连接；
                    Thread.Sleep(10000);
                    continue;
                }

                _isOk = false;
                var se = new SocketException(10058)
                {
                    Source = $"{DeviceInfo.DisplayName}和远端设备失去连接，请检测网络连接！"
                };
                throw se;
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode is SocketError.SocketError or SocketError.Disconnecting)
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
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{DeviceInfo.DisplayName}({DeviceInfo.Id})心跳线程检测到异常，异常信息：{ex}");
            }
    }

    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        try
        {
            var ts = TimeSpan.FromMinutes((DateTime.Now - _timeStart).TotalMinutes);
            var minutes = ts.Minutes;
            var seconds = ts.Seconds;
            if (minutes == SleepTime && seconds is >= 0 and <= 3)
            {
                // 关机，使设备处于待机状态，可以继续控制，发送指令，但是无法执行射频发射任务
                PowerOff();
                _timer.Stop();
            }
        }
        catch
        {
        }
    }

    /// <summary>
    ///     设备自检,查询设备各模块状态信息
    /// </summary>
    /// <returns></returns>
    private void DeviceSelfCheck()
    {
        var df = new DataFrame
        {
            ControlData = new ControlBlock(),
            ParamsData = new ParamsBlock
            {
                Function = Common.Chk,
                Params = "?"
            }
        };
        df.ControlData.SendControl = new SendControlBits(0, 1, 1, 0).DataBits;
        df.ControlData.ParamsLength = (short)df.ParamsData.GetBytes().Length;
        df.ControlData.DataLength = (short)(df.ControlData.GetBytes().Length + df.ParamsData.GetBytes().Length + 1);
        if (df.Data != null) df.ControlData.DataLength += (short)df.Data.GetBytes().Length;
        var sendata = df.GetBytes();
        var result = SendCmd(sendata, true);
        if (VerifyResult(result))
        {
            var info = Encoding.ASCII.GetString(result).Split(':')[1].Split(';');
            foreach (var item in info)
            {
                var temp = item.Split('=');
                if (temp.Length >= 2)
                    switch (temp[0])
                    {
                        case "BBS":
                        case "RFS":
                            _isOk &= temp[1].Split(',')[1].Equals("T");
                            break;
                        case "PAS":
                            switch (temp[1].Split(',')[1])
                            {
                                case "T":
                                    _isOk &= temp[1].Split(',')[1].Equals("T");
                                    break;
                                case "F":
                                case "S":
                                case "I":
                                case "V":
                                case "O":
                                    _isOk = false;
                                    break;
                            }

                            break;
                    }
            }
        }
        else
        {
            _isOk = false;
        }
    }

    /// <summary>
    ///     查询设备版本信息、通讯参数，版本号以及模块型号
    /// </summary>
    private void DeviceInfoQuery()
    {
        var df = new DataFrame
        {
            ControlData = new ControlBlock
            {
                SendControl = new SendControlBits(0, 1, 0, 0).DataBits
            },
            ParamsData = new ParamsBlock
            {
                Function = Common.Inf,
                Params = "?"
            }
        };
        df.ControlData.ParamsLength = (short)df.ParamsData.GetBytes().Length;
        df.ControlData.DataLength = 0;
        var sendData = df.GetBytes();
        var result = SendCmd(sendData, true);
        var resultStr = Encoding.ASCII.GetString(result, 8, result.Length - 8);
        if (!string.IsNullOrEmpty(resultStr))
            if (resultStr.Contains("VERN") || resultStr.Contains("RFTP"))
            {
                var temp = resultStr.Split(';');
                foreach (var t in temp)
                {
                    if (t.StartsWith("<INF")) continue;
                    var strArray = t.Split('=');
                    if (strArray.Length >= 2)
                        // 只将查询到的音频文件数目通知给客户端
                        if (strArray[0].Equals("VNUM"))
                            Trace.WriteLine($"设备名称：{DeviceInfo.DisplayName}，音频个数：{strArray[1]}");
                }
            }
    }

    /// <summary>
    ///     固化文本
    /// </summary>
    private bool WriteText()
    {
        var cmd = "FLAG=B;";
        if (WriteText(cmd)) return true;
        return false;
    }

    private bool WriteText(string cmd)
    {
        var df = new DataFrame
        {
            ControlData = new ControlBlock(),
            ParamsData = new ParamsBlock(),
            Data = new DataBlock()
        };
        df.ParamsData.Function = Common.Tsd;
        df.ParamsData.Params = cmd;
        df.Data.DataType = 0x12;
        df.Data.DataContent = GetBytesFromText(SolidText);
        df.ControlData.SendControl = new SendControlBits(1, 1, 1, 0).DataBits;
        df.ControlData.ParamsLength = (short)df.ParamsData.GetBytes().Length;
        df.ControlData.DataLength = (short)(df.Data.GetBytes().Length + df.ControlData.GetBytes().Length +
                                            df.ParamsData.GetBytes().Length + 1);
        var sendata = df.GetBytes();
        var gi = new GeneralInfo
        {
            Name = cmd.Contains('B') ? $"设备{DeviceInfo.DisplayName}:固化文本" : $"设备{DeviceInfo.DisplayName}:取消固化文本"
        };
        var buff = SendCmd(sendata, true);
        if (VerifyResult(buff))
        {
            gi.Info = "命令发送成功！";
            return true;
        }

        gi.Info = "命令发送失败！";
        return false;
    }

    // 类型 标志 模式 频点数 是否加载音频 音频序号  是否加载亚音 亚音序号 亚音值
    // 同步 FLAG、MODE、FSUM、LDVO、 VSEQ、LDTX、SATP、SANU 同步发射参数在数据块设置
    //      标志  模式 频率下限 频率上限 步进 扫频功率 扫频速度
    // 扫频 FLAG、MODE、FREL、FREU、 STEP、SCPW、SPED,扫频发射参数在参数块设置
    //      标志 模式 频点数 跳速
    // 跳频 FLAG、MODE、FSUM、SPED 跳频发射参数在数据块设置
    // Begin 开始执行
    private bool StartAbility()
    {
        var flag = ExecuteSuppress();
        var gi = new GeneralInfo
        {
            Name = $"设备{DeviceInfo.DisplayName}:执行压制",
            Info = flag ? "发射成功！" : "发射失败！"
        };
        Trace.WriteLine($"设备{DeviceInfo.DisplayName}{gi.Info}");
        return flag;
    }

    /// <summary>
    ///     发送指令集，假如需要接受返回值，则isRecv须设置为True
    /// </summary>
    /// <param name="cmd">要发送的命令集</param>
    /// <param name="isRecv">是否接收设备返回的状态</param>
    private byte[] SendCmd(byte[] cmd, bool isRecv)
    {
        var result = new byte[512];
        try
        {
            if (_socket.Connected)
            {
                Thread.Sleep(200);
                lock (_lockCmd)
                {
                    _socket.Send(cmd, 0, cmd.Length, SocketFlags.None);
                    if (isRecv)
                    {
                        var index = 0;
                        if ((index = _socket.Receive(result)) > 0)
                            Array.Resize(ref result, index);
                    }
                    //System.Text.ASCIIEncoding.ASCII.GetString(result, 0, index);
                }
            }
        }
        catch (SocketException ex)
        {
            Trace.WriteLine($"设备{DeviceInfo.DisplayName}发送命令：{Encoding.ASCII.GetString(cmd)}失败，异常信息：{ex}");
        }
        catch (Exception)
        {
        }

        return result;
    }

    /// <summary>
    ///     发送指令集，不设置超时，也不接收返回值
    /// </summary>
    private void SendCmd(byte[] cmd)
    {
        SendCmd(cmd, true);
    }

    /// <summary>
    ///     验证命令发送是否成功
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    private static bool VerifyResult(byte[] result)
    {
        var str = Encoding.ASCII.GetString(result).Trim();
        if (
            str.Contains("RESU=T;") &&
            CheckSum(result).Equals(result[(result[2] << 8) + result[3] - 1]))
            return true;
        return false;
    }

    /// <summary>
    ///     计算待发送命令的校验和
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private static byte CheckSum(byte[] data)
    {
        byte sum = 0;
        for (var i = 0; i < (data[2] << 8) + data[3] - 1; i++) sum += data[i]; //将每个数相加
        return BitConverter.GetBytes(sum)[0];
    }

    private static byte[] GetBytesFromText(string text)
    {
        var encoding = Encoding.ASCII;
        if (HasChinese(text)) encoding = Encoding.UTF8;
        return encoding.GetBytes(text);
    }

    private static bool HasChinese(string str)
    {
        return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
    }
}