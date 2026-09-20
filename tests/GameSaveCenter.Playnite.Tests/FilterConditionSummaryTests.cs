using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class FilterConditionSummaryTests
{
    [Fact]
    public void GamePickerClearCommandClearsTheDisplayedFilterScope()
    {
        using var picker = new GamePickerViewModel();
        picker.ApplyPersistedState(string.Empty, "全部", "全部", "名称");
        picker.SearchText = "missing";
        picker.StatusFilter = "未匹配";
        picker.PlatformFilter = "Steam";

        Assert.Equal("当前筛选：搜索“missing” · 状态：未匹配 · 平台：Steam", picker.ActiveFiltersSummary);

        picker.ClearFiltersCommand.Execute(null);

        Assert.False(picker.HasActiveFilters);
        Assert.Equal("当前未设置筛选条件", picker.ActiveFiltersSummary);
    }

    [Fact]
    public void GamePickerSummaryNamesOnlyActiveConditions()
    {
        Assert.Equal("当前筛选：搜索“missing” · 状态：未匹配 · 平台：Steam", FilterConditionSummary.GamePicker("missing", "未匹配", "Steam"));
        Assert.Equal("当前未设置筛选条件", FilterConditionSummary.GamePicker(string.Empty, "全部", "全部"));
    }

    [Fact]
    public void MediaSummaryKeepsSearchAndKindScopeVisible()
    {
        Assert.Equal("当前筛选：搜索“clip” · 类型：录像", FilterConditionSummary.Media("clip", "录像"));
        Assert.Equal("当前未设置筛选条件", FilterConditionSummary.Media(string.Empty, "全部"));
    }

    [Fact]
    public void CloudSummaryExplainsCodedFiltersUsingUserFacingLabels()
    {
        var summary = FilterConditionSummary.Cloud("Failed", "Media", "A", "device-1", "24h");

        Assert.Equal("当前筛选：状态：上传失败 · 类型：媒体 · 游戏“A” · 来源设备“device-1” · 时间：最近 24 小时", summary);
    }

    [Fact]
    public void CloudLoadFailureNeverUsesTheZeroResultMessage()
    {
        var filtered = FilterConditionSummary.Cloud("Failed", "", "", "", "");

        Assert.Contains("不是零结果", FilterConditionSummary.CloudEmptyState(true, true, filtered));
        Assert.Contains("当前筛选：状态：上传失败", FilterConditionSummary.CloudEmptyState(false, true, filtered));
        Assert.DoesNotContain("不是零结果", FilterConditionSummary.CloudEmptyState(false, true, filtered));
    }

    [Fact]
    public void StaleDetailKeepsBothLastSuccessAndRefreshFailureReason()
    {
        var detail = FilterConditionSummary.StaleStateDetail(
            new System.DateTime(2026, 9, 20, 8, 30, 0, System.DateTimeKind.Utc),
            "Worker 当前离线");

        Assert.Contains("上次成功读取：", detail);
        Assert.Contains("本次刷新失败：Worker 当前离线", detail);
    }
}
