using System;

namespace Magneto.Device;

public class BsInfo
{
    public long Id { get; set; }
    public Guid TaskId { get; set; }
    public string EdgeId { get; set; }
    public string CarrierOperator { get; set; }
    public string Generation { get; set; }
    public string DuplexMode { get; set; }
    public int Channel { get; set; }
    public double Frequency { get; set; }
    public double Bandwidth { get; set; }
    public string GlobalId { get; set; }
    public uint Mcc { get; set; }
    public uint Mnc { get; set; }
    public uint Lac { get; set; }
    public ulong Ci { get; set; }
    public double RxPower { get; set; }
    public double FieldStrength { get; set; }
    public double? StationLng { get; set; }
    public double? StationLat { get; set; }
    public double? EdgeLng { get; set; }
    public double? EdgeLat { get; set; }
    public int IsFakeStation { get; set; }
    public string CreateTime { get; set; }
    public string UpdateTime { get; set; }
    public string ExInfos { get; set; }
}