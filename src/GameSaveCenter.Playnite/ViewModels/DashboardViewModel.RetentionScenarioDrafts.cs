using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Settings;

namespace GameSaveCenter.Playnite.ViewModels;

public sealed partial class DashboardViewModel
{
    private RetentionScenarioDefinition? selectedRetentionScenarioDraft;
    private string retentionScenarioNameDraft = string.Empty;
    private string retentionScenarioDraftStatusDisplay = "当前参数仅在本次会话中编辑；保存命名方案后可在后续会话继续使用。";
    private bool isLoadingRetentionScenarioDraft;

    internal Func<string, string, string, string, bool, Task<bool>>? RetentionScenarioConfirmationOverrideForAudit { get; set; }

    public ObservableCollection<RetentionScenarioDefinition> RetentionScenarioDrafts { get; } = new ObservableCollection<RetentionScenarioDefinition>();

    public RetentionScenarioDefinition? SelectedRetentionScenarioDraft
    {
        get => selectedRetentionScenarioDraft;
        set
        {
            if (ReferenceEquals(selectedRetentionScenarioDraft, value)) return;
            SetValue(ref selectedRetentionScenarioDraft, value);
            RetentionScenarioNameDraft = value?.Name ?? string.Empty;
            if (value != null)
            {
                isLoadingRetentionScenarioDraft = true;
                try
                {
                    RetentionScenarioInput.KeepAllForHours = value.KeepAllForHours;
                    RetentionScenarioInput.KeepDailyDays = value.KeepDailyDays;
                    RetentionScenarioInput.KeepWeeklyWeeks = value.KeepWeeklyWeeks;
                    RetentionScenarioInput.KeepMonthlyMonths = value.KeepMonthlyMonths;
                    RetentionScenarioInput.GrowthMinimumMiBPer30Days = value.GrowthMinimumMiBPer30Days;
                    RetentionScenarioInput.GrowthMaximumMiBPer30Days = value.GrowthMaximumMiBPer30Days;
                }
                finally
                {
                    isLoadingRetentionScenarioDraft = false;
                }

                RetentionScenarioComparison = new Contracts.RetentionScenarioComparisonDto
                {
                    Summary = BuildRetentionScenarioSnapshotStatus(value, "方案已载入；重新计算会读取当前数据。")
                };
                SelectedRetentionScenarioGame = null;
            }
            else
            {
                RetentionScenarioComparison = new Contracts.RetentionScenarioComparisonDto
                {
                    Summary = "已取消所选保存情景；当前参数保留为未保存输入，请重新比较。"
                };
                SelectedRetentionScenarioGame = null;
            }

            RefreshRetentionScenarioDraftStatusDisplay();
            RaiseCommandStates();
        }
    }

    public string RetentionScenarioNameDraft
    {
        get => retentionScenarioNameDraft;
        set
        {
            var next = value ?? string.Empty;
            if (string.Equals(retentionScenarioNameDraft, next, StringComparison.Ordinal)) return;
            SetValue(ref retentionScenarioNameDraft, next);
            RaiseCommandStates();
        }
    }

    public string RetentionScenarioDraftStatusDisplay
    {
        get => retentionScenarioDraftStatusDisplay;
        private set => SetValue(ref retentionScenarioDraftStatusDisplay, value ?? string.Empty);
    }

    public ICommand SaveRetentionScenarioDraftCommand { get; }
    public ICommand CopyRetentionScenarioDraftCommand { get; }
    public ICommand RenameRetentionScenarioDraftCommand { get; }
    public ICommand DeleteRetentionScenarioDraftCommand { get; }
    public ICommand HandoffRetentionScenarioToPolicyTemplateCommand { get; }

