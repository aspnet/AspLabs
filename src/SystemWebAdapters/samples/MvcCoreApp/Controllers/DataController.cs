using System.Web;
using ClassLibrary;
using Microsoft.AspNetCore.Mvc;

namespace MvcCoreApp.Controllers
{
    [SystemWebAdapter]
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
        [SystemWebAdapter(Enabled = false)]
        public RequestInfo Get2() => RequestInfo.Current;
    }
}
