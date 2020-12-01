using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.AspNetCore.Migration
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ControllerAnalyzer)), Shared]
    public class ActionOverloadSelectionCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticDescriptors.ActionsRelyOnOverloadResolutionForRoutingId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();

            var metadata = context.Diagnostics.First(f => f.Id == DiagnosticDescriptors.ActionsRelyOnOverloadResolutionForRoutingId).Properties;

            var addControllerRoute = metadata.ContainsKey("AddControllerRoute");

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: $"Apply attribute routing",
                    createChangedDocument: (cts) => AddMethodConstraint(context.Document, semanticModel, diagnostic, root, addControllerRoute, cts),
                    equivalenceKey: "Use route"),
                diagnostic);
        }

        private async Task<Document> AddMethodConstraint(Document document, SemanticModel semanticModel, Diagnostic diagnostic, SyntaxNode root, bool addControllerRoute, CancellationToken cancellationToken)
        {
            var documentEditor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

            foreach (var item in diagnostic.AdditionalLocations)
            {
                AddRouteAttribute(semanticModel, root, documentEditor, item.SourceSpan, cancellationToken);
            }

            return documentEditor.GetChangedDocument();
        }

        private static void AddRouteAttribute(SemanticModel semanticModel, SyntaxNode root, DocumentEditor documentEditor, TextSpan diagnosticSpan, CancellationToken cancellationToken)
        {
            var methodDeclaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();

            var method = semanticModel.GetDeclaredSymbol(methodDeclaration, cancellationToken);
            string? route = null;
            foreach (var parameter in method.Parameters)
            {
                // Simple types participate in overload selection. We'll assume they all come from route and use it to produce a route for this action.
                // https://github.com/aspnet/AspNetWebStack/blob/749384689e027a2fcd29eb79a9137b94cea611a8/src/System.Web.Http/Internal/TypeHelper.cs#L63-L72
                if (parameter.Type.IsValueType ||
                    SymbolEqualityComparer.Default.Equals(parameter.Type, semanticModel.Compilation.GetSpecialType(SpecialType.System_String)) ||
                    SymbolEqualityComparer.Default.Equals(parameter.Type, semanticModel.Compilation.GetSpecialType(SpecialType.System_DateTime)) ||
                    SymbolEqualityComparer.Default.Equals(parameter.Type, semanticModel.Compilation.GetSpecialType(SpecialType.System_Decimal)))
                {
                    route += $"/{{{parameter.Name}}}";
                }
            }

            if (route is null)
            {
                return;
            }

            route = "[controller]/[action]" + route;

            var actionAttribute = SyntaxFactory.Attribute(
                    SyntaxFactory.ParseName("Microsoft.AspNetCore.Mvc.Route").WithAdditionalAnnotations(Simplifier.Annotation))
                    .AddArgumentListArguments(
                        SyntaxFactory.AttributeArgument(
                            SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(route))));

            documentEditor.AddAttribute(methodDeclaration, actionAttribute);
        }
    }
}
