using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver.SPEVL;

/// <summary>
///     评估频段信息
/// </summary>
public class EvalueSegmentInfo
{
    #region 初始化

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="startFreq">开始频率</param>
    /// <param name="stopFreq">结束频率</param>
    /// <param name="step">步进</param>
    /// <param name="polarization">极化方式</param>
    /// <param name="dataStartIndex">数据索引</param>
    /// <param name="pointCount">频点数</param>
    public EvalueSegmentInfo(int index, double startFreq, double stopFreq, float step, string polarization,
        int pointCount)
    {
        Index = index;
        StartFreq = startFreq;
        StopFreq = stopFreq;
        Step = step;
        Polarization = polarization;
        PointCount = pointCount;
        EquAndAnt = JoinEquAndAnt();
    }

    #endregion

    #region 私有方法

    /// <summary>
    ///     高四位为设备 低四位为天线极化
    /// </summary>
    /// <returns></returns>
    public byte JoinEquAndAnt()
    {
        var pol = "";
        switch (Polarization.ToUpper())
        {
            case "H":
                pol = "2";
                break;
            case "V":
                pol = "1";
                break;
            case "C":
                pol = "0";
                break;
        }

        var byCode = byte.Parse("1" + pol);
        return (byte)(byCode / 10 * 16 + byCode % 10);
    }

    #endregion

    #region 属性

    /// <summary>
    ///     获取或设置频段索引
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    ///     频点数
    /// </summary>
    public int PointCount { get; }

    /// <summary>
    ///     获取开始频率
    /// </summary>
    public double StartFreq { get; }

    /// <summary>
    ///     获取结束频率
    /// </summary>
    public double StopFreq { get; }

    /// <summary>
    ///     获取步进
    /// </summary>
    public float Step { get; }

    /// <summary>
    ///     获取极化方式
    /// </summary>
    public string Polarization { get; }

    /// <summary>
    ///     获取设备和天线信息
    /// </summary>
    public byte EquAndAnt { get; }

    /// <summary>
    ///     获取或设置天线因子
    /// </summary>
    public float[] Factor { get; set; }

    #endregion
}

/// <summary>
///     评估系统格式监测数据文件
/// </summary>
public class ScanFileWriter
{
    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="filePath">数据文件保存路径</param>
    public ScanFileWriter(string filePath, List<EvalueSegmentInfo> evalSegs, string centerCode, string stationCode,
        bool isMoveStation)
    {
        _scanFilePath = filePath;
        _scanFileThread = new Thread(WriteScanData)
        {
            IsBackground = true
        };
        _scanSegs = evalSegs;
        _centerCode = centerCode;
        _stationCode = stationCode;
        _isMoveStation = isMoveStation;
    }

    /// <summary>
    ///     文件保存进度事件
    /// </summary>
    public event EventHandler<SDataSpevlFileSaveInfo> FileSaved;

    /// <summary>
    ///     文件更新事件
    /// </summary>
    public event EventHandler<FileSavedNotification> FileModified;

    #region 变量定义

    /// <summary>
    ///     频段信息
    /// </summary>
    private readonly List<EvalueSegmentInfo> _scanSegs;

    /// <summary>
    ///     缓存每个频段的数据文件操作流
    /// </summary>
    private readonly Dictionary<int, ScanFileWriteInfo> _dictScanFileStreams = new();

    /// <summary>
    ///     每个频段的天线因子
    /// </summary>
    private readonly Dictionary<int, float[]> _dictFactors = new();

    /// <summary>
    ///     数据缓存
    /// </summary>
    private readonly Queue<EvalDataPacket> _evalFileBuffer = new();

    /// <summary>
    ///     写文件线程
    /// </summary>
    private readonly Thread _scanFileThread;

    /// <summary>
    ///     文件保存路径
    /// </summary>
    private readonly string _scanFilePath;

    private DateTime _preWriteTick = Utils.GetNowTime();

