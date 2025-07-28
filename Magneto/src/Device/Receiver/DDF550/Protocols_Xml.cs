using System;

namespace Magneto.Device.DDF550;

#region Xml枚举

/// <summary>
///     3.1 Enum: eAF_BANDWIDTH 解调带宽
///     Demodulation bandwidth.
/// </summary>
public enum EAfBandWidth : uint
{
    /// <summary>
    ///     100 Hz
    /// </summary>
    Bw0P1 = 100,

    /// <summary>
    ///     150 Hz
    /// </summary>
    Bw0P15 = 150,

    /// <summary>
    ///     300 Hz
    /// </summary>
    Bw0P3 = 300,

    /// <summary>
    ///     600 Hz
    /// </summary>
    Bw0P6 = 600,

    /// <summary>
    ///     1 kHz
    /// </summary>
    Bw1 = 1000,

    /// <summary>
    ///     1.5 kHz
    /// </summary>
    Bw1P5 = 1500,

    /// <summary>
    ///     2.1 kHz
    /// </summary>
    Bw2P1 = 2100,

    /// <summary>
    ///     2.4 kHz
    /// </summary>
    Bw2P4 = 2400,

    /// <summary>
    ///     2.7 kHz
    /// </summary>
    Bw2P7 = 2700,

    /// <summary>
    ///     3.1 kHz
    /// </summary>
    Bw3P1 = 3100,

    /// <summary>
    ///     4 kHz
    /// </summary>
    Bw4 = 4000,

    /// <summary>
    ///     4.8 kHz
    /// </summary>
    Bw4P8 = 4800,

    /// <summary>
    ///     6 kHz
    /// </summary>
    Bw6 = 6000,

    /// <summary>
    ///     8.333 kHz
    /// </summary>
    Bw8P333 = 8333,

    /// <summary>
    ///     9 kHz
    /// </summary>
    Bw9 = 9000,

    /// <summary>
    ///     12 kHz
    /// </summary>
    Bw12 = 12000,

    /// <summary>
    ///     15 kHz
    /// </summary>
    Bw15 = 15000,

    /// <summary>
    ///     25 kHz
    /// </summary>
    Bw25 = 25000,

    /// <summary>
    ///     30 kHz
    /// </summary>
    Bw30 = 30000,

    /// <summary>
    ///     50 kHz
    /// </summary>
    Bw50 = 50000,

    /// <summary>
    ///     75 kHz
    /// </summary>
    Bw75 = 75000,

    /// <summary>
    ///     120 kHz
    /// </summary>
    Bw120 = 120000,

    /// <summary>
    ///     150 kHz
    /// </summary>
    Bw150 = 150000,

    /// <summary>
    ///     250 kHz
    /// </summary>
    Bw250 = 250000,

    /// <summary>
    ///     300 kHz
    /// </summary>
    Bw300 = 300000,

    /// <summary>
    ///     500 kHz
    /// </summary>
    Bw500 = 500000,

    /// <summary>
    ///     800 kHz
    /// </summary>
    Bw800 = 800000,

    /// <summary>
    ///     1 MHz
    /// </summary>
    Bw1000 = 1000000,

    /// <summary>
    ///     1.25 MHz
    /// </summary>
    Bw1250 = 1250000,

    /// <summary>
    ///     1.5 MHz
    /// </summary>
    Bw1500 = 1500000,

    /// <summary>
    ///     2 MHz
    /// </summary>
    Bw2000 = 2000000,

    /// <summary>
    ///     5 MHz
    /// </summary>
    Bw5000 = 5000000,

    /// <summary>
    ///     8 MHz
    /// </summary>
    Bw8000 = 8000000,

    /// <summary>
    ///     10 MHz
    /// </summary>
    Bw10000 = 10000000,

    /// <summary>
    ///     12.5 MHz
    /// </summary>
    Bw12500 = 12500000,

    /// <summary>
    ///     15 MHz
    /// </summary>
    Bw15000 = 15000000,

    /// <summary>
    ///     20 MHz
    /// </summary>
    Bw20000 = 20000000
}

/// <summary>
///     3.2 Enum: eAF_FILTER_MODE 音频过滤模式
///     Audio filter mode.
/// </summary>
public enum EAfFilterMode
{
    /// <summary>
    ///     Off: no filter function active.
    /// </summary>
    AfFilterOff,

    /// <summary>
    ///     Notch filter: automatic elimination of interference signals.
    /// </summary>
    AfFilterNotch,

    /// <summary>
    ///     Noise reduction filter.
    /// </summary>
    AfFilterNr,

    /// <summary>
    ///     Bandpass filter 300 Hz to 3.3 kHz (telephone channel).
    /// </summary>
    AfFilterBp,

    /// <summary>
    ///     High deemphasis (time constant 25 µs).
    /// </summary>
    AfFilterDeemphasis25,

    /// <summary>
    ///     European FM radio deemphasis (time constant 50 µs).
    /// </summary>
    AfFilterDeemphasis50,

    /// <summary>
    ///     USA FM radio deemphasis (time constant 75 µs).
    /// </summary>
    AfFilterDeemphasis75,

    /// <summary>
    ///     FM Radio-telephone deemphasis (time constant 750 µs).
    /// </summary>
    AfFilterDeemphasis750
}

/// <summary>
///     3.3 Enum: eANALOG_VIDEO_OUTPUT 输出IF (X61)和视频(X62)的模拟信号类型。
///     Type of analog signal at outputs IF (X61) and Video (X62).
/// </summary>
public enum EAnalogVideoOutput
{
    /// <summary>
    ///     IF signal: in-phase component on X61, quadrature component on X62.
    /// </summary>
    VideoOutputIf,

    /// <summary>
    ///     Demodulated signal in negative form: AM on X61, FM on X62.
    /// </summary>
    VideoOutputVideoNeg,

    /// <summary>
    ///     Demodulated signal in positive form: AM on X61, FM on X62.
    /// </summary>
    VideoOutputVideoPos
}

/// <summary>
///     3.4 Enum: eANTLEVELSWITCH 天线测试运行:输出电平指示值或不输出电平指示值，天线测试散热器开或关
///     Test operation of antenna: output of level indications or not, antenna test radiator on or off.
/// </summary>
public enum EAntLevelSwitch
{
    /// <summary>
    ///     Normal DF or Rx operation: no output of antenna level indications.
    /// </summary>
    AntlevelOff,

    /// <summary>
    ///     Antenna radiator test operation: output of level indications, antenna test radiator not available or switched off
    ///     (test signal from outside).
    /// </summary>
    AntlevelOnEmitterOff,

    /// <summary>
    ///     Ditto, test radiator on and emitting test signal.
    /// </summary>
    AntlevelOnEmitterOn
}

/// <summary>
///     3.5 Enum: eANT_CTRL_MODE 天线控制模式
/// </summary>
public enum EAntCtrlMode
{
    /// <summary>
    ///     Manual.
    /// </summary>
    AntCtrlModeManual,

    /// <summary>
    ///     Auto.
    /// </summary>
    AntCtrlModeAuto
}

/// <summary>
///     3.6 Enum: eANT_POL 天线极化方式
///     Polarization type of antenna.
/// </summary>
public enum EAntPol
{
    /// <summary>
    ///     垂直极化
    ///     Linear, vertical.
    /// </summary>
    PolVertical,

    /// <summary>
    ///     水平极化
    ///     Linear, horizontal.
    /// </summary>
    PolHorizontal,

    /// <summary>
    ///     圆极化逆时针
    ///     Circular, left-hand (counter-clockwise).
    /// </summary>
    PolLeft,

    /// <summary>
    ///     圆极化顺时针
    ///     Circular, right-hand (clockwise).
    /// </summary>
    PolRight
}

/// <summary>
///     3.7 Enum: eATT_SELECT 衰减模式
///     Selection mode for attenuator.
/// </summary>
public enum EAttSelect
{
    /// <summary>
    ///     Automatic.
    /// </summary>
    AttAuto,

    /// <summary>
    ///     Manual.
    /// </summary>
    AttManual
}

/// <summary>
///     3.8 Enum: eAUDIO_MODE 音频格式
///     All available Audio modes (data formats of issued audio data).
/// </summary>
public enum EAudioMode
{
    /// <summary>
    ///     Audio mode 0: no useful data available (signal level below threshold).
    /// </summary>
    AudioModeOff,

    /// <summary>
    ///     Mode 1: sampling rate 32 kHz, length of samples 16 bit, 2 channels.
    /// </summary>
    AudioMode32Khz16BitStereo,

    /// <summary>
    ///     Mode 2: rate 32 kHz, length 16 bit, 1 channel.
    /// </summary>
    AudioMode32Khz16BitMono,

    /// <summary>
    ///     Mode 3: rate 32 kHz, length 8 bit, 2 channels.
    /// </summary>
    AudioMode32Khz8BitStereo,

    /// <summary>
    ///     Mode 4: rate 32 kHz, length 8 bit, 1 channel.
    /// </summary>
    AudioMode32Khz8BitMono,

    /// <summary>
    ///     Mode 5: rate 16 kHz, length 16 bit, 2 channels.
    /// </summary>
    AudioMode16Khz16BitStereo,

    /// <summary>
    ///     Mode 6: rate 16 kHz, length 16 bit, 1 channel.
    /// </summary>
    AudioMode16Khz16BitMono,

    /// <summary>
    ///     Mode 7: rate 16 kHz, length 8 bit, 2 channels.
    /// </summary>
    AudioMode16Khz8BitStereo,

    /// <summary>
    ///     Mode 8: rate 16 kHz, length 8 bit, 1 channel.
    /// </summary>
    AudioMode16Khz8BitMono,

    /// <summary>
    ///     Mode 9: rate 8 kHz, length 16 bit, 2 channels.
    /// </summary>
    AudioMode8Khz16BitStereo,

    /// <summary>
    ///     Mode 10: rate 8 kHz, length 16 bit, 1 channel.
    /// </summary>
    AudioMode8Khz16BitMono,

    /// <summary>
    ///     Mode 11: rate 8 kHz, length 8 bit, 2 channels.
    /// </summary>
    AudioMode8Khz8BitStereo,

    /// <summary>
    ///     Mode 12: rate 8 kHz, length 8 bit, 1 channel.
    /// </summary>
    AudioMode8Khz8BitMono
}

/// <summary>
///     3.9 Enum: eAUX_CTRL_MODE AUX控制模式
///     AUX control mode.
/// </summary>
public enum EAuxCtrlMode
{
    /// <summary>
    ///     MANUAL  Manual mode: output value specified with command.
    /// </summary>
    AuxCtrlModeManual,

    /// <summary>
    ///     FREQ Frequency mode: output current Rx frequency value.
    /// </summary>
    AuxCtrlModeFreq,

    /// <summary>
    ///     ANTENNA Antenna mode: output value of "AntennaSetup".
    /// </summary>
    AuxCtrlModeAntenna
}

