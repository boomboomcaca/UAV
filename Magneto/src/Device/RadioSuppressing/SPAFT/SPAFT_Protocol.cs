using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Magneto.Protocol.Define;

namespace Magneto.Device.SPAFT;

public partial class Spaft
{
    #region 控制侦封装

    private byte[] ToStartFrame()
    {
        var feature = new byte[] { 0x51 };
        var contentLength = BitConverter.GetBytes((uint)1);
        var content = new byte[] { 0xff };
        var frame = new byte[feature.Length + contentLength.Length + content.Length];
        var offset = 0;
        Buffer.BlockCopy(feature, 0, frame, offset, feature.Length);
        offset += feature.Length;
        Buffer.BlockCopy(contentLength, 0, frame, offset, contentLength.Length);
        offset += contentLength.Length;
        Buffer.BlockCopy(content, 0, frame, offset, content.Length);
        return frame;
    }

    private byte[] ToStopFrame()
    {
        var feature = new byte[] { 0x53 };
        var contentLength = BitConverter.GetBytes((uint)1);
        var content = new byte[] { 0xff };
        var frame = new byte[feature.Length + contentLength.Length + content.Length];
        var offset = 0;
        Buffer.BlockCopy(feature, 0, frame, offset, feature.Length);
        offset += feature.Length;
        Buffer.BlockCopy(contentLength, 0, frame, offset, contentLength.Length);
        offset += contentLength.Length;
        Buffer.BlockCopy(content, 0, frame, offset, content.Length);
        return frame;
    }

    private static byte[] ToAudioFrame(byte[] audioArray)
    {
        if (audioArray == null || audioArray.Length == 0) return null;
        var featureArray = new byte[] { 0x63 };
        var contentLengthArray = BitConverter.GetBytes(audioArray.Length);
        var frame = new byte[featureArray.Length + contentLengthArray.Length + audioArray.Length];
        var offset = 0;
        Buffer.BlockCopy(featureArray, 0, frame, offset, featureArray.Length);
        offset += featureArray.Length;
        Buffer.BlockCopy(contentLengthArray, 0, frame, offset, contentLengthArray.Length);
        offset += contentLengthArray.Length;
        Buffer.BlockCopy(audioArray, 0, frame, offset, audioArray.Length);
        return frame;
    }

    private static byte[] ToQueryPowerValueFrame()
    {
        var feature = new byte[] { 0x55 };
        var contentLength = BitConverter.GetBytes((uint)1);
        var content = new byte[] { 0xff };
        var frame = new byte[feature.Length + contentLength.Length + content.Length];
        var offset = 0;
        Buffer.BlockCopy(feature, 0, frame, offset, feature.Length);
        offset += feature.Length;
        Buffer.BlockCopy(contentLength, 0, frame, offset, contentLength.Length);
        offset += contentLength.Length;
        Buffer.BlockCopy(content, 0, frame, offset, content.Length);
        return frame;
    }

    private static byte[] ToQueryPowerStatusFrame()
    {
        var feature = new byte[] { 0x58 };
        var contentLength = BitConverter.GetBytes((uint)1);
        var content = new byte[] { 0xff };
        var frame = new byte[feature.Length + contentLength.Length + content.Length];
        var offset = 0;
        Buffer.BlockCopy(feature, 0, frame, offset, feature.Length);
        offset += feature.Length;
        Buffer.BlockCopy(contentLength, 0, frame, offset, contentLength.Length);
        offset += contentLength.Length;
        Buffer.BlockCopy(content, 0, frame, offset, content.Length);
        return frame;
    }

