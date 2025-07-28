namespace Magneto.Device.GWGJ_PDU.Models;

public class ItemData<T>
{
    public int Index { get; set; }
    public T Content { get; set; }
}