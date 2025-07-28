using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Magneto.Protocol.Define;

namespace Magneto.Device.YHX_HTCP;

public partial class YhxHtcp
{
    private bool ExecuteSuppress()
    {
        if (_rftxSegments?.Any() != true || _rftxBands?.Any() != true) return false;
        var success = GetLegalSegments(_rftxSegments, _rftxBandExs, out var results, out var frequencyMode);
        if (!success) return false;
        switch (frequencyMode)
        {
            case 0:
            case 4:
            {
                success = ExecuteFixFreq(frequencyMode, results);
            }
                break;
            case 1:
            {
                success = ExecuteJumpFreq(frequencyMode, results);
            }
                break;
            case 2:
            case 3:
            {
                success = ExecuteSweepFreq(results);
            }
                break;
            default:
                return false;
        }

        return success;
    }

    /// * 
    /// 协议帧结构组成
    /// 帧格式  控制块  参数快  数据块  校验
    /// COL     PARA    DATA    VS
    /// 字节数   8        N       M       1 
    /// 
    /// 控制块固定8字节长，包括报文序号、发送控制字段
    /// 控制块开始 #
    /// 参数快起止
    /// 数据块起止5BH  5DH级ASCII码的[]
    /// VS是COL PARA DATA的逐字节累加和（如超过1字节，则高位舍去保留低位）
    /// <summary>
    ///     执行扫频发射功能
    /// </summary>
    /// <returns>参数设置是否成功</returns>
    private bool ExecuteSweepFreq(List<RftxSegmentsTemplate> segments)
    {
        var flag = false;
        // 三个段可以分别设置各自的发射功率
        // 包含第一段和单独的广播段
        if (segments?.Any() != true) return false;
        var df = new DataFrame
        {
            ControlData = new ControlBlock(),
            ParamsData = new ParamsBlock(),
            Data = new DataBlock
            {
                DataType = 0X11,
                DataContent = GetBytesFromText(string.Empty)
            }
        };
        df.ParamsData.Function = Common.Tra;
        foreach (var t in segments)
        {
            var frel = t.StartFrequency;
            var step = t.StopFrequency;
            var freu = t.StepFrequency;
            var seg = t;
            var channelNumber = seg.PhysicalChannelNumber;
            if (channelNumber < 0 || channelNumber >= _powers.Length) continue;
            var power = (int)_powers[channelNumber];
            var jumpSpeed = (int)(1e6 / seg.HoldTime);
            var sb = new StringBuilder();
            sb.Append("FLAG=B;")
                .Append("MODE=").Append(GetEmitterMode(seg.RftxFrequencyMode)).Append(';')
                .Append("SCPW=").Append(power).Append(';')
                .Append("SPED=").Append(jumpSpeed).Append(';')
                .Append("FREL=").Append((int)(frel * 1e6)).Append(';')
                .Append("FREU=").Append((int)(freu * 1e6)).Append(';')
                .Append("STEP=").Append((int)(step * 1e3)).Append(';');
            df.ParamsData.Params = sb.ToString();
            df.ControlData.SendControl = new SendControlBits(1, 1, 1, 0).DataBits;
            df.ControlData.ParamsLength = (short)df.ParamsData.GetBytes().Length;
            df.ControlData.DataLength = (short)(df.Data.GetBytes().Length + df.ControlData.GetBytes().Length +
                                                df.ParamsData.GetBytes().Length + 1);
            var sendata = df.GetBytes();
            var result = SendCmd(sendata, true);
            var resultStr = Encoding.ASCII.GetString(result);
            flag = resultStr.Contains("TRA:RESU=T;");
            Thread.Sleep(1000);
        }

        return flag;
    }

    /// <summary>
    ///     执行同步发射的参数
    /// </summary>
    /// <returns>参数设置是否成功</returns>
    private bool ExecuteFixFreq(int frequencyMode, List<RftxSegmentsTemplate> segments)
    {
        var isLoadAudio = segments.Any(p => p.IsLoadAudio);
        var temp = segments.Find(p => p.SubToneType != Satp.E);
        //参数组装
        var sb = new StringBuilder();
        sb.Append("FLAG=B;").Append("MODE=").Append(GetEmitterMode(frequencyMode)).Append(';').Append("FSUM=")
            .Append(segments.Count).Append(';');
        if (isLoadAudio)
        {
            var audioNo = segments.Find(p => p.IsLoadAudio).AudioNo;
            sb.Append("LDVO=T;").Append("VSEQ=").Append(audioNo).Append(';');
        }
        else
        {
            sb.Append("LDVO=F;");
        }

        if (temp != null)
            sb.Append("SATP=").Append(temp.SubToneType).Append(';').Append("SANU=").Append(temp.SubToneNo).Append(';');
        sb.Append("LDTX=T;");
        var flag = ExecuteFixJump(sb.ToString(), segments);
        return flag;
    }

