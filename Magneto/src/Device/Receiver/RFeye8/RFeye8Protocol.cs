using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Magneto.Device.RFeye8;

/// <summary>
///     用以标识FieldName和ParameterName
/// </summary>
internal static class PacketKey
{
    /// <summary>
    ///     Request/retrieve spectral data from the node.
    ///     BuildKey('S', 'W', 'E', 'P')
    /// </summary>
    public const int FieldSweep = 0x50455753;

    /// <summary>
    ///     BuildKey('F', 'S', 'T', 'A')
    /// </summary>
    public const int SweepStartFreqMHz = 0x41545346;

    /// <summary>
    ///     BuildKey('F', 'S', 'T', 'P')
    /// </summary>
    public const int SweepStopFreqMHz = 0x50545346;

    /// <summary>
    ///     BuildKey('F', 'S', 'A', 'M')
    /// </summary>
    public const int SweepStartFreqMilliHz = 0x4D415346;

    /// <summary>
    ///     BuildKey('F', 'S', 'P', 'M')
    /// </summary>
    public const int SweepStopFreqMilliHz = 0x4D505346;

    /// <summary>
    ///     BuildKey('C', 'H', 'A', 'N')
    /// </summary>
    public const int SweepChannel = 0x4E414843;

    /// <summary>
    ///     BuildKey('C', 'H', 'A', 'E')
    /// </summary>
    public const int SweepChannelEnd = 0x45414843;

    /// <summary>
    ///     BuildKey('F', 'T', 'U', 'N')
    /// </summary>
    public const int SweepQualityFastTune = 0x4E555446;

    /// <summary>
    ///     BuildKey('Q', 'D', 'U', 'P')
    /// </summary>
    public const int SweepQualityDoulex = 0x50554451;

    /// <summary>
    ///     BuildKey('H', 'S', 'P', 'E')
    /// </summary>
    public const int SweepHighSpeed = 0x45505348;

    /// <summary>
    ///     BuildKey('N', 'S', 'P', 'E')
    /// </summary>
    public const int SweepNormalSpeed = 0x4550534E;

    /// <summary>
    ///     BuildKey('R', 'E', 'S', 'B')
    /// </summary>
    public const int SweepResBandwidthHz = 0x42534552;

    /// <summary>
    ///     BuildKey('S', 'F', 'F', 'T')
    /// </summary>
    public const int SweepFftSize = 0x54464653;

    /// <summary>
    ///     BuildKey('N', 'U', 'M', 'L')
    /// </summary>
    public const int SweepNumLoops = 0x4C4D554E;

    /// <summary>
    ///     BuildKey('I', 'N', 'P', 'T')
    /// </summary>
    public const int SweepInput = 0x54504E49;

    /// <summary>
    ///     BuildKey('A', 'U', 'I', 'D')
    /// </summary>
    public const int SweepAntennaUid = 0x44495541;

    /// <summary>
    ///     BuildKey('R', 'L', 'E', 'V')
    /// </summary>
    public const int SweepRefLevel = 0x56454C52;

    /// <summary>
    ///     BuildKey('D', 'W', 'E', 'L')
    /// </summary>
    public const int SweepDwellTime = 0x4C455744;

    /// <summary>
    ///     BuildKey('A', 'T', 'E', 'N')
    /// </summary>
    public const int SweepManualAtten = 0x4E455441;

    /// <summary>
    ///     BuildKey('A', 'G', 'C', ' ')
    /// </summary>
    public const int SweepAgc = 0x20434741;

    /// <summary>
    ///     BuildKey('A', 'G', 'C', 'T')
    /// </summary>
    public const int SweepAgcTableMode = 0x54434741;

    /// <summary>
    ///     BuildKey('C', 'M', 'D', ' ')
    /// </summary>
    public const int SweepCommand = 0x20444D43;

    /// <summary>
    ///     BuildKey('D', 'E', 'C', 'I')
    /// </summary>
    public const int SweepDecimation = 0x49434544;

    /// <summary>
    ///     BuildKey('G', 'E', 'T', 'P')
    /// </summary>
    public const int SweepGetPeakData = 0x50544547;

    /// <summary>
    ///     BuildKey('G', 'E', 'T', 'A')
    /// </summary>
    public const int SweepGetAverageData = 0x41544547;

    /// <summary>
    ///     BuildKey('G', 'E', 'T', 'T')
    /// </summary>
    public const int SweepGetTimeData = 0x54544547;

    /// <summary>
    ///     BuildKey('G', 'E', 'T', 'S')
    /// </summary>
    public const int SweepGetStatusData = 0x53544547;

    /// <summary>
    ///     BuildKey('D', 'B', 'U', 'V')
    /// </summary>
    public const int SweepGetDBuvm = 0x56554244;

    /// <summary>
    ///     BuildKey('P', 'D', 'A', 'T')
    /// </summary>
    public const int SweepPeakData = 0x54414450;

    /// <summary>
    ///     BuildKey('A', 'D', 'A', 'T')
    /// </summary>
    public const int SweepAverageData = 0x54414441;

    /// <summary>
    ///     BuildKey('T', 'D', 'A', 'T')
    /// </summary>
    public const int SweepTimeIqData = 0x54414454;

    /// <summary>
    ///     BuildKey('S', 'D', 'A', 'T')
    /// </summary>
    public const int SweepStatusData = 0x54414453;

    /// <summary>
    ///     BuildKey('T', 'I', 'M', 'M')
    /// </summary>
    public const int SweepTrigModeImmediate = 0x4D4D4954;

    /// <summary>
    ///     BuildKey('T', 'A', 'B', 'S')
    /// </summary>
    public const int SweepTrigModeAbsTime = 0x53424154;

    /// <summary>
    ///     BuildKey('T', 'U', 'N', 'X')
    /// </summary>
    public const int SweepTrigTimeUnix = 0x584E5554;

    /// <summary>
    ///     BuildKey('T', 'N', 'A', 'N')
    /// </summary>
    public const int SweepTrigTimeNano = 0x4E414E54;

    /// <summary>
    ///     BuildKey('T', 'E', 'X', '1')
    /// </summary>
    public const int SweepTrigModeExp1 = 0x31584554;

