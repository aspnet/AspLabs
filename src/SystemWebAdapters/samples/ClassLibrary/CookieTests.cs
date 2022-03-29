using System.Web;

namespace ClassLibrary;

public class CookieTests
{
    public static void RequestCookies(HttpContext context)
    {
        using (var writer = new SimpleJsonWriter(context.Response))
        {
            writer.Write("InitialCount", context.Request.Cookies.Count);
            writer.Write("InitialHeader", context.Request.Headers["cookie"]);

            // Add cookie
            context.Request.Cookies.Add(new HttpCookie("cookie1", "cookie1value"));
            writer.Write("AfterAddCount", context.Request.Cookies.Count);
            writer.Write("AfterAddHeader", context.Request.Headers["cookie"]);

            // remove cookie
            context.Request.Cookies.Remove("ASP.NET_SessionId");
            writer.Write("AfterAddCount", context.Request.Cookies.Count);
            writer.Write("AfterAddHeader", context.Request.Headers["cookie"]);
        }

        context.Response.End();
    }

    public static void ResponseCookies(HttpContext context)
    {
        using (var writer = new SimpleJsonWriter(context.Response))
        {
            writer.Write("InitialCount", context.Response.Cookies.Count);
            writer.Write("InitialHeader", context.Response.Headers["cookie"]);

            // Add cookie
            context.Response.Cookies.Add(new HttpCookie("cookie1", "cookie1value"));
            writer.Write("AfterAddCount", context.Response.Cookies.Count);
            writer.Write("AfterAddHeader", context.Response.Headers["set-cookie"]);
        }

        context.Response.End();
    }
}