    internal void InitializeRetentionScenarioDrafts()
    {
        RetentionScenarioDrafts.Clear();
        foreach (var item in plugin.Settings.RetentionScenarioDrafts ?? new System.Collections.Generic.List<RetentionScenarioDefinition>())
        {
            var normalized = RetentionScenarioDefinition.Normalize(item);
            if (normalized != null && !RetentionScenarioDrafts.Any(existing =>
                    string.Equals(existing.Id, normalized.Id, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(existing.Name, normalized.Name, StringComparison.OrdinalIgnoreCase)))
                RetentionScenarioDrafts.Add(normalized);
        }

        RefreshRetentionScenarioDraftStatusDisplay();
    }

    private bool CanSaveRetentionScenarioDraft
        => !IsBusy && RetentionScenarioInput.IsValid
            && !string.IsNullOrWhiteSpace(RetentionScenarioNameDraft)
            && (SelectedRetentionScenarioDraft != null
                || RetentionScenarioDrafts.Count < RetentionScenarioDefinition.MaximumSavedDrafts);

    private bool CanCopyRetentionScenarioDraft
        => !IsBusy && SelectedRetentionScenarioDraft != null
            && RetentionScenarioDrafts.Count < RetentionScenarioDefinition.MaximumSavedDrafts;

    private bool CanRenameRetentionScenarioDraft
        => !IsBusy && SelectedRetentionScenarioDraft != null
            && !string.IsNullOrWhiteSpace(RetentionScenarioNameDraft);

    private bool CanDeleteRetentionScenarioDraft => !IsBusy && SelectedRetentionScenarioDraft != null;

    private bool CanHandoffRetentionScenarioToPolicyTemplate
        => !IsBusy && SelectedRetentionScenarioDraft != null;

    private void HandoffRetentionScenarioToPolicyTemplate()
    {
        var scenario = SelectedRetentionScenarioDraft;
        if (scenario == null) return;

        if (!RetentionScenarioDraftMatches(scenario))
        {
            StatusMessage = "当前情景参数有未保存修改；请先保存方案，或重新选择已保存方案后再转交。";
            return;
        }

        if (!RetentionScenarioInput.IsValid)
        {
            StatusMessage = "请先修正情景参数和增长范围，再转交策略模板。";
            return;
        }

        if (HasUnsavedPolicyTemplateDraft())
        {
            StatusMessage = "策略模板编辑器中有未保存草稿；请先保存或放弃该草稿，再转交保留情景。";
            return;
        }

        if (HasSelectedGamePolicyChanges)
        {
            StatusMessage = "当前游戏策略有未保存修改；请先保存或取消修改，再转交保留情景。";
            return;
        }

        CreatePolicyTemplate();
        var draftName = $"情景副本 - {scenario.Name}";
        PolicyTemplateDraft.Name = draftName;
        var mappedPolicy = RetentionScenarioPolicyHandoff.CreatePolicyDraft(
            scenario,
            PolicyTemplateDraft.Policy);
        PolicyTemplateDraft.Policy.KeepRecentAllHours = mappedPolicy.KeepRecentAllHours;
        PolicyTemplateDraft.Policy.KeepDailyDays = mappedPolicy.KeepDailyDays;
        PolicyTemplateDraft.Policy.KeepWeeklyWeeks = mappedPolicy.KeepWeeklyWeeks;
        PolicyTemplateDraft.Policy.KeepMonthlyMonths = mappedPolicy.KeepMonthlyMonths;
        PolicyTemplateNameDraft = draftName;

        RetentionScenarioComparison = new Contracts.RetentionScenarioComparisonDto
        {
            Summary = "情景已转交为未保存策略模板草稿；此前比较结果已作废。请在策略页复核字段差异并重新比较。模板应用不会清理归档；清理需另行生成当前 Worker 预览。"
        };
        SelectedRetentionScenarioGame = null;

        SaveTabIndex = 2;
        CurrentWorkspace = WorkspaceKind.Saves;
        StatusMessage = $"已将保留情景“{scenario.Name}”转交到策略模板草稿。仅映射四项保留期限；增长范围仍是比较假设。草稿尚未保存或应用，请复核字段差异；此操作不会清理归档。";
        RequestWorkspaceLoad();
    }

    private bool HasUnsavedPolicyTemplateDraft()
    {
        if (SelectedPolicyTemplate == null)
            return !string.IsNullOrWhiteSpace(PolicyTemplateNameDraft)
                || !string.IsNullOrWhiteSpace(PolicyTemplateDraft?.Name);

        return !string.Equals(PolicyTemplateNameDraft, SelectedPolicyTemplate.Name, StringComparison.Ordinal)
            || GameSaveCenter.Core.Services.BackupPolicyDiff.Compare(
                SelectedPolicyTemplate.Policy,
                PolicyTemplateDraft.Policy).Count > 0;
    }

    private async Task SaveRetentionScenarioDraftAsync()
    {
        await Task.CompletedTask.ConfigureAwait(true);
        try
        {
            if (!RetentionScenarioInput.IsValid)
            {
                StatusMessage = "请先修正情景参数和增长范围，再保存方案。";
                return;
            }

            var name = RetentionScenarioDefinition.NormalizeName(RetentionScenarioNameDraft);
            if (name.Length == 0)
            {
                StatusMessage = "请输入保存方案名称。";
                return;
            }

            if (SelectedRetentionScenarioDraft != null
                && !string.Equals(SelectedRetentionScenarioDraft.Name, name, StringComparison.Ordinal))
            {
                StatusMessage = "方案名称有变化；请使用“重命名”保存名称，或使用“保存方案”保留已选方案名称。";
                return;
            }

            if (HasRetentionScenarioName(name, SelectedRetentionScenarioDraft?.Id))
            {
                StatusMessage = $"已存在同名保留情景“{name}”，请先选择该方案或换一个名称。";
                return;
            }

            var existing = SelectedRetentionScenarioDraft;
            var inputUnchanged = existing != null && RetentionScenarioDraftMatches(existing);
            var saved = CreateRetentionScenarioDefinition(
                name,
                existing?.Id,
                existing?.CreatedUtc,
                inputUnchanged ? existing?.LastSnapshotUtc : null);

            if (existing == null)
                RetentionScenarioDrafts.Add(saved);
            else
            {
                var index = RetentionScenarioDrafts.IndexOf(existing);
                if (index < 0)
                {
                    StatusMessage = "所选保留情景已变化，请重新选择后保存。";
                    return;
                }
                RetentionScenarioDrafts[index] = saved;
            }

            SelectedRetentionScenarioDraft = saved;
            PersistRetentionScenarioDrafts();
            StatusMessage = $"已保存保留情景“{saved.Name}”。它只保存假设，不更改当前生效策略。";
        }
        catch (Exception error)
        {
            ReportDashboardFailure(error, true);
        }
    }

    private async Task CopyRetentionScenarioDraftAsync()
    {
        await Task.CompletedTask.ConfigureAwait(true);
        try
        {
            var selected = SelectedRetentionScenarioDraft;
            if (selected == null) return;
            if (RetentionScenarioDrafts.Count >= RetentionScenarioDefinition.MaximumSavedDrafts)
            {
                StatusMessage = $"最多保存 {RetentionScenarioDefinition.MaximumSavedDrafts} 个保留情景，请先删除一个方案。";
                return;
            }

            var name = RetentionScenarioDefinition.NormalizeName(RetentionScenarioNameDraft);
            if (string.IsNullOrWhiteSpace(name) || string.Equals(name, selected.Name, StringComparison.OrdinalIgnoreCase))
                name = RetentionScenarioDefinition.NormalizeName($"{selected.Name} 副本");
            if (HasRetentionScenarioName(name, null))
            {
                StatusMessage = $"已存在同名保留情景“{name}”，请修改名称后再复制。";
                return;
            }

            var copy = selected.CopyWithName(name);
            RetentionScenarioDrafts.Add(copy);
            SelectedRetentionScenarioDraft = copy;
            PersistRetentionScenarioDrafts();
            StatusMessage = $"已复制保留情景为“{copy.Name}”。";
        }
        catch (Exception error)
        {
            ReportDashboardFailure(error, true);
        }
    }

    private async Task RenameRetentionScenarioDraftAsync()
    {
        try
        {
            var selected = SelectedRetentionScenarioDraft;
            if (selected == null) return;
            var name = RetentionScenarioDefinition.NormalizeName(RetentionScenarioNameDraft);
            if (name.Length == 0)
            {
                StatusMessage = "请输入保留情景名称。";
                return;
            }
            if (string.Equals(name, selected.Name, StringComparison.Ordinal)) return;
            if (HasRetentionScenarioName(name, selected.Id))
            {
                StatusMessage = $"已有同名保留情景“{name}”，请使用其他名称。";
                return;
            }
            if (!await ConfirmRetentionScenarioMutationAsync(
                    "重命名保留情景",
                    $"是否将“{selected.Name}”重命名为“{name}”？此操作只修改保存的情景草稿。",
                    "重命名",
                    "取消").ConfigureAwait(true))
                return;

            var renamed = selected.Rename(name);
            ReplaceSelectedRetentionScenarioDraft(selected, renamed);
            PersistRetentionScenarioDrafts();
            StatusMessage = $"已将保留情景重命名为“{name}”。";
        }
        catch (Exception error)
        {
            ReportDashboardFailure(error, true);
        }
    }

    private async Task DeleteRetentionScenarioDraftAsync()
    {
        try
        {
            var selected = SelectedRetentionScenarioDraft;
            if (selected == null) return;
            if (!await ConfirmRetentionScenarioMutationAsync(
                    "删除保留情景",
                    $"是否删除保留情景“{selected.Name}”？只删除保存的假设，不会修改当前策略或备份。",
                    "删除",
                    "取消",
                    true).ConfigureAwait(true))
                return;

            RetentionScenarioDrafts.Remove(selected);
            SelectedRetentionScenarioDraft = null;
            PersistRetentionScenarioDrafts();
            StatusMessage = $"已删除保留情景“{selected.Name}”。";
        }
        catch (Exception error)
        {
            ReportDashboardFailure(error, true);
        }
    }

    private Task<bool> ConfirmRetentionScenarioMutationAsync(string title, string message, string confirmText, string cancelText, bool isDangerous = false)
    {
        var confirmationOverride = RetentionScenarioConfirmationOverrideForAudit;
        return confirmationOverride != null
            ? confirmationOverride(title, message, confirmText, cancelText, isDangerous)
            : plugin.ConfirmAsync(title, message, confirmText, cancelText, isDangerous);
    }

    private void ReplaceSelectedRetentionScenarioDraft(RetentionScenarioDefinition oldValue, RetentionScenarioDefinition newValue)
    {
        var index = RetentionScenarioDrafts.IndexOf(oldValue);
        if (index >= 0) RetentionScenarioDrafts[index] = newValue;
        SelectedRetentionScenarioDraft = newValue;
    }

    private RetentionScenarioDefinition CreateRetentionScenarioDefinition(
        string name,
        string? id,
        DateTime? createdUtc,
        DateTime? lastSnapshotUtc)
    {
        var now = DateTime.UtcNow;
        return RetentionScenarioDefinition.Normalize(new RetentionScenarioDefinition
        {
            Id = id ?? Guid.NewGuid().ToString("N"),
            Name = name,
            CreatedUtc = createdUtc ?? now,
            UpdatedUtc = now,
            LastSnapshotUtc = lastSnapshotUtc,
            KeepAllForHours = RetentionScenarioInput.KeepAllForHours,
            KeepDailyDays = RetentionScenarioInput.KeepDailyDays,
            KeepWeeklyWeeks = RetentionScenarioInput.KeepWeeklyWeeks,
            KeepMonthlyMonths = RetentionScenarioInput.KeepMonthlyMonths,
            GrowthMinimumMiBPer30Days = RetentionScenarioInput.GrowthMinimumMiBPer30Days,
            GrowthMaximumMiBPer30Days = RetentionScenarioInput.GrowthMaximumMiBPer30Days
        }) ?? throw new InvalidOperationException("保留情景参数无效，无法保存。");
    }

    private bool HasRetentionScenarioName(string name, string? exceptId)
        => RetentionScenarioDrafts.Any(item =>
            !string.Equals(item.Id, exceptId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));

    private bool RetentionScenarioDraftMatches(RetentionScenarioDefinition definition)
        => RetentionScenarioInput.KeepAllForHours == definition.KeepAllForHours
            && RetentionScenarioInput.KeepDailyDays == definition.KeepDailyDays
            && RetentionScenarioInput.KeepWeeklyWeeks == definition.KeepWeeklyWeeks
            && RetentionScenarioInput.KeepMonthlyMonths == definition.KeepMonthlyMonths
            && RetentionScenarioInput.GrowthMinimumMiBPer30Days == definition.GrowthMinimumMiBPer30Days
            && RetentionScenarioInput.GrowthMaximumMiBPer30Days == definition.GrowthMaximumMiBPer30Days;

    private void PersistRetentionScenarioDrafts()
    {
        plugin.Settings.RetentionScenarioDrafts = RetentionScenarioDrafts.Select(item => item.Clone()).ToList();
        plugin.SavePluginSettings(plugin.Settings);
        RefreshRetentionScenarioDraftStatusDisplay();
        RaiseCommandStates();
    }

    private void RefreshRetentionScenarioDraftStatusDisplay()
    {
        var selected = SelectedRetentionScenarioDraft;
        if (selected == null)
        {
            RetentionScenarioDraftStatusDisplay = "当前为未保存的会话输入；保存命名方案后可跨重启继续使用。保存的假设与当前生效策略分离。";
            return;
        }

        var edits = RetentionScenarioDraftMatches(selected) ? string.Empty : " · 当前参数有未保存修改";
        RetentionScenarioDraftStatusDisplay = BuildRetentionScenarioSnapshotStatus(selected, "结果不会缓存；再次比较会读取当前版本快照。") + edits;
    }

    private static string BuildRetentionScenarioSnapshotStatus(RetentionScenarioDefinition definition, string suffix)
        => definition.LastSnapshotUtc.HasValue
            ? $"已载入“{definition.Name}” · 最近快照 {definition.LastSnapshotUtc.Value.ToLocalTime():yyyy-MM-dd HH:mm:ss} · {suffix}"
            : $"已载入“{definition.Name}” · 尚无比较快照 · {suffix}";

    private void RecordRetentionScenarioSnapshot(RetentionScenarioComparisonRequestDto request, RetentionScenarioComparisonDto result)
    {
        var selected = SelectedRetentionScenarioDraft;
        if (selected == null || !RetentionScenarioInputMatches(request) || !RetentionScenarioDraftMatches(selected)) return;

        var previousSnapshot = selected.LastSnapshotUtc;
        selected.LastSnapshotUtc = result.GeneratedUtc.Kind == DateTimeKind.Utc
            ? result.GeneratedUtc
            : result.GeneratedUtc.ToUniversalTime();
        try
        {
            PersistRetentionScenarioDrafts();
        }
        catch (Exception error)
        {
            selected.LastSnapshotUtc = previousSnapshot;
            RetentionScenarioDraftStatusDisplay = "比较已完成，但快照时间未能写入设置；请检查插件设置保存状态。";
            ReportDashboardFailure(error, true);
        }
    }
}
