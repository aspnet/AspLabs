using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.AspNetCore.Migration
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class HttpMethodOverloadSelectionAnalyzer : DiagnosticAnalyzer
    {
        private static DiagnosticDescriptor Rule => DiagnosticDescriptors.ActionsRelyOnOverloadResolutionForRouting;

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

                compilationStartAction.RegisterSymbolAction(symbolAnalysisContext =>
                {
                    if (symbolAnalysisContext.Symbol is not INamedTypeSymbol { TypeKind: TypeKind.Class } type || !WebApiFacts.IsWebApiController(type, symbolCache.ApiController))
                    {
                        return;
                    }

                    var groupedActions = type.GetMembers()
                        .OfType<IMethodSymbol>()
                        .Where(m => WebApiFacts.IsWebApiAction(m, symbolCache.NonActionAttribute) && !HasAttributeRoute(symbolCache, m))
                        .GroupBy(m => m.Name) // Oops, we're not looking at ActionName attribute
                        .Where(g => g.Count() > 1);

                    foreach (var group in groupedActions)
                    {
                        foreach (var item in group)
                        {
                            symbolAnalysisContext.ReportDiagnostic(Diagnostic.Create(
                                Rule,
                                item.Locations.First(),
                                group.Select(m => m.Locations.First())));
                        }
                    }

                    static bool HasAttributeRoute(SymbolCache symbolCache, IMethodSymbol methodSymbol)
                    {
                        var routeAttributes = methodSymbol.GetAttributes(symbolCache.IRouteTemplateProvider);
                        foreach (var attribute in routeAttributes)
                        {
                            var args = attribute.ConstructorArguments;
                            // This is probably a RouteAttribute or a Http*Attribute. Both of these have exactly one ctor that accepts any value viz the literal template.
                            // Therefore if any value is supplied, it's a value for the route template.
                            if (args.Length > 0 && !args[0].IsNull)
                            {
                                return true;
                            }
                        }

                        return false;
                    }
                }, SymbolKind.NamedType);
            });
        }

        internal record SymbolCache
        (
            INamedTypeSymbol ApiController,
            INamedTypeSymbol NonActionAttribute,
            INamedTypeSymbol IActionHttpMethodProvider,
            INamedTypeSymbol IRouteTemplateProvider
        )
        {
            public static bool TryCreate(Compilation compilation, [NotNullWhen(true)] out SymbolCache? symbolCache)
            {
                symbolCache = default;

                if (!TryGetType(SymbolNames.System_Web_Http_ApiController, out var apiController))
                {
                    return false;
                }

                if (!TryGetType(SymbolNames.System_Web_Http_NonActionAttribute, out var nonActionAttribute))
                {
                    return false;
                }

                if (!TryGetType(SymbolNames.Microsoft_AspNetCore_Mvc_Routing_IActionHttpMethodProvider, out var iActionHttpMethodProvider))
                {
                    return false;
                }

                if (!TryGetType(SymbolNames.Microsoft_AspNetCore_Mvc_Routing_IRouteTemplateProvider, out var routeTemplateProvider))
                {
                    return false;
                }

                symbolCache = new SymbolCache(
                    apiController,
                    nonActionAttribute,
                    iActionHttpMethodProvider,
                    routeTemplateProvider);

                return true;

                bool TryGetType(string typeName, out INamedTypeSymbol typeSymbol)
                {
                    typeSymbol = compilation.GetTypeByMetadataName(typeName);
                    return typeSymbol != null && typeSymbol.TypeKind != TypeKind.Error;
                }
            }
        }
    }
}
