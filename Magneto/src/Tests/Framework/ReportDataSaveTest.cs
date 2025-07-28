using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Magneto.Contract;
using Magneto.Core.Define;
using Magneto.Core.Storage;
using Magneto.Core.Tasks;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using MessagePack;
using NUnit.Framework;

namespace Tests.Core;

[TestFixture]
public class ReportDataSaveTest
{
    [OneTimeSetUp]
    public void Setup()
    {
        _random = new Random();
        _dataCount = 100;
        _startFrequency = 88;
        _stopFrequency = 108;
        _stepFrequency = 25;
        _pointCount = Utils.GetTotalCount(_startFrequency, _stopFrequency, _stepFrequency);
        _data = new SDataScan[_dataCount];
        for (var i = 0; i < _dataCount; i++)
        {
            _data[i] = new SDataScan
            {
                SegmentOffset = 0,
                StartFrequency = _startFrequency,
                StopFrequency = _stopFrequency,
                StepFrequency = _stepFrequency,
                Offset = 0,
                Data = new short[_pointCount]
            };
            for (var j = 0; j < _pointCount; j++)
            {
                _data[i].Data[j] = (short)(_random.Next(-10, 10) * 10);
                if (j == _pointCount / 3) _data[i].Data[j] = (short)(_random.Next(-50, -40) * 10);
                if (j == _pointCount / 2) _data[i].Data[j] = (short)(_random.Next(40, 50) * 10);
                if (j == _pointCount * 2 / 3) _data[i].Data[j] = (short)(_random.Next(10, 20) * 10);
            }
        }
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    private SDataScan[] _data;
    private int _dataCount;
    private double _startFrequency;
    private double _stopFrequency;
    private double _stepFrequency;
    private int _pointCount;
    private Random _random;
    private string _fileNameWithNotCompress;
    private string _fileNameWithCompress;
    private string _dir;

    [Test]
    [Order(1)]
    // [TestCase(false, false)]
    [TestCase(true, true)]
    public void FileWriteTest(bool isCompress, bool isSplit)
    {
        try
        {
            var time = DateTime.Now;
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.PathSavedata);
            var save = new ReportDataSave(dir, 1000, FileDataType.Report, isCompress, isSplit);
            var task = new TaskInfo
            {
                Id = Guid.NewGuid(),
                BeginTime = DateTime.Now,
                Factors = new List<SDataFactor>
                {
                    new()
                    {
                        SegmentOffset = 0,
                        StartFrequency = _startFrequency,
                        StopFrequency = _stopFrequency,
                        StepFrequency = _stepFrequency,
                        Data = new short[_pointCount]
                    }
                }
            };
            _dir = Path.Combine(dir, task.Id.ToString());
            save.Initialized(0, _startFrequency, _stopFrequency, _stepFrequency, "142857", task);
            save.Start();
            for (var i = 0; i < _dataCount; i++)
            {
                Thread.Sleep(30);
                save.SetData(new List<object> { _data[i] });
            }

            if (isCompress)
                _fileNameWithCompress = save.GetNowWritingDataFileName();
            else
                _fileNameWithNotCompress = save.GetNowWritingDataFileName();
            save.Stop();
            var span = DateTime.Now.Subtract(time).TotalMilliseconds;
            Console.WriteLine($"存储完毕，用时:{span}ms");
        }
        catch (Exception e)
        {
            Assert.Fail(e.ToString());
        }
    }

    [Test]
    [Order(2)]
    // [TestCase(false, @"F:\git\Magneto\bin\Debug\AnyCPU\saveData\8c58c424-98fa-44fb-be5a-c95ebc30fad1\142857-20210908181003172-20MHz-1000MHz-25kHz.cus")]
    // [TestCase(false)]
    [TestCase(true)]
    public void ReadFileTest(bool isCompress, string file = "")
    {
        try
        {
            var time = DateTime.Now;
            if (string.IsNullOrEmpty(file)) file = isCompress ? _fileNameWithCompress : _fileNameWithNotCompress;
            file = file.Replace(PublicDefine.ExtensionTemporary, "");
            using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var headeLen = new byte[8];
            fs.Read(headeLen, 0, 8);
            BitConverter.ToInt32(headeLen, 0);
            var hlen = BitConverter.ToInt32(headeLen, 4);
            var header = new byte[hlen];
            fs.Read(header, 0, hlen);
            MessagePackSerializer.Deserialize<ReportDataHeader>(header);
            var data = new List<List<List<LevelDistribution>>>();
            while (fs.CanRead)
            {
                var rowBuffer = new byte[20];
                var count = fs.Read(rowBuffer, 0, 20);
                if (count == 0) break;
                BitConverter.ToInt64(rowBuffer, 0);
                var stamp = BitConverter.ToInt64(rowBuffer, 8);
                var localTime = new DateTime(1970, 1, 1).AddMilliseconds(stamp).ToLocalTime();
                var len = BitConverter.ToInt32(rowBuffer, 16);
                var buffer = new byte[len];
                fs.Read(buffer, 0, len);
                if (isCompress) buffer = Decompress(buffer);
                var rowList = new List<List<LevelDistribution>>();
                var offset = 0;
                while (offset < buffer.Length)
                {
                    var levelCount = buffer[offset];
                    offset++;
                    var list = new List<LevelDistribution>();
                    byte index = 0;
                    for (var i = 0; i < levelCount; i++)
                    {
                        var level = new LevelDistribution();
                        var value = (short)buffer[offset];
                        offset++;
                        if (value > 127) value = (short)(127 - value);
                        level.Value = value;
                        var cnt = BitConverter.ToUInt16(buffer, offset);
                        offset += 2;
                        level.Count = cnt;
                        level.Index = index;
                        index++;
                        list.Add(level);
                    }

                    rowList.Add(list);
                }

                data.Add(rowList);
            }

            var span = DateTime.Now.Subtract(time).TotalMilliseconds;
            Console.WriteLine($"读取数据完毕:{data.Count},用时:{span}ms");
        }
        catch (Exception e)
        {
            Assert.Fail(e.ToString());
        }
    }

    /// <summary>
    ///     解压缩字节
    ///     1.创建被压缩的数据流
    ///     2.创建zipStream对象，并传入解压的文件流
    ///     3.创建目标流
    ///     4.zipStream拷贝到目标流
    ///     5.返回目标流输出字节
    /// </summary>
    /// <param name="bytes"></param>
    public byte[] Decompress(byte[] bytes)
    {
        using var compressStream = new MemoryStream(bytes);
        using var zipStream = new GZipStream(compressStream, CompressionMode.Decompress);
        using var resultStream = new MemoryStream();
        zipStream.CopyTo(resultStream);
        return resultStream.ToArray();
    }
}