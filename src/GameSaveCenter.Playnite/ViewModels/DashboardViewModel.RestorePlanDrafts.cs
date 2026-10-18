using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Settings;

namespace GameSaveCenter.Playnite.ViewModels
{
    public sealed partial class DashboardViewModel
    {
        private RestoreMappingPreviewDto? restorePlanMappingPreview;
        private string restorePlanDraftStatus = "先预览所选版本的映射，再保存只读方案草稿。";
        private RestorePlanDraftDefinition? selectedRestorePlanDraftFrom;
        private RestorePlanDraftDefinition? selectedRestorePlanDraftTo;
        private RestorePlanDraftDefinition? selectedRestorePlanDraftForExport;
        private string restorePlanDraftExportPreviewText = string.Empty;
        private RestorePlanDraftComparison? restorePlanDraftComparison;

        public ObservableCollection<RestorePlanDraftDefinition> RestorePlanDrafts { get; } = new ObservableCollection<RestorePlanDraftDefinition>();
        public ICommand PreviewRestorePlanMappingCommand { get; private set; } = null!;
        public ICommand SaveRestorePlanDraftCommand { get; private set; } = null!;
        public ICommand DeleteRestorePlanDraftCommand { get; private set; } = null!;
        public ICommand PreviewRestorePlanDraftExportCommand { get; private set; } = null!;
        public ICommand ExportRestorePlanDraftCommand { get; private set; } = null!;
        public ICommand CompareRestorePlanDraftsCommand { get; private set; } = null!;

        public RestorePlanDraftDefinition? SelectedRestorePlanDraftForExport
        {
            get => selectedRestorePlanDraftForExport;
            private set
            {
                if (ReferenceEquals(selectedRestorePlanDraftForExport, value)) return;
                selectedRestorePlanDraftForExport = value;
                OnPropertyChanged(nameof(SelectedRestorePlanDraftForExport));
                RaiseCommandStates();
            }
        }

        public string RestorePlanDraftExportPreviewText
        {
            get => restorePlanDraftExportPreviewText;
            private set
            {
                if (string.Equals(restorePlanDraftExportPreviewText, value, StringComparison.Ordinal)) return;
                restorePlanDraftExportPreviewText = value ?? string.Empty;
                OnPropertyChanged(nameof(RestorePlanDraftExportPreviewText));
                RaiseCommandStates();
            }
        }

        public RestorePlanDraftDefinition? SelectedRestorePlanDraftFrom
        {
            get => selectedRestorePlanDraftFrom;
            set
            {
                if (ReferenceEquals(selectedRestorePlanDraftFrom, value)) return;
                selectedRestorePlanDraftFrom = value;
                OnPropertyChanged(nameof(SelectedRestorePlanDraftFrom));
                RestorePlanDraftComparisonResult = null;
                RaiseCommandStates();
            }
        }

        public RestorePlanDraftDefinition? SelectedRestorePlanDraftTo
        {
            get => selectedRestorePlanDraftTo;
            set
            {
                if (ReferenceEquals(selectedRestorePlanDraftTo, value)) return;
                selectedRestorePlanDraftTo = value;
                OnPropertyChanged(nameof(SelectedRestorePlanDraftTo));
                RestorePlanDraftComparisonResult = null;
                RaiseCommandStates();
            }
        }

        public RestorePlanDraftComparison? RestorePlanDraftComparisonResult
        {
            get => restorePlanDraftComparison;
            private set
            {
                if (ReferenceEquals(restorePlanDraftComparison, value)) return;
                restorePlanDraftComparison = value;
                OnPropertyChanged(nameof(RestorePlanDraftComparisonResult));
            }
        }

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
            PreviewRestorePlanDraftExportCommand = new RelayCommand(
                PreviewRestorePlanDraftExport,
                value => !IsBusy && value is RestorePlanDraftDefinition);
            ExportRestorePlanDraftCommand = new RelayCommand(
                _ => RunLocal(ExportRestorePlanDraft),
                _ => !IsBusy && SelectedRestorePlanDraftForExport != null && !string.IsNullOrWhiteSpace(RestorePlanDraftExportPreviewText));
            CompareRestorePlanDraftsCommand = new RelayCommand(
                _ => CompareSelectedRestorePlanDrafts(),
                _ => CanCompareSelectedRestorePlanDrafts());
        }

        private void PreviewRestorePlanDraftExport(object? value)
        {
            if (!(value is RestorePlanDraftDefinition requested)) return;
            var draft = RestorePlanDrafts.FirstOrDefault(item =>
                string.Equals(item.DraftId, requested.DraftId, StringComparison.OrdinalIgnoreCase));
            if (draft == null)
            {
                RestorePlanDraftStatus = "该草稿已删除，无法生成导出预览。";
                return;
            }

            SelectedRestorePlanDraftForExport = draft;
            RestorePlanDraftExportPreviewText = RestorePlanDraftExportFormatter.Create(draft);
            RestorePlanDraftStatus = "只读导出预览已生成。请检查下方内容后，再明确保存为 TXT 文件。";
        }

