using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace GroupRenamer
{
	/// <summary>
	/// App.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class App : Application
	{
		public App ()
		{
			System.Windows.Forms.Application.EnableVisualStyles ();

			AppDomain.CurrentDomain.UnhandledException += ( sender, args ) =>
			{
				MessageBox.Show ( "알 수 없는 오류가 발생했습니다. error.log를 참고해주세요." );
				using ( StreamWriter sw = File.AppendText ( "error.log" ) )
					sw.WriteLine ( args.ExceptionObject.ToString () );
			};
		}

		protected override void OnStartup ( StartupEventArgs e )
		{
			if ( e.Args.Length > 0 && e.Args [ 0 ] == "-c" )
			{

				this.Shutdown ( 0 );
			}

			base.OnStartup ( e );
		}
	}
}
