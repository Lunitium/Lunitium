using Lunitium.DependencyInjection.Enums;

namespace Lunitium.DependencyInjection;

/// <summary>
/// Service info
/// </summary>
internal class ServiceToRegister
{
    /// <summary>
    /// Service full name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Lifetime of service
    /// </summary>
    public LifeTime LifeTime { get; set; }
    
    /// <summary>
    /// Reference interface full name
    /// </summary>
    public string? InterfaceName { get; set; }
}