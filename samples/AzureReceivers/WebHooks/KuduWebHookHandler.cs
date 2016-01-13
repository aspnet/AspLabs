using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks;

namespace AzureReceivers.WebHooks
{
    public class KuduWebHookHandler : WebHookHandler
    {
        public KuduWebHookHandler()
        {
            Receiver = KuduWebHookReceiver.ReceiverName;
        }

        public override Task ExecuteAsync(string generator, WebHookHandlerContext context)
        {
            // For more information about Azure Kudu WebHook payloads, please see 
            // 'https://github.com/projectkudu/kudu/wiki/Web-hooks'
            KuduNotification notification = context.GetDataOrDefault<KuduNotification>();

            // Get the notification message
            string message = notification.Message;

            // Get the notification author
            string author = notification.Author;

            return Task.FromResult(true);
        }
    }
}