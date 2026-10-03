#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using TradingPlatform.BusinessLayer;

namespace PVRSAIndicator
{
    /// <summary>
    /// PVRSAIndicator — Quantower overlay context pack (PVSRA, EMA stack, floor pivots,
    /// ADR/AWR/AMR/RD/RW, DST sessions, Psy, VCZ). Not an entry/exit strategy.
    /// Product name is PVRSAIndicator; do not brand the chart as "PVRSA".
    /// </summary>
    public class PVRSAIndicator : Indicator
    {
        #region Inputs — Label offsets (10)

        [InputParameter("General", 10, 0, 999, 1, 0)]
        public int LabelOffsetInput = 50;
        [InputParameter("Pivots", 11, 0, 999, 1, 0)]
        public int PivotOffsetInput = 54;
        [InputParameter("ADR", 12, 0, 999, 1, 0)]
        public int AdrOffsetInput = 12;
        [InputParameter("50% ADR", 13, 0, 999, 1, 0)]
        public int AdrOffsetInput50 = 12;
        [InputParameter("RD/W", 14, 0, 999, 1, 0)]
        public int RdOffsetInput = 24;
        [InputParameter("50% RD/W", 15, 0, 999, 1, 0)]
        public int RdOffsetInput50 = 24;
        [InputParameter("Chart label font size", 16, 6, 24, 1, 0)]
        public double LabelFontSize = 8;

        #endregion
        #region Inputs — PVSRA colors (20)

        [InputParameter("Vector: Red", 20)]
        public Color RedVectorColor = Color.Red;
        [InputParameter("Green", 21)]
        public Color GreenVectorColor = Color.Lime;
        [InputParameter("Violet", 22)]
        public Color VioletVectorColor = Color.Fuchsia;
        [InputParameter("Blue", 23)]
        public Color BlueVectorColor = Color.Blue;
        [InputParameter("Regular: Up Candle", 24)]
        public Color RegularCandleUpColor = PineColor.FromRgb(153, 153, 153);
        [InputParameter("Down Candle", 25)]
        public Color RegularCandleDownColor = PineColor.FromRgb(77, 77, 77);

        #endregion
        #region Inputs — EMAs (30)

        [InputParameter("Show EMAs?", 30)]
        public bool ShowEmas = true;
        [InputParameter("EMA Labels?", 31)]
        public bool LabelEmas = false;
        [InputParameter("EMA Color: 5", 32)]
        public Color OneEmaColor = PineColor.FromRgb(254, 234, 74);
        [InputParameter("13", 33)]
        public Color TwoEmaColor = PineColor.FromRgb(253, 84, 87);
        [InputParameter("50", 34)]
        public Color ThreeEmaColor = PineColor.FromRgb(31, 188, 211);
        [InputParameter("200", 35)]
        public Color FourEmaColor = PineColor.FromRgb(255, 255, 255);
        [InputParameter("800", 36)]
        public Color FiveEmaColor = PineColor.FromRgb(50, 34, 144);
        [InputParameter("EMA Cloud", 37)]
        public Color EmaCloudColor = PineColor.FromRgb(155, 47, 174, 60);
        [InputParameter("Border", 38)]
        public Color EmaCloudBorderColor = PineColor.FromRgb(18, 137, 123, 0);

        #endregion
        #region Inputs — Pivots (40)

        [InputParameter("Show Level: 1 R/S?", 40)]
        public bool ShowLevelOnePivotPoints = false;
        [InputParameter("2 R/S?", 41)]
        public bool ShowLevelTwoPivotPoints = false;
        [InputParameter("3 R/S?", 42)]
        public bool ShowLevelThreePivotPoints = false;
        [InputParameter("Show labels?", 43)]
        public bool ShowPivotLabels = true;
        [InputParameter("R/S Levels Line Style", 44, variants: new object[] { "Dotted", LineStyle.Dot, "Dashed", LineStyle.Dash, "Solid", LineStyle.Solid })]
        public LineStyle RsStyleX = LineStyle.Dash;
        [InputParameter("Show M levels?", 45)]
        public bool ActiveM = true;
        [InputParameter("M Labels?", 46)]
        public bool ShowMLabels = true;
        [InputParameter("Extend lines in both directions?", 47)]
        public bool ExtendPivots = false;
        [InputParameter("Colors: Pivot Point", 48)]
        public Color PivotColor = PineColor.FromRgb(254, 234, 78, 50);
        [InputParameter("Pivot Point Label", 49)]
        public Color PivotLabelColor = PineColor.FromRgb(254, 234, 78, 50);
        [InputParameter("Colors: M Levels", 50)]
        public Color MColor = PineColor.FromRgb(255, 255, 255, 50);
        [InputParameter("M Levels Label", 51)]
        public Color MLabelColor = PineColor.FromRgb(255, 255, 255, 50);
        [InputParameter("M Levels Line Style", 52, variants: new object[] { "Dotted", LineStyle.Dot, "Dashed", LineStyle.Dash, "Solid", LineStyle.Solid })]
        public LineStyle MStyleX = LineStyle.Dash;

        #endregion
        #region Inputs — YDay / LWeek (60)

        [InputParameter("Show Hi/Lo: Daily?", 60)]
        public bool ShowDayHighLow = true;
        [InputParameter("Weekly?", 61)]
        public bool ShowWeekHighLow = true;
        [InputParameter("Show labels?", 62)]
        public bool ShowDayHighLowLabels = true;

        #endregion
        #region Inputs — ADR / AWR / AMR / RD / RW (70)

        [InputParameter("Show ADR?", 70)]
        public bool ShowADR = true;
        [InputParameter("Use Daily Open (DO) calc?", 71)]
        public bool ShowADRDO = false;
        [InputParameter("ADR Labels?", 72)]
        public bool ShowADRLabels = true;
        [InputParameter("ADR Range label?", 73)]
        public bool ShowADRRange = false;
        [InputParameter("Show 50% ADR?", 74)]
        public bool ShowADR50 = false;
        [InputParameter("ADR length (days)?", 75, 1, 31, 1, 0)]
        public int ADRRange = 14;
        [InputParameter("ADR Color", 76)]
        public Color AdrColor = PineColor.FromRgb(192, 192, 192, 50);
        [InputParameter("ADR Line Style", 77, variants: new object[] { "Dotted", LineStyle.Dot, "Dashed", LineStyle.Dash, "Solid", LineStyle.Solid })]
        public LineStyle AdrStyleX = LineStyle.Dot;

