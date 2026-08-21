using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Caissa;

public partial class MainMenu : Form
{
    // Identifies a row for icon/accent purposes. Distinct from GameMode,
    // which is the underlying rule set the engine uses — "Standard vs
    // Player" and "Standard vs Computer" are both GameMode.Standard, just
    // launched with a different opponent.
    private enum MenuOption { StandardPlayer, StandardComputer, Chess960, Atomic }

    // ── Layout Constants ─────────────────────────────────────────────────────
    const int WindowW  = 500;
    const int WindowH  = 620;
    const int MarginL  = 40;
    const int ContentW = 420;
    const int RowH     = 88;
    const int RowGap   = 14;

    readonly List<ModeRow> _modeRows = new();

    Label _titleLabel = null!;
    Label _blurbLabel = null!;
    Label _sectionLabel = null!;
    Label _footerLabel = null!;

    ThemeToggle _themeToggle = null!;
    ToolTip _toggleTip = null!;

    public MainMenu()
    {
        Text            = "Caissa";
        ClientSize      = new Size(WindowW, WindowH);
        StartPosition   = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;
        BackColor       = Theme.BgColor;
        Font            = new Font("Segoe UI", 10F);
        DoubleBuffered  = true;

        BuildLayout();

        // Repaint whenever the theme changes — whether that happened from
        // this window's own toggle or from the one on the chess board.
        Theme.ThemeChanged += ApplyTheme;
    }

