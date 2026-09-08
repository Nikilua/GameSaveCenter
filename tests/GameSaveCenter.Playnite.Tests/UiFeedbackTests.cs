using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class UiFeedbackTests
{
    [Fact]
    public void NotificationKeepsFullDetailSeparateFromShortSummary()
    {
        var args = new UiNotificationEventArgs(
            "操作失败",
            "存档同步失败：路径过长…",
            UiNotificationKind.Error,
            "存档同步失败\r\n错误码：RCLONE_AUTH_FAILED\r\n任务 ID：task-42");

        Assert.Equal("存档同步失败：路径过长…", args.Message);
        Assert.Contains("RCLONE_AUTH_FAILED", args.DetailMessage);
        Assert.Contains("task-42", args.DetailMessage);
    }

    [Fact]
    public void NotificationUsesSummaryAsDetailWhenNoSeparateDetailIsProvided()
    {
        var args = new UiNotificationEventArgs("完成", "已完成", UiNotificationKind.Success);

        Assert.Equal(args.Message, args.DetailMessage);
    }
}
