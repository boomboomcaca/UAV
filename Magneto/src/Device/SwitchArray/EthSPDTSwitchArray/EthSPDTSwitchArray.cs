using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using Magneto.Contract.BaseClass;
using Magneto.Protocol.Define;

namespace Magneto.Device.EthSPDTSwitchArray;

public partial class EthSpdtSwitchArray : SwitchArrayBase
{
    #region 构造函数

    public EthSpdtSwitchArray(Guid deviceId) : base(deviceId)
    {
    }

    #endregion

    #region 辅助方法

    // 初始化网络
    private void InitNetworks()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true
        };
        _socket.Connect(Ip, Port);
    }

    #endregion

    #region 成员变量

    private Socket _socket;
    private readonly ConcurrentDictionary<SwitchUsage, int> _switchUsageTable = new();

    #endregion

    #region 重写基类方法

    public override bool Initialized(ModuleInfo device)
    {
        try
        {
            if (base.Initialized(device))
            {
                InitNetworks();
                SetHeartBeat(_socket);
                Reset(); // 初始化将开关状态复位，如默认打成监测开，管制关
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    protected override Stream GetLazyStream()
    {
        if (_socket == null) return null;
        return new NetworkStream(_socket);
    }

    // 复切换开关到监测状态
    public override void Reset()
    {
        RaiseSwitchChangeNotification(_switchUsageTable.ContainsKey(SwitchUsage.RadioMonitoring)
            ? -1
            : _switchUsageTable[SwitchUsage.RadioMonitoring]);
    }

    public override void Dispose()
    {
        _socket?.Close();
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion
}