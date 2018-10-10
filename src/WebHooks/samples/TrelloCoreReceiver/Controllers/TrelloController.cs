using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace TrelloCoreReceiver.Controllers
{
    public class TrelloController : ControllerBase
    {
        private readonly ILogger _logger;

        public TrelloController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TrelloController>();
        }

        [TrelloWebHook(Id = "It")]
        public IActionResult TrelloForIt(string @event, JObject data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation(
                0,
                $"{nameof(TrelloController)} / 'It' received '{{MessageName}}'.",
                @event);

            return Ok();
        }

        [TrelloWebHook]
        public IActionResult Trello(string id, string @event, JObject data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation(
                1,
                $"{nameof(TrelloController)} / '{{Id}}' received '{{MessageName}}'.",
                id,
                @event);

            return Ok();
        }
    }
}
