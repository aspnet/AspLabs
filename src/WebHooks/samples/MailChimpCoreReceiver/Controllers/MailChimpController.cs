using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks;

namespace MailChimpCoreReceiver.Controllers
{
    public class MailChimpController : ControllerBase
    {
        [MailChimpWebHook(Id = "It")]
        public IActionResult MailChimpForIt(string @event, IFormCollection data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok();
        }

        [MailChimpWebHook]
        public IActionResult MailChimp(string id, string @event, IFormCollection data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok();
        }
    }
}
