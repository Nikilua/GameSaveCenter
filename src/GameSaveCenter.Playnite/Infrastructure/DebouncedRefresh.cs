using System;
using System.Threading;
using System.Threading.Tasks;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Lightweight search debouncer. Rapid text changes cancel the previous pending refresh
    /// and schedule one final callback; clearing the query refreshes immediately. Call
    /// <see cref="Cancel"/> when the owning page or ViewModel unloads to avoid timer leaks.
    /// The callback is responsible for marshalling back to the UI thread when needed.
    /// </summary>
    public sealed class DebouncedRefresh
    {
        private readonly Action refresh;
        private readonly TimeSpan delay;
        private CancellationTokenSource? pending;

        public DebouncedRefresh(Action refresh, TimeSpan delay)
        {
            this.refresh = refresh ?? throw new ArgumentNullException(nameof(refresh));
            this.delay = delay > TimeSpan.Zero ? delay : TimeSpan.FromMilliseconds(180);
        }

        public void Schedule(string? query)
        {
            CancelPending();
            if (string.IsNullOrEmpty(query))
            {
                refresh();
                return;
            }

            var cancellation = new CancellationTokenSource();
            pending = cancellation;
            _ = RunAsync(cancellation.Token);
        }

        public void Cancel()
        {
            CancelPending();
        }

        private void CancelPending()
        {
            var current = Interlocked.Exchange(ref pending, null);
            if (current == null) return;
            try
            {
                current.Cancel();
            }
            finally
            {
                current.Dispose();
            }
        }

        private async Task RunAsync(CancellationToken token)
        {
            try
            {
                await Task.Delay(delay, token).ConfigureAwait(false);
                if (token.IsCancellationRequested) return;
                refresh();
            }
            catch (OperationCanceledException)
            {
                // A newer keystroke or an unload cancelled this pending refresh.
            }
        }
    }
}
