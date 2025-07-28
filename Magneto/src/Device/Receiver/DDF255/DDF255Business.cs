using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.DDF255;

public partial class Ddf255
{
    #region UDP通道处理

    /// <summary>
    ///     根据扫描类型设置对应扫描参数
    /// </summary>
    private void StartScan()
    {
        SendCmd("TRAC SSTART,0;:TRAC SSTOP,0"); //删除接收机中的忽略频点
        switch (ScanMode)
        {
            case ScanMode.Fscan:
                StartFScan();
                break;
            case ScanMode.Pscan:
                StartPScan();
                break;
            case ScanMode.MScan:
                StartMScan();
                break;
            default:
                return;
        }

        Thread.Sleep(200);
        SendCmd("INIT");
    }

    /// <summary>
    ///     启动FSCAN扫描
    /// </summary>
    private void StartFScan()
    {
        //int total = Utils.GetTotalCount(_startFrequency, _stopFrequency, _stepFrequency);
        SendCmd("SENS:FREQ:MODE SWE");
        SendCmd("MEAS:TIME 0");
        //在进行多频段扫描时，频段间切换太频繁会导致扫描参数无法一次性设置正确
        var count = 0;
        while (count++ < 10)
        {
            SendCmd($"SENS:FREQ:STAR {StartFrequency} MHz");
            SendCmd($"SENS:FREQ:STOP {StopFrequency} MHz");
            SendCmd($"SENS:SWE:STEP {StepFrequency} kHz");
            if (CheckFScanParameters()) break;
        }

        SendCmd("SENS:SWE:COUN INF");
        SendCmd("SENS:SWE:DIR UP");
        SendCmd("SENS:GCON:MODE AGC");
        if (CurFeature == FeatureType.FScne)
        {
            //在驻留频段扫描的时候设置实际设置值
            SendCmd("SWEep:CONTrol:ON \"STOP:SIGN\"");
            SendCmd($"SENS:SWE:DWELL {_dwellTime} s");
            SendCmd($"SENS:SWE:HOLD:TIME {_holdTime} ms");
        }
        else
        {
            //在频段扫描-频点扫描模式下设置一下默认值
            SendCmd("OUTP:SQU:STAT OFF");
            SendCmd("SENS:DEM FM");
            SendCmd("SENS:SWE:DWELL 0");
            SendCmd("SENS:SWE:HOLD:TIME 0");
        }
    }

    /// <summary>
    ///     启动PSCAN扫描
    /// </summary>
    private void StartPScan()
    {
        //int total = Utils.GetTotalCount(_startFrequency, _stopFrequency, _stepFrequency);
        SendCmd("SENS:FREQ:MODE PSC");
        SendCmd("SENS:PSC:COUN INF");
        SendCmd("SENS:GCON:MODE AGC");
        SendCmd("OUTP:SQU:STAT OFF");
        ////由于解调模式与滤波带宽有约束且PSCan模式下解调模式为有效参数，
        ////为避免从单频测量切换到全景扫描时违背约束出错，全景扫描时将解调模式置为默认值
        SendCmd("SENS:DEM FM");
        //在进行多频段扫描时，频段间切换太频繁会导致扫描参数无法一次性设置正确
        var count = 0;
        while (count++ < 10)
        {
            SendCmd($"SENS:FREQ:PSC:START {StartFrequency} MHz");
            SendCmd($"SENS:FREQ:PSC:STOP {StopFrequency} MHz");
            SendCmd($"PSC:STEP {StepFrequency} kHz");
            if (CheckPScanParameters()) break;
        }
    }

    /// <summary>
    ///     启动MScan扫描
    /// </summary>
    private void StartMScan()
    {
        if (MscanPoints == null || MscanPoints.Length == 0 || MscanPoints.Length > 1000) return;
        SendCmd("SENSE:FREQ:MODE MSC");
        SendCmd("MEMory:CLEar MEM0,MAXimum");
        SendCmd("OUTPUT:SQUELCH:CONTROL NONE"); //静噪控制方式
        if (CurFeature == FeatureType.MScan)
        {
            SendCmd("OUTP:SQU OFF");
            SendCmd("MSCan:CONTrol:OFF \"STOP:SIGN\"");
            SendCmd("SENSE:MSCAN:DWELL 0 ms");
            SendCmd("SENSE:MSCAN:HOLD:TIME 0 ms");
        }
        else
        {
            SendCmd($"SENSE:MSCAN:DWELL {_dwellTime}s");
            SendCmd($"SENSE:MSCAN:HOLD:TIME {_holdTime}ms");
            SendCmd("MSCan:CONTrol:ON \"STOP:SIGN\"");
        }

        SendCmd("SENSE:MSCAN:COUNT INF");
        SendCmd("SENS:GCON:MODE AGC");
        lock (_lockFreqs)
        {
            var att = Math.Abs(_attenuation - -1) < 1e-9 ? 0 : _attenuation;
            var attAuto = Math.Abs(_attenuation - -1) < 1e-9 ? "ON" : "OFF";
            _scanFreqs.Clear();
            for (var i = 0; i < MscanPoints.Length; ++i)
            {
                string cmd;
                var dic = MscanPoints[i];
                var mscanPoint = (MScanTemplate)dic;
                var freq = mscanPoint.Frequency;
                var bw = mscanPoint.FilterBandwidth;
                var demMode = mscanPoint.DemMode;
                if (CurFeature == FeatureType.MScan)
                {
                    cmd = $"MEM:CONT MEM{i},{freq} MHz,0,{demMode},{bw} kHz,(@1),{att},{attAuto},OFF,OFF,ON";
                }
                else
                {
                    var squcState = _squelchSwitch ? "ON" : "OFF";
                    cmd =
                        $"MEM:CONT MEM{i},{freq} MHz,{_squelchThreshold},{demMode},{bw} kHz,(@1),{att},{attAuto},{squcState},OFF,ON";
                }

                SendCmd(cmd);
                _scanFreqs.Add(freq);
            }
        }
    }

