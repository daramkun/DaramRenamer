using Aprillz.MewUI;
using System.Globalization;

namespace DaramRenamer.MewUI;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            File.WriteAllText("UnhandledException.log", args.ExceptionObject?.ToString() ?? string.Empty);

        if (Directory.Exists("DaramRenamer-Update"))
            Directory.Delete("DaramRenamer-Update", recursive: true);
        if (File.Exists("DaramRenamer-Update.bat"))
            File.Delete("DaramRenamer-Update.bat");

        var preferences = MewUIPreferences.Instance;
        var strings = Strings.Instance;
        MewUITheme.ConfigureDefaults();
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
            var application = Application.Create().UseWin32();
            if (preferences.HaredwareAccelerated)
                application.UseDirect2D();
            else
                application.UseGdi();

            application.Run(new MainWindow());
        }
        finally
        {
            // WPF persists preferences from Application.OnExit as a final safeguard.
            preferences.Save();
        }
    }
}
