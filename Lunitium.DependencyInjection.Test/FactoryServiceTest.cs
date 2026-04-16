using Lunitium.DependencyInjection.Test.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Lunitium.DependencyInjection.Test;

public class FactoryServiceTest
{
    private readonly IServiceProvider _serviceProvider;

    public FactoryServiceTest()
    {
        var builder = new ServiceCollection();
        builder.AddLunitiumDependencies();
        _serviceProvider = builder.BuildServiceProvider();
    }

    [Fact]
    public void FactoryServiceExists()
    {
        var service = _serviceProvider.GetKeyedService<IFactoryService>("test");

        Assert.NotNull(service);
        Assert.Equal(service.Key, "test");
    }
}
