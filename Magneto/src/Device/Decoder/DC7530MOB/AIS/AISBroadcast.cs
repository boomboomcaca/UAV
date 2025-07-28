using System;
using System.Text;
using Contract.AIS;
using Magneto.Contract.AIS.Interface;

namespace Magneto.Device.DC7530MOB.AIS;

public class AisBroadcast : IAisMessage, IAisDecodable, ICloneable
{
    public AisBroadcast()
    {
    }

    public AisBroadcast(int messageId, int repeatInd, int msgId, int dest, string msg)
    {
        MessageId = messageId;
        RepeatInd = repeatInd;
        MsgId = msgId;
        MmsiDest = dest;
        Message = msg;
    }

    /// <summary>
    ///     消息ID
    /// </summary>
    public int MessageId { get; private set; }

    /// <summary>
    ///     转发指示符
    /// </summary>
    public int RepeatInd { get; private set; }

    /// <summary>
    ///     信源ID
    /// </summary>
    public int MsgId { get; private set; }

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
    ///     <para>消息类型：26</para>
    ///     <para>返回多时隙二进制消息</para>
    /// </summary>
    /// <param name="decBytes"></param>
    /// <returns></returns>
    public IAisMessage Decode(string decBytes)
    {
        if (decBytes.Length < 421)
            //DCMS.Server.BasicDefine.Notepad.WriteError("数据长度不符合该类型消息的长度！\r\n消息类型：26");
            return null;
        /* Possition Reports Message ID 1,2,3 bits 0-5 */
        MessageId = AisDecoder.GetDecValueByBinStr(decBytes[..6], false);
        RepeatInd = AisDecoder.GetDecValueByBinStr(decBytes.Substring(6, 2), false);
        /* User ID mmsi bits 8-37 */
        MsgId = AisDecoder.GetDecValueByBinStr(decBytes.Substring(8, 30), false);
        //目的地指示符，0广播，1寻址（对于MMSI目的地ID要用30个数据比特）
        var destInd = AisDecoder.GetDecValueByBinStr(decBytes.Substring(38, 1), false) == 1;
        //二进制标志，0不适用应用标识符比特，1二进制数据的编码（如16比特应用标识符所规定）
        var binary = AisDecoder.GetDecValueByBinStr(decBytes.Substring(39, 1), false) == 1;
        MmsiDest = destInd ? AisDecoder.GetDecValueByBinStr(decBytes.Substring(40, 30), false) : 0;
        Message = AisDecoder.GetDecStringFrom6BitStr(binary ? decBytes.Substring(71, 108) : decBytes.Substring(71, 78));
        return this;
    }

    AisMessageType IAisMessage.MsgId => throw new NotImplementedException();
    public int Mmsi => throw new NotImplementedException();

    public object Clone()
    {
        return MemberwiseClone();
    }

    public override string ToString()
    {
        var sbuilder = new StringBuilder();
        sbuilder.Append("AISBroadcast\n").Append("[").Append("MessageID:" + MessageId + "\n")
            .Append("RepeatIND:" + RepeatInd + "\n").Append("MsgID:" + MsgId + "\n")
            .Append("MMSIDest:" + MmsiDest + "\n").Append("Message:" + Message + "\n").Append("]");
        return sbuilder.ToString();
    }
}