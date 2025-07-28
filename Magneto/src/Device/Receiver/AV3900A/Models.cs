using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Magneto.Device.AV3900A.Common;

namespace Magneto.Device.AV3900A;

internal class SegmentInfo
{
    public uint NumFftPoints { get; set; }
    public uint NumPoints { get; set; }
    public uint StartPoint { get; set; }
    public double SampleRate { get; set; }
}

internal class SpectrumSegmentData
{
    public SalSegmentData Header { get; set; }
    public float[] Data { get; set; }
}

internal class AudioSegmentData
{
    public SalDemodData Header { get; set; }
    public int[] Data { get; set; }
}

internal class IqSegmentData
{
    public SalTimeData Header { get; set; }
    public short[] Data { get; set; }
}

public class GpsStatusInfo
{
    public GpsStatusInfo(uint timeAlarms)
    {
        var sTimeAlarms = Convert.ToString(timeAlarms, 2);
        if (sTimeAlarms.Length < 24) sTimeAlarms = sTimeAlarms.PadLeft(24, '0');
        var dic = new Dictionary<int, (int trueValue, PropertyInfo prop)>();
        var props = GetType().GetProperties();
        foreach (var prop in props)
        {
            if (prop.PropertyType != typeof(bool)) continue;
            var attr = prop.GetCustomAttribute<GpsAlarmAttribute>();
            if (attr == null) continue;
            dic.Add(attr.Index, (attr.TrueValue, prop));
        }

        for (var i = 0; i < sTimeAlarms.Length && i < 24; i++)
        {
            var index = sTimeAlarms.Length - i - 1;
            if (!dic.ContainsKey(index)) continue;
            var real = Convert.ToInt32(sTimeAlarms[i].ToString());
            var prop = dic[index].prop;
            var value = dic[index].trueValue == real;
            prop.SetValue(this, value);
        }
    }

    [GpsAlarm("天线开路", 1, 1)] public bool IsOpenCircuit { get; set; }

    [GpsAlarm("天线短路", 2, 1)] public bool IsShortCircuit { get; set; }

    [GpsAlarm("没有跟踪到卫星", 3, 1)] public bool SatellitesNotTracked { get; set; }

    [GpsAlarm("测量中", 5, 1)] public bool Measuring { get; set; }

    [GpsAlarm("方位信息未记录", 6, 1)] public bool PositionNotRecorded { get; set; }

    [GpsAlarm("跳秒", 7, 1)] public bool LeapSecond { get; set; }

    [GpsAlarm("测试模式", 8, 1)] public bool IsTest { get; set; }

    [GpsAlarm("方位可疑", 9, 1)] public bool SuspiciousPosition { get; set; }

    [GpsAlarm("星历未完成", 11, 1)] public bool EphemerisNotCompleted { get; set; }

    [GpsAlarm("发出秒脉冲", 12, 1)] public bool SendPps { get; set; }

    [GpsAlarm("Use UTC time", 16, 1)] public bool IsUtcTime { get; set; }

    [GpsAlarm("Use UTC PPS", 17, 1)] public bool IsUtcPps { get; set; }

    [GpsAlarm("时间已确定", 18)] public bool IsTimeDetermined { get; set; }

    [GpsAlarm("有UTC信息", 19)] public bool HasUtcInfo { get; set; }

    [GpsAlarm("Time form GPS", 20)] public bool IsTimeFromGps { get; set; }

    public bool IsGpsValid(out string gpsWarnnings)
    {
        gpsWarnnings = null;
        var valid = true;
        var props = GetType().GetProperties();
        var sb = new StringBuilder();
        foreach (var prop in props)
        {
            if (prop.PropertyType != typeof(bool)) continue;
            var attr = prop.GetCustomAttribute<GpsAlarmAttribute>();
            if (attr == null || attr.Index > 15 || attr.Index == 6 || attr.Index == 7 || attr.Index == 12) continue;
            var value = (bool)prop.GetValue(this, null)!;
            valid &= !value;
            if (value) sb.Append($"{attr.Description};");
        }

        if (!valid) gpsWarnnings = sb.ToString();
        return valid;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class GpsAlarmAttribute : Attribute
{
    public GpsAlarmAttribute(string description, int index, int trueValue = 0)
    {
        Description = description;
        Index = index;
        TrueValue = trueValue;
    }

    public string Description { get; set; }
    public int TrueValue { get; set; }
    public int Index { get; set; }
}