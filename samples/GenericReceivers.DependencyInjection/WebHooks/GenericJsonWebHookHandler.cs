using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks;
using Newtonsoft.Json.Linq;

namespace GenericReceivers.DependencyInjection.WebHooks
{
    public class GenericJsonWebHookHandler : WebHookHandler
    {
        public GenericJsonWebHookHandler()
        {
            this.Receiver = GenericJsonWebHookReceiver.ReceiverName;
        }

        public override Task ExecuteAsync(string receiver, WebHookHandlerContext context)
        {
            // Get JSON from WebHook
            JObject data = context.GetDataOrDefault<JObject>();

            // Get the action for this WebHook coming from the action query parameter in the URI
            string action = context.Actions.FirstOrDefault();

            return Task.FromResult(true);
        }
    }
}
