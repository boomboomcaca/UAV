using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Numerics;
using System.Text;
using Magneto.Contract;
using Magneto.Protocol.Define;

#pragma warning disable RCS1085, RCS1181
namespace Magneto.Device.MR5000A;

/// <summary>
///     测向天线信息类
/// </summary>
// [TypeConverter(typeof(PropertySorter))]
[DefaultProperty("")]
[Serializable]
internal class DfAntennaInfo
{
    private int _angleCount = 72;

    [Parameter]
    [Category("01.基础属性")]
    [DisplayName("天线名称")]
    [Description("设置天线名称")]
    [DefaultValue("未命名")]
    [PropertyOrder(0)]
    public string Name { get; set; }

    [Parameter]
    [Category("01.基础属性")]
    [DisplayName("天线编号")]
    [Description("设置天线序号，序号一般不超过255")]
    [ValueRange(0, 255)]
    [DefaultValue(0)]
    [PropertyOrder(1)]
    [Browsable(false)]
    public int Index { get; set; }

    [Parameter]
    [Category("01.基础属性")]
    [DisplayName("极化方式")]
    [Description("设置天线极化方式，特定极化方式的天线适于特定的极化波测向")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|Vertical|Horizontal",
        DisplayValues = "|垂直极化|水平极化")]
    [DefaultValue(Polarization.Vertical)]
    [PropertyOrder(2)]
    public Polarization Polarization { get; set; }

    [Parameter]
    [Category("01.基础属性")]
    [DisplayName("天线孔径")]
    [Description("设置天线孔径，单位：米")]
    [DefaultValue(3)]
    [PropertyOrder(3)]
    public float Aperture { get; set; }

    [Parameter]
    [Category("01.基础属性")]
    [DisplayName("安装夹角")]
    [Description("设置当前天线零号天线相对于地理正北的安装偏角，左偏为负，右偏为正")]
    [DefaultValue(0)]
    [PropertyOrder(4)]
    public int Deviation { get; set; }

    [Parameter]
    [Category("02.测向属性")]
    [DisplayName("起始频率")]
    [Description("设置适用的起始频率，单位：MHz")]
    [DefaultValue(20)]
    [PropertyOrder(5)]
    public int StartFrequency { get; set; }

    [Parameter]
    [Category("02.测向属性")]
    [DisplayName("结束频率")]
    [Description("设置适用的结束频率，单位：MHz")]
    [DefaultValue(200)]
    [PropertyOrder(6)]
    public int StopFrequency { get; set; }

    [Parameter]
    [Category("02.测向属性")]
    [DisplayName("频率步进")]
    [Description("设置收数频率步进，单位：MHz")]
    [DefaultValue(5)]
    [PropertyOrder(7)]
    public int StepFrequency { get; set; }

    [Browsable(false)]
    [Category("02.测向属性")]
    [DisplayName("角度数量")]
    [Description("设置收数时的角度数量")]
    public int AngleCount
    {
        get => _angleCount;
        set => _angleCount = value;
    }

    [Parameter]
    [Category("02.测向属性")]
    [DisplayName("角度步进")]
    [Description("设置收数角度进步")]
    [DefaultValue(5)]
    [PropertyOrder(8)]
    public int StepAngle
    {
        get => (int)(360.0f / _angleCount + 0.05);
        set => _angleCount = (int)(360.0f / value + 0.05);
    }

    [Parameter]
    [Category("02.测向属性")]
    [DisplayName("测向分组")]
    [Description("设置测向天线控制（码）分组，与采用的测向算法有关，例如：九单元单通道测向算法，此值为“32”，天线打通码数量为32个，同理，九单元双通道测向算法，此值为“9”，天线打通码数量为9个")]
    [DefaultValue(9)]
    [PropertyOrder(9)]
    public int GroupCount { get; set; }

    [Parameter]
    [Category("03.控制属性")]
    [DisplayName("关闭码")]
    [Description("设置天线关闭码, 8位十六进制数")]
    [DefaultValue("0x70")]
    [PropertyOrder(10)]
    public string OffCode { get; set; }

    [Parameter]
    [Category("03.控制属性")]
    [DisplayName("打通码")]
    [Description("设置天线打通码，8位十六进制数，每个控制码以“|”分隔")]
    [DefaultValue("0x01|0x02|0x03|0x04|0x05|0x06|0x07|0x08|0x09")]
    [PropertyOrder(11)]
    public string OnCode { get; set; }

    // 将上位机配置的天线信息转换为协议要求的参数格式，比如协议定义的是10进制字符串，但上位机用16进制更容易辨识
    public string ToProtocolString()
    {
        try
        {
            var offCodeString = Convert.ToByte(OffCode.Trim(), 16).ToString();
            var onCodeArray = OnCode.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var onCodeStringArray = new string[onCodeArray.Length];
            for (var index = 0; index < onCodeStringArray.Length; ++index)
                onCodeStringArray[index] = Convert.ToByte(onCodeArray[index].Trim(), 16).ToString();
            var onCodeString = string.Join(",", onCodeStringArray);
            return $"{Index},{GroupCount},{offCodeString},{onCodeString}";
        }
        catch
        {
            return string.Empty;
        }
    }

    public override string ToString()
    {
        return $"{Index},{GroupCount},{OffCode.Trim()},{OnCode.Trim()}";
    }

    // 将Json格式的天线配置信息转换为对应的天线数组
    public static DfAntennaInfo[] CreateAntennas(string filename)
    {
        if (!File.Exists(filename)) return null;
        var text = File.ReadAllText(filename, Encoding.Default);
        return Utils.ConvertFromJson<DfAntennaInfo[]>(text);
    }

    // 将DFAntennaInfo对象隐式转换为字符串
    public static implicit operator string(DfAntennaInfo antenna)
    {
        return antenna.ToString();
    }

    // 使用字典显示构造DFAntennaInfo对象（键/值-->属性/值）
    public static explicit operator DfAntennaInfo(Dictionary<string, object> dict)
    {
        if (dict == null) return null;
        var template = new DfAntennaInfo();
        var type = template.GetType();
        try
        {
            var properties = type.GetProperties();
            foreach (var property in properties)
                if (dict.TryGetValue(property.Name, out var value))
                    property.SetValue(template, value, null);
        }
        catch
        {
        }

        return template;
    }
}

