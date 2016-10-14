namespace Microsoft.AspNetCore.Builder
{
    public static partial class MapRouteRouteBuilderExtensions
    {
        public static Microsoft.AspNetCore.Routing.IRouteBuilder MapRoute(this Microsoft.AspNetCore.Routing.IRouteBuilder routeBuilder, string name, string template) { throw null; }
        public static Microsoft.AspNetCore.Routing.IRouteBuilder MapRoute(this Microsoft.AspNetCore.Routing.IRouteBuilder routeBuilder, string name, string template, object defaults) { throw null; }
        public static Microsoft.AspNetCore.Routing.IRouteBuilder MapRoute(this Microsoft.AspNetCore.Routing.IRouteBuilder routeBuilder, string name, string template, object defaults, object constraints) { throw null; }
        public static Microsoft.AspNetCore.Routing.IRouteBuilder MapRoute(this Microsoft.AspNetCore.Routing.IRouteBuilder routeBuilder, string name, string template, object defaults, object constraints, object dataTokens) { throw null; }
    }
    public partial class RouterMiddleware
    {
        public RouterMiddleware(Microsoft.AspNetCore.Http.RequestDelegate next, Microsoft.Extensions.Logging.ILoggerFactory loggerFactory, Microsoft.AspNetCore.Routing.IRouter router) { }
        public System.Threading.Tasks.Task Invoke(Microsoft.AspNetCore.Http.HttpContext httpContext) { throw null; }
    }
    public static partial class RoutingBuilderExtensions
    {
        public static Microsoft.AspNetCore.Builder.IApplicationBuilder UseRouter(this Microsoft.AspNetCore.Builder.IApplicationBuilder builder, Microsoft.AspNetCore.Routing.IRouter router) { throw null; }
    }
}
namespace Microsoft.AspNetCore.Routing
{
    public partial class DefaultInlineConstraintResolver : Microsoft.AspNetCore.Routing.IInlineConstraintResolver
    {
        public DefaultInlineConstraintResolver(Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Routing.RouteOptions> routeOptions) { }
        public virtual Microsoft.AspNetCore.Routing.IRouteConstraint ResolveConstraint(string inlineConstraint) { throw null; }
    }
    public partial interface IInlineConstraintResolver
    {
        Microsoft.AspNetCore.Routing.IRouteConstraint ResolveConstraint(string inlineConstraint);
    }
    public partial interface INamedRouter : Microsoft.AspNetCore.Routing.IRouter
    {
        string Name { get; }
    }
    public static partial class InlineRouteParameterParser
    {
        public static Microsoft.AspNetCore.Routing.Template.TemplatePart ParseRouteParameter(string routeParameter) { throw null; }
    }
    public partial interface IRouteBuilder
    {
        Microsoft.AspNetCore.Builder.IApplicationBuilder ApplicationBuilder { get; }
        Microsoft.AspNetCore.Routing.IRouter DefaultHandler { get; set; }
        System.Collections.Generic.IList<Microsoft.AspNetCore.Routing.IRouter> Routes { get; }
        System.IServiceProvider ServiceProvider { get; }
        Microsoft.AspNetCore.Routing.IRouter Build();
    }
    public partial interface IRouteCollection : Microsoft.AspNetCore.Routing.IRouter
    {
        void Add(Microsoft.AspNetCore.Routing.IRouter router);
    }
    public static partial class RequestDelegateRouteBuilderExtensions
    {
        public static Microsoft.AspNetCore.Routing.IRouteBuilder MapDelete(this Microsoft.AspNetCore.Routing.IRouteBuilder builder, string template, Microsoft.AspNetCore.Http.RequestDelegate handler) { throw null; }
        public static Microsoft.AspNetCore.Routing.IRouteBuilder MapDelete(this Microsoft.AspNetCore.Routing.IRouteBuilder builder, string template, System.Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> action) { throw null; }
        public static Microsoft.AspNetCore.Routing.IRouteBuilder MapGet(this Microsoft.AspNetCore.Routing.IRouteBuilder builder, string template, Microsoft.AspNetCore.Http.RequestDelegate handler) { throw null; }
        public static Microsoft.AspNetCore.Routing.IRouteBuilder MapGet(this Microsoft.AspNetCore.Routing.IRouteBuilder builder, string template, System.Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> action) { throw null; }
        public static Microsoft.AspNetCore.Routing.IRouteBuilder MapPost(this Microsoft.AspNetCore.Routing.IRouteBuilder builder, string template, Microsoft.AspNetCore.Http.RequestDelegate handler) { throw null; }
        public static Microsoft.AspNetCore.Routing.IRouteBuilder MapPost(this Microsoft.AspNetCore.Routing.IRouteBuilder builder, string template, System.Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> action) { throw null; }
        public static Microsoft.AspNetCore.Routing.IRouteBuilder MapPut(this Microsoft.AspNetCore.Routing.IRouteBuilder builder, string template, Microsoft.AspNetCore.Http.RequestDelegate handler) { throw null; }
        public static Microsoft.AspNetCore.Routing.IRouteBuilder MapPut(this Microsoft.AspNetCore.Routing.IRouteBuilder builder, string template, System.Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> action) { throw null; }
        public static Microsoft.AspNetCore.Routing.IRouteBuilder MapRoute(this Microsoft.AspNetCore.Routing.IRouteBuilder builder, string template, Microsoft.AspNetCore.Http.RequestDelegate handler) { throw null; }
        public static Microsoft.AspNetCore.Routing.IRouteBuilder MapRoute(this Microsoft.AspNetCore.Routing.IRouteBuilder builder, string template, System.Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> action) { throw null; }
        public static Microsoft.AspNetCore.Routing.IRouteBuilder MapVerb(this Microsoft.AspNetCore.Routing.IRouteBuilder builder, string verb, string template, Microsoft.AspNetCore.Http.RequestDelegate handler) { throw null; }
        public static Microsoft.AspNetCore.Routing.IRouteBuilder MapVerb(this Microsoft.AspNetCore.Routing.IRouteBuilder builder, string verb, string template, System.Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> action) { throw null; }
    }
    public partial class Route : Microsoft.AspNetCore.Routing.RouteBase
    {
        public Route(Microsoft.AspNetCore.Routing.IRouter target, string routeTemplate, Microsoft.AspNetCore.Routing.IInlineConstraintResolver inlineConstraintResolver) : base (default(string), default(string), default(Microsoft.AspNetCore.Routing.IInlineConstraintResolver), default(Microsoft.AspNetCore.Routing.RouteValueDictionary), default(System.Collections.Generic.IDictionary<string, object>), default(Microsoft.AspNetCore.Routing.RouteValueDictionary)) { }
        public Route(Microsoft.AspNetCore.Routing.IRouter target, string routeTemplate, Microsoft.AspNetCore.Routing.RouteValueDictionary defaults, System.Collections.Generic.IDictionary<string, object> constraints, Microsoft.AspNetCore.Routing.RouteValueDictionary dataTokens, Microsoft.AspNetCore.Routing.IInlineConstraintResolver inlineConstraintResolver) : base (default(string), default(string), default(Microsoft.AspNetCore.Routing.IInlineConstraintResolver), default(Microsoft.AspNetCore.Routing.RouteValueDictionary), default(System.Collections.Generic.IDictionary<string, object>), default(Microsoft.AspNetCore.Routing.RouteValueDictionary)) { }
        public Route(Microsoft.AspNetCore.Routing.IRouter target, string routeName, string routeTemplate, Microsoft.AspNetCore.Routing.RouteValueDictionary defaults, System.Collections.Generic.IDictionary<string, object> constraints, Microsoft.AspNetCore.Routing.RouteValueDictionary dataTokens, Microsoft.AspNetCore.Routing.IInlineConstraintResolver inlineConstraintResolver) : base (default(string), default(string), default(Microsoft.AspNetCore.Routing.IInlineConstraintResolver), default(Microsoft.AspNetCore.Routing.RouteValueDictionary), default(System.Collections.Generic.IDictionary<string, object>), default(Microsoft.AspNetCore.Routing.RouteValueDictionary)) { }
        public string RouteTemplate { get { throw null; } }
        protected override System.Threading.Tasks.Task OnRouteMatched(Microsoft.AspNetCore.Routing.RouteContext context) { throw null; }
        protected override Microsoft.AspNetCore.Routing.VirtualPathData OnVirtualPathGenerated(Microsoft.AspNetCore.Routing.VirtualPathContext context) { throw null; }
    }
    public abstract partial class RouteBase : Microsoft.AspNetCore.Routing.INamedRouter, Microsoft.AspNetCore.Routing.IRouter
    {
        public RouteBase(string template, string name, Microsoft.AspNetCore.Routing.IInlineConstraintResolver constraintResolver, Microsoft.AspNetCore.Routing.RouteValueDictionary defaults, System.Collections.Generic.IDictionary<string, object> constraints, Microsoft.AspNetCore.Routing.RouteValueDictionary dataTokens) { }
        protected virtual Microsoft.AspNetCore.Routing.IInlineConstraintResolver ConstraintResolver { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public virtual System.Collections.Generic.IDictionary<string, Microsoft.AspNetCore.Routing.IRouteConstraint> Constraints { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]protected set { } }
        public virtual Microsoft.AspNetCore.Routing.RouteValueDictionary DataTokens { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]protected set { } }
        public virtual Microsoft.AspNetCore.Routing.RouteValueDictionary Defaults { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]protected set { } }
        public virtual string Name { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]protected set { } }
        public virtual Microsoft.AspNetCore.Routing.Template.RouteTemplate ParsedTemplate { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]protected set { } }
        protected static System.Collections.Generic.IDictionary<string, Microsoft.AspNetCore.Routing.IRouteConstraint> GetConstraints(Microsoft.AspNetCore.Routing.IInlineConstraintResolver inlineConstraintResolver, Microsoft.AspNetCore.Routing.Template.RouteTemplate parsedTemplate, System.Collections.Generic.IDictionary<string, object> constraints) { throw null; }
        protected static Microsoft.AspNetCore.Routing.RouteValueDictionary GetDefaults(Microsoft.AspNetCore.Routing.Template.RouteTemplate parsedTemplate, Microsoft.AspNetCore.Routing.RouteValueDictionary defaults) { throw null; }
        public virtual Microsoft.AspNetCore.Routing.VirtualPathData GetVirtualPath(Microsoft.AspNetCore.Routing.VirtualPathContext context) { throw null; }
        protected abstract System.Threading.Tasks.Task OnRouteMatched(Microsoft.AspNetCore.Routing.RouteContext context);
        protected abstract Microsoft.AspNetCore.Routing.VirtualPathData OnVirtualPathGenerated(Microsoft.AspNetCore.Routing.VirtualPathContext context);
        public virtual System.Threading.Tasks.Task RouteAsync(Microsoft.AspNetCore.Routing.RouteContext context) { throw null; }
        public override string ToString() { throw null; }
    }
    public partial class RouteBuilder : Microsoft.AspNetCore.Routing.IRouteBuilder
    {
        public RouteBuilder(Microsoft.AspNetCore.Builder.IApplicationBuilder applicationBuilder) { }
        public RouteBuilder(Microsoft.AspNetCore.Builder.IApplicationBuilder applicationBuilder, Microsoft.AspNetCore.Routing.IRouter defaultHandler) { }
        public Microsoft.AspNetCore.Builder.IApplicationBuilder ApplicationBuilder { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public Microsoft.AspNetCore.Routing.IRouter DefaultHandler { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public System.Collections.Generic.IList<Microsoft.AspNetCore.Routing.IRouter> Routes { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public System.IServiceProvider ServiceProvider { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public Microsoft.AspNetCore.Routing.IRouter Build() { throw null; }
    }
    public partial class RouteCollection : Microsoft.AspNetCore.Routing.IRouteCollection, Microsoft.AspNetCore.Routing.IRouter
    {
        public RouteCollection() { }
        public int Count { get { throw null; } }
        public Microsoft.AspNetCore.Routing.IRouter this[int index] { get { throw null; } }
        public void Add(Microsoft.AspNetCore.Routing.IRouter router) { }
        public virtual Microsoft.AspNetCore.Routing.VirtualPathData GetVirtualPath(Microsoft.AspNetCore.Routing.VirtualPathContext context) { throw null; }
        public virtual System.Threading.Tasks.Task RouteAsync(Microsoft.AspNetCore.Routing.RouteContext context) { throw null; }
    }
    public partial class RouteConstraintBuilder
    {
        public RouteConstraintBuilder(Microsoft.AspNetCore.Routing.IInlineConstraintResolver inlineConstraintResolver, string displayName) { }
        public void AddConstraint(string key, object value) { }
        public void AddResolvedConstraint(string key, string constraintText) { }
        public System.Collections.Generic.IDictionary<string, Microsoft.AspNetCore.Routing.IRouteConstraint> Build() { throw null; }
        public void SetOptional(string key) { }
    }
    public static partial class RouteConstraintMatcher
    {
        public static bool Match(System.Collections.Generic.IDictionary<string, Microsoft.AspNetCore.Routing.IRouteConstraint> constraints, Microsoft.AspNetCore.Routing.RouteValueDictionary routeValues, Microsoft.AspNetCore.Http.HttpContext httpContext, Microsoft.AspNetCore.Routing.IRouter route, Microsoft.AspNetCore.Routing.RouteDirection routeDirection, Microsoft.Extensions.Logging.ILogger logger) { throw null; }
    }
    public partial class RouteHandler : Microsoft.AspNetCore.Routing.IRouteHandler, Microsoft.AspNetCore.Routing.IRouter
    {
        public RouteHandler(Microsoft.AspNetCore.Http.RequestDelegate requestDelegate) { }
        public Microsoft.AspNetCore.Http.RequestDelegate GetRequestHandler(Microsoft.AspNetCore.Http.HttpContext httpContext, Microsoft.AspNetCore.Routing.RouteData routeData) { throw null; }
        public Microsoft.AspNetCore.Routing.VirtualPathData GetVirtualPath(Microsoft.AspNetCore.Routing.VirtualPathContext context) { throw null; }
        public System.Threading.Tasks.Task RouteAsync(Microsoft.AspNetCore.Routing.RouteContext context) { throw null; }
    }
    public partial class RouteOptions
    {
        public RouteOptions() { }
        public bool AppendTrailingSlash { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public System.Collections.Generic.IDictionary<string, System.Type> ConstraintMap { get { throw null; } set { } }
        public bool LowercaseUrls { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    }
    public partial class RouteValueEqualityComparer : System.Collections.Generic.IEqualityComparer<object>
    {
        public RouteValueEqualityComparer() { }
        public new bool Equals(object x, object y) { throw null; }
        public int GetHashCode(object obj) { throw null; }
    }
}
namespace Microsoft.AspNetCore.Routing.Constraints
{
    public partial class AlphaRouteConstraint : Microsoft.AspNetCore.Routing.Constraints.RegexRouteConstraint
    {
        public AlphaRouteConstraint() : base (default(System.Text.RegularExpressions.Regex)) { }
    }
    public partial class BoolRouteConstraint : Microsoft.AspNetCore.Routing.IRouteConstraint
    {
        public BoolRouteConstraint() { }
        public bool Match(Microsoft.AspNetCore.Http.HttpContext httpContext, Microsoft.AspNetCore.Routing.IRouter route, string routeKey, Microsoft.AspNetCore.Routing.RouteValueDictionary values, Microsoft.AspNetCore.Routing.RouteDirection routeDirection) { throw null; }
    }
    public partial class CompositeRouteConstraint : Microsoft.AspNetCore.Routing.IRouteConstraint
    {
        public CompositeRouteConstraint(System.Collections.Generic.IEnumerable<Microsoft.AspNetCore.Routing.IRouteConstraint> constraints) { }
        public System.Collections.Generic.IEnumerable<Microsoft.AspNetCore.Routing.IRouteConstraint> Constraints { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public bool Match(Microsoft.AspNetCore.Http.HttpContext httpContext, Microsoft.AspNetCore.Routing.IRouter route, string routeKey, Microsoft.AspNetCore.Routing.RouteValueDictionary values, Microsoft.AspNetCore.Routing.RouteDirection routeDirection) { throw null; }
    }
    public partial class DateTimeRouteConstraint : Microsoft.AspNetCore.Routing.IRouteConstraint
    {
        public DateTimeRouteConstraint() { }
        public bool Match(Microsoft.AspNetCore.Http.HttpContext httpContext, Microsoft.AspNetCore.Routing.IRouter route, string routeKey, Microsoft.AspNetCore.Routing.RouteValueDictionary values, Microsoft.AspNetCore.Routing.RouteDirection routeDirection) { throw null; }
    }
    public partial class DecimalRouteConstraint : Microsoft.AspNetCore.Routing.IRouteConstraint
    {
        public DecimalRouteConstraint() { }
        public bool Match(Microsoft.AspNetCore.Http.HttpContext httpContext, Microsoft.AspNetCore.Routing.IRouter route, string routeKey, Microsoft.AspNetCore.Routing.RouteValueDictionary values, Microsoft.AspNetCore.Routing.RouteDirection routeDirection) { throw null; }
    }
    public partial class DoubleRouteConstraint : Microsoft.AspNetCore.Routing.IRouteConstraint
    {
        public DoubleRouteConstraint() { }
        public bool Match(Microsoft.AspNetCore.Http.HttpContext httpContext, Microsoft.AspNetCore.Routing.IRouter route, string routeKey, Microsoft.AspNetCore.Routing.RouteValueDictionary values, Microsoft.AspNetCore.Routing.RouteDirection routeDirection) { throw null; }
    }
    public partial class FloatRouteConstraint : Microsoft.AspNetCore.Routing.IRouteConstraint
    {
        public FloatRouteConstraint() { }
        public bool Match(Microsoft.AspNetCore.Http.HttpContext httpContext, Microsoft.AspNetCore.Routing.IRouter route, string routeKey, Microsoft.AspNetCore.Routing.RouteValueDictionary values, Microsoft.AspNetCore.Routing.RouteDirection routeDirection) { throw null; }
    }
    public partial class GuidRouteConstraint : Microsoft.AspNetCore.Routing.IRouteConstraint
    {
        public GuidRouteConstraint() { }
        public bool Match(Microsoft.AspNetCore.Http.HttpContext httpContext, Microsoft.AspNetCore.Routing.IRouter route, string routeKey, Microsoft.AspNetCore.Routing.RouteValueDictionary values, Microsoft.AspNetCore.Routing.RouteDirection routeDirection) { throw null; }
    }
    public partial class HttpMethodRouteConstraint : Microsoft.AspNetCore.Routing.IRouteConstraint
    {
        public HttpMethodRouteConstraint(params string[] allowedMethods) { }
        public System.Collections.Generic.IList<string> AllowedMethods { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public virtual bool Match(Microsoft.AspNetCore.Http.HttpContext httpContext, Microsoft.AspNetCore.Routing.IRouter route, string routeKey, Microsoft.AspNetCore.Routing.RouteValueDictionary values, Microsoft.AspNetCore.Routing.RouteDirection routeDirection) { throw null; }
    }
    public partial class IntRouteConstraint : Microsoft.AspNetCore.Routing.IRouteConstraint
    {
        public IntRouteConstraint() { }
        public bool Match(Microsoft.AspNetCore.Http.HttpContext httpContext, Microsoft.AspNetCore.Routing.IRouter route, string routeKey, Microsoft.AspNetCore.Routing.RouteValueDictionary values, Microsoft.AspNetCore.Routing.RouteDirection routeDirection) { throw null; }
    }
    public partial class LengthRouteConstraint : Microsoft.AspNetCore.Routing.IRouteConstraint
    {
        public LengthRouteConstraint(int length) { }
        public LengthRouteConstraint(int minLength, int maxLength) { }
        public int MaxLength { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public int MinLength { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public bool Match(Microsoft.AspNetCore.Http.HttpContext httpContext, Microsoft.AspNetCore.Routing.IRouter route, string routeKey, Microsoft.AspNetCore.Routing.RouteValueDictionary values, Microsoft.AspNetCore.Routing.RouteDirection routeDirection) { throw null; }
    }
    public partial class LongRouteConstraint : Microsoft.AspNetCore.Routing.IRouteConstraint
    {
        public LongRouteConstraint() { }
        public bool Match(Microsoft.AspNetCore.Http.HttpContext httpContext, Microsoft.AspNetCore.Routing.IRouter route, string routeKey, Microsoft.AspNetCore.Routing.RouteValueDictionary values, Microsoft.AspNetCore.Routing.RouteDirection routeDirection) { throw null; }
    }
    public partial class MaxLengthRouteConstraint : Microsoft.AspNetCore.Routing.IRouteConstraint
    {
        public MaxLengthRouteConstraint(int maxLength) { }
        public int MaxLength { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public bool Match(Microsoft.AspNetCore.Http.HttpContext httpContext, Microsoft.AspNetCore.Routing.IRouter route, string routeKey, Microsoft.AspNetCore.Routing.RouteValueDictionary values, Microsoft.AspNetCore.Routing.RouteDirection routeDirection) { throw null; }
    }
    public partial class MaxRouteConstraint : Microsoft.AspNetCore.Routing.IRouteConstraint
    {
        public MaxRouteConstraint(long max) { }
        public long Max { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public bool Match(Microsoft.AspNetCore.Http.HttpContext httpContext, Microsoft.AspNetCore.Routing.IRouter route, string routeKey, Microsoft.AspNetCore.Routing.RouteValueDictionary values, Microsoft.AspNetCore.Routing.RouteDirection routeDirection) { throw null; }
    }
    public partial class MinLengthRouteConstraint : Microsoft.AspNetCore.Routing.IRouteConstraint
    {
        public MinLengthRouteConstraint(int minLength) { }
        public int MinLength { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public bool Match(Microsoft.AspNetCore.Http.HttpContext httpContext, Microsoft.AspNetCore.Routing.IRouter route, string routeKey, Microsoft.AspNetCore.Routing.RouteValueDictionary values, Microsoft.AspNetCore.Routing.RouteDirection routeDirection) { throw null; }
    }
    public partial class MinRouteConstraint : Microsoft.AspNetCore.Routing.IRouteConstraint
    {
        public MinRouteConstraint(long min) { }
        public long Min { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public bool Match(Microsoft.AspNetCore.Http.HttpContext httpContext, Microsoft.AspNetCore.Routing.IRouter route, string routeKey, Microsoft.AspNetCore.Routing.RouteValueDictionary values, Microsoft.AspNetCore.Routing.RouteDirection routeDirection) { throw null; }
    }
    public partial class OptionalRouteConstraint : Microsoft.AspNetCore.Routing.IRouteConstraint
    {
        public OptionalRouteConstraint(Microsoft.AspNetCore.Routing.IRouteConstraint innerConstraint) { }
        public Microsoft.AspNetCore.Routing.IRouteConstraint InnerConstraint { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public bool Match(Microsoft.AspNetCore.Http.HttpContext httpContext, Microsoft.AspNetCore.Routing.IRouter route, string routeKey, Microsoft.AspNetCore.Routing.RouteValueDictionary values, Microsoft.AspNetCore.Routing.RouteDirection routeDirection) { throw null; }
    }
    public partial class RangeRouteConstraint : Microsoft.AspNetCore.Routing.IRouteConstraint
    {
        public RangeRouteConstraint(long min, long max) { }
        public long Max { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public long Min { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public bool Match(Microsoft.AspNetCore.Http.HttpContext httpContext, Microsoft.AspNetCore.Routing.IRouter route, string routeKey, Microsoft.AspNetCore.Routing.RouteValueDictionary values, Microsoft.AspNetCore.Routing.RouteDirection routeDirection) { throw null; }
    }
    public partial class RegexInlineRouteConstraint : Microsoft.AspNetCore.Routing.Constraints.RegexRouteConstraint
    {
        public RegexInlineRouteConstraint(string regexPattern) : base (default(System.Text.RegularExpressions.Regex)) { }
    }
    public partial class RegexRouteConstraint : Microsoft.AspNetCore.Routing.IRouteConstraint
    {
        public RegexRouteConstraint(string regexPattern) { }
        public RegexRouteConstraint(System.Text.RegularExpressions.Regex regex) { }
        public System.Text.RegularExpressions.Regex Constraint { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public bool Match(Microsoft.AspNetCore.Http.HttpContext httpContext, Microsoft.AspNetCore.Routing.IRouter route, string routeKey, Microsoft.AspNetCore.Routing.RouteValueDictionary values, Microsoft.AspNetCore.Routing.RouteDirection routeDirection) { throw null; }
    }
    public partial class RequiredRouteConstraint : Microsoft.AspNetCore.Routing.IRouteConstraint
    {
        public RequiredRouteConstraint() { }
        public bool Match(Microsoft.AspNetCore.Http.HttpContext httpContext, Microsoft.AspNetCore.Routing.IRouter route, string routeKey, Microsoft.AspNetCore.Routing.RouteValueDictionary values, Microsoft.AspNetCore.Routing.RouteDirection routeDirection) { throw null; }
    }
}
namespace Microsoft.AspNetCore.Routing.Internal
{
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public partial struct BufferValue
    {
        public BufferValue(string value, bool requiresEncoding) { throw null;}
        public bool RequiresEncoding { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public string Value { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
    }
    public partial class LinkGenerationDecisionTree
    {
        public LinkGenerationDecisionTree(System.Collections.Generic.IReadOnlyList<Microsoft.AspNetCore.Routing.Tree.OutboundMatch> entries) { }
        public System.Collections.Generic.IList<Microsoft.AspNetCore.Routing.Internal.OutboundMatchResult> GetMatches(Microsoft.AspNetCore.Routing.VirtualPathContext context) { throw null; }
    }
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public partial struct OutboundMatchResult
    {
        public OutboundMatchResult(Microsoft.AspNetCore.Routing.Tree.OutboundMatch match, bool isFallbackMatch) { throw null;}
        public bool IsFallbackMatch { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public Microsoft.AspNetCore.Routing.Tree.OutboundMatch Match { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
    }
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public partial struct PathTokenizer : System.Collections.Generic.IEnumerable<Microsoft.Extensions.Primitives.StringSegment>, System.Collections.Generic.IReadOnlyCollection<Microsoft.Extensions.Primitives.StringSegment>, System.Collections.Generic.IReadOnlyList<Microsoft.Extensions.Primitives.StringSegment>, System.Collections.IEnumerable
    {
        public PathTokenizer(Microsoft.AspNetCore.Http.PathString path) { throw null;}
        public int Count { get { throw null; } }
        public Microsoft.Extensions.Primitives.StringSegment this[int index] { get { throw null; } }
        public Microsoft.AspNetCore.Routing.Internal.PathTokenizer.Enumerator GetEnumerator() { throw null; }
        System.Collections.Generic.IEnumerator<Microsoft.Extensions.Primitives.StringSegment> System.Collections.Generic.IEnumerable<Microsoft.Extensions.Primitives.StringSegment>.GetEnumerator() { throw null; }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { throw null; }
        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public partial struct Enumerator : System.Collections.Generic.IEnumerator<Microsoft.Extensions.Primitives.StringSegment>, System.Collections.IEnumerator, System.IDisposable
        {
            public Enumerator(Microsoft.AspNetCore.Routing.Internal.PathTokenizer tokenizer) { throw null;}
            public Microsoft.Extensions.Primitives.StringSegment Current { get { throw null; } }
            object System.Collections.IEnumerator.Current { get { throw null; } }
            public void Dispose() { }
            public bool MoveNext() { throw null; }
            public void Reset() { }
        }
    }
    public partial class RoutingMarkerService
    {
        public RoutingMarkerService() { }
    }
    public enum SegmentState
    {
        Beginning = 0,
        Inside = 1,
    }
    public static partial class TaskCache
    {
        public static readonly System.Threading.Tasks.Task CompletedTask;
    }
    public partial class UriBuilderContextPooledObjectPolicy : Microsoft.Extensions.ObjectPool.IPooledObjectPolicy<Microsoft.AspNetCore.Routing.Internal.UriBuildingContext>
    {
        public UriBuilderContextPooledObjectPolicy(System.Text.Encodings.Web.UrlEncoder encoder) { }
        public Microsoft.AspNetCore.Routing.Internal.UriBuildingContext Create() { throw null; }
        public bool Return(Microsoft.AspNetCore.Routing.Internal.UriBuildingContext obj) { throw null; }
    }
    [System.Diagnostics.DebuggerDisplayAttribute("{DebuggerToString(),nq}")]
    public partial class UriBuildingContext
    {
        public UriBuildingContext(System.Text.Encodings.Web.UrlEncoder urlEncoder) { }
        public Microsoft.AspNetCore.Routing.Internal.SegmentState BufferState { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public Microsoft.AspNetCore.Routing.Internal.SegmentState UriState { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public System.IO.TextWriter Writer { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public bool Accept(string value) { throw null; }
        public bool Buffer(string value) { throw null; }
        public void Clear() { }
        public void EndSegment() { }
        public void Remove(string literal) { }
        public override string ToString() { throw null; }
    }
}
namespace Microsoft.AspNetCore.Routing.Template
{
    public partial class InlineConstraint
    {
        public InlineConstraint(string constraint) { }
        public string Constraint { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
    }
    public static partial class RoutePrecedence
    {
        public static decimal ComputeInbound(Microsoft.AspNetCore.Routing.Template.RouteTemplate template) { throw null; }
        public static decimal ComputeOutbound(Microsoft.AspNetCore.Routing.Template.RouteTemplate template) { throw null; }
    }
    [System.Diagnostics.DebuggerDisplayAttribute("{DebuggerToString()}")]
    public partial class RouteTemplate
    {
        public RouteTemplate(string template, System.Collections.Generic.List<Microsoft.AspNetCore.Routing.Template.TemplateSegment> segments) { }
        public System.Collections.Generic.IList<Microsoft.AspNetCore.Routing.Template.TemplatePart> Parameters { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public System.Collections.Generic.IList<Microsoft.AspNetCore.Routing.Template.TemplateSegment> Segments { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public string TemplateText { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public Microsoft.AspNetCore.Routing.Template.TemplatePart GetParameter(string name) { throw null; }
        public Microsoft.AspNetCore.Routing.Template.TemplateSegment GetSegment(int index) { throw null; }
    }
    public partial class TemplateBinder
    {
        public TemplateBinder(System.Text.Encodings.Web.UrlEncoder urlEncoder, Microsoft.Extensions.ObjectPool.ObjectPool<Microsoft.AspNetCore.Routing.Internal.UriBuildingContext> pool, Microsoft.AspNetCore.Routing.Template.RouteTemplate template, Microsoft.AspNetCore.Routing.RouteValueDictionary defaults) { }
        public string BindValues(Microsoft.AspNetCore.Routing.RouteValueDictionary acceptedValues) { throw null; }
        public Microsoft.AspNetCore.Routing.Template.TemplateValuesResult GetValues(Microsoft.AspNetCore.Routing.RouteValueDictionary ambientValues, Microsoft.AspNetCore.Routing.RouteValueDictionary values) { throw null; }
        public static bool RoutePartsEqual(object a, object b) { throw null; }
    }
    public partial class TemplateMatcher
    {
        public TemplateMatcher(Microsoft.AspNetCore.Routing.Template.RouteTemplate template, Microsoft.AspNetCore.Routing.RouteValueDictionary defaults) { }
        public Microsoft.AspNetCore.Routing.RouteValueDictionary Defaults { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public Microsoft.AspNetCore.Routing.Template.RouteTemplate Template { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public bool TryMatch(Microsoft.AspNetCore.Http.PathString path, Microsoft.AspNetCore.Routing.RouteValueDictionary values) { throw null; }
    }
    public static partial class TemplateParser
    {
        public static Microsoft.AspNetCore.Routing.Template.RouteTemplate Parse(string routeTemplate) { throw null; }
    }
    [System.Diagnostics.DebuggerDisplayAttribute("{DebuggerToString()}")]
    public partial class TemplatePart
    {
        public TemplatePart() { }
        public object DefaultValue { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public System.Collections.Generic.IEnumerable<Microsoft.AspNetCore.Routing.Template.InlineConstraint> InlineConstraints { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public bool IsCatchAll { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public bool IsLiteral { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public bool IsOptional { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public bool IsOptionalSeperator { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public bool IsParameter { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public string Name { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public string Text { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public static Microsoft.AspNetCore.Routing.Template.TemplatePart CreateLiteral(string text) { throw null; }
        public static Microsoft.AspNetCore.Routing.Template.TemplatePart CreateParameter(string name, bool isCatchAll, bool isOptional, object defaultValue, System.Collections.Generic.IEnumerable<Microsoft.AspNetCore.Routing.Template.InlineConstraint> inlineConstraints) { throw null; }
    }
    [System.Diagnostics.DebuggerDisplayAttribute("{DebuggerToString()}")]
    public partial class TemplateSegment
    {
        public TemplateSegment() { }
        public bool IsSimple { get { throw null; } }
        public System.Collections.Generic.List<Microsoft.AspNetCore.Routing.Template.TemplatePart> Parts { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
    }
    public partial class TemplateValuesResult
    {
        public TemplateValuesResult() { }
        public Microsoft.AspNetCore.Routing.RouteValueDictionary AcceptedValues { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.AspNetCore.Routing.RouteValueDictionary CombinedValues { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    }
}
namespace Microsoft.AspNetCore.Routing.Tree
{
    [System.Diagnostics.DebuggerDisplayAttribute("{DebuggerToString(),nq}")]
    public partial class InboundMatch
    {
        public InboundMatch() { }
        public Microsoft.AspNetCore.Routing.Tree.InboundRouteEntry Entry { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.AspNetCore.Routing.Template.TemplateMatcher TemplateMatcher { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    }
    public partial class InboundRouteEntry
    {
        public InboundRouteEntry() { }
        public System.Collections.Generic.IDictionary<string, Microsoft.AspNetCore.Routing.IRouteConstraint> Constraints { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.AspNetCore.Routing.RouteValueDictionary Defaults { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.AspNetCore.Routing.IRouter Handler { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public int Order { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public decimal Precedence { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public string RouteName { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.AspNetCore.Routing.Template.RouteTemplate RouteTemplate { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    }
    public partial class OutboundMatch
    {
        public OutboundMatch() { }
        public Microsoft.AspNetCore.Routing.Tree.OutboundRouteEntry Entry { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.AspNetCore.Routing.Template.TemplateBinder TemplateBinder { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    }
    public partial class OutboundRouteEntry
    {
        public OutboundRouteEntry() { }
        public System.Collections.Generic.IDictionary<string, Microsoft.AspNetCore.Routing.IRouteConstraint> Constraints { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.AspNetCore.Routing.RouteValueDictionary Defaults { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.AspNetCore.Routing.IRouter Handler { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public int Order { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public decimal Precedence { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.AspNetCore.Routing.RouteValueDictionary RequiredLinkValues { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public string RouteName { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.AspNetCore.Routing.Template.RouteTemplate RouteTemplate { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    }
    public partial class TreeRouteBuilder
    {
        public TreeRouteBuilder(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory, System.Text.Encodings.Web.UrlEncoder urlEncoder, Microsoft.Extensions.ObjectPool.ObjectPool<Microsoft.AspNetCore.Routing.Internal.UriBuildingContext> objectPool, Microsoft.AspNetCore.Routing.IInlineConstraintResolver constraintResolver) { }
        public System.Collections.Generic.IList<Microsoft.AspNetCore.Routing.Tree.InboundRouteEntry> InboundEntries { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public System.Collections.Generic.IList<Microsoft.AspNetCore.Routing.Tree.OutboundRouteEntry> OutboundEntries { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public Microsoft.AspNetCore.Routing.Tree.TreeRouter Build() { throw null; }
        public Microsoft.AspNetCore.Routing.Tree.TreeRouter Build(int version) { throw null; }
        public void Clear() { }
        public Microsoft.AspNetCore.Routing.Tree.InboundRouteEntry MapInbound(Microsoft.AspNetCore.Routing.IRouter handler, Microsoft.AspNetCore.Routing.Template.RouteTemplate routeTemplate, string routeName, int order) { throw null; }
        public Microsoft.AspNetCore.Routing.Tree.OutboundRouteEntry MapOutbound(Microsoft.AspNetCore.Routing.IRouter handler, Microsoft.AspNetCore.Routing.Template.RouteTemplate routeTemplate, Microsoft.AspNetCore.Routing.RouteValueDictionary requiredLinkValues, string routeName, int order) { throw null; }
    }
    public partial class TreeRouter : Microsoft.AspNetCore.Routing.IRouter
    {
        public static readonly string RouteGroupKey;
        public TreeRouter(Microsoft.AspNetCore.Routing.Tree.UrlMatchingTree[] trees, System.Collections.Generic.IEnumerable<Microsoft.AspNetCore.Routing.Tree.OutboundRouteEntry> linkGenerationEntries, System.Text.Encodings.Web.UrlEncoder urlEncoder, Microsoft.Extensions.ObjectPool.ObjectPool<Microsoft.AspNetCore.Routing.Internal.UriBuildingContext> objectPool, Microsoft.Extensions.Logging.ILogger routeLogger, Microsoft.Extensions.Logging.ILogger constraintLogger, int version) { }
        public int Version { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public Microsoft.AspNetCore.Routing.VirtualPathData GetVirtualPath(Microsoft.AspNetCore.Routing.VirtualPathContext context) { throw null; }
        public System.Threading.Tasks.Task RouteAsync(Microsoft.AspNetCore.Routing.RouteContext context) { throw null; }
    }
    [System.Diagnostics.DebuggerDisplayAttribute("{DebuggerToString(),nq}")]
    public partial class UrlMatchingNode
    {
        public UrlMatchingNode(int length) { }
        public Microsoft.AspNetCore.Routing.Tree.UrlMatchingNode CatchAlls { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.AspNetCore.Routing.Tree.UrlMatchingNode ConstrainedCatchAlls { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.AspNetCore.Routing.Tree.UrlMatchingNode ConstrainedParameters { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public int Depth { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public bool IsCatchAll { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public System.Collections.Generic.Dictionary<string, Microsoft.AspNetCore.Routing.Tree.UrlMatchingNode> Literals { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public System.Collections.Generic.List<Microsoft.AspNetCore.Routing.Tree.InboundMatch> Matches { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public Microsoft.AspNetCore.Routing.Tree.UrlMatchingNode Parameters { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    }
    public partial class UrlMatchingTree
    {
        public UrlMatchingTree(int order) { }
        public int Order { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public Microsoft.AspNetCore.Routing.Tree.UrlMatchingNode Root { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
    }
}
namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class RoutingServiceCollectionExtensions
    {
        public static Microsoft.Extensions.DependencyInjection.IServiceCollection AddRouting(this Microsoft.Extensions.DependencyInjection.IServiceCollection services) { throw null; }
        public static Microsoft.Extensions.DependencyInjection.IServiceCollection AddRouting(this Microsoft.Extensions.DependencyInjection.IServiceCollection services, System.Action<Microsoft.AspNetCore.Routing.RouteOptions> configureOptions) { throw null; }
    }
}
