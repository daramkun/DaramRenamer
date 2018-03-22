using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Daramee.DaramCommonLib;
using Daramee.FileTypeDetector;

namespace Daramkun.DaramRenamer
{
	/// <summary>
	/// App.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class App : Application
	{
		Localizer ownLocalizer;

		public App ()
		{
			ProgramHelper.Initialize ( Assembly.GetExecutingAssembly (), "daramkun", "DaramRenamer" );
			ownLocalizer = new Localizer ();

			if ( Environment.OSVersion.Version <= new Version ( 5, 0 ) )
			{
				MessageBox.Show ( Localizer.SharedStrings [ "os_notice" ], Localizer.SharedStrings [ "daram_renamer" ],
					MessageBoxButton.OK, MessageBoxImage.Error );
				Application.Current.Shutdown ( -1 );
			}

			TextWriterTraceListener textWriterTraceListner = new TextWriterTraceListener ( Console.Out );
			Debug.Listeners.Add ( textWriterTraceListner );

			DetectorService.AddDetectors ();

			AppDomain.CurrentDomain.UnhandledException += ( sender, args ) =>
			{
				Daramkun.DaramRenamer.MainWindow.MessageBox ( Localizer.SharedStrings [ "error_raised" ], Localizer.SharedStrings [ "please_check_log" ],
					TaskDialogInterop.VistaTaskDialogIcon.Error, "OK" );
				using ( StreamWriter sw = File.AppendText ( "error.log" ) )
				{
					TextWriterTraceListener textWriterTraceListnerForFile = new TextWriterTraceListener ( sw );
					Debug.Listeners.Add ( textWriterTraceListnerForFile );
					sw.WriteLine ( $"Error: {DateTime.Now.ToString ( "yyyy-MM-dd hh/mm/ss" )} - from Daram Renamer" );
					sw.WriteLine ( "----" );
					sw.WriteLine ( args.ExceptionObject.ToString () );
					sw.WriteLine ( "==========================================================" );
					Debug.Listeners.Remove ( textWriterTraceListnerForFile );
				}
			};
		}

		string [] args;

		protected override void OnStartup ( StartupEventArgs e )
		{
			if ( e.Args.Length > 0 && e.Args [ 0 ] == "--cmd" )
			{
				Debug.WriteLine ( "NOTICE: Sorry. Command Line Mode is not implemented in this version." );
				Shutdown ( 0 );
			}
			else if ( e.Args.Length == 1 && ( e.Args [ 0 ] == "--version" || e.Args [ 0 ] == "-v" ) )
			{
				Version version = Assembly.GetExecutingAssembly ().GetName ().Version;
				Debug.WriteLine ( $"Daram Renamer v{version.Major}.{version.Minor}.{version.Build}.{version.Revision}" );
				Shutdown ( 0 );
			}
			else
			{
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
			Optionizer<SaveData>.SharedOptionizer.Save ();
			base.OnExit ( e );
		}
	}
}
