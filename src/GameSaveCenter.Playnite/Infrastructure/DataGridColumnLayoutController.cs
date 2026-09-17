using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Settings;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Persists only user-resized pixel widths for one stable DataGrid view. Star/default
    /// widths remain owned by the page's responsive layout. Unknown or invalid stored keys
    /// are ignored, so a newer page can safely evolve its column set without applying an
    /// old column by index.
    /// </summary>
    internal sealed class DataGridColumnLayoutController : IDisposable
    {
        private const double MaximumPersistedWidth = 4096d;
        private readonly DataGrid grid;
        private readonly string viewKey;
        private readonly GameSaveCenterSettings settings;
        private readonly Action persist;
        private readonly string[] columnKeys;
        private readonly DataGridLength[] defaultWidths;
        private readonly DependencyPropertyDescriptor? widthDescriptor;
        private readonly DispatcherTimer persistTimer;
        private int layoutPassDepth;
        private bool applying;
        private bool persistPending;
        private bool disposed;

        public DataGridColumnLayoutController(
            DataGrid grid,
            string viewKey,
            IEnumerable<string> columnKeys,
            GameSaveCenterSettings settings,
            Action persist)
        {
            this.grid = grid ?? throw new ArgumentNullException(nameof(grid));
            this.viewKey = string.IsNullOrWhiteSpace(viewKey) ? throw new ArgumentException("View key is required.", nameof(viewKey)) : viewKey;
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.persist = persist ?? throw new ArgumentNullException(nameof(persist));
            this.columnKeys = (columnKeys ?? throw new ArgumentNullException(nameof(columnKeys))).ToArray();
            if (this.columnKeys.Length != grid.Columns.Count)
                throw new ArgumentException("Column key count must match the DataGrid column count.", nameof(columnKeys));
            if (this.columnKeys.Any(string.IsNullOrWhiteSpace) || this.columnKeys.Distinct(StringComparer.Ordinal).Count() != this.columnKeys.Length)
                throw new ArgumentException("Column keys must be non-empty and unique.", nameof(columnKeys));

            defaultWidths = grid.Columns.Select(column => column.Width).ToArray();
            widthDescriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.WidthProperty, typeof(DataGridColumn));
            persistTimer = new DispatcherTimer(DispatcherPriority.Background, grid.Dispatcher)
            {
                Interval = TimeSpan.FromMilliseconds(400)
            };
            persistTimer.Tick += OnPersistTimerTick;

            for (var index = 0; index < grid.Columns.Count; index++)
            {
                if (widthDescriptor != null)
                    widthDescriptor.AddValueChanged(grid.Columns[index], OnColumnWidthChanged);
            }

            ApplyStoredWidths();
        }

        public void BeginLayoutPass()
        {
            if (disposed) return;
            layoutPassDepth++;
        }

        public void EndLayoutPass()
        {
            if (disposed || layoutPassDepth == 0) return;
            layoutPassDepth--;
            if (layoutPassDepth != 0) return;
            ApplyStoredWidths();
        }

        public void ResetToDefaults()
        {
            if (disposed) return;
            settings.ResetDataGridColumnWidths(viewKey);
            applying = true;
            try
            {
                for (var index = 0; index < grid.Columns.Count; index++)
                    grid.Columns[index].Width = defaultWidths[index];
            }
            finally
            {
                applying = false;
            }
            persistPending = true;
            FlushPendingPersistence();
        }

        public void FlushPendingPersistence()
        {
            if (disposed) return;
            persistTimer.Stop();
            if (!persistPending) return;
            persistPending = false;
            persist();
        }

        public void Dispose()
        {
            if (disposed) return;
            FlushPendingPersistence();
            disposed = true;
            persistTimer.Stop();
            persistTimer.Tick -= OnPersistTimerTick;
            if (widthDescriptor == null) return;
            foreach (var column in grid.Columns)
                widthDescriptor.RemoveValueChanged(column, OnColumnWidthChanged);
        }

        private void ApplyStoredWidths()
        {
            if (disposed || layoutPassDepth != 0) return;
            applying = true;
            try
            {
                for (var index = 0; index < grid.Columns.Count; index++)
                {
                    var column = grid.Columns[index];
                    if (!settings.TryGetDataGridColumnWidth(viewKey, columnKeys[index], out var storedWidth)) continue;
                    if (!TryNormalizeWidth(column, storedWidth, out var normalizedWidth)) continue;
                    column.Width = new DataGridLength(normalizedWidth, DataGridLengthUnitType.Pixel);
                }
            }
            finally
            {
                applying = false;
            }
        }

        private void OnColumnWidthChanged(object? sender, EventArgs e)
        {
            if (disposed || applying || layoutPassDepth != 0 || !(sender is DataGridColumn column)) return;
            if (column.Width.UnitType != DataGridLengthUnitType.Pixel) return;
            var index = grid.Columns.IndexOf(column);
            if (index < 0 || !TryNormalizeWidth(column, column.Width.Value, out var normalizedWidth)) return;
            settings.SetDataGridColumnWidth(viewKey, columnKeys[index], normalizedWidth);
            persistPending = true;
            persistTimer.Stop();
            persistTimer.Start();
        }

        private void OnPersistTimerTick(object? sender, EventArgs e)
            => FlushPendingPersistence();

        private bool TryNormalizeWidth(DataGridColumn column, double width, out double normalizedWidth)
        {
            normalizedWidth = 0;
            if (double.IsNaN(width) || double.IsInfinity(width) || width <= 0) return false;
            var minimum = Math.Max(0, Math.Max(grid.MinColumnWidth, column.MinWidth));
            var maximum = column.MaxWidth > 0 && !double.IsInfinity(column.MaxWidth)
                ? Math.Min(MaximumPersistedWidth, column.MaxWidth)
                : MaximumPersistedWidth;
            if (maximum < minimum) maximum = minimum;
            normalizedWidth = Math.Max(minimum, Math.Min(maximum, width));
            return normalizedWidth > 0;
        }
    }
}
