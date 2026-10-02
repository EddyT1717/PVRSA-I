#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;

namespace PVRSAIndicator
{
    /// <summary>
    /// calcPvsra
    /// Lookback = 10 prior bars (exclude current). Thresholds 200% and 150%.
    /// Doji (close <= open) uses the bear/down color — close > open only for bull.
    /// </summary>
    public static class PvsraCalculator
    {
        public const int Lookback = 10;

        public static PvsraResult Classify(
            double volume,
            double high,
            double low,
            double close,
            double open,
            IReadOnlyList<double> priorVolumes,
            IReadOnlyList<double> priorHighs,
            IReadOnlyList<double> priorLows,
            Color red,
            Color green,
            Color violet,
            Color blue,
            Color darkGrey,
            Color lightGrey)
        {
            int counted = 0;
            double volSum = 0.0;
            double highestVs = 0.0;
            int n = priorVolumes.Count;
            int limit = Lookback;
            if (n < limit) limit = n;
            for (int i = 0; i < limit; i++)
            {
                double v = priorVolumes[i];
                double h = i < priorHighs.Count ? priorHighs[i] : 0.0;
                double l = i < priorLows.Count ? priorLows[i] : 0.0;
                volSum += v;
                double vs = v * (h - l);
                if (vs > highestVs) highestVs = vs;
                counted++;
            }

            double avgVol = counted > 0 ? volSum / counted : double.NaN;
            double volSpread = volume * (high - low);
            bool isBull = close > open;

            if (counted < Lookback)
            {
                Color regular = isBull ? lightGrey : darkGrey;
                return new PvsraResult(regular, false, avgVol, volSpread, highestVs, PvsraFlag.None);
            }

            if (volume >= 2.0 * avgVol || volSpread >= highestVs)
            {
                Color c = isBull ? green : red;
                int flag = isBull ? PvsraFlag.GreenClimax : PvsraFlag.RedClimax;
                return new PvsraResult(c, true, avgVol, volSpread, highestVs, flag);
            }

            if (volume >= 1.5 * avgVol)
            {
                Color c = isBull ? blue : violet;
                int flag = isBull ? PvsraFlag.BlueRising : PvsraFlag.VioletRising;
                return new PvsraResult(c, true, avgVol, volSpread, highestVs, flag);
            }

            Color gray = isBull ? lightGrey : darkGrey;
            return new PvsraResult(gray, false, avgVol, volSpread, highestVs, PvsraFlag.None);
        }

        /// <summary>Exact ints UNKNOWN; mapping is the spec suggestion. Regular gray → 0.</summary>
        public static int GetPvsraFlagByColor(Color color, Color red, Color green, Color violet, Color blue)
        {
            if (PineColor.Eq(color, green)) return PvsraFlag.GreenClimax;
            if (PineColor.Eq(color, red)) return PvsraFlag.RedClimax;
            if (PineColor.Eq(color, blue)) return PvsraFlag.BlueRising;
            if (PineColor.Eq(color, violet)) return PvsraFlag.VioletRising;
            return PvsraFlag.None;
        }

        public static PatternFlags DetectPatterns(Color color, Color prev, bool hasPrev, Color red, Color green, Color violet, Color blue)
        {
            PatternFlags p = new PatternFlags();
            if (!hasPrev) return p;
            p.RedGreen = PineColor.Eq(color, green) && PineColor.Eq(prev, red);
            p.GreenRed = PineColor.Eq(color, red) && PineColor.Eq(prev, green);
            p.RedBlue = PineColor.Eq(color, blue) && PineColor.Eq(prev, red);
            p.BlueRed = PineColor.Eq(color, red) && PineColor.Eq(prev, blue);
            p.GreenPurple = PineColor.Eq(color, violet) && PineColor.Eq(prev, green);
            p.PurpleGreen = PineColor.Eq(color, green) && PineColor.Eq(prev, violet);
            p.BluePurple = PineColor.Eq(color, violet) && PineColor.Eq(prev, blue);
            p.PurpleBlue = PineColor.Eq(color, blue) && PineColor.Eq(prev, violet);
            return p;
        }
    }
}
