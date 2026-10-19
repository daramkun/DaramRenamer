using System.Diagnostics;
using System.Reflection;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

namespace DaramRenamer.MewUI;

internal sealed class AboutWindow : Window
{
    public AboutWindow()
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        var versionText = version is null
            ? "UNKNOWN VERSION"
            : $"{version.Major}.{version.Minor}.{version.Build}";
        var copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? string.Empty;

        this.Title(Strings.Instance["AboutWindowTitle"]);
        this.Fixed(340, 280);
        this.ShowInTaskbar(false);
        this.StartCenterOwner();
        this.Content(new StackPanel()
            .Vertical()
            .CenterHorizontal()
            .CenterVertical()
            .Children(
                new Image()
                    .SourceResource(
                        typeof(AboutWindow).Assembly,
                        "DaramRenamer.MewUI.Resources.Icon.jpg")
                    .Size(96, 96)
                    .CenterHorizontal()
                    .Margin(0, 0, 0, 8),
                new Label()
                    .Text(Strings.Instance["DaramRenamer"])
                    .FontSize(24)
                    .CenterHorizontal()
                    .Bold(),
                new StackPanel()
                    .Horizontal()
                    .CenterHorizontal()
                    .Margin(0, 8, 0, 0)
                    .Children(
                        new Label().Text(Strings.Instance["Version"]),
                        new Label().Text($": {versionText}")),
                new Label()
                    .Text(copyright)
                    .FontSize(8)
                    .CenterHorizontal(),
                new Button()
                    .Content(Strings.Instance["GitHub Repository"])
                    .WithTheme((theme, button) => button.Foreground(MewUITheme.AccentFor(theme)))
                    .OnClick(OpenRepository)
                    .CenterHorizontal()
                    .Margin(0, 8, 0, 0)));
    }

    private static void OpenRepository()
    {
        var url = "https://github.com/daramkun/DaramRenamer";
        var startInfo = OperatingSystem.IsWindows()
            ? new ProcessStartInfo(url) { UseShellExecute = true }
            : new ProcessStartInfo(OperatingSystem.IsMacOS() ? "open" : "xdg-open")
            {
                ArgumentList = { url }
            };
        Process.Start(startInfo);
    }
}
