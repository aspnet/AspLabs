using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks;

namespace AzureReceivers.WebHooks
{
    public class AzureAlertWebHookHandler : WebHookHandler
    {
        public AzureAlertWebHookHandler()
        {
            Receiver = "azurealert";
        }

        public override Task ExecuteAsync(string generator, WebHookHandlerContext context)
        {
            // For more information about Azure Alert WebHook payloads, please see 
            // 'https://azure.microsoft.com/en-us/documentation/articles/insights-webhooks-alerts/'
            AzureAlertNotification notification = context.GetDataOrDefault<AzureAlertNotification>();

            // Get the notification status
            string status = notification.Status;

            // Get the notification name
            string name = notification.Context.Name;

            // Get the name of the metric that caused the event
            string author = notification.Context.Condition.MetricName;

            return Task.FromResult(true);
        }
    }
}