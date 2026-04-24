using System.Collections.Immutable;
using System.Text;
using Lunitium.Mapper.Generator.Enums;
using Lunitium.Mapper.Generator.Models;
using Lunitium.Mapper.Generator.Templates;
using Lunitium.Mapper.Generator.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Lunitium.Mapper.Generator;

[Generator]
public class MapperGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var valueProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: "Lunitium.Mapper.Attributes.MappingAttribute`1",
                predicate: (node, _) => node is ClassDeclarationSyntax,
                transform: ParseData
            )
            .Collect();

        context.RegisterSourceOutput(valueProvider, Generate);
    }

    private static MapperInfos ParseData(GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken)
    {
        var modelSymbol = (INamedTypeSymbol)context.TargetSymbol;

        return new MapperInfos
        {
            Namespace = modelSymbol.ContainingNamespace.ToDisplayString(),
            ModelName = modelSymbol.Name,
            Infos = modelSymbol.GetAttributes()
                .Where(x => x.AttributeClass?.MetadataName == "MappingAttribute`1")
                .Select(attributeData =>
                {
                    var mapAction = attributeData.NamedArguments.FirstOrDefault(x => x.Key == "Action").Value;
                    var mapActionValue = mapAction is { IsNull: false, Value: not null }
                        ? (MapAction)(byte)mapAction.Value
                        : MapAction.All;

                    var targetSymbol = (INamedTypeSymbol)attributeData.AttributeClass!.TypeArguments.First();

                    var modelProps = GetProps(modelSymbol);
                    var targetProps = GetProps(targetSymbol);
                    var props = MapperProperty.Mapping(context, modelProps, targetProps);

                    return new MapperInfo()
                    {
                        AttributeSymbol = attributeData.ApplicationSyntaxReference!.GetSyntax(),
                        ModelSymbol = modelSymbol,
                        TargetSymbol = targetSymbol,
                        MapAction = mapActionValue,
                        ModelConstructorParameters =
                            ConstructorUtils.GetBestConstructorParameters(context, modelSymbol),
                        TargetConstructorParameters =
                            ConstructorUtils.GetBestConstructorParameters(context, targetSymbol),
                        Props = props
                    };
                }).ToList()
        };
    }

    private static List<IPropertySymbol> GetProps(INamedTypeSymbol classSymbol)
    {
        return classSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public)
            .Where(p => !p.IsStatic)
            .Where(p => !p.IsIndexer)
            .Where(p => p.Name != "EqualityContract")
            .ToList();
    }

    private static void Generate(SourceProductionContext context, ImmutableArray<MapperInfos> mappers)
    {
        foreach (var mapperToRegister in mappers)
        {
            var template = new MapperTemplate(context, mapperToRegister);
            var result = template.Render();
            context.AddSource($"{mapperToRegister.Namespace}.{mapperToRegister.ModelName}.cs",
                SourceText.From(result, Encoding.UTF8)
            );
        }
    }
}