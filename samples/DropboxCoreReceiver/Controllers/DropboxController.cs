using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks;
using Newtonsoft.Json.Linq;

namespace DropboxCoreReceiver.Controllers
{
    // TODO: Test this sample more thoroughly.
    public class DropboxController : ControllerBase
    {
        [DropboxWebHook(Id = "It")]
        public IActionResult DropboxForIt(JObject data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok();
        }

        [DropboxWebHook]
        public IActionResult Dropbox(string id, JObject data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok();
        }
    }
}