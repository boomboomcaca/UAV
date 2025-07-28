using System;

namespace Magneto.Device.EBD190.IO;

public interface IClient : IDisposable
{
    public int BytesToRead { get; }
    public DateTime LastGetDataTime { get; set; }
    public event EventHandler<string> DataReceived;
    public bool Init(out string err);
    public void SendCmd(string cmd);
    public void Close();
    public void DiscardInBuffer();
}