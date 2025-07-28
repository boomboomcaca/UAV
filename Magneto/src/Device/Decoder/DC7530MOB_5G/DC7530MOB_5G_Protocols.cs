using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Magneto.Protocol.Define;

namespace Magneto.Device.DC7530MOB_5G;

public partial class Dc7530Mob5G
{
    private bool IsLteTdd(int arfcn)
    {
        return arfcn is >= 36000 and <= 46589;
    }

    /// <summary>
    ///     获取带宽
    /// </summary>
    /// <param name="mode">基站制式</param>
    /// <param name="freq">频率</param>
    /// <returns>带宽</returns>
    private double GetBandwidth(DuplexMode mode, double freq)
    {
        switch (mode)
        {
            case DuplexMode.Gsm:
                if (freq is >= 1845 and <= 1860)
                    return 15 * 1e3;
                return 0.2 * 1e3;
            case DuplexMode.LteFdd:
            case DuplexMode.TdLte:
            {
                if (freq is >= 1845 and <= 1860)
                    return 15 * 1e3;
                return 20 * 1e3;
            }
            case DuplexMode.Wcdma:
                return 5 * 1e3;
            case DuplexMode.Cdma20001X:
            case DuplexMode.Cdma20001XEvdo:
                return 1.23 * 1e3;
            case DuplexMode.TdScdma:
                return 1.6 * 1e3;
        }

        return 0.2 * 1e3;
    }

    private double GetFieldStrengthFormRxPower(double rxPower, double freq)
    {
        var u = rxPower + 107;
        var k = -29.77 - 5 + 20 * Math.Log10(freq);
        return u + k + 1;
    }

    private uint ToUInt32(string str, bool isHex = false, uint defaultValue = 0)
    {
        if (string.IsNullOrEmpty(str)) return defaultValue;
        var numberStyles = isHex ? NumberStyles.HexNumber : NumberStyles.Integer;
        if (uint.TryParse(str, numberStyles, null, out var value)) return value;
        return defaultValue;
    }

    private ulong ToUInt64(string str, bool isHex = false, ulong defaultValue = 0)
    {
        if (string.IsNullOrEmpty(str)) return defaultValue;
        var numberStyles = isHex ? NumberStyles.HexNumber : NumberStyles.Integer;
        if (ulong.TryParse(str, numberStyles, null, out var value)) return value;
        return defaultValue;
    }

    private int ToInt32(string str, bool isHex = false, int defaultValue = 0)
    {
        if (string.IsNullOrEmpty(str)) return defaultValue;
        var numberStyles = isHex ? NumberStyles.HexNumber : NumberStyles.Integer;
        if (int.TryParse(str, numberStyles, null, out var value)) return value;
        return defaultValue;
    }

    private double ToDouble(string str, double defaultValue = default)
    {
        if (string.IsNullOrEmpty(str)) return defaultValue;
        if (double.TryParse(str, out var value)) return value;
        return defaultValue;
    }

    #region 信道号转换为频率（MHz）

    private bool ChanNumToFreq(DuplexMode mode, string channelNum, out double downlinkFreq)
    {
        downlinkFreq = 0;
        if (string.IsNullOrWhiteSpace(channelNum)) return false;
        if (!int.TryParse(channelNum, out var chanNum)) return false;
        switch (mode)
        {
            case DuplexMode.Gsm:
            {
                return GsmChanNumToFreq(chanNum, out downlinkFreq);
            }
            case DuplexMode.Wcdma:
            {
                return WcdmaChannelNumToFreq(chanNum, out downlinkFreq);
            }
            case DuplexMode.TdScdma:
            {
                return TdcdmaChannelNumToFreq(chanNum, out downlinkFreq);
            }
            case DuplexMode.LteFdd:
            case DuplexMode.TdLte:
            {
                return LteChannelNumToFreq(chanNum, out downlinkFreq);
            }
            case DuplexMode.Cdma20001X:
            case DuplexMode.Cdma20001XEvdo:
            {
                return CdmaChannelNumToFreq(chanNum, out downlinkFreq);
            }
            case DuplexMode.Nr5G:
            {
                return NrChannelNumToFreq(chanNum, out downlinkFreq);
            }
            default:
                return false;
        }
    }

