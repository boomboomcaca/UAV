using System;
using System.IO;
using System.IO.Compression;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Core.Storage;

internal class ReportDataFile : IDisposable
{
    private readonly string _fileExtension;
    private readonly bool _isCompress;
    private readonly string _tempExtension;

    /// <summary>
    ///     当前写入的行
    /// </summary>
    private long _currentIndex;

    private FileStream _fs;

    /// <summary>
    ///     起始时间
    /// </summary>
    private DateTime _startTime = DateTime.MinValue;

    /// <summary>
    ///     结束时间
    /// </summary>
    private DateTime _stopTime = DateTime.MinValue;

    public ReportDataFile(FileDataType fileType, bool isCompress)
    {
        FileType = fileType;
        _isCompress = isCompress;
        _tempExtension = PublicDefine.ExtensionTemporary;
        if (FileType == FileDataType.Report)
            _fileExtension = PublicDefine.ExtensionReport;
        else
            _fileExtension = PublicDefine.ExtensionSignalCensus;
    }

    /// <summary>
    ///     获取文件大小
    /// </summary>
    public long FileSize { get; private set; }

    /// <summary>
    ///     获取当前正在写入的文件全路径名
    /// </summary>
    public string FileFullName { get; private set; }

    /// <summary>
    ///     获取当前正在写入的文件名（不包含扩展名）
    /// </summary>
    public string FileName { get; private set; }

    /// <summary>
    ///     开始记录时间
    /// </summary>
    public DateTime StartTime => _startTime;

    /// <summary>
    ///     结束记录时间
    /// </summary>
    public DateTime StopTime => _stopTime;

    /// <summary>
    ///     当前记录数
    /// </summary>
    public long RecordCount => _currentIndex;

    /// <summary>
    ///     文件类型
    /// </summary>
    public FileDataType FileType { get; }

    public void Dispose()
    {
        CloseFile();
    }

    public void CreateFile(string dir, string fileName, byte[] fileHeader)
    {
        FileName = fileName;
        var fileFullName = $"{fileName}{_fileExtension}{_tempExtension}";
        FileFullName = Path.Combine(dir, fileFullName);
        _fs = File.Create(FileFullName);
        // 预留本文件总帧数的空间
        var rowCount = new byte[4];
        _fs.Write(rowCount, 0, rowCount.Length);
        var tmp = BitConverter.GetBytes(fileHeader.Length);
        _fs.Write(tmp, 0, tmp.Length);
        _fs.Write(fileHeader, 0, fileHeader.Length);
        _fs.Flush();
        _currentIndex = 0;
        _startTime = DateTime.Now; //记录开始时间
        FileSize = _fs.Position;
    }

    public void WriteData(byte[] data, DateTime time)
    {
        if (data == null || data.Length == 0) return;
        if (_isCompress) data = CompressBytes(data);
        var buffer = new byte[FixRowSize() + data.Length];
        if (_currentIndex == 0) _startTime = time; //记录开始时间
        _stopTime = time;
        long offset = 0;
        // 1. 数据行
        var tmp = BitConverter.GetBytes(_currentIndex);
        Array.Copy(tmp, 0, buffer, offset, tmp.Length);
        offset += tmp.Length;
        // 2. 数据时间 单位:相对于1970年的UTC时间的ms
        var stamp = Magneto.Contract.Utils.GetTimestamp(time);
        stamp /= 1000000;
        tmp = BitConverter.GetBytes(stamp);
        Array.Copy(tmp, 0, buffer, offset, tmp.Length);
        offset += tmp.Length;
        // 3. 数据长度
        tmp = BitConverter.GetBytes(data.Length);
        Array.Copy(tmp, 0, buffer, offset, tmp.Length);
        offset += tmp.Length;
        // 4. 数据内容
        Array.Copy(data, 0, buffer, offset, data.Length);
        _fs.Write(buffer, 0, buffer.Length); //写入数据内容
        _fs.Flush();
        FileSize = _fs.Position;
        _currentIndex++;
    }

    public void CloseFile()
    {
        if (_fs != null)
        {
            _fs.Seek(0, SeekOrigin.Begin);
            var count = (int)_currentIndex;
            var buffer = BitConverter.GetBytes(count);
            _fs.Write(buffer, 0, buffer.Length);
            _fs.Flush();
            _fs.Close();
            _fs.Dispose();
        }

        if (FileFullName.Contains(_tempExtension))
        {
            var newPath = FileFullName.Replace(_tempExtension, "");
            File.Move(FileFullName, newPath);
            FileFullName = newPath;
        }
    }

    /// <summary>
    ///     除数据内容外每行固定的字节长度
    /// </summary>
    /// <returns>一行固定的大小</returns>
    private int FixRowSize()
    {
        return sizeof(long) * 2 + sizeof(int);
    }

    /// <summary>
    ///     压缩字节
    ///     1.创建压缩的数据流
    ///     2.设定compressStream为存放被压缩的文件流,并设定为压缩模式
    ///     3.将需要压缩的字节写到被压缩的文件流
    /// </summary>
    /// <param name="bytes"></param>
    private byte[] CompressBytes(byte[] bytes)
    {
        using var compressStream = new MemoryStream();
        using (var zipStream = new GZipStream(compressStream, CompressionMode.Compress))
        {
            zipStream.Write(bytes, 0, bytes.Length);
        }

        return compressStream.ToArray();
    }
}