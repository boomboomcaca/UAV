using System;
using Contract.AIS;

namespace Magneto.Device;

public static class AisCommonMethods
{
    /// <summary>
    ///     根据船舶类型编码获取船舶类型
    /// </summary>
    /// <param name="st">船舶类型编码</param>
    public static ShipType GetShipType(int st)
    {
        if (st is 3 or 5)
            st = 3;
        else if (st is >= 10 and <= 19)
            st = 10;
        else if (st is >= 25 and <= 29)
            st = 25;
        else if (st is 38 or 39)
            st = 38;
        else if (st is >= 45 and <= 48)
            st = 45;
        else if (st is 56 or 57)
            st = 56;
        else if (st is >= 65 and <= 68)
            st = 65;
        else if (st is >= 75 and <= 78)
            st = 75;
        else if (st is >= 85 and <= 88)
            st = 85;
        else if (st is >= 95 and <= 98) st = 95;
        try
        {
            var shipT = typeof(ShipType);
            if (Enum.GetName(shipT, st) == null) st = 100;
            return (ShipType)Enum.Parse(shipT, Enum.GetName(shipT, st) ?? string.Empty);
        }
        catch
        {
            return ShipType.UnKnown;
        }
    }

    /// <summary>
    ///     根据船舶MMSI获取对应的国家名称
    /// </summary>
    /// <param name="mmsi">船舶MMSI</param>
    public static CountryId GetCountryId(int mmsi)
    {
        var mmsiStr = mmsi.ToString();
        if (mmsiStr.Length < 3) return CountryId.UnKnown;
        var index = int.Parse(mmsiStr[..3]);
        if (index is 209 or 210 or 212)
            index = 209;
        else if (index is 211 or 218)
            index = 211;
        else if (index is 215 or 248 or 249 or 256)
            index = 215;
        else if (index is 219 or 220)
            index = 219;
        else if (index is 224 or 225)
            index = 224;
        else if (index is >= 226 and <= 228)
            index = 226;
        else if (index is >= 232 and <= 235)
            index = 232;
        else if (index is 237 or >= 239 and <= 241)
            index = 237;
        else if (index is >= 244 and <= 246)
            index = 244;
        else if (index is >= 257 and <= 259)
            index = 257;
        else if (index is 265 or 266)
            index = 265;
        else if (index is 304 or 305)
            index = 204;
        else if (index is >= 308 and <= 311)
            index = 308;
        else if (index is 338 or >= 366 and <= 369)
            index = 338;
        else if (index is >= 351 and <= 354 or >= 370 and <= 372)
            index = 351;
        else if (index is >= 355 and <= 357)
            index = 355;
        else if (index is >= 375 and <= 377)
            index = 375;
        else if (index is 412 or 413)
            index = 412;
        else if (index is 431 or 432)
            index = 431;
        else if (index is 440 or 441)
            index = 440;
        else if (index is 563 or 564 or 565)
            index = 563;
        else if (index is 636 or 637)
            index = 636;
        else if (index is 674 or 677) index = 674;
        try
        {
            var t = typeof(CountryId);
            if (Enum.GetName(t, index) == null) index = 776;
            return (CountryId)Enum.Parse(t, Enum.GetName(t, index) ?? string.Empty);
        }
        catch
        {
            return CountryId.UnKnown;
        }
    }

    /// <summary>
    ///     根据消息类型ID获取对应消息类型名称
    /// </summary>
    /// <param name="id"></param>
    public static AisMessageType GetAisMessageType(int id)
    {
        if (id is >= 1 and <= 3)
            id = 1;
        else if (id is 4 or 11) id = 4;
        try
        {
            var msT = typeof(AisMessageType);
            if (Enum.GetName(msT, id) == null) id = 0;
            return (AisMessageType)Enum.Parse(msT, Enum.GetName(msT, id) ?? string.Empty);
        }
        catch
        {
            return AisMessageType.UnKnown;
        }
    }

    /// <summary>
    ///     根据航行状态编码获取对应的航行状态
    /// </summary>
    /// <param name="state"></param>
    public static NavigationState GetNavigationState(int state)
    {
        try
        {
            var nvT = typeof(NavigationState);
            if (Enum.GetName(nvT, state) != null)
                return (NavigationState)Enum.Parse(nvT, Enum.GetName(nvT, state) ?? string.Empty);
            return NavigationState.Undefined;
        }
        catch
        {
            return NavigationState.Undefined;
        }
    }
}