        [InputParameter("Show AWR?", 80)]
        public bool ShowAWR = false;
        [InputParameter("Use Weekly Open (WO) calc?", 81)]
        public bool ShowAWRWO = false;
        [InputParameter("AWR Labels?", 82)]
        public bool ShowAWRLabels = true;
        [InputParameter("AWR Range label?", 83)]
        public bool ShowAWRRange = false;
        [InputParameter("Show 50% AWR?", 84)]
        public bool ShowAWR50 = false;
        [InputParameter("AWR length (weeks)?", 85, 1, 52, 1, 0)]
        public int AWRRange = 4;
        [InputParameter("AWR Color", 86)]
        public Color AwrColor = PineColor.FromRgb(255, 165, 0, 50);
        [InputParameter("AWR Line Style", 87, variants: new object[] { "Dotted", LineStyle.Dot, "Dashed", LineStyle.Dash, "Solid", LineStyle.Solid })]
        public LineStyle AwrStyleX = LineStyle.Dot;

        [InputParameter("Show AMR?", 90)]
        public bool ShowAMR = false;
        [InputParameter("Use Monthly Open (MO) calc?", 91)]
        public bool ShowAMRMO = false;
        [InputParameter("AMR Labels?", 92)]
        public bool ShowAMRLabels = true;
        [InputParameter("AMR Range label?", 93)]
        public bool ShowAMRRange = false;
        [InputParameter("Show 50% AMR?", 94)]
        public bool ShowAMR50 = false;
        [InputParameter("AMR length (months)?", 95, 1, 12, 1, 0)]
        public int AMRRange = 6;
        [InputParameter("AMR Color", 96)]
        public Color AmrColor = PineColor.FromRgb(255, 0, 0, 50);
        [InputParameter("AMR Line Style", 97, variants: new object[] { "Dotted", LineStyle.Dot, "Dashed", LineStyle.Dash, "Solid", LineStyle.Solid })]
        public LineStyle AmrStyleX = LineStyle.Dot;

        [InputParameter("Show RD?", 100)]
        public bool ShowRD = false;
        [InputParameter("RD Use Daily Open (DO) calc?", 101)]
        public bool ShowRDDO = false;
        [InputParameter("RD Labels?", 102)]
        public bool ShowRDLabels = true;
        [InputParameter("RD Range label?", 103)]
        public bool ShowRDRange = false;
        [InputParameter("Show 50% RD?", 104)]
        public bool ShowRD50 = false;
        [InputParameter("RD length (days)?", 105, 1, 31, 1, 0)]
        public int RdRange = 15;
        [InputParameter("RD Color", 106)]
        public Color RdColor = PineColor.FromRgb(255, 0, 0, 70);
        [InputParameter("RD Line Style", 107, variants: new object[] { "Dotted", LineStyle.Dot, "Dashed", LineStyle.Dash, "Solid", LineStyle.Solid })]
        public LineStyle RdStyleX = LineStyle.Solid;

        [InputParameter("Show RW?", 110)]
        public bool ShowRW = false;
        [InputParameter("RW Use Weekly Open (WO) calc?", 111)]
        public bool ShowRWWO = false;
        [InputParameter("RW Labels?", 112)]
        public bool ShowRWLabels = true;
        [InputParameter("RW Range label?", 113)]
        public bool ShowRWRange = false;
        [InputParameter("Show 50% RW?", 114)]
        public bool ShowRW50 = false;
        [InputParameter("RW length (weeks)?", 115, 1, 52, 1, 0)]
        public int RwRange = 13;
        [InputParameter("RW Color", 116)]
        public Color RwColor = PineColor.FromRgb(0, 0, 255, 70);
        [InputParameter("RW Line Style", 117, variants: new object[] { "Dotted", LineStyle.Dot, "Dashed", LineStyle.Dash, "Solid", LineStyle.Solid })]
        public LineStyle RwStyleX = LineStyle.Solid;

        #endregion
        #region Inputs — tables / daily open / override / VCZ (120)

        [InputParameter("Show ADR Table", 120)]
        public bool ShowAdrTable = true;
        [InputParameter("Show ADR PIPS", 121)]
        public bool ShowAdrPips = true;
        [InputParameter("Show ADR Currency", 122)]
        public bool ShowAdrCurrency = false;
        [InputParameter("Show RD PIPS", 123)]
        public bool ShowRDPips = false;
        [InputParameter("Show RD Currency", 124)]
        public bool ShowRDCurrency = false;
        [InputParameter("ADR Table position", 125, variants: new object[] {
            "top_right", TablePosition.TopRight, "top_left", TablePosition.TopLeft, "top_center", TablePosition.TopCenter,
            "bottom_right", TablePosition.BottomRight, "bottom_left", TablePosition.BottomLeft, "bottom_center", TablePosition.BottomCenter })]
        public TablePosition ChoiceAdrTable = TablePosition.TopRight;
        [InputParameter("Background Color", 126)]
        public Color AdrTableBgColor = PineColor.FromRgb(93, 96, 107, 70);
        [InputParameter("Text Color", 127)]
        public Color AdrTableTxtColor = PineColor.FromRgb(31, 188, 211);

        [InputParameter("Daily Open Show: line?", 130)]
        public bool ShowRectangle9 = true;
        [InputParameter("Daily Open Label?", 131)]
        public bool ShowLabel9 = true;
        [InputParameter("Show historical daily opens?", 132)]
        public bool ShowallDly = false;
        [InputParameter("Daily Open Color", 133)]
        public Color Sess9col = PineColor.FromRgb(254, 234, 78);

        [InputParameter("Override chart symbol?", 140)]
        public bool OverrideSym = false;
        [InputParameter("PVSRA symbol", 141)]
        public string PvsraSym = "INDEX:BTCUSD";

        [InputParameter("Show VCZ?", 150)]
        public bool ShowVCZ = true;
        [InputParameter("Maximum zones to draw", 151, 1, 500, 1, 0)]
        public int ZonesMax = 500;
        [InputParameter("Zone top/bottom is defined with", 152, variants: new object[] { "Body only", ZoneGeometry.BodyOnly, "Body with wicks", ZoneGeometry.BodyWithWicks })]
        public ZoneGeometry ZoneType = ZoneGeometry.BodyOnly;
        [InputParameter("Zones are cleared using candle", 153, variants: new object[] { "Body only", ZoneGeometry.BodyOnly, "Body with wicks", ZoneGeometry.BodyWithWicks })]
        public ZoneGeometry ZoneUpdateType = ZoneGeometry.BodyWithWicks;
        [InputParameter("Zone border width", 154, 0, 8, 1, 0)]
        public int BorderWidth = 0;
        [InputParameter("Override color?", 155)]
        public bool ColorOverride = true;
        [InputParameter("Zone Color", 156)]
        public Color ZoneColor = PineColor.FromRgb(255, 230, 75, 90);
        [InputParameter("Zone Transperancy", 157, 0, 100, 1, 0)]
        public int Transperancy = 90;

