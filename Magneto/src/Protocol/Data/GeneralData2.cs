// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-28-2023
//
// Last Modified By : Joker
// Last Modified On : 03-28-2023
// ***********************************************************************
// <copyright file="GeneralData2.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Magneto.Protocol.Define;
using Magneto.Protocol.Extensions;
using MessagePack;

namespace Magneto.Protocol.Data;

/// <summary>
///     Class SDataCellular.
///     Implements the <see cref="Magneto.Protocol.Data.SDataRaw" />
/// </summary>
/// <seealso cref="Magneto.Protocol.Data.SDataRaw" />
[Serializable]
[MessagePackObject]
public class SDataCellular : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataCellular" /> class.
    /// </summary>
    public SDataCellular()
    {
        Type = SDataType.BsDecoding;
    }

    /// <summary>
    ///     当前帧的时间戳
    /// </summary>
    /// <value>The timestamp.</value>
    [Key("timestamp")]
    public ulong Timestamp { get; set; }

    /// <summary>
    ///     载波频率
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    ///     网络制式
    /// </summary>
    /// <value>The duplex mode.</value>
    [Key("duplexMode")]
    public DuplexMode DuplexMode { get; set; }

    /// <summary>
    ///     信道号
    /// </summary>
    /// <value>The channel.</value>
    [Key("channel")]
    public int Channel { get; set; }

    /// <summary>
    ///     带宽 单位：kHz
    /// </summary>
    /// <value>The bandwidth.</value>
    [Key("bandwidth")]
    public double Bandwidth { get; set; }

    /// <summary>
    ///     移动国家编号
    /// </summary>
    /// <value>The MCC.</value>
    [Key("mcc")]
    public uint Mcc { get; set; }

    /// <summary>
    ///     系统编号
    ///     GSM / TD-SCDMA / WCDMA / TD-LTE / LTE FDD：移动网号（MNC）
    ///     CDMA 1X / CDMA2000-EVDO：系统识别号（SID）
    /// </summary>
    /// <value>The MNC.</value>
    [Key("mnc")]
    public uint Mnc { get; set; }

    /// <summary>
    ///     位置区编号
    ///     GSM / TD-SCDMA / WCDMA：位置区码（LAC）
    ///     TD-LTE / LTE FDD：跟踪区码（TAC）
    ///     CDMA 1X / CDMA2000-EVDO：网络识别号（NID）
    /// </summary>
    /// <value>The lac.</value>
    [Key("lac")]
    public uint Lac { get; set; }

    /// <summary>
    ///     小区编号
    ///     GSM / TD-SCDMA / WCDMA：小区识别号CI
    ///     CDMA 1X：基站识别号BASEID
    ///     CDMA2000-EVDO：扇区识别号SECTORID
    ///     TD-LTE / LTE FDD：演进型小区识别号ECI
    /// </summary>
    /// <value>The ci.</value>
    [Key("ci")]
    public ulong Ci { get; set; }

    /// <summary>
    ///     信号强度
    /// </summary>
    /// <value>The rx power.</value>
    [Key("rxPower")]
    public double RxPower { get; set; }

    /// <summary>
    ///     接收场强，单位dBμV/m
    /// </summary>
    /// <value>The field strength.</value>
    [Key("fieldStrength")]
    public double FieldStrength { get; set; }

    /// <summary>
    ///     基站定位信息
    /// </summary>
    /// <value>The bs GPS.</value>
    [Key("bsGps")]
    public GpsDatum BsGps { get; set; }

    /// <summary>
    ///     当前定位信息
    /// </summary>
    /// <value>The location.</value>
    [Key("location")]
    public GpsDatum Location { get; set; }

    /// <summary>
    ///     详细信息
    /// </summary>
    /// <value>The ex infos.</value>
    [Key("exInfos")]
    public Dictionary<string, ExtendedInfo> ExInfos { get; set; }
}

/// <summary>
///     Gps信息
/// </summary>
[Serializable]
[MessagePackObject]
public class GpsDatum
{
    /// <summary>
    ///     经度
    /// </summary>
    /// <value>The longitude.</value>
    [Key("longitude")]
    public double Longitude { get; set; }

