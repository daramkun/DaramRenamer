﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace DaramRenamer;

public sealed class PluginManager : IDisposable
{
    private static PluginManager _instance;
    private readonly List<ICommand> _commands = new();
    private readonly List<ICondition> _conditions = new();

    private readonly List<IPluginInitializer> _pluginInitializers = new();

    public PluginManager()
    {
        var defaultCommands = Assembly.Load(new AssemblyName("DaramRenamer.Commands"));
        LoadPlugin(defaultCommands);
    }

    public static PluginManager Instance => _instance ??= new PluginManager();

    public IEnumerable<ICommand> Commands => _commands;
    public IEnumerable<ICondition> Conditions => _conditions;

    public void Dispose()
    {
        foreach (var pluginInitializer in _pluginInitializers)
            pluginInitializer.Uninitialize();
    }

    public IEnumerable<ICommand> GetCategoriedCommands(CommandCategory category)
    {
        return from command in Commands
            where command.Category == category
            orderby (command as IOrderBy)?.Order ?? 0
            select command;
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
            {
                commandsTypes.Add(type);
            }
            else if (interfaces.Contains(typeof(ICondition)))
            {
                conditionsTypes.Add(type);
            }
        }

        if (pluginInitializerType != null)
        {
            var pluginInitializer = Activator.CreateInstance(pluginInitializerType) as IPluginInitializer;
            pluginInitializer?.Initialize();
            _pluginInitializers.Add(pluginInitializer);
        }

        foreach (var command in commandsTypes.Select(commandsType =>
                     Activator.CreateInstance(commandsType) as ICommand))
            _commands.Add(command);

        foreach (var condition in conditionsTypes.Select(conditionsType =>
                     Activator.CreateInstance(conditionsType) as ICondition))
            _conditions.Add(condition);

        if (pluginInitializerType == null && commandsTypes.Count == 0 && conditionsTypes.Count == 0)
            AssemblyLoadContext.GetLoadContext(assembly)?.Unload();
    }

    public ICommand FindCommand(string buttonTag)
    {
        return Commands.FirstOrDefault(command =>
            command.GetType().GetCustomAttribute<LocalizationKeyAttribute>()?.LocalizationKey == buttonTag);
    }
}