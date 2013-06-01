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
	/// ReplaceWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class DeleteEnclosedWindow : Window
	{
		public string Prestring { get { return textBoxPre.Text; } }
		public string Poststring { get { return textBoxPost.Text; } }
		public bool IsDeleteAllEnclosed { get { return checkBoxAllDelete.IsChecked.Value; } }

		public DeleteEnclosedWindow ()
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
