using System;

namespace Magneto.Driver.ESE;

/// <summary>
///     单个频段的模板信息
/// </summary>
public class TemplateInfo : ICloneable
{
    /// <summary>
    ///     模板ID
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     模板名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     描述信息
    /// </summary>
    public string Description { get; set; }

    public ulong FirstTime { get; set; }
    public ulong LastTime { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double StartFrequency { get; set; }
    public double StopFrequency { get; set; }
    public double StepFrequency { get; set; }
    public float[] MaxLevel { get; set; }
    public float[] AveLevel { get; set; }
    public float[] Threshold { get; set; }
    public float[] Signals { get; set; }
    public double[] Frequencies { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }
}