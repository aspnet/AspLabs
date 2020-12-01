using System;
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
    public class ReturnResponseMessageResultCodeFix : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticDescriptors.UseResponseMessageResultId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Return ResponseMessageResult",
                    createChangedDocument: (cts) => UpdateMethod(context),
                    equivalenceKey: "Return ResponseMessageResult"),
                diagnostic);
        }

        private async Task<Document> UpdateMethod(CodeFixContext context)
        {
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var methodDeclaration = root.FindToken(context.Span.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();

            var method = semanticModel.GetDeclaredSymbol(methodDeclaration);
            var updatedDeclaration = methodDeclaration;
            var returnTypeSyntax = methodDeclaration.ReturnType;

            var currentReturnStatement = updatedDeclaration.FindToken(context.Span.Start).Parent.AncestorsAndSelf().OfType<ReturnStatementSyntax>().First();

            var memberAccess = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.ThisExpression(),
                SyntaxFactory.IdentifierName("ResponseMessage"))
                .WithAdditionalAnnotations(Simplifier.Annotation);

            var invocationSyntax = SyntaxFactory.InvocationExpression(memberAccess,
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(new[] { SyntaxFactory.Argument(currentReturnStatement.Expression) })));

            var updatedReturnStatement = currentReturnStatement.WithExpression(invocationSyntax);
            updatedDeclaration = updatedDeclaration.ReplaceNode(currentReturnStatement, updatedReturnStatement);

            if (method.ReturnType.Name.Equals("HttpResponseMessage", StringComparison.Ordinal))
            {
                updatedDeclaration = updatedDeclaration.WithReturnType(
                    SyntaxFactory.IdentifierName(SymbolNames.System_Web_Http_IHttpActionResult).WithTriviaFrom(returnTypeSyntax));
            }
            else
            {
                updatedDeclaration = updatedDeclaration.WithReturnType(SyntaxFactory.GenericName(
                    SyntaxFactory.Identifier("System.Threading.Tasks.Task"),
                    SyntaxFactory.TypeArgumentList(
                        SyntaxFactory.SeparatedList<TypeSyntax>(new[] { SyntaxFactory.IdentifierName(SymbolNames.System_Web_Http_IHttpActionResult) })))
                    .WithTriviaFrom(returnTypeSyntax))
                    .WithAdditionalAnnotations(Simplifier.Annotation);
            }

            return context.Document.WithSyntaxRoot(root.ReplaceNode(methodDeclaration, updatedDeclaration));
       }
    }
}
