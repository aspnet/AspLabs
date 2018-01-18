using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks;
using Microsoft.Extensions.Logging;

namespace WordPressCoreReceiver.Controllers
{
    public class WordPressController : ControllerBase
    {
        private readonly ILogger _logger;

        public WordPressController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<WordPressController>();
        }

        [WordPressWebHook(Id = "It")]
        public IActionResult WordPressForIt(string @event, IFormCollection data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation(
               0,
               "{ControllerName} / '{ReceiverId}' received {Count} properties with event '{EventName}').",
               nameof(WordPressController),
               "It",
               data.Count,
               @event);
            foreach (var keyValuePair in data)
            {
                if (string.Equals(
                    WordPressConstants.EventBodyPropertyPath,
                    keyValuePair.Key,
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                _logger.LogInformation(
                    1,
                    "{FieldName}: {FieldValue}",
                    keyValuePair.Key,
                    keyValuePair.Value.ToString());
            }

            return Ok();
        }

        [WordPressWebHook]
        public IActionResult WordPress(string id, string @event, IFormCollection data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation(
               2,
               "{ControllerName} / '{ReceiverId}' received {Count} properties with event '{EventName}').",
               nameof(WordPressController),
               id,
               data.Count,
               @event);
            foreach (var keyValuePair in data)
            {
                if (string.Equals(
                    WordPressConstants.EventBodyPropertyPath,
                    keyValuePair.Key,
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                _logger.LogInformation(
                    3,
                    "{FieldName}: {FieldValue}",
                    keyValuePair.Key,
                    keyValuePair.Value.ToString());
            }

            return Ok();
        }
    }
}