    /// <summary>
    /////     上一次写文件错误记录时间
    ///// </summary>
    //private readonly DateTime _preWriteErrorTime = DateTime.MinValue;
    private readonly object _lockEvalFileBuffer = new();

    private bool _runWrite;

    /// <summary>
    ///     中心CODE
    /// </summary>
    private readonly string _centerCode;

    /// <summary>
    ///     监测站CODE
    /// </summary>
    private readonly string _stationCode;

    /// <summary>
    ///     频谱数据存储时间间隔，单位秒，默认为全部存储
    /// </summary>
    private readonly int _scanSaveGap = 0;

    /// <summary>
    ///     文件校验头
    /// </summary>
    private readonly uint _checkHead = 0xABCDABCD;

    /// <summary>
    ///     文件最大尺寸
    /// </summary>
    private readonly long _fileMaxSize = 200 * 1000 * 1000;

    /// <summary>
    ///     当前是否为移动站
    /// </summary>
    private readonly bool _isMoveStation;

    private readonly int _dataFormatVersion = 2017;
    private readonly int _deviceScanSpeed = 20;

    #endregion

    #region 启动停止

    /// <summary>
    ///     启动数据处理线程
    /// </summary>
    public void Start()
    {
        _runWrite = true;
        _scanFileThread.Start();
    }

    /// <summary>
    ///     停止
    /// </summary>
    public void Stop()
    {
        _runWrite = false;
        //清除数据缓存
        lock (_lockEvalFileBuffer)
        {
            _evalFileBuffer.Clear();
        }

        Thread.Sleep(200);
        if (_dictScanFileStreams != null)
        {
            //关闭每一个数据文件
            foreach (var item in _dictScanFileStreams)
            {
                if (item.Value.FileStream == null) continue;
                var fileSize = item.Value.FileStream.Length;
                try
                {
                    item.Value?.FileStream.Close();
                }
                catch
                {
                }
                finally
                {
                    //发送事件通知，文件已关闭
                    if (item.Value != null)
                    {
                        OnFileSaved(item.Value.FileName, 100, fileSize, fileSize, item.Value.CreatedTime);
                        OnFileModified(item.Value.FileName, FileNotificationType.Modified, item.Value.CreatedTime,
                            fileSize);
                    }
                }
            }

            _dictScanFileStreams.Clear();
        }
    }

    /// <summary>
    ///     文件创建修改消息通知
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="type"></param>
    /// <param name="createdTime"></param>
    /// <param name="fileSize"></param>
    private void OnFileModified(string fileName, FileNotificationType type, DateTime createdTime, long fileSize)
    {
        var notification = new FileSavedNotification
        {
            FileName = fileName,
            NotificationType = type,
            BeginRecordTime = Utils.GetTimestamp(createdTime),
            LastModifiedTime = Utils.GetNowTimestamp(),
            Size = fileSize
        };
        if (type != FileNotificationType.Created) notification.EndRecordTime = Utils.GetNowTimestamp();
        FileModified?.Invoke(this, notification);
    }

    /// <summary>
    ///     文件保存进度消息通知
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="progress"></param>
    /// <param name="currentFileSize"></param>
    /// <param name="maxFileSize"></param>
    /// <param name="createdTime"></param>
    private void OnFileSaved(string fileName, int progress, long currentFileSize, long maxFileSize,
        DateTime createdTime)
    {
        FileSaved?.Invoke(this, new SDataSpevlFileSaveInfo
        {
            Progress = progress,
            FileName = fileName,
            CurrentFileSize = currentFileSize,
            MaxFileSize = maxFileSize,
            CreatedTime = Utils.GetTimestamp(createdTime)
        });
    }

    #endregion

    #region 数据处理相关

