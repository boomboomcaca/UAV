using System;
using System.Diagnostics;
using Magneto.Protocol.Data;

namespace Magneto.Device;

/// <summary>
///     加密类型
/// </summary>
internal enum CaType
{
    UnEncryped,
    Encryped
}

/// <summary>
///     节目操作类型
/// </summary>
internal enum ProgramType
{
    /// <summary>
    ///     搜索
    /// </summary>
    Search,

    /// <summary>
    ///     播放
    /// </summary>
    Play
}

/// <summary>
///     参数下达指令
/// </summary>
internal class ProgramCmd
{
    /// <summary>
    ///     指令类容
    /// </summary>
    public object Cmd;

    /// <summary>
    ///     指令类型
    /// </summary>
    public ProgramType Type;
}

/// <summary>
///     S7000节目信息结构
/// </summary>
internal struct S7000Program
{
    /// <summary>
    ///     序号，数值从0开始，最大值为PMT Real Quant-1
    /// </summary>
    public int No { get; set; }

    /// <summary>
    ///     节目号
    /// </summary>
    public int PNum { get; set; }

    /// <summary>
    ///     CA加密标志，UnEncryped为解密，Encryped为加密
    /// </summary>
    public CaType Ca { get; set; }

    /// <summary>
    ///     节目名，表示节目名称为cctv-1
    /// </summary>
    public string SerName { get; set; }

    /// <summary>
    ///     节目提供商名称，CTV
    /// </summary>
    public string ProName { get; set; }

    /// <summary>
    ///     该节目的流类型 DIG TV
    /// </summary>
    public string SerType { get; set; }

    /// <summary>
    ///     视频分辨率
    /// </summary>
    public string Resolution { get; set; }

    private int _index;

    public bool GetData(string s)
    {
        var split = s.Split(',');
        _index = 0;
        var filter = '=';
        try
        {
            No = int.Parse(split[_index].Split(filter)[1]);
            _index++;
            PNum = int.Parse(split[_index].Split(filter)[1]);
            _index++;
            Ca = (CaType)Enum.Parse(typeof(CaType), split[_index].Split(filter)[1]);
            _index++;
            SerName = split[_index].Split(filter)[1];
            _index++;
            ProName = split[_index].Split(filter)[1];
            _index++;
            SerType = split[_index].Split(filter)[1];
            _index++;
            Resolution = split[_index].Split(filter)[1];
            return true;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"解析节目内容 {s} 出错:{ex}");
            return false;
        }
    }

    /// <summary>
    ///     将s7000的节目信息转换为客户端展示的节目信息
    /// </summary>
    public ChannelProgramInfo ToProgram()
    {
        return new ChannelProgramInfo
        {
            Index = No,
            ProgramNumber = No.ToString(),
            Provider = ProName,
            Resolution = Resolution,
            ProgramName = SerName,
            FlowType = SerType,
            Ca = Ca == CaType.Encryped
        };
    }
}