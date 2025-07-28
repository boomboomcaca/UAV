using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract.BaseClass;
using Magneto.Protocol.Define;

namespace Magneto.Device.SerialAntController;

public partial class SerialAntController : AntennaControllerBase
{
    /// <summary>
    ///     心跳线程
    /// </summary>
    private Task _heartBeat;

    /// <summary>
    ///     串口
    /// </summary>
    private SerialPort _serialPort;

    private CancellationTokenSource _tokenSource;

    public SerialAntController(Guid deviceId) : base(deviceId)
    {
    }

    public override bool Initialized(ModuleInfo device)
    {
        var result = base.Initialized(device);
        if (!result) return false;
        try
        {
            InitSerialPort();
            InitHeartBeat();
        }
        catch (IOException)
        {
            return false;
        }

        return true;
    }

    public override void Dispose()
    {
        ReleaseHeartBeat();
        ReleaseSerialPort();
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    public override bool SendControlCode(string code)
    {
        try
        {
            SendCmd(code);
            return true;
        }
        catch (Exception ex)
        {
#if DEBUG
            Trace.WriteLine(ex.ToString());
#endif
            return false;
        }
    }

    private void SendCmd(string code)
    {
        const string pattern = @"^((0[xX])?[0-9A-Fa-f]{1,2}\|)*(0[xX])?[0-9A-Fa-f]{1,2}$"; // 只匹配以“|”分隔的16进制数据，已考虑大小写情况
        var reg = new Regex(pattern, RegexOptions.CultureInvariant |
                                     RegexOptions.IgnorePatternWhitespace);
        if (!reg.IsMatch(code)) throw new FormatException("天线控制码格式有误");
        var codeArray = code.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        var commands = new byte[codeArray.Length];
        for (var index = 0; index < codeArray.Length; ++index)
            commands[index] = Convert.ToByte(codeArray[index], 16); // 无论字符串是否带有“0x”前缀，都可以转换为对应的16进制数据
        if (_serialPort?.IsOpen != true) return;
        _serialPort.Write(commands, 0, commands.Length);
    }
}