// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace System.Web.Adapters;

internal class BufferedHttpResponseFeature : Stream, IHttpResponseBodyFeature, IBufferedResponseFeature
{
    private readonly IHttpResponseBodyFeature _other;
    private readonly HttpResponseCore _response;
    private readonly Stream _stream;

    private PipeWriter? _pipeWriter;

    public BufferedHttpResponseFeature(HttpResponseCore response, IHttpResponseBodyFeature other)
    {
        _other = other;
        _response = response;

        if (other.Stream.CanSeek)
        {
            _stream = other.Stream;
            _pipeWriter = other.Writer;
        }
        else
        {
            _stream = new MemoryStream();
        }
    }

    public Stream Stream => this;

    public PipeWriter Writer => _pipeWriter ??= PipeWriter.Create(Stream, new StreamPipeWriterOptions(leaveOpen: true));

    public bool IsEnded { get; set; }

    public bool SuppressContent { get; set; }

    public async ValueTask FlushBufferedStreamAsync()
    {
        if (SuppressContent)
        {
            _stream.SetLength(0);
        }
        else if (!ReferenceEquals(_stream, _other.Stream))
        {
            _stream.Position = 0;
            await _stream.CopyToAsync(_other.Stream);
        }
    }

    public override bool CanRead
    {
        get
        {
            VerifyNotEnded();
            return _stream.CanRead;
        }
    }

    public override bool CanSeek
    {
        get
        {
            VerifyNotEnded();
            return _stream.CanSeek;
        }
    }

    public override bool CanWrite
    {
        get
        {
            VerifyNotEnded();
            return _stream.CanWrite;
        }
    }

    public override long Length
    {
        get
        {
            VerifyNotEnded();
            return _stream.Length;
        }
    }

    public override long Position
    {
        get
        {
            VerifyNotEnded();
            return _stream.Position;
        }

        set
        {
            VerifyNotEnded();
            _stream.Position = value;
        }
    }

    public Task CompleteAsync() => _other.CompleteAsync();

    public void DisableBuffering()
    {
        _other.DisableBuffering();
    }

    public override void Flush()
    {
        VerifyNotEnded();
        _stream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        VerifyNotEnded();
        return _stream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        VerifyNotEnded();
        return _stream.Seek(offset, origin);
    }

    public Task SendFileAsync(string path, long offset, long? count, CancellationToken cancellationToken = default)
        => _other.SendFileAsync(path, offset, count, cancellationToken);

    public override void SetLength(long value)
    {
        VerifyNotEnded();
        _stream.SetLength(value);
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
        => _other.StartAsync(cancellationToken);

    public override void Write(byte[] buffer, int offset, int count)
    {
        VerifyNotEnded();
        _stream.Write(buffer, offset, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void VerifyNotEnded()
    {
        if (IsEnded)
        {
            throw new InvalidOperationException("End() has been called on the response.");
        }
    }

    public void Clear()
    {
        _response.Clear();
    }

    public void ClearContent()
    {
        SetLength(0);
    }
}