        #endregion
        #region Inputs — sessions / psy / DST / alerts (160)

        [InputParameter("Line style of Market Session hi/lo line", 160, variants: new object[] { "Dashed", LineStyle.Dash, "Solid", LineStyle.Solid })]
        public LineStyle RectStyle = LineStyle.Dash;
        [InputParameter("Show Market Sessions?", 161)]
        public bool ShowMarkets = true;
        [InputParameter("Show Market Session on Weekends?", 162)]
        public bool ShowMarketsWeekends = false;

        [InputParameter("London Show: session?", 170)] public bool ShowRectangle1 = true;
        [InputParameter("London Label?", 171)] public bool ShowLabel1 = true;
        [InputParameter("London Opening Range?", 172)] public bool ShowOr1 = true;
        [InputParameter("London Name", 173)] public string Sess1Label = "London";
        [InputParameter("London Color: Box", 174)] public Color Sess1col = PineColor.FromRgb(120, 123, 134, 75);
        [InputParameter("London Label color", 175)] public Color Sess1colLabel = PineColor.FromRgb(120, 123, 134, 0);

        [InputParameter("New York Show: session?", 180)] public bool ShowRectangle2 = true;
        [InputParameter("New York Label?", 181)] public bool ShowLabel2 = true;
        [InputParameter("New York Opening Range?", 182)] public bool ShowOr2 = true;
        [InputParameter("New York Name", 183)] public string Sess2Label = "NewYork";
        [InputParameter("New York Color: Box", 184)] public Color Sess2col = PineColor.FromRgb(251, 86, 91, 75);
        [InputParameter("New York Label color", 185)] public Color Sess2colLabel = PineColor.FromRgb(253, 84, 87, 25);

        [InputParameter("Tokyo Show: session?", 190)] public bool ShowRectangle3 = true;
        [InputParameter("Tokyo Label?", 191)] public bool ShowLabel3 = true;
        [InputParameter("Tokyo Opening Range?", 192)] public bool ShowOr3 = true;
        [InputParameter("Tokyo Name", 193)] public string Sess3Label = "Tokyo";
        [InputParameter("Tokyo Color: Box", 194)] public Color Sess3col = PineColor.FromRgb(80, 174, 85, 75);
        [InputParameter("Tokyo Label color", 195)] public Color Sess3colLabel = PineColor.FromRgb(80, 174, 85, 25);

        [InputParameter("HongKong Show: session?", 200)] public bool ShowRectangle4 = true;
        [InputParameter("HongKong Label?", 201)] public bool ShowLabel4 = true;
        [InputParameter("HongKong Opening Range?", 202)] public bool ShowOr4 = true;
        [InputParameter("HongKong Name", 203)] public string Sess4Label = "HongKong";
        [InputParameter("HongKong Color: Box", 204)] public Color Sess4col = PineColor.FromRgb(128, 127, 23, 75);
        [InputParameter("HongKong Label color", 205)] public Color Sess4colLabel = PineColor.FromRgb(128, 127, 23, 25);

        [InputParameter("Sydney Show: session?", 210)] public bool ShowRectangle5 = true;
        [InputParameter("Sydney Label?", 211)] public bool ShowLabel5 = true;
        [InputParameter("Sydney Opening Range?", 212)] public bool ShowOr5 = true;
        [InputParameter("Sydney Name", 213)] public string Sess5Label = "Sydney";
        [InputParameter("Sydney Color: Box", 214)] public Color Sess5col = PineColor.FromRgb(37, 228, 123, 75);
        [InputParameter("Sydney Label color", 215)] public Color Sess5colLabel = PineColor.FromRgb(37, 228, 123, 25);

        [InputParameter("EU Brinks Show: session?", 220)] public bool ShowRectangle6 = true;
        [InputParameter("EU Brinks Label?", 221)] public bool ShowLabel6 = true;
        [InputParameter("EU Brinks Opening Range?", 222)] public bool ShowOr6 = true;
        [InputParameter("EU Brinks Name", 223)] public string Sess6Label = "EU Brinks";
        [InputParameter("EU Brinks Color: Box", 224)] public Color Sess6col = PineColor.FromRgb(255, 255, 255, 65);
        [InputParameter("EU Brinks Label color", 225)] public Color Sess6colLabel = PineColor.FromRgb(255, 255, 255, 25);

        [InputParameter("US Brinks Show: session?", 230)] public bool ShowRectangle7 = true;
        [InputParameter("US Brinks Label?", 231)] public bool ShowLabel7 = true;
        [InputParameter("US Brinks Opening Range?", 232)] public bool ShowOr7 = true;
        [InputParameter("US Brinks Name", 233)] public string Sess7Label = "US Brinks";
        [InputParameter("US Brinks Color: Box", 234)] public Color Sess7col = PineColor.FromRgb(255, 255, 255, 65);
        [InputParameter("US Brinks Label color", 235)] public Color Sess7colLabel = PineColor.FromRgb(255, 255, 255, 25);

        [InputParameter("Frankfurt Show: session?", 240)] public bool ShowRectangle8 = false;
        [InputParameter("Frankfurt Label?", 241)] public bool ShowLabel8 = true;
        [InputParameter("Frankfurt Opening Range?", 242)] public bool ShowOr8 = true;
        [InputParameter("Frankfurt Name", 243)] public string Sess8Label = "Frankfurt";
        [InputParameter("Frankfurt Color: Box", 244)] public Color Sess8col = PineColor.FromRgb(253, 152, 39, 75);
        [InputParameter("Frankfurt Label color", 245)] public Color Sess8colLabel = PineColor.FromRgb(253, 152, 39, 25);

        [InputParameter("Psy Show: Levels?", 250)] public bool ShowPsylevels = true;
        [InputParameter("Psy Labels?", 251)] public bool ShowPsylabel = true;
        [InputParameter("Show historical psy levels?", 252)] public bool ShowAllPsy = false;
        [InputParameter("Psy Hi Color", 253)] public Color PsyColH = PineColor.FromRgb(255, 165, 0, 70);
        [InputParameter("Psy Lo Color", 254)] public Color PsyColL = PineColor.FromRgb(255, 165, 0, 70);
        [InputParameter("Override PsyType", 255)] public bool OverridePsyType = false;
        [InputParameter("Psy calc type", 256, variants: new object[] { "crypto", PsyType.Crypto, "forex", PsyType.Forex })]
        public PsyType PsyTypeX = PsyType.Crypto;

