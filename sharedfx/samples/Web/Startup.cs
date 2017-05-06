using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;

namespace Web
{
    public class Startup
    {
        public static void Main()
        {
            WebHost.Start(context => context.Response.WriteAsync("Hello, World!"));
        }
    }
}
