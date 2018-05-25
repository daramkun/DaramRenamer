using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using Daramee.DaramCommonLib;
using Daramee.Winston.Dialogs;

namespace Daramkun.DaramRenamer
{
	/// <summary>
	/// App.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class App : Application
	{
		StringTable ownLocalizer;

		public App ()
		{
			ProgramHelper.Initialize ( Assembly.GetExecutingAssembly (), "daramkun", "DaramRenamer" );
			ownLocalizer = new StringTable ();

			if ( Environment.OSVersion.Version <= new Version ( 5, 0 ) )
			{
				MessageBox.Show ( StringTable.SharedStrings [ "os_notice" ], StringTable.SharedStrings [ "daram_renamer" ],
					MessageBoxButton.OK, MessageBoxImage.Error );
				Application.Current.Shutdown ( -1 );
			}

			TextWriterTraceListener textWriterTraceListner = new TextWriterTraceListener ( Console.Out );
			Debug.Listeners.Add ( textWriterTraceListner );

			AppDomain.CurrentDomain.UnhandledException += ( sender, args ) =>
			{
				//Daramkun.DaramRenamer.MainWindow.SharedWindow.UndoManager.Backup ();

				Daramkun.DaramRenamer.MainWindow.MessageBox ( StringTable.SharedStrings [ "error_raised" ], StringTable.SharedStrings [ "please_check_log" ],
					TaskDialogIcon.Error, TaskDialogCommonButtonFlags.OK );
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
			args = e.Args;
			base.OnStartup ( e );
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
			base.OnExit ( e );
		}
	}
}
