using Microsoft.CodeAnalysis;

namespace Lunitium.Mapper.Generator.Analysis;

public static class AnalysisError
{
    private const string MapperCategory = "Mapper";

    public static readonly DiagnosticDescriptor ModelDontHaveThisProp = new(
        id: "LUNIMP001",
        title: "Model don't have this property",
        messageFormat: "{0} doesn't have {1} property",
        category: MapperCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor DontMatchType = new(
        id: "LUNIMP002",
        title: "Don't match type",
        messageFormat: "{0} doesn't match type {1}",
        category: MapperCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor NullableMismatch = new(
        id: "LUNIMP003",
        title: "Nullable type mismatch",
        messageFormat: "{0} isn't nullable `{1}?`",
        category: MapperCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor MultipleConstructors = new(
        id: "LUNIMP004",
        title: "Multiple constructors",
        messageFormat: "{0} have many constructors",
        category: MapperCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor RequiredProperty = new(
        id: "LUNIMP005",
        title: "Required property",
        messageFormat: "{0} is a required property",
        category: MapperCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
}