        [InputParameter("Show DST Table", 260)] public bool ShowDstTable = false;
        [InputParameter("DST Table position", 261, variants: new object[] {
            "bottom_center", TablePosition.BottomCenter, "top_right", TablePosition.TopRight, "top_left", TablePosition.TopLeft,
            "top_center", TablePosition.TopCenter, "bottom_right", TablePosition.BottomRight, "bottom_left", TablePosition.BottomLeft })]
        public TablePosition ChoiceDstTable = TablePosition.BottomCenter;
        [InputParameter("DST Background", 262)] public Color DstTableBgColor = PineColor.FromRgb(93, 96, 107, 70);
        [InputParameter("DST Text Color", 263)] public Color DstTableTxtColor = PineColor.FromRgb(31, 188, 211);

        [InputParameter("Alert frequency", 270, variants: new object[] { "OnBarClose", AlertFrequency.OnBarClose, "OnEachTick", AlertFrequency.OnEachTick })]
        public AlertFrequency AlertMode = AlertFrequency.OnBarClose;

        #endregion

        private readonly EmaMath _ema5 = new EmaMath(5);
        private readonly EmaMath _ema13 = new EmaMath(13);
        private readonly EmaMath _ema50 = new EmaMath(50);
        private readonly EmaMath _ema200 = new EmaMath(200);
        private readonly EmaMath _ema800 = new EmaMath(800);
        private readonly SampleStdev _stdev = new SampleStdev(100);
        private readonly HtfCache _htf = new HtfCache();
        private readonly VectorZoneEngine _zones = new VectorZoneEngine();
        private readonly Dictionary<string, bool> _alertLatch = new Dictionary<string, bool>();

        private Color _prevPvsra = Color.Empty;
        private bool _hasPrevPvsra;
        private List<HorizontalLevel> _levels = new List<HorizontalLevel>();
        private List<SessionDraw> _sessions = new List<SessionDraw>();
        private List<ChartLabel> _labels = new List<ChartLabel>();
        private List<TableRow> _adrRows = new List<TableRow>();
        private List<TableRow> _dstRows = new List<TableRow>();
        private bool _cloudStarted;

        private static readonly Color YDayColor = PineColor.FromRgb(0, 0, 255, 50);
        private static readonly Color LWeekColor = PineColor.FromRgb(0, 128, 0, 40);
        private static readonly Color RColor = PineColor.FromRgb(0, 128, 0, 50);
        private static readonly Color SColor = PineColor.FromRgb(255, 0, 0, 50);

        public PVRSAIndicator()
        {
            Name = "PVRSAIndicator";
            Description = "Price-overlay context pack: PVSRA vectors, EMA stack, floor pivots, ADR ranges, DST sessions, Psy levels, vector zones.";
            SeparateWindow = false;
            AddLineSeries("EMA5", OneEmaColor, 1, LineStyle.Solid);
            AddLineSeries("EMA13", TwoEmaColor, 1, LineStyle.Solid);
            AddLineSeries("EMA50", ThreeEmaColor, 1, LineStyle.Solid);
            AddLineSeries("EMA200", FourEmaColor, 1, LineStyle.Solid);
            AddLineSeries("EMA800", FiveEmaColor, 2, LineStyle.Solid);
            AddLineSeries("CloudUpper", EmaCloudBorderColor, 1, LineStyle.Solid);
            AddLineSeries("CloudLower", EmaCloudBorderColor, 1, LineStyle.Solid);
        }

        protected override void OnInit()
        {
            _ema5.Reset(); _ema13.Reset(); _ema50.Reset(); _ema200.Reset(); _ema800.Reset();
            _stdev.Reset();
            _zones.Reset();
            _alertLatch.Clear();
            _hasPrevPvsra = false;
            _cloudStarted = false;
            Symbol? ov = null;
            if (OverrideSym && !string.IsNullOrWhiteSpace(PvsraSym))
            {
                GetSymbolRequestParameters req = new GetSymbolRequestParameters();
                req.SymbolId = PvsraSym;
                try { ov = Core.Instance.GetSymbol(req); }
                catch { ov = null; }
            }
            _htf.Subscribe(this.Symbol, this.HistoricalData, ov);
        }

        protected override void OnClear()
        {
            _htf.Dispose();
            _zones.Reset();
            _alertLatch.Clear();
        }

