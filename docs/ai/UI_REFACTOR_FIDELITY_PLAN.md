# GameSaveCenter UI 重构功能保真计划

> 生成时间：2026-08-14
> 基线 Commit：`4ab44fe`
> 事实来源优先级：当前生产 main > UI Audit（`4ab44fe` 产物）> 实施包 v1 锁定/范围 > WPF Demo v6.1 > 旧布局。

## 1. 绝对锁定区域

| Route | 区域 | Action | 约束 |
|---|---|---|---|
| dashboard | 顶部全局 GamePicker（GameBrowserPanel / CompactGameSelector / GamePicker 全部行为） | `LOCKED` | 不删除、不复制、不替换、不改样式/模板/Binding/Command/event/搜索/筛选/排序/运行中定位/持久化/图标/虚拟化/性能策略 |
| dashboard | 一级导航路由、顺序、当前导航行为 | `KEEP` | 六个工作区顺序与语义不变 |
| overview | 今日工作台 / TODAY / 当前游戏 | `KEEP` + 轻度 `RESTYLE` | 只修间距、对齐、换行、响应式；不重做信息架构 |
| trainer | 导入确认流程 | `KEEP FLOW` | 只允许视觉、间距、窗口适配修正 |
| save-center | 备份策略业务结构 | `KEEP` | 字段、模板、自动备份、媒体/云端、安全边界、保留预览全部保留 |
| settings | 五个设置分区与业务字段 | `KEEP` | 本轮只做主题一致性、间距、输入控件、小窗口可达性 |
| dev-probe | Development UI probe | `NO CHANGE` | 除非共享 Style 编译失败需要最小兼容修正 |

## 2. 全量功能映射总则

所有生产元素只允许以下 Action：

- `KEEP`：位置/结构基本保留。
- `MOVE`：移动到新分区。
- `RESTYLE`：只改视觉/尺寸/间距。
- `COLLAPSE`：默认折叠，仍可访问。
- `RESPONSIVE_MOVE`：仅在 Compact/Narrow 时进入 Drawer/更多区域。

默认禁止 `REMOVE`。任何无法归类元素先 `KEEP`。

## 3. 逐页映射与实施范围

### 3.1 Dashboard Shell

可改：GamePicker 下方 Workspace 容器 Row/Grid 结构、页面内容统一 Padding、响应式宽/高状态传递、防止页面内容越过 Header/导航。

不可改：GamePicker、一级导航、Workspace 路由。

### 3.2 Overview 首页

| 旧元素 | Type | Command/Binding | Action | 新位置 |
|---|---|---|---|---|
| 今日工作台标题/副标题 | Section | - | `KEEP/RESTYLE` | 保持首页顶部 |
| TODAY Hero | Section | Snapshot 状态 | `KEEP/RESTYLE` | 保持 Hero |
| 当前游戏卡 | Section | BackupSelected / LoadDetails / OpenAttentionCenter | `KEEP/RESTYLE` | 保持当前游戏卡 |
| 六项状态指标 | Cards | Snapshot | `RESTYLE` | 压成紧凑 Summary Strip，信息全留 |
| 最近活动 | ListBox | Refresh / SelectedTask | `KEEP` | 保留，不再成为独立长滚动 |
| 风险与提醒 | Section | OpenAttentionCenter / OpenProtectionGames / ApplyRecommendedProtection | `KEEP` | 保留 |
| 最近 30 天玩过的游戏 | Section | OpenProtectionItem / ApplyRecommendedProtection | `COLLAPSE`（明细） | 汇总保留，明细 Expander |
| 全局活动 | ItemsControl | Activities | `KEEP` | 保留，稳定列宽 |
| 页面滚动 | 4 个 ScrollViewer | - | `MOVE` 收敛 | 下半页统一一个明确页面滚动上下文 |

禁止合并删除“最近活动”与“全局活动”。

### 3.3 Save Center 存档中心

顶级 4 Tab 锁定：历史版本 / 路径与校验 / 备份策略 / 比较与保留。

| Tab | 关键生产元素 | Action | 新位置 |
|---|---|---|---|
| 历史版本 | SaveHistoryGrid 7 列、备注/锁定、比较、恢复就绪、安全恢复、撤销 | `KEEP/MOVE` | 主 DataGrid + 右侧稳定 Inspector |
| 路径与校验 | SaveCandidateGrid 4 列、扫描/校验/刷新/接受/忽略 | `KEEP/MOVE` | 主 DataGrid 优先剩余高度；Inspector 窄窗收起 |
| 备份策略 | 模板、模板参数、自动化、媒体/云端、安全边界、保留预览 | `COLLAPSE`（模板区） | 单页滚动；模板高级编辑进 Expander |
| 比较与保留 | CompareBackup / PreviewRetention | `MOVE` | 内部二级 Tab 或并列区域，避免纵向堆叠 |

