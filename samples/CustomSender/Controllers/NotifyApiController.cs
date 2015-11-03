using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CustomSender.Controllers
{
    [Authorize]
    public class NotifyApiController : ApiController
    {
        public async Task<IHttpActionResult> Post()
        {
            // Create an event with 'event2' and additional data
            await this.NotifyAsync("event2", new { P1 = "p1" });
            return Ok();
        }
    }
}
