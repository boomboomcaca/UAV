using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract.Defines;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using Newtonsoft.Json.Linq;

namespace Magneto.Contract.BaseClass;

/// <summary>
///     功能基类，暂定名称为Driver
/// </summary>
public abstract class DriverBase : IDriver, IDataPort, IDisposable
{
    private readonly ConcurrentDictionary<SDataType, DataCacheInfo> _dataFrameDic = new();
    private readonly ConcurrentQueue<List<object>> _dataQueue = new();
    private CancellationTokenSource _cts;
    private Task _dataCacheTask;
    private DateTime _lastSendTime = DateTime.Now;
    protected bool AntennaChanged;
    protected volatile bool CanPause;
    protected IDataPort DataPort;

    /// <summary>
    ///     存放此功能所需的所有设备，键为参数名，值为设备实例集合
    /// </summary>
    protected Dictionary<string, List<IDevice>> Devices;

    protected Guid DriverId;

    /// <summary>
    ///     任务运行中标记
    /// </summary>
    protected bool IsTaskRunning;

    protected IDataPort MessageDataPort;
    protected ModuleInfo Module;

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="driverId">模块ID</param>
    protected DriverBase(Guid driverId)
    {
        DriverId = driverId;
    }

    /// <summary>
    ///     当前是否存在竞争（主要指设备竞争）
    /// </summary>
    public bool IsCompetition { get; set; }

    public Guid TaskId { get; private set; } = Guid.Empty;

    /// <summary>
    ///     数据通道
    /// </summary>
    /// <param name="data"></param>
    public virtual void OnData(List<object> data)
    {
    }

    public void OnMessage(SDataMessage message)
    {
    }

    public virtual void Dispose()
    {
        DataPort = null;
        MessageDataPort = null;
        GC.SuppressFinalize(this);
    }

    public Guid Id => DriverId;

    /// <summary>
    ///     清理缓存数据
    /// </summary>
    protected void ClearData()
    {
        _dataQueue.Clear();
        foreach (var pair in _dataFrameDic) pair.Value.Clear();
        _dataFrameDic.Clear();
    }

    protected virtual void SendData(List<object> data)
    {
        if (!IsTaskRunning) return;
        var audioList = new List<object>();
        foreach (var item in data)
            if (item is SDataAudio || (item is SDataDdc ddc && ddc.Data.Any(p => p is SDataAudio)))
                audioList.Add(item);
        // 数据发送到下一级前需要移除集合中的设备ID
        /*
            在设备层数据中添加了设备ID，此举是为了满足以下场景
            单频测向功能中配置了接收机时，示向度数据通过测向机获取，而频谱、电平、音频数据则从接收机获取
            因此这里需要将其移除，在上层不需要此数据了
        */
        data.RemoveAll(item => item is Guid);
        audioList.ForEach(p => data.Remove(p));
        DataPort?.OnData(audioList);
        _dataQueue.Enqueue(data);
    }

    /// <summary>
    ///     发送消息
    /// </summary>
    /// <param name="message"></param>
    protected virtual void SendMessage(SDataMessage message)
    {
        MessageDataPort?.OnMessage(message);
    }

    /// <summary>
    ///     通过消息通道发送数据
    /// </summary>
    /// <param name="data"></param>
    protected virtual void SendMessageData(List<object> data)
    {
        MessageDataPort?.OnData(data);
    }

