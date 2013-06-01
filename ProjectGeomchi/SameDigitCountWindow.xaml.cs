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
	/// SameDigitCountWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class SameDigitCountWindow : Window
	{
		public int DigitCount { get { return int.Parse ( textBoxCount.Text ); } }
		public int ProcessOffset { get { return comboBoxOffset.SelectedIndex; } }

		public SameDigitCountWindow ()
		{
			InitializeComponent ();
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
