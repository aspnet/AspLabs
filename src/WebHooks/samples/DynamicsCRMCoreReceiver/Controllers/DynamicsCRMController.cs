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
                $"{nameof(DynamicsCRMController)} / 'It' received '{{MessageName}}'.",
                @event);

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
                $"{nameof(DynamicsCRMController)} / '{{Id}}' received '{{MessageName}}'.",
                id,
                @event);

            return Ok();
        }
    }
}
