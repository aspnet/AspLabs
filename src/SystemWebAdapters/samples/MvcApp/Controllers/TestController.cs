using System.Web;
using System.Web.Http;
using ClassLibrary;

namespace MvcApp.Controllers
{
    [RoutePrefix("api/test")]
    public class TestController : ApiController
    {
        [Route("request/info")]
        [HttpGet]
        public void GetData() => RequestInfo.WriteRequestInfo(false);

        [Route("request/cookie")]
        [HttpGet]
        public void TestRequestCookie() => CookieTests.RequestCookies(HttpContext.Current);

        [Route("response/cookie")]
        [HttpGet]
        public void TestResponseCookie() => CookieTests.ResponseCookies(HttpContext.Current);
    }
}
