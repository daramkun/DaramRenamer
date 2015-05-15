using System;
using System.Collections.Generic;
using System.IO;
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
			AppDomain.CurrentDomain.UnhandledException += ( sender, args ) =>
			{
				Daramkun.DaramRenamer.MainWindow.SimpleErrorMessage ( "알 수 없는 오류가 발생했습니다. error.log를 참고해주세요." );
				using ( StreamWriter sw = File.AppendText ( "error.log" ) )
					sw.WriteLine ( args.ExceptionObject.ToString () );
			};
		}

		protected override void OnStartup ( StartupEventArgs e )
		{
			if ( !Settings.Default.HardwareTurnOn )
				RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
			base.OnStartup ( e );
		}
	}
}
