using Lunitium.DependencyInjection.Test.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lunitium.DependencyInjection.Test;

public class KeyedServiceTest
{
    private readonly IServiceProvider _serviceProvider;
    
    public KeyedServiceTest()
    {
        var builder = new ServiceCollection();
        builder.AddLunitiumDependencies();
        _serviceProvider = builder.BuildServiceProvider();
    }

    [Fact]
    public void KeyedServiceExists()
    {
        var service = _serviceProvider.GetKeyedService<NumberKeyedService>(10);
        
        Assert.NotNull(service);
    }

    [Fact]
    public void KeyedServiceNotExists()
    {
        var service = _serviceProvider.GetKeyedService<TransientService>(10);
        
        Assert.Null(service);
    }
}