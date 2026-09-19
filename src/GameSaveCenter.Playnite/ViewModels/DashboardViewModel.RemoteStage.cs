using System;
using System.Threading;
using System.Windows.Input;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.ViewModels;

internal sealed class RemoteBackupStageProjection
{
    private RemoteBackupStageProjection(bool isRelevant,bool isActive,int progress,string statusText)
    {
        IsRelevant=isRelevant;
        IsActive=isActive;
        Progress=progress;
        StatusText=statusText;
    }

    public bool IsRelevant { get; }
    public bool IsActive { get; }
    public int Progress { get; }
    public string StatusText { get; }

    public static RemoteBackupStageProjection FromTask(TaskStatusDto task)
    {
        if (task==null||!string.Equals(task.TaskType,"RemoteStage",StringComparison.OrdinalIgnoreCase))
            return new RemoteBackupStageProjection(false,false,0,string.Empty);

        var progress=Math.Max(0,Math.Min(100,task.ProgressPercent));
        if(task.State==TaskState.Queued||task.State==TaskState.Running)
            return new RemoteBackupStageProjection(true,true,progress,
                string.IsNullOrWhiteSpace(task.Message)?"正在准备远端备份下载":task.Message);
        if(task.State==TaskState.Succeeded)
            return new RemoteBackupStageProjection(true,false,100,"下载完成并已校验，等待恢复确认。");
        if(task.State==TaskState.Cancelled)
            return new RemoteBackupStageProjection(true,false,progress,"远端备份下载已取消；正在确认隔离区清理结果。");

        var detail=string.IsNullOrWhiteSpace(task.ErrorMessage)?task.Message:task.ErrorMessage;
        return new RemoteBackupStageProjection(true,false,progress,
            string.IsNullOrWhiteSpace(detail)?"远端备份下载或校验失败。":"远端备份下载或校验失败；"+detail);
    }
}

public sealed partial class DashboardViewModel
{
    private CancellationTokenSource? remoteStageCancellation;
    private bool isRemoteBackupStageActive;
    private int remoteBackupStageProgress;

    public bool IsRemoteBackupStageActive
    {
        get=>isRemoteBackupStageActive;
        private set=>SetValue(ref isRemoteBackupStageActive,value);
    }

    public int RemoteBackupStageProgress
    {
        get=>remoteBackupStageProgress;
        private set=>SetValue(ref remoteBackupStageProgress,value);
    }

    private void ApplyRemoteBackupStageTaskUpdate(TaskStatusDto task)
    {
        var projection=RemoteBackupStageProjection.FromTask(task);
        if(!projection.IsRelevant)return;
        IsRemoteBackupStageActive=projection.IsActive;
        RemoteBackupStageProgress=projection.Progress;
        if(task.State!=TaskState.Succeeded||StagedRemoteBackup==null)
            StagedRemoteBackupStatus=projection.StatusText;
        OnPropertyChanged(nameof(IsRemoteBackupStageActive));
        OnPropertyChanged(nameof(RemoteBackupStageProgress));
        RaiseCommandStates();
    }

    private void BeginRemoteBackupStage()
    {
        remoteStageCancellation?.Cancel();
        remoteStageCancellation=new CancellationTokenSource();
        IsRemoteBackupStageActive=true;
        RemoteBackupStageProgress=0;
        StagedRemoteBackupStatus="正在准备远端备份下载；当前存档不会被覆盖。";
        OnPropertyChanged(nameof(IsRemoteBackupStageActive));
        OnPropertyChanged(nameof(RemoteBackupStageProgress));
        RaiseCommandStates();
    }

    private void CancelRemoteBackupStage()
    {
        try { remoteStageCancellation?.Cancel(); }
        catch(ObjectDisposedException) { }
        StagedRemoteBackupStatus="正在请求取消远端下载，等待隔离区收尾……";
        OnPropertyChanged(nameof(StagedRemoteBackupStatus));
        RaiseCommandStates();
    }

    private void EndRemoteBackupStage()
    {
        IsRemoteBackupStageActive=false;
        OnPropertyChanged(nameof(IsRemoteBackupStageActive));
        OnPropertyChanged(nameof(RemoteBackupStageProgress));
        RaiseCommandStates();
        var cancellation=remoteStageCancellation;
        remoteStageCancellation=null;
        cancellation?.Dispose();
    }
}
