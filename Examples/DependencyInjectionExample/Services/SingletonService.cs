using Lunitium.DependencyInjection.Attributes;
using Lunitium.DependencyInjection.Enums;

namespace DependencyInjectionExample.Services;

[Dependency(LifeTime.Singleton)]
public class SingletonService
{
    public int Counter => field++;
}