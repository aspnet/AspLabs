// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.CodeAnalysis;

namespace Microsoft.AspNetCore.OpenApi.SourceGenerators;

public static class XmlDocumentationExtensions
{
    public static string GetDocumentationComment(this ISymbol symbol, Compilation compilation, CultureInfo? preferredCulture = null, bool expandIncludes = false, bool expandInheritdoc = false, CancellationToken cancellationToken = default)
       => GetDocumentationComment(symbol, visitedSymbols: null, compilation, preferredCulture, expandIncludes, expandInheritdoc, cancellationToken);

    private static string GetDocumentationComment(ISymbol symbol, HashSet<ISymbol>? visitedSymbols, Compilation compilation, CultureInfo? preferredCulture, bool expandIncludes, bool expandInheritdoc, CancellationToken cancellationToken)
    {
        var xmlText = symbol.GetDocumentationCommentXml(preferredCulture, expandIncludes, cancellationToken);
        if (expandInheritdoc)
        {
            if (string.IsNullOrEmpty(xmlText))
            {
                if (IsEligibleForAutomaticInheritdoc(symbol))
                {
                    xmlText = $@"<doc><inheritdoc/></doc>";
                }
                else
                {
                    return string.Empty;
                }
            }

            try
            {
                var element = XElement.Parse(xmlText, LoadOptions.PreserveWhitespace);
                element.ReplaceNodes(RewriteMany(symbol, visitedSymbols, compilation, element.Nodes().ToArray(), cancellationToken));
                xmlText = element.ToString(SaveOptions.DisableFormatting);
            }
            catch (XmlException)
            {
                // Malformed documentation comments will produce an exception during parsing. This is not directly
                // actionable, so avoid the overhead of telemetry reporting for it.
                // https://devdiv.visualstudio.com/DevDiv/_workitems/edit/1385578
            }
            catch (Exception)
            {
            }
        }

        return xmlText ?? string.Empty;

        static bool IsEligibleForAutomaticInheritdoc(ISymbol symbol)
        {
            // Only the following symbols are eligible to inherit documentation without an <inheritdoc/> element:
            //
            // * Members that override an inherited member
            // * Members that implement an interface member
            if (symbol.IsOverride)
            {
                return true;
            }

            if (symbol.ContainingType is null)
            {
                // Observed with certain implicit operators, such as operator==(void*, void*).
                return false;
            }

            switch (symbol.Kind)
            {
                case SymbolKind.Method:
                case SymbolKind.Property:
                case SymbolKind.Event:
                    if (symbol.ExplicitOrImplicitInterfaceImplementations().Any())
                    {
                        return true;
                    }

                    break;

                default:
                    break;
            }

            return false;
        }
    }

    private static XNode[] RewriteInheritdocElements(ISymbol symbol, HashSet<ISymbol>? visitedSymbols, Compilation compilation, XNode node, CancellationToken cancellationToken)
    {
        if (node.NodeType == XmlNodeType.Element)
        {
            var element = (XElement)node;
            if (ElementNameIs(element, DocumentationCommentXmlNames.InheritdocElementName))
            {
                var rewritten = RewriteInheritdocElement(symbol, visitedSymbols, compilation, element, cancellationToken);
                if (rewritten is object)
                {
                    return rewritten;
                }
            }
        }

        var container = node as XContainer;
        if (container == null)
        {
            return [Copy(node, copyAttributeAnnotations: false)];
        }

        var oldNodes = container.Nodes();

        // Do this after grabbing the nodes, so we don't see copies of them.
        container = Copy(container, copyAttributeAnnotations: false);

        // WARN: don't use node after this point - use container since it's already been copied.

        if (oldNodes != null)
        {
            var rewritten = RewriteMany(symbol, visitedSymbols, compilation, oldNodes.ToArray(), cancellationToken);
            container.ReplaceNodes(rewritten);
        }

        return [container];
    }

    private static XNode[] RewriteMany(ISymbol symbol, HashSet<ISymbol>? visitedSymbols, Compilation compilation, XNode[] nodes, CancellationToken cancellationToken)
    {
        var result = new List<XNode>();
        foreach (var child in nodes)
        {
            result.AddRange(RewriteInheritdocElements(symbol, visitedSymbols, compilation, child, cancellationToken));
        }

        return [.. result];
    }

