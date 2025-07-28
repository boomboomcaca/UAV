//----------------------------------------------------------------------------------------
//	Copyright © 2006 - 2020 Tangible Software Solutions, Inc.
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class provides the ability to initialize and delete array elements.
//----------------------------------------------------------------------------------------

using System;

namespace Magneto.Device.Nvt.SDK;

internal static class Arrays
{
    public static T[] InitializeWithDefaultInstances<T>(int length) where T : new()
    {
        var array = new T[length];
        for (var i = 0; i < length; i++) array[i] = new T();
        return array;
    }

    public static void DeleteArray<T>(T[] array) where T : IDisposable
    {
        foreach (var element in array)
            if (element != null)
                element.Dispose();
    }
}