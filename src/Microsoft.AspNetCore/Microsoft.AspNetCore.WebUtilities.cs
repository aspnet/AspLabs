namespace Microsoft.AspNetCore.WebUtilities
{
    public partial class FileBufferingReadStream : System.IO.Stream
    {
        public FileBufferingReadStream(System.IO.Stream inner, int memoryThreshold, System.Nullable<long> bufferLimit, System.Func<string> tempFileDirectoryAccessor) { }
        public FileBufferingReadStream(System.IO.Stream inner, int memoryThreshold, System.Nullable<long> bufferLimit, System.Func<string> tempFileDirectoryAccessor, System.Buffers.ArrayPool<byte> bytePool) { }
        public FileBufferingReadStream(System.IO.Stream inner, int memoryThreshold, System.Nullable<long> bufferLimit, string tempFileDirectory) { }
        public FileBufferingReadStream(System.IO.Stream inner, int memoryThreshold, System.Nullable<long> bufferLimit, string tempFileDirectory, System.Buffers.ArrayPool<byte> bytePool) { }
        public override bool CanRead { get { throw null; } }
        public override bool CanSeek { get { throw null; } }
        public override bool CanWrite { get { throw null; } }
        public bool InMemory { get { throw null; } }
        public override long Length { get { throw null; } }
        public override long Position { get { throw null; } set { } }
        public string TempFileName { get { throw null; } }
        protected override void Dispose(bool disposing) { }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) { throw null; }
        public override System.Threading.Tasks.Task<int> ReadAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken) { throw null; }
        public override long Seek(long offset, System.IO.SeekOrigin origin) { throw null; }
        public override void SetLength(long value) { }
        public override void Write(byte[] buffer, int offset, int count) { }
        public override System.Threading.Tasks.Task WriteAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken) { throw null; }
    }
    public partial class FormReader : System.IDisposable
    {
        public const int DefaultKeyLengthLimit = 2048;
        public const int DefaultValueCountLimit = 1024;
        public const int DefaultValueLengthLimit = 4194304;
        public FormReader(System.IO.Stream stream) { }
        public FormReader(System.IO.Stream stream, System.Text.Encoding encoding) { }
        public FormReader(System.IO.Stream stream, System.Text.Encoding encoding, System.Buffers.ArrayPool<char> charPool) { }
        public FormReader(string data) { }
        public FormReader(string data, System.Buffers.ArrayPool<char> charPool) { }
        public int KeyLengthLimit { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public int ValueCountLimit { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public int ValueLengthLimit { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public void Dispose() { }
        public System.Collections.Generic.Dictionary<string, Microsoft.Extensions.Primitives.StringValues> ReadForm() { throw null; }
        public System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<string, Microsoft.Extensions.Primitives.StringValues>> ReadFormAsync(System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken)) { throw null; }
        public System.Nullable<System.Collections.Generic.KeyValuePair<string, string>> ReadNextPair() { throw null; }
        public System.Threading.Tasks.Task<System.Nullable<System.Collections.Generic.KeyValuePair<string, string>>> ReadNextPairAsync(System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken)) { throw null; }
    }
    public partial class HttpRequestStreamReader : System.IO.TextReader
    {
        public HttpRequestStreamReader(System.IO.Stream stream, System.Text.Encoding encoding) { }
        public HttpRequestStreamReader(System.IO.Stream stream, System.Text.Encoding encoding, int bufferSize) { }
        public HttpRequestStreamReader(System.IO.Stream stream, System.Text.Encoding encoding, int bufferSize, System.Buffers.ArrayPool<byte> bytePool, System.Buffers.ArrayPool<char> charPool) { }
        protected override void Dispose(bool disposing) { }
        public override int Peek() { throw null; }
        public override int Read() { throw null; }
        public override int Read(char[] buffer, int index, int count) { throw null; }
        public override System.Threading.Tasks.Task<int> ReadAsync(char[] buffer, int index, int count) { throw null; }
    }
    public partial class HttpResponseStreamWriter : System.IO.TextWriter
    {
        public const int DefaultBufferSize = 1024;
        public HttpResponseStreamWriter(System.IO.Stream stream, System.Text.Encoding encoding) { }
        public HttpResponseStreamWriter(System.IO.Stream stream, System.Text.Encoding encoding, int bufferSize) { }
        public HttpResponseStreamWriter(System.IO.Stream stream, System.Text.Encoding encoding, int bufferSize, System.Buffers.ArrayPool<byte> bytePool, System.Buffers.ArrayPool<char> charPool) { }
        public override System.Text.Encoding Encoding { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        protected override void Dispose(bool disposing) { }
        public override void Flush() { }
        public override System.Threading.Tasks.Task FlushAsync() { throw null; }
        public override void Write(char value) { }
        public override void Write(char[] values, int index, int count) { }
        public override void Write(string value) { }
        public override System.Threading.Tasks.Task WriteAsync(char value) { throw null; }
        public override System.Threading.Tasks.Task WriteAsync(char[] values, int index, int count) { throw null; }
        public override System.Threading.Tasks.Task WriteAsync(string value) { throw null; }
    }
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public partial struct KeyValueAccumulator
    {
        public bool HasValues { get { throw null; } }
        public int KeyCount { get { throw null; } }
        public int ValueCount { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public void Append(string key, string value) { }
        public System.Collections.Generic.Dictionary<string, Microsoft.Extensions.Primitives.StringValues> GetResults() { throw null; }
    }
    public partial class MultipartReader
    {
        public const int DefaultHeadersCountLimit = 16;
        public const int DefaultHeadersLengthLimit = 16384;
        public MultipartReader(string boundary, System.IO.Stream stream) { }
        public MultipartReader(string boundary, System.IO.Stream stream, int bufferSize) { }
        public System.Nullable<long> BodyLengthLimit { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public int HeadersCountLimit { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public int HeadersLengthLimit { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public System.Threading.Tasks.Task<Microsoft.AspNetCore.WebUtilities.MultipartSection> ReadNextSectionAsync(System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken)) { throw null; }
    }
    public partial class MultipartSection
    {
        public MultipartSection() { }
        public System.Nullable<long> BaseStreamOffset { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public System.IO.Stream Body { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public string ContentDisposition { get { throw null; } }
        public string ContentType { get { throw null; } }
        public System.Collections.Generic.Dictionary<string, Microsoft.Extensions.Primitives.StringValues> Headers { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    }
    public static partial class QueryHelpers
    {
        public static string AddQueryString(string uri, System.Collections.Generic.IDictionary<string, string> queryString) { throw null; }
        public static string AddQueryString(string uri, string name, string value) { throw null; }
        public static System.Collections.Generic.Dictionary<string, Microsoft.Extensions.Primitives.StringValues> ParseNullableQuery(string queryString) { throw null; }
        public static System.Collections.Generic.Dictionary<string, Microsoft.Extensions.Primitives.StringValues> ParseQuery(string queryString) { throw null; }
    }
    public static partial class ReasonPhrases
    {
        public static string GetReasonPhrase(int statusCode) { throw null; }
    }
    public static partial class StreamHelperExtensions
    {
        public static System.Threading.Tasks.Task DrainAsync(this System.IO.Stream stream, System.Buffers.ArrayPool<byte> bytePool, System.Nullable<long> limit, System.Threading.CancellationToken cancellationToken) { throw null; }
        public static System.Threading.Tasks.Task DrainAsync(this System.IO.Stream stream, System.Nullable<long> limit, System.Threading.CancellationToken cancellationToken) { throw null; }
        public static System.Threading.Tasks.Task DrainAsync(this System.IO.Stream stream, System.Threading.CancellationToken cancellationToken) { throw null; }
    }
    public static partial class WebEncoders
    {
        public static byte[] Base64UrlDecode(string input) { throw null; }
        public static byte[] Base64UrlDecode(string input, int offset, char[] buffer, int bufferOffset, int count) { throw null; }
        public static byte[] Base64UrlDecode(string input, int offset, int count) { throw null; }
        public static string Base64UrlEncode(byte[] input) { throw null; }
        public static int Base64UrlEncode(byte[] input, int offset, char[] output, int outputOffset, int count) { throw null; }
        public static string Base64UrlEncode(byte[] input, int offset, int count) { throw null; }
        public static int GetArraySizeRequiredToDecode(int count) { throw null; }
        public static int GetArraySizeRequiredToEncode(int count) { throw null; }
    }
}