    private Dictionary<int, List<byte[]>> ToLocalOscillatorFrame(RftxSegmentsTemplate[] frequencies)
    {
        if (_channelFrequencyEnds == null || !_channelFrequencyEnds.Any())
        {
            Trace.WriteLine("通道频率信息为空");
            return null;
        }

        var channelLocalOscillatorDict = new Dictionary<int, List<byte[]>>();
        foreach (var frequencyInfo in frequencies)
        {
            if (frequencyInfo.PhysicalChannelNumber < 3) continue;
            // 本振频率
            var localOscillator = _channelFrequencyEnds[frequencyInfo.PhysicalChannelNumber].Item2 > 5000000000
                ? _channelFrequencyEnds[frequencyInfo.PhysicalChannelNumber].Item4
                : _channelFrequencyEnds[frequencyInfo.PhysicalChannelNumber].Item3;
            localOscillator /= 4;
            var featureArray = new byte[] { 0x70 };
            var channelNumberArray = new[] { (byte)(frequencyInfo.PhysicalChannelNumber % 3 + 1) };
            var localOscillatorArray = BitConverter.GetBytes((int)localOscillator);
            var contentLengthArray =
                BitConverter.GetBytes((uint)(channelNumberArray.Length + localOscillatorArray.Length));
            var frame = new byte[featureArray.Length + contentLengthArray.Length + channelNumberArray.Length +
                                 localOscillatorArray.Length];
            var offset = 0;
            Buffer.BlockCopy(featureArray, 0, frame, offset, featureArray.Length);
            offset += featureArray.Length;
            Buffer.BlockCopy(contentLengthArray, 0, frame, offset, contentLengthArray.Length);
            offset += contentLengthArray.Length;
            Buffer.BlockCopy(channelNumberArray, 0, frame, offset, channelNumberArray.Length);
            offset += channelNumberArray.Length;
            Buffer.BlockCopy(localOscillatorArray, 0, frame, offset, localOscillatorArray.Length);
            channelLocalOscillatorDict[frequencyInfo.PhysicalChannelNumber] = new List<byte[]> { frame };
        }

        return channelLocalOscillatorDict;
    }

