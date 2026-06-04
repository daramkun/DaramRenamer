using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;

namespace DaramRenamer.Avalonia;

internal sealed class NativeTheme
{
    public static NativeTheme Current => Create();

    public bool IsMacOS { get; }
    public bool IsWindows { get; }
    public bool IsDark { get; }
    public double ToolbarButtonSize { get; }
    public double ToolbarIconSize { get; }
    public Thickness ToolbarPadding { get; }
    public Thickness ContentPadding { get; }
    public CornerRadius SurfaceRadius { get; }
    public CornerRadius ControlRadius { get; }
    public IBrush AppBackground { get; }
    public IBrush Surface { get; }
    public IBrush SurfaceElevated { get; }
    public IBrush HeaderBackground { get; }
    public IBrush StatusBackground { get; }
    public IBrush Border { get; }
    public IBrush Text { get; }
    public IBrush MutedText { get; }
    public IBrush Accent { get; }
    public IBrush AccentSoft { get; }
    public IBrush ChangedText { get; }
    public IBrush WarningText { get; }

    private NativeTheme(
        bool isMacOS,
        bool isWindows,
        bool isDark,
        double toolbarButtonSize,
        double toolbarIconSize,
        Thickness toolbarPadding,
        Thickness contentPadding,
        CornerRadius surfaceRadius,
        CornerRadius controlRadius,
        IBrush appBackground,
        IBrush surface,
        IBrush surfaceElevated,
        IBrush headerBackground,
        IBrush statusBackground,
        IBrush border,
        IBrush text,
        IBrush mutedText,
        IBrush accent,
        IBrush accentSoft,
        IBrush changedText,
        IBrush warningText)
    {
        IsMacOS = isMacOS;
        IsWindows = isWindows;
        IsDark = isDark;
        ToolbarButtonSize = toolbarButtonSize;
        ToolbarIconSize = toolbarIconSize;
        ToolbarPadding = toolbarPadding;
        ContentPadding = contentPadding;
        SurfaceRadius = surfaceRadius;
        ControlRadius = controlRadius;
        AppBackground = appBackground;
        Surface = surface;
        SurfaceElevated = surfaceElevated;
        HeaderBackground = headerBackground;
        StatusBackground = statusBackground;
        Border = border;
        Text = text;
        MutedText = mutedText;
        Accent = accent;
        AccentSoft = accentSoft;
        ChangedText = changedText;
        WarningText = warningText;
    }

    private static NativeTheme Create()
    {
        var isMacOS = OperatingSystem.IsMacOS();
        var isWindows = OperatingSystem.IsWindows();
        var isDark = Application.Current?.ActualThemeVariant == ThemeVariant.Dark;
        var accent = isMacOS ? Brush("#0A84FF") : isDark ? Brush("#60A5FA") : Brush("#2563EB");
        var accentSoft = isMacOS
            ? isDark ? Brush("#12395F") : Brush("#EAF4FF")
            : isDark ? Brush("#1E3A5F") : Brush("#EFF6FF");

        return new NativeTheme(
            isMacOS,
            isWindows,
            isDark,
            isMacOS ? 32 : 30,
            isMacOS ? 16 : 15,
            isMacOS ? new Thickness(14, 9, 14, 8) : new Thickness(10, 7, 10, 7),
            isMacOS ? new Thickness(14, 12, 14, 12) : new Thickness(10, 10, 10, 10),
            isMacOS ? new CornerRadius(9) : new CornerRadius(6),
            isMacOS ? new CornerRadius(7) : new CornerRadius(4),
            isDark ? Brush("#1C1C1E") : Brush("#F6F7F9"),
            isDark ? Brush("#242426") : Brush("#FFFFFF"),
            isDark ? Brush("#2C2C2E") : Brush("#FBFCFE"),
            isDark ? Brush("#2F3136") : Brush("#F1F5F9"),
            isDark ? Brush("#202124") : Brush("#FAFBFC"),
            isDark ? Brush("#3A3A3C") : Brush("#D7DEE8"),
            isDark ? Brush("#F5F5F7") : Brush("#1F2937"),
            isDark ? Brush("#A1A1AA") : Brush("#64748B"),
            accent,
            accentSoft,
            isDark ? Brush("#5EEAD4") : Brush("#0F766E"),
            isDark ? Brush("#F59E0B") : Brush("#B45309"));
    }

    private static SolidColorBrush Brush(string color) => new(Color.Parse(color));
}
