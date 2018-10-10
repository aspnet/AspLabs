using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks;
using Newtonsoft.Json.Linq;

namespace AzureContainerRegistryCoreReceiver.Controllers
{
    public class AzureContainerRegistryController : ControllerBase
    {
        [AzureContainerRegistryWebHook(Id = "It")]
        public IActionResult AzureContainerRegistryForIt(string @event, JObject data)
        {
            // Get the registry
            var registry = data["request"]["host"].Value<string>();

            // Get the repository
            var repository = data["target"]["repository"].Value<string>();

            // Get the tag
            var tag = data["target"]["tag"].Value<string>();

            return Ok();
        }

        [AzureContainerRegistryWebHook]
        public IActionResult AzureContainerRegistry(string id, string @event, JObject data)
        {
            // Get the registry
            var registry = data["request"]["host"].Value<string>();

            // Get the repository
            var repository = data["target"]["repository"].Value<string>();

            // Get the tag
            var tag = data["target"]["tag"].Value<string>();

            return Ok();
        }
    }
}
