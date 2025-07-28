using System.Collections.Generic;
using System.Diagnostics;
using Magneto.Protocol.Define;

namespace Magneto.Device.DDF550;

public partial class Ddf550
{
    #region 单频测量

    /// <summary>
    ///     设置DFMode为Rx模式
    /// </summary>
    private bool SetModeRx()
    {
        return DfMode(EDfMode.DfmodeRx);
    }

    private void SetParamsFq()
    {
        // 以下参数暂时未暴露给外部
        // 测量模式-这里存疑,Per模式下周期性的根据MearuseTime发送(0为自动测量时间),如果是Cont模式则在下发Initiate以后连续发送★★★★★★★★★★★★★★★
        RxSettings(0, EMeasureModeCp.MeasuremodecpPer);
        // 设置天线
        AntennaControl(EAntCtrlMode.AntCtrlModeAuto, null, null);
        SelCall(false);
        ReferenceMode(EReferenceMode.ReferenceModeInternal);
        // 增益定时特性
        SendCommand(CmdDemodulationsettings, "eLevelIndicator", nameof(ELevelIndicatir.LevelIndicatorAvg));
        SendCommand(CmdDemodulationsettings, "eGainTiming", nameof(EGainTiming.GcDefault));
        // 设置差拍频率-是否提出来成为参数
        SendCommand(CmdDemodulationsettings, "iBfoFrequency", "1000");
        // 设置自动频率控制或手动频率控制-是否提出来成为参数 AFC (automatic frequency control) or MFC (manual f. c.) in use (true: AFC, false: MFC). 
        SendCommand(CmdDemodulationsettings, "bAfc", "false");
        // 设置音频参数
        SetAudioMode(EAudioMode.AudioMode32Khz16BitMono);
        // IQ设置为16位
        SendCommand(CmdIfmode, "eIfMode", nameof(EifMode.If16Bit));
        // 天线前置放大器关闭
        // 单频测量的保持时间需要设置为0
        // 设置中频模式
        MeasureSettingsFfm(new Param("iAttHoldTime", "0"),
            new Param("eIFPanSelectivity", nameof(EifPanSelectivity.IfpanSelectivityAuto)));
        // 设置频谱参数
        SetSpectrumSpan(_ifBandwidth);
        CheckAllSwitch();
    }

    /// <summary>
    ///     设置频谱带宽
    /// </summary>
    private void SetSpectrumSpan(double bandwidth)
    {
        var span = (int)(uint)bandwidth;
        var low = 0 - span / 2;
        var high = span / 2;
        SetItu(new Param("bUseAutoBandwidthLimits", "true"), new Param("iLowerBandwidthLimit", low.ToString()),
            new Param("iUpperBandwidthLimit", high.ToString()));
        // 同时设置频谱带宽和中频带宽
        if (CurFeature == FeatureType.FFM)
        {
            _ifbw = GetDefaultIfBandWidth();
            _resolutionBandwidth = GetDefaultIfBandWidth();
            Dictionary<string, string> keyValuePairs = new()
            {
                { "eSpan", ((ESpan)(uint)bandwidth).ToString() },
                { "eIFPanStep", ((EifPanStep)(long)(_ifbw * 1000 * 100)).ToString() },
                { "eDfPanStep", ((EDfPanStep)(long)(_resolutionBandwidth * 1000 * 100)).ToString() }
            };
            SendCommand(CmdMeasuresettingsffm, keyValuePairs);
        }
        else
        {
            SendCommand(CmdMeasuresettingsffm, "eSpan", ((ESpan)(uint)bandwidth).ToString());
        }
    }

    #endregion 单频测量

    #region 单频测向/宽带测向

    private bool SetModeFfm()
    {
        return DfMode(EDfMode.DfmodeFfm);
    }

