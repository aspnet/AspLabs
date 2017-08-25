using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks;

namespace MailChimpCoreReceiver.Controllers
{
    public class MailChimpController : ControllerBase
    {
        [MailChimpWebHook(Id = "It")]
        public IActionResult MailChimpForIt(IFormCollection data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the event name.
            var eventNames = data[MailChimpConstants.EventRequestPropertyName];

            return Ok();
        }

        [MailChimpWebHook]
        public IActionResult MailChimp(string id, IFormCollection data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the event name.
            var eventNames = data[MailChimpConstants.EventRequestPropertyName];

            return Ok();
        }
    }
}