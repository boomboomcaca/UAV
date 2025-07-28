using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Magneto.Contract;

namespace Magneto.Device.AP510;

/// <summary>
///     指令相关的常量字符串
/// </summary>
internal class Constants
{
    // 带宽与采样率关系
    public const string BwRate =
        "40000|51200|1600;" +
        "20000|25600|1600;" +
        "10000|12800|1600;" +
        "5000|6400|1600;" +
        "2500|3200|1600;" +
        "2000|2560|1600;" +
        "1250|1600|1600;" +
        "1000|1280|1600;" +
        "800|1024|1600;" +
        "600|800|1536;" +
        "500|640|1600;" +
        "400|512|1600;" +
        "300|400|1536;" +
        "250|320|1600;" +
        "200|256|1600;" +
        "150|200|1536;" +
        "125|160|1600;" +
        "100|128|1600;" +
        "80|100|1638;" +
        "60|80|1536;" +
        "50|64|1600;" +
        "40|50|1638;" +
        "30|40|1536;" +
        "25|32|1600;" +
        "20|25|1638;" +
        "15|20|1536;" +
        "12.5|16|1600;" +
        "10|12.5|1638;" +
        "8|10|1638;" +
        "6|8|1536;" +
        "5|6.25|1638;" +
        "4|5|1638;" +
        "3|4|1536;" +
        "2.5|3.125|1638;" +
        "2|2.5|1638;" +
        "1.5|2|1536;" +
        "1.2|1.5625|1572;" +
        "1|1.25|1638;" +
        "0.8|1|1638;" +
        "0.6|0.78125|1572";

    // 公共指令（一级指令）
    public const string IdnQuery = "*IDN?"; // 查询设备ID；返回结果：厂商，型号，固件版本，序列号（真信科技,AP510,3.0,2012-210-004）
    public const string OptQuery = "*OPT?"; // 功能查询；多任务，监测，测向，调制模式识别等
    public const string Abort = "ABORt"; // 终止任务。须带任务号：ABORt 1101(结束任务号为1101的任务)
    public const string Start = ":STARt"; // 连续IQ模式下启动

    public const string Stop = ":STOP"; // 连续IQ模式下停止

    // 功能指令（一级指令）
    public const string DfNarrow = "DF:NORRow"; // 单频测向指令前缀
    public const string DfWideband = "DF:WIDEband"; // 宽带测向指令前缀
    public const string DfList = "DF:LIST"; // 频率表测向指令前缀
    public const string RxSingle = "RX:SINGle"; // 单频测量指令前缀
    public const string RxCiq = "RX:CIQ"; // 连续IQ测量
    public const string RxPscan = "RX:PSCan"; // 中频全景扫描指令前缀
    public const string RxFscan = "RX:FScan"; // 频段扫描指令前缀
    public const string RxMscan = "RX:MSCan"; // 离散扫描指令前缀
    public const string Audio = "AUDio"; // 数字音频采集

    public const string Antenna = "ANTenna"; // 天线控制

    // 调用约定（二级指令）
    public const string Singlecall = ":SINGlecall"; // 单次调用，单次应答
    public const string Servicecall = ":SERVicecall"; // 单次调用，连续应答
    public const string ServicecallStar = ":SERVicecall*"; // 任务执行过程中修改参数（连续应答）
    public const string Plist = "PLISt"; // 指令参数选项，用于查询命令
    public const string Query = "?"; // 查询

    public const string Space = " "; // 空格符

    // 以下二级指令用于频率列表的指令（频率表测向，离散扫描）
    public const string Newtable = ":NEWTable"; // 新建频表
    public const string Addpoint = ":ADDPoint"; // 增加频点
    public const string Addrange = ":ADDRange"; // 增加频点范围

    public const string Clear = ":CLEar";

    // 以下二级指令主要用于音频指令（AUDio）
    public const string AudioOn = "ON"; // 音频开
    public const string AudioOff = "OFF"; // 音频关
    public const string AudioFormat = ":FORMat"; // 音频格式设置或查询
    public const string AudioFormatPcm = "PCM"; // PCM音频格式
    public const string AudioFormatMp3 = "MP3"; // MP3音频格工
    public const string AudioIp = ":IP"; // 音频返回的地址

    public const string AudioPort = ":PORT"; // 音频返回的端口号

    // 以下二三级指令主要用于天线控制（ANTenna）
    public const string AntConfig = ":CONFig"; // 天线配置
    public const string AntAdd = ":ADD"; // 增加天线配置

    public const string AntAuto = ":AUTO"; // 天线自动选择

    // 因子文件目录
    public const string FactorDir = "\\GeneFiles";

