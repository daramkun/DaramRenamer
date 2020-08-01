using System;
using System.Collections.Generic;
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
	/// PreferencesWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class PreferencesWindow : Window
	{
		public PreferencesWindow ()
		{
			InitializeComponent ();
		}

		private void ComboBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
		{
			Preferences.Instance.RenameModeInteger = ((ComboBox) sender).SelectedIndex;
		}

		private void Window_Closing (object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
			Hide();
		}
	}
}
