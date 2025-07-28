using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NAudio.Wave;
using NAudio.Wave.Compression;

namespace Magneto.Contract.Audio;

public class AudioProcess
{
    private AcmStream _resampleStream;
    private int _srcBitPerSample;
    private int _srcChannels;
    private int _srcSampleRate;
    private int _targetBitPerSample;
    private int _targetChannels;
    private int _targetSampleRate;

    public List<byte> Convert(byte[] buffer, int sampleRate, int bitPerSample, int channels)
    {
        if (_srcChannels != channels || _srcSampleRate != sampleRate || _srcBitPerSample != bitPerSample)
        {
            _srcChannels = channels;
            _srcSampleRate = sampleRate;
            _srcBitPerSample = bitPerSample;
            _resampleStream?.Dispose();
            _resampleStream = new AcmStream(new WaveFormat(_srcSampleRate, _srcBitPerSample, _srcChannels),
                new WaveFormat(_targetSampleRate, _targetBitPerSample, _targetChannels));
        }

        List<byte> list = new();
        var count = buffer.Length;
        if (count > _resampleStream.SourceBuffer.Length) count = _resampleStream.SourceBuffer.Length;
        var offset = 0;
        while (offset < buffer.Length)
        {
            Buffer.BlockCopy(buffer, offset, _resampleStream.SourceBuffer, 0, count);
            var convertedBytes = _resampleStream.Convert(count, out var sourceBytesConverted);
            if (sourceBytesConverted != count)
            {
                Console.WriteLine("We didn't convert everything {0} bytes in, {1} bytes converted");
                break;
            }

            offset += count;
            count = buffer.Length - offset;
            if (count > _resampleStream.SourceBuffer.Length) count = _resampleStream.SourceBuffer.Length;
            var converted = new byte[convertedBytes];
            Buffer.BlockCopy(_resampleStream.DestBuffer, 0, converted, 0, convertedBytes);
            list.AddRange(converted);
        }

        return list;
    }

    public void Start(int targetBitPerSample, int targetSampleRate, int targetChannels)
    {
        _targetBitPerSample = targetBitPerSample;
        _targetChannels = targetChannels;
        _targetSampleRate = targetSampleRate;
    }

    public void Stop()
    {
        _resampleStream?.Dispose();
    }

    public void Test()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audiodata.bin");
        var buffer = File.ReadAllBytes(path);
        var outFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audiodata1.wav");
        using var resampleStream = new AcmStream(new WaveFormat(22050, 16, 1), new WaveFormat(16000, 16, 1));
        var count = buffer.Length;
        if (count > resampleStream.SourceBuffer.Length) count = resampleStream.SourceBuffer.Length;
        var offset = 0;
        List<byte> list = new();
        while (offset < buffer.Length)
        {
            Buffer.BlockCopy(buffer, offset, resampleStream.SourceBuffer, 0, count);
            var convertedBytes = resampleStream.Convert(count, out var sourceBytesConverted);
            if (sourceBytesConverted != count)
                Console.WriteLine("We didn't convert everything {0} bytes in, {1} bytes converted");
            offset += count;
            count = buffer.Length - offset;
            if (count > resampleStream.SourceBuffer.Length) count = resampleStream.SourceBuffer.Length;
            var converted = new byte[convertedBytes];
            Buffer.BlockCopy(resampleStream.DestBuffer, 0, converted, 0, convertedBytes);
            list.AddRange(converted);
        }

        var header = CreateWaveFileHeader(list.Count, 1, 16000, 16);
        var fs = new FileStream(outFile, FileMode.Create, FileAccess.Write);
        fs.Write(header, 0, header.Length);
        fs.Write(list.ToArray(), 0, list.Count);
        fs.Flush();
        fs.Close();
    }

    /// <summary>
    ///     创建WAV音频文件头信息
    /// </summary>
    /// <param name="dataLen">音频数据长度</param>
    /// <param name="dataSoundCh">音频声道数</param>
    /// <param name="dataSample">采样率，常见有：11025、22050、44100等</param>
    /// <param name="dataSamplingBits">采样位数，常见有：4、8、12、16、24、32</param>
    private byte[] CreateWaveFileHeader(int dataLen, int dataSoundCh, int dataSample, int dataSamplingBits)
    {
        // WAV音频文件头信息
        var wavHeaderInfo = new List<byte>(); // 长度应该是44个字节
        wavHeaderInfo.AddRange(
            Encoding.ASCII
                .GetBytes("RIFF")); // 4个字节：固定格式，“RIFF”对应的ASCII码，表明这个文件是有效的 "资源互换文件格式（Resources lnterchange File Format）"
        wavHeaderInfo.AddRange(BitConverter.GetBytes(dataLen + 44 - 8)); // 4个字节：总长度-8字节，表明从此后面所有的数据长度，小端模式存储数据
        wavHeaderInfo.AddRange(Encoding.ASCII.GetBytes("WAVE")); // 4个字节：固定格式，“WAVE”对应的ASCII码，表明这个文件的格式是WAV
        wavHeaderInfo.AddRange(Encoding.ASCII.GetBytes("fmt ")); // 4个字节：固定格式，“fmt ”(有一个空格)对应的ASCII码，它是一个格式块标识
        wavHeaderInfo.AddRange(BitConverter.GetBytes(16)); // 4个字节：fmt的数据块的长度（如果没有其他附加信息，通常为16），小端模式存储数据
        var fmtStruct = new
        {
            PCM_Code = (short)1, // 4B，编码格式代码：常见WAV文件采用PCM脉冲编码调制格式，通常为1。
            SoundChannel = (short)dataSoundCh, // 2B，声道数
            SampleRate = dataSample, // 4B，没个通道的采样率：常见有：11025、22050、44100等
            BytesPerSec =
                dataSamplingBits * dataSample * dataSoundCh /
                8, // 4B，数据传输速率 = 声道数×采样频率×每样本的数据位数/8。播放软件利用此值可以估计缓冲区的大小。
            BlockAlign = (short)(dataSamplingBits * dataSoundCh / 8), // 2B，采样帧大小 = 声道数×每样本的数据位数/8。
            SamplingBits = (short)dataSamplingBits // 4B，每个采样值（采样本）的位数，常见有：4、8、12、16、24、32
        };
        // 依次写入fmt数据块的数据（默认长度为16）
        wavHeaderInfo.AddRange(BitConverter.GetBytes(fmtStruct.PCM_Code));
        wavHeaderInfo.AddRange(BitConverter.GetBytes(fmtStruct.SoundChannel));
        wavHeaderInfo.AddRange(BitConverter.GetBytes(fmtStruct.SampleRate));
        wavHeaderInfo.AddRange(BitConverter.GetBytes(fmtStruct.BytesPerSec));
        wavHeaderInfo.AddRange(BitConverter.GetBytes(fmtStruct.BlockAlign));
        wavHeaderInfo.AddRange(BitConverter.GetBytes(fmtStruct.SamplingBits));
        /* 还 可以继续写入其他的扩展信息，那么fmt的长度计算要增加。*/
        wavHeaderInfo.AddRange(Encoding.ASCII.GetBytes("data")); // 4个字节：固定格式，“data”对应的ASCII码
        wavHeaderInfo.AddRange(BitConverter.GetBytes(dataLen)); // 4个字节：正式音频数据的长度。数据使用小端模式存放，如果是多声道，则声道数据交替存放。
        /* 到这里文件头信息填写完成，通常情况下共44个字节*/
        return wavHeaderInfo.ToArray();
    }
}