using System.Collections.Immutable;
using System.Text;
using Lunitium.Mapper.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Lunitium.Mapper;

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

    private static MapperToRegister ParseData(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        var classSymbol = (INamedTypeSymbol)context.TargetSymbol;
        var attributeData = classSymbol.GetAttributes().First(a => a.AttributeClass?.MetadataName == "MappingAttribute`1");

        return new MapperToRegister
        {
            ModelName = classSymbol.ToDisplayString(),
            DtoName = attributeData.AttributeClass!.TypeArguments[0].ToDisplayString(),
        };
    }
        
    private static void Generate(SourceProductionContext context, ImmutableArray<MapperToRegister> mappers)
    {
        var result = string.Join(", ", mappers.Select(m => m.ModelName));

        context.AddSource("Mappers.g.cs", SourceText.From(result, Encoding.UTF8));
    }
}