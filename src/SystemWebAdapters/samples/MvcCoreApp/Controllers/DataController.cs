using System.Web.Adapters;
using ClassLibrary;
using Microsoft.AspNetCore.Mvc;

namespace MvcCoreApp.Controllers
{
    [PreBufferRequestStream]
    [BufferResponseStream]
    public class DataController : Controller
    {
        [HttpGet]
        [HttpPost]
        [Route("/api/data")]
        public void Get([FromQuery] bool? suppress = false) => RequestInfo.WriteRequestInfo(suppress ?? false);
    }
}
