using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace System.Web.Http
{
    public class ShimJsonResult : JsonResult, IHttpActionResult
    {
        public ShimJsonResult(object value) : base(value)
        {
        }

        public ShimJsonResult(object value, JsonSerializerSettings serializerSettings) : base(value, serializerSettings)
        {
        }
    }
}
