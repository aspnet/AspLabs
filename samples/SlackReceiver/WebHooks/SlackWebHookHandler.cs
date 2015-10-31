using System.Collections.Specialized;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks;

namespace SlackReceiver.WebHooks
{
    public class SlackWebHookHandler : WebHookHandler
    {
        public SlackWebHookHandler()
        {
            this.Receiver = "slack";
        }

        public override Task ExecuteAsync(string generator, WebHookHandlerContext context)
        {
            // For more information about Slack WebHook payloads, please see 
            // 'https://api.slack.com/outgoing-webhooks'
            NameValueCollection entry = context.GetDataOrDefault<NameValueCollection>();

            // We can trace to see what is going on.
            Trace.WriteLine(entry.ToString());

            // Switch over the IDs we used when configuring this WebHook 
            switch (context.Id)
            {
                case "trigger":
                    // Information can be returned using a SlackResponse
                    var triggerReply = new SlackResponse("Hello trigger!");
                    context.Response = context.Request.CreateResponse(triggerReply);
                    break;

                case "slash":
                    // Information can be returned in a plain text response
                    context.Response = context.Request.CreateResponse();
                    context.Response.Content = new StringContent("Hello slash command!");
                    break;

            }

            return Task.FromResult(true);
        }
    }
}