    /// <summary>
    ///     设置中频多路窄带参数
    /// </summary>
    /// <param name="channels"></param>
    private void SetIfmch(Dictionary<string, object>[] channels)
    {
        if (channels == null) return;
        for (var i = 0; i < channels.Length; ++i)
        {
            var template = (IfmcaTemplate)channels[i];
            var frequency = template.Frequency;
            var filterBw = template.FilterBandwidth;
            var demodulation = template.DemMode;
            var sqc = template.SquelchThreshold;
            var sqcswitch = template.SquelchSwitch;
            var iqswitch = template.IqSwitch;
            var audioswitch = template.AudioSwitch;
            var ifswitch = template.IfSwitch;
            var levelswitch = template.LevelSwitch;
            if (_preChannels == null || i >= _preChannels.Count)
            {
                _preChannels ??= new List<IfmcaTemplate>();
                SendCmd($"FREQ:DDC{i + 1} {frequency} MHz");
                SendCmd($"BAND:DDC{i + 1} {filterBw} kHz");
                SendCmd($"DEM:DDC{i + 1} {demodulation}");
                SendCmd($"OUTP:SQU:DDC{i + 1}:THR {sqc} dbuV");
                SendCmd($"OUTP:SQU:DDC{i + 1} {sqcswitch}");
                if (ifswitch)
                {
                    SendCmd($"SYST:IF:DDC{i + 1}:REM:MODE {(iqswitch ? "SHORT" : "OFF")}");
                    SendCmd($"SYST:AUD:DDC{i + 1}:REM:MOD {(audioswitch ? 1 : 0)}");
                }
                else
                {
                    SendCmd($"SYST:IF:DDC{i + 1}:REM:MODE OFF");
                    SendCmd($"SYST:AUD:DDC{i + 1}:REM:MOD {0}");
                }

                var channel = new IfmcaTemplate
                {
                    Frequency = frequency,
                    FilterBandwidth = filterBw,
                    DemMode = demodulation,
                    SquelchThreshold = sqc,
                    SquelchSwitch = sqcswitch,
                    LevelSwitch = levelswitch,
                    IqSwitch = iqswitch,
                    AudioSwitch = audioswitch,
                    IfSwitch = ifswitch
                };
                _preChannels.Add(channel);
            }
            else
            {
                //和之前的对比
                if (!frequency.EqualTo(_preChannels[i].Frequency, Epsilon))
                {
                    SendCmd($"FREQ:DDC{i + 1} {frequency} MHz");
                    _preChannels[i].Frequency = frequency;
                }

                if (!filterBw.EqualTo(_preChannels[i].FilterBandwidth, Epsilon))
                {
                    SendCmd($"BAND:DDC{i + 1} {filterBw} kHz");
                    _preChannels[i].FilterBandwidth = filterBw;
                }

                if (demodulation != _preChannels[i].DemMode)
                {
                    SendCmd($"DEM:DDC{i + 1} {demodulation}");
                    _preChannels[i].DemMode = demodulation;
                }

                if (!sqc.EqualTo(_preChannels[i].SquelchThreshold, (float)Epsilon))
                {
                    SendCmd($"OUTP:SQU:DDC{i + 1}:THR {sqc} dbuV");
                    _preChannels[i].SquelchThreshold = sqc;
                }

                if (!sqcswitch)
                {
                    SendCmd($"OUTP:SQU:DDC{i + 1} {sqcswitch}");
                    _preChannels[i].SquelchSwitch = sqcswitch;
                }

                if (iqswitch != _preChannels[i].IqSwitch)
                {
                    if (ifswitch) SendCmd($"SYST:IF:DDC{i + 1}:REM:MODE {(iqswitch ? "SHORT" : "OFF")}");
                    _preChannels[i].IqSwitch = iqswitch;
                }

                if (levelswitch != _preChannels[i].LevelSwitch) _preChannels[i].LevelSwitch = levelswitch;
                if (audioswitch != _preChannels[i].AudioSwitch)
                {
                    if (ifswitch) SendCmd($"SYST:AUD:DDC{i + 1}:REM:MOD {(audioswitch ? 1 : 0)}");
                    _preChannels[i].AudioSwitch = audioswitch;
                }

                if (ifswitch != _preChannels[i].IfSwitch)
                {
                    if (ifswitch)
                    {
                        SendCmd($"SYST:IF:DDC{i + 1}:REM:MODE {(iqswitch ? "SHORT" : "OFF")}");
                        SendCmd($"SYST:AUD:DDC{i + 1}:REM:MOD {(audioswitch ? 1 : 0)}");
                    }
                    else
                    {
                        SendCmd($"SYST:IF:DDC{i + 1}:REM:MODE OFF");
                        SendCmd($"SYST:AUD:DDC{i + 1}:REM:MOD {0}");
                    }

                    _preChannels[i].IfSwitch = ifswitch;
                }
            }
        }
    }

