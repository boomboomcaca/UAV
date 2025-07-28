using System;
using System.Collections.Generic;
using System.Threading;

namespace Magneto.Contract;

/// <summary>
///     线程安全队列
/// </summary>
/// <typeparam name="T"></typeparam>
public class MQueue<T> : IDisposable
{
    private const int MaxDataCount = int.MaxValue;
    private readonly Queue<T> _dataQueue = new();
    private readonly object _queueLock = new();
    private Semaphore _queueSemap = new(0, MaxDataCount);

    /// <summary>
    ///     获取当前队列长度
    /// </summary>
    public int Count
    {
        get
        {
            lock (_queueLock)
            {
                return _dataQueue.Count;
            }
        }
    }

    /// <summary>
    ///     释放非托管资源
    /// </summary>
    public void Dispose()
    {
        if (_queueSemap != null)
        {
            _queueSemap.Close();
            _queueSemap = null;
        }
    }

    /// <summary>
    ///     入队操作
    /// </summary>
    /// <param name="data"></param>
    public void EnQueue(T data)
    {
        lock (_queueLock)
        {
            _dataQueue.Enqueue(data);
        }

        _queueSemap.Release();
    }

    /// <summary>
    ///     出队操作 调用此方法时注意出队的结果可以为null 也即default(T)
    /// </summary>
    /// <param name="milliSecondsTimeout"></param>
    public T DeQueue(int milliSecondsTimeout = -1)
    {
        _queueSemap.WaitOne(milliSecondsTimeout);
        try
        {
            lock (_queueLock)
            {
                if (_dataQueue.Count == 0) return default;
                return _dataQueue.Dequeue();
            }
        }
        catch
        {
            //DeQueue()和Clear()在多线程中的使用可能会引发队列为空的异常，调用此方法时注意判空
            return default;
        }
    }

    /// <summary>
    ///     清空队列
    /// </summary>
    public void Clear()
    {
        lock (_queueLock)
        {
            //将信号量置为0，避免清空队列后信号量还在，当频繁切换任务有可能造成信号量累加超过最大值，导致Release时异常
            while (_queueSemap.WaitOne(0))
            {
            }

            _dataQueue.Clear();
        }
    }
}