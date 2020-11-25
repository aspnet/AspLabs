using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.AspNetCore.Migration
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ReturnResponseMessageResultAnalyzer : DiagnosticAnalyzer
    {
        private static DiagnosticDescriptor Rule => DiagnosticDescriptors.UseResponseMessageResult;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            SymbolCache? symbolCache = null;

            context.RegisterOperationBlockStartAction(operationBlockContext =>
            {
                if (symbolCache is null)
                {
                    if (!SymbolCache.TryCreate(operationBlockContext.Compilation, out var createdSymbolCache))
                    {
                        // No-op if we can't find types we care about.
                        return;
                    }

                    symbolCache = createdSymbolCache;
                }

                if (operationBlockContext.OwningSymbol is not IMethodSymbol method ||
                    method.ContainingType is not INamedTypeSymbol type ||
                    !WebApiFacts.IsWebApiController(type, symbolCache.IHttpControllerAttribute) ||
                    !WebApiFacts.IsWebApiAction(method, symbolCache.NonActionAttribute))
                {
                    return;
                }

                var returnType = method.ReturnType;

                if (!SymbolEqualityComparer.Default.Equals(returnType, symbolCache.HttpResponseMessage) &&
                    !(SymbolEqualityComparer.Default.Equals(returnType.OriginalDefinition, symbolCache.TaskOfT) &&
                    returnType is INamedTypeSymbol { TypeParameters: { Length: > 0 } } namedType &&
                    SymbolEqualityComparer.Default.Equals(namedType.TypeArguments[0], symbolCache.HttpResponseMessage)))
                {
                    // Method does not return HttpResponseMessage
                    return;
                }

                operationBlockContext.RegisterOperationAction(operationContext =>
                {
                    operationContext.ReportDiagnostic(Diagnostic.Create(
                        Rule,
                        operationContext.Operation.Syntax.GetLocation()));

                }, OperationKind.Return);
            });
        }

        internal record SymbolCache
        (
            INamedTypeSymbol IHttpControllerAttribute,
            INamedTypeSymbol NonActionAttribute,
            INamedTypeSymbol HttpResponseMessage,
            INamedTypeSymbol TaskOfT
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

                if (!TryGetType(SymbolNames.System_Neb_Http_HttpResponseMessage, out var httpResponseMessage))
                {
                    return false;
                }

                if (!TryGetType(SymbolNames.System_Threading_Tasks_TaskOfT, out var task))
                {
                    return false;
                }

                symbolCache = new SymbolCache(
                    httpControllerAttribute,
                    nonActionAttribute,
                    httpResponseMessage,
                    task);

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
