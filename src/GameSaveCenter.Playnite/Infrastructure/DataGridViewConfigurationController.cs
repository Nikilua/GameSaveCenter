using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
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
        private readonly ComboBox selector;
        private readonly TextBox nameBox;
        private readonly Button saveButton;
        private readonly Button copyButton;
        private readonly Button renameButton;
        private readonly Button deleteButton;
        private readonly Action persist;
        private readonly Func<string> captureFilterPresetId;
        private readonly Action<string> applyFilterPresetId;
        private readonly Action<bool> setFilterPersistenceEnabled;
        private readonly Func<string, string, string, string, bool, Task<bool>> confirm;
        private readonly Action<string> reportStatus;
        private readonly List<ViewConfigurationOption> options = new List<ViewConfigurationOption>();
        private bool applyingSelection;
        private bool disposed;
        private string activeDensityMode;

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
            Action persist,
            Func<string>? captureFilterPresetId,
            Action<string>? applyFilterPresetId,
            Action<bool>? setFilterPersistenceEnabled,
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
            this.selector = selector ?? throw new ArgumentNullException(nameof(selector));
            this.nameBox = nameBox ?? throw new ArgumentNullException(nameof(nameBox));
            this.saveButton = saveButton ?? throw new ArgumentNullException(nameof(saveButton));
            this.copyButton = copyButton ?? throw new ArgumentNullException(nameof(copyButton));
            this.renameButton = renameButton ?? throw new ArgumentNullException(nameof(renameButton));
            this.deleteButton = deleteButton ?? throw new ArgumentNullException(nameof(deleteButton));
            this.persist = persist ?? throw new ArgumentNullException(nameof(persist));
            this.captureFilterPresetId = captureFilterPresetId ?? (() => string.Empty);
            this.applyFilterPresetId = applyFilterPresetId ?? (_ => { });
            this.setFilterPersistenceEnabled = setFilterPersistenceEnabled ?? (_ => { });
            this.confirm = confirm ?? throw new ArgumentNullException(nameof(confirm));
            this.reportStatus = reportStatus ?? throw new ArgumentNullException(nameof(reportStatus));
            activeDensityMode = settings.DefaultTableDensityMode;

            selector.ItemsSource = options;
            selector.SelectionChanged += OnSelectionChanged;
            saveButton.Click += OnSaveCurrentClick;
            copyButton.Click += OnSaveCopyClick;
            renameButton.Click += OnRenameClick;
            deleteButton.Click += OnDeleteClick;
            AutomationProperties.SetName(selector, "表格视图");
            RestoreSelection();
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            selector.SelectionChanged -= OnSelectionChanged;
            saveButton.Click -= OnSaveCurrentClick;
            copyButton.Click -= OnSaveCopyClick;
            renameButton.Click -= OnRenameClick;
            deleteButton.Click -= OnDeleteClick;
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
