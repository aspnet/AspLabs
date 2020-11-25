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

namespace Microsoft.AspNetCore.Migration
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ControllerAnalyzer)), Shared]
    public class HttpMethodConstraintCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticDescriptors.ActionsShouldExplicitlySpecifyHttpMethodConstraintId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();

            var httpMethodAttribute = context.Diagnostics.First(f => f.Id == DiagnosticDescriptors.ActionsShouldExplicitlySpecifyHttpMethodConstraintId).Properties["HttpMethodAttribute"];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: $"Apply {httpMethodAttribute}",
                    createChangedDocument: (cts) => AddMethodConstraint(context.Document, declaration, httpMethodAttribute, cts),
                    equivalenceKey: "Specify HttpMethod constraint"),
                diagnostic);
        }

        private async Task<Document> AddMethodConstraint(Document document, MethodDeclarationSyntax methodDeclaration, string httpMethodConstraint, CancellationToken cancellationToken)
        {
            var documentEditor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var attribute = SyntaxFactory.Attribute(
                SyntaxFactory.ParseName($"Microsoft.AspNetCore.Mvc.{httpMethodConstraint}")
                    .WithAdditionalAnnotations(Simplifier.Annotation));

            documentEditor.AddAttribute(methodDeclaration, attribute);

            return documentEditor.GetChangedDocument();
        }
    }
}
