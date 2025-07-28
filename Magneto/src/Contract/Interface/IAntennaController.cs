using System;
using System.Collections.Generic;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Contract.Interface;

public interface IAntennaController
{
    /// <summary>
    ///     获取/设置当前天线控制行为是“频率自动”、“极化手动”、“天线手动”
    ///     注：此参数在实际配置某个具体功能时，应该序列化为当前功能的“安装参数”	（“安装参数”先于“用户参数”设置，“用户参数”可能以“安装参数”不同的取值作为不同的分支条件）
    /// </summary>
    AntennaSelectionMode AntennaSelectedType { get; set; }

    /// <summary>
    ///     获取/设置天线频率，适用于“频率自动”和“极化手动”条件下选择天线，以及获取特定天线特定频率下的天线因子
    ///     注：此为用户交互参数，需要用户在选天线时显示设置（通过UI由用户操作设置或通过接口直接设置）
    /// </summary>
    double Frequency { get; set; }

    /// <summary>
    ///     获取/设置天线极化方式，适用于“极化手动”条件下选择符合特定频率的天线
    ///     注：此为用户交互参数，当前功能如果将AntennaSelectedType配置为“极化手动”时，需要将此参数暴露到客户端；否则需要对客户端隐藏此参数
    /// </summary>
    Polarization PolarityType { get; set; }

    /// <summary>
    ///     获取/设置选中的天线，适用于“天线手动”条件
    ///     注：此为用户交互参数，当前功能如果将AntennaSelectedType配置为“天线手动”时，需要将此参数暴露到客户端；否则需要对客户端隐藏此参数
    /// </summary>
    Guid AntennaId { get; set; }

    /// <summary>
    ///     获取/设置当前可用的天线集合
    ///     注：此天线集合不一定为天线控制器配置的所有天线，特定的功能可能只使用了所有天线集合的一个子集
    ///     此参数在实际配置某个具体功能时，应该序列化为当前功能的“安装参数”	（“安装参数”先于“用户参数”设置，“用户参数”可能以“安装参数”不同的取值作为不同的分支条件）
    /// </summary>
    List<AntennaInfo> Antennas { get; set; }

    /// <summary>
    ///     设置当前选择的天线是有源还是无源
    /// </summary>
    bool IsActive { get; set; }

    /// <summary>
    ///     发送天线控制码，打通/关闭天线
    /// </summary>
    /// <param name="code">天线码，以十六进制字节码的字符串形式给出</param>
    /// <returns>成功返回True，否则返回False</returns>
    bool SendControlCode(string code);
}

/// <summary>
///     IAntennaController接口扩展类（方法）
///     封装所有继承自该接口的公共处理逻辑（重复代码），模拟实现类的多重继承
/// </summary>
public static class AntennaControllerExtension
{
    /// <summary>
    ///     获取指定频率的天线因子
    /// </summary>
    /// <param name="antennaController">天线控制器接口</param>
    /// <param name="frequency">频率</param>
    /// <returns>因子</returns>
    /// <remarks>返回结果，如果是因子无效，则返回零</remarks>
    public static short GetFactor(this IAntennaController antennaController, double frequency)
    {
        var freqs = GetFactor(antennaController, [frequency]);
        return freqs[0];
    }

    /// <summary>
    ///     获取指定频率集合的天线因子
    /// </summary>
    /// <param name="antennaController">天线控制器接口</param>
    /// <param name="frequencies">频率集合</param>
    /// <returns>因子集合，长度与频率集合相同</returns>
    /// <remarks>返回数组不为空，如果因子无效，则数组元素全为零</remarks>
    public static short[] GetFactor(this IAntennaController antennaController, double[] frequencies)
    {
        if (frequencies == null) throw new ArgumentNullException(nameof(frequencies));
        var values = new short[frequencies.Length];
        // 可供选择的天线集合为空，则因子为空
        if (antennaController.Antennas == null || antennaController.Antennas.Count == 0 ||
            antennaController.AntennaId.Equals(Guid.Empty)) return values;
        // 当前选中天线的编号不在可选天线的范围之内，则天线因子为空
        var antenna = antennaController.Antennas.Find(item => item.Id.Equals(antennaController.AntennaId));
        if (antenna == null) return values;
        ///////////////////////////////////////////////////////////////////////////////////////////
        //以上逻辑皆为容错判断，实际上，因子一定也应该关联到与当前已经选中的天线，即以下程序逻辑
        //////////////////////////////////////////////////////////////////////////////////////////
        var antennaFactor = antenna.GetFactor(antennaController.IsActive);
        if (antennaFactor == null) // 若发生异常，则天线因子对象为空，返回预设的因子数据（全为零）
            return values;
        // 处理从当前选中天线的因子文件中获取的数据
        var factors = new SDataFactor { Data = new short[frequencies.Length], Total = frequencies.Length };
        for (var i = 0; i < frequencies.Length; ++i)
        {
            var index = (int)((frequencies[i] - antennaFactor.StartFrequency) * 1000000 /
                              (antennaFactor.StepFrequency * 1000));
            if (index < 0)
                factors.Data[i] = antennaFactor.Data[0];
            else if (index >= antennaFactor.Total)
                factors.Data[i] = antennaFactor.Data[antennaFactor.Total - 1];
            else
                factors.Data[i] = antennaFactor.Data[index];
        }

        return factors.Data;
    }

