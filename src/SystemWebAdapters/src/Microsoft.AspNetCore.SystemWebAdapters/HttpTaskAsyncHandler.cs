// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web;

public abstract class HttpTaskAsyncHandler : IHttpAsyncHandler
{
    protected HttpTaskAsyncHandler()
    {
    }

    public virtual bool IsReusable => false;

    public abstract Task ProcessRequestAsync(HttpContext context);

    IAsyncResult IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback? callback, object? extraData)
    {
        var task = ProcessRequestAsync(context);

        if (callback is not null)
        {
            task.ContinueWith(r => callback(r));
        }

        // We wrap to ensure the extraData object is exposed as IAsyncResult.AsyncState
        return new TaskWrapper(task, extraData);
    }

    void IHttpAsyncHandler.EndProcessRequest(IAsyncResult result) => ((TaskWrapper)result).Finish();

    [EditorBrowsable(EditorBrowsableState.Never)]
    public virtual void ProcessRequest(HttpContext context)
        => throw new NotSupportedException($"IHttpHandler {GetType()} cannot be executed synchronously");

    private class TaskWrapper : IAsyncResult
    {
        private readonly Task _task;

        public TaskWrapper(Task task, object? state)
        {
            AsyncState = state;
            CompletedSynchronously = task.IsCompleted;

            _task = task;
        }

        public object? AsyncState { get; }

        public WaitHandle AsyncWaitHandle => ((IAsyncResult)_task).AsyncWaitHandle;

        public bool CompletedSynchronously { get; }

        public bool IsCompleted => _task.IsCompleted;

        /// <summary>
        /// Ensure task is actually complete and raise any exceptions encountered
        /// </summary>
        public void Finish() => _task.GetAwaiter().GetResult();
    }
}