/// <summary>
///     3.10 Enum: eAVERAGE_MODE 取平均值模式?
///     Averaging mode.
/// </summary>
public enum EAverageMode
{
    /// <summary>
    ///     CONT    Continuous Mode: no signal level threshold exists.
    /// </summary>
    DfsquOff,

    /// <summary>
    ///     GATE    Gated Mode: averaging resumed after "inactive" period.
    /// </summary>
    DfsquGate,

    /// <summary>
    ///     NORM    Normal Mode: averaging restarts after "inactive" period.
    /// </summary>
    DfsquNorm
}

/// <summary>
///     3.11 Enum: eBLANKING_INPUT 消隐信号输入?(视频使用)
///     Blanking input.
/// </summary>
public enum EBlankingInput
{
    /// <summary>
    ///     Blanking signal taken from Auxiliary input (connector X17 AUX, pin 9 BLANKING) (also default value).
    /// </summary>
    BlankingInputAux,

    /// <summary>
    ///     Blanking signal taken from Trigger input (connector X44 TRIGGER).
    /// </summary>
    BlankingInputTrigger
}

/// <summary>
///     3.12 Enum: eBLANKING_MODE 消隐模式
///     Blanking mode.
/// </summary>
public enum EBlankingMode
{
    /// <summary>
    ///     Blanking signal (input) is ignored.
    /// </summary>
    BlankingModeOff,

    /// <summary>
    ///     During averaging phase, with blanking signal active antenna signals are ignored (no contribution to averaging)
    ///     (also default value).
    /// </summary>
    BlankingModeSuspend,

    /// <summary>
    ///     During averaging phase, with blanking signal active only status bit in DFPScan:DF status is set, but DF results and
    ///     level generated (averaging continued).
    /// </summary>
    BlankingModeStatus
}

/// <summary>
///     3.13 Enum: eBLANKING_POLARITY 消隐信号的极性
///     Polarity of Blanking signal.
/// </summary>
public enum EBlankingPolarity
{
    /// <summary>
    ///     Blanking signal is active low (also default value).
    /// </summary>
    BlankingPolarityLow,

    /// <summary>
    ///     Blanking signal is active high.
    /// </summary>
    BlankingPolarityHigh
}

/// <summary>
///     3.14 Enum: eBLOCK_AVERAGING_SELECT 平均值取值模式
///     Indication type for duration of averaging block.
/// </summary>
public enum EBlockAveragingSelect
{
    /// <summary>
    ///     按次数取值
    ///     Number of cycles.
    /// </summary>
    BlockAveragingSelectCycles,

    /// <summary>
    ///     按时间(ms)取值
    ///     Time duration [ms].
    /// </summary>
    BlockAveragingSelectTime
}

/// <summary>
///     3.15 Enum: eCALMOD 校准发电机的调制。
///     Modulation of calibration generator.
/// </summary>
public enum ECalMod
{
    /// <summary>
    ///     Calibration generator off.
    /// </summary>
    CalmodCwoff,

    /// <summary>
    ///     CW (continuous wave: no modulation).
    /// </summary>
    CalmodCwon,

    /// <summary>
    ///     Comb spectrum 10 kHz, 10 spectral lines.
    /// </summary>
    CalmodF10K10L,

    /// <summary>
    ///     1 kHz, 100 lines.
    /// </summary>
    CalmodF1K100L,

    /// <summary>
    ///     20 kHz, 10 lines.
    /// </summary>
    CalmodF20K10L,

    /// <summary>
    ///     2 kHz, 100 lines.
    /// </summary>
    CalmodF2K100L,

    /// <summary>
    ///     50 kHz, 10 lines.
    /// </summary>
    CalmodF50K10L,

    /// <summary>
    ///     5 kHz, 100 lines.
    /// </summary>
    CalmodF5K100L,

    /// <summary>
    ///     100 kHz, 10 lines.
    /// </summary>
    CalmodF100K10L,

    /// <summary>
    ///     10 kHz, 100 lines.
    /// </summary>
    CalmodF10K100L,

    /// <summary>
    ///     200 kHz, 10 lines.
    /// </summary>
    CalmodF200K10L,

    /// <summary>
    ///     20 kHz, 100 lines.
    /// </summary>
    CalmodF20K100L,

    /// <summary>
    ///     500 kHz, 10 lines.
    /// </summary>
    CalmodF500K10L,

    /// <summary>
    ///     50 kHz, 100 lines.
    /// </summary>
    CalmodF50K100L,

    /// <summary>
    ///     1 MHz, 10 lines.
    /// </summary>
    CalmodF1M10L,

    /// <summary>
    ///     100 kHz, 100 lines.
    /// </summary>
    CalmodF100K100L,

    /// <summary>
    ///     2 MHz, 10 lines.
    /// </summary>
    CalmodF2M10L,

    /// <summary>
    ///     200 kHz, 100 lines.
    /// </summary>
    CalmodF200K100L,

    /// <summary>
    ///     4 MHz, 10 lines.
    /// </summary>
    CalmodF4M10L,

    /// <summary>
    ///     400 kHz, 100 lines.
    /// </summary>
    CalmodF400K100L,

    /// <summary>
    ///     800 kHz, 100 lines.
    /// </summary>
    CalmodF800K100L,

    /// <summary>
    ///     10 MHz, 10 lines.
    /// </summary>
    CalmodF10M10L,

    /// <summary>
    ///     1 MHz, 100 lines.
    /// </summary>
    CalmodF1M100L,

    /// <summary>
    /// </summary>
    CalmodExt
}

/// <summary>
///     3.16 Enum: eCALOUT 用于校准信号的输出连接器。
///     Output connector for calibration signal.
///     NOTE 1: In case of connected antenna, input connector(s) for antenna signal(s) and output connector for calibration
///     signal must be in same connector group.
///     NOTE 2: With DDF1GTX, both calibration outputs are equivalent, but selection must be done with command
///     CalibrationGenerator.
///     NOTE 3: Mind that some connectors for HF range are available only if the corresponding option is installed:
/// </summary>
public enum ECalOut
{
    /// <summary>
    ///     No signal output.
    /// </summary>
    CaloutNone,

    /// <summary>
    ///     Calibration HF (see above notes 2 and 3).
    /// </summary>
    CaloutHf,

    /// <summary>
    ///     Calibration V/UHF.
    /// </summary>
    CaloutVuhf,

    /// <summary>
    ///     Calibration U/SHF.
    /// </summary>
    CaloutUshf
}

/// <summary>
///     3.17 Enum: eCAL_SWITCH 天线信号输入开关
///     Position of antenna signal switch.
/// </summary>
public enum ECalSwitch
{
    /// <summary>
    ///     Get input signal from antenna elements (normal operation).
    /// </summary>
    AntcalReceive,

    /// <summary>
    ///     Get input signal from calibration generator (perform calibration).
    /// </summary>
    AntcalCalibrate
}

/// <summary>
///     3.18 Enum: eCLOCK_ORIGIN 设置时间同步的模式
///     Origin of timing the data clock has been set to on latest setting.
/// </summary>
public enum EClockOrigin
{
    /// <summary>
    ///     通过命令[DateAndTime]手动更新时间
    ///     Timing data set manually (command DateAndTime).
    /// </summary>
    ClockOriginManual,

    /// <summary>
    ///     通过自身的时钟更新时间
    ///     Timing data set from internal RTC (Real Time Clock, battery-buffered, setting only with power-up).
    /// </summary>
    ClockOriginBackup,

    /// <summary>
    ///     通过接收GPS信息更新时间
    ///     Timing data set from external GPS receiver (or option R/&/S DDFx-IGT, Integrated GPS Module) to maintain continuity
    ///     of clock time in periods of power-down, (command LocationAndTimeSource:eLocTimeSource=LOC_TIME_SRC_GPS).
    /// </summary>
    ClockOriginGps,

    /// <summary>
    ///     通过网络更新时间
    ///     Timing data set from external NTP (Network Time Protocol) server (command NTP).
    /// </summary>
    ClockOriginNtp
}

/// <summary>
///     3.19 Enum: eCLOCK_START 系统时钟的启动模式
///     Mode of system clock start.
/// </summary>
public enum EClockStart
{
    /// <summary>
    ///     Clock is started immediately upon setting (any pulse at PPS input ignored).
    /// </summary>
    ClockStartAuto,

    /// <summary>
    ///     Clock is only started on pulse detected at PPS input (clock remains halted if such pulse missing).
    /// </summary>
    ClockStartExternal
}

/// <summary>
///     3.20 Enum: eCOMPASS_CODE 所有可能的罗盘类型。
///     All possible compass types.
/// </summary>
public enum ECompassCode
{
    /// <summary>
    ///     No known type of compass.
    /// </summary>
    CompassUndefined,

    /// <summary>
    ///     User defined type.
    /// </summary>
    CompassUser,

    /// <summary>
    ///     R/&/S GH150 antenna compass.
    /// </summary>
    CompassGh150,

    /// <summary>
    ///     NMEA compass (e.g. vehicle compass).
    /// </summary>
    CompassNmea,

    /// <summary>
    ///     Software compass (command SwCompassHeading).
    /// </summary>
    CompassSw,

    /// <summary>
    ///     GPS compass.
    /// </summary>
    CompassGps,

    /// <summary>
    ///     Compass values received via UDP.
    /// </summary>
    CompassUdpNmea
}

/// <summary>
///     3.21 Enum: eDDCE_BW DDCE带宽
///     DDCE (DDC/Digital Down Converter Signal Extraction) bandwidth.
/// </summary>
public enum EddceBw
{
    /// <summary>
    ///     100 Hz	100 Hz.
    /// </summary>
    DdceBw100,

    /// <summary>
    ///     150 Hz	150 Hz.
    /// </summary>
    DdceBw150,

    /// <summary>
    ///     300 Hz	300 Hz.
    /// </summary>
    DdceBw300,

    /// <summary>
    ///     600 Hz	600 Hz.
    /// </summary>
    DdceBw600,

    /// <summary>
    ///     1 kHz	1 kHz.
    /// </summary>
    DdceBw1000,

    /// <summary>
    ///     1.5 kHz	1.5 kHz.
    /// </summary>
    DdceBw1500,

    /// <summary>
    ///     2.1 kHz	2.1 kHz.
    /// </summary>
    DdceBw2100,

    /// <summary>
    ///     2.4 kHz	2.4 kHz.
    /// </summary>
    DdceBw2400,

    /// <summary>
    ///     2.7 kHz	2.7 kHz.
    /// </summary>
    DdceBw2700,

    /// <summary>
    ///     3.1 kHz	3.1 kHz.
    /// </summary>
    DdceBw3100,

    /// <summary>
    ///     4 kHz	4 kHz.
    /// </summary>
    DdceBw4000,

