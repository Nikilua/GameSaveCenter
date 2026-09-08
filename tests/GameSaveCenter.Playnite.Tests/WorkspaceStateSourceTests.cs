using System;
using System.IO;
using System.Linq;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class WorkspaceStateSourceTests
{
    [LegacyProductionUiBaselineFact]
    public void SharedWorkspaceStatePresenterExistsAndIsUsedAcrossPages()
    {
        var root = FindRepositoryRoot();
        var control = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Controls", "WorkspaceStatePresenter.cs"));
        var redesign = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "Redesign.xaml"));
        var overview = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml"));
        var task = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml"));
        var save = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
        var trainer = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TrainerCenterView.xaml"));
        var media = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));
        var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));

        Assert.Contains("class WorkspaceStatePresenter", control);
        Assert.Contains("StateProperty", control);
        Assert.Contains("TitleProperty", control);
        Assert.Contains("MessageProperty", control);
        Assert.Contains("RetryCommandProperty", control);
        Assert.Contains("x:Key=\"GscWorkspaceStatePresenter\"", redesign);
        Assert.Contains("<ui:Button x:Name=\"RetryButton\"", redesign);
        Assert.DoesNotContain("<Button x:Name=\"RetryButton\"", redesign);
        Assert.Contains("ui:WorkspaceStatePresenter", overview);
        Assert.Contains("ui:WorkspaceStatePresenter", task);
        Assert.Contains("ui:WorkspaceStatePresenter", save);
        Assert.Contains("ui:WorkspaceStatePresenter", trainer);
        Assert.Contains("ui:WorkspaceStatePresenter", media);
        Assert.Contains("ui:WorkspaceStatePresenter", maintenance);
        Assert.Contains("State=\"Loading\"", save);
        Assert.Contains("State=\"Empty\"", trainer);
        Assert.Contains("State=\"Loading\"", trainer);
        Assert.Contains("State=\"Offline\"", media);
        Assert.Contains("State=\"Degraded\"", maintenance);
    }

    [Fact]
    public void StatePresenterSupportsSixUnifiedStates()
    {
        var root = FindRepositoryRoot();
        var control = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Controls", "WorkspaceStatePresenter.cs"));

        Assert.Contains("\"Loading\"", control);
        Assert.Contains("\"Empty\"", control);
        Assert.Contains("\"Error\"", control);
        Assert.Contains("\"Degraded\"", control);
        Assert.Contains("\"Offline\"", control);
        Assert.Contains("\"Disabled\"", control);
    }

    [Fact]
    public void TrainerCatalogAndReleaseEmptyStatesWaitForRealLoadingToFinish()
    {
        var root = FindRepositoryRoot();
        var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.WorkspaceStates.cs"));
        var implementation = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var trainer = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TrainerCenterView.xaml"));

        Assert.Contains("IsTrainerCatalogLoading", viewModel);
        Assert.Contains("IsTrainerReleasesLoading", viewModel);
        Assert.Contains("IsTrainerCatalogLoading = true", implementation);
        Assert.Contains("IsTrainerReleasesLoading = true", implementation);
        Assert.Contains("Condition Binding=\"{Binding IsTrainerCatalogLoading}\" Value=\"False\"", trainer);
        Assert.Contains("Condition Binding=\"{Binding IsTrainerReleasesLoading}\" Value=\"False\"", trainer);
        Assert.Contains("Title=\"正在读取 FLiNG 目录\"", trainer);
        Assert.Contains("Title=\"正在读取可下载版本\"", trainer);
    }

    [Fact]
    public void TrainerReleaseLoadingIgnoresStaleSelectionsAndQueuesTheLatestOne()
    {
        var root = FindRepositoryRoot();
        var implementation = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var trainer = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TrainerCenterView.xaml"));

        Assert.Contains("trainerReleaseLoadGeneration", implementation);
        Assert.Contains("pendingTrainerReleaseCatalogId", implementation);
        Assert.Contains("generation != Interlocked.Read(ref trainerReleaseLoadGeneration)", implementation);
        Assert.Contains("StartQueuedTrainerReleaseLoad();", implementation);
        Assert.Contains("RequestTrainerReleasesLoad(TrainerCatalogItemDto? requested = null)", implementation);
        Assert.Contains("CommandParameter=\"{Binding}\"", trainer);
    }

    [Fact]
    public void MediaInboxLoadingKeepsTheLatestModeAndIgnoresStaleSelections()
    {
        var root = FindRepositoryRoot();
        var implementation = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var media = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.Media.cs"));

        Assert.Contains("mediaInboxLoadGeneration", implementation);
        Assert.Contains("pendingMediaInboxLoadMode", implementation);
        Assert.Contains("StartQueuedMediaInboxLoad();", implementation);
        Assert.Contains("requestGeneration != Interlocked.Read(ref mediaInboxLoadGeneration)", media);
        Assert.Contains("var currentSelectedId = SelectedInboxMedia?.MediaId", media);
        Assert.Contains("var currentTargetId = InboxTargetGame?.PlayniteId", media);
        Assert.Contains("LoadMediaInboxModeAsync", media);
        Assert.Contains("RequestMediaInboxPageAsync(true, ignored: false, requestGeneration: requestGeneration)", media);
        Assert.Contains("ListUnassignedMediaPage", media);
        Assert.Contains("ListIgnoredMediaPage", media);
        Assert.Contains("LoadMoreMediaInboxPageAsync", media);
        Assert.Contains("if (requestGeneration != Interlocked.Read(ref mediaInboxLoadGeneration))", media);
        Assert.Contains("if (!string.Equals(MediaInboxMode, requestMode, StringComparison.Ordinal)", media);
        Assert.Contains("if (inbox == null) return;", media);
        Assert.Contains("if (ignored == null) return;", media);
        Assert.Contains("if (MediaInboxMode == \"已忽略\") await LoadIgnoredMediaAsync();", implementation);
        Assert.Contains("var selectedBackupId = SelectedBackup?.BackupId", implementation);
        Assert.Contains("var selectedMediaId = SelectedMedia?.MediaId", implementation);
        Assert.Contains("SelectedMedia = Media.FirstOrDefault(x => string.Equals(x.MediaId, selectedId", media);
        Assert.Contains("媒体收件箱暂时不可用", File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml")));
    }

    [Fact]
    public void ConsistentPagedViewsPreserveSelectionAndStopAfterRepeatedResets()
    {
        var root = FindRepositoryRoot();
        var cloud = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.CloudTransfers.cs"));
        var history = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.MediaClassification.cs"));
        var state = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));

        Assert.Contains("pendingCloudTransferKey = selectedKey", cloud);
        Assert.Contains("var shouldLoadPending = restored == null", cloud);
        Assert.Contains("response?.HasMore == true", cloud);
        Assert.Contains("!pageResetReceived && loadPendingSelectionPage", cloud);
        Assert.Contains("请点击“刷新队列”后继续", cloud);
        Assert.DoesNotContain("else if (!string.IsNullOrWhiteSpace(pendingCloudTransferKey))", cloud);

        Assert.Contains("pendingMediaClassificationBatchId = selectedBatchId", history);
        Assert.Contains("var shouldLoadPending = restored == null", history);
        Assert.Contains("!pageResetReceived && loadPendingSelectionPage", history);
        Assert.Contains("请点击“刷新”后继续", history);
        Assert.DoesNotContain("if (restored != null || reset)", history);

        Assert.Contains("CloudTransferNeedsManualRefresh", state);
        Assert.Contains("MediaClassificationHistoryNeedsManualRefresh", state);
    }

    [Fact]
    public void MediaAndMaintenanceUseTheSharedRealDataStateContract()
    {
        var root = FindRepositoryRoot();
        var states = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.WorkspaceStates.cs"));
        var dashboard = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var media = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));
        var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));

        foreach (var state in new[] { "Loading", "Ready", "Empty", "Stale", "Error", "Offline" })
            Assert.Contains(state, states);

        Assert.Contains("MediaDetailsPresenterState", states);
        Assert.Contains("MediaInboxPresenterState", states);
        Assert.Contains("MaintenancePresenterState", states);
        Assert.Contains("FailMediaDetailsLoad", states);
        Assert.Contains("FailMediaInboxLoad", states);
        Assert.Contains("FailMaintenanceLoad", states);
        Assert.Contains("RefreshCoreBodyAsync", dashboard);
        Assert.Contains("MediaDetailsStaleVisible", media);
        Assert.Contains("MediaInboxStaleVisible", media);
        Assert.Contains("MaintenanceStaleVisible", maintenance);
        Assert.Contains("RetryCommand=\"{Binding ReloadMediaWindowCommand}\"", media);
        Assert.Contains("RetryCommand=\"{Binding ReloadMediaInboxCommand}\"", media);
        Assert.Contains("RetryCommand=\"{Binding RefreshDiagnosticsCommand}\"", maintenance);
        Assert.Contains("<Trigger Property=\"RetryCommand\" Value=\"{x:Null}\">", File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "Redesign.xaml")));
        Assert.DoesNotContain("IsHitTestVisible=\"False\"", media.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).First(line => line.IndexOf("RetryCommand=\"{Binding ReloadMediaInboxCommand}\"", StringComparison.Ordinal) >= 0));
        Assert.DoesNotContain("IsHitTestVisible=\"False\"", media.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).First(line => line.IndexOf("RetryCommand=\"{Binding ReloadMediaWindowCommand}\"", StringComparison.Ordinal) >= 0));
        Assert.DoesNotContain("IsHitTestVisible=\"False\"", maintenance.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).First(line => line.IndexOf("RetryCommand=\"{Binding RefreshDiagnosticsCommand}\"", StringComparison.Ordinal) >= 0));
    }

    [Fact]
    public void RenderHarnessStateFixturesCoverEveryProductionStateBinding()
    {
        var root = FindRepositoryRoot();
        var fake = File.ReadAllText(Path.Combine(root, "tests", "GameSaveCenter.RenderHarness", "FakeDashboardData.cs"));
        var harness = File.ReadAllText(Path.Combine(root, "tests", "GameSaveCenter.RenderHarness", "Program.cs"));

        foreach (var state in new[] { "Ready", "Empty", "Loading", "Error", "Stale", "Offline" })
            Assert.Contains(state, fake);

        foreach (var binding in new[]
        {
            "MediaDetailsPresenterState", "MediaDetailsStateOverlayVisible", "MediaDetailsStaleVisible",
            "MediaInboxPresenterState", "MediaInboxStateOverlayVisible", "MediaInboxStaleVisible",
            "MediaInboxCountDisplay", "MediaInboxCountCaption",
            "MaintenancePresenterState", "MaintenanceStateOverlayVisible", "MaintenanceStaleVisible",
            "MaintenanceActionSummary", "MaintenanceActionItems"
        })
            Assert.Contains(binding, fake);

        Assert.Contains("statefixtures", harness);
        Assert.Contains("VerifyWorkspaceStateFixtureBindingSurface", harness);
        Assert.Contains("MediaInboxScrollSurface", harness);
        Assert.Contains("MediaCurrentScrollSurface", harness);
        Assert.Contains("MaintenanceAuditScrollSurface", harness);
        Assert.Contains("ThemeWindowSizes", harness);
    }

    [Fact]
    public void DetailEditorDraftsSurviveRefreshOfTheSameItem()
    {
        var root = FindRepositoryRoot();
        var implementation = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var media = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.Media.cs"));

        Assert.Contains("backupCommentDirty", implementation);
        Assert.Contains("backupLockDirty", implementation);
        Assert.Contains("mediaCommentDirty", implementation);
        Assert.Contains("mediaFavoriteDirty", implementation);
        Assert.Contains("SyncBackupEditor(value, sameBackup)", implementation);
        Assert.Contains("SyncMediaEditor(value, sameMedia)", implementation);
        Assert.Contains("if (applyComment) backupCommentDirty = false", implementation);
        Assert.Contains("if (applyFavorite) mediaFavoriteDirty = false", implementation);
        Assert.Contains("backupCommentDirty = false;", implementation);
        Assert.Contains("mediaCommentDirty = false;", media);
        Assert.Contains("var playniteId = selected.PlayniteId;", implementation);
        Assert.Contains("var policy = GameSaveCenter.Core.Services.BackupPolicyTemplateCatalog.ClonePolicy(selected.Policy);", implementation);
        Assert.Contains("if (IsSelectedGame(playniteId))", implementation);
        Assert.Contains("var gameId = SelectedGame?.PlayniteId", implementation);
        Assert.Contains("string.Equals(SelectedBackup?.BackupId, backupId, StringComparison.OrdinalIgnoreCase)", implementation);
        Assert.Contains("var gameId = SelectedGame?.PlayniteId ?? throw new InvalidOperationException(\"请先选择游戏。\");", media);
        Assert.Contains("if (CurrentWorkspace == WorkspaceKind.Media && IsSelectedGame(gameId))", media);
        Assert.Contains("var mediaIds = selected.Select(x=>x.MediaId).ToList();", media);
        Assert.Contains("if (updateComment && string.Equals(MediaComment, comment, StringComparison.Ordinal))", media);
        Assert.Contains("if (favorite.HasValue && MediaFavorite == favorite.Value)", media);
        Assert.Contains("var sourceGameId = SelectedGame?.PlayniteId", media);
        Assert.Contains("var targetName = target.Name;", media);
        Assert.Contains("var executable = ProcessMappingExecutable;", implementation);
        Assert.Contains("if (string.Equals(ProcessMappingExecutable, executable, StringComparison.Ordinal))", implementation);
        Assert.Contains("var gameName = game.Name;", implementation);
        Assert.Contains("if (CurrentWorkspace != WorkspaceKind.Saves", implementation);
        Assert.Contains("var templateName = template.Name;", implementation);
    }

    [Fact]
    public void GamePickerRefreshPropagatesCurrentGameChangesToDashboardBindings()
    {
        var root = FindRepositoryRoot();
        var picker = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "GamePickerViewModel.cs"));
        var dashboard = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));

        Assert.Contains("if (ReferenceEquals(previousSelectedItem, candidate))", picker);
        Assert.Contains("OnPropertyChanged(nameof(SelectedGame));", picker);
        Assert.Contains("nameof(GamePickerViewModel.SelectedGame)", dashboard);
        Assert.Contains("OnPropertyChanged(nameof(SelectedGame));", dashboard);
        Assert.Contains("CaptureSelectedGamePolicyDraft", dashboard);
        Assert.Contains("CloneGameWithPolicy", dashboard);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
