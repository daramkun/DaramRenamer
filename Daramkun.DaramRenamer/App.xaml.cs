using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
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
			if ( !Settings.Default.HardwareTurnOn )
				RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

			AppDomain.CurrentDomain.UnhandledException += ( sender, args ) =>
			{
				MessageBox.Show ( "알 수 없는 오류가 발생했습니다. error.log를 참고해주세요." );
				using ( StreamWriter sw = File.AppendText ( "error.log" ) )
					sw.WriteLine ( args.ExceptionObject.ToString () );
			};
		}

		protected override void OnStartup ( StartupEventArgs e )
		{
			base.OnStartup ( e );
		}
	}
}
