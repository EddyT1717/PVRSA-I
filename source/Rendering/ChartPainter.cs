#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using TradingPlatform.BusinessLayer;

namespace PVRSAIndicator
{
    public delegate double ChartXFn(DateTime time);
    public delegate double ChartYFn(double price);

    /// <summary>
    /// GDI+ overlay painter for pivots, ranges, sessions, Psy, daily open, VCZ, labels, tables.
    /// </summary>
    public static class ChartPainter
    {
        public static void Paint(
            Graphics g,
            RectangleF clip,
            ChartXFn getX,
            ChartYFn getY,
            List<HorizontalLevel> levels,
            List<SessionDraw> sessions,
            List<ZoneBox> zonesAbove,
            List<ZoneBox> zonesBelow,
            List<ChartLabel> labels,
            List<TableRow> adrRows,
            TablePosition adrPos,
            Color adrBg,
            Color adrFg,
            List<TableRow> dstRows,
            TablePosition dstPos,
            Color dstBg,
            Color dstFg,
            int zoneBorderWidth,
            float labelFontSize)
        {
            if (g == null || getX == null || getY == null) return;

            RectangleF prev = g.ClipBounds;
            g.SetClip(clip);

            foreach (SessionDraw s in sessions)
            {
                if (!s.ShowBox || double.IsNaN(s.High) || double.IsNaN(s.Low)) continue;
                FillPriceRect(g, getX, getY, s.Start, s.End, s.High, s.Low, s.BoxColor);
            }

            foreach (ZoneBox z in zonesBelow)
                FillZone(g, getX, getY, z, zoneBorderWidth);
            foreach (ZoneBox z in zonesAbove)
                FillZone(g, getX, getY, z, zoneBorderWidth);

            foreach (HorizontalLevel lv in levels)
            {
                if (double.IsNaN(lv.Price)) continue;
                float y = (float)getY(lv.Price);
                float x1;
                float x2;
                if (lv.Extend == "both")
                {
                    x1 = clip.Left;
                    x2 = clip.Right;
                }
                else if (lv.Extend == "none")
                {
                    x1 = (float)getX(lv.StartTime);
                    DateTime end = lv.EndTime ?? DateTime.UtcNow;
                    x2 = (float)getX(end);
                }
                else
                {
                    x1 = (float)getX(lv.StartTime);
                    x2 = clip.Right;
                }
                using (Pen pen = MakePen(lv.Color, lv.Width, lv.Style))
                    g.DrawLine(pen, x1, y, x2, y);
                if (lv.ShowLabel && !string.IsNullOrEmpty(lv.Label))
                {
                    Color lc = lv.LabelColor.A == 0 ? lv.Color : lv.LabelColor;
                    using (Brush br = new SolidBrush(lc))
                    using (Font font = new Font("Segoe UI", (float)labelFontSize, FontStyle.Regular))
                    {
                        SizeF sz = g.MeasureString(lv.Label, font);
                        g.DrawString(lv.Label, font, br, Math.Min(x2, clip.Right) - sz.Width - 4, y - sz.Height - 1);
                    }
                }
            }

            using (Font labFont = new Font("Segoe UI", (float)labelFontSize, FontStyle.Regular))
            {
                foreach (ChartLabel lab in labels)
                {
                    float x = (float)getX(lab.Time);
                    float y = (float)getY(lab.Price);
                    using (Brush br = new SolidBrush(lab.Color))
                        g.DrawString(lab.Text, labFont, br, x + 2, y - 8);
                }
            }

            if (adrRows != null && adrRows.Count > 0)
                DrawTable(g, clip, adrPos, adrBg, adrFg, "Range", adrRows, false);
            if (dstRows != null && dstRows.Count > 0)
                DrawTable(g, clip, dstPos, dstBg, dstFg, "DST", dstRows, true);

            g.SetClip(prev);
        }

