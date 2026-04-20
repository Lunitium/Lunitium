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
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );
}