        private void ExportRestorePlanDraft()
        {
            var previewed = SelectedRestorePlanDraftForExport;
            if (previewed == null) return;

            var draft = RestorePlanDrafts.FirstOrDefault(item =>
                string.Equals(item.DraftId, previewed.DraftId, StringComparison.OrdinalIgnoreCase));
            if (draft == null)
            {
                SelectedRestorePlanDraftForExport = null;
                RestorePlanDraftExportPreviewText = string.Empty;
                RestorePlanDraftStatus = "该草稿已删除，导出已取消。";
                return;
            }

            var currentText = RestorePlanDraftExportFormatter.Create(draft);
            if (!string.Equals(currentText, RestorePlanDraftExportPreviewText, StringComparison.Ordinal))
            {
                RestorePlanDraftExportPreviewText = currentText;
                RestorePlanDraftStatus = "草稿状态已变化，预览已更新；请复核后再次选择导出。";
                return;
            }

            var dialog = new SaveFileDialog
            {
                Title = "导出只读恢复方案摘要",
                Filter = "文本摘要 (*.txt)|*.txt",
                FileName = $"GameSaveCenter-restore-plan-{DateTime.Now:yyyyMMdd-HHmmss}.txt",
                AddExtension = true,
                DefaultExt = ".txt",
                OverwritePrompt = true
            };
            if (dialog.ShowDialog() != true) return;

            File.WriteAllText(dialog.FileName, currentText, new UTF8Encoding(false));
            RestorePlanDraftStatus = "只读摘要已导出；文本不包含存档、路径或凭据，也不是可导入或执行的恢复指令。";
        }

