using System.Threading.Tasks;
using System.Web.Http;
using InstaSharp.Endpoints;

namespace InstagramReceiver.Controllers
{
    [RoutePrefix("api/instagram")]
    public class InstagramSubscriptionController : ApiController
    {
        [Route("subscribe")]
        public async Task<IHttpActionResult> PostSubscribe()
        {
            // Create Instasharp subscription endpoint
            var subscriptions = new Subscription(Dependencies.InstagramConfig);

            // Subscribe for updates from Instagram
            var response = await subscriptions.CreateUser();
            return Ok(response);
        }

        [Route("unsubscribe")]
        public async Task<IHttpActionResult> PostUnsubscribeAll()
        {
            // Create Instasharp subscription endpoint
            var subscriptions = new Subscription(Dependencies.InstagramConfig);

            // Subscribe for updates from Instagram
            var response = await subscriptions.RemoveAllSubscriptions();
            return Ok(response);
        }
    }
}
