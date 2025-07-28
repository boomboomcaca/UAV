using System;
using System.Text;
using Magneto.Contract.AIS.Interface;

namespace Magneto.Device.DC7530MOB.AIS;

public class AisDecoder
{
    /// <summary>
    ///     将编码的消息解码为相应对象
    /// </summary>
    /// <param name="encodedMsg">编码消息串</param>
    /// <returns>解码后对应的对象</returns>
    public static IAisMessage Decode(string encodedMsg)
    {
        if (string.IsNullOrEmpty(encodedMsg))
            //DCMS.Server.BasicDefine.Notepad.WriteError("空的AIS消息串，无法解析！");
            return null;
        var toDecBytes = Encoding.ASCII.GetBytes(encodedMsg);
        var decBytes = Ascii8To6BitBin(toDecBytes);
        if (decBytes == null) return null;
        int msgId = decBytes[0];
        var decodedBinString = GetDecodedStr(decBytes);
        switch (msgId)
        {
            case 1:
            case 2:
            case 3:
                //A类船舶位置报告
                return new AisPositionA().Decode(decodedBinString);
            case 4:
            case 11:
                //固定位置AIS基站信息
                return new AisBaseStation().Decode(decodedBinString);
            case 5:
                //船舶静态与相关航行数据
                return new AisStaticAVoyage().Decode(decodedBinString);
            case 18:
                //B类船舶位置数据（通常由小型船只发送）
                return new AisPositionB().Decode(decodedBinString);
            case 19:
                //扩展的B类船舶位置信息
                return new AisPositionExtB().Decode(decodedBinString);
            case 24:
                return new AisStaticDataReport().Decode(decodedBinString);
            case 26:
                //多时隙二进制消息
                return new AisBroadcast().Decode(decodedBinString);
        }

        //DCMS.Server.BasicDefine.Notepad.WriteError("暂不支持该类型数据的解析，消息类型= " + " " + msgId);
        return null; //temporary
    }

    /// <summary>
    ///     将标准二进制编码字符转换为6位二进制串
    /// </summary>
    /// <param name="toDecBytes">要转换的二进制串</param>
    /// <returns>转换后的6位二进制串</returns>
    private static byte[] Ascii8To6BitBin(byte[] toDecBytes)
    {
        var convertedBytes = new byte[toDecBytes.Length];
        try
        {
            //对63个编码字符进行6位二进制串的转换
            for (var i = 0; i < toDecBytes.Length; i++)
            {
                if (toDecBytes[i] < 48 || (toDecBytes[i] > 87 && toDecBytes[i] < 96) || toDecBytes[i] > 119)
                {
                    //DCMS.Server.BasicDefine.Notepad.WriteError(string.Format("AIS消息串中包含非法字符！非法字符为：{0}", (char)toDecBytes[i]));
                    convertedBytes = null;
                    break;
                }

                if (toDecBytes[i] >= 48 && toDecBytes[i] <= 87)
                    convertedBytes[i] = (byte)(toDecBytes[i] - 48);
                else if (toDecBytes[i] >= 96 && toDecBytes[i] <= 119) convertedBytes[i] = (byte)(toDecBytes[i] - 56);
            }
        }
        catch (Exception)
        {
            convertedBytes = null;
        }

        return convertedBytes;
    }

    /// <summary>
    ///     将二进制字符串补齐6位每字符，然后连接成解码字符串
    /// </summary>
    /// <param name="decBytes"></param>
    /// <returns></returns>
    private static string GetDecodedStr(byte[] decBytes)
    {
        var decStr = "";
        foreach (int decByte in decBytes)
        {
            var bitStr = Convert.ToString(decByte, 2);
            if (bitStr.Length < 6) bitStr = bitStr.PadLeft(6, '0');
            decStr += bitStr;
        }

        return decStr;
    }

    /// <summary>
    ///     将六位二进制字符转换为八位二进制字符
    /// </summary>
    /// <param name="byteToDec"></param>
    /// <returns></returns>
    private static byte Convert6BitCharToStandartdAscii(byte byteToDec)
    {
        byte decByte = 0;
        if (byteToDec < 32)
            decByte = (byte)(byteToDec + 64);
        else if (byteToDec < 63) decByte = byteToDec;
        return decByte;
    }

    /// <summary>
    ///     从二进制字符串中获取对应的十进制值
    /// </summary>
    /// <param name="decStr">二进制串</param>
    /// <param name="signBit"></param>
    /// <returns>十进制值</returns>
    public static int GetDecValueByBinStr(string decStr, bool signBit)
    {
        var decValue = 0;
        try
        {
            decValue = Convert.ToInt32(decStr, 2);
            if (signBit && decStr[0] == '1') //符号位为1表明是负数，补码+1=>负数
            {
                var invert = new char[decStr.Length];
                invert.Fill('1'); //自定义扩展方法
                decValue ^= Convert.ToInt32(new string(invert), 2);
                decValue += 1;
                decValue = -decValue;
            }
        }
        catch (Exception)
        {
        }

        return decValue;
    }

    /// <summary>
    ///     将六位二进制串转换为标准二进制串，并转换为字符
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string GetDecStringFrom6BitStr(string str)
    {
        var txt = "";
        try
        {
            for (var i = 0; i < str.Length; i = i + 6)
            {
                var byt = (byte)Convert.ToInt32(str.Substring(i, 6), 2); //Integer.parseInt(str.Substring(i, i + 6), 2);
                byt = Convert6BitCharToStandartdAscii(byt);
                var convChar = (char)byt;
                if (convChar == '@') break;
                txt += (char)byt;
            }

            txt = txt.Trim();
        }
        catch (Exception)
        {
        }

        return txt;
    }
}

public static class Array
{
    public static void Fill(this char[] arr, char val)
    {
        for (var i = 0; i < arr.Length; i++) arr[i] = val;
    }
}