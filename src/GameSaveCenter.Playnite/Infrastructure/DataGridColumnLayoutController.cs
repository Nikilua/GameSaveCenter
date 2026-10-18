using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
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
        private const double ResponsiveGridChromeAllowance = 28d;
        private readonly DataGrid grid;
        private readonly string viewKey;
        private readonly GameSaveCenterSettings settings;
        private readonly Action persist;
        private readonly string[] columnKeys;
        private readonly DataGridLength[] defaultWidths;
        private readonly DataGridLength[] preferredWidths;
        private readonly DependencyPropertyDescriptor? widthDescriptor;
        private readonly DispatcherTimer persistTimer;
        private int layoutPassDepth;
        private bool applying;
        private bool settingsPersistenceEnabled = true;
        private bool persistPending;
        private double responsiveAvailableWidth;
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
            preferredWidths = defaultWidths.ToArray();
            widthDescriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.WidthProperty, typeof(DataGridColumn));
            persistTimer = new DispatcherTimer(DispatcherPriority.Background, grid.Dispatcher)
            {
                Interval = TimeSpan.FromMilliseconds(400)
            };
            persistTimer.Tick += OnPersistTimerTick;
            DataGridLayoutResetCoordinator.AllLayoutsReset += OnAllLayoutsReset;
            grid.SizeChanged += OnGridSizeChanged;
            grid.Loaded += OnGridLoaded;

            for (var index = 0; index < grid.Columns.Count; index++)
            {
                if (widthDescriptor != null)
                    widthDescriptor.AddValueChanged(grid.Columns[index], OnColumnWidthChanged);
            }

            ApplyStoredWidths();
            if (grid.IsLoaded)
                ApplyResponsiveWidthBudget(grid.ActualWidth);
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
            ApplyPreferredWidths();
        }

        internal void RefreshResponsiveLayout()
        {
            if (disposed) return;
            var width = grid.ActualWidth > 0 ? grid.ActualWidth : responsiveAvailableWidth;
            ApplyResponsiveWidthBudget(width);
        }

        internal void ApplyResponsiveWidthBudget(double availableWidth)
        {
            if (disposed || double.IsNaN(availableWidth) || double.IsInfinity(availableWidth) || availableWidth <= 0)
                return;

            responsiveAvailableWidth = availableWidth;
            if (layoutPassDepth != 0)
                return;

            ApplyResponsiveWidths();
        }

        public void ResetToDefaults()
        {
            if (disposed) return;
            if (settingsPersistenceEnabled)
                settings.ResetDataGridColumnWidths(viewKey);
            SetPreferredWidths(defaultWidths);
            ApplyPreferredWidths();
            if (settingsPersistenceEnabled)
            {
                persistPending = true;
                FlushPendingPersistence();
            }
        }

        internal IReadOnlyDictionary<string, double> CaptureConfigurationWidths()
        {
            var result = new Dictionary<string, double>(StringComparer.Ordinal);
            for (var index = 0; index < grid.Columns.Count; index++)
            {
                var width = preferredWidths[index];
                if (width.UnitType == DataGridLengthUnitType.Pixel
                    && TryNormalizeWidth(grid.Columns[index], width.Value, out var normalized))
                    result[columnKeys[index]] = normalized;
            }
            return result;
        }

        internal DataGrid Grid => grid;

        internal void SetSettingsPersistenceEnabled(bool enabled)
        {
            if (disposed || settingsPersistenceEnabled == enabled) return;
            if (!enabled)
                FlushPendingPersistence();
            settingsPersistenceEnabled = enabled;
        }

        internal void ApplyStoredWidthsFromDefault()
        {
            if (disposed) return;
            LoadPreferredWidthsFromSettings();
            ApplyPreferredWidths();
        }

        internal void ApplyConfigurationWidths(IDictionary<string, double>? widths)
        {
            if (disposed) return;
            var values = widths ?? new Dictionary<string, double>(StringComparer.Ordinal);
            var nextWidths = defaultWidths.ToArray();
            for (var index = 0; index < grid.Columns.Count; index++)
            {
                var column = grid.Columns[index];
                if (values.TryGetValue(columnKeys[index], out var storedWidth)
                    && TryNormalizeWidth(column, storedWidth, out var normalizedWidth))
                    nextWidths[index] = new DataGridLength(normalizedWidth, DataGridLengthUnitType.Pixel);
            }
            SetPreferredWidths(nextWidths);
            ApplyPreferredWidths();
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
            DataGridLayoutResetCoordinator.AllLayoutsReset -= OnAllLayoutsReset;
            grid.SizeChanged -= OnGridSizeChanged;
            grid.Loaded -= OnGridLoaded;
            if (widthDescriptor == null) return;
            foreach (var column in grid.Columns)
                widthDescriptor.RemoveValueChanged(column, OnColumnWidthChanged);
        }

        private void ApplyStoredWidths()
        {
            if (disposed || layoutPassDepth != 0) return;
            LoadPreferredWidthsFromSettings();
            ApplyPreferredWidths();
        }

        private void OnColumnWidthChanged(object? sender, EventArgs e)
        {
            if (disposed || applying || layoutPassDepth != 0 || !(sender is DataGridColumn column)) return;
            var index = grid.Columns.IndexOf(column);
            if (index < 0) return;
            if (column.Width.UnitType == DataGridLengthUnitType.Auto)
            {
                // WPF uses Auto when a header gripper is double-clicked. Remove the old
                // pixel override for the default view; named views capture the Auto
                // preference when the user saves them.
                preferredWidths[index] = column.Width;
                if (settingsPersistenceEnabled && settings.RemoveDataGridColumnWidth(viewKey, columnKeys[index]))
                {
                    persistPending = true;
                    persistTimer.Stop();
                    persistTimer.Start();
                }
                return;
            }
            if (column.Width.UnitType != DataGridLengthUnitType.Pixel) return;
            if (!TryNormalizeWidth(column, column.Width.Value, out var normalizedWidth)) return;
            preferredWidths[index] = new DataGridLength(normalizedWidth, DataGridLengthUnitType.Pixel);
            if (!settingsPersistenceEnabled) return;

            settings.SetDataGridColumnWidth(viewKey, columnKeys[index], normalizedWidth);
            persistPending = true;
            persistTimer.Stop();
            persistTimer.Start();
        }

        private void OnPersistTimerTick(object? sender, EventArgs e)
            => FlushPendingPersistence();

        private void OnAllLayoutsReset(object? sender, EventArgs e)
        {
            if (disposed) return;
            if (!grid.Dispatcher.CheckAccess())
            {
                grid.Dispatcher.BeginInvoke(new Action(() => OnAllLayoutsReset(sender, e)));
                return;
            }
            ApplyStoredWidthsFromDefault();
        }

        private void OnGridLoaded(object sender, RoutedEventArgs e)
            => ApplyResponsiveWidthBudget(grid.ActualWidth);

        private void OnGridSizeChanged(object sender, SizeChangedEventArgs e)
            => ApplyResponsiveWidthBudget(e.NewSize.Width);

        private void LoadPreferredWidthsFromSettings()
        {
            var nextWidths = defaultWidths.ToArray();
            for (var index = 0; index < grid.Columns.Count; index++)
            {
                var column = grid.Columns[index];
                if (!settings.TryGetDataGridColumnWidth(viewKey, columnKeys[index], out var storedWidth)) continue;
                if (!TryNormalizeWidth(column, storedWidth, out var normalizedWidth)) continue;
                nextWidths[index] = new DataGridLength(normalizedWidth, DataGridLengthUnitType.Pixel);
            }
            SetPreferredWidths(nextWidths);
        }

        private void SetPreferredWidths(IReadOnlyList<DataGridLength> widths)
        {
            var count = Math.Min(preferredWidths.Length, widths.Count);
            for (var index = 0; index < count; index++)
                preferredWidths[index] = widths[index];
        }

        private void ApplyPreferredWidths()
        {
            if (disposed || layoutPassDepth != 0) return;
            applying = true;
            try
            {
                for (var index = 0; index < grid.Columns.Count; index++)
                    grid.Columns[index].Width = preferredWidths[index];
            }
            finally
            {
                applying = false;
            }

            if (responsiveAvailableWidth > 0)
                ApplyResponsiveWidths();
        }

        private void ApplyResponsiveWidths()
        {
            if (disposed || applying || layoutPassDepth != 0 || responsiveAvailableWidth <= 0)
                return;

            var availableColumnWidth = Math.Max(0, responsiveAvailableWidth - ResponsiveGridChromeAllowance);
            var requiredWidth = 0d;
            var visibleIndexes = new List<int>();
            for (var index = 0; index < grid.Columns.Count; index++)
            {
                var column = grid.Columns[index];
                if (column.Visibility != Visibility.Visible) continue;
                visibleIndexes.Add(index);
                requiredWidth += GetPreferredBudgetWidth(index);
            }

            var effectiveWidths = preferredWidths.ToArray();
            var remainingExcess = Math.Max(0, requiredWidth - availableColumnWidth);
            foreach (var index in visibleIndexes.OrderByDescending(item => grid.Columns[item].DisplayIndex))
            {
                if (remainingExcess <= 0.5) break;
                var preferred = preferredWidths[index];
                if (preferred.UnitType != DataGridLengthUnitType.Pixel) continue;

                var minimumWidth = GetMinimumWidth(grid.Columns[index]);
                var reducibleWidth = Math.Max(0, preferred.Value - minimumWidth);
                var reduction = Math.Min(remainingExcess, reducibleWidth);
                if (reduction <= 0) continue;

                effectiveWidths[index] = new DataGridLength(preferred.Value - reduction, DataGridLengthUnitType.Pixel);
                remainingExcess -= reduction;
            }

            applying = true;
            try
            {
                for (var index = 0; index < grid.Columns.Count; index++)
                {
                    if (grid.Columns[index].Width != effectiveWidths[index])
                        grid.Columns[index].Width = effectiveWidths[index];
                }
            }
            finally
            {
                applying = false;
            }
        }

        private double GetPreferredBudgetWidth(int index)
        {
            var column = grid.Columns[index];
            var minimum = GetMinimumWidth(column);
            var preferred = preferredWidths[index];
            if (preferred.UnitType == DataGridLengthUnitType.Pixel)
                return Math.Max(minimum, preferred.Value);
            if (preferred.UnitType == DataGridLengthUnitType.Star)
                return minimum;
            return Math.Max(minimum, column.ActualWidth > 0 ? column.ActualWidth : minimum);
        }

        private double GetMinimumWidth(DataGridColumn column)
            => Math.Max(0, Math.Max(grid.MinColumnWidth, column.MinWidth));

        private bool TryNormalizeWidth(DataGridColumn column, double width, out double normalizedWidth)
        {
            normalizedWidth = 0;
            if (double.IsNaN(width) || double.IsInfinity(width) || width <= 0) return false;
            var minimum = GetMinimumWidth(column);
            var maximum = column.MaxWidth > 0 && !double.IsInfinity(column.MaxWidth)
                ? Math.Min(MaximumPersistedWidth, column.MaxWidth)
                : MaximumPersistedWidth;
            if (maximum < minimum) maximum = minimum;
            normalizedWidth = Math.Max(minimum, Math.Min(maximum, width));
            return normalizedWidth > 0;
        }
    }
}
