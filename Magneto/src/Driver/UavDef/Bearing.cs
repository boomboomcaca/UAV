using System;

namespace Magneto.Driver.UavDef;

public abstract class Bearing
{
    private const double Pi = Math.PI;

    private static readonly Func<double, double> _rad = Radians;
    private static readonly Func<double, double> _abs = Math.Abs;
    private static readonly Func<double, double> _sin = Math.Sin;
    private static readonly Func<double, double> _cos = Math.Cos;
    private static readonly Func<double, double, double> _atan2 = Math.Atan2;

    /// <summary>
    ///     Convert degrees to radians
    /// </summary>
    /// <param name="x">Degrees</param>
    /// <returns>The equivalent in radians</returns>
    private static double Radians(double x)
    {
        return x * Pi / 180;
    }

    /// <summary>
    ///     Calculate the bearing between two points using spherical laws(Spherical law of sines and cosines).
    /// </summary>
    /// <param name="lat1"></param>
    /// <param name="lon1"></param>
    /// <param name="lat2"></param>
    /// <param name="lon2"></param>
    /// <returns>The bearing in degrees.</returns>
    public static double Complex(double lat1, double lon1, double lat2, double lon2)
    {
        var numerator = _sin(_rad(lon2 - lon1)) * _cos(_rad(lat2));
        var denominator = _cos(_rad(lat1)) * _sin(_rad(lat2))
                          - _sin(_rad(lat1)) * _cos(_rad(lat2)) * _cos(_rad(lon2 - lon1));

        var x = _atan2(_abs(numerator), _abs(denominator));
        double result;

        if (lon2 > lon1) // right quadrant
        {
            if (lat2 > lat1) // first quadrant
                result = x;
            else if (lat2 < lat1) // forth quadrant
                result = Pi - x;
            else
                result = Pi / 2; // in positive-x axis
        }
        else if (lon2 < lon1) // left quadrant
        {
            if (lat2 > lat1) // second quadrant
                result = 2 * Pi - x;
            else if (lat2 < lat1) // third quadrant
                result = Pi + x;
            else
                result = Pi * 3 / 2; // in negative-x axis
        }
        else // same longitude
        {
            if (lat2 > lat1) // in positive-y axis
                result = 0;
            else if (lat2 < lat1)
                result = Pi; // in negative-y axis
            else
                throw new Exception("Shouldn't be same location!");
        }

        return result * 180 / Pi;
    }
}