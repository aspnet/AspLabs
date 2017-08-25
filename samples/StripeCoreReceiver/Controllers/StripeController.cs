using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace StripeCoreReceiver.Controllers
{
    public class StripeController : ControllerBase
    {
        private readonly ILogger _logger;

        public StripeController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<StripeController>();
        }

        [StripeWebHook(Id = "It")]
        public IActionResult StripeForIt(string @event, JObject data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok();
        }

        [StripeWebHook]
        public IActionResult Stripe(string id, string @event, StripeEvent data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation(
                0,
                "{ControllerName} / '{ReceiverId}' received a '{EventType}' notification (event '{EventName}').",
                nameof(StripeController),
                id,
                data.EventType,
                @event);

            _logger.LogInformation(
                1,
                "Data created at '{Created}' and contains Notification ID '{Id}', Live mode '{DetailsLiveMode}', " +
                "and Request ID '{RequestId}'.",
                data.Created,
                data.Id,
                data.LiveMode,
                data.Request);

            var details = data.Data.Object;
            var created = DateTimeOffset.FromUnixTimeMilliseconds(
                details.Value<long>(StripeConstants.CreatedPropertyName));
            _logger.LogInformation(
                2,
                "Event detail created at '{DetailsCreated}' and contains {PropertyCount} properties, including " +
                "Account '{Account}', Id '{DetailsId}', Live mode '{DetailsLiveMode}', and Name '{Name}'.",
                created,
                details.Count,
                details.Value<string>("account"),
                details.Value<string>("id"),
                details.Value<string>(StripeConstants.LiveModePropertyName),
                details.Value<string>("name"));

            return Ok();
        }
    }
}
