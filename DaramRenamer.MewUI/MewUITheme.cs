using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

namespace DaramRenamer.MewUI;

internal static class MewUITheme
{
    public static readonly Color Accent = new(72, 61, 139);

    public static void ConfigureDefaults() => ThemeManager.DefaultAccentColor = Accent;

    public static Label CellText() => new Label()
        .FontFamily("Segoe UI")
        .FontSize(12)
        .WithTheme((theme, label) => label.Foreground(theme.IsDark
            ? new Color(190, 184, 255)
            : Accent));
}
