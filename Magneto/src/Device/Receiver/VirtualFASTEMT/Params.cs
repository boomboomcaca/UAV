using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.VirtualFASTEMT;

[DeviceDescription(Name = "虚拟射电天文电测频谱仪",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.Monitoring,
    FeatureType = FeatureType.FASTEMT,
    MaxInstance = 1,
    Version = "1.0.0",
    Model = "VirtualFASTEMT",
    Description = "虚拟射电天文电测频谱仪")]
public partial class VirtualFastemt
{
    public double StartFrequency { get; set; }
    public double StopFrequency { get; set; }
    public float ReferenceLevel { get; set; }
    public float Attenuation { get; set; }
    public double ResolutionBandwidth { get; set; }
    public double VideoBandwidth { get; set; }
    public bool PreAmpSwitch { get; set; }
    public float IntegrationTime { get; set; }
    public int RepeatTimes { get; set; }
    public int ScanTime { get; set; }
}