#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;

namespace PVRSAIndicator
{
    /// <summary>
    /// Vector candle zones.
    /// All four vector colors create zones; regular does not.
    ///
    ///   Above (dir=1): if clearTop enters from below, box.bottom = max(box.bottom, clearTop)
    ///   Below (dir=0): if clearBot enters from above, box.top = min(box.top, clearBot)
    /// Delete when empty. Cap zonesMax per side.
    /// The creating bar does not mitigate its own box (otherwise Body-only + wick-clear
    /// would immediately delete every new zone).
    /// </summary>
    public sealed class VectorZoneEngine
    {
        private int _nextId = 1;
        public readonly List<ZoneBox> Above = new List<ZoneBox>();
        public readonly List<ZoneBox> Below = new List<ZoneBox>();

        public void Reset()
        {
            Above.Clear();
            Below.Clear();
            _nextId = 1;
        }

        public static void ZoneBounds(double open, double high, double low, double close, ZoneGeometry type, out double top, out double bot)
        {
            if (type == ZoneGeometry.BodyOnly)
            {
                top = open > close ? open : close;
                bot = open < close ? open : close;
            }
            else
            {
                top = high;
                bot = low;
            }
        }

        public void Update(
            int flag,
            double open,
            double high,
            double low,
            double close,
            DateTime barTime,
            bool showVcz,
            int zonesMax,
            ZoneGeometry zoneType,
            ZoneGeometry zoneUpdateType,
            bool colorOverride,
            Color zoneColor,
            int transperancy,
            Color pvsraColor)
        {
            if (!showVcz) return;
            int max = zonesMax < 1 ? 1 : zonesMax;

            UpdateOneSide(1, flag, open, high, low, close, barTime, max, zoneType, zoneUpdateType, colorOverride, zoneColor, transperancy, pvsraColor, Above);
            UpdateOneSide(0, flag, open, high, low, close, barTime, max, zoneType, zoneUpdateType, colorOverride, zoneColor, transperancy, pvsraColor, Below);
            Cleanarr(Above);
            Cleanarr(Below);
        }

        /// <summary>
        /// On a forming-bar tick: re-mitigate existing boxes, and create at most one box per
        /// bar per side (replace the in-progress box geometry if this bar is still a vector).
        /// </summary>
        public void UpdateForming(
            int flag,
            double open,
            double high,
            double low,
            double close,
            DateTime barTime,
            bool showVcz,
            int zonesMax,
            ZoneGeometry zoneType,
            ZoneGeometry zoneUpdateType,
            bool colorOverride,
            Color zoneColor,
            int transperancy,
            Color pvsraColor)
        {
            if (!showVcz) return;
            RemoveBoxesStartingAt(Above, barTime);
            RemoveBoxesStartingAt(Below, barTime);
            Update(flag, open, high, low, close, barTime, showVcz, zonesMax, zoneType, zoneUpdateType, colorOverride, zoneColor, transperancy, pvsraColor);
        }

        private void RemoveBoxesStartingAt(List<ZoneBox> list, DateTime barTime)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].StartTime == barTime) list.RemoveAt(i);
            }
        }

        private void UpdateOneSide(
            int direction,
            int flag,
            double open,
            double high,
            double low,
            double close,
            DateTime barTime,
            int max,
            ZoneGeometry zoneType,
            ZoneGeometry zoneUpdateType,
            bool colorOverride,
            Color zoneColor,
            int transperancy,
            Color pvsraColor,
            List<ZoneBox> boxArr)
        {
            if (Math.Abs(flag) >= 1)
            {
                ZoneBounds(open, high, low, close, zoneType, out double createdTop, out double createdBot);
                double mid = (createdTop + createdBot) / 2.0;
                bool goesAbove = direction == 1;
                bool belongs = goesAbove ? mid >= close : mid < close;
                if (belongs)
                {
                    Color color = colorOverride ? zoneColor : PineColor.ApplyTransp(pvsraColor, transperancy);
                    ZoneBox box = new ZoneBox
                    {
                        Id = _nextId++,
                        Direction = direction,
                        Top = createdTop,
                        Bot = createdBot,
                        StartTime = barTime,
                        EndTime = barTime,
                        Color = color,
                        Flag = flag
                    };
                    boxArr.Add(box);
                    while (boxArr.Count > max) boxArr.RemoveAt(0);
                }
            }

            ZoneBounds(open, high, low, close, zoneUpdateType, out double clearTop, out double clearBot);
            for (int i = boxArr.Count - 1; i >= 0; i--)
            {
                ZoneBox box = boxArr[i];
                box.EndTime = barTime;
                // RECONSTRUCTED: skip self-mitigation on the creating bar.
                if (box.StartTime == barTime) continue;

                if (direction == 1)
                {
                    if (clearTop > box.Bot && clearTop < box.Top + 1e-12)
                        box.Bot = box.Bot > clearTop ? box.Bot : clearTop;
                    if (clearTop >= box.Top)
                    {
                        boxArr.RemoveAt(i);
                        continue;
                    }
                }
                else
                {
                    if (clearBot < box.Top && clearBot > box.Bot - 1e-12)
                        box.Top = box.Top < clearBot ? box.Top : clearBot;
                    if (clearBot <= box.Bot)
                    {
                        boxArr.RemoveAt(i);
                        continue;
                    }
                }

                if (box.Top <= box.Bot) boxArr.RemoveAt(i);
            }
        }

        public static bool Cleanarr(List<ZoneBox> arr)
        {
            int before = arr.Count;
            for (int i = arr.Count - 1; i >= 0; i--)
            {
                ZoneBox z = arr[i];
                if (z == null || !IsFinite(z.Top) || !IsFinite(z.Bot) || z.Top <= z.Bot)
                    arr.RemoveAt(i);
            }
            return arr.Count != before;
        }

        private static bool IsFinite(double x)
        {
            return !double.IsNaN(x) && !double.IsInfinity(x);
        }
    }
}
