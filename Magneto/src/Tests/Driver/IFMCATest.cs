using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Magneto.Contract;
using Magneto.Core.Define;
using Magneto.Device.DemoAntennaController;
using Magneto.Device.DemoReceiver;
using Magneto.Driver.IFMCA;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using MessagePack;
using NUnit.Framework;

namespace Tests.Driver;

[TestFixture]
public class IfmcaTest
{
    [OneTimeSetUp]
    public void Setup()
    {
        RunningInfo.EdgeId = _edgeId;
        _dataPort = new DataPort(_taskId);
        _messagePort = new DataPort(Guid.Empty);
        var devId = Guid.NewGuid();
        _device = new DemoReceiver(devId);
        var antId = Guid.NewGuid();
        _antennaController = new DemoAntennaController(antId)
        {
            Antennas = new List<AntennaInfo>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Model = AntennaModel.Dh8911
                }
            }
        };
        _module = new ModuleInfo
        {
            Id = Guid.NewGuid(),
            Category = ModuleCategory.Monitoring
        };
        var freqPara = new Parameter
        {
            Name = ParameterNames.Frequency,
            Owners = new List<string> { devId.ToString() },
            Value = 101.7
        };
        var bwPara = new Parameter
        {
            Name = ParameterNames.IfBandwidth,
            Owners = new List<string> { devId.ToString() },
            Value = 40000
        };
        var specSwitch = new Parameter
        {
            Name = ParameterNames.SpectrumSwitch,
            Owners = new List<string> { devId.ToString() },
            Value = true
        };
        var maxChanCount = new Parameter
        {
            Name = "maxChanCount",
            Owners = new List<string> { devId.ToString() },
            Value = 32
        };
        var ddcChannels = new Parameter
        {
            Name = ParameterNames.DdcChannels,
            Owners = new List<string> { devId.ToString() },
            Template = new List<Parameter>
            {
                new()
                {
                    Name = ParameterNames.Frequency,
                    Value = 101.7
                },
                new()
                {
                    Name = ParameterNames.FilterBandwidth,
                    Value = false
                },
                new()
                {
                    Name = ParameterNames.IfSwitch,
                    Value = false
                },
                new()
                {
                    Name = ParameterNames.AudioSwitch,
                    Value = false
                },
                new()
                {
                    Name = ParameterNames.SpectrumSwitch,
                    Value = false
                },
                new()
                {
                    Name = ParameterNames.LevelSwitch,
                    Value = false
                }
            }
        };
        _module.Parameters = new List<Parameter>
        {
            freqPara,
            bwPara,
            specSwitch,
            maxChanCount,
            ddcChannels
        };
        _driver = new Ifmca(_module.Id);
        _driver.Attach(_messagePort);
        var devModule = new ModuleInfo
        {
            Id = _device.Id,
            Category = ModuleCategory.Monitoring
        };
        var antModule = new ModuleInfo
        {
            Id = _antennaController.Id,
            Category = ModuleCategory.AntennaControl
        };
        _device.Initialized(devModule);
        _antennaController.Initialized(antModule);
        _driver.SetParameter(ParameterNames.Receiver, _device);
        _driver.SetParameter(ParameterNames.AntennaController, _antennaController);
        _driver.Initialized(_module);
        _driver.SetParameter("maxChanCount", 32);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        if (_dataPort != null) _dataPort.DataArrived -= DataArrived;
        if (_messagePort != null) _messagePort.DataArrived -= MsgDataArrived;
        _driver?.Dispose();
        _driver = null;
        _device?.Dispose();
        _device = null;
        _module = null;
    }

    private ModuleInfo _module;
    private DemoReceiver _device;
    private DemoAntennaController _antennaController;
    private DataPort _dataPort;
    private DataPort _messagePort;
    private Ifmca _driver;
    private readonly Guid _taskId = Guid.NewGuid();
    private readonly string _edgeId = "testEdge";
    private readonly AutoResetEvent _autoResetEvent = new(false);
    private SDataAvicgFrequencies _frequencies;

    [Test]
    [Order(1)]
    public void StartAvicgTest()
    {
        _driver.SetParameter("autoChannelMode", true);
        _dataPort.DataArrived += DataArrived;
        _messagePort.DataArrived += MsgDataArrived;
        _driver.SetParameter(ParameterNames.Frequency, 101.7);
        _driver.SetParameter(ParameterNames.IfBandwidth, 40000);
        _driver.Start(_dataPort, MediaType.None);
        _autoResetEvent.WaitOne(130000);
        if (_frequencies != null)
            Assert.Pass($"测试通过...{_frequencies.Frequency},{_frequencies.IfBandwidth},{_frequencies.Frequencies.Count}");
        else
            Assert.Fail("测试超时");
    }

    [Test]
    [Order(2)]
    public void ReadFileTest()
    {
        var idxFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data2", "avicg",
            DateTime.Now.ToString("yyyyMMdd"), $"{RunningInfo.EdgeId}_avicg_{DateTime.Now:yyyyMMdd}.idx");
        var dataFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data2", "avicg",
            DateTime.Now.ToString("yyyyMMdd"), $"{RunningInfo.EdgeId}_avicg_{DateTime.Now:yyyyMMdd}.1.dat");
        if (!File.Exists(idxFile) || !File.Exists(dataFile))
        {
            Assert.Fail("文件不存在");
            return;
        }

        //读索引文件
        uint index = 0;
        using var idxStream = File.OpenRead(idxFile);
        idxStream.Seek(index, SeekOrigin.Begin);
        var frameTotalArray = new byte[4];
        var unused = idxStream.Read(frameTotalArray, 0, frameTotalArray.Length);
        var frameTotal = BitConverter.ToUInt32(frameTotalArray);
        index += (uint)frameTotalArray.Length;
        idxStream.Seek(index, SeekOrigin.Begin);
        var headerLenArray = new byte[4];
        idxStream.Read(headerLenArray, 0, headerLenArray.Length);
        var headerLen = BitConverter.ToUInt32(headerLenArray);
        index += (uint)headerLenArray.Length;
        idxStream.Seek(index, SeekOrigin.Begin);
        var headerArray = new byte[headerLen];
        idxStream.Read(headerArray, 0, headerArray.Length);
        MessagePackSerializer.Deserialize<Dictionary<string, object>>(headerArray);
        index += headerLen;
        const uint size = sizeof(long) + sizeof(ushort) + sizeof(int) + sizeof(ulong);
        var frameList = new List<(long dataIndex, ushort fileIndex, int position, ulong timestamp)>();
        for (var i = 0; i < frameTotal; i++)
        {
            idxStream.Seek(index, SeekOrigin.Begin);
            var tempIdxArray = new byte[size];
            idxStream.Read(tempIdxArray, 0, tempIdxArray.Length);
            GetIndexValue(tempIdxArray, size, out var dataIndex, out var fileIndex, out var position,
                out var timestamp);
            frameList.Add((dataIndex, fileIndex, position, timestamp));
            index += size;
        }

        var maxIndex = frameList.Max(p => p.dataIndex);
        if (maxIndex != frameTotal - 1) return;
        //读取数据文件
        var list = new List<object>();
        using var dataStream = File.OpenRead(dataFile);
        var selectedFrames = frameList.Where(p => p.fileIndex == 1)
            .OrderBy(p => p.position)
            .ToList();
        for (var i = 0; i < selectedFrames.Count; i++)
        {
            int len;
            var frame = selectedFrames[i];
            if (i == selectedFrames.Count - 1)
                len = (int)(dataStream.Length - frame.position);
            else
                len = selectedFrames[i + 1].position - frame.position;
            if (len <= 0) continue;
            var dataArray = new byte[len];
            dataStream.Seek(frame.position, SeekOrigin.Begin);
            dataStream.Read(dataArray, 0, dataArray.Length);
            var obj = MessagePackSerializer.Deserialize<object>(dataArray);
            list.Add((frame.dataIndex, obj));
        }

        Assert.Pass($"测试通过,读取到数据个数：{list.Count}");
    }

    private void GetIndexValue(byte[] array, uint size, out long dataIndex, out ushort fileIndex,
        out int position, out ulong timestamp)
    {
        dataIndex = 0;
        fileIndex = 0;
        position = 0;
        timestamp = 0;
        if (array.Length != size) return;
        var index = 0;
        var temp = new byte[sizeof(long)];
        Array.Copy(array, index, temp, 0, temp.Length);
        dataIndex = BitConverter.ToInt64(temp);
        index += sizeof(long);
        temp = new byte[sizeof(ushort)];
        Array.Copy(array, index, temp, 0, temp.Length);
        fileIndex = BitConverter.ToUInt16(temp);
        index += sizeof(ushort);
        temp = new byte[sizeof(int)];
        Array.Copy(array, index, temp, 0, temp.Length);
        position = BitConverter.ToInt32(temp);
        index += sizeof(int);
        temp = new byte[sizeof(ulong)];
        Array.Copy(array, index, temp, 0, temp.Length);
        timestamp = BitConverter.ToUInt64(temp);
    }

    private void DataArrived(Guid taskId, List<object> data)
    {
    }

    private int _count;

    private void MsgDataArrived(Guid taskId, List<object> data)
    {
        foreach (var item in data)
            if (item is SDataAvicgFrequencies freqs)
            {
                _frequencies = freqs;
                _count++;
                if (_count >= 2) _autoResetEvent.Set();
            }
    }
}