    /// <summary>
    ///     GSM制式信道号转换为频率
    /// </summary>
    /// <param name="channelNum">信道号</param>
    /// <param name="downlinkFreq">频率(单位MHz)</param>
    /// <returns>是否转换成功</returns>
    private bool GsmChanNumToFreq(int channelNum, out double downlinkFreq)
    {
        downlinkFreq = 0;
        if (channelNum is >= 0 and <= 124)
            //0≤信道号≤124时的频率计算公式
            downlinkFreq = 935 + 0.2 * channelNum;
        else if (channelNum is >= 512 and <= 885)
            //512≤信道号≤885时的频率计算公式
            downlinkFreq = 1805.2 + 0.2 * (channelNum - 512);
        else if (channelNum is >= 975 and <= 1023)
            //975≤信道号≤1023时的频率计算公式
            downlinkFreq = 935 + 0.2 * (channelNum - 1024);
        else
            return false;
        return true;
    }

    /// <summary>
    ///     移动3G TD-SCDMA制式信道号转换为频率
    /// </summary>
    /// <param name="channelNum">信道号</param>
    /// <param name="downlinkFreq">频率(单位MHz)</param>
    /// <returns>是否转换成功</returns>
    private bool TdcdmaChannelNumToFreq(int channelNum, out double downlinkFreq)
    {
        downlinkFreq = channelNum / 5d; //信道频率信道号/5
        if (downlinkFreq is >= 1880 and <= 1920 or >= 2010 and <= 2025 or >= 2300 and <= 2400) return true;

        downlinkFreq = 0;
        return false;
    }

    /// <summary>
    ///     联通电信3G WCDMA制式信道号转换为频率
    /// </summary>
    /// <param name="channelNum">信道号</param>
    /// <param name="downlinkFreq">频率(单位MHz)</param>
    /// <returns>是否转换成功</returns>
    private bool WcdmaChannelNumToFreq(int channelNum, out double downlinkFreq)
    {
        downlinkFreq = 0;
        if (channelNum is >= 10550 and <= 10850)
            downlinkFreq = channelNum / 5d; //信道频率信道号/5
        else if (channelNum is >= 3070 and <= 3100)
            downlinkFreq = channelNum / 5d + 340;
        else
            return false;
        return true;
    }

