using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DaramRenamer.CodeGen;

public class CommandInfo(string name, string? commandCategory, int order)
{
    public string Name { get; } = name;
    public string? CommandCategory { get; } = commandCategory;
    public int Order { get; } = order;
}

public class CommandAndConditionReceiver : ISyntaxContextReceiver
{
    private readonly List<CommandInfo> _commands = [];
    private readonly List<string> _conditions = [];

    public IEnumerable<CommandInfo> Commands => _commands;
    public IEnumerable<string> Conditions => _conditions;
        
    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classDeclarationSyntax)
            return;

        var declaredSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
        if (declaredSymbol == null)
            return;
        
        var isCommand = declaredSymbol.BaseType?.Name == "BaseCommand";
        var isCondition = declaredSymbol.BaseType?.Name == "BaseCondition";

        if (!isCommand && !isCondition)
            return;

        var typeFullName = $"{GetFullNamespace(declaredSymbol.ContainingNamespace)}.{declaredSymbol.Name}";

        if (isCondition)
        {
            _conditions.Add(typeFullName);
            return;
        }

        var commandCategoryAttribute = declaredSymbol.GetAttributes().FirstOrDefault(attr => attr.AttributeClass?.Name == "CommandCategoryAttribute");
        var orderAttribute = declaredSymbol.GetAttributes().FirstOrDefault(attr => attr.AttributeClass?.Name == "OrderAttribute");

        var commandCategory = commandCategoryAttribute?.ConstructorArguments[0].Value?.ToString();
        var order = (int)(orderAttribute?.ConstructorArguments[0].Value ?? int.MinValue);

        if (isCommand)
        {
            _commands.Add(new CommandInfo(typeFullName, commandCategory, order));
        }
    }

    private static string GetFullNamespace(INamespaceSymbol namespaceSymbol)
    {
        List<string> namespaces = [];
        while (namespaceSymbol != null)
        {
            if (string.IsNullOrEmpty(namespaceSymbol.Name))
                break;
            
            namespaces.Add(namespaceSymbol.Name);
            namespaceSymbol = namespaceSymbol.ContainingNamespace;
        }

        namespaces.Reverse();
        
        return string.Join(".", namespaces);
    }
}