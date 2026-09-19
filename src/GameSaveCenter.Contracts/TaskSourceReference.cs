using System;

namespace GameSaveCenter.Contracts;

/// <summary>Durable kind of object that a task can safely navigate back to.</summary>
public enum TaskSourceReferenceKind
{
    Game,
    BackupVersion,
    MediaBatch,
    CloudTransfer
}

/// <summary>
/// A credential-free, stable task source identity. Display names are diagnostic only;
/// navigation must use StableId and PlayniteId when the target type needs them.
/// </summary>
public sealed class TaskSourceReferenceDto
{
    public TaskSourceReferenceKind Kind { get; set; }
    public string StableId { get; set; } = string.Empty;
    public string PlayniteId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;

    public bool HasStableIdentity => !string.IsNullOrWhiteSpace(StableId);
    public string KindDisplay => Kind switch
    {
        TaskSourceReferenceKind.Game => "游戏",
        TaskSourceReferenceKind.BackupVersion => "存档版本",
        TaskSourceReferenceKind.MediaBatch => "媒体批次",
        TaskSourceReferenceKind.CloudTransfer => "云队列",
        _ => "任务来源"
    };
    public string IdentityDisplay => string.IsNullOrWhiteSpace(DisplayName)
        ? StableId
        : string.IsNullOrWhiteSpace(StableId) ? DisplayName : $"{DisplayName} · {StableId}";

    public TaskSourceReferenceDto Clone() => new TaskSourceReferenceDto
    {
        Kind = Kind,
        StableId = StableId ?? string.Empty,
        PlayniteId = PlayniteId ?? string.Empty,
        DisplayName = DisplayName ?? string.Empty,
        Detail = Detail ?? string.Empty
    };
}
