using Lunitium.DependencyInjection.Test.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Lunitium.DependencyInjection.Test;

public class TransientServiceTest
{
    private readonly IServiceProvider _serviceProvider;
    
    public TransientServiceTest()
    {
        var builder = new ServiceCollection();
        builder.AddLunitiumDependencies();
        _serviceProvider = builder.BuildServiceProvider();
    }

    [Fact]
    public void TestTransientService()
    {
        var scope = _serviceProvider.CreateScope();
        var firstService = scope.ServiceProvider.GetRequiredService<ITransientService>();

        var number = new Random().Next();
        firstService.SetNumber(number);
        
        Assert.Equal(number, firstService.GetNumber());
        
        var secondService = scope.ServiceProvider.GetRequiredService<ITransientService>();
        
        Assert.NotEqual(number, secondService.GetNumber());
    }
}