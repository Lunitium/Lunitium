using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Lunitium.DependencyInjection.Analysis;
using Lunitium.DependencyInjection.Attributes;
using Lunitium.DependencyInjection.Enums;
using Lunitium.DependencyInjection.Models;
using Lunitium.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Lunitium.DependencyInjection;

[Generator]
public class DependencyGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var valueProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: GeneratorFilter.FilterClassAttribute(nameof(DependencyAttribute)),
                transform: ParseData
            )
            .Where(static service => service is not null)
            .Collect();

        context.RegisterSourceOutput(valueProvider, Generate);
    }

    private static ServiceToRegister? ParseData(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        var node = (ClassDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(node, cancellationToken) is not { } classSymbol)
            return null;

        foreach (var attributeData in classSymbol.GetAttributes())
        {
            if (attributeData.AttributeClass?.Name != "DependencyAttribute")
                continue;

            ITypeSymbol? interfaceType = null;

            if (attributeData.AttributeClass.IsGenericType)
            {
                interfaceType = attributeData.AttributeClass.TypeArguments.FirstOrDefault();
            }

            var selfClass = classSymbol.ToDisplayString();
            var lifeTimeValue = (int?)attributeData.ConstructorArguments.FirstOrDefault().Value;

            string? key = null;
            var keyArg = attributeData.NamedArguments.FirstOrDefault(x => x.Key == nameof(DependencyAttribute.Key));
            if (!keyArg.Value.IsNull)
            {
                key = keyArg.Value.ToCSharpString();
            }

            var serviceFactoryToRegister = ParseFactory(classSymbol, !string.IsNullOrWhiteSpace(key));

            return new ServiceToRegister
            {
                Name = selfClass,
                LifeTime = lifeTimeValue is null ? LifeTime.Scoped : (LifeTime)lifeTimeValue,
                InterfaceName = interfaceType?.ToDisplayString(),
                KeyLiteral = key,
                Factory = serviceFactoryToRegister,
            };
        }

        return null;
    }

    private static ServiceFactoryToRegister? ParseFactory(INamedTypeSymbol classSymbol, bool isKeyed)
    {
        var factoryArgs = classSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(x => x.GetAttributes()
                .Any(a => a.AttributeClass?.Name is "DependencyFactoryAttribute")
            ).ToList();

        if (!factoryArgs.Any())
            return null;

        if (factoryArgs.Count > 1)
        {
            return new ServiceFactoryToRegister
            {
                Errors = factoryArgs.Select(x => Diagnostic.Create(
                    AnalysisError.MoreThanOneFactory,
                    x.Locations[0],
                    classSymbol.Name
                ))
            };
        }

        var factoryMethod = factoryArgs[0];

        if (factoryMethod is null)
            return null;

        var functionRefName = $"{classSymbol.Name}.{factoryMethod.Name}";
        if (!factoryMethod.IsStatic)
        {
            return new ServiceFactoryToRegister
            {
                Errors =
                [
                    Diagnostic.Create(
                        AnalysisError.NotStaticMethod,
                        factoryMethod.Locations[0],
                        functionRefName
                    )
                ]
            };
        }

        var maxNumberOfParameters = isKeyed ? 2 : 1;
        if (factoryMethod.Parameters.Length > maxNumberOfParameters)
        {
            return new ServiceFactoryToRegister
            {
                Errors =
                [
                    Diagnostic.Create(
                        AnalysisError.ManyParameters,
                        factoryMethod.Locations[0],
                        functionRefName,
                        maxNumberOfParameters
                    )
                ]
            };
        }

        if (isKeyed)
        {
            var parameterSymbols = factoryMethod.Parameters.Where(p =>
                p.Type.ToDisplayString() != "System.IServiceProvider" &&
                p.Type.SpecialType != SpecialType.System_Object).ToList();

            if (parameterSymbols.Any())
            {
                return new ServiceFactoryToRegister
                {
                    Errors = parameterSymbols.Select(p => Diagnostic.Create(
                        AnalysisError.ParameterTypeNotAllowed,
                        p.Locations[0],
                        p.Type.Name
                    ))
                };
            }
        }
        else
        {
            var parameterSymbols = factoryMethod.Parameters.Where(p =>
                p.Type.ToDisplayString() != "System.IServiceProvider").ToList();

            if (parameterSymbols.Any())
            {
                return new ServiceFactoryToRegister
                {
                    Errors = parameterSymbols.Select(p => Diagnostic.Create(
                        AnalysisError.ParameterTypeNotAllowed,
                        p.Locations[0],
                        p.Type.Name
                    ))
                };
            }
        }

        var parameterOrder = factoryMethod.Parameters.Select(p =>
        {
            if (p.Type.ToDisplayString() == "System.IServiceProvider")
            {
                return FactoryParameter.Service;
            }

            return p.Type.SpecialType == SpecialType.System_Object
                ? FactoryParameter.Key
                : throw new InvalidOperationException("Parâmetro de factory inválido.");
        }).ToArray();

        return new ServiceFactoryToRegister
        {
            Parameters = parameterOrder,
            FactoryName = factoryMethod.Name
        };
    }

    private static void Generate(SourceProductionContext context, ImmutableArray<ServiceToRegister?> services)
    {
        var result = GenerateExtensionClass(context, services);

        context.AddSource("DependencyInjection.g.cs", SourceText.From(result, Encoding.UTF8));
    }

    private static string GenerateExtensionClass(SourceProductionContext context,
        ImmutableArray<ServiceToRegister?> services)
    {
        var sb = new StringBuilder(2048);

        sb.AppendLine("// <auto-generated />");
        sb.AppendLine();
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine();
        sb.AppendLine("namespace Lunitium.DependencyInjection;");
        sb.AppendLine();
        sb.AppendLine("public static class DependencyInjectionExtensions");
        sb.AppendLine("{");
        sb.AppendLine("    public static IServiceCollection AddLunitiumDependencies(this IServiceCollection services)");
        sb.AppendLine("    {");

        foreach (var service in Enumerable.OfType<ServiceToRegister>(services))
        {
            if (service.Factory?.Errors.Any() ?? false)
            {
                foreach (var factoryError in service.Factory.Errors)
                {
                    context.ReportDiagnostic(factoryError);
                }

                continue;
            }

            var factoryName = service.Factory is null
                ? null
                : $"sp => {service.Name}.{service.Factory.FactoryName}({ParseParameters(service.Factory.Parameters)})";

            if (string.IsNullOrWhiteSpace(service.KeyLiteral))
            {
                sb.AppendLine(!string.IsNullOrEmpty(service.InterfaceName)
                    ? $"        services.Add{service.LifeTime.ToString()}<{service.InterfaceName}, {service.Name}>({factoryName});"
                    : $"        services.Add{service.LifeTime.ToString()}<{service.Name}>({factoryName});");
                continue;
            }

            var parameters = service.Factory is null
                ? service.KeyLiteral
                : $"{service.KeyLiteral}, (sp, k) => {service.Name}.{service.Factory.FactoryName}({ParseParameters(service.Factory.Parameters)})";

            sb.AppendLine(!string.IsNullOrEmpty(service.InterfaceName)
                ? $"        services.AddKeyed{service.LifeTime.ToString()}<{service.InterfaceName}, {service.Name}>({parameters});"
                : $"        services.AddKeyed{service.LifeTime.ToString()}<{service.Name}>({parameters});");
        }

        sb.AppendLine("        return services;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string ParseParameters(IEnumerable<FactoryParameter> parameters)
    {
        return string.Join(", ", parameters.Select(p => p switch
        {
            FactoryParameter.Service => "sp",
            FactoryParameter.Key => "k",
            _ => throw new ArgumentOutOfRangeException(nameof(p), p, null)
        }));
    }
}