    /// <summary>
    ///     执行跳频发射功能
    /// </summary>
    /// <returns>参数设置是否成功</returns>
    private bool ExecuteJumpFreq(int frequencyMode, List<RftxSegmentsTemplate> segments)
    {
        //参数组装
        var freqCount = 0;
        var jumpSpeed = 1;
        foreach (var segment in segments)
        {
            var speed = (int)(1e6 / segment.HoldTime);
            jumpSpeed = Math.Max(jumpSpeed, speed);
            freqCount += segment.Frequencies.Length;
        }

        var sb = new StringBuilder();
        sb.Append("FLAG=B;")
            .Append("MODE=").Append(GetEmitterMode(frequencyMode)).Append(';').Append("FSUM=").Append(freqCount)
            .Append(';').Append("SPED=").Append(jumpSpeed).Append(';');
        var flag = ExecuteFixJump(sb.ToString(), segments);
        return flag;
    }

    /// <summary>
    ///     执行同步与跳频的频点发送任务，传入参数cmd为同步与跳频的参数块部分
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="segments"></param>
    /// <returns></returns>
    private bool ExecuteFixJump(string cmd, List<RftxSegmentsTemplate> segments)
    {
        var df = new DataFrame
        {
            ControlData = new ControlBlock(),
            ParamsData = new ParamsBlock(),
            Data = new DataBlock()
        };
        df.ParamsData.Function = Common.Tra;
        df.ParamsData.Params = cmd;
        // 组装数据块；同步发射时参数是放在数据块的
        df.Data.DataType = 0X11;
        df.Data.DataContent = GetDiscretFrequenciesBytes(segments);
        df.ControlData.SendControl = new SendControlBits(1, 1, 1, 0).DataBits;
        df.ControlData.ParamsLength = (short)df.ParamsData.GetBytes().Length;
        df.ControlData.DataLength = (short)(df.Data.GetBytes().Length + df.ControlData.GetBytes().Length +
                                            df.ParamsData.GetBytes().Length + 1);
        var sendata = df.GetBytes();
        var result = SendCmd(sendata, true);
        if (VerifyResult(result)) return true;

        DeviceSelfCheck();
        return false;
    }

    private byte[] GetDiscretFrequenciesBytes(List<RftxSegmentsTemplate> segments)
    {
        var list = new List<byte>();
        foreach (var seg in segments)
        {
            var channelNumber = seg.PhysicalChannelNumber;
            if (channelNumber < 0 || channelNumber >= _powers.Length) continue;
            var power = (int)_powers[channelNumber];
            foreach (var f in seg.Frequencies)
            {
                var bts = GetDiscretFreqCmdBytes(f, seg.Modulation, seg.Modulation1, seg.Modulation2, seg.Modulation3,
                    power);
                list.AddRange(bts);
            }
        }

        return list.ToArray();
    }

    private static byte[] GetDiscretFreqCmdBytes(double frequency, Modulation modulation, double modulation1,
        double modulation2, double modulation3, int power)
    {
        var data = new byte[19];
        // 频率值 4字节
        var temp = BitConverter.GetBytes((uint)(frequency * 1e6));
        var result = new byte[5];
        Array.Copy(temp, 0, result, 0, temp.Length);
        Array.Reverse(result);
        Array.Copy(result, 0, data, 0, result.Length);
        // 调制方式 1字节  
        data[5] = TransMode(modulation);
        // 调制参数1 4字节
        temp = BitConverter.GetBytes((uint)modulation1).Reverse().ToArray();
        Array.Copy(temp, 0, data, 6, temp.Length);
        // 调制参数2 4字节
        temp = BitConverter.GetBytes((uint)(modulation2 * 1e6)).Reverse().ToArray();
        Array.Copy(temp, 0, data, 10, temp.Length);
        // 调制参数3 4字节
        temp = BitConverter.GetBytes((uint)(modulation3 * 1e6)).Reverse().ToArray();
        Array.Copy(temp, 0, data, 14, temp.Length);
        // 发射功率 1字节
        data[^1] = (byte)power;
        return data;
    }

