using Microsoft.CodeAnalysis;

namespace Lunitium.DependencyInjection.Generator.Analysis;

public static class AnalysisError
{
    private const string FactoryCategory = "Factory";

    public static readonly DiagnosticDescriptor MoreThanOneFactory = new(
        id: "LUNIDI001",
        title: "More than one factory found",
        messageFormat: "{0} found more than one factory",
        category: FactoryCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor NotStaticMethod = new(
        id: "LUNIDI002",
        title: "Not static method",
        messageFormat: "{0} is not a static method",
        category: FactoryCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor ManyParameters = new(
        id: "LUNIDI003",
        title: "Many parameters",
        messageFormat: "{0} found more than {1} parameter",
        category: FactoryCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor ParameterTypeNotAllowed = new(
        id: "LUNIDI004",
        title: "Parameter type not allowed",
        messageFormat: "{0} is not an allowed parameter type",
        category: FactoryCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
}