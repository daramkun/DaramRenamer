using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Daramkun.DaramRenamer.Properties;

namespace Daramkun.DaramRenamer
{
	/// <summary>
	/// App.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class App : Application
	{
		public App ()
		{
			if ( Environment.OSVersion.Version <= new Version ( 5, 0 ) )
			{
				MessageBox.Show ( Daramkun.DaramRenamer.Properties.Resources.PleaseUseLegacy,
					Daramkun.DaramRenamer.Properties.Resources.DaramRenamer, MessageBoxButton.OK, MessageBoxImage.Error );
				Application.Current.Shutdown ( -1 );
			}

			AppDomain.CurrentDomain.UnhandledException += ( sender, args ) =>
			{
				Daramkun.DaramRenamer.MainWindow.SimpleErrorMessage ( Daramkun.DaramRenamer.Properties.Resources.PleaseCheckLog );
				using ( StreamWriter sw = File.AppendText ( "error.log" ) )
					sw.WriteLine ( args.ExceptionObject.ToString () );
			};
		}

		string [] args;

		protected override void OnStartup ( StartupEventArgs e )
		{
			if ( e.Args.Length > 0 && e.Args [ 0 ] == "--cmd" )
			{
				Console.WriteLine ( "NOTICE: Sorry. Command Line Mode is not implemented in this version." );
				this.Shutdown ( 0 );
			}
			else
			{
				if ( !Settings.Default.HardwareTurnOn )
					RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

				args = e.Args;

				base.OnStartup ( e );
			}
		}

		protected override void OnActivated ( EventArgs e )
		{
			if (args != null)
			{
				foreach ( string filename in args )
					( MainWindow as MainWindow ).AddItem ( filename );
				args = null;
			}
			base.OnActivated ( e );
		}
	}
}