    /// <summary>
    ///     添加数据
    /// </summary>
    /// <param name="scanData">扫描数据包</param>
    /// <returns>True:成功  False:失败</returns>
    internal bool AddData(EvalDataPacket scanData)
    {
        // 判断磁盘
        try
        {
            var di = new DriveInfo(_scanFilePath[..2] + "\\");
            if (di.TotalFreeSpace < 209715200) return false;
        }
        catch
        {
        }

        lock (_lockEvalFileBuffer)
        {
            _evalFileBuffer.Enqueue(scanData);
        }

        return true;
    }

    /// <summary>
    ///     创建频谱扫描数据文件
    /// </summary>
    /// <param name="eval">频段信息</param>
    /// <param name="time">时间</param>
    /// <param name="fileName">文件名</param>
    /// <returns>文件操作流</returns>
    private FileStream CreateScanFile(EvalueSegmentInfo eval, DateTime time, out string fileName)
    {
        var stationType = _isMoveStation ? "M" : "F";
        var createDate = time.ToString("yyyyMMdd");
        var createTime = time.ToString("HHmmss");
        var strName =
            $@"{_centerCode}_{_stationCode}_{createDate}_{createTime}_{eval.StartFreq}MHz_{eval.StopFreq}MHz_{eval.Step}kHz_{eval.Polarization}_{stationType}.bin";
        // 创建数据文件
        fileName = Path.Combine(_scanFilePath, strName);
        if (!File.Exists(fileName))
            try
            {
                var fs = File.Create(fileName);
                return fs;
            }
            catch (Exception)
            {
            }

        return null;
    }

    /// <summary>
    ///     写数据到文件
    /// </summary>
    private void WriteScanData()
    {
        while (_runWrite)
        {
            if (_evalFileBuffer == null) break;
            if (_evalFileBuffer.Count == 0)
            {
                Thread.Sleep(30);
                continue;
            }

            EvalDataPacket dataPacket = null;
            //取出缓存的一帧全频段数据
            lock (_lockEvalFileBuffer)
            {
                if (_evalFileBuffer.Count > 0) dataPacket = _evalFileBuffer.Dequeue();
            }

            if (dataPacket == null) continue;
            var ts = dataPacket.MeasureTime - _preWriteTick;
            if (ts.TotalMilliseconds >= _scanSaveGap)
            {
                _preWriteTick = dataPacket.MeasureTime;
                switch (_dataFormatVersion)
                {
                    case 2017:
                        try
                        {
                            Write2017ScanFile(dataPacket);
                        }
                        catch
                        {
                        }

                        break;
                }
            }
        }
    }