    /// <summary>
    ///     4.8 kHz	4.8 kHz.
    /// </summary>
    DdceBw4800,

    /// <summary>
    ///     6 kHz	6 kHz.
    /// </summary>
    DdceBw6000,

    /// <summary>
    ///     9 kHz	9 kHz.
    /// </summary>
    DdceBw9000,

    /// <summary>
    ///     12 kHz	12 kHz.
    /// </summary>
    DdceBw12000,

    /// <summary>
    ///     15 kHz	15 kHz.
    /// </summary>
    DdceBw15000,

    /// <summary>
    ///     30 kHz	30 kHz.
    /// </summary>
    DdceBw30000,

    /// <summary>
    ///     50 kHz	50 kHz.
    /// </summary>
    DdceBw50000,

    /// <summary>
    ///     120 kHz	120 kHz.
    /// </summary>
    DdceBw120000,

    /// <summary>
    ///     150 kHz	150 kHz.
    /// </summary>
    DdceBw150000,

    /// <summary>
    ///     250 kHz	250 kHz.
    /// </summary>
    DdceBw250000,

    /// <summary>
    ///     300 kHz	300 kHz.
    /// </summary>
    DdceBw300000
}

/// <summary>
///     3.22 Enum: eDDCE_REMOTE_MODE DDCE远程模式
///     DDCE (DDC/Digital Down Converter Signal Extraction) remote mode.
/// </summary>
public enum EddceRemoteMode
{
    /// <summary>
    ///     Stop transfer via remote interface.
    /// </summary>
    DdceRemoteModeOff,

    /// <summary>
    ///     Digital IF, AMMOS format 16 bit I and 16 bit Q
    /// </summary>
    DdceRemoteModeShort,

    /// <summary>
    ///     Digital IF, AMMOS format 32 bit I and 32 bit Q
    /// </summary>
    DdceRemoteModeLong
}

/// <summary>
///     3.23 Enum: eDDCE_STATE DDCE状态
///     DDCE (DDC/Digital Down Converter Signal Extraction) state.
/// </summary>
public enum EddceState
{
    /// <summary>
    ///     Deactivate selected DDCE or remove binding to Short-Time Synthesizer.
    /// </summary>
    DdceStateOff,

    /// <summary>
    ///     Activate selected DDCE.
    /// </summary>
    DdceStateOn,

    /// <summary>
    ///     Bind selected DDCE to Short-Time Synthesizer.
    /// </summary>
    DdceStateStif
}

/// <summary>
///     3.24 Enum: eDECLINATION_SOURCE 磁北与真北的偏差模式
///     Source of indication for declination (deviation of magnetic north vs. true [geographic] north; m. N. assumed to the
///     right of g. N.).
/// </summary>
public enum EDeclinationSource
{
    /// <summary>
    ///     No declination value incorporated.
    /// </summary>
    DeclSourNo,

    /// <summary>
    ///     Declination indication has been specified manually; entered value used.
    /// </summary>
    DeclSourMan,

    /// <summary>
    ///     Declination indication from GPS receiver; manual value not used (but kept until next evocation of manual mode).
    /// </summary>
    DeclSourGps
}

/// <summary>
///     3.25 Enum: eDEMODULATION 解调模式
///     Demodulation mode.
/// </summary>
public enum EDemodulation
{
    /// <summary>
    ///     FM (frequency modulation).
    /// </summary>
    ModFm = 1,

    /// <summary>
    ///     AM (amplitude modulation).
    /// </summary>
    ModAm = 2,

    /// <summary>
    ///     Pulse.
    /// </summary>
    ModPuls = 11,

    /// <summary>
    ///     PM (pulse modulation).
    /// </summary>
    ModPm = 3,

    /// <summary>
    ///     I/Q (in-phase and quadrature).
    /// </summary>
    ModIq = 4,

    /// <summary>
    ///     ISB (independent side band).
    /// </summary>
    ModIsb = 9,

    /// <summary>
    ///     CW (continuous wave).
    /// </summary>
    ModCw = 5,

    /// <summary>
    ///     USB (upper side band).
    /// </summary>
    ModUsb = 7,

    /// <summary>
    ///     LSB (lower side band).
    /// </summary>
    ModLsb = 6,

    /// <summary>
    ///     TV (television).
    /// </summary>
    ModTv = 111
}

/// <summary>
///     3.26 Enum: eDEVICE_INFO 当前连接的DDF单元的设备类型
///     Type of device parameters of currently connected DDF unit.
/// </summary>
public enum EDeviceInfo
{
    /// <summary>
    ///     Minimum and maximum frequency of DDF.
    /// </summary>
    DevInfoFrequencyMinMax,

    /// <summary>
    ///     Minimum and maximum frequency of HF limit.
    /// </summary>
    DevInfoHfLimitMinMax,

    /// <summary>
    ///     Minimum and maximum channel count for normal DF.
    /// </summary>
    DevInfoDfChannelcountMinMax,

    /// <summary>
    ///     Minimum and maximum channel count for super-resolution DF.
    /// </summary>
    DevInfoSrChannelcountMinMax,

    /// <summary>
    ///     List of all supported spans.
    /// </summary>
    DevInfoSpanList,

    /// <summary>
    ///     List of all supported spans for DDCE option.
    /// </summary>
    DevInfoDdceSpanList,

    /// <summary>
    ///     List of all supported spans for HRP option.
    /// </summary>
    DevInfoHrpSpanList,

    /// <summary>
    ///     List of all supported spans for ST option.
    /// </summary>
    DevInfoStSpanList,

    /// <summary>
    ///     List of all supported DFPan steps.
    /// </summary>
    DevInfoDfpanStepList,

    /// <summary>
    ///     List of all supported IFPan steps.
    /// </summary>
    DevInfoIfpanStepList,

    /// <summary>
    ///     List of connectors for calibration signal (output) (eCALOUT).
    /// </summary>
    DevInfoCaloutList,

    /// <summary>
    ///     List of connectors for V/U/SHF signal (input) (eRF_INPUT).
    /// </summary>
    DevInfoRfInputList,

    /// <summary>
    ///     List of connectors for HF signal (input) (eHF_INPUT).
    /// </summary>
    DevInfoHfInputList,

    /// <summary>
    ///     List of all possible DF paths for Rx antenna VHF/UHF.
    /// </summary>
    DevInfoRfPathList,

    /// <summary>
    ///     List of all possible DF paths for Rx antenna HF.
    /// </summary>
    DevInfoHfPathList
}

/// <summary>
///     3.27 Enum: eDFMODE 测向模式
///     DF Mode.
/// </summary>
public enum EDfMode
{
    /// <summary>
    ///     FFM (Fixed Frequency Mode).
    /// </summary>
    DfmodeFfm,

    /// <summary>
    ///     Scan.
    /// </summary>
    DfmodeScan,

    /// <summary>
    ///     Search.
    /// </summary>
    DfmodeSearch,

    /// <summary>
    ///     Rx (Receive only, no DF).
    /// </summary>
    DfmodeRx,

    /// <summary>
    ///     Rx Panorama Scan.
    /// </summary>
    DfmodeRxpscan
}

/// <summary>
///     3.28 Enum: eDFPAN_SELECTIVITY
///     Selectivity of DF panorama.
/// </summary>
public enum EDfPanSelectivity
{
    /// <summary>
    ///     Select automatically.
    /// </summary>
    DfpanSelectivityAuto,

    /// <summary>
    ///     Normal (factor 1, i.e. no prolongation).
    /// </summary>
    DfpanSelectivityNormal,

    /// <summary>
    ///     Narrow (factor 2).
    /// </summary>
    DfpanSelectivityNarrow,

    /// <summary>
    ///     Sharp (factor 4).
    /// </summary>
    DfpanSelectivitySharp
}

/// <summary>
///     3.29 Enum: eDFPAN_STEP 测向信道带宽(0.01Hz
///     DF channel spacing.
/// </summary>
public enum EDfPanStep : ulong
{
    /// <summary>
    ///     12.5 Hz	12.5 Hz.
    /// </summary>
    DfpanStep12P5Hz = 1250,

    /// <summary>
    ///     20 Hz	20 Hz.
    /// </summary>
    DfpanStep20Hz = 2000,

    /// <summary>
    ///     25 Hz	25 Hz.
    /// </summary>
    DfpanStep25Hz = 2500,

    /// <summary>
    ///     31.25 Hz	31.25 Hz.
    /// </summary>
    DfpanStep31P25Hz = 3125,

    /// <summary>
    ///     50 Hz	50 Hz.
    /// </summary>
    DfpanStep50Hz = 5000,

    /// <summary>
    ///     62.5 Hz	62.5 Hz.
    /// </summary>
    DfpanStep62P5Hz = 6250,

    /// <summary>
    ///     100 Hz	100 Hz.
    /// </summary>
    DfpanStep100Hz = 10000,

    /// <summary>
    ///     125 Hz	125 Hz.
    /// </summary>
    DfpanStep125Hz = 12500,

    /// <summary>
    ///     200 Hz	200 Hz.
    /// </summary>
    DfpanStep200Hz = 20000,

    /// <summary>
    ///     250 Hz	250 Hz.
    /// </summary>
    DfpanStep250Hz = 25000,

    /// <summary>
    ///     312.5 Hz	312.5 Hz.
    /// </summary>
    DfpanStep312P5Hz = 31250,

    /// <summary>
    ///     500 Hz	500 Hz.
    /// </summary>
    DfpanStep500Hz = 50000,

    /// <summary>
    ///     625 Hz	625 Hz.
    /// </summary>
    DfpanStep625Hz = 62500,

    /// <summary>
    ///     1 kHz	1 kHz.
    /// </summary>
    DfpanStep1Khz = 100000,

    /// <summary>
    ///     1.25 kHz	1.25 kHz.
    /// </summary>
    DfpanStep1P25Khz = 125000,

    /// <summary>
    ///     2 kHz	2 kHz.
    /// </summary>
    DfpanStep2Khz = 200000,

    /// <summary>
    ///     2.5 kHz	2.5 kHz.
    /// </summary>
    DfpanStep2P5Khz = 250000,

    /// <summary>
    ///     3.125 kHz	3.125 kHz.
    /// </summary>
    DfpanStep3P125Khz = 312500,

    /// <summary>
    ///     5 kHz	5 kHz.
    /// </summary>
    DfpanStep5Khz = 500000,

    /// <summary>
    ///     6.25 kHz	6.25 kHz.
    /// </summary>
    DfpanStep6P25Khz = 625000,

    /// <summary>
    ///     8.333 kHz	8.333 kHz.
    /// </summary>
    DfpanStep8P333Khz = 833300,

    /// <summary>
    ///     10 kHz	10 kHz.
    /// </summary>
    DfpanStep10Khz = 1000000,

    /// <summary>
    ///     12.5 kHz	12.5 kHz.
    /// </summary>
    DfpanStep12P5Khz = 1250000,

