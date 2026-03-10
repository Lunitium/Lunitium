using Lunitium.DependencyInjection.Attributes;
using Lunitium.DependencyInjection.Enums;
using Lunitium.DependencyInjection.Test.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Lunitium.DependencyInjection.Test.Services;

[Dependency<IFactoryService>(LifeTime.Singleton, Key = "test")]
public class FactoryService(ITransientService service, object? key) : IFactoryService
{
    public object? Key { get; } = key;

    [DependencyFactory]
    public static FactoryService Factory(IServiceProvider serviceProvider, object? key)
    {
        var transientService = serviceProvider.GetRequiredService<ITransientService>();

        return new FactoryService(transientService, key);
    }
}