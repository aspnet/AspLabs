using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks;
using Microsoft.Extensions.Logging;

namespace SalesforceCoreReceiver.Controllers
{
    public class SalesforceController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ISalesforceResultCreator _resultCreator;

        public SalesforceController(ILoggerFactory loggerFactory, ISalesforceResultCreator resultCreator)
        {
            _logger = loggerFactory.CreateLogger<SalesforceController>();
            _resultCreator = resultCreator;
        }

        [SalesforceWebHook(Id = "It")]
        public async Task<IActionResult> SalesforceForIt(string @event, XElement data)
        {
            if (!ModelState.IsValid)
            {
                return await _resultCreator.GetFailedResultAsync("Model binding failed.");
            }

            return await _resultCreator.GetSuccessResultAsync();
        }

        [SalesforceWebHook]
        public async Task<IActionResult> Salesforce(string id, string @event, SalesforceNotifications data)
        {
            if (!ModelState.IsValid)
            {
                return await _resultCreator.GetFailedResultAsync("Model binding failed.");
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

            return await _resultCreator.GetSuccessResultAsync();
        }
    }
}
