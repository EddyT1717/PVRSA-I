#nullable enable
using System;

namespace PVRSAIndicator
{
    /// <summary>
    ///
    /// Evaluated on the current calendar date in UTC.
    /// UK: last Sunday of March through the day before last Sunday of October.
    /// NY: 2nd Sunday of March through the day before 1st Sunday of November.
    /// Syd: 1st Sunday of October OR before 1st Sunday of April (southern hemisphere).
    /// </summary>
    public static class DstCalendar
    {
        public static DstFlags GetFlags(DateTime utc)
        {
            DateTime u = utc.Kind == DateTimeKind.Utc ? utc : utc.ToUniversalTime();
            int y = u.Year;
            int m = u.Month;
            int d = u.Day;

            DateTime lastSunMar = LastSundayOfMonth(y, 3);
            DateTime lastSunOct = LastSundayOfMonth(y, 10);
            DateTime secondSunMar = NthSundayOfMonth(y, 3, 2);
            DateTime firstSunNov = NthSundayOfMonth(y, 11, 1);
            DateTime firstSunOct = NthSundayOfMonth(y, 10, 1);
            DateTime firstSunApr = NthSundayOfMonth(y, 4, 1);

            bool ukDst = CmpYmd(y, m, d, lastSunMar) >= 0 && CmpYmd(y, m, d, lastSunOct) < 0;
            bool nyDst = CmpYmd(y, m, d, secondSunMar) >= 0 && CmpYmd(y, m, d, firstSunNov) < 0;
            bool sydDst = CmpYmd(y, m, d, firstSunOct) >= 0 || CmpYmd(y, m, d, firstSunApr) < 0;
            return new DstFlags(nyDst, ukDst, sydDst);
        }

        /// <summary>n is 1-based. month is 1–12.</summary>
        public static DateTime NthSundayOfMonth(int year, int month, int n)
        {
            DateTime first = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            int dow = (int)first.DayOfWeek; // 0 = Sunday
            int firstSunday = dow == 0 ? 1 : 8 - dow;
            return new DateTime(year, month, firstSunday + (n - 1) * 7, 0, 0, 0, DateTimeKind.Utc);
        }

        public static DateTime LastSundayOfMonth(int year, int month)
        {
            int days = DateTime.DaysInMonth(year, month);
            DateTime last = new DateTime(year, month, days, 0, 0, 0, DateTimeKind.Utc);
            int dow = (int)last.DayOfWeek;
            return last.AddDays(-dow);
        }

        private static int CmpYmd(int y, int m, int d, DateTime b)
        {
            if (y != b.Year) return y - b.Year;
            if (m != b.Month) return m - b.Month;
            return d - b.Day;
        }

        /// <summary>
        /// DST table text. Spec §11: source typo “Arpil” — display corrected “April”.
        /// </summary>
        public static readonly string[] TableText = new string[]
        {
            "London DST Starts Last Sunday of March | DST Ends Last Sunday of October",
            "New York DST Starts 2nd Sunday of March | DST Ends 1st Sunday of November",
            "Tokyo does not observe DST",
            "Hong Kong does not observe DST",
            "Sydney DST Start on 1st Sunday of October | DST Ends 1st Sunday of April",
            "EU Brinks DST Starts Last Sunday of March | DST Ends Last Sunday of October",
            "US Brinks DST Starts 2nd Sunday of March | DST Ends 1st Sunday of November",
            "Frankfurt DST Starts Last Sunday of March | DST Ends Last Sunday of October"
        };
    }
}
