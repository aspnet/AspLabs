using System;
using Microsoft.CodeAnalysis;

namespace Microsoft.AspNetCore.Migration
{
    static class WebApiFacts
    {
        public static bool IsWebApiController(INamedTypeSymbol type, INamedTypeSymbol ihttpController)
        {
            if (type.TypeKind != TypeKind.Class)
            {
                return false;
            }

            if (type.IsAbstract)
            {
                return false;
            }

            // We only consider public top-level classes as controllers.
            if (type.DeclaredAccessibility != Accessibility.Public)
            {
                return false;
            }

            if (!ihttpController.IsAssignableFrom(type))
            {
                return false;
            }

            if (!type.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        // Based on https://github.com/aspnet/AspNetWebStack/blob/749384689e027a2fcd29eb79a9137b94cea611a8/src/System.Web.Http/Controllers/ApiControllerActionSelector.cs#L600-L621
        public static bool IsWebApiAction(IMethodSymbol method, ITypeSymbol nonActionAttribute)
        {
            if (method.MethodKind is MethodKind.Constructor or MethodKind.EventAdd or MethodKind.EventRaise or MethodKind.EventRemove)
            {
                // not a normal method, e.g. a constructor or an event
                return false;
            }

            if (method.HasAttribute(nonActionAttribute))
            {
                return false;
            }

            return true;
        }
    }
}