已知重点：`SaveCandidateGrid` 全尺寸约 3.7 行，必须优先提升到 1040×700 至少约 4 行、Standard/Wide 至少 6 行以上。

### 3.4 Trainer Center 修改器中心

顶级 4 Tab 锁定：已绑定工具 / 导入确认 / FLiNG 在线库 / 可下载版本。

| Tab | 关键生产元素 | Action | 新位置 |
|---|---|---|---|
| 已绑定工具 | 导入修改器/目录/CT/自定义启动项、工具列表、全部编辑字段与动作 | `KEEP/MOVE` | 左工具列表 + 右 Inspector；窄窗详情入口 |
| 导入确认 | TrainerImportEntryComboBox、确认/取消 | `KEEP` | 流程不变，只统一视觉 |
| FLiNG 在线库 | 搜索、刷新目录、读取版本 | `KEEP` | 搜索区 Auto，结果区 `*` |
| 可下载版本 | 版本列表、下载并绑定/仅下载、版本详情 | `KEEP/MOVE` | 左版本列表 + 右 Inspector |

### 3.5 Media Center 媒体中心

顶级 3 Tab 锁定：待归类 / 当前游戏媒体 / 来源规则。

| Tab | 关键生产元素 | Action | 新位置 |
|---|---|---|---|
| 待归类 | MediaInboxGrid 5 列、目标游戏、归类、忽略 | `KEEP/MOVE` | 主 DataGrid + Inspector |
| 当前游戏媒体 | 搜索、筛选、评论、更新元数据、打开、目录、重新归类、收藏/取消收藏 | `KEEP/MOVE` | 主媒体列表 + Inspector；窄窗详情入口 |
| 来源规则 | 目录/模式/共享开关、添加来源、来源列表、启用/移除 | `COLLAPSE`（添加表单） | “添加来源”默认折叠，来源列表获得主高度 |

收敛 `MediaCurrentScrollSurface` 与 `MediaGrid` 的双重纵向滚动。

### 3.6 Task Center 任务中心

| 旧元素 | Action | 新位置 |
|---|---|---|
| 搜索 / 状态 / 游戏 / 类型四筛选 | `KEEP` / `RESPONSIVE_MOVE` | 主筛选行；矮窗“游戏”等次级筛选进“更多筛选” |
| TaskGrid 六列 | `KEEP` | 页面无外层纵向滚动，表格 `*` 占剩余高度 |
| 复制详情 / 安全重试 / 取消任务 | `KEEP` | Inspector |
| 摘要四卡 | `RESTYLE` | 保留紧凑摘要 |

已知重点：`TaskDetailActions` 在 narrow 曾 114 DIP，避免按钮竖向膨胀。

### 3.7 Maintenance Center 维护中心

顶级 5 Tab 锁定：诊断 / 设备状态 / 保留策略 / 异常与审计 / 进程映射。

| Tab | 关键生产元素 | Action | 新位置 |
|---|---|---|---|
| 诊断 | 环境检查、测试备份、完成/跳过首次、刷新/复制/导出诊断、健康报告、目录/日志、完整性、灾备、重建索引、协调任务、安全模式、路径迁移、诊断摘要、FindingsGrid 5 列 | `COLLAPSE`（低频/危险工具） | 常用操作直接显示；目录/日志、完整性/自愈/安全模式、灾备、路径迁移、首次检查进 Expander；FindingsGrid 主内容 |
| 设备状态 | 6 列、同步设备摘要、决策、备注、暂存/恢复远端 | `KEEP/MOVE` | 主 DataGrid + Inspector；窄窗收起 |
| 保留策略 | 当前游戏预览、存储分析、全局模拟器、第二本地镜像 | `MOVE` | 内部二级 Tab，四块业务全保留 |
| 异常与审计 | FindingsGrid + AuditLogGrid | `MOVE` | 内部二级 Tab，一次显示一张主表 |
| 进程映射 | 输入、目标游戏、绑定、删除、三列表格 | `KEEP` | 主 DataGrid + Inspector |

已知重点：`MaintenanceAuditLogGrid` 约 1.6–1.9 行，必须通过二级 Tab 消除；`MaintenanceDeviceGrid` / `MaintenanceProcessGrid` narrow 约 3.7 行必须改善；诊断 13 工具 narrow 138 DIP 按钮墙必须消除。

### 3.8 Settings 设置

5 Tab 锁定：常规与目录 / 备份与恢复 / 外观与可访问性 / 自动化与媒体 / 设置迁移。

仅做：主题 token、间距、输入控件统一、小窗口滚动、Tab 可用性。不重组设置业务信息架构。

## 4. Command Coverage

以下 92 条命令来自 `UI_MANIFEST.json`（commit `4ab44fe`）。本轮计划 `Missing = 0`；换位置不等于删除。

