using System.Web;
using ClassLibrary;
using Microsoft.AspNetCore.Mvc;

namespace MvcCoreApp.Controllers
{
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
        public RequestInfo Get2() => RequestInfo.Current;
    }
}
