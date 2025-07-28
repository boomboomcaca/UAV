using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Magneto.Contract;

namespace Magneto.Device.DDF550;

public partial class Ddf550
{
    /// <summary>
    ///     向设备发送数据并接收返回的数据
    /// </summary>
    /// <param name="request"></param>
    private Reply SetAnalyzeData(Request request)
    {
        var buffer = Utils.SerializeToXml(request);
        Encoding.ASCII.GetString(buffer);
        // Console.WriteLine("发送:" + log);
        var cmd = PackedXmlCommand(buffer);
        var data = SendCmd(cmd);
        try
        {
            if (data == null || data.Length == 0)
            {
                var rep = new Reply
                {
                    Command =
                    {
                        RtnCode = "-1",
                        RtnMessage = "数据接收错误"
                    }
                };
                return rep;
            }

            Encoding.ASCII.GetString(data);
            var reply = (Reply)Utils.DeserializeFromXml<Reply>(data);
            if (reply.Command != null)
            {
                if (reply.Command.Name.Equals(IncorrectCommand))
                {
                    reply.Command.RtnCode = "-1";
                    reply.Command.RtnMessage = "命令错误";
                    return reply;
                }

                return reply;
            }

            if (reply.Command != null)
            {
                reply.Command.RtnCode = "-1";
                reply.Command.RtnMessage = "数据解析失败";
            }

            return reply;
        }
        catch (Exception ex)
        {
            Reply reply = new()
            {
                Command =
                {
                    RtnCode = "-1",
                    RtnMessage = "数据解析失败/" + ex.Message
                }
            };
            return reply;
        }
    }

