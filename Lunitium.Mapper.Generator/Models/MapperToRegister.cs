using Lunitium.Mapper.Generator.Enums;
using Microsoft.CodeAnalysis;

namespace Lunitium.Mapper.Generator.Models;

/// <summary>
/// Mapper info
/// </summary>
public class MapperToRegister
{
    public string Namespace { get; set; } = string.Empty;

    public INamedTypeSymbol ModelSymbol { get; set; } = null!;

    public INamedTypeSymbol DtoSymbol { get; set; } = null!;

    public List<IPropertySymbol> ModelProps { get; set; } = null!;

    public List<IPropertySymbol> DtoProps { get; set; } = null!;
    
    public MapDirection MapDirection { get; set; }
    
    public IEnumerable<Diagnostic> Errors { get; set; } = [];
}