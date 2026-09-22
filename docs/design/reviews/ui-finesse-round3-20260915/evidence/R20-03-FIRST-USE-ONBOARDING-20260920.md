# R20-03 首次配置引导证据

日期：2026-09-20  
分支：`codex/ui-finesse-round2`  
结论：已满足，待环境验证；本阶段无生产代码变更

## 1. 现有实现复核

R20-03 要求在现有页面提供最小必要步骤和已完成标记；未配外部工具时用户知道下一步；引导可返回，且不能自动更改用户配置。

当前实现来自已在本分支祖先链中的 `1a90cd061fd617c9f53c43b92e85cbc95ccdec1e`（`feat: add first-use environment onboarding`），本阶段没有重建新服务、DTO 或设置页面：

- `MaintenanceView.xaml` 的 `EnvironmentCheckCard` 复用维护工作区，显示 `OnboardingTitle`、说明、最近检查时间、逐项 `EnvironmentCheck.Items` 和可展开详情；未完成时标题为“首次使用：准备环境”，完成后回到“环境检查”。
- `DashboardViewModel` 复用 `EnvironmentCheckService` 与 `MessageTypes.CheckEnvironment`。进入维护页时首次未检查会运行非破坏性检查；检查覆盖 Worker、数据/存档/媒体目录、SQLite、Playnite 游戏库、Ludusavi、可选 Rclone 和磁盘空间，并返回逐项通过/警告/失败/跳过状态。
- “运行检查”只发起已有只读/探测请求；“完成设置”要求最近检查存在且 `EnvironmentCheck.IsReady`；“跳过首次检查”明确写入 `OnboardingCompleted`，并说明之后可在维护中心重新运行。首次检查卡没有强制把用户导航到维护页，`SessionNavigationState` 保留用户返回的工作区。
- “对当前游戏做测试备份”沿用 `BackupSelectedAsync` 的真实 `BackupGame` 手动管道，只有选中已匹配游戏且 Ludusavi 可用时可执行；文案明确不会自动执行，不会在引导加载时替用户启动写入。

## 2. 实际证据

- `EnvironmentCheckServiceTests.MissingOptionalRcloneIsSkippedAndDatabaseProbeIsWritable`：`1 passed / 0 failed / 0 skipped / 1 total`。使用临时隔离目录和空 Rclone/Ludusavi 配置，验证 SQLite/目录探针、可选 Rclone 跳过、库为空警告、Ludusavi 失败和 `IsReady=false`；测试结束删除隔离目录。
- `SettingsAndAutoSelectSourceTests.OnboardingTestBackupReusesProductionBackupPipeline`、`SessionNavigationStateTests.NavigationRestoreIsSessionOnlyAndStartsAtOverview`、`UiLayoutRegressionTests.MaintenanceEnvironmentChecksUsePredictableResponsiveColumns`：`3 passed / 0 failed / 0 skipped / 3 total`。`OnboardingUsesMaintenanceWorkspaceAndExplicitCommands` 因 `LegacyProductionUiBaselineFact` 在当前隔离宿主条件下跳过，未把源码断言跳过写成通过。
- 当前分支 source-copy 定向 Release 构建 Playnite `net462` / Playnite.Tests `net472`：`0 errors / 2 warnings`，仅为既有 `MediaCenterView.xaml.cs:671` `CS8602`；Worker.Tests 定向构建 `0 warnings / 0 errors`。最初直接复用旧工作树产物时 3 条源码测试被 `GscBuildCommit` 身份门拒绝，随后使用 `31784686` 固定身份隔离构建并重新执行，未把身份失败计入行为失败。
- `scripts/validate-source.py` 通过；`scripts/check-xaml.ps1` 检查 `24/24`；`git diff --check` 通过。本项是既有能力核对，没有用 `Assert.Contains` 单独签收真实引导交互、焦点或性能。

## 3. 未确认边界与安全语义

- 未运行真实 Playnite/package-host、真实外部工具、真实存档或云端；只用合成/fake/隔离目录。没有修改真实配置、存档、媒体、用户云端或对外发送诊断。
- 环境检查中的目录探针会在其收到的配置目录创建并清理临时探针文件；本阶段测试只使用隔离临时目录，未对用户真实目录执行检查。没有绕过被拒绝的 ETW/系统跟踪权限。
- 未宣称最终 presented frame、物理 DPI/跨屏、UI Automation/读屏、真实键盘/IME 或宿主性能；Demo 原目录不可用，仍以恢复的生产基线为视觉依据。维护卡窄/短区域和真实外部工具结果仍需宿主复核。
- `OnboardingCompleted` 是现有设置字段；“完成设置/跳过”会按用户明确点击持久化标记，但不会自动修改外部工具路径、目录或备份策略。main 的用户改动未碰、未合并。

## 4. 下一步

下一可执行任务为 `R20-04 零结果恢复`：先核对任务/媒体/游戏选框的筛选状态、条件摘要和错误/离线来源，确保清除条件只改筛选，不重置用户数据，也不把离线失败显示成零结果。
