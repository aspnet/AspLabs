using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks;

namespace AzureAlertCoreReceiver.Controllers
{
    public class AzureAlertController : ControllerBase
    {
        [AzureAlertWebHook(Id = "It")]
        public IActionResult AzureAlertForIt(AzureAlertNotification data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the notification status
            var status = data.Status;

            // Get the notification name
            var name = data.Context.Name;

            // Get the name of the metric that caused the event
            var author = data.Context.Condition.MetricName;

            return Ok();
        }

        [AzureAlertWebHook]
        public IActionResult AzureAlert(string id, AzureAlertNotification data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the notification status
            var status = data.Status;

            // Get the notification name
            var name = data.Context.Name;

            // Get the name of the metric that caused the event
            var author = data.Context.Condition.MetricName;

            return Ok();
        }
    }
}