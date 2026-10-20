using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GameSaveCenter.Playnite.Settings;

namespace GameSaveCenter.Playnite.ViewModels
{
    public sealed partial class DashboardViewModel
    {
        private bool isAddingWorklistEntry;

        public ICommand AddSelectedBackupToWorklistCommand { get; private set; } = null!;
        public ICommand AddSelectedMediaToWorklistCommand { get; private set; } = null!;
        public ICommand AddSelectedTaskToWorklistCommand { get; private set; } = null!;
        public ICommand AddSelectedFindingToWorklistCommand { get; private set; } = null!;

        partial void OnWorklistsInitialize()
        {
            AddSelectedBackupToWorklistCommand = new RelayCommand(
                _ => Observe(AddWorklistEntryAsync(() => WorklistEntryPreviewFactory.FromBackupVersion(
                    SelectedBackup,
                    ResolveWorklistGameName(SelectedBackup?.PlayniteId, SelectedBackup?.LudusaviName ?? string.Empty)))),
                _ => !isAddingWorklistEntry && SelectedBackup != null);
            AddSelectedMediaToWorklistCommand = new RelayCommand(
                _ => Observe(AddWorklistEntryAsync(() => WorklistEntryPreviewFactory.FromMediaItem(
                    SelectedMedia,
                    ResolveWorklistGameName(SelectedMedia?.PlayniteId, string.Empty)))),
                _ => !isAddingWorklistEntry && SelectedMedia != null);
            AddSelectedTaskToWorklistCommand = new RelayCommand(
                _ => Observe(AddWorklistEntryAsync(() => WorklistEntryPreviewFactory.FromBackgroundTask(SelectedTask))),
                _ => !isAddingWorklistEntry && SelectedTask != null);
            AddSelectedFindingToWorklistCommand = new RelayCommand(
                _ => Observe(AddWorklistEntryAsync(() => WorklistEntryPreviewFactory.FromValidationFinding(SelectedFinding))),
                _ => !isAddingWorklistEntry && WorklistEntryPreviewFactory.FromValidationFinding(SelectedFinding) != null);
        }

        private string ResolveWorklistGameName(string? playniteId, string? fallback)
        {
            if (string.IsNullOrWhiteSpace(playniteId)) return fallback ?? string.Empty;
            var game = Games.FirstOrDefault(candidate =>
                string.Equals(candidate.PlayniteId, playniteId, StringComparison.OrdinalIgnoreCase));
            var gameName = game?.Name;
            var normalizedGameName = gameName ?? string.Empty;
            return string.IsNullOrWhiteSpace(normalizedGameName) ? fallback ?? string.Empty : normalizedGameName;
        }

        private async Task AddWorklistEntryAsync(Func<WorklistEntryPreview?> createPreview)
        {
            if (isAddingWorklistEntry) return;
            var preview = createPreview();
            if (preview == null)
            {
                StatusMessage = "所选对象没有可保存的稳定编号，未加入清单。";
                return;
            }

            isAddingWorklistEntry = true;
            RaiseCommandStates();
            try
            {
                var settings = plugin.Settings;
                var initialLists = WorklistDefinition.NormalizeMany(settings.Worklists);
                var activeList = ResolveWorklist(initialLists, settings.ActiveWorklistId);
                var targetListId = activeList?.Id ?? Guid.NewGuid();
                var targetListName = activeList?.Name ?? "待办清单";
                if (activeList?.Contains(preview.Kind, preview.StableObjectId) == true)
                {
                    StatusMessage = $"“{preview.ObjectTitle}”已在“{targetListName}”中。";
                    return;
                }
                if (activeList != null && activeList.Items.Count >= WorklistDefinition.MaximumItemsPerWorklist)
                {
                    StatusMessage = $"“{targetListName}”已达到 {WorklistDefinition.MaximumItemsPerWorklist} 项上限，未加入对象。";
                    return;
                }

                var confirmationText = string.Join(Environment.NewLine, new[]
                {
                    $"对象类型：{preview.ObjectKindDisplay}",
                    preview.ObjectTitle,
                    preview.Summary,
                    $"目标清单：{targetListName}",
                    "确认后只保存稳定对象引用，不会执行备份、恢复、校验、重试或修改对象状态。"
                }.Where(line => !string.IsNullOrWhiteSpace(line)));
                if (!await plugin.ConfirmAsync("加入待办清单", confirmationText, "加入清单", "返回"))
                {
                    StatusMessage = "已取消加入待办；对象状态未改变。";
                    return;
                }

                var latestLists = WorklistDefinition.NormalizeMany(settings.Worklists);
                var target = latestLists.FirstOrDefault(list => list.Id == targetListId);
                if (target == null)
                {
                    if (activeList != null)
                    {
                        StatusMessage = "当前清单在确认期间已变化；请重新选择对象后再加入。";
                        return;
                    }
                    if (latestLists.Count >= WorklistDefinition.MaximumWorklists)
                    {
                        StatusMessage = $"已达到 {WorklistDefinition.MaximumWorklists} 份清单上限，未加入对象。";
                        return;
                    }
                    target = new WorklistDefinition { Id = targetListId, Name = targetListName };
                    latestLists.Add(target);
                }

                var addResult = target.TryAdd(preview.ToRecord());
                if (addResult == WorklistItemAddResult.AlreadyPresent)
                {
                    StatusMessage = $"“{preview.ObjectTitle}”已在“{target.Name}”中。";
                    return;
                }
                if (addResult == WorklistItemAddResult.LimitReached)
                {
                    StatusMessage = $"“{target.Name}”已达到 {WorklistDefinition.MaximumItemsPerWorklist} 项上限，未加入对象。";
                    return;
                }
                if (addResult != WorklistItemAddResult.Added)
                {
                    StatusMessage = "对象编号无效或超出长度上限，未加入清单。";
                    return;
                }

                var previousLists = WorklistDefinition.NormalizeMany(settings.Worklists);
                var previousActiveId = settings.ActiveWorklistId;
                settings.Worklists = latestLists;
                settings.ActiveWorklistId = target.Id.ToString("D");
                try
                {
                    plugin.SavePluginSettings(settings);
                }
                catch
                {
                    settings.Worklists = previousLists;
                    settings.ActiveWorklistId = previousActiveId;
                    throw;
                }

                StatusMessage = $"已加入“{target.Name}”（共 {target.Items.Count} 项）；仅保存人工待办引用。";
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "GameSaveCenter could not add a worklist reference.");
                StatusMessage = "加入待办失败；对象状态未改变，请稍后重试。";
            }
            finally
            {
                isAddingWorklistEntry = false;
                RaiseCommandStates();
            }
        }

        private static WorklistDefinition? ResolveWorklist(
            IReadOnlyList<WorklistDefinition> worklists,
            string? activeWorklistId)
        {
            if (Guid.TryParse(activeWorklistId, out var id))
            {
                var active = worklists.FirstOrDefault(list => list.Id == id);
                if (active != null) return active;
            }
            return worklists.FirstOrDefault();
        }
    }
}