    /// <summary>
    ///     2017年数据文件格式
    /// </summary>
    /// <param name="dataPacket">数据包</param>
    private void Write2017ScanFile(EvalDataPacket dataPacket)
    {
        var index = 0;
        //循环每一个频段
        for (var s = 0; s < _scanSegs.Count; s++)
        {
            var evalSeg = _scanSegs[s];
            var sdf = new ScanDataFrame2017(evalSeg.PointCount)
            {
                Altitude = dataPacket.Height,
                Head = Convert.ToString(_checkHead, 16),
                Latitude = dataPacket.Latitude,
                Longitude = dataPacket.Longitude,
                MeasureTime = dataPacket.MeasureTime,
                Parameter = $"1{evalSeg.Polarization}",
                PointCount = evalSeg.PointCount,
                ScanSpeed = _deviceScanSpeed,
                StartFreq = (float)evalSeg.StartFreq,
                Step = evalSeg.Step,
                StationId = _stationCode
            };
            //从全频段数据中取出当前频段的数据
            var btVal = GetDataBytes(s, dataPacket.Data, index, evalSeg.PointCount);
            sdf.SetDatas(btVal);
            index += evalSeg.PointCount;
            if (!_dictScanFileStreams.ContainsKey(s))
            {
                var fs = CreateScanFile(evalSeg, sdf.MeasureTime, out var strFileName);
                if (fs != null)
                {
                    var fsw = new ScanFileWriteInfo(Guid.NewGuid(), strFileName, fs)
                    {
                        CreatedTime = sdf.MeasureTime
                    };
                    _dictScanFileStreams.Add(s, fsw);
                    //发送事件通知
                    OnFileSaved(strFileName, (int)(fs.Length * 100 / _fileMaxSize), fs.Length, _fileMaxSize,
                        fsw.CreatedTime);
                    OnFileModified(strFileName, FileNotificationType.Created, fsw.CreatedTime, fs.Length);
                }
                else
                {
                    continue;
                }
            }

            //获取文件操作对象
            var stream = _dictScanFileStreams[s].FileStream;
            //文件操作对象未被创建，1.上一次未创建成功  2.达到文件最大尺寸，关闭后需新建文件
            if (stream == null)
            {
                //创建文件
                stream = CreateScanFile(evalSeg, sdf.MeasureTime, out var strFileName);
                _dictScanFileStreams[s].Id = Guid.NewGuid();
                _dictScanFileStreams[s].FileName = strFileName;
                _dictScanFileStreams[s].FileStream = stream;
                _dictScanFileStreams[s].CreatedTime = sdf.MeasureTime;
                //发送事件通知文件已创建
                OnFileSaved(strFileName, (int)(stream.Length * 100 / _fileMaxSize), stream.Length, _fileMaxSize,
                    _dictScanFileStreams[s].CreatedTime);
                OnFileModified(strFileName, FileNotificationType.Created, _dictScanFileStreams[s].CreatedTime,
                    stream.Length);
            }

            {
                //写数据
                var frameByte = sdf.ToBytes();
                stream.Write(frameByte, 0, frameByte.Length);
                stream.Flush();
                //如果文件大于最大限定大小
                if (stream.Length >= _fileMaxSize)
                {
                    //发送事件通知，文件已关闭
                    OnFileSaved(_dictScanFileStreams[s].FileName, 100, stream.Length, stream.Length,
                        _dictScanFileStreams[s].CreatedTime);
                    OnFileModified(_dictScanFileStreams[s].FileName, FileNotificationType.Modified,
                        _dictScanFileStreams[s].CreatedTime, stream.Length);
                    //关闭文件
                    stream.Close();
                    //设置文件操作对象为空，以让写下一包数据时创建新文件。
                    _dictScanFileStreams[s].FileStream = null;
                }
                else
                {
                    OnFileSaved(_dictScanFileStreams[s].FileName, (int)(stream.Length * 100 / _fileMaxSize),
                        stream.Length, _fileMaxSize, _dictScanFileStreams[s].CreatedTime);
                }
            }
        }
    }

    /// <summary>
    ///     多频段时，从完整的一帧数据中提取当前频段的数据
    /// </summary>
    /// <param name="segIndex">频段索引</param>
    /// <param name="data">扫描数据</param>
    /// <param name="startDataIndex">复制的起始位置</param>
    /// <param name="len">长度</param>
    /// <returns>提取的数据</returns>
    private byte[] GetDataBytes(int segIndex, float[] data, int startDataIndex, int len)
    {
        //从全频段数据中取出当前频段的数据
        var valIndex = 0;
        var val = new float[len];
        var btVal = new byte[len * 2]; //写入数据文件中的扫描数据
        Array.Copy(data, startDataIndex, val, 0, val.Length);
        var factors = new float[val.Length];
        if (_dictFactors.TryGetValue(segIndex, out var factor))
            factors = factor;
        for (var i = 0; i < val.Length; i++)
        {
            //电平+天线因子 *10 取整
            var temp = BitConverter.GetBytes((short)((val[i] + factors[i]) * 10));
            Array.Copy(temp, 0, btVal, valIndex, temp.Length);
            valIndex += temp.Length;
        }

        return btVal;
    }

    #endregion
}

public class ScanFileWriteInfo
{
    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="strFileName">文件名</param>
    /// <param name="fs">文件操作对象</param>
    public ScanFileWriteInfo(Guid id, string strFileName, FileStream fs)
    {
        Id = id;
        FileName = strFileName;
        FileStream = fs;
        CreatedTime = Utils.GetNowTime();
    }

