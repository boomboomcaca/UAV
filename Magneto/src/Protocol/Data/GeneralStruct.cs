// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="GeneralStruct.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using Magneto.Protocol.Define;
using MessagePack;
using Newtonsoft.Json;

namespace Magneto.Protocol.Data;

#region 结构体

/// <summary>
///     Struct SegmentOccupancyData
/// </summary>
[MessagePackObject]
public struct SegmentOccupancyData
{
    /// <summary>
    ///     频段索引
    /// </summary>
    /// <value>The index of the segment.</value>
    [Key("index")]
    public int SegmentIndex { get; set; }

    /// <summary>
    ///     本频段的总占用度
    /// </summary>
    /// <value>The total occupancy.</value>
    [Key("totalOccupancy")]
    public double TotalOccupancy { get; set; }

    /// <summary>
    ///     占用度数据
    /// </summary>
    /// <value>The occupancy.</value>
    [Key("occupancy")]
    public short[] Occupancy { get; set; }

    /// <summary>
    ///     信噪比
    /// </summary>
    /// <value>The SNR.</value>
    [Key("snr")]
    public float[] Snr { get; set; }
}

/// <summary>
///     Struct RunningTaskInfo
/// </summary>
[MessagePackObject]
public struct RunningTaskInfo
{
    /// <summary>
    ///     任务ID
    /// </summary>
    /// <value>The identifier.</value>
    [Key("id")]
    public Guid Id { get; set; }

    /// <summary>
    ///     设备ID
    /// </summary>
    /// <value>The device identifier.</value>
    [Key("deviceId")]
    public Guid DeviceId { get; set; }

    /// <summary>
    ///     前端模块ID
    /// </summary>
    /// <value>The plugin identifier.</value>
    [Key("pluginId")]
    public string PluginId { get; set; }

    /// <summary>
    ///     功能ID
    /// </summary>
    /// <value>The module identifier.</value>
    [Key("moduleId")]
    public Guid ModuleId { get; set; }

    /// <summary>
    ///     计划ID
    /// </summary>
    /// <value>The crond identifier.</value>
    [Key("crondId")]
    public string CrondId { get; set; }

    /// <summary>
    ///     任务名称
    /// </summary>
    /// <value>The name.</value>
    [Key("name")]
    public string Name { get; set; }

    /// <summary>
    ///     主要参数集合
    /// </summary>
    /// <value>The major parameters.</value>
    [Key("majorParameters")]
    public List<SimpleParameter> MajorParameters { get; set; }

    /// <summary>
    ///     所有参数集合
    /// </summary>
    /// <value>The parameters.</value>
    [Key("parameters")]
    public List<SimpleParameter> Parameters { get; set; }

    /// <summary>
    ///     任务入口
    /// </summary>
    /// <value>The URI.</value>
    [Key("uri")]
    public string Uri { get; set; }

    /// <summary>
    ///     任务开始时间
    /// </summary>
    /// <value>The start time.</value>
    [Key("startTime")]
    public ulong StartTime { get; set; }

    /// <summary>
    ///     任务停止时间
    /// </summary>
    /// <value>The stop time.</value>
    [Key("stopTime")]
    public ulong? StopTime { get; set; }

    /// <summary>
    ///     执行时长
    /// </summary>
    /// <value>The work time.</value>
    [Key("workTime")]
    public int WorkTime { get; set; }

    /// <summary>
    ///     操作用户账户
    /// </summary>
    /// <value>The account.</value>
    [Key("account")]
    public string Account { get; set; }

    /// <summary>
    ///     本任务的天线因子
    /// </summary>
    /// <value>The factor.</value>
    [Key("factor")]
    public List<SDataFactor> Factor { get; set; }

    /// <summary>
    ///     天线描述信息
    /// </summary>
    /// <value>The antenna description.</value>
    [Key("antennaDescription")]
    public string AntennaDescription { get; set; }

    /// <summary>
    ///     Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
    public override string ToString()
    {
        return $"taskID:{Id},start:{StartTime},stop:{StopTime},work:{WorkTime}";
    }
}

/// <summary>
///     Struct EnvInfo
/// </summary>
[MessagePackObject]
public struct EnvInfo
{
    /// <summary>
    ///     名称
    /// </summary>
    /// <value>The name.</value>
    [Key("name")]
    public string Name { get; set; }

    /// <summary>
    ///     显示名称
    /// </summary>
    /// <value>The display.</value>
    [Key("display")]
    public string Display { get; set; }

    /// <summary>
    ///     单位
    /// </summary>
    /// <value>The unit.</value>
    [Key("unit")]
    public string Unit { get; set; }

    /// <summary>
    ///     值
    /// </summary>
    /// <value>The value.</value>
    [Key("value")]
    public object Value { get; set; }

    /// <summary>
    ///     信息
    /// </summary>
    /// <value>The message.</value>
    [Key("message")]
    public string Message { get; set; }
}

/// <summary>
///     Struct RecognizeItem
/// </summary>
[MessagePackObject]
public struct RecognizeItem
{
    /// <summary>
    ///     调制类型
    /// </summary>
    /// <value>The name.</value>
    [Key("name")]
    public string Name { get; set; }