    private void SetParamsDf()
    {
        RxSettings(0, EMeasureModeCp.MeasuremodecpPer);
        //设置天线
        AntennaControl(EAntCtrlMode.AntCtrlModeAuto, null, null);
        SelCall(false);
        ReferenceMode(EReferenceMode.ReferenceModeInternal);
        SendCommand(CmdItu, "bUseAutoBandwidthLimits", "true");
        SendCommand(CmdMeasuresettingsffm, "eWindowType", nameof(EWindowType.DfWindowTypeBlackman));
        SendCommand(CmdMeasuresettingsffm, "eDfPanSelectivity", nameof(EDfPanSelectivity.DfpanSelectivityAuto));
        //// 天线前置放大器打开或关闭
        var eState = _amplifier ? EState.StateOn : EState.StateOff;
        SendCommand(CmdMeasuresettingsffm, "eAntPreAmp", eState.ToString());
        SendCommand(CmdMeasuresettingsffm, "eIFPanMode", nameof(EifPanMode.IfpanModeClrwrite));
        SendCommand(CmdRxsettings, "iMeasureTime", "0");
        // 设置音频参数
        SetAudioMode(EAudioMode.AudioMode32Khz16BitMono);
        if (CurFeature == FeatureType.WBDF)
        {
            var span = (int)((uint)_dfBandwidth * 1000);
            var low = 0 - span / 2;
            var high = span / 2;
            SetItu(new Param("bUseAutoBandwidthLimits", "true"),
                new Param("iLowerBandwidthLimit", low.ToString()), new Param("iUpperBandwidthLimit", high.ToString()));
            MeasureSettingsFfm(
                new Param("eDfPanStep", ((EDfPanStep)(long)(_resolutionBandwidth * 1000 * 100)).ToString()),
                new Param("eSpan", ((ESpan)(uint)_dfBandwidth).ToString()));
        }

        if (CurFeature == FeatureType.FFDF) SetDfBandWidthAndSpectrumSpan();
        _iqSwitch = false;
        _ituSwitch = false;
        CheckAllSwitch();
    }

    /// <summary>
    ///     设置测向带宽和频谱带宽
    /// </summary>
    private void SetDfBandWidthAndSpectrumSpan()
    {
        //_spectrumSpan = GetDefaultSpectrumSpan(_dfBandWidth);
        _ifbw = -1;
        Dictionary<string, string> keyValuePairs = new()
        {
            { "eDfPanStep", ((EDfPanStep)(long)(_resolutionBandwidth * 1000 * 100)).ToString() },
            { "eSpan", ((ESpan)(uint)_dfBandwidth).ToString() },
            { "eIFPanStep", ((EifPanStep)(long)(_ifbw * 1000 * 100)).ToString() }
        };
        SendCommand(CmdMeasuresettingsffm, keyValuePairs);
    }

    #endregion 单频测向/宽带测向

    #region 全景扫描PScan

    /// <summary>
    ///     设置全景扫描模式
    /// </summary>
    internal bool SetModeRxpscan()
    {
        return DfMode(EDfMode.DfmodeRxpscan);
    }

    /// <summary>
    ///     设置全景扫描参数
    /// </summary>
    internal void SetRxpscanParams()
    {
        var keyValuePairs = new Dictionary<string, string>
        {
            { "iFreqBegin", ((long)(StartFrequency * 1000000)).ToString() },
            { "iFreqEnd", ((long)(StopFrequency * 1000000)).ToString() },
            { "eStep", ((EpScanStep)(int)(StepFrequency * 1000)).ToString() }
        };
        _ = SendCommand("MeasureSettingsPScan", keyValuePairs);
        CheckAllSwitch();
    }

    #endregion 全景扫描PScan

    #region 扫描测向

    /// <summary>
    ///     设置扫描测向模式
    /// </summary>
    private bool SetModeScan()
    {
        return DfMode(EDfMode.DfmodeScan);
    }

