using System.Web;
using System.Web.Adapters;
using ClassLibrary;
using Microsoft.AspNetCore.Mvc;

namespace MvcCoreApp.Controllers
{
    [PreBufferRequestStream]
    public class DataController : Controller
    {
        [HttpGet]
        [Route("/api/data")]
        public RequestInfo Get() => RequestInfo.Current;

        [Route("/api/data")]
        [HttpPost]
        public RequestInfo Post() => RequestInfo.Current;

        [HttpGet]
        [Route("/api/data2")]
        [PreBufferRequestStream(IsEnabled = false)]
        public RequestInfo Get2() => RequestInfo.Current;
    }
}