        protected override void OnUpdate(UpdateArgs args)
        {
            if (Count < 1) return;
            bool forming = args.Reason == UpdateReason.NewTick;
            double close = Close();
            double ema5 = forming ? _ema5.Peek(close) : _ema5.Push(close);
            double ema13 = forming ? _ema13.Peek(close) : _ema13.Push(close);
            double ema50 = forming ? _ema50.Peek(close) : _ema50.Push(close);
            double ema200 = forming ? _ema200.Peek(close) : _ema200.Push(close);
            double ema800 = forming ? _ema800.Peek(close) : _ema800.Push(close);
            double sd = forming ? _stdev.PeekWithCurrent(close) : _stdev.Push(close);
            double cloud = sd / 4.0;
            double cup = ema50 + cloud;
            double clo = ema50 - cloud;

            if (ShowEmas)
            {
                SetValue(ema5, 0);
                SetValue(ema13, 1);
                SetValue(ema50, 2);
                SetValue(ema200, 3);
                SetValue(ema800, 4);
                SetValue(cup, 5);
                SetValue(clo, 6);
                LinesSeries[0].Color = OneEmaColor;
                LinesSeries[1].Color = TwoEmaColor;
                LinesSeries[2].Color = ThreeEmaColor;
                LinesSeries[3].Color = FourEmaColor;
                LinesSeries[4].Color = FiveEmaColor;
                LinesSeries[5].Color = EmaCloudBorderColor;
                LinesSeries[6].Color = EmaCloudBorderColor;
                for (int i = 0; i < 7; i++) LinesSeries[i].Visible = true;
                if (!_cloudStarted && !double.IsNaN(cup) && !double.IsNaN(clo))
                {
                    BeginCloud(5, 6, EmaCloudColor);
                    _cloudStarted = true;
                }
            }
            else
            {
                for (int i = 0; i < 7; i++) LinesSeries[i].Visible = false;
            }

            double o = Open(), h = High(), l = Low(), c = close, v = Volume();
            DateTime t = Time();
            if (OverrideSym)
            {
                double oo, hh, ll, cc, vv;
                if (_htf.TryGetOverrideBar(t, out oo, out hh, out ll, out cc, out vv))
                {
                    o = oo; h = hh; l = ll; c = cc; v = vv;
                }
            }

            List<double> pVol = new List<double>(10);
            List<double> pHi = new List<double>(10);
            List<double> pLo = new List<double>(10);
            int look = Math.Min(10, Count - 1);
            for (int i = 1; i <= look; i++)
            {
                pVol.Add(Volume(i));
                pHi.Add(High(i));
                pLo.Add(Low(i));
            }
            PvsraResult pvsra = PvsraCalculator.Classify(v, h, l, c, o, pVol, pHi, pLo,
                RedVectorColor, GreenVectorColor, VioletVectorColor, BlueVectorColor,
                RegularCandleDownColor, RegularCandleUpColor);
            SetBarColor(pvsra.Color);
            PatternFlags patterns = PvsraCalculator.DetectPatterns(pvsra.Color, _prevPvsra, _hasPrevPvsra,
                RedVectorColor, GreenVectorColor, VioletVectorColor, BlueVectorColor);

            if (forming)
                _zones.UpdateForming(pvsra.Flag, o, h, l, c, t, ShowVCZ, ZonesMax, ZoneType, ZoneUpdateType, ColorOverride, ZoneColor, Transperancy, pvsra.Color);
            else
                _zones.Update(pvsra.Flag, o, h, l, c, t, ShowVCZ, ZonesMax, ZoneType, ZoneUpdateType, ColorOverride, ZoneColor, Transperancy, pvsra.Color);

            ChartPeriodInfo tf = ReadPeriod();
            HtfBar? prevDay = _htf.PreviousCompletedDaily();
            HtfBar? prevWeek = _htf.PreviousCompletedWeekly();
            HtfBar? formDay = _htf.FormingDaily();
            FloorPivotLevels pivots = prevDay.HasValue
                ? FloorPivots.Compute(prevDay.Value.High, prevDay.Value.Low, prevDay.Value.Close)
                : FloorPivotLevels.Invalid;
            AdrPack adr = RangeProjector.AdrHiLo(_htf.DailyOldestFirst(), ADRRange, 1, ShowADRDO);
            AdrPack rd = RangeProjector.AdrHiLo(_htf.DailyOldestFirst(), RdRange, 1, ShowRDDO);
            AdrPack awr = RangeProjector.AdrHiLo(_htf.WeeklyOldestFirst(), AWRRange, 1, ShowAWRWO);
            AdrPack rw = RangeProjector.AdrHiLo(_htf.WeeklyOldestFirst(), RwRange, 1, ShowRWWO);
            AdrPack amr = RangeProjector.AdrHiLo(_htf.MonthlyOldestFirst(), AMRRange, 1, ShowAMRMO);

            DstFlags dst = DstCalendar.GetFlags(t);
            PsyType psyType = OverridePsyType ? PsyTypeX : (Symbol != null && Symbol.SymbolType == SymbolType.Forex ? PsyType.Forex : PsyType.Crypto);
            bool showPsy = ShowPsylevels && PsyLevelCalculator.IsAllowedTf(tf.Minutes, tf.IsMinute);
            PsyLevels psy = PsyLevels.Empty;
            if (showPsy)
                psy = PsyLevelCalculator.Compute(t, psyType, dst.SydDst, _htf.H4OldestFirst(), TimeSpan.FromHours(4));

            bool showSessions = ShowMarkets && tf.IsMinute && tf.Minutes >= 1 && tf.Minutes <= 240;
            bool validIntraday = !tf.IsDaily;
            bool validDhl = validIntraday;
            bool validWhl = validIntraday || tf.IsDaily;
            bool showAmr = ShowAMR && tf.IsMinute && tf.Minutes >= 3;
            DateTime dayStart = StartOfUtcDay(t);
            DateTime levelStart = (tf.IsSecond || (tf.IsMinute && tf.Minutes < 5)) ? dayStart : dayStart.AddDays(-1);

            RebuildDrawings(t, tf, dayStart, levelStart, formDay, prevDay, prevWeek, pivots, adr, awr, amr, rd, rw, psy, dst, showSessions, showPsy, showAmr, validIntraday, validDhl, validWhl, ema5, ema13, ema50, ema200, ema800);

            if (Count > 1)
            {
                double prevClose = Close(1);
                FireAlerts(forming, args, pvsra, patterns, prevClose, Close(), adr, awr, amr, rd, rw, psy, formDay);
            }

            if (!forming)
            {
                _prevPvsra = pvsra.Color;
                _hasPrevPvsra = true;
            }
        }

        public override void OnPaintChart(PaintChartEventArgs args)
        {
            base.OnPaintChart(args);
            if (CurrentChart == null || args == null) return;
            Graphics g = args.Graphics;
            var win = CurrentChart.MainWindow;
            if (win == null) return;
            var conv = win.CoordinatesConverter;
            RectangleF clip = win.ClientRectangle;
            ChartPainter.Paint(
                g, clip,
                time => conv.GetChartX(time),
                price => conv.GetChartY(price),
                _levels, _sessions, _zones.Above, _zones.Below, _labels,
                ShowAdrTable ? _adrRows : new List<TableRow>(),
                ChoiceAdrTable, AdrTableBgColor, AdrTableTxtColor,
                ShowDstTable ? _dstRows : new List<TableRow>(),
                ChoiceDstTable, DstTableBgColor, DstTableTxtColor,
                BorderWidth, (float)LabelFontSize);
        }

