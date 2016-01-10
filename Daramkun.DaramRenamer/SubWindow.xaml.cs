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
	/// SubWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class SubWindow : UserControl
	{
		public event RoutedEventHandler OKButtonClicked;
		public event RoutedEventHandler CancelButtonClicked;

		public IProcessor Processor { get; private set; }

		public SubWindow ( IProcessor processor )
		{
			InitializeComponent ();

			Processor = processor;

			Type type = processor.GetType ();
			var props = type.GetProperties ();
			foreach ( var prop in props )
			{
				object [] attrs = prop.GetCustomAttributes ( typeof ( GlobalizedAttribute ), true );
				if ( attrs.Length > 0)
				{

				}
			}
		}

		private void OK_Button ( object sender, RoutedEventArgs e )
		{
			if ( OKButtonClicked != null ) OKButtonClicked ( this, e );
		}

		private void Cancel_Button ( object sender, RoutedEventArgs e )
		{
			if ( CancelButtonClicked != null ) CancelButtonClicked ( this, e );
		}
	}
}
