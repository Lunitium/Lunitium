using Lunitium.DependencyInjection.Attributes;
using Lunitium.DependencyInjection.Enums;
using Lunitium.DependencyInjection.Test.Services.Interfaces;

namespace Lunitium.DependencyInjection.Test.Services;

[Dependency<ITransientService>(LifeTime.Transient)]
public class TransientService : ITransientService
{
    
    
}