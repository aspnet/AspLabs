using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.AspNetCore.Migration
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ControllerAnalyzer : DiagnosticAnalyzer
    {
        private static DiagnosticDescriptor Rule => DiagnosticDescriptors.ControllersShouldUseApiController;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            SymbolCache? symbolCache = null;

            context.RegisterCompilationStartAction(compilationStartAction =>
            {
                if (symbolCache is null)
                {
                    if (!SymbolCache.TryCreate(compilationStartAction.Compilation, out var createdSymbolCache))
                    {
                        // No-op if we can't find types we care about.
                        return;
                    }

                    symbolCache = createdSymbolCache;
                }

                compilationStartAction.RegisterSymbolStartAction(symbolStartAnalysisContext =>
                {
                    symbolStartAnalysisContext.RegisterSymbolEndAction(symbolEndAnalysisContext =>
                    {
                        AnalyzeType(symbolEndAnalysisContext, symbolCache!);
                    });
                }, SymbolKind.NamedType);
            });
        }

        private static void AnalyzeType(SymbolAnalysisContext context, SymbolCache symbols)
        {
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

            if (!WebApiFacts.IsWebApiController(namedTypeSymbol, symbols.IHttpControllerAttribute))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, namedTypeSymbol.Locations.FirstOrDefault()));
        }

        internal sealed class SymbolCache
        {
            public SymbolCache(
                INamedTypeSymbol iHttpControllerAttribute)
            {
                IHttpControllerAttribute = iHttpControllerAttribute;
            }

            public static bool TryCreate(Compilation compilation, out SymbolCache? symbolCache)
            {
                symbolCache = default;

                if (!TryGetType(SymbolNames.System_Web_Http_ApiController, out var httpControllerAttribute))
                {
                    return false;
                }

                symbolCache = new SymbolCache(
                    httpControllerAttribute);

                return true;

                bool TryGetType(string typeName, out INamedTypeSymbol typeSymbol)
                {
                    typeSymbol = compilation.GetTypeByMetadataName(typeName);
                    return typeSymbol != null && typeSymbol.TypeKind != TypeKind.Error;
                }
            }

            public INamedTypeSymbol IHttpControllerAttribute { get; }
        }
    }
}