    public static readonly Dictionary<double, double> BwRateMap = new()
    {
        { 40000, 51200 },
        { 20000, 25600 },
        { 10000, 12800 },
        { 5000, 6400 },
        { 2500, 3200 },
        { 2000, 2560 },
        { 1250, 1600 },
        { 1000, 1280 },
        { 800, 1024 },
        { 600, 800 },
        { 500, 640 },
        { 400, 512 },
        { 300, 400 },
        { 250, 320 },
        { 200, 256 },
        { 150, 200 },
        { 125, 160 },
        { 100, 128 },
        { 80, 100 },
        { 60, 80 },
        { 50, 64 },
        { 40, 50 },
        { 30, 40 },
        { 25, 32 },
        { 20, 25 },
        { 15, 20 },
        { 12.5, 16 },
        { 10, 12.5 },
        { 8, 10 },
        { 6, 8 },
        { 5, 6.25 },
        { 4, 5 },
        { 3, 4 },
        { 2.5, 3.125 },
        { 2, 2.5 },
        { 1.5, 2 },
        { 1.2, 1.5625 },
        { 1, 1.25 },
        { 0.8, 1 },
        { 0.6, 0.78125 }
    };
}

/// <summary>
///     天线因子数据，值类型
/// </summary>
internal struct Ap510Factor
{
    /// <summary>
    ///     频率
    /// </summary>
    public double Freq { get; set; }

    /// <summary>
    ///     因子
    /// </summary>
    public float Gene { get; set; }

    public static List<Ap510Factor> CreateGeneStructs(XmlDocument doc)
    {
        var geneDatas = new List<Ap510Factor>();
        if (doc.DocumentElement == null) return geneDatas;
        foreach (XmlNode node in doc.DocumentElement.ChildNodes) geneDatas.Add(CreateGeneStruct(node));
        return geneDatas;
    }

    public static Ap510Factor CreateGeneStruct(XmlNode node)
    {
        var geneData = new Ap510Factor();
        var list = new Dictionary<string, string>();
        foreach (XmlNode childNode in node) list.Add(childNode.Name, childNode.InnerText);
        geneData.Freq = double.Parse(list["Freq"]);
        geneData.Gene = float.Parse(list["Value"]);
        return geneData;
    }
}

public enum ScpiDataType
{
    Default,
    String,
    Map
}

public enum ScpiMapItemType
{
    String,
    Array
}

public abstract class ScpiMapItem
{
    public abstract ScpiMapItemType ItemType { get; }
    public abstract object GetData();
    protected abstract bool Convert(Encoding encoding, byte[] data, ref List<byte> bytes);

    public static ScpiMapItem Parse(Encoding encoding, byte[] data, ref List<byte> bytes)
    {
        if (bytes?.Any() != true) return null;
        ScpiMapItem mapItem;
        if (bytes[0] == '#')
            mapItem = new ScpiArrayMapItem();
        else
            mapItem = new ScpiStringMapItem();
        var b = mapItem.Convert(encoding, data, ref bytes);
        if (!b) return null;
        return mapItem;
    }
}

public class ScpiStringMapItem : ScpiMapItem
{
    public override ScpiMapItemType ItemType => ScpiMapItemType.String;
    public string Data { get; private set; }

    protected override bool Convert(Encoding encoding, byte[] data, ref List<byte> bytes)
    {
        encoding ??= Encoding.ASCII;
        Data = encoding.GetString(data);
        bytes.RemoveRange(0, data.Length);
        return true;
    }

    public override string ToString()
    {
        return Data;
    }

    public override object GetData()
    {
        return Data;
    }
}

public class ScpiArrayMapItem : ScpiMapItem
{
    public override ScpiMapItemType ItemType => ScpiMapItemType.Array;
    public int Payload { get; private set; }
    public int DataLength { get; private set; }
    public int Length { get; private set; }
    public byte[] Data { get; private set; }

    protected override bool Convert(Encoding encoding, byte[] data, ref List<byte> bytes)
    {
        encoding ??= Encoding.ASCII;
        if (data.Length < 2) return false;
        Payload = data[1] - '0';
        if (data.Length < 2 + Payload) return false;
        var sLen = encoding.GetString(data.Skip(2).Take(Payload).ToArray());
        if (!int.TryParse(sLen, out var len)) return false;
        DataLength = len;
        Length = 2 + Payload + DataLength;
        if (data.Length < Length)
        {
            if (bytes.Count < Length) return false;
            data = bytes.Take(Length).ToArray();
        }

        Data = data.Skip(2 + Payload).Take(DataLength).ToArray();
        bytes.RemoveRange(0, Length);
        return true;
    }