    /// <summary>
    ///     初始化DDC数据通道
    /// </summary>
    private void InitDdcPath()
    {
        var ddcIpEndPoint = _ddcChannel.LocalEndPoint as IPEndPoint;
        var address = ddcIpEndPoint?.Address.ToString();
        if (ddcIpEndPoint != null)
        {
            var port = ddcIpEndPoint.Port;
            for (var i = 0; i < 4; ++i)
            {
                SendCmd($"TRAC:DDC{i + 1}:UDP:DEL ALL");
                SendCmd($"TRAC:DDC{i + 1}:UDP:TAG \"{address}\", {port}, AUD, IF");
                SendCmd($"TRAC:DDC{i + 1}:UDP:FLAG \"{address}\", {port}, \"SWAP\",\"OPT\"");
            }
        }

        ResetDdcPath();
    }

    /// <summary>
    ///     重置DDC子通道
    /// </summary>
    private void ResetDdcPath()
    {
        for (var i = 0; i < 4; ++i)
        {
            SendCmd($"SYST:IF:DDC{i + 1}:REM:MODE OFF");
            SendCmd($"SYST:AUD:DDC{i + 1}:REM:MOD {0}");
        }
    }

    #endregion

    #region 数据校验

    /// <summary>
    ///     检查Fscan扫描参数是否已经设置成功
    /// </summary>
    private bool CheckFScanParameters()
    {
        var start = SendSyncCmd("SENS:FREQ:STAR?");
        if (!NumberExtension.IsValueEqual(start, StartFrequency, 1e-6)) return false;
        var stop = SendSyncCmd("SENS:FREQ:STOP?");
        if (!NumberExtension.IsValueEqual(stop, StopFrequency, 1e-6)) return false;
        var step = SendSyncCmd("SENS:SWE:STEP?");
        return NumberExtension.IsValueEqual(step, StepFrequency, 1e-3);
    }

    /// <summary>
    ///     检查PScan扫描参数是否已经设置成功
    /// </summary>
    private bool CheckPScanParameters()
    {
        var start = SendSyncCmd("SENS:FREQ:PSC:START?");
        if (!NumberExtension.IsValueEqual(start, StartFrequency, 1e-6)) return false;
        var stop = SendSyncCmd("SENS:FREQ:PSC:STOP?");
        if (!NumberExtension.IsValueEqual(stop, StopFrequency, 1e-6)) return false;
        var step = SendSyncCmd("PSC:STEP?");
        return NumberExtension.IsValueEqual(step, StepFrequency, 1e-3);
    }

    #endregion

    #region 辅助方法

    /// <summary>
    ///     接收子通道电平
    /// </summary>
    /// <param name="endflag"></param>
    private string RecvIfmchLevels(int endflag)
    {
        var total = 0;
        var buffer = new byte[1024 * 1024];
        while (_ddcCtrlChannel.Receive(buffer, total, 1, SocketFlags.None) > 0)
            if (buffer[total++] == endflag)
                break;
        return Encoding.ASCII.GetString(buffer, 0, total);
    }

    /// <summary>
    ///     宽带测向设置频谱带宽，约束脚本中以频谱带宽为主更新信道带宽列表
    /// </summary>
    /// <param name="span"></param>
    private void SetWbSpan(double span)
    {
        SendCmd("CALC:IFP:STEP:AUTO ON");
        Thread.Sleep(10);
        SendCmd($"FREQ:SPAN {span}KHz");
        Thread.Sleep(10);
        SendCmd("CALC:IFP:STEP:AUTO OFF");
        //TODO:如果宽带测向为运行时可修改参数，此处还需设置STEP
    }

    #endregion

    #region 角度矫正

    private float AngleAdjust(float angle, double frequency)
    {
        if (_angleCompensationList == null || _angleCompensationList.Count == 0) return angle;
        foreach (var info in _angleCompensationList)
            if (frequency >= info.StartFrequency && frequency <= info.StopFrequency)
                return (angle + info.Angle + 360) % 360;
        return angle;
    }

    private float[] AngleAdjust(float[] angle, double frequency)
    {
        if (_angleCompensationList == null || _angleCompensationList.Count == 0) return angle;
        foreach (var info in _angleCompensationList)
            if (frequency >= info.StartFrequency || frequency <= info.StopFrequency)
            {
                var newAngle = new float[angle.Length];
                for (var i = 0; i < newAngle.Length; i++) newAngle[i] = angle[i] + info.Angle;
                return newAngle;
            }

        return angle;
    }

    #endregion
}