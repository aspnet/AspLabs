using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks;

namespace MailChimpReceiver.Selfhost.WebHooks
{
    public class MailChimpWebHookHandler : WebHookHandler
    {
        public MailChimpWebHookHandler()
        {
            this.Receiver = MailChimpWebHookReceiver.ReceiverName;
        }

        public override Task ExecuteAsync(string generator, WebHookHandlerContext context)
        {
            // Get Form data from WebHook
            // For more information about MailChimp WebHooks, please see 'https://apidocs.mailchimp.com/webhooks/'
            NameValueCollection data = context.GetDataOrDefault<NameValueCollection>();

            // Get the action for this WebHook coming from the action query parameter in the URI
            string action = context.Actions.FirstOrDefault();

            return Task.FromResult(true);
        }
    }
}