    /// <summary>
    ///     BuildKey('T', 'E', 'X', '2')
    /// </summary>
    public const int SweepTrigModeExp2 = 0x32584554;

    /// <summary>
    ///     BuildKey('T', 'G', 'P', 'S')
    /// </summary>
    public const int SweepTrigModeGps = 0x53504754;

    /// <summary>
    ///     BuildKey('S', 'Y', 'N', 'C')
    /// </summary>
    public const int SweepSyncMode = 0x434E5953;

    /// <summary>
    ///     BuildKey('S', 'T', 'D', 'L')
    /// </summary>
    public const int SweepSyncTuneInterval = 0x4C445453;

    /// <summary>
    ///     BuildKey('R', 'A', 'R', 'M')
    /// </summary>
    public const int SweepSyncMaxReArmTimeNs = 0x4D524152;

    /// <summary>
    ///     BuildKey('N', 'S', 'W', 'P')
    /// </summary>
    public const int SweepSyncNumSweeps = 0x5057534E;

    /// <summary>
    ///     BuildKey('S', 'S', 'D', 'T')
    /// </summary>
    public const int SweepSyncDelayTune = 0x54445353;

    /// <summary>
    ///     BuildKey('T', 'S', 'T', 'R')
    /// </summary>
    public const int SweepTrigTimeStr = 0x52545354;

    /// <summary>
    ///     BuildKey('R', 'U', 'N', 'X')
    /// </summary>
    public const int SweepRepeatDelayUnix = 0x584E5552;

    /// <summary>
    ///     BuildKey('R', 'N', 'A', 'N')
    /// </summary>
    public const int SweepRepeatDelayNano = 0x4E414E52;

    /// <summary>
    ///     BuildKey('T', 'M', 'I', 'S')
    /// </summary>
    public const int SweepRepeatMissedTriggers = 0x53494D54;

    /// <summary>
    ///     BuildKey('M', 'U', 'N', 'X')
    /// </summary>
    public const int SweepRepeatTriggerMarginUnix = 0x584E554D;

    /// <summary>
    ///     BuildKey('M', 'N', 'A', 'N')
    /// </summary>
    public const int SweepRepeatTriggerMarginNano = 0x4E414E4D;

    /// <summary>
    ///     BuildKey('A', 'S', 'Y', '1')
    /// </summary>
    public const int SweepAsyncMode = 0x31595341;

    /// <summary>
    ///     BuildKey('A', 'S', 'T', 'A')
    /// </summary>
    public const int SweepAsyncTable = 0x41545341;

    /// <summary>
    ///     BuildKey('A', 'S', 'E', 'X')
    /// </summary>
    public const int SweepAsyncExpPort = 0x58455341;

    /// <summary>
    ///     Field and options to request\\retreive audio data
    /// </summary>
    public const int FieldDemodulation = 0x444F4D44; //BuildKey('D', 'M', 'O', 'D')

    /// <summary>
    ///     BuildKey('F', 'C', 'T', 'R')
    /// </summary>
    public const int DemodRadioTuneFreqMHz = 0x52544346;

    /// <summary>
    ///     BuildKey('F', 'D', 'D', 'S')
    /// </summary>
    public const int DemodDdsFreqKHz = 0x53444446;

    /// <summary>
    ///     BuildKey('I', 'N', 'P', 'T')
    /// </summary>
    public const int DemodInput = 0x54504E49;

    /// <summary>
    ///     BuildKey('D', 'E', 'C', 'I')
    /// </summary>
    public const int DemodRfDecimation = 0x49434544;

    /// <summary>
    ///     BuildKey('D', 'G', 'A', 'I')
    /// </summary>
    public const int DemodGain = 0x49414744;

    /// <summary>
    ///     BuildKey('M', 'U', 'T', 'E')
    /// </summary>
    public const int DemodFmMuteThd = 0x4554554D;

    /// <summary>
    ///     BuildKey('S', 'I', 'L', 'E')
    /// </summary>
    public const int DemodSilenceThreshold = 0x454C4953;

    /// <summary>
    ///     BuildKey('C', 'O', 'M', 'P')
    /// </summary>
    public const int DemodCompressionLevel = 0x504D4F43;

    /// <summary>
    ///     BuildKey('F', 'M', 'D', 'E')
    /// </summary>
    public const int DemodModeFm = 0x45444D46;

    /// <summary>
    ///     BuildKey('A', 'M', 'D', 'E')
    /// </summary>
    public const int DemodModeAm = 0x45444D41;

    /// <summary>
    ///     BuildKey('D', 'O', 'F', 'F')
    /// </summary>
    public const int DemodModeOff = 0x46464F44;

    /// <summary>
    ///     BuildKey('D', 'D', 'E', 'C')
    /// </summary>
    public const int DemodDataDecimation = 0x43454444;

    /// <summary>
    ///     BuildKey('D', 'D', 'A', 'T')
    /// </summary>
    public const int DemodData = 0x54414444;

    /// <summary>
    ///     BuildKey('G', 'S', 'P', 'E')
    /// </summary>
    public const int DemodGetSpectrumData = 0x45505347;

    /// <summary>
    ///     BuildKey('S', 'D', 'E', 'L')
    /// </summary>
    public const int DemodSpectrumDelay = 0x4C454453;

    /// <summary>
    ///     BuildKey('S', 'P', 'E', 'C')
    /// </summary>
    public const int DemodSpectrumData = 0x43455053;

    /// <summary>
    ///     Request/retrieve IQ time data from the node
    /// </summary>
    public const int FieldTime = 0x454D4954; //BuildKey('T', 'I', 'M', 'E')

    /// <summary>
    ///     BuildKey('F', 'C', 'T', 'R')
    /// </summary>
    public const int TimeCenterFreqMHz = 0x52544346;

    /// <summary>
    ///     BuildKey('D', 'D', 'S', 'O')
    /// </summary>
    public const int TimeDdsOffsetHz = 0x4F534444;

    /// <summary>
    ///     BuildKey('D', 'D', 'S', 'S')
    /// </summary>
    public const int TimeDdsScale = 0x53534444;

    /// <summary>
    ///     BuildKey('S', 'A', 'M', 'P')
    /// </summary>
    public const int TimeNumSamples = 0x504D4153;

