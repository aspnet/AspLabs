using ClassLibrary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SystemWebAdapters;

namespace MvcCoreApp.Controllers
{
    [PreBufferRequestStream]
    [BufferResponseStream]
    public class TestController : Controller
    {
        [HttpGet]
        [Session(IsReadOnly = true)]
        [Route("/api/test/request/info")]
        public void Get([FromQuery] bool? suppress = false) => RequestInfo.WriteRequestInfo(suppress ?? false);

        [Route("/api/test/request/cookie")]
        public void TestRequestCookie() => CookieTests.RequestCookies(HttpContext);

        [Route("/api/test/response/cookie")]
        [HttpGet]
        public void TestResponseCookie() => CookieTests.ResponseCookies(HttpContext);
    }
}
