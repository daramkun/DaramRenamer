using System.Collections.Generic;
using System.Windows.Controls;

namespace DaramRenamer
{
	public static class PluginToMenu
	{
		public static void Initialize(ItemCollection commandsMenuItems)
		{
			Dictionary<CommandCategory, MenuItem> menuItems = new Dictionary<CommandCategory, MenuItem>();
			menuItems.Add(CommandCategory.Filename, commandsMenuItems[0] as MenuItem);
			menuItems.Add(CommandCategory.Extension, commandsMenuItems[1] as MenuItem);
			menuItems.Add(CommandCategory.Path, commandsMenuItems[2] as MenuItem);
			menuItems.Add(CommandCategory.Number, commandsMenuItems[3] as MenuItem);
			menuItems.Add(CommandCategory.Date, commandsMenuItems[4] as MenuItem);
			menuItems.Add(CommandCategory.Tag, commandsMenuItems[5] as MenuItem);
			menuItems.Add(CommandCategory.Etc, commandsMenuItems[6] as MenuItem);

			foreach (var (category, menuItem) in menuItems)
			{
				menuItem.ItemsSource = PluginManager.Instance.GetCategoriedCommands(category);
			}
		}
	}
}
