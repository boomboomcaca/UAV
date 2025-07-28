namespace Magneto.Device.DT1000AS.Driver.Base;

internal enum CommandHeader
{
    CommdCellSearch = 1,
    CommdStopOneSdcch,
    CommdStopTch,
    CommdSetBcchArfcn,
    CommdStartTch,
    CommdStartOneSdcch,
    CommdBcchSdcchTchRx,
    CommdTestRf,
    CommdIdle,
    CommdTestDelay,
    CommdCellSearch900,
    CommdCellSearch1800,
    CommdRssiScan,
    CommdC2I,
    CommdAgcDisable
}