    private static XNode[]? RewriteInheritdocElement(ISymbol memberSymbol, HashSet<ISymbol>? visitedSymbols, Compilation compilation, XElement element, CancellationToken cancellationToken)
    {
        var crefAttribute = element.Attribute(XName.Get(DocumentationCommentXmlNames.CrefAttributeName));
        var pathAttribute = element.Attribute(XName.Get(DocumentationCommentXmlNames.PathAttributeName));

        var candidate = GetCandidateSymbol(memberSymbol);
        var hasCandidateCref = candidate is object;

        var hasCrefAttribute = crefAttribute is object;
        var hasPathAttribute = pathAttribute is object;
        if (!hasCrefAttribute && !hasCandidateCref)
        {
            // No cref available
            return null;
        }

        ISymbol? symbol;
        if (crefAttribute is null)
        {
            if (candidate is null)
            {
                throw new InvalidOperationException("Both candidate and cref are null");
            }
            symbol = candidate;
        }
        else
        {
            var crefValue = crefAttribute.Value;
            symbol = DocumentationCommentId.GetFirstSymbolForDeclarationId(crefValue, compilation);
            if (symbol is null)
            {
                return null;
            }
        }

        visitedSymbols ??= [];
        if (!visitedSymbols.Add(symbol))
        {
            // Prevent recursion
            return null;
        }

        try
        {
            var inheritedDocumentation = GetDocumentationComment(symbol, visitedSymbols, compilation, preferredCulture: null, expandIncludes: true, expandInheritdoc: true, cancellationToken);
            if (inheritedDocumentation == string.Empty)
            {
                return [];
            }

            var document = XDocument.Parse(inheritedDocumentation, LoadOptions.PreserveWhitespace);
            string xpathValue;
            if (string.IsNullOrEmpty(pathAttribute?.Value))
            {
                xpathValue = BuildXPathForElement(element.Parent!);
            }
            else
            {
                xpathValue = pathAttribute!.Value;
                if (xpathValue.StartsWith("/", StringComparison.Ordinal))
                {
                    // Account for the root <doc> or <member> element
                    xpathValue = "/*" + xpathValue;
                }
            }

            // Consider the following code, we want Test<int>.Clone to say "Clones a Test<int>" instead of "Clones a int", thus
            // we rewrite `typeparamref`s as cref pointing to the correct type:
            /*
                public class Test<T> : ICloneable<Test<T>>
                {
                    /// <inheritdoc/>
                    public Test<T> Clone() => new();
                }

                /// <summary>A type that has clonable instances.</summary>
                /// <typeparam name="T">The type of instances that can be cloned.</typeparam>
                public interface ICloneable<T>
                {
                    /// <summary>Clones a <typeparamref name="T"/>.</summary>
                    public T Clone();
                }
            */
            // Note: there is no way to cref an instantiated generic type. See https://github.com/dotnet/csharplang/issues/401
            var typeParameterRefs = document.Descendants(DocumentationCommentXmlNames.TypeParameterReferenceElementName).ToImmutableArray();
            foreach (var typeParameterRef in typeParameterRefs)
            {
                if (typeParameterRef.Attribute(DocumentationCommentXmlNames.NameAttributeName) is XAttribute typeParamName)
                {
                    var index = symbol.OriginalDefinition.GetAllTypeParameters().IndexOf(p => p.Name == typeParamName.Value);
                    if (index >= 0)
                    {
                        var typeArgs = symbol.GetAllTypeArguments();
                        if (index < typeArgs.Length)
                        {
                            var docId = typeArgs[index].GetDocumentationCommentId();
                            if (docId != null && !docId.StartsWith("!", StringComparison.Ordinal))
                            {
                                var replacement = new XElement(DocumentationCommentXmlNames.SeeElementName);
                                replacement.SetAttributeValue(DocumentationCommentXmlNames.CrefAttributeName, docId);
                                typeParameterRef.ReplaceWith(replacement);
                            }
                        }
                    }
                }
            }

            var loadedElements = TrySelectNodes(document, xpathValue);
            return loadedElements ?? [];
        }
        catch (XmlException)
        {
            return [];
        }
        finally
        {
            visitedSymbols.Remove(symbol);
        }

        // Local functions
        static ISymbol? GetCandidateSymbol(ISymbol memberSymbol)
        {
            if (memberSymbol.ExplicitInterfaceImplementations().Any())
            {
                return memberSymbol.ExplicitInterfaceImplementations().First();
            }
            else if (memberSymbol.IsOverride)
            {
                return memberSymbol.GetOverriddenMember();
            }

            if (memberSymbol is IMethodSymbol methodSymbol)
            {
                if (methodSymbol.MethodKind is MethodKind.Constructor or MethodKind.StaticConstructor)
                {
                    var baseType = memberSymbol.ContainingType.BaseType;
#nullable disable // Can 'baseType' be null here? https://github.com/dotnet/roslyn/issues/39166
                    return baseType.Constructors.Where(c => IsSameSignature(methodSymbol, c)).FirstOrDefault();
#nullable enable
                }
                else
                {
                    // check for implicit interface
                    return methodSymbol.ExplicitOrImplicitInterfaceImplementations().FirstOrDefault();
                }
            }
            else if (memberSymbol is INamedTypeSymbol typeSymbol)
            {
                if (typeSymbol.TypeKind == TypeKind.Class)
                {
                    // Classes use the base type as the default inheritance candidate. A different target (e.g. an
                    // interface) can be provided via the 'path' attribute.
                    return typeSymbol.BaseType;
                }
                else if (typeSymbol.TypeKind == TypeKind.Interface)
                {
                    return typeSymbol.Interfaces.FirstOrDefault();
                }
                else
                {
                    // This includes structs, enums, and delegates as mentioned in the inheritdoc spec
                    return null;
                }
            }

            return memberSymbol.ExplicitOrImplicitInterfaceImplementations().FirstOrDefault();
        }

        static bool IsSameSignature(IMethodSymbol left, IMethodSymbol right)
        {
            if (left.Parameters.Length != right.Parameters.Length)
            {
                return false;
            }

            if (left.IsStatic != right.IsStatic)
            {
                return false;
            }

            if (!SymbolEqualityComparer.Default.Equals(left.ReturnType, right.ReturnType))
            {
                return false;
            }

            for (var i = 0; i < left.Parameters.Length; i++)
            {
                if (!SymbolEqualityComparer.Default.Equals(left.Parameters[i].Type, right.Parameters[i].Type))
                {
                    return false;
                }
            }

            return true;
        }

        static string BuildXPathForElement(XElement element)
        {
            if (ElementNameIs(element, "member") || ElementNameIs(element, "doc"))
            {
                // Avoid string concatenation allocations for inheritdoc as a top-level element
                return "/*/node()[not(self::overloads)]";
            }

            var path = "/node()[not(self::overloads)]";
            for (var current = element; current != null; current = current.Parent)
            {
                var currentName = current.Name.ToString();
                if (ElementNameIs(current, "member") || ElementNameIs(current, "doc"))
                {
                    // Allow <member> and <doc> to be used interchangeably
                    currentName = "*";
                }

                path = "/" + currentName + path;
            }

            return path;
        }
    }

