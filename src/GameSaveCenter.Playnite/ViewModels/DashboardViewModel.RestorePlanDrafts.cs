using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Settings;

namespace GameSaveCenter.Playnite.ViewModels
{
    public sealed partial class DashboardViewModel
    {
        private RestoreMappingPreviewDto? restorePlanMappingPreview;
        private string restorePlanDraftStatus = "先预览所选版本的映射，再保存只读方案草稿。";

        public ObservableCollection<RestorePlanDraftDefinition> RestorePlanDrafts { get; } = new ObservableCollection<RestorePlanDraftDefinition>();
        public ICommand PreviewRestorePlanMappingCommand { get; private set; } = null!;
        public ICommand SaveRestorePlanDraftCommand { get; private set; } = null!;
        public ICommand DeleteRestorePlanDraftCommand { get; private set; } = null!;

        public RestoreMappingPreviewDto? RestorePlanMappingPreview
        {
            get => restorePlanMappingPreview;
            private set
            {
                if (ReferenceEquals(restorePlanMappingPreview, value)) return;
                restorePlanMappingPreview = value;
                OnPropertyChanged(nameof(RestorePlanMappingPreview));
                OnPropertyChanged(nameof(CanSaveRestorePlanDraft));
                RaiseCommandStates();
            }
        }

        public string RestorePlanDraftStatus
        {
            get => restorePlanDraftStatus;
            private set => SetValue(ref restorePlanDraftStatus, value ?? string.Empty);
        }

        public bool CanSaveRestorePlanDraft
        {
            get
            {
                var preview = RestorePlanMappingPreview;
                var age = preview == null ? TimeSpan.MaxValue : DateTime.UtcNow - preview.PreviewedUtc.ToUniversalTime();
                return preview?.Success == true
                    && SelectedGame != null
                    && SelectedBackup != null
                    && string.Equals(preview.PlayniteId, SelectedGame.PlayniteId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(preview.BackupId, SelectedBackup.BackupId, StringComparison.OrdinalIgnoreCase)
                    && age >= TimeSpan.Zero
                    && age <= TimeSpan.FromMinutes(30);
            }
        }

        partial void OnRestorePlanDraftsInitialize()
        {
            foreach (var draft in plugin.Settings.RestorePlanDrafts)
                RestorePlanDrafts.Add(draft);

            PreviewRestorePlanMappingCommand = new RelayCommand(
                _ => Run(PreviewRestorePlanMappingAsync),
                _ => !IsBusy && SelectedGame != null && SelectedBackup != null && SelectedGame.LudusaviMatched && Snapshot.LudusaviAvailable);
            SaveRestorePlanDraftCommand = new RelayCommand(
                _ => Run(SaveRestorePlanDraftAsync),
                _ => !IsBusy && CanSaveRestorePlanDraft);
            DeleteRestorePlanDraftCommand = new RelayCommand(
                value =>
                {
                    if (value is RestorePlanDraftDefinition draft) Run(() => DeleteRestorePlanDraftAsync(draft));
                },
                value => !IsBusy && value is RestorePlanDraftDefinition);
        }

        private async Task PreviewRestorePlanMappingAsync()
        {
            var game = SelectedGame ?? throw new InvalidOperationException("请先选择游戏。");
            var backup = SelectedBackup ?? throw new InvalidOperationException("请先选择备份版本。");
            var gameId = game.PlayniteId;
            var backupId = backup.BackupId;
            RestorePlanMappingPreview = null;
            RestorePlanDraftStatus = "正在只读检查版本映射；不会写入当前存档。";
            var preview = await plugin.RequestAsync<RestoreMappingPreviewDto>(MessageTypes.RestorePreview,
                new RestoreMappingPreviewRequestDto { PlayniteId = gameId, BackupId = backupId },
                TimeSpan.FromMinutes(2));

            if (!string.Equals(SelectedGame?.PlayniteId, gameId, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(SelectedBackup?.BackupId, backupId, StringComparison.OrdinalIgnoreCase))
            {
                ResetRestorePlanMappingPreview("选择已变化，旧版本预览已丢弃；请重新检查当前选择。");
                return;
            }

            RestorePlanMappingPreview = preview;
            RestorePlanDraftStatus = preview.Success
                ? preview.Summary
                : preview.Summary + (string.IsNullOrWhiteSpace(preview.ErrorCode) ? string.Empty : $"（{preview.ErrorCode}）");
        }

        private Task SaveRestorePlanDraftAsync()
        {
            if (!CanSaveRestorePlanDraft)
                throw new InvalidOperationException("映射预览已失效或与当前选择不匹配，请重新预览后保存。");

            var game = SelectedGame ?? throw new InvalidOperationException("请先选择游戏。");
            var target = SelectedBackup ?? throw new InvalidOperationException("请先选择备份版本。");
            var mapping = RestorePlanMappingPreview ?? throw new InvalidOperationException("请先生成映射预览。");
            var draft = RestorePlanDraftDefinition.Create(game, target, mapping);
            var previous = plugin.Settings.RestorePlanDrafts;
            var candidates = new System.Collections.Generic.List<RestorePlanDraftDefinition> { draft };
            candidates.AddRange(previous);
            var updated = RestorePlanDraftDefinition.NormalizeMany(candidates);
            plugin.Settings.RestorePlanDrafts = updated;
            try
            {
                plugin.SavePluginSettings(plugin.Settings);
            }
            catch
            {
                plugin.Settings.RestorePlanDrafts = previous;
                throw;
            }

            RestorePlanDrafts.Insert(0, draft);
            while (RestorePlanDrafts.Count > RestorePlanDraftDefinition.MaximumSavedDrafts)
                RestorePlanDrafts.RemoveAt(RestorePlanDrafts.Count - 1);
            RestorePlanMappingPreview = null;
            RestorePlanDraftStatus = "只读方案草稿已保存；它不包含执行授权，也不会写入当前存档。";
            return Task.CompletedTask;
        }

        private Task DeleteRestorePlanDraftAsync(RestorePlanDraftDefinition draft)
        {
            var previous = plugin.Settings.RestorePlanDrafts;
            var updated = previous.Where(item => !string.Equals(item.DraftId, draft.DraftId, StringComparison.OrdinalIgnoreCase)).ToList();
            if (updated.Count == previous.Count) return Task.CompletedTask;

            plugin.Settings.RestorePlanDrafts = updated;
            try
            {
                plugin.SavePluginSettings(plugin.Settings);
            }
            catch
            {
                plugin.Settings.RestorePlanDrafts = previous;
                throw;
            }

            var visible = RestorePlanDrafts.FirstOrDefault(item => string.Equals(item.DraftId, draft.DraftId, StringComparison.OrdinalIgnoreCase));
            if (visible != null) RestorePlanDrafts.Remove(visible);
            RestorePlanDraftStatus = "方案草稿已删除。";
            return Task.CompletedTask;
        }

        private void ResetRestorePlanMappingPreview(string? status = null)
        {
            RestorePlanMappingPreview = null;
            if (!string.IsNullOrWhiteSpace(status)) RestorePlanDraftStatus = status!;
            else RestorePlanDraftStatus = "所选游戏或版本已变化；请重新预览映射后再保存草稿。";
        }
    }
}
