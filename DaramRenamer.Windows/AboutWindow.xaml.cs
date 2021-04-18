using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DaramRenamer
{
	/// <summary>
	/// AboutWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class AboutWindow : Window
	{
		public AboutWindow ()
		{
			InitializeComponent ();
			VersionTextBlock.Text = MainWindow.GetVersionString();
			CopyrightTextBlock.Text = MainWindow.GetCopyrightString();
		}

		private void Hyperlink_Click (object sender, RoutedEventArgs e)
		{
			Process.Start(new ProcessStartInfo()
			{
				FileName = "https://github.com/daramkun/DaramRenamer",
				UseShellExecute = true,
			});
		}
	}
}
