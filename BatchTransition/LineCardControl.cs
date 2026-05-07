using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace BatchTransition
{
    public class LineCardControl : Panel
    {
        public ProductionLine Line { get; }
        public BatchPropagationService Propagation { get; }
        public bool IsSelected { get; set; }

        private const int CARD_PADDING = 10;
        private const int HEADER_H = 28;
        private const int INFO_H = 26;
        private const int PROG_H = 16;
        private const int PIPE_TOP = 80;
        private const int PIPE_H = 44;
        private const int BOX_W = 96;
        private const int BOX_H = 36;
        private const int ARROW_W = 18;

        private static readonly (string name, string abbr)[] MachineLabels =
        {
            ("Filler",      "FIL"),
            ("Checkweigher","CHK"),
            ("Capper",      "CAP"),
            ("Labeller",    "LBL"),
            ("Case Packer", "C.PK"),
            ("Case Erector","C.ER"),
            ("Case Sealer", "C.SE"),
            ("Cobot",       "CBT"),
        };

        private static readonly Color C_HEADER_RUN = Color.FromArgb(0, 110, 60);
        private static readonly Color C_HEADER_IDLE = Color.FromArgb(80, 90, 110);
        private static readonly Color C_BOX_RUN = Color.FromArgb(40, 180, 90);
        private static readonly Color C_BOX_EMPTY = Color.FromArgb(195, 200, 210);
        private static readonly Color C_BOX_BORDER = Color.FromArgb(140, 150, 165);
        private static readonly Color C_PROG_FILL = Color.FromArgb(40, 160, 220);
        private static readonly Color C_PROG_BG = Color.FromArgb(210, 215, 225);
        private static readonly Color C_CARD_BG = Color.White;
        private static readonly Color C_CARD_SEL = Color.FromArgb(210, 235, 255);
        private static readonly Color C_TEXT_DIM = Color.FromArgb(120, 125, 135);

        private readonly Font _fHeader = new Font("Segoe UI", 10f, FontStyle.Bold);
        private readonly Font _fInfo = new Font("Segoe UI", 9f, FontStyle.Regular);
        private readonly Font _fInfoBold = new Font("Segoe UI", 9f, FontStyle.Bold);
        private readonly Font _fMach = new Font("Segoe UI", 7.5f, FontStyle.Bold);
        private readonly Font _fAbbr = new Font("Segoe UI", 8.5f, FontStyle.Regular);
        private readonly StringFormat _sfCenter = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter
        };

        public LineCardControl(ProductionLine line, BatchPropagationService prop)
        {
            Line = line;
            Propagation = prop;
            Height = 138;
            Margin = new Padding(6, 6, 6, 0);
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);
        }

        public new void Refresh() => Invalidate();

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var bounds = new Rectangle(2, 2, Width - 4, Height - 4);

            using (var br = new SolidBrush(IsSelected ? C_CARD_SEL : C_CARD_BG))
                g.FillRoundedRect(br, bounds, 8);
            using (var pen = new Pen(IsSelected
                ? Color.FromArgb(80, 140, 220)
                : Color.FromArgb(200, 205, 215), IsSelected ? 2f : 1f))
                g.DrawRoundedRect(pen, bounds, 8);

            var (batch, item, po, _, _) = Propagation.GetDisplayInfo(Line.Filler);
            int counter = Line.Filler.CounterOut;
            int target = Line.Filler.Buffer.Count > 0
                          ? Line.Filler.Buffer.Peek().OriginalQty : 0;
            bool running = batch != "-";
            float progress = (target > 0 && running)
                           ? Math.Min(1f, (float)counter / target) : 0f;

            var headerRect = new Rectangle(bounds.X, bounds.Y, bounds.Width, HEADER_H);
            using (var br = new SolidBrush(running ? C_HEADER_RUN : C_HEADER_IDLE))
                g.FillRoundedRectTop(br, headerRect, 8);

            using (var br = new SolidBrush(Color.White))
                g.DrawString(Line.Name.ToUpper(), _fHeader, br,
                    new RectangleF(bounds.X + 12, bounds.Y + 4, 120, HEADER_H - 4));

            string statusTxt = running ? "▶  RUNNING" : "⏸  IDLE";
            using (var badgeBr = new SolidBrush(Color.FromArgb(60, 255, 255, 255)))
            {
                var badgeRect = new RectangleF(bounds.X + 130, bounds.Y + 6, 100, 18);
                g.FillRectangle(badgeBr, badgeRect);
                using (var br = new SolidBrush(Color.White))
                    g.DrawString(statusTxt, _fInfo, br, badgeRect, _sfCenter);
            }

            if (running && target > 0)
            {
                string ctrTxt = $"{counter:N0} / {target:N0}  ({progress * 100:F0}%)";
                using (var br = new SolidBrush(Color.FromArgb(210, 255, 255, 255)))
                {
                    var r = new RectangleF(bounds.X + 240, bounds.Y + 4,
                                          bounds.Width - 252, HEADER_H - 8);
                    g.DrawString(ctrTxt, _fInfoBold, br, r,
                        new StringFormat
                        {
                            Alignment = StringAlignment.Far,
                            LineAlignment = StringAlignment.Center
                        });
                }
            }

            int iy = bounds.Y + HEADER_H + 6;
            if (running)
            {
                DrawInfoField(g, "BATCH", batch, bounds.X + 12, iy,
                    (int)(bounds.Width * 0.36));

                DrawInfoField(g, "ITEM", item, bounds.X + 12 + (int)(bounds.Width * 0.37), iy,
                    (int)(bounds.Width * 0.25));

                DrawInfoField(g, "PO", po,
                    bounds.X + 12 + (int)(bounds.Width * 0.63), iy,
                    bounds.Width - (int)(bounds.Width * 0.64) - 14);
            }
            else
            {
                using (var br = new SolidBrush(C_TEXT_DIM))
                    g.DrawString("Không có batch đang chạy", _fInfo, br,
                        new RectangleF(bounds.X + 12, iy, bounds.Width - 20, INFO_H),
                        new StringFormat { LineAlignment = StringAlignment.Center });
            }

            int pBy = bounds.Y + HEADER_H + INFO_H + 10;
            var progBg = new Rectangle(bounds.X + 12, pBy,
                                       bounds.Width - 24, PROG_H);
            using (var br = new SolidBrush(C_PROG_BG))
                g.FillRoundedRect(br, progBg, 4);
            if (progress > 0)
            {
                var progFill = new Rectangle(progBg.X, progBg.Y,
                    (int)(progBg.Width * progress), PROG_H);
                using (var br = new SolidBrush(C_PROG_FILL))
                    g.FillRoundedRect(br, progFill, 4);
            }

            var allMachines = new List<Machine> { Line.Filler };
            allMachines.AddRange(Line.Machines);

            int pipeTotal = allMachines.Count * BOX_W + (allMachines.Count - 1) * ARROW_W;
            int pipeLeft = bounds.X + (bounds.Width - pipeTotal) / 2;
            if (pipeLeft < bounds.X + 8) pipeLeft = bounds.X + 8;
            int pipeTop = PIPE_TOP + bounds.Y;

            for (int i = 0; i < allMachines.Count; i++)
            {
                var m = allMachines[i];
                bool hasBatch = m.Buffer.Count > 0;
                int bx = pipeLeft + i * (BOX_W + ARROW_W);

                if (i > 0)
                {
                    int ax = bx - ARROW_W;
                    int ay = pipeTop + BOX_H / 2;
                    using (var pen = new Pen(C_BOX_BORDER, 1.5f))
                    {
                        pen.EndCap = LineCap.Round;
                        g.DrawLine(pen, ax, ay, bx - 2, ay);
                    }

                    var pts = new[]
                    {
                        new Point(bx - 2, ay),
                        new Point(bx - 7, ay - 4),
                        new Point(bx - 7, ay + 4),
                    };
                    using (var br = new SolidBrush(C_BOX_BORDER))
                        g.FillPolygon(br, pts);
                }


                var boxRect = new Rectangle(bx, pipeTop, BOX_W, BOX_H);
                Color boxColor = hasBatch ? C_BOX_RUN : C_BOX_EMPTY;
                Color textColor = hasBatch ? Color.White : Color.FromArgb(80, 85, 100);

                using (var br = new SolidBrush(boxColor))
                    g.FillRoundedRect(br, boxRect, 5);
                using (var pen = new Pen(hasBatch
                    ? Color.FromArgb(30, 140, 70) : C_BOX_BORDER, 1f))
                    g.DrawRoundedRect(pen, boxRect, 5);

                var topHalf = new RectangleF(bx, pipeTop, BOX_W, BOX_H * 0.55f);
                using (var br = new SolidBrush(textColor))
                    g.DrawString(MachineLabels[i].abbr, _fMach, br, topHalf, _sfCenter);

                var botHalf = new RectangleF(bx, pipeTop + BOX_H * 0.52f,
                                             BOX_W, BOX_H * 0.45f);
                using (var br = new SolidBrush(hasBatch
                    ? Color.FromArgb(220, 255, 220) : C_TEXT_DIM))
                    g.DrawString(MachineLabels[i].name, _fAbbr, br, botHalf, _sfCenter);
            }

            base.OnPaint(e);
        }

        private void DrawInfoField(Graphics g, string label, string value,
            int x, int y, int w)
        {
            using (var lb = new SolidBrush(C_TEXT_DIM))
                g.DrawString(label + ": ", _fInfo, lb,
                    new RectangleF(x, y, 45, INFO_H),
                    new StringFormat { LineAlignment = StringAlignment.Center });

            using (var vb = new SolidBrush(Color.FromArgb(30, 35, 45)))
                g.DrawString(value, _fInfoBold, vb,
                    new RectangleF(x + 44, y, w - 44, INFO_H),
                    new StringFormat
                    {
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter
                    });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fHeader?.Dispose(); _fInfo?.Dispose(); _fInfoBold?.Dispose();
                _fMach?.Dispose(); _fAbbr?.Dispose(); _sfCenter?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    internal static class GfxExt
    {
        public static void FillRoundedRect(this Graphics g, Brush br,
            Rectangle r, int radius)
        {
            using (var path = RoundedPath(r, radius))
                g.FillPath(br, path);
        }

        public static void DrawRoundedRect(this Graphics g, Pen pen,
            Rectangle r, int radius)
        {
            using (var path = RoundedPath(r, radius))
                g.DrawPath(pen, path);
        }

        public static void FillRoundedRectTop(this Graphics g, Brush br,
            Rectangle r, int radius)
        {
            using (var path = new GraphicsPath())
            {
                path.AddArc(r.X, r.Y, radius * 2, radius * 2, 180, 90);
                path.AddArc(r.Right - radius * 2, r.Y, radius * 2, radius * 2, 270, 90);
                path.AddLine(r.Right, r.Bottom, r.X, r.Bottom);
                path.CloseFigure();
                g.FillPath(br, path);
            }
        }

        private static GraphicsPath RoundedPath(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