    /// <summary>
    ///     纬度
    /// </summary>
    /// <value>The latitude.</value>
    [Key("latitude")]
    public double Latitude { get; set; }

    /// <summary>
    ///     海拔高度 单位：米
    /// </summary>
    /// <value>The altitude.</value>
    [Key("altitude")]
    public float Altitude { get; set; }

    /// <summary>
    ///     航向 单位：度
    /// </summary>
    /// <value>The heading.</value>
    [Key("heading")]
    public float Heading { get; set; }

    /// <summary>
    ///     GPS运行速度 单位：KM/小时
    /// </summary>
    /// <value>The speed.</value>
    [Key("speed")]
    public ushort Speed { get; set; }
}

/// <summary>
///     Class ExtendedInfo.
/// </summary>
[Serializable]
[MessagePackObject]
public class ExtendedInfo
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ExtendedInfo" /> class.
    /// </summary>
    public ExtendedInfo()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExtendedInfo" /> class.
    /// </summary>
    /// <param name="displayName">The display name.</param>
    /// <param name="value">The value.</param>
    public ExtendedInfo(string displayName, object value)
    {
        DisplayName = displayName;
        Value = value;
    }

    /// <summary>
    ///     Gets or sets the display name.
    /// </summary>
    /// <value>The display name.</value>
    [Key("displayName")]
    public string DisplayName { get; set; }

    /// <summary>
    ///     Gets or sets the value.
    /// </summary>
    /// <value>The value.</value>
    [Key("value")]
    public object Value { get; set; }
}

/// <summary>
///     信号解调数据
/// </summary>
[Serializable]
[MessagePackObject]
public class SDataSignalDemod : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataSignalDemod" /> class.
    /// </summary>
    public SDataSignalDemod()
    {
        Type = SDataType.SignalDemod;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether this <see cref="SDataSignalDemod" /> is success.
    /// </summary>
    /// <value><c>true</c> if success; otherwise, <c>false</c>.</value>
    [Key("success")]
    public bool Success { get; set; }

    /// <summary>
    ///     Gets or sets the spectrum.
    /// </summary>
    /// <value>The spectrum.</value>
    [Key("spectrum")]
    public SignalDemodSpectrum Spectrum { get; set; }

    /// <summary>
    ///     Gets or sets the spectrogram.
    /// </summary>
    /// <value>The spectrogram.</value>
    [Key("spectrogram")]
    public SignalDemodSpectrogram Spectrogram { get; set; }

    /// <summary>
    ///     Gets or sets the math spectrum.
    /// </summary>
    /// <value>The math spectrum.</value>
    [Key("mathSpectrum")]
    public SignalDemodMathSpectrum MathSpectrum { get; set; }

    /// <summary>
    ///     Gets or sets the amplitude time domain.
    /// </summary>
    /// <value>The amplitude time domain.</value>
    [Key("amplitudeTimeDomain")]
    public SignalDemodAmplitudeTimeDomain AmplitudeTimeDomain { get; set; }

    /// <summary>
    ///     Gets or sets the iq time domain.
    /// </summary>
    /// <value>The iq time domain.</value>
    [Key("iqTimeDomain")]
    public SignalDemodIqTimeDomain IqTimeDomain { get; set; }

    /// <summary>
    ///     Gets or sets the iq constellation.
    /// </summary>
    /// <value>The iq constellation.</value>
    [Key("iqConstellation")]
    public SignalDemodIqConstellation IqConstellation { get; set; }

    /// <summary>
    ///     Gets or sets the vector error.
    /// </summary>
    /// <value>The vector error.</value>
    [Key("vectorError")]
    public SignalDemodVectorError VectorError { get; set; }

    /// <summary>
    ///     Gets or sets the phase error.
    /// </summary>
    /// <value>The phase error.</value>
    [Key("phaseError")]
    public SignalDemodPhaseError PhaseError { get; set; }

    /// <summary>
    ///     Gets or sets the freq domain result.
    /// </summary>
    /// <value>The freq domain result.</value>
    [Key("freqDomainResult")]
    public SignalDemodFreqDomainResult FreqDomainResult { get; set; }

    /// <summary>
    ///     Gets or sets the time domain result.
    /// </summary>
    /// <value>The time domain result.</value>
    [Key("timeDomainResult")]
    public SignalDemodTimeDomainResult TimeDomainResult { get; set; }

    /// <summary>
    ///     Gets or sets the demod result.
    /// </summary>
    /// <value>The demod result.</value>
    [Key("demodResult")]
    public SignalDemodDemodResult DemodResult { get; set; }

    /// <summary>
    ///     Gets or sets the business type result.
    /// </summary>
    /// <value>The business type result.</value>
    [Key("businessTypeResult")]
    public SignalDemodBusinessTypeResult BusinessTypeResult { get; set; }
}

