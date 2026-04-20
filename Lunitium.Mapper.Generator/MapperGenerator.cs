using System.Collections.Immutable;
using System.Text;
using Lunitium.Mapper.Generator.Analysis;
using Lunitium.Mapper.Generator.Enums;
using Lunitium.Mapper.Generator.Models;
using Lunitium.Mapper.Generator.Templates;
using Microsoft.CodeAnalysis;
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

    private static MapperToRegister ParseData(GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken)
    {
        var classSymbol = (INamedTypeSymbol)context.TargetSymbol;
        var attributeData = classSymbol.GetAttributes()
            .First(a => a.AttributeClass?.MetadataName == "MappingAttribute`1");
        var mapDirectionValue = (int?)attributeData.ConstructorArguments.FirstOrDefault().Value;
        var dtoSymbol = (INamedTypeSymbol)attributeData.AttributeClass!.TypeArguments.First();

        var modelProps = GetProps(classSymbol);
        var dtoProps = GetProps(dtoSymbol);

        var missingPropsErrors = dtoProps.Where(d => modelProps.All(m => m.Name != d.Name))
            .Select(d => Diagnostic.Create(
                AnalysisError.ModelDontHaveThisProp,
                attributeData.ApplicationSyntaxReference!.GetSyntax().GetLocation(),
                classSymbol.Name,
                d.Name
            ));

        return new MapperToRegister
        {
            Namespace = classSymbol.ContainingNamespace.ToDisplayString(),
            ModelSymbol = classSymbol,
            DtoSymbol = dtoSymbol,
            ModelProps = modelProps,
            DtoProps = dtoProps,
            MapDirection = mapDirectionValue is null ? MapDirection.All : (MapDirection)mapDirectionValue,
            Errors = missingPropsErrors
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

    private static void Generate(SourceProductionContext context, ImmutableArray<MapperToRegister> mappers)
    {
        foreach (var mapperToRegister in mappers)
        {
            foreach (var diagnostic in mapperToRegister.Errors)
            {
                context.ReportDiagnostic(diagnostic);
            }

            var template = new MapperDtoRecordTemplate(mapperToRegister);
            var result = template.Render();
            context.AddSource($"{mapperToRegister.Namespace}.{mapperToRegister.ModelSymbol.Name}.cs",
                SourceText.From(result, Encoding.UTF8)
            );
        }
    }
}