        private async Task PreviewRestorePlanMappingAsync()
        {
            var game = SelectedGame ?? throw new InvalidOperationException("请先选择游戏。");
            var backup = SelectedBackup ?? throw new InvalidOperationException("请先选择备份版本。");
            var gameId = game.PlayniteId;
            var backupId = backup.BackupId;
            RestorePlanMappingPreview = null;
            RestorePlanDraftStatus = "正在只读检查版本映射；不会写入当前存档。";
            RestoreMappingPreviewDto preview;
            using (var key = RestorePlanMappingFingerprintKeyStore.GetOrCreate(
                plugin.Settings,
                () => plugin.SavePluginSettings(plugin.Settings)))
            {
                preview = await plugin.RequestAsync<RestoreMappingPreviewDto>(MessageTypes.RestorePreview,
                    new RestoreMappingPreviewRequestDto
                    {
                        PlayniteId = gameId,
                        BackupId = backupId,
                        MappingFingerprintKeyBase64 = key.KeyBase64,
                        MappingFingerprintKeyId = key.KeyId
                    },
                    TimeSpan.FromMinutes(2));
            }

            if (!string.Equals(SelectedGame?.PlayniteId, gameId, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(SelectedBackup?.BackupId, backupId, StringComparison.OrdinalIgnoreCase))
            {
                ResetRestorePlanMappingPreview("选择已变化，旧版本预览已丢弃；请重新检查当前选择。");
                return;
            }

            RestorePlanMappingPreview = preview;
            MarkRestorePlanDraftsForMappingPreview(preview);
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
            RestorePlanDraftDefinition draft;
            using (var key = RestorePlanMappingFingerprintKeyStore.GetOrCreate(
                plugin.Settings,
                () => plugin.SavePluginSettings(plugin.Settings)))
            {
                if (!string.Equals(key.KeyId, mapping.MappingFingerprintKeyId, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("映射签名密钥已变化，请重新预览后再保存草稿。");
                draft = RestorePlanDraftDefinition.Create(
                    game,
                    target,
                    mapping,
                    key.ComputeFileIdentityFingerprint(target.ContentFingerprint),
                    key.KeyId,
                    key.ComputeReadinessFingerprint(target.RestoreReadiness));
            }
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
            {
                var removed = RestorePlanDrafts[RestorePlanDrafts.Count - 1];
                RestorePlanDrafts.RemoveAt(RestorePlanDrafts.Count - 1);
                ClearComparisonSelectionFor(removed);
            }
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
            ClearComparisonSelectionFor(draft);
            RestorePlanDraftStatus = "方案草稿已删除。";
            return Task.CompletedTask;
        }

        private bool CanCompareSelectedRestorePlanDrafts()
            => SelectedRestorePlanDraftFrom != null
                && SelectedRestorePlanDraftTo != null
                && !string.Equals(SelectedRestorePlanDraftFrom.DraftId, SelectedRestorePlanDraftTo.DraftId, StringComparison.OrdinalIgnoreCase);

        private void CompareSelectedRestorePlanDrafts()
        {
            if (!CanCompareSelectedRestorePlanDrafts())
            {
                RestorePlanDraftComparisonResult = null;
                return;
            }

            RestorePlanDraftComparisonResult = RestorePlanDraftComparison.Create(
                SelectedRestorePlanDraftFrom!,
                SelectedRestorePlanDraftTo!);
        }

        private void ClearComparisonSelectionFor(RestorePlanDraftDefinition draft)
        {
            if (string.Equals(SelectedRestorePlanDraftFrom?.DraftId, draft.DraftId, StringComparison.OrdinalIgnoreCase))
                SelectedRestorePlanDraftFrom = null;
            if (string.Equals(SelectedRestorePlanDraftTo?.DraftId, draft.DraftId, StringComparison.OrdinalIgnoreCase))
                SelectedRestorePlanDraftTo = null;
            if (string.Equals(SelectedRestorePlanDraftForExport?.DraftId, draft.DraftId, StringComparison.OrdinalIgnoreCase))
            {
                SelectedRestorePlanDraftForExport = null;
                RestorePlanDraftExportPreviewText = string.Empty;
            }
        }

        private void ResetRestorePlanMappingPreview(string? status = null)
        {
            RestorePlanMappingPreview = null;
            if (!string.IsNullOrWhiteSpace(status)) RestorePlanDraftStatus = status!;
            else RestorePlanDraftStatus = "所选游戏或版本已变化；请重新预览映射后再保存草稿。";
        }

        private void MarkRestorePlanDraftsForMappingPreview(RestoreMappingPreviewDto preview)
        {
            var changed = false;
            foreach (var draft in RestorePlanDrafts)
            {
                if (!string.Equals(draft.PlayniteId, preview.PlayniteId, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(draft.TargetBackupId, preview.BackupId, StringComparison.OrdinalIgnoreCase))
                    continue;

                var reason = RestorePlanDraftRevalidation.CompareMapping(draft, preview);
                if (reason.Length > 0) changed |= draft.MarkNeedsRevalidation(reason);
            }

            PersistRestorePlanInvalidations(changed);
        }

        public void MarkRestorePlanDraftsForSnapshot(
            string playniteId,
            GameStatusDto? game,
            IEnumerable<BackupVersionDto> currentBackups,
            bool historyComplete)
        {
            if (string.IsNullOrWhiteSpace(playniteId) || game == null
                || !string.Equals(playniteId, game.PlayniteId, StringComparison.OrdinalIgnoreCase))
                return;

            var byId = currentBackups?.ToDictionary(item => item.BackupId, StringComparer.OrdinalIgnoreCase)
                ?? new Dictionary<string, BackupVersionDto>(StringComparer.OrdinalIgnoreCase);
            using var key = RestorePlanMappingFingerprintKeyStore.TryLoad(plugin.Settings);
            var changed = false;
            foreach (var draft in RestorePlanDrafts)
            {
                if (!string.Equals(draft.PlayniteId, playniteId, StringComparison.OrdinalIgnoreCase)) continue;

                string reason;
                if (byId.TryGetValue(draft.TargetBackupId, out var currentBackup))
                {
                    var currentFileIdentity = key?.ComputeFileIdentityFingerprint(currentBackup.ContentFingerprint) ?? string.Empty;
                    reason = RestorePlanDraftRevalidation.CompareSnapshot(
                        draft,
                        game,
                        currentBackup,
                        currentFileIdentity,
                        key?.KeyId ?? string.Empty,
                        key?.ComputeReadinessFingerprint(currentBackup.RestoreReadiness) ?? string.Empty);
                }
                else if (historyComplete)
                {
                    reason = "目标版本已不存在，需要重新选择版本并保存草稿。";
                }
                else
                {
                    continue;
                }

                if (reason.Length > 0) changed |= draft.MarkNeedsRevalidation(reason);
            }

            PersistRestorePlanInvalidations(changed);
        }

        private void PersistRestorePlanInvalidations(bool changed)
        {
            if (!changed) return;
            try
            {
                var selectedFromId = SelectedRestorePlanDraftFrom?.DraftId;
                var selectedToId = SelectedRestorePlanDraftTo?.DraftId;
                plugin.Settings.RestorePlanDrafts = RestorePlanDrafts.ToList();
                RestorePlanDrafts.Clear();
                foreach (var draft in plugin.Settings.RestorePlanDrafts)
                    RestorePlanDrafts.Add(draft);
                SelectedRestorePlanDraftFrom = RestorePlanDrafts.FirstOrDefault(draft =>
                    string.Equals(draft.DraftId, selectedFromId, StringComparison.OrdinalIgnoreCase));
                SelectedRestorePlanDraftTo = RestorePlanDrafts.FirstOrDefault(draft =>
                    string.Equals(draft.DraftId, selectedToId, StringComparison.OrdinalIgnoreCase));
                RestorePlanDraftComparisonResult = null;
                plugin.SavePluginSettings(plugin.Settings);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "GameSaveCenter could not persist restore-plan revalidation markers.");
            }
        }
    }
}
