using System;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.ViewModels;

/// <summary>
/// Explains the same prerequisites that the workspace action commands enforce.
/// The text is intentionally independent from ToolTip so keyboard and assistive
/// technology users can understand a disabled action in the page flow.
/// </summary>
internal static class ActionAvailabilityHints
{
    public static string Restore(bool hasGame, bool hasBackup, bool ludusaviAvailable, bool isBusy)
    {
        if (isBusy)
            return "正在执行其他操作；当前恢复动作会在本次操作完成后重新评估。";
        if (!hasGame)
            return "请先在页面顶部的游戏选择器选中一个游戏；选择后才能读取历史版本。";
        if (!hasBackup)
            return "请先在历史版本列表选中一个版本；选中后可先验证可恢复性。";
        if (!ludusaviAvailable)
            return "当前不能安全恢复：Ludusavi 不可用。请到维护中心运行环境检查；危险恢复命令保持禁用。";

        return "已选中版本；可先验证可恢复性。安全恢复会先创建并锁定当前快照，完成后可撤销。";
    }

    public static bool RestoreNeedsMaintenance(bool hasGame, bool hasBackup, bool ludusaviAvailable, bool isBusy)
        => !isBusy && hasGame && hasBackup && !ludusaviAvailable;

    public static string MediaInbox(bool workerHealthy, string mode, bool hasSelectedMedia, bool hasTargetGame, bool isBusy)
    {
        if (isBusy)
            return "正在执行其他操作；当前媒体动作会在本次操作完成后重新评估。";
        if (!workerHealthy)
            return "媒体收件箱动作暂不可用：后台服务当前离线。请到维护中心检查并重试；来源文件不会被删除。";
        if (string.Equals(mode, "已忽略", StringComparison.Ordinal))
        {
            return hasSelectedMedia
                ? "当前是已忽略视图；可将所选媒体恢复到待归类，之后再选择目标游戏归类。"
                : "当前是已忽略视图；请先在列表选中媒体，才可恢复到待归类。";
        }
        if (!hasSelectedMedia)
            return "请先在媒体列表选中媒体（一项或多项）；选中后才能归类或忽略。";
        if (!hasTargetGame)
            return "已选中媒体；请先选择目标游戏，归类按钮才会启用。";

        return "已选中媒体和目标游戏；可确认归类，来源文件和原始副本仍会保留。";
    }

    public static bool MediaInboxNeedsMaintenance(bool workerHealthy, bool isBusy)
        => !isBusy && !workerHealthy;

    public static string CloudTransfer(
        bool workerHealthy,
        bool cloudUploadEnabled,
        bool rcloneAvailable,
        CloudTransferStatusDto? selected,
        bool isBusy)
    {
        if (isBusy)
            return "正在执行其他操作；当前云端动作会在本次操作完成后重新评估。";
        if (!workerHealthy)
            return "云端队列动作暂不可用：后台服务当前离线。请到维护中心检查并重试；本地副本不会被当作远端已校验。";
        if (!cloudUploadEnabled)
            return "云端上传未启用；请到维护中心检查设置。当前不会把本地副本当作已上传或已校验。";
        if (!rcloneAvailable)
            return "云端上传暂不可用：云端工具或远端配置尚未就绪。请到维护中心运行环境检查；上传/校验动作保持受限。";
        if (selected == null)
            return "请先在云端队列选中一条记录；详情中的远端校验和上传重试会按记录状态启用。";

        var state = selected.State ?? string.Empty;
        return state switch
        {
            "Pending" => "该记录仍待上传；请等待队列处理完成，远端校验需在上传后进行。",
            "Transferring" => "该记录正在传输；请等待本次上传完成，暂不能重复提交。",
            "Verifying" => "该记录正在进行远端校验；请等待只读校验完成。",
            "RetryScheduled" => "该记录已安排下次上传；当前可重试上传，上传成功后仍需执行远端校验。",
            "AuthenticationRequired" => "该记录需要先处理远端认证；请到维护中心检查认证配置，认证失败不会自动重试。",
            "Uploaded" => "该记录已上传但尚未远端校验；可以执行只读校验，不会上传或覆盖本地副本。",
            "RemoteVerified" => "该记录已完成远端校验；本地副本未被修改，无需重复上传。",
            "CheckFailed" => "该记录的远端校验未通过；可以重新执行只读校验，确认前不会覆盖本地副本。",
            "CheckCancelled" => "该记录的远端校验已取消；可以重新执行只读校验。",
            "Failed" => "该记录上传失败；当前可重试上传，上传成功后仍需执行远端校验。",
            "Paused" => "云端队列已暂停；请到维护中心检查队列策略，当前不会把本地副本当作远端已校验。",
            _ => $"当前记录状态为“{selected.StateDisplay}”；请先确认状态后再执行远端校验。"
        };
    }

    public static bool CloudTransferNeedsMaintenance(
        bool workerHealthy,
        bool cloudUploadEnabled,
        bool rcloneAvailable,
        CloudTransferStatusDto? selected,
        bool isBusy)
        => !isBusy
           && (!workerHealthy
               || !cloudUploadEnabled
               || !rcloneAvailable
               || string.Equals(selected?.State, "AuthenticationRequired", StringComparison.OrdinalIgnoreCase));

    public static string RemoteRestore(
        bool hasComparison,
        bool hasRemoteBackupId,
        bool hasStagedBackup,
        bool stagedBackupVerified,
        bool isBusy)
    {
        if (isBusy)
            return "正在执行其他操作；远端恢复动作会在本次操作完成后重新评估。";
        if (!hasComparison)
            return "请先在设备对比列表选中一条记录；没有选中记录时不会下载或恢复。";
        if (!hasRemoteBackupId)
            return "该设备记录没有远端备份标识，不能开始隔离下载；请刷新设备状态后再确认。";
        if (!hasStagedBackup)
            return "请先执行“1 · 下载到隔离区并校验”；只有隔离区结果准备好后，才会开放“2 · 快照并恢复”。";
        if (!stagedBackupVerified)
            return "隔离下载结果尚未通过校验，恢复命令保持禁用；请重新下载到隔离区并校验。";

        return "隔离备份已校验；执行恢复前会创建并锁定本机当前快照，并要求确认游戏已关闭。";
    }
}
