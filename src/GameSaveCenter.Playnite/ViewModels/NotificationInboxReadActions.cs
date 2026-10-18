using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Services;

namespace GameSaveCenter.Playnite.ViewModels;

internal sealed class NotificationInboxReadActions
{
    private readonly Func<string, object, Task<int>> writeRequest;
    private readonly Func<string, string, string, string, bool, Task<bool>> confirm;

    public NotificationInboxReadActions(
        Func<string, object, Task<int>> writeRequest,
        Func<string, string, string, string, bool, Task<bool>> confirm)
    {
        this.writeRequest = writeRequest ?? throw new ArgumentNullException(nameof(writeRequest));
        this.confirm = confirm ?? throw new ArgumentNullException(nameof(confirm));
    }

    public Task<int> MarkSelectedAsReadAsync(NotificationInboxItem? selected)
    {
        if (selected == null || selected.IsRead)
            return Task.FromResult(0);

        return MarkIdsAsReadAsync(new[] { selected.NotificationId });
    }

    public Task<int> MarkVisibleAsReadAsync(IEnumerable<NotificationInboxItem> visibleItems)
    {
        if (visibleItems == null) throw new ArgumentNullException(nameof(visibleItems));

        var ids = NotificationRecordPolicy.NormalizeNotificationIds(
            visibleItems.Where(item => item != null && !item.IsRead).Select(item => item.NotificationId));
        return MarkIdsAsReadAsync(ids);
    }

    public async Task<int?> ClearReadRecordsAsync(int readRecordCount)
    {
        if (readRecordCount <= 0) return null;

        var confirmed = await confirm(
            "清除已读通知",
            $"将永久删除 {readRecordCount} 条已读通知记录。此操作不会解决或关闭关联任务，也不会删除任务、审计日志或恢复证据。",
            "清除已读",
            "取消",
            true).ConfigureAwait(false);
        if (!confirmed) return null;

        return await writeRequest(
            MessageTypes.ClearReadNotificationRecords,
            new NotificationRecordsClearReadDto()).ConfigureAwait(false);
    }

    private Task<int> MarkIdsAsReadAsync(IEnumerable<string> notificationIds)
    {
        var ids = NotificationRecordPolicy.NormalizeNotificationIds(notificationIds);
        if (ids.Count == 0) return Task.FromResult(0);

        return writeRequest(
            MessageTypes.MarkNotificationRecordsRead,
            new NotificationRecordIdsDto { NotificationIds = ids.ToList() });
    }
}
