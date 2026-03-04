using Lunitium.DependencyInjection.Test.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lunitium.DependencyInjection.Test;

public class ScopedServiceTest
{
    private readonly IServiceProvider _serviceProvider;
    
    public ScopedServiceTest()
    {
        var builder = new ServiceCollection();
        builder.AddLunitiumDependencies();
        _serviceProvider = builder.BuildServiceProvider();
    }
    
    [Fact]
    public void ScopedServiceExists()
    {
        using var scope = _serviceProvider.CreateScope();
        var registeredService = scope.ServiceProvider.GetService<RegisteredService>();
        
        Assert.NotNull(registeredService);
    }
    
    [Fact]
    public void ScopedServiceNotExists()
    {
        using var scope = _serviceProvider.CreateScope();
        var notRegisteredService = scope.ServiceProvider.GetService<NotRegisteredService>();
        
        Assert.Null(notRegisteredService);
    }
}