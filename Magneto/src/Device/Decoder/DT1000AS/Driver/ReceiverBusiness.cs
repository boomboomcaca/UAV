using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Device.DT1000AS.Driver.Base;

namespace Magneto.Device.DT1000AS.Driver;

public partial class Receiver
{
    private void ResolveData(byte[] data, int crcFlag)
    {
        switch (crcFlag)
        {
            case (int)PacketHeader.PacketBcchArfcn:
                ProcessCellSearchData(data);
                break;
            case (int)PacketHeader.PacketSlot:
                BcchSdcchProcess(data);
                break;
            case (int)PacketHeader.PacketBcchRssi:
                var bcchRssi = (short)(data[16] - RssiOffset);
                _bcchData.RSSI = bcchRssi;
                break;
            case (int)PacketHeader.PacketSdcchRssi:
                byte sdcchExsit = 1;
                for (var n = 0; n < 8; n++)
                    if (_taskSdcch8[n].RFNum > 0)
                        sdcchExsit = 0;
                if (sdcchExsit == 1)
                    _countAbnormal1++;
                else
                    _countAbnormal1 = 0;
                if (_countAbnormal1 > 2)
                {
                    CommandToDSP_SetBCCH_ARFCN(_bcchC0);
                    _countAbnormal1 = 0;
                    //_countAbnormal2++;
                }

                break;
        }
    }

