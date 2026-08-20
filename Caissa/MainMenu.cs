using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Caissa;

public enum GameMode { Standard, Chess960, AtomicChess }

public partial class MainMenu : Form
{
    // ── Design Tokens ────────────────────────────────────────────────────────
    public static bool IsDarkMode = false;

    // Light palette
    static readonly Color LightBg            = Color.FromArgb(247, 247, 248);
    static readonly Color LightCard          = Color.White;
    static readonly Color LightBorder        = Color.FromArgb(228, 228, 231);
    static readonly Color LightTextPrimary   = Color.FromArgb(31,  35,  40);
    static readonly Color LightTextSecondary = Color.FromArgb(110, 118, 129);
    static readonly Color LightTextTertiary  = Color.FromArgb(145, 152, 161);

    // Dark palette
    static readonly Color DarkBg             = Color.FromArgb(13,  17,  23);
    static readonly Color DarkCard           = Color.FromArgb(22,  27,  34);
    static readonly Color DarkCardHover      = Color.FromArgb(30,  36,  44);
    static readonly Color DarkBorder         = Color.FromArgb(48,  54,  61);
    static readonly Color DarkTextPrimary    = Color.FromArgb(230, 237, 243);
    static readonly Color DarkTextSecondary  = Color.FromArgb(139, 148, 158);
    static readonly Color DarkTextTertiary   = Color.FromArgb(101, 108, 118);

    public static Color BgColor        => IsDarkMode ? DarkBg : LightBg;
    public static Color CardBg         => IsDarkMode ? DarkCard : LightCard;
    public static Color CardHoverBg    => IsDarkMode ? DarkCardHover : LightCard;
    public static Color BorderColor    => IsDarkMode ? DarkBorder : LightBorder;
    public static Color TextPrimary    => IsDarkMode ? DarkTextPrimary : LightTextPrimary;
    public static Color TextSecondary  => IsDarkMode ? DarkTextSecondary : LightTextSecondary;
    public static Color TextTertiary   => IsDarkMode ? DarkTextTertiary : LightTextTertiary;
    public static Color HoverTint      => IsDarkMode
        ? Color.FromArgb(33, 38, 45)
        : Color.FromArgb(240, 240, 241);

    static readonly Color StandardAccentLight = Color.FromArgb(76,  154, 91);
    static readonly Color StandardAccentDark  = Color.FromArgb(63,  185, 80);
    static readonly Color Chess960AccentLight = Color.FromArgb(91,  111, 203);
    static readonly Color Chess960AccentDark  = Color.FromArgb(130, 148, 255);
    static readonly Color AtomicAccentLight   = Color.FromArgb(224, 98,  63);
    static readonly Color AtomicAccentDark    = Color.FromArgb(255, 138, 101);

    public static Color AccentFor(GameMode m) => m switch
    {
        GameMode.Standard    => IsDarkMode ? StandardAccentDark : StandardAccentLight,
        GameMode.Chess960    => IsDarkMode ? Chess960AccentDark : Chess960AccentLight,
        GameMode.AtomicChess => IsDarkMode ? AtomicAccentDark   : AtomicAccentLight,
        _                    => IsDarkMode ? StandardAccentDark : StandardAccentLight,
    };

    // ── Layout Constants ─────────────────────────────────────────────────────
    const int WindowW  = 500;
    const int WindowH  = 530;
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
        Text           = "Caissa";
        ClientSize     = new Size(WindowW, WindowH);
        StartPosition  = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox    = false;
        BackColor      = BgColor;
        Font           = new Font("Segoe UI", 10F);
        DoubleBuffered = true;

