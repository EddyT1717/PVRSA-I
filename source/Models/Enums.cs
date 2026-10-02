#nullable enable
using System;
using System.Drawing;
using TradingPlatform.BusinessLayer;

namespace PVRSAIndicator
{
    /// <summary>Zone box geometry at creation / mitigation.</summary>
    public enum ZoneGeometry
    {
        BodyOnly = 0,
        BodyWithWicks = 1
    }

    /// <summary>Weekly Psy calculation type (spec Phase 9).</summary>
    public enum PsyType
    {
        Crypto = 0,
        Forex = 1
    }

    /// <summary>Corner / center placement for ADR and DST overlay tables.</summary>
    public enum TablePosition
    {
        TopLeft = 0,
        TopCenter = 1,
        TopRight = 2,
        BottomLeft = 3,
        BottomCenter = 4,
        BottomRight = 5
    }

    /// <summary>Quantower-only (spec §8). Default OnBarClose.</summary>
    public enum AlertFrequency
    {
        OnBarClose = 0,
        OnEachTick = 1
    }

    /// <summary>
    /// Internal PVSRA zone flag. Exact ints UNKNOWN; mapping is the spec suggestion.
    /// 0 = none/regular, +2 green climax, -2 red climax, +1 blue rising, -1 violet rising.
    /// </summary>
    public static class PvsraFlag
    {
        public const int None = 0;
        public const int BlueRising = 1;
        public const int VioletRising = -1;
        public const int GreenClimax = 2;
        public const int RedClimax = -2;
    }

    /// <summary>Market session identifiers (clocks are hardcoded, not inputs).</summary>
    public enum SessionId
    {
        London = 0,
        NewYork = 1,
        Tokyo = 2,
        HongKong = 3,
        Sydney = 4,
        EuBrinks = 5,
        UsBrinks = 6,
        Frankfurt = 7
    }

    public enum SessionDstRegion
    {
        None = 0,
        Uk = 1,
        Ny = 2,
        Syd = 3
    }

    public readonly struct DstFlags
    {
        public readonly bool NyDst;
        public readonly bool UkDst;
        public readonly bool SydDst;

        public DstFlags(bool nyDst, bool ukDst, bool sydDst)
        {
            NyDst = nyDst;
            UkDst = ukDst;
            SydDst = sydDst;
        }
    }

    public readonly struct AdrPack
    {
        public readonly double Adr;
        public readonly double AdrLow;
        public readonly double AdrHigh;
        public readonly double Hi50;
        public readonly double Lo50;
        public readonly bool IsValid;

        public AdrPack(double adr, double adrLow, double adrHigh)
        {
            Adr = adr;
            AdrLow = adrLow;
            AdrHigh = adrHigh;
            Hi50 = adrHigh - adr / 2.0;
            Lo50 = adrLow + adr / 2.0;
            IsValid = !double.IsNaN(adr) && !double.IsInfinity(adr);
        }

        public static AdrPack Invalid => new AdrPack(double.NaN, double.NaN, double.NaN);
    }

    public readonly struct FloorPivotLevels
    {
        public readonly double Pp;
        public readonly double R1;
        public readonly double S1;
        public readonly double R2;
        public readonly double S2;
        public readonly double R3;
        public readonly double S3;
        public readonly double M0;
        public readonly double M1;
        public readonly double M2;
        public readonly double M3;
        public readonly double M4;
        public readonly double M5;
        public readonly bool IsValid;

        public FloorPivotLevels(
            double pp, double r1, double s1, double r2, double s2, double r3, double s3,
            double m0, double m1, double m2, double m3, double m4, double m5)
        {
            Pp = pp;
            R1 = r1;
            S1 = s1;
            R2 = r2;
            S2 = s2;
            R3 = r3;
            S3 = s3;
            M0 = m0;
            M1 = m1;
            M2 = m2;
            M3 = m3;
            M4 = m4;
            M5 = m5;
            IsValid = !double.IsNaN(pp);
        }

        public static FloorPivotLevels Invalid => new FloorPivotLevels(
            double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN,
            double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN);
    }

