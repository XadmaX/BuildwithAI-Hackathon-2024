namespace Assistant.Functions.Abstractions.Generators;

[Generator]
internal class AssistantToolsGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        //System.Diagnostics.Debugger.Launch();
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var enumSource = GenerateEnumSource(FindFunctionTypes(context));

        context.AddSource("AssistantTools.g.cs", SourceText.From(enumSource, Encoding.UTF8));
    }

    private List<INamedTypeSymbol> FindFunctionTypes(GeneratorExecutionContext context)
    {
        var implementingTypes = new List<INamedTypeSymbol>();
        var compilation = context.Compilation;

        var interfaceName = "Assistant.Functions.Abstractions.Interfaces.IFunction`1";

        var interfaceSymbol = compilation.GetTypeByMetadataName(interfaceName);

        if (interfaceSymbol == null)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("IIG001", "Interface Not Found",
                    $"Interface {interfaceName} not found in the project.", "SourceGen", DiagnosticSeverity.Warning,
                    true),
                Location.None));
            return implementingTypes;
        }

        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var classDeclarations = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var classDecl in classDeclarations)
            {
                var classSymbol = semanticModel.GetDeclaredSymbol(classDecl)!;

                if (classSymbol == null || !ImplementsGenericInterface(classSymbol, interfaceSymbol))
                    continue;

                implementingTypes.Add(classSymbol);
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("IIG002", "Implementation Found",
                        $"Class {classSymbol.Name} implements {interfaceSymbol.Name}", "SourceGen",
                        DiagnosticSeverity.Info, true),
                    Location.None));
            }
        }

        foreach (var reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assemblySymbol) continue;

            var classes = GetAllTypes(assemblySymbol).Where(t => t.TypeKind == TypeKind.Class);

            foreach (var classSymbol in classes)
            {
                if (ImplementsGenericInterface(classSymbol, interfaceSymbol))
                {
                    implementingTypes.Add(classSymbol);
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor("IIG003", "Implementation Found in Assembly",
                            $"Class {classSymbol.Name} in assembly {assemblySymbol.Name} implements {interfaceSymbol.Name}",
                            "SourceGen", DiagnosticSeverity.Info, true),
                        Location.None));
                }
            }
        }

        return implementingTypes;
    }

    private IEnumerable<INamedTypeSymbol> GetAllTypes(IAssemblySymbol symbol)
    {
        foreach (var namespaceMember in symbol.GlobalNamespace.GetNamespaceMembers())
        {
            foreach (var namedTypeSymbol in GetTypesFromNamespace(namespaceMember))
            {
                yield return namedTypeSymbol;
            }
        }
    }

    private IEnumerable<INamedTypeSymbol> GetTypesFromNamespace(INamespaceSymbol namespaceSymbol)
    {
        foreach (var namedTypeSymbol in namespaceSymbol.GetTypeMembers())
        {
            yield return namedTypeSymbol;
        }

        foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            foreach (var namedTypeSymbol in GetTypesFromNamespace(nestedNamespace))
            {
                yield return namedTypeSymbol;
            }
        }
    }

    private bool ImplementsGenericInterface(INamedTypeSymbol classSymbol, INamedTypeSymbol interfaceSymbol)
    {
        return classSymbol.AllInterfaces.Any(i =>
            i.OriginalDefinition.Equals(interfaceSymbol, SymbolEqualityComparer.Default));
    }

    private string GenerateEnumSource(IEnumerable<INamedTypeSymbol> matchingTypes)
    {
        var sb = new StringBuilder();

        sb.AppendLine("using System;");
        sb.AppendLine();
        sb.AppendLine("namespace Assistant.Functions.Abstractions.Interfaces;");
        sb.AppendLine();
        sb.AppendLine("[Flags]");
        sb.AppendLine("public enum AssistantTools");
        sb.AppendLine("{");
        sb.AppendLine("\tNone = 0,");

        var bit = 1;

        foreach (var symbol in matchingTypes)
        {
            sb.AppendLine($"\t{symbol.Name} = {bit},");
            bit *= 2;
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    [Obsolete("Old method for search type in caller assembly")]
    private IEnumerable<INamedTypeSymbol> FindFunctionTypes(Compilation compilation)
    {
        return from syntax in compilation.SyntaxTrees
               let typeDeclarations = syntax.GetRoot().DescendantNodes()
                   .OfType<TypeDeclarationSyntax>()
               from typeDeclaration in typeDeclarations
               select compilation.GetSemanticModel(syntax).GetDeclaredSymbol(typeDeclaration)
            into symbol
               where symbol != null && symbol.AllInterfaces.Any(i =>
                   i.IsGenericType &&
                   i.ConstructedFrom.MetadataName == "IFunction`1")
               select symbol;
    }
}