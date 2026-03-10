using Lunitium.DependencyInjection.Enums;

namespace Lunitium.DependencyInjection.Attributes;

/// <summary>
/// Register service in dependency injection
/// </summary>
/// <typeparam name="TInterface">Interface type</typeparam>
[AttributeUsage(AttributeTargets.Class)]
public class DependencyAttribute<TInterface> : DependencyAttribute
{
    /// <summary>
    /// Register service in dependency injection
    /// </summary>
    /// <param name="lifeTime">Service lifetime</param>
    public DependencyAttribute(LifeTime lifeTime = LifeTime.Scoped) : base(lifeTime)
    {
        Type = typeof(TInterface);
    }
}

/// <summary>
/// Register service in dependency injection
/// </summary>
/// <param name="lifeTime">Service lifetime</param>
[AttributeUsage(AttributeTargets.Class)]
public class DependencyAttribute(LifeTime lifeTime = LifeTime.Scoped) : Attribute
{
    /// <summary>
    /// Service lifetime
    /// </summary>
    public LifeTime LifeTime { get; } = lifeTime;
    
    /// <summary>
    /// Interface type reference
    /// </summary>
    public Type? Type { get; protected set; }

    /// <summary>
    /// Keyed service name
    /// </summary>
    public object? Key { get; set; }
}