    /// <summary>
    ///     BuildKey('A', 'T', 'E', 'N')
    /// </summary>
    public const int TimeManualAtten = 0x4E455441;

    /// <summary>
    ///     BuildKey('A', 'G', 'C', ' ')
    /// </summary>
    public const int TimeAgc = 0x20434741;

    /// <summary>
    ///     BuildKey('A', 'G', 'C', 'T')
    /// </summary>
    public const int TimeAgcTableMode = 0x54434741;

    /// <summary>
    ///     BuildKey('I', 'N', 'P', 'T')
    /// </summary>
    public const int TimeInput = 0x54504E49;

    /// <summary>
    ///     BuildKey('A', 'U', 'I', 'D')
    /// </summary>
    public const int TimeAntennaUid = 0x44495541;

    /// <summary>
    ///     BuildKey('D', 'E', 'C', 'I')
    /// </summary>
    public const int TimeDecimation = 0x49434544;

    /// <summary>
    ///     BuildKey('L', 'N', 'O', 'I')
    /// </summary>
    public const int TimeQualityLowNoise = 0x494F4E4C;

    /// <summary>
    ///     BuildKey('F', 'T', 'U', 'N')
    /// </summary>
    public const int TimeQualityFastTune = 0x4E555446;

    /// <summary>
    ///     BuildKey('T', 'S', 'D', 'C')
    /// </summary>
    public const int TimeSimpleDcNumCap = 0x43445354;

    /// <summary>
    ///     BuildKey('T', 'I', 'M', 'M')
    /// </summary>
    public const int TimeTrigModeImmediate = 0x4D4D4954;

    /// <summary>
    ///     BuildKey('T', 'E', 'X', '1')
    /// </summary>
    public const int TimeTrigModeExp1 = 0x31584554;

    /// <summary>
    ///     BuildKey('T', 'E', 'X', '2')
    /// </summary>
    public const int TimeTrigModeExp2 = 0x32584554;

    /// <summary>
    ///     BuildKey('T', 'A', 'B', 'S')
    /// </summary>
    public const int TimeTrigModeAbsTime = 0x53424154;

    /// <summary>
    ///     BuildKey('T', 'U', 'N', 'X')
    /// </summary>
    public const int TimeTrigAbsUnix = 0x584E5554;

    /// <summary>
    ///     BuildKey('T', 'N', 'A', 'N')
    /// </summary>
    public const int TimeTrigAbsNano = 0x4E414E54;

    /// <summary>
    ///     BuildKey('T', 'G', 'P', 'S')
    /// </summary>
    public const int TimeTrigModeGps = 0x53504754;

    /// <summary>
    ///     BuildKey('T', 'G', 'S', 'M')
    /// </summary>
    public const int TimeTrigModeGsmFrame = 0x4D534754;

    /// <summary>
    ///     BuildKey('T', 'P', 'T', 'H')
    /// </summary>
    public const int TimeTrigModePowerThresh = 0x48545054;

    /// <summary>
    ///     BuildKey('T', 'P', 'P', 'T')
    /// </summary>
    public const int TimeTrigPtThresh = 0x54505054;

    /// <summary>
    ///     BuildKey('T', 'P', 'D', 'W')
    /// </summary>
    public const int TimeTrigPtDwell = 0x57445054;

    /// <summary>
    ///     BuildKey('T', 'P', 'P', 'O')
    /// </summary>
    public const int TimeTrigPtPort = 0x4F505054;

    /// <summary>
    ///     BuildKey('T', 'T', 'N', 'C')
    /// </summary>
    public const int TimeTrigNumberCaptures = 0x434E5454;

    /// <summary>
    ///     BuildKey('R', 'U', 'N', 'X')
    /// </summary>
    public const int TimeTrigRepeatUnix = 0x584E5552;

    /// <summary>
    ///     BuildKey('R', 'N', 'A', 'N')
    /// </summary>
    public const int TimeTrigRepeatNano = 0x4E414E52;

    /// <summary>
    ///     BuildKey('T', 'M', 'I', 'S')
    /// </summary>
    public const int TimeRepeatMissedTriggers = 0x53494D54;

    /// <summary>
    ///     BuildKey('M', 'U', 'N', 'X')
    /// </summary>
    public const int TimeRepeatTriggerMarginUnix = 0x584E554D;

    /// <summary>
    ///     BuildKey('M', 'N', 'A', 'N')
    /// </summary>
    public const int TimeRepeatTriggerMarginNano = 0x4E414E4D;

    /// <summary>
    ///     BuildKey('R', 'G', 'A', 'I')
    /// </summary>
    public const int TimeRadioGain = 0x49414752;

    /// <summary>
    ///     BuildKey('R', 'A', 'T', 'N')
    /// </summary>
    public const int TimeAgcAtten = 0x4E544152;

    /// <summary>
    ///     BuildKey('F', 'I', 'N', 'V')
    /// </summary>
    public const int TimeFreqPlanInvert = 0x564E4946;

    /// <summary>
    ///     BuildKey('D', 'C', 'O', 'I')
    /// </summary>
    public const int TimeDcOffsetI = 0x494F4344;

    /// <summary>
    ///     BuildKey('D', 'C', 'O', 'Q')
    /// </summary>
    public const int TimeDcOffsetQ = 0x514F4344;

    /// <summary>
    ///     BuildKey('G', 'I', 'Q', 'T')
    /// </summary>
    public const int TimeGetIqData = 0x54514947;

    /// <summary>
    ///     BuildKey('D', 'I', 'Q', 'T')
    /// </summary>
    public const int TimeIqData = 0x54514944;

    /// <summary>
    ///     BuildKey('T', 'N', 'R', 'T')
    /// </summary>
    public const int TimeNoRetune = 0x54524E54;

    /// <summary>
    ///     BuildKey('T', 'P', 'D', 'A')
    /// </summary>
    public const int TimePackData = 0x41445054;

    /// <summary>
    ///     BuildKey('T', 'P', 'S', 'C')
    /// </summary>
    public const int TimePackScale = 0x43535054;

    /// <summary>
    ///     BuildKey('M', 'I', 'N', 'I')
    /// </summary>
    public const int TimePackMinI = 0x494E494D;

    /// <summary>
    ///     BuildKey('M', 'A', 'X', 'I')
    /// </summary>
    public const int TimePackMaxI = 0x4958414D;

