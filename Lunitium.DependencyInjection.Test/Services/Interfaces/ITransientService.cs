namespace Lunitium.DependencyInjection.Test.Services.Interfaces;

public interface ITransientService
{
    void SetNumber(int number);
    int GetNumber();
}