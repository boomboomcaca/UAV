namespace Magneto.Device.DT1000AS.Driver;

internal static class Crc8Helper
{
    public const int CrcMaskSlotdata = 0;
    public const int CrcMaskC2I = 0x0f;
    public const int CrcMaskId = 0xf0;
    public const int CrcMaskBcchRssi = 0x03;
    public const int CrcMaskSdcchRssi = 0x30;
    public const int CrcMaskTchRssi = 0x33;
    public const int CrcMaskBcchArfcn = 0xaa;
    public const int CrcMaskHeartBeat = 0x55;
    public const int PacketSlot = 1;
    public const int PacketC2I = 2;
    public const int PacketId = 3;
    public const int PacketBcchRssi = 4;
    public const int PacketSdcchRssi = 5;
    public const int PacketTchRssi = 6;
    public const int PacketBcchArfcn = 7;
    public const int PacketHeartBeat = 8;

    /// <summary>
    ///     编码
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="len"></param>
    public static void Encode(byte[] buffer, short len)
    {
        short m;
        byte[] crc8Table =
        {
            0, 213, 127, 170, 254, 43, 129, 84, 41, 252, 86, 131,
            215, 2, 168, 125, 82, 135, 45, 248, 172, 121, 211, 6, 123, 174, 4, 209,
            133, 80, 250, 47, 164, 113, 219, 14, 90, 143, 37, 240, 141, 88, 242, 39,
            115, 166, 12, 217, 246, 35, 137, 92, 8, 221, 119, 162, 223, 10, 160, 117,
            33, 244, 94, 139, 157, 72, 226, 55, 99, 182, 28, 201, 180, 97, 203, 30,
            74, 159, 53, 224, 207, 26, 176, 101, 49, 228, 78, 155, 230, 51, 153, 76,
            24, 205, 103, 178, 57, 236, 70, 147, 199, 18, 184, 109, 16, 197, 111, 186,
            238, 59, 145, 68, 107, 190, 20, 193, 149, 64, 234, 63, 66, 151, 61, 232,
            188, 105, 195, 22, 239, 58, 144, 69, 17, 196, 110, 187, 198, 19, 185, 108,
            56, 237, 71, 146, 189, 104, 194, 23, 67, 150, 60, 233, 148, 65, 235, 62,
            106, 191, 21, 192, 75, 158, 52, 225, 181, 96, 202, 31, 98, 183, 29, 200,
            156, 73, 227, 54, 25, 204, 102, 179, 231, 50, 152, 77, 48, 229, 79, 154,
            206, 27, 177, 100, 114, 167, 13, 216, 140, 89, 243, 38, 91, 142, 36, 241,
            165, 112, 218, 15, 32, 245, 95, 138, 222, 11, 161, 116, 9, 220, 118, 163,
            247, 34, 136, 93, 214, 3, 169, 124, 40, 253, 87, 130, 255, 42, 128, 85, 1,
            212, 126, 171, 132, 81, 251, 46, 122, 175, 5, 208, 173, 120, 210, 7, 83, 134, 44, 249
        };
        buffer[len] = 0;
        var parityBits = buffer[0];
        for (m = 0; m < len; m++) parityBits = (byte)(buffer[m + 1] ^ crc8Table[parityBits]);
        buffer[len] = parityBits;
    }

    /// <summary>
    ///     解码
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="len"></param>
    /// <returns></returns>
    public static byte Decode(byte[] buffer, short len)
    {
        short m;
        byte[] crc8Table =
        {
            0, 213, 127, 170, 254, 43, 129, 84, 41, 252, 86, 131, 215, 2, 168,
            125, 82, 135, 45, 248, 172, 121, 211, 6, 123, 174, 4, 209, 133, 80, 250, 47, 164,
            113, 219, 14, 90, 143, 37, 240, 141, 88, 242, 39, 115, 166, 12, 217, 246, 35, 137,
            92, 8, 221, 119, 162, 223, 10, 160, 117, 33, 244, 94, 139, 157, 72, 226, 55, 99,
            182, 28, 201, 180, 97, 203, 30, 74, 159, 53, 224, 207, 26, 176, 101, 49, 228, 78,
            155, 230, 51, 153, 76, 24, 205, 103, 178, 57, 236, 70, 147, 199, 18, 184, 109, 16,
            197, 111, 186, 238, 59, 145, 68, 107, 190, 20, 193, 149, 64, 234, 63, 66, 151, 61,
            232, 188, 105, 195, 22, 239, 58, 144, 69, 17, 196, 110, 187, 198, 19, 185, 108, 56,
            237, 71, 146, 189, 104, 194, 23, 67, 150, 60, 233, 148, 65, 235, 62, 106, 191, 21,
            192, 75, 158, 52, 225, 181, 96, 202, 31, 98, 183, 29, 200, 156, 73, 227, 54, 25,
            204, 102, 179, 231, 50, 152, 77, 48, 229, 79, 154, 206, 27, 177, 100, 114, 167,
            13, 216, 140, 89, 243, 38, 91, 142, 36, 241, 165, 112, 218, 15, 32, 245, 95, 138,
            222, 11, 161, 116, 9, 220, 118, 163, 247, 34, 136, 93, 214, 3, 169, 124, 40, 253,
            87, 130, 255, 42, 128, 85, 1, 212, 126, 171, 132, 81, 251, 46, 122, 175, 5, 208,
            173, 120, 210, 7, 83, 134, 44, 249
        };
        var parityBits = buffer[0];
        for (m = 1; m < len; m++) parityBits = (byte)(buffer[m] ^ crc8Table[parityBits]);
        if (len == 18)
        {
            if ((parityBits ^ CrcMaskSlotdata) == 0)
                return PacketSlot;
            if ((parityBits ^ CrcMaskId) == 0)
                return PacketId;
            if ((parityBits ^ CrcMaskBcchRssi) == 0)
                return PacketBcchRssi;
            if ((parityBits ^ CrcMaskSdcchRssi) == 0)
                return PacketSdcchRssi;
            if ((parityBits ^ CrcMaskTchRssi) == 0)
                return PacketTchRssi;
            if ((parityBits ^ CrcMaskBcchArfcn) == 0)
                return PacketBcchArfcn;
            if ((parityBits ^ CrcMaskHeartBeat) == 0)
                return PacketHeartBeat;
            return 0;
        }

        if (parityBits == 0x0)
            return 1;
        return 0;
    }
}