    public override string ToString()
    {
        return BitConverter.ToString(Data);
    }

    public override object GetData()
    {
        return Data;
    }
}

public abstract class ScpiDataStruct
{
    public int Payload { get; set; }
    public int DataLength { get; set; }
    public byte[] Data { get; set; }
    public Encoding Encoding { get; set; }
    public abstract ScpiDataType DataType { get; }
    public abstract string GetValueByKey(string key);

    protected virtual void Convert(int payload, int dataLen, byte[] data, Encoding encoding)
    {
        encoding ??= Encoding.ASCII;
        Encoding = encoding;
        Data = data;
        Payload = payload;
        DataLength = dataLen;
    }

    public static ScpiDataStruct Parse(int payload, int dataLen, byte[] data, Encoding encoding)
    {
        if (data?.Any() != true) return null;
        ScpiDataStruct dataStruct;
        var colonPos = Array.FindIndex(data, x => x == ':');
        if (colonPos < 0)
        {
            dataStruct = new ScpiDefaultData();
        }
        else
        {
            var equalSignPos = Array.FindIndex(data, colonPos, x => x == '=');
            if (equalSignPos < 0)
                dataStruct = new ScpiStringData();
            else
                dataStruct = new ScpiMapData();
        }

        dataStruct.Convert(payload, dataLen, data, encoding);
        return dataStruct;
    }
}

public class ScpiDefaultData : ScpiDataStruct
{
    public override ScpiDataType DataType => ScpiDataType.String;
    public string Content { get; set; }

    public override string GetValueByKey(string key)
    {
        return null;
    }

    protected override void Convert(int payload, int dataLen, byte[] data, Encoding encoding)
    {
        base.Convert(payload, dataLen, data, encoding);
        Content = encoding.GetString(data);
    }

    public override string ToString()
    {
        return Content;
    }
}

public class ScpiStringData : ScpiDataStruct
{
    public override ScpiDataType DataType => ScpiDataType.String;
    public string Code { get; set; }
    public string Content { get; set; }

    public override string GetValueByKey(string key)
    {
        if (string.Equals(key, Content, StringComparison.OrdinalIgnoreCase)) return Content;
        return null;
    }

    protected override void Convert(int payload, int dataLen, byte[] data, Encoding encoding)
    {
        base.Convert(payload, dataLen, data, encoding);
        var colonPos = Array.FindIndex(data, x => x == ':');
        var codeArray = data.Take(colonPos).ToArray();
        var contentArray = data.Skip(colonPos + 1).ToArray();
        Code = encoding.GetString(codeArray);
        Content = encoding.GetString(contentArray);
    }

    public override string ToString()
    {
        return Content;
    }
}

public class ScpiMapData : ScpiDataStruct
{
    public override ScpiDataType DataType => ScpiDataType.String;
    public string Code { get; set; }
    public Dictionary<string, ScpiMapItem> Content { get; set; }

    public override string GetValueByKey(string key)
    {
        if (Content == null) return null;
        if (!Content.TryGetValue(key, out var item)) return null;
        return item?.ToString();
    }

    protected override void Convert(int payload, int dataLen, byte[] data, Encoding encoding)
    {
        base.Convert(payload, dataLen, data, encoding);
        var colonPos = Array.FindIndex(data, x => x == ':');
        var codeArray = data.Take(colonPos).ToArray();
        var list = data.Skip(colonPos + 1).ToList();
        Code = encoding.GetString(codeArray);
        Content = new Dictionary<string, ScpiMapItem>();
        encoding.GetString(list.ToArray());
        while (list.Count > 0)
        {
            var commaPos = list.FindIndex(x => x == ',');
            var temp = new List<byte>();
            if (commaPos < 0)
            {
                temp.AddRange(list);
            }
            else
            {
                temp.AddRange(list.Take(commaPos));
                if (temp.Count == 0)
                {
                    list.RemoveRange(0, colonPos + 1);
                    continue;
                }
            }

            var equalSignPos = temp.FindIndex(x => x == '=');
            if (equalSignPos <= 0)
            {
                list.Clear();
                continue;
            }

            list.RemoveRange(0, equalSignPos + 1);
            var keyArray = temp.Take(equalSignPos).ToArray();
            var valueArray = temp.Skip(equalSignPos + 1).ToArray();
            var key = encoding.GetString(keyArray);
            if (valueArray.Length == 0) continue;

            var item = ScpiMapItem.Parse(encoding, valueArray, ref list);
            if (item == null) continue;
            Content.AddOrUpdate(key, item);
            if (list.Count > 0) list.RemoveAt(0);
        }
    }

    public override string ToString()
    {
        return Utils.ConvertToJson(Content);
    }
}