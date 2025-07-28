//----------------------------------------------------------------------------------------
//	Copyright © 2006 - 2020 Tangible Software Solutions, Inc.
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class includes methods to convert C++ rectangular arrays (jagged arrays
//	with inner arrays of the same length).
//----------------------------------------------------------------------------------------

namespace Magneto.Device.Nvt.SDK;

internal static class RectangularArrays
{
    public static char[][] RectangularCharArray(int size1, int size2)
    {
        var newArray = new char[size1][];
        for (var array1 = 0; array1 < size1; array1++) newArray[array1] = new char[size2];
        return newArray;
    }

    public static TmSchedTimeT[][] RectangularTmSchedTime_tArray(int size1, int size2)
    {
        var newArray = new TmSchedTimeT[size1][];
        for (var array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new TmSchedTimeT[size2];
            for (var array2 = 0; array2 < size2; array2++) newArray[array1][array2] = new TmSchedTimeT();
        }

        return newArray;
    }

    public static TmPtzAuxTimeT[][] RectangularTmPtzAuxTime_tArray(int size1, int size2)
    {
        var newArray = new TmPtzAuxTimeT[size1][];
        for (var array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new TmPtzAuxTimeT[size2];
            for (var array2 = 0; array2 < size2; array2++) newArray[array1][array2] = new TmPtzAuxTimeT();
        }

        return newArray;
    }

    public static TmPtzSchedTimeT[][] RectangularTmPtzSchedTime_tArray(int size1, int size2)
    {
        var newArray = new TmPtzSchedTimeT[size1][];
        for (var array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new TmPtzSchedTimeT[size2];
            for (var array2 = 0; array2 < size2; array2++) newArray[array1][array2] = new TmPtzSchedTimeT();
        }

        return newArray;
    }

    public static byte[][] RectangularByteArray(int size1, int size2)
    {
        var newArray = new byte[size1][];
        for (var array1 = 0; array1 < size1; array1++) newArray[array1] = new byte[size2];
        return newArray;
    }

    public static TmRecordSchedT[][] RectangularTmRecordSched_tArray(int size1, int size2)
    {
        var newArray = new TmRecordSchedT[size1][];
        for (var array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new TmRecordSchedT[size2];
            for (var array2 = 0; array2 < size2; array2++) newArray[array1][array2] = new TmRecordSchedT();
        }

        return newArray;
    }
}