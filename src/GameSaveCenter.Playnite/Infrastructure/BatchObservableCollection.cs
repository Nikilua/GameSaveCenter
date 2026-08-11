using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace GameSaveCenter.Playnite.Infrastructure;

/// <summary>
/// ObservableCollection that can replace its whole contents with one Reset notification.
/// WPF measures/arranges a virtualized list once per notification, so bulk dashboard
/// snapshots avoid hundreds of per-item Add events on large libraries.
/// </summary>
public sealed class BatchObservableCollection<T> : ObservableCollection<T>
{
    private bool suppressNotifications;

    public BatchObservableCollection()
    {
    }

    public BatchObservableCollection(IEnumerable<T> items)
        : base(items)
    {
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (!suppressNotifications)
            base.OnCollectionChanged(e);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (!suppressNotifications)
            base.OnPropertyChanged(e);
    }

    public void ReplaceAll(IEnumerable<T> items)
    {
        var incoming = items is IList<T> list ? list : new List<T>(items);
        if (Count == 0 && incoming.Count == 0)
            return;
        if (Count == incoming.Count)
        {
            var same = true;
            for (var i = 0; i < Count; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(this[i], incoming[i]))
                {
                    same = false;
                    break;
                }
            }
            if (same)
                return;
        }

        suppressNotifications = true;
        try
        {
            Clear();
            foreach (var item in incoming)
                Add(item);
        }
        finally
        {
            suppressNotifications = false;
        }

        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}
