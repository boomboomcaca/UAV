using System.Collections.Generic;
using System.Xml.Serialization;

namespace Magneto.Device.DDF550;

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
    private static readonly object _lockSign = new();

    [XmlElement("Command")] public Command Command;

    [XmlAttribute("id")] public string Id;

    [XmlAttribute("type")] public string Type;

    public Request() : this(OpreationMode.Get)
    {
    }

    /// <summary>
    ///     构造函数
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
        if (paramName != null)
        {
            Param param = new()
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