    private Task DataSendAsync(object obj)
    {
        if (obj is not CancellationToken token) return Task.CompletedTask;
        while (!token.IsCancellationRequested)
            try
            {
                if (!_dataQueue.TryDequeue(out var data))
                {
                    Thread.Sleep(1);
                    // await Task.Delay(0).ConfigureAwait(false);
                    continue;
                }

                DataSampling(data);
                var span = PublicDefine.DataSpan;
                if (IsCompetition) span = 0;
                if (DateTime.Now.Subtract(_lastSendTime).TotalMilliseconds >= span)
                {
                    _lastSendTime = DateTime.Now;
                    var list = GetData();
                    if (list?.Count > 0) data.AddRange(list);
                }

                if (data.Count > 0) DataPort?.OnData(data);
            }
            catch
            {
                // 这是容错代码，防止循环跳出
            }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     数据抽点
    ///     其中扫描数据已经在ScanBase中进行了抽取，因此这里不需要对扫描数据再做抽点了
    /// </summary>
    /// <param name="data"></param>
    private void DataSampling(List<object> data)
    {
        List<object> remove = new();
        foreach (var obj in data)
        {
            if (obj is not SDataRaw raw) continue;
            switch (raw.Type)
            {
                case SDataType.Scan:
                case SDataType.DfScan:
                    break;
                case SDataType.Spectrum:
                case SDataType.Level:
                case SDataType.Dfind:
                case SDataType.Sse:
                case SDataType.Itu:
                    _dataFrameDic.AddOrUpdate(raw.Type,
                        _ =>
                        {
                            var info = new DataCacheInfo(raw.Type);
                            info.AddData(raw);
                            return info;
                        },
                        (_, i) =>
                        {
                            i.AddData(raw);
                            return i;
                        });
                    remove.Add(obj);
                    break;
            }
        }

        remove.ForEach(p => data.Remove(p));
    }

    private List<object> GetData()
    {
        List<object> list = new();
        foreach (var pair in _dataFrameDic)
        {
            var data = pair.Value.GetData();
            if (data != null) list.AddRange(data);
        }

        return list;
    }

    /// <summary>
    ///     获得功能模块对应的执行者（设备）列表 如果没有，则返回值为空的表，但不为null
    ///     <param name="items">参数信息</param>
    ///     <returns>参数及其执行者，如果没有，则返回值为空的表，但不为null</returns>
    /// </summary>
    /// <param name="items"></param>
    private Dictionary<string, List<IDevice>> GetDevices(List<Parameter> items)
    {
        var devices = new List<IDevice>();
        // 查找功能模块定义的设备属性，并添加到列表
        foreach (var pi in GetType().GetProperties())
            if (pi.PropertyType == typeof(IDevice))
            {
                if (pi.GetValue(this, null) is IDevice device
                    && !devices.Contains(device))
                    devices.Add(device);
            }
            else if (pi.PropertyType == typeof(IDevice[]))
            {
                if (pi.GetValue(this, null) is not IDevice[] deviceArray) continue; // warning !!!!!!!!!
                foreach (var device in deviceArray)
                    if (device != null && !devices.Contains(device))
                        devices.Add(device);
            }

        var dicDevices = new Dictionary<string, List<IDevice>>();
        foreach (var item in items.Where(item => !item.IsInstallation))
            if (item.Owners?.Count > 0)
            {
                var list = devices.Where(dev => item.Owners.Contains(dev.Id.ToString()))
                    .ToList();
                dicDevices.Add(item.Name, list);
                if (item.Name != ParameterNames.ScanSegments) continue;
                foreach (var child in item.Template)
                    dicDevices.TryAdd(child.Name, list);
            }

        return dicDevices;
    }

    #region 参数

    [Parameter(AbilitySupport = ~(FeatureType.None | FeatureType.Amia))]
    [Name(ParameterNames.RawSwitch)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("原始数据开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0|1|2|3|4|5",
        DisplayValues = "|关|开|清除|WAV|MP3|WMA")]
    [Description("是否开启原始数据保存")]
    [DefaultValue(0)]
    [PropertyOrder(34)]
    [Style(DisplayStyle.Radio)]
    [Browsable(false)]
    public int RawSwitch { get; set; }

    [Parameter(AbilitySupport = FeatureType.Ffm | FeatureType.Fdf)]
    [Name(ParameterNames.RecByThreshold)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("超过门限存储开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [Description("是否开启超过测量门限以后才保存数据")]
    [DefaultValue(false)]
    [PropertyOrder(34)]
    [Style(DisplayStyle.Switch)]
    [Browsable(false)]
    public bool RecByThreshold { get; set; }

    /// <summary>
    ///     IQ、频谱、音频数据是否存储原始数据的开关
    ///     第0位为音频数据
    ///     第1位为频谱数据
    ///     第2位为IQ数据
    ///     往后依次...
    ///     具体以枚举MediaType定义的为准
    ///     数据类型    预留    预留	预留	示向度数据	电平数据	IQ数据	频谱数据	音频数据
    ///     位序号	    7	    6	    5	    4	    3	        2	    1	        0
    /// </summary>
    [Parameter(AbilitySupport = ~FeatureType.None)]
    [Name(ParameterNames.SaveDataSwitch)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("录制数据保存开关")]
    [Description("IQ、频谱、音频数据是否存储原始数据的开关")]
    [ValueRange(0, 65535, 0)]
    [DefaultValue(0)]
    [Browsable(false)]
    [PropertyOrder(34)]
    [Style(DisplayStyle.Input)]
    public int SaveDataSwitch { get; set; }

    [Parameter(AbilitySupport = FeatureType.Scan)]
    [Name(ParameterNames.MrdSwitch)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("日报数据开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [Description("是否开启日报数据保存")]
    [DefaultValue(false)]
    [PropertyOrder(34)]
    [Style(DisplayStyle.Switch)]
    [Browsable(false)]
    public bool MrdSwitch { get; set; }

    [Parameter(AbilitySupport = ~FeatureType.None)]
    [Name(ParameterNames.UnitSelection)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("单位选择")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0|1|2",
        DisplayValues = "|dBμV|dBμV/m|dBm")]
    [Description("单位选择")]
    [DefaultValue(0)]
    [PropertyOrder(35)]
    [Style(DisplayStyle.Radio)]
    [Browsable(false)]
    public int UnitSelection { get; set; }

    [Parameter(AbilitySupport = ~FeatureType.None)]
    [Name(ParameterNames.MaximumSwitch)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("频谱最大值显示")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [Description("设置是否显示频谱数据的最大值")]
    [DefaultValue(false)]
    [PropertyOrder(30)]
    [Style(DisplayStyle.Switch)]
    [Browsable(false)]
    public bool MaximumSwitch { get; set; }

    [Parameter(AbilitySupport = ~FeatureType.None)]
    [Name(ParameterNames.MinimumSwitch)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("频谱最小值显示")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [Description("设置是否显示频谱数据的最小值")]
    [DefaultValue(false)]
    [PropertyOrder(31)]
    [Style(DisplayStyle.Switch)]
    [Browsable(false)]
    public bool MinimumSwitch { get; set; }

    [Parameter(AbilitySupport = ~FeatureType.None)]
    [Name(ParameterNames.MeanSwitch)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("频谱平均值显示")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [Description("设置是否显示频谱数据的平均值")]
    [DefaultValue(false)]
    [PropertyOrder(32)]
    [Style(DisplayStyle.Switch)]
    [Browsable(false)]
    public bool MeanSwitch { get; set; }

    [Parameter(AbilitySupport = ~FeatureType.None)]
    [Name(ParameterNames.NoiseSwitch)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("频谱噪声显示")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [Description("设置是否显示频谱数据的噪声")]
    [DefaultValue(false)]
    [Browsable(false)]
    [PropertyOrder(33)]
    [Style(DisplayStyle.Switch)]
    public bool NoiseSwitch { get; set; }

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.Scan
                                | FeatureType.Amia
                                | FeatureType.Emda
                                | FeatureType.Fbands)]
    [Name(ParameterNames.ThresholdSwitch)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [Resident]
    [DisplayName("门限开关")]
    [Description("切换门限是否显示")]
    [DefaultValue(true)]
    [Style(DisplayStyle.Switch)]
    [Browsable(false)]
    public bool ThresholdSwitch { get; set; }

    #endregion

    #region

    /// <summary>
    ///     初始化
    /// </summary>
    /// <param name="module"></param>
    public virtual void Initialized(ModuleInfo module)
    {
        Module = module;
        Devices = GetDevices(module.Parameters);
    }

    public virtual void Attach(IDataPort dataPort)
    {
        MessageDataPort = dataPort;
    }

    /// <summary>
    ///     设置参数
    /// </summary>
    /// <param name="name">参数名</param>
    /// <param name="value">参数值</param>
    public virtual void SetParameter(string name, object value)
    {
        // if (_devices?.ContainsKey(name) != true)
        {
            // 设置功能参数
            var type = GetType();
            var prop = Utils.FindPropertyByName(name, type);
            if (prop != null)
            {
                var objValue = value;
                if (prop.PropertyType.IsEnum)
                {
                    objValue = Utils.ConvertStringToEnum(value.ToString(), prop.PropertyType);
                }
                else if (prop.PropertyType == typeof(Guid))
                {
                    objValue = Guid.Parse(value.ToString() ?? string.Empty);
                }
                else if (prop.PropertyType.IsValueType)
                {
                    objValue = Convert.ChangeType(value, prop.PropertyType);
                }
                else if (prop.PropertyType.IsArray)
                {
                    // 没有其他办法，只能暴力解决
                    var tName = prop.PropertyType.FullName!.Replace("[]", string.Empty);
                    var itemType = prop.PropertyType.Assembly.GetType(tName);
                    var tmpValue = value;
                    if (value is JArray jArray) tmpValue = jArray.Select(item => item.ToObject(itemType!)).ToArray();
                    if (tmpValue is object[] array)
                    {
                        // objValue = array.Select(item => Convert.ChangeType(item, itemType));
                        // 这里没办法，上面那种转换方式不可行，只能用下面这种方式了
                        if (itemType! == typeof(int))
                            objValue = array.Select(Convert.ToInt32).ToArray();
                        else if (itemType == typeof(double))
                            objValue = array.Select(Convert.ToDouble).ToArray();
                        else if (itemType == typeof(float))
                            objValue = array.Select(Convert.ToSingle).ToArray();
                        else if (itemType == typeof(short)) objValue = array.Select(Convert.ToInt16).ToArray();
                    }
                }
                else
                {
                    objValue = value; //Convert.ChangeType(value, prop.PropertyType);
                }

                prop.SetValue(this, objValue);
            }
        }
        if (Devices?.TryGetValue(name, out var device) is true)
            foreach (var dev in device)
                dev.SetParameter(name, value);
        if (name.Equals(ParameterNames.AntennaId) || name.Equals("isActive")) AntennaChanged = true;
    }

    /// <summary>
    ///     启动任务
    /// </summary>
    /// <param name="dataPort"></param>
    /// <param name="mediaType"></param>
    public virtual bool Start(IDataPort dataPort, MediaType mediaType)
    {
        TaskId = dataPort.TaskId;
        DataPort = dataPort;
        IsTaskRunning = true;
        _cts = new CancellationTokenSource();
        _dataCacheTask = Task.Run(() => DataSendAsync(_cts.Token));
        CanPause = false;
        _lastSendTime = DateTime.Now;
        // Debug.WriteLine($"{DateTime.Now:HH:mm:ss.fff}    功能{_module.Feature}启动 DriverID={_driverId}");
        return true;
    }

    public virtual bool Pause()
    {
        // Debug.WriteLine($"{DateTime.Now:HH:mm:ss.fff}    功能{_module.Feature}暂停 DriverID={_driverId}");
        return CanPause;
    }

    /// <summary>
    ///     停止任务
    /// </summary>
    public virtual bool Stop()
    {
        try
        {
            _cts?.Cancel();
            _dataCacheTask?.Dispose();
        }
        catch
        {
            // ignored
        }

        // _dataPort = null;
        IsTaskRunning = false;
        CanPause = false;
        ClearData();
        // Debug.WriteLine($"{DateTime.Now:HH:mm:ss.fff}    功能{_module.Feature}停止 DriverID={_driverId}");
        return true;
    }

    #endregion

    // private bool IsDeviceParameterExists(string name, IDevice device)
    // {
    //     var type = device.GetType();
    //     var property = Utils.FindPropertyByName(name, type);
    //     return property != null;
    // }
}