### Overview

- RefreshCommand
- BackupAllCommand
- SyncMediaCommand
- OpenAttentionCenterCommand
- BackupSelectedCommand
- LoadDetailsCommand
- OpenProtectionGamesCommand
- ApplyRecommendedProtectionCommand
- DataContext.OpenProtectionItemCommand
- DataContext.OpenAttentionFindingCommand

### Save Center

- UpdateBackupMetadataCommand
- CompareBackupCommand
- ValidateRestoreReadinessCommand
- RestoreCommand
- UndoRestoreCommand
- DetectPathsCommand
- ValidateCommand
- AcceptCandidateCommand
- RejectCandidateCommand
- SavePolicyCommand
- CreatePolicyTemplateCommand
- SavePolicyTemplateCommand
- ApplyPolicyTemplateCommand
- DeletePolicyTemplateCommand
- PreviewRetentionCommand

### Trainer Center

- ImportTrainerCommand
- ImportToolFolderCommand
- ImportCheatTableCommand
- ImportCustomLaunchItemCommand
- ConfirmGameToolImportCommand
- CancelGameToolImportCommand
- LaunchGameToolCommand
- SaveGameToolCommand
- OpenGameToolDirectoryCommand
- RelocateGameToolCommand
- DeleteGameToolCommand
- SearchTrainerCatalogCommand
- SyncTrainerCatalogCommand
- DataContext.LoadTrainerReleasesCommand
- DownloadTrainerCommand

### Media Center

- AssignInboxMediaCommand
- IgnoreInboxMediaCommand
- UpdateMediaMetadataCommand
- OpenSelectedMediaCommand
- RevealSelectedMediaCommand
- ReassignMediaCommand
- FavoriteSelectedMediaCommand
- UnfavoriteSelectedMediaCommand
- CommentSelectedMediaCommand
- AddMediaSourceCommand
- DataContext.DeleteMediaSourceCommand

### Task Center

- CopyTaskErrorCommand
- RetryTaskCommand
- CancelTaskCommand

### Maintenance Center

- RunEnvironmentCheckCommand
- OnboardingTestBackupCommand
- CompleteOnboardingCommand
- SkipOnboardingCommand
- RefreshDiagnosticsCommand
- CopyDiagnosticsCommand
- CreateDiagnosticsPackageCommand
- CopyMaintenanceReportCommand
- ExportMaintenanceReportCommand
- OpenDataDirectoryCommand
- OpenBackupDirectoryCommand
- OpenMediaDirectoryCommand
- OpenWorkerLogCommand
- RunIntegrityCheckCommand
- CreateMetadataBackupCommand
- RestoreMetadataBackupCommand
- RebuildRepositoryCommand
- ReconcileTasksCommand
- ExitSafeModeCommand
- RunPathRemapCommand
- SyncDeviceStatesCommand
- SaveDeviceDecisionCommand
- StageRemoteBackupCommand
- RestoreStagedRemoteBackupCommand
- PreviewRetentionCommand
- RefreshStorageAnalysisCommand
- RefreshRetentionSimulationCommand
- ApplyRetentionSimulationCommand
- RefreshLocalMirrorStatusCommand
- SyncLocalMirrorCommand
- SaveProcessMappingCommand
- DataContext.DeleteProcessMappingCommand
- DeleteProcessMappingCommand

## 5. DataGrid Column Coverage

以下 43 列全部保留，`Removed = No`。

| Grid | Columns |
|---|---|
| SaveHistoryGrid | 时间、类型、文件数、大小、设备、备注、状态 |
| SaveCandidateGrid | 可信度、状态、路径、依据 |
| MediaInboxGrid | 拍摄时间、类型、来源、文件、原因 |
| TaskGrid | 本地时间、任务、游戏、状态、进度、详情 |
| FindingsGrid | 等级、游戏、标题、详情、建议处理 |
| MaintenanceDeviceGrid | 状态、游戏、其他设备、原因、人工决策、建议 |
| MaintenanceAuditFindingsGrid | 等级、游戏、标题、详情 |
| MaintenanceAuditLogGrid | 时间、分类、消息 |
| MaintenanceProcessGrid | EXE、目标游戏、操作 |

## 6. Scroll / Inspector Coverage

30 个现有 ScrollViewer 全部保留或收敛到一个明确语义滚动上下文，不删除可达性：

