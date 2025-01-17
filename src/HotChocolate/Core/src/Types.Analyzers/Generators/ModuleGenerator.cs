using System.Text;
using HotChocolate.Types.Analyzers.Inspectors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using static HotChocolate.Types.Analyzers.StringConstants;
using static HotChocolate.Types.Analyzers.WellKnownFileNames;
using TypeInfo = HotChocolate.Types.Analyzers.Inspectors.TypeInfo;

namespace HotChocolate.Types.Analyzers.Generators;

public class ModuleGenerator : ISyntaxGenerator
{
    public bool Consume(ISyntaxInfo syntaxInfo)
        => syntaxInfo is TypeInfo or TypeExtensionInfo or DataLoaderInfo or ModuleInfo;

    public void Generate(
        SourceProductionContext context,
        Compilation compilation,
        IReadOnlyCollection<ISyntaxInfo> syntaxInfos)
    {
        var module =
            syntaxInfos.OfType<ModuleInfo>().FirstOrDefault() ??
            new ModuleInfo(
                compilation.AssemblyName is null
                    ? "AssemblyTypes"
                    : compilation.AssemblyName?.Split('.').Last() + "Types",
                ModuleOptions.Default);

        var batch = new List<ISyntaxInfo>(syntaxInfos.Where(static t => t is not ModuleInfo));
        if (batch.Count == 0)
        {
            return;
        }

        var code = new StringBuilder();
        code.AppendLine("using System;");
        code.AppendLine("using HotChocolate.Execution.Configuration;");

        code.AppendLine();
        code.AppendLine("namespace Microsoft.Extensions.DependencyInjection");
        code.AppendLine("{");

        code.Append(Indent)
            .Append("public static class ")
            .Append(module.ModuleName)
            .AppendLine("RequestExecutorBuilderExtensions");

        code.Append(Indent)
            .AppendLine("{");

        code.Append(Indent)
            .Append(Indent)
            .Append("public static IRequestExecutorBuilder Add")
            .Append(module.ModuleName)
            .AppendLine("(this IRequestExecutorBuilder builder)");

        code.Append(Indent).Append(Indent).AppendLine("{");

        var operations = OperationType.No;

        foreach (var syntaxInfo in batch.Distinct())
        {
            switch (syntaxInfo)
            {
                case TypeInfo type:
                    if ((module.Options & ModuleOptions.RegisterTypes) ==
                        ModuleOptions.RegisterTypes)
                    {
                        code.Append(Indent)
                            .Append(Indent)
                            .Append(Indent)
                            .Append("builder.AddType<")
                            .Append(type.Name)
                            .AppendLine(">();");
                    }
                    break;

                case TypeExtensionInfo extension:
                    if ((module.Options & ModuleOptions.RegisterTypes) ==
                        ModuleOptions.RegisterTypes)
                    {
                        if (extension.IsStatic)
                        {
                            code.Append(Indent)
                                .Append(Indent)
                                .Append(Indent)
                                .Append("builder.AddTypeExtension(typeof(")
                                .Append(extension.Name)
                                .AppendLine("));");
                        }
                        else
                        {
                            code.Append(Indent)
                                .Append(Indent)
                                .Append(Indent)
                                .Append("builder.AddTypeExtension<")
                                .Append(extension.Name)
                                .AppendLine(">();");
                        }

                        if (extension.Type is not OperationType.No &&
                            (operations & extension.Type) != extension.Type)
                        {
                            operations |= extension.Type;
                        }
                    }
                    break;

                case DataLoaderInfo dataLoader:
                    if ((module.Options & ModuleOptions.RegisterDataLoader) ==
                        ModuleOptions.RegisterDataLoader)
                    {
                        code.Append(Indent)
                            .Append(Indent)
                            .Append(Indent)
                            .Append("builder.AddDataLoader<")
                            .Append(dataLoader.Name)
                            .AppendLine(">();");
                    }
                    break;
            }
        }

        if ((operations & OperationType.Query) == OperationType.Query)
        {
            WriteTryAddOperationType(code, OperationType.Query);
        }

        if ((operations & OperationType.Mutation) == OperationType.Mutation)
        {
            WriteTryAddOperationType(code, OperationType.Mutation);
        }

        if ((operations & OperationType.Subscription) == OperationType.Subscription)
        {
            WriteTryAddOperationType(code, OperationType.Subscription);
        }

        code.Append(Indent).Append(Indent).Append(Indent).AppendLine("return builder;");
        code.Append(Indent).Append(Indent).AppendLine("}");
        code.Append(Indent).AppendLine("}");
        code.AppendLine("}");

        context.AddSource(TypeModuleFile, SourceText.From(code.ToString(), Encoding.UTF8));
    }

    private static void WriteTryAddOperationType(StringBuilder code, OperationType type)
        => code.Append(Indent)
            .Append(Indent)
            .Append(Indent)
            .Append("builder.ConfigureSchema(")
            .AppendLine()
            .Append(Indent)
            .Append(Indent)
            .Append(Indent)
            .Append(Indent)
            .Append("b => b.TryAddRootType(")
            .AppendLine()
            .Append(Indent)
            .Append(Indent)
            .Append(Indent)
            .Append(Indent)
            .Append(Indent)
            .Append("() => new global::HotChocolate.Types.ObjectType(")
            .AppendLine()
            .Append(Indent)
            .Append(Indent)
            .Append(Indent)
            .Append(Indent)
            .Append(Indent)
            .Append(Indent)
            .Append($"d => d.Name(global::HotChocolate.Types.OperationTypeNames.{type})),")
            .AppendLine()
            .Append(Indent)
            .Append(Indent)
            .Append(Indent)
            .Append(Indent)
            .Append(Indent)
            .Append($"HotChocolate.Language.OperationType.{type}));")
            .AppendLine();
}
