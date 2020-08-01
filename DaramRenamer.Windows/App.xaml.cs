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
		protected override void OnStartup(StartupEventArgs e)
		{
			RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

			base.OnStartup(e);
		}

		protected override void OnExit(ExitEventArgs e)
		{
			Preferences.Instance.Save();
			base.OnExit(e);
		}
	}
}
