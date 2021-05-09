using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace DaramRenamer
{
	public static class PluginToMenu
	{
		private static readonly List<ObservableCollection<ICommand>> InitializedCommandLists = new();
		private static ObservableCollection<ICondition> _initializedConditionList = new();

		public static void InitializeCommands(ItemCollection commandsMenuItems, bool initializeMain = true)
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
				var initializedCommandList = new ObservableCollection<ICommand>(PluginManager.Instance.GetCategoriedCommands(category));
				menuItem.ItemsSource = initializedCommandList;
				if (initializeMain)
					InitializedCommandLists.Add(initializedCommandList);
			}
		}

		public static void InitializeConditions(MenuItem conditionsMenu, bool initializeMain = true)
		{
			if (initializeMain)
				_initializedConditionList = new ObservableCollection<ICondition>(PluginManager.Instance.Conditions);
			conditionsMenu.ItemsSource = _initializedConditionList;
		}

		public static void RefreshBinding()
		{
			foreach (var list in InitializedCommandLists)
			{
				for (var i = 0; i < list.Count; ++i)
				{
					var temp = list[i];
					list[i] = null;
					list[i] = temp;
				}
			}


			for (var i = 0; i < _initializedConditionList.Count; ++i)
			{
				var temp = _initializedConditionList[i];
				_initializedConditionList[i] = null;
				_initializedConditionList[i] = temp;
			}
		}
	}
}
