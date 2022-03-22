using System;
using System.Web;

namespace ClassLibrary
{
    public class RequestInfo
    {
        public static void WriteRequestInfo(bool suppress)
        {
            var context = HttpContext.Current;

            using (var writer = new SimpleJsonWriter(context.Response))
            {
                writer.Write("Path", context.Request.Path);
                writer.Write("Length", context.Request.InputStream.Length);
                writer.Write("Charset", context.Response.Charset);
                writer.Write("StatusCode", context.Response.StatusCode);
                writer.Write("StatusDescription", context.Response.StatusDescription);
            }

            context.Response.SuppressContent = suppress;
            context.Response.End();
        }

        private struct SimpleJsonWriter : IDisposable
        {
            private readonly HttpResponse _response;
            private bool _hasWritten;

            public SimpleJsonWriter(HttpResponse response)
            {
                _response = response;
                _hasWritten = false;

                _response.Output.WriteLine("{");
            }

            public void Dispose()
            {
                if (_hasWritten)
                {
                    _response.Output.WriteLine();
                }

                _response.Output.WriteLine("}");
            }

            public void Write<T>(string name, T item)
            {
                if (_hasWritten)
                {
                    _response.Output.WriteLine(",");
                }

                _hasWritten = true;
                _response.Write("  ");
                _response.Write('\"');
                _response.Write(name);
                _response.Write("\" : \"");
                _response.Write(item);
                _response.Output.Write('\"');
            }
        }
    }
}
