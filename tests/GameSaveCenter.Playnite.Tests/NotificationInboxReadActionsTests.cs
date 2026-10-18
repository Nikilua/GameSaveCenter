using System.Collections.Generic;
using System.Threading.Tasks;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class NotificationInboxReadActionsTests
{
    [Fact]
    public async Task MarkSelectedSendsOnlyTheUnreadSelectedRecord()
    {
        var requests = new List<(string Type, object Payload)>();
        var actions = CreateActions(requests, (_, _, _, _, _) => Task.FromResult(false));

        var changed = await actions.MarkSelectedAsReadAsync(CreateItem("selected", isRead: false));

        Assert.Equal(1, changed);
        var request = Assert.Single(requests);
        Assert.Equal(MessageTypes.MarkNotificationRecordsRead, request.Type);
        Assert.Equal(new[] { "selected" }, Assert.IsType<NotificationRecordIdsDto>(request.Payload).NotificationIds);
    }

    [Fact]
    public async Task MarkVisibleUsesOnlyUnreadItemsInTheProvidedFilteredSet()
    {
        var requests = new List<(string Type, object Payload)>();
        var actions = CreateActions(requests, (_, _, _, _, _) => Task.FromResult(false));
        var visible = new[]
        {
            CreateItem("visible-unread-1", isRead: false),
            CreateItem("already-read", isRead: true),
            CreateItem(" visible-unread-2 ", isRead: false),
            CreateItem("visible-unread-1", isRead: false)
        };

        var changed = await actions.MarkVisibleAsReadAsync(visible);

        Assert.Equal(2, changed);
        var request = Assert.Single(requests);
        Assert.Equal(MessageTypes.MarkNotificationRecordsRead, request.Type);
        Assert.Equal(
            new[] { "visible-unread-1", "visible-unread-2" },
            Assert.IsType<NotificationRecordIdsDto>(request.Payload).NotificationIds);
    }

    [Fact]
    public async Task ClearReadRequiresDangerousConfirmationAndLeavesRelatedStateMessageExplicit()
    {
        var requests = new List<(string Type, object Payload)>();
        string? confirmationMessage = null;
        var dangerous = false;
        var actions = CreateActions(requests, (_, message, _, _, isDangerous) =>
        {
            confirmationMessage = message;
            dangerous = isDangerous;
            return Task.FromResult(false);
        });

        var result = await actions.ClearReadRecordsAsync(3);

        Assert.Null(result);
        Assert.Empty(requests);
        Assert.True(dangerous);
        Assert.Contains("3 条已读通知", confirmationMessage);
        Assert.Contains("任务、审计日志或恢复证据", confirmationMessage);
    }

    [Fact]
    public async Task ConfirmedClearCallsOnlyTheReadNotificationEndpoint()
    {
        var requests = new List<(string Type, object Payload)>();
        var actions = CreateActions(requests, (_, _, _, _, _) => Task.FromResult(true));

        var deleted = await actions.ClearReadRecordsAsync(2);

        Assert.Equal(2, deleted);
        var request = Assert.Single(requests);
        Assert.Equal(MessageTypes.ClearReadNotificationRecords, request.Type);
        Assert.IsType<NotificationRecordsClearReadDto>(request.Payload);
    }

    private static NotificationInboxReadActions CreateActions(
        List<(string Type, object Payload)> requests,
        System.Func<string, string, string, string, bool, Task<bool>> confirm)
        => new((type, payload) =>
        {
            requests.Add((type, payload));
            var count = type == MessageTypes.MarkNotificationRecordsRead
                ? Assert.IsType<NotificationRecordIdsDto>(payload).NotificationIds.Count
                : 2;
            return Task.FromResult(count);
        }, confirm);

    private static NotificationInboxItem CreateItem(string id, bool isRead)
        => new(new NotificationRecordDto
        {
            NotificationId = id,
            DeduplicationKey = "event:" + id.Trim(),
            Title = "通知",
            Summary = "摘要",
            IsRead = isRead
        });
}
