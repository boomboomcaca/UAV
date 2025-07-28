using System;
using System.Linq;
using System.Text;
using System.Threading;
using Magneto.Device.XE_VUHF_28.Protocols;
using Magneto.Device.XE_VUHF_28.Protocols.Data;

namespace Magneto.Device.XE_VUHF_28.Commands;

internal class MScanCommand : CommandBase
{
    protected override void InitCommand(DeviceParams device)
    {
        var antenna = Encoding.ASCII.GetBytes(device.CurrAntenna);
        byte agcType = 2;
        int rfAtt = 0, ifAtt = 0;
        if (device.Attenuation != -1)
        {
            PartAttenuation(device.Attenuation, ref rfAtt, ref ifAtt);
            agcType = 1;
        }

        var templates = Array.ConvertAll(device.MScanPoints, p => (MScanTemplate)p).ToArray();
        var startFrequency = templates.Min(p => p.Frequency);
        var stopFrequency = templates.Max(p => p.Frequency);
        if (!device.SharedFolderValid) //设备共享文件夹不可用时，先执行一次单频测量
        {
            ProgramingInfo = new ReceiverProgramingInfo();
            ProgramingInfo.NumOfChannels.Value = 2;
            ProgramingInfo.Channels = new ChannelProgramming[2];
            for (var i = 0; i < 2; ++i)
            {
                ProgramingInfo.Channels[i] = new ChannelProgramming();
                var chan = ProgramingInfo.Channels[i];
                chan.ChannelNo.Value = (byte)(i + 1);
                chan.FMin.Value = 0;
                chan.FMax.Value = 0;
                if (device.Attenuation == -1)
                {
                    //AGC
                    chan.AgcType.Value = 2;
                    chan.RfAttenuator.Value = 0;
                    chan.IfAttenuator.Value = 0;
                }
                else
                {
                    chan.AgcType.Value = 1;
                    chan.RfAttenuator.Value = (byte)rfAtt;
                    chan.IfAttenuator.Value = (byte)ifAtt;
                }

                chan.AmpliConfig.Value = (byte)(device.AmpliConfig ? AmpliValue : 1);
                Array.Copy(antenna, chan.Antenna.Value, antenna.Length);
                chan.FmFilter.Value = 2; //TODO:单频测量暂未暴露此参数
                chan.LevelUnits.Value = 1;
            }

            SendCmd(ProgramingInfo.GetBytes());
            //宽带参数设置
            BbRequest = new BbInterceptionRequest();
            BbRequest.ChannelNo.Value = 1;
            BbRequest.UdpFftwbPort.Value = 0; //(ushort)device._udpBBFFTPort;//TODO:暂时不返回宽带频谱
            BbRequest.DetectionMode.Value = device.DetectionMode;
            BbRequest.IntTime.Value = (ushort)device.XeIntTime;
            BbRequest.Resolution.Value =
                DefaultResolution; //device.Resolution * 1000;//TODO:影响宽带FFT,但单频测量中不需要宽带FFT结果所以固定为25k
            BbRequest.Sensitivity.Value = 1; //0：灵敏，1：快速
            BbRequest.RelativeThreshold.Value = 0;
            BbRequest.ThresholdMinValue.Value = 20;
            BbRequest.ThresholdMaxValue.Value = 20;
            BbRequest.Turbo.Value = 0;
            BbRequest.PhaseNo.Value = 0; //TODO:后续完善
            BbRequest.MeasurementsRequested.Value = 0; //TODO:单频测量暂不需要宽带数据
            BbRequest.Band.FMin.Value = (uint)(startFrequency * 1000000);
            BbRequest.Band.FMax.Value = (uint)(stopFrequency * 1000000);
            SendCmd(BbRequest.GetBytes());
            //窄带参数设置
            var nbRequest = new IqFluxRequest();
            nbRequest.ChannelNo.Value = 2;
            nbRequest.HomingIdChannel.Value = 0;
            nbRequest.NumOfTracks.Value = 1;
            nbRequest.Tracks = new IqFluxTrack[1];
            nbRequest.Tracks[0] = new IqFluxTrack();
            var track = nbRequest.Tracks[0];
            track.ChannelNo.Value = 1;
            track.Modification.Value = 1;
            track.Activation.Value = 1;
            track.IqPort.Value = 0;
            track.Type.Value = 1;
            track.CentreFrequency.Value = (uint)((startFrequency + stopFrequency) / 2 * 1000000);
            track.Width.Value = 1000 * 1000;
            track.Mode.Value = 0;
            track.Fft.Value = 1;
            track.FftPort.Value = (ushort)device.UdpNbfftPort;
            track.FftIntegration.Value = 1; //Number of FFTs to be included(1 by default)
            SendCmd(nbRequest.GetBytes());
            //音频请求
            var audioRequest = new AudioDemodulationRequest();
            audioRequest.ChannelNo.Value = 2;
            audioRequest.ChannelId.Value = 1;
            audioRequest.ModulationType.Value = XeAssister.GetDemoduMode(device.DemMode);
            audioRequest.Bfo.Value = 0;
            audioRequest.Frequency.Value = 44100;
            audioRequest.UdpPort.Value = (ushort)device.UdpAudioPort;
            audioRequest.Action.Value = (sbyte)(device.AudioSwitch ? 1 : 0);
            SendCmd(audioRequest.GetBytes(), 100);
            var squelchRequest = new SquelchActivationRequest();
            squelchRequest.ChannelNo.Value = 2;
            squelchRequest.ChannelId.Value = 1;
            squelchRequest.Threshold.Value = (short)(device.SquelchThreshold - 107);
            squelchRequest.Activation.Value = (sbyte)(device.SquelchSwitch ? 1 : 0);
            SendCmd(squelchRequest.GetBytes());
            Thread.Sleep(1000);
            StopProc(-1, 0);
            var testRequest = new TestRequest();
            testRequest.ForceTest.Value = 1;
            SendCmd(testRequest.GetBytes());
            Thread.Sleep(1000);
            PhaseNo++;
            var delManualChannelsRequest = new ManualChannelsDeletionRequest();
            delManualChannelsRequest.NumOfChannels.Value = 0;
            SendCmd(delManualChannelsRequest.GetBytes());
            SetAntenna(Device.CurrAntenna, Device.AmpliConfig);
        }

        //设置ITU测量参数
        var ituRequest = new ItuRequest();
        ituRequest.ChannelNo.Value = 255; //抓包发现离散扫描设置的为255
        ituRequest.ChannelId.Value = 0; //TODO:
        ituRequest.UdpPort.Value = (ushort)device.UdpBbituPort; //49700;
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
        ituRequest.AcquisitionTime.Value = 0.5;
        SendCmd(ituRequest.GetBytes());
        var mscanRequest = new MemoryScanningEmissions();
        mscanRequest.NumOfEmission.Value = (uint)device.MScanPoints.Length;
        mscanRequest.Emissions = new MScanEmission[device.MScanPoints.Length];
        //设置离散扫描参数
        for (var i = 0; i < templates.Length; ++i)
        {
            var template = templates[i];
            var emission = mscanRequest.Emissions[i] = new MScanEmission();
            emission.Identifier.Value = (uint)i;
            emission.EmissionType.Value = 0;
            emission.CentreFrequency.Value = (ulong)(template.Frequency * 1000000);
            emission.Bandwidth.Value = (uint)(template.FilterBandwidth * 1000);
            emission.ThresholdType.Value = 0;
            emission.Threshold.Value = (short)(template.MeasureThreshold - 107);
            emission.Resolution.Value = 25000;
            emission.Bfo.Value = 0;
            emission.TypeOfModulation.Value = XeAssister.GetDemoduMode(template.DemMode);
            emission.AgcType.Value = agcType;
            emission.RfAttenuator.Value = (byte)rfAtt;
            emission.IfAttenuator.Value = (byte)ifAtt;
            emission.AmpliConfig.Value = 1;
            emission.FmFilter.Value = 2;
            Array.Copy(antenna, emission.Antenna.Value, antenna.Length);
            emission.Sensitivity.Value = 1;
            emission.Listening.Value = 1;
            emission.IqRecord.Value = 0;
            emission.WaveRecord.Value = 0;
            emission.IqFlux.Value = 0;
            emission.Itu.Value = 1;
            emission.TotalDuration.Value = (long)((device.HoldTime + device.DwellTime) * 1000);
            emission.ReloadingDuration.Value = 0;
            emission.PreservationDuration.Value = (long)(device.DwellTime * 1000);
        }

        SendCmd(mscanRequest.GetBytes());
        //开始扫描
        var mscanControl = new MemoryScanningControl();
        mscanControl.Action.Value = 1;
        SendCmd(mscanControl.GetBytes());
    }

    public override void Stop()
    {
        var mscanControl = new MemoryScanningControl();
        mscanControl.Action.Value = 0;
        SendCmd(mscanControl.GetBytes());
        base.Stop();
    }
}