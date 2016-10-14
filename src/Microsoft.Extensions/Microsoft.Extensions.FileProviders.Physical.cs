namespace Microsoft.Extensions.FileProviders
{
    public partial class FileSystemInfoHelper
    {
        public FileSystemInfoHelper() { }
    }
    public partial class PhysicalFileProvider : Microsoft.Extensions.FileProviders.IFileProvider, System.IDisposable
    {
        public PhysicalFileProvider(string root) { }
        public string Root { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public void Dispose() { }
        public Microsoft.Extensions.FileProviders.IDirectoryContents GetDirectoryContents(string subpath) { throw null; }
        public Microsoft.Extensions.FileProviders.IFileInfo GetFileInfo(string subpath) { throw null; }
        public Microsoft.Extensions.Primitives.IChangeToken Watch(string filter) { throw null; }
    }
}
namespace Microsoft.Extensions.FileProviders.Physical
{
    public partial class PhysicalDirectoryInfo : Microsoft.Extensions.FileProviders.IFileInfo
    {
        public PhysicalDirectoryInfo(System.IO.DirectoryInfo info) { }
        public bool Exists { get { throw null; } }
        public bool IsDirectory { get { throw null; } }
        public System.DateTimeOffset LastModified { get { throw null; } }
        public long Length { get { throw null; } }
        public string Name { get { throw null; } }
        public string PhysicalPath { get { throw null; } }
        public System.IO.Stream CreateReadStream() { throw null; }
    }
    public partial class PhysicalFileInfo : Microsoft.Extensions.FileProviders.IFileInfo
    {
        public PhysicalFileInfo(System.IO.FileInfo info) { }
        public bool Exists { get { throw null; } }
        public bool IsDirectory { get { throw null; } }
        public System.DateTimeOffset LastModified { get { throw null; } }
        public long Length { get { throw null; } }
        public string Name { get { throw null; } }
        public string PhysicalPath { get { throw null; } }
        public System.IO.Stream CreateReadStream() { throw null; }
    }
    public partial class PhysicalFilesWatcher : System.IDisposable
    {
        public PhysicalFilesWatcher(string root, System.IO.FileSystemWatcher fileSystemWatcher, bool pollForChanges) { }
        public Microsoft.Extensions.Primitives.IChangeToken CreateFileChangeToken(string filter) { throw null; }
        public void Dispose() { }
    }
    public partial class PollingFileChangeToken : Microsoft.Extensions.Primitives.IChangeToken
    {
        public PollingFileChangeToken(System.IO.FileInfo fileInfo) { }
        public bool ActiveChangeCallbacks { get { throw null; } }
        public bool HasChanged { get { throw null; } }
        public System.IDisposable RegisterChangeCallback(System.Action<object> callback, object state) { throw null; }
    }
}
