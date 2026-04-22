using Lunitium.Mapper.Generator.Enums;
using Microsoft.CodeAnalysis;

namespace Lunitium.Mapper.Generator.Models;

/// <summary>
/// Mapper info
/// </summary>
public class MapperToRegister
{
    public string Namespace { get; set; } = string.Empty;

    public SyntaxNode AttributeSymbol { get; set; } = null!;

    public INamedTypeSymbol ModelSymbol { get; set; } = null!;

    public INamedTypeSymbol DtoSymbol { get; set; } = null!;

    public IDictionary<string, MapperProperty> Props { get; set; } = null!;

    public MapDirection MapDirection { get; set; }
}