    /// <summary>
    ///     BuildKey('M', 'I', 'N', 'Q')
    /// </summary>
    public const int TimePackMinQ = 0x514E494D;

    /// <summary>
    ///     BuildKey('M', 'A', 'X', 'Q')
    /// </summary>
    public const int TimePackMaxQ = 0x5158414D;

    /// <summary>
    ///     BuildKey('T', 'S', 'I', 'G')
    /// </summary>
    public const int TimeTestSignal = 0x47495354;

    /// <summary>
    ///     BuildKey('T', 'M', 'I', 'N')
    /// </summary>
    public const int TimeTestMin = 0x4E494D54;

    /// <summary>
    ///     BuildKey('T', 'M', 'A', 'X')
    /// </summary>
    public const int TimeTestMax = 0x58414D54;

    /// <summary>
    ///     BuildKey('T', 'S', 'T', 'R')
    /// </summary>
    public const int TimeStreamData = 0x52545354;

    /// <summary>
    ///     brief Retrieve GPS information
    /// </summary>
    public const int FieldGps = 0x53504753; //BuildKey('S', 'G', 'P', 'S')

    /// <summary>
    ///     BuildKey('L', 'A', 'T', 'I')
    /// </summary>
    public const int GpsLatitude = 0x4954414C;

    /// <summary>
    ///     BuildKey('L', 'O', 'N', 'G')
    /// </summary>
    public const int GpsLongitude = 0x474E4F4C;

    /// <summary>
    ///     BuildKey('S', 'A', 'T', 'S')
    /// </summary>
    public const int GpsSatellites = 0x53544153;

    /// <summary>
    ///     BuildKey('G', 'F', 'I', 'X')
    /// </summary>
    public const int GpsFix = 0x58494647;

    /// <summary>
    ///     BuildKey('S', 'T', 'A', 'T')
    /// </summary>
    public const int GpsStatus = 0x54415453;

    /// <summary>
    ///     BuildKey('U', 'T', 'I', 'M')
    /// </summary>
    public const int GpsUtim = 0x4D495455;

    /// <summary>
    ///     BuildKey('S', 'P', 'E', 'E')
    /// </summary>
    public const int GpsSpeed = 0x45455053;

    /// <summary>
    ///     BuildKey('H', 'E', 'A', 'D')
    /// </summary>
    public const int GpsHeading = 0x44414548;

    /// <summary>
    ///     BuildKey('A', 'L', 'T', 'I')
    /// </summary>
    public const int GpsAltitude = 0x49544C41;

    /// <summary>
    ///     BuildKey('T', 'S', 'T', 'R')
    /// </summary>
    public const int GpsDatetimeString = 0x52545354;

    /// <summary>
    ///     brief Update/Retrieve reference clock source
    /// </summary>
    public const int FieldRefClock = 0x4B4C4352; //BuildKey('R', 'C', 'L', 'K')

    /// <summary>
    ///     BuildKey('R', 'D', 'A', 'C')
    /// </summary>
    public const int RefClockSourceDac = 0x43414452;

    /// <summary>
    ///     BuildKey('R', 'G', 'P', 'S')
    /// </summary>
    public const int RefClockSourceGps = 0x53504752;

    /// <summary>
    ///     BuildKey('R', 'E', 'X', '1')
    /// </summary>
    public const int RefClockSourceExp1 = 0x31584552;

    /// <summary>
    ///     BuildKey('R', 'E', 'X', '2')
    /// </summary>
    public const int RefClockSourceExp2 = 0x32584552;

    /// <summary>
    ///     BuildKey('O', 'E', 'X', '1')
    /// </summary>
    public const int RefClockOutExp1 = 0x3158454F;

    /// <summary>
    ///     BuildKey('O', 'E', 'X', '2')
    /// </summary>
    public const int RefClockOutExp2 = 0x3258454F;

    /// <summary>
    ///     BuildKey('R', 'D', 'T', 'T')
    /// </summary>
    public const int RefClockDisableTimeTransmission = 0x54544452;

    /// <summary>
    ///     BuildKey('S', 'D', 'A', 'C')
    /// </summary>
    public const int RefClockDacSetting = 0x43414453;

    /// <summary>
    ///     brief Update/Retrieve RTC settings
    /// </summary>
    public const int FieldDspRtc = 0x4C435452; //BuildKey('R', 'T', 'C', 'L')

    /// <summary>
    ///     BuildKey('G', 'T', 'I', 'M')
    /// </summary>
    public const int DspRtcGetTime = 0x4D495447;

    /// <summary>
    ///     BuildKey('G', 'T', 'F', 'U')
    /// </summary>
    public const int DspRtcGetTimeFutureNs = 0x55465447;

    /// <summary>
    ///     BuildKey('S', 'T', 'I', 'M')
    /// </summary>
    public const int DspRtcSetTime = 0x4D495453;

    /// <summary>
    ///     BuildKey('N', 'D', 'A', 'T')
    /// </summary>
    public const int DspRtcNowDate = 0x5441444E;

    /// <summary>
    ///     BuildKey('N', 'T', 'I', 'M')
    /// </summary>
    public const int DspRtcNowTime = 0x4D49544E;

    /// <summary>
    ///     BuildKey('N', 'U', 'N', 'X')
    /// </summary>
    public const int DspRtcNowUnixTime = 0x584E554E;

    /// <summary>
    ///     BuildKey('N', 'N', 'A', 'N')
    /// </summary>
    public const int DspRtcNowNano = 0x4E414E4E;

    /// <summary>
    ///     BuildKey('N', 'S', 'T', 'R')
    /// </summary>
    public const int DspRtcNowStr = 0x5254534E;

    /// <summary>
    ///     BuildKey('F', 'U', 'N', 'X')
    /// </summary>
    public const int DspRtcFutureUnixTime = 0x584E5546;

    /// <summary>
    ///     BuildKey('F', 'N', 'A', 'N')
    /// </summary>
    public const int DspRtcFutureNano = 0x4E414E46;

    /// <summary>
    ///     BuildKey('F', 'S', 'T', 'R')
    /// </summary>
    public const int DspRtcFutureString = 0x52545346;

