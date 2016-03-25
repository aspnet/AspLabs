using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.WebHooks;

namespace InstagramReceiver.Controllers
{
    [RoutePrefix("api/instagram")]
    public class InstagramSubscriptionController : ApiController
    {
        [Route("subscribe")]
        public async Task<IHttpActionResult> PostSubscribe()
        {
            // Get our WebHook Client
            InstagramWebHookClient client = Dependencies.Client;

            // Subscribe for updates from Instagram
            var sub = await client.SubscribeAsync(string.Empty, Url);

            return Ok(sub);
        }

        [Route("unsubscribe")]
        public async Task PostUnsubscribeAll()
        {
            // Get our WebHook Client
            InstagramWebHookClient client = Dependencies.Client;

            // Unsubscribe from all subscriptions for the client configuration with id="".
            await client.UnsubscribeAsync(string.Empty);
        }

        [Route("unsubscribe/{subId}")]
        public async Task PostUnsubscribe(string subId)
        {
            // Get our WebHook Client
            InstagramWebHookClient client = Dependencies.Client;

            // Unsubscribe from the given subscription using client configuration with id="".
            await client.UnsubscribeAsync(string.Empty, subId);
        }
    }
}
