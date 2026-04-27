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

        sb.AppendLine(
            $"    public static {mapper.ModelSymbol.Name} From{mapper.TargetSymbol.Name}({mapper.TargetSymbol.ToDisplayString()} value)");
        sb.AppendLine("    {");
        sb.AppendLine($"        return ({mapper.ModelSymbol.Name})value;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine(
            $"    public static implicit operator {mapper.ModelSymbol.Name}({mapper.TargetSymbol.ToDisplayString()} value)");
        sb.AppendLine("    {");
        sb.Append($"        return new {mapper.ModelSymbol.Name}({RenderDtoCtor()})");

        var renderTargetParameters = RenderTargetParameters();

        if (renderTargetParameters is not null)
        {
            sb.Append(renderTargetParameters);
        }

        sb.AppendLine(";");
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
                var cast = GetConversionCast(ctorProp);

                if (cast is not null)
                {
                    parameters.Add($"{cast}value.{symbol.Name}");
                }

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

        if (parameters.Count == 0)
            return string.Empty;

        return "\n            " + string.Join(",\n            ", parameters) + "\n        ";
    }

    private string? RenderTargetParameters()
    {
        var parameters = new List<string>();

        var props = mapper.Props
            .Where(x =>
                mapper.ModelConstructorParameters is null ||
                !mapper.ModelConstructorParameters.Any(p => p.Name.Equals(x.Key, StringComparison.OrdinalIgnoreCase))
            );

        foreach (var prop in props)
        {
            if (prop.Value.ModelProp is null)
                continue;

            if (prop.Value.TargetProp is null && prop.Value.ModelProp.IsRequired)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    AnalysisError.RequiredProperty,
                    mapper.AttributeSymbol.GetLocation(),
                    prop.Value.ModelProp.Name
                ));
                continue;
            }

            if (prop.Value.TargetProp is null)
                continue;

            var cast = GetConversionCast(prop.Value.ModelProp);

            if (cast is not null)
            {
                parameters.Add($"{prop.Value.ModelProp.Name} = {cast}value.{prop.Value.TargetProp.Name}");
            }
        }

        if (parameters.Count == 0)
            return null;

        return "\n        {\n            " + string.Join(",\n            ", parameters) + "\n        }";
    }

    private string? GetConversionCast(ISymbol targetSymbol)
    {
        var targetType = targetSymbol switch
        {
            IParameterSymbol param => param.Type,
            IPropertySymbol prop => prop.Type,
            _ => null
        };

        if (targetType == null)
            return null;

        var propMapping = mapper.Props.FirstOrDefault(x =>
            x.Key.Equals(targetSymbol.Name, StringComparison.OrdinalIgnoreCase));

        if (propMapping.Value.TargetProp == null)
        {
            return null;
        }

        var conversion = propMapping.Value.TargetToModel;

        if (conversion is { Exists: false })
        {
            context.ReportDiagnostic(Diagnostic.Create(
                AnalysisError.DontMatchType,
                mapper.AttributeSymbol.GetLocation(),
                targetSymbol.Name,
                targetType.ToDisplayString()
            ));
            return null;
        }

        if (propMapping.Value.TargetProp.Type.NullableAnnotation == NullableAnnotation.Annotated &&
            targetType.NullableAnnotation == NullableAnnotation.NotAnnotated)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                AnalysisError.NullableMismatch,
                mapper.AttributeSymbol.GetLocation(),
                propMapping.Value.TargetProp.Name,
                targetType.ToDisplayString()
            ));
        }

        return targetType.Equals(propMapping.Value.TargetProp.Type, SymbolEqualityComparer.IncludeNullability)
            ? string.Empty
            : $"({targetType.ToDisplayString()})";
    }
}