/// <summary>
///     Enum SignalDemodType
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<SignalDemodType>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<SignalDemodType>))]
public enum SignalDemodType
{
    /// <summary>
    ///     The spectrum
    /// </summary>
    [Key("spectrum")] Spectrum,

    /// <summary>
    ///     The spectrogram
    /// </summary>
    [Key("spectrogram")] Spectrogram,

    /// <summary>
    ///     The math spectrum
    /// </summary>
    [Key("mathSpectrum")] MathSpectrum,

    /// <summary>
    ///     The amplitude time domain
    /// </summary>
    [Key("amplitudeTimeDomain")] AmplitudeTimeDomain,

    /// <summary>
    ///     The iq time domain
    /// </summary>
    [Key("iqTimeDomain")] IqTimeDomain,

    /// <summary>
    ///     The iq constellation
    /// </summary>
    [Key("iqConstellation")] IqConstellation,

    /// <summary>
    ///     The vector error
    /// </summary>
    [Key("vectorError")] VectorError,

    /// <summary>
    ///     The phase error
    /// </summary>
    [Key("phaseError")] PhaseError,

    /// <summary>
    ///     The freq domain result
    /// </summary>
    [Key("freqDomainResult")] FreqDomainResult,

    /// <summary>
    ///     The time domain result
    /// </summary>
    [Key("timeDomainResult")] TimeDomainResult,

    /// <summary>
    ///     The demod result
    /// </summary>
    [Key("demodResult")] DemodResult,

    /// <summary>
    ///     The business type result
    /// </summary>
    [Key("businessTypeResult")] BusinessTypeResult
}

/// <summary>
///     Interface ISignalDemodItem
/// </summary>
public interface ISignalDemodItem
{
    /// <summary>
    ///     Gets the type.
    /// </summary>
    /// <value>The type.</value>
    SignalDemodType Type { get; }
}

/// <summary>
///     Class SignalDemodSpectrum.
///     Implements the <see cref="Magneto.Protocol.Data.ISignalDemodItem" />
/// </summary>
/// <seealso cref="Magneto.Protocol.Data.ISignalDemodItem" />
[Serializable]
[MessagePackObject]
public class SignalDemodSpectrum : ISignalDemodItem
{
    /// <summary>
    ///     Gets or sets the center frequency.
    /// </summary>
    /// <value>The center frequency.</value>
    [Key("centerFrequency")]
    public double CenterFrequency { get; set; }

    /// <summary>
    ///     Gets or sets the span.
    /// </summary>
    /// <value>The span.</value>
    [Key("span")]
    public double Span { get; set; }

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public float[] Data { get; set; }

    /// <summary>
    ///     Gets the type.
    /// </summary>
    /// <value>The type.</value>
    [Key("type")]
    public SignalDemodType Type => SignalDemodType.Spectrum;
}

/// <summary>
///     Class SignalDemodSpectrogram.
///     Implements the <see cref="Magneto.Protocol.Data.ISignalDemodItem" />
/// </summary>
/// <seealso cref="Magneto.Protocol.Data.ISignalDemodItem" />
[Serializable]
[MessagePackObject]
public class SignalDemodSpectrogram : ISignalDemodItem
{
    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public SignalDemodSpectrum[] Data { get; set; }

    /// <summary>
    ///     Gets the type.
    /// </summary>
    /// <value>The type.</value>
    [Key("type")]
    public SignalDemodType Type => SignalDemodType.Spectrogram;
}

