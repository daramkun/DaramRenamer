using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

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
				MessageBox.Show ( "Please execute Daram Renamer in Windows 7 or Higher version.", Globalizer.Strings [ "daram_renamer" ],
					MessageBoxButton.OK, MessageBoxImage.Error );
				Application.Current.Shutdown ( -1 );
			}

			AppDomain.CurrentDomain.UnhandledException += ( sender, args ) =>
			{
				Daramkun.DaramRenamer.MainWindow.MessageBox ( Globalizer.Strings [ "error_raised" ], Globalizer.Strings [ "please_check_log" ],
					TaskDialogInterop.VistaTaskDialogIcon.Error, "OK" );
				using ( StreamWriter sw = File.AppendText ( "error.log" ) )
				{
					sw.WriteLine ( args.ExceptionObject.ToString () );
					sw.WriteLine ( "==========================================================" );
				}
			};
		}

		string [] args;

		protected override void OnStartup ( StartupEventArgs e )
		{
			if ( e.Args.Length > 0 && e.Args [ 0 ] == "--cmd" )
			{
				Console.WriteLine ( "NOTICE: Sorry. Command Line Mode is not implemented in this version." );
				Shutdown ( 0 );
			}
			else
			{
				if ( !Optionizer.SharedOptionizer.HardwareAccelerationMode )
					RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

				args = e.Args;

				base.OnStartup ( e );
			}
		}

		protected override void OnActivated ( EventArgs e )
		{
			if ( args != null )
			{
				foreach ( string filename in args )
					( MainWindow as MainWindow ).AddItem ( filename );
				args = null;
			}
			base.OnActivated ( e );
		}

		protected override void OnExit ( ExitEventArgs e )
		{
			Optionizer.SharedOptionizer.Save ();
			base.OnExit ( e );
		}
	}
}
