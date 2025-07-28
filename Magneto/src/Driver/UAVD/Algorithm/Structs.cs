using System;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace Magneto.Driver.UAVD.Algorithm;

/// <summary>
///     无人机模版结构
/// </summary>
//[StructLayout(LayoutKind.Sequential)]
//public struct Drone
//{
//    /// <summary>
//    /// 无人机名称
//    /// </summary>
//    [MarshalAs(UnmanagedType.LPStr)]
//    public string Name;
//    /// <summary>
//    /// 无人机特征值，总共九个，分别为：起始频率（MHz），结束频率（MHz），带宽下限（kHz），带宽上限（kHz），
//    /// 跳频步进1（MHz），跳频步进2（MHz）跳频步进确认次数，脉冲间隔（帧数），脉冲间隔确认次数，从配置文件或配置项读取
//    /// </summary>
//    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9, ArraySubType = UnmanagedType.R4)]
//    public float[] Characters;
//    public override string ToString()
//    {
//        var characters = string.Join(", ", Characters);
//        return string.Format($"{Name}; {characters}");
//    }
//}
[StructLayout(LayoutKind.Sequential)]
public struct DroneEx
{
    public uint Flag;

    /// <summary>
    ///     无人机名称
    /// </summary>
    [MarshalAs(UnmanagedType.LPStr)] public string Name;

    [MarshalAs(UnmanagedType.LPStr)] public string Link;
    [MarshalAs(UnmanagedType.LPStr)] public string Band;
    public float StartFrequency;
    public float StopFrequency;
    public float StepFrequency;
    public float Bandwidth;
    public int Hopping;
    public int HoppingStep1;
    public int HoppingStep2;
    public int HoppingStepTimes;
    public int HoppingPeriod;
    public int HoppingInterval;
    public int HoppingIntervalTimes;
    public int MaskMatchTimes;
    public int MaskMatchRate;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public int[] Mask1;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public float[] Mask2;

    public int MaskLength;
}

[Serializable]
public class DroneModel
{
    [JsonProperty("flag")] public uint Flag { get; set; }

    [JsonProperty("name")] public string Name { get; set; }

    [JsonProperty("link")] public string Link { get; set; }

    [JsonProperty("band")] public string Band { get; set; }

    [JsonProperty("start_frequency")] public float StartFrequency { get; set; }

    [JsonProperty("stop_frequency")] public float StopFrequency { get; set; }

    [JsonProperty("step_frequency")] public float StepFrequency { get; set; }

    [JsonProperty("bandwidth")] public float Bandwidth { get; set; }

    [JsonProperty("hopping")] public int Hopping { get; set; }

    [JsonProperty("hopping_step1")] public int HoppingStep1 { get; set; }

    [JsonProperty("hopping_step2")] public int HoppingStep2 { get; set; }

    [JsonProperty("hopping_step_times")] public int HoppingStepTimes { get; set; }

    [JsonProperty("hopping_period")] public int HoppingPeriod { get; set; }

    [JsonProperty("hopping_interval")] public int HoppingInterval { get; set; }

    [JsonProperty("hopping_interval_times")]
    public int HoppingIntervalTimes { get; set; }

    [JsonProperty("mask_match_times")] public int MaskMatchTimes { get; set; }

    [JsonProperty("mask_match_rate")] public int MaskMatchRate { get; set; }

    [JsonProperty("mask1")] public int[] Mask1 { get; set; }

    [JsonProperty("mask2")] public float[] Mask2 { get; set; }

    [JsonProperty("mask_length")] public int MaskLength { get; set; }

    public DroneEx ToDrone()
    {
        const int size = 64;
        var mask1 = new int[size];
        var mask2 = new float[size];
        Array.Copy(Mask1, mask1, Math.Min(Mask1.Length, size));
        Array.Copy(Mask2, mask2, Math.Min(Mask2.Length, size));
        return new DroneEx
        {
            Flag = Flag,
            Name = Name,
            Link = Link,
            Band = Band,
            StartFrequency = StartFrequency,
            StopFrequency = StopFrequency,
            StepFrequency = StepFrequency,
            Hopping = Hopping,
            HoppingStep1 = HoppingStep1,
            HoppingStep2 = HoppingStep2,
            HoppingStepTimes = HoppingStepTimes,
            HoppingPeriod = HoppingPeriod,
            HoppingInterval = HoppingInterval,
            HoppingIntervalTimes = HoppingIntervalTimes,
            MaskMatchTimes = MaskMatchTimes,
            MaskMatchRate = MaskMatchRate,
            Mask1 = mask1,
            Mask2 = mask2,
            Bandwidth = Bandwidth,
            MaskLength = MaskLength
        };
    }
}

/// <summary>
///     频谱数据
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Spectra
{
    /// <summary>
    ///     频谱长度
    /// </summary>
    public int Length;

    /// <summary>
    ///     频谱数据头指针
    /// </summary>
    public IntPtr Data;
}