    private static TNode Copy<TNode>(TNode node, bool copyAttributeAnnotations)
        where TNode : XNode
    {
        XNode copy;

        // Documents can't be added to containers, so our usual copy trick won't work.
        if (node.NodeType == XmlNodeType.Document)
        {
            copy = new XDocument(((XDocument)(object)node));
        }
        else
        {
            XContainer temp = new XElement("temp");
            temp.Add(node);
            copy = temp.LastNode!;
            temp.RemoveNodes();
        }

        Debug.Assert(copy != node);
        Debug.Assert(copy.Parent == null); // Otherwise, when we give it one, it will be copied.

        // Copy annotations, the above doesn't preserve them.
        // We need to preserve Location annotations as well as line position annotations.
        CopyAnnotations(node, copy);

        // We also need to preserve line position annotations for all attributes
        // since we report errors with attribute locations.
        if (copyAttributeAnnotations && node.NodeType == XmlNodeType.Element)
        {
            var sourceElement = (XElement)(object)node;
            var targetElement = (XElement)copy;

            var sourceAttributes = sourceElement.Attributes().GetEnumerator();
            var targetAttributes = targetElement.Attributes().GetEnumerator();
            while (sourceAttributes.MoveNext() && targetAttributes.MoveNext())
            {
                Debug.Assert(sourceAttributes.Current.Name == targetAttributes.Current.Name);
                CopyAnnotations(sourceAttributes.Current, targetAttributes.Current);
            }
        }

        return (TNode)copy;
    }

    private static void CopyAnnotations(XObject source, XObject target)
    {
        foreach (var annotation in source.Annotations<object>())
        {
            target.AddAnnotation(annotation);
        }
    }

