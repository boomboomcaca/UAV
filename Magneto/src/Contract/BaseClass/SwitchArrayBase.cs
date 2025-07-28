using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Contract.BaseClass;

/// <summary>
///     开关阵列控制器基类
/// </summary>
public abstract class SwitchArrayBase : DeviceBase, ISwitchCallback
{
    #region 构造函数

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="deviceId">设备编号</param>
    public SwitchArrayBase(Guid deviceId) : base(deviceId)
    {
    }

    #endregion

    #region Disposable

    public override void Dispose()
    {
        _writer?.Close();
        _writer = null;
        _stream?.Close();
        _stream = null;
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion

    #region 成员变量

    // 已注册开关序号对应的回调接口
    private readonly ConcurrentDictionary<int, Action<SwitchInfo>> _switchActionTable = new();

    // 开关列表
    private readonly ConcurrentBag<SwitchInfo> _switchCollection = new();

    // 按用途索引的开关逻辑注册数量，理论上不同的分类与开关之间应该是一一对应的，但实际情况下可能存在不同开关具备相同的分类，为了在分配开关时做到“均衡负载”，人为产生了这样的逻辑处理
    private readonly ConcurrentDictionary<SwitchUsage, int> _switchUsageCounter = new();

    // 开关阵列的控制方式应该是基于流的，如文件读写，TCP套接字，串口通信等
    private Stream _stream;

    // 基于流，使用二进制模式写对流进行写操作
    private BinaryWriter _writer;

    #endregion

    #region 成员函数

    // 添加开关信息，包含用途，名称，打通码
    protected int AddSwitch(SwitchUsage switchUsage, string switchName, string code)
    {
        var switchInfo = new SwitchInfo(_switchCollection.Count, switchUsage, switchName, code);
        _switchCollection.Add(switchInfo);
        return switchInfo.Index;
    }

    // 或取子类的流对象
    protected abstract Stream GetLazyStream();

    // 打通控制码
    protected virtual bool SendCode(string code)
    {
        Debug.WriteLine($"控制码：{code}");
        var pattern = @"^((0[xX])?[0-9A-Fa-f]{1,2}\|)*(0[xX])?[0-9A-Fa-f]{1,2}$"; // 只匹配以“|”分隔的16进制数据，已考虑大小写情况
        var reg = new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace);
        if (!reg.IsMatch(code)) throw new FormatException("开关控制码格式有误");
        // 按需构造流对象，但要保证只调用一次
        _stream ??= GetLazyStream();
        if (_stream != null && _writer == null) // 重大bug 原代码：else if (_writer == null) 祝兴志 2018-05-11
            _writer = new BinaryWriter(_stream);
        if (_writer == null || string.IsNullOrEmpty(code)) return false;
        var codeArray = code.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        var commands = new byte[codeArray.Length];
        for (var index = 0; index < codeArray.Length; ++index)
            // commands[index] = byte.Parse(codeArray[index], NumberStyles.HexNumber);
            commands[index] = Convert.ToByte(codeArray[index], 16); // 无论字符串是否带有“0x”前缀，都可以转换为对应的16进制数据
        _writer.Write(commands, 0, commands.Length);
        _writer.Flush();
        return true;
    }

    // 触发指定序号的开关打通，开关序号来自AddSwitch的返回值
    protected void RaiseSwitchChangeNotification(int index)
    {
        if (_switchCollection.FirstOrDefault(item => item.Index == index) is { } switchInfo)
        {
            var flag = SendCode(switchInfo.Code);
            var attempt = 0;
            while (!flag && attempt < 3)
            {
                Thread.Sleep(500);
                attempt++;
                flag = SendCode(switchInfo.Code);
            }

            var b = _switchActionTable.TryGetValue(index, out var action);
            if (b && action != null) action.Invoke(switchInfo);
        }
    }

    #endregion

    #region ISwitchCallback

    public int Register(SwitchUsage usage, Action<SwitchInfo> action)
    {
        var switchCollection = _switchCollection.Where(item => item.Usage == usage);
        var switchInfos = switchCollection as SwitchInfo[] ?? switchCollection.ToArray();
        if (!switchInfos.Any()) throw new ArgumentOutOfRangeException(nameof(usage), "未找到类型符合要求的开关");
        if (!_switchUsageCounter.ContainsKey(usage))
            _switchUsageCounter[usage] = 1;
        else
            _switchUsageCounter[usage] += 1;
        var index = _switchUsageCounter[usage] % switchInfos.Length;
        var switchInfo = switchInfos.ToImmutableSortedSet()[index];
        if (!_switchActionTable.ContainsKey(switchInfo.Index))
            _switchActionTable[switchInfo.Index] = action;
        else
            _switchActionTable[switchInfo.Index] += action;
        return switchInfo.Index;
    }

    public void UnRegister(int index, Action<SwitchInfo> action)
    {
        if (!_switchActionTable.ContainsKey(index)) throw new KeyNotFoundException($"未找到符合开关序号为 {index} 的注册接口");
        _switchActionTable[index] -= action;
        var switchInfo = _switchCollection.FirstOrDefault(item => item.Index == index);
        if (switchInfo != null && _switchUsageCounter.ContainsKey(switchInfo.Usage))
            _switchUsageCounter[switchInfo.Usage]--;
    }

    public virtual void Reset()
    {
    }

    #endregion
}