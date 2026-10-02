#nullable enable
using System;

namespace PVRSAIndicator
{
    /// <summary>
    /// toPips
    /// Safest Quantower default: priceDistance / instrument.PointSize.
    /// PointSize is Symbol.TickSize (do NOT invent a 10× pip multiplier).
    /// </summary>
    public static class PipConverter
    {
        public static double ToPips(double priceDistance, double pointSize)
        {
            if (!IsFinite(priceDistance) || !IsFinite(pointSize) || pointSize == 0.0)
                return double.NaN;
            return priceDistance / pointSize;
        }

        public static string FormatPips(double pips, int digits = 1)
        {
            if (!IsFinite(pips)) return "—";
            return pips.ToString("F" + digits);
        }

        public static string FormatPrice(double v, int digits = 5)
        {
            if (!IsFinite(v)) return "—";
            return v.ToString("F" + digits);
        }

        private static bool IsFinite(double x)
        {
            return !double.IsNaN(x) && !double.IsInfinity(x);
        }
    }
}
