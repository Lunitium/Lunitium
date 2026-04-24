using System.Text;
using Lunitium.Mapper.Generator.Analysis;
using Lunitium.Mapper.Generator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lunitium.Mapper.Generator.Templates;

public class MapperFromTemplate(SourceProductionContext context, MapperInfo mapper)
{
    public string Render()
    {
        var sb = new StringBuilder(2048);

        sb.AppendLine($"    public static {mapper.ModelSymbol.Name} From{mapper.TargetSymbol.Name}({mapper.TargetSymbol.ToDisplayString()} value)");
        sb.AppendLine("    {");
        sb.AppendLine($"        return ({mapper.ModelSymbol.Name})value;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine($"    public static implicit operator {mapper.ModelSymbol.Name}({mapper.TargetSymbol.ToDisplayString()} value)");
        sb.AppendLine("    {");
        sb.AppendLine($"        return new {mapper.ModelSymbol.Name}(");
        sb.AppendLine($"            {RenderDtoCtor()}");
        sb.AppendLine("        );");
        sb.AppendLine("    }");

        return sb.ToString();
    }

    private string RenderDtoCtor()
    {
        if (mapper.ModelConstructorParameters is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                AnalysisError.MultipleConstructors,
                mapper.AttributeSymbol.GetLocation(),
                mapper.ModelSymbol.Name
            ));
            return string.Empty;
        }

        var parameters = new List<string>();

        foreach (var ctorProp in mapper.ModelConstructorParameters)
        {
            var prop = mapper.Props.FirstOrDefault(x =>
                x.Key.Equals(ctorProp.Name, StringComparison.OrdinalIgnoreCase));
            var symbol = prop.Value.TargetProp;

            if (symbol != null)
            {
                var conversion = prop.Value.TargetToModel;

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
                mapper.TargetSymbol.Name,
                ctorProp.Name
            ));
        }

        return string.Join(", ", parameters);
    }
}