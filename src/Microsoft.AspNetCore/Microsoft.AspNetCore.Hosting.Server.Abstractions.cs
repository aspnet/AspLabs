namespace Microsoft.AspNetCore.Hosting.Server
{
    public partial interface IHttpApplication<TContext>
    {
        TContext CreateContext(Microsoft.AspNetCore.Http.Features.IFeatureCollection contextFeatures);
        void DisposeContext(TContext context, System.Exception exception);
        System.Threading.Tasks.Task ProcessRequestAsync(TContext context);
    }
    public partial interface IServer : System.IDisposable
    {
        Microsoft.AspNetCore.Http.Features.IFeatureCollection Features { get; }
        void Start<TContext>(Microsoft.AspNetCore.Hosting.Server.IHttpApplication<TContext> application);
    }
}
namespace Microsoft.AspNetCore.Hosting.Server.Features
{
    public partial interface IServerAddressesFeature
    {
        System.Collections.Generic.ICollection<string> Addresses { get; }
    }
}
