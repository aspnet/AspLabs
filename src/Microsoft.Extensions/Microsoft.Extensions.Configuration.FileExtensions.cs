namespace Microsoft.Extensions.Configuration
{
    public static partial class FileConfigurationExtensions
    {
        public static Microsoft.Extensions.FileProviders.IFileProvider GetFileProvider(this Microsoft.Extensions.Configuration.IConfigurationBuilder builder) { throw null; }
        public static Microsoft.Extensions.Configuration.IConfigurationBuilder SetBasePath(this Microsoft.Extensions.Configuration.IConfigurationBuilder builder, string basePath) { throw null; }
        public static Microsoft.Extensions.Configuration.IConfigurationBuilder SetFileProvider(this Microsoft.Extensions.Configuration.IConfigurationBuilder builder, Microsoft.Extensions.FileProviders.IFileProvider fileProvider) { throw null; }
    }
    public abstract partial class FileConfigurationProvider : Microsoft.Extensions.Configuration.ConfigurationProvider
    {
        public FileConfigurationProvider(Microsoft.Extensions.Configuration.FileConfigurationSource source) { }
        public Microsoft.Extensions.Configuration.FileConfigurationSource Source { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public override void Load() { }
        public abstract void Load(System.IO.Stream stream);
    }
    public abstract partial class FileConfigurationSource : Microsoft.Extensions.Configuration.IConfigurationSource
    {
        protected FileConfigurationSource() { }
        public Microsoft.Extensions.FileProviders.IFileProvider FileProvider { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public bool Optional { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public string Path { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public bool ReloadOnChange { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public abstract Microsoft.Extensions.Configuration.IConfigurationProvider Build(Microsoft.Extensions.Configuration.IConfigurationBuilder builder);
    }
}
