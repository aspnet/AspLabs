using System.Threading;
using System.Threading.Tasks;

namespace SampleWebApp
{
    public class ReallyWeirdAsyncService
    {
        private TaskCompletionSource<object> _asyncTask = new TaskCompletionSource<object>();

        public void Reset()
        {
            var oldTcs = Interlocked.Exchange(ref _asyncTask, new TaskCompletionSource<object>());
            oldTcs.TrySetCanceled();
        }

        public async Task WaitAsync()
        {
            var task = _asyncTask.Task;
            await task;
        }

        public void Release()
        {
            var tcs = _asyncTask;
            tcs.TrySetResult(null);
            Reset();
        }
    }
}
