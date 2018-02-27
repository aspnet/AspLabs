namespace Microsoft.Net.Http.Headers
{
    public partial class CacheControlHeaderValue
    {
        public CacheControlHeaderValue() { }
        public System.Collections.Generic.IList<Microsoft.Net.Http.Headers.NameValueHeaderValue> Extensions { get { throw null; } }
        public System.Nullable<System.TimeSpan> MaxAge { get { throw null; } set { } }
        public bool MaxStale { get { throw null; } set { } }
        public System.Nullable<System.TimeSpan> MaxStaleLimit { get { throw null; } set { } }
        public System.Nullable<System.TimeSpan> MinFresh { get { throw null; } set { } }
        public bool MustRevalidate { get { throw null; } set { } }
        public bool NoCache { get { throw null; } set { } }
        public System.Collections.Generic.ICollection<string> NoCacheHeaders { get { throw null; } }
        public bool NoStore { get { throw null; } set { } }
        public bool NoTransform { get { throw null; } set { } }
        public bool OnlyIfCached { get { throw null; } set { } }
        public bool Private { get { throw null; } set { } }
        public System.Collections.Generic.ICollection<string> PrivateHeaders { get { throw null; } }
        public bool ProxyRevalidate { get { throw null; } set { } }
        public bool Public { get { throw null; } set { } }
        public System.Nullable<System.TimeSpan> SharedMaxAge { get { throw null; } set { } }
        public override bool Equals(object obj) { throw null; }
        public override int GetHashCode() { throw null; }
        public static Microsoft.Net.Http.Headers.CacheControlHeaderValue Parse(string input) { throw null; }
        public override string ToString() { throw null; }
        public static bool TryParse(string input, out Microsoft.Net.Http.Headers.CacheControlHeaderValue parsedValue) { parsedValue = default(Microsoft.Net.Http.Headers.CacheControlHeaderValue); throw null; }
    }
    public partial class ContentDispositionHeaderValue
    {
        public ContentDispositionHeaderValue(string dispositionType) { }
        public System.Nullable<System.DateTimeOffset> CreationDate { get { throw null; } set { } }
        public string DispositionType { get { throw null; } set { } }
        public string FileName { get { throw null; } set { } }
        public string FileNameStar { get { throw null; } set { } }
        public System.Nullable<System.DateTimeOffset> ModificationDate { get { throw null; } set { } }
        public string Name { get { throw null; } set { } }
        public System.Collections.Generic.IList<Microsoft.Net.Http.Headers.NameValueHeaderValue> Parameters { get { throw null; } }
        public System.Nullable<System.DateTimeOffset> ReadDate { get { throw null; } set { } }
        public System.Nullable<long> Size { get { throw null; } set { } }
        public override bool Equals(object obj) { throw null; }
        public override int GetHashCode() { throw null; }
        public static Microsoft.Net.Http.Headers.ContentDispositionHeaderValue Parse(string input) { throw null; }
        public void SetHttpFileName(string fileName) { }
        public void SetMimeFileName(string fileName) { }
        public override string ToString() { throw null; }
        public static bool TryParse(string input, out Microsoft.Net.Http.Headers.ContentDispositionHeaderValue parsedValue) { parsedValue = default(Microsoft.Net.Http.Headers.ContentDispositionHeaderValue); throw null; }
    }
    public partial class ContentRangeHeaderValue
    {
        public ContentRangeHeaderValue(long length) { }
        public ContentRangeHeaderValue(long from, long to) { }
        public ContentRangeHeaderValue(long from, long to, long length) { }
        public System.Nullable<long> From { get { throw null; } }
        public bool HasLength { get { throw null; } }
        public bool HasRange { get { throw null; } }
        public System.Nullable<long> Length { get { throw null; } }
        public System.Nullable<long> To { get { throw null; } }
        public string Unit { get { throw null; } set { } }
        public override bool Equals(object obj) { throw null; }
        public override int GetHashCode() { throw null; }
        public static Microsoft.Net.Http.Headers.ContentRangeHeaderValue Parse(string input) { throw null; }
        public override string ToString() { throw null; }
        public static bool TryParse(string input, out Microsoft.Net.Http.Headers.ContentRangeHeaderValue parsedValue) { parsedValue = default(Microsoft.Net.Http.Headers.ContentRangeHeaderValue); throw null; }
    }
    public partial class CookieHeaderValue
    {
        public CookieHeaderValue(string name) { }
        public CookieHeaderValue(string name, string value) { }
        public string Name { get { throw null; } set { } }
        public string Value { get { throw null; } set { } }
        public override bool Equals(object obj) { throw null; }
        public override int GetHashCode() { throw null; }
        public static Microsoft.Net.Http.Headers.CookieHeaderValue Parse(string input) { throw null; }
        public static System.Collections.Generic.IList<Microsoft.Net.Http.Headers.CookieHeaderValue> ParseList(System.Collections.Generic.IList<string> inputs) { throw null; }
        public static System.Collections.Generic.IList<Microsoft.Net.Http.Headers.CookieHeaderValue> ParseStrictList(System.Collections.Generic.IList<string> inputs) { throw null; }
        public override string ToString() { throw null; }
        public static bool TryParse(string input, out Microsoft.Net.Http.Headers.CookieHeaderValue parsedValue) { parsedValue = default(Microsoft.Net.Http.Headers.CookieHeaderValue); throw null; }
        public static bool TryParseList(System.Collections.Generic.IList<string> inputs, out System.Collections.Generic.IList<Microsoft.Net.Http.Headers.CookieHeaderValue> parsedValues) { parsedValues = default(System.Collections.Generic.IList<Microsoft.Net.Http.Headers.CookieHeaderValue>); throw null; }
        public static bool TryParseStrictList(System.Collections.Generic.IList<string> inputs, out System.Collections.Generic.IList<Microsoft.Net.Http.Headers.CookieHeaderValue> parsedValues) { parsedValues = default(System.Collections.Generic.IList<Microsoft.Net.Http.Headers.CookieHeaderValue>); throw null; }
    }
    public partial class EntityTagHeaderValue
    {
        public EntityTagHeaderValue(string tag) { }
        public EntityTagHeaderValue(string tag, bool isWeak) { }
        public static Microsoft.Net.Http.Headers.EntityTagHeaderValue Any { get { throw null; } }
        public bool IsWeak { get { throw null; } }
        public string Tag { get { throw null; } }
        public override bool Equals(object obj) { throw null; }
        public override int GetHashCode() { throw null; }
        public static Microsoft.Net.Http.Headers.EntityTagHeaderValue Parse(string input) { throw null; }
        public static System.Collections.Generic.IList<Microsoft.Net.Http.Headers.EntityTagHeaderValue> ParseList(System.Collections.Generic.IList<string> inputs) { throw null; }
        public static System.Collections.Generic.IList<Microsoft.Net.Http.Headers.EntityTagHeaderValue> ParseStrictList(System.Collections.Generic.IList<string> inputs) { throw null; }
        public override string ToString() { throw null; }
        public static bool TryParse(string input, out Microsoft.Net.Http.Headers.EntityTagHeaderValue parsedValue) { parsedValue = default(Microsoft.Net.Http.Headers.EntityTagHeaderValue); throw null; }
        public static bool TryParseList(System.Collections.Generic.IList<string> inputs, out System.Collections.Generic.IList<Microsoft.Net.Http.Headers.EntityTagHeaderValue> parsedValues) { parsedValues = default(System.Collections.Generic.IList<Microsoft.Net.Http.Headers.EntityTagHeaderValue>); throw null; }
        public static bool TryParseStrictList(System.Collections.Generic.IList<string> inputs, out System.Collections.Generic.IList<Microsoft.Net.Http.Headers.EntityTagHeaderValue> parsedValues) { parsedValues = default(System.Collections.Generic.IList<Microsoft.Net.Http.Headers.EntityTagHeaderValue>); throw null; }
    }
    public static partial class HeaderNames
    {
        public const string Accept = "Accept";
        public const string AcceptCharset = "Accept-Charset";
        public const string AcceptEncoding = "Accept-Encoding";
        public const string AcceptLanguage = "Accept-Language";
        public const string AcceptRanges = "Accept-Ranges";
        public const string Age = "Age";
        public const string Allow = "Allow";
        public const string Authorization = "Authorization";
        public const string CacheControl = "Cache-Control";
        public const string Connection = "Connection";
        public const string ContentDisposition = "Content-Disposition";
        public const string ContentEncoding = "Content-Encoding";
        public const string ContentLanguage = "Content-Language";
        public const string ContentLength = "Content-Length";
        public const string ContentLocation = "Content-Location";
        public const string ContentMD5 = "ContentMD5";
        public const string ContentRange = "Content-Range";
        public const string ContentType = "Content-Type";
        public const string Cookie = "Cookie";
        public const string Date = "Date";
        public const string ETag = "ETag";
        public const string Expect = "Expect";
        public const string Expires = "Expires";
        public const string From = "From";
        public const string Host = "Host";
        public const string IfMatch = "If-Match";
        public const string IfModifiedSince = "If-Modified-Since";
        public const string IfNoneMatch = "If-None-Match";
        public const string IfRange = "If-Range";
        public const string IfUnmodifiedSince = "If-Unmodified-Since";
        public const string LastModified = "Last-Modified";
        public const string Location = "Location";
        public const string MaxForwards = "Max-Forwards";
        public const string Pragma = "Pragma";
        public const string ProxyAuthenticate = "Proxy-Authenticate";
        public const string ProxyAuthorization = "Proxy-Authorization";
        public const string Range = "Range";
        public const string Referer = "Referer";
        public const string RefererPolicy = "Referrer-Policy";
        public const string RetryAfter = "Retry-After";
        public const string Server = "Server";
        public const string SetCookie = "Set-Cookie";
        public const string StrictTransportSecurity = "Strict-Transport-Security";
        public const string TE = "TE";
        public const string Trailer = "Trailer";
        public const string TransferEncoding = "Transfer-Encoding";
        public const string Upgrade = "Upgrade";
        public const string UserAgent = "User-Agent";
        public const string Vary = "Vary";
        public const string Via = "Via";
        public const string Warning = "Warning";
        public const string WebSocketSubProtocols = "Sec-WebSocket-Protocol";
        public const string WWWAuthenticate = "WWW-Authenticate";
        public const string XContentTypeOptions = "X-Content-Type-Options";
        public const string XFrameOptions = "X-Frame-Options";
        public const string XXssProtection = "X-XSS-Protection";
    }
    public static partial class HeaderQuality
    {
        public const double Match = 1;
        public const double NoMatch = 0;
    }
    public static partial class HeaderUtilities
    {
        public static string FormatDate(System.DateTimeOffset dateTime) { throw null; }
        public static string FormatInt64(long value) { throw null; }
        public static string RemoveQuotes(string input) { throw null; }
        public static bool TryParseDate(string input, out System.DateTimeOffset result) { result = default(System.DateTimeOffset); throw null; }
        public static bool TryParseInt64(string value, out long result) { result = default(long); throw null; }
    }
    public partial class MediaTypeHeaderValue
    {
        public MediaTypeHeaderValue(string mediaType) { }
        public MediaTypeHeaderValue(string mediaType, double quality) { }
        public string Boundary { get { throw null; } set { } }
        public string Charset { get { throw null; } set { } }
        public System.Text.Encoding Encoding { get { throw null; } set { } }
        public bool IsReadOnly { get { throw null; } }
        public bool MatchesAllSubTypes { get { throw null; } }
        public bool MatchesAllTypes { get { throw null; } }
        public string MediaType { get { throw null; } set { } }
        public System.Collections.Generic.IList<Microsoft.Net.Http.Headers.NameValueHeaderValue> Parameters { get { throw null; } }
        public System.Nullable<double> Quality { get { throw null; } set { } }
        public string SubType { get { throw null; } }
        public string Type { get { throw null; } }
        public Microsoft.Net.Http.Headers.MediaTypeHeaderValue Copy() { throw null; }
        public Microsoft.Net.Http.Headers.MediaTypeHeaderValue CopyAsReadOnly() { throw null; }
        public override bool Equals(object obj) { throw null; }
        public override int GetHashCode() { throw null; }
        public bool IsSubsetOf(Microsoft.Net.Http.Headers.MediaTypeHeaderValue otherMediaType) { throw null; }
        public static Microsoft.Net.Http.Headers.MediaTypeHeaderValue Parse(string input) { throw null; }
        public static System.Collections.Generic.IList<Microsoft.Net.Http.Headers.MediaTypeHeaderValue> ParseList(System.Collections.Generic.IList<string> inputs) { throw null; }
        public static System.Collections.Generic.IList<Microsoft.Net.Http.Headers.MediaTypeHeaderValue> ParseStrictList(System.Collections.Generic.IList<string> inputs) { throw null; }
        public override string ToString() { throw null; }
        public static bool TryParse(string input, out Microsoft.Net.Http.Headers.MediaTypeHeaderValue parsedValue) { parsedValue = default(Microsoft.Net.Http.Headers.MediaTypeHeaderValue); throw null; }
        public static bool TryParseList(System.Collections.Generic.IList<string> inputs, out System.Collections.Generic.IList<Microsoft.Net.Http.Headers.MediaTypeHeaderValue> parsedValues) { parsedValues = default(System.Collections.Generic.IList<Microsoft.Net.Http.Headers.MediaTypeHeaderValue>); throw null; }
        public static bool TryParseStrictList(System.Collections.Generic.IList<string> inputs, out System.Collections.Generic.IList<Microsoft.Net.Http.Headers.MediaTypeHeaderValue> parsedValues) { parsedValues = default(System.Collections.Generic.IList<Microsoft.Net.Http.Headers.MediaTypeHeaderValue>); throw null; }
    }
    public partial class MediaTypeHeaderValueComparer : System.Collections.Generic.IComparer<Microsoft.Net.Http.Headers.MediaTypeHeaderValue>
    {
        internal MediaTypeHeaderValueComparer() { }
        public static Microsoft.Net.Http.Headers.MediaTypeHeaderValueComparer QualityComparer { get { throw null; } }
        public int Compare(Microsoft.Net.Http.Headers.MediaTypeHeaderValue mediaType1, Microsoft.Net.Http.Headers.MediaTypeHeaderValue mediaType2) { throw null; }
    }
    public partial class NameValueHeaderValue
    {
        public NameValueHeaderValue(string name) { }
        public NameValueHeaderValue(string name, string value) { }
        public bool IsReadOnly { get { throw null; } }
        public string Name { get { throw null; } }
        public string Value { get { throw null; } set { } }
        public Microsoft.Net.Http.Headers.NameValueHeaderValue Copy() { throw null; }
        public Microsoft.Net.Http.Headers.NameValueHeaderValue CopyAsReadOnly() { throw null; }
        public override bool Equals(object obj) { throw null; }
        public static Microsoft.Net.Http.Headers.NameValueHeaderValue Find(System.Collections.Generic.IList<Microsoft.Net.Http.Headers.NameValueHeaderValue> values, string name) { throw null; }
        public override int GetHashCode() { throw null; }
        public static Microsoft.Net.Http.Headers.NameValueHeaderValue Parse(string input) { throw null; }
        public static System.Collections.Generic.IList<Microsoft.Net.Http.Headers.NameValueHeaderValue> ParseList(System.Collections.Generic.IList<string> input) { throw null; }
        public static System.Collections.Generic.IList<Microsoft.Net.Http.Headers.NameValueHeaderValue> ParseStrictList(System.Collections.Generic.IList<string> input) { throw null; }
        public override string ToString() { throw null; }
        public static bool TryParse(string input, out Microsoft.Net.Http.Headers.NameValueHeaderValue parsedValue) { parsedValue = default(Microsoft.Net.Http.Headers.NameValueHeaderValue); throw null; }
        public static bool TryParseList(System.Collections.Generic.IList<string> input, out System.Collections.Generic.IList<Microsoft.Net.Http.Headers.NameValueHeaderValue> parsedValues) { parsedValues = default(System.Collections.Generic.IList<Microsoft.Net.Http.Headers.NameValueHeaderValue>); throw null; }
        public static bool TryParseStrictList(System.Collections.Generic.IList<string> input, out System.Collections.Generic.IList<Microsoft.Net.Http.Headers.NameValueHeaderValue> parsedValues) { parsedValues = default(System.Collections.Generic.IList<Microsoft.Net.Http.Headers.NameValueHeaderValue>); throw null; }
    }
    public partial class RangeConditionHeaderValue
    {
        public RangeConditionHeaderValue(Microsoft.Net.Http.Headers.EntityTagHeaderValue entityTag) { }
        public RangeConditionHeaderValue(System.DateTimeOffset lastModified) { }
        public RangeConditionHeaderValue(string entityTag) { }
        public Microsoft.Net.Http.Headers.EntityTagHeaderValue EntityTag { get { throw null; } }
        public System.Nullable<System.DateTimeOffset> LastModified { get { throw null; } }
        public override bool Equals(object obj) { throw null; }
        public override int GetHashCode() { throw null; }
        public static Microsoft.Net.Http.Headers.RangeConditionHeaderValue Parse(string input) { throw null; }
        public override string ToString() { throw null; }
        public static bool TryParse(string input, out Microsoft.Net.Http.Headers.RangeConditionHeaderValue parsedValue) { parsedValue = default(Microsoft.Net.Http.Headers.RangeConditionHeaderValue); throw null; }
    }
    public partial class RangeHeaderValue
    {
        public RangeHeaderValue() { }
        public RangeHeaderValue(System.Nullable<long> from, System.Nullable<long> to) { }
        public System.Collections.Generic.ICollection<Microsoft.Net.Http.Headers.RangeItemHeaderValue> Ranges { get { throw null; } }
        public string Unit { get { throw null; } set { } }
        public override bool Equals(object obj) { throw null; }
        public override int GetHashCode() { throw null; }
        public static Microsoft.Net.Http.Headers.RangeHeaderValue Parse(string input) { throw null; }
        public override string ToString() { throw null; }
        public static bool TryParse(string input, out Microsoft.Net.Http.Headers.RangeHeaderValue parsedValue) { parsedValue = default(Microsoft.Net.Http.Headers.RangeHeaderValue); throw null; }
    }
    public partial class RangeItemHeaderValue
    {
        public RangeItemHeaderValue(System.Nullable<long> from, System.Nullable<long> to) { }
        public System.Nullable<long> From { get { throw null; } }
        public System.Nullable<long> To { get { throw null; } }
        public override bool Equals(object obj) { throw null; }
        public override int GetHashCode() { throw null; }
        public override string ToString() { throw null; }
    }
    public partial class SetCookieHeaderValue
    {
        public SetCookieHeaderValue(string name) { }
        public SetCookieHeaderValue(string name, string value) { }
        public string Domain { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public System.Nullable<System.DateTimeOffset> Expires { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public bool HttpOnly { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public System.Nullable<System.TimeSpan> MaxAge { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public string Name { get { throw null; } set { } }
        public string Path { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public bool Secure { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public string Value { get { throw null; } set { } }
        public void AppendToStringBuilder(System.Text.StringBuilder builder) { }
        public override bool Equals(object obj) { throw null; }
        public override int GetHashCode() { throw null; }
        public static Microsoft.Net.Http.Headers.SetCookieHeaderValue Parse(string input) { throw null; }
        public static System.Collections.Generic.IList<Microsoft.Net.Http.Headers.SetCookieHeaderValue> ParseList(System.Collections.Generic.IList<string> inputs) { throw null; }
        public static System.Collections.Generic.IList<Microsoft.Net.Http.Headers.SetCookieHeaderValue> ParseStrictList(System.Collections.Generic.IList<string> inputs) { throw null; }
        public override string ToString() { throw null; }
        public static bool TryParse(string input, out Microsoft.Net.Http.Headers.SetCookieHeaderValue parsedValue) { parsedValue = default(Microsoft.Net.Http.Headers.SetCookieHeaderValue); throw null; }
        public static bool TryParseList(System.Collections.Generic.IList<string> inputs, out System.Collections.Generic.IList<Microsoft.Net.Http.Headers.SetCookieHeaderValue> parsedValues) { parsedValues = default(System.Collections.Generic.IList<Microsoft.Net.Http.Headers.SetCookieHeaderValue>); throw null; }
        public static bool TryParseStrictList(System.Collections.Generic.IList<string> inputs, out System.Collections.Generic.IList<Microsoft.Net.Http.Headers.SetCookieHeaderValue> parsedValues) { parsedValues = default(System.Collections.Generic.IList<Microsoft.Net.Http.Headers.SetCookieHeaderValue>); throw null; }
    }
    public partial class StringWithQualityHeaderValue
    {
        public StringWithQualityHeaderValue(string value) { }
        public StringWithQualityHeaderValue(string value, double quality) { }
        public System.Nullable<double> Quality { get { throw null; } }
        public string Value { get { throw null; } }
        public override bool Equals(object obj) { throw null; }
        public override int GetHashCode() { throw null; }
        public static Microsoft.Net.Http.Headers.StringWithQualityHeaderValue Parse(string input) { throw null; }
        public static System.Collections.Generic.IList<Microsoft.Net.Http.Headers.StringWithQualityHeaderValue> ParseList(System.Collections.Generic.IList<string> input) { throw null; }
        public static System.Collections.Generic.IList<Microsoft.Net.Http.Headers.StringWithQualityHeaderValue> ParseStrictList(System.Collections.Generic.IList<string> input) { throw null; }
        public override string ToString() { throw null; }
        public static bool TryParse(string input, out Microsoft.Net.Http.Headers.StringWithQualityHeaderValue parsedValue) { parsedValue = default(Microsoft.Net.Http.Headers.StringWithQualityHeaderValue); throw null; }
        public static bool TryParseList(System.Collections.Generic.IList<string> input, out System.Collections.Generic.IList<Microsoft.Net.Http.Headers.StringWithQualityHeaderValue> parsedValues) { parsedValues = default(System.Collections.Generic.IList<Microsoft.Net.Http.Headers.StringWithQualityHeaderValue>); throw null; }
        public static bool TryParseStrictList(System.Collections.Generic.IList<string> input, out System.Collections.Generic.IList<Microsoft.Net.Http.Headers.StringWithQualityHeaderValue> parsedValues) { parsedValues = default(System.Collections.Generic.IList<Microsoft.Net.Http.Headers.StringWithQualityHeaderValue>); throw null; }
    }
    public partial class StringWithQualityHeaderValueComparer : System.Collections.Generic.IComparer<Microsoft.Net.Http.Headers.StringWithQualityHeaderValue>
    {
        internal StringWithQualityHeaderValueComparer() { }
        public static Microsoft.Net.Http.Headers.StringWithQualityHeaderValueComparer QualityComparer { get { throw null; } }
        public int Compare(Microsoft.Net.Http.Headers.StringWithQualityHeaderValue stringWithQuality1, Microsoft.Net.Http.Headers.StringWithQualityHeaderValue stringWithQuality2) { throw null; }
    }
}
