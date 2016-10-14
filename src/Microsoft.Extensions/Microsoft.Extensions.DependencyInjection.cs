namespace Microsoft.Extensions.DependencyInjection
{
    public partial class ServiceCollection : Microsoft.Extensions.DependencyInjection.IServiceCollection, System.Collections.Generic.ICollection<Microsoft.Extensions.DependencyInjection.ServiceDescriptor>, System.Collections.Generic.IEnumerable<Microsoft.Extensions.DependencyInjection.ServiceDescriptor>, System.Collections.Generic.IList<Microsoft.Extensions.DependencyInjection.ServiceDescriptor>, System.Collections.IEnumerable
    {
        public ServiceCollection() { }
        public int Count { get { throw null; } }
        public bool IsReadOnly { get { throw null; } }
        public Microsoft.Extensions.DependencyInjection.ServiceDescriptor this[int index] { get { throw null; } set { } }
        public void Clear() { }
        public bool Contains(Microsoft.Extensions.DependencyInjection.ServiceDescriptor item) { throw null; }
        public void CopyTo(Microsoft.Extensions.DependencyInjection.ServiceDescriptor[] array, int arrayIndex) { }
        public System.Collections.Generic.IEnumerator<Microsoft.Extensions.DependencyInjection.ServiceDescriptor> GetEnumerator() { throw null; }
        public int IndexOf(Microsoft.Extensions.DependencyInjection.ServiceDescriptor item) { throw null; }
        public void Insert(int index, Microsoft.Extensions.DependencyInjection.ServiceDescriptor item) { }
        public bool Remove(Microsoft.Extensions.DependencyInjection.ServiceDescriptor item) { throw null; }
        public void RemoveAt(int index) { }
        void System.Collections.Generic.ICollection<Microsoft.Extensions.DependencyInjection.ServiceDescriptor>.Add(Microsoft.Extensions.DependencyInjection.ServiceDescriptor item) { }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { throw null; }
    }
    public static partial class ServiceCollectionContainerBuilderExtensions
    {
        public static System.IServiceProvider BuildServiceProvider(this Microsoft.Extensions.DependencyInjection.IServiceCollection services) { throw null; }
    }
}