- Dashboard：HeaderScrollViewer、SidebarNavigationScrollViewer、TopActionsScroller。
- Overview：OverviewStackScrollSurface、OverviewPrimaryScrollSurface、OverviewSecondaryScrollViewer、OverviewRiskScrollViewer。
- Save Center：SaveHistoryActionsScrollViewer、SaveCandidateInspectorScrollViewer、SavePolicyStack 页面滚动、SaveCompareMainScrollViewer、SaveCompareRetentionScrollViewer。
- Trainer Center：TrainerToolsSettingsScrollViewer、TrainerReleaseInfoScrollViewer。
- Media Center：MediaInboxScrollSurface、MediaCurrentScrollSurface、MediaInspectorScrollViewer、来源规则页面滚动。
- Task Center：TaskPageScrollSurface、TaskDetailScrollViewer。
- Maintenance：MaintenanceDiagnosticsScrollSurface、MaintenanceDiagnosticsInspector、MaintenanceDeviceScrollSurface、MaintenanceDeviceInspectorScrollViewer、保留策略页面滚动、MaintenanceAuditScrollSurface、MaintenanceAuditInspector、MaintenanceProcessScrollSurface。
- Settings：SettingsHeaderScroller、SettingsScroller。

## 7. 条件 UI 与表单字段

Audit 基线共有 143 个条件 UI、38 个 TextBox/PasswordBox、25 个 ComboBox、3 个 CheckBox、621 个 TextBlock。本轮全部按“原语义保留”，条件绑定不改；只允许移动/折叠/响应式移动。完整逐项明细以 `UI_MANIFEST.json` 为机器可查来源，最终提交前用 Audit 再跑 before/after 对比。

## 8. 折叠边界

适合 `COLLAPSE`：高级工具、灾备、恢复/危险操作、批量迁移、长说明、二级诊断结果、低频筛选器、模板高级编辑、添加来源、首次引导完成后的说明。

不默认隐藏：页面核心状态、主要任务入口、当前对象名称/状态、主 DataGrid/List、高频刷新/执行、当前错误/风险摘要、当前游戏上下文。

## 9. 分阶段计划

| Phase | 范围 | 提交要求 |
|---|---|---|
| 0 | 基线验证 + 本保真计划 | 独立提交 |
| 1 | 共享布局基础：Section/token、Inspector、Expander、Toolbar overflow、响应式宽/高状态、主题资源 | 独立提交 |
| 2 | Overview 首页 | 独立提交 |
| 3 | Save Center | 独立提交 |
| 4 | Trainer + Media（分页提交） | 每页独立提交 |
| 5 | Task Center | 独立提交 |
| 6 | Maintenance Center（诊断→设备→保留→审计→进程，分步） | 每步独立提交 |
| 7 | Settings 轻量统一 | 独立提交 |
| 8 | 最终回归：Audit before/after、render QA、测试、真实宿主验证记录 | 独立提交 |

每阶段 Gate：Core / Worker / Playnite 测试、`validate-source.py`、WPF UI validator、`render-qa.ps1`、`git diff --check`，如涉及 DataGrid 滚动回归先停后续阶段。

## 10. Open Questions

- 任何 Audit 有但 Demo 未画的元素：按生产优先，先 `KEEP`。
- 不确定是否可折叠：先 `COLLAPSE` 但保持 1～2 次操作可达。
- 不确定是否可合并：不合并业务语义。
- 所有不确定项记录到 `docs/ai/UI_REFACTOR_OPEN_QUESTIONS.md`，不自行做不可逆信息架构决定。

## 11. Phase 状态与 After Audit

| Phase | 范围 | 状态 | Commit |
|---|---|---|---|
| 0 | 基线 + 保真计划 | 完成 | `85e3d71` |
| 1 | 共享布局基础 | 完成 | `a4deb1e` |
| 2 | Overview | 完成 | `283342f` |
| 3 | Save Center | 完成 | `c7bc847` |
| 4a | Trainer Center | 完成 | `258c67b` |
| 4b | Media Center | 完成 | `e84c904` |
| 5 | Task Center | 完成 | `d6701cf` |
| 6 | Maintenance Center | 完成 | `c29d831` |
| 7 | Settings | 完成 | `5f9fca5` |
| 8 | 最终回归 | 完成 | 当前提交 |

### After Audit（扩档后的最终基线）

- 静态 View 9、Tab 30、Button/ToggleButton 131、DataGrid 9、ScrollViewer 30、条件 UI 143、运行时快照 161（新增 2K 与 1100×720）、失败路由 0。
- HIGH：Before 10 项（候选表、审计日志、设备、进程等）→ After **0 项**。
- MEDIUM：Before 4 项 → After **0 项**（最后一项为 Overview“当前游戏”操作行未命名 WrapPanel 92 DIP，已通过按钮底部间距 8→4 DIP 收口）。
- 运行时警告：Before 62 → After 39；Audit 的工作区高度参数已与生产 Dashboard 和 render-qa 对齐（传窗口高度而非内容高度），避免在 1100×720 等尺寸下误报维护表过矮。
- 已知 INFO：仍有语义明确的 Master/Detail 与页面滚动并存，但不构成 HIGH；DataGrid 虚拟化与内部滚动全部保留。
