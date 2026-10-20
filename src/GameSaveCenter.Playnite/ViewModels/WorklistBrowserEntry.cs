using System;
using System.ComponentModel;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Settings;

namespace GameSaveCenter.Playnite.ViewModels
{
    public sealed class WorklistBrowserEntry : INotifyPropertyChanged
    {
        private WorklistReferenceState state = WorklistReferenceState.NotChecked;
        private string groupMode = string.Empty;
        private string targetPlayniteId = string.Empty;
        private string currentSummary = string.Empty;
        private string parentSummary = string.Empty;

        public WorklistBrowserEntry(WorklistItemRecord record, int position)
        {
            Record = record ?? throw new ArgumentNullException(nameof(record));
            Position = position;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public WorklistItemRecord Record { get; }
        public int Position { get; }
        public string PositionDisplay => (Position + 1).ToString(System.Globalization.CultureInfo.CurrentCulture);
        public WorklistObjectKind Kind => Record.Kind;
        public string KindDisplay => Kind switch
        {
            WorklistObjectKind.BackupVersion => "存档版本",
            WorklistObjectKind.MediaItem => "媒体项",
            WorklistObjectKind.BackgroundTask => "后台任务",
            WorklistObjectKind.ValidationFinding => "校验问题",
            _ => "未知对象"
        };
        public string StableObjectId => Record.StableObjectId;
        public string Note => Record.Note;
        public bool IsManuallyComplete => Record.IsManuallyComplete;
        public string ManualStateDisplay => IsManuallyComplete ? "已完成（手动）" : "未完成";
        public WorklistReferenceState State => state;
        public string TargetPlayniteId => targetPlayniteId;
        public string CurrentSummary => currentSummary;
        public string ParentSummary => parentSummary;
        public string CurrentSummaryDisplay
        {
            get
            {
                var summary = currentSummary.Length > 0
                    ? currentSummary
                    : State switch
                    {
                        WorklistReferenceState.NotFound => "当前索引未找到对象",
                        WorklistReferenceState.InvalidReference => "引用无效，无法读取摘要",
                        WorklistReferenceState.Unavailable => "当前状态未知",
                        WorklistReferenceState.NotChecked => "刷新后显示当前摘要",
                        _ => "索引未提供对象摘要"
                    };
                return parentSummary.Length == 0 ? summary : $"{summary} · 所属：{parentSummary}";
            }
        }
        public string CurrentSummaryHelpText => currentSummary.Length > 0
            ? $"{CurrentSummaryDisplay}\n摘要来自 Worker 当前索引，不代表源文件存在或可读取。"
            : State switch
            {
                WorklistReferenceState.NotFound => "当前索引未找到此对象；引用仍保留在清单中。",
                WorklistReferenceState.InvalidReference => "引用格式无效，无法按稳定编号读取摘要。",
                WorklistReferenceState.Unavailable => "Worker 或索引暂不可用；当前不能判断对象状态。",
                WorklistReferenceState.NotChecked => "刷新状态后显示当前摘要。",
                _ => "Worker 当前索引没有提供摘要。"
            };
        public bool CanContinue => State == WorklistReferenceState.Indexed || State == WorklistReferenceState.Resolved;
        public string StateDisplay => State switch
        {
            WorklistReferenceState.Indexed => "有效 · 索引存在",
            WorklistReferenceState.Resolved => "已解决 · 记录保留",
            WorklistReferenceState.NotFound => "失效 · 未找到记录",
            WorklistReferenceState.InvalidReference => "无效引用",
            WorklistReferenceState.Unavailable => "无法检查",
            _ => "待刷新"
        };
        public string StateHelpText => State switch
        {
            WorklistReferenceState.Indexed => "Worker 在对应索引中找到该 ID；这不代表磁盘文件存在或可恢复。",
            WorklistReferenceState.Resolved => "校验问题记录已标记解决，但仍可从清单查看其稳定编号。",
            WorklistReferenceState.NotFound => "Worker 已按稳定 ID 查询当前索引且未找到记录；源文件没有被删除。",
            WorklistReferenceState.InvalidReference => "引用格式无效，无法定位源对象。",
            WorklistReferenceState.Unavailable => "Worker 查询失败或不可用；当前不能判定该引用是否有效。",
            _ => "尚未刷新该引用；不能据此判断源对象已失效。"
        };
        public string GroupKey => groupMode switch
        {
            "按对象类型" => KindDisplay,
            "按引用状态" => StateDisplay,
            _ => string.Empty
        };

        public void SetState(WorklistReferenceState value)
            => SetResolution(value, targetPlayniteId, null, null);

        public void SetResolution(WorklistReferenceState value, string? playniteId, string? summary = null, string? ownerSummary = null)
        {
            var normalizedState = Enum.IsDefined(typeof(WorklistReferenceState), value)
                ? value
                : WorklistReferenceState.Unavailable;
            var normalizedPlayniteId = (playniteId ?? string.Empty).Trim();
            var normalizedSummary = NormalizeSummary(summary);
            var normalizedParentSummary = NormalizeSummary(ownerSummary);
            if (state != normalizedState)
            {
                state = normalizedState;
                OnPropertyChanged(nameof(State));
                OnPropertyChanged(nameof(StateDisplay));
                OnPropertyChanged(nameof(StateHelpText));
                OnPropertyChanged(nameof(GroupKey));
                OnPropertyChanged(nameof(CanContinue));
                OnPropertyChanged(nameof(CurrentSummaryDisplay));
                OnPropertyChanged(nameof(CurrentSummaryHelpText));
            }
            if (!string.Equals(targetPlayniteId, normalizedPlayniteId, StringComparison.OrdinalIgnoreCase))
            {
                targetPlayniteId = normalizedPlayniteId;
                OnPropertyChanged(nameof(TargetPlayniteId));
            }
            if (!string.Equals(currentSummary, normalizedSummary, StringComparison.Ordinal))
            {
                currentSummary = normalizedSummary;
                OnPropertyChanged(nameof(CurrentSummary));
                OnPropertyChanged(nameof(CurrentSummaryDisplay));
                OnPropertyChanged(nameof(CurrentSummaryHelpText));
            }
            if (!string.Equals(parentSummary, normalizedParentSummary, StringComparison.Ordinal))
            {
                parentSummary = normalizedParentSummary;
                OnPropertyChanged(nameof(ParentSummary));
                OnPropertyChanged(nameof(CurrentSummaryDisplay));
                OnPropertyChanged(nameof(CurrentSummaryHelpText));
            }
        }

        public void SetGroupMode(string value)
        {
            var normalized = value ?? string.Empty;
            if (string.Equals(groupMode, normalized, StringComparison.Ordinal)) return;
            groupMode = normalized;
            OnPropertyChanged(nameof(GroupKey));
        }

        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private static string NormalizeSummary(string? value)
        {
            if (value is null || string.IsNullOrWhiteSpace(value)) return string.Empty;
            var normalized = value.Trim();
            if (normalized.Length > 240) normalized = normalized.Substring(0, 240);
            return normalized;
        }
    }
}
