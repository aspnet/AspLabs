using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DynamicsCRMCoreReceiver.Controllers
{
    public class DynamicsCRMController : ControllerBase
    {
        private readonly ILogger _logger;

        public DynamicsCRMController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DynamicsCRMController>();
        }

        [DynamicsCRMWebHook(Id = "It")]
        public IActionResult DynamicsCRMForIt(string @event, JObject data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation(
                0,
                "{ControllerName} received '{MessageName}' for '{Id}'.",
                nameof(DynamicsCRMController),
                @event,
                "It");

            return Ok();
        }

        [DynamicsCRMWebHook]
        public IActionResult DynamicsCRM(string id, string @event, JObject data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation(
                1,
                "{ControllerName} received '{MessageName}' for '{Id}'.",
                nameof(DynamicsCRMController),
                @event,
                id);

            return Ok();
        }
    }
}