    /// <summary>
    ///     DF模式修改
    /// </summary>
    /// <param name="eOperationMode"></param>
    private bool DfMode(EDfMode eOperationMode)
    {
        var request = new Request(OpreationMode.Set);
        request.Excute(CmdDfmode, nameof(eOperationMode), eOperationMode.ToString());
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode)) return false;

        return true;
    }

    /// <summary>
    ///     通用设置方法
    /// </summary>
    /// <param name="commandName"></param>
    /// <param name="paramName"></param>
    /// <param name="paramValue"></param>
    private void SendCommand(string commandName, string paramName, string paramValue)
    {
        var request = new Request(OpreationMode.Set);
        request.Excute(commandName, paramName, paramValue);
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode))
        {
            var message = reply.Command.RtnMessage;
            Trace.WriteLine($"命令{commandName}下发失败{message}");
            //throw new Exception(message);
        }
    }

    /// <summary>
    ///     通用查询方法
    /// </summary>
    /// <param name="commandName"></param>
    /// <param name="paramName"></param>
    /// <param name="paramValue"></param>
    private Reply QueryCommand(string commandName, string paramName = null, string paramValue = null)
    {
        var request = new Request(OpreationMode.Get);
        request.Excute(commandName, paramName, paramValue);
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode))
        {
            var message = reply.Command.RtnMessage;
            var err = $"命令{commandName}下发失败{message}";
            Console.WriteLine(err);
            //throw new Exception(message);
        }

        return reply;
    }

    /// <summary>
    ///     设置多个参数
    /// </summary>
    /// <param name="commandName"></param>
    /// <param name="keyValuePairs"></param>
    private bool SendCommand(string commandName, Dictionary<string, string> keyValuePairs)
    {
        var request = new Request(OpreationMode.Set);
        foreach (var keyValuePair in keyValuePairs) request.Excute(commandName, keyValuePair.Key, keyValuePair.Value);
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode))
        {
            var message = reply.Command.RtnMessage;
            Trace.WriteLine($"命令{commandName}下发失败{message}");
            return false;
        }

        return true;
    }

    #region Command定义

    /// <summary>
    ///     未知的Command
    /// </summary>
    private const string IncorrectCommand = "CouldNotParseCommandError";

    /// <summary>
    ///     通用设置
    ///     设置DDF550工作模式;不执行任务的时候工作模式需要切回Rx模式
    /// </summary>
    private const string CmdDfmode = "DfMode";

    /// <summary>
    ///     设置FFM参数,单频测量与单频测向/宽带测向需要设置这个
    /// </summary>
    private const string CmdMeasuresettingsffm = "MeasureSettingsFFM";

    /// <summary>
    ///     ITU参数设置,其中包含开启ITU数据开关
    /// </summary>
    private const string CmdItu = "ITU";

    /// <summary>
    ///     选乎功能,暂时关闭
    /// </summary>
    private const string CmdSelcall = "SelCall";

    /// <summary>
    ///     音频解调设置
    /// </summary>
    private const string CmdDemodulationsettings = "DemodulationSettings";

    /// <summary>
    ///     音频格式设置
    /// </summary>
    private const string CmdAudiomode = "AudioMode";

    private const string CmdReferencemode = "ReferenceMode";

    /// <summary>
    ///     射频模式
    /// </summary>
    private const string CmdRfmode = "RfMode";

    /// <summary>
    ///     设置IQ数据的格式(Tracker800设置为IF_16BIT)
    /// </summary>
    private const string CmdIfmode = "IfMode";

    /// <summary>
    ///     订阅数据
    /// </summary>
    private const string CmdTraceenable = "TraceEnable";

    /// <summary>
    ///     取消订阅
    /// </summary>
    private const string CmdTracedisable = "TraceDisable";

    /// <summary>
    ///     删除数据通道
    /// </summary>
    private const string CmdTracedelete = "TraceDelete";

    /// <summary>
    ///     清理不活动的数据通道
    /// </summary>
    private const string CmdTracedeleteinactive = "TraceDeleteInactive";

    /// <summary>
    ///     TraceFlag设置
    ///     禁用数据的发送
    ///     (与TraceDisable区分开来,上面那个是启用/停用IQ数据频谱数据等数据的发送,这个是启用/停用IQ数据频谱数据等数据包中的子项数据的发送)
    /// </summary>
    private const string CmdTraceflagdisable = "TraceFlagDisable";

    /// <summary>
    ///     启用数据发送
    /// </summary>
    private const string CmdTraceflagenable = "TraceFlagEnable";

    /// <summary>
    ///     Rx或RxPScan模式下的设置
    ///     Rx或RxPScan模式下开启测量
    /// </summary>
    private const string CmdInitiate = "Initiate";

    /// <summary>
    ///     Rx或RxPScan模式下停止测量(Argus抓包中这个方法并没有使用,不知为何)
    /// </summary>
    private const string CmdAbort = "Abort";

    /// <summary>
    ///     设置PScan全景扫描
    /// </summary>
    private const string CmdMeasuresettingspscan = "MeasureSettingsPScan";

    /// <summary>
    ///     设置测量模式与测量时间
    /// </summary>
    private const string CmdRxsettings = "RxSettings";

    /// <summary>
    ///     天线设置
    /// </summary>
    private const string CmdAntennacontrol = "AntennaControl";

    private const string CmdAntennaproperties = "AntennaProperties";
    private const string CmdAntennasetup = "AntennaSetup";
    private const string CmdAntennaused = "AntennaUsed";
    private const string CmdAntennadelete = "AntennaDelete";
    private const string CmdHwrefresh = "HwRefresh";

    private const string CmdHwinfo = "HwInfo";

    //扫描测向设置
    private const string CmdScanrange = "ScanRange";
    private const string CmdScanrangeadd = "ScanRangeAdd";
    private const string CmdScanrangedelete = "ScanRangeDelete";
    private const string CmdScanrangedeleteall = "ScanRangeDeleteAll";
    private const string CmdScanrangenext = "ScanRangeNext";

    #endregion Command定义

    #region FFM测量

    /// <summary>
    ///     MeasureSettingsFFM
    /// </summary>
    /// <param name="iFrequency"></param>
    /// <param name="eAvgMode"></param>
    /// <param name="eDfPanStep"></param>
    /// <param name="eBlockAveragingSelect"></param>
    /// <param name="iBlockAveragingCycles"></param>
    /// <param name="iBlockAveragingTime"></param>
    /// <param name="iThreshold"></param>
    /// <param name="eAntPol"></param>
    /// <param name="eAntPreAmp"></param>
    /// <param name="eDfAlt"></param>
    /// <param name="eSpan"></param>
    /// <param name="eWindowType"></param>
    /// <param name="eDfPanSelectivity"></param>
    /// <param name="eAttSelect"></param>
    /// <param name="iAttValue"></param>
    /// <param name="iAttHoldTime"></param>
    /// <param name="eIfPanStep"></param>
    /// <param name="eIfPanSelectivity"></param>
    /// <param name="eIfPanMode"></param>
    /// <param name="message"></param>
    private bool MeasureSettingsFfm(long iFrequency,
        EAverageMode eAvgMode,
        EDfPanStep eDfPanStep,
        EBlockAveragingSelect eBlockAveragingSelect,
        int iBlockAveragingCycles,
        int iBlockAveragingTime,
        int iThreshold,
        EAntPol eAntPol,
        EState eAntPreAmp,
        EDfAlt eDfAlt,
        ESpan eSpan,
        EWindowType eWindowType,
        EDfPanSelectivity eDfPanSelectivity,
        EAttSelect eAttSelect,
        int iAttValue,
        int iAttHoldTime,
        EifPanStep eIfPanStep,
        EifPanSelectivity eIfPanSelectivity,
        EifPanMode eIfPanMode,
        out string message)
    {
        Request request = new(OpreationMode.Set);
        request.Excute(CmdMeasuresettingsffm, nameof(iFrequency), iFrequency.ToString());
        request.Excute(CmdMeasuresettingsffm, nameof(eAvgMode), eAvgMode.ToString());
        request.Excute(CmdMeasuresettingsffm, nameof(eDfPanStep), eDfPanStep.ToString());
        request.Excute(CmdMeasuresettingsffm, nameof(eBlockAveragingSelect), eBlockAveragingSelect.ToString());
        request.Excute(CmdMeasuresettingsffm, nameof(iBlockAveragingCycles), iBlockAveragingCycles.ToString());
        request.Excute(CmdMeasuresettingsffm, nameof(iBlockAveragingTime), iBlockAveragingTime.ToString());
        request.Excute(CmdMeasuresettingsffm, nameof(iThreshold), iThreshold.ToString());
        request.Excute(CmdMeasuresettingsffm, nameof(eAntPol), eAntPol.ToString());
        request.Excute(CmdMeasuresettingsffm, nameof(eAntPreAmp), eAntPreAmp.ToString());
        request.Excute(CmdMeasuresettingsffm, nameof(eDfAlt), eDfAlt.ToString());
        request.Excute(CmdMeasuresettingsffm, nameof(eSpan), eSpan.ToString());
        request.Excute(CmdMeasuresettingsffm, nameof(eWindowType), eWindowType.ToString());
        request.Excute(CmdMeasuresettingsffm, nameof(eDfPanSelectivity), eDfPanSelectivity.ToString());
        request.Excute(CmdMeasuresettingsffm, nameof(eAttSelect), eAttSelect.ToString());
        request.Excute(CmdMeasuresettingsffm, nameof(iAttValue), iAttValue.ToString());
        request.Excute(CmdMeasuresettingsffm, nameof(iAttHoldTime), iAttHoldTime.ToString());
        request.Excute(CmdMeasuresettingsffm, nameof(eIfPanStep), eIfPanStep.ToString());
        request.Excute(CmdMeasuresettingsffm, nameof(eIfPanSelectivity), eIfPanSelectivity.ToString());
        request.Excute(CmdMeasuresettingsffm, nameof(eIfPanMode), eIfPanMode.ToString());
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode))
        {
            message = reply.Command.RtnMessage;
            return false;
        }

        message = "";
        return true;
    }

    /// <summary>
    ///     设置FFM参数
    /// </summary>
    /// <param name="paras"></param>
    private bool MeasureSettingsFfm(params Param[] paras)
    {
        var request = new Request(OpreationMode.Set);
        if (paras == null || paras.Length == 0) return false;

        foreach (var pairs in paras) request.Excute(CmdMeasuresettingsffm, pairs.Name, pairs.Value);
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode)) return false;

        return true;
    }

    #endregion FFM测量

    #region ITU

    /// <summary>
    ///     SelCall设置(一般不用,所以关闭)
    /// </summary>
    /// <param name="bDecoderEnabled"></param>
    private bool SelCall(bool bDecoderEnabled)
    {
        Request request = new(OpreationMode.Set);
        request.Excute(CmdSelcall, nameof(bDecoderEnabled), bDecoderEnabled.ToString());
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode)) return false;

        return true;
    }

    /// <summary>
    ///     ITU设置
    /// </summary>
    /// <param name="paras"></param>
    private bool SetItu(params Param[] paras)
    {
        Request request = new(OpreationMode.Set);
        if (paras == null || paras.Length == 0) return false;

        foreach (var pairs in paras) request.Excute(CmdItu, pairs.Name, pairs.Value);
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode)) return false;

        return true;
    }

    private void SetItuFlag()
    {
        ulong itu = 0x80000003;
        if (_ituSwitch) itu = 0x800007FF;
        foreach (ESelectorFlag flag in Enum.GetValues(typeof(ESelectorFlag)))
        {
            if ((ulong)flag < 0x02 || (ulong)flag > 0x400) continue;
            if ((itu & (ulong)flag) > 0)
                TraceFlagEnable(flag, Ip, Port);
            else
                TraceFlagDisable(flag, Ip, Port);
        }
    }

    #endregion ITU

    #region Rx模式

    /// <summary>
    ///     Rx或RxPScan模式下开启任务
    /// </summary>
    private bool Initiate()
    {
        var request = new Request(OpreationMode.Set)
        {
            Command =
            {
                Name = CmdInitiate
            }
        };
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode)) return false;

        return true;
    }

    /// <summary>
    ///     Rx或RxPScan模式下关闭任务(Argus抓包显示这个方法没有用过)
    /// </summary>
    /// <param name="message"></param>
    private bool Abort(out string message)
    {
        var request = new Request(OpreationMode.Set)
        {
            Command =
            {
                Name = CmdAbort
            }
        };
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode))
        {
            message = reply.Command.RtnMessage;
            return false;
        }

        message = "";
        return true;
    }

    /// <summary>
    ///     Rx或RxPScan模式下设置测量模式与测量时间
    /// </summary>
    /// <param name="iMeasureTime"></param>
    /// <param name="eMeasureMode"></param>
    private bool RxSettings(int iMeasureTime, EMeasureModeCp eMeasureMode)
    {
        var request = new Request(OpreationMode.Set);
        request.Excute(CmdRxsettings, nameof(iMeasureTime), iMeasureTime.ToString());
        request.Excute(CmdRxsettings, nameof(eMeasureMode), eMeasureMode.ToString());
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode)) return false;

        return true;
    }

    #endregion Rx模式

    #region 音频设置

    /// <summary>
    ///     DemodulationSettings
    /// </summary>
    /// <param name="eDemodulation"></param>
    /// <param name="iBfoFrequency"></param>
    /// <param name="iAfFrequency"></param>
    /// <param name="eAfBandwidth"></param>
    /// <param name="iAfThreshold"></param>
    /// <param name="bUseAfThreshold"></param>
    /// <param name="iPassbandFrequency"></param>
    /// <param name="eLevelIndicator"></param>
    /// <param name="bAfc"></param>
    /// <param name="eGainSelect"></param>
    /// <param name="iGainValue"></param>
    /// <param name="eGainTiming"></param>
    /// <param name="bStereoDecoder"></param>
    /// <param name="message"></param>
    private bool DemodulationSettings(EDemodulation eDemodulation,
        long iBfoFrequency,
        long iAfFrequency,
        EAfBandWidth eAfBandwidth,
        int iAfThreshold,
        bool bUseAfThreshold,
        long iPassbandFrequency,
        ELevelIndicatir eLevelIndicator,
        bool bAfc,
        EGainControl eGainSelect,
        int iGainValue,
        EGainTiming eGainTiming,
        bool bStereoDecoder,
        out string message)
    {
        Request request = new(OpreationMode.Set);
        request.Excute(CmdDemodulationsettings, nameof(eDemodulation), eDemodulation.ToString());
        request.Excute(CmdDemodulationsettings, nameof(iBfoFrequency), iBfoFrequency.ToString());
        request.Excute(CmdDemodulationsettings, nameof(iAfFrequency), iAfFrequency.ToString());
        request.Excute(CmdDemodulationsettings, nameof(eAfBandwidth), eAfBandwidth.ToString());
        request.Excute(CmdDemodulationsettings, nameof(iAfThreshold), iAfThreshold.ToString());
        request.Excute(CmdDemodulationsettings, nameof(bUseAfThreshold), bUseAfThreshold.ToString());
        request.Excute(CmdDemodulationsettings, nameof(iPassbandFrequency), iPassbandFrequency.ToString());
        request.Excute(CmdDemodulationsettings, nameof(eLevelIndicator), eLevelIndicator.ToString());
        request.Excute(CmdDemodulationsettings, nameof(bAfc), bAfc.ToString());
        request.Excute(CmdDemodulationsettings, nameof(eGainSelect), eGainSelect.ToString());
        request.Excute(CmdDemodulationsettings, nameof(iGainValue), iGainValue.ToString());
        request.Excute(CmdDemodulationsettings, nameof(eGainTiming), eGainTiming.ToString());
        request.Excute(CmdDemodulationsettings, nameof(bStereoDecoder), bStereoDecoder.ToString());
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode))
        {
            message = reply.Command.RtnMessage;
            return false;
        }

        message = "";
        return true;
    }

    /// <summary>
    ///     音频解调设置
    /// </summary>
    /// <param name="message"></param>
    /// <param name="paras"></param>
    private bool DemodulationSettings(out string message, params Param[] paras)
    {
        message = "";
        Request request = new(OpreationMode.Set);
        if (paras == null || paras.Length == 0)
        {
            message = "参数不能为空";
            return false;
        }

        foreach (var pairs in paras) request.Excute(CmdDemodulationsettings, pairs.Name, pairs.Value);
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode))
        {
            message = reply.Command.RtnMessage;
            return false;
        }

        message = "";
        return true;
    }

    /// <summary>
    ///     设置音频格式
    /// </summary>
    /// <param name="eAudioMode"></param>
    private bool SetAudioMode(EAudioMode eAudioMode)
    {
        Request request = new(OpreationMode.Set);
        request.Excute(CmdAudiomode, nameof(eAudioMode), eAudioMode.ToString());
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode)) return false;

        return true;
    }

    #endregion 音频设置

    #region TraceTags开关Set

    /// <summary>
    ///     删除不活动的连接
    /// </summary>
    /// <param name="message"></param>
    private bool TraceDeleteInactive(out string message)
    {
        var request = new Request(OpreationMode.Set)
        {
            Type = "set",
            Command =
            {
                Name = CmdTracedeleteinactive
            }
        };
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode))
        {
            message = reply.Command.RtnMessage;
            return false;
        }

        message = "";
        return true;
    }

    /// <summary>
    ///     删除跟踪的连接
    /// </summary>
    /// <param name="zIp"></param>
    /// <param name="iPort"></param>
    private bool TraceDelete(string zIp, int iPort)
    {
        Request request = new(OpreationMode.Set);
        request.Excute(CmdTracedelete, nameof(zIp), zIp);
        request.Excute(CmdTracedelete, nameof(iPort), iPort.ToString());
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode)) return false;

        return true;
    }

    /// <summary>
    ///     启用某个数据的传输
    /// </summary>
    /// <param name="eTraceTag"></param>
    /// <param name="zIp"></param>
    /// <param name="iPort"></param>
    private bool TraceEnable(ETraceTag eTraceTag, string zIp, int iPort)
    {
        Request request = new(OpreationMode.Set);
        request.Excute(CmdTraceenable, nameof(eTraceTag), eTraceTag.ToString());
        request.Excute(CmdTraceenable, nameof(zIp), zIp);
        request.Excute(CmdTraceenable, nameof(iPort), iPort.ToString());
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode)) return false;

        return true;
    }

    /// <summary>
    ///     禁用某个数据的传输
    /// </summary>
    /// <param name="eTraceTag"></param>
    /// <param name="zIp"></param>
    /// <param name="iPort"></param>
    /// <param name="message"></param>
    private bool TraceDisable(ETraceTag eTraceTag, string zIp, int iPort, out string message)
    {
        var request = new Request(OpreationMode.Set);
        request.Excute(CmdTracedisable, nameof(eTraceTag), eTraceTag.ToString());
        request.Excute(CmdTracedisable, nameof(zIp), zIp);
        request.Excute(CmdTracedisable, nameof(iPort), iPort.ToString());
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode))
        {
            message = reply.Command.RtnMessage;
            return false;
        }

        message = "";
        return true;
    }

    /// <summary>
    ///     禁用某个传输数据下面的数据项
    /// </summary>
    /// <param name="eSelectorFlag"></param>
    /// <param name="zIp"></param>
    /// <param name="iPort"></param>
    private bool TraceFlagDisable(ESelectorFlag eSelectorFlag, string zIp, int iPort)
    {
        Request request = new(OpreationMode.Set);
        request.Excute(CmdTraceflagdisable, nameof(eSelectorFlag), eSelectorFlag.ToString());
        request.Excute(CmdTraceflagdisable, nameof(zIp), zIp);
        request.Excute(CmdTraceflagdisable, nameof(iPort), iPort.ToString());
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode)) return false;

        return true;
    }

    /// <summary>
    ///     启用某个传输数据下面的数据项
    /// </summary>
    /// <param name="eSelectorFlag"></param>
    /// <param name="zIp"></param>
    /// <param name="iPort"></param>
    private bool TraceFlagEnable(ESelectorFlag eSelectorFlag, string zIp, int iPort)
    {
        var request = new Request(OpreationMode.Set);
        request.Excute(CmdTraceflagenable, nameof(eSelectorFlag), eSelectorFlag.ToString());
        request.Excute(CmdTraceflagenable, nameof(zIp), zIp);
        request.Excute(CmdTraceflagenable, nameof(iPort), iPort.ToString());
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode)) return false;

        return true;
    }

    #endregion TraceTags开关Set

    #region 天线设置

    private bool AntennaControl(EAntCtrlMode eAntennaControlMode, ErfInput? eRfInput, EhfInput? eHfInput)
    {
        Request request = new(OpreationMode.Set);
        request.Excute(CmdAntennacontrol, nameof(eAntennaControlMode), eAntennaControlMode.ToString());
        if (eRfInput != null) request.Excute(CmdAntennacontrol, nameof(eRfInput), eRfInput.ToString());
        if (eHfInput != null) request.Excute(CmdAntennacontrol, nameof(eHfInput), eHfInput.ToString());
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode)) return false;

        return true;
    }

    /// <summary>
    ///     天线设置
    /// </summary>
    /// <param name="zAntennaName"></param>
    /// <param name="message"></param>
    /// <param name="paras"></param>
    private bool AntennaSetup(string zAntennaName, out string message, params Param[] paras)
    {
        message = "";
        Request request = new(OpreationMode.Set);
        request.Excute(CmdAntennasetup, nameof(zAntennaName), zAntennaName);
        if (paras == null || paras.Length == 0)
        {
            message = "参数不能为空";
            return false;
        }

        foreach (var pairs in paras) request.Excute(CmdAntennasetup, pairs.Name, pairs.Value);
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode))
        {
            message = reply.Command.RtnMessage;
            return false;
        }

        message = "";
        return true;
    }

    private bool AntennaSetup(AntennaInfo info)
    {
        Request request = new(OpreationMode.Set);
        request.Excute(CmdAntennasetup, "zAntennaName", info.AntennaName);
        request.Excute(CmdAntennasetup, "iFreqBegin", info.FreqBegin.ToString());
        request.Excute(CmdAntennasetup, "iFreqEnd", info.FreqEnd.ToString());
        request.Excute(CmdAntennasetup, "zCompassName", info.CompassName);
        // 下面有部分参数不允许修改
        //request.Excute(CMD_ANTENNASETUP, "iNorthCorrection", info.NorthCorrection.ToString());
        //request.Excute(CMD_ANTENNASETUP, "iRollCorrection", info.RollCorrection.ToString());
        //request.Excute(CMD_ANTENNASETUP, "iPitchCorrection", info.PitchCorrection.ToString());
        //request.Excute(CMD_ANTENNASETUP, "eRfInput", info.RfInput.ToString());
        //request.Excute(CMD_ANTENNASETUP, "eHfInput", info.HfInput.ToString());
        //request.Excute(CMD_ANTENNASETUP, "eRfRxPath", info.RfRxPath.ToString());
        //request.Excute(CMD_ANTENNASETUP, "eHfRxPath", info.HfRxPath.ToString());
        //request.Excute(CMD_ANTENNASETUP, "iCtrlPort", info.CtrlPort.ToString());
        //request.Excute(CMD_ANTENNASETUP, "bGpsRead", info.GpsRead.ToString().ToLower());
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode)) return false;

        return true;
    }

    private AntennaInfo GetAntennaSetup(string antennaName)
    {
        Request request = new(OpreationMode.Get);
        request.Excute(CmdAntennasetup, "zAntennaName", antennaName);
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode)) return default;

        var cmd = reply.Command;
        AntennaInfo info = new();
        var zAntennaName = cmd.Params.Find(i => i.Name == "zAntennaName");
        var iFreqBegin = cmd.Params.Find(i => i.Name == "iFreqBegin");
        var iFreqEnd = cmd.Params.Find(i => i.Name == "iFreqEnd");
        var zCompassName = cmd.Params.Find(i => i.Name == "zCompassName");
        var iNorthCorrection = cmd.Params.Find(i => i.Name == "iNorthCorrection");
        var iRollCorrection = cmd.Params.Find(i => i.Name == "iRollCorrection");
        var iPitchCorrection = cmd.Params.Find(i => i.Name == "iPitchCorrection");
        var eRfInput = cmd.Params.Find(i => i.Name == "eRfInput");
        var eHfInput = cmd.Params.Find(i => i.Name == "eHfInput");
        var eRfRxPath = cmd.Params.Find(i => i.Name == "eRfRxPath");
        var eHfRxPath = cmd.Params.Find(i => i.Name == "eHfRxPath");
        var iCtrlPort = cmd.Params.Find(i => i.Name == "iCtrlPort");
        var bGpsRead = cmd.Params.Find(i => i.Name == "bGpsRead");
        info.AntennaName = zAntennaName.Value;
        if (long.TryParse(iFreqBegin.Value, out var freqBegin)) info.FreqBegin = freqBegin;
        if (long.TryParse(iFreqEnd.Value, out var freqEnd)) info.FreqEnd = freqEnd;
        info.CompassName = zCompassName.Value;
        if (int.TryParse(iNorthCorrection.Value, out var north)) info.NorthCorrection = north;
        if (int.TryParse(iRollCorrection.Value, out var roll)) info.RollCorrection = roll;
        if (int.TryParse(iPitchCorrection.Value, out var pitch)) info.PitchCorrection = pitch;
        if (Enum.TryParse(eRfInput.Value, out ErfInput rfInput)) info.RfInput = rfInput;
        if (Enum.TryParse(eHfInput.Value, out EhfInput hfInput)) info.HfInput = hfInput;
        if (Enum.TryParse(eRfRxPath.Value, out ERxPath rfRxPath)) info.RfRxPath = rfRxPath;
        if (Enum.TryParse(eHfRxPath.Value, out ERxPath hfRxPath)) info.HfRxPath = hfRxPath;
        if (int.TryParse(iCtrlPort.Value, out var port)) info.CtrlPort = port;
        if (bool.TryParse(bGpsRead.Value, out var gps)) info.GpsRead = gps;
        return info;
    }

    private AntennaProperty GetAntennaProperties(string antennaName)
    {
        Request request = new(OpreationMode.Get);
        request.Excute(CmdAntennaproperties, "zAntennaName", antennaName);
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode)) return default;

        var cmd = reply.Command;
        AntennaProperty property = new();
        var zName = cmd.Params.Find(i => i.Name == "zAntennaName");
        var iCode = cmd.Params.Find(i => i.Name == "iAntCode");
        var iFreqBegin = cmd.Params.Find(i => i.Name == "iFreqBegin");
        var iFreqEnd = cmd.Params.Find(i => i.Name == "iFreqEnd");
        var bGpsAvailable = cmd.Params.Find(i => i.Name == "bGpsAvailable");
        var bTestElementAvailable = cmd.Params.Find(i => i.Name == "bTestElementAvailable");
        if (cmd.Array is { Name: "asAntRangeProp", Structs.Count: > 0 })
        {
            property.SAntRangeProp = new List<SAntRangeProp>();
            var structs = cmd.Array.Structs;
            foreach (var item in structs)
            {
                if (item.Name != "sAntRangeProp") continue;
                SAntRangeProp info = new();
                var bAntPreAmp = item.Params.Find(i => i.Name == "bAntPreAmp");
                var bAntElevation = item.Params.Find(i => i.Name == "bAntElevation");
                var iFreqRangeBegin = item.Params.Find(i => i.Name == "iFreqRangeBegin");
                var iFreqRangeEnd = item.Params.Find(i => i.Name == "iFreqRangeEnd");
                var eInputRange = item.Params.Find(i => i.Name == "eInputRange");
                var eDfAlt = item.Params.Find(i => i.Name == "eDfAlt");
                var eAntPol = item.Params.Find(i => i.Name == "eAntPol");
                if (bool.TryParse(bAntPreAmp.Value, out var antPreAmp)) info.AntPreAmp = antPreAmp;
                if (bool.TryParse(bAntElevation.Value, out var antEle)) info.AntElevation = antEle;
                if (long.TryParse(iFreqRangeBegin.Value, out var begin)) info.FreqRangeBegin = begin;
                if (long.TryParse(iFreqRangeEnd.Value, out var end)) info.FreqRangeEnd = end;
                if (Enum.TryParse(eInputRange.Value, out EInputRange range)) info.InputRange = range;
                if (Enum.TryParse(eDfAlt.Value, out EDfAlt alt)) info.DfAlt = alt;
                if (Enum.TryParse(eAntPol.Value, out EAntPol pol)) info.AntPol = pol;
                property.SAntRangeProp.Add(info);
            }
        }

        property.Name = zName.Value;
        if (int.TryParse(iCode.Value, out var code)) property.AntCode = code;
        if (long.TryParse(iFreqBegin.Value, out var freqBegin)) property.FreqBegin = freqBegin;
        if (long.TryParse(iFreqEnd.Value, out var freqEnd)) property.FreqEnd = freqEnd;
        if (bool.TryParse(bGpsAvailable.Value, out var gps)) property.GpsAvailable = gps;
        if (bool.TryParse(bTestElementAvailable.Value, out var test)) property.TestElementAvailable = test;
        return property;
    }

    /// <summary>
    ///     查询当前正在使用的天线
    /// </summary>
    private string AntennaUsed()
    {
        Request request = new(OpreationMode.Get);
        request.Excute(CmdAntennaused);
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode)) return string.Empty;

        var cmd = reply.Command;
        if (cmd.Params?.Count > 0)
        {
            var param = cmd.Params.Find(item => item.Name == "zAntennaName");
            if (param != null) return param.Value;
        }

        return string.Empty;
    }

    private void HwRefresh(out string message)
    {
        Request request = new(OpreationMode.Set);
        request.Excute(CmdHwrefresh);
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode))
        {
            message = reply.Command.RtnMessage;
            return;
        }

        message = "";
    }

    private List<ShwInfo> GetHwInfo()
    {
        Request request = new(OpreationMode.Get);
        request.Excute(CmdHwinfo);
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode)) return null;

        List<ShwInfo> list = new();
        var cmd = reply.Command;
        if (cmd.Array is { Name: "asHwInfo", Structs.Count: > 0 })
        {
            var structs = cmd.Array.Structs;
            foreach (var item in structs)
            {
                if (item.Name != "sHwInfo") continue;
                ShwInfo info = new();
                var hwType = item.Params.Find(i => i.Name == "eHwType");
                var hwStatus = item.Params.Find(i => i.Name == "eHwStatus");
                var name = item.Params.Find(i => i.Name == "zName");
                var code = item.Params.Find(i => i.Name == "iCode");
                var handle = item.Params.Find(i => i.Name == "iHandle");
                var port = item.Params.Find(i => i.Name == "iPort");
                var version = item.Structs.Find(i => i.Name == "sVersion");
                if (Enum.TryParse<EhwType>(hwType.Value, out var eType)) info.HwType = eType;
                if (Enum.TryParse(hwStatus.Value, out EhwStatus eStatus)) info.HwStatus = eStatus;
                info.Name = name.Value;
                if (int.TryParse(code.Value, out var iCode)) info.Code = iCode;
                if (int.TryParse(handle.Value, out var iHandle)) info.Handle = iHandle;
                if (int.TryParse(port.Value, out var iPort)) info.Port = iPort;
                if (version != null)
                {
                    info.Version = new SVersion();
                    var mainVersion = version.Params.Find(i => i.Name == "iMainVersion");
                    var subVersion = version.Params.Find(i => i.Name == "iSubVersion");
                    if (int.TryParse(mainVersion.Value, out var v1)) info.Version.MainVersion = v1;
                    if (int.TryParse(subVersion.Value, out var v2)) info.Version.SubVersion = v2;
                }

                list.Add(info);
            }
        }

        return list;
    }

    #endregion 天线设置

    #region 通用设置

    /// <summary>
    ///     设置参考模式
    /// </summary>
    /// <param name="eReferenceMode"></param>
    private bool ReferenceMode(EReferenceMode eReferenceMode)
    {
        Request request = new(OpreationMode.Set);
        request.Excute(CmdReferencemode, nameof(eReferenceMode), eReferenceMode.ToString());
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode)) return false;

        return true;
    }

    /// <summary>
    ///     设置射频模式
    /// </summary>
    /// <param name="eRfMode"></param>
    private bool SetRfMode(ERfMode eRfMode)
    {
        Request request = new(OpreationMode.Set);
        request.Excute(CmdRfmode, nameof(eRfMode), eRfMode.ToString());
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode)) return false;

        return true;
    }

    #endregion 通用设置

    #region 数据开关

    /// <summary>
    ///     开启或关闭IQ数据
    /// </summary>
    /// <param name="isOpened"></param>
    private void CheckIqSwitch(bool isOpened)
    {
        if (isOpened)
            TraceEnable(ETraceTag.TracetagIf, _localIqIp, _localIqPort);
        else
            TraceDisable(ETraceTag.TracetagIf, _localIqIp, _localIqPort, out _);
    }

    /// <summary>
    ///     开启或关闭CW数据
    /// </summary>
    /// <param name="isOpened"></param>
    private void CheckCwSwitch(bool isOpened)
    {
        if (isOpened)
            TraceEnable(ETraceTag.TracetagCwave, _localIp, _localPort);
        else
            TraceDisable(ETraceTag.TracetagCwave, _localIp, _localPort, out _);
    }

    /// <summary>
    ///     开启或关闭频谱数据
    /// </summary>
    /// <param name="isOpened"></param>
    private void CheckSpectrumSwitch(bool isOpened)
    {
        if (isOpened)
            TraceEnable(ETraceTag.TracetagIfpan, _localIp, _localPort);
        else
            TraceDisable(ETraceTag.TracetagIfpan, _localIp, _localPort, out _);
    }

    /// <summary>
    ///     开启或关闭音频数据
    /// </summary>
    /// <param name="isOpened"></param>
    private void CheckAudioSwitch(bool isOpened)
    {
        if (isOpened)
            TraceEnable(ETraceTag.TracetagAudio, _localIp, _localPort);
        else
            TraceDisable(ETraceTag.TracetagAudio, _localIp, _localPort, out _);
    }

    /// <summary>
    ///     开启或关闭测向数据
    /// </summary>
    /// <param name="isOpened"></param>
    private void CheckDfSwitch(bool isOpened)
    {
        if (isOpened)
            TraceEnable(ETraceTag.TracetagDf, _localIp, _localPort);
        else
            TraceDisable(ETraceTag.TracetagDf, _localIp, _localPort, out _);
    }

    /// <summary>
    ///     开启或关闭全景扫描数据
    /// </summary>
    /// <param name="isOpened"></param>
    private void CheckPScanSwitch(bool isOpened)
    {
        if (isOpened)
            TraceEnable(ETraceTag.TracetagPscan, _localIp, _localPort);
        else
            TraceDisable(ETraceTag.TracetagPscan, _localIp, _localPort, out _);
    }

    /// <summary>
    ///     开启或关闭GPS/罗盘数据
    /// </summary>
    /// <param name="isOpened"></param>
    private void CheckGpsCompassSwitch(bool isOpened)
    {
        if (isOpened)
            TraceEnable(ETraceTag.TracetagGpsCompass, _localIp, _localPort);
        else
            TraceDisable(ETraceTag.TracetagGpsCompass, _localIp, _localPort, out _);
    }

    #endregion

    #region 扫描测向

    /// <summary>
    ///     设置扫描参数
    /// </summary>
    /// <param name="iScanRangeId"></param>
    /// <param name="iFreqBegin"></param>
    /// <param name="iFreqEnd"></param>
    /// <param name="eAntPol"></param>
    /// <param name="eSpan"></param>
    /// <param name="eDfPanStep"></param>
    /// <param name="eAvgMode"></param>
    /// <param name="eBlockAveragingSelect"></param>
    /// <param name="iBlockAveragingCycles"></param>
    /// <param name="iBlockAveragingTime"></param>
    /// <param name="iThreshold"></param>
    /// <param name="eAntPreAmp"></param>
    /// <param name="eDfAlt"></param>
    /// <param name="eWindowType"></param>
    /// <param name="eDfPanSelectivity"></param>
    /// <param name="eAttSelect"></param>
    /// <param name="iAttValue"></param>
    /// <param name="iAttHoldTime"></param>
    /// <param name="iTsTimeMeas"></param>
    /// <param name="iTsTimeFreqChange"></param>
    /// <param name="iTsTimeScanRange"></param>
    /// <param name="message"></param>
    /// <returns>返回跃点数</returns>
    private int ScanRange(int iScanRangeId,
        long iFreqBegin,
        long iFreqEnd,
        EAntPol eAntPol,
        ESpan eSpan,
        EDfPanStep eDfPanStep,
        EAverageMode eAvgMode,
        EBlockAveragingSelect eBlockAveragingSelect,
        int iBlockAveragingCycles,
        int iBlockAveragingTime,
        int iThreshold,
        EState eAntPreAmp,
        EDfAlt eDfAlt,
        EWindowType eWindowType,
        EDfPanSelectivity eDfPanSelectivity,
        EAttSelect eAttSelect,
        int iAttValue,
        int iAttHoldTime,
        long iTsTimeMeas,
        long iTsTimeFreqChange,
        long iTsTimeScanRange,
        out string message)
    {
        Request request = new(OpreationMode.Set);
        request.Excute(CmdScanrange, nameof(iScanRangeId), iScanRangeId.ToString());
        request.Excute(CmdScanrange, nameof(iFreqBegin), iFreqBegin.ToString());
        request.Excute(CmdScanrange, nameof(iFreqEnd), iFreqEnd.ToString());
        request.Excute(CmdScanrange, nameof(eAntPol), eAntPol.ToString());
        request.Excute(CmdScanrange, nameof(eSpan), eSpan.ToString());
        request.Excute(CmdScanrange, nameof(eDfPanStep), eDfPanStep.ToString());
        request.Excute(CmdScanrange, nameof(eAvgMode), eAvgMode.ToString());
        request.Excute(CmdScanrange, nameof(eBlockAveragingSelect), eBlockAveragingSelect.ToString());
        request.Excute(CmdScanrange, nameof(iBlockAveragingCycles), iBlockAveragingCycles.ToString());
        request.Excute(CmdScanrange, nameof(iBlockAveragingTime), iBlockAveragingTime.ToString());
        request.Excute(CmdScanrange, nameof(iThreshold), iThreshold.ToString());
        request.Excute(CmdScanrange, nameof(eAntPreAmp), eAntPreAmp.ToString());
        request.Excute(CmdScanrange, nameof(eDfAlt), eDfAlt.ToString());
        request.Excute(CmdScanrange, nameof(eWindowType), eWindowType.ToString());
        request.Excute(CmdScanrange, nameof(eDfPanSelectivity), eDfPanSelectivity.ToString());
        request.Excute(CmdScanrange, nameof(eAttSelect), eAttSelect.ToString());
        request.Excute(CmdScanrange, nameof(iAttValue), iAttValue.ToString());
        request.Excute(CmdScanrange, nameof(iAttHoldTime), iAttHoldTime.ToString());
        request.Excute(CmdScanrange, nameof(iTsTimeMeas), iTsTimeMeas.ToString());
        request.Excute(CmdScanrange, nameof(iTsTimeFreqChange), iTsTimeFreqChange.ToString());
        request.Excute(CmdScanrange, nameof(iTsTimeScanRange), iTsTimeScanRange.ToString());
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode))
        {
            message = reply.Command.RtnMessage;
            return -1;
        }

        message = "";
        foreach (var para in reply.Command.Params)
            if (para.Name == "iNumHops")
            {
                var val = para.Value;
                if (int.TryParse(val, out var hops)) return hops;
                message = "设置失败";
                return -1;
            }

        message = "设置失败";
        return -1;
    }

    /// <summary>
    ///     设置频段参数
    /// </summary>
    /// <param name="iScanRangeId">频段ID</param>
    /// <param name="paras">键值对参数,键为频段参数名,值为参数值</param>
    private int ScanRange(int iScanRangeId, params Param[] paras)
    {
        Request request = new(OpreationMode.Set);
        request.Excute(CmdScanrange, nameof(iScanRangeId), iScanRangeId.ToString());
        if (paras == null || paras.Length == 0) return -1;

        foreach (var pairs in paras) request.Excute(CmdScanrange, pairs.Name, pairs.Value);
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode)) return -1;

        foreach (var para in reply.Command.Params)
            if (para.Name == "iNumHops")
            {
                var val = para.Value;
                if (int.TryParse(val, out var hops)) return hops;
                return -1;
            }

        return -1;
    }

    /// <summary>
    ///     添加扫描频段
    /// </summary>
    /// <param name="iFreqBegin"></param>
    /// <param name="iFreqEnd"></param>
    /// <param name="eAntPol"></param>
    /// <param name="eSpan"></param>
    /// <param name="eDfPanStep"></param>
    /// <param name="eAvgMode"></param>
    /// <param name="eBlockAveragingSelect"></param>
    /// <param name="iBlockAveragingCycles"></param>
    /// <param name="iBlockAveragingTime"></param>
    /// <param name="iThreshold"></param>
    /// <param name="eAntPreAmp"></param>
    /// <param name="eDfAlt"></param>
    /// <param name="eWindowType"></param>
    /// <param name="eDfPanSelectivity"></param>
    /// <param name="eAttSelect"></param>
    /// <param name="iAttValue"></param>
    /// <param name="iAttHoldTime"></param>
    /// <param name="iTsTimeMeas"></param>
    /// <param name="iTsTimeFreqChange"></param>
    /// <param name="iTsTimeScanRange"></param>
    /// <param name="message"></param>
    private void ScanRangeAdd(long iFreqBegin,
        long iFreqEnd,
        EAntPol eAntPol,
        ESpan eSpan,
        EDfPanStep eDfPanStep,
        EAverageMode eAvgMode,
        EBlockAveragingSelect eBlockAveragingSelect,
        int iBlockAveragingCycles,
        int iBlockAveragingTime,
        int iThreshold,
        EState eAntPreAmp,
        EDfAlt eDfAlt,
        EWindowType eWindowType,
        EDfPanSelectivity eDfPanSelectivity,
        EAttSelect eAttSelect,
        int iAttValue,
        int iAttHoldTime,
        long iTsTimeMeas,
        long iTsTimeFreqChange,
        long iTsTimeScanRange,
        out string message)
    {
        Request request = new(OpreationMode.Set);
        request.Excute(CmdScanrangeadd, nameof(iFreqBegin), iFreqBegin.ToString());
        request.Excute(CmdScanrangeadd, nameof(iFreqEnd), iFreqEnd.ToString());
        request.Excute(CmdScanrangeadd, nameof(eAntPol), eAntPol.ToString());
        request.Excute(CmdScanrangeadd, nameof(eSpan), eSpan.ToString());
        request.Excute(CmdScanrangeadd, nameof(eDfPanStep), eDfPanStep.ToString());
        request.Excute(CmdScanrangeadd, nameof(eAvgMode), eAvgMode.ToString());
        request.Excute(CmdScanrangeadd, nameof(eBlockAveragingSelect), eBlockAveragingSelect.ToString());
        request.Excute(CmdScanrangeadd, nameof(iBlockAveragingCycles), iBlockAveragingCycles.ToString());
        request.Excute(CmdScanrangeadd, nameof(iBlockAveragingTime), iBlockAveragingTime.ToString());
        request.Excute(CmdScanrangeadd, nameof(iThreshold), iThreshold.ToString());
        request.Excute(CmdScanrangeadd, nameof(eAntPreAmp), eAntPreAmp.ToString());
        request.Excute(CmdScanrangeadd, nameof(eDfAlt), eDfAlt.ToString());
        request.Excute(CmdScanrangeadd, nameof(eWindowType), eWindowType.ToString());
        request.Excute(CmdScanrangeadd, nameof(eDfPanSelectivity), eDfPanSelectivity.ToString());
        request.Excute(CmdScanrangeadd, nameof(eAttSelect), eAttSelect.ToString());
        request.Excute(CmdScanrangeadd, nameof(iAttValue), iAttValue.ToString());
        request.Excute(CmdScanrangeadd, nameof(iAttHoldTime), iAttHoldTime.ToString());
        request.Excute(CmdScanrangeadd, nameof(iTsTimeMeas), iTsTimeMeas.ToString());
        request.Excute(CmdScanrangeadd, nameof(iTsTimeFreqChange), iTsTimeFreqChange.ToString());
        request.Excute(CmdScanrangeadd, nameof(iTsTimeScanRange), iTsTimeScanRange.ToString());
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode))
        {
            message = reply.Command.RtnMessage;
            return;
        }

        message = "";
        foreach (var para in reply.Command.Params)
        {
            var value = para.Value;
            switch (para.Name)
            {
                case "iScanRangeId":
                    if (!int.TryParse(value, out _scanRangeId)) _scanRangeId = -1;
                    break;
                case "iNumHops":
                    if (!int.TryParse(value, out _numHops)) _numHops = -1;
                    break;
                case "iTsTimeMeas": //只有TS选件下才有
                case "iTsTimeFreqChange": //只有TS选件下才有
                case "iTsTimeScanRange": //只有TS选件下才有
                    break;
            }
        }
    }

    /// <summary>
    ///     添加扫描频段
    /// </summary>
    /// <param name="iFreqBegin">必选参数:起始频率</param>
    /// <param name="iFreqEnd">必选参数:结束频率</param>
    /// <param name="eSpan">必选参数:频谱跨距,频谱带宽</param>
    /// <param name="eDfPanStep">必选参数:测向步进,测向带宽</param>
    /// <param name="paras">可选参数</param>
    private void ScanRangeAdd(long iFreqBegin, long iFreqEnd, ESpan eSpan, EDfPanStep eDfPanStep,
        params Param[] paras)
    {
        Request request = new(OpreationMode.Set);
        //这四个是必选参数
        request.Excute(CmdScanrangeadd, nameof(iFreqBegin), iFreqBegin.ToString());
        request.Excute(CmdScanrangeadd, nameof(iFreqEnd), iFreqEnd.ToString());
        request.Excute(CmdScanrangeadd, nameof(eSpan), eSpan.ToString());
        request.Excute(CmdScanrangeadd, nameof(eDfPanStep), eDfPanStep.ToString());
        if (paras?.Length > 0)
            foreach (var pairs in paras)
                request.Excute(CmdScanrangeadd, pairs.Name, pairs.Value);
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode))
            //throw new Exception("参数设置错误！" + message);
            return;

        foreach (var para in reply.Command.Params)
        {
            var value = para.Value;
            switch (para.Name)
            {
                case "iScanRangeId":
                    if (!int.TryParse(value, out _scanRangeId)) _scanRangeId = -1;
                    break;
                case "iNumHops":
                    if (!int.TryParse(value, out _numHops)) _numHops = -1;
                    break;
                case "iTsTimeMeas": //只有TS选件下才有
                case "iTsTimeFreqChange": //只有TS选件下才有
                case "iTsTimeScanRange": //只有TS选件下才有
                    break;
            }
        }
    }

    /// <summary>
    ///     删除扫描频段
    /// </summary>
    /// <param name="iScanRangeId">要删除的频段ID</param>
    /// <param name="message"></param>
    private bool ScanRangeDelete(int iScanRangeId, out string message)
    {
        Request request = new(OpreationMode.Set)
        {
            Command =
            {
                Name = CmdScanrangedelete
            }
        };
        request.Excute(CmdScanrangedelete, nameof(iScanRangeId), iScanRangeId.ToString());
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode))
        {
            message = reply.Command.RtnMessage;
            return false;
        }

        message = "";
        return true;
    }

    /// <summary>
    ///     删除所有扫描频段
    /// </summary>
    private bool ScanRangeDeleteAll()
    {
        Request request = new(OpreationMode.Set)
        {
            Command =
            {
                Name = CmdScanrangedeleteall
            }
        };
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode)) return false;

        return true;
    }

    /// <summary>
    ///     删除所有扫描频段
    /// </summary>
    private bool SearchRangeDeleteAll()
    {
        Request request = new(OpreationMode.Set)
        {
            Command =
            {
                Name = "SearchRangeDeleteAll"
            }
        };
        var reply = SetAnalyzeData(request);
        if (!string.IsNullOrEmpty(reply.Command.RtnCode)) return false;

        return true;
    }

    #endregion 扫描测向

    #region 内部参数约束

    // 存放频谱带宽与中频带宽的键值对
    private readonly Dictionary<double, double> _defaultStepSpanDic = new()
    {
        { 80000, 50 }, { 40000, 25 }, { 20000, 12.5 }, { 10000, 6.25 }, { 5000, 3.125 }, { 2000, 1.25 },
        { 1000, 0.625 },
        { 500, 0.3125 }, { 200, 0.125 }, { 100, 0.0625 }
    };

    /// <summary>
    ///     根据频谱带宽获取默认中频带宽值
    /// </summary>
    /// <returns></returns>
    public double GetDefaultIfBandWidth()
    {
        var defaultVaue = 0.0;
        try
        {
            _defaultStepSpanDic.TryGetValue(IfBandwidth, out defaultVaue);
        }
        catch
        {
        }

        return defaultVaue;
    }

    // 存放频谱带宽集合
    private readonly double[] _spectrumSpanSample = { 100, 200, 500, 1000, 2000, 5000, 10000, 20000, 40000, 80000 };

    // 对应频谱带宽的中频带宽的默认值集合
    private readonly double[] _ifPanDefault = { 62.5, 125, 312.5, 625, 1250, 3125, 6250, 12500, 25000, 50000 };

    /// <summary>
    ///     根据测向带宽获取频谱带宽
    /// </summary>
    /// <returns></returns>
    public double GetDefaultSpectrumSpan(double bandWidth)
    {
        var isFind = false;
        var defaultSpan = 0.0;
        var cnt = _spectrumSpanSample.Length;
        for (var i = 0; i < cnt; i++)
        {
            var span = _spectrumSpanSample[i] / bandWidth;
            if (span is >= 40 and <= 80)
            {
                isFind = true;
                defaultSpan = _spectrumSpanSample[i];
                break;
            }
        }

        if (!isFind) defaultSpan = _spectrumSpanSample[cnt - 1];
        return defaultSpan;
    }

    /// <summary>
    ///     根据频谱带宽获取中频带宽
    /// </summary>
    /// <param name="span"></param>
    /// <returns></returns>
    public double GetDefaultIfPanStep(double span)
    {
        var index = Array.IndexOf(_spectrumSpanSample, span);
        if (index >= 0) return _ifPanDefault[index];
        return _ifPanDefault[_ifPanDefault.Length / 2];
    }

    // 存放扫描测向所有步进值
    private readonly double[] _stepSample =
        { 1, 1.25, 2, 2.5, 3.125, 5, 6.25, 8.333, 10, 12.5, 20, 25, 50, 100, 200, 500, 1000, 2000 };

    // 对应扫描测向每个步进的带宽最大值(DDF550有bug,当扫描测向的带宽大于40M的时候经常报错IFPan太大,因此屏蔽40M以上的带宽)
    private readonly double[] _spanMax =
    {
        2000, 2000, 5000, 5000, 10000, 10000, 20000, 20000, 20000, 20000, 20000, 20000, 20000, 20000, 20000, 20000,
        20000, 20000
    };

    /// <summary>
    ///     根据Step获取Span最大值
    /// </summary>
    /// <param name="step"></param>
    /// <returns></returns>
    public double GetMaxSpecturmSpan(double step)
    {
        var index = Array.IndexOf(_stepSample, step);
        if (index < 0) return _spanMax[_spanMax.Length / 2];
        if (index > _spanMax.Length - 1) return _spanMax[^1];
        return _spanMax[index];
    }

    #endregion
}