    /// <summary>
    ///     获取ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     获取文件名
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    ///     获取文件操作对象
    /// </summary>
    public FileStream FileStream { get; set; }

    /// <summary>
    ///     文件创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }
}

/// <summary>
///     评估数据包
/// </summary>
internal class EvalDataPacket
{
    /// <summary>
    ///     监测数据
    /// </summary>
    public readonly float[] Data;

    /// <summary>
    ///     高度
    /// </summary>
    public readonly float Height;

    /// <summary>
    ///     纬度
    /// </summary>
    public readonly double Latitude;

    /// <summary>
    ///     经度
    /// </summary>
    public readonly double Longitude;

    /// <summary>
    ///     接收机扫描速度
    /// </summary>
    public int DeviceScanSpeed;

    /// <summary>
    ///     监测时间
    /// </summary>
    public DateTime MeasureTime;

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="measureTime">监测时间</param>
    /// <param name="data">扫描数据</param>
    /// <param name="longitude">经度</param>
    /// <param name="latigude">纬度</param>
    /// <param name="height">高度</param>
    public EvalDataPacket(DateTime measureTime, float[] data, double longitude, double latitude, float height,
        int scanSpeed)
    {
        MeasureTime = measureTime;
        Data = data;
        Longitude = longitude;
        Latitude = latitude;
        Height = height;
        DeviceScanSpeed = scanSpeed;
    }
}

public class ScanDataFrame2017
{
    private readonly bool _writeData = true;
    private short _altitude = -99;
    private uint _head = 0xABCDABCD;
    private byte _parameter;
    private float[] _scanData;
    private byte[] _scanDatas;

    public ScanDataFrame2017(int pointCount)
    {
        ScanData = new float[pointCount];
    }

    public ScanDataFrame2017(bool writeData = true)
    {
        _writeData = writeData;
    }

    /// <summary>
    ///     记录头校验
    /// </summary>
    public string Head
    {
        get => Convert.ToString(_head, 16);
        set => _head = Convert.ToUInt32(value, 16);
    }

    /// <summary>
    ///     监测站编号
    /// </summary>
    public string StationId { get; set; } = string.Empty;

    /// <summary>
    ///     设备参数设置和天线信息
    /// </summary>
    public string Parameter
    {
        get
        {
            var par = _parameter / 16;
            var ant = _parameter % 16;
            return $"{par}{ant}";
        }
        set
        {
            var ant = value.ToCharArray()[1].ToString();
            var pol = "";
            switch (ant.ToUpper())
            {
                case "H":
                    pol = "2";
                    break;
                case "V":
                    pol = "1";
                    break;
                case "C":
                    pol = "0";
                    break;
            }

            var byCode = byte.Parse($"1{pol}");
            _parameter = (byte)(byCode / 10 * 16 + byCode % 10);
        }
    }

    /// <summary>
    ///     监测时间
    /// </summary>
    public DateTime MeasureTime { get; set; } = DateTime.MinValue;

    /// <summary>
    ///     经度
    /// </summary>
    public double Longitude { get; set; } = -999;

    /// <summary>
    ///     纬度
    /// </summary>
    public double Latitude { get; set; } = -999;

    /// <summary>
    ///     扫描速度
    /// </summary>
    public int ScanSpeed { get; set; } = 20;

    /// <summary>
    ///     天线高度
    /// </summary>
    public float Altitude
    {
        get => _altitude / 10.0f;
        set => _altitude = (short)(value * 10);
    }

    /// <summary>
    ///     起始频率
    /// </summary>
    public float StartFreq { get; set; } = -999;

    /// <summary>
    ///     步进
    /// </summary>
    public float Step { get; set; } = -999;

    /// <summary>
    ///     频点数量
    /// </summary>
    public int PointCount { get; set; }

