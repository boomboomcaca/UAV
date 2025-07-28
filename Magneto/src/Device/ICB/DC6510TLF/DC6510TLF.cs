using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.DC6510TLF;

public partial class Dc6510Tlf : DeviceBase, IFastIcb
{
    #region 成员变量

    /// <summary>
    ///     串口
    /// </summary>
    private SerialPortClient _client;

    #endregion

    #region 构造函数

    public Dc6510Tlf(Guid id) : base(id)
    {
    }

    #endregion

    #region 重写父类方法

    public override bool Initialized(ModuleInfo device)
    {
        try
        {
            var result = base.Initialized(device);
            if (!result) return false;
            //读取配置
            ReadConfigurations();
            //释放资源
            ReleaseResources();
            //初始化设备
            InitDevices();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            ReleaseResources();
            return false;
        }
    }

    public override void Stop()
    {
        ReleaseResources();
        base.Stop();
    }

    #endregion

    #region 辅助方法

    /// <summary>
    ///     初始化串口连接
    /// </summary>
    private void InitDevices()
    {
        _client = new SerialPortClient(Com, BaudRate);
        if (IsDemo) return;
        var ret = _client.Init(out var err);
        if (!ret)
        {
            var info = new SDataMessage
            {
                LogType = LogType.Warning,
                ErrorCode = (int)InternalMessageType.DeviceRestart,
                Description = err,
                Detail = DeviceInfo.DisplayName
            };
            SendMessage(info);
        }
    }

    /// <summary>
    ///     释放非托管资源
    /// </summary>
    private void ReleaseResources()
    {
        if (IsDemo) return;
        _client.Close();
    }

    /// <summary>
    ///     设置天线测试路径
    /// </summary>
    /// <param name="antPath"></param>
    /// <returns></returns>
    private bool SetAntPath(int antPath)
    {
        string cmd = null;
        switch (antPath)
        {
            case 1:
            case 2:
                cmd = "ANT1:PATH" + antPath;
                break;
            case 3:
                cmd = "ANT2:PATH" + antPath;
                break;
        }

        if (!string.IsNullOrEmpty(cmd))
        {
            var ret = SendAndReceive(cmd, x => x.Contains(cmd + " OK") ? 1 : -1);
            Thread.Sleep(1000);
            return ret;
        }

        return false;
    }

    /// <summary>
    ///     设置噪声源校准路径
    /// </summary>
    /// <param name="noisPath"></param>
    /// <returns></returns>
    private bool SetNoisPath(int noisPath)
    {
        string cmd = null;
        switch (noisPath)
        {
            case 1:
            case 2:
            case 3:
                cmd = "NOIS:PATH" + noisPath;
                break;
        }

        if (!string.IsNullOrEmpty(cmd))
        {
            var ret = SendAndReceive(cmd, x => x.Contains(cmd + " OK") ? 1 : -1);
            Thread.Sleep(1000);
            return ret;
        }

        return false;
    }

    /// <summary>
    ///     设置噪声源供电
    /// </summary>
    /// <param name="noisStatus">true-打开|false-关闭</param>
    /// <returns></returns>
    private bool SetNoisStatus(bool noisStatus)
    {
        var cmd = "NOIS:ON";
        if (!noisStatus) cmd = "NOIS:OFF";
        var ret = SendAndReceive(cmd, x => x.Contains(cmd + " OK") ? 1 : -1);
        Thread.Sleep(1000);
        return ret;
    }

    /// <summary>
    ///     设备复位
    /// </summary>
    /// <returns></returns>
    private bool SetReset()
    {
        var cmd = "RESET";
        var ret = SendAndReceive(cmd, x => x.Contains(cmd + " OK") ? 1 : -1);
        Thread.Sleep(1000);
        return ret;
    }

    /// <summary>
    ///     设置水平转台角度
    /// </summary>
    /// <param name="degree">degree代表需要水平电机旋转到设定角度，可设置度数步进为5°，取0，5，10，15........360</param>
    /// <returns></returns>
    private bool SetDegree(float degree)
    {
        if (degree is < 0 or > 360) return false;
        if (degree % 5 != 0) return false;
        var cmd = "SETH:" + degree;
        return SendAndReceive(cmd, x =>
        {
            if (!x.Contains(cmd + " OK")) return -1;
            return x.Contains(cmd + " Finished") ? 1 : 0;
        });
    }

