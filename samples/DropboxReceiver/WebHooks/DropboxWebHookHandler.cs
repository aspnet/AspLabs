using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks;
using Newtonsoft.Json.Linq;

namespace DropboxReceiver.WebHooks
{
    public class DropboxWebHookHandler : WebHookHandler
    {
        public DropboxWebHookHandler()
        {
            this.Receiver = DropboxWebHookReceiver.ReceiverName;
        }

        public override Task ExecuteAsync(string generator, WebHookHandlerContext context)
        {
            // For more information about Dropbox WebHook payloads, please see 
            // 'https://www.dropbox.com/developers/reference/webhooks'
            JObject entry = context.GetDataOrDefault<JObject>();

            return Task.FromResult(true);
        }
    }
}