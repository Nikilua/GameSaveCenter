using System;
using System.Threading;
using System.Threading.Tasks;

namespace GameSaveCenter.Playnite.ViewModels;

/// <summary>
/// Owns the single interactive Worker operation slot used by DashboardViewModel.
/// The gate is atomic so a second input cannot start work between the CanExecute
/// query and the first operation setting its busy state.
/// </summary>
internal sealed class BusyOperationCoordinator
{
    private int running;

    public async Task<bool> TryRunAsync(
        Func<Task> prepare,
        Func<Task> action,
        Action<bool> setBusy,
        Action<string> reportCancellation,
        Action<Exception> reportFailure,
        Action onFinished)
    {
        if (Interlocked.CompareExchange(ref running, 1, 0) != 0)
            return false;

        setBusy(true);
        try
        {
            await prepare().ConfigureAwait(true);
            await action().ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            reportCancellation("操作已取消");
        }
        catch (Exception ex)
        {
            reportFailure(ex);
        }
        finally
        {
            try
            {
                setBusy(false);
                onFinished();
            }
            finally
            {
                Volatile.Write(ref running, 0);
            }
        }

        return true;
    }
}
