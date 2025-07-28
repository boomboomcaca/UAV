using System.Collections.Generic;
using Magneto.Protocol.Define;

namespace Magneto.Device.DDF5GTS;

public partial class Ddf5Gts
{
    #region 单频测量

    /// <summary>
    ///     设置DFMode为Rx模式
    /// </summary>
    /// <returns></returns>
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
        AntennaControl(EAntCtrlMode.AntCtrlModeAuto, ErfInput.RfInputVushf2, EhfInput.HfInputHf2);
        AntennaControl(EAntCtrlMode.AntCtrlModeAuto, ErfInput.RfInputVushf1, EhfInput.HfInputHf1);
        SelCall(false);
        ReferenceMode(EReferenceMode.ReferenceModeInternal);
        // 增益定时特性
        SendCommand(CmdDemodulationsettings, "eLevelIndicator", ELevelIndicatir.LevelIndicatorAvg.ToString());
        SendCommand(CmdDemodulationsettings, "eGainTiming", EGainTiming.GcDefault.ToString());
        // 设置差拍频率-是否提出来成为参数
        SendCommand(CmdDemodulationsettings, "iBfoFrequency", "1000");
        // 设置自动频率控制或手动频率控制-是否提出来成为参数 AFC (automatic frequency control) or MFC (manual f. c.) in use (true: AFC, false: MFC). 
        SendCommand(CmdDemodulationsettings, "bAfc", "false");
        // 设置音频参数
        SetAudioMode(EAudioMode.AudioMode32Khz16BitMono);
        // IQ设置为16位
        SendCommand(CmdIfmode, "eIfMode", EifMode.If16Bit.ToString());
        // 天线前置放大器关闭
        // 单频测量的保持时间需要设置为0
        // 设置中频模式
        MeasureSettingsFfm(new Param("iAttHoldTime", "0"),
            new Param("eIFPanSelectivity", EifPanSelectivity.IfpanSelectivityAuto.ToString()));
        // 设置频谱参数
        SetSpectrumSpan();
        CheckAllSwitch();
    }

    /// <summary>
    ///     设置频谱带宽
    /// </summary>
    private void SetSpectrumSpan()
    {
        var span = (int)(uint)_ifBandwidth;
        var low = 0 - span / 2;
        var high = span / 2;
        SetItu(new Param("bUseAutoBandwidthLimits", "true"),
            new Param("iLowerBandwidthLimit", low.ToString()), new Param("iUpperBandwidthLimit", high.ToString()));
        // 同时设置频谱带宽和中频带宽
        if (CurFeature == FeatureType.FFM)
        {
            _ifbw = GetDefaultIfBandWidth();
            var keyValuePairs = new Dictionary<string, string>
            {
                { "eSpan", ((ESpan)(uint)_ifBandwidth).ToString() },
                { "eIFPanStep", ((EifPanStep)(long)(_ifbw * 1000 * 100)).ToString() }
            };
            SendCommand(CmdMeasuresettingsffm, keyValuePairs);
        }
        else if (CurFeature is FeatureType.FDF or FeatureType.SSE)
        {
            _ifbw = GetDefaultIfBandWidth();
            var keyValuePairs = new Dictionary<string, string>
            {
                { "eSpan", ((ESpan)(uint)_ifBandwidth).ToString() },
                { "eIFPanStep", ((EifPanStep)(long)(_ifbw * 1000 * 100)).ToString() },
                { "eDfPanStep", ((EDfPanStep)(long)(_dfBandWidth * 1000 * 100)).ToString() }
            };
            SendCommand(CmdMeasuresettingsffm, keyValuePairs);
        }
        else
        {
            SendCommand(CmdMeasuresettingsffm, "eSpan", ((ESpan)(uint)_ifBandwidth).ToString());
        }
    }

    #endregion 单频测量

    #region 单频测向/宽带测向/超分辨率测向

    private bool SetModeFfm()
    {
        return DfMode(EDfMode.DfmodeFfm);
    }

    private void SetParamsDf()
    {
        //设置天线
        AntennaControl(EAntCtrlMode.AntCtrlModeAuto, ErfInput.RfInputVushf2, EhfInput.HfInputHf2);
        //AntennaControl(EAnt_Ctrl_Mode.ANT_CTRL_MODE_AUTO, ERF_Input.RF_INPUT_VUSHF1, EHF_Input.HF_INPUT_HF1, out msg);
        SelCall(false);
        ReferenceMode(EReferenceMode.ReferenceModeInternal);
        SendCommand(CmdItu, "bUseAutoBandwidthLimits", "true");
        SendCommand(CmdMeasuresettingsffm, "eWindowType", EWindowType.DfWindowTypeBlackman.ToString());
        SendCommand(CmdMeasuresettingsffm, "eDfPanSelectivity", EDfPanSelectivity.DfpanSelectivityAuto.ToString());
        //// 天线前置放大器打开或关闭
        //SendCommand(CMD_MEASURESETTINGSFFM, "eAntPreAmp", EState.STATE_OFF.ToString());
        SendCommand(CmdMeasuresettingsffm, "eIFPanMode", EifPanMode.IfpanModeClrwrite.ToString());
        SendCommand(CmdRxsettings, "iMeasureTime", "0");
        // 设置音频参数
        SetAudioMode(EAudioMode.AudioMode32Khz16BitMono);
        if (CurFeature == FeatureType.WBDF)
        {
            var span = (int)((uint)_ifBandwidth * 1000);
            var low = 0 - span / 2;
            var high = span / 2;
            SetItu(new Param("bUseAutoBandwidthLimits", "true"),
                new Param("iLowerBandwidthLimit", low.ToString()), new Param("iUpperBandwidthLimit", high.ToString()));
            MeasureSettingsFfm(
                new Param("eDfPanStep", ((EDfPanStep)(long)(ResolutionBandwidth * 1000 * 100)).ToString()),
                new Param("eSpan", ((ESpan)(uint)_ifBandwidth).ToString()));
            SendCommand(CmdMeasuresettingsffm, "eDfAlt", EDfAlt.DfaltCorrelation.ToString());
        }
        else if (CurFeature == FeatureType.FDF)
        {
            SetDfBandWidthAndSpectrumSpan();
            SendCommand(CmdMeasuresettingsffm, "eDfAlt", EDfAlt.DfaltCorrelation.ToString());
        }
        else if (CurFeature == FeatureType.SSE)
        {
            SetDfBandWidthAndSpectrumSpan();
            SendCommand(CmdMeasuresettingsffm, "eDfAlt", EDfAlt.DfaltSuperresolution.ToString());
        }

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
        var keyValuePairs = new Dictionary<string, string>
        {
            { "eDfPanStep", ((EDfPanStep)(long)(_dfBandWidth * 1000 * 100)).ToString() },
            { "eSpan", ((ESpan)(uint)_ifBandwidth).ToString() },
            { "eIFPanStep", ((EifPanStep)(long)(_ifbw * 1000 * 100)).ToString() }
        };
        SendCommand(CmdMeasuresettingsffm, keyValuePairs);
    }

    #endregion 单频测向/宽带测向

    #region 全景扫描PScan

    /// <summary>
    ///     设置全景扫描模式
    /// </summary>
    /// <returns></returns>
    internal bool SetModeRxpscan()
    {
        return DfMode(EDfMode.DfmodeRxpscan);
    }

    /// <summary>
    ///     设置全景扫描参数
    /// </summary>
    internal void SetRxpscanParams()
    {
        Dictionary<string, string> keyValuePairs = new()
        {
            { "iFreqBegin", ((long)StartFrequency * 1000000).ToString() },
            { "iFreqEnd", ((long)StopFrequency * 1000000).ToString() },
            { "eStep", ((EpScanStep)(int)(StepFrequency * 1000)).ToString() }
        };
        SendCommand("MeasureSettingsPScan", keyValuePairs);
        CheckAllSwitch();
    }

    #endregion 全景扫描PScan

    #region 扫描测向

    /// <summary>
    ///     设置扫描测向模式
    /// </summary>
    /// <returns></returns>
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
        ReferenceMode(EReferenceMode.ReferenceModeInternal);
        RxSettings(0, EMeasureModeCp.MeasuremodecpCont);
        var begin = (long)(StartFrequency * 1000000);
        var end = (long)(StopFrequency * 1000000);
        _ifBandwidth = GetMaxSpecturmSpan(StepFrequency);
        var span = (ESpan)(uint)_ifBandwidth;
        var dfStep = (EDfPanStep)(long)(StepFrequency * 1000 * 100);
        //MeasureSettingsFFM(out msg, new Param("eSpan", ESpan.IFPAN_FREQ_RANGE_1000.ToString()));
        ScanRangeAdd(begin, end, span, dfStep);
        ScanRange(_scanRangeId, new Param("eWindowType", EWindowType.DfWindowTypeBlackman.ToString()),
            new Param("iThreshold", _levelThreshold.ToString()),
            new Param("eAvgMode", ConvertDFindMode(_dfindMode).ToString()));
        if (_attenuation == -1)
            ScanRange(_scanRangeId, new Param("eAttSelect", EAttSelect.AttAuto.ToString()));
        else
            ScanRange(_scanRangeId, new Param("eAttSelect", EAttSelect.AttManual.ToString()),
                new Param("iAttValue", _attenuation.ToString()));
        ScanRange(_scanRangeId,
            new Param("eBlockAveragingSelect", EBlockAveragingSelect.BlockAveragingSelectTime.ToString()),
            new Param("iBlockAveragingTime", _integralTime.ToString()),
            new Param("eDfPanSelectivity", EDfPanSelectivity.DfpanSelectivityAuto.ToString()));
        //设置天线
        AntennaControl(EAntCtrlMode.AntCtrlModeAuto, ErfInput.RfInputVushf2, EhfInput.HfInputHf2);
        AntennaControl(EAntCtrlMode.AntCtrlModeAuto, ErfInput.RfInputVushf1, EhfInput.HfInputHf1);
        CheckAllSwitch();
    }

    #endregion 扫描测向
}