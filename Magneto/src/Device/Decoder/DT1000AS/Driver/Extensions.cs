namespace Magneto.Device.DT1000AS.Driver;

internal static class Extensions
{
    public static int ToInt32(this bool value)
    {
        return value ? 1 : 0;
    }

    public static void FromArray<T>(this T[,] original, T[] input, int x, int y)
    {
        var length = input.Length;
        original.FromArray(input, x, y, length);
    }

    public static void FromArray<T>(this T[,] original, T[] input, int x, int y, int length)
    {
        for (var i = 0; i < original.GetLength(0); i++)
            if (i == x)
            {
                var index = 0;
                for (var j = y; j < original.GetLength(1) && j < y + length; j++)
                {
                    original[i, j] = input[index];
                    index++;
                }

                break;
            }
    }

    public static T[] ToArray<T>(this T[,] original, int x, int y)
    {
        var len = original.GetLength(1) - y;
        var buffer = new T[len];
        for (var i = 0; i < original.GetLength(0); i++)
            if (i == x)
            {
                var index = 0;
                for (var j = y; j < original.GetLength(1); j++)
                {
                    buffer[index] = original[i, j];
                    index++;
                }

                break;
            }

        return buffer;
    }

    public static sbyte[] ToSByteArray(this byte[] bts)
    {
        var result = new sbyte[bts.Length];
        for (var i = 0; i < bts.Length; i++)
            if (bts[i] > 127)
                result[i] = (sbyte)(bts[i] - 256);
            else
                result[i] = (sbyte)bts[i];
        return result;
    }
}