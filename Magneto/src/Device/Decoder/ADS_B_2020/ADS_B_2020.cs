/***************************************************************
 *
 * 类    名: ADS_B_2020
 * 作    者: 侯华
 * 创作日期: 2020-9-24 9:58:06
 * 功能概述：
 *
 * --------------修改记录------------
 * 修改时间：xxxx.xx.xx    修改者：xxx
 * 修改说明：
 *
 ***************************************************************/

using System;
using System.Collections.Generic;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device;

public partial class AdsB2020 : DeviceBase
{
    private Client _client;

    public AdsB2020(Guid id) : base(id)
    {
    }

    #region Client类对象回调方法

    /// <summary>
    ///     设备数据到达
    /// </summary>
    /// <param name="data">数据：包括飞机信息数据和设备状态上报数据</param>
    private void Client_OnData(SDataAdsB data)
    {
        SendMessageData(new List<object> { data });
    }

    #endregion Client类对象回调方法

    #region Implement

    /// <summary>
    ///     初始化设备
    /// </summary>
    /// <param name="moduleInfo">模块信息</param>
    /// <returns>true=成功；false=失败</returns>
    public override bool Initialized(ModuleInfo moduleInfo)
    {
        var result = base.Initialized(moduleInfo);
        if (!result) return false;
        if (_client == null)
        {
            _client = new Client(DeviceIp, DevicePort, Interval);
            _client.OnData += Client_OnData;
        }

        result = _client.InitConnect();
        if (!result) return false;
        Start(FeatureType.None, null);
        SetHeartBeat(_client.Sokcet);
        return true;
    }

    /// <summary>
    ///     没有外部功能调用，由内部初始化时调用
    /// </summary>
    /// <param name="featureType"></param>
    /// <param name="dataPort"></param>
    public override void Start(FeatureType featureType, IDataPort dataPort)
    {
        base.Start(featureType, dataPort);
        _client.Start();
    }

    /// <summary>
    ///     没有外部功能调用，由内部释放资源时调用
    /// </summary>
    /// <returns>true=成功；false=失败</returns>
    public override void Stop()
    {
        base.Stop();
        _client.Stop();
    }

    /// <summary>
    ///     释放资源
    /// </summary>
    public override void Dispose()
    {
        Stop();
        if (_client == null) return;
        _client.OnData -= Client_OnData;
        _client.Dispose();
        _client = null;
    }

    #endregion Implement
}