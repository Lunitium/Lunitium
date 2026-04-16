using System.Collections.Generic;
using Lunitium.DependencyInjection.Generator.Enums;
using Microsoft.CodeAnalysis;

namespace Lunitium.DependencyInjection.Generator.Models;

internal class ServiceFactoryToRegister
{
    /// <summary>
    /// Service factory method name
    /// </summary>
    public string? FactoryName { get; set; }
        
    /// <summary>
    /// Parameter from factory function
    /// </summary>
    public IEnumerable<FactoryParameter> Parameters { get; set; } = [];

    public IEnumerable<Diagnostic> Errors { get; set; } = [];
}