internal interface IDFindCalibration
{
    float[] ReadData(double frequency);
}

internal interface IChannelCalibration
{
    float[] ReadData(double frequency, double bandwidth);
}

internal interface IBandCalibration
{
    int Total { get; }
    float[] ReadData(int index);
}

internal class DualDFindCalibrator : IDFindCalibration
{
    #region 构造函数

    private DualDFindCalibrator()
    {
    }

    #endregion

    #region 类方法

    public static DualDFindCalibrator CreateInstance(DfAntennaInfo antennaInfo, string foldername = "",
        bool createNewFile = false)
    {
        var calibrator = new DualDFindCalibrator
        {
            _antennaInfo = antennaInfo ?? throw new ArgumentNullException(nameof(antennaInfo))
        };
        var filename =
            $"{(antennaInfo.Polarization == Polarization.Horizontal ? "H" : "V")}{antennaInfo.StartFrequency}~{antennaInfo.StopFrequency}.vcy";
        filename = $"{(string.IsNullOrEmpty(foldername) ? "." : foldername)}\\{filename}";
        var length = (int)((antennaInfo.StopFrequency - antennaInfo.StartFrequency) * 1.0d / antennaInfo.StepFrequency +
                           0.0000005d) + 1;
        length = length * antennaInfo.AngleCount * antennaInfo.GroupCount * (antennaInfo.GroupCount - 1) / 2;
        calibrator._buffer = new float[length];
        try
        {
            var fileMode = createNewFile ? FileMode.OpenOrCreate : FileMode.Open;
            using var stream = new FileStream(filename, fileMode, FileAccess.Read);
            if (length * 4 <= stream.Length)
            {
                var buffer = new byte[length * 4];
                var total = 0;
                var offset = 0;
                while (total < buffer.Length)
                {
                    var count = stream.Read(buffer, offset, buffer.Length - total);
                    offset += count;
                    total += count;
                }

                Buffer.BlockCopy(buffer, 0, calibrator._buffer, 0, buffer.Length);
            }
        }
        catch
        {
        }

        return calibrator;
    }

    #endregion

    #region 成员变量

    private DfAntennaInfo _antennaInfo;
    private float[] _buffer;

    #endregion

    #region IDFindCalibrator

