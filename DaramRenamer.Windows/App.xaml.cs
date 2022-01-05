using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace DaramRenamer
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				File.WriteAllText("UnhandledException.log", e.ExceptionObject.ToString());
			};
			
			if (Directory.Exists("DaramRenamer-Update"))
				Directory.Delete("DaramRenamer-Update", true);
			if (File.Exists("DaramRenamer-Update.bat"))
				File.Delete("DaramRenamer-Update.bat");

			var temp1 = Preferences.Instance;
			var temp2 = Strings.Instance;
			

			if (!Strings.Instance.AvailableLanguages.Any(lang =>
				    lang.Name.Equals(CultureInfo.CurrentUICulture.Name,
					    StringComparison.OrdinalIgnoreCase)))
			{
				Preferences.Instance.CurrentLanguage = Strings.Instance.GetDefaultLanguage().Name;
			}
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
		}

		protected override void OnExit(ExitEventArgs e)
		{
			Preferences.Instance.Save();
			base.OnExit(e);
		}
	}
}