    /// <summary>
    ///     20 kHz	20 kHz.
    /// </summary>
    DfpanStep20Khz = 2000000,

    /// <summary>
    ///     25 kHz	25 kHz.
    /// </summary>
    DfpanStep25Khz = 2500000,

    /// <summary>
    ///     50 kHz	50 kHz.
    /// </summary>
    DfpanStep50Khz = 5000000,

    /// <summary>
    ///     100 kHz	100 kHz.
    /// </summary>
    DfpanStep100Khz = 10000000,

    /// <summary>
    ///     200 kHz	200 kHz.
    /// </summary>
    DfpanStep200Khz = 20000000,

    /// <summary>
    ///     500 kHz	500 kHz.
    /// </summary>
    DfpanStep500Khz = 50000000,

    /// <summary>
    ///     1 MHz	1 MHz.
    /// </summary>
    DfpanStep1000Khz = 100000000,

    /// <summary>
    ///     2 MHz	2 MHz.
    /// </summary>
    DfpanStep2000Khz = 200000000
}

/// <summary>
///     3.30 Enum: eDF_ALT 测向体制?不确定
///     DF evaluation principle.
/// </summary>
public enum EDfAlt
{
    /// <summary>
    ///     Select automatically according to antenna type.
    /// </summary>
    DfaltAuto,

    /// <summary>
    ///     Watson-Watt.
    /// </summary>
    DfaltWatsonwatt,

    /// <summary>
    ///     Correlation.
    /// </summary>
    DfaltCorrelation,

    /// <summary>
    ///     Super-resolution.
    /// </summary>
    DfaltSuperresolution,

    /// <summary>
    ///     Vector matching.
    /// </summary>
    DfaltVectormatching
}

/// <summary>
///     3.31 Enum: eDF_METHOD
/// </summary>
public enum EDfMethod
{
    /// <summary>
    ///     Watson-Watt.
    /// </summary>
    DfWw,

    /// <summary>
    ///     Correlation, 5 antenna elements.
    /// </summary>
    DfCor5,

    /// <summary>
    ///     Correlation, 6 elements.
    /// </summary>
    DfCor6,

    /// <summary>
    ///     Correlation, 8 elements.
    /// </summary>
    DfCor8,

    /// <summary>
    ///     Correlation, 8 elements, omniphase correction.
    /// </summary>
    DfCor8Omni,

    /// <summary>
    ///     Correlation, 9 elements.
    /// </summary>
    DfCor9,

    /// <summary>
    ///     Correlation, 9 elements as one antenna base.
    /// </summary>
    DfCor9OneBase,

    /// <summary>
    ///     Super-resolution, 5 elements.
    /// </summary>
    DfSr5,

    /// <summary>
    ///     Super-resolution, 8 elements.
    /// </summary>
    DfSr8,

    /// <summary>
    ///     Super-resolution, 9 elements.
    /// </summary>
    DfSr9,

    /// <summary>
    ///     Super-resolution, 9 elements, antenna ADD011SR.
    /// </summary>
    DfSr9011Sr,

    /// <summary>
    ///     Vector matching, 2 elements.
    /// </summary>
    DfVm2,

    /// <summary>
    ///     Vector matching, 6 elements.
    /// </summary>
    DfVm6,

    /// <summary>
    ///     Vector matching, 6 antenna sectors.
    /// </summary>
    DfVm6Sector,

    /// <summary>
    ///     Vector matching, 8 elements.
    /// </summary>
    DfVm8,

    /// <summary>
    ///     Vector matching, 9 elements.
    /// </summary>
    DfVm9
}

/// <summary>
///     3.32 Enum: eDIRECTION 精度和维度的方向?
///     Direction of geographical latitude and longitude
/// </summary>
public enum EDirection
{
    /// <summary>
    ///     North(latitude).
    /// </summary>
    DirectionNorth,

    /// <summary>
    ///     East (longitude).
    /// </summary>
    DirectionEast,

    /// <summary>
    ///     South (latitude).
    /// </summary>
    DirectionSouth,

    /// <summary>
    ///     West (longitude).
    /// </summary>
    DirectionWest
}

/// <summary>
///     3.33 Enum: eDISPLAY_VARIANTS VDPan(视频全景)数据跟踪状态和显示变量。
///     VDPan (video panorama) data trace state and display variants.
/// </summary>
public enum EDisplayVariants
{
    /// <summary>
    ///     VDPan data trace off (no demodulated data).
    /// </summary>
    DisplayVariantsOff,

    /// <summary>
    ///     IF panorama (level) data.
    /// </summary>
    DisplayVariantsIfPan,

    /// <summary>
    ///     Demodulated data: AM.
    /// </summary>
    DisplayVariantsVideoPanAm,

    /// <summary>
    ///     Demodulated data: FM.
    /// </summary>
    DisplayVariantsVideoPanFm,

    /// <summary>
    ///     Demodulated data: I/Q.
    /// </summary>
    DisplayVariantsVideoPanIq,

    /// <summary>
    ///     Demodulated data, squared: AM.
    /// </summary>
    DisplayVariantsVideoPanAmSquare,

    /// <summary>
    ///     Demodulated data, squared: FM.
    /// </summary>
    DisplayVariantsVideoPanFmSquare,

    /// <summary>
    ///     Demodulated data, squared: I/Q.
    /// </summary>
    DisplayVariantsVideoPanIqSquare
}

/// <summary>
///     3.34 Enum: eFPGA FPGA的ID
///     ID of FPGA.
/// </summary>
public enum Efpga
{
    /// <summary>
    ///     FPGA A (also default).
    /// </summary>
    FpgaA,

    /// <summary>
    ///     FPGA B.
    /// </summary>
    FpgaB,

    /// <summary>
    ///     FPGA C.
    /// </summary>
    FpgaC,

    /// <summary>
    ///     FPGA D.
    /// </summary>
    FpgaD
}

/// <summary>
///     3.35 Enum: eGAIN_CONTROL 增益模式
///     Mode of gain control for demodulation path.
/// </summary>
public enum EGainControl
{
    /// <summary>
    ///     AGC (automatic gain control).
    /// </summary>
    GainAuto,

    /// <summary>
    ///     MGC (manual gain control).
    /// </summary>
    GainManual
}

/// <summary>
///     3.36 Enum: eGAIN_TIMING
///     AGC gain timing characteristics.
/// </summary>
public enum EGainTiming
{
    /// <summary>
    ///     Fast.
    /// </summary>
    GcFast,

    /// <summary>
    ///     Default.
    /// </summary>
    GcDefault,

    /// <summary>
    ///     Slow.
    /// </summary>
    GcSlow
}

/// <summary>
///     3.37 Enum: eGPS_ANT_STATUS GPS天线状态
///     GPS antenna (error) status of option R/&/S DDFx-IGT, Integrated GPS Module).
/// </summary>
public enum EgpsAntStatus
{
    /// <summary>
    ///     No GPS antenna error detected.
    /// </summary>
    GpsAntStatusNoError,

    /// <summary>
    ///     GPS antenna connector unconnected (no GPS antenna detected).
    /// </summary>
    GpsAntStatusOpen,

    /// <summary>
    ///     GPS antenna connector shorted.
    /// </summary>
    GpsAntStatusShort
}

/// <summary>
///     3.38 Enum: eGPS_ANT_TYPE GPS天线类型(无源/有源)
///     Type of GPS antenna connected to option R/&/S DDFx-IGT, Integrated GPS Module.
/// </summary>
public enum EgpsAntType
{
    /// <summary>
    ///     Active antenna.
    /// </summary>
    GpsAntActive,

    /// <summary>
    ///     Passive antenna.
    /// </summary>
    GpsAntPassive
}

/// <summary>
///     3.39 Enum: eGPS_CODE 连接到X15或X16的GPS接收器类型。
///     Type of GPS receiver connected to X15 or X16.
/// </summary>
public enum EgpsCode
{
    /// <summary>
    ///     Unknown type.
    /// </summary>
    GpsUndefined,

    /// <summary>
    ///     Not used.
    /// </summary>
    GpsUser,

    /// <summary>
    ///     Not used.
    /// </summary>
    GpsGina,

    /// <summary>
    ///     Conventional GPS receiver.
    /// </summary>
    GpsNmea,

    /// <summary>
    ///     LEA (option R/&/S DDFx-IGT, Integrated GPS Module).
    /// </summary>
    GpsLea,

    /// <summary>
    ///     Not used.
    /// </summary>
    GpsTsip,

    /// <summary>
    ///     LEA6.
    /// </summary>
    GpsLea6,

    /// <summary>
    ///     LEA6T.
    /// </summary>
    GpsLea6T,

    /// <summary>
    ///     LEAM8.
    /// </summary>
    GpsLeam8,

    /// <summary>
    ///     LEAM8T.
    /// </summary>
    GpsLeam8T,

    /// <summary>
    ///     GPS NMEA sentences received (from NMEA UDP port: command NmeaUdpPort).
    /// </summary>
    GpsUdpNmea
}

/// <summary>
///     3.40 Enum: eGPS_EDGE
///     Edge of PPS (GPS one-second pulse) to be used.
/// </summary>
public enum EgpsEdge
{
    /// <summary>
    ///     Rising edge.
    /// </summary>
    EdgeRaising
}

/// <summary>
///     3.41 Enum: eGPS_ERROR GPS错误
///     Error status of option R/&/S DDFx-IGT, Integrated GPS Module.
/// </summary>
public enum EgpsError
{
    /// <summary>
    ///     No GPS error detected.
    /// </summary>
    GpsNoError,

    /// <summary>
    ///     No GPS antenna detected (antenna connector unconnected).
    /// </summary>
    GpsAntennaOpen,

    /// <summary>
    ///     GPS antenna connector shorted.
    /// </summary>
    GpsAntennaShort,

    /// <summary>
    ///     Fixed position entered does not match current position detected within averaging accuracy.
    /// </summary>
    GpsPosMismatch,

    /// <summary>
    ///     Unknown GPS error.
    /// </summary>
    GpsUnknownError
}

/// <summary>
///     3.42 Enum: eGPS_OPMODE GPS工作模式
///     Operation mode of option R/&/S DDFx-IGT, Integrated GPS Module (for GET and SET operation).
/// </summary>
public enum EgpsOpMode
{
    /// <summary>
    ///     Free Run.
    /// </summary>
    GpsOpmodeFreeRun,

    /// <summary>
    ///     Averaging.
    /// </summary>
    GpsOpmodeAveraging,

    /// <summary>
    ///     Fixed Location.
    /// </summary>
    GpsOpmodeFixed
}

/// <summary>
///     3.43 Enum: eGPS_OPMODE_STATUS GPS工作模式状态
///     Operation mode status of option R/&/S DDFx-IGT, Integrated GPS Module (for GET operation only).
/// </summary>
public enum EgpsOpModeStatus
{
    /// <summary>
    ///     No GPS connected.
    /// </summary>
    GpsOpmodeStatNoGps,

