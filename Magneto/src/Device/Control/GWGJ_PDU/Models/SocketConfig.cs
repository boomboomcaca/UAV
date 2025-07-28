using Magneto.Device.GWGJ_PDU.Common;

namespace Magneto.Device.GWGJ_PDU.Models;

public class SocketConfig
{
    public string Name { get; set; }
    public int OnDelay { get; set; }
    public int OffDelay { get; set; }
    public int RebootDelay { get; set; }
    public ActionMode Action { get; set; }
    public byte IcoId { get; set; }
}