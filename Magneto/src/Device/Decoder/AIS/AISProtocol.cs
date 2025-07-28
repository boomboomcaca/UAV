using System.Text;

namespace Magneto.Device;

/// <summary>
///     定义AVIDM,AVIDO结构对分条发送的语句进行合并
///     <para>AVIDM和AVIDO协议格式一致</para>
/// </summary>
public struct Avidm
{
    /// <summary>
    ///     包类型（AVIDM（其他船舶信息），AVIDO（本船信息））
    /// </summary>
    public readonly string PacketType;

    /// <summary>
    ///     该条语句被分成多少条语句发送
    /// </summary>
    public readonly int SegCount;

    /// <summary>
    ///     子句编号（1，2，3...）
    /// </summary>
    public readonly int Fragment;

    /// <summary>
    ///     消息识别号
    /// </summary>
    public readonly string Identify;

    /// <summary>
    ///     发送数据的信道（A,B）
    /// </summary>
    public readonly string Channel;

    /// <summary>
    ///     消息体
    /// </summary>
    public readonly string Body;

    /// <summary>
    ///     结束符（*）
    /// </summary>
    public readonly string End;

    public Avidm(string packet, int segcount, int frag, string identify, string channel, string body, string end)
    {
        PacketType = packet;
        SegCount = segcount;
        Fragment = frag;
        Identify = identify;
        Channel = channel;
        Body = body;
        End = end;
    }

    public override string ToString()
    {
        var sbuilder = new StringBuilder();
        sbuilder.Append(PacketType)
            .Append(",")
            .Append(SegCount)
            .Append(",")
            .Append(Fragment)
            .Append(",")
            .Append(Identify)
            .Append(",")
            .Append(Channel)
            .Append(",")
            .Append(Body)
            .Append(",")
            .Append(End);
        return sbuilder.ToString();
    }

    public override bool Equals(object obj)
    {
        if (obj is not Avidm avidm) return false;
        return PacketType == avidm.PacketType &&
               SegCount == avidm.SegCount &&
               Fragment == avidm.Fragment &&
               Identify == avidm.Identify &&
               Channel == avidm.Channel &&
               Body == avidm.Body &&
               End == avidm.End;
    }

    public override int GetHashCode()
    {
        return PacketType.GetHashCode() ^ SegCount ^ Fragment ^
               Identify.GetHashCode() ^ Channel.GetHashCode() ^
               Body.GetHashCode() ^ End.GetHashCode();
    }
}