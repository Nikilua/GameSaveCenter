using System;
using System.Threading;
using System.Threading.Tasks;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Runs only the latest validation request and suppresses late success/failure callbacks.
    /// The validator receives a cancellation token but may still finish after cancellation when
    /// an underlying file-system call cannot be interrupted.
    /// </summary>
    internal sealed class LatestAsyncValidationCoordinator<TRequest, TResult> : IDisposable
    {
        private readonly object sync = new object();
        private CancellationTokenSource? activeCancellation;
        private long version;
        private bool disposed;

        internal long Start(
            TRequest request,
            Func<TRequest, CancellationToken, Task<TResult>> validate,
            Action<long, TResult> apply,
            Action<long, Exception> fail)
        {
            if (validate == null) throw new ArgumentNullException(nameof(validate));
            if (apply == null) throw new ArgumentNullException(nameof(apply));
            if (fail == null) throw new ArgumentNullException(nameof(fail));

            CancellationTokenSource cancellation;
            long requestVersion;
            lock (sync)
            {
                if (disposed) return 0;
                activeCancellation?.Cancel();
                cancellation = new CancellationTokenSource();
                activeCancellation = cancellation;
                requestVersion = ++version;
            }

            _ = RunAsync(request, requestVersion, cancellation, validate, apply, fail);
            return requestVersion;
        }

        internal void Cancel()
        {
            lock (sync)
            {
                version++;
                activeCancellation?.Cancel();
            }
        }

        public void Dispose()
        {
            lock (sync)
            {
                if (disposed) return;
                disposed = true;
                version++;
                activeCancellation?.Cancel();
            }
        }

        private async Task RunAsync(
            TRequest request,
            long requestVersion,
            CancellationTokenSource cancellation,
            Func<TRequest, CancellationToken, Task<TResult>> validate,
            Action<long, TResult> apply,
            Action<long, Exception> fail)
        {
            try
            {
                var result = await validate(request, cancellation.Token).ConfigureAwait(false);
                if (IsCurrent(requestVersion, cancellation))
                    apply(requestVersion, result);
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
                // Cancellation is expected when a newer field version or page lifecycle event wins.
            }
            catch (Exception ex)
            {
                if (IsCurrent(requestVersion, cancellation))
                    fail(requestVersion, ex);
            }
            finally
            {
                lock (sync)
                {
                    if (ReferenceEquals(activeCancellation, cancellation))
                        activeCancellation = null;
                }
                cancellation.Dispose();
            }
        }

        private bool IsCurrent(long requestVersion, CancellationTokenSource cancellation)
        {
            lock (sync)
            {
                return !disposed
                    && requestVersion == version
                    && ReferenceEquals(activeCancellation, cancellation)
                    && !cancellation.IsCancellationRequested;
            }
        }
    }
}
