using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace BitbucketStronglyTypedCoreReceiver.Controllers
{
    public class BitbucketController : ControllerBase
    {
        private readonly ILogger _logger;

        public BitbucketController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<BitbucketController>();
        }

        [BitbucketWebHook(Id = "It")]
        public IActionResult BitbucketForIt(string @event, string webHookId, JObject data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation(
                0,
                $"{nameof(BitbucketController)} / 'It' received '{{EventName}}' and '{{WebHookId}}'.",
                @event,
                webHookId);

            return Ok();
        }

        [BitbucketWebHook(EventName = "repo:push")]
        public IActionResult BitbucketForPush(string id, string webHookId, BitbucketPushNotification data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Extract information about the repository
            var repository = data.Repository;

            // Information about the user causing the event
            var actor = data.Actor;

            // Information about the specific changes
            foreach (var change in data.Push.Changes)
            {
                // The previous commit
                var oldTarget = change.Old.Target;

                // The new commit
                var newTarget = change.New.Target;
            }

            return Ok();
        }

        [BitbucketWebHook]
        public IActionResult Bitbucket(string id, string @event, string webHookId, JObject data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation(
                1,
                $"{nameof(BitbucketController)} / '{{Id}}' received '{{EventName}}' and '{{WebHookId}}'.",
                id,
                @event,
                webHookId);

            return Ok();
        }
    }
}
