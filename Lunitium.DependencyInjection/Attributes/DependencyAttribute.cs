using Lunitium.DependencyInjection.Enums;

namespace Lunitium.DependencyInjection.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class DependencyAttribute<TInterface> : DependencyAttribute
{
    public DependencyAttribute(LifeTime lifeTime = LifeTime.Scoped) : base(lifeTime)
    {
        Type = typeof(TInterface);
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class DependencyAttribute(LifeTime lifeTime = LifeTime.Scoped) : Attribute
{
    public LifeTime LifeTime { get; } = lifeTime;
    public Type? Type { get; set; }
}