    public float[] ReadData(double frequency)
    {
        var tmp = (int)Math.Round(frequency);
        var previous = tmp;
        var next = tmp;
        for (var value = _antennaInfo.StartFrequency;
             value <= _antennaInfo.StopFrequency - _antennaInfo.StepFrequency;
             value += _antennaInfo.StepFrequency)
            if (tmp > value && tmp < value + _antennaInfo.StepFrequency)
            {
                previous = value;
                next = value + _antennaInfo.StepFrequency;
                break;
            }

        var length = _antennaInfo.AngleCount * _antennaInfo.GroupCount * (_antennaInfo.GroupCount - 1) / 2;
        var data = new float[length];
        if (tmp > previous && tmp < next)
        {
            var lowData = new float[length];
            var highData = new float[length];
            var lowIndex = (int)((previous - _antennaInfo.StartFrequency) * 1.0d / _antennaInfo.StepFrequency +
                                 0.0000005d);
            var highIndex =
                (int)((next - _antennaInfo.StartFrequency) * 1.0d / _antennaInfo.StepFrequency + 0.0000005d);
            Array.Copy(_buffer, lowIndex * length, lowData, 0, length);
            Array.Copy(_buffer, highIndex * length, highData, 0, length);
            var coe = (float)((tmp - previous) * 1.0d / (next - previous));
            for (var index = 0; index < length; ++index)
                // data[index] = coe * (highData[index] - lowData[index]) + lowData[index];
                data[index] = GetAngleByRatio(lowData[index], highData[index], coe);
        }
        else
        {
            if (tmp < _antennaInfo.StartFrequency)
                tmp = _antennaInfo.StartFrequency;
            else if (tmp > _antennaInfo.StopFrequency) tmp = _antennaInfo.StopFrequency;
            var index = (int)((tmp - _antennaInfo.StartFrequency) * 1.0d / _antennaInfo.StepFrequency + 0.0000005d);
            Array.Copy(_buffer, index * length, data, 0, length);
        }

        return data;
    }

    private static float GetAngleByRatio(float beginAngle, float endAngle, float ratio)
    {
        if (ratio is <= 0 or > 1) throw new InvalidDataException("ratio should be larger than 0 and less than 1");
        beginAngle = (float)(beginAngle / 180.0d * Math.PI);
        endAngle = (float)(endAngle / 180.0d * Math.PI);
        var begin = new Complex(Math.Cos(beginAngle), Math.Sin(beginAngle));
        var end = new Complex(Math.Cos(endAngle), Math.Sin(endAngle));
        var value = begin + end;
        value *= ratio;
        var result = (float)((value.Phase / Math.PI * 180.0d % 360 + 360) % 360);
        return result;
    }

    #endregion
}

internal class NineDFindCalibrator : IDFindCalibration
{
    #region 构造函数

    private NineDFindCalibrator()
    {
    }

    #endregion

    #region 类方法

    /// <summary>
    ///     创建双通道测向校准器
    /// </summary>
    /// <param name="antennaInfo">测向天线信息</param>
    /// <param name="foldername">校准文件所在目录</param>
    /// <param name="createNewFile">是否创建新文件，确定如果不存在校准文件的情况下，是否主动创建</param>
    /// <returns>返回双通道测向数据校准器</returns>
    public static NineDFindCalibrator CreateInstance(DfAntennaInfo antennaInfo, string foldername = "",
        bool createNewFile = false)
    {
        var calibrator = new NineDFindCalibrator
        {
            _antennaInfo = antennaInfo ?? throw new ArgumentNullException(nameof(antennaInfo))
        };
        // 构造文件名
        var filename =
            $"{(antennaInfo.Polarization == Polarization.Horizontal ? "H" : "V")}{antennaInfo.StartFrequency}~{antennaInfo.StopFrequency}.vcy";
        filename = $"{(string.IsNullOrEmpty(foldername) ? "." : foldername)}\\{filename}";
        // 构造文件缓存
        var length = (int)((antennaInfo.StopFrequency - antennaInfo.StartFrequency) * 1.0d / antennaInfo.StepFrequency +
                           0.0000005d) + 1;
        length = length * antennaInfo.AngleCount * (antennaInfo.GroupCount == 1
            ? 36
            : antennaInfo.GroupCount * (antennaInfo.GroupCount - 1) / 2);
        calibrator._buffer = new float[length];
        try
        {
            // 构建文件操作在确认文件不小于缓存要求的情况下，认为来自文件的数据是合法的（还是不完全准确，理论上就该一样大才对），并将数据全部导入到缓存
            var fileMode = createNewFile ? FileMode.OpenOrCreate : FileMode.Open;
            using var stream = new FileStream(filename, fileMode, FileAccess.Read);
            if (length * 4 <= stream.Length)
            {
                var buffer = new byte[length * 4];
                var total = 0;
                var offset = 0;
                while (total < buffer.Length)
                {
                    var count = stream.Read(buffer, offset, buffer.Length - total);
                    offset += count;
                    total += count;
                }

                Buffer.BlockCopy(buffer, 0, calibrator._buffer, 0, buffer.Length);
            }
        }
        catch
        {
        }

        return calibrator;
    }

