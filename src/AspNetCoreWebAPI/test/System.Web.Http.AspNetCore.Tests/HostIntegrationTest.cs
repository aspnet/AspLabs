// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Web.Http.Dispatcher;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;

namespace System.Web.Http.AspNetCore
{
    public class HostIntegrationTest : IDisposable
    {
        private readonly IHost _host;

        public HostIntegrationTest()
        {
            _host = GetHost();
            var testServer = _host.GetTestServer();
            DefaultClient = testServer.CreateClient();
        }

        public HttpClient DefaultClient { get; }

        [Fact]
        public async Task SimpleGet_Works()
        {
            var response = await DefaultClient.GetAsync("HelloWorld/Get");

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("\"Hello from ASP.NET Core\"", await response.Content.ReadAsStringAsync());
            Assert.Null(response.Headers.TransferEncodingChunked);
        }

        [Fact]
        public async Task SimplePost_Works()
        {
            var content = new StringContent("\"Echo this\"", Encoding.UTF8, "application/json");

            var response = await DefaultClient.PostAsync("Echo/Post", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
            }
            Assert.Equal("\"Echo this\"", await response.Content.ReadAsStringAsync());
            Assert.Null(response.Headers.TransferEncodingChunked);
        }

        [Fact]
        public async Task GetThatThrowsDuringSerializations_RespondsWith500()
        {
            var response = await DefaultClient.GetAsync("Error/Get");

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            JObject json = Assert.IsType<JObject>(JToken.Parse(await response.Content.ReadAsStringAsync()));
            JToken exceptionMessage;
            Assert.True(json.TryGetValue("ExceptionMessage", out exceptionMessage));
            Assert.Null(response.Headers.TransferEncodingChunked);
        }

        [Fact]
        public async Task EchoPocoTypesDefaultFormatterWorks()
        {
            var input = new SomePoco
            {
                Id = 7,
                Name = "testName",
            };
            var request = new HttpRequestMessage(HttpMethod.Post, "Echo/EchoPoco");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new ObjectContent<SomePoco>(input, new JsonMediaTypeFormatter());

            var response = await DefaultClient.SendAsync(request);

            Assert.True(response.IsSuccessStatusCode);
            var echo = await response.Content.ReadAsAsync<SomePoco>(new[] { new JsonMediaTypeFormatter() });
            Assert.Equal(input.Id, echo.Id);
            Assert.Equal(input.Name, echo.Name);
        }

        [Fact]
        public async Task SystemTextJsonFormattersWorks()
        {
            using var host = GetHost(typeof(TestStartupWithSystemTextJsonFormatting));
            var testServer = host.GetTestServer();
            var client = testServer.CreateClient();

            // We should be able to round-trip values using the new S.T.J extensions
            var response = await System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync(client, "Echo/EchoPoco", new SomePoco
            {
                Id = 15,
                Name = "test123",
            });

            var echo = await response.Content.ReadFromJsonAsync<SomePoco>();
            Assert.Equal(15, echo.Id);
            Assert.Equal("test123", echo.Name);
        }

        private class TestStartup
        {
            public void Configure(IApplicationBuilder appBuilder)
            {
                var config = new HttpConfiguration();
                config.Services.Replace(typeof(IAssembliesResolver), new TestAssemblyResolver());
                config.Routes.MapHttpRoute("Default", "{controller}/{action}");
                appBuilder.UseWebApi(config);
            }
        }

        private class TestStartupWithSystemTextJsonFormatting
        {
            public void Configure(IApplicationBuilder appBuilder)
            {
                var config = new HttpConfiguration();
                config.Services.Replace(typeof(IAssembliesResolver), new TestAssemblyResolver());
                config.Formatters.Insert(0, new Net.Http.Formatting.SystemTextJsonMediaTypeFormatter());
                config.Routes.MapHttpRoute("Default", "{controller}/{action}");
                appBuilder.UseWebApi(config);
            }
        }

        private static IHost GetHost(Type startup = null)
        {
            startup ??= typeof(TestStartup);
            var host = new HostBuilder()
                .UseEnvironment(Environments.Development)
                .ConfigureWebHost(b => b.UseStartup(startup).UseTestServer())
                .Build();
            host.Start();
            return host;
        }

        public void Dispose()
        {
            _host.Dispose();
        }

        private sealed class TestAssemblyResolver : IAssembliesResolver
        {
            public ICollection<Assembly> GetAssemblies()
            {
                // Running the tests in Arcade has a weird issue where the test assembly gets loaded multiple times
                // all of which get used during controller discovery and result in failure.
                // Working around it with a test-only solution.
                var testAssembly = AssemblyLoadContext.Default.LoadFromAssemblyName(GetType().Assembly.GetName());
                return new[] { testAssembly };
            }
        }
    }

    public class HelloWorldController : ApiController
    {
        public string Get()
        {
            return "Hello from ASP.NET Core";
        }
    }

    public class EchoController : ApiController
    {
        [HttpPost]
        public string Post([FromBody] string s)
        {
            return s;
        }

        [HttpPost]
        public SomePoco EchoPoco([FromBody] SomePoco poco)
        {
            return poco;
        }
    }

    public class SomePoco
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class ErrorController : ApiController
    {
        public ExceptionThrower Get()
        {
            return new ExceptionThrower();
        }

        public class ExceptionThrower
        {
            public string Throws
            {
                get
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}
