using System.Collections.Specialized;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks;

namespace StripeReceiver.WebHooks
{
    public class StripeWebHookHandler : WebHookHandler
    {
        public StripeWebHookHandler()
        {
            this.Receiver = StripeWebHookReceiver.ReceiverName;
        }

        public override Task ExecuteAsync(string generator, WebHookHandlerContext context)
        {
            // For more information about Stripe WebHook payloads, please see 
            // 'https://stripe.com/docs/webhooks'
            StripeEvent entry = context.GetDataOrDefault<StripeEvent>();

            // We can trace to see what is going on.
            Trace.WriteLine(entry.ToString());

            // Switch over the event types if you want to
            switch (entry.EventType)
            {
                default:
                    // Information can be returned in a plain text response
                    context.Response = context.Request.CreateResponse();
                    context.Response.Content = new StringContent(string.Format("Hello {0} event!", entry.EventType));
                    break;
            }

            return Task.FromResult(true);
        }
    }
}