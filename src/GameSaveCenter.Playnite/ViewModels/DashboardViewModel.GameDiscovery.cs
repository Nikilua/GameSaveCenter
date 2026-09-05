using System;
using System.Linq;
using System.Threading.Tasks;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.ViewModels
{
    public sealed partial class DashboardViewModel
    {
        private string gameDiagnosticPlayniteId = string.Empty;
        private GameDiscoveryDiagnosticDto? gameDiscoveryDiagnostic;
        private bool isGameDiagnosticLoading;

        public string GameDiagnosticPlayniteId
        {
            get => gameDiagnosticPlayniteId;
            set
            {
                var normalized = value ?? string.Empty;
                if (string.Equals(gameDiagnosticPlayniteId, normalized, StringComparison.Ordinal)) return;
                gameDiagnosticPlayniteId = normalized;
                OnPropertyChanged(nameof(GameDiagnosticPlayniteId));
                RaiseCommandStates();
            }
        }

        public GameDiscoveryDiagnosticDto? GameDiscoveryDiagnostic
        {
            get => gameDiscoveryDiagnostic;
            private set
            {
                SetValue(ref gameDiscoveryDiagnostic, value);
                OnPropertyChanged(nameof(GameDiscoveryDiagnosticSummary));
            }
        }

        public bool IsGameDiagnosticLoading
        {
            get => isGameDiagnosticLoading;
            private set
            {
                SetValue(ref isGameDiagnosticLoading, value);
                RaiseCommandStates();
            }
        }

        public string GameDiscoveryDiagnosticSummary => FormatGameDiscoveryDiagnostic(GameDiscoveryDiagnostic);

        private void RunGameDiscoveryDiagnostic()
        {
            if (IsBusy) return;
            Observe(LoadGameDiscoveryDiagnosticAsync());
        }

        private async Task LoadGameDiscoveryDiagnosticAsync()
        {
            var id = (GameDiagnosticPlayniteId ?? string.Empty).Trim();
            if (id.Length == 0)
            {
                StatusMessage = "请输入 Playnite 游戏 ID，或先选择一个游戏。";
                return;
            }

            IsBusy = true;
            IsGameDiagnosticLoading = true;
            try
            {
                var result = await plugin.GetGameDiscoveryDiagnosticAsync(
                    id,
                    gamePicker.StatusFilter,
                    gamePicker.PlatformFilter,
                    gamePicker.SearchText).ConfigureAwait(false);
                ApplyOnUi(() => ApplyGameDiscoveryDiagnostic(result));
                StatusMessage = result.WorkerReachable ? "游戏来源诊断已更新。" : "已显示 Playnite 侧诊断；Worker 当前不可用。";
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "诊断已取消。";
            }
            catch (Exception ex)
            {
                ReportDashboardFailure(ex, true);
            }
            finally
            {
                IsGameDiagnosticLoading = false;
                IsBusy = false;
            }
        }

        private async Task SynchronizeGameDescriptorAsync()
        {
            var id = RequireDiagnosticId();
            var result = await plugin.SynchronizeGameDescriptorAsync(
                id,
                gamePicker.StatusFilter,
                gamePicker.PlatformFilter,
                gamePicker.SearchText).ConfigureAwait(false);
            ApplyOnUi(() => ApplyGameDiscoveryDiagnostic(result));
            await RefreshDashboardAsync(false, false, TimeSpan.FromSeconds(8)).ConfigureAwait(false);
            StatusMessage = "已同步此游戏描述；未触发其他游戏的匹配。";
        }

        private async Task RetryGameMatchAsync()
        {
            var id = RequireDiagnosticId();
            var result = await plugin.RetryGameMatchAsync(
                id,
                gamePicker.StatusFilter,
                gamePicker.PlatformFilter,
                gamePicker.SearchText).ConfigureAwait(false);
            ApplyOnUi(() => ApplyGameDiscoveryDiagnostic(result));
            await RefreshDashboardAsync(false, false, TimeSpan.FromSeconds(8)).ConfigureAwait(false);
            StatusMessage = "已重试此游戏匹配；未触发其他游戏的匹配。";
        }

        private string RequireDiagnosticId()
        {
            var id = (GameDiagnosticPlayniteId ?? string.Empty).Trim();
            if (id.Length == 0) throw new InvalidOperationException("请输入 Playnite 游戏 ID，或先选择一个游戏。");
            return id;
        }

        private void ClearGamePickerFilters()
        {
            gamePicker.ClearFiltersCommand.Execute(null);
            gameSearchText = gamePicker.SearchText;
            gameStatusFilter = gamePicker.StatusFilter;
            OnPropertyChanged(nameof(GameSearchText));
            OnPropertyChanged(nameof(GameStatusFilter));
            RefreshGameView();
            StatusMessage = "已清除游戏搜索、状态和平台筛选。";
        }

        private void ApplyGameDiscoveryDiagnostic(GameDiscoveryDiagnosticDto result)
        {
            var current = Games.FirstOrDefault(game =>
                string.Equals(game.PlayniteId, result.PlayniteId, StringComparison.OrdinalIgnoreCase));
            var status = current ?? new GameStatusDto
            {
                PlayniteId = result.PlayniteId,
                Name = result.Name,
                Platform = result.Platform,
                IsInstalled = result.IsInstalled,
                PlayniteIsInstalled = result.PlayniteIsInstalled,
                InstallStateSource = result.InstallStateSource,
                LudusaviMatched = result.LudusaviMatched,
                LudusaviName = result.LudusaviName,
                BackupVersionCount = result.BackupVersionCount,
                LastBackupUtc = result.LastBackupUtc,
                HealthState = "Unknown",
                CloudState = "Disabled"
            };
            result.FilterExclusionReasons = gamePicker.GetFilterExclusionReasons(status).ToList();
            GameDiscoveryDiagnostic = result;
        }

        private static string FormatGameDiscoveryDiagnostic(GameDiscoveryDiagnosticDto? diagnostic)
        {
            if (diagnostic == null) return "尚未诊断游戏。请输入 Playnite 游戏 ID。";

            var lines = new System.Collections.Generic.List<string>
            {
                $"游戏：{(string.IsNullOrWhiteSpace(diagnostic.Name) ? "（未知）" : diagnostic.Name)}",
                $"Playnite：{(diagnostic.PlayniteExists ? "存在" : "不存在")}{(diagnostic.SourceMissing ? "（来源缺失，备份与历史仍保留）" : string.Empty)}",
                $"Worker 描述：{(diagnostic.WorkerRecordExists ? "存在" : diagnostic.WorkerReachable ? "不存在" : "未确认")}",
                $"安装：Playnite 原始标志={(diagnostic.PlayniteIsInstalled ? "已安装" : "未安装")}；当前判定={(diagnostic.IsInstalled ? "已安装" : "未安装")}；来源={FormatInstallSource(diagnostic.InstallStateSource)}",
                $"安装目录信号：{(diagnostic.HasInstallDirectoryConfigured ? diagnostic.InstallDirectoryPresent ? "已配置且存在" : "已配置但不存在" : "未配置")}",
                $"Worker 描述同步：{FormatUtc(diagnostic.DescriptorSyncedUtc)}",
                $"匹配：{FormatMatchState(diagnostic.MatchState, diagnostic.LudusaviName, diagnostic.MatchConfidence)}；最后尝试：{FormatUtc(diagnostic.LastMatchAttemptUtc)}",
                $"备份：{diagnostic.BackupVersionCount} 个版本；最近备份：{FormatUtc(diagnostic.LastBackupUtc)}",
                $"当前筛选：状态={diagnostic.CurrentStatusFilter}；平台={diagnostic.CurrentPlatformFilter}；搜索={(string.IsNullOrWhiteSpace(diagnostic.CurrentSearchText) ? "（空）" : "“" + diagnostic.CurrentSearchText.Trim() + "”")}"
            };

            if (!diagnostic.WorkerReachable)
                lines.Add("Worker：当前不可用；上面的 Playnite 侧事实仍可用于判断来源。稍后可重试诊断。\n" + diagnostic.WorkerMessage);
            if (diagnostic.FilterExclusionReasons.Count == 0)
                lines.Add("排除原因：无；按当前筛选应可显示此游戏。\n若列表仍为空，请检查选择器是否正在刷新。");
            else
                lines.Add("排除原因：\n· " + string.Join("\n· ", diagnostic.FilterExclusionReasons));
            return string.Join("\n", lines);
        }

        private static string FormatInstallSource(string source)
        {
            switch (source)
            {
                case GameInstallStateSources.PlayniteFlag: return "Playnite 安装标志";
                case GameInstallStateSources.InstallDirectory: return "安装目录存在";
                case GameInstallStateSources.PlayAction: return "启动动作路径存在";
                case GameInstallStateSources.None: return "未发现安装信号";
                default: return "来源未知";
            }
        }

        private static string FormatMatchState(string state, string name, double confidence)
        {
            if (string.Equals(state, "Matched", StringComparison.OrdinalIgnoreCase))
                return $"已匹配 {name}（置信度 {confidence:0.00}）";
            if (string.Equals(state, "NeverAttempted", StringComparison.OrdinalIgnoreCase)) return "尚未尝试";
            if (string.Equals(state, "UnmatchedAfterAttempt", StringComparison.OrdinalIgnoreCase)) return "已尝试但未匹配";
            return "未知";
        }

        private static string FormatUtc(DateTime? value)
            => value.HasValue ? value.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") : "未知";
    }
}
