using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace ApplicationInsights.AspNetCore.TelemetryInitializers
{
    public class ClientIpHeaderTelemetryInitializer : TelemetryInitializerBase
    {
        private readonly char[] HeaderValuesSeparatorDefault = new char[] { ',' };
        private const string HeaderNameDefault = "X-Forwarded-For";

        private char[] headerValueSeparators;

        private readonly ICollection<string> headerNames;


        public ClientIpHeaderTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
             : base(httpContextAccessor)
        {
            this.headerNames = new List<string>();
            this.HeaderNames.Add(HeaderNameDefault);
            this.UseFirstIp = true;
            this.headerValueSeparators = HeaderValuesSeparatorDefault;
        }

        /// <summary>
        /// Gets or sets comma separated list of request header names that is used to check client id.
        /// </summary>
        public ICollection<string> HeaderNames
        {
            get
            {
                return this.headerNames;
            }
        }

        /// <summary>
        /// Gets or sets a header values separator.
        /// </summary>
        public string HeaderValueSeparators
        {
            get
            {
                return string.Concat(this.headerValueSeparators);
            }

            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.headerValueSeparators = value.ToCharArray();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the first or the last IP should be used from the lists of IPs in the header.
        /// </summary>
        public bool UseFirstIp { get; set; }

        private static string CutPort(string address)
        {
            // For Web sites in Azure header contains ip address with port e.g. 50.47.87.223:54464
            int portSeparatorIndex = address.IndexOf(":", StringComparison.OrdinalIgnoreCase);

            if (portSeparatorIndex > 0)
            {
                return address.Substring(0, portSeparatorIndex);
            }

            return address;
        }

        private static bool IsCorrectIpAddress(string address)
        {
            IPAddress outParameter;
            address = address.Trim();

            // Core SDK does not support setting Location.Ip to malformed ip address
            if (IPAddress.TryParse(address, out outParameter))
            {
                // Also SDK supports only ipv4!
                if (outParameter.AddressFamily == AddressFamily.InterNetwork)
                {
                    return true;
                }
            }

            return false;
        }

        private string GetIpFromHeader(string clientIpsFromHeader)
        {
            var ips = clientIpsFromHeader.Split(this.headerValueSeparators, StringSplitOptions.RemoveEmptyEntries);
            return this.UseFirstIp ? ips[0].Trim() : ips[ips.Length - 1].Trim();
        }

        protected override void OnInitializeTelemetry(HttpContext platformContext, RequestTelemetry requestTelemetry, ITelemetry telemetry)
        {
            if (!string.IsNullOrEmpty(telemetry.Context.Location.Ip))
            {
                // Ip is already populated.
                //AspNetCoreEventSource.Instance.LogClientIpHeaderTelemetryInitializerOnInitializeTelemetryIpNull();
                return;
            }

            if (string.IsNullOrEmpty(requestTelemetry.Context.Location.Ip))
            {
                string resultIp = null;
                foreach (var name in this.HeaderNames)
                {
                    var headerValue = platformContext.Request.Headers[name];
                    if (!string.IsNullOrEmpty(headerValue))
                    {
                        var ip = GetIpFromHeader(headerValue);
                        ip = CutPort(ip);
                        if (IsCorrectIpAddress(ip))
                        {
                            resultIp = ip;
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(resultIp))
                {
                    var connectionFeature = platformContext.Features.Get<IHttpConnectionFeature>();

                    if (connectionFeature != null)
                    {
                        resultIp = connectionFeature.RemoteIpAddress.ToString();
                    }
                }

                requestTelemetry.Context.Location.Ip = resultIp;
            }

            telemetry.Context.Location.Ip = requestTelemetry.Context.Location.Ip;
        }
    }
}
