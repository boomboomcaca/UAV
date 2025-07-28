using System.Threading;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.ESMD;

public partial class Esmd
{
    #region 任务开启

    //开始测量
    private void StartMeasure()
    {
        if (CurFeature == FeatureType.TDOA)
        {
            SendCmd("INIT");
            SendCmd(":Meas:Mode PER");
            SendCmd(":VIDEO:PIC SINGLE");
            return;
        }

        if ((_media & MediaType.Scan) == 0)
        {
            if ((_media & MediaType.Itu) > 0)
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
            if (CurFeature == FeatureType.IFMCA) SetIfmch(_ddcChannels);
        }
        else
        {
            SendCmd("FUNC:CONC OFF;:FUNC \"VOLT:AC\"");
            StartScan();
        }
    }

    private void StartScan()
    {
        if (_ifBandwidth > 20000)
        {
            _ifBandwidth = 20000;
            SendCmd($"FREQ:SPAN {_ifBandwidth}kHz");
        }

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

    private bool CheckFScanParameters()
    {
        var start = SendSyncCmd("FREQ:STAR?");
        if (!NumberExtension.IsValueEqual(start, StartFrequency, 1e-6)) return false;
        var stop = SendSyncCmd("SENS:FREQ:STOP?");
        if (!NumberExtension.IsValueEqual(stop, StopFrequency, 1e-6)) return false;
        var step = SendSyncCmd("SENS:SWE:STEP?");
        if (!NumberExtension.IsValueEqual(step, StepFrequency, 1e-3)) return false;
        return true;
    }

    private bool CheckPScanParameters()
    {
        var start = SendSyncCmd("FREQ:PSC:START?");
        if (!NumberExtension.IsValueEqual(start, StartFrequency, 1e-6)) return false;
        var stop = SendSyncCmd("FREQ:PSC:STOP?");
        if (!NumberExtension.IsValueEqual(stop, StopFrequency, 1e-6)) return false;
        var step = SendSyncCmd("PSC:STEP?");
        if (!NumberExtension.IsValueEqual(step, StepFrequency, 1e-3)) return false;
        return true;
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
            //在频段扫描-频点扫描模式下设置一下默认值
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

        //由于解调模式与滤波带宽有约束且ESMD的PSCan模式下解调模式为有效参数，
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
            SendCmd("MScan:CONTrol:OFF \"STOP:SIGN\"");
            SendCmd("SENSE:MScan:DWELL 0");
            SendCmd("SENSE:MScan:HOLD:TIME 0");
        }
        else
        {
            SendCmd("MScan:CONTrol:ON \"STOP:SIGN\"");
            SendCmd($"SENSE:MScan:DWELL {_dwellTime} s");
            SendCmd($"SENSE:MScan:HOLD:TIME {_holdTime} ms");
        }

        SendCmd("SENSE:MScan:COUNT INF");
        SendCmd("SENS:GCON:MODE AGC");
        lock (_lockFreqs)
        {
            var att = _attenuation == -1 ? 0 : _attenuation;
            var attAuto = _attenuation == -1 ? "ON" : "OFF";
            _scanFreqs.Clear();
            for (var i = 0; i < MScanPoints.Length; ++i)
            {
                string cmd;
                var dic = MScanPoints[i];
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

    private void StartTdoa()
    {
        _tdoaQueue?.Clear();
        // 设置数字中频模式 32bit I/32bit Q
        SendCmd("ABORT");
        // 当中频带宽超过1MHz以后只支持16位IQ
        if (_ifBandwidth > 1000 || IqWidth == 16)
            SendCmd("SYSTem:IF:REMote:MODe SHORT");
        else
            SendCmd("SYSTem:IF:REMote:MODe LONG");
        SendCmd("*SRE 0;*ESE 0;*CLS");
        SendCmd(":VIDEO:PIC SINGLE");
        SendCmd(":FORM:DATA ASCII;:FORMAT:BORD NORM");
        SendCmd(":FUNC:OFF \"VOLT: AC\"");
        SendCmd(":TRAC:FEED:CONT MTRACE, NEV;CONT ITRACE, NEV;CONT IFPAN, NEV");
        SendCmd(":STAT:TRAC:ENAB 0;*CLS");
        // SendCmd("ROUT:HF (@0)");
        // SendCmd("ROUT:VUHF (@0)");
        SendCmd(":FREQ:MODE CW");
        SendCmd(":DEM FM;:Band " + (int)(_ifBandwidth * 1000));
        var cmd = string.Empty;
        switch (_detector)
        {
            case DetectMode.Fast:
                cmd = "FAST";
                break;
            case DetectMode.Pos:
                cmd = "POS";
                break;
            case DetectMode.Avg:
                cmd = "PAV";
                break;
            case DetectMode.Rms:
                cmd = "RMS";
                break;
        }

        SendCmd(":DET " + cmd);
        if (_attenuation.Equals(-1))
        {
            SendCmd("INP:ATT:AUTO ON");
            SendCmd("INP:ATT:AUTO:HOLD:TIME 0");
        }
        else
        {
            SendCmd("INP:ATT:AUTO OFF");
            SendCmd($"INP:ATT {_attenuation}");
        }

        SendCmd($"INP:ATT:MODE {_rfMode.ToString().ToUpper()}");
        SendCmd(":OUTP:FILT:MODE OFF");
        SendCmd("SENSE:GCON:AUTO:TIME DEF");
        SendCmd(":INP:ATT:AUTO:HOLD:TIME 0 ");
        SendCmd("SENSE:DEM:BFO 1000 Hz");
        SendCmd(":FREQ:AFC OFF");
        SendCmd(
            $"CALC:IFPAN:STEP:AUTO ON;:FREQ:SPAN {(int)(_ifBandwidth * 1000)}Hz;:meas:band:lim:auto ON;:CALC:IFPAN:AVER:TYPE OFF");
        SendCmd("meas:band:lim:auto on");
        SendCmd("CALC:IFPAN:STEP:AUTO ON");
        SendCmd("CALC:PIFP:MODE OFF");
        SendCmd("CALC:PIFP:ACTT 0.0150");
        SendCmd("CALC:PIFP:OBST 0.5000");
        SendCmd($":FREQ {_frequency} MHZ"); ///////////
        SendCmd($":OUTP:SQU:THR {_squelchThreshold} dBuV"); //////////
        if (_squelchSwitch)
            SendCmd("OUTPut:SQUelch ON");
        else
            SendCmd("OUTPut:SQUelch OFF");
        SendCmd(":GCON:MODE AUTO");
        // SendCmd(":SYST:SPEAKER:STAT OFF;:SYSTEM:AUDIO:VOL MIN");
        SendCmd($"FREQ:DEM {(long)(_frequency * 1000000)}Hz");
        if (_measureTime == -1)
            SendCmd("MEAS:TIME DEF");
        else
            SendCmd($"MEAS:TIME {_measureTime}us");
        SendCmd($":Meas:Band:Mode {_bandMeasureMode}");
        SendCmd($":Meas:Band:Beta {_beta}");
        SendCmd($":Meas:Band:XDB {_xdB} dB");
        SendCmd(":SYSTEM:VIDEO:REMOTE:MODE OFF;:DISP:MENU IFPAN");
        //SendCmd(":SYSTEM:IF:REMOTE:MODE LONG");
        SendCmd(":SENS:VID:STAN B");
        SendCmd("CALC:IFPAN:SEL AUTO");
        SendCmd(":SENSE:DEC:SELC:STATE OFF");
        SendCmd($"OUTP:VID:MODE IF;Freq {(long)(_frequency * 1000000)} Hz");
        SendCmd(":ROSC:SOUR INT");
        SendCmd(":FUNC:OFF \"VOLT: AC\"");
        SendCmd(":FUNC:OFF \"FREQ: OFFS\"");
        SendCmd(":FUNC:OFF \"AM\"");
        SendCmd(":FUNC:OFF \"AM: POS\"");
        SendCmd(":FUNC:OFF \"AM: NEG\"");
        SendCmd(":FUNC:OFF \"FM\"");
        SendCmd(":FUNC:OFF \"FM: POS\"");
        SendCmd(":FUNC:OFF \"FM: NEG\"");
        SendCmd(":FUNC:OFF \"PM\"");
        SendCmd(":FUNC:OFF \"BAND\"");
        SendCmd(":FUNC:OFF \"DFL\"");
        SendCmd(":FUNC:OFF \"AZIM\"");
        SendCmd(":FUNC:OFF \"DFQ\"");
        SendCmd(":FUNC:OFF \"VOLT: AC\"");
        SendCmd(":FUNC:OFF \"FREQ: OFFS\"");
        SendCmd(":FUNC:OFF \"AM\"");
        SendCmd(":FUNC:OFF \"AM: POS\"");
        SendCmd(":FUNC:OFF \"AM: NEG\"");
        SendCmd(":FUNC:OFF \"FM\"");
        SendCmd(":FUNC:OFF \"FM: POS\"");
        SendCmd(":FUNC:OFF \"FM: NEG\"");
        SendCmd(":FUNC:OFF \"PM\"");
        SendCmd(":FUNC:OFF \"BAND\"");
        SendCmd(":FUNC:OFF \"DFL\"");
        SendCmd(":FUNC:OFF \"AZIM\"");
        SendCmd(":FUNC:OFF \"DFQ\"");
        SendCmd("Meas:Mode PER");
    }

    #endregion
}