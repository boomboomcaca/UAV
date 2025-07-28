using System.Collections.Generic;
using System.Xml.Serialization;

namespace Magneto.Device.DDF5GTS;

public enum OpreationMode
{
    Get,
    Set
}

public class Param
{
    [XmlAttribute("name")] public string Name;

    [XmlText] public string Value;

    public Param()
    {
    }

    public Param(string name, string value)
    {
        Name = name;
        Value = value;
    }
}

public class Struct
{
    [XmlAttribute("name")] public string Name;

    [XmlElement("Param")] public List<Param> Params;

    [XmlElement("Struct")] public List<Struct> Structs;
}

public class Command
{
    [XmlElement("Param")] public List<Param> Params;
    [XmlElement("Array")] public ArrayParam Array;

    [XmlAttribute("name")] public string Name;


    [XmlAttribute("returnCode")] public string RtnCode;

    [XmlAttribute("returnMessage")] public string RtnMessage;

    [XmlText] public string Value;

    public Command()
    {
        Value = string.Empty;
        Params = new List<Param>();
        Array = new ArrayParam();
    }
}

public class ArrayParam
{
    [XmlAttribute("name")] public string Name;
    [XmlElement("Struct")] public List<Struct> Structs;
    [XmlElement("Param")] public List<Param> Params;
}

[XmlRoot("Reply")]
public class Reply
{
    [XmlElement("Command")] public Command Command;

    [XmlAttribute("id")] public string Id;

    [XmlAttribute("type")] public string Type;

    public Reply()
    {
        Command = new Command();
    }
}

[XmlRoot("Request")]
public class Request
{
    private static int _idSign = 123;
    private static object _lockSign = new();

    [XmlElement("Command")] public Command Command;

    [XmlAttribute("id")] public string Id;

    [XmlAttribute("type")] public string Type;

    public Request() : this(OpreationMode.Get)
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="opreationMode">操作方式（查询/设置）</param>
    public Request(OpreationMode opreationMode)
    {
        if (opreationMode == OpreationMode.Set)
            Type = "set";
        else
            Type = "get";
        lock (_lockSign)
        {
            _idSign++;
            if (_idSign >= 100000) _idSign = 123;
        }

        Id = _idSign.ToString();
        Command = new Command();
    }

    /// <summary>
    ///     查询指令
    /// </summary>
    /// <param name="commandName"></param>
    public void Query(string commandName)
    {
        Type = "get";
        Command.Name = commandName;
    }

    /// <summary>
    ///     设置或查询
    /// </summary>
    /// <param name="commandName">命令名称</param>
    /// <param name="paramName">参数名称</param>
    /// <param name="paramValue">参数值</param>
    public void Excute(string commandName, string paramName = null, string paramValue = null)
    {
        Command.Name = commandName;
        if (!string.IsNullOrEmpty(paramName))
        {
            var param = new Param
            {
                Name = paramName,
                Value = paramValue
            };
            Command.Params.Add(param);
        }
    }
}

public struct AntennaProperty
{
    public string Name;
    public int AntCode;
    public long FreqBegin;
    public long FreqEnd;
    public bool GpsAvailable;
    public bool TestElementAvailable;
    public List<SAntRangeProp> SAntRangeProp;
}

public struct AntennaInfo
{
    public string AntennaName;
    public long FreqBegin;
    public long FreqEnd;
    public string CompassName;
    public int NorthCorrection;
    public int RollCorrection;
    public int PitchCorrection;
    public ErfInput RfInput;
    public EhfInput HfInput;
    public ERxPath RfRxPath;
    public ERxPath HfRxPath;
    public int CtrlPort;
    public bool GpsRead;
}

public struct DfStatus
{
    /// <summary>
    ///     DF mode:
    ///     ● 2 (0010): FFM: Fixed Frequency Mode
    ///     ● 3 (0011): SCAN: Scan
    ///     ● 4 (0100): SEARCH: Search
    ///     ● 5 (0101): GSM: GSM
    ///     ● 15 (1111): CAL: calibration
    /// </summary>
    public byte DfMode { get; set; }

