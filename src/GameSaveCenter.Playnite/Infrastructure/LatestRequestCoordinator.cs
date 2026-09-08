using System;
using System.Threading;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Owns the cancellation and commit boundary for a latest-wins read request.
    /// Starting a newer scope cancels the previous scope; callers must still check
    /// <see cref="IsCurrent(RequestScope)"/> immediately before applying a result.
    /// </summary>
    public sealed class LatestRequestCoordinator : IDisposable
    {
        private readonly object gate = new object();
        private CancellationTokenSource? activeCancellation;
        private long generation;
        private bool disposed;

        public RequestScope Begin()
        {
            CancellationTokenSource? previous;
            RequestScope scope;
            lock (gate)
            {
                ThrowIfDisposed();
                previous = activeCancellation;
                activeCancellation = new CancellationTokenSource();
                scope = new RequestScope(++generation, activeCancellation);
            }

            CancelAndDispose(previous);
            return scope;
        }

        public bool IsCurrent(RequestScope scope)
        {
            if (scope == null) return false;
            lock (gate)
            {
                return !disposed
                    && ReferenceEquals(activeCancellation, scope.Cancellation)
                    && scope.Generation == generation
                    && !scope.Token.IsCancellationRequested;
            }
        }

        public void End(RequestScope scope)
        {
            if (scope == null) return;
            CancellationTokenSource? owned = null;
            lock (gate)
            {
                if (ReferenceEquals(activeCancellation, scope.Cancellation))
                {
                    activeCancellation = null;
                    owned = scope.Cancellation;
                }
            }

            CancelAndDispose(owned);
        }

        public void Cancel()
        {
            CancellationTokenSource? previous;
            lock (gate)
            {
                if (disposed) return;
                ++generation;
                previous = activeCancellation;
                activeCancellation = null;
            }

            CancelAndDispose(previous);
        }

        public void Dispose()
        {
            CancellationTokenSource? previous;
            lock (gate)
            {
                if (disposed) return;
                disposed = true;
                ++generation;
                previous = activeCancellation;
                activeCancellation = null;
            }

            CancelAndDispose(previous);
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(LatestRequestCoordinator));
        }

        private static void CancelAndDispose(CancellationTokenSource? cancellation)
        {
            if (cancellation == null) return;
            try { cancellation.Cancel(); }
            finally { cancellation.Dispose(); }
        }

        public sealed class RequestScope
        {
            internal RequestScope(long generation, CancellationTokenSource cancellation)
            {
                Generation = generation;
                Cancellation = cancellation;
                Token = cancellation.Token;
            }

            public long Generation { get; }
            public CancellationToken Token { get; }
            internal CancellationTokenSource Cancellation { get; }
        }
    }
}
