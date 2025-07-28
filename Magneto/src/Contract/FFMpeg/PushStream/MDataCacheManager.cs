using System;
using System.Collections.Concurrent;

namespace Magneto.Contract.FFMpeg.PushStream;

/// <summary>
///     媒体数据缓存管理对象
/// </summary>
internal sealed class MDataCacheManager
{
    private static readonly Lazy<MDataCacheManager> _lazy = new(() => new MDataCacheManager());

    private readonly ConcurrentDictionary<string, MDataItem> _mData = new();

    private MDataCacheManager()
    {
    }

    /// <summary>
    ///     单例
    /// </summary>
    public static MDataCacheManager Instance => _lazy.Value;

    /// <summary>
    ///     GetMDataItem
    /// </summary>
    /// <param name="terminalNum"></param>
    /// <param name="channel"></param>
    public MDataItem GetMDataItem(string terminalNum, int channel)
    {
        if (string.IsNullOrWhiteSpace(terminalNum)) return null;
        var key = GetKey(terminalNum, channel);
        if (!_mData.ContainsKey(key)) _mData[key] = new MDataItem(terminalNum, channel);
        return _mData[key];
    }

    /// <summary>
    ///     GetKey
    /// </summary>
    /// <param name="terminalNum"></param>
    /// <param name="channel"></param>
    private string GetKey(string terminalNum, int channel)
    {
        return $"{terminalNum}-{channel}";
    }
}

internal class MDataItem(string terminalNum, int channel, int capacity = 1024 * 4)
{
    /// <summary>
    ///     512MB
    /// </summary>
    private const int MaxSize = 1024 * 1024 * 512;

    private readonly int _capacity = capacity;

    // this.Data = new ArrayList(capacity);

    /// <summary>
    ///     设备号
    /// </summary>
    public string TerminalNum { get; private set; } = terminalNum;

    /// <summary>
    ///     通道号
    /// </summary>
    public int Channel { get; private set; } = channel;

    //public byte[] Data { get; set; }
    //public ArraySegment<byte> Data { get; set; }
    ///// <summary>
    ///// 数据缓存
    ///// </summary>
    // ArrayList Data { get; set; }

    /// <summary>
    ///     当前缓存的总数据量
    /// </summary>
    public int Size => Data?.Count ?? 0;

    /// <summary>
    ///     数据缓存
    /// </summary>
    private ConcurrentQueue<byte> Data { get; } = new();

    /// <summary>
    ///     添加数据
    /// </summary>
    /// <param name="data"></param>
    public void AddData(byte[] data)
    {
        if (data?.Length > 0)
        {
            if (Size + data.Length > MaxSize) throw new OutOfMemoryException($"当前累计写入的数据大于允许的最大缓存容量({MaxSize}MB).");
            // Data?.AddRange(data);
            foreach (var item in data) Data.Enqueue(item);
        }
    }

    /// <summary>
    ///     读取数据
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="size"></param>
    public byte[] GetData(int offset, int size)
    {
        var result = Array.Empty<byte>();
        if (Data?.Count > 0 && offset <= Data.Count - 1)
        {
            int len;
            // 计算实际数组长度
            if (offset + size <= Data.Count - 1)
                len = size;
            else
                len = Data.Count - offset;
            result = new byte[len];
            // Array.Copy(Data.ToArray(), offset, result, 0, result.Length);
            for (var index = 0; index < len; index++)
            {
                var suc = Data.TryDequeue(out var value);
                if (suc) result[index] = value;
            }
        }

        return result;
    }

    ///// <summary>
    ///// nous
    ///// </summary>
    ///// <param name="offset"></param>
    ///// <param name="size"></param>
    //public void Remove(int offset, int size)
    //{
    //    if (Data == null || Data.Count == 0) return;
    //    if(offset >= Data.Count || 
    //       offset + size >= Data.Count)
    //    {
    //        Data?.Clear();
    //    }
    //    else
    //    {
    //        Data?.RemoveRange(offset, size);
    //    }
    //}
    /// <summary>
    ///     重置，即清空缓存数据
    /// </summary>
    public void Reset()
    {
        // Data?.Clear();
        while (Data.TryDequeue(out _))
        {
        }
    }
}