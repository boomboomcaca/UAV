using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Magneto.Device.DC7520MOB.IO;

public interface IClient : IDisposable
{
    public int Timeout { get; set; }
    public event EventHandler<string> DataReceived;
    public event EventHandler<bool> ConnectionChanged;
    public bool Init(bool[] channels, out string err);
    public bool SendSyncCmd(string cmd, out string recv);
    public void SendCommand(string cmd);
    public bool SendCommands(string[] cmds, out List<string> recv);
    public Task<(bool success, string data)> SendCommandAsync(string cmd, CancellationToken token);
    public Task<(bool success, List<string> datas)> SendCommandsAsync(string[] cmds, CancellationToken token);
    public void Close();
}