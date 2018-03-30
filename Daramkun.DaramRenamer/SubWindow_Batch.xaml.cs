using Daramkun.DaramRenamer.Processors;
using System;
using System.Collections.Generic;
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
	}
}