    private static byte TransMode(Modulation mod)
    {
        byte result = 0x03;
        switch (mod)
        {
            case Modulation.Cw:
                result = 0x01;
                break;
            case Modulation.Am:
                result = 0x02;
                break;
            case Modulation.Fm:
                result = 0x03;
                break;
            case Modulation.Bpsk:
                result = 0x04;
                break;
            case Modulation.Qpsk:
                result = 0x05;
                break;
            case Modulation._2FSK:
                result = 0x06;
                break;
            case Modulation.Ask:
                result = 0x08;
                break;
            case Modulation.Dpsk:
                result = 0x09;
                break;
            case Modulation.Gmsk:
                result = 0x0A;
                break;
            case Modulation._4FSK:
                result = 0x0B;
                break;
        }

        return result;
    }

    /// <summary>
    ///     关机后设备进入待机状态（仍可随时响应客户端的开机指令）
    /// </summary>
    private void PowerOff()
    {
        var cmd = "FLAG=S;";
        Power(cmd);
    }

    /// <summary>
    ///     当客户端下达开机指令时，传感器进入正常工作状态
    /// </summary>
    private void PowerOn()
    {
        var cmd = "FLAG=B;";
        Power(cmd);
    }

    /// <summary>
    ///     电源控制
    /// </summary>
    /// <param name="cmd"></param>
    private void Power(string cmd)
    {
        var df = new DataFrame
        {
            ControlData = new ControlBlock
            {
                SendControl = new SendControlBits(0, 1, 1, 0).DataBits
            },
            ParamsData = new ParamsBlock
            {
                Function = Common.Smm,
                Params = cmd
            }
        };
        df.ControlData.ParamsLength = (short)df.ParamsData.GetBytes().Length;
        df.ControlData.DataLength = 0;
        var sendData = df.GetBytes();
        var result = SendCmd(sendData, true);
        var name = string.Empty;
        if (cmd.Contains('B'))
            name = $"设备{DeviceInfo.DisplayName}:开机";
        else if (cmd.Contains('S')) name = $"设备{DeviceInfo.DisplayName}:关机";
        var gi = new GeneralInfo
        {
            Name = name,
            Info = VerifyResult(result) ? $"{name}成功！" : $"{name}失败！"
        };
        Trace.WriteLine(gi.Info);
    }

    /// <summary>
    ///     停止发射某一段或某几段，传入命令类似100-500,500,1000;传入参数不符合则停止所有段
    /// </summary>
    private bool StopEmit()
    {
        var stopSeg = "100-500,500-1000,1000-1700";
        // 停止所有频段的发射
        var df = new DataFrame
        {
            ControlData = new ControlBlock(),
            ParamsData = new ParamsBlock
            {
                Function = Common.Tra,
                Params = $"FLAG=S;CHNU={stopSeg};"
            }
        };
        df.ControlData.SendControl = new SendControlBits(0, 1, 1, 0).DataBits;
        df.ControlData.ParamsLength = (short)df.ParamsData.GetBytes().Length;
        df.ControlData.DataLength = (short)(df.ControlData.GetBytes().Length + df.ParamsData.GetBytes().Length + 1);
        if (df.Data != null) df.ControlData.DataLength += (short)df.Data.GetBytes().Length;
        var sendata = df.GetBytes();
        var gi = new GeneralInfo
        {
            Name = $"设备{DeviceInfo.DisplayName}:停止发射"
        };
        if (VerifyResult(SendCmd(sendata, true)))
        {
            gi.Info = $"停止发射的段：{stopSeg}，命令发送状态：成功！";
            Trace.WriteLine(gi.Info);
            return true;
        }

        gi.Info = $"停止发射的段：{stopSeg}，命令发送状态：失败！";
        Trace.WriteLine(gi.Info);
        return false;
    }

    private static string GetEmitterMode(int frequencyMode)
    {
        return frequencyMode switch
        {
            0 => "F",
            1 => "J",
            2 => "S",
            3 => "S",
            4 => "F",
            _ => "N"
        };
    }
}