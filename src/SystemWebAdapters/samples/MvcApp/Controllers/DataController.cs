using System.Web.Http;
using ClassLibrary;

namespace MvcApp.Controllers
{
    [RoutePrefix("api/data")]
    public class DataController : ApiController
    {
        [Route("")]
        [HttpGet]
        public RequestInfo GetData() => RequestInfo.Current;

        [Route("")]
        [HttpPost]
        public RequestInfo Post() => RequestInfo.Current;
    }
}
