using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace SOFTTEK.SCMS.SRA.Controllers
{
    public abstract class BaseApiController : ApiController
    {
        protected string GetDeviceIdentifier()
        {
            string authHeaderValue = null;
            var authRequest = ActionContext.HttpContext.CreateHttpRequestMessage().Headers.Authorization;
            if (authRequest != null && !String.IsNullOrEmpty(authRequest.Scheme) && authRequest.Scheme == "Basic")
                authHeaderValue = authRequest.Parameter;
            if (string.IsNullOrEmpty(authHeaderValue))
                return null;

            authHeaderValue = Encoding.Default.GetString(Convert.FromBase64String(authHeaderValue));
            var credentials = authHeaderValue.Split(':');

            if (credentials.Length == 2)
            {
                string deviceIdentifier = credentials[0];
                return deviceIdentifier;
            }

            return null;
        }

        public IHttpActionResult Error(Exception ex)
        {
            IHttpActionResult result = Content(HttpStatusCode.Conflict, ex);
            return result;
        }
    }
}
