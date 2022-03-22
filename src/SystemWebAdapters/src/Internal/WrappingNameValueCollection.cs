using System.Collections.Specialized;

namespace System.Web.Internal
{
    internal abstract class WrappingNameValueCollection : NameValueCollection
    {
        private const string IndexErrorMessage = "ASP.NET Core doesn't support accessing server variables by index.";

        public sealed override string? Get(int index) => throw new PlatformNotSupportedException(IndexErrorMessage);

        public sealed override string? GetKey(int index) => throw new PlatformNotSupportedException(IndexErrorMessage);

        public sealed override string[]? GetValues(int index) => throw new PlatformNotSupportedException(IndexErrorMessage);

        public sealed override KeysCollection Keys => throw new PlatformNotSupportedException("KeysCollection is not supported as Get(int) is not available.");
    }
}
