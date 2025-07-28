using System.Collections.Generic;
using Magneto.Protocol.Define;

namespace Magneto.Contract.Interface;

#region 射电天文电测项目专用接口

/// <summary>
///     射电天文电测控制箱
/// </summary>
public interface IFastIcb
{
    /// <summary>
    ///     设置天线
    /// </summary>
    /// <param name="path">测试路径编号，有效编号从1-3，50MHz~150MHz-1,150MHz~1GHz-2,1GHz~12GHz-3</param>
    /// <param name="degree">水平转台角度</param>
    /// <param name="polarization">天线极化方式</param>
    void SwitchAntenna(int path, float degree, Polarization polarization);

    /// <summary>
    ///     设置噪声源校准路径
    /// </summary>
    /// <param name="path">噪声源校准路径编号，有效编号从1-3，50MHz~150MHz-1,150MHz~1GHz-2,1GHz~12GHz-3</param>
    /// <param name="isOpen">噪声源是否打开</param>
    void SwitchNoiseSource(int path, bool isOpen);

    /// <summary>
    ///     获取当前水平转台角度
    /// </summary>
    /// <returns></returns>
    float GetCurrentDegree();

    /// <summary>
    ///     获取当前天线测试路径
    /// </summary>
    /// <returns>测试路径编号，有效编号从1-3，50MHz~150MHz-1,150MHz~1GHz-2,1GHz~12GHz-3</returns>
    int GetCurrentAntPath();

    /// <summary>
    ///     获取当前噪声源校准路径
    /// </summary>
    /// <returns>噪声源校准路径编号，有效编号从1-3，50MHz~150MHz-1,150MHz~1GHz-2,1GHz~12GHz-3</returns>
    int GetCurrentNosPath();

    /// <summary>
    ///     获取当前的天线极化方式
    /// </summary>
    /// <returns></returns>
    Polarization GetCurrentPolarization();

    /// <summary>
    ///     获取指定天线路径的增益表    频率(MHz),增益(dBd)
    /// </summary>
    /// <param name="path">测试路径编号，有效编号从1-3，50MHz~150MHz-1,150MHz~1GHz-2,1GHz~12GHz-3</param>
    /// <param name="polarization">极化方式</param>
    /// <returns>返回增益值列表</returns>
    Dictionary<double, float> GetAntennaGainTable(int path, Polarization polarization);

    /// <summary>
    ///     获取控制箱内连接天线的通道损耗表 频率(MHz),损耗(dB)
    /// </summary>
    /// <param name="path">测试路径编号，有效编号从1-3，50MHz~150MHz-1,150MHz~1GHz-2,1GHz~12GHz-3</param>
    /// <param name="polarization"></param>
    /// <returns></returns>
    Dictionary<double, float> GetAntPathLossTable(int path, Polarization polarization);

    /// <summary>
    ///     获取外部线缆的损耗表
    /// </summary>
    /// <returns></returns>
    Dictionary<double, float> GetRfCableLossTable();

    /// <summary>
    ///     获取 噪声源超噪比配置表 频率(MHz),ENR(dB)
    /// </summary>
    /// <returns></returns>
    Dictionary<double, float> GetNoiseSourceEnr();

    /// <summary>
    ///     设备复位
    /// </summary>
    void Reset();
}

/// <summary>
///     用于Fast项目的频谱扫描接口
/// </summary>
public interface IFastSpectrumScan
{
    /// <summary>
    ///     开始频率
    /// </summary>
    public double StartFrequency { get; set; }

    /// <summary>
    ///     结束频率
    /// </summary>
    public double StopFrequency { get; set; }

    /// <summary>
    ///     参考电平
    /// </summary>
    public float ReferenceLevel { get; set; }

    /// <summary>
    ///     衰减
    /// </summary>
    public float Attenuation { get; set; }

    /// <summary>
    ///     分辨率带宽
    /// </summary>
    public double ResolutionBandwidth { get; set; }

    /// <summary>
    ///     视频带宽
    /// </summary>
    public double VideoBandwidth { get; set; }

    /// <summary>
    ///     前置预放
    /// </summary>
    public bool PreAmpSwitch { get; set; }

    /// <summary>
    ///     积分时间
    /// </summary>
    public float IntegrationTime { get; set; }

    /// <summary>
    ///     重复次数
    /// </summary>
    public int RepeatTimes { get; set; }

    /// <summary>
    ///     扫描时间
    /// </summary>
    public int ScanTime { get; set; }

    /// <summary>
    ///     获取校准数据
    /// </summary>
    /// <returns></returns>
    FastGeneralScan GetCalibrationData();

    /// <summary>
    ///     返回当前实际的Rbw值
    /// </summary>
    /// <returns></returns>
    double GetCurrentRealRbw();

    /// <summary>
    ///     返回当前实际的Vbw值
    /// </summary>
    /// <returns></returns>
    double GetCurrentRealVbw();

    /// <summary>
    ///     返回当前实际的衰减值
    /// </summary>
    /// <returns></returns>
    float GetCurrentRealAtt();

    /// <summary>
    ///     设备复位
    /// </summary>
    void Reset();
}

/// <summary>
///     扫描数据[fast项目专用]
/// </summary>
public class FastGeneralScan
{
    /// <summary>
    ///     扫描数据，单位dBm
    /// </summary>
    public float[] Data;

    /// <summary>
    ///     扫描的频点集合
    /// </summary>
    public double[] Freqs;

    /// <summary>
    ///     起始频率（MHz）
    /// </summary>
    public double Start;

    /// <summary>
    ///     起始频率（MHz）
    /// </summary>
    public double Stop;

    /// <summary>
    ///     数据点数
    /// </summary>
    public int TotalPoints;
}

#endregion