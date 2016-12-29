using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks;

namespace SalesforceReceiver.WebHooks
{
    public class SalesforceWebHookHandler : WebHookHandler
    {
        public SalesforceWebHookHandler()
        {
            this.Receiver = SalesforceSoapWebHookReceiver.ReceiverName;
        }

        public override Task ExecuteAsync(string receiver, WebHookHandlerContext context)
        {
            SalesforceNotifications updates = context.GetDataOrDefault<SalesforceNotifications>();
            string sessionId = updates.SessionId;
            string company = updates.Notifications.FirstOrDefault()?["Company"];
            return Task.FromResult(true);
        }
    }
}
