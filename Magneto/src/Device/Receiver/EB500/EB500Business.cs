using System;
using System.Linq;
using System.Net;
using System.Threading;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.EB500;

public partial class Eb500
{
    #region 自定义函数

    private void SendMediaRequest()
    {
        if (_mediaType == MediaType.None) return;
        //由于单频测量可以在任务运行过程中更改参数，所以此处需要先删除之前的UDP通道
        if (CurFeature == FeatureType.FFM) CloseUdpPath();
        OpenUdpPath();
        StartMeasure();
    }

    private void StartMeasure()
    {
        if ((_mediaType & MediaType.Scan) == 0)
        {
            if ((_mediaType & MediaType.Itu) > 0)
            {
                SendCmd("FUNC:CONC ON");
                SendCmd(
                    "FUNC \"VOLT:AC\", \"AM\", \"AM:POS\", \"AM:NEG\", \"FM\", \"FM:POS\", \"FM:NEG\", \"PM\", \"BAND\"");
            }
            else
            {
                SendCmd("FUNC:CONC OFF;:FUNC \"VOLT:AC\"");
            }

            SendCmd("SENS:FREQ:MODE FIX");
        }
        else
        {
            if (CurFeature == FeatureType.MScne)
                SendCmd("FUNC:CONC ON;:FUNC \"VOLT:AC\"");
            else
                SendCmd("FUNC:CONC OFF;:FUNC \"VOLT:AC\"");
            StartScan();
        }
    }

    private void StartScan()
    {
        SendCmd("TRAC SSTART,0;TRAC SSTOP,0"); //TODO:删除接收机中的忽略频点
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

        //开启快速扫描,扫描速度最快 LOWQ:低相噪，NORM:常规 速度介于低相噪和快速之间，FAST:快速
        SendCmd("FREQ:SYNT:MODE FAST");
        SendCmd("CALC:IFP:AVER:TYPE OFF"); //关闭FFT模式，保证快速扫描
        Thread.Sleep(10);
        SendCmd("INIT");
    }

    /// <summary>
    ///     关闭UDP数据通道
    /// </summary>
    private void CloseUdpPath()
    {
        SendCmd("TRAC:UDP:DEL ALL");
        Thread.Sleep(10);
        SendCmd("TRAC:UDP:DEF:DEL ALL");
        Thread.Sleep(10);
    }

    /// <summary>
    ///     开启UDP数据通道
    /// </summary>
    private void OpenUdpPath()
    {
        var localIpAddress = (_cmdSocket.LocalEndPoint as IPEndPoint)?.Address.ToString(); //本地连接设备的ip
        var localUdpPort = (_dataSocket.LocalEndPoint as IPEndPoint)?.Port ?? 0;
        string tag = null;
        if ((_mediaType & MediaType.Audio) > 0) tag += "AUD,";
        if ((_mediaType & MediaType.Scan) > 0) tag += "FSC,PSC,MSC,";
        if ((_mediaType & MediaType.Spectrum) > 0) tag += "IFP,";
        if ((_mediaType & MediaType.Iq) > 0) tag += "IF,";
        if (tag == null) return;
        tag = tag.Remove(tag.Length - 1);
        SendCmd($"TRAC:UDP:TAG:ON \"{localIpAddress}\",{localUdpPort},{tag}");
        if (tag.Split(',').ToList().Contains("IF"))
            Thread.Sleep(50);
        else
            Thread.Sleep(10);
        SendCmd($"TRAC:UDP:FLAG:ON \"{localIpAddress}\",{localUdpPort},\"SWAP\",\"OPT\",\"VOLT:AC\",\"FREQ:RX\"");
        Thread.Sleep(10);
    }

