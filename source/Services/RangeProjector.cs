#nullable enable
using System;
using System.Collections.Generic;

namespace PVRSAIndicator
{
    /// <summary>
    /// adrHiLo.
    /// Average of completed period ranges; barsBack is always 1 in the main script
    /// so the average skips the current (possibly forming) HTF bar.
    /// fromDO → periodOpen ± adr; else developing low+adr / high−adr.
    /// </summary>
    public static class RangeProjector
    {
        public static AdrPack AdrHiLo(IReadOnlyList<HtfBar> periodBars, int length, int barsBack, bool fromDo)
        {
            if (periodBars == null || periodBars.Count < barsBack + length)
                return AdrPack.Invalid;

            int last = periodBars.Count - 1;
            HtfBar current = periodBars[last];

            double sum = 0.0;
            for (int i = 0; i < length; i++)
            {
                int idx = last - barsBack - i;
                if (idx < 0) return AdrPack.Invalid;
                HtfBar b = periodBars[idx];
                sum += b.High - b.Low;
            }

            double adr = sum / length;
            double adrHigh;
            double adrLow;
            if (fromDo)
            {
                adrHigh = current.Open + adr;
                adrLow = current.Open - adr;
            }
            else
            {
                // classic “from today’s developing H/L”
                adrHigh = current.Low + adr;
                adrLow = current.High - adr;
            }

            return new AdrPack(adr, adrLow, adrHigh);
        }
    }
}
