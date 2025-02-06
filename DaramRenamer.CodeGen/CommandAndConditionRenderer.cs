using System.Linq;

namespace DaramRenamer.CodeGen;

public static class CommandAndConditionRenderer
{
    private const string TemplateText =
        """
        using System;
        using System.Collections.Generic;
        using System.Collections.Immutable;
        
        namespace DaramRenamer;

        static partial class CommandsCollection
        {{
            public static ImmutableArray<Type> Types {{ get; }} = [
        {0}
            ];
            
            public static BaseCommand CreateCommand(Type commandType)
            {{
        {2}
                throw new ArgumentOutOfRangeException(nameof(commandType));
            }}
            
            public static CommandCategory GetCommandCategory(Type commandType)
            {{
        {4}
                throw new ArgumentOutOfRangeException(nameof(commandType));
            }}
            
            public static int GetCommandOrder(Type commandType)
            {{
        {5}
                throw new ArgumentOutOfRangeException(nameof(commandType));
            }}
        }}

        static partial class ConditionsCollection
        {{
            public static ImmutableArray<Type> Types {{ get; }} = [
        {1}
            ];
        
            public static BaseCondition CreateCondition(Type conditionType)
            {{
        {3}
                throw new ArgumentOutOfRangeException(nameof(conditionType));
            }}
        }}

        """;

    public static string Render(CommandAndConditionReceiver receiver)
    {
        var commandTypes = string.Join("\n", receiver.Commands.Select(command => $"        typeof({command.Name}),"));
        var conditionTypes = string.Join("\n", receiver.Conditions.Select(condition => $"        typeof({condition}),"));

        var commandCreateConditions = string.Join("\n",
            receiver.Commands.Select(command => $"        if (commandType == typeof({command.Name})) new {command.Name}();"));
        var conditionCreateConditions = string.Join("\n",
            receiver.Conditions.Select(condition => $"        if (conditionType == typeof({condition})) new {condition}();"));

        var commandCategoryConditions = string.Join("\n",
            receiver.Commands.Select(command => $"        if (commandType == typeof({command.Name})) return CommandCategory.{command.CommandCategory ?? "NoCategorized"};"));
        var commandOrderConditions = string.Join("\n",
            receiver.Commands.Select(command => $"        if (commandType == typeof({command.Name})) return {command.Order};"));

        return string.Format(TemplateText,
            commandTypes, conditionTypes,
            commandCreateConditions, conditionCreateConditions,
            commandCategoryConditions, commandOrderConditions);
    }
}