    #endregion

    #region 成员变量

    private DfAntennaInfo _antennaInfo; // 测向天线信息
    private float[] _buffer; // 校准数据缓存

    #endregion

    #region IDFindCalibrator

    public float[] ReadData(double frequency)
    {
        var tmp = (int)Math.Round(frequency); // 四舍五入取整
        var previous = tmp;
        var next = tmp;
        for (var value = _antennaInfo.StartFrequency;
             value <= _antennaInfo.StopFrequency - _antennaInfo.StepFrequency;
             value += _antennaInfo.StepFrequency)
            if (tmp > value && tmp < value + _antennaInfo.StepFrequency)
            {
                previous = value;
                next = value + _antennaInfo.StepFrequency;
                break;
            }

        var length = _antennaInfo.AngleCount * (_antennaInfo.GroupCount == 1
            ? 36
            : _antennaInfo.GroupCount * (_antennaInfo.GroupCount - 1) / 2);
        var data = new float[length];
        if (tmp > previous && tmp < next)
        {
            var lowData = new float[length];
            var highData = new float[length];
            var lowIndex = (int)((previous - _antennaInfo.StartFrequency) * 1.0d / _antennaInfo.StepFrequency +
                                 0.0000005d);
            var highIndex =
                (int)((next - _antennaInfo.StartFrequency) * 1.0d / _antennaInfo.StepFrequency + 0.0000005d);
            Array.Copy(_buffer, lowIndex * length, lowData, 0, length);
            Array.Copy(_buffer, highIndex * length, highData, 0, length);
            var coe = (float)((tmp - previous) * 1.0d / (next - previous));
            for (var index = 0; index < length; ++index)
                // data[index] = coe * (highData[index] - lowData[index]) + lowData[index];	// 因为样本是带周期的，不能单纯用算数运算，需要考虑周期
                data[index] = GetAngleByRatio(lowData[index], highData[index], coe);
        }
        else
        {
            if (tmp < _antennaInfo.StartFrequency)
                tmp = _antennaInfo.StartFrequency;
            else if (tmp > _antennaInfo.StopFrequency) tmp = _antennaInfo.StopFrequency;
            var index = (int)((tmp - _antennaInfo.StartFrequency) * 1.0d / _antennaInfo.StepFrequency + 0.0000005d);
            Array.Copy(_buffer, index * length, data, 0, length);
        }

        return data;
    }

    // 取两个角度夹角比例所对应的角度值
    private static float GetAngleByRatio(float beginAngle, float endAngle, float ratio)
    {
        if (ratio is <= 0 or > 1) throw new InvalidDataException("ratio should be larger than 0 and less than 1");
        beginAngle = (float)(beginAngle / 180.0d * Math.PI);
        endAngle = (float)(endAngle / 180.0d * Math.PI);
        var begin = new Complex(Math.Cos(beginAngle), Math.Sin(beginAngle));
        var end = new Complex(Math.Cos(endAngle), Math.Sin(endAngle));
        var value = begin + end;
        value *= ratio;
        var result = (float)((value.Phase / Math.PI * 180.0d % 360 + 360) % 360);
        return result;
    }

    #endregion
}

internal class NineChannelCalibrator : IChannelCalibration
{
    #region 构造函数

    private NineChannelCalibrator(int startFrequency, int stopFrequency)
    {
        StartFrequency = startFrequency;
        StopFrequency = stopFrequency;
    }

    #endregion

    #region IChannelCalibrate

    public float[] ReadData(double frequency, double bandwidth)
    {
        var buffer = new float[_perDataLength];
        var tmp = (int)Math.Round(frequency);
        tmp = tmp > 35 ? tmp : 35;
        if (tmp < StartFrequency)
            tmp = StartFrequency;
        else if (tmp > StopFrequency) tmp = StopFrequency;
        var gears = _sortedValidIfBandwidths == null ? 1 : _sortedValidIfBandwidths.Length;
        var gearIndex = GetGearIndex(bandwidth);
        var index = ((tmp - StartFrequency) * gears + gearIndex) * _perDataLength;
        lock (_bufferLock)
        {
            Array.Copy(_buffer, index, buffer, 0, buffer.Length);
        }

        return buffer;
    }

    #endregion

    #region 类方法

