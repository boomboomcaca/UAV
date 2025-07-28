using System;
using System.Text;
using Contract.AIS;
using Magneto.Contract.AIS.Interface;

namespace Magneto.Device.Parser;

public class AisBroadcast : IAisMessage, IAisDecodable, ICloneable
{
    public AisBroadcast()
    {
    }

    public AisBroadcast(AisMessageType messageId, int repeatInd, int msgId, int dest, string msg)
    {
        MsgId = messageId;
        RepeatInd = repeatInd;
        Mmsi = msgId;
        MmsiDest = dest;
        Message = msg;
    }

    /// <summary>
    ///     转发指示符
    /// </summary>
    public int RepeatInd { get; private set; }

    /// <summary>
    ///     目的地ID
    /// </summary>
    public int MmsiDest { get; private set; }

    /// <summary>
    ///     二进制数据
    /// </summary>
    public string Message { get; private set; }

    /// <summary>
    ///     解析编码数据
    ///     <para>消息类型:26</para>
    ///     <para>返回多时隙二进制消息</para>
    /// </summary>
    /// <param name="decBytes"></param>
    public IAisMessage Decode(string decBytes)
    {
        if (decBytes.Length < 421)
            //DCMS.Server.BasicDefine.Notepad.WriteError("数据长度不符合该类型消息的长度!\r\n消息类型:26");
            return null;
        /* Possition Reports Message ID 1,2,3 bits 0-5 */
        var id = AisDecoder.GetDecValueByBinStr(decBytes[..6], false);
        MsgId = AisCommonMethods.GetAisMessageType(id);
        RepeatInd = AisDecoder.GetDecValueByBinStr(decBytes.Substring(6, 2), false);
        /* User ID mmsi bits 8-37 */
        Mmsi = AisDecoder.GetDecValueByBinStr(decBytes.Substring(8, 30), false);
        //目的地指示符，0广播，1寻址（对于MMSI目的地ID要用30个数据比特）
        var destInd = AisDecoder.GetDecValueByBinStr(decBytes.Substring(38, 1), false) == 1;
        //二进制标志，0不适用应用标识符比特，1二进制数据的编码（如16比特应用标识符所规定）
        var binary = AisDecoder.GetDecValueByBinStr(decBytes.Substring(39, 1), false) == 1;
        if (destInd)
            MmsiDest = AisDecoder.GetDecValueByBinStr(decBytes.Substring(40, 30), false);
        else
            MmsiDest = 0;
        if (binary)
            Message = AisDecoder.GetDecStringFrom6BitStr(decBytes.Substring(71, 108));
        else
            Message = AisDecoder.GetDecStringFrom6BitStr(decBytes.Substring(71, 78));
        return this;
    }

    /// <summary>
    ///     消息ID
    /// </summary>
    public AisMessageType MsgId { get; private set; }

    /// <summary>
    ///     信源ID
    /// </summary>
    public int Mmsi { get; private set; }

    public object Clone()
    {
        return MemberwiseClone();
    }

    public override string ToString()
    {
        var sbuilder = new StringBuilder();
        sbuilder.Append("AISBroadcast\n")
            .Append("[")
            .Append("MessageID:")
            .Append(MsgId)
            .Append('\n')
            .Append("RepeatIND:")
            .Append(RepeatInd)
            .Append('\n')
            .Append("MsgID:")
            .Append(Mmsi)
            .Append('\n')
            .Append("MMSIDest:")
            .Append(MmsiDest)
            .Append('\n')
            .Append("Message:")
            .Append(Message)
            .Append('\n')
            .Append("]");
        return sbuilder.ToString();
    }
}