    /// <summary>
    ///     "Any" DSP options can be included\\returned in any packet of type DSPC or DSPL
    /// </summary>
    public const int AnyDspRtcUnixTime = 0x4D495455; //BuildKey('U', 'T', 'I', 'M')

    /// <summary>
    ///     BuildKey('N', 'A', 'N', 'O')
    /// </summary>
    public const int AnyDspRtcNano = 0x4F4E414E;

    /// <summary>
    ///     "Any" options can be returned in any packet to the client
    /// </summary>
    public const int AnyErrorCode = 0x43525245; //BuildKey('E', 'R', 'R', 'C')

    /// <summary>
    ///     BuildKey('W', 'A', 'R', 'C')
    /// </summary>
    public const int AnyWarningCode = 0x43524157;

    /// <summary>
    ///     BuildKey('A', 'C', 'K', 'N')
    /// </summary>
    public const int AnyAcknowledgePacket = 0x4E4B4341;

    /// <summary>
    ///     BuildKey('S', 'E', 'G', 'N')
    /// </summary>
    public const int AnyDataSegmentNumber = 0x4E474553;

    /// <summary>
    ///     BuildKey('N', 'S', 'E', 'G')
    /// </summary>
    public const int AnyNumDataSegments = 0x4745534E;

    /// <summary>
    ///     BuildKey('A', 'U', 'I', 'D')
    /// </summary>
    public const int AnyAntennaUid = 0x44495541;

    //TODO
    /// <summary>
    ///     BuildKey('H', 'E', 'L', 'O')
    /// </summary>
    public const int LinkFieldServerGreeting = 0x4F4C4548;

    /// <summary>
    ///     BuildKey('C', 'C', 'R', 'E')
    /// </summary>
    public const int LinkFieldClientConnReq = 0x45524343;

    /// <summary>
    ///     BuildKey('S', 'C', 'A', 'R')
    /// </summary>
    public const int LinkFieldServerAuthReq = 0x52414353;

    /// <summary>
    ///     BuildKey('C', 'A', 'R', 'E')
    /// </summary>
    public const int LinkFieldClientAuthResp = 0x45524143;

    /// <summary>
    ///     BuildKey('S', 'C', 'O', 'N')
    /// </summary>
    public const int LinkFieldServerCobfirm = 0x4E4F4353;

    /// <summary>
    ///     BuildKey('T', 'E', 'R', 'M')
    /// </summary>
    public const int LinkFieldTermReq = 0x4D524554;

    /// <summary>
    ///     BuildKey('C', 'I', 'W', 0)
    /// </summary>
    public const int LinkParamClientId = 0x574943;

    /// <summary>
    ///     BuildKey('C', 'A', 'R', 0);
    /// </summary>
    public const int LinkParamClientAuth = 0x524143;

    //TODO: 50-8新增的参数，FieldTime和FieldDemodulation中的中心频率小数部分都用FCTM
    /// <summary>
    ///     BuildKey('F', 'C', 'T', 'M')
    /// </summary>
    public const int TimeCenterFreqMilliHz = 0x4D544346;

    /// <summary>
    ///     BuildKey('F', 'C', 'T', 'M')
    /// </summary>
    public const int DemodRadioTuneFreqMilliHz = 0x4D544346;

    /// <summary>
    ///     BuildKey('O', 'R', 'A', 'T')
    /// </summary>
    public const int DemodSampleRateHz = 0x5441524F;

    /// <summary>
    ///     BuildKey('R', 'B', 'M', 'E')
    /// </summary>
    public const int DemodBandwidthMHz = 0x454D4252;

    /// <summary>
    ///     BuildKey('R', 'B', 'M', 'I')
    /// </summary>
    public const int DemodBandwidthMilliHz = 0x494D4252;

    /// <summary>
    ///     BuildKey('L', 'S', 'E', 'C')
    /// </summary>
    public const int TimeLengthSeconds = 0x4345534C;

    /// <summary>
    ///     BuildKey('L', 'N', 'A', 'N')
    /// </summary>
    public const int TimeLengthNano = 0x4E414E4C;

    /// <summary>
    ///     BuildKey('R', 'B', 'M', 'E')
    /// </summary>
    public const int TimeRealTimeBandwidthMHz = 0x454D4252;

    /// <summary>
    ///     BuildKey('R', 'B', 'M', 'I')
    /// </summary>
    public const int TimeRealTimeBandwidthMilliHz = 0x494D4252;
    //public static int BuildKey(char first, char second, char third, char fourth)
    //{
    //    return (first + (second << 8) + (third << 16) + (fourth << 24));
    //}
}

internal static class WarningCode
{
    public const int WarningUnknown = 0x0f00;
    public const int WarningUnrecognisedField = 0x0f01;
    public const int WarningUnrecognisedParameter = 0x0f02;
    public const int WarningCalibrationNotSupported = 0x0f03;
    public const int WarningAgcOverflow = 0x0f04;
    public const int WarningUncalibratedSweep = 0x0f05;
    public const int WarningBdcNotFound = 0x0f06;
    public const int WarningAdcOverflow = 0x0f07;
    public const int WarningDbmMeanUnderflow = 0x0f08;
    public const int WarningDbmMeanOverflow = 0x0f09;
    public const int WarningDbmPeakUnderflow = 0x0f0A;
    public const int WarningDbmPeakOverfolw = 0x0f0B;
    public const int WarningSynthFailed = 0x0f0C;
    public const int WarningNoGpsUsingIntenal = 0x0f0D;
    public const int WarningLocked = 0x0f0E;
    public const int WarningInvalidConfigurationFile = 0x0f0F;
    public const int WarningInvalidUid = 0x0f10;
    public const int WarningDefaultConfiguration = 0x0f11;
    public const int WarningAntennaOutsideRange = 0x0f12;
    public const int WarningModemTransmission = 0x0f13;
    public const int WarningLfdfPowerError = 0x0f14;

