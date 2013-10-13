using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GroupRenamer
{
	/// <summary>
	/// RegularExpressionWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class RegularExpressionWindow : Window
	{
		static List<string> regexpList;
		static List<string> formStrList;

		static RegularExpressionWindow ()
		{
			regexpList = new List<string> ();
			formStrList = new List<string> ();

			// Japanese Animation Illegal Raw Rip Filename Format
			regexpList.Add ( @"\[(.*)\](.*) - ([0-9]*)(.*)\((([A-Z]|[a-z])*) ([0-9]*)x([0-9]*) (([A-Z]|[a-z]|[0-9])*) (([A-Z]|[a-z]|[0-9])*)\).(([0-9]|[a-z])*)" );
			formStrList.Add ( @"[{1}] {2} - {3} ({4} {5}x{6} {7} {8}).{9}" );

			// Filename - Number Filename Format
			regexpList.Add ( @"(.*) - ([0-9]*).(([0-9]|[a-z])*)" );
			formStrList.Add ( @"{1} - {2}.{3}" );

			// Digital Camera Photo Filename Format
			regexpList.Add ( @"([A-Z]*)([0-9]*).(([0-9]|[a-z])*)" );
			formStrList.Add ( @"{1}{2}.{3}" );

			// Windows Phone Photo Filename Format
			regexpList.Add ( @"([A-Z]*)_([0-9]*).(([0-9]|[a-z])*)" );
			formStrList.Add ( @"{1}_{2}.{3}" );
		}

		public string RegularExpression { get { return textBoxOriginal.Text; } }
		public string FormatString { get { return textBoxReplace.Text; } }

		public RegularExpressionWindow ()
		{
			InitializeComponent ();

			textBoxOriginal.ItemsSource = regexpList;
			textBoxReplace.ItemsSource = formStrList;
		}

		private void buttonApply_Click ( object sender, RoutedEventArgs e )
		{
			DialogResult = true;
		}

		private void buttonCancel_Click ( object sender, RoutedEventArgs e )
		{
			DialogResult = false;
		}
	}
}