        private void RebuildDrawings(
            DateTime t, ChartPeriodInfo tf, DateTime dayStart, DateTime levelStart,
            HtfBar? formDay, HtfBar? prevDay, HtfBar? prevWeek,
            FloorPivotLevels pivots, AdrPack adr, AdrPack awr, AdrPack amr, AdrPack rd, AdrPack rw,
            PsyLevels psy, DstFlags dst, bool showSessions, bool showPsy, bool showAmr,
            bool validIntraday, bool validDhl, bool validWhl,
            double ema5, double ema13, double ema50, double ema200, double ema800)
        {
            _levels = new List<HorizontalLevel>();
            _labels = new List<ChartLabel>();
            TimeSpan dur = tf.Duration;
            DateTime lastClose = t + dur;
            DateTime LabelX(int offset) { return lastClose.AddMilliseconds(offset * dur.TotalMilliseconds); }

            string ext = ExtendPivots ? "both" : "right";
            bool anyPivot = ShowLevelOnePivotPoints || ShowLevelTwoPivotPoints || ShowLevelThreePivotPoints || ActiveM;
            if (anyPivot && validIntraday && pivots.IsValid)
            {
                AddLv("PP", pivots.Pp, PivotColor, LineStyle.Solid, 1, ext, levelStart, ShowPivotLabels, "PP", PivotLabelColor);
                if (ShowLevelOnePivotPoints) { AddLv("R1", pivots.R1, RColor, RsStyleX, 1, ext, levelStart, ShowPivotLabels, "R1", RColor); AddLv("S1", pivots.S1, SColor, RsStyleX, 1, ext, levelStart, ShowPivotLabels, "S1", SColor); }
                if (ShowLevelTwoPivotPoints) { AddLv("R2", pivots.R2, RColor, RsStyleX, 1, ext, levelStart, ShowPivotLabels, "R2", RColor); AddLv("S2", pivots.S2, SColor, RsStyleX, 1, ext, levelStart, ShowPivotLabels, "S2", SColor); }
                if (ShowLevelThreePivotPoints) { AddLv("R3", pivots.R3, RColor, RsStyleX, 1, ext, levelStart, ShowPivotLabels, "R3", RColor); AddLv("S3", pivots.S3, SColor, RsStyleX, 1, ext, levelStart, ShowPivotLabels, "S3", SColor); }
                if (ActiveM)
                {
                    AddLv("M0", pivots.M0, MColor, MStyleX, 1, ext, levelStart, ShowMLabels, "M0", MLabelColor);
                    AddLv("M1", pivots.M1, MColor, MStyleX, 1, ext, levelStart, ShowMLabels, "M1", MLabelColor);
                    AddLv("M2", pivots.M2, MColor, MStyleX, 1, ext, levelStart, ShowMLabels, "M2", MLabelColor);
                    AddLv("M3", pivots.M3, MColor, MStyleX, 1, ext, levelStart, ShowMLabels, "M3", MLabelColor);
                    AddLv("M4", pivots.M4, MColor, MStyleX, 1, ext, levelStart, ShowMLabels, "M4", MLabelColor);
                    AddLv("M5", pivots.M5, MColor, MStyleX, 1, ext, levelStart, ShowMLabels, "M5", MLabelColor);
                }
            }

            if (ShowDayHighLow && validDhl && prevDay.HasValue)
            {
                AddLv("YDay Hi", prevDay.Value.High, YDayColor, LineStyle.Solid, 2, "right", levelStart, ShowDayHighLowLabels, "YDay Hi", YDayColor);
                AddLv("YDay Lo", prevDay.Value.Low, YDayColor, LineStyle.Solid, 2, "right", levelStart, ShowDayHighLowLabels, "YDay Lo", YDayColor);
            }
            if (ShowWeekHighLow && validWhl && prevWeek.HasValue)
            {
                AddLv("LWeek Hi", prevWeek.Value.High, LWeekColor, LineStyle.Solid, 2, "right", levelStart, ShowDayHighLowLabels, "LWeek Hi", LWeekColor);
                AddLv("LWeek Lo", prevWeek.Value.Low, LWeekColor, LineStyle.Solid, 2, "right", levelStart, ShowDayHighLowLabels, "LWeek Lo", LWeekColor);
            }

            if (ShowADR && validIntraday) PackRange("ADR", adr, AdrColor, AdrStyleX, 2, ShowADR50, levelStart, ShowADRLabels);
            if (ShowAWR && validIntraday) PackRange("AWR", awr, AwrColor, AwrStyleX, 1, ShowAWR50, levelStart, ShowAWRLabels);
            if (showAmr && validIntraday) PackRange("AMR", amr, AmrColor, AmrStyleX, 1, ShowAMR50, levelStart, ShowAMRLabels);
            if (ShowRD && validIntraday) PackRange("RD", rd, RdColor, RdStyleX, 2, ShowRD50, levelStart, ShowRDLabels);
            if (ShowRW && validIntraday) PackRange("RW", rw, RwColor, RwStyleX, 2, ShowRW50, levelStart, ShowRWLabels);

            if (ShowEmas && LabelEmas)
            {
                PushLab("EMA 5", ema5, LabelX(LabelOffsetInput), OneEmaColor);
                PushLab("EMA 13", ema13, LabelX(LabelOffsetInput), TwoEmaColor);
                PushLab("EMA 50", ema50, LabelX(LabelOffsetInput), ThreeEmaColor);
                PushLab("EMA 200", ema200, LabelX(LabelOffsetInput), FourEmaColor);
                PushLab("EMA 800", ema800, LabelX(LabelOffsetInput), FiveEmaColor);
            }
            if (OverrideSym)
                PushLab("PVSRA Override Active!", (High() + Low()) / 2.0, LabelX(LabelOffsetInput), Color.Orange);

            if (ShowADR && ShowADRRange && adr.IsValid)
                PushLab("ADR " + PipConverter.FormatPips(PipConverter.ToPips(adr.Adr, PointSize())) + " pips | " + PipConverter.FormatPrice(adr.Adr),
                    (adr.AdrHigh + adr.AdrLow) / 2.0, LabelX(AdrOffsetInput), AdrColor);
            if (ShowRD && ShowRDRange && rd.IsValid)
                PushLab("RD " + PipConverter.FormatPips(PipConverter.ToPips(rd.Adr, PointSize())) + " pips | " + PipConverter.FormatPrice(rd.Adr),
                    (rd.AdrHigh + rd.AdrLow) / 2.0, LabelX(RdOffsetInput), RdColor);
            if (ShowADR && ShowADR50 && adr.IsValid)
                PushLab("ADR 50%", (adr.Hi50 + adr.Lo50) / 2.0, LabelX(AdrOffsetInput50), AdrColor);
            // Pine bug: 50% RD mid used High50+High50 — intended High50+Low50 (spec §11).
            if (ShowRD && ShowRD50 && rd.IsValid)
                PushLab("RD 50%", (rd.Hi50 + rd.Lo50) / 2.0, LabelX(RdOffsetInput50), RdColor);

            if (ShowRectangle9 && tf.IsMinute && formDay.HasValue && !ShowallDly)
            {
                AddLv("Daily Open", formDay.Value.Open, Sess9col, LineStyle.Solid, 1, "none", dayStart, ShowLabel9, "Daily Open", Sess9col);
                _levels[_levels.Count - 1].EndTime = lastClose;
            }
            if (showPsy && !double.IsNaN(psy.Hi))
            {
                AddLv("Psy Hi", psy.Hi, PsyColH, LineStyle.Solid, 1, "none", psy.SessionStartTime, ShowPsylabel, psy.HiLabel, PsyColH);
                _levels[_levels.Count - 1].EndTime = t;
                AddLv("Psy Lo", psy.Lo, PsyColL, LineStyle.Solid, 1, "none", psy.SessionStartTime, ShowPsylabel, psy.LoLabel, PsyColL);
                _levels[_levels.Count - 1].EndTime = t;
            }

            SessionItem[] items = PackSessions();
            _sessions = SessionClock.BuildCurrentSessions(new ChartBarsView(this), t, dst, showSessions, ShowMarkets, ShowMarketsWeekends, items);
            foreach (SessionDraw s in _sessions)
            {
                if (!s.ShowLines || double.IsNaN(s.High)) continue;
                Color lc = s.LabelColor.A == 0 ? s.BoxColor : s.LabelColor;
                AddLv(s.Name + " Hi", s.High, lc, RectStyle, 1, "none", s.Start, s.ShowLabel, s.Name, lc);
                _levels[_levels.Count - 1].EndTime = s.End;
                AddLv(s.Name + " Lo", s.Low, lc, RectStyle, 1, "none", s.Start, false, s.Name, lc);
                _levels[_levels.Count - 1].EndTime = s.End;
            }

            _adrRows = new List<TableRow>();
            if (ShowAdrTable && validIntraday)
            {
                double ps = PointSize();
                if (ShowAdrPips)
                {
                    _adrRows.Add(Row("ADR", PipConverter.FormatPips(PipConverter.ToPips(adr.Adr, ps))));
                    _adrRows.Add(Row("ADRx3", PipConverter.FormatPips(PipConverter.ToPips(adr.Adr * 3.0, ps))));
                    _adrRows.Add(Row("AWR", PipConverter.FormatPips(PipConverter.ToPips(awr.Adr, ps))));
                    _adrRows.Add(Row("AMR", PipConverter.FormatPips(PipConverter.ToPips(amr.Adr, ps))));
                }
                if (ShowAdrCurrency)
                {
                    _adrRows.Add(Row("ADR $", PipConverter.FormatPrice(adr.Adr)));
                    _adrRows.Add(Row("ADRx3 $", PipConverter.FormatPrice(adr.Adr * 3.0)));
                    _adrRows.Add(Row("AWR $", PipConverter.FormatPrice(awr.Adr)));
                    _adrRows.Add(Row("AMR $", PipConverter.FormatPrice(amr.Adr)));
                }
                if (ShowRDPips)
                {
                    _adrRows.Add(Row("RD", PipConverter.FormatPips(PipConverter.ToPips(rd.Adr, ps))));
                    _adrRows.Add(Row("RDx3", PipConverter.FormatPips(PipConverter.ToPips(rd.Adr * 3.0, ps))));
                    _adrRows.Add(Row("RW", PipConverter.FormatPips(PipConverter.ToPips(rw.Adr, ps))));
                }
                if (ShowRDCurrency)
                {
                    // Pine used range/2 for currency — intended full range (spec §11).
                    _adrRows.Add(Row("RD $", PipConverter.FormatPrice(rd.Adr)));
                    _adrRows.Add(Row("RDx3 $", PipConverter.FormatPrice(rd.Adr * 3.0)));
                    _adrRows.Add(Row("RW $", PipConverter.FormatPrice(rw.Adr)));
                }
            }
            _dstRows = new List<TableRow>();
            if (ShowDstTable)
            {
                for (int i = 0; i < DstCalendar.TableText.Length; i++)
                    _dstRows.Add(new TableRow { Name = "", Value = DstCalendar.TableText[i] });
            }
        }

