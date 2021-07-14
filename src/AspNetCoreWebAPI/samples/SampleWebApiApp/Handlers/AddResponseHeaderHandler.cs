using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SampleWebApiApp.Handlers
{
    public class AddResponseHeaderHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            response.Headers.TryAddWithoutValidation("X-ResponseHeader", "Header from WebAPI");

            return response;
        }
    }
}