    /// <summary>
    ///     电平值
    /// </summary>
    public float[] ScanData
    {
        get => _scanData;
        set
        {
            _scanData = value;
            if (_writeData)
            {
                _scanDatas = new byte[PointCount * 2];
                for (var i = 0; i < PointCount; i++)
                {
                    var temp = BitConverter.GetBytes((short)(value[i] * 10));
                    Array.Copy(temp, 0, _scanDatas, i * 2, temp.Length);
                }
            }
        }
    }

    //电平+天线因子 *10 取整 后的数据
    public void SetDatas(byte[] data)
    {
        if (!_writeData)
        {
            // 生成 外部使用float数组
            ScanData = new float[PointCount];
            for (var i = 0; i < PointCount; i++) ScanData[i] = BitConverter.ToInt16(data, i * 2) / 10.0f;
        }
        else
        {
            _scanDatas = data;
        }
    }

    /// <summary>
    ///     转换为标准的byte数组
    /// </summary>
    /// <returns></returns>
    public byte[] ToBytes()
    {
        var byteLen = 52 + PointCount * 2;
        var frame = new byte[byteLen];
        var startIndex = 0;
        // 校验头  4字节
        var byteTmp = BitConverter.GetBytes(_head);
        startIndex = AddDataToFrameBytes(byteTmp, startIndex, ref frame);
        // 监测车（固定站）编号  2字节
        byteTmp = ConvertToBcd(short.Parse(StationId));
        startIndex = AddDataToFrameBytes(byteTmp, startIndex, ref frame);
        // 设备参数设置和天线信息  1字节
        startIndex = AddDataToFrameBytes(_parameter, startIndex, ref frame);
        // 监测时间年 2字节
        var year = (ushort)MeasureTime.Year;
        byteTmp = BitConverter.GetBytes(year);
        startIndex = AddDataToFrameBytes(byteTmp, startIndex, ref frame);
        // 监测时间月 1字节
        var month = (byte)MeasureTime.Month;
        startIndex = AddDataToFrameBytes(month, startIndex, ref frame);
        // 监测时间日 1字节
        var day = (byte)MeasureTime.Day;
        startIndex = AddDataToFrameBytes(day, startIndex, ref frame);
        // 监测时间时 1字节
        var hour = (byte)MeasureTime.Hour;
        startIndex = AddDataToFrameBytes(hour, startIndex, ref frame);
        // 监测时间分 1字节
        var minute = (byte)MeasureTime.Minute;
        startIndex = AddDataToFrameBytes(minute, startIndex, ref frame);
        // 监测时间秒 1字节
        var second = (byte)MeasureTime.Second;
        startIndex = AddDataToFrameBytes(second, startIndex, ref frame);
        // 监测时间毫秒（文档中为纳秒 4字节），这里为2字节？？？
        var millSecond = (ushort)MeasureTime.Millisecond;
        byteTmp = BitConverter.GetBytes(millSecond);
        startIndex = AddDataToFrameBytes(byteTmp, startIndex, ref frame);
        // 扫描速度 2字节
        byteTmp = BitConverter.GetBytes((short)ScanSpeed);
        startIndex = AddDataToFrameBytes(byteTmp, startIndex, ref frame);
        // 经度 8字节
        byteTmp = BitConverter.GetBytes((long)(Longitude * 100000000));
        startIndex = AddDataToFrameBytes(byteTmp, startIndex, ref frame);
        // 纬度  8字节
        byteTmp = BitConverter.GetBytes((long)(Latitude * 100000000));
        startIndex = AddDataToFrameBytes(byteTmp, startIndex, ref frame);
        // 天线挂高 2字节
        byteTmp = BitConverter.GetBytes(_altitude);
        startIndex = AddDataToFrameBytes(byteTmp, startIndex, ref frame);
        // 开始频率 8字节
        byteTmp = BitConverter.GetBytes((double)(StartFreq * 1000000));
        startIndex = AddDataToFrameBytes(byteTmp, startIndex, ref frame);
        // 步长 2字节
        byteTmp = BitConverter.GetBytes(Step * 1000);
        startIndex = AddDataToFrameBytes(byteTmp, startIndex, ref frame);
        // 频点数 4字节
        byteTmp = BitConverter.GetBytes(PointCount);
        startIndex = AddDataToFrameBytes(byteTmp, startIndex, ref frame);
        // 测试数据   每个点2字节,实际存储时 * 10
        AddDataToFrameBytes(_scanDatas, startIndex, ref frame);
        return frame;
    }

