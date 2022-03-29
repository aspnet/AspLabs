using System;
using System.Web;

namespace ClassLibrary;

internal struct SimpleJsonWriter : IDisposable
{
    private readonly HttpResponse _response;
    private bool _hasWritten;

    public SimpleJsonWriter(HttpResponse response)
    {
        response.ContentType = "application/json";

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
