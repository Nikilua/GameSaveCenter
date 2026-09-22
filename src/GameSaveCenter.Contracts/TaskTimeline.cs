using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSaveCenter.Contracts;

/// <summary>One known task event suitable for the compact task detail timeline.</summary>
public sealed class TaskTimelineEntryDto
{
    public long Sequence { get; set; }
    public string Kind { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public DateTime? OccurredUtc { get; set; }
    public bool HasKnownTime => OccurredUtc.HasValue;
    public string RelativeTimeDisplay => OccurredUtc.HasValue
        ? TimeDisplayFormatter.Relative(OccurredUtc.Value, DateTime.UtcNow)
        : "时间未知";
    public string FullTimeDisplay => OccurredUtc.HasValue
        ? TimeDisplayFormatter.Full(OccurredUtc.Value)
        : "时间未知";
    public string RawUtcTimeDisplay => OccurredUtc.HasValue
        ? TimeDisplayFormatter.RawUtc(OccurredUtc.Value)
        : "未记录 UTC 时间";
    public string LocalTimeDisplay => OccurredUtc.HasValue
        ? OccurredUtc.Value.ToLocalTime().ToString("MM-dd HH:mm:ss")
        : "时间未知";
    public string UtcTimeDisplay => OccurredUtc.HasValue
        ? OccurredUtc.Value.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss 'UTC'")
        : "未记录 UTC 时间";
    public string TimeDisplay => OccurredUtc.HasValue
        ? $"{LocalTimeDisplay} · {UtcTimeDisplay}"
        : "时间未知 · 未记录 UTC 时间";
}

/// <summary>
/// Builds a timeline only from durable task timestamps and observed Worker snapshots.
/// Missing intermediate events are kept unknown instead of being inferred.
/// </summary>
public static class TaskTimelineBuilder
{
    private const string Created = "Created";
    private const string Started = "Started";
    private const string Stage = "Stage";
    private const string Cancellation = "Cancellation";
    private const string Finished = "Finished";

    public static IReadOnlyList<TaskTimelineEntryDto> Build(
        TaskStatusDto task,
        IEnumerable<TaskChangeEventDto>? observedChanges = null)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));

        var entries = new List<TaskTimelineEntryDto>();
        var sequence = 0L;
        var changes = (observedChanges ?? Enumerable.Empty<TaskChangeEventDto>())
            .Where(change => change?.Task != null && string.Equals(change.Task.TaskId, task.TaskId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(change => change.Sequence)
            .ToList();

        Add(entries, ref sequence, Created, "已创建", "任务进入队列。", task.CreatedUtc == DateTime.MinValue ? null : task.CreatedUtc);

        var startedObserved = false;
        var lastStage = string.Empty;
        var cancellationState = string.Empty;
        var finishedObserved = false;
        foreach (var change in changes)
        {
            var changeTime = NormalizeUtc(change.OccurredUtc);
            if (!startedObserved && (change.Task.State == TaskState.Running || change.Task.StartedUtc.HasValue))
            {
                startedObserved = true;
                Add(entries, ref sequence, Started, "开始执行", "Worker 已记录任务开始。", NormalizeUtc(change.Task.StartedUtc) ?? changeTime, change.Sequence);
            }

            var stage = change.Task.StageMessage;
            if (!string.IsNullOrWhiteSpace(stage) && !string.Equals(stage, lastStage, StringComparison.Ordinal))
            {
                lastStage = stage;
                Add(entries, ref sequence, Stage, change.Task.StageDisplay, stage, changeTime, change.Sequence);
            }

            var currentCancellation = change.Task.CancellationState;
            if (!string.IsNullOrWhiteSpace(currentCancellation)
                && !string.Equals(currentCancellation, cancellationState, StringComparison.Ordinal))
            {
                cancellationState = currentCancellation;
                if (currentCancellation == TaskCancellationStates.Requested)
                    Add(entries, ref sequence, Cancellation, "取消请求", "已接受取消请求，等待安全边界。", changeTime, change.Sequence);
                else if (currentCancellation == TaskCancellationStates.Finalizing)
                    Add(entries, ref sequence, Cancellation, "安全收尾", "任务无法立即中断，Worker 正在收尾。", changeTime, change.Sequence);
            }

            if (!finishedObserved && IsTerminal(change.Task.State))
            {
                finishedObserved = true;
                Add(entries, ref sequence, Finished, TerminalTitle(change.Task), TerminalDetail(change.Task), NormalizeUtc(change.Task.FinishedUtc) ?? changeTime, change.Sequence);
            }
        }

        if (!startedObserved && task.State != TaskState.Queued)
        {
            startedObserved = true;
            Add(entries, ref sequence, Started, "开始执行", "缺少 Worker 开始事件，未推断中间过程。", NormalizeUtc(task.StartedUtc));
        }

        if (string.IsNullOrWhiteSpace(lastStage) && !string.IsNullOrWhiteSpace(task.StageMessage))
            Add(entries, ref sequence, Stage, task.StageDisplay, task.StageMessage, null);

        if (!finishedObserved && IsTerminal(task.State))
        {
            finishedObserved = true;
            Add(entries, ref sequence, Finished, TerminalTitle(task), TerminalDetail(task), NormalizeUtc(task.FinishedUtc));
        }

        return entries
            .OrderBy(entry => entry.OccurredUtc.HasValue ? 0 : 1)
            .ThenBy(entry => entry.OccurredUtc ?? DateTime.MaxValue)
            .ThenBy(entry => entry.Sequence)
            .ToArray();
    }

    private static void Add(
        ICollection<TaskTimelineEntryDto> entries,
        ref long fallbackSequence,
        string kind,
        string title,
        string detail,
        DateTime? occurredUtc,
        long sequence = 0)
    {
        entries.Add(new TaskTimelineEntryDto
        {
            Sequence = sequence > 0 ? sequence : ++fallbackSequence,
            Kind = kind,
            Title = string.IsNullOrWhiteSpace(title) ? "未知事件" : title,
            Detail = string.IsNullOrWhiteSpace(detail) ? "未知" : detail,
            OccurredUtc = occurredUtc
        });
    }

    private static DateTime? NormalizeUtc(DateTime value)
        => value == DateTime.MinValue ? null : value.ToUniversalTime();

    private static DateTime? NormalizeUtc(DateTime? value)
        => value.HasValue ? NormalizeUtc(value.Value) : null;

    private static bool IsTerminal(TaskState state)
        => state == TaskState.Succeeded || state == TaskState.Failed || state == TaskState.Cancelled;

    private static string TerminalTitle(TaskStatusDto task)
        => task.State switch
        {
            TaskState.Succeeded => "已完成",
            TaskState.Failed => "执行失败",
            TaskState.Cancelled => "已取消",
            _ => "结束状态未知"
        };

    private static string TerminalDetail(TaskStatusDto task)
    {
        if (task.State == TaskState.Failed && !string.IsNullOrWhiteSpace(task.ErrorCode))
            return $"错误码：{task.ErrorCode}";
        if (task.State == TaskState.Cancelled && !string.IsNullOrWhiteSpace(task.CancellationDisplay))
            return task.CancellationDisplay;
        return task.Message;
    }
}
