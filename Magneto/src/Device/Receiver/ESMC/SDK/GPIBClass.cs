using System;
using System.Text;

namespace Magneto.Device.ESMC.SDK;

/// <summary>
///     GPIB读取
/// </summary>
public class GpibClass
{
    private int _dev;

    public short BoardNumber { get; set; }
    public bool DataAsString { get; set; }
    public string EosChar { get; set; }
    public bool EotMode { get; set; }
    public short PrimaryAddress { get; set; }
    public short SecondaryAddress { get; set; }

    public void Clear()
    {
        if (_dev <= 0) return;
        Gpib.ibclr(_dev);
        Gpib.gpib_get_globals(out var ibsta, out _, out _, out _);
        if ((ibsta & (int)Gpib.IbstaBits.Err) != 0)
            throw new Exception("Error in clear the specific GPIB instrument.");
    }

    public void Configure()
    {
        if (!int.TryParse(EosChar, out var eosmode)) eosmode = 0;
        _dev = Gpib.ibdev(BoardNumber, PrimaryAddress, SecondaryAddress, (int)Gpib.GpibTimeout.T1S, 1, eosmode);
        Gpib.gpib_get_globals(out var ibsta, out _, out _, out _);
        if ((ibsta & (int)Gpib.IbstaBits.Err) != 0) throw new Exception("Error in initializing the GPIB instrument.");
    }

    public bool IsOnline()
    {
        if (_dev > 0) return true;
        return false;
    }

    public object Read(short n)
    {
        //Read the response string from the GPIB instrument using the ibrd() command
        if (_dev <= 0) return string.Empty;
        var str = new StringBuilder(n);
        Gpib.ibrd(_dev, str, str.Capacity);
        Gpib.gpib_get_globals(out var ibsta, out _, out _, out _);
        if ((ibsta & (int)Gpib.IbstaBits.Err) != 0)
            throw new Exception("Error in Read the response string from the GPIB.");
        return str.ToString();
    }

    public void Send(string vNewValue)
    {
        //Write a string command to a GPIB instrument using the ibwrt() command
        if (_dev <= 0) return;
        Gpib.ibwrt(_dev, vNewValue, vNewValue.Length);
        Gpib.gpib_get_globals(out var ibsta, out _, out _, out _);
        if ((ibsta & (int)Gpib.IbstaBits.Err) != 0) throw new Exception("Error in Write a string command to a GPIB.");
    }

    public void Reset()
    {
        //Offline the GPIB interface card
        if (_dev <= 0) return;
        Gpib.ibonl(_dev, 0);
        Gpib.gpib_get_globals(out var ibsta, out _, out _, out _);
        if ((ibsta & (int)Gpib.IbstaBits.Err) != 0) throw new Exception("Error in offline the GPIB interface card.");
    }
}