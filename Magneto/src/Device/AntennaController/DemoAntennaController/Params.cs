using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.DemoAntennaController;

[DeviceDescription(Name = "虚拟天线控制器",
    Version = "2.4.3", DeviceCategory = ModuleCategory.AntennaControl,
    Description = "用于模拟天线控制器，并获取天线因子数据",
    Model = "VIRT-ANT-001",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    Sn = "003-34-54232",
    MaxInstance = 0,
    FeatureType = FeatureType.None)]
public partial class DemoAntennaController
{
}