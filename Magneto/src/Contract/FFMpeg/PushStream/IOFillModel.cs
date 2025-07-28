namespace Magneto.Contract.FFMpeg.PushStream;

/// <summary>
///     数据填充模式
/// </summary>
public enum IoFillModel
{
    /// <summary>
    ///     一次性读取本地文件，将字节数据缓存到内存，需要时根据偏移量及大小取出数据块
    /// </summary>
    ReadLocalFile = 0,

    /// <summary>
    ///     网络接收数据场景：通过socket接收到数据，并将数据写入到缓存中，需要时根据偏移量及大小取出数据块
    /// </summary>
    ContinueWrite = 1
}