    private void StartFScan()
    {
        //设置频率模式
        SendCmd("SENS:FREQ:MODE SWE");
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
            SendCmd($"SENS:SWE:DWELL {DwellTime} s");
            SendCmd($"SENS:SWE:HOLD:TIME {HoldTime} s");
        }
        else
        {
            //在频段扫描-频点扫描模式下设置以下默认值
            SendCmd("OUTP:SQU:STAT OFF");
            SendCmd("SENS:DEM FM");
            SendCmd("SENS:SWE:DWELL 0");
            SendCmd("SENS:SWE:HOLD:TIME 0");
        }
    }

    private void StartPScan()
    {
        SendCmd("SENS:FREQ:MODE PSC");
        SendCmd("SENS:PSC:COUN INF");
        SendCmd("SENS:GCON:MODE AGC");
        SendCmd("OUTP:SQU:STAT OFF");
        //在进行多频段扫描时，频段间切换太频繁会导致扫描参数无法一次性设置正确
        var count = 0;
        while (count++ < 10)
        {
            SendCmd($"SENS:FREQ:PSC:START {StartFrequency} MHz");
            SendCmd($"SENS:FREQ:PSC:STOP {StopFrequency} MHz");
            SendCmd($"PSC:STEP {StepFrequency} kHz");
            if (CheckPScanParameters()) break;
        }

        //由于解调模式与滤波带宽有约束且EB500的PSCan模式下解调模式为有效参数，
        //为避免从单频测量切换到全景扫描时违背约束出错，全景扫描时将解调模式置为默认值
        SendCmd("SENS:DEM FM");
    }

    private void StartMScan()
    {
        //频点个数校验,放到脚本里去验证
        //设置频点模式为离散扫描
        SendCmd("SENS:FREQ:MODE MSC");
        //清除所有频点
        SendCmd("MEMory:CLEar MEM0,MAXimum");
        if (CurFeature == FeatureType.MScan)
        {
            SendCmd("OUTP:SQU OFF");
            SendCmd("MSCan:CONTrol:OFF \"STOP:SIGN\"");
            SendCmd("SENSE:MSCAN:DWELL 0.1");
            SendCmd("SENSE:MSCAN:HOLD:TIME 0.1");
        }
        else
        {
            SendCmd("MSCan:CONTrol:ON \"STOP:SIGN\"");
            SendCmd($"SENSE:MSCAN:DWELL {DwellTime} s");
            SendCmd($"SENSE:MSCAN:HOLD:TIME {HoldTime} ms");
        }

        SendCmd("SENSE:MSCAN:COUNT INF");
        SendCmd("SENS:GCON:MODE AGC");
        var att = _attenuation == -1 ? 0 : _attenuation;
        var attAuto = _attenuation == -1 ? "ON" : "OFF";
        _scanFreqs.Clear();
        for (var i = 0; i < MscanPoints.Length; ++i)
        {
            var dic = MscanPoints[i];
            var mscanPoint = (MscanTemplate)dic;
            var freq = mscanPoint.Frequency;
            var bw = mscanPoint.FilterBandwidth;
            var demMode = mscanPoint.DemMode;
            var squelchThreshold = _squelchSwitch ? SquelchThreshold : 0;
            var squcState = _squelchSwitch ? "ON" : "OFF";
            var cmd =
                $"MEM:CONT MEM{i},{freq} MHz,{squelchThreshold},{demMode},{bw} kHz,(@1),{att},{attAuto},{squcState},OFF,ON";
            SendCmd(cmd);
            _scanFreqs.Add(freq);
        }
    }

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
    ///     检查Dscan扫描参数是否已经设置成功
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

    private void SetDemodulation(Modulation dstDemMode)
    {
        if (dstDemMode is Modulation.Cw or Modulation.Usb or Modulation.Lsb)
        {
            //当解调带宽 > 9kHz时，解调带宽不能设置为CW,USB,LSB
            var strIfBandwidth = SendSyncCmd("SENS:BAND?");
            var ifBandwidth = double.Parse(strIfBandwidth) / 1000;
            if (ifBandwidth > 9)
            {
                SendCmd("SENS:BAND 9 kHz");
                Thread.Sleep(10);
            }
        }

        SendCmd($"SENS:DEM {dstDemMode}");
    }

    private void SetIfStep(double dstIfStep)
    {
        var strSpan = SendSyncCmd("FREQ:SPAN?");
        var span = double.Parse(strSpan) / 1000;
        var stepsTemp = GetIfSteps(span);
        if (stepsTemp.Contains(dstIfStep))
        {
            SendCmd($"CALC:IFP:STEP {dstIfStep}kHz");
            return;
        }

        if (dstIfStep < stepsTemp[0])
        {
            SendCmd($"CALC:IFP:STEP {stepsTemp[0]}kHz");
            Thread.Sleep(10);
            var spansTemp = GetSpans(stepsTemp[0]);
            SendCmd($"FREQ:SPAN {spansTemp[0]}kHz");
            Thread.Sleep(10);
            SetIfStep(dstIfStep);
        }
        else if (dstIfStep > stepsTemp[^1])
        {
            SendCmd($"CALC:IFP:STEP {stepsTemp[^1]}kHz");
            Thread.Sleep(10);
            var spansTemp = GetSpans(stepsTemp[^1]);
            SendCmd($"FREQ:SPAN {spansTemp[^1]}KHz");
            Thread.Sleep(10);
            SetIfStep(dstIfStep);
        }
    }

    private void SetFilterBandwidth(double dstIfBandwidth)
    {
        if (dstIfBandwidth > 9)
        {
            //当解调模式为CW,USB,LSB时,解调带宽只能<=9kHz
            var strDemMode = SendSyncCmd("SENS:DEM?");
            if (strDemMode is "CW" or "USB" or "LSB")
            {
                SendCmd("SENS:DEM FM");
                Thread.Sleep(10);
            }
        }

        SendCmd($"SENS:BAND {dstIfBandwidth} kHz");
    }

    private double[] GetSpans(double ifstep)
    {
        var idstep = Array.IndexOf(Consts.ArrayStep, ifstep);
        var isFind = Consts.Spans.TryGetValue(idstep, out var value);
        if (!isFind) return null;
        var start = value.start;
        var end = value.end;
        var result = new double[end - start + 1];
        Array.Copy(Consts.ArraySpan, start, result, 0, result.Length);
        return result;
    }

    private double[] GetIfSteps(double span)
    {
        var idspan = Array.IndexOf(Consts.ArraySpan, span);
        var isMatch = Consts.IfSteps.TryGetValue(idspan, out var value);
        if (!isMatch) return null;
        var result = new double[value.end - value.start + 1];
        Array.Copy(Consts.ArrayStep, value.start, result, 0, result.Length);
        return result;
    }

    #endregion
}