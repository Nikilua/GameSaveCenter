using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class WorklistBrowserEntryTests
{
    [Fact]
    public void BrowserSearchMatchesStableIdentityNoteAndCurrentStatus()
    {
        var entry = new WorklistBrowserEntry(new WorklistItemRecord
        {
            Kind = WorklistObjectKind.BackgroundTask,
            StableObjectId = "task-42",
            Note = "复核取消边界"
        }, 0);
        entry.SetState(WorklistReferenceState.Unavailable);
        var dashboard = (DashboardViewModel)FormatterServices.GetUninitializedObject(typeof(DashboardViewModel));
        var searchField = typeof(DashboardViewModel).GetField("worklistSearchText", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var filter = typeof(DashboardViewModel).GetMethod("FilterWorklistBrowserEntry", BindingFlags.Instance | BindingFlags.NonPublic)!;

        foreach (var query in new[] { "task-42", "取消边界", "后台任务", "无法检查" })
        {
            searchField.SetValue(dashboard, query);
            Assert.True((bool)filter.Invoke(dashboard, new object[] { entry })!);
        }

        searchField.SetValue(dashboard, "不存在的编号");
        Assert.False((bool)filter.Invoke(dashboard, new object[] { entry })!);
    }

    [Fact]
    public void EntryProjectsStableReferencePositionAndManualStateWithoutChangingTheRecord()
    {
        var record = new WorklistItemRecord
        {
            Kind = WorklistObjectKind.BackgroundTask,
            StableObjectId = "task-42",
            Note = "检查重试边界",
            IsManuallyComplete = true
        };

        var entry = new WorklistBrowserEntry(record, 4);

        Assert.Same(record, entry.Record);
        Assert.Equal((5).ToString(CultureInfo.CurrentCulture), entry.PositionDisplay);
        Assert.Equal("task-42", entry.StableObjectId);
        Assert.Equal("检查重试边界", entry.Note);
        Assert.Equal("已完成（手动）", entry.ManualStateDisplay);
        Assert.True(record.IsManuallyComplete);
    }

    [Fact]
    public void GroupProjectionTracksCurrentReferenceStateAndKeepsUnknownSeparateFromMissing()
    {
        var entry = new WorklistBrowserEntry(new WorklistItemRecord
        {
            Kind = WorklistObjectKind.ValidationFinding,
            StableObjectId = "finding-7"
        }, 0);

        entry.SetGroupMode("按对象类型");
        Assert.Equal("校验问题", entry.GroupKey);
        entry.SetState(WorklistReferenceState.Unavailable);
        Assert.Equal("校验问题", entry.GroupKey);
        Assert.Contains("无法检查", entry.StateDisplay);

        entry.SetGroupMode("按引用状态");
        Assert.Equal(entry.StateDisplay, entry.GroupKey);
        var unknownGroup = entry.GroupKey;

        entry.SetState(WorklistReferenceState.NotFound);

        Assert.NotEqual(unknownGroup, entry.GroupKey);
        Assert.Contains("未找到", entry.StateDisplay);
        Assert.Equal(WorklistReferenceState.NotFound, entry.State);
        Assert.Contains("源文件没有被删除", entry.StateHelpText);
    }
}
