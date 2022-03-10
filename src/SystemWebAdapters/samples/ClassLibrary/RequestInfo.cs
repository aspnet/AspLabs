using System.Web;

namespace ClassLibrary
{
    public class RequestInfo
    {
        public string Path { get; set; }

        public long Length { get; set; }

        public static RequestInfo Current
        {
            get
            {
                var context = HttpContext.Current;

                return new RequestInfo
                {
                    Path = context.Request.Path,
                    Length = context.Request.InputStream.Length,
                };
            }
        }
    }
}
