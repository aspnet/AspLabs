using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace System.Web.Http
{
    public sealed class ShimStatusCodeResult : StatusCodeResult, IHttpActionResult
    {
        public ShimStatusCodeResult(HttpStatusCode statusCode) : base((int)statusCode)
        {
        }
    }
}