    /// <summary>
    ///     调制类型描述信息
    /// </summary>
    /// <value>The description.</value>
    [Key("description")]
    public string Description { get; set; }

    /// <summary>
    ///     占比 %
    /// </summary>
    /// <value>The percent.</value>
    [Key("percent")]
    public double Percent { get; set; }
}

/// <summary>
///     Struct SegmentTemplateData
/// </summary>
[MessagePackObject]
public struct SegmentTemplateData
{
    /// <summary>
    ///     频段索引
    /// </summary>
    /// <value>The index of the segment.</value>
    [Key("segmentIndex")]
    [JsonProperty("segmentIndex")]
    public int SegmentIndex { get; set; }

    /// <summary>
    ///     起始频率 MHz
    /// </summary>
    /// <value>The start frequency.</value>
    [Key("startFrequency")]
    [JsonProperty("startFrequency")]
    public double StartFrequency { get; set; }

    /// <summary>
    ///     结束频率 MHz
    /// </summary>
    /// <value>The stop frequency.</value>
    [Key("stopFrequency")]
    [JsonProperty("stopFrequency")]
    public double StopFrequency { get; set; }

    /// <summary>
    ///     频率步进 KHz
    /// </summary>
    /// <value>The step frequency.</value>
    [Key("stepFrequency")]
    [JsonProperty("stepFrequency")]
    public double StepFrequency { get; set; }

    /// <summary>
    ///     最大值 dBμV
    /// </summary>
    /// <value>The maximum.</value>
    [Key("maximum")]
    [JsonProperty("maximum")]
    public float[] Maximum { get; set; }

    /// <summary>
    ///     均值 dBμV
    /// </summary>
    /// <value>The average.</value>
    [Key("average")]
    [JsonProperty("average")]
    public float[] Average { get; set; }

    /// <summary>
    ///     门限 dBμV
    /// </summary>
    /// <value>The threshold.</value>
    [Key("threshold")]
    [JsonProperty("threshold")]
    public float[] Threshold { get; set; }

    /// <summary>
    ///     最大值-门限 dBμV
    /// </summary>
    /// <value>The signals.</value>
    [Key("signals")]
    [JsonProperty("signals")]
    public float[] Signals { get; set; }
}

/// <summary>
///     新信号截获比对出的信号集合
/// </summary>
[MessagePackObject]
public struct SegmentResultData
{
    /// <summary>
    ///     频段索引
    /// </summary>
    /// <value>The index of the segment.</value>
    [Key("segmentIndex")]
    public int SegmentIndex { get; set; }

    /// <summary>
    ///     频段信息
    /// </summary>
    /// <value>The results.</value>
    [Key("segmentInfo")]
    public List<FrequencyResult> Results { get; set; }
}

/// <summary>
///     考试保障的比对结果
/// </summary>
[MessagePackObject]
public struct EseSegmentResultData
{
    /// <summary>
    ///     频段索引
    /// </summary>
    /// <value>The index of the segment.</value>
    [Key("segmentIndex")]
    public int SegmentIndex { get; set; }

    /// <summary>
    ///     频段信息
    /// </summary>
    /// <value>The results.</value>
    [Key("segmentInfo")]
    public List<EseFrequencyResult> Results { get; set; }
}

/// <summary>
///     Struct SignalsData
/// </summary>
[MessagePackObject]
public struct SignalsData
{
    /// <summary>
    ///     频段索引
    /// </summary>
    /// <value>The index of the segment.</value>
    [Key("segmentIndex")]
    public int SegmentIndex { get; set; }

    /// <summary>
    ///     频段信息
    /// </summary>
    /// <value>The results.</value>
    [Key("segmentInfo")]
    public List<SignalsResult> Results { get; set; }
}

/// <summary>
///     Struct FreeSignalsData
/// </summary>
[MessagePackObject]
public struct FreeSignalsData
{
    /// <summary>
    ///     频段索引
    /// </summary>
    /// <value>The index of the segment.</value>
    [Key("segmentIndex")]
    public int SegmentIndex { get; set; }

    /// <summary>
    ///     频段信息
    /// </summary>
    /// <value>The results.</value>
    [Key("segmentInfo")]
    public List<FreeSignalsResult> Results { get; set; }
}

/// <summary>
///     新信号截获比对出的信号信息
/// </summary>
[MessagePackObject]
public struct FrequencyResult
{
    /// <summary>
    ///     频点索引
    /// </summary>
    /// <value>The index of the frequency.</value>
    [Key("freqIndex")]
    public int FrequencyIndex { get; set; }

    /// <summary>
    ///     频点频率值
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    ///     估测带宽
    /// </summary>
    /// <value>The bandwidth.</value>
    [Key("bandwidth")]
    public double Bandwidth { get; set; }

    /// <summary>
    ///     第一次捕获时间
    /// </summary>
    /// <value>The first time.</value>
    [Key("firstTime")]
    public ulong FirstTime { get; set; }

