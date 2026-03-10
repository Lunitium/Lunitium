using Lunitium.DependencyInjection.Attributes;
using Lunitium.DependencyInjection.Enums;
using Lunitium.DependencyInjection.Test.Services.Interfaces;

namespace Lunitium.DependencyInjection.Test.Services;

[Dependency<ITransientService>(LifeTime.Transient)]
public class TransientService : ITransientService
{
    private int _number = 0;
    
    public void SetNumber(int number)
    {
        _number = number;
    }
    
    public int GetNumber()
    {
        return _number;
    }
}