        private void FireAlerts(bool forming, UpdateArgs args, PvsraResult pvsra, PatternFlags p, double prevClose, double curr,
            AdrPack adr, AdrPack awr, AdrPack amr, AdrPack rd, AdrPack rw, PsyLevels psy, HtfBar? formDay)
        {
            if (AlertMode == AlertFrequency.OnBarClose && forming) return;
            string sym = Symbol != null ? Symbol.Name : "";
            string tf = TfLabel();
            void Fire(string key, bool cond, string msg)
            {
                bool was;
                _alertLatch.TryGetValue(key, out was);
                if (cond && !was)
                {
                    try { Core.Instance.Loggers.Log(msg, LoggingLevel.System); } catch { }
                }
                _alertLatch[key] = cond;
            }
            Fire("any", pvsra.AlertFlag, sym + " Vector Candle on the " + tf);
            Fire("g", PineColor.Eq(pvsra.Color, GreenVectorColor), sym + " Green Vector Candle on the " + tf);
            Fire("r", PineColor.Eq(pvsra.Color, RedVectorColor), sym + " Red Vector Candle on the " + tf);
            Fire("b", PineColor.Eq(pvsra.Color, BlueVectorColor), sym + " Blue Vector Candle on the " + tf);
            Fire("v", PineColor.Eq(pvsra.Color, VioletVectorColor), sym + " Purple Vector Candle on the " + tf);
            Fire("rg", p.RedGreen, sym + " Red/Green Vector Candle Pattern on the " + tf);
            Fire("gr", p.GreenRed, sym + " Green/Red Vector Candle Pattern on the " + tf);
            Fire("rb", p.RedBlue, sym + " Red/Blue Vector Candle Pattern on the " + tf);
            Fire("br", p.BlueRed, sym + " Blue/Red Vector Candle Pattern on the " + tf);
            Fire("gp", p.GreenPurple, sym + " Green/Purple Vector Candle Pattern on the " + tf);
            Fire("pg", p.PurpleGreen, sym + " Purple/Green Vector Candle Pattern on the " + tf);
            Fire("bp", p.BluePurple, sym + " Blue/Purple Vector Candle Pattern on the " + tf);
            Fire("pb", p.PurpleBlue, sym + " Purple/Blue Vector Candle Pattern on the " + tf);
            Fire("adrH", CrossOver(prevClose, curr, adr.AdrHigh), "PA has reached the calculated ADR High");
            Fire("adrL", CrossUnder(prevClose, curr, adr.AdrLow), "PA has reached the calculated ADR Low");
            Fire("adrH50", CrossOver(prevClose, curr, adr.Hi50), "PA has reached the calculated 50% ADR High");
            Fire("adrL50", CrossUnder(prevClose, curr, adr.Lo50), "PA has reached the calculated 50% ADR Low");
            Fire("awrH", CrossOver(prevClose, curr, awr.AdrHigh), "PA has reached the calculated AWR High");
            Fire("awrL", CrossUnder(prevClose, curr, awr.AdrLow), "PA has reached the calculated AWR Low");
            Fire("amrH", CrossOver(prevClose, curr, amr.AdrHigh), "PA has reached the calculated AMR High");
            Fire("amrL", CrossUnder(prevClose, curr, amr.AdrLow), "PA has reached the calculated AMR Low");
            Fire("rdH", CrossOver(prevClose, curr, rd.AdrHigh), "PA has reached the calculated RD High");
            Fire("rdL", CrossUnder(prevClose, curr, rd.AdrLow), "PA has reached the calculated RD Low");
            Fire("rwH", CrossOver(prevClose, curr, rw.AdrHigh), "PA has reached the calculated RW High");
            Fire("rwL", CrossUnder(prevClose, curr, rw.AdrLow), "PA has reached the calculated RW Low");
            Fire("psyHo", CrossOver(prevClose, curr, psy.Hi), "PA has crossed over the Psy Hi");
            Fire("psyHu", CrossUnder(prevClose, curr, psy.Hi), "PA has crossed under the Psy Hi");
            Fire("psyLo", CrossOver(prevClose, curr, psy.Lo), "PA has crossed over the Psy Lo");
            Fire("psyLu", CrossUnder(prevClose, curr, psy.Lo), "PA has crossed under the Psy Lo");
            double dopen = formDay.HasValue ? formDay.Value.Open : double.NaN;
            Fire("do", CrossOver(prevClose, curr, dopen) || CrossUnder(prevClose, curr, dopen), "PA has crossed the Daily open");
        }

