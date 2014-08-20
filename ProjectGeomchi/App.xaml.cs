using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
			if ( e.Args.Length > 0 && e.Args [ 0 ] == "--cmd" )
			{
				int offset = 1;
				List<FileInfo> files = new List<FileInfo> ();
				string command = null;
				List<object> argumentData = new List<object> ();

				if ( e.Args [ offset ] != "-c" )
				{
					Console.WriteLine ( "처리할 파일이 가장 먼저 입력되어야 합니다." );
					return;
				}

				while ( e.Args [ offset + 1 ] != "-o" && e.Args [ offset + 1 ] != "-c" )
				{
					string s = e.Args [ ++offset ];
					string filename = System.IO.Path.GetFileName ( s );
					string path = System.IO.Path.GetDirectoryName ( s );
					files.Add ( new FileInfo ()
					{
						ON = filename.Clone () as string,
						CN = filename.Clone () as string,
						OP = path.Clone () as string,
						CP = path.Clone () as string,
					} );
				}

				while ( offset < e.Args.Length )
				{
					switch ( e.Args [ offset ] )
					{
						case "-c":
							command = e.Args [ ++offset ];
							argumentData.Clear ();
							switch ( command )
							{
								case "replace":
								case "del-enclosed":
									argumentData.Add ( e.Args [ ++offset ] );
									argumentData.Add ( e.Args [ ++offset ] );
									argumentData.Add ( bool.Parse ( e.Args [ ++offset ] ) );
									break;
								case "prestring":
								case "poststring":
								case "add-ext":
								case "change-ext":
								case "path":
									argumentData.Add ( e.Args [ ++offset ] );
									break;
								case "same-num":
								case "incr-num":
									argumentData.Add ( int.Parse ( e.Args [ ++offset ] ) );
									argumentData.Add ( bool.Parse ( e.Args [ ++offset ] ) );
									break;
								case "regexp":
									argumentData.Add ( new Regex ( e.Args [ ++offset ] ) );
									argumentData.Add ( e.Args [ ++offset ] );
									break;
							}

							Parallel.ForEach ( files, ( FileInfo fileInfo ) =>
							{
								switch ( command )
								{
									case "replace":
										fileInfo.CN = FilenameProcessor.Replace ( fileInfo.CN, argumentData [ 0 ] as string, argumentData [ 1 ] as string,
											( bool ) argumentData [ 2 ] );
										break;
									case "prestring":
										fileInfo.CN = FilenameProcessor.Prestring ( fileInfo.CN, argumentData [ 0 ] as string );
										break;
									case "poststring":
										fileInfo.CN = FilenameProcessor.Poststring ( fileInfo.CN, argumentData [ 0 ] as string );
										break;
									case "del-name":
										fileInfo.CN = FilenameProcessor.DeleteName ( fileInfo.CN );
										break;
									case "del-enclosed":
										fileInfo.CN = FilenameProcessor.DeleteEnclosed ( fileInfo.CN, argumentData [ 0 ] as string, argumentData [ 1 ] as string,
											( bool ) argumentData [ 2 ] );
										break;
									case "del-char":
										fileInfo.CN = FilenameProcessor.DeleteWithoutNumber ( fileInfo.CN );
										break;
									case "same-num":
										fileInfo.CN = FilenameProcessor.SameNumberOfDigit ( fileInfo.CN, ( int ) argumentData [ 0 ], ( bool ) argumentData [ 1 ] );
										break;
									case "add-num":
										fileInfo.CN = FilenameProcessor.AddNumber ( fileInfo.CN, files.IndexOf ( fileInfo ) + 1 );
										break;
									case "incr-num":
										fileInfo.CN = FilenameProcessor.NumberIncrese ( fileInfo.CN, ( int ) argumentData [ 0 ], ( bool ) argumentData [ 1 ] );
										break;
									case "del-ext":
										fileInfo.CN = FilenameProcessor.RemoveExtension ( fileInfo.CN );
										break;
									case "add-ext":
										fileInfo.CN = FilenameProcessor.AddExtension ( fileInfo.CN, argumentData [ 0 ] as string );
										break;
									case "change-ext":
										fileInfo.CN = FilenameProcessor.ChangeExtension ( fileInfo.CN, argumentData [ 0 ] as string );
										break;
									case "ext-upper":
										fileInfo.CN = FilenameProcessor.ExtensionToUpper ( fileInfo.CN );
										break;
									case "ext-lower":
										fileInfo.CN = FilenameProcessor.ExtensionToLower ( fileInfo.CN );
										break;
									case "regexp":
										fileInfo.CN = FilenameProcessor.RegularExpression ( fileInfo.CN, argumentData [ 0 ] as Regex, argumentData [ 1 ] as string );
										break;
									case "path":
										fileInfo.CP = argumentData [ 0 ] as string;
										break;
								}
							} );
							break;

						case "-s":
							foreach ( FileInfo fileInfo in files )
							{
								try
								{
									File.Move ( System.IO.Path.Combine ( fileInfo.OP, fileInfo.ON ),
										System.IO.Path.Combine ( fileInfo.CP, fileInfo.CN ) );
									fileInfo.Changed ();
								}
								catch ( UnauthorizedAccessException ex )
								{
									System.Windows.Forms.MessageBox.Show ( String.Format (
										"\"{0}\" 파일의 경로를 변경할 권한이 없습니다.", fileInfo.ON ) );
									Debug.WriteLine ( ex.Message );
								}
								catch ( PathTooLongException ex )
								{
									System.Windows.Forms.MessageBox.Show ( String.Format (
										"\"{0}\"파일의 경로가 너무 깁니다.", fileInfo.ON ) );
									Debug.WriteLine ( ex.Message );
								}
								catch ( DirectoryNotFoundException ex )
								{
									System.Windows.Forms.MessageBox.Show ( String.Format (
										"\"{0}\"의 디렉토리가 존재하지 않습니다.", fileInfo.ON ) );
									Debug.WriteLine ( ex.Message );
								}
								catch ( IOException ex )
								{
									System.Windows.Forms.MessageBox.Show ( String.Format (
										"\"{0}\" 경로에 파일이 이미 있거나 원본 파일을 찾을 수 없습니다.", fileInfo.ON ) );
									Debug.WriteLine ( ex.Message );
								}
								catch ( Exception ex )
								{
									System.Windows.Forms.MessageBox.Show ( String.Format (
										"알 수 없는 이유로 파일의 경로를 변경할 수 없었습니다: {0}", fileInfo.ON ) );
									Debug.WriteLine ( ex.Message );
								}
							}
							break;
					}
					++offset;
				}

				this.Shutdown ( 0 );
			}

			base.OnStartup ( e );
		}
	}
}