    public static string GetWarningInfo(int warningCode)
    {
        var warningInfo = string.Empty;
        switch (warningCode)
        {
            case WarningUnknown:
                warningInfo = "An internal error has occured.";
                break;
            case WarningUnrecognisedField:
                warningInfo = "Unrecognised field in packet.";
                break;
            case WarningUnrecognisedParameter:
                warningInfo = "Unrecognised parameter in packet.";
                break;
            case WarningCalibrationNotSupported:
                warningInfo = "Calibration requested but not supported.";
                break;
            case WarningAgcOverflow:
                warningInfo = "AGC overflow. Data may be effected.";
                break;
            case WarningUncalibratedSweep:
                warningInfo = "Unused.";
                break;
            case WarningBdcNotFound:
                warningInfo = "Command requested use of BDC and no BDC found.";
                break;
            case WarningAdcOverflow:
                warningInfo = "ADC overflow. Data may be effected.";
                break;
            case WarningDbmMeanUnderflow:
                warningInfo = "Unused.";
                break;
            case WarningDbmMeanOverflow:
                warningInfo = "Conversion of mean data to dBm overflowed.";
                break;
            case WarningDbmPeakUnderflow:
                warningInfo = "Unused.";
                break;
            case WarningDbmPeakOverfolw:
                warningInfo = "Conversion of peak data to dBm overflowed.";
                break;
            case WarningSynthFailed:
                warningInfo =
                    "The synth(High Speed, High quality) requested has failed. Other synth is used for command.";
                break;
            case WarningNoGpsUsingIntenal:
                warningInfo = "GPS has been requested for timing and there is no fix. Internal clock used for timing.";
                break;
            case WarningLocked:
                warningInfo = "Another client is using the client exclusivily.";
                break;
            case WarningInvalidConfigurationFile:
                warningInfo = "NCPd configuration file is invalid. This is _ONLY_ returned once on connection.";
                break;
            case WarningInvalidUid:
                warningInfo = "No antenna exists with given UID.";
                break;
            case WarningDefaultConfiguration:
                warningInfo = "Default configuration in use. This is _ONLY_ returned once on connection.";
                break;
            case WarningAntennaOutsideRange:
                warningInfo = "Antenna selected is being used outside of its specified range.";
                break;
            case WarningModemTransmission:
                warningInfo = "Internal modem was transmitting during data capture. Data may be effected.";
                break;
            case WarningLfdfPowerError:
                warningInfo = "LFDF power error.";
                break;
        }

        return warningInfo;
    }
}

internal static class ErrorCode
{
    public const int ErrorUnknown = 0xf000;
    public const int ErrorTimeout = 0xf002;
    public const int ErrorNcpInit = 0xf003;
    public const int ErrorUnrecongnisedPacket = 0xf004;
    public const int ErrorMultiSync = 0xf005;
    public const int ErrorOutOfMemory = 0xf006;
    public const int ErrorNoLock = 0xf007;
    public const int ErrorNoAntenna = 0xf008;
    public const int ErrorNoLights = 0xf009;
    public const int ErrorNoChannel = 0xf010;
    public const int ErrorNoDevice = 0xf011;
    public const int ErrorRadio = 0xf012;

    public static string GetErrorInfo(int errorCode)
    {
        var errorInfo = string.Empty;
        switch (errorCode)
        {
            case ErrorUnknown:
                errorInfo = "An internal error has occured.";
                break;
            case ErrorTimeout:
                errorInfo = "A command sent to the radio has timed out.";
                break;
            case ErrorNcpInit:
                errorInfo = "NCP Initialization failed.";
                break;
            case ErrorUnrecongnisedPacket:
                errorInfo = "Packet type not recongnized.";
                break;
            case ErrorMultiSync:
                errorInfo = "Multi sync sweeps failed. Timing is to tight.";
                break;
            case ErrorOutOfMemory:
                errorInfo = "Out of memory.";
                break;
            case ErrorNoLock:
                errorInfo = "Unused.";
                break;
            case ErrorNoAntenna:
                errorInfo = "No antenna to perform specified request.";
                break;
            case ErrorNoLights:
                errorInfo = "No lights configure for use in lights command.";
                break;
            case ErrorNoChannel:
                errorInfo = "No channel specified.";
                break;
            case ErrorNoDevice:
                errorInfo = "Unused.";
                break;
            case ErrorRadio:
                errorInfo = "Radio returned error code.";
                break;
        }

        return errorInfo;
    }
}

internal enum ClientConnectionState
{
    TcpConnectionEstablished,
    ReceivedGreeting,
    SentClientConnectionRequest,
    ReceivedAuthenticationRequest,
    SentAuthenticationResponse,
    ReceivedAuthenticationOk,
    ConnectionActive
}

internal static class DataSize
{
    /// <summary>
    ///     以下Size的单位都为4个字节
    /// </summary>
    public const int HeaderSize = 8;

    public const int FooterSize = 2;
    public const int FieldSize = 3;
    public const int IntParamSize = 3;
}

internal enum PacketType
{
    /// <summary>
    ///     DSP control instructions & data
    /// </summary>
    DspControl = 0x43505344,

    /// <summary>
    ///     Node control instructions
    /// </summary>
    Node = 0x45444F4E,

    /// <summary>
    ///     Link administration data(i.e. Keep alives)
    /// </summary>
    Link = 0x4B4E494C,

    /// <summary>
    ///     DSP control background instructions & data
    /// </summary>
    DspLoop = 0x4C505344,

    /// <summary>
    ///     deprecated Removed
    ///     BuildKey('C', 'R', 'F', 'S')
    /// </summary>
    Crfs = 0x53465243,

    /// <summary>
    ///     Node status, Once per second
    /// </summary>
    Status = 0x54415453
}

internal enum ParamType
{
    /// <summary>
    ///     32-bit signed integer value (int32_t)
    /// </summary>
    Int = 0x00,

    /// <summary>
    ///     32-bit un-signed integer value (uint32_t)
    /// </summary>
    UnsignedInt = 0x01,

    /// <summary>
    ///     String (char[])
    /// </summary>
    String = 0x02,

    /// <summary>
    ///     Raw data type
    /// </summary>
    DataRaw = 0x80,

    /// <summary>
    ///     Array of unsigned 8 bit bytes (uint8_t[])
    /// </summary>
    DataUnsigned8 = 0x81,

    /// <summary>
    ///     Array of unsigned 16 bit bytes (uint16_t[])
    /// </summary>
    DataUnsigned16 = 0x82,

    /// <summary>
    ///     Array of unsigned 32 bit bytes (uint32_t[])
    /// </summary>
    DataUnsigned32 = 0x83,

