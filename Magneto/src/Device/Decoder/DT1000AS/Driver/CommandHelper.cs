using Magneto.Device.DT1000AS.Driver.Base;

namespace Magneto.Device.DT1000AS.Driver;

public struct CellSearchRequest
{
    public bool Gsm1800Search { get; set; }
    public bool Gsm900Search { get; set; }
    public bool Cmcc900Search { get; set; }
    public bool Cucc900Search { get; set; }
    public bool Cmcc1800Search { get; set; }
    public bool Cucc1800Search { get; set; }
    public bool GsmrSearch { get; set; }
    public int MinRssiScan { get; set; }
    public bool RepeatScan { get; set; }
}

internal static class CommandHelper
{
    /// <summary>
    ///     小区搜索命令
    /// </summary>
    /// <param name="request">请求参数体</param>
    /// <returns>命令</returns>
    public static byte[] CreateCellSearchCommand(CellSearchRequest request)
    {
        var buffer = new byte[7];
        buffer[0] = 0xff;
        buffer[1] = 7;
        if (request.Gsm1800Search == request.Gsm900Search)
            buffer[2] = (byte)CommandHeader.CommdCellSearch;
        else if (request is { Gsm1800Search: true, Gsm900Search: false })
            buffer[2] = (byte)CommandHeader.CommdCellSearch1800;
        else
            buffer[2] = (byte)CommandHeader.CommdCellSearch900;
        var minRssiScan = -request.MinRssiScan;
        if (minRssiScan < 0) minRssiScan = -minRssiScan;
        if (minRssiScan > 255) minRssiScan = 255;
        buffer[3] = (byte)minRssiScan;
        buffer[4] = 0;
        buffer[4] = (byte)((request.Cmcc900Search.ToInt32() << 0) +
                           (request.Cucc900Search.ToInt32() << 1) +
                           (request.Cmcc1800Search.ToInt32() << 2) +
                           (request.Cucc1800Search.ToInt32() << 3) +
                           (request.GsmrSearch.ToInt32() << 4));
        buffer[5] = 0;
        Crc8Helper.Encode(buffer, 6);
        return buffer;
    }

    /// <summary>
    ///     停止搜索命令
    /// </summary>
    /// <returns>命令</returns>
    public static byte[] CreateStopSearchCommand()
    {
        var buffer = new byte[4];
        buffer[0] = 0xff;
        buffer[1] = 4; //length
        buffer[2] = (byte)CommandHeader.CommdIdle;
        Crc8Helper.Encode(buffer, 3);
        return buffer;
    }
}