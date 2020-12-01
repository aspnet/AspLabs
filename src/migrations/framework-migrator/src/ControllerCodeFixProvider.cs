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

namespace Microsoft.AspNetCore.Migration
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ControllerAnalyzer)), Shared]
    public class ControllerCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticDescriptors.ControllersShouldUseApiControllerId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Use ControllerBase instead",
                    createChangedDocument: (cts) => UpdateControllerBase(context.Document, declaration, cts),
                    equivalenceKey: "Use ControllerBase instead"),
                diagnostic);
        }

        private async Task<Document> UpdateControllerBase(Document document, TypeDeclarationSyntax typeDeclaration, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var baseType = typeDeclaration.BaseList.Types.First();

            var newBaseType = "Microsoft.AspNetCore.Mvc.ControllerBase";
            var updated = baseType.WithType(SyntaxFactory.ParseTypeName(newBaseType))
                .WithLeadingTrivia(baseType.GetLeadingTrivia())
                .WithTrailingTrivia(baseType.GetTrailingTrivia());

            var newRoot = root.ReplaceNode(baseType, updated);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
