using System;

namespace Magneto.Device.PR100;

[Flags]
public enum DataType
{
    Fscan = 101,
    Mscan = 201,
    Dscan = 301,
    Audio = 401,
    Ifpan = 501,
    Fastl = 601,
    Listf = 701,
    Cw = 801,
    If = 901,
    Video = 1001,
    Vdpan = 1101,
    Pscan = 1201,
    Selcall = 1301,
    DfPan = 1401,
    LastTag
}

[Flags]
internal enum Flags : uint
{
    Level = 0x1,
    Offset = 0x2,
    Fstrength = 0x4,
    Am = 0x8,
    Ampos = 0x10,
    Amneg = 0x20,
    Fm = 0x40,
    Fmpos = 0x80,
    Fmneg = 0x100,
    Pm = 0x200,
    Band = 0x400,
    Channel = 0x00010000,
    Frequency = 0x00020000,
    Audio = 0x00040000,
    If = 0x00080000,
    Video = 0x00100000,

    /// <summary>
    ///     swap ON means: do NOT swap (for little endian machines)
    /// </summary>
    Swap = 0x20000000,
    Siggtsqu = 0x40000000,
    Optheader = 0x80000000
}