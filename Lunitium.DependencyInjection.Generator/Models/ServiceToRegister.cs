using Lunitium.DependencyInjection.Generator.Enums;

namespace Lunitium.DependencyInjection.Generator.Models;

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

    /// <summary>
    /// Keyed service name
    /// </summary>
    public string? KeyLiteral { get; set; }

    /// <summary>
    /// Factory data
    /// </summary>
    public ServiceFactoryToRegister? Factory { get; set; }
}