/// <summary>
///     Class SignalDemodMathSpectrum.
///     Implements the <see cref="Magneto.Protocol.Data.ISignalDemodItem" />
/// </summary>
/// <seealso cref="Magneto.Protocol.Data.ISignalDemodItem" />
[Serializable]
[MessagePackObject]
public class SignalDemodMathSpectrum : ISignalDemodItem
{
    /// <summary>
    ///     Gets or sets the center frequency.
    /// </summary>
    /// <value>The center frequency.</value>
    [Key("centerFrequency")]
    public double CenterFrequency { get; set; }

    /// <summary>
    ///     Gets or sets the span.
    /// </summary>
    /// <value>The span.</value>
    [Key("span")]
    public double Span { get; set; }

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public float[] Data { get; set; }

    /// <summary>
    ///     Gets the type.
    /// </summary>
    /// <value>The type.</value>
    [Key("type")]
    public SignalDemodType Type => SignalDemodType.MathSpectrum;
}

/// <summary>
///     Class SignalDemodAmplitudeTimeDomain.
///     Implements the <see cref="Magneto.Protocol.Data.ISignalDemodItem" />
/// </summary>
/// <seealso cref="Magneto.Protocol.Data.ISignalDemodItem" />
[Serializable]
[MessagePackObject]
public class SignalDemodAmplitudeTimeDomain : ISignalDemodItem
{
    /// <summary>
    ///     Gets or sets the start time.
    /// </summary>
    /// <value>The start time.</value>
    [Key("startTime")]
    public double StartTime { get; set; }

    /// <summary>
    ///     Gets or sets the stop time.
    /// </summary>
    /// <value>The stop time.</value>
    [Key("stopTime")]
    public double StopTime { get; set; }

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public float[] Data { get; set; }

    /// <summary>
    ///     Gets the type.
    /// </summary>
    /// <value>The type.</value>
    [Key("type")]
    public SignalDemodType Type => SignalDemodType.AmplitudeTimeDomain;
}

/// <summary>
///     Class SignalDemodIQTimeDomain.
///     Implements the <see cref="Magneto.Protocol.Data.ISignalDemodItem" />
/// </summary>
/// <seealso cref="Magneto.Protocol.Data.ISignalDemodItem" />
[Serializable]
[MessagePackObject]
public class SignalDemodIqTimeDomain : ISignalDemodItem
{
    /// <summary>
    ///     Gets or sets the i data.
    /// </summary>
    /// <value>The i data.</value>
    [Key("idata")]
    public float[] Data { get; set; }

    /// <summary>
    ///     Gets or sets the q data.
    /// </summary>
    /// <value>The q data.</value>
    [Key("qdata")]
    public float[] QData { get; set; }

    /// <summary>
    ///     Gets the type.
    /// </summary>
    /// <value>The type.</value>
    [Key("type")]
    public SignalDemodType Type => SignalDemodType.IqTimeDomain;
}

/// <summary>
///     Class SignalDemodIQConstellation.
///     Implements the <see cref="Magneto.Protocol.Data.ISignalDemodItem" />
/// </summary>
/// <seealso cref="Magneto.Protocol.Data.ISignalDemodItem" />
[Serializable]
[MessagePackObject]
public class SignalDemodIqConstellation : ISignalDemodItem
{
    /// <summary>
    ///     Gets or sets the i data.
    /// </summary>
    /// <value>The i data.</value>
    [Key("idata")]
    public float[] Data { get; set; }

    /// <summary>
    ///     Gets or sets the q data.
    /// </summary>
    /// <value>The q data.</value>
    [Key("qdata")]
    public float[] QData { get; set; }

    /// <summary>
    ///     Gets the type.
    /// </summary>
    /// <value>The type.</value>
    [Key("type")]
    public SignalDemodType Type => SignalDemodType.IqConstellation;
}

/// <summary>
///     Class SignalDemodVectorError.
///     Implements the <see cref="Magneto.Protocol.Data.ISignalDemodItem" />
/// </summary>
/// <seealso cref="Magneto.Protocol.Data.ISignalDemodItem" />
[Serializable]
[MessagePackObject]
public class SignalDemodVectorError : ISignalDemodItem
{
    /// <summary>
    ///     Gets or sets the start time.
    /// </summary>
    /// <value>The start time.</value>
    [Key("startTime")]
    public double StartTime { get; set; }

