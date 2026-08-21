using System.Drawing.Drawing2D;

namespace Caissa;

/// <summary>
/// The little sun/moon pill button. Lives on its own so MainMenu and
/// ChessBoard can drop in the exact same control instead of two look-alikes
/// drifting apart over time.
/// </summary>
public class ThemeToggle : Control
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
        BackColor = Theme.BgColor;
        TabStop = true;

        AccessibleName = "Toggle dark mode";
        AccessibleRole = AccessibleRole.PushButton;

        // Default behavior: flip the shared theme. Forms don't need to
        // wire this up themselves — they just subscribe to
        // Theme.ThemeChanged to repaint when it (or any other toggle) fires.
        ToggleAction = Theme.ToggleDarkMode;
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

    protected override bool IsInputKey(Keys keyData)
    {
        return keyData == Keys.Space || base.IsInputKey(keyData);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
        {
            ToggleAction?.Invoke();
            e.Handled = true;
        }

        base.OnKeyDown(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new RectangleF(1, 1, Width - 3, Height - 3);
        var faceColor = _hover ? Theme.HoverTint : Theme.CardBg;

        // Shadow
        using (var shadow = new SolidBrush(Color.FromArgb(_hover ? 24 : 14, 20, 20, 20)))
        {
            g.FillEllipse(shadow, rect.X, rect.Y + (_hover ? 2.5f : 1.5f), rect.Width, rect.Height);
        }

        // Background
        using (var bg = new SolidBrush(faceColor))
        {
            g.FillEllipse(bg, rect);
        }

        // Border
        using (var pen = new Pen(_hover ? Theme.TextSecondary : Theme.TextTertiary, _hover ? 1.5f : 1.3f))
        {
            g.DrawEllipse(pen, rect);
        }

        float cx = Width / 2f;
        float cy = Height / 2f;

        using var iconBrush = new SolidBrush(Theme.TextPrimary);

        if (Theme.IsDarkMode)
        {
            // Sun — currently dark mode.
            g.FillEllipse(iconBrush, cx - 5, cy - 5, 10, 10);

            using var rayPen = new Pen(Theme.TextPrimary, 1.6f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };

            for (int i = 0; i < 8; i++)
            {
                double a = i * Math.PI / 4;

                float x1 = cx + (float)(Math.Cos(a) * 7);
                float y1 = cy + (float)(Math.Sin(a) * 7);
                float x2 = cx + (float)(Math.Cos(a) * 10.5);
                float y2 = cy + (float)(Math.Sin(a) * 10.5);

                g.DrawLine(rayPen, x1, y1, x2, y2);
            }
        }
        else
        {
            // Moon — currently light mode.
            g.FillEllipse(iconBrush, cx - 7, cy - 7, 14, 14);

            using var biteBrush = new SolidBrush(faceColor);
            g.FillEllipse(biteBrush, cx - 1, cy - 6, 12, 12);
        }

        // Keyboard focus ring
        if (Focused)
        {
            var focusRect = new RectangleF(rect.X - 3, rect.Y - 3, rect.Width + 6, rect.Height + 6);

            using var focusPen = new Pen(Theme.TextSecondary, 1.3f) { DashStyle = DashStyle.Dot };
            g.DrawEllipse(focusPen, focusRect);
        }
    }
}