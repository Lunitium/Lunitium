using System.Text;
using Lunitium.Mapper.Generator.Analysis;
using Lunitium.Mapper.Generator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lunitium.Mapper.Generator.Templates;

public class MapperToTemplate(SourceProductionContext context, MapperInfo mapper)
{
    public string Render()
    {
        var sb = new StringBuilder(2048);

        sb.AppendLine($"    public {mapper.DtoSymbol.ToDisplayString()} To{mapper.DtoSymbol.Name}()");
        sb.AppendLine("    {");
        sb.AppendLine($"        return ({mapper.DtoSymbol.ToDisplayString()})this;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine($"    public static implicit operator {mapper.DtoSymbol.ToDisplayString()}({mapper.ModelSymbol.Name} value)");
        sb.AppendLine("    {");
        sb.AppendLine($"        return new {mapper.DtoSymbol.ToDisplayString()}(");
        sb.AppendLine($"            {RenderDtoCtor()}");
        sb.AppendLine("        );");
        sb.AppendLine("    }");

        return sb.ToString();
    }

    private string RenderDtoCtor()
    {
        var ctorProps = GetConstructorProps(mapper.DtoSymbol);
        var parameters = new List<string>();

        foreach (var ctorProp in ctorProps)
        {
            var prop = mapper.Props.FirstOrDefault(x => x.Key.Equals(ctorProp.Name, StringComparison.OrdinalIgnoreCase));
            var symbol = prop.Value.ModelProp;

            if (symbol != null)
            {
                var conversion = prop.Value.ModelToDto;

                if (conversion is { Exists: false })
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        AnalysisError.DontMatchType,
                        mapper.AttributeSymbol.GetLocation(),
                        symbol.Name,
                        ctorProp.Type.ToDisplayString()
                    ));
                    continue;
                }

                if (ctorProp.Type.NullableAnnotation == NullableAnnotation.NotAnnotated &&
                    symbol.Type.NullableAnnotation == NullableAnnotation.Annotated)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        AnalysisError.NullableMismatch,
                        mapper.AttributeSymbol.GetLocation(),
                        symbol.Name,
                        ctorProp.Type.ToDisplayString()
                    ));
                }

                if (conversion is { IsExplicit: true })
                {
                    parameters.Add($"({ctorProp.Type.ToDisplayString()})value.{symbol.Name}");
                    continue;
                }

                parameters.Add($"value.{symbol.Name}");
                continue;
            }

            if (ctorProp.HasExplicitDefaultValue)
            {
                var parameterSyntax =
                    ctorProp.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as ParameterSyntax;

                parameters.Add($"{parameterSyntax?.Default?.Value.ToString()}");
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                AnalysisError.ModelDontHaveThisProp,
                mapper.AttributeSymbol.GetLocation(),
                mapper.ModelSymbol.Name,
                ctorProp.Name
            ));
        }

        return string.Join(", ", parameters);
    }
    
    private static List<IParameterSymbol> GetConstructorProps(INamedTypeSymbol classSymbol)
    {
        return classSymbol.IsRecord ? GetRecordConstructorProps(classSymbol) : GetClassConstructorProps(classSymbol);
    }

    private static List<IParameterSymbol> GetRecordConstructorProps(INamedTypeSymbol classSymbol)
    {
        var ctor = classSymbol.Constructors.FirstOrDefault(x =>
            x.DeclaringSyntaxReferences.Any(s => s.GetSyntax() is RecordDeclarationSyntax));
        
        return ctor == null ? [] : ctor.Parameters.ToList();
    }

    private static List<IParameterSymbol> GetClassConstructorProps(INamedTypeSymbol classSymbol)
    {
        var ctor = classSymbol.Constructors.FirstOrDefault(x =>
            x.DeclaringSyntaxReferences.Any(s => s.GetSyntax() is ClassDeclarationSyntax));

        return ctor == null ? [] : ctor.Parameters.ToList();
    }

    private static List<IParameterSymbol> GetBestConstructorParameters(INamedTypeSymbol classSymbol)
    {
        // 1. Tenta obter o Construtor Primário (Records ou Class C# 12+)
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

        // 2. Filtra construtores declarados explicitamente
        var explicitCtors = classSymbol.Constructors
            .Where(c => !c.IsImplicitlyDeclared)
            .ToList();

        // 3. Se não houver nenhum construtor explícito (ou primário)
        if (explicitCtors.Count == 0)
        {
            // Tenta o construtor implícito padrão (vazio)
            var implicitCtor = classSymbol.Constructors.FirstOrDefault();
            return implicitCtor?.Parameters.ToList() ?? new List<IParameterSymbol>();
        }

        // 4. Se houver apenas um construtor manual
        if (explicitCtors.Count == 1)
        {
            return explicitCtors[0].Parameters.ToList();
        }

        // 5. Se houver múltiplos, busca obrigatoriamente pelo atributo MapperConstructor
        var annotatedCtor = explicitCtors.FirstOrDefault(c =>
            c.GetAttributes().Any(a => a.AttributeClass?.MetadataName is "MapperConstructor" or "MapperConstructorAttribute"));

        if (annotatedCtor != null)
        {
            return annotatedCtor.Parameters.ToList();
        }

        return [];
        // Se chegou aqui, há mais de 1 ctor e nenhum marcado: erro de ambiguidade
        throw new Exception($"A classe {classSymbol.Name} possui múltiplos construtores. " +
                            "Use [MapperConstructor] para indicar qual deve ser usado.");
    }
}