    /// <summary>
    ///     设置天线极化方向
    /// </summary>
    /// <param name="polarityType">polarity代表天线极化方向，设置H代表水平极化，设置V代表垂直极化</param>
    /// <returns></returns>
    private bool SetPolarity(Polarization polarityType)
    {
        var polarity = string.Empty;
        switch (polarityType)
        {
            case Polarization.Horizontal:
                polarity = "H";
                break;
            case Polarization.Vertical:
                polarity = "V";
                break;
        }

        if (!string.IsNullOrEmpty(polarity))
        {
            var cmd = "SETP:" + polarity;
            return SendAndReceive(cmd, x =>
            {
                if (!x.Contains(cmd + " OK")) return -1;
                return x.Contains(cmd + " Finished") ? 1 : 0;
            });
        }

        return false;
    }

    /// <summary>
    ///     获取水平转台当前角度,X|Horizon rotating.(X取0，5，10，15........360，Horizon rotating.表示水平转台正在旋转中)
    /// </summary>
    private int? GetDegree()
    {
        var result = SendAndReceive2("GETH？");
        if (int.TryParse(result, out var d)) return d;
        return null;
    }

    /// <summary>
    ///     获取竖直转台当前角度。H|V|Polarity rotating.(H为水平极化，V为垂直极化，Polarity rotating.表示极化转台正在旋转中)
    /// </summary>
    private Polarization? GetPolarity()
    {
        var result = SendAndReceive2("GETP？");
        switch (result)
        {
            case "V":
                return Polarization.Vertical;
            case "H":
                return Polarization.Horizontal;
        }

        return null;
    }

    #endregion

    #region 指令发送与数据读取

    private void SendCmd(string cmd)
    {
        _client.SendCommand(cmd);
    }

    private bool SendAndReceive(string cmd, Func<string, int> check)
    {
        return _client.SendAndReceive(cmd, check);
    }

    private string SendAndReceive2(string cmd)
    {
        return _client.SendAndReceive2(cmd);
    }

    #endregion

    #region 射电天文电测

    private readonly string _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FastConfig");
    private readonly Dictionary<int, Dictionary<double, float>> _antHPolarGains = new(); //天线水平极化增益表,MHz-dBd
    private readonly Dictionary<int, Dictionary<double, float>> _antVPolarGains = new(); //天线垂直极化增益表,MHz-dBd
    private Dictionary<double, float> _noiseSourceEnr = new(); //噪声源ENR配置表
    private readonly Dictionary<int, Dictionary<double, float>> _hPolarCableLoss = new(); //水平极化路径下的线缆损耗,MHz-dB
    private readonly Dictionary<int, Dictionary<double, float>> _vPolarCableLoss = new(); //垂直极化路径下的线缆损耗,MHz-dB
    private Dictionary<double, float> _rFCableLoss = new(); //天线控制箱外部连接仪表的线缆损耗

    public void SwitchAntenna(int path, float degree, Polarization polarization)
    {
        AntPath = path;
        Degree = degree;
        PolarityType = polarization;
    }

    public void SwitchNoiseSource(int path, bool isOpen)
    {
        NoisPath = path;
        NoisStatus = isOpen;
    }

    public float GetCurrentDegree()
    {
        return Degree;
    }

    public int GetCurrentAntPath()
    {
        return AntPath;
    }

    public int GetCurrentNosPath()
    {
        return NoisPath;
    }

    public Polarization GetCurrentPolarization()
    {
        return PolarityType;
    }

    /// <summary>
    ///     解析天线增益表[初始化的时候使用]
    /// </summary>
    public Dictionary<double, float> GetAntennaGainTable(int path, Polarization polarization)
    {
        if (polarization == Polarization.Horizontal) return _antHPolarGains[path];
        return _antVPolarGains[path];
    }