    /// <summary>
    ///     获取指定频段范围内的天线因子
    /// </summary>
    /// <param name="antennaController">天线控制器接口</param>
    /// <param name="startFrequency">起始频率，单位：MHz</param>
    /// <param name="stopFrequency">结束频率，单位：MHz</param>
    /// <param name="stepFrequency">步进频率，单位：kHz</param>
    /// <param name="count">需要的因子集合长度</param>
    /// <param name="exception">警告信息，获取因子时产生的异常</param>
    /// <returns>因子集合，长度与要求的因子集合长度一致</returns>
    /// <remarks>返回数组不为空，如果因子无效，则数组元素全为零</remarks>
    public static short[] GetFactor(this IAntennaController antennaController, double startFrequency,
        double stopFrequency, double stepFrequency, int count, ref Exception exception)
    {
        const double epsilon = 1.0E-6d;
        // 频段范围不符合规范，则返回因子为空
        // 频率单位先换成Hz，再判断
        if (Math.Abs(stepFrequency) <= epsilon ||
            Math.Abs((int)((stopFrequency - startFrequency) * 1000000) / (int)(stepFrequency * 1000)) <= 0 ||
            count <= 0) exception = new ArgumentException("频段信息或频点数量设置不符合要求");
        var values = new short[count];
        // 可供选择的天线集合为空，则返回因子为空
        if (antennaController.Antennas == null || antennaController.Antennas.Count == 0 ||
            antennaController.AntennaId.Equals(Guid.Empty)) return values;
        // 当前选中的天线不在可供选择的天线范围之内，则因子为空
        var antenna = antennaController.Antennas.Find(item => item.Id.Equals(antennaController.AntennaId));
        if (antenna == null) return values;
        ///////////////////////////////////////////////////////////////////////////////////////////
        //	以上逻辑皆为容错判断，实际上，因子一定也应该关联到与当前已经选中的天线，即以下程序逻辑
        //////////////////////////////////////////////////////////////////////////////////////////
        SDataFactor antennaFactor = null;
        try
        {
            antennaFactor = antenna.GetFactor(antennaController.IsActive);
        }
        catch (Exception ex)
        {
            // TODO: 异常应该往上通知或记录到服务端日志
            // 异常来源：天线因子文件不存在，或因子文件格式有误
            exception = ex;
        }

        if (antennaFactor == null) return values;
        // 处理从当前选中天线的因子文件中获取的数据
        var factors = new SDataFactor
        {
            StartFrequency = startFrequency,
            StopFrequency = stopFrequency,
            StepFrequency = stepFrequency,
            Data = new short[count],
            Total = count
        };
        for (var i = 0; i < count; ++i)
        {
            var index = (int)((startFrequency * 1000000 + i * stepFrequency * 1000 -
                               antennaFactor.StartFrequency * 1000000) / (antennaFactor.StepFrequency * 1000));
            if (index < 0)
                factors.Data[i] = antennaFactor.Data[0];
            else if (index >= antennaFactor.Total)
                factors.Data[i] = antennaFactor.Data[antennaFactor.Total - 1];
            else
                factors.Data[i] = antennaFactor.Data[index];
        }

        return factors.Data;
    }

    /// <summary>
    ///     根据频率和极化方式以及有源无源打通天线
    /// </summary>
    /// <param name="antennaController">天线控制器</param>
    /// <param name="frequency">频率</param>
    /// <param name="polarityType">极化方式</param>
    /// <param name="isActive">是否有源</param>
    /// <param name="antennaId">天线编号</param>
    /// <remarks>函数返回后，通过antennaID返回打通的天线，如果天线打通失败，则保持之前已经选通的天线</remarks>
    public static void OpenAntenna(this IAntennaController antennaController, double frequency,
        Polarization polarityType, ref bool isActive, ref Guid antennaId)
    {
        var antenna = GetControlCode(antennaController, frequency, polarityType, antennaId);
        if (antenna == null) return;
        if (antennaId == antenna.Id && antennaController.IsActive == isActive) return;
        //保证通过天线手动和极化手动AntennaID一致即不管SendControlCode成不成功AntennaID都为新的天线ID,以保证子类在重载的SendControlCode函数中能通过AntennaID准确的提示出出错信息
        //此处必须明确设置当前选中的天线编号，以便通过IAntennaController.GetFactor时，能准确获取已打通天线的因子数据
        antennaId = antenna.Id;
        var active = isActive ? 2 : 1;
        if ((antenna.IsActive & active) == 0)
            // 如果天线不支持当前的有源模式，则将当前的有源无源模式修改为天线支持的那个模式
            isActive = antenna.IsActive == 2;
        var code = isActive ? antenna.ActiveCode : antenna.PassiveCode;
        antennaController.SendControlCode(code);
    }

