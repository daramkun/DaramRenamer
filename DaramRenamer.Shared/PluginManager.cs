using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace DaramRenamer
{
	public sealed class PluginManager : IDisposable
	{
		private static PluginManager _instance;
		public static PluginManager Instance => _instance ??= new PluginManager();

		private readonly List<IPluginInitializer> pluginInitializers = new List<IPluginInitializer>();
		private readonly List<ICommand> commands = new List<ICommand>();
		private readonly List<ICondition> conditions = new List<ICondition>();

		public IEnumerable<ICommand> Commands => commands;
		public IEnumerable<ICondition> Conditions => conditions;

		public IEnumerable<ICommand> GetCategoriedCommands(CommandCategory category)
			=> from command in Commands
				where command.Category == category
				orderby (command as IOrderBy)?.Order ?? 0
				select command;

		public PluginManager()
		{
			var defaultCommands = Assembly.Load (new AssemblyName ("DaramRenamer.Commands"));
			LoadPlugin (defaultCommands);
		}

		public void Dispose()
		{
			foreach (var pluginInitializer in pluginInitializers)
				pluginInitializer.Uninitialize();
		}

		public void LoadPlugins()
		{
			foreach (var path in FileInfo.FileOperator.GetFiles("Plugins", true))
				LoadPlugin(Assembly.LoadFile(path));
		}

		private void LoadPlugin(Assembly assembly)
		{
			Type pluginInitializerType = null;
			var commandsTypes = new List<Type>();
			var conditionsTypes = new List<Type>();
			foreach (var type in assembly.GetTypes())
			{
				var interfaces = type.GetInterfaces();
				if (interfaces.Contains(typeof(IPluginInitializer)))
				{
					if (pluginInitializerType != null)
						return;
					pluginInitializerType = type;
				}
				else if (interfaces.Contains(typeof(ICommand)))
					commandsTypes.Add(type);
				else if (interfaces.Contains(typeof(ICondition)))
					conditionsTypes.Add(type);
			}

			if (pluginInitializerType != null)
			{
				var pluginInitializer = Activator.CreateInstance(pluginInitializerType) as IPluginInitializer;
				pluginInitializer?.Initialize();
				pluginInitializers.Add(pluginInitializer);
			}

			foreach (var command in commandsTypes.Select(commandsType =>
				Activator.CreateInstance(commandsType) as ICommand))
				commands.Add(command);

			foreach (var condition in conditionsTypes.Select(conditionsType =>
				Activator.CreateInstance(conditionsType) as ICondition))
				conditions.Add(condition);

			if (pluginInitializerType == null && commandsTypes.Count == 0 && conditionsTypes.Count == 0)
				AssemblyLoadContext.GetLoadContext(assembly)?.Unload();
		}

		public ICommand FindCommand(string buttonTag)
		{
			return Commands.FirstOrDefault(command => command.GetType().GetCustomAttribute<LocalizationKeyAttribute>()?.LocalizationKey == buttonTag);
		}
	}
}
