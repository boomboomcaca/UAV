using System;
using System.Text;
using Magneto.Device.XE_VUHF.Protocols.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.XE_VUHF.Commands;

internal class MScanCommand : CommandBase
{
    protected override void InitCommand(DeviceParams device)
    {
        //设置ITU测量参数
        ItuRequest ituRequest = new();
        ituRequest.ChannelNo.Value = 255; //抓包发现离散扫描设置的为255
        ituRequest.ChannelId.Value = 0; //TODO:
        ituRequest.UdpPort.Value = 49700; //(ushort)device._udpNBITUPort;//49700;
        ituRequest.AntennaChargeTime.Value = 0.001;
        ituRequest.AntennaDischargeTime.Value = 0.6;
        ituRequest.FieldChargeTime.Value = 0.001;
        ituRequest.FieldDischargeTime.Value = 0.6;
        ituRequest.XdB1Threshold.Value = 6;
        ituRequest.XdB2Threshold.Value = 26; //device.XdB;
        ituRequest.BetaBandThreshold.Value = 1; //device.Beta;
        ituRequest.GammaVqmam.Value = 1.424214; //TODO:抓包数据
        ituRequest.GammaVqmfm.Value = 1.424214;
        ituRequest.GammaVqmpm.Value = 1.424214;
        ituRequest.NumOfFftPoints.Value = 2048;
        ituRequest.FftWindow.Value = 5;
        ituRequest.Mode.Value = 0;
        ituRequest.NumOfLoops.Value = 2;
        ituRequest.NumOfIntegrations.Value = 1;
        ituRequest.TypeOfIntegration.Value = 0;
        ituRequest.AcquisitionTime.Value = 0.2; //0.2;//TODO:
        SendCmd(ituRequest.GetBytes());
        byte agcType = 2;
        int rfAtt = 0, ifAtt = 0;
        if (device.Attenuation != -1)
        {
            PartAttenuation(device.Attenuation, ref rfAtt, ref ifAtt);
            agcType = 1;
        }

        var antenna = Encoding.ASCII.GetBytes(device.CurrAntenna);
        //设置离散扫描参数
        MemoryScanningEmissions mscanRequest = new();
        mscanRequest.NumOfEmission.Value = (uint)device.Frequencys.Length;
        mscanRequest.Emissions = new MScanEmission[device.Frequencys.Length];
        for (var i = 0; i < device.Frequencys.Length; ++i)
        {
            var emission = mscanRequest.Emissions[i] = new MScanEmission();
            emission.Identifier.Value = (uint)i;
            emission.EmissionType.Value = 0;
            emission.CentreFrequency.Value = (ulong)((double)device.Frequencys[i][ParameterNames.Frequency] * 1000000);
            emission.Bandwidth.Value = (uint)((double)device.Frequencys[i][ParameterNames.IfBandwidth] * 1000);
            emission.ThresholdType.Value = 0;
            emission.Threshold.Value = -174; //(-23 + 6) - 107;//-174;
            emission.Resolution.Value = 25000;
            emission.Bfo.Value = 0;
            emission.TypeOfModulation.Value = 1;
            emission.AgcType.Value = agcType;
            emission.RfAttenuator.Value = (byte)rfAtt;
            emission.IfAttenuator.Value = (byte)ifAtt;
            emission.AmpliConfig.Value = (byte)(device.AmpliConfig ? AmpliValue : 1);
            emission.FmFilter.Value = (byte)(device.FmFilter ? 1 : 2);
            Array.Copy(antenna, emission.Antenna.Value, antenna.Length);
            emission.Sensitivity.Value = 1; //(byte)(device.Sensitivity ? 0 : 1);
            emission.Listening.Value = 0;
            emission.IqRecord.Value = 0;
            emission.WaveRecord.Value = 0;
            emission.IqFlux.Value = 0;
            emission.Itu.Value = 1;
            emission.TotalDuration.Value = 500; //5000;//8000;
            emission.ReloadingDuration.Value = 0; //10000;
            emission.PreservationDuration.Value = 200; //1000;
        }

        SendCmd(mscanRequest.GetBytes(Version));
        //删除所有通道
        ManualChannelsDeletionRequest delManualChannelsRequest = new();
        delManualChannelsRequest.NumOfChannels.Value = 0;
        SendCmd(delManualChannelsRequest.GetBytes());
        //开始扫描
        MemoryScanningControl mscanControl = new();
        mscanControl.Action.Value = 1;
        SendCmd(mscanControl.GetBytes());
    }

    public override void Stop()
    {
        MemoryScanningControl mscanControl = new();
        mscanControl.Action.Value = 0;
        SendCmd(mscanControl.GetBytes());
        base.Stop();
    }
}