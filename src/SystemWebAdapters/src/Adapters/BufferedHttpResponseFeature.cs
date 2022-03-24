// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;

namespace System.Web.Adapters;

internal class BufferedHttpResponseFeature : Stream, IHttpResponseBodyFeature, IBufferedResponseFeature
{
    public enum StreamState
    {
        NotStarted,
        Buffering,
        NotBuffering,
        Complete,
    }

    private readonly IHttpResponseBodyFeature _other;
    private readonly IBufferResponseStreamMetadata _metadata;

    private FileBufferingWriteStream? _bufferedStream;
    private PipeWriter? _pipeWriter;

    public BufferedHttpResponseFeature(IHttpResponseBodyFeature other, IBufferResponseStreamMetadata metadata)
    {
        _other = other;
        _metadata = metadata;
        State = StreamState.NotStarted;
    }

    public StreamState State { get; private set; }

    public Stream Stream => this;

    public PipeWriter Writer => _pipeWriter ??= PipeWriter.Create(this, new StreamPipeWriterOptions(leaveOpen: true));

    public bool SuppressContent { get; set; }

    private Stream CurrentStream
    {
        get
        {
            if (State == StreamState.NotBuffering)
            {
                return _other.Stream;
            }
            else if (State == StreamState.Complete)
            {
                return Stream.Null;
            }
            else
            {
                State = StreamState.Buffering;
                return _bufferedStream ??= new FileBufferingWriteStream(_metadata.MemoryThreshold, _metadata.BufferLimit);
            }
        }
    }

    public void End() => Task.Run(async () => await CompleteAsync()).GetAwaiter().GetResult();

    public override async ValueTask DisposeAsync()
    {
        if (_bufferedStream is not null)
        {
            await _bufferedStream.DisposeAsync();
        }

        await base.DisposeAsync();
    }

    public async ValueTask FlushBufferedStreamAsync()
    {
        if (State is StreamState.Buffering && _bufferedStream is not null && !SuppressContent)
        {
            await _bufferedStream.DrainBufferAsync(_other.Stream);
        }
    }

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => CurrentStream.Length;

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public async Task CompleteAsync()
    {
        await FlushBufferedStreamAsync();
        await _other.CompleteAsync();
        State = StreamState.Complete;
    }

    public void DisableBuffering()
    {
        if (State == StreamState.NotStarted)
        {
            State = StreamState.NotBuffering;
            _other.DisableBuffering();
            _pipeWriter = _other.Writer;
        }
    }

    public override void Flush() => _bufferedStream?.Flush();

    public override Task FlushAsync(CancellationToken cancellationToken)
        => _bufferedStream?.FlushAsync(cancellationToken) ?? Task.CompletedTask;

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public Task SendFileAsync(string path, long offset, long? count, CancellationToken cancellationToken = default)
        => SendFileFallback.SendFileAsync(CurrentStream, path, offset, count, cancellationToken);

    public override void SetLength(long value) => throw new NotSupportedException();

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (State == StreamState.NotStarted)
        {
            State = StreamState.Buffering;
        }

        return _other.StartAsync(cancellationToken);
    }

    public override void Write(byte[] buffer, int offset, int count) => CurrentStream.Write(buffer, offset, count);

    public override void Write(ReadOnlySpan<byte> buffer) => CurrentStream.Write(buffer);

    public override void WriteByte(byte value) => CurrentStream.WriteByte(value);

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        => CurrentStream.WriteAsync(buffer, cancellationToken);

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => CurrentStream.WriteAsync(buffer, offset, count, cancellationToken);

    public void ClearContent()
    {
        if (_bufferedStream is not null)
        {
            _bufferedStream.Dispose();
            _bufferedStream = null;
        }
    }
}
