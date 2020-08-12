using System.Globalization;
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
			var temp1 = Preferences.Instance;
			var temp2 = Strings.Instance;

			if (!Strings.Instance.AvailableLanguages.Contains(CultureInfo.CurrentUICulture))
				Preferences.Instance.CurrentLanguage = Strings.Instance.GetDefaultLanguage().ToString();
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
