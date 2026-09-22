using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>One visible DataGrid column's raw-value sort contract.</summary>
    internal sealed class DataGridSortColumn
    {
        private DataGridSortColumn(
            string key,
            string sortMemberPath,
            ListSortDirection defaultDirection,
            Func<object, object, int> compareAscending,
            Func<object, bool> isUnknown,
            Func<object, string> stableKey)
        {
            Key = key;
            SortMemberPath = sortMemberPath;
            DefaultDirection = defaultDirection;
            CompareAscending = compareAscending;
            IsUnknown = isUnknown;
            StableKey = stableKey;
        }

        internal string Key { get; }
        internal string SortMemberPath { get; }
        internal ListSortDirection DefaultDirection { get; }
        internal Func<object, object, int> CompareAscending { get; }
        internal Func<object, bool> IsUnknown { get; }
        internal Func<object, string> StableKey { get; }

        internal static DataGridSortColumn Create<T>(
            string key,
            string sortMemberPath,
            ListSortDirection defaultDirection,
            Comparison<T> compareAscending,
            Predicate<T> isUnknown,
            Func<T, string> stableKey)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Sort key is required.", nameof(key));
            if (string.IsNullOrWhiteSpace(sortMemberPath)) throw new ArgumentException("Sort member path is required.", nameof(sortMemberPath));
            if (compareAscending == null) throw new ArgumentNullException(nameof(compareAscending));
            if (isUnknown == null) throw new ArgumentNullException(nameof(isUnknown));
            if (stableKey == null) throw new ArgumentNullException(nameof(stableKey));

            return new DataGridSortColumn(
                key,
                sortMemberPath,
                defaultDirection,
                (left, right) => compareAscending((T)left, (T)right),
                value => isUnknown((T)value),
                value => stableKey((T)value) ?? string.Empty);
        }
    }

    /// <summary>
    /// Keeps the production DataGrid sort arrow and the collection view's actual order in
    /// sync. Every selected column gets the same stable ID tie-breaker, while unknown values
    /// are kept at the end in either direction.
    /// </summary>
    internal sealed class DataGridStableSortController : IDisposable
    {
        private readonly DataGrid grid;
        private readonly IReadOnlyList<DataGridSortColumn> columns;
        private readonly DependencyPropertyDescriptor? itemsSourceDescriptor;
        private ICollectionView? view;
        private int activeColumnIndex;
        private ListSortDirection activeDirection;
        private bool disposed;

        internal DataGridStableSortController(
            DataGrid grid,
            IEnumerable<DataGridSortColumn> columns,
            int defaultColumnIndex)
        {
            this.grid = grid ?? throw new ArgumentNullException(nameof(grid));
            this.columns = (columns ?? throw new ArgumentNullException(nameof(columns))).ToArray();
            if (this.columns.Count != grid.Columns.Count)
                throw new ArgumentException("Sort contract count must match the DataGrid column count.", nameof(columns));
            if (this.columns.Count == 0)
                throw new ArgumentException("At least one sort contract is required.", nameof(columns));
            if (defaultColumnIndex < 0 || defaultColumnIndex >= this.columns.Count)
                throw new ArgumentOutOfRangeException(nameof(defaultColumnIndex));
            if (this.columns.Select(column => column.Key).Distinct(StringComparer.Ordinal).Count() != this.columns.Count)
                throw new ArgumentException("Sort keys must be unique.", nameof(columns));

            activeColumnIndex = defaultColumnIndex;
            activeDirection = this.columns[defaultColumnIndex].DefaultDirection;
            grid.Sorting += OnSorting;
            grid.Loaded += OnGridLoaded;
            itemsSourceDescriptor = DependencyPropertyDescriptor.FromProperty(DataGrid.ItemsSourceProperty, typeof(DataGrid));
            itemsSourceDescriptor?.AddValueChanged(grid, OnItemsSourceChanged);
            AttachView();
        }

        internal string ActiveColumnKey => columns[activeColumnIndex].Key;
        internal ListSortDirection ActiveDirection => activeDirection;

        private void OnGridLoaded(object sender, RoutedEventArgs e) => AttachView();

        private void OnItemsSourceChanged(object? sender, EventArgs e) => AttachView();

        private void AttachView()
        {
            if (disposed) return;
            var next = ResolveView(grid.ItemsSource);
            if (ReferenceEquals(view, next))
            {
                ApplyCurrentSort();
                return;
            }

            view = next;
            ApplyCurrentSort();
        }

        private static ICollectionView? ResolveView(object? source)
        {
            if (source is ICollectionView collectionView)
                return collectionView;
            if (source == null) return null;
            return new CollectionViewSource { Source = source }.View;
        }

        private void OnSorting(object sender, DataGridSortingEventArgs e)
        {
            if (disposed) return;
            var index = grid.Columns.IndexOf(e.Column);
            if (index < 0 || index >= columns.Count) return;

            e.Handled = true;
            ToggleSort(index);
        }

        private void ToggleSort(int index)
        {
            if (index == activeColumnIndex)
                activeDirection = activeDirection == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            else
            {
                activeColumnIndex = index;
                activeDirection = columns[index].DefaultDirection;
            }

            ApplyCurrentSort();
        }

        private void ApplyCurrentSort()
        {
            if (disposed) return;

            for (var i = 0; i < grid.Columns.Count; i++)
                grid.Columns[i].SortDirection = i == activeColumnIndex ? activeDirection : (ListSortDirection?)null;

            if (view == null) return;
            var selected = columns[activeColumnIndex];
            if (view is ListCollectionView listView)
            {
                using (listView.DeferRefresh())
                {
                    listView.CustomSort = null;
                    listView.SortDescriptions.Clear();
                    listView.CustomSort = new StableComparer(selected, activeDirection);
                }
                return;
            }

            // The production collections are ListCollectionView. Keep a predictable
            // property-path fallback for a host-provided ICollectionView.
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription(selected.SortMemberPath, activeDirection));
        }

        /// <summary>Drives the same sort application path as a header click in an isolated fixture.</summary>
        internal void ApplySortForVerification(int columnIndex, ListSortDirection direction)
        {
            if (disposed) throw new ObjectDisposedException(nameof(DataGridStableSortController));
            if (columnIndex < 0 || columnIndex >= columns.Count) throw new ArgumentOutOfRangeException(nameof(columnIndex));
            activeColumnIndex = columnIndex;
            activeDirection = direction;
            ApplyCurrentSort();
        }

        /// <summary>Drives the same toggle path as a real DataGrid header click in an isolated fixture.</summary>
        internal void ToggleSortForVerification(int columnIndex)
        {
            if (disposed) throw new ObjectDisposedException(nameof(DataGridStableSortController));
            if (columnIndex < 0 || columnIndex >= columns.Count) throw new ArgumentOutOfRangeException(nameof(columnIndex));
            ToggleSort(columnIndex);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            grid.Sorting -= OnSorting;
            grid.Loaded -= OnGridLoaded;
            itemsSourceDescriptor?.RemoveValueChanged(grid, OnItemsSourceChanged);
            view = null;
        }

        private sealed class StableComparer : System.Collections.IComparer
        {
            private readonly DataGridSortColumn column;
            private readonly ListSortDirection direction;

            internal StableComparer(DataGridSortColumn column, ListSortDirection direction)
            {
                this.column = column;
                this.direction = direction;
            }

            public int Compare(object? x, object? y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (x == null) return 1;
                if (y == null) return -1;

                var xUnknown = column.IsUnknown(x);
                var yUnknown = column.IsUnknown(y);
                if (xUnknown != yUnknown)
                    return xUnknown ? 1 : -1;

                var result = column.CompareAscending(x, y);
                if (direction == ListSortDirection.Descending)
                    result = -result;
                if (result != 0)
                    return result;

                return StringComparer.OrdinalIgnoreCase.Compare(column.StableKey(x), column.StableKey(y));
            }
        }
    }

    /// <summary>Stable raw-value sort profiles for the production R06 tables.</summary>
    internal static class ProductionDataGridSortProfiles
    {
        internal static DataGridStableSortController AttachSaveHistory(DataGrid grid)
            => new DataGridStableSortController(grid, new[]
            {
                Column<BackupVersionDto>("time", "CreatedUtc", ListSortDirection.Descending, (a, b) => a.CreatedUtc.CompareTo(b.CreatedUtc), a => a.CreatedUtc == DateTime.MinValue, a => a.BackupId),
                Column<BackupVersionDto>("type", "IsPreRestore", ListSortDirection.Ascending, (a, b) => a.IsPreRestore.CompareTo(b.IsPreRestore), _ => false, a => a.BackupId),
                Column<BackupVersionDto>("file-count", "FileCount", ListSortDirection.Ascending, (a, b) => a.FileCount.CompareTo(b.FileCount), a => a.FileCount < 0, a => a.BackupId),
                Column<BackupVersionDto>("size", "TotalBytes", ListSortDirection.Ascending, (a, b) => a.TotalBytes.CompareTo(b.TotalBytes), a => a.TotalBytes < 0, a => a.BackupId),
                Column<BackupVersionDto>("device", "SourceDevice", ListSortDirection.Ascending, (a, b) => CompareText(a.SourceDevice, b.SourceDevice), a => string.IsNullOrWhiteSpace(a.SourceDevice), a => a.BackupId),
                Column<BackupVersionDto>("note", "Comment", ListSortDirection.Ascending, (a, b) => CompareText(a.Comment, b.Comment), a => string.IsNullOrWhiteSpace(a.Comment), a => a.BackupId),
                Column<BackupVersionDto>("state", "IsLocked", ListSortDirection.Ascending, (a, b) => a.IsLocked.CompareTo(b.IsLocked), _ => false, a => a.BackupId)
            }, 0);

        internal static DataGridStableSortController AttachSaveCandidates(DataGrid grid)
            => new DataGridStableSortController(grid, new[]
            {
                Column<SavePathCandidateDto>("confidence", "Score", ListSortDirection.Descending, (a, b) => a.Score.CompareTo(b.Score), a => double.IsNaN(a.Score) || double.IsInfinity(a.Score), StableCandidateKey),
                Column<SavePathCandidateDto>("status", "Status", ListSortDirection.Ascending, (a, b) => CompareText(a.Status, b.Status), a => string.IsNullOrWhiteSpace(a.Status), StableCandidateKey),
                Column<SavePathCandidateDto>("path", "Path", ListSortDirection.Ascending, (a, b) => CompareText(a.Path, b.Path), a => string.IsNullOrWhiteSpace(a.Path), StableCandidateKey),
                Column<SavePathCandidateDto>("reason", "ReasonsDisplay", ListSortDirection.Ascending, (a, b) => CompareText(a.ReasonsDisplay, b.ReasonsDisplay), a => string.IsNullOrWhiteSpace(a.ReasonsDisplay), StableCandidateKey)
            }, 0);

        internal static DataGridStableSortController AttachTasks(DataGrid grid)
            => new DataGridStableSortController(grid, new[]
            {
                Column<TaskStatusDto>("local-time", "CreatedUtc", ListSortDirection.Descending, (a, b) => a.CreatedUtc.CompareTo(b.CreatedUtc), a => a.CreatedUtc == DateTime.MinValue, a => a.TaskId),
                Column<TaskStatusDto>("task", "TaskTypeDisplay", ListSortDirection.Ascending, (a, b) => CompareText(a.TaskTypeDisplay, b.TaskTypeDisplay), a => string.IsNullOrWhiteSpace(a.TaskType), a => a.TaskId),
                Column<TaskStatusDto>("stage", "StageDisplay", ListSortDirection.Ascending, (a, b) => CompareText(a.StageDisplay, b.StageDisplay), a => !a.HasKnownStage, a => a.TaskId),
                Column<TaskStatusDto>("game", "GameName", ListSortDirection.Ascending, (a, b) => CompareText(a.GameName, b.GameName), a => string.IsNullOrWhiteSpace(a.GameName), a => a.TaskId),
                Column<TaskStatusDto>("state", "State", ListSortDirection.Ascending, (a, b) => ((int)a.State).CompareTo((int)b.State), a => !Enum.IsDefined(typeof(TaskState), a.State), a => a.TaskId),
                Column<TaskStatusDto>("progress", "ProgressValue", ListSortDirection.Ascending, (a, b) => a.ProgressValue.CompareTo(b.ProgressValue), IsUnknownProgress, a => a.TaskId),
                Column<TaskStatusDto>("detail", "DetailMessage", ListSortDirection.Ascending, (a, b) => CompareText(a.DetailMessage, b.DetailMessage), a => string.IsNullOrWhiteSpace(a.DetailMessage), a => a.TaskId)
            }, 0);

        internal static DataGridStableSortController AttachMediaInbox(DataGrid grid)
            => new DataGridStableSortController(grid, new[]
            {
                Column<MediaItemDto>("captured-time", "CapturedUtc", ListSortDirection.Descending, (a, b) => a.CapturedUtc.CompareTo(b.CapturedUtc), a => a.CapturedUtc == DateTime.MinValue, a => a.MediaId),
                Column<MediaItemDto>("type", "Kind", ListSortDirection.Ascending, (a, b) => ((int)a.Kind).CompareTo((int)b.Kind), a => !Enum.IsDefined(typeof(MediaKind), a.Kind) || a.Kind == MediaKind.Unknown, a => a.MediaId),
                Column<MediaItemDto>("source", "Source", ListSortDirection.Ascending, (a, b) => ((int)a.Source).CompareTo((int)b.Source), a => !Enum.IsDefined(typeof(MediaSourceKind), a.Source) || a.Source == MediaSourceKind.Unknown, a => a.MediaId),
                Column<MediaItemDto>("file", "FileName", ListSortDirection.Ascending, (a, b) => CompareText(a.FileName, b.FileName), a => string.IsNullOrWhiteSpace(a.FileName), a => a.MediaId),
                Column<MediaItemDto>("reason", "ClassificationReason", ListSortDirection.Ascending, (a, b) => CompareText(a.ClassificationReason, b.ClassificationReason), a => string.IsNullOrWhiteSpace(a.ClassificationReason), a => a.MediaId)
            }, 0);

        private static DataGridSortColumn Column<T>(
            string key,
            string sortMemberPath,
            ListSortDirection defaultDirection,
            Comparison<T> compareAscending,
            Predicate<T> isUnknown,
            Func<T, string> stableKey)
            => DataGridSortColumn.Create(key, sortMemberPath, defaultDirection, compareAscending, isUnknown, stableKey);

        private static int CompareText(string? left, string? right)
            => StringComparer.OrdinalIgnoreCase.Compare(left ?? string.Empty, right ?? string.Empty);

        private static string StableCandidateKey(SavePathCandidateDto candidate)
            => (candidate.Path ?? string.Empty) + "\u001f" + (candidate.PlayniteId ?? string.Empty);

        private static bool IsUnknownProgress(TaskStatusDto task)
            => task.ProgressPercent < 0 || (task.State == TaskState.Queued && task.ProgressPercent == 0);
    }
}
