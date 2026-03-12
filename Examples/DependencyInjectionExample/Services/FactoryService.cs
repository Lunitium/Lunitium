using Lunitium.DependencyInjection.Attributes;

namespace DependencyInjectionExample.Services;

[Dependency]
public class FactoryService(DateTimeOffset date)
{
    public DateTimeOffset Date { get; } = date;

    [DependencyFactory]
    public static FactoryService Factory()
    {
        return new FactoryService(DateTimeOffset.Now);
    }
}