    public static NineChannelCalibrator CreateInstance(string foldername = "", int[] validIfBandwidths = null,
        bool createNew = false, int startFreqeuncy = 20, int stopFrequency = 3600)
    {
        var calibrator = new NineChannelCalibrator(startFreqeuncy, stopFrequency);
        if (validIfBandwidths != null) Array.Sort(validIfBandwidths, (x, y) => x.CompareTo(y));
        calibrator._sortedValidIfBandwidths = validIfBandwidths;
        var gears = validIfBandwidths == null ? 1 : validIfBandwidths.Length;
        calibrator._buffer =
            new float[(stopFrequency - calibrator.StartFrequency + 1) * calibrator._perDataLength * gears];
        var filename = $"{foldername}\\channel.cal";
        try
        {
            var fileMode = createNew ? FileMode.OpenOrCreate : FileMode.Open;
            using var stream = new FileStream(filename, fileMode, FileAccess.Read);
            if (stream.Length > 8)
            {
                var buffer = new byte[8];
                stream.Read(buffer, 0, buffer.Length);
                calibrator.StartFrequency = BitConverter.ToInt32(buffer, 0);
                calibrator.StopFrequency = BitConverter.ToInt32(buffer, 4);
                var remainingLength = (int)(stream.Length - 8);
                buffer = new byte[remainingLength];
                var total = 0;
                var offset = 0;
                while (total < buffer.Length)
                {
                    var count = stream.Read(buffer, offset, buffer.Length - total);
                    offset += count;
                    total += count;
                }

                var length = (calibrator.StopFrequency - calibrator.StartFrequency + 1) * calibrator._perDataLength *
                             4 * gears;
                var delta = length - buffer.Length;
                if (delta != 0) Array.Resize(ref buffer, length);
                delta = length - calibrator._buffer.Length * 4;
                if (delta != 0) Array.Resize(ref calibrator._buffer, length / 4);
                Buffer.BlockCopy(buffer, 0, calibrator._buffer, 0, length);
            }
        }
        catch
        {
        }

        return calibrator;
    }

    #endregion

    #region Helper

    private int GetGearIndex(double bandwidth)
    {
        if (_sortedValidIfBandwidths != null)
            for (var index = 0; index < _sortedValidIfBandwidths.Length; ++index)
                if ((int)Math.Round(bandwidth) <= _sortedValidIfBandwidths[index])
                    return index;
        return 0;
    }

    #endregion

    #region 成员变量

    private readonly int _perDataLength = 9;
    private readonly object _bufferLock = new();
    private int[] _sortedValidIfBandwidths;
    private float[] _buffer;

    #endregion

    #region 属性

    public int StartFrequency { get; private set; }
    public int StopFrequency { get; private set; }

    #endregion
}

internal class NineBandCalibrator : IBandCalibration
{
    private readonly object _bufferLock = new();
    private readonly int _perDataLength = 9;
    private float[] _buffer;

    private NineBandCalibrator(int total)
    {
        Total = total;
    }

    public int Total { get; private set; }

    public float[] ReadData(int index)
    {
        var buffer = new float[_perDataLength];
        if (index < 0)
            index = 0;
        else if (index >= Total) index = Total - 1;
        index *= _perDataLength;
        lock (_bufferLock)
        {
            Array.Copy(_buffer, index, buffer, 0, buffer.Length);
        }

        return buffer;
    }

    public static NineBandCalibrator CreateInstance(string foldername = "", bool createNew = false, int total = 1601)
    {
        var calibrator = new NineBandCalibrator(total);
        calibrator._buffer = new float[total * calibrator._perDataLength];
        var filename = $"{foldername}\\band.cal";
        try
        {
            var fileMode = createNew ? FileMode.OpenOrCreate : FileMode.Open;
            using var stream = new FileStream(filename, fileMode, FileAccess.Read);
            if (stream.Length > 4)
            {
                var buffer = new byte[4];
                stream.Read(buffer, 0, buffer.Length);
                calibrator.Total = BitConverter.ToInt32(buffer, 0);
                var remainingLength = (int)(stream.Length - 4);
                buffer = new byte[remainingLength];
                var totalTemp = 0;
                var offset = 0;
                while (totalTemp < buffer.Length)
                {
                    var count = stream.Read(buffer, offset, buffer.Length - totalTemp);
                    offset += count;
                    totalTemp += count;
                }

                var length = calibrator.Total * calibrator._perDataLength * 4;
                var delta = length - buffer.Length;
                if (delta != 0) Array.Resize(ref buffer, length);
                delta = length - calibrator._buffer.Length * 4;
                if (delta != 0) Array.Resize(ref calibrator._buffer, length / 4);
                Buffer.BlockCopy(buffer, 0, calibrator._buffer, 0, length);
            }
        }
        catch
        {
        }

        return calibrator;
    }
}