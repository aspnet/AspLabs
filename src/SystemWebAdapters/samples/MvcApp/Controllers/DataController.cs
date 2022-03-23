using System.Web.Http;
using ClassLibrary;

namespace MvcApp.Controllers
{
    [RoutePrefix("api/data")]
    public class DataController : ApiController
    {
        [Route("")]
        [HttpGet]
        [HttpPost]
        public void GetData() => RequestInfo.WriteRequestInfo(true);
    }
}
