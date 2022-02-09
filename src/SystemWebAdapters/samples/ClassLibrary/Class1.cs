using System.Web;

namespace ClassLibrary
{
    public static class Helper
    {
        public static string UserAgent => HttpContext.Current.Request.UserAgent;

        public static string GetUserAgent(HttpContext context) => context.Request.UserAgent;
    }
}