    /// <summary>
    ///     Free Run.
    /// </summary>
    GpsOpmodeStatFreeRun,

    /// <summary>
    ///     Averaging.
    /// </summary>
    GpsOpmodeStatAveraging,

    /// <summary>
    ///     Fixed Location.
    /// </summary>
    GpsOpmodeStatFixed
}

/// <summary>
///     3.44 Enum: eGPS_RESET GPS复位方式
///     Type of Reset of GPS receiver.
/// </summary>
public enum EgpsReset
{
    /// <summary>
    ///     Cold Reset: Discard: calculated position (P), almanac data (A), UTC time (U), satellites in view (S); try to locate
    ///     satellites and to recalculate position.
    /// </summary>
    GpsResetCold,

    /// <summary>
    ///     Warm Reset: Keep: P, A, U; discard: S; try to locate satellites and to recalculate position.
    /// </summary>
    GpsResetWarm,

    /// <summary>
    ///     Hot Reset: Keep: P, A, U, S; try to recalculate position.
    /// </summary>
    GpsResetHot
}

/// <summary>
///     3.45 Enum: eHEADING_TYPE
///     Heading type of compass.
/// </summary>
public enum EHeadingType
{
    /// <summary>
    ///     No heading value available.
    /// </summary>
    HeadingTypeUndefined,

    /// <summary>
    ///     Heading unknown.
    /// </summary>
    HeadingTypeUnknown,

    /// <summary>
    ///     Reference is compass north marker.
    /// </summary>
    HeadingTypeCompass,

    /// <summary>
    ///     Reference is magnetic north.
    /// </summary>
    HeadingTypeMagnetic,

    /// <summary>
    ///     Reference is true (geographic) north.
    /// </summary>
    HeadingTypeTrue,

    /// <summary>
    ///     Heading value unusable.
    /// </summary>
    HeadingTypeUnusable,

    /// <summary>
    ///     Heading value derived from GPS data.
    /// </summary>
    HeadingTypeTrack,

    /// <summary>
    ///     Heading value bad because movement is too slow (GPS compass only).
    /// </summary>
    HeadingTypeTrackSlow
}

/// <summary>
///     3.46 Enum: eHF_INPUT HF天线输入
///     Antenna signal input connector group for HF.
/// </summary>
public enum EhfInput
{
    /// <summary>
    ///     HF  HF (see above note 2).
    /// </summary>
    HfInputHf1,

    /// <summary>
    ///     HF/V/U/SHF  HF/V/U/SHF.
    /// </summary>
    HfInputHf2
}

/// <summary>
///     3.47 Enum: eHRPAN_MODE
///     HRPAN (High-Resolution Panorama) mode.
/// </summary>
public enum EhrPanMode
{
    /// <summary>
    ///     OFF Deactivate data stream.
    /// </summary>
    HrpanOff,

    /// <summary>
    ///     SHORT Activate data stream and set mode to 16 bit format.
    /// </summary>
    Hrpan16Bit
}

/// <summary>
///     3.48 Enum: eHRPAN_STEP
/// </summary>
public enum EhrPanStep
{
    /// <summary>
    ///     0.39 Hz	0.39 Hz.
    /// </summary>
    HrpanStep0P39Hz,

    /// <summary>
    ///     0.77 Hz	0.77 Hz.
    /// </summary>
    HrpanStep0P77Hz,

    /// <summary>
    ///     1.53 Hz	1.53 Hz.
    /// </summary>
    HrpanStep1P53Hz,

    /// <summary>
    ///     3.06 Hz	3.06 Hz.
    /// </summary>
    HrpanStep3P06Hz,

    /// <summary>
    ///     6.11 Hz	6.11 Hz.
    /// </summary>
    HrpanStep6P11Hz,

    /// <summary>
    ///     12.21 Hz	12.21 Hz.
    /// </summary>
    HrpanStep12P21Hz,

    /// <summary>
    ///     24.42 Hz	24.42 Hz.
    /// </summary>
    HrpanStep24P42Hz,

    /// <summary>
    ///     48.83 Hz	48.83 Hz.
    /// </summary>
    HrpanStep48P83Hz,

    /// <summary>
    ///     97.66 Hz	97.66 Hz.
    /// </summary>
    HrpanStep97P66Hz,

    /// <summary>
    ///     195.32 Hz	195.32 Hz.
    /// </summary>
    HrpanStep195P32Hz,

    /// <summary>
    ///     390.63 Hz	390.63 Hz.
    /// </summary>
    HrpanStep390P63Hz,

    /// <summary>
    ///     781.25 Hz	781.25 Hz.
    /// </summary>
    HrpanStep781P25Hz,

    /// <summary>
    ///     1.5625 kHz	1.5625 kHz.
    /// </summary>
    HrpanStep1562P5Hz,

    /// <summary>
    ///     3.125 kHz	3.125 kHz.
    /// </summary>
    HrpanStep3125Hz,

    /// <summary>
    ///     6.25 kHz	6.25 kHz.
    /// </summary>
    HrpanStep6250Hz,

    /// <summary>
    ///     12.5 kHz	12.5 kHz.
    /// </summary>
    HrpanStep12500Hz
}

/// <summary>
///     3.49 Enum: eHW_STATUS 硬件模块状态
///     Status of hardware module (peripheral device: antenna, compass, ...).
/// </summary>
public enum EhwStatus
{
    /// <summary>
    ///     All right.
    /// </summary>
    HwStatusOk,

    /// <summary>
    ///     Disconnected.
    /// </summary>
    HwStatusDisconnected,

    /// <summary>
    ///     Transitional state
    /// </summary>
    HwStatusConnectPending
}

/// <summary>
///     3.50 Enum: eHW_TYPE
///     Type of hardware module (peripheral device: antenna, compass, ...).
/// </summary>
public enum EhwType
{
    /// <summary>
    ///     DF antenna.
    /// </summary>
    HwDfAntenna,

    /// <summary>
    ///     Internal HW module.
    /// </summary>
    HwBoard,

    /// <summary>
    ///     Signal converting device.
    /// </summary>
    HwConverter,

    /// <summary>
    ///     Compass.
    /// </summary>
    HwCompass,

    /// <summary>
    ///     GPS receiver.
    /// </summary>
    HwGps,

    /// <summary>
    ///     Processing device.
    /// </summary>
    HwEbd,

    /// <summary>
    ///     CPLD (Complex Programmable Logical Device).
    /// </summary>
    HwCpld,

    /// <summary>
    ///     Miscellaneous (other hardware).
    /// </summary>
    HwMisc,

    /// <summary>
    ///     Rx antenna.
    /// </summary>
    HwRxAntenna
}

/// <summary>
///     3.51 Enum: eIFPAN_MODE
/// </summary>
public enum EifPanMode
{
    /// <summary>
    ///     Display plain data (no averaging or holding).
    /// </summary>
    IfpanModeClrwrite,

    /// <summary>
    ///     Min Hold.
    /// </summary>
    IfpanModeMinhold,

    /// <summary>
    ///     Max Hold.
    /// </summary>
    IfpanModeMaxhold,

    /// <summary>
    ///     Average.
    /// </summary>
    IfpanModeAverage
}

/// <summary>
///     3.52 Enum: eIFPAN_SELECTIVITY
/// </summary>
public enum EifPanSelectivity
{
    /// <summary>
    ///     Select automatically.
    /// </summary>
    IfpanSelectivityAuto,

    /// <summary>
    ///     Normal.
    /// </summary>
    IfpanSelectivityNormal,

    /// <summary>
    ///     Narrow.
    /// </summary>
    IfpanSelectivityNarrow,

    /// <summary>
    ///     Sharp.
    /// </summary>
    IfpanSelectivitySharp
}

/// <summary>
///     3.53 Enum: eIFPAN_STEP 中频带宽(*0.01Hz)
/// </summary>
public enum EifPanStep : long
{
    /// <summary>
    ///     auto    Select automatically depending on span.
    /// </summary>
    IfpanStepAuto = -100000,

    /// <summary>
    ///     31.25 Hz	31.25 Hz.
    /// </summary>
    IfpanStep31P25Hz = 3125,

    /// <summary>
    ///     50 Hz	50 Hz.
    /// </summary>
    IfpanStep50Hz = 5000,

    /// <summary>
    ///     62.5 Hz	62.5 Hz.
    /// </summary>
    IfpanStep62P5Hz = 6250,

    /// <summary>
    ///     100 Hz	100 Hz.
    /// </summary>
    IfpanStep100Hz = 10000,

    /// <summary>
    ///     125 Hz	125 Hz.
    /// </summary>
    IfpanStep125Hz = 12500,

    /// <summary>
    ///     200 Hz	200 Hz.
    /// </summary>
    IfpanStep200Hz = 20000,

    /// <summary>
    ///     250 Hz	250 Hz.
    /// </summary>
    IfpanStep250Hz = 25000,

    /// <summary>
    ///     312.5 Hz	312.5 Hz.
    /// </summary>
    IfpanStep312P5Hz = 31250,

    /// <summary>
    ///     500 Hz	500 Hz.
    /// </summary>
    IfpanStep500Hz = 50000,

    /// <summary>
    ///     625 Hz	625 Hz.
    /// </summary>
    IfpanStep625Hz = 62500,

    /// <summary>
    ///     1 kHz	1 kHz.
    /// </summary>
    IfpanStep1Khz = 100000,

    /// <summary>
    ///     1.25 kHz	1.25 kHz.
    /// </summary>
    IfpanStep1P25Khz = 125000,

    /// <summary>
    ///     2 kHz	2 kHz.
    /// </summary>
    IfpanStep2Khz = 200000,

    /// <summary>
    ///     2.5 kHz	2.5 kHz.
    /// </summary>
    IfpanStep2P5Khz = 250000,

    /// <summary>
    ///     3.125 kHz	3.125 kHz.
    /// </summary>
    IfpanStep3P125Khz = 312500,

    /// <summary>
    ///     5 kHz	5 kHz.
    /// </summary>
    IfpanStep5Khz = 500000,

    /// <summary>
    ///     6.25 kHz	6.25 kHz.
    /// </summary>
    IfpanStep6P25Khz = 625000,

    /// <summary>
    ///     8.333 kHz	8.333 kHz.
    /// </summary>
    IfpanStep8P333Khz = 833300,

    /// <summary>
    ///     10 kHz	10 kHz.
    /// </summary>
    IfpanStep10Khz = 1000000,

    /// <summary>
    ///     12.5 kHz	12.5 kHz.
    /// </summary>
    IfpanStep12P5Khz = 1250000,

    /// <summary>
    ///     20 kHz	20 kHz.
    /// </summary>
    IfpanStep20Khz = 2000000,