    private void BuildLayout()
    {
        // ── Header ────────────────────────────────────────────────────────
        _titleLabel = new Label
        {
            Text = "Caissa",
            Font = new Font("Segoe UI", 28, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            BackColor = Color.Transparent,
            AutoSize = false,
            Width = ContentW,
            Height = 50,
            Top = 32,
            Left = MarginL,
            TextAlign = ContentAlignment.MiddleCenter,
        };

        Controls.Add(_titleLabel);

        _blurbLabel = new Label
        {
            Text = "A clean, modern client for playing chess — standard rules, Chess960, and Atomic variants, all in one place.",
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Theme.TextSecondary,
            AutoSize = false,
            Width = ContentW,
            Height = 40,
            Left = MarginL,
            Top = 80,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Theme.BgColor,
        };

        Controls.Add(_blurbLabel);

        _sectionLabel = new Label
        {
            Text = "Select a mode",
            Font = new Font("Segoe UI Semibold", 9.5F),
            ForeColor = Theme.TextSecondary,
            AutoSize = false,
            Width = 200,
            Height = 18,
            Left = MarginL,
            Top = 134,
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = Theme.BgColor,
        };

        Controls.Add(_sectionLabel);

        // ── Theme Toggle ──────────────────────────────────────────────────
        const int toggleWidth = 35;
        const int toggleHeight = 35;

        _themeToggle = new ThemeToggle
        {
            Left = MarginL + ContentW - toggleWidth - 10,
            Top = 35 + (50 - toggleHeight) / 2,
            Width = toggleWidth,
            Height = toggleHeight,
        };

        Controls.Add(_themeToggle);
        _themeToggle.BringToFront();

        _toggleTip = new ToolTip();
        _toggleTip.SetToolTip(_themeToggle, Theme.IsDarkMode ? "Switch to light mode" : "Switch to dark mode");

        // ── Mode rows ─────────────────────────────────────────────────────
        int rowTop = 160;
        int step = RowH + RowGap;

        AddModeRow(
            "Standard",
            "Classic rules — play a friend, side by side.",
            MenuOption.StandardPlayer,
            GameMode.Standard,
            vsComputer: false,
            top: rowTop
        );

        AddModeRow(
            "Standard · vs Computer",
            "Classic rules — test yourself against the built-in engine.",
            MenuOption.StandardComputer,
            GameMode.Standard,
            vsComputer: true,
            top: rowTop + step
        );

        AddModeRow(
            "Chess960",
            "Fischer Random — the back rank is shuffled each game.",
            MenuOption.Chess960,
            GameMode.Chess960,
            vsComputer: false,
            top: rowTop + step * 2
        );

        AddModeRow(
            "Atomic Chess",
            "Captures explode, clearing out nearby pieces too.",
            MenuOption.Atomic,
            GameMode.AtomicChess,
            vsComputer: false,
            top: rowTop + step * 3
        );

        // ── Footer ────────────────────────────────────────────────────────
        _footerLabel = new Label
        {
            Text = "Press Esc during a game to return to this menu",
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Theme.TextTertiary,
            AutoSize = false,
            Width = WindowW,
            Height = 20,
            Top = rowTop + step * 3 + RowH + 26,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Theme.BgColor,
        };

        Controls.Add(_footerLabel);
    }

    private void AddModeRow(
        string title,
        string desc,
        MenuOption option,
        GameMode engineMode,
        bool vsComputer,
        int top)
    {
        var row = new ModeRow(title, desc, option)
        {
            Left = MarginL,
            Top = top,
            Width = ContentW,
            Height = RowH,
        };

        row.ClickAction = () =>
        {
            var game = new ChessBoard(engineMode, vsComputer);

            // Theme.ThemeChanged keeps this window in sync live, even
            // while it's hidden behind the board — nothing to resync here.
            game.FormClosed += (_, __) => Show();

            Hide();
            game.Show();
        };

        Controls.Add(row);
        _modeRows.Add(row);
    }

    private static Color AccentFor(MenuOption option) => option switch
    {
        MenuOption.StandardPlayer   => Theme.AccentFor(GameMode.Standard),
        MenuOption.StandardComputer => Theme.ComputerAccent,
        MenuOption.Chess960         => Theme.AccentFor(GameMode.Chess960),
        MenuOption.Atomic           => Theme.AccentFor(GameMode.AtomicChess),
        _                           => Theme.AccentFor(GameMode.Standard),
    };

    // ── Theme Application ────────────────────────────────────────────────────
    private void ApplyTheme()
    {
        BackColor = Theme.BgColor;

        _titleLabel.ForeColor = Theme.TextPrimary;
        _titleLabel.BackColor = Theme.BgColor;

        _blurbLabel.ForeColor = Theme.TextSecondary;
        _blurbLabel.BackColor = Theme.BgColor;

        _sectionLabel.ForeColor = Theme.TextSecondary;
        _sectionLabel.BackColor = Theme.BgColor;

        _footerLabel.ForeColor = Theme.TextTertiary;
        _footerLabel.BackColor = Theme.BgColor;

        foreach (var row in _modeRows)
        {
            row.BackColor = Theme.BgColor;
            row.Invalidate();
        }

        _themeToggle.BackColor = Theme.BgColor;
        _themeToggle.Invalidate();

        _toggleTip.SetToolTip(
            _themeToggle,
            Theme.IsDarkMode ? "Switch to light mode" : "Switch to dark mode"
        );

        Invalidate(true);
        _themeToggle.BringToFront();
        Update();
    }

    // ── Shared Icon Drawing Helpers ─────────────────────────────────────────
    private static void DrawArrowhead(Graphics g, Brush brush, PointF tip, double angle)
    {
        const float size = 4.5f;

        var p2 = new PointF(
            tip.X - (float)(size * Math.Cos(angle - 0.5)),
            tip.Y - (float)(size * Math.Sin(angle - 0.5))
        );

        var p3 = new PointF(
            tip.X - (float)(size * Math.Cos(angle + 0.5)),
            tip.Y - (float)(size * Math.Sin(angle + 0.5))
        );

        g.FillPolygon(brush, new[] { tip, p2, p3 });
    }

    private static PointF[] StarPoints(float cx, float cy, float outerR, float innerR, int points)
    {
        var pts = new PointF[points * 2];

        double step = Math.PI / points;
        double angle = -Math.PI / 2;

        for (int i = 0; i < points * 2; i++)
        {
            float r = (i % 2 == 0) ? outerR : innerR;

            pts[i] = new PointF(
                cx + (float)(r * Math.Cos(angle)),
                cy + (float)(r * Math.Sin(angle))
            );

            angle += step;
        }

        return pts;
    }

    private static void DrawModeIcon(Graphics g, RectangleF chip, MenuOption option, Color accent)
    {
        float cx = chip.X + chip.Width / 2f;
        float cy = chip.Y + chip.Height / 2f;

        using var brush = new SolidBrush(accent);

        switch (option)
        {
            case MenuOption.StandardPlayer:

                g.FillEllipse(brush, cx - 5, cy - 13, 10, 10);

                g.FillPolygon(
                    brush,
                    new[]
                    {
                        new PointF(cx - 3, cy - 4),
                        new PointF(cx + 3, cy - 4),
                        new PointF(cx + 7, cy + 8),
                        new PointF(cx - 7, cy + 8),
                    }
                );

                g.FillRectangle(brush, cx - 9, cy + 8, 18, 4);

                break;

            case MenuOption.StandardComputer:

                // A small monitor silhouette makes it unmistakably "vs the machine".
                var body = new RectangleF(cx - 12, cy - 11, 24, 16);

                using (var bodyPath = Theme.RoundedRect(body, 2.5f))
                {
                    g.FillPath(brush, bodyPath);
                }

                g.FillRectangle(brush, cx - 3, cy + 5, 6, 5);
                g.FillRectangle(brush, cx - 9, cy + 10, 18, 3);

                using (var dotPen = new Pen(accent, 1.6f))
                {
                    g.DrawEllipse(dotPen, cx + 9, cy - 17, 6, 6);
                    g.DrawEllipse(dotPen, cx + 12, cy - 20, 3, 3);
                }

                break;

            case MenuOption.Chess960:

                using (var pen = new Pen(accent, 2.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    g.DrawLine(pen, cx - 13, cy - 8, cx + 11, cy + 8);
                    g.DrawLine(pen, cx - 13, cy + 8, cx + 11, cy - 8);
                }

                DrawArrowhead(g, brush, new PointF(cx + 13, cy + 9), Math.Atan2(16, 24));
                DrawArrowhead(g, brush, new PointF(cx + 13, cy - 9), Math.Atan2(-16, 24));

                break;

            case MenuOption.Atomic:

                g.FillPolygon(brush, StarPoints(cx, cy, 13, 6, 8));

                break;
        }
    }

    // ── ModeRow ──────────────────────────────────────────────────────────────
    private class ModeRow : Control
    {
        readonly string _title;
        readonly string _desc;
        readonly MenuOption _option;

        bool _hover;
        bool _pressed;

        public Action? ClickAction;

        static readonly Font TitleFont = new("Segoe UI Semibold", 12.5F);
        static readonly Font DescFont = new("Segoe UI", 9.5F);

        public ModeRow(string title, string desc, MenuOption option)
        {
            _title = title;
            _desc = desc;
            _option = option;

            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true
            );

            Cursor = Cursors.Hand;
            BackColor = Theme.BgColor;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hover = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = false;
            _pressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _pressed = true;
            Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _pressed = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            ClickAction?.Invoke();
            base.OnMouseClick(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            const float radius = 10f;

            var accent = AccentFor(_option);

            var cardRect = new RectangleF(0, 0, Width - 4, Height - 8);

            Theme.DrawCardShadow(g, cardRect, radius, _hover);

            using (var path = Theme.RoundedRect(cardRect, radius))
            {
                using (var bg = new SolidBrush(_hover ? Theme.CardHoverBg : Theme.CardBg))
                {
                    g.FillPath(bg, path);
                }

                if (_pressed)
                {
                    using var pressTint = new SolidBrush(Color.FromArgb(Theme.IsDarkMode ? 26 : 14, 0, 0, 0));
                    g.FillPath(pressTint, path);
                }

                using var pen = new Pen(_hover ? Color.FromArgb(150, accent) : Theme.BorderColor, 1.3f);
                g.DrawPath(pen, path);
            }

            // Icon chip
            var chipRect = new RectangleF(16, (cardRect.Height - 44) / 2, 44, 44);

            int chipAlpha = Theme.IsDarkMode ? 45 : 30;

            using (var chipPath = Theme.RoundedRect(chipRect, 10f))
            using (var chipBg = new SolidBrush(Color.FromArgb(chipAlpha, accent)))
            {
                g.FillPath(chipBg, chipPath);
            }

            DrawModeIcon(g, chipRect, _option, accent);

            // Text block
            float textLeft = chipRect.Right + 14;
            float textWidth = cardRect.Width - textLeft - 40;

            var descSize = g.MeasureString(_desc, DescFont, (int)textWidth);
            float titleH = TitleFont.GetHeight(g);
            const float spacing = 4f;
            float blockH = titleH + spacing + descSize.Height;
            float blockTop = (cardRect.Height - blockH) / 2f;

            using (var titleFmt = new StringFormat
                   {
                       Trimming = StringTrimming.EllipsisCharacter,
                       FormatFlags = StringFormatFlags.NoWrap
                   })
            using (var tbr = new SolidBrush(Theme.TextPrimary))
            {
                g.DrawString(
                    _title,
                    TitleFont,
                    tbr,
                    new RectangleF(textLeft, blockTop, textWidth, titleH),
                    titleFmt
                );
            }

            using (var dbr = new SolidBrush(Theme.TextSecondary))
            {
                g.DrawString(
                    _desc,
                    DescFont,
                    dbr,
                    new RectangleF(textLeft, blockTop + titleH + spacing, textWidth, descSize.Height)
                );
            }

            // Trailing chevron
            using (var cf = new Font("Segoe UI", 14F))
            using (var cbr = new SolidBrush(_hover ? accent : Theme.TextTertiary))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Far,
                    LineAlignment = StringAlignment.Center
                };

                g.DrawString("›", cf, cbr, new RectangleF(0, 0, cardRect.Width - 16, cardRect.Height), sf);
            }
        }
    }
}