    private static int AddDataToFrameBytes(byte[] source, int destinationIndex, ref byte[] destination)
    {
        Array.Copy(source, 0, destination, destinationIndex, source.Length);
        destinationIndex += source.Length;
        return destinationIndex;
    }

    private static int AddDataToFrameBytes(byte source, int destinationIndex, ref byte[] destination)
    {
        destination[destinationIndex] = source;
        destinationIndex += 1;
        return destinationIndex;
    }

    public static ScanDataFrame2017 GetDataFrameByBytes(byte[] dataByte)
    {
        var df = new ScanDataFrame2017(false);
        var index = 0;
        df.Head = Convert.ToString(BitConverter.ToInt32(dataByte, index), 16);
        index += 4;
        df.StationId = BitConverter.ToInt16(dataByte, index).ToString();
        index += 2;
        try
        {
            var par = dataByte[index] / 16;
            var ant = dataByte[index] % 16;
            df.Parameter = $"{par}{ant}";
        }
        catch
        {
            // 写入错误，导入不用，自吞
        }

        index++;
        df.MeasureTime = ParseTime(dataByte, ref index);
        df.ScanSpeed = BitConverter.ToInt16(dataByte, index);
        index += 2;
        df.Longitude = BitConverter.ToInt64(dataByte, index) / 100000000.0d;
        index += 8;
        df.Latitude = BitConverter.ToInt64(dataByte, index) / 100000000.0d;
        index += 8;
        df.Altitude = BitConverter.ToInt16(dataByte, index) / 10.0f;
        index += 2;
        df.StartFreq = (float)BitConverter.ToDouble(dataByte, index) / 1000000.0f;
        index += 8;
        df.Step = BitConverter.ToSingle(dataByte, index) / 1000.0f;
        index += 4;
        df.PointCount = (int)BitConverter.ToUInt32(dataByte, index);
        index += 4;
        df.ScanData ??= new float[df.PointCount];
        for (var i = 0; i < df.PointCount; i++) df.ScanData[i] = BitConverter.ToInt16(dataByte, index + i * 2) / 10.0f;
        return df;
    }

    protected static DateTime ParseTime(byte[] dataByte, ref int dIndx)
    {
        int y = BitConverter.ToInt16(dataByte, dIndx);
        dIndx += 2;
        var m = dataByte[dIndx];
        dIndx++;
        var d = dataByte[dIndx];
        dIndx++;
        var h = dataByte[dIndx];
        dIndx++;
        var mm = dataByte[dIndx];
        dIndx++;
        var s = dataByte[dIndx];
        dIndx++;
        int f = BitConverter.ToInt16(dataByte, dIndx); //int.Parse(Encoding.Default.GetString(dataByte, dIndx + 4, 2));
        dIndx += 2;
        return new DateTime(y, m, d, h, mm, s, f);
    }

    private static byte[] ConvertToBcd(short data)
    {
        try
        {
            var tme = BitConverter.GetBytes(data);
            var res = new byte[tme.Length];
            for (var i = 0; i < tme.Length; i++)
            {
                //高四位  
                var b1 = (byte)(tme[i] / 10);
                //低四位  
                var b2 = (byte)(tme[i] % 10);
                res[i] = (byte)((b1 << 4) | b2);
            }

            return res; //高位在前
        }
        catch
        {
            return null;
        }
    }
}