    /// <summary>
    ///     25 kHz	25 kHz.
    /// </summary>
    IfpanStep25Khz = 2500000,

    /// <summary>
    ///     50 kHz	50 kHz.
    /// </summary>
    IfpanStep50Khz = 5000000,

    /// <summary>
    ///     100 kHz	100 kHz.
    /// </summary>
    IfpanStep100Khz = 10000000,

    /// <summary>
    ///     200 kHz	200 kHz.
    /// </summary>
    IfpanStep200Khz = 20000000,

    /// <summary>
    ///     500 kHz	500 kHz.
    /// </summary>
    IfpanStep500Khz = 50000000,

    /// <summary>
    ///     1 MHz	1 MHz.
    /// </summary>
    IfpanStep1000Khz = 100000000,

    /// <summary>
    ///     2 MHz	2 MHz.
    /// </summary>
    IfpanStep2000Khz = 200000000
}

/// <summary>
///     3.54 Enum: eIF_MODE IQ数据格式
///     IF (I/Q data) data trace state and data format.
/// </summary>
public enum EifMode
{
    /// <summary>
    ///     Both IF data traces off (no demodulated I/Q data).
    /// </summary>
    IfOff,

    /// <summary>
    ///     Data trace IF, 16 bit per data value.
    /// </summary>
    If16Bit,

    /// <summary>
    ///     Data trace IF, 32 bit per data value.
    /// </summary>
    If32Bit,

    /// <summary>
    ///     Data trace AMMOS IF, 16 bit per data value.
    /// </summary>
    If16BitAmmos,

    /// <summary>
    ///     Data trace AMMOS IF, 32 bit per data value.
    /// </summary>
    If32BitAmmos
}

/// <summary>
///     3.55 Enum: eINPUT_RANGE
///     Type of input connector to use. If a type describes more than one connector, the specific one is determined by
///     additional parameters eHF_INPUT and eRF_INPUT .
/// </summary>
public enum EInputRange
{
    /// <summary>
    ///     Undefined input.
    /// </summary>
    InputUndefined,

    /// <summary>
    ///     Use HF connector; specific connector selected by eHF_INPUT.
    /// </summary>
    InputHf,

    /// <summary>
    ///     Use VUHF connector; specific connector selected by eRF_INPUT.
    /// </summary>
    InputVuhf
}

/// <summary>
///     3.56 Enum: eLEVEL_INDICATOR
///     ITU measurements level detector characteristics (option R/&/S DDFx-IM, ITU Measurement Software, required).
/// </summary>
public enum ELevelIndicatir
{
    /// <summary>
    ///     Measure average value of momentary amplitudes.
    /// </summary>
    LevelIndicatorAvg,

    /// <summary>
    ///     Extract peak value of momentary amplitudes.
    /// </summary>
    LevelIndicatorPeak,

    /// <summary>
    ///     Fix current value at moment of readout query.
    /// </summary>
    LevelIndicatorFast,

    /// <summary>
    ///     Measure RMS value of momentary amplitudes.
    /// </summary>
    LevelIndicatorRms
}

/// <summary>
///     3.57 Enum: eLOC_TIME_SOURCE 位置和时间的数据来源
///     Source of location and timing data in use.
/// </summary>
public enum ELocTimeSource
{
    /// <summary>
    ///     Manual: neutral state.
    /// </summary>
    LocTimeSrcManual,

    /// <summary>
    ///     GPS: 1st NMEA sentence "GPRMC" arriving after having entered this state will be evaluated.
    /// </summary>
    LocTimeSrcGps
}

/// <summary>
///     3.58 Enum: eMEASUREMODE 用于带宽测量的带宽类型(需要R/&/S DDFx-IM选项，ITU测量软件)。
///     Type of bandwidth for bandwidth measurement in use (option R/&/S DDFx-IM, ITU Measurement Software, required).
/// </summary>
public enum EMeasureMode
{
    /// <summary>
    ///     X dB bandwidth.
    /// </summary>
    MeasuremodeXdb,

    /// <summary>
    ///     Beta % bandwidth.
    /// </summary>
    MeasuremodeBeta
}

/// <summary>
///     3.59 Enum: eMEASUREMODECP
///     Measuring mode with ITU measurements (option R/&/S DDFx-IM, ITU Measurement Software, required).
/// </summary>
public enum EMeasureModeCp
{
    /// <summary>
    ///     Continuous Measuring Mode.
    /// </summary>
    MeasuremodecpCont,

    /// <summary>
    ///     Periodic Measuring Mode.
    /// </summary>
    MeasuremodecpPer
}

/// <summary>
///     3.60 Enum: eOUT_OF_RANGE
///     Status of test point.
/// </summary>
public enum EOutOfRange
{
    /// <summary>
    ///     Test point value inside tolerance range.
    /// </summary>
    LimitIn,

    /// <summary>
    ///     Test point value below tolerance range.
    /// </summary>
    LimitLower,

    /// <summary>
    ///     Test point value above tolerance range.
    /// </summary>
    LimitUpper
}

/// <summary>
///     3.61 Enum: ePSCAN_STEP 全景扫描步进
///     Range setting for RxPSCan (Rx Panorama Scan).
/// </summary>
public enum EpScanStep : uint
{
    /// <summary>
    ///     100 Hz	100 Hz.
    /// </summary>
    PscanStep0P1 = 100,

    /// <summary>
    ///     125 Hz	125 Hz.
    /// </summary>
    PscanStep0P125 = 125,

    /// <summary>
    ///     200 Hz	200 Hz.
    /// </summary>
    PscanStep0P2 = 200,

    /// <summary>
    ///     250 Hz	250 Hz.
    /// </summary>
    PscanStep0P25 = 250,

    /// <summary>
    ///     500 Hz	500 Hz.
    /// </summary>
    PscanStep0P5 = 500,

    /// <summary>
    ///     625 Hz	625 Hz.
    /// </summary>
    PscanStep0P625 = 625,

    /// <summary>
    ///     1 kHz	1 kHz.
    /// </summary>
    PscanStep1 = 1000,

    /// <summary>
    ///     1.25 kHz	1.25 kHz.
    /// </summary>
    PscanStep1P25 = 1250,

    /// <summary>
    ///     2 kHz	2 kHz.
    /// </summary>
    PscanStep2 = 2000,

    /// <summary>
    ///     2.5 kHz	2.5 kHz.
    /// </summary>
    PscanStep2P5 = 2500,

    /// <summary>
    ///     3.125 kHz	3.125 kHz.
    /// </summary>
    PscanStep3P125 = 3215,

    /// <summary>
    ///     5 kHz	5 kHz.
    /// </summary>
    PscanStep5 = 5000,

    /// <summary>
    ///     6.25 kHz	6.25 kHz.
    /// </summary>
    PscanStep6P25 = 6250,

    /// <summary>
    ///     8.333 kHz	8.333 kHz.
    /// </summary>
    PscanStep8P333 = 8333,

    /// <summary>
    ///     10 kHz	10 kHz.
    /// </summary>
    PscanStep10 = 10000,

    /// <summary>
    ///     12.5 kHz	12.5 kHz.
    /// </summary>
    PscanStep12P5 = 12500,

    /// <summary>
    ///     20 kHz	20 kHz.
    /// </summary>
    PscanStep20 = 20000,

    /// <summary>
    ///     25 kHz	25 kHz.
    /// </summary>
    PscanStep25 = 25000,

    /// <summary>
    ///     50 kHz	50 kHz.
    /// </summary>
    PscanStep50 = 50000,

    /// <summary>
    ///     100 kHz	100 kHz.
    /// </summary>
    PscanStep100 = 100000,

    /// <summary>
    ///     200 kHz	200 kHz.
    /// </summary>
    PscanStep200 = 200000,

    /// <summary>
    ///     500 kHz	500 kHz.
    /// </summary>
    PscanStep500 = 500000,

    /// <summary>
    ///     1 MHz	1 MHz.
    /// </summary>
    PscanStep1000 = 1000000,

    /// <summary>
    ///     2 MHz	2 MHz.
    /// </summary>
    PscanStep2000 = 2000000
}

/// <summary>
///     3.62 Enum: eREFERENCE_MODE
///     System reference (10 MHz reference frequency) mode.
/// </summary>
public enum EReferenceMode
{
    /// <summary>
    ///     System reference external (supplied to REF IN connector).
    /// </summary>
    ReferenceModeExternal,

    /// <summary>
    ///     System reference internal (generated by internal OCXO).
    /// </summary>
    ReferenceModeInternal
}

/// <summary>
///     3.63 Enum: eREFERENCE_SYNCH 同步状态
///     System reference (10 MHz reference frequency) state of synchronization.
/// </summary>
public enum EReferenceSynch
{
    /// <summary>
    ///     Internally synchronized.
    /// </summary>
    ReferenceSynchInternal,

    /// <summary>
    ///     Synchronized to external reference (X41 REF IN).
    /// </summary>
    ReferenceSynchExternal,

    /// <summary>
    ///     Not locked to PPS (GPS one-second pulse, X43 GPS PPS).
    /// </summary>
    ReferenceSynchPpsUnlock,

    /// <summary>
    ///     Locked to PPS.
    /// </summary>
    ReferenceSynchPpsLock
}

/// <summary>
///     3.64 Enum: eRESET_TYPE 重置类型
///     Type of reset (see command Reset for explanation).
/// </summary>
public enum EResetType
{
    /// <summary>
    ///     Reset R/&/S DDFx settings to factory defaults, do not perform reboot.
    /// </summary>
    ResetSettings,

    /// <summary>
    ///     Warm reset: perform reboot, do not set to factory defaults (also default value).
    /// </summary>
    ResetWarm,

    /// <summary>
    ///     Cold reset: perform reboot, set to factory defaults.
    /// </summary>
    ResetCold
}

/// <summary>
///     3.65 Enum: eRESULT 自检结果
///     Self test result.
/// </summary>
public enum EResult
{
    /// <summary>
    ///     Self test successful: all tests passed.
    /// </summary>
    ResultGo,

    /// <summary>
    ///     Self test unsuccessful: at least one test point failed.
    /// </summary>
    ResultNogo
}

/// <summary>
///     3.66 Enum: eRF_INPUT
///     Antenna signal input connector group for VHF/UHF/SHF.
/// </summary>
public enum ErfInput
{
    /// <summary>
    ///     V/UHF   V/UHF.
    /// </summary>
    RfInputVushf1,

    /// <summary>
    ///     HF/V/U/SHF  HF/V/U/SHF.
    /// </summary>
    RfInputVushf2
}

/// <summary>
///     3.67 Enum: eRF_MODE 射频模式
///     Preselection mode.
/// </summary>
public enum ERfMode
{
    /// <summary>
    ///     Normal.
    /// </summary>
    RfmodeNormal,

