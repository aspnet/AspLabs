namespace System.Threading.RateLimiting
{
    public sealed class MetadataName<T> : IEquatable<MetadataName<T>>
    {
        private readonly string _name;

        public MetadataName(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public override string ToString()
        {
            return _name ?? string.Empty;
        }

        public override int GetHashCode()
        {
            return _name == null ? 0 : _name.GetHashCode();
        }
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is MetadataName<T> && Equals((MetadataName<T>)obj);
        }

        public bool Equals(MetadataName<T> other)
        {
            // NOTE: intentionally ordinal and case sensitive, matches CNG.
            return _name == other._name;
        }

        public static bool operator ==(MetadataName<T> left, MetadataName<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MetadataName<T> left, MetadataName<T> right)
        {
            return !(left == right);
        }
    }
}