        BuildLayout();
    }

    private void BuildLayout()
    {
        // ── Header ────────────────────────────────────────────────────────
        // Keep the title centered across the entire content area.
        _titleLabel = new Label
        {
            Text = "Caissa",
            Font = new Font("Segoe UI", 28, FontStyle.Bold),
            ForeColor = TextPrimary,
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
            ForeColor = TextSecondary,
            AutoSize = false,
            Width = ContentW,
            Height = 40,
            Left = MarginL,
            Top = 80,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = BgColor,
        };

        Controls.Add(_blurbLabel);

        _sectionLabel = new Label
        {
            Text = "Select a mode",
            Font = new Font("Segoe UI Semibold", 9.5F),
            ForeColor = TextSecondary,
            AutoSize = false,
            Width = 200,
            Height = 18,
            Left = MarginL,
            Top = 134,
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = BgColor,
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

        _themeToggle.ToggleAction = () =>
        {
            IsDarkMode = !IsDarkMode;
            ApplyTheme();
        };

        Controls.Add(_themeToggle);

        // Make sure the toggle is always drawn above the title.
        _themeToggle.BringToFront();

        _toggleTip = new ToolTip();
        _toggleTip.SetToolTip(
            _themeToggle,
            "Switch to dark mode"
        );

        // ── Mode rows ─────────────────────────────────────────────────────
        int rowTop = 160;

        AddModeRow(
            "Standard",
            "Classic rules and the traditional starting position.",
            GameMode.Standard,
            rowTop
        );

        AddModeRow(
            "Chess960",
            "Fischer Random — the back rank is shuffled each game.",
            GameMode.Chess960,
            rowTop + (RowH + RowGap)
        );

        AddModeRow(
            "Atomic Chess",
            "Captures explode, clearing out nearby pieces too.",
            GameMode.AtomicChess,
            rowTop + (RowH + RowGap) * 2
        );

        // ── Footer ────────────────────────────────────────────────────────
        _footerLabel = new Label
        {
            Text = "Press Esc during a game to return to this menu",
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = TextTertiary,
            AutoSize = false,
            Width = WindowW,
            Height = 20,
            Top = rowTop + (RowH + RowGap) * 2 + RowH + 26,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = BgColor,
        };

        Controls.Add(_footerLabel);
    }

    private void AddModeRow(
        string title,
        string desc,
        GameMode mode,
        int top)
    {
        var row = new ModeRow(title, desc, mode)
        {
            Left = MarginL,
            Top = top,
            Width = ContentW,
            Height = RowH,
        };

        row.ClickAction = () =>
        {
            var game = new ChessBoard(mode);

            game.FormClosed += (_, __) => Show();

            Hide();
            game.Show();
        };

        Controls.Add(row);
        _modeRows.Add(row);
    }

    // ── Theme Application ────────────────────────────────────────────────────
    private void ApplyTheme()
    {
        BackColor = BgColor;

        _titleLabel.ForeColor = TextPrimary;
        _titleLabel.BackColor = BgColor;

        _blurbLabel.ForeColor = TextSecondary;
        _blurbLabel.BackColor = BgColor;

        _sectionLabel.ForeColor = TextSecondary;
        _sectionLabel.BackColor = BgColor;

        _footerLabel.ForeColor = TextTertiary;
        _footerLabel.BackColor = BgColor;

        foreach (var row in _modeRows)
        {
            row.BackColor = BgColor;
            row.Invalidate();
        }

        _themeToggle.BackColor = BgColor;
        _themeToggle.Invalidate();

        _toggleTip.SetToolTip(
            _themeToggle,
            IsDarkMode
                ? "Switch to light mode"
                : "Switch to dark mode"
        );

        Invalidate(true);

        // Keep the toggle above the title after a theme change.
        _themeToggle.BringToFront();

        Update();
    }

    // ── Shared Drawing Helpers ───────────────────────────────────────────────
    private static GraphicsPath RoundedRect(
        RectangleF rect,
        float radius)
    {
        float d = radius * 2;

        var path = new GraphicsPath();

        path.AddArc(
            rect.X,
            rect.Y,
            d,
            d,
            180,
            90
        );

        path.AddArc(
            rect.Right - d,
            rect.Y,
            d,
            d,
            270,
            90
        );

        path.AddArc(
            rect.Right - d,
            rect.Bottom - d,
            d,
            d,
            0,
            90
        );

        path.AddArc(
            rect.X,
            rect.Bottom - d,
            d,
            d,
            90,
            90
        );

        path.CloseFigure();

        return path;
    }

    private static void DrawCardShadow(
        Graphics g,
        RectangleF rect,
        float radius,
        bool hover)
    {
        int layers = hover ? 4 : 2;

        for (int i = layers; i >= 1; i--)
        {
            float offset = hover
                ? i * 1.6f
                : i * 1.0f;

            int alpha = hover ? 10 : 6;

            var shadowRect = new RectangleF(
                rect.X,
                rect.Y + offset,
                rect.Width,
                rect.Height
            );

            using var path = RoundedRect(
                shadowRect,
                radius
            );

            using var brush = new SolidBrush(
                Color.FromArgb(
                    alpha,
                    20,
                    20,
                    20
                )
            );

            g.FillPath(brush, path);
        }
    }

    private static void DrawArrowhead(
        Graphics g,
        Brush brush,
        PointF tip,
        double angle)
    {
        const float size = 4.5f;

        var p2 = new PointF(
            tip.X - (float)(
                size * Math.Cos(angle - 0.5)
            ),
            tip.Y - (float)(
                size * Math.Sin(angle - 0.5)
            )
        );

        var p3 = new PointF(
            tip.X - (float)(
                size * Math.Cos(angle + 0.5)
            ),
            tip.Y - (float)(
                size * Math.Sin(angle + 0.5)
            )
        );

        g.FillPolygon(
            brush,
            new[] { tip, p2, p3 }
        );
    }

    private static PointF[] StarPoints(
        float cx,
        float cy,
        float outerR,
        float innerR,
        int points)
    {
        var pts = new PointF[points * 2];

        double step = Math.PI / points;
        double angle = -Math.PI / 2;

        for (int i = 0; i < points * 2; i++)
        {
            float r = (i % 2 == 0)
                ? outerR
                : innerR;

            pts[i] = new PointF(
                cx + (float)(r * Math.Cos(angle)),
                cy + (float)(r * Math.Sin(angle))
            );

            angle += step;
        }

        return pts;
    }

    private static void DrawModeIcon(
        Graphics g,
        RectangleF chip,
        GameMode mode,
        Color accent)
    {
        float cx = chip.X + chip.Width / 2f;
        float cy = chip.Y + chip.Height / 2f;

        using var brush = new SolidBrush(accent);

        switch (mode)
        {
            case GameMode.Standard:

                g.FillEllipse(
                    brush,
                    cx - 5,
                    cy - 13,
                    10,
                    10
                );

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

                g.FillRectangle(
                    brush,
                    cx - 9,
                    cy + 8,
                    18,
                    4
                );

                break;

            case GameMode.Chess960:

                using (var pen = new Pen(
                           accent,
                           2.2f)
                       {
                           StartCap = LineCap.Round,
                           EndCap = LineCap.Round
                       })
                {
                    g.DrawLine(
                        pen,
                        cx - 13,
                        cy - 8,
                        cx + 11,
                        cy + 8
                    );

                    g.DrawLine(
                        pen,
                        cx - 13,
                        cy + 8,
                        cx + 11,
                        cy - 8
                    );
                }

                DrawArrowhead(
                    g,
                    brush,
                    new PointF(cx + 13, cy + 9),
                    Math.Atan2(16, 24)
                );

                DrawArrowhead(
                    g,
                    brush,
                    new PointF(cx + 13, cy - 9),
                    Math.Atan2(-16, 24)
                );

                break;

            case GameMode.AtomicChess:

                g.FillPolygon(
                    brush,
                    StarPoints(
                        cx,
                        cy,
                        13,
                        6,
                        8
                    )
                );

                break;
        }
    }

    // ── ModeRow ──────────────────────────────────────────────────────────────
    private class ModeRow : Control
    {
        readonly string _title;
        readonly string _desc;
        readonly GameMode _mode;

        bool _hover;
        bool _pressed;

        public Action? ClickAction;

        static readonly Font TitleFont =
            new("Segoe UI Semibold", 12.5F);

        static readonly Font DescFont =
            new("Segoe UI", 9.5F);

        public ModeRow(
            string title,
            string desc,
            GameMode mode)
        {
            _title = title;
            _desc = desc;
            _mode = mode;

            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true
            );

            Cursor = Cursors.Hand;
            BackColor = MainMenu.BgColor;
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

            var accent = MainMenu.AccentFor(_mode);

            var cardRect = new RectangleF(
                0,
                0,
                Width - 4,
                Height - 8
            );

            DrawCardShadow(
                g,
                cardRect,
                radius,
                _hover
            );

            using (var path = RoundedRect(
                       cardRect,
                       radius))
            {
                using (var bg = new SolidBrush(
                           _hover
                               ? MainMenu.CardHoverBg
                               : MainMenu.CardBg))
                {
                    g.FillPath(bg, path);
                }

                if (_pressed)
                {
                    using var pressTint = new SolidBrush(
                        Color.FromArgb(
                            MainMenu.IsDarkMode ? 26 : 14,
                            0,
                            0,
                            0
                        )
                    );

                    g.FillPath(
                        pressTint,
                        path
                    );
                }

                using var pen = new Pen(
                    _hover
                        ? Color.FromArgb(150, accent)
                        : MainMenu.BorderColor,
                    1.3f
                );

                g.DrawPath(
                    pen,
                    path
                );
            }

            // Icon chip
            var chipRect = new RectangleF(
                16,
                (cardRect.Height - 44) / 2,
                44,
                44
            );

            int chipAlpha =
                MainMenu.IsDarkMode
                    ? 45
                    : 30;

            using (var chipPath = RoundedRect(
                       chipRect,
                       10f))
            using (var chipBg = new SolidBrush(
                       Color.FromArgb(
                           chipAlpha,
                           accent
                       )))
            {
                g.FillPath(
                    chipBg,
                    chipPath
                );
            }

            DrawModeIcon(
                g,
                chipRect,
                _mode,
                accent
            );

            // Text block
            float textLeft = chipRect.Right + 14;

            float textWidth =
                cardRect.Width -
                textLeft -
                40;

            var descSize = g.MeasureString(
                _desc,
                DescFont,
                (int)textWidth
            );

            float titleH =
                TitleFont.GetHeight(g);

            const float spacing = 4f;

            float blockH =
                titleH +
                spacing +
                descSize.Height;

            float blockTop =
                (cardRect.Height - blockH) / 2f;

            using (var titleFmt = new StringFormat
                   {
                       Trimming =
                           StringTrimming.EllipsisCharacter,
                       FormatFlags =
                           StringFormatFlags.NoWrap
                   })
            using (var tbr = new SolidBrush(
                       MainMenu.TextPrimary))
            {
                g.DrawString(
                    _title,
                    TitleFont,
                    tbr,
                    new RectangleF(
                        textLeft,
                        blockTop,
                        textWidth,
                        titleH
                    ),
                    titleFmt
                );
            }

            using (var dbr = new SolidBrush(
                       MainMenu.TextSecondary))
            {
                g.DrawString(
                    _desc,
                    DescFont,
                    dbr,
                    new RectangleF(
                        textLeft,
                        blockTop + titleH + spacing,
                        textWidth,
                        descSize.Height
                    )
                );
            }

            // Trailing chevron
            using (var cf = new Font(
                       "Segoe UI",
                       14F))
            using (var cbr = new SolidBrush(
                       _hover
                           ? accent
                           : MainMenu.TextTertiary))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Far,
                    LineAlignment = StringAlignment.Center
                };

                g.DrawString(
                    "›",
                    cf,
                    cbr,
                    new RectangleF(
                        0,
                        0,
                        cardRect.Width - 16,
                        cardRect.Height
                    ),
                    sf
                );
            }
        }
    }

    // ── ThemeToggle ──────────────────────────────────────────────────────────
    private class ThemeToggle : Control
    {
        bool _hover;

        public Action? ToggleAction;

        public ThemeToggle()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true
            );

            Cursor = Cursors.Hand;
            BackColor = MainMenu.BgColor;

            TabStop = true;

            AccessibleName =
                "Toggle dark mode";

            AccessibleRole =
                AccessibleRole.PushButton;
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
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            ToggleAction?.Invoke();
            base.OnMouseClick(e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            Invalidate();
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            Invalidate();
            base.OnLostFocus(e);
        }

        protected override bool IsInputKey(
            Keys keyData)
        {
            return keyData == Keys.Space ||
                   base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(
            KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter ||
                e.KeyCode == Keys.Space)
            {
                ToggleAction?.Invoke();

                e.Handled = true;
            }

            base.OnKeyDown(e);
        }

        protected override void OnPaint(
            PaintEventArgs e)
        {
            var g = e.Graphics;

            g.SmoothingMode =
                SmoothingMode.AntiAlias;

            var rect = new RectangleF(
                1,
                1,
                Width - 3,
                Height - 3
            );

            var faceColor =
                _hover
                    ? MainMenu.HoverTint
                    : MainMenu.CardBg;

            // Shadow
            using (var shadow = new SolidBrush(
                       Color.FromArgb(
                           _hover ? 24 : 14,
                           20,
                           20,
                           20
                       )))
            {
                g.FillEllipse(
                    shadow,
                    rect.X,
                    rect.Y +
                    (_hover ? 2.5f : 1.5f),
                    rect.Width,
                    rect.Height
                );
            }

            // Background
            using (var bg = new SolidBrush(
                       faceColor))
            {
                g.FillEllipse(
                    bg,
                    rect
                );
            }

            // Border
            using (var pen = new Pen(
                       _hover
                           ? MainMenu.TextSecondary
                           : MainMenu.TextTertiary,
                       _hover ? 1.5f : 1.3f))
            {
                g.DrawEllipse(
                    pen,
                    rect
                );
            }

            float cx = Width / 2f;
            float cy = Height / 2f;

            using var iconBrush =
                new SolidBrush(
                    MainMenu.TextPrimary
                );

            if (MainMenu.IsDarkMode)
            {
                // Sun — currently dark mode.
                g.FillEllipse(
                    iconBrush,
                    cx - 5,
                    cy - 5,
                    10,
                    10
                );

                using var rayPen =
                    new Pen(
                        MainMenu.TextPrimary,
                        1.6f
                    )
                    {
                        StartCap = LineCap.Round,
                        EndCap = LineCap.Round
                    };

                for (int i = 0; i < 8; i++)
                {
                    double a =
                        i * Math.PI / 4;

                    float x1 =
                        cx +
                        (float)(
                            Math.Cos(a) * 7
                        );

                    float y1 =
                        cy +
                        (float)(
                            Math.Sin(a) * 7
                        );

                    float x2 =
                        cx +
                        (float)(
                            Math.Cos(a) * 10.5
                        );

                    float y2 =
                        cy +
                        (float)(
                            Math.Sin(a) * 10.5
                        );

                    g.DrawLine(
                        rayPen,
                        x1,
                        y1,
                        x2,
                        y2
                    );
                }
            }
            else
            {
                // Moon — currently light mode.
                g.FillEllipse(
                    iconBrush,
                    cx - 7,
                    cy - 7,
                    14,
                    14
                );

                using var biteBrush =
                    new SolidBrush(
                        faceColor
                    );

                g.FillEllipse(
                    biteBrush,
                    cx - 1,
                    cy - 6,
                    12,
                    12
                );
            }

            // Keyboard focus ring
            if (Focused)
            {
                var focusRect =
                    new RectangleF(
                        rect.X - 3,
                        rect.Y - 3,
                        rect.Width + 6,
                        rect.Height + 6
                    );

                using var focusPen =
                    new Pen(
                        MainMenu.TextSecondary,
                        1.3f
                    )
                    {
                        DashStyle = DashStyle.Dot
                    };

                g.DrawEllipse(
                    focusPen,
                    focusRect
                );
            }
        }
    }
}