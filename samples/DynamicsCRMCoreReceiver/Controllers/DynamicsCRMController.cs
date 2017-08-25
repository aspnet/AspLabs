using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DynamicsCRMCoreReceiver.Controllers
{
    // TODO: Test this sample more thoroughly.
    public class DynamicsCRMController : ControllerBase
    {
        private readonly ILogger _logger;

        public DynamicsCRMController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DynamicsCRMController>();
        }

        [DynamicsCRMWebHook(Id = "It")]
        public IActionResult DynamicsCRMForIt(JObject data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var messageName = data.Value<string>(DynamicsCRMConstants.EventRequestPropertyName);
            _logger.LogInformation(
                0,
                "{ControllerName} received '{MessageName}' for '{Id}'.",
                nameof(DynamicsCRMController),
                messageName,
                "It");

            return Ok();
        }

        [DynamicsCRMWebHook]
        public IActionResult DynamicsCRM(string id, JObject data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var messageName = data.Value<string>(DynamicsCRMConstants.EventRequestPropertyName);
            _logger.LogInformation(
                1,
                "{ControllerName} received '{MessageName}' for '{Id}'.",
                nameof(DynamicsCRMController),
                messageName,
                id);

            return Ok();
        }
    }
}