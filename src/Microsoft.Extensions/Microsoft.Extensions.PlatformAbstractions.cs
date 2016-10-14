namespace Microsoft.Extensions.PlatformAbstractions
{
    public partial class ApplicationEnvironment
    {
        public ApplicationEnvironment() { }
        public string ApplicationBasePath { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public string ApplicationName { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public string ApplicationVersion { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public System.Runtime.Versioning.FrameworkName RuntimeFramework { get { throw null; } }
    }
    public partial class PlatformServices
    {
        internal PlatformServices() { }
        public Microsoft.Extensions.PlatformAbstractions.ApplicationEnvironment Application { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public static Microsoft.Extensions.PlatformAbstractions.PlatformServices Default { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
    }
}
