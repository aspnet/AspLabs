using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks;

namespace KuduCoreReceiver.Controllers
{
    public class KuduController : ControllerBase
    {
        [KuduWebHook(Id = "It")]
        public IActionResult KuduForIt(KuduNotification data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the notification message
            var message = data.Message;

            // Get the notification author
            var author = data.Author;

            return Ok();
        }

        [KuduWebHook]
        public IActionResult Kudu(string id, KuduNotification data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the notification message
            var message = data.Message;

            // Get the notification author
            var author = data.Author;

            return Ok();
        }
    }
}