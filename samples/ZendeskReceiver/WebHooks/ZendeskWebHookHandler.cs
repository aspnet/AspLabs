using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks;

namespace ZendeskReceiver.WebHooks
{
    /// <summary>
    /// This <see cref="WebHookHandler"/> implementation handles Zendesk WebHooks.
    /// For more information about Zendesk push payloads, please see 
    /// <c>https://developer.zendesk.com/embeddables/docs/android/push_notifications_webhook</c>.
    /// </summary>
    public class ZendeskWebHookHandler : WebHookHandler
    {
        public ZendeskWebHookHandler()
        {
            Receiver = ZendeskWebHookReceiver.ReceiverName;
        }

        public override Task ExecuteAsync(string generator, WebHookHandlerContext context)
        {
            ZendeskPost post = context.GetDataOrDefault<ZendeskPost>();

            // Implementation logic goes here
            return Task.FromResult(true);
        }
    }
}