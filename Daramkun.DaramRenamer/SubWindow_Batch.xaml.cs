using Daramee.DaramCommonLib;
using Daramkun.DaramRenamer.Processors;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

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

		public SubWindow_Batch ()
		{
			InitializeComponent ();
			Processor = new BatchProcessor ();

			textEditor?.Focus ();
		}

		private void OK_Button ( object sender, RoutedEventArgs e )
		{
			( Processor as BatchProcessor ).Script = textEditor.Text;

			btnOKButton?.Focus ();
			OKButtonClicked?.Invoke ( this, e );
		}

		private void Cancel_Button ( object sender, RoutedEventArgs e )
		{
			btnCancelButton?.Focus ();
			CancelButtonClicked?.Invoke ( this, e );
		}

		private void LoadScript_Click ( object sender, RoutedEventArgs e )
		{
			Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog ()
			{
				Filter = Localizer.SharedStrings [ "batch_filters" ] + "(*.drjs)|*.drjs",
			};
			if ( ofd.ShowDialog () == false )
				return;
			textEditor.Text = File.ReadAllText ( ofd.FileName, Encoding.UTF8 );
		}

		private void SaveScript_Click ( object sender, RoutedEventArgs e )
		{
			Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog ()
			{
				Filter = Localizer.SharedStrings [ "batch_filters" ] + "(*.drjs)|*.drjs",
			};
			if ( sfd.ShowDialog () == false )
				return;
			File.WriteAllText ( sfd.FileName, textEditor.Text, Encoding.UTF8 );
		}
	}
}
