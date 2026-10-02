#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;

namespace PVRSAIndicator
{
    /// <summary>
    /// Session clocks are DST-off in UTC+0, then drawn in GMT+0 or GMT+1
    /// depending on region DST. GMT+1 means UTC = clock − 1 hour.
    /// Tokyo / Hong Kong: never DST. Frankfurt default OFF is a settings concern.
    /// RECONSTRUCTED from spec Phase 7.
    /// </summary>
    public static class SessionClock
    {
        public sealed class SessionDef
        {
            public SessionId Id;
            public string Clock = "";
            public SessionDstRegion Dst;
        }

        public static readonly SessionDef[] Defs = new SessionDef[]
        {
            new SessionDef { Id = SessionId.London, Clock = "0800-1630", Dst = SessionDstRegion.Uk },
            new SessionDef { Id = SessionId.NewYork, Clock = "1430-2100", Dst = SessionDstRegion.Ny },
            new SessionDef { Id = SessionId.Tokyo, Clock = "0000-0600", Dst = SessionDstRegion.None },
            new SessionDef { Id = SessionId.HongKong, Clock = "0130-0800", Dst = SessionDstRegion.None },
            new SessionDef { Id = SessionId.Sydney, Clock = "2200-0600", Dst = SessionDstRegion.Syd },
            new SessionDef { Id = SessionId.EuBrinks, Clock = "0800-0900", Dst = SessionDstRegion.Uk },
            new SessionDef { Id = SessionId.UsBrinks, Clock = "1400-1500", Dst = SessionDstRegion.Ny },
            new SessionDef { Id = SessionId.Frankfurt, Clock = "0700-1630", Dst = SessionDstRegion.Uk }
        };

        public static void ParseClock(string clock, out int startMin, out int endMin)
        {
            int dash = clock.IndexOf('-');
            string a = dash >= 0 ? clock.Substring(0, dash) : clock;
            string b = dash >= 0 ? clock.Substring(dash + 1) : clock;
            startMin = ParseHhmm(a);
            endMin = ParseHhmm(b);
        }

        private static int ParseHhmm(string s)
        {
            if (s.Length < 4) return 0;
            int hh = (s[0] - '0') * 10 + (s[1] - '0');
            int mm = (s[2] - '0') * 10 + (s[3] - '0');
            return hh * 60 + mm;
        }

        public static bool DstOn(SessionDef def, DstFlags flags)
        {
            if (def.Dst == SessionDstRegion.Uk) return flags.UkDst;
            if (def.Dst == SessionDstRegion.Ny) return flags.NyDst;
            if (def.Dst == SessionDstRegion.Syd) return flags.SydDst;
            return false;
        }

        /// <summary>DST-off clocks in UTC+0; when DST on, UTC = clock − 1h.</summary>
        public static void SessionUtcMinutes(SessionDef def, DstFlags flags, out int startMin, out int endMin)
        {
            ParseClock(def.Clock, out startMin, out endMin);
            int shift = DstOn(def, flags) ? 60 : 0;
            int day = 24 * 60;
            startMin = (startMin - shift + day) % day;
            endMin = (endMin - shift + day) % day;
        }

        /// <summary>Latest session occurrence at or before <paramref name="now"/> (and the one that may still be running).</summary>
        public static SessionInterval GetInterval(SessionId id, DstFlags flags, DateTime now)
        {
            SessionDef def = Defs[(int)id];
            return GetInterval(def, flags, now);
        }

