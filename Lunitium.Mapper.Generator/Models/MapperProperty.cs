using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Lunitium.Mapper.Generator.Models;

public class MapperProperty
{
    public IPropertySymbol? ModelProp { get; set; }
    public IPropertySymbol? DtoProp { get; set; }

    public Conversion? ModelToDto { get; set; }
    public Conversion? DtoToModel { get; set; }

    private MapperProperty(GeneratorAttributeSyntaxContext context, IPropertySymbol? modelProp, IPropertySymbol? dtoProp)
    {
        ModelProp = modelProp;
        DtoProp = dtoProp;

        if (modelProp is null || dtoProp is null)
            return;

        ModelToDto = context.SemanticModel.Compilation.ClassifyConversion(modelProp.Type, dtoProp.Type);
        DtoToModel = context.SemanticModel.Compilation.ClassifyConversion(dtoProp.Type, dtoProp.Type);
    }

    public static Dictionary<string, MapperProperty> Mapping(GeneratorAttributeSyntaxContext context,
        List<IPropertySymbol> modelProps, List<IPropertySymbol> dtoProps)
    {
        return modelProps.Concat(dtoProps)
            .Select(x => x.Name)
            .Distinct()
            .ToDictionary(x => x, x =>
            {
                var modelProp = modelProps.FirstOrDefault(p => p.Name == x);
                var dtoProp = dtoProps.FirstOrDefault(p => p.Name == x);

                return new MapperProperty(context, modelProp, dtoProp);
            });
    }
}