    /// <summary>
    ///     最后一次捕获时间
    /// </summary>
    /// <value>The last time.</value>
    [Key("lastTime")]
    public ulong LastTime { get; set; }

    /// <summary>
    ///     最大电平值
    /// </summary>
    /// <value>The maximum level.</value>
    [Key("maxLevel")]
    public float MaxLevel { get; set; }

    /// <summary>
    ///     均值电平
    /// </summary>
    /// <value>The average level.</value>
    [Key("avgLevel")]
    public float AvgLevel { get; set; }

    /// <summary>
    ///     信号是否正在发射
    /// </summary>
    /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
    [Key("isActive")]
    public bool IsActive { get; set; }

    /// <summary>
    ///     计算结果
    /// </summary>
    /// <value>The result.</value>
    [Key("result")]
    public string Result { get; set; }

    /// <summary>
    ///     信号名称
    /// </summary>
    /// <value>The name.</value>
    [Key("name")]
    public string Name { get; set; }
}

/// <summary>
///     考试保障的比对信号信息
/// </summary>
[MessagePackObject]
public struct EseFrequencyResult
{
    /// <summary>
    ///     频点索引
    /// </summary>
    /// <value>The index of the frequency.</value>
    [Key("freqIndex")]
    public int FrequencyIndex { get; set; }

    /// <summary>
    ///     频点频率值
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    ///     估测带宽
    /// </summary>
    /// <value>The bandwidth.</value>
    [Key("bandwidth")]
    public double Bandwidth { get; set; }

    /// <summary>
    ///     第一次捕获时间
    /// </summary>
    /// <value>The first time.</value>
    [Key("firstTime")]
    public ulong FirstTime { get; set; }

    /// <summary>
    ///     最后一次捕获时间
    /// </summary>
    /// <value>The last time.</value>
    [Key("lastTime")]
    public ulong LastTime { get; set; }

    /// <summary>
    ///     最大电平值
    /// </summary>
    /// <value>The maximum level.</value>
    [Key("maxLevel")]
    public float MaxLevel { get; set; }

    /// <summary>
    ///     均值电平
    /// </summary>
    /// <value>The average level.</value>
    [Key("avgLevel")]
    public float AvgLevel { get; set; }

    /// <summary>
    ///     信号是否正在发射
    /// </summary>
    /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
    [Key("isActive")]
    public bool IsActive { get; set; }

    /// <summary>
    ///     计算结果
    /// </summary>
    /// <value>The result.</value>
    [Key("result")]
    public string Result { get; set; }

    /// <summary>
    ///     信号名称
    /// </summary>
    /// <value>The name.</value>
    [Key("name")]
    public string Name { get; set; }

    /// <summary>
    ///     解码结果
    /// </summary>
    /// <value>The decoder.</value>
    [Key("decoder")]
    public string Decoder { get; set; }
}

/// <summary>
///     信号结果数据
/// </summary>
[MessagePackObject]
public struct SignalsResult
{
    /// <summary>
    ///     频点索引
    /// </summary>
    /// <value>The index of the frequency.</value>
    [Key("freqIndex")]
    [JsonProperty("freqIndex")]
    public int FrequencyIndex { get; set; }

