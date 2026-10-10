using Aprillz.MewUI;
using System.Globalization;

namespace DaramRenamer.MewUI;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (SelfUpdater.TryRunCommand(args, out var exitCode))
            return exitCode;

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            File.WriteAllText("UnhandledException.log", args.ExceptionObject?.ToString() ?? string.Empty);

        var preferences = MewUIPreferences.Instance;
        var strings = Strings.Instance;
        MewUITheme.ConfigureDefaults(preferences.Theme);
        if (!strings.AvailableLanguages.Any(language =>
                language.Name.Equals(CultureInfo.CurrentUICulture.Name, StringComparison.OrdinalIgnoreCase)))
        {
            var defaultLanguage = strings.GetDefaultLanguage().Name;
            preferences.CurrentLanguage = defaultLanguage;
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(defaultLanguage);
            strings.Load();
        }

        try
        {
            var application = Application.Create();
            if (OperatingSystem.IsWindows())
            {
                application.UseWin32();
                if (preferences.HaredwareAccelerated)
                    application.UseDirect2D();
                else
                    application.UseGdi();
            }
            else if (OperatingSystem.IsLinux())
                application.UseX11().UseMewVGX11();
            else if (OperatingSystem.IsMacOS())
                application.UseMacOS().UseMewVGMetal();
            else
                throw new PlatformNotSupportedException("DaramRenamer MewUI supports Windows, Linux/X11, and macOS.");

            application.Run(new MainWindow());
        }
        finally
        {
            // WPF persists preferences from Application.OnExit as a final safeguard.
            preferences.Save();
        }

        return 0;
    }
}