    /// <summary>
    ///     Array of signed 8 bit bytes (int8_t[])
    /// </summary>
    DataSigned8 = 0x84,

    /// <summary>
    ///     Array of signed 16 bit bytes (int16_t[])
    /// </summary>
    DataSigned16 = 0x85,

    /// <summary>
    ///     Array of signed 32 bit bytes (int32_t[])
    /// </summary>
    DataSigned32 = 0x86
}

internal class Packet
{
    private int _nextFieldId;

    /// <summary>
    ///     包头信息
    /// </summary>
    public Header HeaderInfo { get; set; }

    /// <summary>
    ///     内容
    /// </summary>
    public List<Field> ListFieldInfo { get; set; } = new();

    /// <summary>
    ///     包尾信息
    /// </summary>
    public Footer FooterInfo { get; set; }

    /// <summary>
    ///     从接收到的二进制数据中解析数据包
    /// </summary>
    /// <param name="value"></param>
    /// <param name="startIndex"></param>
    public static Packet Parse(byte[] value, int startIndex)
    {
        var packet = new Packet
        {
            HeaderInfo = Header.Parse(value, startIndex)
        };
        var fieldStartPos = DataSize.HeaderSize;
        var fieldEndPos = (int)packet.HeaderInfo.PacketSize - DataSize.FooterSize;
        while (fieldStartPos < fieldEndPos)
        {
            var field = Field.Parse(value, startIndex + (fieldStartPos << 2));
            packet.ListFieldInfo.Add(field);
            fieldStartPos += field.GetSize();
        }

        packet.FooterInfo = new Footer();
        return packet;
    }

    /// <summary>
    ///     获取序列化后的二进制数据包
    /// </summary>
    public byte[] GetBytes()
    {
        var bytes = new List<byte>();
        bytes.AddRange(HeaderInfo.GetBytes());
        foreach (var field in ListFieldInfo) bytes.AddRange(field.GetBytes());
        bytes.AddRange(FooterInfo.GetBytes());
        return bytes.ToArray();
    }

    /// <summary>
    ///     打包包头
    /// </summary>
    /// <param name="packetType"></param>
    /// <param name="packetId"></param>
    public void BeginPacket(PacketType packetType, int packetId)
    {
        HeaderInfo = new Header
        {
            PacketType = (int)packetType
        };
        HeaderInfo.PacketId = packetId == -1 ? ++HeaderInfo.PacketId : packetId;
        //_header.PacketSize = DataSize.HeaderSize; //PacketSize以4字节为单位
        HeaderInfo.PacketTimeSecond = 0; //TODO: 暂时写0
        HeaderInfo.PacketTimeNanosecond = 0;
    }

    /// <summary>
    ///     添加数据
    /// </summary>
    /// <param name="fieldName"></param>
    /// <param name="fieldId"></param>
    public void AddField(int fieldName, int fieldId)
    {
        ListFieldInfo ??= new List<Field>();
        var field = new Field
        {
            FieldName = fieldName,
            Info = 0,
            FieldId = fieldId == -1 ? ++_nextFieldId : fieldId
        };
        ListFieldInfo.Add(field);
    }

    //TODO:暂时只使用该种方式
    //添加请求参数
    public void AddParamInt(int paramName, int data)
    {
        var field = ListFieldInfo.Last();
        field.AddParamInt(paramName, data);
    }

    public void AddParamString(int paramName, string data)
    {
        var field = ListFieldInfo.Last();
        field.AddParamString(paramName, data);
    }

    /// <summary>
    ///     打包包尾
    /// </summary>
    public void EndPacket()
    {
        //在上一个Field中填充下一个Field的相对位置，单位为4字节
        foreach (var field in ListFieldInfo) field.Info = (uint)field.GetSize();
        //添加数据包尾
        FooterInfo = new Footer
        {
            Checksum = 0,
            FooterCode = 0xDDCCBBAA
        };
        //更新数据包长度
        var packetBuffer = GetBytes();
        HeaderInfo.PacketSize = (uint)(packetBuffer.Length / 4);
    }
}

internal class Header
{
    /// <summary>
    ///     Header to allow successful packet sync & decode (0xAABBCCDD)
    /// </summary>
    public uint HeaderCode { get; set; } = 0xAABBCCDD;

    /// <summary>
    ///     Packet type
    /// </summary>
    public int PacketType { get; set; }

    /// <summary>
    ///     Packet size in 32 bit words of the entire packet
    /// </summary>
    public uint PacketSize { get; set; }

    /// <summary>
    ///     User definable identifier word
    /// </summary>
    public int PacketId { get; set; }

    /// <summary>
    ///     Packet format information
    /// </summary>
    public uint PacketFormat { get; set; } = 1;

    /// <summary>
    ///     Packet time, second / nanoseconds
    /// </summary>
    public uint PacketTimeSecond { get; set; }

    public uint PacketTimeNanosecond { get; set; }
    public uint Spare { get; set; }

    public static Header Parse(byte[] value, int startIndex)
    {
        return new Header
        {
            HeaderCode = BitConverter.ToUInt32(value, startIndex),
            PacketType = BitConverter.ToInt32(value, startIndex + 4),
            PacketSize = BitConverter.ToUInt32(value, startIndex + 8),
            PacketId = BitConverter.ToInt32(value, startIndex + 12),
            PacketFormat = BitConverter.ToUInt32(value, startIndex + 16),
            PacketTimeSecond = BitConverter.ToUInt32(value, startIndex + 20),
            PacketTimeNanosecond = BitConverter.ToUInt32(value, startIndex + 24),
            Spare = BitConverter.ToUInt32(value, startIndex + 28)
        };
    }

    public byte[] GetBytes()
    {
        var bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(HeaderCode));
        bytes.AddRange(BitConverter.GetBytes(PacketType));
        bytes.AddRange(BitConverter.GetBytes(PacketSize));
        bytes.AddRange(BitConverter.GetBytes(PacketId));
        bytes.AddRange(BitConverter.GetBytes(PacketFormat));
        bytes.AddRange(BitConverter.GetBytes(PacketTimeSecond));
        bytes.AddRange(BitConverter.GetBytes(PacketTimeNanosecond));
        bytes.AddRange(BitConverter.GetBytes(Spare));
        return bytes.ToArray();
    }
}

