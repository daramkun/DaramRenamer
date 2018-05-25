using Daramee.DaramCommonLib;
using Daramee.Winston.Dialogs;
using Daramkun.DaramRenamer.Processors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Daramkun.DaramRenamer
{
	/// <summary>
	/// SubWindow_Batch.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class SubWindow_Batch : UserControl, ISubWindow
	{
		public event RoutedEventHandler OKButtonClicked;
		public event RoutedEventHandler CancelButtonClicked;
		
		public IProcessor Processor { get; private set; }

		string loadedFilename;

		public SubWindow_Batch ( bool titleBarVisible = true )
		{
			InitializeComponent ();

			if ( !titleBarVisible )
			{
				titleBar.Visibility = Visibility.Hidden;
				titleBar.Height = 0;
			}

			Processor = new BatchProcessor ();

			textEditor?.Focus ();
		}

		private void OK_Button ( object sender, RoutedEventArgs e )
		{
			( Processor as BatchProcessor ).Script = textEditor.Text;

			if ( loadedFilename != null && loadedFilename.IndexOf ( Path.GetTempPath () ) >= 0 )
				File.Delete ( loadedFilename );

			btnOKButton?.Focus ();
			OKButtonClicked?.Invoke ( this, e );
		}

		private void Cancel_Button ( object sender, RoutedEventArgs e )
		{
			if ( loadedFilename != null && loadedFilename.IndexOf ( Path.GetTempPath () ) >= 0 )
				File.Delete ( loadedFilename );

			btnCancelButton?.Focus ();
			CancelButtonClicked?.Invoke ( this, e );
		}

		private void LoadScript_Click ( object sender, RoutedEventArgs e )
		{
			Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog ()
			{
				Filter = StringTable.SharedStrings [ "batch_filters" ],
			};
			if ( ofd.ShowDialog () == false )
				return;
			textEditor.Text = File.ReadAllText ( ofd.FileName, Encoding.UTF8 );
			loadedFilename = ofd.FileName;
		}

		private void SaveScript_Click ( object sender, RoutedEventArgs e )
		{
			Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog ()
			{
				Filter = StringTable.SharedStrings [ "batch_filters" ],
			};
			if ( sfd.ShowDialog () == false )
				return;
			File.WriteAllText ( sfd.FileName, textEditor.Text, Encoding.UTF8 );
			loadedFilename = sfd.FileName;
		}

		internal void Activated ()
		{
			if ( loadedFilename != null )
			{
				string newContent = File.ReadAllText ( loadedFilename );
				if ( loadedFilename != newContent )
				{
					int selectionStart = textEditor.SelectionStart;
					textEditor.Text = newContent;
					textEditor.SelectionStart = selectionStart;
				}
			}
		}

		private void VSCode_Click ( object sender, RoutedEventArgs e )
		{
			if ( loadedFilename != null )
			{
				File.WriteAllText ( loadedFilename, textEditor.Text );
			}
			else
			{
				if ( !Directory.Exists ( Path.Combine ( Path.GetTempPath (), "DaramRenamer" ) ) )
					Directory.CreateDirectory ( Path.Combine ( Path.GetTempPath (), "DaramRenamer" ) );
				File.WriteAllText (
					loadedFilename = Path.Combine ( Path.GetTempPath (), "DaramRenamer", $"{Guid.NewGuid ()}.js" ),
					textEditor.Text );
			}

			try
			{
				Process process = new Process ()
				{
					StartInfo = new ProcessStartInfo ( "vscode://file/" + loadedFilename.Replace ( '\\', '/' ) )
					{

					}
				};
				process.Start ();
			}
			catch
			{

			}
		}
	}
}
