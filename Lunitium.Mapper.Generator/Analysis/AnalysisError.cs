using Microsoft.CodeAnalysis;

namespace Lunitium.Mapper.Generator.Analysis;

public static class AnalysisError
{
    private const string DtoCategory = "Dto";

    public static readonly DiagnosticDescriptor ModelDontHaveThisProp = new(
        id: "LUNIMP001",
        title: "Model don't have this property",
        messageFormat: "{0} doesn't have {1} property",
        category: DtoCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor DontMatchType = new(
        id: "LUNIMP002",
        title: "Don't match type",
        messageFormat: "{0} doesn't match type {1}",
        category: DtoCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor NullableMismatch = new(
        id: "LUNIMP003",
        title: "Nullable type mismatch",
        messageFormat: "{0} doesn't nullable `{1}?`",
        category: DtoCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );
}