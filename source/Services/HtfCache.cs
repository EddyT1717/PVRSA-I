#nullable enable
using System;
using System.Collections.Generic;
using TradingPlatform.BusinessLayer;

namespace PVRSAIndicator
{
    /// <summary>
    /// HTF subscriptions: Daily, Weekly, Monthly, 4H (spec §3).
    /// Closed-bar for pivots/YDay (previous completed = HistoricalData[1]).
    /// Forming HTF for ADR lookahead_on (current period H/L/O; average skips current via barsBack=1).
    /// Dispose all HistoricalData in OnClear.
    /// </summary>
    public sealed class HtfCache : IDisposable
    {
        private HistoricalData? _daily;
        private HistoricalData? _weekly;
        private HistoricalData? _monthly;
        private HistoricalData? _h4;
        private HistoricalData? _overrideHd;
        private bool _disposed;

        public HistoricalData? Daily => _daily;
        public HistoricalData? Weekly => _weekly;
        public HistoricalData? Monthly => _monthly;
        public HistoricalData? H4 => _h4;
        public HistoricalData? OverrideHd => _overrideHd;

        public void Subscribe(Symbol symbol, HistoricalData chartHistory, Symbol? overrideSymbol)
        {
            DisposeInner();
            if (symbol == null || chartHistory == null) return;

            HistoryType ht = symbol.HistoryType;
            DateTime from = chartHistory.FromTime;

            _daily = symbol.GetHistory(Period.DAY1, ht, from);
            _weekly = symbol.GetHistory(Period.WEEK1, ht, from);
            // MONTH1 is the Quantower monthly period (same pattern as DAY1 / WEEK1).
            _monthly = symbol.GetHistory(Period.MONTH1, ht, from);
            _h4 = symbol.GetHistory(Period.HOUR4, ht, from);

            if (overrideSymbol != null)
            {
                Period chartPeriod = Period.DAY1;
                if (chartHistory.Aggregation is HistoryAggregationTime hat)
                    chartPeriod = hat.Period;
                _overrideHd = overrideSymbol.GetHistory(chartPeriod, ht, from);
            }
        }

        public HtfBar? PreviousCompletedDaily() => PreviousCompleted(_daily);
        public HtfBar? PreviousCompletedWeekly() => PreviousCompleted(_weekly);
        public HtfBar? FormingDaily() => Forming(_daily);
        public HtfBar? FormingWeekly() => Forming(_weekly);
        public HtfBar? FormingMonthly() => Forming(_monthly);

        /// <summary>Quantower indexer: [0] is latest (forming). Previous completed is [1].</summary>
        public static HtfBar? PreviousCompleted(HistoricalData? hd)
        {
            if (hd == null || hd.Count < 2) return null;
            HistoryItemBar? bar = hd[1] as HistoryItemBar;
            if (bar == null) return null;
            return ToBar(bar);
        }

        public static HtfBar? Forming(HistoricalData? hd)
        {
            if (hd == null || hd.Count < 1) return null;
            HistoryItemBar? bar = hd[0] as HistoryItemBar;
            if (bar == null) return null;
            return ToBar(bar);
        }

        /// <summary>Oldest-first list of HTF bars. Last item is the forming (current) period.</summary>
        public static List<HtfBar> OldestFirst(HistoricalData? hd)
        {
            List<HtfBar> list = new List<HtfBar>();
            if (hd == null) return list;
            int n = hd.Count;
            for (int i = n - 1; i >= 0; i--)
            {
                HistoryItemBar? bar = hd[i] as HistoryItemBar;
                if (bar == null) continue;
                list.Add(ToBar(bar));
            }
            return list;
        }

        public List<HtfBar> DailyOldestFirst() => OldestFirst(_daily);
        public List<HtfBar> WeeklyOldestFirst() => OldestFirst(_weekly);
        public List<HtfBar> MonthlyOldestFirst() => OldestFirst(_monthly);
        public List<HtfBar> H4OldestFirst() => OldestFirst(_h4);

        /// <summary>
        /// Lookup override-symbol OHLCV at the same open time as the chart bar.
        /// UNKNOWN: exact time alignment if the override venue uses a different session.
        /// </summary>
        public bool TryGetOverrideBar(DateTime chartBarTime, out double open, out double high, out double low, out double close, out double volume)
        {
            open = high = low = close = volume = double.NaN;
            if (_overrideHd == null) return false;
            int n = _overrideHd.Count;
            for (int i = 0; i < n; i++)
            {
                HistoryItemBar? bar = _overrideHd[i] as HistoryItemBar;
                if (bar == null) continue;
                if (bar.TimeLeft == chartBarTime)
                {
                    open = bar.Open;
                    high = bar.High;
                    low = bar.Low;
                    close = bar.Close;
                    volume = bar.Volume;
                    return true;
                }
            }
            return false;
        }

        public static HtfBar ToBar(HistoryItemBar bar)
        {
            return new HtfBar(bar.TimeLeft, bar.Open, bar.High, bar.Low, bar.Close, bar.Volume);
        }

        public void Dispose()
        {
            if (_disposed) return;
            DisposeInner();
            _disposed = true;
        }

        private void DisposeInner()
        {
            DisposeOne(ref _daily);
            DisposeOne(ref _weekly);
            DisposeOne(ref _monthly);
            DisposeOne(ref _h4);
            DisposeOne(ref _overrideHd);
        }

        private static void DisposeOne(ref HistoricalData? hd)
        {
            if (hd == null) return;
            try { hd.Dispose(); }
            catch
            {
                // UNKNOWN: Dispose behavior if Quantower already released the history.
            }
            hd = null;
        }
    }
}
