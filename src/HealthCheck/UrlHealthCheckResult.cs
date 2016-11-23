using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace HealthChecks
{
    class UrlHealthCheckResult : IHealthCheckResult
    {
        public string Name {get;set;}

        public CheckStatus CheckStatus { get; set; }

        public long ResponseMilliseconds { get; set;  }

        public HttpStatusCode StatusCode { get; set; }

        public string Url { get; set; }
    }
}
