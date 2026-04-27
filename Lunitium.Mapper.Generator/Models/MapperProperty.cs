using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Lunitium.Mapper.Generator.Models;

public class MapperProperty
{
    public IPropertySymbol? ModelProp { get; set; }
    public IPropertySymbol? TargetProp { get; set; }

    public Conversion? ModelToTarget { get; set; }
    public Conversion? TargetToModel { get; set; }

    private MapperProperty(GeneratorAttributeSyntaxContext context, IPropertySymbol? modelProp, IPropertySymbol? targetProp)
    {
        ModelProp = modelProp;
        TargetProp = targetProp;

        if (modelProp is null || targetProp is null)
            return;

        ModelToTarget = context.SemanticModel.Compilation.ClassifyConversion(modelProp.Type, targetProp.Type);
        TargetToModel = context.SemanticModel.Compilation.ClassifyConversion(targetProp.Type, targetProp.Type);
    }

    public static Dictionary<string, MapperProperty> Mapping(GeneratorAttributeSyntaxContext context,
        List<IPropertySymbol> modelProps, List<IPropertySymbol> targetProps)
    {
        return modelProps.Concat(targetProps)
            .Select(x => x.Name)
            .Distinct()
            .ToDictionary(x => x, x =>
            {
                var modelProp = modelProps.FirstOrDefault(p => p.Name.Equals(x, StringComparison.OrdinalIgnoreCase));
                var targetProp = targetProps.FirstOrDefault(p => p.Name.Equals(x, StringComparison.OrdinalIgnoreCase));

                return new MapperProperty(context, modelProp, targetProp);
            });
    }
}