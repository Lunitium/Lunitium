namespace Lunitium.Mapper.Generator.Models;

/// <summary>
/// Mapper info
/// </summary>
public class MapperInfos
{
    public string Namespace { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public List<MapperInfo> Infos { get; set; } = [];
}