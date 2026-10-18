using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using GameSaveCenter.Playnite.Settings;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>Manages named, workspace-scoped arrangements for one production DataGrid.</summary>
    internal sealed class DataGridViewConfigurationController : IDisposable
    {
        private readonly FrameworkElement resourceOwner;
        private readonly string workspaceKey;
        private readonly GameSaveCenterSettings settings;
        private readonly DataGridColumnLayoutController layout;
        private readonly DataGridColumnVisibilityController visibility;
        private readonly DataGridStableSortController sort;
        private readonly DataGrid grid;
        private readonly ComboBox selector;
        private readonly TextBox nameBox;
        private readonly Button saveButton;
        private readonly Button copyButton;
        private readonly Button renameButton;
        private readonly Button deleteButton;
        private readonly TextBlock dirtyStatus;
        private readonly Button restoreButton;
        private readonly Button resetLayoutButton;
        private readonly ContextMenu resetMenu;
        private readonly MenuItem resetCurrentMenuItem;
        private readonly MenuItem resetAllMenuItem;
        private readonly Action persist;
        private readonly Func<string> captureFilterPresetId;
        private readonly Action<string> applyFilterPresetId;
        private readonly Action<bool> setFilterPersistenceEnabled;
        private readonly INotifyPropertyChanged? viewStateNotifications;
        private readonly Func<string>? captureFilterStateFingerprint;
        private readonly Func<string, string?>? filterPresetFingerprint;
        private readonly Func<string, string, string, string, bool, Task<bool>> confirm;
        private readonly Action<string> reportStatus;
        private readonly List<ViewConfigurationOption> options = new List<ViewConfigurationOption>();
        private readonly DependencyPropertyDescriptor? columnWidthDescriptor;
        private readonly DependencyPropertyDescriptor? columnVisibilityDescriptor;
        private bool applyingSelection;
        private bool disposed;
        private string activeDensityMode;
        private string? baselineFilterStateFingerprint;

        internal DataGridViewConfigurationController(
            FrameworkElement resourceOwner,
            string workspaceKey,
            GameSaveCenterSettings settings,
            DataGridColumnLayoutController layout,
            DataGridColumnVisibilityController visibility,
            DataGridStableSortController sort,
            ComboBox selector,
            TextBox nameBox,
            Button saveButton,
            Button copyButton,
            Button renameButton,
            Button deleteButton,
            TextBlock dirtyStatus,
            Button restoreButton,
            Button resetLayoutButton,
            Action persist,
            Func<string>? captureFilterPresetId,
            Action<string>? applyFilterPresetId,
            Action<bool>? setFilterPersistenceEnabled,
            INotifyPropertyChanged? viewStateNotifications,
            Func<string>? captureFilterStateFingerprint,
            Func<string, string?>? filterPresetFingerprint,
            Func<string, string, string, string, bool, Task<bool>> confirm,
            Action<string> reportStatus)
        {
            this.resourceOwner = resourceOwner ?? throw new ArgumentNullException(nameof(resourceOwner));
            this.workspaceKey = string.IsNullOrWhiteSpace(workspaceKey)
                ? throw new ArgumentException("Workspace key is required.", nameof(workspaceKey))
                : workspaceKey;
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.layout = layout ?? throw new ArgumentNullException(nameof(layout));
            this.visibility = visibility ?? throw new ArgumentNullException(nameof(visibility));
            this.sort = sort ?? throw new ArgumentNullException(nameof(sort));
            grid = layout.Grid;
            this.selector = selector ?? throw new ArgumentNullException(nameof(selector));
            this.nameBox = nameBox ?? throw new ArgumentNullException(nameof(nameBox));
            this.saveButton = saveButton ?? throw new ArgumentNullException(nameof(saveButton));
            this.copyButton = copyButton ?? throw new ArgumentNullException(nameof(copyButton));
            this.renameButton = renameButton ?? throw new ArgumentNullException(nameof(renameButton));
            this.deleteButton = deleteButton ?? throw new ArgumentNullException(nameof(deleteButton));
            this.dirtyStatus = dirtyStatus ?? throw new ArgumentNullException(nameof(dirtyStatus));
            this.restoreButton = restoreButton ?? throw new ArgumentNullException(nameof(restoreButton));
            this.resetLayoutButton = resetLayoutButton ?? throw new ArgumentNullException(nameof(resetLayoutButton));
            this.persist = persist ?? throw new ArgumentNullException(nameof(persist));
            this.captureFilterPresetId = captureFilterPresetId ?? (() => string.Empty);
            this.applyFilterPresetId = applyFilterPresetId ?? (_ => { });
            this.setFilterPersistenceEnabled = setFilterPersistenceEnabled ?? (_ => { });
            this.viewStateNotifications = viewStateNotifications;
            this.captureFilterStateFingerprint = captureFilterStateFingerprint;
            this.filterPresetFingerprint = filterPresetFingerprint;
            this.confirm = confirm ?? throw new ArgumentNullException(nameof(confirm));
            this.reportStatus = reportStatus ?? throw new ArgumentNullException(nameof(reportStatus));
            resetMenu = CreateResetMenu(out resetCurrentMenuItem, out resetAllMenuItem);
            this.resetLayoutButton.ContextMenu = resetMenu;
            activeDensityMode = settings.DefaultTableDensityMode;
            columnWidthDescriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.WidthProperty, typeof(DataGridColumn));
            columnVisibilityDescriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.VisibilityProperty, typeof(DataGridColumn));

            selector.ItemsSource = options;
            selector.SelectionChanged += OnSelectionChanged;
            grid.Sorting += OnGridSorting;
            if (viewStateNotifications != null)
                viewStateNotifications.PropertyChanged += OnViewStatePropertyChanged;
            foreach (var column in grid.Columns)
            {
                columnWidthDescriptor?.AddValueChanged(column, OnColumnPresentationChanged);
                columnVisibilityDescriptor?.AddValueChanged(column, OnColumnPresentationChanged);
            }
            saveButton.Click += OnSaveCurrentClick;
            copyButton.Click += OnSaveCopyClick;
            renameButton.Click += OnRenameClick;
            deleteButton.Click += OnDeleteClick;
            restoreButton.Click += OnRestoreClick;
            this.resetLayoutButton.Click += OnResetMenuButtonClick;
            resetCurrentMenuItem.Click += OnResetCurrentLayoutClick;
            resetAllMenuItem.Click += OnResetAllLayoutsClick;
            DataGridLayoutResetCoordinator.AllLayoutsReset += OnAllLayoutsReset;
            AutomationProperties.SetName(selector, "表格视图");
            AutomationProperties.SetName(this.resetLayoutButton, "预览表格布局重置范围");
            RestoreSelection();
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            selector.SelectionChanged -= OnSelectionChanged;
            grid.Sorting -= OnGridSorting;
            if (viewStateNotifications != null)
                viewStateNotifications.PropertyChanged -= OnViewStatePropertyChanged;
            foreach (var column in grid.Columns)
            {
                columnWidthDescriptor?.RemoveValueChanged(column, OnColumnPresentationChanged);
                columnVisibilityDescriptor?.RemoveValueChanged(column, OnColumnPresentationChanged);
            }
            saveButton.Click -= OnSaveCurrentClick;
            copyButton.Click -= OnSaveCopyClick;
            renameButton.Click -= OnRenameClick;
            deleteButton.Click -= OnDeleteClick;
            restoreButton.Click -= OnRestoreClick;
            resetLayoutButton.Click -= OnResetMenuButtonClick;
            resetCurrentMenuItem.Click -= OnResetCurrentLayoutClick;
            resetAllMenuItem.Click -= OnResetAllLayoutsClick;
            DataGridLayoutResetCoordinator.AllLayoutsReset -= OnAllLayoutsReset;
            resetMenu.IsOpen = false;
            if (ReferenceEquals(resetLayoutButton.ContextMenu, resetMenu))
                resetLayoutButton.ContextMenu = null;
        }

        private ContextMenu CreateResetMenu(out MenuItem resetCurrent, out MenuItem resetAll)
        {
            var result = new ContextMenu
            {
                Placement = PlacementMode.Bottom,
                PlacementTarget = resetLayoutButton,
                StaysOpen = false
            };
            if (resetLayoutButton.TryFindResource("GscColumnPickerContextMenuStyle") is Style menuStyle)
                result.Style = menuStyle;
            var itemStyle = resetLayoutButton.TryFindResource("GscColumnPickerMenuItemStyle") as Style;
            resetCurrent = new MenuItem
            {
                Header = "重置当前表格布局…",
                Style = itemStyle,
                StaysOpenOnClick = false
            };
            AutomationProperties.SetName(resetCurrent, "预览并重置当前表格布局");
            AutomationProperties.SetHelpText(resetCurrent, "只影响当前表格或当前选中的已保存视图。");
            resetAll = new MenuItem
            {
                Header = "重置全部工作视图…",
                Style = itemStyle,
                StaysOpenOnClick = false
            };
            AutomationProperties.SetName(resetAll, "预览并重置全部工作视图布局");
            AutomationProperties.SetHelpText(resetAll, "重置所有表格布局，保留筛选预设和业务设置。");
            result.Items.Add(resetCurrent);
            result.Items.Add(new Separator());
            result.Items.Add(resetAll);
            return result;
        }

        private void OnResetMenuButtonClick(object sender, RoutedEventArgs e)
        {
            if (disposed) return;
            resetMenu.PlacementTarget = resetLayoutButton;
            resetMenu.Placement = PlacementMode.Bottom;
            resetMenu.IsOpen = true;
            e.Handled = true;
        }

        private async void OnResetCurrentLayoutClick(object sender, RoutedEventArgs e)
        {
            if (disposed) return;
            var selected = SelectedConfiguration;
            try
            {
                var impact = ViewConfigurationCatalog.BuildCurrentLayoutResetImpact(settings, workspaceKey, selected);
                if (!await confirm("确认重置当前表格布局", impact, "重置当前表格", "取消", true).ConfigureAwait(true))
                    return;

                if (selected == null)
                    ResetDefaultTableLayout();
                else if (!ResetSelectedViewLayout(selected))
                    return;

                resetMenu.IsOpen = false;
                reportStatus(selected == null
                    ? $"已重置“{workspaceKey}”表格布局；筛选预设和业务设置未更改。"
                    : $"已重置视图“{selected.Name}”的表格布局；名称和筛选预设已保留。");
            }
            catch (Exception error)
            {
                reportStatus($"重置当前表格布局失败：{error.Message}");
            }
        }

        private bool ResetSelectedViewLayout(ViewConfigurationDefinition selected)
        {
            SetPersistenceMode(isDefault: false);
            layout.ApplyConfigurationWidths(new Dictionary<string, double>(StringComparer.Ordinal));
            visibility.ApplyVisibleColumnKeys(visibility.CaptureAllColumnKeys());
            sort.ResetToDefaultSort();
            activeDensityMode = TableDensityPolicy.NormalizeMode(settings.DefaultTableDensityMode);
            TableDensityPolicy.ApplyResources(resourceOwner, activeDensityMode);

            var resetLayout = CaptureCurrent(selected.Name);
            resetLayout.FilterPresetId = selected.FilterPresetId;
            resetLayout.ColumnWidths.Clear();
            var result = ViewConfigurationCatalog.ResetConfigurationLayout(
                settings, workspaceKey, selected.Id, resetLayout, out var reset);
            if (result != ViewConfigurationMutationResult.Success || reset == null)
            {
                ReportMutationResult(result);
                return false;
            }

            RebuildOptions();
            var option = options.First(item => item.Configuration?.Id == reset.Id);
            SetSelection(option, apply: false, persistSelection: false);
            baselineFilterStateFingerprint = filterPresetFingerprint?.Invoke(reset.FilterPresetId);
            UpdateButtons();
            persist();
            return true;
        }

        private void ResetDefaultTableLayout()
        {
            SetPersistenceMode(isDefault: false);
            settings.ResetDataGridColumnWidths(workspaceKey);
            settings.ResetDataGridHiddenColumnKeys(workspaceKey);
            layout.ResetToDefaults();
            visibility.ResetToDefaults();
            sort.ResetToDefaultSort();
            activeDensityMode = TableDensityPolicy.NormalizeMode(settings.DefaultTableDensityMode);
            TableDensityPolicy.ApplyResources(resourceOwner, activeDensityMode);
            SetPersistenceMode(isDefault: true);
            persist();
            UpdateButtons();
        }

        private async void OnResetAllLayoutsClick(object sender, RoutedEventArgs e)
        {
            if (disposed) return;
            try
            {
                var impact = ViewConfigurationCatalog.BuildAllLayoutsResetImpact(settings);
                if (!await confirm("确认重置全部工作视图", impact, "重置全部视图", "取消", true).ConfigureAwait(true))
                    return;
                if (!ViewConfigurationCatalog.ResetAllLayouts(settings))
                {
                    resetMenu.IsOpen = false;
                    reportStatus("所有表格布局已是默认状态；筛选预设和业务设置未更改。");
                    return;
                }

                persist();
                resetMenu.IsOpen = false;
                DataGridLayoutResetCoordinator.NotifyAllLayoutsReset();
                reportStatus("已重置全部工作视图；所有筛选预设和业务设置均已保留。");
            }
            catch (Exception error)
            {
                reportStatus($"重置全部工作视图失败：{error.Message}");
            }
        }

        private void OnAllLayoutsReset(object? sender, EventArgs e)
        {
            if (disposed) return;
            if (!resourceOwner.Dispatcher.CheckAccess())
            {
                resourceOwner.Dispatcher.BeginInvoke(new Action(() => OnAllLayoutsReset(sender, e)));
                return;
            }
            RebuildOptions();
            SetSelection(options[0], apply: false, persistSelection: false);
            SetPersistenceMode(isDefault: true);
            layout.ApplyStoredWidthsFromDefault();
            visibility.ApplyStoredVisibility();
            sort.ResetToDefaultSort();
            activeDensityMode = TableDensityPolicy.NormalizeMode(settings.DefaultTableDensityMode);
            TableDensityPolicy.ApplyResources(resourceOwner, activeDensityMode);
            baselineFilterStateFingerprint = null;
            UpdateButtons();
        }

        private void RestoreSelection()
        {
            RebuildOptions();
            var activeId = settings.GetActiveViewConfigurationId(workspaceKey);
            var selected = options.FirstOrDefault(option => option.Configuration?.Id == activeId);
            if (!string.IsNullOrEmpty(activeId) && selected == null)
            {
                settings.SetActiveViewConfigurationId(workspaceKey, null);
                persist();
            }
            SetSelection(selected ?? options[0], apply: true, persistSelection: false);
            UpdateButtons();
        }

        private void RebuildOptions()
        {
            options.Clear();
            options.Add(ViewConfigurationOption.Default);
            options.AddRange(ViewConfigurationCatalog.GetForWorkspace(settings, workspaceKey)
                .Select(configuration => new ViewConfigurationOption(configuration)));
            selector.Items.Refresh();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (disposed || applyingSelection || !(selector.SelectedItem is ViewConfigurationOption option)) return;
            SetSelection(option, apply: true, persistSelection: true);
        }

        private void SetSelection(ViewConfigurationOption option, bool apply, bool persistSelection)
        {
            applyingSelection = true;
            try
            {
                selector.SelectedItem = options.FirstOrDefault(item => item.Configuration?.Id == option.Configuration?.Id)
                    ?? options[0];
                var selected = (ViewConfigurationOption)selector.SelectedItem;
                nameBox.Text = selected.Configuration?.Name ?? string.Empty;
                if (apply)
                    Apply(selected.Configuration);
                else
                    SetPersistenceMode(selected.Configuration == null);
                baselineFilterStateFingerprint = selected.Configuration == null
                    ? null
                    : captureFilterStateFingerprint?.Invoke();
                if (persistSelection)
                {
                    settings.SetActiveViewConfigurationId(workspaceKey, selected.Configuration?.Id);
                    persist();
                }
            }
            finally
            {
                applyingSelection = false;
                UpdateButtons();
            }
        }

        private void Apply(ViewConfigurationDefinition? configuration)
        {
            if (configuration == null)
            {
                applyFilterPresetId(string.Empty);
                SetPersistenceMode(isDefault: true);
                layout.ApplyStoredWidthsFromDefault();
                visibility.ApplyStoredVisibility();
                sort.ResetToDefaultSort();
                activeDensityMode = settings.DefaultTableDensityMode;
                TableDensityPolicy.ApplyResources(resourceOwner, activeDensityMode);
                return;
            }

            SetPersistenceMode(isDefault: false);
            layout.ApplyConfigurationWidths(configuration.ColumnWidths);
            visibility.ApplyVisibleColumnKeys(configuration.VisibleColumnKeys);
            sort.ApplySort(
                configuration.SortColumnKey,
                string.Equals(configuration.SortDirection, "Descending", StringComparison.OrdinalIgnoreCase)
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending);
            activeDensityMode = TableDensityPolicy.NormalizeMode(configuration.DensityMode);
            TableDensityPolicy.ApplyResources(resourceOwner, activeDensityMode);
            applyFilterPresetId(configuration.FilterPresetId);
        }

        private void SetPersistenceMode(bool isDefault)
        {
            setFilterPersistenceEnabled(isDefault);
            layout.SetSettingsPersistenceEnabled(isDefault);
            visibility.SetSettingsPersistenceEnabled(isDefault);
        }

        private ViewConfigurationDefinition CaptureCurrent(string name)
        {
            return new ViewConfigurationDefinition
            {
                Name = name,
                WorkspaceKey = workspaceKey,
                FilterPresetId = captureFilterPresetId(),
                SortColumnKey = sort.ActiveColumnKey,
                SortDirection = sort.ActiveDirection == ListSortDirection.Descending ? "Descending" : "Ascending",
                VisibleColumnKeys = visibility.CaptureVisibleColumnKeys().ToList(),
                ColumnWidths = layout.CaptureConfigurationWidths().ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal),
                DensityMode = activeDensityMode
            };
        }

        private void OnSaveCurrentClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanSaveCurrentFilterState()) return;
                var selected = SelectedConfiguration;
                var result = ViewConfigurationCatalog.SaveCurrent(
                    settings,
                    workspaceKey,
                    selected == null ? nameBox.Text : selected.Name,
                    CaptureCurrent(selected?.Name ?? nameBox.Text),
                    selected?.Id,
                    saveCopy: false,
                    out var saved);
                CompleteMutation(result, saved, "已保存当前视图");
            }
            catch (Exception error)
            {
                reportStatus($"保存视图失败：{error.Message}");
            }
        }

        private void OnSaveCopyClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanSaveCurrentFilterState()) return;
                var result = ViewConfigurationCatalog.SaveCurrent(
                    settings,
                    workspaceKey,
                    nameBox.Text,
                    CaptureCurrent(nameBox.Text),
                    SelectedConfiguration?.Id,
                    saveCopy: true,
                    out var saved);
                CompleteMutation(result, saved, "已另存视图副本");
            }
            catch (Exception error)
            {
                reportStatus($"另存视图失败：{error.Message}");
            }
        }

        private async void OnRenameClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var selected = SelectedConfiguration;
                if (selected == null) return;
                var name = ViewConfigurationCatalog.NormalizeName(nameBox.Text);
                if (name.Length == 0)
                {
                    ReportMutationResult(ViewConfigurationMutationResult.InvalidName);
                    return;
                }
                if (string.Equals(name, selected.Name, StringComparison.Ordinal)) return;
                if (!await confirm("重命名视图", $"是否将“{selected.Name}”重命名为“{name}”？", "重命名", "取消", false).ConfigureAwait(true))
                    return;
                var result = ViewConfigurationCatalog.Rename(settings, workspaceKey, selected.Id, name, out var renamed);
                CompleteMutation(result, renamed, "已重命名视图");
            }
            catch (Exception error)
            {
                reportStatus($"重命名视图失败：{error.Message}");
            }
        }

        private async void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var selected = SelectedConfiguration;
                if (selected == null) return;
                if (!await confirm("删除已保存视图", $"是否删除视图“{selected.Name}”？此操作不可自动恢复。", "删除", "取消", true).ConfigureAwait(true))
                    return;
                var result = ViewConfigurationCatalog.Delete(settings, workspaceKey, selected.Id, out var deleted);
                if (result != ViewConfigurationMutationResult.Success)
                {
                    ReportMutationResult(result);
                    return;
                }

                RebuildOptions();
                SetSelection(options[0], apply: true, persistSelection: true);
                reportStatus($"已删除视图“{deleted!.Name}”，当前已恢复默认视图。");
            }
            catch (Exception error)
            {
                reportStatus($"删除视图失败：{error.Message}");
            }
        }

        private void CompleteMutation(ViewConfigurationMutationResult result, ViewConfigurationDefinition? configuration, string successMessage)
        {
            if (result != ViewConfigurationMutationResult.Success || configuration == null)
            {
                ReportMutationResult(result);
                return;
            }
            RebuildOptions();
            var option = options.First(item => item.Configuration?.Id == configuration.Id);
            SetSelection(option, apply: false, persistSelection: true);
            reportStatus($"{successMessage}“{configuration.Name}”。");
        }

        private void ReportMutationResult(ViewConfigurationMutationResult result)
        {
            reportStatus(result switch
            {
                ViewConfigurationMutationResult.InvalidName => $"请输入 1 至 {ViewConfigurationCatalog.MaximumNameLength} 个字符的视图名称。",
                ViewConfigurationMutationResult.DuplicateName => "当前页面已有同名视图，请使用其他名称。",
                ViewConfigurationMutationResult.LimitReached => $"最多保存 {ViewConfigurationCatalog.MaximumConfigurations} 个视图，请先删除不再使用的视图。",
                ViewConfigurationMutationResult.MissingConfiguration => "所选视图已不存在，已保留当前页面状态。",
                _ => "视图操作未完成。"
            });
        }

        private void UpdateButtons()
        {
            var hasSavedView = SelectedConfiguration != null;
            renameButton.IsEnabled = hasSavedView;
            deleteButton.IsEnabled = hasSavedView;
            saveButton.ToolTip = hasSavedView ? "将当前布局更新到已选视图" : "将当前默认布局保存为命名视图";
            copyButton.ToolTip = hasSavedView ? "以新名称另存当前布局" : "以新名称保存当前默认布局副本";
            var isDirty = IsSelectedViewDirty(out var filterChanged);
            dirtyStatus.Text = filterChanged
                ? "筛选条件有未保存更改"
                : "视图有未保存更改";
            dirtyStatus.ToolTip = filterChanged
                ? "先保存并应用此筛选为筛选预设，再保存视图；也可以恢复已保存视图。"
                : "当前排列与已保存视图不同。恢复会放弃这些临时改动。";
            dirtyStatus.Visibility = isDirty ? Visibility.Visible : Visibility.Collapsed;
            restoreButton.Visibility = isDirty ? Visibility.Visible : Visibility.Collapsed;
            restoreButton.IsEnabled = isDirty;
        }

        private bool IsSelectedViewDirty(out bool filterChanged)
        {
            filterChanged = false;
            var saved = SelectedConfiguration;
            if (saved == null) return false;

            var current = CaptureCurrent(saved.Name);
            var viewChanged = !AreEquivalent(saved, current);
            var currentFilterState = captureFilterStateFingerprint?.Invoke();
            filterChanged = baselineFilterStateFingerprint != null
                && currentFilterState != null
                && !string.Equals(baselineFilterStateFingerprint, currentFilterState, StringComparison.Ordinal);
            return viewChanged || filterChanged;
        }

        private bool CanSaveCurrentFilterState()
        {
            if (captureFilterStateFingerprint == null || filterPresetFingerprint == null) return true;
            var current = captureFilterStateFingerprint();
            var preset = filterPresetFingerprint(captureFilterPresetId());
            if (current == null || preset == null || string.Equals(current, preset, StringComparison.Ordinal))
                return true;

            reportStatus("当前筛选尚未保存在所选筛选预设中。请先保存并应用筛选预设，或恢复已保存视图。");
            return false;
        }

        private void OnRestoreClick(object sender, RoutedEventArgs e)
        {
            var selected = SelectedConfiguration;
            if (disposed || selected == null) return;
            Apply(selected);
            baselineFilterStateFingerprint = captureFilterStateFingerprint?.Invoke();
            UpdateButtons();
            reportStatus($"已恢复已保存视图“{selected.Name}”。");
        }

        private void OnGridSorting(object sender, DataGridSortingEventArgs e)
            => UpdateButtons();

        private void OnColumnPresentationChanged(object? sender, EventArgs e)
            => UpdateButtons();

        private void OnViewStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName)
                || e.PropertyName == "SelectedTaskFilterPreset"
                || e.PropertyName.StartsWith("Task", StringComparison.Ordinal))
                UpdateButtons();
        }

        private static bool AreEquivalent(ViewConfigurationDefinition saved, ViewConfigurationDefinition current)
        {
            if (!string.Equals(saved.FilterPresetId, current.FilterPresetId, StringComparison.Ordinal)
                || !string.Equals(saved.SortColumnKey, current.SortColumnKey, StringComparison.Ordinal)
                || !string.Equals(saved.SortDirection, current.SortDirection, StringComparison.Ordinal)
                || !string.Equals(saved.DensityMode, current.DensityMode, StringComparison.Ordinal)
                || !saved.VisibleColumnKeys.SequenceEqual(current.VisibleColumnKeys, StringComparer.Ordinal)
                || saved.ColumnWidths.Count != current.ColumnWidths.Count)
                return false;

            foreach (var pair in saved.ColumnWidths)
            {
                if (!current.ColumnWidths.TryGetValue(pair.Key, out var width)
                    || Math.Abs(pair.Value - width) > 0.01d)
                    return false;
            }
            return true;
        }

        private ViewConfigurationDefinition? SelectedConfiguration
            => selector.SelectedItem is ViewConfigurationOption option ? option.Configuration : null;

        private sealed class ViewConfigurationOption
        {
            internal static readonly ViewConfigurationOption Default = new ViewConfigurationOption();
            private ViewConfigurationOption() => DisplayName = "默认视图";
            internal ViewConfigurationOption(ViewConfigurationDefinition configuration)
            {
                Configuration = configuration;
                DisplayName = configuration.Name;
            }
            internal string DisplayName { get; }
            internal ViewConfigurationDefinition? Configuration { get; }
            public override string ToString() => DisplayName;
        }
    }
}
