using System;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.ViewModels
{
    /// <summary>Describes the single highest-priority action presented by the overview hero.</summary>
    public sealed class OverviewPriorityState
    {
        public OverviewPriorityState(string kind, string actionKind, string title, string description, string actionText)
        {
            Kind = kind;
            ActionKind = actionKind;
            Title = title;
            Description = description;
            ActionText = actionText;
        }

        public string Kind { get; }
        public string ActionKind { get; }
        public string Title { get; }
        public string Description { get; }
        public string ActionText { get; }
        public string ActionToolTip => Description;
    }

    /// <summary>
    /// Keeps overview hierarchy deterministic and independent from WPF. The first matching
    /// state owns the hero so the page never presents several competing "next" actions.
    /// </summary>
    public static class OverviewPriorityResolver
    {
        public static OverviewPriorityState Resolve(DashboardSnapshotDto snapshot, bool isOnboardingPending)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));

            if (!snapshot.WorkerHealthy)
            {
                return new OverviewPriorityState(
                    "Worker",
                    "Maintenance",
                    "Worker 需要处理",
                    "后台服务当前不可用，先打开维护中心检查服务状态。",
                    "打开维护中心");
            }

            if (isOnboardingPending)
            {
                return new OverviewPriorityState(
                    "Onboarding",
                    "Maintenance",
                    "先完成环境准备",
                    "首次使用先完成非破坏性环境检查，完成后再执行备份。",
                    "开始环境检查");
            }

            var cloud = snapshot.CloudTransfers ?? new CloudTransferSummaryDto();
            if (cloud.AttentionCount > 0)
            {
                return new OverviewPriorityState(
                    "Cloud",
                    "CloudQueue",
                    $"{cloud.AttentionCount} 项云端任务需要处理",
                    "云端队列中有失败、认证或待重试记录，打开队列查看具体原因。",
                    "查看云端队列");
            }

            if (snapshot.UnassignedMediaCount > 0)
            {
                return new OverviewPriorityState(
                    "Media",
                    "Media",
                    $"{snapshot.UnassignedMediaCount} 项媒体待归类",
                    "待归类媒体不会自动归入游戏，先确认目标游戏后再应用归类。",
                    "打开媒体中心");
            }

            if (snapshot.WarningGames > 0)
            {
                return new OverviewPriorityState(
                    "Attention",
                    "Attention",
                    $"{snapshot.WarningGames} 个游戏需要注意",
                    "打开关注中心查看具体原因和建议处理方式。",
                    "查看关注项");
            }

            return new OverviewPriorityState(
                "Healthy",
                "Refresh",
                "整体状态安全",
                "当前没有更高优先级事项；需要时可以刷新概览获取最新快照。",
                "刷新概览");
        }
    }
}