    /// <summary>
    ///     Gets or sets the stop time.
    /// </summary>
    /// <value>The stop time.</value>
    [Key("stopTime")]
    public double StopTime { get; set; }

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public float[] Data { get; set; }

    /// <summary>
    ///     Gets the type.
    /// </summary>
    /// <value>The type.</value>
    [Key("type")]
    public SignalDemodType Type => SignalDemodType.VectorError;
}

/// <summary>
///     Class SignalDemodPhaseError.
///     Implements the <see cref="Magneto.Protocol.Data.ISignalDemodItem" />
/// </summary>
/// <seealso cref="Magneto.Protocol.Data.ISignalDemodItem" />
[Serializable]
[MessagePackObject]
public class SignalDemodPhaseError : ISignalDemodItem
{
    /// <summary>
    ///     Gets or sets the start time.
    /// </summary>
    /// <value>The start time.</value>
    [Key("startTime")]
    public double StartTime { get; set; }

    /// <summary>
    ///     Gets or sets the stop time.
    /// </summary>
    /// <value>The stop time.</value>
    [Key("stopTime")]
    public double StopTime { get; set; }

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public float[] Data { get; set; }

    /// <summary>
    ///     Gets the type.
    /// </summary>
    /// <value>The type.</value>
    [Key("type")]
    public SignalDemodType Type => SignalDemodType.PhaseError;
}

/// <summary>
///     Class SignalDemodFreqDomainResult.
///     Implements the <see cref="Magneto.Protocol.Data.ISignalDemodItem" />
/// </summary>
/// <seealso cref="Magneto.Protocol.Data.ISignalDemodItem" />
[Serializable]
[MessagePackObject]
public class SignalDemodFreqDomainResult : ISignalDemodItem
{
    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public SignalDemodResultItem[] Data { get; set; }

    /// <summary>
    ///     Gets the type.
    /// </summary>
    /// <value>The type.</value>
    [Key("type")]
    public SignalDemodType Type => SignalDemodType.FreqDomainResult;
}

/// <summary>
///     Class SignalDemodTimeDomainResult.
///     Implements the <see cref="Magneto.Protocol.Data.ISignalDemodItem" />
/// </summary>
/// <seealso cref="Magneto.Protocol.Data.ISignalDemodItem" />
[Serializable]
[MessagePackObject]
public class SignalDemodTimeDomainResult : ISignalDemodItem
{
    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public SignalDemodResultItem[] Data { get; set; }

    /// <summary>
    ///     Gets the type.
    /// </summary>
    /// <value>The type.</value>
    [Key("type")]
    public SignalDemodType Type => SignalDemodType.TimeDomainResult;
}

/// <summary>
///     Class SignalDemodDemodResult.
///     Implements the <see cref="Magneto.Protocol.Data.ISignalDemodItem" />
/// </summary>
/// <seealso cref="Magneto.Protocol.Data.ISignalDemodItem" />
[Serializable]
[MessagePackObject]
public class SignalDemodDemodResult : ISignalDemodItem
{
    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public SignalDemodResultItem[] Data { get; set; }

    /// <summary>
    ///     Gets the type.
    /// </summary>
    /// <value>The type.</value>
    [Key("type")]
    public SignalDemodType Type => SignalDemodType.DemodResult;
}

/// <summary>
///     Class SignalDemodBusinessTypeResult.
///     Implements the <see cref="Magneto.Protocol.Data.ISignalDemodItem" />
/// </summary>
/// <seealso cref="Magneto.Protocol.Data.ISignalDemodItem" />
[Serializable]
[MessagePackObject]
public class SignalDemodBusinessTypeResult : ISignalDemodItem
{
    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public SignalDemodResultItem[] Data { get; set; }

    /// <summary>
    ///     Gets the type.
    /// </summary>
    /// <value>The type.</value>
    [Key("type")]
    public SignalDemodType Type => SignalDemodType.BusinessTypeResult;
}

/// <summary>
///     Class SignalDemodResultItem.
/// </summary>
[Serializable]
[MessagePackObject]
public class SignalDemodResultItem
{
    /// <summary>
    ///     Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    [Key("name")]
    public string Name { get; set; }

