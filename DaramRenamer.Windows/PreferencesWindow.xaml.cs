using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

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

		private void Shortcut_TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			var keyBindingInfo = (sender as TextBox)?.DataContext as KeyBindingInfo;
			var keyBinding = new StringBuilder();

			var ctrl = (e.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0;
			var alt = (e.KeyboardDevice.Modifiers & ModifierKeys.Alt) != 0;
			var shift = (e.KeyboardDevice.Modifiers & ModifierKeys.Shift) != 0;
			if (ctrl)
				keyBinding.Append("Ctrl").Append("+");
			if (alt)
				keyBinding.Append("Alt").Append("+");
			if (shift)
				keyBinding.Append("Shift").Append("+");
			keyBinding.Append(e.Key);

			if (keyBindingInfo != null)
				keyBindingInfo.KeyBinding = keyBinding.ToString();
		}
		private void Shortcut_Button_Loaded(object sender, RoutedEventArgs e)
		{
			PluginToMenu.InitializeCommands(((sender as Button)?.ContextMenu?.Items[0] as MenuItem)?.Items, false);
		}

		private void Shortcut_Button_Click(object sender, RoutedEventArgs e)
		{
			var contextMenu = ((Button) sender).ContextMenu;
			if (contextMenu == null)
				return;

			contextMenu.Tag = sender;
			contextMenu.IsOpen = true;
		}

		private void CommandMenuItem_Click(object sender, RoutedEventArgs e)
		{
			if (!((sender as MenuItem)?.DataContext is ICommand command))
				return;

			if (ItemsControl.ItemsControlFromItemContainer((MenuItem) sender) is MenuItem menuItem && 
			    (menuItem.Parent as MenuItem)?.Parent is ContextMenu { Tag: Button { DataContext: KeyBindingInfo keyBindingInfo } })
				keyBindingInfo.Command = command.GetType().FullName;
		}

		private void Shortcut_Button_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
		}
	}
}
