using System.Text;
using Lunitium.Mapper.Generator.Analysis;
using Lunitium.Mapper.Generator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lunitium.Mapper.Generator.Templates;

public class MapperToTemplate(SourceProductionContext context, MapperInfo mapper)
{
    public string Render()
    {
        var sb = new StringBuilder(2048);

        sb.AppendLine($"    public {mapper.TargetSymbol.ToDisplayString()} To{mapper.TargetSymbol.Name}()");
        sb.AppendLine("    {");
        sb.AppendLine($"        return ({mapper.TargetSymbol.ToDisplayString()})this;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine(
            $"    public static implicit operator {mapper.TargetSymbol.ToDisplayString()}({mapper.ModelSymbol.Name} value)");
        sb.AppendLine("    {");
        sb.Append($"        return new {mapper.TargetSymbol.ToDisplayString()}({RenderTargetConstructor()})");

        var renderTargetParameters = RenderTargetParameters();

        if (renderTargetParameters is not null)
        {
            sb.Append(renderTargetParameters);
        }

        sb.AppendLine(";");
        sb.AppendLine("    }");

        return sb.ToString();
    }

    private string RenderTargetConstructor()
    {
        if (mapper.TargetConstructorParameters is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                AnalysisError.MultipleConstructors,
                mapper.AttributeSymbol.GetLocation(),
                mapper.TargetSymbol.Name
            ));
            return string.Empty;
        }

        var parameters = new List<string>();

        foreach (var ctorProp in mapper.TargetConstructorParameters)
        {
            if (ctorProp is null)
                continue;
            
            var prop = mapper.Props.FirstOrDefault(x =>
                x.Key.Equals(ctorProp.Name, StringComparison.OrdinalIgnoreCase));
            var symbol = prop.Value.ModelProp;

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
                mapper.ModelSymbol.Name,
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
                mapper.TargetConstructorParameters is null ||
                !mapper.TargetConstructorParameters.Any(p => p.Name.Equals(x.Key, StringComparison.OrdinalIgnoreCase))
            );

        foreach (var prop in props)
        {
            if (prop.Value.TargetProp is null)
                continue;

            if (prop.Value.ModelProp is null && prop.Value.TargetProp.IsRequired)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    AnalysisError.RequiredProperty,
                    mapper.AttributeSymbol.GetLocation(),
                    prop.Value.TargetProp.Name
                ));
                continue;
            }
            
            if (prop.Value.ModelProp is null)
                continue;

            parameters.Add($"{prop.Value.TargetProp.Name} = value.{prop.Value.ModelProp.Name}");
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

        if (propMapping.Value.ModelProp == null)
        {
            return null;
        }

        var conversion = propMapping.Value.ModelToTarget;

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

        if (propMapping.Value.ModelProp.Type.NullableAnnotation == NullableAnnotation.NotAnnotated &&
            targetType.NullableAnnotation == NullableAnnotation.Annotated)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                AnalysisError.NullableMismatch,
                mapper.AttributeSymbol.GetLocation(),
                propMapping.Value.ModelProp.Name,
                targetType.ToDisplayString()
            ));
        }

        return conversion is { IsExplicit: true } ? $"({targetType.ToDisplayString()})" : string.Empty;
    }
}