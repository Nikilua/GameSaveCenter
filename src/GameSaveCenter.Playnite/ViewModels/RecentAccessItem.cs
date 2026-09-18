using System;
using GameSaveCenter.Playnite.Settings;

namespace GameSaveCenter.Playnite.ViewModels
{
    /// <summary>Current-snapshot presentation of one durable recent-access record.</summary>
    public sealed class RecentAccessItem
    {
        public RecentAccessItem(RecentAccessRecord record, string gameName)
        {
            PlayniteId = record.PlayniteId;
            GameName = string.IsNullOrWhiteSpace(gameName) ? "未知游戏" : gameName;
            Workspace = record.Workspace;
            WorkspaceDisplay = ToWorkspaceDisplay(record.Workspace);
            TabIndex = record.TabIndex;
            LastAccessUtc = record.LastAccessUtc;
        }

        public string PlayniteId { get; }
        public string GameName { get; }
        public string Workspace { get; }
        public string WorkspaceDisplay { get; }
        public int TabIndex { get; }
        public DateTime LastAccessUtc { get; }
        public string LastAccessDisplay => LastAccessUtc.ToLocalTime().ToString("MM-dd HH:mm");
        public string SummaryDisplay => $"{WorkspaceDisplay} · {LastAccessDisplay}";

        public static string ToWorkspaceDisplay(string workspace)
            => workspace switch
            {
                RecentAccessRecord.SavesWorkspace => "存档中心",
                RecentAccessRecord.TrainersWorkspace => "工具中心",
                RecentAccessRecord.MediaWorkspace => "媒体中心",
                RecentAccessRecord.TasksWorkspace => "任务中心",
                RecentAccessRecord.MaintenanceWorkspace => "维护中心",
                _ => "首页"
            };
    }
}
