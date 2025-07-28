using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading;
using Magneto.Contract.BaseClass;
using Magneto.Protocol.Define;

namespace Magneto.Device.SerialSPDTSwitchArray;

public partial class SerialSpdtSwitchArray : SwitchArrayBase
{
    #region 构造函数

    public SerialSpdtSwitchArray(Guid deviceId) : base(deviceId)
    {
    }

    #endregion

    #region 辅助方法

    // 初始化网络
    private void InitNetworks()
    {
        _serialPort = new SerialPort(Com, BaudRate);
        _serialPort.Open();
    }

    #endregion

    #region 成员变量

    private SerialPort _serialPort;
    private readonly ConcurrentDictionary<SwitchUsage, int> _switchUsageTable = new();

    #endregion

    #region 重写基类方法

    public override bool Initialized(ModuleInfo device)
    {
        try
        {
            if (base.Initialized(device))
            {
                InitNetworks();
                Reset(); // 初始化将开关状态复位，如默认打成监测开，管制关
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    protected override Stream GetLazyStream()
    {
        return _serialPort?.BaseStream;
    }

    protected override bool SendCode(string code)
    {
        Debug.WriteLine($"控制码：{code}");
        var pattern = @"^((0[xX])?[0-9A-Fa-f]{1,2}\|)*(0[xX])?[0-9A-Fa-f]{1,2}$"; // 只匹配以“|”分隔的16进制数据，已考虑大小写情况
        var reg = new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace);
        if (!reg.IsMatch(code)) throw new FormatException("开关控制码格式有误");
        // 按需构造流对象，但要保证只调用一次
        var codeArray = code.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        var commands = new byte[codeArray.Length];
        for (var index = 0; index < codeArray.Length; ++index)
            commands[index] = Convert.ToByte(codeArray[index], 16); // 无论字符串是否带有“0x”前缀，都可以转换为对应的16进制数据
        if (_serialPort?.IsOpen == true) _serialPort.Write(commands, 0, commands.Length);
        Thread.Sleep(100);
        var str = _serialPort?.ReadExisting();
        Trace.WriteLine($"SerialSPDTSwitch=>{str}");
        return !string.IsNullOrWhiteSpace(str) && !Regex.IsMatch(str, @"Code\s*error", RegexOptions.IgnoreCase);
    }

    // 复切换开关到监测状态
    public override void Reset()
    {
        RaiseSwitchChangeNotification(_switchUsageTable.ContainsKey(SwitchUsage.RadioMonitoring)
            ? -1
            : _switchUsageTable[SwitchUsage.RadioMonitoring]);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _serialPort?.Close();
    }

    #endregion
}