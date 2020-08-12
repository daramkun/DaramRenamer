using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace DaramRenamer
{
	public static class PluginToMenu
	{
		private static List<ObservableCollection<ICommand>> initializedCommandLists = new List<ObservableCollection<ICommand>> ();
		private static ObservableCollection<ICondition> initializedConditionList = new ObservableCollection<ICondition>();

		public static void InitializeCommands(ItemCollection commandsMenuItems)
		{
			var menuItems = new Dictionary<CommandCategory, MenuItem>
			{
				{CommandCategory.Filename, commandsMenuItems[0] as MenuItem},
				{CommandCategory.Extension, commandsMenuItems[1] as MenuItem},
				{CommandCategory.Path, commandsMenuItems[2] as MenuItem},
				{CommandCategory.Number, commandsMenuItems[3] as MenuItem},
				{CommandCategory.Date, commandsMenuItems[4] as MenuItem},
				{CommandCategory.Tag, commandsMenuItems[5] as MenuItem},
				{CommandCategory.Etc, commandsMenuItems[6] as MenuItem}
			};

			foreach (var (category, menuItem) in menuItems)
			{
				var initializedCommandList = new ObservableCollection<ICommand>(PluginManager.Instance.GetCategoriedCommands (category));
				menuItem.ItemsSource = initializedCommandList;
				initializedCommandLists.Add(initializedCommandList);
			}
		}

		public static void InitializeConditions(MenuItem conditionsMenu)
		{
			initializedConditionList = new ObservableCollection<ICondition>(PluginManager.Instance.Conditions);
			conditionsMenu.ItemsSource = initializedConditionList;
		}

		public static void RefreshBinding()
		{
			foreach (var list in initializedCommandLists)
			{
				for (var i = 0; i < list.Count; ++i)
				{
					var temp = list[i];
					list[i] = null;
					list[i] = temp;
				}
			}


			for (var i = 0; i < initializedConditionList.Count; ++i)
			{
				var temp = initializedConditionList [i];
				initializedConditionList [i] = null;
				initializedConditionList [i] = temp;
			}
		}
	}
}