    /// <summary>
    ///     设置扫描测向的参数
    /// </summary>
    private void SetParamsScan()
    {
        ScanRangeDeleteAll();
        // ReferenceMode(EReference_Mode.REFERENCE_MODE_INTERNAL, out _);
        // RxSettings(0, EMeasureModeCP.MEASUREMODECP_CONT, out _);
        var begin = (long)(StartFrequency * 1000000);
        var end = (long)(StopFrequency * 1000000);
        _ifBandwidth = GetMaxSpecturmSpan(StepFrequency);
        var span = (ESpan)(uint)_ifBandwidth;
        var dfStep = (EDfPanStep)(long)(StepFrequency * 1000 * 100);
        //MeasureSettingsFFM(out msg, new Param("eSpan", ESpan.IFPAN_FREQ_RANGE_1000.ToString()));
        ScanRangeAdd(begin, end, span, dfStep);
        var paras = new List<Param>
        {
            new("iFreqBegin", begin.ToString()),
            new("iFreqEnd", end.ToString())
        };
        var pol = _dfPolarization == Polarization.Horizontal ? EAntPol.PolHorizontal : EAntPol.PolVertical;
        paras.Add(new Param("eAntPol", pol.ToString()));
        paras.Add(new Param("eSpan", span.ToString()));
        paras.Add(new Param("eDfPanStep", dfStep.ToString()));
        paras.Add(new Param("eAvgMode", ConvertDFindMode(_dfindMode).ToString()));
        paras.Add(new Param("eBlockAveragingSelect", nameof(EBlockAveragingSelect.BlockAveragingSelectTime)));
        paras.Add(new Param("iBlockAveragingTime", _integralTime.ToString()));
        // paras.Add(new Param("iThreshold", _levelThreshold.ToString()));
        paras.Add(new Param("iThreshold", "-30"));
        var eState = _amplifier ? EState.StateOn : EState.StateOff;
        paras.Add(new Param("eAntPreAmp", eState.ToString()));
        paras.Add(new Param("eDfAlt", nameof(EDfAlt.DfaltCorrelation)));
        paras.Add(new Param("eWindowType", nameof(EWindowType.DfWindowTypeBlackman)));
        paras.Add(new Param("eDfPanSelectivity", nameof(EDfPanSelectivity.DfpanSelectivityAuto)));
        paras.Add(new Param("iHopDwellTime", "0"));
        paras.Add(new Param("iBlockAveragingCycles", "200"));
        paras.Add(new Param("iTsTimeMeas", "-1"));
        paras.Add(new Param("iTsTimeFreqChange", "-1"));
        paras.Add(new Param("iTsTimeScanRange", "-1"));
        if (_attCtrlType)
        {
            paras.Add(new Param("eAttSelect", nameof(EAttSelect.AttAuto)));
        }
        else
        {
            paras.Add(new Param("eAttSelect", nameof(EAttSelect.AttManual)));
            paras.Add(new Param("iAttValue", _attenuation.ToString()));
        }

        paras.Add(new Param("iAttHoldTime", "10")); // TODO: ?是否需要修改为10？
        ScanRange(_scanRangeId, paras.ToArray());
        //设置天线
        AntennaControl(EAntCtrlMode.AntCtrlModeAuto, null, null);
        CheckAllSwitch();
    }

    #endregion 扫描测向

    #region 数据开关

    /// <summary>
    ///     禁用所有数据的返回
    /// </summary>
    private void DisableAllSwitch()
    {
        //_dataQueue.Clear();
        TraceDisable(ETraceTag.TracetagSelCall, _localIp, _localPort, out var msg);
        if (!string.IsNullOrWhiteSpace(msg)) Trace.WriteLine($"禁用TRACETAG_SEL_CALL数据消息：{msg}");
        CheckIqSwitch(false);
        CheckSpectrumSwitch(false);
        CheckCwSwitch(false);
        CheckAudioSwitch(false);
        CheckDfSwitch(false);
        CheckPScanSwitch(false);
        // 所有测量停止的时候都需要将模式改为Rx
        // 下发参数的时候也需要改为Rx下发参数
        SetModeRx();
    }