    /// <summary>
    ///     Gets or sets the unit.
    /// </summary>
    /// <value>The unit.</value>
    [Key("unit")]
    public string Unit { get; set; }

    /// <summary>
    ///     Gets or sets the value.
    /// </summary>
    /// <value>The value.</value>
    [Key("value")]
    public string Value { get; set; }

    /// <summary>
    ///     Gets or sets the values.
    /// </summary>
    /// <value>The values.</value>
    [Key("values")]
    public List<SignalDemodResultItem> Values { get; set; }

    /// <summary>
    ///     Gets or sets the description.
    /// </summary>
    /// <value>The description.</value>
    [Key("description")]
    public string Description { get; set; }
}

/// <summary>
///     Class SDataIQRecordNotice.
///     Implements the <see cref="Magneto.Protocol.Data.SDataRaw" />
/// </summary>
/// <seealso cref="Magneto.Protocol.Data.SDataRaw" />
[Serializable]
[MessagePackObject]
public class SDataIqRecordNotice : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataIqRecordNotice" /> class.
    /// </summary>
    public SDataIqRecordNotice()
    {
        Type = SDataType.IqRecordNotice;
    }

    /// <summary>
    ///     Gets or sets the name of the iq file.
    /// </summary>
    /// <value>The name of the iq file.</value>
    [Key("iqFileName")]
    public string IqFileName { get; set; }
}

/// <summary>
///     Class SDataSignalDecodeNotice.
///     Implements the <see cref="Magneto.Protocol.Data.SDataRaw" />
/// </summary>
/// <seealso cref="Magneto.Protocol.Data.SDataRaw" />
[Serializable]
[MessagePackObject]
public class SDataSignalDecodeNotice : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataSignalDecodeNotice" /> class.
    /// </summary>
    public SDataSignalDecodeNotice()
    {
        Type = SDataType.SignalDecodeNotice;
    }

    /// <summary>
    ///     Gets or sets the task identifier.
    /// </summary>
    /// <value>The task identifier.</value>
    [Key("taskId")]
    public Guid TaskId { get; set; }

    /// <summary>
    ///     Gets or sets the name of the file.
    /// </summary>
    /// <value>The name of the file.</value>
    [Key("fileName")]
    public string FileName { get; set; }

    /// <summary>
    ///     Gets or sets the feature.
    /// </summary>
    /// <value>The feature.</value>
    [Key("feature")]
    public FeatureType Feature { get; set; }
}

