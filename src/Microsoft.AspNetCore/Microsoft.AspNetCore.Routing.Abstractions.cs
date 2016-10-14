namespace Microsoft.AspNetCore.Routing
{
    public partial interface IRouteConstraint
    {
        bool Match(Microsoft.AspNetCore.Http.HttpContext httpContext, Microsoft.AspNetCore.Routing.IRouter route, string routeKey, Microsoft.AspNetCore.Routing.RouteValueDictionary values, Microsoft.AspNetCore.Routing.RouteDirection routeDirection);
    }
    public partial interface IRouteHandler
    {
        Microsoft.AspNetCore.Http.RequestDelegate GetRequestHandler(Microsoft.AspNetCore.Http.HttpContext httpContext, Microsoft.AspNetCore.Routing.RouteData routeData);
    }
    public partial interface IRouter
    {
        Microsoft.AspNetCore.Routing.VirtualPathData GetVirtualPath(Microsoft.AspNetCore.Routing.VirtualPathContext context);
        System.Threading.Tasks.Task RouteAsync(Microsoft.AspNetCore.Routing.RouteContext context);
    }
    public partial interface IRoutingFeature
    {
        Microsoft.AspNetCore.Routing.RouteData RouteData { get; set; }
    }
    public partial class RouteContext
    {
        public RouteContext(Microsoft.AspNetCore.Http.HttpContext httpContext) { }
        public Microsoft.AspNetCore.Http.RequestDelegate Handler { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public Microsoft.AspNetCore.Http.HttpContext HttpContext { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public Microsoft.AspNetCore.Routing.RouteData RouteData { get { throw null; } set { } }
    }
    public partial class RouteData
    {
        public RouteData() { }
        public RouteData(Microsoft.AspNetCore.Routing.RouteData other) { }
        public Microsoft.AspNetCore.Routing.RouteValueDictionary DataTokens { get { throw null; } }
        public System.Collections.Generic.IList<Microsoft.AspNetCore.Routing.IRouter> Routers { get { throw null; } }
        public Microsoft.AspNetCore.Routing.RouteValueDictionary Values { get { throw null; } }
        public Microsoft.AspNetCore.Routing.RouteData.RouteDataSnapshot PushState(Microsoft.AspNetCore.Routing.IRouter router, Microsoft.AspNetCore.Routing.RouteValueDictionary values, Microsoft.AspNetCore.Routing.RouteValueDictionary dataTokens) { throw null; }
        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public partial struct RouteDataSnapshot
        {
            public RouteDataSnapshot(Microsoft.AspNetCore.Routing.RouteData routeData, Microsoft.AspNetCore.Routing.RouteValueDictionary dataTokens, System.Collections.Generic.IList<Microsoft.AspNetCore.Routing.IRouter> routers, Microsoft.AspNetCore.Routing.RouteValueDictionary values) { throw null;}
            public void Restore() { }
        }
    }
    public enum RouteDirection
    {
        IncomingRequest = 0,
        UrlGeneration = 1,
    }
    public partial class RouteValueDictionary : System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<string, object>>, System.Collections.Generic.IDictionary<string, object>, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, object>>, System.Collections.Generic.IReadOnlyCollection<System.Collections.Generic.KeyValuePair<string, object>>, System.Collections.Generic.IReadOnlyDictionary<string, object>, System.Collections.IEnumerable
    {
        public RouteValueDictionary() { }
        public RouteValueDictionary(object values) { }
        public System.Collections.Generic.IEqualityComparer<string> Comparer { get { throw null; } }
        public int Count { get { throw null; } }
        public object this[string key] { get { throw null; } set { } }
        public System.Collections.Generic.ICollection<string> Keys { get { throw null; } }
        bool System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<System.String,System.Object>>.IsReadOnly { get { throw null; } }
        System.Collections.Generic.IEnumerable<string> System.Collections.Generic.IReadOnlyDictionary<System.String,System.Object>.Keys { get { throw null; } }
        System.Collections.Generic.IEnumerable<object> System.Collections.Generic.IReadOnlyDictionary<System.String,System.Object>.Values { get { throw null; } }
        public System.Collections.Generic.ICollection<object> Values { get { throw null; } }
        public void Add(string key, object value) { }
        public void Clear() { }
        public bool ContainsKey(string key) { throw null; }
        public Microsoft.AspNetCore.Routing.RouteValueDictionary.Enumerator GetEnumerator() { throw null; }
        public bool Remove(string key) { throw null; }
        void System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<System.String,System.Object>>.Add(System.Collections.Generic.KeyValuePair<string, object> item) { }
        bool System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<System.String,System.Object>>.Contains(System.Collections.Generic.KeyValuePair<string, object> item) { throw null; }
        void System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<System.String,System.Object>>.CopyTo(System.Collections.Generic.KeyValuePair<string, object>[] array, int arrayIndex) { }
        bool System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<System.String,System.Object>>.Remove(System.Collections.Generic.KeyValuePair<string, object> item) { throw null; }
        System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<string, object>> System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<System.String,System.Object>>.GetEnumerator() { throw null; }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { throw null; }
        public bool TryGetValue(string key, out object value) { value = default(object); throw null; }
        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public partial struct Enumerator : System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<string, object>>, System.Collections.IEnumerator, System.IDisposable
        {
            public Enumerator(Microsoft.AspNetCore.Routing.RouteValueDictionary dictionary) { throw null;}
            public System.Collections.Generic.KeyValuePair<string, object> Current { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
            object System.Collections.IEnumerator.Current { get { throw null; } }
            public void Dispose() { }
            public bool MoveNext() { throw null; }
            public void Reset() { }
        }
    }
    public static partial class RoutingHttpContextExtensions
    {
        public static Microsoft.AspNetCore.Routing.RouteData GetRouteData(this Microsoft.AspNetCore.Http.HttpContext httpContext) { throw null; }
        public static object GetRouteValue(this Microsoft.AspNetCore.Http.HttpContext httpContext, string key) { throw null; }
    }
    public partial class VirtualPathContext
    {
        public VirtualPathContext(Microsoft.AspNetCore.Http.HttpContext httpContext, Microsoft.AspNetCore.Routing.RouteValueDictionary ambientValues, Microsoft.AspNetCore.Routing.RouteValueDictionary values) { }
        public VirtualPathContext(Microsoft.AspNetCore.Http.HttpContext httpContext, Microsoft.AspNetCore.Routing.RouteValueDictionary ambientValues, Microsoft.AspNetCore.Routing.RouteValueDictionary values, string routeName) { }
        public Microsoft.AspNetCore.Routing.RouteValueDictionary AmbientValues { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public Microsoft.AspNetCore.Http.HttpContext HttpContext { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public string RouteName { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } }
        public Microsoft.AspNetCore.Routing.RouteValueDictionary Values { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    }
    public partial class VirtualPathData
    {
        public VirtualPathData(Microsoft.AspNetCore.Routing.IRouter router, string virtualPath) { }
        public VirtualPathData(Microsoft.AspNetCore.Routing.IRouter router, string virtualPath, Microsoft.AspNetCore.Routing.RouteValueDictionary dataTokens) { }
        public Microsoft.AspNetCore.Routing.RouteValueDictionary DataTokens { get { throw null; } }
        public Microsoft.AspNetCore.Routing.IRouter Router { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw null; } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public string VirtualPath { get { throw null; } set { } }
    }
}
