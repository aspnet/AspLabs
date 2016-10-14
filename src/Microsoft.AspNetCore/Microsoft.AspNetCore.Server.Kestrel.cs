namespace Microsoft.AspNetCore.Hosting
{
    public static partial class KestrelServerOptionsConnectionLoggingExtensions
    {
        public static Microsoft.AspNetCore.Server.Kestrel.KestrelServerOptions UseConnectionLogging(this Microsoft.AspNetCore.Server.Kestrel.KestrelServerOptions options) { throw null; }
        public static Microsoft.AspNetCore.Server.Kestrel.KestrelServerOptions UseConnectionLogging(this Microsoft.AspNetCore.Server.Kestrel.KestrelServerOptions options, string loggerName) { throw null; }
    }
    public static partial class WebHostBuilderKestrelExtensions
    {
        public static Microsoft.AspNetCore.Hosting.IWebHostBuilder UseKestrel(this Microsoft.AspNetCore.Hosting.IWebHostBuilder hostBuilder) { throw null; }
        public static Microsoft.AspNetCore.Hosting.IWebHostBuilder UseKestrel(this Microsoft.AspNetCore.Hosting.IWebHostBuilder hostBuilder, System.Action<Microsoft.AspNetCore.Server.Kestrel.KestrelServerOptions> options) { throw null; }
    }
}
namespace Microsoft.AspNetCore.Server.Kestrel
{
    public sealed partial class BadHttpRequestException : System.IO.IOException
    {
        internal BadHttpRequestException() { }
    }
    public partial class KestrelServer : Microsoft.AspNetCore.Hosting.Server.IServer, System.IDisposable
    {
        public KestrelServer(Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Server.Kestrel.KestrelServerOptions> options, Microsoft.AspNetCore.Hosting.IApplicationLifetime applicationLifetime, Microsoft.Extensions.Logging.ILoggerFactory loggerFactory) { }
        public Microsoft.AspNetCore.Http.Features.IFeatureCollection Features { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public Microsoft.AspNetCore.Server.Kestrel.KestrelServerOptions Options { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public void Dispose() { }
        public void Start<TContext>(Microsoft.AspNetCore.Hosting.Server.IHttpApplication<TContext> application) { }
    }
    public partial class KestrelServerOptions
    {
        public KestrelServerOptions() { }
        public bool AddServerHeader { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public System.IServiceProvider ApplicationServices { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.AspNetCore.Server.Kestrel.Filter.IConnectionFilter ConnectionFilter { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public System.Nullable<long> MaxRequestBufferSize { get { throw null; } set { } }
        public bool NoDelay { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public System.TimeSpan ShutdownTimeout { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public int ThreadCount { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    }
    public partial class ServerAddress
    {
        public ServerAddress() { }
        public string Host { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public bool IsUnixPipe { get { throw null; } }
        public string PathBase { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public int Port { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public string Scheme { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public string UnixPipePath { get { throw null; } }
        public override bool Equals(object obj) { throw null; }
        public static Microsoft.AspNetCore.Server.Kestrel.ServerAddress FromUrl(string url) { throw null; }
        public override int GetHashCode() { throw null; }
        public override string ToString() { throw null; }
    }
}
namespace Microsoft.AspNetCore.Server.Kestrel.Filter
{
    public partial class ConnectionFilterContext
    {
        public ConnectionFilterContext() { }
        public Microsoft.AspNetCore.Server.Kestrel.ServerAddress Address { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public System.IO.Stream Connection { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public System.Action<Microsoft.AspNetCore.Http.Features.IFeatureCollection> PrepareRequest { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    }
    public partial interface IConnectionFilter
    {
        System.Threading.Tasks.Task OnConnectionAsync(Microsoft.AspNetCore.Server.Kestrel.Filter.ConnectionFilterContext context);
    }
    public partial class LoggingConnectionFilter : Microsoft.AspNetCore.Server.Kestrel.Filter.IConnectionFilter
    {
        public LoggingConnectionFilter(Microsoft.Extensions.Logging.ILogger logger, Microsoft.AspNetCore.Server.Kestrel.Filter.IConnectionFilter previous) { }
        public System.Threading.Tasks.Task OnConnectionAsync(Microsoft.AspNetCore.Server.Kestrel.Filter.ConnectionFilterContext context) { throw null; }
    }
    public partial class NoOpConnectionFilter : Microsoft.AspNetCore.Server.Kestrel.Filter.IConnectionFilter
    {
        public NoOpConnectionFilter() { }
        public System.Threading.Tasks.Task OnConnectionAsync(Microsoft.AspNetCore.Server.Kestrel.Filter.ConnectionFilterContext context) { throw null; }
    }
}
namespace Microsoft.AspNetCore.Server.Kestrel.Filter.Internal
{
    public partial class FilteredStreamAdapter : System.IDisposable
    {
        public FilteredStreamAdapter(string connectionId, System.IO.Stream filteredStream, Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPool memory, Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IKestrelTrace logger, Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IThreadPool threadPool, Microsoft.AspNetCore.Server.Kestrel.Internal.Http.IBufferSizeControl bufferSizeControl) { }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Http.SocketInput SocketInput { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Http.ISocketOutput SocketOutput { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public void Abort() { }
        public void Dispose() { }
        public System.Threading.Tasks.Task ReadInputAsync() { throw null; }
    }
    public partial class LibuvStream : System.IO.Stream
    {
        public LibuvStream(Microsoft.AspNetCore.Server.Kestrel.Internal.Http.SocketInput input, Microsoft.AspNetCore.Server.Kestrel.Internal.Http.ISocketOutput output) { }
        public override bool CanRead { get { throw null; } }
        public override bool CanSeek { get { throw null; } }
        public override bool CanWrite { get { throw null; } }
        public override long Length { get { throw null; } }
        public override long Position { get { throw null; } set { } }
        public override void Flush() { }
        public override System.Threading.Tasks.Task FlushAsync(System.Threading.CancellationToken cancellationToken) { throw null; }
        public override int Read(byte[] buffer, int offset, int count) { throw null; }
        public override System.Threading.Tasks.Task<int> ReadAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken) { throw null; }
        public override long Seek(long offset, System.IO.SeekOrigin origin) { throw null; }
        public override void SetLength(long value) { }
        public override void Write(byte[] buffer, int offset, int count) { }
        public override System.Threading.Tasks.Task WriteAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken token) { throw null; }
    }
    public partial class StreamSocketOutput : Microsoft.AspNetCore.Server.Kestrel.Internal.Http.ISocketOutput
    {
        public StreamSocketOutput(string connectionId, System.IO.Stream outputStream, Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPool memory, Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IKestrelTrace logger) { }
        public void ProducingComplete(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator end) { }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator ProducingStart() { throw null; }
        public void Write(System.ArraySegment<byte> buffer, bool chunk) { }
        public System.Threading.Tasks.Task WriteAsync(System.ArraySegment<byte> buffer, bool chunk, System.Threading.CancellationToken cancellationToken) { throw null; }
    }
}
namespace Microsoft.AspNetCore.Server.Kestrel.Internal
{
    public partial class Disposable : System.IDisposable
    {
        public Disposable(System.Action dispose) { }
        public void Dispose() { }
        protected virtual void Dispose(bool disposing) { }
    }
    public partial class KestrelEngine : Microsoft.AspNetCore.Server.Kestrel.Internal.ServiceContext, System.IDisposable
    {
        public KestrelEngine(Microsoft.AspNetCore.Server.Kestrel.Internal.ServiceContext context) { }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv Libuv { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public System.Collections.Generic.List<Microsoft.AspNetCore.Server.Kestrel.Internal.KestrelThread> Threads { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public System.IDisposable CreateServer(Microsoft.AspNetCore.Server.Kestrel.ServerAddress address) { throw null; }
        public void Dispose() { }
        public void Start(int count) { }
    }
    public partial class KestrelServerOptionsSetup : Microsoft.Extensions.Options.IConfigureOptions<Microsoft.AspNetCore.Server.Kestrel.KestrelServerOptions>
    {
        public KestrelServerOptionsSetup(System.IServiceProvider services) { }
        public void Configure(Microsoft.AspNetCore.Server.Kestrel.KestrelServerOptions options) { }
    }
    public partial class KestrelThread
    {
        public KestrelThread(Microsoft.AspNetCore.Server.Kestrel.Internal.KestrelEngine engine) { }
        public System.Runtime.ExceptionServices.ExceptionDispatchInfo FatalError { get { throw null; } }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvLoopHandle Loop { get { throw null; } }
        public System.Action<System.Action<System.IntPtr>, System.IntPtr> QueueCloseHandle { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public void AllowStop() { }
        public void Post(System.Action<object> callback, object state) { }
        public System.Threading.Tasks.Task PostAsync(System.Action<object> callback, object state) { throw null; }
        public System.Threading.Tasks.Task StartAsync() { throw null; }
        public void Stop(System.TimeSpan timeout) { }
        public void Walk(System.Action<System.IntPtr> callback) { }
    }
    public partial class KestrelTrace : Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IKestrelTrace, Microsoft.Extensions.Logging.ILogger
    {
        protected readonly Microsoft.Extensions.Logging.ILogger _logger;
        public KestrelTrace(Microsoft.Extensions.Logging.ILogger logger) { }
        public virtual void ApplicationError(string connectionId, System.Exception ex) { }
        public virtual System.IDisposable BeginScope<TState>(TState state) { throw null; }
        public void ConnectionBadRequest(string connectionId, Microsoft.AspNetCore.Server.Kestrel.BadHttpRequestException ex) { }
        public virtual void ConnectionDisconnect(string connectionId) { }
        public virtual void ConnectionDisconnectedWrite(string connectionId, int count, System.Exception ex) { }
        public virtual void ConnectionError(string connectionId, System.Exception ex) { }
        public virtual void ConnectionKeepAlive(string connectionId) { }
        public virtual void ConnectionPause(string connectionId) { }
        public virtual void ConnectionRead(string connectionId, int count) { }
        public virtual void ConnectionReadFin(string connectionId) { }
        public virtual void ConnectionResume(string connectionId) { }
        public virtual void ConnectionStart(string connectionId) { }
        public virtual void ConnectionStop(string connectionId) { }
        public virtual void ConnectionWrite(string connectionId, int count) { }
        public virtual void ConnectionWriteCallback(string connectionId, int status) { }
        public virtual void ConnectionWriteFin(string connectionId) { }
        public virtual void ConnectionWroteFin(string connectionId, int status) { }
        public virtual bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) { throw null; }
        public virtual void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, System.Exception exception, System.Func<TState, System.Exception, string> formatter) { }
        public virtual void NotAllConnectionsClosedGracefully() { }
    }
    public partial class ServiceContext
    {
        public ServiceContext() { }
        public ServiceContext(Microsoft.AspNetCore.Server.Kestrel.Internal.ServiceContext context) { }
        public Microsoft.AspNetCore.Hosting.IApplicationLifetime AppLifetime { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Http.DateHeaderValueManager DateHeaderValueManager { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public System.Func<Microsoft.AspNetCore.Server.Kestrel.Internal.Http.ConnectionContext, Microsoft.AspNetCore.Server.Kestrel.Internal.Http.Frame> FrameFactory { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IKestrelTrace Log { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.AspNetCore.Server.Kestrel.KestrelServerOptions ServerOptions { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IThreadPool ThreadPool { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    }
}
namespace Microsoft.AspNetCore.Server.Kestrel.Internal.Http
{
    public partial class BufferSizeControl : Microsoft.AspNetCore.Server.Kestrel.Internal.Http.IBufferSizeControl
    {
        public BufferSizeControl(long maxSize, Microsoft.AspNetCore.Server.Kestrel.Internal.Http.IConnectionControl connectionControl, Microsoft.AspNetCore.Server.Kestrel.Internal.KestrelThread connectionThread) { }
        public void Add(int count) { }
        public void Subtract(int count) { }
    }
    public static partial class ChunkWriter
    {
        public static System.ArraySegment<byte> BeginChunkBytes(int dataCount) { throw null; }
        public static int WriteBeginChunkBytes(ref Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator start, int dataCount) { throw null; }
        public static void WriteEndChunkBytes(ref Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator start) { }
    }
    public partial class Connection : Microsoft.AspNetCore.Server.Kestrel.Internal.Http.ConnectionContext, Microsoft.AspNetCore.Server.Kestrel.Internal.Http.IConnectionControl
    {
        public Connection(Microsoft.AspNetCore.Server.Kestrel.Internal.Http.ListenerContext context, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle socket) { }
        public virtual void Abort() { }
        void Microsoft.AspNetCore.Server.Kestrel.Internal.Http.IConnectionControl.End(Microsoft.AspNetCore.Server.Kestrel.Internal.Http.ProduceEndType endType) { }
        void Microsoft.AspNetCore.Server.Kestrel.Internal.Http.IConnectionControl.Pause() { }
        void Microsoft.AspNetCore.Server.Kestrel.Internal.Http.IConnectionControl.Resume() { }
        public virtual void OnSocketClosed() { }
        public void Start() { }
        public System.Threading.Tasks.Task StopAsync() { throw null; }
    }
    public partial class ConnectionContext : Microsoft.AspNetCore.Server.Kestrel.Internal.Http.ListenerContext
    {
        public ConnectionContext() { }
        public ConnectionContext(Microsoft.AspNetCore.Server.Kestrel.Internal.Http.ConnectionContext context) { }
        public ConnectionContext(Microsoft.AspNetCore.Server.Kestrel.Internal.Http.ListenerContext context) { }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Http.IConnectionControl ConnectionControl { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public string ConnectionId { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public System.Net.IPEndPoint LocalEndPoint { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public System.Action<Microsoft.AspNetCore.Http.Features.IFeatureCollection> PrepareRequest { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public System.Net.IPEndPoint RemoteEndPoint { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Http.SocketInput SocketInput { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Http.ISocketOutput SocketOutput { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    }
    public partial class ConnectionManager
    {
        public ConnectionManager(Microsoft.AspNetCore.Server.Kestrel.Internal.KestrelThread thread) { }
        public System.Threading.Tasks.Task WaitForConnectionCloseAsync() { throw null; }
        public void WalkConnectionsAndClose() { }
    }
    public partial class DateHeaderValueManager : System.IDisposable
    {
        public DateHeaderValueManager() { }
        public void Dispose() { }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Http.DateHeaderValueManager.DateHeaderValues GetDateHeaderValues() { throw null; }
        public partial class DateHeaderValues
        {
            public byte[] Bytes;
            public string String;
            public DateHeaderValues() { }
        }
    }
    public abstract partial class Frame : Microsoft.AspNetCore.Server.Kestrel.Internal.Http.ConnectionContext, Microsoft.AspNetCore.Http.Features.IFeatureCollection, Microsoft.AspNetCore.Http.Features.IHttpConnectionFeature, Microsoft.AspNetCore.Http.Features.IHttpRequestFeature, Microsoft.AspNetCore.Http.Features.IHttpRequestLifetimeFeature, Microsoft.AspNetCore.Http.Features.IHttpResponseFeature, Microsoft.AspNetCore.Http.Features.IHttpUpgradeFeature, Microsoft.AspNetCore.Server.Kestrel.Internal.Http.IFrameControl, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<System.Type, object>>, System.Collections.IEnumerable
    {
        protected System.Exception _applicationException;
        protected bool _keepAlive;
        protected System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<System.Func<object, System.Threading.Tasks.Task>, object>> _onCompleted;
        protected System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<System.Func<object, System.Threading.Tasks.Task>, object>> _onStarting;
        protected int _requestAborted;
        protected Microsoft.AspNetCore.Server.Kestrel.Internal.Http.Frame.RequestProcessingStatus _requestProcessingStatus;
        protected bool _requestProcessingStopping;
        protected bool _requestRejected;
        public Frame(Microsoft.AspNetCore.Server.Kestrel.Internal.Http.ConnectionContext context) { }
        public string ConnectionIdFeature { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public System.IO.Stream DuplexStream { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Http.IFrameControl FrameControl { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        protected Microsoft.AspNetCore.Server.Kestrel.Internal.Http.FrameRequestHeaders FrameRequestHeaders { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        protected Microsoft.AspNetCore.Server.Kestrel.Internal.Http.FrameResponseHeaders FrameResponseHeaders { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public bool HasResponseStarted { get { throw null; } }
        public string HttpVersion { get { throw null; } set { } }
        public System.Net.IPAddress LocalIpAddress { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public int LocalPort { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public string Method { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        bool Microsoft.AspNetCore.Http.Features.IFeatureCollection.IsReadOnly { get { throw null; } }
        object Microsoft.AspNetCore.Http.Features.IFeatureCollection.this[System.Type key] { get { throw null; } set { } }
        int Microsoft.AspNetCore.Http.Features.IFeatureCollection.Revision { get { throw null; } }
        string Microsoft.AspNetCore.Http.Features.IHttpConnectionFeature.ConnectionId { get { throw null; } set { } }
        System.Net.IPAddress Microsoft.AspNetCore.Http.Features.IHttpConnectionFeature.LocalIpAddress { get { throw null; } set { } }
        int Microsoft.AspNetCore.Http.Features.IHttpConnectionFeature.LocalPort { get { throw null; } set { } }
        System.Net.IPAddress Microsoft.AspNetCore.Http.Features.IHttpConnectionFeature.RemoteIpAddress { get { throw null; } set { } }
        int Microsoft.AspNetCore.Http.Features.IHttpConnectionFeature.RemotePort { get { throw null; } set { } }
        System.IO.Stream Microsoft.AspNetCore.Http.Features.IHttpRequestFeature.Body { get { throw null; } set { } }
        Microsoft.AspNetCore.Http.IHeaderDictionary Microsoft.AspNetCore.Http.Features.IHttpRequestFeature.Headers { get { throw null; } set { } }
        string Microsoft.AspNetCore.Http.Features.IHttpRequestFeature.Method { get { throw null; } set { } }
        string Microsoft.AspNetCore.Http.Features.IHttpRequestFeature.Path { get { throw null; } set { } }
        string Microsoft.AspNetCore.Http.Features.IHttpRequestFeature.PathBase { get { throw null; } set { } }
        string Microsoft.AspNetCore.Http.Features.IHttpRequestFeature.Protocol { get { throw null; } set { } }
        string Microsoft.AspNetCore.Http.Features.IHttpRequestFeature.QueryString { get { throw null; } set { } }
        string Microsoft.AspNetCore.Http.Features.IHttpRequestFeature.RawTarget { get { throw null; } set { } }
        string Microsoft.AspNetCore.Http.Features.IHttpRequestFeature.Scheme { get { throw null; } set { } }
        System.Threading.CancellationToken Microsoft.AspNetCore.Http.Features.IHttpRequestLifetimeFeature.RequestAborted { get { throw null; } set { } }
        System.IO.Stream Microsoft.AspNetCore.Http.Features.IHttpResponseFeature.Body { get { throw null; } set { } }
        bool Microsoft.AspNetCore.Http.Features.IHttpResponseFeature.HasStarted { get { throw null; } }
        Microsoft.AspNetCore.Http.IHeaderDictionary Microsoft.AspNetCore.Http.Features.IHttpResponseFeature.Headers { get { throw null; } set { } }
        string Microsoft.AspNetCore.Http.Features.IHttpResponseFeature.ReasonPhrase { get { throw null; } set { } }
        int Microsoft.AspNetCore.Http.Features.IHttpResponseFeature.StatusCode { get { throw null; } set { } }
        bool Microsoft.AspNetCore.Http.Features.IHttpUpgradeFeature.IsUpgradableRequest { get { throw null; } }
        public string Path { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public string PathBase { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public string QueryString { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public string RawTarget { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public string ReasonPhrase { get { throw null; } set { } }
        public System.Net.IPAddress RemoteIpAddress { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public int RemotePort { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public System.Threading.CancellationToken RequestAborted { get { throw null; } set { } }
        public System.IO.Stream RequestBody { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.AspNetCore.Http.IHeaderDictionary RequestHeaders { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public System.IO.Stream ResponseBody { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.AspNetCore.Http.IHeaderDictionary ResponseHeaders { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public string Scheme { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public int StatusCode { get { throw null; } set { } }
        public void Abort() { }
        protected System.Threading.Tasks.Task FireOnCompleted() { throw null; }
        protected System.Threading.Tasks.Task FireOnStarting() { throw null; }
        public void Flush() { }
        public System.Threading.Tasks.Task FlushAsync(System.Threading.CancellationToken cancellationToken) { throw null; }
        public void InitializeHeaders() { }
        public void InitializeStreams(Microsoft.AspNetCore.Server.Kestrel.Internal.Http.MessageBody messageBody) { }
        TFeature Microsoft.AspNetCore.Http.Features.IFeatureCollection.Get<TFeature>() { throw null; }
        void Microsoft.AspNetCore.Http.Features.IFeatureCollection.Set<TFeature>(TFeature instance) { }
        void Microsoft.AspNetCore.Http.Features.IHttpRequestLifetimeFeature.Abort() { }
        void Microsoft.AspNetCore.Http.Features.IHttpResponseFeature.OnCompleted(System.Func<object, System.Threading.Tasks.Task> callback, object state) { }
        void Microsoft.AspNetCore.Http.Features.IHttpResponseFeature.OnStarting(System.Func<object, System.Threading.Tasks.Task> callback, object state) { }
        System.Threading.Tasks.Task<System.IO.Stream> Microsoft.AspNetCore.Http.Features.IHttpUpgradeFeature.UpgradeAsync() { throw null; }
        public void OnCompleted(System.Func<object, System.Threading.Tasks.Task> callback, object state) { }
        public void OnStarting(System.Func<object, System.Threading.Tasks.Task> callback, object state) { }
        public void PauseStreams() { }
        public void ProduceContinue() { }
        protected System.Threading.Tasks.Task ProduceEnd() { throw null; }
        public System.Threading.Tasks.Task ProduceStartAndFireOnStarting() { throw null; }
        public void RejectRequest(string message) { }
        protected void ReportApplicationError(System.Exception ex) { }
        public abstract System.Threading.Tasks.Task RequestProcessingAsync();
        public void Reset() { }
        public void ResetFeatureCollection() { }
        public void ResumeStreams() { }
        public void SetBadRequestState(Microsoft.AspNetCore.Server.Kestrel.BadHttpRequestException ex) { }
        public void Start() { }
        public bool StatusCanHaveBody(int statusCode) { throw null; }
        public System.Threading.Tasks.Task Stop() { throw null; }
        public void StopStreams() { }
        System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<System.Type, object>> System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<System.Type,System.Object>>.GetEnumerator() { throw null; }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { throw null; }
        public bool TakeMessageHeaders(Microsoft.AspNetCore.Server.Kestrel.Internal.Http.SocketInput input, Microsoft.AspNetCore.Server.Kestrel.Internal.Http.FrameRequestHeaders requestHeaders) { throw null; }
        protected Microsoft.AspNetCore.Server.Kestrel.Internal.Http.Frame.RequestLineStatus TakeStartLine(Microsoft.AspNetCore.Server.Kestrel.Internal.Http.SocketInput input) { throw null; }
        protected System.Threading.Tasks.Task TryProduceInvalidRequestResponse() { throw null; }
        public void Write(System.ArraySegment<byte> data) { }
        public System.Threading.Tasks.Task WriteAsync(System.ArraySegment<byte> data, System.Threading.CancellationToken cancellationToken) { throw null; }
        public System.Threading.Tasks.Task WriteAsyncAwaited(System.ArraySegment<byte> data, System.Threading.CancellationToken cancellationToken) { throw null; }
        protected enum RequestLineStatus
        {
            Done = 5,
            Empty = 0,
            Incomplete = 4,
            MethodIncomplete = 1,
            TargetIncomplete = 2,
            VersionIncomplete = 3,
        }
        protected enum RequestProcessingStatus
        {
            RequestPending = 0,
            RequestStarted = 1,
            ResponseStarted = 2,
        }
    }
    public partial class Frame<TContext> : Microsoft.AspNetCore.Server.Kestrel.Internal.Http.Frame
    {
        public Frame(Microsoft.AspNetCore.Hosting.Server.IHttpApplication<TContext> application, Microsoft.AspNetCore.Server.Kestrel.Internal.Http.ConnectionContext context) : base (default(Microsoft.AspNetCore.Server.Kestrel.Internal.Http.ConnectionContext)) { }
        public override System.Threading.Tasks.Task RequestProcessingAsync() { throw null; }
    }
    public abstract partial class FrameHeaders : Microsoft.AspNetCore.Http.IHeaderDictionary, System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>, System.Collections.Generic.IDictionary<string, Microsoft.Extensions.Primitives.StringValues>, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>, System.Collections.IEnumerable
    {
        protected bool _isReadOnly;
        protected System.Collections.Generic.Dictionary<string, Microsoft.Extensions.Primitives.StringValues> MaybeUnknown;
        protected FrameHeaders() { }
        Microsoft.Extensions.Primitives.StringValues Microsoft.AspNetCore.Http.IHeaderDictionary.this[string key] { get { throw null; } set { } }
        int System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<System.String,Microsoft.Extensions.Primitives.StringValues>>.Count { get { throw null; } }
        bool System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<System.String,Microsoft.Extensions.Primitives.StringValues>>.IsReadOnly { get { throw null; } }
        Microsoft.Extensions.Primitives.StringValues System.Collections.Generic.IDictionary<System.String,Microsoft.Extensions.Primitives.StringValues>.this[string key] { get { throw null; } set { } }
        System.Collections.Generic.ICollection<string> System.Collections.Generic.IDictionary<System.String,Microsoft.Extensions.Primitives.StringValues>.Keys { get { throw null; } }
        System.Collections.Generic.ICollection<Microsoft.Extensions.Primitives.StringValues> System.Collections.Generic.IDictionary<System.String,Microsoft.Extensions.Primitives.StringValues>.Values { get { throw null; } }
        protected System.Collections.Generic.Dictionary<string, Microsoft.Extensions.Primitives.StringValues> Unknown { get { throw null; } }
        protected virtual void AddValueFast(string key, Microsoft.Extensions.Primitives.StringValues value) { }
        protected static Microsoft.Extensions.Primitives.StringValues AppendValue(Microsoft.Extensions.Primitives.StringValues existing, string append) { throw null; }
        protected static int BitCount(long value) { throw null; }
        protected virtual void ClearFast() { }
        protected virtual void CopyToFast(System.Collections.Generic.KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>[] array, int arrayIndex) { }
        protected virtual int GetCountFast() { throw null; }
        protected virtual System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>> GetEnumeratorFast() { throw null; }
        protected virtual Microsoft.Extensions.Primitives.StringValues GetValueFast(string key) { throw null; }
        protected virtual bool RemoveFast(string key) { throw null; }
        public void Reset() { }
        public void SetReadOnly() { }
        protected virtual void SetValueFast(string key, Microsoft.Extensions.Primitives.StringValues value) { }
        void System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<System.String,Microsoft.Extensions.Primitives.StringValues>>.Add(System.Collections.Generic.KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> item) { }
        void System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<System.String,Microsoft.Extensions.Primitives.StringValues>>.Clear() { }
        bool System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<System.String,Microsoft.Extensions.Primitives.StringValues>>.Contains(System.Collections.Generic.KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> item) { throw null; }
        void System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<System.String,Microsoft.Extensions.Primitives.StringValues>>.CopyTo(System.Collections.Generic.KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>[] array, int arrayIndex) { }
        bool System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<System.String,Microsoft.Extensions.Primitives.StringValues>>.Remove(System.Collections.Generic.KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> item) { throw null; }
        void System.Collections.Generic.IDictionary<System.String,Microsoft.Extensions.Primitives.StringValues>.Add(string key, Microsoft.Extensions.Primitives.StringValues value) { }
        bool System.Collections.Generic.IDictionary<System.String,Microsoft.Extensions.Primitives.StringValues>.ContainsKey(string key) { throw null; }
        bool System.Collections.Generic.IDictionary<System.String,Microsoft.Extensions.Primitives.StringValues>.Remove(string key) { throw null; }
        bool System.Collections.Generic.IDictionary<System.String,Microsoft.Extensions.Primitives.StringValues>.TryGetValue(string key, out Microsoft.Extensions.Primitives.StringValues value) { value = default(Microsoft.Extensions.Primitives.StringValues); throw null; }
        System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>> System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<System.String,Microsoft.Extensions.Primitives.StringValues>>.GetEnumerator() { throw null; }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { throw null; }
        protected void ThrowArgumentException() { }
        protected void ThrowDuplicateKeyException() { }
        protected void ThrowKeyNotFoundException() { }
        protected void ThrowReadOnlyException() { }
        protected virtual bool TryGetValueFast(string key, out Microsoft.Extensions.Primitives.StringValues value) { value = default(Microsoft.Extensions.Primitives.StringValues); throw null; }
        public static void ValidateHeaderCharacters(Microsoft.Extensions.Primitives.StringValues headerValues) { }
        public static void ValidateHeaderCharacters(string headerCharacters) { }
    }
    public partial class FrameRequestHeaders : Microsoft.AspNetCore.Server.Kestrel.Internal.Http.FrameHeaders
    {
        public FrameRequestHeaders() { }
        public Microsoft.Extensions.Primitives.StringValues HeaderAccept { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderAcceptCharset { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderAcceptEncoding { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderAcceptLanguage { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderAccessControlRequestHeaders { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderAccessControlRequestMethod { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderAllow { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderAuthorization { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderCacheControl { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderConnection { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderContentEncoding { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderContentLanguage { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderContentLength { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderContentLocation { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderContentMD5 { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderContentRange { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderContentType { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderCookie { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderDate { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderExpect { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderExpires { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderFrom { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderHost { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderIfMatch { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderIfModifiedSince { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderIfNoneMatch { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderIfRange { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderIfUnmodifiedSince { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderKeepAlive { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderLastModified { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderMaxForwards { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderOrigin { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderPragma { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderProxyAuthorization { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderRange { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderReferer { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderTE { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderTrailer { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderTransferEncoding { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderTranslate { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderUpgrade { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderUserAgent { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderVia { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderWarning { get { throw null; } set { } }
        protected override void AddValueFast(string key, Microsoft.Extensions.Primitives.StringValues value) { }
        public void Append(byte[] keyBytes, int keyOffset, int keyLength, string value) { }
        protected override void ClearFast() { }
        protected override void CopyToFast(System.Collections.Generic.KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>[] array, int arrayIndex) { }
        protected override int GetCountFast() { throw null; }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Http.FrameRequestHeaders.Enumerator GetEnumerator() { throw null; }
        protected override System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>> GetEnumeratorFast() { throw null; }
        protected override Microsoft.Extensions.Primitives.StringValues GetValueFast(string key) { throw null; }
        protected override bool RemoveFast(string key) { throw null; }
        protected override void SetValueFast(string key, Microsoft.Extensions.Primitives.StringValues value) { }
        protected override bool TryGetValueFast(string key, out Microsoft.Extensions.Primitives.StringValues value) { value = default(Microsoft.Extensions.Primitives.StringValues); throw null; }
        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public partial struct Enumerator : System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>, System.Collections.IEnumerator, System.IDisposable
        {
            public System.Collections.Generic.KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> Current { get { throw null; } }
            object System.Collections.IEnumerator.Current { get { throw null; } }
            public void Dispose() { }
            public bool MoveNext() { throw null; }
            public void Reset() { }
        }
    }
    public partial class FrameResponseHeaders : Microsoft.AspNetCore.Server.Kestrel.Internal.Http.FrameHeaders
    {
        public FrameResponseHeaders() { }
        public bool HasConnection { get { throw null; } }
        public bool HasContentLength { get { throw null; } }
        public bool HasDate { get { throw null; } }
        public bool HasServer { get { throw null; } }
        public bool HasTransferEncoding { get { throw null; } }
        public Microsoft.Extensions.Primitives.StringValues HeaderAcceptRanges { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderAccessControlAllowCredentials { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderAccessControlAllowHeaders { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderAccessControlAllowMethods { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderAccessControlAllowOrigin { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderAccessControlExposeHeaders { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderAccessControlMaxAge { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderAge { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderAllow { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderCacheControl { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderConnection { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderContentEncoding { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderContentLanguage { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderContentLength { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderContentLocation { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderContentMD5 { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderContentRange { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderContentType { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderDate { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderETag { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderExpires { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderKeepAlive { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderLastModified { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderLocation { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderPragma { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderProxyAutheticate { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderRetryAfter { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderServer { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderSetCookie { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderTrailer { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderTransferEncoding { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderUpgrade { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderVary { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderVia { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderWarning { get { throw null; } set { } }
        public Microsoft.Extensions.Primitives.StringValues HeaderWWWAuthenticate { get { throw null; } set { } }
        protected override void AddValueFast(string key, Microsoft.Extensions.Primitives.StringValues value) { }
        protected override void ClearFast() { }
        public void CopyTo(ref Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator output) { }
        protected void CopyToFast(ref Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator output) { }
        protected override void CopyToFast(System.Collections.Generic.KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>[] array, int arrayIndex) { }
        protected override int GetCountFast() { throw null; }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Http.FrameResponseHeaders.Enumerator GetEnumerator() { throw null; }
        protected override System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>> GetEnumeratorFast() { throw null; }
        protected override Microsoft.Extensions.Primitives.StringValues GetValueFast(string key) { throw null; }
        protected override bool RemoveFast(string key) { throw null; }
        public void SetRawConnection(Microsoft.Extensions.Primitives.StringValues value, byte[] raw) { }
        public void SetRawContentLength(Microsoft.Extensions.Primitives.StringValues value, byte[] raw) { }
        public void SetRawDate(Microsoft.Extensions.Primitives.StringValues value, byte[] raw) { }
        public void SetRawServer(Microsoft.Extensions.Primitives.StringValues value, byte[] raw) { }
        public void SetRawTransferEncoding(Microsoft.Extensions.Primitives.StringValues value, byte[] raw) { }
        protected override void SetValueFast(string key, Microsoft.Extensions.Primitives.StringValues value) { }
        protected override bool TryGetValueFast(string key, out Microsoft.Extensions.Primitives.StringValues value) { value = default(Microsoft.Extensions.Primitives.StringValues); throw null; }
        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public partial struct Enumerator : System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>, System.Collections.IEnumerator, System.IDisposable
        {
            public System.Collections.Generic.KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> Current { get { throw null; } }
            object System.Collections.IEnumerator.Current { get { throw null; } }
            public void Dispose() { }
            public bool MoveNext() { throw null; }
            public void Reset() { }
        }
    }
    public partial interface IBufferSizeControl
    {
        void Add(int count);
        void Subtract(int count);
    }
    public partial interface IConnectionControl
    {
        void End(Microsoft.AspNetCore.Server.Kestrel.Internal.Http.ProduceEndType endType);
        void Pause();
        void Resume();
    }
    public partial interface IFrameControl
    {
        void Flush();
        System.Threading.Tasks.Task FlushAsync(System.Threading.CancellationToken cancellationToken);
        void ProduceContinue();
        void Write(System.ArraySegment<byte> data);
        System.Threading.Tasks.Task WriteAsync(System.ArraySegment<byte> data, System.Threading.CancellationToken cancellationToken);
    }
    public partial interface ISocketOutput
    {
        void ProducingComplete(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator end);
        Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator ProducingStart();
        void Write(System.ArraySegment<byte> buffer, bool chunk=false);
        System.Threading.Tasks.Task WriteAsync(System.ArraySegment<byte> buffer, bool chunk=false, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
    }
    public abstract partial class Listener : Microsoft.AspNetCore.Server.Kestrel.Internal.Http.ListenerContext
    {
        protected Listener(Microsoft.AspNetCore.Server.Kestrel.Internal.ServiceContext serviceContext) { }
        protected Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle ListenSocket { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        protected static void ConnectionCallback(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle stream, int status, System.Exception error, object state) { }
        protected abstract Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle CreateListenSocket();
        protected virtual void DispatchConnection(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle socket) { }
        public virtual System.Threading.Tasks.Task DisposeAsync() { throw null; }
        protected abstract void OnConnection(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle listenSocket, int status);
        public System.Threading.Tasks.Task StartAsync(Microsoft.AspNetCore.Server.Kestrel.ServerAddress address, Microsoft.AspNetCore.Server.Kestrel.Internal.KestrelThread thread) { throw null; }
    }
    public partial class ListenerContext : Microsoft.AspNetCore.Server.Kestrel.Internal.ServiceContext
    {
        public ListenerContext() { }
        public ListenerContext(Microsoft.AspNetCore.Server.Kestrel.Internal.Http.ListenerContext listenerContext) { }
        public ListenerContext(Microsoft.AspNetCore.Server.Kestrel.Internal.ServiceContext serviceContext) { }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Http.ConnectionManager ConnectionManager { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPool Memory { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.AspNetCore.Server.Kestrel.ServerAddress ServerAddress { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.KestrelThread Thread { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public System.Collections.Generic.Queue<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvWriteReq> WriteReqPool { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    }
    public abstract partial class ListenerPrimary : Microsoft.AspNetCore.Server.Kestrel.Internal.Http.Listener
    {
        protected ListenerPrimary(Microsoft.AspNetCore.Server.Kestrel.Internal.ServiceContext serviceContext) : base (default(Microsoft.AspNetCore.Server.Kestrel.Internal.ServiceContext)) { }
        protected override void DispatchConnection(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle socket) { }
        public override System.Threading.Tasks.Task DisposeAsync() { throw null; }
        public System.Threading.Tasks.Task StartAsync(string pipeName, Microsoft.AspNetCore.Server.Kestrel.ServerAddress address, Microsoft.AspNetCore.Server.Kestrel.Internal.KestrelThread thread) { throw null; }
    }
    public abstract partial class ListenerSecondary : Microsoft.AspNetCore.Server.Kestrel.Internal.Http.ListenerContext
    {
        protected ListenerSecondary(Microsoft.AspNetCore.Server.Kestrel.Internal.ServiceContext serviceContext) { }
        protected abstract Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle CreateAcceptSocket();
        public System.Threading.Tasks.Task DisposeAsync() { throw null; }
        public System.Threading.Tasks.Task StartAsync(string pipeName, Microsoft.AspNetCore.Server.Kestrel.ServerAddress address, Microsoft.AspNetCore.Server.Kestrel.Internal.KestrelThread thread) { throw null; }
    }
    public abstract partial class MessageBody
    {
        protected MessageBody(Microsoft.AspNetCore.Server.Kestrel.Internal.Http.Frame context) { }
        public bool RequestKeepAlive { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]protected set { } }
        public System.Threading.Tasks.Task Consume(System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken)) { throw null; }
        public static Microsoft.AspNetCore.Server.Kestrel.Internal.Http.MessageBody For(string httpVersion, Microsoft.AspNetCore.Server.Kestrel.Internal.Http.FrameRequestHeaders headers, Microsoft.AspNetCore.Server.Kestrel.Internal.Http.Frame context) { throw null; }
        public System.Threading.Tasks.ValueTask<int> ReadAsync(System.ArraySegment<byte> buffer, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken)) { throw null; }
        public abstract System.Threading.Tasks.ValueTask<int> ReadAsyncImplementation(System.ArraySegment<byte> buffer, System.Threading.CancellationToken cancellationToken);
    }
    public static partial class PathNormalizer
    {
        public static string RemoveDotSegments(string path) { throw null; }
    }
    public partial class PipeListener : Microsoft.AspNetCore.Server.Kestrel.Internal.Http.Listener
    {
        public PipeListener(Microsoft.AspNetCore.Server.Kestrel.Internal.ServiceContext serviceContext) : base (default(Microsoft.AspNetCore.Server.Kestrel.Internal.ServiceContext)) { }
        protected override Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle CreateListenSocket() { throw null; }
        protected override void OnConnection(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle listenSocket, int status) { }
    }
    public partial class PipeListenerPrimary : Microsoft.AspNetCore.Server.Kestrel.Internal.Http.ListenerPrimary
    {
        public PipeListenerPrimary(Microsoft.AspNetCore.Server.Kestrel.Internal.ServiceContext serviceContext) : base (default(Microsoft.AspNetCore.Server.Kestrel.Internal.ServiceContext)) { }
        protected override Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle CreateListenSocket() { throw null; }
        protected override void OnConnection(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle listenSocket, int status) { }
    }
    public partial class PipeListenerSecondary : Microsoft.AspNetCore.Server.Kestrel.Internal.Http.ListenerSecondary
    {
        public PipeListenerSecondary(Microsoft.AspNetCore.Server.Kestrel.Internal.ServiceContext serviceContext) : base (default(Microsoft.AspNetCore.Server.Kestrel.Internal.ServiceContext)) { }
        protected override Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle CreateAcceptSocket() { throw null; }
    }
    public enum ProduceEndType
    {
        ConnectionKeepAlive = 2,
        SocketDisconnect = 1,
        SocketShutdown = 0,
    }
    public static partial class ReasonPhrases
    {
        public static byte[] ToStatusBytes(int statusCode, string reasonPhrase=null) { throw null; }
    }
    public partial class SocketInput : System.IDisposable, System.Runtime.CompilerServices.ICriticalNotifyCompletion, System.Runtime.CompilerServices.INotifyCompletion
    {
        public SocketInput(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPool memory, Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IThreadPool threadPool, Microsoft.AspNetCore.Server.Kestrel.Internal.Http.IBufferSizeControl bufferSizeControl=null) { }
        public bool IsCompleted { get { throw null; } }
        public bool RemoteIntakeFin { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public void AbortAwaiting() { }
        public void CompleteAwaiting() { }
        public void ConsumingComplete(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator consumed, Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator examined) { }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator ConsumingStart() { throw null; }
        public void Dispose() { }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Http.SocketInput GetAwaiter() { throw null; }
        public void GetResult() { }
        public void IncomingComplete(int count, System.Exception error) { }
        public void IncomingData(byte[] buffer, int offset, int count) { }
        public void IncomingDeferred() { }
        public void IncomingFin() { }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolBlock IncomingStart() { throw null; }
        public void OnCompleted(System.Action continuation) { }
        public void UnsafeOnCompleted(System.Action continuation) { }
    }
    public static partial class SocketInputExtensions
    {
        public static System.Threading.Tasks.ValueTask<int> ReadAsync(this Microsoft.AspNetCore.Server.Kestrel.Internal.Http.SocketInput input, byte[] buffer, int offset, int count) { throw null; }
    }
    public partial class SocketOutput : Microsoft.AspNetCore.Server.Kestrel.Internal.Http.ISocketOutput
    {
        public const int MaxPooledWriteReqs = 1024;
        public SocketOutput(Microsoft.AspNetCore.Server.Kestrel.Internal.KestrelThread thread, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle socket, Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPool memory, Microsoft.AspNetCore.Server.Kestrel.Internal.Http.Connection connection, string connectionId, Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IKestrelTrace log, Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IThreadPool threadPool, System.Collections.Generic.Queue<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvWriteReq> writeReqPool) { }
        public void End(Microsoft.AspNetCore.Server.Kestrel.Internal.Http.ProduceEndType endType) { }
        void Microsoft.AspNetCore.Server.Kestrel.Internal.Http.ISocketOutput.Write(System.ArraySegment<byte> buffer, bool chunk) { }
        System.Threading.Tasks.Task Microsoft.AspNetCore.Server.Kestrel.Internal.Http.ISocketOutput.WriteAsync(System.ArraySegment<byte> buffer, bool chunk, System.Threading.CancellationToken cancellationToken) { throw null; }
        public void ProducingComplete(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator end) { }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator ProducingStart() { throw null; }
        public System.Threading.Tasks.Task WriteAsync(System.ArraySegment<byte> buffer, System.Threading.CancellationToken cancellationToken, bool chunk=false, bool socketShutdownSend=false, bool socketDisconnect=false, bool isSync=false) { throw null; }
    }
    public partial class TcpListener : Microsoft.AspNetCore.Server.Kestrel.Internal.Http.Listener
    {
        public TcpListener(Microsoft.AspNetCore.Server.Kestrel.Internal.ServiceContext serviceContext) : base (default(Microsoft.AspNetCore.Server.Kestrel.Internal.ServiceContext)) { }
        protected override Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle CreateListenSocket() { throw null; }
        protected override void OnConnection(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle listenSocket, int status) { }
    }
    public partial class TcpListenerPrimary : Microsoft.AspNetCore.Server.Kestrel.Internal.Http.ListenerPrimary
    {
        public TcpListenerPrimary(Microsoft.AspNetCore.Server.Kestrel.Internal.ServiceContext serviceContext) : base (default(Microsoft.AspNetCore.Server.Kestrel.Internal.ServiceContext)) { }
        protected override Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle CreateListenSocket() { throw null; }
        protected override void OnConnection(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle listenSocket, int status) { }
    }
    public partial class TcpListenerSecondary : Microsoft.AspNetCore.Server.Kestrel.Internal.Http.ListenerSecondary
    {
        public TcpListenerSecondary(Microsoft.AspNetCore.Server.Kestrel.Internal.ServiceContext serviceContext) : base (default(Microsoft.AspNetCore.Server.Kestrel.Internal.ServiceContext)) { }
        protected override Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle CreateAcceptSocket() { throw null; }
    }
    public partial class UrlPathDecoder
    {
        public UrlPathDecoder() { }
        public static Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator Unescape(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator start, Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator end) { throw null; }
    }
}
namespace Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure
{
    public partial interface IKestrelTrace : Microsoft.Extensions.Logging.ILogger
    {
        void ApplicationError(string connectionId, System.Exception ex);
        void ConnectionBadRequest(string connectionId, Microsoft.AspNetCore.Server.Kestrel.BadHttpRequestException ex);
        void ConnectionDisconnect(string connectionId);
        void ConnectionDisconnectedWrite(string connectionId, int count, System.Exception ex);
        void ConnectionError(string connectionId, System.Exception ex);
        void ConnectionKeepAlive(string connectionId);
        void ConnectionPause(string connectionId);
        void ConnectionRead(string connectionId, int count);
        void ConnectionReadFin(string connectionId);
        void ConnectionResume(string connectionId);
        void ConnectionStart(string connectionId);
        void ConnectionStop(string connectionId);
        void ConnectionWrite(string connectionId, int count);
        void ConnectionWriteCallback(string connectionId, int status);
        void ConnectionWriteFin(string connectionId);
        void ConnectionWroteFin(string connectionId, int status);
        void NotAllConnectionsClosedGracefully();
    }
    public partial interface IThreadPool
    {
        void Cancel(System.Threading.Tasks.TaskCompletionSource<object> tcs);
        void Complete(System.Threading.Tasks.TaskCompletionSource<object> tcs);
        void Error(System.Threading.Tasks.TaskCompletionSource<object> tcs, System.Exception ex);
        void Run(System.Action action);
    }
    public partial class LoggingThreadPool : Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IThreadPool
    {
        public LoggingThreadPool(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IKestrelTrace log) { }
        public void Cancel(System.Threading.Tasks.TaskCompletionSource<object> tcs) { }
        public void Complete(System.Threading.Tasks.TaskCompletionSource<object> tcs) { }
        public void Error(System.Threading.Tasks.TaskCompletionSource<object> tcs, System.Exception ex) { }
        public void Run(System.Action action) { }
    }
    public partial class MemoryPool : System.IDisposable
    {
        public const int MaxPooledBlockLength = 4032;
        public MemoryPool() { }
        public void Dispose() { }
        protected virtual void Dispose(bool disposing) { }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolBlock Lease() { throw null; }
        public void Return(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolBlock block) { }
    }
    public partial class MemoryPoolBlock
    {
        public System.ArraySegment<byte> Data;
        public readonly System.IntPtr DataArrayPtr;
        public int End;
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolBlock Next;
        public int Start;
        protected MemoryPoolBlock(System.IntPtr dataArrayPtr) { }
        public byte[] Array { get { throw null; } }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPool Pool { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolSlab Slab { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        ~MemoryPoolBlock() { }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator GetIterator() { throw null; }
        public void Reset() { }
        public override string ToString() { throw null; }
    }
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public partial struct MemoryPoolIterator
    {
        public MemoryPoolIterator(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolBlock block) { throw null;}
        public MemoryPoolIterator(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolBlock block, int index) { throw null;}
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolBlock Block { get { throw null; } }
        public int Index { get { throw null; } }
        public bool IsDefault { get { throw null; } }
        public bool IsEnd { get { throw null; } }
        public void CopyFrom(System.ArraySegment<byte> buffer) { }
        public void CopyFrom(byte[] data) { }
        public void CopyFrom(byte[] data, int offset, int count) { }
        public void CopyFromAscii(string data) { }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator CopyTo(byte[] array, int offset, int count, out int actual) { actual = default(int); throw null; }
        public int GetLength(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator end) { throw null; }
        public int Peek() { throw null; }
        public long PeekLong() { throw null; }
        public bool Put(byte data) { throw null; }
        public int Seek(ref System.Numerics.Vector<byte> byte0Vector) { throw null; }
        public int Seek(ref System.Numerics.Vector<byte> byte0Vector, ref System.Numerics.Vector<byte> byte1Vector) { throw null; }
        public int Seek(ref System.Numerics.Vector<byte> byte0Vector, ref System.Numerics.Vector<byte> byte1Vector, ref System.Numerics.Vector<byte> byte2Vector) { throw null; }
        public void Skip(int bytesToSkip) { }
        public int Take() { throw null; }
    }
    public static partial class MemoryPoolIteratorExtensions
    {
        public const string Http10Version = "HTTP/1.0";
        public const string Http11Version = "HTTP/1.1";
        public const string HttpConnectMethod = "CONNECT";
        public const string HttpDeleteMethod = "DELETE";
        public const string HttpGetMethod = "GET";
        public const string HttpHeadMethod = "HEAD";
        public const string HttpOptionsMethod = "OPTIONS";
        public const string HttpPatchMethod = "PATCH";
        public const string HttpPostMethod = "POST";
        public const string HttpPutMethod = "PUT";
        public const string HttpTraceMethod = "TRACE";
        public static System.ArraySegment<byte> GetArraySegment(this Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator start, Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator end) { throw null; }
        public static string GetAsciiString(this Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator start, Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator end) { throw null; }
        public static bool GetKnownMethod(this Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator begin, out string knownMethod) { knownMethod = default(string); throw null; }
        public static bool GetKnownVersion(this Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator begin, out string knownVersion) { knownVersion = default(string); throw null; }
        public static string GetUtf8String(this Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator start, Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator end) { throw null; }
    }
    public partial class MemoryPoolSlab : System.IDisposable
    {
        public byte[] Array;
        public System.IntPtr ArrayPtr;
        public bool IsActive;
        public MemoryPoolSlab() { }
        public static Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolSlab Create(int length) { throw null; }
        public void Dispose() { }
        protected virtual void Dispose(bool disposing) { }
        ~MemoryPoolSlab() { }
    }
    public static partial class TaskUtilities
    {
        public static System.Threading.Tasks.Task CompletedTask;
        public static System.Threading.Tasks.Task<int> ZeroTask;
        public static System.Threading.Tasks.Task GetCancelledTask(System.Threading.CancellationToken cancellationToken) { throw null; }
        public static System.Threading.Tasks.Task<int> GetCancelledZeroTask(System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken)) { throw null; }
    }
}
namespace Microsoft.AspNetCore.Server.Kestrel.Internal.Networking
{
    public partial class Libuv
    {
        protected System.Func<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle, int> _uv_accept;
        protected System.Func<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvLoopHandle, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvAsyncHandle, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_async_cb, int> _uv_async_init;
        protected System.Func<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvAsyncHandle, int> _uv_async_send;
        protected System.Action<System.IntPtr, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_close_cb> _uv_close;
        protected System.Func<int, System.IntPtr> _uv_err_name;
        protected Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_fileno_func _uv_fileno;
        protected System.Func<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.HandleType, int> _uv_handle_size;
        protected Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_ip4_addr_func _uv_ip4_addr;
        protected Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_ip6_addr_func _uv_ip6_addr;
        protected System.Func<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle, int, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_connection_cb, int> _uv_listen;
        protected System.Func<System.IntPtr, int> _uv_loop_close;
        protected System.Func<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvLoopHandle, int> _uv_loop_init;
        protected System.Func<int> _uv_loop_size;
        protected System.Func<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvPipeHandle, string, int> _uv_pipe_bind;
        protected System.Action<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvConnectRequest, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvPipeHandle, string, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_connect_cb> _uv_pipe_connect;
        protected System.Func<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvLoopHandle, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvPipeHandle, int, int> _uv_pipe_init;
        protected System.Func<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvPipeHandle, int> _uv_pipe_pending_count;
        protected System.Func<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_alloc_cb, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_read_cb, int> _uv_read_start;
        protected System.Func<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle, int> _uv_read_stop;
        protected System.Action<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvHandle> _uv_ref;
        protected System.Func<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.RequestType, int> _uv_req_size;
        protected System.Func<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvLoopHandle, int, int> _uv_run;
        protected System.Func<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvShutdownReq, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_shutdown_cb, int> _uv_shutdown;
        protected System.Action<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvLoopHandle> _uv_stop;
        protected System.Func<int, System.IntPtr> _uv_strerror;
        protected Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_tcp_bind_func _uv_tcp_bind;
        protected Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_tcp_getpeername_func _uv_tcp_getpeername;
        protected Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_tcp_getsockname_func _uv_tcp_getsockname;
        protected System.Func<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvLoopHandle, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvTcpHandle, int> _uv_tcp_init;
        protected System.Func<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvTcpHandle, int, int> _uv_tcp_nodelay;
        protected System.Func<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvTcpHandle, System.IntPtr, int> _uv_tcp_open;
        protected System.Func<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_buf_t[], int, int> _uv_try_write;
        protected System.Action<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvHandle> _uv_unref;
        protected System.Func<System.IntPtr, int> _uv_unsafe_async_send;
        protected System.Func<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvLoopHandle, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_walk_cb, System.IntPtr, int> _uv_walk;
        protected Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_write_func _uv_write;
        protected Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_write2_func _uv_write2;
        public readonly bool IsWindows;
        public Libuv() { }
        public void accept(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle server, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle client) { }
        public void async_init(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvLoopHandle loop, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvAsyncHandle handle, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_async_cb cb) { }
        public void async_send(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvAsyncHandle handle) { }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_buf_t buf_init(System.IntPtr memory, int len) { throw null; }
        public int Check(int statusCode) { throw null; }
        public int Check(int statusCode, out System.Exception error) { error = default(System.Exception); throw null; }
        public void close(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvHandle handle, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_close_cb close_cb) { }
        public void close(System.IntPtr handle, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_close_cb close_cb) { }
        public string err_name(int err) { throw null; }
        public int handle_size(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.HandleType handleType) { throw null; }
        public int ip4_addr(string ip, int port, out Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.SockAddr addr, out System.Exception error) { addr = default(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.SockAddr); error = default(System.Exception); throw null; }
        public int ip6_addr(string ip, int port, out Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.SockAddr addr, out System.Exception error) { addr = default(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.SockAddr); error = default(System.Exception); throw null; }
        public void listen(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle handle, int backlog, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_connection_cb cb) { }
        public void loop_close(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvLoopHandle handle) { }
        public void loop_init(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvLoopHandle handle) { }
        public int loop_size() { throw null; }
        public void pipe_bind(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvPipeHandle handle, string name) { }
        public void pipe_connect(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvConnectRequest req, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvPipeHandle handle, string name, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_connect_cb cb) { }
        public void pipe_init(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvLoopHandle loop, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvPipeHandle handle, bool ipc) { }
        public int pipe_pending_count(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvPipeHandle handle) { throw null; }
        public void read_start(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle handle, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_alloc_cb alloc_cb, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_read_cb read_cb) { }
        public void read_stop(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle handle) { }
        public void @ref(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvHandle handle) { }
        public int req_size(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.RequestType reqType) { throw null; }
        public int run(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvLoopHandle handle, int mode) { throw null; }
        public void shutdown(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvShutdownReq req, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle handle, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_shutdown_cb cb) { }
        public void stop(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvLoopHandle handle) { }
        public string strerror(int err) { throw null; }
        public void tcp_bind(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvTcpHandle handle, ref Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.SockAddr addr, int flags) { }
        public void tcp_getpeername(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvTcpHandle handle, out Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.SockAddr addr, ref int namelen) { addr = default(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.SockAddr); }
        public void tcp_getsockname(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvTcpHandle handle, out Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.SockAddr addr, ref int namelen) { addr = default(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.SockAddr); }
        public void tcp_init(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvLoopHandle loop, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvTcpHandle handle) { }
        public void tcp_nodelay(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvTcpHandle handle, bool enable) { }
        public void tcp_open(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvTcpHandle handle, System.IntPtr hSocket) { }
        public int try_write(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle handle, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_buf_t[] bufs, int nbufs) { throw null; }
        public void unref(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvHandle handle) { }
        public void unsafe_async_send(System.IntPtr handle) { }
        public int uv_fileno(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvHandle handle, ref System.IntPtr socket) { throw null; }
        public void walk(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvLoopHandle loop, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_walk_cb walk_cb, System.IntPtr arg) { }
        public unsafe void write(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvRequest req, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle handle, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_buf_t* bufs, int nbufs, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_write_cb cb) { }
        public unsafe void write2(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvRequest req, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle handle, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_buf_t* bufs, int nbufs, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle sendHandle, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_write_cb cb) { }
        public enum HandleType
        {
            ASYNC = 1,
            CHECK = 2,
            FS_EVENT = 3,
            FS_POLL = 4,
            HANDLE = 5,
            IDLE = 6,
            NAMED_PIPE = 7,
            POLL = 8,
            PREPARE = 9,
            PROCESS = 10,
            SIGNAL = 16,
            STREAM = 11,
            TCP = 12,
            TIMER = 13,
            TTY = 14,
            UDP = 15,
            Unknown = 0,
        }
        public enum RequestType
        {
            CONNECT = 2,
            FS = 6,
            GETADDRINFO = 8,
            GETNAMEINFO = 9,
            REQ = 1,
            SHUTDOWN = 4,
            UDP_SEND = 5,
            Unknown = 0,
            WORK = 7,
            WRITE = 3,
        }
        [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute((System.Runtime.InteropServices.CallingConvention)(2))]
        public delegate void uv_alloc_cb(System.IntPtr server, int suggested_size, out Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_buf_t buf);
        [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute((System.Runtime.InteropServices.CallingConvention)(2))]
        public delegate void uv_async_cb(System.IntPtr handle);
        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public partial struct uv_buf_t
        {
            public uv_buf_t(System.IntPtr memory, int len, bool IsWindows) { throw null;}
        }
        [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute((System.Runtime.InteropServices.CallingConvention)(2))]
        public delegate void uv_close_cb(System.IntPtr handle);
        [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute((System.Runtime.InteropServices.CallingConvention)(2))]
        public delegate void uv_connect_cb(System.IntPtr req, int status);
        [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute((System.Runtime.InteropServices.CallingConvention)(2))]
        public delegate void uv_connection_cb(System.IntPtr server, int status);
        [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute((System.Runtime.InteropServices.CallingConvention)(2))]
        protected delegate int uv_fileno_func(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvHandle handle, ref System.IntPtr socket);
        protected delegate int uv_ip4_addr_func(string ip, int port, out Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.SockAddr addr);
        protected delegate int uv_ip6_addr_func(string ip, int port, out Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.SockAddr addr);
        [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute((System.Runtime.InteropServices.CallingConvention)(2))]
        public delegate void uv_read_cb(System.IntPtr server, int nread, ref Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_buf_t buf);
        [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute((System.Runtime.InteropServices.CallingConvention)(2))]
        public delegate void uv_shutdown_cb(System.IntPtr req, int status);
        protected delegate int uv_tcp_bind_func(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvTcpHandle handle, ref Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.SockAddr addr, int flags);
        public delegate int uv_tcp_getpeername_func(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvTcpHandle handle, out Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.SockAddr addr, ref int namelen);
        public delegate int uv_tcp_getsockname_func(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvTcpHandle handle, out Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.SockAddr addr, ref int namelen);
        [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute((System.Runtime.InteropServices.CallingConvention)(2))]
        public delegate void uv_walk_cb(System.IntPtr handle, System.IntPtr arg);
        [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute((System.Runtime.InteropServices.CallingConvention)(2))]
        public delegate void uv_write_cb(System.IntPtr req, int status);
        protected unsafe delegate int uv_write_func(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvRequest req, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle handle, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_buf_t* bufs, int nbufs, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_write_cb cb);
        protected unsafe delegate int uv_write2_func(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvRequest req, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle handle, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_buf_t* bufs, int nbufs, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle sendHandle, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_write_cb cb);
    }
    public static partial class PlatformApis
    {
        public static bool IsDarwin { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public static bool IsWindows { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
    }
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public partial struct SockAddr
    {
        public SockAddr(long ignored) { throw null;}
        public System.Net.IPEndPoint GetIPEndPoint() { throw null; }
    }
    public partial class UvAsyncHandle : Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvHandle
    {
        public UvAsyncHandle(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IKestrelTrace logger) : base (default(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IKestrelTrace)) { }
        public void Init(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvLoopHandle loop, System.Action callback, System.Action<System.Action<System.IntPtr>, System.IntPtr> queueCloseHandle) { }
        protected override bool ReleaseHandle() { throw null; }
        public void Send() { }
    }
    public partial class UvConnectRequest : Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvRequest
    {
        public UvConnectRequest(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IKestrelTrace logger) : base (default(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IKestrelTrace)) { }
        public void Connect(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvPipeHandle pipe, string name, System.Action<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvConnectRequest, int, System.Exception, object> callback, object state) { }
        public void Init(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvLoopHandle loop) { }
    }
    public partial class UvException : System.Exception
    {
        public UvException(string message, int statusCode) { }
        public int StatusCode { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
    }
    public abstract partial class UvHandle : Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvMemory
    {
        protected UvHandle(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IKestrelTrace logger) : base (default(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IKestrelTrace)) { }
        protected void CreateHandle(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv uv, int threadId, int size, System.Action<System.Action<System.IntPtr>, System.IntPtr> queueCloseHandle) { }
        public void Reference() { }
        protected override bool ReleaseHandle() { throw null; }
        public void Unreference() { }
    }
    public partial class UvLoopHandle : Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvMemory
    {
        public UvLoopHandle(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IKestrelTrace logger) : base (default(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IKestrelTrace)) { }
        public void Init(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv uv) { }
        protected override bool ReleaseHandle() { throw null; }
        public int Run(int mode=0) { throw null; }
        public void Stop() { }
    }
    public abstract partial class UvMemory : System.Runtime.InteropServices.SafeHandle
    {
        protected readonly Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IKestrelTrace _log;
        protected int _threadId;
        protected Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv _uv;
        protected UvMemory(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IKestrelTrace logger) : base (default(System.IntPtr), default(bool)) { }
        public override bool IsInvalid { get { throw null; } }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv Libuv { get { throw null; } }
        public int ThreadId { get { throw null; } }
        protected void CreateMemory(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv uv, int threadId, int size) { }
        protected static void DestroyMemory(System.IntPtr memory) { }
        protected static void DestroyMemory(System.IntPtr memory, System.IntPtr gcHandlePtr) { }
        public static THandle FromIntPtr<THandle>(System.IntPtr handle) { throw null; }
        public void Validate(bool closed=false) { }
    }
    public partial class UvPipeHandle : Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle
    {
        public UvPipeHandle(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IKestrelTrace logger) : base (default(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IKestrelTrace)) { }
        public void Bind(string name) { }
        public void Init(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvLoopHandle loop, System.Action<System.Action<System.IntPtr>, System.IntPtr> queueCloseHandle, bool ipc=false) { }
        public int PendingCount() { throw null; }
    }
    public partial class UvRequest : Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvMemory
    {
        protected UvRequest(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IKestrelTrace logger) : base (default(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IKestrelTrace)) { }
        public virtual void Pin() { }
        protected override bool ReleaseHandle() { throw null; }
        public virtual void Unpin() { }
    }
    public partial class UvShutdownReq : Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvRequest
    {
        public UvShutdownReq(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IKestrelTrace logger) : base (default(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IKestrelTrace)) { }
        public void Init(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvLoopHandle loop) { }
        public void Shutdown(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle handle, System.Action<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvShutdownReq, int, object> callback, object state) { }
    }
    public abstract partial class UvStreamHandle : Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvHandle
    {
        protected UvStreamHandle(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IKestrelTrace logger) : base (default(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IKestrelTrace)) { }
        public Microsoft.AspNetCore.Server.Kestrel.Internal.Http.Connection Connection { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public void Accept(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle handle) { }
        public void Listen(int backlog, System.Action<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle, int, System.Exception, object> callback, object state) { }
        public void ReadStart(System.Func<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle, int, object, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_buf_t> allocCallback, System.Action<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle, int, object> readCallback, object state) { }
        public void ReadStop() { }
        protected override bool ReleaseHandle() { throw null; }
        public int TryWrite(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.Libuv.uv_buf_t buf) { throw null; }
    }
    public partial class UvTcpHandle : Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle
    {
        public UvTcpHandle(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IKestrelTrace logger) : base (default(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IKestrelTrace)) { }
        public void Bind(Microsoft.AspNetCore.Server.Kestrel.ServerAddress address) { }
        public static System.Net.IPEndPoint CreateIPEndpoint(Microsoft.AspNetCore.Server.Kestrel.ServerAddress address) { throw null; }
        public System.Net.IPEndPoint GetPeerIPEndPoint() { throw null; }
        public System.Net.IPEndPoint GetSockIPEndPoint() { throw null; }
        public void Init(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvLoopHandle loop, System.Action<System.Action<System.IntPtr>, System.IntPtr> queueCloseHandle) { }
        public void NoDelay(bool enable) { }
        public void Open(System.IntPtr hSocket) { }
    }
    public partial class UvWriteReq : Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvRequest
    {
        public UvWriteReq(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IKestrelTrace logger) : base (default(Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.IKestrelTrace)) { }
        public void Init(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvLoopHandle loop) { }
        public void Write(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle handle, Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator start, Microsoft.AspNetCore.Server.Kestrel.Internal.Infrastructure.MemoryPoolIterator end, int nBuffers, System.Action<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvWriteReq, int, System.Exception, object> callback, object state) { }
        public void Write2(Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle handle, System.ArraySegment<System.ArraySegment<byte>> bufs, Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvStreamHandle sendHandle, System.Action<Microsoft.AspNetCore.Server.Kestrel.Internal.Networking.UvWriteReq, int, System.Exception, object> callback, object state) { }
    }
}
