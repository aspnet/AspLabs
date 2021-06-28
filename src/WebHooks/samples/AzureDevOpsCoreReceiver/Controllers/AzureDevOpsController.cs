using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace AzureDevOpsStronglyTypedCoreReceiver.Controllers
{
    public class AzureDevOpsController : ControllerBase
    {
        private readonly ILogger _logger;

        public AzureDevOpsController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AzureDevOpsController>();
        }

        [AzureDevOpsWebHook]
        public IActionResult AzureDevOps(string id, string @event, string webHookId, JObject data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation(
                1,
                $"{nameof(AzureDevOpsController)} / '{{Id}}' received '{{EventName}}' and '{{WebHookId}}'.",
                id,
                @event,
                webHookId);

            return Ok();
        }
    }
}
