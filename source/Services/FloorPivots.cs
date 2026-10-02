#nullable enable

namespace PVRSAIndicator
{
    /// <summary>Classic floor pivots from the PREVIOUS COMPLETED daily H/L/C</summary>
    public static class FloorPivots
    {
        public static FloorPivotLevels Compute(double dayHigh, double dayLow, double dayClose)
        {
            double pp = (dayHigh + dayLow + dayClose) / 3.0;
            double r1 = 2.0 * pp - dayLow;
            double s1 = 2.0 * pp - dayHigh;
            double r2 = pp - s1 + r1;
            double s2 = pp - r1 + s1;
            double r3 = 2.0 * pp + dayHigh - 2.0 * dayLow;
            double s3 = 2.0 * pp - (2.0 * dayHigh - dayLow);
            return new FloorPivotLevels(
                pp, r1, s1, r2, s2, r3, s3,
                (s2 + s3) / 2.0,
                (s1 + s2) / 2.0,
                (pp + s1) / 2.0,
                (pp + r1) / 2.0,
                (r1 + r2) / 2.0,
                (r2 + r3) / 2.0);
        }
    }
}
