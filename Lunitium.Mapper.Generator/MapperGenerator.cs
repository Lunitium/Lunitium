using System.Collections.Immutable;
using System.Text;
using Lunitium.Mapper.Generator.Enums;
using Lunitium.Mapper.Generator.Models;
using Lunitium.Mapper.Generator.Templates;
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
        var classSymbol = (INamedTypeSymbol)context.TargetSymbol;

        return new MapperInfos
        {
            Namespace = classSymbol.ContainingNamespace.ToDisplayString(),
            ModelName = classSymbol.Name,
            Infos = classSymbol.GetAttributes()
                .Where(x => x.AttributeClass?.MetadataName == "MappingAttribute`1")
                .Select(attributeData =>
                {
                    var mapDirectionValue = (int?)attributeData.ConstructorArguments.FirstOrDefault().Value;
                    var dtoSymbol = (INamedTypeSymbol)attributeData.AttributeClass!.TypeArguments.First();

                    var modelProps = GetProps(classSymbol);
                    var dtoProps = GetProps(dtoSymbol);
                    var props = MapperProperty.Mapping(context, modelProps, dtoProps);

                    return new MapperInfo()
                    {
                        AttributeSymbol = attributeData.ApplicationSyntaxReference!.GetSyntax(),
                        ModelSymbol = classSymbol,
                        DtoSymbol = dtoSymbol,
                        MapDirection = mapDirectionValue is null ? MapDirection.All : (MapDirection)mapDirectionValue,
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