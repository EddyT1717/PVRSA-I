#nullable enable
using System;
using System.Collections.Generic;

namespace PVRSAIndicator
{
    /// <summary>
    /// calcPsyLevels.
    /// 8-hour window on a 4H grid. Label strings UNKNOWN; use "Psy Hi" / "Psy Lo".
    /// Crypto: Saturday 22:00 UTC (21:00 if Sydney DST). Forex: Monday 00:00 UTC.
    /// Gate TF in {1,3,5,15,30,60} min is applied by the indicator, not here.
    /// </summary>
    public static class PsyLevelCalculator
    {
        public static readonly int[] AllowedMinutes = new int[] { 1, 3, 5, 15, 30, 60 };

        public static bool IsAllowedTf(double tfMinutes, bool isMinuteTf)
        {
            if (!isMinuteTf) return false;
            int m = (int)Math.Round(tfMinutes);
            for (int i = 0; i < AllowedMinutes.Length; i++)
            {
                if (AllowedMinutes[i] == m) return true;
            }
            return false;
        }

        public static DateTime CryptoPsyStart(DateTime nowUtc, bool sydDst)
        {
            int hourUtc = sydDst ? 21 : 22;
            DateTime u = EnsureUtc(nowUtc);
            int dow = (int)u.DayOfWeek; // 0 Sun .. 6 Sat
            int daysFromSat = (dow + 1) % 7; // sat=0, sun=1, ... fri=6
            DateTime start = new DateTime(u.Year, u.Month, u.Day, hourUtc, 0, 0, DateTimeKind.Utc).AddDays(-daysFromSat);
            if (u < start) start = start.AddDays(-7);
            return start;
        }

        public static DateTime ForexPsyStart(DateTime nowUtc)
        {
            DateTime u = EnsureUtc(nowUtc);
            int dow = (int)u.DayOfWeek;
            int daysFromMon = (dow + 6) % 7; // mon=0, sun=6
            DateTime start = new DateTime(u.Year, u.Month, u.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(-daysFromMon);
            if (u < start) start = start.AddDays(-7);
            return start;
        }

        public static PsyLevels Compute(DateTime nowUtc, PsyType psyType, bool sydDst, IReadOnlyList<HtfBar> windowBars, TimeSpan barDuration)
        {
            DateTime sessionStart = psyType == PsyType.Crypto
                ? CryptoPsyStart(nowUtc, sydDst)
                : ForexPsyStart(nowUtc);
            DateTime windowEnd = sessionStart.AddHours(8);

            double hi = double.NegativeInfinity;
            double lo = double.PositiveInfinity;
            TimeSpan dur = barDuration;
            if (dur.Ticks <= 0) dur = TimeSpan.FromHours(4);

            int n = windowBars.Count;
            for (int i = 0; i < n; i++)
            {
                HtfBar b = windowBars[i];
                DateTime t = b.Time.Kind == DateTimeKind.Utc ? b.Time : DateTime.SpecifyKind(b.Time, DateTimeKind.Utc);
                // Bar intersects [sessionStart, windowEnd)
                if (t >= windowEnd) continue;
                if (t + dur > sessionStart && t < windowEnd)
                {
                    if (b.High > hi) hi = b.High;
                    if (b.Low < lo) lo = b.Low;
                }
            }

            if (double.IsInfinity(hi) || double.IsInfinity(lo) || double.IsNaN(hi) || double.IsNaN(lo))
            {
                hi = double.NaN;
                lo = double.NaN;
            }

            return new PsyLevels(hi, lo, "Psy Hi", "Psy Lo", sessionStart);
        }

        private static DateTime EnsureUtc(DateTime t)
        {
            if (t.Kind == DateTimeKind.Utc) return t;
            if (t.Kind == DateTimeKind.Local) return t.ToUniversalTime();
            return DateTime.SpecifyKind(t, DateTimeKind.Utc);
        }
    }
}