    private Dictionary<int, List<byte[]>> ToFrequencyListFrame(RftxSegmentsTemplate[] frequencies)
    {
        if (_channelFrequencyEnds == null || !_channelFrequencyEnds.Any())
        {
            Trace.WriteLine("通道频率信息为空");
            return null;
        }

        var channelFrequencyFrame = new Dictionary<int, List<byte[]>>();
        foreach (var frequencyInfo in frequencies)
        {
            var dataList = new List<byte[]>();
            var featureArray = new byte[] { 0x52 }; // 功能标识
            dataList.Add(featureArray);
            var logicalChannel = frequencyInfo.PhysicalChannelNumber < 3
                ? frequencyInfo.PhysicalChannelNumber * 10 + frequencyInfo.LogicalChannelNumber
                : (frequencyInfo.PhysicalChannelNumber % 3 + 1) * 8 + frequencyInfo.LogicalChannelNumber;
            var logicalChannelArray = new[] { (byte)logicalChannel }; // 逻辑载波编号
            dataList.Add(logicalChannelArray);
            var enabledArray = new[] { (byte)(frequencyInfo.RftxSwitch ? 1 : 0) }; // 使能标识
            dataList.Add(enabledArray);
            var frequencyModeArray = new[]
            {
                (byte)(frequencyInfo.RftxFrequencyMode == 0 ? 0 : frequencyInfo.RftxFrequencyMode == 1 ? 2 : 1)
            }; // 频率模式（干扰体制）
            dataList.Add(frequencyModeArray);
            var reserved1Array = new byte[] { 127 }; // 保留字段
            dataList.Add(reserved1Array);
            byte[] modulationModeArray = null; // 调制模式
            if (frequencyInfo.PhysicalChannelNumber < 3)
                modulationModeArray = new[] { (byte)frequencyInfo.ModulationIndex };
            else
                modulationModeArray = frequencyInfo.Modulation is Modulation._8PSK or Modulation._16QAM
                    ? new[] { (byte)(frequencyInfo.ModulationIndex - 1) }
                    : new[] { (byte)(frequencyInfo.ModulationIndex % 8) };

            dataList.Add(modulationModeArray);
            var fmModulationSourceArray = new[] { (byte)frequencyInfo.ModulationSource }; // 调制源
            dataList.Add(fmModulationSourceArray);
            var bandwidthArray = BitConverter.GetBytes((int)(frequencyInfo.Bandwidth * 1000)); // 带宽
            dataList.Add(bandwidthArray);
            var reserved2Array = BitConverter.GetBytes(0); // 保留字段
            dataList.Add(reserved2Array);
            var baudrateArray = BitConverter.GetBytes((int)(frequencyInfo.Baudrate * 1000)); // 波特率
            dataList.Add(baudrateArray);
            // 本振频率
            var localOscillator = frequencyInfo.PhysicalChannelNumber < 3
                ? 0
                : _channelFrequencyEnds[frequencyInfo.PhysicalChannelNumber].Item3;
            switch (frequencyInfo.RftxFrequencyMode)
            {
                case 0: // 定频
                    var frequencyArray =
                        BitConverter.GetBytes((int)(frequencyInfo.Frequency * 1000000L - localOscillator));
                    dataList.Add(frequencyArray);
                    break;
                case 1: // 跳频
                    var holdTimeArrayforFrequencies = BitConverter.GetBytes((int)frequencyInfo.HoldTime);
                    dataList.Add(holdTimeArrayforFrequencies);
                    var frequencyLengthArray = BitConverter.GetBytes(frequencyInfo.Frequencies.Length);
                    dataList.Add(frequencyLengthArray);
                    var collectionofFrequency =
                        frequencyInfo.Frequencies.Select(item => BitConverter.GetBytes((int)(item * 1000000L)));
                    dataList.AddRange(collectionofFrequency);
                    break;
                case 2: // 扫频
                    var startFrequencyArray =
                        BitConverter.GetBytes((int)(frequencyInfo.StartFrequency * 1000000L - localOscillator));
                    dataList.Add(startFrequencyArray);
                    var stopFrequencyArray =
                        BitConverter.GetBytes((int)(frequencyInfo.StopFrequency * 1000000L - localOscillator));
                    dataList.Add(stopFrequencyArray);
                    var stepFrequencyArray = BitConverter.GetBytes((int)(frequencyInfo.StepFrequency * 1000L));
                    dataList.Add(stepFrequencyArray);
                    var holdTimeArray = BitConverter.GetBytes((int)frequencyInfo.HoldTime);
                    dataList.Add(holdTimeArray);
                    break;
            }

            var dataLength = dataList.Sum(item => item.Length) - 1;
            var dataLengthArray = BitConverter.GetBytes(dataLength);
            dataList.Insert(1, dataLengthArray);
            if (!channelFrequencyFrame.ContainsKey(frequencyInfo.PhysicalChannelNumber))
                channelFrequencyFrame[frequencyInfo.PhysicalChannelNumber] = new List<byte[]>();
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            foreach (var item in dataList) writer.Write(item);
            channelFrequencyFrame[frequencyInfo.PhysicalChannelNumber].Add(stream.ToArray());
        }

        return channelFrequencyFrame;
    }

    private Dictionary<int, List<byte[]>> ToPowerListFrame(float[] powers)
    {
        var channelPowerDict = new Dictionary<int, List<byte[]>>();
        for (var index = 0; index < powers.Length; ++index)
        {
            var featureArray = new byte[] { 0x56 };
            var channelNumberArray = new[] { (byte)(index % 3) };
            var powerArray = BitConverter.GetBytes((short)(powers[index] * 10));
            var contentLengthArray = BitConverter.GetBytes((uint)(channelNumberArray.Length + powerArray.Length));
            var frame = new byte[featureArray.Length + contentLengthArray.Length + channelNumberArray.Length +
                                 powerArray.Length];
            var offset = 0;
            Buffer.BlockCopy(featureArray, 0, frame, offset, featureArray.Length);
            offset += featureArray.Length;
            Buffer.BlockCopy(contentLengthArray, 0, frame, offset, contentLengthArray.Length);
            offset += contentLengthArray.Length;
            Buffer.BlockCopy(channelNumberArray, 0, frame, offset, channelNumberArray.Length);
            offset += channelNumberArray.Length;
            Buffer.BlockCopy(powerArray, 0, frame, offset, powerArray.Length);
            channelPowerDict[index] = new List<byte[]> { frame };
        }

        return channelPowerDict;
    }

    #endregion
}