using Lunitium.Mapper.Generator.Enums;
using Microsoft.CodeAnalysis;

namespace Lunitium.Mapper.Generator.Models;

public class MapperInfo
{
    public SyntaxNode AttributeSymbol { get; set; } = null!;

    public INamedTypeSymbol ModelSymbol { get; set; } = null!;

    public INamedTypeSymbol TargetSymbol { get; set; } = null!;

    public IDictionary<string, MapperProperty> Props { get; set; } = null!;
    
    public List<IParameterSymbol>? ModelConstructorParameters { get; set; }
    
    public List<IParameterSymbol>? TargetConstructorParameters { get; set; }

    public MapAction MapAction { get; set; }
}