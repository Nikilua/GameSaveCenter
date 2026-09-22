using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSaveCenter.Playnite.Infrastructure;

/// <summary>
/// Suppresses duplicate page-scoped reads during a short hot revisit window.
/// The key must include the page's stable data context; a different game or filter
/// therefore cannot inherit the previous page's freshness. Explicit reload commands
/// do not use this gate.
/// </summary>
internal sealed class WorkspaceRevisitLoadGate
{
    private const int MaxRememberedKeys = 64;
    private readonly object syncRoot = new object();
    private readonly TimeSpan freshnessWindow;
    private readonly HashSet<string> pendingKeys = new HashSet<string>(StringComparer.Ordinal);
    private readonly Dictionary<string, DateTime> successfulAt = new Dictionary<string, DateTime>(StringComparer.Ordinal);

    public WorkspaceRevisitLoadGate(TimeSpan freshnessWindow)
    {
        if (freshnessWindow <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(freshnessWindow));

        this.freshnessWindow = freshnessWindow;
    }

    public bool TryBegin(string key, DateTime nowUtc)
    {
        key = Normalize(key);
        lock (syncRoot)
        {
            if (pendingKeys.Contains(key)) return false;
            if (successfulAt.TryGetValue(key, out var completedUtc)
                && nowUtc >= completedUtc
                && nowUtc - completedUtc < freshnessWindow)
                return false;

            pendingKeys.Add(key);
            return true;
        }
    }

    public bool Complete(string key, DateTime completedUtc)
    {
        key = Normalize(key);
        lock (syncRoot)
        {
            if (!pendingKeys.Remove(key)) return false;
            successfulAt[key] = completedUtc;
            TrimRememberedKeys();
            return true;
        }
    }

    public void Fail(string key)
        => FinishWithoutFreshness(key);

    public void Cancel(string key)
        => FinishWithoutFreshness(key);

    public void Invalidate(string key)
    {
        key = Normalize(key);
        lock (syncRoot)
        {
            pendingKeys.Remove(key);
            successfulAt.Remove(key);
        }
    }

    public void InvalidateAll()
    {
        lock (syncRoot)
        {
            pendingKeys.Clear();
            successfulAt.Clear();
        }
    }

    private void FinishWithoutFreshness(string key)
    {
        lock (syncRoot)
            pendingKeys.Remove(Normalize(key));
    }

    private void TrimRememberedKeys()
    {
        while (successfulAt.Count > MaxRememberedKeys)
        {
            var oldest = successfulAt.OrderBy(item => item.Value).First().Key;
            successfulAt.Remove(oldest);
        }
    }

    private static string Normalize(string key)
        => key ?? string.Empty;
}