    /// <summary>
    ///     检查开关状态,统一打开开关
    ///     DDF550修改参数的时候需要先关闭所有的订阅数据,然后重新订阅-----在现场通过实际设备的测试发现,修改参数的时候不需要关闭数据,可以直接修改参数
    /// </summary>
    private void CheckAllSwitch()
    {
        // 开启任务的时候根据功能不同切换不同的DfMode
        if (CurFeature == FeatureType.FFM)
            SetModeRx();
        else if (CurFeature is FeatureType.FFDF or FeatureType.WBDF)
            SetModeFfm();
        else if (CurFeature == FeatureType.ScanDF)
            SetModeScan();
        else if (CurFeature == FeatureType.SCAN) SetModeRxpscan();
        if (_iqSwitch) CheckIqSwitch(true);
        // 单频测量下CW数据永远返回(里面包含电平数据)
        if (CurFeature == FeatureType.FFM) CheckCwSwitch(true);
        if (_spectrumSwitch && CurFeature == FeatureType.FFM) CheckSpectrumSwitch(true);
        if (_audioSwitch) CheckAudioSwitch(true);
        // 测向功能返回的数据体为DFPScan,都需要开启DF数据
        if (CurFeature is FeatureType.FFDF or FeatureType.WBDF or FeatureType.ScanDF)
        {
            CheckCwSwitch(true);
            CheckSpectrumSwitch(true);
            CheckDfSwitch(true);
        }

        if (CurFeature == FeatureType.SCAN) CheckPScanSwitch(true);
        if (CurFeature is FeatureType.FFM or FeatureType.SCAN or FeatureType.FFDF) Initiate();
    }

    #endregion 数据开关

    #region 数据转换 将参数转换为DDF550的Xml协议可以识别的枚举

    /// <summary>
    ///     检波方式转换
    /// </summary>
    /// <param name="mode"></param>
    private static ELevelIndicatir ConvertDetect(DetectMode mode)
    {
        var cmd = ELevelIndicatir.LevelIndicatorFast;
        switch (mode)
        {
            case DetectMode.Fast:
                cmd = ELevelIndicatir.LevelIndicatorFast;
                break;
            case DetectMode.Pos:
                cmd = ELevelIndicatir.LevelIndicatorPeak;
                break;
            case DetectMode.Avg:
                cmd = ELevelIndicatir.LevelIndicatorAvg;
                break;
            case DetectMode.Rms:
                cmd = ELevelIndicatir.LevelIndicatorRms;
                break;
        }

        return cmd;
    }

    /// <summary>
    ///     带宽测量方式转换
    /// </summary>
    /// <param name="mode"></param>
    private static EMeasureMode ConvertMeasureMode(string mode)
    {
        if (mode == "XDB") return EMeasureMode.MeasuremodeXdb;
        return EMeasureMode.MeasuremodeBeta;
    }

    /// <summary>
    ///     测向模式转换
    /// </summary>
    /// <param name="mode"></param>
    private static EAverageMode ConvertDFindMode(DFindMode mode)
    {
        return mode switch
        {
            DFindMode.Feebleness => EAverageMode.DfsquOff,
            DFindMode.Gate => EAverageMode.DfsquGate,
            _ => EAverageMode.DfsquNorm
        };
    }

    /// <summary>
    ///     测向取值方式转换
    /// </summary>
    /// <param name="mode"></param>
    private static EBlockAveragingSelect ConvertDfSelectMode(string mode)
    {
        if (mode == "TIME") return EBlockAveragingSelect.BlockAveragingSelectTime;
        return EBlockAveragingSelect.BlockAveragingSelectCycles;
    }

    /// <summary>
    ///     FFT模式转换
    /// </summary>
    /// <param name="mode"></param>
    private static EifPanMode ConvertIfPanMode(string mode)
    {
        return mode switch
        {
            "MIN" => EifPanMode.IfpanModeMinhold,
            "MAX" => EifPanMode.IfpanModeMaxhold,
            "SCALar" => EifPanMode.IfpanModeAverage,
            "OFF" => EifPanMode.IfpanModeClrwrite,
            _ => EifPanMode.IfpanModeClrwrite
        };
    }

    #endregion 数据转换 将参数转换为DDF550的Xml协议可以识别的枚举
}