    /// <summary>
    ///     Antenna factor (k-factor) correction data:
    ///     ● 0: not applied
    ///     ● 1: applied
    /// </summary>
    public byte Factor { get; set; }

    /// <summary>
    ///     Azimuth correction data:
    ///     ● 0: not applied
    ///     ● 1: applied
    /// </summary>
    public byte AzimCor { get; set; }

    /// <summary>
    ///     Averaging mode:
    ///     ● 0 (0000): Continuous Mode
    ///     ● 1 (0001): Gated Mode
    ///     ● 2 (0010): Normal Mode
    /// </summary>
    public byte AvgMode { get; set; }

    /// <summary>
    ///     DF method:
    ///     ● 0 (0000): (reserved)
    ///     ● 1 (0001): WW: Watson-Watt
    ///     ● 2 (0010): CORR: correlation
    ///     ● 3 (0011): SR: super-resolution
    ///     ● 4 (0100): VM: vector matching
    /// </summary>
    public byte DfMethod { get; set; }

    /// <summary>
    ///     Antenna preamplifier:
    ///     ● 0: OFF: off
    ///     ● 1: ON: on
    /// </summary>
    public byte PreAmp { get; set; }

    /// <summary>
    ///     Polarization:
    ///     ● 0 (00): VERT: linear vertical
    ///     ● 1 (01): HOR: linear horizontal
    ///     ● 2 (10): LEFT: circular left-hand(counter-clockwise)
    ///     ● 3 (11): RIGHT: circular right-hand(clockwise)
    /// </summary>
    public byte AntPolarization { get; set; }

    /// <summary>
    ///     Simulated DF results (azimuth, elevation, omniphase, DF level,DF level[continuous])
    /// </summary>
    public byte SimulatedDfResult { get; set; }

    /// <summary>
    ///     Last hop in sweep (sent in each mode, therefore continuously set in FFM):
    ///     ● 0: another than last
    ///     ● 1: last
    ///     If a hop dwell time(cf Chapter 4.7.6) setting leads to more than
    ///     one DFPScan data trace, this flag is only set with the terminating trace.
    /// </summary>
    public byte SweepEnd { get; set; }

    /// <summary>
    ///     Calibration outdated:
    ///     ● 0: valid
    ///     ● 1: outdated
    ///     If active, last calibration longer ago than specified calibration interval
    ///     (only if no automatic calibration active)
    /// </summary>
    public byte CalibrationOld { get; set; }

    /// <summary>
    ///     Status of the Blanking source, i.e. user controllable validation of bearings(see Chapter 4.11.6.3, "Blanking", on
    ///     page 179):
    ///     ● 0: not active
    ///     ● 1: active
    ///     Mind Note "Significance of Settings" on page 179
    /// </summary>
    public byte BlankOut { get; set; }

    /// <summary>
    ///     Overflow of ADC input signal of at least one antenna base
    ///     (Chapter 4.11.6):
    ///     ● 0: no data overflow
    ///     ● 1: data overflow
    /// </summary>
    public byte OverFlow { get; set; }

    /// <summary>
    ///     Bearing result valid (Chapter 4.11.6):
    ///     ● 0: bearing not valid due to internal reasons, do not use result
    ///     ● 1: bearing valid
    /// </summary>
    public byte Valid { get; set; }

    public DfStatus(int status) : this()
    {
        Valid = (byte)(status & 0x00000001);
        OverFlow = (byte)((status & 0x00000002) >> 1);
        BlankOut = (byte)((status & 0x00000004) >> 2);
        CalibrationOld = (byte)((status & 0x00000008) >> 3);
        SweepEnd = (byte)((status & 0x00000010) >> 4);
        SimulatedDfResult = (byte)((status & 0x00000020) >> 5);
        AntPolarization = (byte)((status & 0x00000300) >> 8);
        PreAmp = (byte)((status & 0x00000400) >> 10);
        DfMethod = (byte)((status & 0x0000F000) >> 12);
        AvgMode = (byte)((status & 0x000F0000) >> 16);
        AzimCor = (byte)((status & 0x00100000) >> 20);
        Factor = (byte)((status & 0x00200000) >> 21);
        DfMode = (byte)((status & 0x0F000000) >> 24);
    }
}