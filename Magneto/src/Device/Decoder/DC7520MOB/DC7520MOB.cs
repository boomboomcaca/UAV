using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Device.DC7520MOB.IO;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.DC7520MOB;

public partial class Dc7520Mob : DeviceBase
{
    private IClient _client;

    public Dc7520Mob(Guid deviceId) : base(deviceId)
    {
    }

    private void Client_ConnectionChanged(object sender, bool e)
    {
        if (e) return;
        // 连接中断
        var info = new SDataMessage
        {
            LogType = LogType.Warning,
            ErrorCode = (int)InternalMessageType.DeviceRestart,
            Description = DeviceId.ToString(),
            Detail = DeviceInfo.DisplayName
        };
        SendMessage(info);
    }

    #region 成员变量

    /// <summary>
    ///     通道2指令发送线程
    /// </summary>
    private Task _cmdSendTask;

    private CancellationTokenSource _cmdSendTokenSource;

    #endregion

    #region DeviceBase

    /// <summary>
    ///     初始化设备模块
    /// </summary>
    /// <param name="moduleInfo">设备信息</param>
    /// <returns>true=成功；false=失败</returns>
    public override bool Initialized(ModuleInfo moduleInfo)
    {
        var result = base.Initialized(moduleInfo);
        if (result)
        {
            // 清理非托管资源
            ReleaseResource();
            // 实例化设备控制客户端
            _client = GetClient();
            if (_client == null)
            {
                Trace.WriteLine("初始化设备失败");
                return false;
            }

            // 连接设备
            var success = _client.Init(new[] { UseChannel1, UseChannel2, UseChannel3 }, out var err);
            if (!success)
            {
                Trace.WriteLine($"初始化设备连接失败，失败原因：{err}");
                return false;
            }

            _client.ConnectionChanged += Client_ConnectionChanged;
            // 启动线程
            StartCmdSendTasks();
        }

        return result;
    }

    /// <summary>
    ///     释放资源
    /// </summary>
    public override void Dispose()
    {
        ReleaseResource();
        base.Dispose();
    }

    #endregion
}