    /// <summary>
    ///     移动联通电信4G制式信道号转换为频率
    /// </summary>
    /// <param name="channelNum">信道号</param>
    /// <param name="downlinkFreq">频率(单位MHz)</param>
    /// <returns>是否转换成功</returns>
    private bool LteChannelNumToFreq(int channelNum, out double downlinkFreq)
    {
        downlinkFreq = 0;
        var bands =
            new List<(int band, double dFDL_low, int nOffs_DL, int nMax_DL, double dFUL_low, int nOffs_UL, int nMax_UL,
                bool isFdd)>
            {
                (1, 2110d, 0, 599, 1920d, 18000, 18599, true),
                (2, 1930d, 600, 1199, 1850d, 18600, 19199, true),
                (3, 1805d, 1200, 1949, 1710d, 19200, 19949, true),
                (4, 2110d, 1950, 2399, 1710d, 19950, 20399, true),
                (5, 869d, 2400, 2659, 824d, 20400, 20649, true),
                (6, 875d, 2650, 2749, 830d, 20650, 20749, true),
                (7, 2620d, 2750, 3449, 2500d, 20750, 21449, true),
                (8, 925d, 3450, 3799, 880d, 21450, 21799, true),
                (9, 1844.9d, 3800, 4149, 1749.9d, 21800, 22149, true),
                (10, 2110d, 4150, 4749, 1710d, 22150, 22749, true),
                (11, 1475.9d, 4750, 4949, 1427.9d, 22750, 22949, true),
                (12, 729d, 5010, 5179, 699d, 23010, 23179, true),
                (13, 746d, 5180, 5279, 777d, 23180, 23279, true),
                (14, 758d, 5280, 5379, 788d, 23280, 23379, true),
                (17, 734d, 5730, 5849, 704d, 23730, 23849, true),
                (18, 860d, 5850, 5999, 815d, 23850, 23999, true),
                (19, 875d, 6000, 6149, 830d, 24000, 24149, true),
                (20, 791d, 6150, 6449, 832d, 24150, 24449, true),
                (21, 1495.9d, 6450, 6599, 1447.9d, 24450, 24599, true),
                (22, 3510d, 6600, 7399, 3410d, 24600, 25399, true),
                (23, 2180d, 7500, 7699, 2000d, 25500, 25699, true),
                (24, 1525d, 7700, 8039, 1626.5d, 25700, 26039, true),
                (25, 1930d, 8040, 8689, 1850d, 26040, 26689, true),
                (26, 859d, 8690, 9039, 814d, 26690, 27039, true),
                (27, 852d, 9040, 9209, 807d, 27040, 27209, true),
                (28, 758d, 9210, 9659, 703d, 27210, 27659, true),
                (29, 717d, 9660, 9769, -1d, -1, -1, true),
                (33, 1900d, 36000, 36199, 1900d, 36000, 36199, false),
                (34, 2010d, 36200, 36349, 2010d, 36200, 36349, false),
                (35, 1850d, 36350, 36949, 1850d, 36350, 36949, false),
                (36, 1930d, 36950, 37549, 1930d, 36950, 37549, false),
                (37, 1910d, 37550, 37749, 1910d, 37550, 37749, false),
                (38, 2570d, 37750, 38249, 2570d, 37750, 38249, false),
                (39, 1880d, 38250, 38649, 1880d, 38250, 38649, false),
                (40, 2300d, 38650, 39649, 2300d, 38650, 39649, false),
                (41, 2496d, 39650, 41589, 2496d, 39650, 41589, false),
                (42, 3400d, 41590, 43589, 3400d, 41590, 43589, false),
                (43, 3600d, 43590, 45589, 3600d, 43590, 45589, false),
                (44, 703d, 45590, 46589, 703d, 45590, 46589, false)
            };
        var band = bands.FirstOrDefault(p => channelNum >= p.nOffs_DL && channelNum <= p.nMax_DL);
        if (band.band == 0) return false;
        //FDL = FDL_low + 0.1(NDL– NOffs-DL)
        downlinkFreq = band.dFDL_low + 0.1 * (channelNum - band.nOffs_DL);
        return true;
    }

    /// <summary>
    ///     电信2G3G制式信道号转换为频率
    /// </summary>
    /// <param name="channelNum">信道号</param>
    /// <param name="downlinkFreq">频率(单位MHz)</param>
    /// <returns>是否转换成功</returns>
    private bool CdmaChannelNumToFreq(int channelNum, out double downlinkFreq)
    {
        downlinkFreq = 0;
        if (channelNum is >= 1 and <= 799)
            downlinkFreq = 870 + channelNum * 0.03;
        else if (channelNum is >= 991 and <= 1023)
            downlinkFreq = 870 + (channelNum - 1023) * 0.03;
        else if (channelNum is >= 1024 and <= 1323)
            downlinkFreq = 860.04 + (channelNum - 1024) * 0.03;
        else
            return false;
        return true;
    }

    /// <summary>
    ///     5G NR制式信道号转换为频率
    /// </summary>
    /// <param name="channelNum">信道号</param>
    /// <param name="downlinkFreq">频率(单位MHz)</param>
    /// <returns>是否转换成功</returns>
    private bool NrChannelNumToFreq(int channelNum, out double downlinkFreq)
    {
        downlinkFreq = 0;
        var list = new List<(bool flag, int nMin, int nMax, double freqGlobalMHz, double freqRefOffsetMHz)>
        {
            (true, 0, 599999, 5 * 1e-3, 0),
            (true, 600000, 2016666, 15 * 1e-3, 3000),
            (true, 2016667, 3279165, 60 * 1e-3, 24250.08)
        };
        var (flag, nMin, _, freqGlobal, freqRefOffset) =
            list.FirstOrDefault(p => channelNum >= p.nMin && channelNum <= p.nMax);
        if (!flag) return false;
        downlinkFreq = Math.Round(freqRefOffset + freqGlobal * (channelNum - nMin), 6);
        return true;
    }

    #endregion
}