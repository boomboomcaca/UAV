using System.Threading;
using Magneto.Protocol.Define;

namespace Magneto.Device.DDF200M;

public partial class Ddf200M
{
    #region UDP通道处理

    private void InitUdpPath()
    {
        if (_media == MediaType.None) return;
        CloseUdpPath();
        OpenUdpPath();
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

            SendCmd("FREQ:MODE FIX");
        }
    }

    /// <summary>
    ///     打通各数据通道
    /// </summary>
    private void OpenUdpPath()
    {
        string tag = null;
        if ((_media & MediaType.Iq) > 0) tag += "IF,";
        if ((_media & MediaType.Spectrum) > 0) tag += "IFP,";
        if ((_media & MediaType.Audio) > 0) tag += "AUD,";
        if ((_media & MediaType.Scan) > 0) tag += "FSC,PSC,MSC,";
        if ((_media & (MediaType.Dfind | MediaType.Dfpan)) > 0) tag += "DFP,";
        if (tag != null)
        {
            tag = tag.Remove(tag.Length - 1);
            SendCmd($"TRAC:UDP:TAG:ON \"{_localaddr}\",{_localudpport},{tag}");
            Thread.Sleep(10);
            if (CurFeature == FeatureType.SCAN && ScanMode == ScanMode.Pscan)
                SendCmd($"TRAC:UDP:FLAG:ON \"{_localaddr}\",{_localudpport},\"SWAP\",\"OPT\",\"VOLT:AC\"");
            else if (CurFeature is FeatureType.FDF or FeatureType.WBDF)
                SendCmd($"TRAC:UDP:FLAG:ON \"{_localaddr}\",{_localudpport},\"SWAP\",\"OPT\",\"AZIM\",\"DFQ\",\"DFL\"");
            else
                SendCmd(
                    $"TRAC:UDP:FLAG:ON \"{_localaddr}\",{_localudpport},\"SWAP\",\"OPT\",\"VOLT:AC\",\"FREQ:RX\",\"FREQ:HIGH:RX\"");
            Thread.Sleep(10);
        }
    }

    /// <summary>
    ///     删除UDP数据通道
    /// </summary>
    private void CloseUdpPath()
    {
        SendCmd("TRAC:UDP:DEL ALL");
        Thread.Sleep(10);
    }

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

    #endregion
}