        private static bool CrossOver(double prev, double curr, double level)
        {
            return IsFinite(level) && level != 0.0 && IsFinite(prev) && IsFinite(curr) && prev <= level && curr > level;
        }
        private static bool CrossUnder(double prev, double curr, double level)
        {
            return IsFinite(level) && level != 0.0 && IsFinite(prev) && IsFinite(curr) && prev >= level && curr < level;
        }
        private static bool IsFinite(double x) { return !double.IsNaN(x) && !double.IsInfinity(x); }

        private void AddLv(string tag, double price, Color color, LineStyle style, int width, string extend, DateTime start, bool showLabel, string label, Color labelColor)
        {
            _levels.Add(new HorizontalLevel
            {
                Tag = tag, Price = price, Color = color, Style = style, Width = width,
                Extend = extend, StartTime = start, ShowLabel = showLabel, Label = label, LabelColor = labelColor
            });
        }
        private void PackRange(string tag, AdrPack pack, Color color, LineStyle style, int width, bool show50, DateTime start, bool labels)
        {
            if (!pack.IsValid) return;
            AddLv(tag + " High", pack.AdrHigh, color, style, width, "right", start, labels, tag + " High", color);
            AddLv(tag + " Low", pack.AdrLow, color, style, width, "right", start, labels, tag + " Low", color);
            if (show50)
            {
                AddLv(tag + " 50% High", pack.Hi50, color, style, width, "right", start, labels, tag + " 50H", color);
                AddLv(tag + " 50% Low", pack.Lo50, color, style, width, "right", start, labels, tag + " 50L", color);
            }
        }
        private void PushLab(string text, double price, DateTime time, Color color)
        {
            if (!IsFinite(price)) return;
            _labels.Add(new ChartLabel { Text = text, Price = price, Time = time, Color = color });
        }
        private static TableRow Row(string n, string v) { return new TableRow { Name = n, Value = v }; }

        private SessionItem[] PackSessions()
        {
            return new SessionItem[]
            {
                Item(ShowRectangle1, ShowLabel1, ShowOr1, Sess1Label, Sess1col, Sess1colLabel),
                Item(ShowRectangle2, ShowLabel2, ShowOr2, Sess2Label, Sess2col, Sess2colLabel),
                Item(ShowRectangle3, ShowLabel3, ShowOr3, Sess3Label, Sess3col, Sess3colLabel),
                Item(ShowRectangle4, ShowLabel4, ShowOr4, Sess4Label, Sess4col, Sess4colLabel),
                Item(ShowRectangle5, ShowLabel5, ShowOr5, Sess5Label, Sess5col, Sess5colLabel),
                Item(ShowRectangle6, ShowLabel6, ShowOr6, Sess6Label, Sess6col, Sess6colLabel),
                Item(ShowRectangle7, ShowLabel7, ShowOr7, Sess7Label, Sess7col, Sess7colLabel),
                Item(ShowRectangle8, ShowLabel8, ShowOr8, Sess8Label, Sess8col, Sess8colLabel)
            };
        }
        private static SessionItem Item(bool show, bool lab, bool or, string name, Color box, Color lc)
        {
            return new SessionItem { ShowSession = show, ShowLabel = lab, ShowOpeningRange = or, Name = name, BoxColor = box, LabelColor = lc };
        }

        private double PointSize()
        {
            // UNKNOWN TV toPips: distance / PointSize. Quantower TickSize is the analog — no 10×.
            if (Symbol == null) return 0.0001;
            double t = Symbol.TickSize;
            return t > 0 ? t : 0.0001;
        }

        private string TfLabel()
        {
            ChartPeriodInfo p = ReadPeriod();
            if (p.IsDaily) return "1D";
            if (p.IsSecond) return ((int)p.Duration.TotalSeconds) + "S";
            if (p.Minutes >= 60 && p.Minutes % 60 == 0) return (p.Minutes / 60) + "H";
            return p.Minutes + "m";
        }

        private ChartPeriodInfo ReadPeriod()
        {
            TimeSpan dur = TimeSpan.FromMinutes(15);
            if (HistoricalData != null && HistoricalData.Aggregation is HistoryAggregationTime hat)
                dur = hat.Period.Duration;
            double minutes = dur.TotalMinutes;
            bool isSecond = dur.TotalMinutes < 1 && dur.TotalSeconds > 0;
            bool isDaily = dur.TotalDays >= 1 && dur.TotalDays < 7;
            bool isMinute = !isSecond && minutes >= 1 && minutes < 1440;
            return new ChartPeriodInfo { Duration = dur, Minutes = minutes, IsSecond = isSecond, IsMinute = isMinute, IsDaily = isDaily };
        }

        private static DateTime StartOfUtcDay(DateTime t)
        {
            DateTime u = t.Kind == DateTimeKind.Utc ? t : DateTime.SpecifyKind(t, DateTimeKind.Utc);
            return new DateTime(u.Year, u.Month, u.Day, 0, 0, 0, DateTimeKind.Utc);
        }

        private struct ChartPeriodInfo
        {
            public TimeSpan Duration;
            public double Minutes;
            public bool IsSecond;
            public bool IsMinute;
            public bool IsDaily;
        }

        private sealed class ChartBarsView : HistoricalBarsView
        {
            private readonly PVRSAIndicator _ind;
            public ChartBarsView(PVRSAIndicator ind) { _ind = ind; }
            public override int Count => _ind.Count;
            public override DateTime TimeAt(int oldestFirstIndex)
            {
                int offset = _ind.Count - 1 - oldestFirstIndex;
                return _ind.Time(offset);
            }
            public override double HighAt(int oldestFirstIndex)
            {
                int offset = _ind.Count - 1 - oldestFirstIndex;
                return _ind.High(offset);
            }
            public override double LowAt(int oldestFirstIndex)
            {
                int offset = _ind.Count - 1 - oldestFirstIndex;
                return _ind.Low(offset);
            }
        }
    }
}