    private static XNode[]? TrySelectNodes(XNode node, string xpath)
    {
        try
        {
            var xpathResult = (IEnumerable)System.Xml.XPath.Extensions.XPathEvaluate(node, xpath);

            // Throws InvalidOperationException if the result of the XPath is an XDocument:
            return xpathResult?.Cast<XNode>().ToArray();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (XPathException)
        {
            return null;
        }
    }

    private static bool ElementNameIs(XElement element, string name)
        => string.IsNullOrEmpty(element.Name.NamespaceName) && DocumentationCommentXmlNames.ElementEquals(element.Name.LocalName, name);

    public static ImmutableArray<ISymbol> ExplicitOrImplicitInterfaceImplementations(this ISymbol symbol)
    {
        if (symbol.Kind is not SymbolKind.Method and not SymbolKind.Property and not SymbolKind.Event)
        {
            return [];
        }

        var containingType = symbol.ContainingType;
        var query = from iface in containingType.AllInterfaces
                    from interfaceMember in iface.GetMembers()
                    let impl = containingType.FindImplementationForInterfaceMember(interfaceMember)
                    where SymbolEqualityComparer.Default.Equals(symbol, impl)
                    select interfaceMember;
        return query.ToImmutableArray();
    }

    public static ImmutableArray<ISymbol> ExplicitInterfaceImplementations(this ISymbol symbol)
        => symbol switch
        {
            IEventSymbol @event => ImmutableArray<ISymbol>.CastUp(@event.ExplicitInterfaceImplementations),
            IMethodSymbol method => ImmutableArray<ISymbol>.CastUp(method.ExplicitInterfaceImplementations),
            IPropertySymbol property => ImmutableArray<ISymbol>.CastUp(property.ExplicitInterfaceImplementations),
            _ => [],
        };

    public static ImmutableArray<ITypeParameterSymbol> GetAllTypeParameters(this ISymbol? symbol)
    {
        var results = ImmutableArray.CreateBuilder<ITypeParameterSymbol>();

        while (symbol != null)
        {
            results.AddRange(symbol.GetTypeParameters());
            symbol = symbol.ContainingType;
        }

        return results.ToImmutable();
    }

    public static ImmutableArray<ITypeParameterSymbol> GetTypeParameters(this ISymbol? symbol)
        => symbol switch
        {
            IMethodSymbol m => m.TypeParameters,
            INamedTypeSymbol nt => nt.TypeParameters,
            _ => [],
        };

    public static ImmutableArray<ITypeSymbol> GetTypeArguments(this ISymbol? symbol)
        => symbol switch
        {
            IMethodSymbol m => m.TypeArguments,
            INamedTypeSymbol nt => nt.TypeArguments,
            _ => [],
        };

    public static ISymbol? GetOverriddenMember(this ISymbol? symbol)
       => symbol switch
       {
           IMethodSymbol method => method.OverriddenMethod,
           IPropertySymbol property => property.OverriddenProperty,
           IEventSymbol @event => @event.OverriddenEvent,
           _ => null,
       };

    public static ImmutableArray<ITypeSymbol> GetAllTypeArguments(this ISymbol symbol)
    {
        var results = ImmutableArray.CreateBuilder<ITypeSymbol>();
        results.AddRange(symbol.GetTypeArguments());

        var containingType = symbol.ContainingType;
        while (containingType != null)
        {
            results.AddRange(containingType.GetTypeArguments());
            containingType = containingType.ContainingType;
        }

        return results.ToImmutable();
    }

    public static int IndexOf<T>(this IList<T> list, Func<T, bool> predicate)
    {
        for (var i = 0; i < list.Count; i++)
        {
            if (predicate(list[i]))
            {
                return i;
            }
        }

        return -1;
    }
}

internal static class DocumentationCommentXmlNames
{
    public const string CElementName = "c";
    public const string CodeElementName = "code";
    public const string CompletionListElementName = "completionlist";
    public const string DescriptionElementName = "description";
    public const string ExampleElementName = "example";
    public const string ExceptionElementName = "exception";
    public const string IncludeElementName = "include";
    public const string InheritdocElementName = "inheritdoc";
    public const string ItemElementName = "item";
    public const string ListElementName = "list";
    public const string ListHeaderElementName = "listheader";
    public const string ParaElementName = "para";
    public const string ParameterElementName = "param";
    public const string ParameterReferenceElementName = "paramref";
    public const string PermissionElementName = "permission";
    public const string PlaceholderElementName = "placeholder";
    public const string PreliminaryElementName = "preliminary";
    public const string RemarksElementName = "remarks";
    public const string ReturnsElementName = "returns";
    public const string SeeElementName = "see";
    public const string SeeAlsoElementName = "seealso";
    public const string SummaryElementName = "summary";
    public const string TermElementName = "term";
    public const string ThreadSafetyElementName = "threadsafety";
    public const string TypeParameterElementName = "typeparam";
    public const string TypeParameterReferenceElementName = "typeparamref";
    public const string ValueElementName = "value";
    public const string CrefAttributeName = "cref";
    public const string HrefAttributeName = "href";
    public const string FileAttributeName = "file";
    public const string InstanceAttributeName = "instance";
    public const string LangwordAttributeName = "langword";
    public const string NameAttributeName = "name";
    public const string PathAttributeName = "path";
    public const string StaticAttributeName = "static";
    public const string TypeAttributeName = "type";
    public static bool ElementEquals(string name1, string name2, bool fromVb = false)
    {
        return string.Equals(name1, name2, fromVb ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
    }
    public static bool AttributeEquals(string name1, string name2)
    {
        return string.Equals(name1, name2, StringComparison.Ordinal);
    }
    public static new bool Equals(object left, object right)
    {
        return object.Equals(left, right);
    }
}