    private void ProcessCellSearchData(byte[] data)
    {
        var arfcn = (ushort)((data[1] & 0x7f) * 256 + data[2]);
        short power = data[3];
        if (arfcn == 0) Trace.WriteLine("串口工作正常");
        if (power != 0)
        {
            var adcFrameCount = (data[7] << 16) + (data[8] << 8) + (data[9] << 0);
            var bsic = data[10];
            var index = _historyList.FindIndex(p => adcFrameCount > 0 &&
                                                    Math.Abs(adcFrameCount - p.ADCFrameCount) <= 1 &&
                                                    bsic == p.BSIC && arfcn == p.C0);
            var newflag = index < 0;
            power -= RssiOffset;
            //Trace.WriteLine($"C0:{arfcn}:  rxLevel:{power}dBm ({(newflag ? "新小区" : "已解码")})");
            if (!newflag) //已解码
            {
                var info = _historyList[index];
                info.RSSI = power;
                info.ADCFrameCount = adcFrameCount;
                SendData(new GsmData { Data = info });
            }
            else //新小区
            {
                //解码
                lock (_decodeLocker)
                {
                    _needDecodeList.Add(new BcchDataStruct
                    {
                        C0 = arfcn,
                        RSSI = power,
                        ADCFrameCount = adcFrameCount,
                        BSIC = bsic
                    });
                }
            }
        }

        if (arfcn < 1023 || !_isCellSearch) return;
        _isCellSearch = false;
        _ = Task.Factory.StartNew(DecodeCell, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
    }

    private void BcchSdcchProcess(byte[] data)
    {
        var tsBits114 = new short[114];
        short sdcch8Blk = 0;
        var stealBits2 = new byte[2];
        byte sdcchStopFlag = 0;
        byte mType = 0;
        byte sdcch4SlotReady = 0;
        byte errorFlag = 0;
        byte rfIndex = 0;
        byte lapdmCompleteFlag = 0;
        byte crcFlag = 0;
        byte linktype = 0;
        var lastBsic = 0;
        Array.Clear(_byte23, 0, _byte23.Length);
        DllInvoker.Byte18Decoder(data, tsBits114, stealBits2, ref errorFlag, ref rfIndex, ref _tnSlot, ref _fnSlot);
        _frameNum = _fn / 51 * 51 + _fnSlot;
        if (rfIndex == 0)
            linktype = (byte)NetworkType.Downlink;
        else if (rfIndex == 1) linktype = (byte)NetworkType.Uplink;
        var sdcchFlag = stealBits2[0]; //指示是SDCCH还是CCCH信道时隙数据
        //////////////////////BCCH 接收//////////////////////////////////
        if (linktype == (byte)NetworkType.Downlink && sdcchFlag == 0)
        {
            /////////////////////// SCH //////////////////////////////
            var modf = _fnSlot % 51 % 10; // 
            if (modf == 1 && _tnSlot == 0)
            {
                var fn2 = 0;
                byte tsc = 0;
                byte bsic = 0;
                byte errorBits = 0;
                if (DllInvoker.SCH_Decoder(tsBits114, ref fn2, ref tsc, ref bsic, ref errorBits) > 0)
                {
                    _fn = fn2;
                    _timeoutCount.RxSchPacket++;
                    if (lastBsic != bsic || Math.Abs(_fn) > 500)
                        _count.SbCount = 0;
                    _count.SbCount++;
                    _bcchData.TSC = tsc;
                    _bcchData.BSIC = bsic;
                    KillOverTimeSdcch();
                }
            }

            if (modf is >= 2 and <= 9) // CCCH
            {
                modf = (modf - 2) % 4;
                _ccchDataBuffer.FromArray(tsBits114, rfIndex, (_tnSlot / 2 * 4 + modf) * 114);
                if (modf == 3)
                {
                    crcFlag = 0;
                    var lapdmBit184 = new byte[184];
                    var errorBits = DllInvoker.Decoder_456(ref _ccchDataBuffer[rfIndex, _tnSlot / 2 * 4 * 114],
                        lapdmBit184, (short)LogicalChannel.Bcch, ref crcFlag);
                    if (crcFlag > 0)
                    {
                        _count.RxBits += 456;
                        _count.RxErrorBits += errorBits;
                        _phyicalChannel = new ChannelStruct
                            { MobileIdentity = new byte[20], Phone_number = new byte[20] };
                        _bcchData.C0 = _bcchC0;
                        DllInvoker.Lapdm_ByteReverse(lapdmBit184, _byte23);
                        var type = DllInvoker.BCCH_Info(lapdmBit184, _byte23, ref _bcchData, ref _phyicalChannel,
                            ref _phyicalChannel2);
                        if (type is RrPagReq1 or RrPagReq2 or RrPagReq3)
                            _bcchData.paging_count += DllInvoker.Page_Request(_byte23);
                        //if (_bcchData.paging_count > 10 && _cellSearchFlag == 0) //如果有寻呼，并且勾选了该标志，则认为是正常基站
                        //    _normalBsFlag = true;
                        if (type is RrImmAss or RrImmAssExt)
                        {
                            if (_phyicalChannel.RFNum > 0 && _smsShow == 1)
                            {
                                byte flagTmp = 0;
                                for (var i = 0; i < _phyicalChannel.RFNum; i++)
                                    if (_phyicalChannel.RFList[i] == _bcchC0)
                                        flagTmp = 1;
                                if (flagTmp == 0)
                                {
                                }
                                //if (countRFnum > 1) _normalBsFlag = true;
                            }

                            if (_phyicalChannel.RFNum > 0)
                            {
                                _tAcount++;
                                _tAsum += _phyicalChannel.TA;
                            }

                            ImmediatAss_Process();
                            if (_cellSearchFlag > 0)
                            {
                                _phyicalChannel = new ChannelStruct
                                    { MobileIdentity = new byte[20], Phone_number = new byte[20] };
                                _phyicalChannel2 = new ChannelStruct
                                    { MobileIdentity = new byte[20], Phone_number = new byte[20] };
                            }
                        }

                        //if (_normalBsFlag = true || type == RrSysinfo3) _newStateFlag = true;
                        if (_bcchData.SIB3 == 1 && _cellSearchFlag == 0)
                        {
                            var lastBsCcChans = _bsCcchans;
                            if (_bcchData.CCCH_CONF == 6)
                                _bsCcchans = 4;
                            else if (_bcchData.CCCH_CONF == 4)
                                _bsCcchans = 3;
                            else if (_bcchData.CCCH_CONF == 2)
                                _bsCcchans = 2;
                            else
                                _bsCcchans = 1;
                            if (lastBsCcChans != _bsCcchans) CommandToDSP_SetBCCH_ARFCN(_bcchC0);
                        }

                        if ((_phyicalChannel.AssChannelType == (short)ChannelType.ChSdcch4 ||
                             _phyicalChannel2.ChannelType == (short)ChannelType.ChSdcch4) &&
                            _phyicalChannel.RFNum > 0)
                        {
                            _count.Sdcch4++;
                            if (_phyicalChannel.BlkIndex is >= 0 and <= 3)
                            {
                                _sDcch4ImmAssFn[_phyicalChannel.BlkIndex] = _frameNum;
                                _taskSdcch4[_phyicalChannel.BlkIndex] = new ChannelStruct
                                    { MobileIdentity = new byte[20], Phone_number = new byte[20] };
                            }
                        }

                        if (_phyicalChannel is { AssChannelType: (short)ChannelType.ChSdcch8, RFNum: > 0 })
                        {
                            //byte IA_permit_flag=Check_IA_Permit(PhyicalChannel);
                            _phyicalChannel.C0 = (short)_bcchC0;
                            _phyicalChannel.ImmAssFN = _frameNum;
                            _phyicalChannel.IAslot = _tnSlot;
                            if (_targetIAslot == -1 || (_targetIAslot != -1 && _tnSlot == _targetIAslot))
                                //若被叫的寻呼类型异常则抛弃
                                if (!(_iAPageType != 0 && _phyicalChannel.PageType != _iAPageType &&
                                      _phyicalChannel.ImmAssType == (byte)ImmAssignType.IaAnswerPaging))
                                    CommandToDSP_Start_SDCCH(_phyicalChannel);
                            _phyicalChannel = new ChannelStruct
                                { MobileIdentity = new byte[20], Phone_number = new byte[20] };
                        }

                        if (_phyicalChannel2 is { AssChannelType: (byte)ChannelType.ChSdcch8, RFNum: > 0 })
                        {
                            _phyicalChannel2.C0 = (short)_bcchC0;
                            _phyicalChannel2.ImmAssFN = _frameNum;
                            _phyicalChannel2.IAslot = _tnSlot;
                            if (_targetIAslot == -1 || (_targetIAslot != -1 && _tnSlot == _targetIAslot))
                                //若被叫的寻呼类型异常则抛弃
                                if (!(_iAPageType != 0 && _phyicalChannel2.PageType != _iAPageType &&
                                      _phyicalChannel2.ImmAssType == (byte)ImmAssignType.IaAnswerPaging))
                                    CommandToDSP_Start_SDCCH(_phyicalChannel2);
                            _phyicalChannel2 = new ChannelStruct
                                { MobileIdentity = new byte[20], Phone_number = new byte[20] };
                        }

                        var modf51 = _fnSlot % 51;
                        var sdcch4Idx = 4;
                        if (modf51 is >= 22 and <= 25) sdcch4Idx = 0;
                        if (modf51 is >= 26 and <= 29) sdcch4Idx = 1;
                        if (modf51 is >= 32 and <= 35) sdcch4Idx = 2;
                        if (modf51 is >= 36 and <= 39) sdcch4Idx = 3;
                        if (sdcch4Idx <= 3)
                        {
                            if (_byte23[3] == 0x05 && _byte23[4] == 0x08)
                            {
                                _taskSdcch4[sdcch4Idx].location_update_request = 1;
                                _taskSdcch4[sdcch4Idx].location_update_accept = 0;
                                _taskSdcch4[sdcch4Idx].location_update_reject = 0;
                                _taskSdcch4[sdcch4Idx].imsi_request = 0;
                            }

                            if (_byte23[3] == 0x05 && _byte23[4] == 0x04) //位置更新拒绝
                            {
                                if (_taskSdcch4[sdcch4Idx].location_update_reject == 0)
                                    _count.LocationUpdateReject++;
                                _taskSdcch4[sdcch4Idx].location_update_reject = 1;
                            }

                            if (_byte23[3] == 0x05 && _byte23[4] == 0x02) //位置更新接受
                            {
                                if (_taskSdcch4[sdcch4Idx].location_update_accept == 0)
                                    _count.LocationUpdateAccept++;
                                _taskSdcch4[sdcch4Idx].location_update_accept = 1;
                            }

                            if (_byte23[3] == 0x05 && _byte23[4] == 0x18 && _byte23[5] == 0x01)
                            {
                                if (_taskSdcch4[sdcch4Idx].imsi_request == 0) _count.ImsiRequest++;
                                _taskSdcch4[sdcch4Idx].imsi_request = 1;
                            }

                            if (!(_l3Message[8 + sdcch4Idx].sms_flag > 0 && (_byte23[1] & 0xf0) > 0))
                            {
                                _l3Message[8 + sdcch4Idx].dataBuff ??= new byte[512];
                                lapdmCompleteFlag = DllInvoker.Byte23ToFormatBLAPDm(_byte23,
                                    _l3Message[8 + sdcch4Idx].dataBuff, ref _l3Message[8 + sdcch4Idx].dataIdx);
                            }

                            _messageHeaderIdx[sdcch4Idx + 8]++;
                            _messageHeaderIdx[sdcch4Idx + 8] &= 31;
                            _messageHeader[sdcch4Idx + 8, 2 * _messageHeaderIdx[sdcch4Idx + 8]] = _byte23[3];
                            _messageHeader[sdcch4Idx + 8, 2 * _messageHeaderIdx[sdcch4Idx + 8] + 1] = _byte23[4];
                            if (lapdmCompleteFlag > 0 && _l3Message[sdcch4Idx + 8].sms_flag > 0)
                                ///////////////////////下行短信数据接收完整//////////////////////////////////
                                //_smsSdcchType = (byte)AssChannelType.B_SDCCH4_D;
                                SMS_Process(_l3Message[sdcch4Idx + 8].dataBuff, _l3Message[sdcch4Idx + 8].dataIdx,
                                    sdcch4Idx + 8);

                            if (_byte23[0] == 0x0f && (_byte23[1] & 0x0f) == 0x00 && _byte23[2] == 0x53)
                            {
                                _l3Message[sdcch4Idx + 8].sms_flag = 1;
                                Array.Clear(_l3Message[sdcch4Idx + 8].dataBuff, 0, MaxLadpdmLen);
                                _l3Message[sdcch4Idx + 8].dataIdx = 20;
                                Array.Copy(_byte23, 3, _l3Message[sdcch4Idx + 8].dataBuff, 0, 20);
                            }
                        }
                    }
                }
            }
        }

        //////////////////////SDCCH 接收//////////////////////////////////
        crcFlag = 0;
        if (sdcchFlag == 1)
        {
            short modf;
            if (linktype == (byte)NetworkType.Downlink)
            {
                sdcch8Blk = DllInvoker.Return_SDCCH8_BlkIdx(_fnSlot, (byte)NetworkType.Downlink);
                modf = (short)(_fnSlot % 51 % 4); // 
            }
            else
            {
                sdcch8Blk = DllInvoker.Return_SDCCH8_BlkIdx(_fnSlot, (byte)NetworkType.Uplink);
                modf = (short)((_fnSlot - 15) % 51 % 4);
            }

            if (sdcch8Blk < 0) return;
            if (modf == 3) sdcch4SlotReady = 1;
            // FNslot = (FNslot+FN_offset)%51; //用于做时间限制
            _taskSdcch8[sdcch8Blk].last_RX_Data_FN = _fn;
            _rxBuffer.FromArray(tsBits114, rfIndex, modf * 114);
            ////// 统计时隙连续错误数
            if (errorFlag > 0 && linktype == (byte)NetworkType.Downlink)
                _taskSdcch8[sdcch8Blk].downlinkTimeSlotSuccesiveError++;
            if (errorFlag == 0 && linktype == (byte)NetworkType.Downlink)
                _taskSdcch8[sdcch8Blk].downlinkTimeSlotSuccesiveError = 0;
            if (sdcch4SlotReady > 0)
            {
                var lapdmBit184 = new byte[184];
                DllInvoker.Decoder_456(ref _ccchDataBuffer[rfIndex, _tnSlot / 2 * 4 * 114], lapdmBit184,
                    (short)LogicalChannel.Bcch, ref crcFlag);
                if (crcFlag == 1) // CRC 校验通过
                {
                    _taskSdcch8[sdcch8Blk].sdcch8succesiveError = 0;
                    DllInvoker.Lapdm_ByteReverse(lapdmBit184, _byte23);
                    //  下行SDCCH 的接收 		 
                    if (linktype == (byte)NetworkType.Downlink)
                    {
                        _taskSdcch8[sdcch8Blk].downlinkSDCCHPacket++;
                        if (!(_l3Message[sdcch8Blk].sms_flag > 0 && (_byte23[1] & 0xf0) > 0))
                        {
                            _l3Message[sdcch8Blk].dataBuff ??= new byte[512];
                            lapdmCompleteFlag = DllInvoker.Byte23ToFormatBLAPDm(_byte23,
                                _l3Message[sdcch8Blk].dataBuff, ref _l3Message[sdcch8Blk].dataIdx);
                        }

                        _messageHeaderIdx[sdcch8Blk]++;
                        _messageHeaderIdx[sdcch8Blk] &= 31;
                        _messageHeader[sdcch8Blk, 2 * _messageHeaderIdx[sdcch8Blk]] = _byte23[3];
                        _messageHeader[sdcch8Blk, 2 * _messageHeaderIdx[sdcch8Blk] + 1] = _byte23[4];
                        if (lapdmCompleteFlag > 0)
                        {
                            _l3Message[sdcch8Blk].dataBuff ??= new byte[512];
                            mType = DllInvoker.L3MessageType(_l3Message[sdcch8Blk].dataBuff);
                        }

                        if (lapdmCompleteFlag > 0 && _l3Message[sdcch8Blk].sms_flag > 0)
                        {
                            ///////////////////////下行短信数据接收完整//////////////////////////////////
                            //_smsSdcchType = (byte)AssChannelType.B_SDCCH8_D;
                            SMS_Process(_l3Message[sdcch8Blk].dataBuff, _l3Message[sdcch8Blk].dataIdx, sdcch8Blk);
                            sdcchStopFlag = 1;
                        }

                        // indicate a SMS message
                        if (_byte23[0] == 0x0f && (_byte23[1] & 0x0f) == 0x00 && _byte23[2] == 0x53)
                        {
                            _l3Message[sdcch8Blk].sms_flag = 1;
                            Array.Clear(_l3Message[sdcch8Blk].dataBuff, 0, MaxLadpdmLen);
                            _l3Message[sdcch8Blk].dataIdx = 20;
                            Array.Copy(_byte23, 3, _l3Message[sdcch8Blk].dataBuff, 0, 20);
                        }

                        if (mType is (byte)MType.MAssignmentCommand or (byte)MType.MSetup
                            or (byte)MType.MCallProce)
                            sdcchStopFlag = 1;
                        //_normalBsFlag = true;
                    }
                }

                ////// SDCCH 译码错误
                if (crcFlag == 0)
                {
                    /*    连续2下行SDCCH错误终止该SDCCH  */
                    _taskSdcch8[sdcch8Blk].sdcch8succesiveError++;
                    if (_taskSdcch8[sdcch8Blk].sdcch8succesiveError >= 2) sdcchStopFlag = 1;
                }
            }
        }

        if (crcFlag > 0)
        {
            if (_byte23[3] == 0x05 && _byte23[4] == 0x08)
            {
                _taskSdcch8[sdcch8Blk].location_update_request = 1;
                _taskSdcch8[sdcch8Blk].location_update_accept = 0;
                _taskSdcch8[sdcch8Blk].location_update_reject = 0;
                _taskSdcch8[sdcch8Blk].imsi_request = 0;
            }

            if (_byte23[3] == 0x05 && _byte23[4] == 0x04) //位置更新拒绝
            {
                if (_taskSdcch8[sdcch8Blk].location_update_reject == 0) _count.LocationUpdateReject++;
                _taskSdcch8[sdcch8Blk].location_update_reject = 1;
            }

            if (_byte23[3] == 0x05 && _byte23[4] == 0x02) //位置更新接受
            {
                if (_taskSdcch8[sdcch8Blk].location_update_accept == 0) _count.LocationUpdateAccept++;
                _taskSdcch8[sdcch8Blk].location_update_accept = 1;
            }

            if (_byte23[3] == 0x05 && _byte23[4] == 0x18 && _byte23[5] == 0x01)
            {
                if (_taskSdcch8[sdcch8Blk].imsi_request == 0) _count.ImsiRequest++;
                _taskSdcch8[sdcch8Blk].imsi_request = 1;
            }

            // CM service request contains the TMSI  9.2.9 //主叫
            if (_byte23[0] == 0x01 && _byte23[20] == 0x2b && _byte23[3] == 0x05 && _byte23[4] == 0x24)
            {
                _taskSdcch8[sdcch8Blk].tmsi_get = 1;
                var bcdLen = _taskSdcch8[sdcch8Blk].MobileIdentity[0];
                DllInvoker.Mobile_Identity(ref _byte23[10], ref _taskSdcch8[sdcch8Blk].MobileIdentity[1], ref bcdLen,
                    ref _taskSdcch8[sdcch8Blk].MobileIdentityType);
                _taskSdcch8[sdcch8Blk].MobileIdentity[0] = bcdLen;
                _taskSdcch8[sdcch8Blk].serviceType = (byte)ServiceType.ServiceOthers;
                _taskSdcch8[sdcch8Blk].CMServiceInd = 1;
                if ((_byte23[5] & 0x0f) == 0x01)
                {
                    _taskSdcch8[sdcch8Blk].connectType = (byte)ConnectType.SpeechOriginate; // Mobile originating call
                    _taskSdcch8[sdcch8Blk].serviceType = (byte)ServiceType.ServiceSpeech;
                }

                if ((_byte23[5] & 0x0f) == 0x04)
                {
                    _taskSdcch8[sdcch8Blk].connectType = (byte)ConnectType.SmsOriginate; // Mobile originating SMS
                    _taskSdcch8[sdcch8Blk].serviceType = (byte)ServiceType.ServiceSms;
                }
            }

            if (_byte23[0] == 0x0d) // uplink SMS, SAPI=3;
            {
                _taskSdcch8[sdcch8Blk].connectType = (byte)ConnectType.SmsOriginate; // Mobile originating SMS
                _taskSdcch8[sdcch8Blk].serviceType = (byte)ServiceType.ServiceSms;
            }

            if (_byte23[0] == 0x0f) // downlink SMS, SAPI=3;
            {
                _taskSdcch8[sdcch8Blk].connectType = (byte)ConnectType.SmsTerminate; // 
                _taskSdcch8[sdcch8Blk].serviceType = (byte)ServiceType.ServiceSms;
            }

            if (_byte23[0] == 0x01 && _byte23[3] == 0x06 && _byte23[4] == 0x27)
            {
                _taskSdcch8[sdcch8Blk].tmsi_get = 1;
                var bcdLen = _taskSdcch8[sdcch8Blk].MobileIdentity[0];
                DllInvoker.Mobile_Identity(ref _byte23[10], ref _taskSdcch8[sdcch8Blk].MobileIdentity[1], ref bcdLen,
                    ref _taskSdcch8[sdcch8Blk].MobileIdentityType);
                _taskSdcch8[sdcch8Blk].MobileIdentity[0] = bcdLen;
                _taskSdcch8[sdcch8Blk].PageResponseInd = 1;
                _taskSdcch8[sdcch8Blk].pageResponseByte = _byte23[5];
            }

            if (lapdmCompleteFlag == 1 || _l3Message[sdcch8Blk].dataIdx >= 200) _l3Message[sdcch8Blk].dataIdx = 0;
            if (_speech == 0 && _taskSdcch8[sdcch8Blk].serviceType == (byte)ServiceType.ServiceSpeech)
                sdcchStopFlag = 1;
            if (_taskSdcch8[sdcch8Blk].serviceType == (byte)ServiceType.ServiceOthers) sdcchStopFlag = 1;
            if (_byte23[0] == 0x03 && _byte23[3] == 0x06 && _byte23[4] == 0x2e) sdcchStopFlag = 1;
            //// channel release,则退出该SDCCH
            if (_byte23[0] == 0x03 && _byte23[3] == 0x06 && _byte23[4] == 0x0d) sdcchStopFlag = 1;
        }

        if (sdcchStopFlag == 1)
        {
            _taskSdcch8[sdcch8Blk] = default;
            _messageHeader.SetValue(null, sdcch8Blk);
            _messageHeaderIdx[sdcch8Blk] = 0;
            CommandToDSP_Stop_One_SDCCH(sdcch8Blk);
            _l3Message[sdcch8Blk] = default;
        }
    }

    private void ImmediatAss_Process()
    {
        if (_phyicalChannel.RFNum > 0)
        {
            GetImmAssignType(_byte23[7]);
            if ((_speech > 0 && _originatingCall > 0 && _channelRequestType.OriginateCall > 0) ||
                (_speech > 0 && _pageCall > 0 && _channelRequestType.PagingCall > 0) ||
                (_sms > 0 && _originatingCall > 0 && _channelRequestType.OriginateSms > 0) ||
                (_sms > 0 && _pageCall > 0 && _channelRequestType.PagingSms > 0) ||
                (_locationUpdate > 0 && _channelRequestType.LocationUpdate > 0)
               )
            {
                _phyicalChannel.ImmAssType = 0; //LAPDm23[7];
                _phyicalChannel.PageType = DllInvoker.ImmAssignPageType(_byte23[7]);
            }
            else
            {
                _phyicalChannel = new ChannelStruct { MobileIdentity = new byte[20], Phone_number = new byte[20] };
            }
        }

        if (_phyicalChannel2.RFNum <= 0) return;
        _count.ImmAssignExt++;
        GetImmAssignType(_byte23[14]);
        if ((_speech > 0 && _originatingCall > 0 && _channelRequestType.OriginateCall > 0) ||
            (_speech > 0 && _pageCall > 0 && _channelRequestType.PagingCall > 0) ||
            (_sms > 0 && _originatingCall > 0 && _channelRequestType.OriginateSms > 0) ||
            (_sms > 0 && _pageCall > 0 && _channelRequestType.PagingSms > 0) ||
            (_locationUpdate > 0 && _channelRequestType.LocationUpdate > 0)
           )
        {
            _phyicalChannel2.ImmAssType = 0;
            _phyicalChannel2.PageType = DllInvoker.ImmAssignPageType(_byte23[14]);
        }
        else
        {
            _phyicalChannel2 = new ChannelStruct { MobileIdentity = new byte[20], Phone_number = new byte[20] };
        }
    }

    private byte GetImmAssignType(byte bt)
    {
        _channelRequestType = default;
        _count.ImmAssign++;
        if ((bt & 0xe0) == 0x00)
        {
            _channelRequestType.LocationUpdate = 1;
            _count.LocationUpdateRequest++;
        }
        else
        {
            _channelRequestType.PagingCall = 1;
            _channelRequestType.PagingSms = 1;
        }

        return 0;
    }

    private void SMS_Process(byte[] dataBuff, ushort dataLen, int sdcchBlk)
    {
        var chineseSms = new ushort[129];
        var phoneId = new byte[22];
        int smslen = DllInvoker.SMS_Decode_FaksBS(dataBuff, (short)dataLen, _messageHeader.ToArray(sdcchBlk, 0),
            phoneId, chineseSms);

        if (chineseSms[0] > 0)
        {
            var list = new List<byte>();
            foreach (var sms in chineseSms)
            {
                var chs = BitConverter.GetBytes(sms);
                list.AddRange(chs);
            }

            Encoding.UTF8.GetString(list.ToArray());
        }

        if (smslen <= 0) return;
        _count.Sms++;
    }

    private void KillOverTimeSdcch()
    {
        for (var i = 0; i < 8; i++)
            if (_taskSdcch8[i].RFNum > 0)
                if (Math.Abs(_fn - _taskSdcch8[i].last_RX_Data_FN) > 500)
                    _taskSdcch8[i] = default;
    }
}