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
using System.Windows.Shapes;

namespace DaramRenamer
{
	/// <summary>
	/// BatchWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class BatchWindow : Window
	{
		private readonly WeakReference<MainWindow> mainWindow;

		private BatchNode rootNode = new RootBatchNode();

		public BatchWindow (MainWindow mainWindow)
		{
			this.mainWindow = new WeakReference<MainWindow>(mainWindow);

			InitializeComponent ();

			TreeViewCommands.ItemsSource = new List<BatchNode>() {rootNode};
		}

		private void CommandMenuItem_Click(object sender, RoutedEventArgs e)
		{
			var selectedItem = TreeViewCommands.SelectedItem;
			if (selectedItem == null)
				return;

			ICommand command;
			if ((sender as MenuItem)?.Header is ICommand asCommandFromMenuItem)
				command = asCommandFromMenuItem;
			else if (sender is Button button)
				command = PluginManager.Instance.FindCommand (button.Tag as string);
			else if (sender is ICommand asCommand)
				command = asCommand;
			else
				return;

			if (command == null)
				return;

			command = Activator.CreateInstance (command.GetType ()) as ICommand;

			var properties = command?.GetType ().GetProperties ();
			if (properties?.Length > (command is IOrderBy ? 3 : 2))
			{
				var commandWindow = new CommandWindow (command) { Owner = this };
				if (commandWindow.ShowDialog () != true)
					return;
			}

			var node = new BatchNode() { Command = command };
			if (selectedItem is TreeViewItem)
				rootNode.Children.Add(node);
			else
				(selectedItem as BatchNode)?.Children.Add(node);
		}

		private void ConditionMenuItem_Click(object sender, RoutedEventArgs e)
		{
			var selectedItem = TreeViewCommands.SelectedItem;
			if (selectedItem == null)
				return;

			var condition = ((MenuItem) sender).Header as ICondition;
			var properties = condition.GetType ().GetProperties ();
			if (properties.Length > (condition is IOrderBy ? 1 : 0))
			{
				var commandWindow = new CommandWindow (condition) { Owner = this };
				if (commandWindow.ShowDialog() != true)
					return;
			}

			var node = new BatchNode () { Condition = condition };
			if (selectedItem is TreeViewItem)
				rootNode.Children.Add (node);
			else
				(selectedItem as BatchNode)?.Children.Add (node);
		}

		private void RemoveMenuItem_Click (object sender, RoutedEventArgs e)
		{
			if (!(TreeViewCommands.SelectedItem is BatchNode selectedItem))
				return;

			DeleteItem(rootNode, selectedItem);
		}

		private static bool DeleteItem(BatchNode from, BatchNode target)
		{
			if (from.Children.Count == 0)
				return false;

			return @from.Children.Remove (target) || @from.Children.Any(child => DeleteItem(child, target));
		}

		private void ContextMenu_Loaded(object sender, RoutedEventArgs e)
		{
			var contextMenu = (sender as ContextMenu);
			var commandsMenu = contextMenu?.Items[0] as MenuItem;
			var conditionsMenu = contextMenu?.Items[1] as MenuItem;
			var treeViewItem = TreeViewCommands.ItemContainerGenerator.ContainerFromItem (contextMenu?.PlacementTarget) as TreeViewItem ?? contextMenu?.PlacementTarget as TreeViewItem;
			treeViewItem.IsSelected = true;
			if (contextMenu?.DataContext is RootBatchNode)
			{
				((MenuItem) contextMenu?.Items[3]).IsEnabled = false;
			}
			else
			{
				((MenuItem) contextMenu?.Items[3]).IsEnabled = true;
			}

			PluginToMenu.InitializeCommands(commandsMenu?.Items, false);
			PluginToMenu.InitializeConditions(conditionsMenu, false);
		}

		private void ButtonBatchProcess_Click (object sender, RoutedEventArgs e)
		{
			Parallel.ForEach(FileInfo.Files, fileInfo => rootNode.Execute(fileInfo));
		}

		private void ButtonClose_Click (object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