        private static void FillZone(Graphics g, ChartXFn getX, ChartYFn getY, ZoneBox z, int border)
        {
            float x1 = (float)getX(z.StartTime);
            float x2 = (float)getX(z.EndTime);
            if (x2 < x1 + 3) x2 = x1 + 6;
            float y1 = (float)getY(z.Top);
            float y2 = (float)getY(z.Bot);
            float top = Math.Min(y1, y2);
            float h = Math.Abs(y2 - y1);
            if (h < 1) h = 1;
            using (Brush br = new SolidBrush(z.Color))
                g.FillRectangle(br, x1, top, Math.Max(3, x2 - x1), h);
            if (border > 0)
            {
                using (Pen pen = new Pen(Color.FromArgb(Math.Min(255, z.Color.A + 40), z.Color), border))
                    g.DrawRectangle(pen, x1, top, Math.Max(3, x2 - x1), h);
            }
        }

        private static void FillPriceRect(Graphics g, ChartXFn getX, ChartYFn getY, DateTime t1, DateTime t2, double hi, double lo, Color c)
        {
            float x1 = (float)getX(t1);
            float x2 = (float)getX(t2);
            float y1 = (float)getY(hi);
            float y2 = (float)getY(lo);
            using (Brush br = new SolidBrush(c))
                g.FillRectangle(br, Math.Min(x1, x2), Math.Min(y1, y2), Math.Max(2, Math.Abs(x2 - x1)), Math.Max(2, Math.Abs(y2 - y1)));
        }

        private static Pen MakePen(Color color, int width, LineStyle style)
        {
            Pen pen = new Pen(color, width < 1 ? 1 : width);
            if (style == LineStyle.Dash) pen.DashStyle = DashStyle.Dash;
            else if (style == LineStyle.Dot) pen.DashStyle = DashStyle.Dot;
            else if (style == LineStyle.DashDot) pen.DashStyle = DashStyle.DashDot;
            else pen.DashStyle = DashStyle.Solid;
            return pen;
        }

        private static void DrawTable(
            Graphics g,
            RectangleF clip,
            TablePosition pos,
            Color bg,
            Color fg,
            string title,
            List<TableRow> rows,
            bool wide)
        {
            float w = wide ? 420 : 168;
            float rowH = 16;
            float h = 22 + rows.Count * rowH + 8;
            float x, y;
            Anchor(clip, pos, w, h, out x, out y);
            using (Brush br = new SolidBrush(bg))
                g.FillRectangle(br, x, y, w, h);
            using (Pen border = new Pen(Color.FromArgb(40, 244, 244, 245)))
                g.DrawRectangle(border, x, y, w, h);
            using (Font titleFont = new Font("Segoe UI", 7f, FontStyle.Bold))
            using (Font bodyFont = new Font("Consolas", 8f, FontStyle.Regular))
            using (Brush text = new SolidBrush(fg))
            {
                g.DrawString(title, titleFont, text, x + 8, y + 4);
                for (int i = 0; i < rows.Count; i++)
                {
                    TableRow r = rows[i];
                    float yy = y + 20 + i * rowH;
                    if (!string.IsNullOrEmpty(r.Name))
                        g.DrawString(r.Name, bodyFont, text, x + 8, yy);
                    SizeF sz = g.MeasureString(r.Value, bodyFont);
                    float vx = string.IsNullOrEmpty(r.Name) ? x + 8 : x + w - 8 - sz.Width;
                    g.DrawString(r.Value, bodyFont, text, vx, yy);
                }
            }
        }

        private static void Anchor(RectangleF clip, TablePosition pos, float w, float h, out float x, out float y)
        {
            float pad = 10;
            switch (pos)
            {
                case TablePosition.TopLeft:
                    x = clip.Left + pad; y = clip.Top + pad; break;
                case TablePosition.TopCenter:
                    x = clip.Left + (clip.Width - w) / 2; y = clip.Top + pad; break;
                case TablePosition.TopRight:
                    x = clip.Right - w - 64; y = clip.Top + pad; break;
                case TablePosition.BottomLeft:
                    x = clip.Left + pad; y = clip.Bottom - h - 28; break;
                case TablePosition.BottomCenter:
                    x = clip.Left + (clip.Width - w) / 2; y = clip.Bottom - h - 28; break;
                default:
                    x = clip.Right - w - 64; y = clip.Bottom - h - 28; break;
            }
        }
    }
}
