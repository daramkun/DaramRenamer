using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#pragma warning disable RS1035
#pragma warning disable RS1036

namespace DaramRenamer.CodeGen;

[Generator]
public class CommandAndConditionCollectionGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new CommandAndConditionReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not CommandAndConditionReceiver receiver)
            return;
        
        var rendered = CommandAndConditionRenderer.Render(receiver);
        context.AddSource("CommandsAndConditionsCollection.g.cs", rendered);
    }
}