        public static SessionInterval GetInterval(SessionDef def, DstFlags flags, DateTime now)
        {
            DateTime utc = now.Kind == DateTimeKind.Local ? now.ToUniversalTime() : DateTime.SpecifyKind(now, DateTimeKind.Utc);
            SessionUtcMinutes(def, flags, out int startMin, out int endMin);
            bool wraps = endMin <= startMin;
            DateTime day0 = new DateTime(utc.Year, utc.Month, utc.Day, 0, 0, 0, DateTimeKind.Utc);

            SessionInterval containing = default;
            bool hasContaining = false;
            SessionInterval latestPast = default;
            bool hasPast = false;

            for (int dayOffset = -2; dayOffset <= 1; dayOffset++)
            {
                DateTime start = day0.AddDays(dayOffset).AddMinutes(startMin);
                DateTime end = wraps
                    ? day0.AddDays(dayOffset + 1).AddMinutes(endMin)
                    : day0.AddDays(dayOffset).AddMinutes(endMin);
                SessionInterval iv = new SessionInterval(start, end);
                if (utc >= start && utc < end)
                {
                    containing = iv;
                    hasContaining = true;
                }
                if (start <= utc)
                {
                    if (!hasPast || start > latestPast.Start)
                    {
                        latestPast = iv;
                        hasPast = true;
                    }
                }
            }

            if (hasContaining) return containing;
            if (hasPast) return latestPast;
            DateTime fbStart = day0.AddMinutes(startMin);
            DateTime fbEnd = wraps ? day0.AddDays(1).AddMinutes(endMin) : day0.AddMinutes(endMin);
            return new SessionInterval(fbStart, fbEnd);
        }

        public static void SessionHighLow(HistoricalBarsView bars, DateTime start, DateTime end, out double high, out double low)
        {
            high = double.NegativeInfinity;
            low = double.PositiveInfinity;
            int count = bars.Count;
            for (int i = 0; i < count; i++)
            {
                DateTime t = bars.TimeAt(i);
                if (t >= end) continue;
                if (t >= start && t < end)
                {
                    double h = bars.HighAt(i);
                    double l = bars.LowAt(i);
                    if (h > high) high = h;
                    if (l < low) low = l;
                }
            }
            if (!IsFinite(high) || !IsFinite(low))
            {
                high = double.NaN;
                low = double.NaN;
            }
        }

        private static bool IsFinite(double x)
        {
            return !double.IsNaN(x) && !double.IsInfinity(x);
        }

        private static bool WeekdayAllowed(DateTime start, bool weekends)
        {
            if (weekends) return true;
            DayOfWeek d = start.DayOfWeek;
            return d != DayOfWeek.Saturday && d != DayOfWeek.Sunday;
        }

        public static List<SessionDraw> BuildCurrentSessions(
            HistoricalBarsView bars,
            DateTime now,
            DstFlags flags,
            bool showGate,
            bool showMarkets,
            bool showWeekends,
            SessionItem[] items)
        {
            List<SessionDraw> outList = new List<SessionDraw>();
            if (!showGate || !showMarkets) return outList;
            for (int i = 0; i < Defs.Length; i++)
            {
                SessionDef def = Defs[i];
                SessionItem item = items[i];
                if (!item.ShowSession) continue;
                SessionInterval iv = GetInterval(def, flags, now);
                if (!WeekdayAllowed(iv.Start, showWeekends)) continue;
                SessionHighLow(bars, iv.Start, iv.End, out double hi, out double lo);
                SessionDraw draw = new SessionDraw
                {
                    Id = def.Id,
                    Name = item.Name,
                    Start = iv.Start,
                    End = iv.End,
                    High = hi,
                    Low = lo,
                    BoxColor = item.BoxColor,
                    LabelColor = item.LabelColor,
                    ShowBox = item.ShowOpeningRange,
                    ShowLines = true,
                    ShowLabel = item.ShowLabel
                };
                outList.Add(draw);
            }
            return outList;
        }
    }

    /// <summary>Per-session input bundle (flattened in the indicator, packed here for the clock).</summary>
    public struct SessionItem
    {
        public bool ShowSession;
        public bool ShowLabel;
        public bool ShowOpeningRange;
        public string Name;
        public Color BoxColor;
        public Color LabelColor;
    }

    /// <summary>
    /// Thin view over chart bars oldest-first. Implemented by the indicator so SessionClock
    /// does not call Quantower APIs directly.
    /// </summary>
    public abstract class HistoricalBarsView
    {
        public abstract int Count { get; }
        public abstract DateTime TimeAt(int oldestFirstIndex);
        public abstract double HighAt(int oldestFirstIndex);
        public abstract double LowAt(int oldestFirstIndex);
    }
}
