using System;
using System.Text;
using System.Text.RegularExpressions;
using Magneto.Contract.AIS.Interface;

namespace Magneto.Device.DC7530MOB.AIS;

public class AisParser
{
    /**
     * Regular expression for AIS-Messages
     */
    //private static readonly Pattern pattern = Pattern.compile("!AIVDM\\,[1-9]{1}\\,[1-9]{1}\\,([0-9]{0,1})\\,[0-3A-B]{1}\\,([0-9\\:\\;\\<\\=\\>\\?\\@A-W\\`a-w]+)\\,[0-5]\\*[A-F0-9]{2}");
    private static readonly Regex _pattern =
        new(
            "!AIVDM\\,[1-9]{1}\\,[1-9]{1}\\,([0-9]{0,1})\\,[0-3A-B]{0,1}\\,([0-9\\:\\;\\<\\=\\>\\?\\@A-W\\`a-w]+)\\,[0-5]\\*[A-F0-9]{2}");

    private static readonly Regex _patternVdo =
        new(
            "!AIVDO\\,[1-9]{1}\\,[1-9]{1}\\,([0-9]{0,1})\\,[0-3A-B]{0,1}\\,([0-9\\:\\;\\<\\=\\>\\?\\@A-W\\`a-w]+)\\,[0-5]\\*[A-F0-9]{2}");

    /**
         * current decoded message
         */
    private string _currMsg = "";

    /**
         * current number of sentences, in case the message was sent in multiparts
         */
    private int _currSentenceNumber;

    /**
         * current sequence number identifier, in case the message was sent in
         * multiparts
         */
    private int _currSequenceNumber;

    /**
         * current total number of sentences needed to transfer the message, in case
         * the message was sent in multiparts
         */
    private int _currTotalNumOfMsgs;

    private bool _isWholeMsg;

    /**
         * previous number of sentences, in case the message was sent in multiparts
         */
    private int _oldSentenceNumber;

    /**
         * previous sequence number identifier, in case the message was sent in
         * multiparts
         */
    private int _oldSequenceNumber;

    /**
         * previous total number of sentences needed to transfer the message, in
         * case the message was sent in multiparts
         */
    private int _oldTotalNumOfMsgs;

    /**
         * Method for parsing encoded AIS-Messages. All messages are passed to the
         * parser in a raw format, will be validated and decoded.
         *
         * @param encodedMsg
         * @return decoded object of a type IAISMessage
         * @throws AISParseException
         */
    public IAisMessage Parse(string encodedMsg)
    {
        if (IsValidAis(encodedMsg))
        {
            var msgTokens = new string[6];
            var tokenCnt = 0;
            var index = 5;
            while ((index = encodedMsg.IndexOf(",", index, StringComparison.Ordinal)) != -1)
            {
                var currIndex = encodedMsg.IndexOf(",", index + 1, StringComparison.Ordinal);
                if (currIndex != -1)
                    msgTokens[tokenCnt] = encodedMsg.Substring(index + 1, currIndex - index - 1);
                else
                    msgTokens[tokenCnt] = encodedMsg.Substring(index + 1, encodedMsg.Length - index - 1);
                tokenCnt++;
                index++;
            }

            _isWholeMsg = false;
            if (msgTokens[0].Equals("1"))
            {
                _currMsg = msgTokens[4];
                _isWholeMsg = true;
            }
            else
            {
                _currTotalNumOfMsgs = int.Parse(msgTokens[0]);
                _currSentenceNumber = int.Parse(msgTokens[1]);
                _currSequenceNumber = int.Parse(msgTokens[2]);
                if (_currSentenceNumber == 1)
                {
                    _oldTotalNumOfMsgs = _currTotalNumOfMsgs;
                    _oldSentenceNumber = _currSentenceNumber;
                    _oldSequenceNumber = _currSequenceNumber;
                    _currMsg = msgTokens[4];
                }
                else
                {
                    if (_currTotalNumOfMsgs > _oldTotalNumOfMsgs
                        || _currSentenceNumber != _oldSentenceNumber + 1
                        || _currSequenceNumber != _oldSequenceNumber)
                    {
                        InitMsgParams();
                        return null;
                    }

                    _currMsg += msgTokens[4];
                    _oldSentenceNumber = _currSentenceNumber;
                    if (_currSentenceNumber == _oldTotalNumOfMsgs) _isWholeMsg = true;
                }
            }

            if (_isWholeMsg)
            {
                InitMsgParams();
                var aisMessage = AisDecoder.Decode(_currMsg);
                return aisMessage;
            }

            return null;
        }

        throw new Exception("不是合法的!AVIDM\\!AVIDO包数据，或者包含不合法字符！！");
    }

    /**
         * Prepare parser for a new raw AIS-Message to parse.
         */
    private void InitMsgParams()
    {
        _oldTotalNumOfMsgs = 0;
        _oldSentenceNumber = 0;
        _oldSequenceNumber = 0;
    }

    /// <summary>
    ///     AIS消息合法性验证
    /// </summary>
    /// <param name="ais"></param>
    /// <returns></returns>
    public static bool IsValidAis(string ais)
    {
        var isValid = false;
        var index = ais.IndexOf("!AIVDM", StringComparison.Ordinal);
        var msg = ais;
        if (index != -1) msg = ais.Substring(index, ais.Length);
        if (!ValidCrc(msg))
        {
            isValid = false;
        }
        else
        {
            isValid = _pattern.IsMatch(msg); //pattern.matcher(msg).matches();
            isValid = _patternVdo.IsMatch(msg) || isValid;
        }

        return isValid;
    }

    /// <summary>
    ///     计算AIS消息串的校验和 AIVDM,,,,,,0之间的字符
    /// </summary>
    /// <param name="ais"></param>
    /// <returns></returns>
    public static string CalcCrc(string ais)
    {
        byte[] data = null;
        if (ais.Contains("!") && ais.Contains("*"))
            data = Encoding.ASCII.GetBytes(ais[1..ais.IndexOf("*", StringComparison.Ordinal)]);
        else
            data = Encoding.ASCII.GetBytes(ais);
        var crc = 0;
        foreach (var pos in data)
            if (crc == 0)
                crc = pos;
            else
                crc ^= pos;
        //转换为16进制,且统一为大写
        var result = Convert.ToString(crc, 16).ToUpper();
        return result;
    }

    /// <summary>
    ///     提取AIS消息穿里面的校验和（*后面的十六进制值）
    /// </summary>
    /// <param name="ais"></param>
    /// <returns></returns>
    public static string ExtractCrc(string ais)
    {
        var crc = ais[(ais.IndexOf('*') + 1)..];
        return crc;
    }

    /// <summary>
    ///     验证抽取的校验和是否和计算的校验和相等
    /// </summary>
    /// <param name="ais">输入的AIS消息串</param>
    /// <returns>是否合法的消息串</returns>
    public static bool ValidCrc(string ais)
    {
        var cmp1 = ExtractCrc(ais);
        var cmp2 = CalcCrc(ais);
        if (cmp1.Length != cmp2.Length)
        {
            var len = cmp1.Length > cmp2.Length ? cmp1.Length : cmp2.Length;
            if (cmp1.Length < cmp2.Length)
                cmp1 = cmp1.PadLeft(len, '0');
            else
                cmp2 = cmp2.PadLeft(len, '0');
        }

        return cmp1.Equals(cmp2);
    }
}