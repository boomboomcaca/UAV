using System.Threading;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.PR100;

public partial class Pr100
{
    private void StartScan()
    {
        _index = 0;
        SendCmd("TRAC SSTART,0;TRAC SSTOP,0"); //删除接收机中的忽略频点
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

        SendCmd("CALC:IFP:AVER:TYPE OFF"); //关闭FFT模式，保证快速扫描
        Thread.Sleep(20);
        SendCmd("INIT");
    }

    private void StartFScan()
    {
        var total = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
        _cacheData = new short[total];
        SendCmd("SENS:FREQ:MODE SWE");
        //在进行多频段扫描时，频段间切换太频繁会导致扫描参数无法一次性设置正确
        var count = 0;
        while (count++ < 10)
        {
            SendCmd($"SENS:FREQ:STAR {StartFrequency} MHz");
            SendCmd($"SENS:FREQ:STOP {StopFrequency} MHz");
            SendCmd($"SENS:SWE:STEP {StepFrequency} kHz");
            if (CheckFscanParameters()) break;
        }

        SendCmd("SENS:SWE:COUN INF");
        SendCmd("SENS:SWE:DIR UP");
        SendCmd("SENS:GCON:MODE AGC");
        SendCmd("OUTP:SQU:STAT OFF");
        //以下三个参数未暴露给用户，取默认值
        SendCmd("SENS:DEM FM");
        SendCmd("SENSE:SWE:DWELL 0");
        SendCmd("SENSE:SWE:HOLD:TIME 0");
    }

    private void StartPScan()
    {
        var total = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
        _cacheData = new short[total];
        SendCmd("SENS:FREQ:MODE PSC");
        //在进行多频段扫描时，频段间切换太频繁会导致扫描参数无法一次性设置正确
        var count = 0;
        while (count++ < 10)
        {
            SendCmd($"SENS:FREQ:PSC:START {StartFrequency} MHz");
            SendCmd($"SENS:FREQ:PSC:STOP {StopFrequency} MHz");
            SendCmd($"PSC:STEP {StepFrequency} kHz");
            if (CheckPscanParameters()) break;
        }

        //由于解调模式与滤波带宽有约束且PR100的DSCan模式下解调模式为有效参数，
        //为避免从单频测量切换到全景扫描时违背约束出错，全景扫描时将解调模式置为默认值
        SendCmd("SENS:DEM FM");
        SendCmd("SENSE:SWE:DWELL 0");
        SendCmd("SENSE:SWE:HOLD:TIME 0");
    }

    private void StartMScan()
    {
        if (MscanPoints == null || MscanPoints.Length == 0 || MscanPoints.Length > 1000) return;
        //设置频点模式为离散扫描
        SendCmd("SENS:FREQ:MODE MSC");
        //清除所有频点
        SendCmd("MEMory:CLEar MEM0,MAXimum");
        // if (_curFeature == FeatureType.MScan)
        // {
        //     SendCmd("OUTP:SQU OFF");
        //     SendCmd("MSCan:CONTrol:OFF \"STOP:SIGN\"");
        //     SendCmd("SENSE:MSCAN:DWELL 0");
        //     SendCmd("SENSE:MSCAN:HOLD:TIME 0");
        // }
        // else if (_curFeature == FeatureType.MScne)
        // {
        SendCmd("MSCan:CONTrol:ON \"STOP:SIGN\"");
        SendCmd($"SENSE:MSCAN:DWELL {DwellTime} s");
        SendCmd($"SENSE:MSCAN:HOLD:TIME {HoldTime} s");
        // }
        SendCmd("SENSE:MSCAN:COUNT INF");
        SendCmd("SENS:GCON:MODE AGC");
        var att = _attenuation == -100 ? 0 : _attenuation;
        var attAuto = _attenuation == -1 ? "ON" : "OFF";
        _scanFreqs.Clear();
        for (var i = 0; i < MscanPoints.Length; ++i)
        {
            var dic = MscanPoints[i];
            var mscanPoint = (MscanTemplate)dic;
            var freq = mscanPoint.Frequency;
            var bw = mscanPoint.FilterBandwidth;
            var demMode = mscanPoint.DemMode;
            string cmd;
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

    /// <summary>
    ///     检查Fscan扫描参数是否已经设置成功
    /// </summary>
    private bool CheckFscanParameters()
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
    private bool CheckPscanParameters()
    {
        var start = SendSyncCmd("SENS:FREQ:PSC:START?");
        if (!NumberExtension.IsValueEqual(start, StartFrequency, 1e-6)) return false;
        var stop = SendSyncCmd("SENS:FREQ:PSC:STOP?");
        if (!NumberExtension.IsValueEqual(stop, StopFrequency, 1e-6)) return false;
        var step = SendSyncCmd("PSC:STEP?");
        return NumberExtension.IsValueEqual(step, StepFrequency, 1e-3);
    }
}