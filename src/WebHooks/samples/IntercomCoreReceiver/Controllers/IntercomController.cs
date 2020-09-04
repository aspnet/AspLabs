using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IntercomCoreReceiver.Controllers
{
    public class IntercomController : ControllerBase
    {
        private readonly ILogger _logger;

        public IntercomController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<IntercomController>();
        }

        [IntercomWebHook(Id = "It")]
        public IActionResult IntercomForIt(string @event, string notificationId, JObject data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok();
        }

        [IntercomWebHook]
        public IActionResult Intercom(string id, string @event, string notificationId, JObject dataObj)
        {

            var data = dataObj.ToObject<IntercomNotification>();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation(
                0,
                $"{nameof(IntercomController)} / '{{ReceiverId}}' received a '{{EventType}}' notification (event " +
                "'{EventName}').",
                id,
                data.Type,
                @event);

            _logger.LogInformation(
                1,
                "Data created at '{Created}' and contains Notification ID '{Id}' / '{NotificationId}', Topic " +
                "'{Topic}'.",
                data.CreatedAt,
                data.Id,
                notificationId,
                data.Topic);

            var details = data.Data.Item;

            return Ok();
        }
    }
}