    /// <summary>
    ///     Low noise.
    /// </summary>
    RfmodeLowNoise,

    /// <summary>
    ///     Low distortion.
    /// </summary>
    RfmodeLowDistortion
}

/// <summary>
///     3.68 Enum: eRX_PATH
///     DF path an RX antenna is connected to.
/// </summary>
public enum ERxPath
{
    /// <summary>
    ///     DF1 DF path 1.
    /// </summary>
    RxPathDf1,

    /// <summary>
    ///     DF2 DF path 2.
    /// </summary>
    RxPathDf2,

    /// <summary>
    ///     DF3 DF path 3.
    /// </summary>
    RxPathDf3
}

/// <summary>
///     3.69 Enum: eSELECTORFLAG
///     Selector flags: subset of data that mass data output is to be enabled for. Find more detailed information in R/&/S
///     DDFx system manuals.
/// </summary>
[Flags]
public enum ESelectorFlag : ulong
{
    /// <summary>
    ///     Level.
    /// </summary>
    SelflagLevel = 0x01,

    /// <summary>
    ///     Frequency offset.
    /// </summary>
    SelflagOffset = 0x02,

    /// <summary>
    ///     Field strength.
    /// </summary>
    SelflagFstrength = 0x04,

    /// <summary>
    ///     Amplitude swing, average.
    /// </summary>
    SelflagAm = 0x08,

    /// <summary>
    ///     Amplitude swing, positive.
    /// </summary>
    SelflagAmPos = 0x10,

    /// <summary>
    ///     Amplitude swing, negative.
    /// </summary>
    SelflagAmNeg = 0x20,

    /// <summary>
    ///     Frequency deviation, average.
    /// </summary>
    SelflagFm = 0x40,

    /// <summary>
    ///     Frequency deviation, positive.
    /// </summary>
    SelflagFmPos = 0x80,

    /// <summary>
    ///     Frequency deviation, negative.
    /// </summary>
    SelflagFmNeg = 0x100,

    /// <summary>
    ///     Phase swing.
    /// </summary>
    SelflagPm = 0x200,

    /// <summary>
    ///     Bandwidth.
    /// </summary>
    SelflagBandwidth = 0x400,

    /// <summary>
    ///     DF level.
    /// </summary>
    SelflagDfLevel = 0x800,

    /// <summary>
    ///     Azimuth.
    /// </summary>
    SelflagAzimuth = 0x1000,

    /// <summary>
    ///     DF quality.
    /// </summary>
    SelflagDfQuality = 0x2000,

    /// <summary>
    ///     DF field strength.
    /// </summary>
    SelflagDfFstrength = 0x4000,

    /// <summary>
    ///     DF level (continuous).
    /// </summary>
    SelflagDfLevelCont = 0x8000,

    /// <summary>
    ///     Channel.
    /// </summary>
    SelflagChannel = 0x10000,

    /// <summary>
    ///     Lower 32 bit of the frequency.
    /// </summary>
    SelflagFreqLow = 0x20000,

    /// <summary>
    ///     Elevation.
    /// </summary>
    SelflagElevation = 0x40000,

    /// <summary>
    ///     DF channel status.
    /// </summary>
    SelflagDfChannelStatus = 0x80000,

    /// <summary>
    ///     DF omniphase.
    /// </summary>
    SelflagDfOmniphase = 0x100000,

    /// <summary>
    ///     Upper 32 bit of the frequency.
    /// </summary>
    SelflagFreqHigh = 0x200000,

    /// <summary>
    ///     Base counter.
    /// </summary>
    SelflagBasecounter = 0x1000000,

    /// <summary>
    ///     Swap endianness.
    /// </summary>
    SelflagSwap = 0x20000000,

    /// <summary>
    ///     Show only values above squelch.
    /// </summary>
    SelflagSignalGreaterSquelch = 0x40000000,

    /// <summary>
    ///     Optional Header.
    /// </summary>
    SelflagOptionalHeader = 0x80000000
}

/// <summary>
///     3.70 Enum: eSELFTEST 自检类型
///     Type of self test.
/// </summary>
public enum ESelfTest
{
    /// <summary>
    ///     Short self test.
    /// </summary>
    SelftestShort,

    /// <summary>
    ///     Long self test.
    /// </summary>
    SelftestLong
}

/// <summary>
///     3.71 Enum: eSIGP_DATATYPE 信号处理数据的类型
///     Type of signal processing data.
///     NOTE: Item is used for output of interim results from signal processing and therefore is subject to future
///     enhancement.
/// </summary>
public enum ESigpDataType
{
    /// <summary>
    ///     Averaged data.
    /// </summary>
    SigpDataAvg
}

/// <summary>
///     3.72 Enum: eSPAN 频谱带宽
///     Realtime bandwidth (frequency span).
/// </summary>
public enum ESpan
{
    /// <summary>
    ///     100 kHz	100 kHz.
    /// </summary>
    IfpanFreqRange100 = 100,

    /// <summary>
    ///     200 kHz	200 kHz.
    /// </summary>
    IfpanFreqRange200 = 200,

    /// <summary>
    ///     500 kHz	500 kHz.
    /// </summary>
    IfpanFreqRange500 = 500,

    /// <summary>
    ///     1 MHz	1 MHz.
    /// </summary>
    IfpanFreqRange1000 = 1000,

    /// <summary>
    ///     2 MHz	2 MHz.
    /// </summary>
    IfpanFreqRange2000 = 2000,

    /// <summary>
    ///     5 MHz	5 MHz.
    /// </summary>
    IfpanFreqRange5000 = 5000,

    /// <summary>
    ///     10 MHz	10 MHz.
    /// </summary>
    IfpanFreqRange10000 = 10000,

    /// <summary>
    ///     20 MHz	20 MHz.
    /// </summary>
    IfpanFreqRange20000 = 20000,

    /// <summary>
    ///     40 MHz	40 MHz.
    /// </summary>
    IfpanFreqRange40000 = 40000,

    /// <summary>
    ///     80 MHz	80 MHz.
    /// </summary>
    IfpanFreqRange80000 = 80000
}

/// <summary>
///     3.73 Enum: eSTATE 双极开关状态(天线前置放大器等)。
///     Bipolar switching states (antenna preamplifier etc.).
/// </summary>
public enum EState
{
    /// <summary>
    ///     Switched off (i.e. disabled) (unless noted otherwise).
    /// </summary>
    StateOff,

    /// <summary>
    ///     Switched on (i.e. enabled).
    /// </summary>
    StateOn
}

/// <summary>
///     3.74 Enum: eSTD_STEP 步进
///     Short-Time Detector spectral step size.
/// </summary>
public enum EStdStep
{
    /// <summary>
    ///     390.63 Hz	390.63 Hz.
    /// </summary>
    StdStep390P63Hz,

    /// <summary>
    ///     781.25 Hz	781.25 Hz.
    /// </summary>
    StdStep781P25Hz,

    /// <summary>
    ///     1.5625 kHz	1.5625 kHz.
    /// </summary>
    StdStep1562P5Hz,

    /// <summary>
    ///     3.125 kHz	3.125 kHz.
    /// </summary>
    StdStep3125Hz,

    /// <summary>
    ///     6.25 kHz	6.25 kHz.
    /// </summary>
    StdStep6250Hz,

    /// <summary>
    ///     12.5 kHz	12.5 kHz.
    /// </summary>
    StdStep12500Hz,

    /// <summary>
    ///     25 kHz	25 kHz.
    /// </summary>
    StdStep25000Hz,

    /// <summary>
    ///     50 kHz	50 kHz.
    /// </summary>
    StdStep50000Hz,

    /// <summary>
    ///     100 kHz	100 kHz.
    /// </summary>
    StdStep100000Hz
}

/// <summary>
///     3.75 Enum: eSTS_BW 带宽
///     Short-Time Synthesizer bandwidth.
/// </summary>
public enum EStsBw
{
    /// <summary>
    ///     30 kHz	30 kHz.
    /// </summary>
    StsBw30000,

    /// <summary>
    ///     300 kHz	300 kHz.
    /// </summary>
    StsBw300000
}

/// <summary>
///     3.76 Enum: eTRACETAG
///     Trace tags: set of data that mass data output is to be enabled for. Find more detailed information in R/&/S DDFx
///     system manual.
/// </summary>
public enum ETraceTag
{
    /// <summary>
    ///     Audio: digital audio signal.
    /// </summary>
    TracetagAudio,

    /// <summary>
    ///     IFPan: spectrum (panorama) of the IF signal.
    /// </summary>
    TracetagIfpan,

    /// <summary>
    ///     DF: Direction finding massdata (DFPScan).
    /// </summary>
    TracetagDf,

    /// <summary>
    ///     GPS_COMPASS: GPS and compass data.
    /// </summary>
    TracetagGpsCompass,

    /// <summary>
    ///     ANT_LEVEL: antenna level data.
    /// </summary>
    TracetagAntLevel,

    /// <summary>
    ///     SelCall: SelCal analysis data.
    /// </summary>
    TracetagSelCall,

    /// <summary>
    ///     CW: data from measurements (triggered manually or periodically).
    /// </summary>
    TracetagCwave,

    /// <summary>
    ///     IF: IF signal (I/Q data, unregulated).
    /// </summary>
    TracetagIf,

    /// <summary>
    ///     VIDEO: Video data.
    /// </summary>
    TracetagVideo,

    /// <summary>
    ///     VDPan: Video panorama data.
    /// </summary>
    TracetagVideopan,

    /// <summary>
    ///     PScan: Panorama Scan level data.
    /// </summary>
    TracetagPscan,

    /// <summary>
    ///     SignalProcessing.
    /// </summary>
    TracetagSigp,
    TracetagDebug,

    /// <summary>
    ///     AMMOS I/Q Data.
    /// </summary>
    TracetagAmmosIf,

    /// <summary>
    ///     HRPAN: High-Resolution Panorama.
    /// </summary>
    TracetagHrpan,

    /// <summary>
    ///     DDCE: AMMOS DDC data.
    /// </summary>
    TracetagDdce,

    /// <summary>
    ///     STD: AMMOS Burst Emission List data.
    /// </summary>
    TracetagStd
}

/// <summary>
///     3.77 Enum: eTRIGGER_MEASUREMODE 触发式测量工作模式
///     Operating mode of triggered measuring mode.
/// </summary>
public enum ETriggerMeasureMode
{
    /// <summary>
    ///     Single measurement per trigger event.
    /// </summary>
    DftriggermeasmodeSingle,

    /// <summary>
    ///     Continuous measuring.
    /// </summary>
    DftriggermeasmodeCont
}

/// <summary>
///     3.78 Enum: eTRIGGER_MODE 触发测量模式状态。
///     State of triggered measuring mode.
/// </summary>
public enum ETriggerMode
{
    /// <summary>
    ///     Disabled: no triggered measuring.
    /// </summary>
    DftriggermodeDisabled,

