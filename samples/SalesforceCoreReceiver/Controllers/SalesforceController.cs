using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks;
using Microsoft.Extensions.Logging;

namespace SalesforceCoreReceiver.Controllers
{
    // TODO: Test this sample more thoroughly.
    public class SalesforceController : ControllerBase
    {
        private readonly ILogger _logger;

        public SalesforceController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SalesforceController>();
        }

        [SalesforceWebHook(Id = "It")]
        public IActionResult SalesforceForIt(string @event, XElement data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok();
        }

        [SalesforceWebHook]
        public IActionResult Salesforce(string id, string @event, SalesforceNotifications data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation(
                0,
                "{ControllerName} / '{ReceiverId}' received {Count} notifications with ActionId '{ActionId}' (event " +
                "'{EventName}').",
                nameof(SalesforceController),
                id,
                data.Notifications.Count(),
                data.ActionId,
                @event);
            _logger.LogInformation(
                1,
                "Data contains OrganizationId '{OrganizationId}' and SessionId '{SessionId}'.",
                data.OrganizationId,
                data.SessionId);
            _logger.LogInformation(
                2,
                "Contained URLs include EnterpriseUrl '{EnterpriseUrl}' and PartnerUrl '{PartnerUrl}'.",
                data.EnterpriseUrl,
                data.PartnerUrl);

            var index = 0;
            foreach (var notification in data.Notifications)
            {
                _logger.LogInformation(
                    3,
                    "Notification #{Number} contained {Count} values.",
                    index,
                    notification.Count);
                index++;
            }

            return Ok();
        }
    }
}
