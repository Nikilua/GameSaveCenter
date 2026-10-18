using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using GameSaveCenter.Playnite.Settings;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Provides an accessible column picker for a stable DataGrid scope. Only explicitly
    /// hidden optional keys are persisted, so future columns remain visible by default.
    /// Required identity/status columns are always visible.
    /// </summary>
    internal sealed class DataGridColumnVisibilityController : IDisposable
    {
        private readonly DataGrid grid;
        private readonly Button button;
        private readonly string viewKey;
        private readonly GameSaveCenterSettings settings;
        private readonly Action persist;
        private readonly List<ColumnEntry> entries;
        private readonly HashSet<string> requiredKeys;
        private readonly ContextMenu menu;
        private readonly List<MenuItem> columnMenuItems = new List<MenuItem>();
        private MenuItem resetMenuItem = null!;
        private bool disposed;

        internal DataGridColumnVisibilityController(
            DataGrid grid,
            Button button,
            string viewKey,
            IEnumerable<string> columnKeys,
            IEnumerable<string> requiredColumnKeys,
            GameSaveCenterSettings settings,
            Action persist)
        {
            this.grid = grid ?? throw new ArgumentNullException(nameof(grid));
            this.button = button ?? throw new ArgumentNullException(nameof(button));
            this.viewKey = string.IsNullOrWhiteSpace(viewKey)
                ? throw new ArgumentException("View key is required.", nameof(viewKey))
                : viewKey;
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.persist = persist ?? throw new ArgumentNullException(nameof(persist));

            var keys = (columnKeys ?? throw new ArgumentNullException(nameof(columnKeys))).ToArray();
            if (keys.Length != grid.Columns.Count)
                throw new ArgumentException("Column key count must match the DataGrid column count.", nameof(columnKeys));
            if (keys.Any(string.IsNullOrWhiteSpace) || keys.Distinct(StringComparer.Ordinal).Count() != keys.Length)
                throw new ArgumentException("Column keys must be non-empty and unique.", nameof(columnKeys));

            requiredKeys = new HashSet<string>(
                requiredColumnKeys ?? throw new ArgumentNullException(nameof(requiredColumnKeys)),
                StringComparer.Ordinal);
            if (requiredKeys.Count == 0 || requiredKeys.Any(key => !keys.Contains(key, StringComparer.Ordinal)))
                throw new ArgumentException("At least one required key must match a DataGrid column.", nameof(requiredColumnKeys));

            entries = grid.Columns
                .Select((column, index) => new ColumnEntry(keys[index], column, requiredKeys.Contains(keys[index])))
                .ToList();
            menu = CreateMenu();
            button.ContextMenu = menu;
            button.Click += OnButtonClick;
            menu.Opened += OnMenuOpened;
            ApplyStoredHiddenColumns();
            RefreshMenuState();
        }

        internal ContextMenu Menu => menu;

        internal void ResetToDefaults()
        {
            if (disposed) return;
            settings.ResetDataGridHiddenColumnKeys(viewKey);
            foreach (var entry in entries)
                entry.Column.Visibility = Visibility.Visible;
            RefreshMenuState();
            persist();
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            menu.IsOpen = false;
            menu.Opened -= OnMenuOpened;
            button.Click -= OnButtonClick;
            if (ReferenceEquals(button.ContextMenu, menu))
                button.ContextMenu = null;
            foreach (var item in columnMenuItems)
                item.Click -= OnColumnMenuItemClick;
            resetMenuItem.Click -= OnResetMenuItemClick;
        }

        private ContextMenu CreateMenu()
        {
            var result = new ContextMenu
            {
                Placement = PlacementMode.Bottom,
                PlacementTarget = button,
                StaysOpen = false
            };
            if (button.TryFindResource("GscColumnPickerContextMenuStyle") is Style menuStyle)
                result.Style = menuStyle;

            var itemStyle = button.TryFindResource("GscColumnPickerMenuItemStyle") as Style;
            foreach (var entry in entries.OrderBy(item => item.Column.DisplayIndex))
            {
                var label = GetHeaderText(entry.Column, entry.Key);
                var item = new MenuItem
                {
                    Header = label,
                    IsCheckable = true,
                    IsEnabled = !entry.IsRequired,
                    StaysOpenOnClick = true,
                    Tag = entry,
                    Style = itemStyle
                };
                AutomationProperties.SetName(item, entry.IsRequired ? label + "（必要列）" : label);
                AutomationProperties.SetHelpText(
                    item,
                    entry.IsRequired
                        ? "用于识别对象或状态，始终保持显示。"
                        : "勾选显示此列，取消勾选隐藏此列。新版本增加的列默认显示。");
                item.Click += OnColumnMenuItemClick;
                columnMenuItems.Add(item);
                result.Items.Add(item);
            }

            result.Items.Add(new Separator());
            resetMenuItem = new MenuItem
            {
                Header = "恢复默认列",
                StaysOpenOnClick = false,
                Style = itemStyle
            };
            AutomationProperties.SetName(resetMenuItem, "恢复默认列显示");
            AutomationProperties.SetHelpText(resetMenuItem, "显示此表格的所有列，并保留各列宽度。");
            resetMenuItem.Click += OnResetMenuItemClick;
            result.Items.Add(resetMenuItem);
            return result;
        }

        private void ApplyStoredHiddenColumns()
        {
            if (!settings.TryGetDataGridHiddenColumnKeys(viewKey, out var hiddenKeys))
                return;

            var hidden = new HashSet<string>(hiddenKeys, StringComparer.Ordinal);
            foreach (var entry in entries)
            {
                entry.Column.Visibility = !entry.IsRequired && hidden.Contains(entry.Key)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            if (disposed) return;
            RefreshMenuState();
            menu.PlacementTarget = button;
            menu.Placement = PlacementMode.Bottom;
            menu.IsOpen = true;
        }

        private void OnMenuOpened(object? sender, RoutedEventArgs e)
            => RefreshMenuState();

        private void OnColumnMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (disposed || !(sender is MenuItem item) || !(item.Tag is ColumnEntry entry)) return;
            if (entry.IsRequired)
            {
                item.IsChecked = true;
                entry.Column.Visibility = Visibility.Visible;
                return;
            }

            entry.Column.Visibility = item.IsChecked ? Visibility.Visible : Visibility.Collapsed;
            MoveCurrentCellToVisibleColumn(entry.Column);
            PersistHiddenColumns();
            RefreshMenuState();
        }

        private void OnResetMenuItemClick(object sender, RoutedEventArgs e)
            => ResetToDefaults();

        private void PersistHiddenColumns()
        {
            var hiddenKeys = entries
                .Where(entry => !entry.IsRequired && entry.Column.Visibility != Visibility.Visible)
                .OrderBy(entry => entry.Column.DisplayIndex)
                .Select(entry => entry.Key)
                .ToArray();
            settings.SetDataGridHiddenColumnKeys(viewKey, hiddenKeys);
            persist();
        }

        private void RefreshMenuState()
        {
            foreach (var item in columnMenuItems)
            {
                if (!(item.Tag is ColumnEntry entry)) continue;
                item.IsChecked = entry.IsRequired || entry.Column.Visibility == Visibility.Visible;
            }
        }

        private void MoveCurrentCellToVisibleColumn(DataGridColumn hiddenColumn)
        {
            if (!ReferenceEquals(grid.CurrentColumn, hiddenColumn)) return;
            var nextVisible = entries
                .Where(entry => entry.Column.Visibility == Visibility.Visible)
                .OrderBy(entry => entry.Column.DisplayIndex)
                .FirstOrDefault();
            if (nextVisible != null)
                grid.CurrentColumn = nextVisible.Column;
        }

        private static string GetHeaderText(DataGridColumn column, string fallback)
            => column.Header is string header && !string.IsNullOrWhiteSpace(header)
                ? header.Trim()
                : fallback;

        private sealed class ColumnEntry
        {
            internal ColumnEntry(string key, DataGridColumn column, bool isRequired)
            {
                Key = key;
                Column = column;
                IsRequired = isRequired;
            }

            internal string Key { get; }
            internal DataGridColumn Column { get; }
            internal bool IsRequired { get; }
        }
    }
}
