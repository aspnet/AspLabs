using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.AspNetCore.Migration
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class HttpMethodConstraintAnalyzer : DiagnosticAnalyzer
    {
        private static readonly string[] SupportedHttpMethodConventions = new[]
        {
            "Get",
            "Put",
            "Post",
            "Delete",
            "Patch",
            "Head",
            "Options",
        };

        private static DiagnosticDescriptor Rule => DiagnosticDescriptors.ActionsShouldExplicitlySpecifyHttpMethodConstraint;

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

                compilationStartAction.RegisterOperationBlockAction(operationBlockContext =>
                {
                    if (operationBlockContext.OwningSymbol is not IMethodSymbol method ||
                        method.ContainingType is not INamedTypeSymbol type ||
                        !WebApiFacts.IsWebApiController(type, symbolCache.IHttpControllerAttribute) ||
                        !WebApiFacts.IsWebApiAction(method, symbolCache.NonActionAttribute))
                    {
                        return;
                    }

                    if (method.HasAttribute(symbolCache.IActionHttpMethodProvider))
                    {
                        // Method has already been annotated. We're cool.
                        return;
                    }

                    var verb = SupportedHttpMethodConventions.FirstOrDefault(v => method.Name.StartsWith(v, StringComparison.OrdinalIgnoreCase));
                    // If no verb is inferred, POST is implied.  Either way, we need to tell the user to annotate it to get the correct Mvc behavior.
                    verb ??= "POST";

                    var properties = ImmutableDictionary.Create<string, string>()
                        .Add("HttpMethodAttribute", $"Http{verb}Attribute");

                    operationBlockContext.ReportDiagnostic(Diagnostic.Create(
                        Rule,
                        method.Locations.First(),
                        properties,
                        $"Annotate method with HTTP{verb}Attribute."));
                });
            });
        }

        internal record SymbolCache
        (
            INamedTypeSymbol IHttpControllerAttribute,
            INamedTypeSymbol NonActionAttribute,
            INamedTypeSymbol IActionHttpMethodProvider
        )
        {
            public static bool TryCreate(Compilation compilation, [NotNullWhen(true)] out SymbolCache? symbolCache)
            {
                symbolCache = default;

                if (!TryGetType(SymbolNames.System_Web_Http_ApiController, out var httpControllerAttribute))
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

                symbolCache = new SymbolCache(
                    httpControllerAttribute,
                    nonActionAttribute,
                    iActionHttpMethodProvider);

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