    /// <summary>
    ///     解析线缆损耗配置表
    /// </summary>
    public Dictionary<double, float> GetAntPathLossTable(int path, Polarization polarization)
    {
        if (polarization == Polarization.Horizontal) return _hPolarCableLoss[path];
        return _vPolarCableLoss[path];
    }

    /// <summary>
    ///     读取外部线缆损耗配置
    /// </summary>
    public Dictionary<double, float> GetRfCableLossTable()
    {
        return _rFCableLoss;
    }

    /// <summary>
    ///     解析信号源ENR配置表
    /// </summary>
    public Dictionary<double, float> GetNoiseSourceEnr()
    {
        return _noiseSourceEnr;
    }

    /// <summary>
    ///     设备复位设置
    /// </summary>
    public void Reset()
    {
        SetReset();
    }

    /// <summary>
    ///     读取配置
    /// </summary>
    public void ReadConfigurations()
    {
        _antHPolarGains.Clear();
        _antVPolarGains.Clear();
        _noiseSourceEnr.Clear();
        _hPolarCableLoss.Clear();
        _vPolarCableLoss.Clear();
        _rFCableLoss.Clear();
        //解析天线增益表
        try
        {
            var cfgNode = GetConfigRootNode(Path.Combine(_configPath, "FastAntennaGainTable.xml"));
            foreach (XmlNode item in cfgNode.ChildNodes)
            {
                if (item is XmlComment)
                    continue;
                var antIndex = Convert.ToInt32(item.Attributes?["AntIndex"]?.Value);
                var polar = item.Attributes?["AntPolar"]?.Value;
                if (polar is "H")
                    _antHPolarGains.Add(antIndex, GetKeyValuePairsFromXmlNode(item));
                else if (polar is "V") _antVPolarGains.Add(antIndex, GetKeyValuePairsFromXmlNode(item));
            }
        }
        catch
        {
        }

        //解析噪声源超噪比配置
        try
        {
            var cfgNode = GetConfigRootNode(Path.Combine(_configPath, "FastNoiseSourceENR.xml"));
            _noiseSourceEnr = GetKeyValuePairsFromXmlNode(cfgNode);
        }
        catch
        {
        }

        try
        {
            var cfgNode = GetConfigRootNode(Path.Combine(_configPath, "FastAntPathLoss.xml"));
            foreach (XmlNode item in cfgNode.ChildNodes)
            {
                var antIndex = Convert.ToInt32(item.Attributes?["AntIndex"]?.Value);
                var polar = item.Attributes?["AntPolar"]?.Value;
                if (polar is "H")
                    _hPolarCableLoss.Add(antIndex, GetKeyValuePairsFromXmlNode(item));
                else if (polar is "V") _vPolarCableLoss.Add(antIndex, GetKeyValuePairsFromXmlNode(item));
            }
        }
        catch
        {
        }

        //读取外部数据损耗表
        try
        {
            var cfgNode = GetConfigRootNode(Path.Combine(_configPath, "RFCableLoss.xml"));
            _rFCableLoss = GetKeyValuePairsFromXmlNode(cfgNode);
        }
        catch
        {
        }
    }

    /// <summary>
    ///     获取配置根节点
    /// </summary>
    /// <param name="configFileName"></param>
    /// <returns></returns>
    private static XmlNode GetConfigRootNode(string configFileName)
    {
        XmlDocument doc = new();
        doc.Load(configFileName);
        foreach (XmlNode item in doc.ChildNodes)
            if (item.NodeType == XmlNodeType.Element && item.Name == "Root")
                return item;
        return null;
    }

    /// <summary>
    ///     从XmlNode获取键值对集合
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private static Dictionary<double, float> GetKeyValuePairsFromXmlNode(XmlNode node)
    {
        Dictionary<double, float> keyValuePairs = new();
        foreach (XmlNode item in node.ChildNodes)
        {
            var freq = Convert.ToDouble(item.Attributes?["Freq"]?.Value);
            var gainValue = Convert.ToSingle(item.Attributes?["Value"]?.Value);
            keyValuePairs.Add(freq, gainValue);
        }

        return keyValuePairs;
    }

    #endregion
}