    /// <summary>
    ///     Enabled: trigger event on external connector starts a measurement.
    /// </summary>
    DftriggermodeExtern,

    /// <summary>
    ///     Synchronous Scan: measurement start is controlled by scan range settings
    /// </summary>
    DftriggermodeTimesync
}

/// <summary>
///     3.79 Enum: eTRIGGER_SOURCE 触发测量模式的触发源
///     Trigger source for triggered measuring mode.
/// </summary>
public enum ETriggerSource
{
    /// <summary>
    ///     X44 External trigger: connector X44 TRIGGER.
    /// </summary>
    DftriggersourceExtTrigIn1,

    /// <summary>
    ///     X17 GSM: connector X17 AUX, pin 8 GSM_STROBE.
    /// </summary>
    DftriggersourceGsmStrobe,

    /// <summary>
    /// </summary>
    DftriggersourceTrigger,

    /// <summary>
    ///     X43 PPS (GPS one-second pulse): connector X43 GPS PPS.
    /// </summary>
    DftriggersourceGpsPps
}

/// <summary>
///     3.80 Enum: eUART
///     UART (Universal Asynchronous Receiver/Transmitter) for individual connector.
/// </summary>
public enum Euart
{
    /// <summary>
    ///     X15 (GPS)	X15 (GPS).
    /// </summary>
    UartGps,

    /// <summary>
    ///     X16 (Compass)	X16 (Compass).
    /// </summary>
    UartCompass
}

/// <summary>
///     3.81 Enum: eVIDEO_MODE 视频格式
///     VIDEO data trace state and data format.
/// </summary>
public enum EVideoMode
{
    /// <summary>
    ///     VIDEO data trace off (no demodulated data).
    /// </summary>
    VideoOff,

    /// <summary>
    ///     VIDEO data trace on, 16 bit per data value.
    /// </summary>
    Video16Bit,

    /// <summary>
    ///     VIDEO data trace on, 32 bit per data value.
    /// </summary>
    Video32Bit
}

/// <summary>
///     3.82 Enum: eWINDOW_TYPE FFT窗函数类型
///     Type (function) of FFT window.
/// </summary>
public enum EWindowType
{
    /// <summary>
    ///     (for internal use only)
    /// </summary>
    DfWindowTypeTest,

    /// <summary>
    ///     Window rectangle(i.e.no window).
    /// </summary>
    DfWindowTypeRectangle,

    /// <summary>
    ///     Window Hamming.
    /// </summary>
    DfWindowTypeHamming,

    /// <summary>
    ///     Window Blackman-Harris.
    /// </summary>
    DfWindowTypeBlackmanHarris,

    /// <summary>
    ///     Window Hann.
    /// </summary>
    DfWindowTypeHann,

    /// <summary>
    ///     Window Blackman.
    /// </summary>
    DfWindowTypeBlackman,

    /// <summary>
    ///     Window Nuttal.
    /// </summary>
    DfWindowTypeNuttal,

    /// <summary>
    ///     Window Blackman-Nuttal.
    /// </summary>
    DfWindowTypeBlackmanNuttal
}

#endregion Xml枚举

#region Xml交互用结构体

/// <summary>
///     Version indication (version number) of hardware module.
/// </summary>
public struct SVersion
{
    /// <summary>
    ///     Main version number.
    /// </summary>
    public int MainVersion;

    /// <summary>
    ///     Sub version number.
    /// </summary>
    public int SubVersion;
}

/// <summary>
///     Antenna properties for an individual antenna frequency range
///     (all items apply only to addressed frequency range).
/// </summary>
public struct SAntRangeProp
{
    /// <summary>
    ///     Antenna has preamplifier (true: yes, false: no).
    /// </summary>
    public bool AntPreAmp;

    /// <summary>
    ///     Measuring elevation possible (ditto).
    /// </summary>
    public bool AntElevation;

    /// <summary>
    ///     Lower border frequency [Hz].
    /// </summary>
    public long FreqRangeBegin;

    /// <summary>
    ///     Upper border frequency [Hz].
    /// </summary>
    public long FreqRangeEnd;

    /// <summary>
    ///     Type of input connector relevant to addressed range (to be used with AntennaSetup).
    /// </summary>
    public EInputRange InputRange;

    /// <summary>
    ///     DF evaluation principle.
    /// </summary>
    public EDfAlt DfAlt;

    /// <summary>
    ///     Antenna polarization.
    /// </summary>
    public EAntPol AntPol;
}

/// <summary>
///     Frequency switch point: frequency that cannot be used inside a scan range if R/&/S DDFx is
///     operated in synchronous scan mode (option R/&/S DDFx-TS) (e.g. antenna switches over at this
///     frequency).Frequency may only be used as start frequency of the scan range.
/// </summary>
public struct SFreqTimePair
{
    /// <summary>
    ///     Frequency of switch point [Hz].
    /// </summary>
    public long Frequency;

    /// <summary>
    ///     Switching time [ns], indicates latency until DDFx is able to scan frequency.
    /// </summary>
    public long Time;
}

/// <summary>
///     Information on hardware module (peripheral device).
/// </summary>
public struct ShwInfo
{
    /// <summary>
    ///     Type of hardware (antenna, compass, ...).
    /// </summary>
    public EhwType HwType;

    /// <summary>
    ///     Status of module (OK or disconnected).
    /// </summary>
    public EhwStatus HwStatus;

    /// <summary>
    ///     Hardware code.
    /// </summary>
    public int Code;

    /// <summary>
    ///     Hardware handle (internal).
    /// </summary>
    public int Handle;

    /// <summary>
    ///     Port module is connected to.
    /// </summary>
    public int Port;

    /// <summary>
    ///     Version indication.
    /// </summary>
    public SVersion Version;

    /// <summary>
    ///     Name of module (24 chars max.).
    /// </summary>
    public string Name;
}

/// <summary>
///     Data record of antenna factors (k-factors) for Rx antenna.
/// </summary>
public struct SkFactor
{
    /// <summary>
    ///     Antenna number (used for backward compatibility).
    /// </summary>
    public int AntNo;

    /// <summary>
    ///     Name of data record (usually corresponds to antenna model in AntennaRxDefine, 24 chars max.).
    /// </summary>
    public string Name;

    /// <summary>
    ///     Lower border frequency [Hz] of antenna factors.
    /// </summary>
    public long FreqRangeBegin;

    /// <summary>
    ///     Upper border frequency [Hz].
    /// </summary>
    public long FreqRangeEnd;

    /// <summary>
    ///     Validity flags (1: applies, 0: does not apply; see table above for details).
    /// </summary>
    public int AntParams;
}

/// <summary>
///     Information on hardware module.
/// </summary>
public struct SModuleInfo
{
    /// <summary>
    ///     Part number (24 chars max.).
    /// </summary>
    public string PartNumber;

    /// <summary>
    ///     Hardware code.
    /// </summary>
    public int HwCode;

    /// <summary>
    ///     Product index (24 chars max.).
    /// </summary>
    public string ProductIndex;

    /// <summary>
    ///     Serial number (24 chars max.).
    /// </summary>
    public string SerialNumber;

    /// <summary>
    ///     Production date (24 chars max.).
    /// </summary>
    public string ProductDate;

    /// <summary>
    ///     Name of hardware module to address (24 chars max.).
    /// </summary>
    public string Name;
}

/// <summary>
///     Location of temperature sensor (name of module and sensor) and temperature value.
/// </summary>
public struct STempInfo
{
    /// <summary>
    ///     Name of hardware module to address (24 chars max.) or "ALL" (also default value).
    /// </summary>
    public string Module;

    /// <summary>
    ///     Name of temperature sensor (24 chars max.).
    /// </summary>
    public string Sensor;

    /// <summary>
    ///     Temperature value [°C].
    /// </summary>
    public int Value;
}

/// <summary>
///     Test command.
/// </summary>
public struct STestInfo
{
    /// <summary>
    ///     Description (256 chars max.) of this test and the meaning of the parameters for this test.
    /// </summary>
    public string TestDescription;

    /// <summary>
    ///     Name (24 chars max.) of the test that should be executed.
    /// </summary>
    public string TestName;

    /// <summary>
    ///     Default value (32 chars max.) for string parameter.
    /// </summary>
    public string TestString;

    /// <summary>
    ///     Default value (32 chars max.) for parameter 1.
    /// </summary>
    public long Param1Default;

    /// <summary>
    ///     Default value (32 chars max.) for parameter 2.
    /// </summary>
    public long Param2Default;

    /// <summary>
    ///     Default value (32 chars max.) for parameter 3.
    /// </summary>
    public long Param3Default;

    /// <summary>
    ///     Default value (32 chars max.) for parameter 4.
    /// </summary>
    public long Param4Default;
}

/// <summary>
///     Location and state of test point: name of point, name of module, measured value, tolerance range, value location
///     related to tolerance range,
///     test point validity. If testpoint is invalid, it can be ignored.
/// </summary>
public struct STestPoint
{
    /// <summary>
    ///     Name of test point (24 chars max.).
    /// </summary>
    public string Name;

    /// <summary>
    ///     Name of module (24 chars max.).
    /// </summary>
    public string Module;

    /// <summary>
    ///     Measured value.
    /// </summary>
    public int Value;

    /// <summary>
    ///     Upper limit of tolerance range.
    /// </summary>
    public int UpperLimit;

    /// <summary>
    ///     Lower limit.
    /// </summary>
    public int LowerLimit;

    /// <summary>
    ///     Location of measured value related to tolerance range (below, inside, above tolerance range).
    /// </summary>
    public EOutOfRange OutOfRange;

    /// <summary>
    ///     Test point validity (true: valid, false: invalid).
    /// </summary>
    public bool Valid;
}

/// <summary>
///     Characteristics of a trace (massdata socket connection with a client).
/// </summary>
public struct STraceInfo
{
    /// <summary>
    ///     Slot number of socket connection (0: default client).
    /// </summary>
    public int Slot;

    /// <summary>
    ///     IP address of the client (24 chars max.).
    /// </summary>
    public string Ip;

    /// <summary>
    ///     Local port in use on client host for mass data connection.
    /// </summary>
    public int Port;

    /// <summary>
    ///     Array with all enabled trace tags.
    /// </summary>
    public ETraceTag TraceTag;

    /// <summary>
    ///     Array with all enabled selector flags.
    /// </summary>
    public ESelectorFlag SelectorFlag;
}

/// <summary>
///     Version indication (version number) of software assigned to an individual hardware module.
/// </summary>
public struct SswVersion
{
    /// <summary>
    ///     Name of hardware module (24 chars max.).
    /// </summary>
    public string ModuleName;

    /// <summary>
    ///     Software version number of hardware module (80 chars max.).
    /// </summary>
    public string SwVersion;
}

#endregion Xml交互用结构体