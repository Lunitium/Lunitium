namespace Lunitium.Mapper.Models;

/// <summary>
/// Mapper info
/// </summary>
public class MapperToRegister
{
    /// <summary>
    /// Model full name
    /// </summary>
    public string ModelName { get; set; } = string.Empty;
    
    /// <summary>
    /// Dto interface full name
    /// </summary>
    public string DtoName { get; set; } = string.Empty;
}