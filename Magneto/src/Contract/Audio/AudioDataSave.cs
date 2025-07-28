using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Magneto.Contract.Audio;

public class AudioDataSave : IDisposable
{
    /// <summary>
    ///     音频通道数
    /// </summary>
    private int _channels;

    private int _dataLength;
    private FileStream _fs;

    /// <summary>
    ///     采样点数
    /// </summary>
    private int _sampleBits;

    /// <summary>
    ///     采样率
    /// </summary>
    private int _samplingRate;

    private string _saveDir;
    public bool SaveStopped { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime StopTime { get; private set; }

    /// <summary>
    ///     存储根目录
    /// </summary>
    public string RootPath { get; private set; }

    /// <summary>
    ///     相对路径
    /// </summary>
    public string RelativePath { get; private set; }

    /// <summary>
    ///     文件名
    /// </summary>
    public string FileName { get; private set; }

    public long RecordCount { get; private set; }
    public long Size { get; private set; }
    public double Frequency { get; private set; }
    public bool Running { get; private set; }

    public void Dispose()
    {
        try
        {
            _fs?.Dispose();
        }
        catch
        {
            // ignored
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     开始存储音频
    /// </summary>
    /// <param name="rootPath">根目录</param>
    /// <param name="folder">音频文件所在目录</param>
    /// <param name="fileName">文件名</param>
    /// <param name="frequency">中心频率</param>
    /// <param name="channel">音频通道数</param>
    /// <param name="sampleRate">采样率</param>
    /// <param name="samplingBits">采样点数</param>
    public void SaveStart(string rootPath, string folder, string fileName, double frequency, int channel,
        int sampleRate, int samplingBits)
    {
        Frequency = frequency;
        RootPath = rootPath; //Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.PATH_SAVEDATA);
        _saveDir = Path.Combine(RootPath, folder);
        _sampleBits = samplingBits;
        _samplingRate = sampleRate;
        _channels = channel;
        if (!Directory.Exists(_saveDir)) Directory.CreateDirectory(_saveDir);
        RelativePath = folder; //Path.Combine(createTime.ToString("yyyyMMdd"), taskID);
        FileName = fileName;
        var path = Path.Combine(_saveDir, $"{FileName}.wav.temp");
        var header = CreateWaveFileHeader(0, channel, sampleRate, samplingBits);
        _fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        _fs.Write(header, 0, header.Length);
        _fs.Flush();
        StartTime = Utils.GetNowTime();
        Running = true;
    }

    public void SaveData(byte[] data)
    {
        if (!Running) return;
        if (data == null || data.Length == 0) return;
        RecordCount++;
        _dataLength += data.Length;
        _fs.Write(data, 0, data.Length);
        _fs.Flush();
    }

    /// <summary>
    ///     获取当前存储的音频时长 精确到秒
    /// </summary>
    public double GetDuration()
    {
        // 公式 = 总字节数/每秒字节数
        //      = 总字节数/ (比特率*采样位数*通道数/8)
        /*
            譬如 "Windows XP 启动.wav" 的文件长度是 424,644 字节, 它是 "22050HZ / 16bit / 立体声" 格式(这可以从其 "属性->摘要" 里看到),
            那么它的每秒的传输速率(位速, 也叫比特率、取样率)是 22050*16*2 = 705600(bit/s), 换算成字节单位就是 705600/8 = 88200(字节/秒),
            播放时间：424644(总字节数) / 88200(每秒字节数) ≈ 4.8145578(秒)。
            但是这还不够精确, 包装标准的 PCM 格式的 WAVE 文件(*.wav)中至少带有 42 个字节的头信息, 在计算播放时间时应该将其去掉,
            所以就有：(424644-42) / (22050*16*2/8) ≈ 4.8140816(秒). 这样就比较精确了.
            ————————————————
            版权声明：本文为CSDN博主「OH,CGWLMXUP」的原创文章，遵循CC 4.0 BY-SA版权协议，转载请附上原文出处链接及本声明。
            原文链接：https://blog.csdn.net/xiaomucgwlmx/article/details/82787745
        */
        return _dataLength / ((double)_samplingRate * _sampleBits * _channels / 8);
    }

    public void SaveComplete(string newFileName = "")
    {
        SaveStopped = true;
        Running = false;
        try
        {
            _fs.Seek(4, SeekOrigin.Begin);
            var len = BitConverter.GetBytes(_dataLength + 44 - 8);
            _fs.Write(len, 0, len.Length);
            _fs.Flush();
            _fs.Seek(40, SeekOrigin.Begin);
            len = BitConverter.GetBytes(_dataLength);
            _fs.Write(len, 0, len.Length);
            _fs.Flush();
            Size = _fs.Length;
            StopTime = Utils.GetNowTime();
        }
        catch
        {
            // ignored
        }
        finally
        {
            _fs.Close();
        }

        var path = Path.Combine(_saveDir, $"{FileName}.wav.temp");
        var newName = string.IsNullOrEmpty(newFileName) ? FileName : newFileName;
        var newPath = Path.Combine(_saveDir, $"{newName}.wav");
        File.Move(path, newPath);
    }

    /// <summary>
    ///     保存为指定格式（单频测量使用）
    /// </summary>
    /// <param name="format">3-WAV,4-MP3,5-WMA</param>
    public void SaveComplete(int format)
    {
        SaveStopped = true;
        Running = false;
        try
        {
            _fs.Seek(4, SeekOrigin.Begin);
            var len = BitConverter.GetBytes(_dataLength + 44 - 8);
            _fs.Write(len, 0, len.Length);
            _fs.Flush();
            _fs.Seek(40, SeekOrigin.Begin);
            len = BitConverter.GetBytes(_dataLength);
            _fs.Write(len, 0, len.Length);
            _fs.Flush();
            Size = _fs.Length;
            StopTime = Utils.GetNowTime();
        }
        catch
        {
            // ignored
        }
        finally
        {
            _fs.Close();
        }

        _saveDir = Path.GetFullPath(_saveDir);
        var path = Path.Combine(_saveDir, $"{FileName}.wav.temp");
        switch (format)
        {
            case 3:
            {
                Trace.WriteLine("开始生成wav音频");
                var newPath = Path.Combine(_saveDir, $"{FileName}.1.wav.dat");
                File.Move(path, newPath);
                Trace.WriteLine("生成wav音频完毕");
            }
                break;
            case 4:
            {
                _ = Task.Run(() =>
                {
                    Trace.WriteLine("开始生成mp3音频");
                    var newPath = Path.Combine(_saveDir, $"{FileName}.1.mp3.dat");
                    var ret = Convert(path, newPath, 4);
                    if (ret)
                    {
                        Trace.WriteLine("生成mp3音频完毕");
                        File.Delete(path);
                    }
                }).ConfigureAwait(false);
            }
                break;
            case 5:
            {
                _ = Task.Run(() =>
                {
                    Trace.WriteLine("开始生成wma音频");
                    var newPath = Path.Combine(_saveDir, $"{FileName}.wma");
                    var ret = Convert(path, newPath, 5);
                    if (ret)
                    {
                        var wmaPath = Path.Combine(_saveDir, $"{FileName}.1.wma.dat");
                        File.Move(newPath, wmaPath);
                        Trace.WriteLine("生成wma音频完毕");
                        File.Delete(path);
                    }
                }).ConfigureAwait(false);
            }
                break;
        }
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

    #region ffmpeg转码

    /// <summary>
    ///     回放用，ffmpeg进程退出事件
    /// </summary>
    private bool Convert(string sourceFile, string targetFile, int format)
    {
        if (format <= 3) return true;
        var checkPath = GetFFmpegPath(out var ffmpegPath);
        if (!checkPath) return false;
        var arguments = $"-i {sourceFile} -f mp3 -acodec libmp3lame -y {targetFile} -loglevel error";
        if (format == 5) arguments = $"-i {sourceFile} -acodec wmav2 -y {targetFile} -loglevel error";
        Console.WriteLine($"ffmpeg路径:{ffmpegPath},参数:{arguments}");
        return ExecuteCmd(ffmpegPath, arguments, AppDomain.CurrentDomain.BaseDirectory);
    }

    private static bool GetFFmpegPath(out string ffmpegPath)
    {
        ffmpegPath = "";
        var dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.PathLibrary, "ffmpeg");
        if (!Directory.Exists(dllPath))
        {
            Trace.WriteLine("未找到ffmpeg安装路径！");
            return false;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var path = Path.Combine(dllPath, "linux", "ffmpeg");
            //设置ffmpeg权限
            var ret = ExecuteCmd("chmod", $"+x {path}", AppDomain.CurrentDomain.BaseDirectory);
            if (!ret)
            {
                Trace.WriteLine("设置ffmpeg执行权限失败！");
                return false;
            }

            ffmpegPath = path;
            return true;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            ffmpegPath = Path.Combine(dllPath, "windows", "ffmpeg.exe");
            return true;
        }

        Trace.WriteLine("不受支持的系统");
        return false;
    }

    private static bool ExecuteCmd(string fileName, string arguments, string workingDirectory = "")
    {
        try
        {
            var p = new Process();
            var noError = true;
            p.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    noError = false;
                    Trace.WriteLine($"ffmpeg编码错误:{e.Data}");
                }
            };
            p.StartInfo.FileName = fileName;
            p.StartInfo.Arguments = arguments;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.WorkingDirectory = workingDirectory;
            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            p.WaitForExit();
            p.Close();
            return noError;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"执行命令错误:{ex.Message}");
            return false;
        }
    }

    #endregion
}