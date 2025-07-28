using Magneto.Device.DT1000AS.Driver.Base;

namespace Magneto.Device.DT1000AS.Driver;

public partial class Receiver
{
    private void CommandToDSP_Stop_One_SDCCH(short blkIdx)
    {
        var buffer = new byte[5];
        buffer[0] = 0xff;
        buffer[1] = 5;
        buffer[2] = (byte)CommandHeader.CommdStopOneSdcch;
        buffer[3] = (byte)blkIdx;
        Crc8Helper.Encode(buffer, 4);
        _taskSdcch8[blkIdx] = default;
        SendCommand(buffer);
    }

    private void CommandToDSP_Start_SDCCH(ChannelStruct phyicalChannel)
    {
        var idx = (byte)phyicalChannel.BlkIndex;
        // 忽略重复的立即指配
        if (_taskSdcch8[idx].RFNum == phyicalChannel.RFNum &&
            _taskSdcch8[idx].RFList[0] == phyicalChannel.RFList[0] &&
            _taskSdcch8[idx].MAIO == phyicalChannel.MAIO
           )
            return;
        if (phyicalChannel.AssChannelType != (short)ChannelType.ChSdcch8 || _taskSdcch8[idx].RFNum != 0) return;
        _taskSdcch8[idx] = phyicalChannel;
        var buffer = CreateCommand2Dsp(phyicalChannel);
        SendCommand(buffer);
        _taskSdcch8[idx].last_RX_Data_FN = _fn;
    }

    private byte[] CreateCommand2Dsp(ChannelStruct phyicalChannel)
    {
        if (phyicalChannel.RFNum > 20) return null;
        var len = 11 + 2 * phyicalChannel.RFNum;
        var buffer = new byte[len];
        buffer[0] = 0xff; // header
        switch (phyicalChannel.AssChannelType)
        {
            case (int)ChannelType.ChSdcch8:
                buffer[2] = (byte)CommandHeader.CommdStartOneSdcch;
                buffer[3] = (byte)phyicalChannel.BlkIndex;
                break;
            case (int)ChannelType.ChTchF:
                buffer[2] = (byte)CommandHeader.CommdStartTch;
                buffer[3] = 3;
                break;
            case (int)ChannelType.ChTchH0:
                buffer[2] = (byte)CommandHeader.CommdStartTch;
                buffer[3] = 0;
                break;
            case (int)ChannelType.ChTchH1:
                buffer[2] = (byte)CommandHeader.CommdStartTch;
                buffer[3] = 1;
                break;
        }

        short i = 4;
        buffer[i++] = phyicalChannel.TN;
        // 填充Time adnvace
        buffer[3] += (byte)((phyicalChannel.TA >> 5) << 3);
        buffer[4] += (byte)((phyicalChannel.TA & 0x1f) << 3);
        buffer[i++] = (byte)phyicalChannel.MAIO;
        buffer[i++] = (byte)phyicalChannel.HSN;
        buffer[i++] = (byte)(phyicalChannel.C0 >> 8);
        buffer[7] += (byte)(phyicalChannel.TSC << 2);
        buffer[i++] = (byte)(phyicalChannel.C0 & 0xff);
        buffer[i++] = (byte)phyicalChannel.RFNum;
        for (var n = 0; n < phyicalChannel.RFNum; n++)
        {
            buffer[i++] = (byte)(phyicalChannel.RFList[n] >> 8);
            buffer[i++] = (byte)(phyicalChannel.RFList[n] & 0xff);
        }

        buffer[1] = (byte)(i + 1);
        Crc8Helper.Encode(buffer, i);
        return buffer;
    }

    private void CommandToDSP_SetBCCH_ARFCN(ushort arfcn)
    {
        var buffer = new byte[8];
        buffer[0] = 0xff;
        buffer[1] = 8; //length
        buffer[2] = (byte)CommandHeader.CommdSetBcchArfcn;
        buffer[3] = (byte)(arfcn >> 8);
        buffer[4] = (byte)(arfcn & 0xff);
        if (_cellSearchFlag == 0)
            buffer[5] = _bsCcchans;
        else
            buffer[5] = 3;
        const int rssiReportRate = 6;
        buffer[5] += rssiReportRate << 4;
        buffer[6] = 1; //(x[6]&1)==1, send out heart beat every 0.25s
        Crc8Helper.Encode(buffer, 7);
        SendCommand(buffer);
    }
}