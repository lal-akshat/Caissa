using System.Drawing.Drawing2D;

namespace Caissa;

public enum GameMode { Standard, Chess960, AtomicChess }

/// <summary>
/// Single source of truth for colors, dark-mode state, and the small drawing
/// helpers (rounded rects, card shadows) shared by MainMenu and ChessBoard.
/// Keeping this in one place is what lets the board pick up the exact same
/// look — and the exact same toggle state — as the menu.
/// </summary>
public static class Theme
{
    public static bool IsDarkMode = false;

    /// <summary>
    /// Raised whenever the theme changes. Any open form can subscribe its
    /// ApplyTheme method so a toggle flipped on one screen is reflected on
    /// every other screen immediately, not just the next time it's shown.
    /// </summary>
    public static event Action? ThemeChanged;

    public static void ToggleDarkMode()
    {
        IsDarkMode = !IsDarkMode;
        ThemeChanged?.Invoke();
    }

    // ── Light palette ───────────────────────────────────────────────────
    static readonly Color LightBg            = Color.FromArgb(247, 247, 248);
    static readonly Color LightCard          = Color.White;
    static readonly Color LightCardHover     = Color.FromArgb(250, 250, 251);
    static readonly Color LightBorder        = Color.FromArgb(228, 228, 231);
    static readonly Color LightTextPrimary   = Color.FromArgb(31,  35,  40);
    static readonly Color LightTextSecondary = Color.FromArgb(110, 118, 129);
    static readonly Color LightTextTertiary  = Color.FromArgb(145, 152, 161);

    // ── Dark palette ────────────────────────────────────────────────────
    static readonly Color DarkBg             = Color.FromArgb(13,  17,  23);
    static readonly Color DarkCard           = Color.FromArgb(22,  27,  34);
    static readonly Color DarkCardHover      = Color.FromArgb(30,  36,  44);
    static readonly Color DarkBorder         = Color.FromArgb(48,  54,  61);
    static readonly Color DarkTextPrimary    = Color.FromArgb(230, 237, 243);
    static readonly Color DarkTextSecondary  = Color.FromArgb(139, 148, 158);
    static readonly Color DarkTextTertiary   = Color.FromArgb(101, 108, 118);

    public static Color BgColor        => IsDarkMode ? DarkBg : LightBg;
    public static Color CardBg         => IsDarkMode ? DarkCard : LightCard;
    public static Color CardHoverBg    => IsDarkMode ? DarkCardHover : LightCardHover;
    public static Color BorderColor    => IsDarkMode ? DarkBorder : LightBorder;
    public static Color TextPrimary    => IsDarkMode ? DarkTextPrimary : LightTextPrimary;
    public static Color TextSecondary  => IsDarkMode ? DarkTextSecondary : LightTextSecondary;
    public static Color TextTertiary   => IsDarkMode ? DarkTextTertiary : LightTextTertiary;
    public static Color HoverTint      => IsDarkMode
        ? Color.FromArgb(33, 38, 45)
        : Color.FromArgb(240, 240, 241);

    // ── Accents ─────────────────────────────────────────────────────────
    static readonly Color StandardAccentLight = Color.FromArgb(76,  154, 91);
    static readonly Color StandardAccentDark  = Color.FromArgb(63,  185, 80);
    static readonly Color ComputerAccentLight = Color.FromArgb(56,  128, 168);
    static readonly Color ComputerAccentDark  = Color.FromArgb(96,  176, 214);
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

    public static Color ComputerAccent => IsDarkMode ? ComputerAccentDark : ComputerAccentLight;

    // ── Shared drawing helpers ──────────────────────────────────────────
    public static GraphicsPath RoundedRect(RectangleF rect, float radius)
    {
        float d = radius * 2;
        var path = new GraphicsPath();

        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();

        return path;
    }

    public static void DrawCardShadow(Graphics g, RectangleF rect, float radius, bool hover)
    {
        int layers = hover ? 4 : 2;

        for (int i = layers; i >= 1; i--)
        {
            float offset = hover ? i * 1.6f : i * 1.0f;
            int alpha = hover ? 10 : 6;

            var shadowRect = new RectangleF(rect.X, rect.Y + offset, rect.Width, rect.Height);

            using var path = RoundedRect(shadowRect, radius);
            using var brush = new SolidBrush(Color.FromArgb(alpha, 20, 20, 20));

            g.FillPath(brush, path);
        }
    }
}