internal class Field
{
    //TODO:此成员仅供调试时方便查看
    public string Name { get; set; }

    /// <summary>
    ///     Name key
    /// </summary>
    public int FieldName { get; set; }

    /// <summary>
    ///     Bits 0-23 Next field position, Bits 24-31 data type
    /// </summary>
    public uint Info { get; set; }

    /// <summary>
    ///     User definable identifier word. Returned in responses
    /// </summary>
    public int FieldId { get; set; }

    /// <summary>
    ///     all parameters in current field
    /// </summary>
    public List<Parameter> ListParameter { get; set; } = new();

    /// <summary>
    ///     get the serialized binary data
    /// </summary>
    public byte[] GetBytes()
    {
        var bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(FieldName));
        bytes.AddRange(BitConverter.GetBytes(Info));
        bytes.AddRange(BitConverter.GetBytes(FieldId));
        foreach (var param in ListParameter) bytes.AddRange(param.GetBytes());
        return bytes.ToArray();
    }

    /// <summary>
    ///     Size in 32 bit words of the entire field
    /// </summary>
    public int GetSize()
    {
        var totalSize = DataSize.FieldSize;
        foreach (var param in ListParameter) totalSize += param.GetSize();
        return totalSize;
    }

    /// <summary>
    ///     add parameter
    /// </summary>
    /// <param name="paramName"></param>
    /// <param name="data"></param>
    public void AddParamString(int paramName, string data)
    {
        var bytes = Encoding.ASCII.GetBytes(data);
        Array.Resize(ref bytes, bytes.Length + 1); //需要包含最后一位'\0'
        var param = new Parameter
        {
            ParameterName = paramName
        };
        var len32 = bytes.Length >> 2;
        if ((bytes.Length & 0x03) != 0) len32++;
        param.Info = (uint)(2 + len32 + ((int)ParamType.String << 24));
        if (bytes.Length != len32 << 2) Array.Resize(ref bytes, len32 << 2);
        param.Data = bytes;
        ListParameter.Add(param);
    }

    /// <summary>
    ///     add parameter
    /// </summary>
    /// <param name="paramName"></param>
    /// <param name="data"></param>
    public void AddParamInt(int paramName, int data)
    {
        var param = new Parameter
        {
            ParameterName = paramName,
            Info = DataSize.IntParamSize,
            Data = BitConverter.GetBytes(data)
        };
        ListParameter.Add(param);
    }

    /// <summary>
    ///     parse the field info from the received binary data
    /// </summary>
    /// <param name="value"></param>
    /// <param name="startIndex"></param>
    public static Field Parse(byte[] value, int startIndex)
    {
        try
        {
            var field = new Field
            {
                FieldName = BitConverter.ToInt32(value, startIndex),
                Name = Encoding.ASCII.GetString(value, startIndex, 4), //TODO:此成员仅供调试时方便查看
                Info = BitConverter.ToUInt32(value, startIndex + 4),
                FieldId = BitConverter.ToInt32(value, startIndex + 8)
            };
            var nextFieldOffset = field.Info & 0xFFFFFF; //该值包含了当前的FieldSize
            var paramStartPos = DataSize.FieldSize;
            while (paramStartPos < nextFieldOffset)
            {
                var param = Parameter.Parse(value, startIndex + (paramStartPos << 2));
                field.ListParameter.Add(param);
                paramStartPos += param.GetSize();
            }

            return field;
        }
        catch (ArgumentException e)
        {
#if DEBUG
            Trace.WriteLine(e.ToString());
#endif
            return null;
        }
    }
}

internal class Parameter
{
    //TODO:此成员仅供调试时方便查看
    public string Name { get; set; }

    /// <summary>
    ///     Name key
    /// </summary>
    public int ParameterName { get; set; }

    /// <summary>
    ///     Bits 0-23 Length & Offset to next field position, Bits 24-31 data type
    /// </summary>
    public uint Info { get; set; }

    /// <summary>
    ///     Packet payload of parameter type
    /// </summary>
    public byte[] Data { get; set; }

    /// <summary>
    ///     get the serialized binary data
    /// </summary>
    public byte[] GetBytes()
    {
        var bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(ParameterName));
        bytes.AddRange(BitConverter.GetBytes(Info));
        if (Data.Length > 0) bytes.AddRange(Data);
        return bytes.ToArray();
    }

    /// <summary>
    ///     Size in 32 bit words of the entire parameter
    /// </summary>
    public int GetSize()
    {
        return (int)(Info & 0xFFFFFF);
    }

    /// <summary>
    ///     Get the parameter type
    /// </summary>
    public ParamType GetParamType()
    {
        var type = (int)(Info >> 24);
        return (ParamType)type;
    }

    /// <summary>
    ///     parse the parameter info from the received binary data
    /// </summary>
    /// <param name="value"></param>
    /// <param name="startIndex"></param>
    public static Parameter Parse(byte[] value, int startIndex)
    {
        try
        {
            var param = new Parameter
            {
                ParameterName = BitConverter.ToInt32(value, startIndex),
                Name = Encoding.ASCII.GetString(value, startIndex, 4), //TODO:此成员仅供调试时方便查看
                Info = BitConverter.ToUInt32(value, startIndex + 4)
            };
            var paramLen = (int)(param.Info & 0xFFFFFF); //该值包含了整个Parameter的大小
            param.Data = new byte[(paramLen - 2) * 4];
            Buffer.BlockCopy(value, startIndex + 8, param.Data, 0, param.Data.Length);
            return param;
        }
        catch (ArgumentException e)
        {
#if DEBUG
            Trace.WriteLine(e.ToString());
#endif
            return null;
        }
    }
}

internal class Footer
{
    /// <summary>
    ///     Not used. Always 0
    /// </summary>
    public uint Checksum { get; set; }

    /// <summary>
    ///     Footer to allow successful packet sync & decode (0xDDCCBBAA)
    /// </summary>
    public uint FooterCode { get; set; } = 0xDDCCBBAA;

    /// <summary>
    ///     get the serialized binary data
    /// </summary>
    public byte[] GetBytes()
    {
        var bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(Checksum));
        bytes.AddRange(BitConverter.GetBytes(FooterCode));
        return bytes.ToArray();
    }
}