    public readonly struct HtfBar
    {
        public readonly DateTime Time;
        public readonly double Open;
        public readonly double High;
        public readonly double Low;
        public readonly double Close;
        public readonly double Volume;

        public HtfBar(DateTime time, double open, double high, double low, double close, double volume)
        {
            Time = time;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
        }
    }

    public readonly struct SessionInterval
    {
        public readonly DateTime Start;
        public readonly DateTime End;

        public SessionInterval(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }
    }

    public sealed class SessionDraw
    {
        public SessionId Id;
        public string Name = "";
        public DateTime Start;
        public DateTime End;
        public double High = double.NaN;
        public double Low = double.NaN;
        public Color BoxColor;
        public Color LabelColor;
        public bool ShowBox;
        public bool ShowLines;
        public bool ShowLabel;
    }

    public sealed class ZoneBox
    {
        public int Id;
        public int Direction;
        public double Top;
        public double Bot;
        public DateTime StartTime;
        public DateTime EndTime;
        public Color Color;
        public int Flag;
    }

    public readonly struct PsyLevels
    {
        public readonly double Hi;
        public readonly double Lo;
        public readonly string HiLabel;
        public readonly string LoLabel;
        public readonly DateTime SessionStartTime;

        public PsyLevels(double hi, double lo, string hiLabel, string loLabel, DateTime sessionStartTime)
        {
            Hi = hi;
            Lo = lo;
            HiLabel = hiLabel;
            LoLabel = loLabel;
            SessionStartTime = sessionStartTime;
        }

        public static PsyLevels Empty => new PsyLevels(double.NaN, double.NaN, "Psy Hi", "Psy Lo", DateTime.MinValue);
    }

    public readonly struct PvsraResult
    {
        public readonly Color Color;
        public readonly bool AlertFlag;
        public readonly double AvgVol;
        public readonly double VolSpread;
        public readonly double HighestVolSpread;
        public readonly int Flag;

        public PvsraResult(Color color, bool alertFlag, double avgVol, double volSpread, double highestVolSpread, int flag)
        {
            Color = color;
            AlertFlag = alertFlag;
            AvgVol = avgVol;
            VolSpread = volSpread;
            HighestVolSpread = highestVolSpread;
            Flag = flag;
        }
    }

    public struct PatternFlags
    {
        public bool RedGreen;
        public bool GreenRed;
        public bool RedBlue;
        public bool BlueRed;
        public bool GreenPurple;
        public bool PurpleGreen;
        public bool BluePurple;
        public bool PurpleBlue;
    }

    public sealed class HorizontalLevel
    {
        public string Tag = "";
        public double Price;
        public Color Color;
        public LineStyle Style = LineStyle.Solid;
        public int Width = 1;
        /// <summary>"right" | "both" | "none"</summary>
        public string Extend = "right";
        public DateTime StartTime;
        public DateTime? EndTime;
        public string? Label;
        public Color LabelColor;
        public bool ShowLabel;
    }

    public sealed class ChartLabel
    {
        public string Text = "";
        public double Price;
        public DateTime Time;
        public Color Color;
    }

    public sealed class TableRow
    {
        public string Name = "";
        public string Value = "";
    }

    /// <summary>Pine transp 0–100 → GDI alpha. alpha = 255 * (100-transp)/100.</summary>
    public static class PineColor
    {
        public static Color FromRgb(int r, int g, int b, int transp = 0)
        {
            int t = transp;
            if (t < 0) t = 0;
            if (t > 100) t = 100;
            int alpha = (int)Math.Round(255.0 * (100 - t) / 100.0);
            if (alpha < 0) alpha = 0;
            if (alpha > 255) alpha = 255;
            return Color.FromArgb(alpha, r, g, b);
        }

        public static Color ApplyTransp(Color c, int transp)
        {
            int t = transp;
            if (t < 0) t = 0;
            if (t > 100) t = 100;
            int alpha = (int)Math.Round(255.0 * (100 - t) / 100.0);
            return Color.FromArgb(alpha, c.R, c.G, c.B);
        }

        public static bool Eq(Color a, Color b)
        {
            return a.A == b.A && a.R == b.R && a.G == b.G && a.B == b.B;
        }
    }
}
