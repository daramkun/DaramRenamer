using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

namespace DaramRenamer.MewUI;

internal static class MewUITheme
{
    public static readonly Color Accent = new(72, 61, 139);

    public static void ConfigureDefaults(ThemeVariant variant)
    {
        ThemeManager.Default = variant;
        ThemeManager.DefaultAccentColor = Accent;
    }

    public static void Apply(ThemeVariant variant) => Application.Current.SetTheme(variant);

    public static Color AccentFor(Theme theme) => theme.IsDark
        ? new Color(190, 184, 255)
        : Accent;

    public static Label CellText() => new Label()
        .FontFamily("Segoe UI")
        .FontSize(12)
        .WithTheme((theme, label) => label.Foreground(AccentFor(theme)));
}
