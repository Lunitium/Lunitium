using Lunitium.DependencyInjection.Enums;

namespace Lunitium.DependencyInjection;

public class ServiceToRegister
{
    public string Name { get; set; } = string.Empty;
    public LifeTime LifeTime { get; set; }
    public string? InterfaceName { get; set; }
}