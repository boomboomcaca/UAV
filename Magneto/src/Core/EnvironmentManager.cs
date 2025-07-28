using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Core.Configuration;
using Core.Environment;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using StreamJsonRpc;

namespace Core;

public class EnvironmentManager
{
    private static readonly Lazy<EnvironmentManager> _lazy = new(() => new EnvironmentManager());
    private readonly ConcurrentQueue<object> _dataQueue = new();
    private readonly Dictionary<Guid, EnvBusiness> _environments = new();
    private bool _isRunning;

    /// <summary>
    ///     环境监控管理器
    /// </summary>
    public static EnvironmentManager Instance => _lazy.Value;

    public void Initialized()
    {
        var devices = DeviceConfig.Instance.Devices;
        devices.ForEach(item =>
        {
            if (item.State == ModuleState.Disabled) return;
            var env = new EnvBusiness(item.Id, item.EdgeId);
            env.DataPort.DataArrived += OnDataArrived;
            _environments.Add(item.Id, env);
        });
        _isRunning = true;
        ThreadPool.QueueUserWorkItem(IniEnvironment);
        ThreadPool.QueueUserWorkItem(SendData);
    }

    public void SetParameter(Guid moduleId, Parameter parameter)
    {
        if (!_environments.ContainsKey(moduleId))
        {
            var ex = new LocalRpcException("未找到此模块ID")
            {
                ErrorCode = -32097
            };
            throw ex;
            // return;
        }

        var env = _environments[moduleId];
        env.SetParameter(parameter);
    }

    public void SetParameters(Guid moduleId, List<Parameter> parameter)
    {
        if (!_environments.ContainsKey(moduleId))
        {
            var ex = new LocalRpcException("未找到此模块ID")
            {
                ErrorCode = -32097
            };
            throw ex;
            // return;
        }

        var env = _environments[moduleId];
        env.SetParameters(parameter);
    }

    public void Close()
    {
        foreach (var pair in _environments) pair.Value.DataPort.DataArrived -= OnDataArrived;
        _isRunning = false;
    }

    internal void DeviceStateChange(SDataMessage message)
    {
        if (message.ErrorCode != (int)InternalMessageType.DeviceRestart) return;
        var id = Guid.Parse(message.Description);
        if (!_environments.ContainsKey(id)) return;
        var env = _environments[id];
        env.Stop();
    }

    private void IniEnvironment(object obj)
    {
        while (_isRunning)
        {
            Thread.Sleep(1000);
            foreach (var pair in _environments)
                if (!pair.Value.IsRunning)
                    pair.Value.Start();
        }
    }

    private void SendData(object obj)
    {
        while (_isRunning)
        {
            if (_dataQueue.IsEmpty)
            {
                Thread.Sleep(10);
                continue;
            }

            if (!_dataQueue.TryDequeue(out var data)) Thread.Sleep(10);
            if (data != null) MessageManager.Instance.SendMessage(data);
        }
    }

    private void OnDataArrived(Guid moduleId, List<object> data)
    {
        data.RemoveAll(item => item is Guid);
        _dataQueue.Enqueue(data);
    }
}