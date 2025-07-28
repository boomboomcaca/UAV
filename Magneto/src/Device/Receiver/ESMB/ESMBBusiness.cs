using System;
using System.Net;
using System.Threading;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.ESMB;

public partial class Esmb
{
    #region 开启任务

    private void SendMeadiaRequest()
    {
        if (_media == MediaType.None) return;
        //由于单频测量可以在任务运行过程中更改参数，所以此处需要先删除之前的UDP通道
        if (CurFeature is FeatureType.FFM or FeatureType.IFOUT) CloseUdpPath();
        OpenUdpPath();
        StartMeasure();
    }

    private void StartMeasure()
    {
        // 中频输出
        if ((_media & MediaType.Iq) == MediaType.Iq)
        {
            SendCmd("FUNC:CONC OFF;:FUNC \"VOLT:AC\"");
            SendCmd("SENS:FREQ:MODE FIX");
        }
        else if ((_media & MediaType.Scan) == 0)
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
        }
        else
        {
            SendCmd(CurFeature == FeatureType.MScne
                ? "FUNC:CONC ON;:FUNC \"VOLT:AC\""
                : "FUNC:CONC OFF;:FUNC \"VOLT:AC\"");
            StartScan();
        }
    }

    private void StartScan()
    {
        SendCmd("TRAC SSTRAT,0;:TRAC SSTOP,0"); //删除接收机中的忽略频点
        switch (ScanMode)
        {
            case ScanMode.Fscan:
                StartFScan();
                break;
            case ScanMode.Pscan:
                StartDScan();
                break;
            case ScanMode.MScan:
                StartMScan();
                break;
        }

        SendCmd("FREQ:SYNT:MODE FAST");
        SendCmd("CALC:IFP:AVER:TYPE OFF"); //关闭FFT模式，保证快速扫描
        Thread.Sleep(200);
        SendCmd("INIT");
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
    private bool CheckDScanParameters()
    {
        var start = SendSyncCmd("SENS:FREQ:DSC:START?");
        if (!NumberExtension.IsValueEqual(start, StartFrequency, 1e-6)) return false;
        var stop = SendSyncCmd("SENS:FREQ:DSC:STOP?");
        if (!NumberExtension.IsValueEqual(stop, StopFrequency, 1e-6)) return false;
        var band = SendSyncCmd("SENS:BAND?");
        return NumberExtension.IsValueEqual(band, FilterBandwidth, 1e-3);
    }

    private void StartFScan()
    {
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

    private void StartDScan()
    {
        SendCmd("SENS:FREQ:MODE DSC");
        SendCmd("SENS:DSC:COUN INF");
        SendCmd("SENS:GCON:MODE AGC");
        SendCmd("OUTP:SQU:STAT OFF");
        //由于解调模式与滤波带宽有约束且ESMB的DSCan模式下解调模式为有效参数，
        //为避免从单频测量切换到全景扫描时违背约束出错，全景扫描时将解调模式置为默认值
        SendCmd("SENS:DEM FM");
        //在进行多频段扫描时，频段间切换太频繁会导致扫描参数无法一次性设置正确
        var count = 0;
        while (count++ < 10)
        {
            SendCmd($"SENS:FREQ:DSC:START {StartFrequency} MHz");
            SendCmd($"SENS:FREQ:DSC:STOP {StopFrequency} MHz");
            _filterBandwidth = StepFrequency * 2;
            SendCmd($"SENS:BAND {_filterBandwidth} kHz");
            if (CheckDScanParameters()) break;
        }
    }

    private void StartMScan()
    {
        if (MscanPoints == null || MscanPoints.Length == 0 || MscanPoints.Length > 1000) return;
        SendCmd("MEAS:TIME DEF");
        SendCmd("FREQ:MODE MSC");
        SendCmd("MEMory:CLEar MEM0,MAXimum");
        SendCmd("OUTPUT:SQUELCH:CONTROL MEM"); //静噪控制方式
        if (CurFeature == FeatureType.MScan)
        {
            SendCmd("SENSE:MSCAN:DWELL 0");
            SendCmd("SENSE:MSCAN:HOLD:TIME 0");
            SendCmd("OUTP:SQU:STAT OFF");
            SendCmd("MSCan:CONTrol:OFF \"STOP:SIGN\"");
        }
        else
        {
            SendCmd("MSCan:CONTrol:ON \"STOP:SIGN\"");
            SendCmd($"SENSE:MSCAN:DWELL {_dwellTime}s");
            SendCmd($"SENSE:MSCAN:HOLD:TIME {_holdTime}ms");
        }

        SendCmd("SENSE:MSCAN:COUNT INF");
        SendCmd("SENS:GCON:MODE AGC");
        string att;
        string attA;
        if (Math.Abs(_attenuation - 1) < 1e-9)
        {
            att = "ON";
            attA = "OFF";
        }
        else if (_attenuation == 0)
        {
            att = "OFF";
            attA = "OFF";
        }
        else
        {
            att = "ON";
            attA = "ON";
        }

        var squc = _squelchSwitch ? "ON" : "OFF";
        _scanFreqs.Clear();
        for (var i = 0; i < MscanPoints.Length; i++)
        {
            var dic = MscanPoints[i];
            var mscanPoint = (MscanTemplate)dic;
            var freq = mscanPoint.Frequency;
            var bw = mscanPoint.FilterBandwidth;
            var demMode = mscanPoint.DemMode;
            // if (!_dwellSwitch)
            // {
            //     SendCmd($"MEM:CONT MEM{i},{freq} MHz,0,FM,{bw} kHz,(@1),{att},{attA},OFF,OFF,ON");
            // }
            // else
            // {
            SendCmd(
                $"MEM:CONT MEM{i},{freq} MHz,{_squelchThreshold},{demMode},{bw} kHz,(@1),{att},{attA},{squc},OFF,ON");
            // }
            _scanFreqs.Add(freq);
        }
    }

    private void CloseUdpPath()
    {
        SendCmd("TRAC:UDP:DEL ALL");
        Thread.Sleep(10);
        SendCmd("TRAC:UDP:DEF:DEL ALL");
        Thread.Sleep(10);
    }

    private void OpenUdpPath()
    {
        var localIpAddress = (_cmdSocket.LocalEndPoint as IPEndPoint)?.Address.ToString(); //本地连接设备的ip
        var localUdpPort = (_dataSocket.LocalEndPoint as IPEndPoint)?.Port ?? 19000;
        string tag = null;
        if ((_media & MediaType.Audio) > 0) tag += "AUD,";
        if ((_media & MediaType.Scan) > 0) tag += "FSC,DSC,MSC,";
        if ((_media & MediaType.Spectrum) > 0) tag += "IFP,";
        if (tag == null) return;
        tag = tag.Remove(tag.Length - 1);
        SendCmd($"TRAC:UDP:TAG:ON \"{localIpAddress}\",{localUdpPort},{tag}");
        Thread.Sleep(10);
        SendCmd($"TRAC:UDP:FLAG:ON \"{localIpAddress}\",{localUdpPort},\"SWAP\",\"OPT\",\"VOLT:AC\",\"FREQ:RX\"");
        Thread.Sleep(10);
    }

    #endregion

    #region IFBandwidth/DemMode

    private void SetFilterBandwidth(double dstIfBandwidth)
    {
        if (dstIfBandwidth > 9)
        {
            //当解调模式为CW,USB,LSB时，解调带宽只能 <= 9kHz
            //当解调模式为ISB时，解调带宽只能 <= 15k
            var strDemMode = SendSyncCmd("SENS:DEM?");
            if (strDemMode == "CW" || strDemMode == "USB" || strDemMode == "LSB"
                || (dstIfBandwidth > 15 && strDemMode == "ISB"))
            {
                SendCmd("SENS:DEM FM");
                Thread.Sleep(10);
            }
        }

        SendCmd($"SENS:BAND {dstIfBandwidth} kHz");
    }

    private void SetDemodulation(Modulation dstDemMode)
    {
        if (dstDemMode is Modulation.Cw or Modulation.Usb or Modulation.Lsb or Modulation.Isb)
        {
            //当解调带宽 > 9kHz时，解调带宽不能设置为CW, USB, LSB
            //当解调带宽 > 15kHz时，解调带宽不能设置为CW, USB, LSB, ISB
            var strIfBandwidth = SendSyncCmd("SENS:BAND?");
            var ifBandwidth = double.Parse(strIfBandwidth) / 1000;
            if (dstDemMode == Modulation.Isb)
            {
                if (ifBandwidth > 15)
                {
                    SendCmd("SENS:BAND 9 kHz");
                    Thread.Sleep(10);
                }
            }
            else //CW, USB, LSB
            {
                if (ifBandwidth > 9)
                {
                    SendCmd("SENS:BAND 9 kHz");
                    Thread.Sleep(10);
                }
            }
        }

        SendCmd($"SENS:DEM {dstDemMode}");
    }

    #endregion
}