/// <summary>
///     无人机报警信息：详细说明（定位信息）。
/// </summary>
[Serializable]
[MessagePackObject]
public class SDataDjiFlightInfoStr : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataDjiFlightInfoStr" /> class.
    /// </summary>
    public SDataDjiFlightInfoStr()
    {
        Type = SDataType.SecurityAlarm;
    }

    /// <summary>
    ///     飞手纬度
    /// </summary>
    [Key("pilotLatitude")]
    public double PilotLatitude { get; set; }

    /// <summary>
    ///     飞手经度
    /// </summary>
    [Key("pilotLongitude")]
    public double PilotLongitude { get; set; }

    /// <summary>
    ///     only for ver1
    /// </summary>
    [Key("pitchAngle")]
    public short PitchAngle { get; set; }

    /// <summary>
    ///     产品型号数字代号
    /// </summary>
    [Key("ProductType")]
    public byte ProductType { get; set; }

    /// <summary>
    ///     产品型号字符串
    /// </summary>
    [Key("productTypeStr")]
    public string ProductTypeStr { get; set; } = new(new char[32]);

    /// <summary>
    ///     only for ver1
    /// </summary>
    [Key("rollAngle")]
    public short RollAngle { get; set; }

    /// <summary>
    ///     编号
    /// </summary>
    [Key("seqNum")]
    public ushort SeqNum { get; set; }

    /// <summary>
    ///     站信息
    /// </summary>
    [Key("stateInfo")]
    public ushort StateInfo { get; set; }

    /// <summary>
    ///     向上速度
    /// </summary>
    [Key("upSpeed")]
    public float UpSpeed { get; set; }

    /// <summary>
    ///     uuid
    /// </summary>
    [Key("uuid")]
    public string Uuid { get; set; }

    /// <summary>
    ///     UuidLength
    /// </summary>
    [Key("uuidLength")]
    public byte UuidLength { get; set; }

    /// <summary>
    ///     The yaw angle
    /// </summary>
    [Key("yawAngle")]
    public short YawAngle { get; set; }

    /// <summary>
    ///     海拔
    /// </summary>
    /// <value>The altitude.</value>
    [Key("altitude")]
    public float Altitude { get; set; }

    /// <summary>
    ///     无人机纬度
    /// </summary>
    /// <value>The drone latitude.</value>
    [Key("droneLatitude")]
    public double DroneLatitude { get; set; }

    /// <summary>
    ///     无人机经度
    /// </summary>
    /// <value>The drone longitude.</value>
    [Key("droneLongitude")]
    public double DroneLongitude { get; set; }

    /// <summary>
    ///     唯一序列号
    /// </summary>
    /// <value>The drone serial number.</value>
    [Key("droneSerialNum")]
    public string DroneSerialNum { get; set; }

    /// <summary>
    ///     向东速度
    /// </summary>
    /// <value>The east speed.</value>
    [Key("eastSpeed")]
    public float EastSpeed { get; set; }

    /// <summary>
    ///     GPS时间
    /// </summary>
    /// <value>The GPS time.</value>
    [Key("gpsTime")]
    public ulong GpsTime { get; set; }

    /// <summary>
    ///     高度
    /// </summary>
    /// <value>The height.</value>
    [Key("height")]
    public float Height { get; set; }

    /// <summary>
    ///     返航点纬度
    /// </summary>
    /// <value>The home latitude.</value>
    [Key("homeLatitude")]
    public double HomeLatitude { get; set; }

    /// <summary>
    ///     返航点经度
    /// </summary>
    /// <value>The home longitude.</value>
    [Key("homeLongitude")]
    public double HomeLongitude { get; set; }

    /// <summary>
    ///     The license
    /// </summary>
    /// <value>The license.</value>
    [Key("license")]
    public byte[] License { get; set; } = new byte[10];

    /// <summary>
    ///     向北速度
    /// </summary>
    /// <value>The north speed.</value>
    [Key("northSpeed")]
    public float NorthSpeed { get; set; }

    /// <summary>
    ///     Gets or sets the type of the packet.
    /// </summary>
    /// <value>The type of the packet.</value>
    [Key("packetType")]
    public ushort PacketType { get; set; }

    /// <summary>
    /// 是否在白名单中
    /// </summary>
    [Key("isWhite")]
    public bool IsWhite { get; set; }
}

/// <summary>
///     无人机报警数据。
/// </summary>
[Serializable]
[MessagePackObject]
public class AlarmMessage : SDataRaw
{
    /// <summary>
    ///     使用安防报警。
    /// </summary>
    public AlarmMessage()
    {
        Type = SDataType.SecurityAlarm;
    }

    /// <summary>
    ///     概述，关键信息
    /// </summary>
    /// <value>The message.</value>
    [Key("message")]
    public string Message { get; set; }

    /// <summary>
    ///     Gets or sets the timestamp.
    /// </summary>
    /// <value>The timestamp.</value>
    [Key("timeStamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    ///     Gets or sets the status.
    /// </summary>
    /// <value>The status.</value>
    [Key("alarmStatus")]
    public AlarmStatus Status { get; set; }

    /// <summary>
    ///     信息来自于哪个设备或功能
    /// </summary>
    /// <value>The source.</value>
    [Key("source")]
    public string Source { get; set; }

    /// <summary>
    ///     严重程度
    /// </summary>
    /// <value>The severity.</value>
    [Key("severity")]
    public string Severity { get; set; }

    /// <summary>
    ///     简单的备注
    /// </summary>
    /// <value>The description.</value>
    [Key("description")]
    public string Description { get; set; }

    /// <summary>
    ///     设备报警信息的专用结构体，使用设备或功能内部使用的报警信息
    /// </summary>
    /// <value>The details.</value>
    [Key("details")]
    public List<object> Details { get; set; }
}