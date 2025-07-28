using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Magneto.Contract;
using Magneto.Contract.Storage;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using NUnit.Framework;

namespace Tests.Core;

[TestFixture]
public class FileDataTest
{
    [OneTimeSetUp]
    public void Setup()
    {
        _rootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.PathSavedata);
        if (Directory.Exists(_rootPath))
        {
            var di = new DirectoryInfo(_rootPath);
            di.Delete(true);
        }
    }

    private string _rootPath;
    private string _dataFolderPath;
    private bool _isFileNotified;
    private string _fileName;
    private DateTime _createdTime;

    [Test]
    [Order(1)]
    [TestCase("示例任务1", "监测站1", "接收机1", "单频测量")]
    public void FileWrite(string taskName, string stationName, string deviceName, string functionName)
    {
        var parameters = new List<Parameter>
        {
            new()
            {
                Browsable = true,
                Description = "示例参数描述1", DisplayName = "示例参数名1"
            },
            new()
            {
                Browsable = true,
                Description = "示例参数描述2", DisplayName = "示例参数名2"
            }
        };
        try
        {
            var taskId = Guid.NewGuid();
            var edgeId = Guid.NewGuid().ToString("N");
            _createdTime = DateTime.Now;
            var summary = new FileDataSummary
            {
                DataType = FileDataType.Raw,
                DeviceId = Guid.NewGuid().ToString(),
                DriverId = Guid.NewGuid().ToString(),
                EdgeId = edgeId,
                PluginId = Guid.NewGuid().ToString(),
                TaskId = taskId.ToString()
            };
            var manager = RawDataStorage.Instance;
            manager.FileModified += FileModified;
            manager.Create(summary, FeatureType.FFM, _createdTime, parameters);
            for (var i = 0; i < 100; i++)
            {
                var data = new SDataScan
                {
                    SegmentOffset = i,
                    StartFrequency = 100000,
                    StopFrequency = 100000000,
                    Total = 2000,
                    Data = new short[] { 20, 21, 22, 23, 24, 25, 26, 27, 28, 29 },
                    Minimum = new short[] { 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 },
                    Maximum = new short[] { 30, 31, 32, 33, 34, 35, 36, 37, 38, 39 }
                };
                manager.Write(taskId, new List<object> { data });
                Thread.Sleep(10);
            }

            manager.Complete(taskId);
            var timeDir = Path.Combine(_rootPath, _createdTime.ToString("yyyyMMdd"));
            _dataFolderPath = Path.Combine(timeDir, taskId.ToString());
            if (!Directory.Exists(_dataFolderPath)) Assert.Fail("没有任务数据文件夹");
            var files = Directory.GetFiles(_dataFolderPath);
            var name = $"{edgeId}_ffm_{_createdTime:yyyyMMddHHmmss}";
            _fileName = name;
            var file = files.Where(f => Regex.IsMatch(Path.GetFileName(f), $"^{name}\\.\\d+\\.dat$"));
            if (!file.Any()) Assert.Fail("没有任务数据文件");
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    [Test]
    [Order(2)]
    public void FileNotify()
    {
        Assert.AreEqual(_isFileNotified, true);
    }

    [Test]
    [Order(3)]
    public void FileRead()
    {
        try
        {
            var manager = RawDataStorage.Instance;
            RawDataStorage.ReadFile(_dataFolderPath, _fileName, out _, out var data, out _);
            if (data.Count != 100) Assert.Fail();
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    //[Test]
    //public void ReadTemp()
    //{
    //    var manager = RawDataStorage.Instance;
    //    var folderPath = @"E:\00_WORK\Magneto\1130\bin\Debug\AnyCPU\data\20211201\7e766fb1-7c7f-40c6-9c43-9043246fb699";
    //    manager.ReadFile(folderPath, "20211119_ffm_20211201192451", out var summary, out var data, out var parametersInfo);
    //    if (data.Count <= 0)
    //    {
    //        Assert.Fail();
    //    }
    //}
    private void FileModified(object sender, FileSavedNotification notification)
    {
        _isFileNotified = true;
    }
}