    /// <summary>
    ///     根据天线编号打通天线
    /// </summary>
    /// <param name="antennaController">天线控制器</param>
    /// <param name="isActive"></param>
    /// <param name="antennaId">当前天线编号</param>
    /// <param name="dstAntennaId">目标天线编号</param>
    public static void OpenAntenna(this IAntennaController antennaController, ref bool isActive, ref Guid antennaId,
        Guid dstAntennaId)
    {
        var antenna = GetControlCode(antennaController, dstAntennaId);
        if (antenna == null) return;
        if (antennaId == dstAntennaId && antennaController.IsActive == isActive) return;
        antennaId = dstAntennaId;
        var active = isActive ? 2 : 1;
        if ((antenna.IsActive & active) == 0)
            // 如果天线不支持当前的有源模式，则将当前的有源无源模式修改为天线支持的那个模式
            isActive = antenna.IsActive == 2;
        var code = isActive ? antenna.ActiveCode : antenna.PassiveCode;
        antennaController.SendControlCode(code);
    }

    /// <summary>
    ///     频率自动或极化手动获取天线控制码
    /// </summary>
    /// <param name="antennaController">天线控制器接口</param>
    /// <param name="frequency">天线频率</param>
    /// <param name="polarityType">极化方式</param>
    /// <param name="antennaId">原来打通的天线ID</param>
    /// <returns>成功返回具体的天线信息，否则返回空字</returns>
    public static AntennaInfo GetControlCode(this IAntennaController antennaController, double frequency,
        Polarization polarityType, Guid antennaId)
    {
        // 可供选择的天线集合为空，则天线控制码信息为空
        if (antennaController.Antennas == null || antennaController.Antennas.Count == 0) return null;
        AntennaInfo antenna = null;
        if (antennaController.AntennaSelectedType == AntennaSelectionMode.Auto) // 频率自动，和极化方式无关，在当前天线列表里面找到第一个适用于当前频率的天线
        {
            // 自动模式下，新打通的天线需要极化方式与上一次打通的极化方式一样（如果不存在，则只以频率为条件进行查询）
            var oldAnt = antennaController.Antennas.Find(item => item.Id == antennaId && item.Id != Guid.Empty);
            if (oldAnt != null)
                // 如果没有找到同时符合极化方式与频率范围内的天线，则只使用第一根满足频率要求的天线
                antenna = antennaController.Antennas.Find(item => item.Polarization == oldAnt.Polarization
                                                                  && item.Id != Guid.Empty
                                                                  && frequency >= item.StartFrequency
                                                                  && frequency <= item.StopFrequency)
                          ?? antennaController.Antennas.Find(item => frequency >= item.StartFrequency
                                                                     && frequency <= item.StopFrequency);
            else
                antenna = antennaController.Antennas.Find(item =>
                    item.Id != Guid.Empty && frequency >= item.StartFrequency && frequency <= item.StopFrequency);
        }
        else if (antennaController.AntennaSelectedType ==
                 AntennaSelectionMode.Polarization) // 极化方式手动，需要参考当前设置的频率，并在天线列表里面找到第一个同时满足频率和极化方式的天线
        {
            // 如果没有找到同时符合极化方式与频率范围内的天线，则只使用第一根满足极化方式要求的天线
            antenna = antennaController.Antennas.Find(item => item.Polarization.Equals(polarityType)
                                                              && item.Id != Guid.Empty
                                                              && frequency >= item.StartFrequency
                                                              && frequency <= item.StopFrequency)
                      ?? antennaController.Antennas.Find(item => item.Polarization.Equals(polarityType));
        }

        return antenna;
    }

    /// <summary>
    ///     根据天线编号获取天线控制码
    /// </summary>
    /// <param name="antennaController">天线控制器接口</param>
    /// <param name="antennaId">天线编号</param>
    /// <returns>成功返回具体的天线编号与码值对，否则返回空</returns>
    public static AntennaInfo GetControlCode(this IAntennaController antennaController, Guid antennaId)
    {
        if (antennaController.Antennas == null || antennaController.Antennas.Count == 0 // 可供选择的天线集合为空，则天线控制码信息为空
                                               || antennaId.Equals(Guid.Empty)) // 天线编码为空，则天线控制码信息为空
            return null;
        return antennaController.Antennas.Find(item => item.Id.Equals(antennaId));
    }
}