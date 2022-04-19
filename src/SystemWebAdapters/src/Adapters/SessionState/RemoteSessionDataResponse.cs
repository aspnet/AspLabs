using System.Net.Http;

namespace System.Web.Adapters.SessionState;

internal class RemoteSessionDataResponse: IDisposable
{
    public RemoteSessionData RemoteSessionData { get; }
    public HttpResponseMessage HttpRespone { get; }

    public RemoteSessionDataResponse(RemoteSessionData remoteSessionData, HttpResponseMessage httpResponse)
    {
        RemoteSessionData = remoteSessionData;
        HttpRespone = httpResponse;
    }

    public void Dispose() => HttpRespone.Dispose();
}