    /// <summary>
    ///     频点频率值
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    [JsonProperty("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    ///     估测带宽
    /// </summary>
    /// <value>The bandwidth.</value>
    [Key("bandwidth")]
    [JsonProperty("bandwidth")]
    public double Bandwidth { get; set; }

    /// <summary>
    ///     第一次捕获时间
    /// </summary>
    /// <value>The first time.</value>
    [Key("firstTime")]
    [JsonProperty("firstTime")]
    public ulong FirstTime { get; set; }

    /// <summary>
    ///     最后一次捕获时间
    /// </summary>
    /// <value>The last time.</value>
    [Key("lastTime")]
    [JsonProperty("lastTime")]
    public ulong LastTime { get; set; }

    /// <summary>
    ///     最大电平值
    /// </summary>
    /// <value>The maximum level.</value>
    [Key("maxLevel")]
    [JsonProperty("maxLevel")]
    public float MaxLevel { get; set; }

    /// <summary>
    ///     均值电平
    /// </summary>
    /// <value>The average level.</value>
    [Key("avgLevel")]
    [JsonProperty("avgLevel")]
    public float AvgLevel { get; set; }

    /// <summary>
    ///     信号是否正在发射
    /// </summary>
    /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
    [Key("isActive")]
    [JsonProperty("isActive")]
    public bool IsActive { get; set; }

    /// <summary>
    ///     计算结果
    /// </summary>
    /// <value>The result.</value>
    [Key("result")]
    [JsonProperty("result")]
    public string Result { get; set; }

    /// <summary>
    ///     信号名称
    /// </summary>
    /// <value>The name.</value>
    [Key("name")]
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    ///     Gets or sets the occupancy.
    /// </summary>
    /// <value>The occupancy.</value>
    [Key("occupancy")]
    [JsonProperty("occupancy")]
    public double Occupancy { get; set; }
}

/// <summary>
///     根据门限统计出来的信号信息。
/// </summary>
[MessagePackObject]
public struct Signal
{
    /// <summary>
    ///     信号标识
    /// </summary>
    [Key("guid")]
    public Guid Guid { get; set; }

    /// <summary>
    ///     表示频段索引号，适用于多频段扫描的情况，索引从0开始。
    /// </summary>
    /// <value>The segment offset.</value>
    [Key("segmentIdx")]
    public int SegmentIdx { get; set; }

    /// <summary>
    ///     信号索引
    /// </summary>
    [Key("freqIdxs")]
    public (int StartFreqIdx, int StopFreqIdx) FreqIdxs { get; set; }

    /// <summary>
    ///     最优示向度
    /// </summary>
    [Key("azimuth")]
    public float Azimuth { get; set; }
}

/// <summary>
///     空闲频点数据
/// </summary>
[MessagePackObject]
public struct FreeSignalsResult
{
    /// <summary>
    ///     频点索引
    /// </summary>
    /// <value>The index of the frequency.</value>
    [Key("freqIndex")]
    [JsonProperty("freqIndex")]
    public int FrequencyIndex { get; set; }

    /// <summary>
    ///     频点频率值
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    [JsonProperty("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    ///     估测带宽
    /// </summary>
    /// <value>The bandwidth.</value>
    [Key("bandwidth")]
    [JsonProperty("bandwidth")]
    public double Bandwidth { get; set; }

    /// <summary>
    ///     信号名称
    /// </summary>
    /// <value>The name.</value>
    [Key("name")]
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    ///     占用度
    /// </summary>
    /// <value>The occupancy.</value>
    [Key("occupancy")]
    [JsonProperty("occupancy")]
    public double Occupancy { get; set; }
}

/// <summary>
///     ITU杂项，所有ITU子项均在这里面汇总，发送到前端的时候采用键值对的方式发送
///     需要过滤掉null值
///     内部使用
/// </summary>
[MessagePackObject]
public class ItuMisc
{
    /// <summary>
    ///     测量的中心频率 MHz
    /// </summary>
    /// <value>The frequency stat.</value>
    [Key(ParameterNames.ItuFrequency)]
    public ItuStatData FrequencyStat { get; set; }

    /// <summary>
    ///     电平值 dBμV
    /// </summary>
    /// <value>The level stat.</value>
    [Key(ParameterNames.ItuLevel)]
    public ItuStatData LevelStat { get; set; }

    /// <summary>
    ///     场强值 dBμV/m
    /// </summary>
    /// <value>The field strength stat.</value>
    [Key(ParameterNames.ItuStrength)]
    public ItuStatData FieldStrengthStat { get; set; }

    /// <summary>
    ///     AM调幅度 单位 %
    /// </summary>
    /// <value>The am depth stat.</value>
    [Key(ParameterNames.ItuAmDepth)]
    public ItuStatData AmDepthStat { get; set; }

    /// <summary>
    ///     AM+ 单位 %
    /// </summary>
    /// <value>The am depth position stat.</value>
    [Key(ParameterNames.ItuAmDepthPos)]
    public ItuStatData AmDepthPosStat { get; set; }

    /// <summary>
    ///     AM- 单位 %
    /// </summary>
    /// <value>The am depth neg stat.</value>
    [Key(ParameterNames.ItuAmDepthNeg)]
    public ItuStatData AmDepthNegStat { get; set; }

    /// <summary>
    ///     FM频偏 单位 kHz
    /// </summary>
    /// <value>The fm dev stat.</value>
    [Key(ParameterNames.ItuFmDev)]
    public ItuStatData FmDevStat { get; set; }

    /// <summary>
    ///     FM正频偏 单位 kHz
    /// </summary>
    /// <value>The fm dev position stat.</value>
    [Key(ParameterNames.ItuFmDevPos)]
    public ItuStatData FmDevPosStat { get; set; }

    /// <summary>
    ///     FM负频偏 单位 kHz
    /// </summary>
    /// <value>The fm dev neg stat.</value>
    [Key(ParameterNames.ItuFmDevNeg)]
    public ItuStatData FmDevNegStat { get; set; }

    /// <summary>
    ///     PM调制度 单位 rad
    /// </summary>
    /// <value>The pm depth stat.</value>
    [Key(ParameterNames.ItuPmDepth)]
    public ItuStatData PmDepthStat { get; set; }

    /// <summary>
    ///     XdB带宽 单位 kHz
    /// </summary>
    /// <value>The XDB stat.</value>
    [Key(ParameterNames.ItuXdb)]
    public ItuStatData XdbStat { get; set; }

    /// <summary>
    ///     百分比占用带宽 单位 kHz
    /// </summary>
    /// <value>The beta stat.</value>
    [Key(ParameterNames.ItuBeta)]
    public ItuStatData BetaStat { get; set; }

    /// <summary>
    ///     转换为字典
    /// </summary>
    /// <param name="sign">
    ///     子属性转换方式
    ///     0:不转换ITUStatData;
    ///     1:仅转换ITUStatData的值
    ///     2:将ITUStatData整体转换为字典
    /// </param>
    /// <returns>Dictionary&lt;System.String, System.Object&gt;.</returns>
    public Dictionary<string, object> ToDictionary(int sign = 0)
    {
        var dic = new Dictionary<string, object>();
        foreach (var property in GetType().GetProperties())
        {
            if (Attribute.GetCustomAttribute(property, typeof(KeyAttribute)) is not KeyAttribute keyAtt) continue;
            var name = keyAtt.StringKey;
            var value = property.GetValue(this);
            if (value is not ItuStatData data) continue;
            if (sign == 2)
            {
                var itu = data.ToDictionary();
                if (itu == null) continue;
                if (name != null) dic.Add(name, itu);
            }
            else if (sign == 1)
            {
                if (name != null) dic.Add(name, data.Value);
            }
            else
            {
                if (name != null) dic.Add(name, data);
            }
        }

        return dic;
    }
}

/// <summary>
///     Class ITUStatData.
/// </summary>
[MessagePackObject]
public class ItuStatData
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ItuStatData" /> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="unit">The unit.</param>
    public ItuStatData(string name, string unit)
    {
        Name = name;
        Unit = unit;
    }

    /// <summary>
    ///     Gets or sets the value.
    /// </summary>
    /// <value>The value.</value>
    [Key("value")]
    public double Value { get; set; }

    /// <summary>
    ///     Determines the maximum of the parameters.
    /// </summary>
    /// <value>The maximum.</value>
    [Key("max")]
    public double Max { get; set; } = double.MinValue;

    /// <summary>
    ///     Determines the minimum of the parameters.
    /// </summary>
    /// <value>The minimum.</value>
    [Key("min")]
    public double Min { get; set; } = double.MaxValue;

    /// <summary>
    ///     Gets or sets the average.
    /// </summary>
    /// <value>The average.</value>
    [Key("avg")]
    public double Avg { get; set; }

    /// <summary>
    ///     Gets or sets the RMS.
    /// </summary>
    /// <value>The RMS.</value>
    [Key("rms")]
    public double Rms { get; set; }

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
    ///     Gets or sets the count.
    /// </summary>
    /// <value>The count.</value>
    public int Count { get; set; }

    /// <summary>
    ///     Converts to dictionary.
    /// </summary>
    /// <returns>Dictionary&lt;System.String, System.Object&gt;.</returns>
    public Dictionary<string, object> ToDictionary()
    {
        var dic = new Dictionary<string, object>();
        foreach (var property in GetType().GetProperties())
        {
            if (Attribute.GetCustomAttribute(property, typeof(KeyAttribute)) is not KeyAttribute keyAtt) continue;
            var name = keyAtt.StringKey;
            var value = property.GetValue(this);
            if (value == null) continue;
            if (name != null) dic.Add(name, value);
        }

        return dic;
    }
}

/// <summary>
///     测向信号数据
/// </summary>
[MessagePackObject]
public struct DfSignalData
{
    /// <summary>
    ///     Gets or sets the index.
    /// </summary>
    /// <value>The index.</value>
    [Key("index")]
    [JsonProperty("index")]
    public int Index { get; set; }

    /// <summary>
    ///     Gets or sets the frequency.
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    [JsonProperty("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    ///     Gets or sets the bandwidth.
    /// </summary>
    /// <value>The bandwidth.</value>
    [Key("bandwidth")]
    [JsonProperty("bandwidth")]
    public double Bandwidth { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this instance is strength field.
    /// </summary>
    /// <value><c>true</c> if this instance is strength field; otherwise, <c>false</c>.</value>
    [Key("isStrengthField")]
    [JsonProperty("isStrengthField")]
    public bool IsStrengthField { get; set; }

    /// <summary>
    ///     Gets or sets the amplitude.
    /// </summary>
    /// <value>The amplitude.</value>
    [Key("amplitude")]
    [JsonProperty("amplitude")]
    public float Amplitude { get; set; }

    /// <summary>
    ///     Gets or sets the azimuth.
    /// </summary>
    /// <value>The azimuth.</value>
    [Key("azimuth")]
    [JsonProperty("azimuth")]
    public float Azimuth { get; set; }

    /// <summary>
    ///     Gets or sets the quality.
    /// </summary>
    /// <value>The quality.</value>
    [Key("quality")]
    [JsonProperty("quality")]
    public float Quality { get; set; }

    /// <summary>
    ///     Gets or sets the optimal azimuth.
    /// </summary>
    /// <value>The optimal azimuth.</value>
    [Key("optimalAzimuth")]
    [JsonProperty("optimalAzimuth")]
    public float OptimalAzimuth { get; set; }
}

/// <summary>
///     电视节目信息
/// </summary>
[MessagePackObject]
public struct ChannelProgramInfo
{
    /// <summary>
    ///     序号,从0开始
    /// </summary>
    /// <value>The index.</value>
    [Key("index")]
    public int Index { get; set; }

    /// <summary>
    ///     节目号
    /// </summary>
    /// <value>The program number.</value>
    [Key("programNumber")]
    public string ProgramNumber { get; set; }

    /// <summary>
    ///     当前频道的频率 MHz
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    ///     频道的电视制式
    /// </summary>
    /// <value>The standard.</value>
    [Key("standard")]
    public string Standard { get; set; }

    /// <summary>
    ///     CA加密标志
    /// </summary>
    /// <value><c>true</c> if ca; otherwise, <c>false</c>.</value>
    [Key("ca")]
    public bool Ca { get; set; }

    /// <summary>
    ///     节目名称
    /// </summary>
    /// <value>The name of the program.</value>
    [Key("programName")]
    public string ProgramName { get; set; }

    /// <summary>
    ///     节目提供商
    /// </summary>
    /// <value>The provider.</value>
    [Key("provider")]
    public string Provider { get; set; }

    /// <summary>
    ///     该节目的流类型
    /// </summary>
    /// <value>The type of the flow.</value>
    [Key("flowType")]
    public string FlowType { get; set; }

    /// <summary>
    ///     视频分辨率
    /// </summary>
    /// <value>The resolution.</value>
    [Key("resolution")]
    public string Resolution { get; set; }

    /// <summary>
    ///     电视节目缩略图
    /// </summary>
    /// <value>The thumbnail.</value>
    [Key("thumbnail")]
    public byte[] Thumbnail { get; set; }
}

/// <summary>
///     Struct DVRFileInfo
/// </summary>
[MessagePackObject]
public struct DvrFileInfo
{
    /// <summary>
    ///     Gets or sets the edge identifier.
    /// </summary>
    /// <value>The edge identifier.</value>
    [JsonProperty("edgeId")]
    [IgnoreMember]
    public string EdgeId { get; set; }

    /// <summary>
    ///     文件名
    /// </summary>
    /// <value>The name of the file.</value>
    [Key("fileName")]
    [JsonProperty("fileName")]
    public string FileName { get; set; }

    /// <summary>
    ///     制式
    /// </summary>
    /// <value>The standard.</value>
    [Key("standard")]
    [JsonProperty("standard")]
    public string Standard { get; set; }

    /// <summary>
    ///     节目名称
    /// </summary>
    /// <value>The name of the program.</value>
    [Key("programName")]
    [JsonProperty("programName")]
    public string ProgramName { get; set; }

    /// <summary>
    ///     当前频道的频率 MHz
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    [JsonProperty("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    ///     录制起始时间
    /// </summary>
    /// <value>The start time.</value>
    [Key("startTime")]
    [JsonProperty("startTime")]
    public ulong StartTime { get; set; }

    /// <summary>
    ///     Gets or sets the start time string.
    /// </summary>
    /// <value>The start time string.</value>
    [IgnoreMember]
    [JsonIgnore]
    public string StartTimeStr { get; set; }

    /// <summary>
    ///     录制结束时间
    /// </summary>
    /// <value>The stop time.</value>
    [Key("stopTime")]
    [JsonProperty("stopTime")]
    public ulong StopTime { get; set; }

    /// <summary>
    ///     Gets or sets the stop time string.
    /// </summary>
    /// <value>The stop time string.</value>
    [IgnoreMember]
    [JsonIgnore]
    public string StopTimeStr { get; set; }
}

/// <summary>
///     无人机数据
/// </summary>
[MessagePackObject]
public struct DroneData
{
    /// <summary>
    ///     Gets or sets the model.
    /// </summary>
    /// <value>The model.</value>
    [IgnoreMember]
    public string Model { get; set; }

    /// <summary>
    ///     Gets or sets the start frequency.
    /// </summary>
    /// <value>The start frequency.</value>
    [IgnoreMember]
    public double StartFrequency { get; set; }

    /// <summary>
    ///     Gets or sets the stop frequency.
    /// </summary>
    /// <value>The stop frequency.</value>
    [IgnoreMember]
    public double StopFrequency { get; set; }

    /// <summary>
    ///     Gets or sets the lower bandwidth.
    /// </summary>
    /// <value>The lower bandwidth.</value>
    [Key("lowerBandwidth")]
    [JsonProperty("lowerBandwidth")]
    public double LowerBandwidth { get; set; }

    /// <summary>
    ///     Gets or sets the upper bandwidth.
    /// </summary>
    /// <value>The upper bandwidth.</value>
    [Key("upperBandwidth")]
    [JsonProperty("upperBandwidth")]
    public double UpperBandwidth { get; set; }

    /// <summary>
    ///     Gets or sets the frequency hopping step.
    /// </summary>
    /// <value>The frequency hopping step.</value>
    [Key("frequencHoppingStep")]
    [JsonProperty("frequencHoppingStep")]
    public double FrequencyHoppingStep { get; set; }

    /// <summary>
    ///     Gets the frequency range.
    /// </summary>
    /// <value>The frequency range.</value>
    [Key("frequencyRange")]
    [JsonProperty("frequencyRange")]
    public string FrequencyRange => $"{StartFrequency}MHz-{StopFrequency}MHz";
}

/// <summary>
///     航班信息
/// </summary>
[MessagePackObject]
public struct FlightInfo
{
    /// <summary>
    ///     飞机地址
    /// </summary>
    /// <value>The plane address.</value>
    [Key("planeAddress")]
    public string PlaneAddress { get; set; }

    /// <summary>
    ///     航班号
    /// </summary>
    /// <value>The flight number.</value>
    [Key("flightNumber")]
    public string FlightNumber { get; set; }

    /// <summary>
    ///     国家
    /// </summary>
    /// <value>The country.</value>
    [Key("country")]
    public string Country { get; set; }

    /// <summary>
    ///     机龄 单位 年
    /// </summary>
    /// <value>The age.</value>
    [Key("age")]
    public double Age { get; set; }

    /// <summary>
    ///     机型
    /// </summary>
    /// <value>The model.</value>
    [Key("model")]
    public string Model { get; set; }

    /// <summary>
    ///     更新时间（时间戳）
    /// </summary>
    /// <value>The update time.</value>
    [Key("updateTime")]
    public ulong UpdateTime { get; set; }

    /// <summary>
    ///     经度 单位 °
    /// </summary>
    /// <value>The longitude.</value>
    [Key("longitude")]
    public double Longitude { get; set; }

    /// <summary>
    ///     纬度 单位 °
    /// </summary>
    /// <value>The latitude.</value>
    [Key("latitude")]
    public double Latitude { get; set; }

    /// <summary>
    ///     海拔高度 单位 米
    /// </summary>
    /// <value>The altitude.</value>
    [Key("altitude")]
    public double Altitude { get; set; }

    /// <summary>
    ///     水平速度 单位 km/h
    /// </summary>
    /// <value>The horizontal speed.</value>
    [Key("horizontalSpeed")]
    public double HorizontalSpeed { get; set; }

    /// <summary>
    ///     垂直速度 单位 m/s
    /// </summary>
    /// <value>The vertical speed.</value>
    [Key("verticalSpeed")]
    public double VerticalSpeed { get; set; }

    /// <summary>
    ///     方位角 单位 °
    /// </summary>
    /// <value>The azimuth.</value>
    [Key("azimuth")]
    public double Azimuth { get; set; }

    /// <summary>
    ///     应答机编码
    /// </summary>
    /// <value>The transponder code.</value>
    [Key("transponderCode")]
    public string TransponderCode { get; set; }
}

/// <summary>
///     Struct AISInfo
/// </summary>
[MessagePackObject]
public struct AisInfo
{
    /// <summary>
    ///     船舶名称
    /// </summary>
    /// <value>The name.</value>
    [Key("name")]
    public string Name { get; set; }

    /// <summary>
    ///     识别标识
    /// </summary>
    /// <value>The mmsi.</value>
    [Key("mmsi")]
    public string Mmsi { get; set; }

    /// <summary>
    ///     呼号
    /// </summary>
    /// <value>The callsign.</value>
    [Key("callsign")]
    public string Callsign { get; set; }

    /// <summary>
    ///     船首向 单位 °
    /// </summary>
    /// <value>The ship header.</value>
    [Key("shipHeader")]
    public double ShipHeader { get; set; }

    /// <summary>
    ///     航迹向 单位 °
    /// </summary>
    /// <value>The track header.</value>
    [Key("trackHeader")]
    public double TrackHeader { get; set; }

    /// <summary>
    ///     国际海事组织编号
    /// </summary>
    /// <value>The imo.</value>
    [Key("imo")]
    public string Imo { get; set; }

    /// <summary>
    ///     船舶速度 单位 节
    /// </summary>
    /// <value>The speed.</value>
    [Key("speed")]
    public double Speed { get; set; }

    /// <summary>
    ///     类型
    /// </summary>
    /// <value>The category.</value>
    [Key("category")]
    public string Category { get; set; }

    /// <summary>
    ///     国家
    /// </summary>
    /// <value>The country.</value>
    [Key("country")]
    public string Country { get; set; }

    /// <summary>
    ///     经度 单位 °
    /// </summary>
    /// <value>The longitude.</value>
    [Key("longitude")]
    public double Longitude { get; set; }

    /// <summary>
    ///     纬度 单位 °
    /// </summary>
    /// <value>The latitude.</value>
    [Key("latitude")]
    public double Latitude { get; set; }

    /// <summary>
    ///     长度 单位 米
    /// </summary>
    /// <value>The length.</value>
    [Key("length")]
    public double Length { get; set; }

    /// <summary>
    ///     宽度 单位 米
    /// </summary>
    /// <value>The width.</value>
    [Key("width")]
    public double Width { get; set; }

    /// <summary>
    ///     目的地
    /// </summary>
    /// <value>The destination.</value>
    [Key("destination")]
    public string Destination { get; set; }

    /// <summary>
    ///     状态
    /// </summary>
    /// <value>The state.</value>
    [Key("state")]
    public string State { get; set; }

    /// <summary>
    ///     航行中标记
    ///     true: 航行中
    ///     false: 静止状态
    /// </summary>
    /// <value><c>true</c> if underway; otherwise, <c>false</c>.</value>
    [Key("underway")]
    public bool Underway { get; set; }

    /// <summary>
    ///     吃水深度 单位 米
    /// </summary>
    /// <value>The draught.</value>
    [Key("draught")]
    public double Draught { get; set; }

    /// <summary>
    ///     到达时间 时间戳
    /// </summary>
    /// <value>The arrival time.</value>
    [Key("arrivalTime")]
    public ulong ArrivalTime { get; set; }

    /// <summary>
    ///     更新时间 时间戳
    /// </summary>
    /// <value>The update time.</value>
    [Key("updateTime")]
    public ulong UpdateTime { get; set; }
}

/// <summary>
///     电磁环境分析信号结构
/// </summary>
[MessagePackObject]
public struct EmdaInfo
{
    /// <summary>
    ///     信号序号
    /// </summary>
    /// <value>The index.</value>
    [Key("index")]
    public int Index { get; set; }

    /// <summary>
    ///     起始频率 MHz
    /// </summary>
    /// <value>The start frequency.</value>
    [Key("startFrequency")]
    public double StartFrequency { get; set; }

    /// <summary>
    ///     结束频率 MHz
    /// </summary>
    /// <value>The stop frequency.</value>
    [Key("stopFrequency")]
    public double StopFrequency { get; set; }

    /// <summary>
    ///     统计带宽 kHz
    /// </summary>
    /// <value>The bandwidth.</value>
    [Key("bandwidth")]
    public double Bandwidth { get; set; }

    /// <summary>
    ///     扫描步进 kHz
    /// </summary>
    /// <value>The step frequency.</value>
    [Key("stepFrequency")]
    public double StepFrequency { get; set; }

    /// <summary>
    ///     本频段的频点个数
    /// </summary>
    /// <value>The total.</value>
    [Key("total")]
    public int Total { get; set; }

    /// <summary>
    ///     最大电平 dBμV
    /// </summary>
    /// <value>The maximum level.</value>
    [Key("maxLevel")]
    public double MaxLevel { get; set; }

    /// <summary>
    ///     测量门限 dBμV
    /// </summary>
    /// <value>The threshold.</value>
    [Key("threshold")]
    public double Threshold { get; set; }

    /// <summary>
    ///     占用度 %
    /// </summary>
    /// <value>The occupancy.</value>
    [Key("occupancy")]
    public double Occupancy { get; set; }
}

/// <summary>
///     信号捕获结构
/// </summary>
[MessagePackObject]
public struct CaptureStruct
{
    /// <summary>
    ///     信号序号
    /// </summary>
    /// <value>The start time.</value>
    [Key("startTime")]
    public ulong StartTime { get; set; }

    /// <summary>
    ///     信号序号
    /// </summary>
    /// <value>The stop time.</value>
    [Key("stopTime")]
    public ulong StopTime { get; set; }
}

/// <summary>
///     离散扫描信号结果数据
/// </summary>
[MessagePackObject]
public struct MScanSignalsResult
{
    /// <summary>
    ///     频点索引
    /// </summary>
    /// <value>The index of the frequency.</value>
    [Key("freqIndex")]
    [JsonProperty("freqIndex")]
    public int FrequencyIndex { get; set; }

    /// <summary>
    ///     频点频率值
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    [JsonProperty("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    ///     估测带宽
    /// </summary>
    /// <value>The bandwidth.</value>
    [Key("bandwidth")]
    [JsonProperty("bandwidth")]
    public double Bandwidth { get; set; }

    /// <summary>
    ///     实时电平值
    /// </summary>
    /// <value>The level.</value>
    [Key("level")]
    [JsonProperty("level")]
    public float Level { get; set; }

    /// <summary>
    ///     最大电平值
    /// </summary>
    /// <value>The maximum level.</value>
    [Key("maxLevel")]
    [JsonProperty("maxLevel")]
    public float MaxLevel { get; set; }

    /// <summary>
    ///     最小电平
    /// </summary>
    /// <value>The minimum level.</value>
    [Key("minLevel")]
    [JsonProperty("minLevel")]
    public float MinLevel { get; set; }

    /// <summary>
    ///     调制方式
    /// </summary>
    /// <value>The modulation.</value>
    [Key("modulation")]
    [JsonProperty("modulation")]
    public Modulation Modulation { get; set; }

    /// <summary>
    ///     第一次捕获时间
    /// </summary>
    /// <value>The first time.</value>
    [Key("firstTime")]
    [JsonProperty("firstTime")]
    public ulong FirstTime { get; set; }

    /// <summary>
    ///     最后一次捕获时间
    /// </summary>
    /// <value>The last time.</value>
    [Key("lastTime")]
    [JsonProperty("lastTime")]
    public ulong LastTime { get; set; }

    /// <summary>
    ///     Gets or sets the occupancy.
    /// </summary>
    /// <value>The occupancy.</value>
    [Key("occupancy")]
    [JsonProperty("occupancy")]
    public float Occupancy { get; set; }
}

#endregion