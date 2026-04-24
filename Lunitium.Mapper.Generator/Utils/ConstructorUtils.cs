using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lunitium.Mapper.Generator.Utils;

public static class ConstructorUtils
{
    public static List<IParameterSymbol>? GetBestConstructorParameters(GeneratorAttributeSyntaxContext context,
        INamedTypeSymbol classSymbol)
    {
        var primaryCtorSyntax = classSymbol.DeclaringSyntaxReferences
            .Select(s => s.GetSyntax())
            .OfType<TypeDeclarationSyntax>()
            .FirstOrDefault(t => t.ParameterList != null);

        if (primaryCtorSyntax != null)
        {
            var primaryCtor = classSymbol.Constructors.FirstOrDefault(x =>
                x.DeclaringSyntaxReferences.Any(s => s.GetSyntax() == primaryCtorSyntax.ParameterList?.Parent));

            if (primaryCtor != null)
                return primaryCtor.Parameters.ToList();
        }

        var explicitCtors = classSymbol.Constructors
            .Where(c => !c.IsImplicitlyDeclared)
            .ToList();

        switch (explicitCtors.Count)
        {
            case 0:
            {
                var implicitCtor = classSymbol.Constructors.FirstOrDefault();
                return implicitCtor?.Parameters.ToList() ?? [];
            }
            case 1:
                return explicitCtors[0].Parameters.ToList();
        }

        var mapperConstructorSymbol =
            context.SemanticModel.Compilation.GetTypeByMetadataName(
                "Lunitium.Mapper.Attributes.MapperConstructorAttribute");

        var annotatedCtor = explicitCtors.FirstOrDefault(c =>
            c.GetAttributes().Any(a =>
                SymbolEqualityComparer.Default.Equals(a.AttributeClass, mapperConstructorSymbol)));

        return annotatedCtor?.Parameters.ToList();
    }
}