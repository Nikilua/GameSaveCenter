# GameSaveCenter AI/Codex 长期项目记忆

> 维护时间：2026-08-12
> 本文件面向新的 AI/Codex 会话，目标是在几分钟内恢复项目状态，避免重复实现已完成的工作。

## AI/Codex 启动协议

开始 GameSaveCenter 开发前，请依次阅读：

1. `docs/ai/PROJECT_MEMORY.md`（本文件）
2. `docs/ai/WORKLOG.md`
3. `docs/DEVELOPMENT_HANDOFF.md`
4. `docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md`（如果存在）
5. `docs/design/UI_CHANGE_GATE.md`（如果存在）
6. `git log` 最近 15～30 个 commit
7. `git status`

然后才开始修改代码。不要仅凭历史对话假设当前项目状态；代码、文档和 Git 历史是唯一事实来源。

## 项目定位

- GameSaveCenter 是 Playnite 的 GenericPlugin，提供存档备份/恢复/校验、媒体同步、任务中心、维护中心、修改器与 CT 管理，以及新增的自定义游戏启动项能力。
- Playnite 是唯一主要 UI（WPF），后台 Worker 是独立 .NET 8 进程，两者通过 Named Pipe IPC 通信。
- `GameSaveCenter.Contracts`：Playnite/Worker 共享的 DTO、枚举、消息类型，netstandard2.0。
- `GameSaveCenter.Core`：Playnite 侧可复用逻辑（目前主要是启动/包装与少量辅助）。
- `GameSaveCenter.Worker`：持久化、Ludusavi、Rclone、媒体索引、任务编排、游戏 Session、GameTool 导入/启动/追踪。
- `GameSaveCenter.Playnite`：WPF Dashboard 外壳 + 六个 Workspace 页面 + 设置页。
- 数据持久化：SQLite（`SqliteStateStore`）+ 文件系统（存档、媒体归档、GameTools 目录）。
- 模块关系：Ludusavi 负责存档底层；Rclone 只允许 copy/check，不使用 sync/delete/purge；媒体为增量同步；GameTool 绑定在游戏级。

## 当前主要架构

### 程序集与入口
- Solution：`GameSaveCenter.sln`，版本 `0.6.70-development-preview`（`Directory.Build.props` 0.6.70）。
- 插件入口：`src/GameSaveCenter.Playnite/GameSaveCenterPlugin.cs`，扩展 ID `66e9f2d7-67bb-43ef-b62a-b8e60734fcec`。
- Worker 入口：`src/GameSaveCenter.Worker`，IPC dispatcher 为 `IpcRequestDispatcher`。
- 测试：Core 13、Worker 51、Playnite 197（2026-08-12 基线）。

### Dashboard / Workspace
- `DashboardViewModel` 是大型聚合 ViewModel（技术债，暂不拆分），持有所有 Workspace 数据与命令。
- 六个 Workspace：Overview（首页）、Saves（存档中心）、Trainers（修改器中心）、Media（媒体中心）、Tasks（任务中心）、Maintenance（维护中心）；另有 Settings 页面。
- 工作区页面位于 `Views/`：DashboardView + 各 CenterView；共享资源在 `Themes/DesignTokens.xaml`、`Themes/WpfUiProduction.xaml`、`Themes/Redesign.xaml`。
- Dashboard 视图有响应式 code-behind 协调（`DashboardView.xaml.cs`），页面级滚动面 + 主表/主列表有限视口 + 内部虚拟化滚动。

### UI-207 当前约束（2026-08-12）

- Settings 的 `SettingsScroller` 位于共享 `GscRedesignSettingsTabControl` 模板内容区；`SettingsHeaderScroller` 是分类导航区。宽屏分类栏为 232 DIP 左侧有限滚动，紧凑布局为顶部横向 `Auto`，不能把根 UserControl 再包回第二个页面滚动器。
- `GscSelectedGameIconControl` 只用于当前游戏上下文表面（Dashboard、Overview、Save、Trainer、Media），GamePicker 虚拟化列表不得加载真实 Icon。
- GamePicker 选择可被当前筛选隐藏但不能静默丢失；必须保留 `SelectedItem`、显示恢复语义并保持 `GamePickerSelectedGameId` 持久化。默认筛选只对新用户/未知值归一为“已安装”。
- 事件驱动的 `PlayniteGameStarted` 自动定位优先于普通刷新；游戏停止不改变当前选择。不得为此新增轮询、进程扫描、IPC 或网络请求，也不得改动 DataGrid 滚动/虚拟化契约。
- 本轮自动化结果：源码门禁、WPF 静态门禁、隔离 Release 构建、Core 13/13、Worker 51/51、Playnite 197/197、render-qa 均通过；真实 Playnite 宿主/DPI/主题人工验证仍待环境。

### 数据流
- Playnite → Worker：Named Pipe 请求（`GameSaveCenter.Playnite/Ipc`、`GameSaveCenter.Worker/Ipc`）。
- 任务状态：Worker `TaskCoordinator` 持久化 + `TaskEventBroadcaster` 事件流 + Dashboard 轮询兜底。
- 快照：`MessageTypes.GetDashboard` 返回 `DashboardSnapshotDto`；大库先渲染 SQLite 缓存，后台再同步。

### GameTool 模型
- `GameToolType`：Trainer / CheatTable / CustomExecutable（自定义启动项）。
- `GameToolDto` + `GameToolVersionDto`：DisplayName、Enabled、AutoStart、LaunchTiming、LaunchDelaySeconds、CloseOnGameExit、RequiresAdmin、ActiveVersionId、EntryPath、WorkingDirectory、Arguments、ResolvedTargetPath 等；`game_tool_versions` 已补 `resolved_target_path` 兼容列。
- Worker `GameToolService`：导入（Trainer/CT 复制进 GameTools 目录；自定义启动项默认保留外部路径引用）、更新、删除、启动、随游戏自动启动/延迟/关闭追踪。
- Session 追踪：`GameToolSessionTracker`（SessionId → PID + 实际 StartTime + CloseOnExit），关闭时要求 PID 与实际 StartTime 双向匹配，禁止按进程名杀。

### 任务系统
- `TaskCoordinator` 统一编排；`TaskStatusDto` 有 Progress/Message/ErrorCode/ErrorMessage/State/时间戳。
- Dashboard `TaskIndexedCollection` 按 TaskId 索引增量合并；`knownTaskStates` 去重通知。

### 媒体系统
- `MediaItemDto` 由 Worker 索引；列表与详情预览已改为 `AsyncThumbnailImage` 异步加载（`Task.Run` 强制后台、3 并发、LRU 96、Freeze 后回 UI、Unloaded 取消）；`MediaThumbnailConverter` 保留为兼容实现。
- Media 列表使用 ListBox + Recycling 虚拟化；页面滚动面与列表滚动分工明确。

### 缓存与性能机制
- `BatchObservableCollection<T>`：批量 Replace 只发一次 Reset（默认引用相等比较；PERF-005 起支持内容比较器跳过未变化）。
- GamePicker 有 180ms 搜索防抖、按 PlayniteId 缓存 `GamePickerItem`、平台指纹短路。
- Task 筛选指纹短路（`ComputeTaskFilterFingerprint`）、平台指纹短路（`ComputePlatformFingerprint`）。
- Dashboard 大库 cache-first + 延迟后台同步；`[PERF]` 日志设施见 `docs/ai/PERFORMANCE_BASELINE.md`。

## UI 设计原则

- 目标是 Apple-inspired 的原生 WPF 桌面工具：清晰层级、克制毛玻璃、圆角、统一设计令牌、自然微动效、深浅色、跟随 Playnite、高对比度、DPI 适配、响应式布局、不使用突兀的原生控件视觉。
- 所有 UI 修改必须先读 `docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md` 与 `docs/design/UI_CHANGE_GATE.md`，并遵循 `.codex/skills/wpf-apple-desktop-ui/SKILL.md`。
- 常用窗口下限 1040×700 DIP；1080p/2K/4K 必须按 DPI 换算后的逻辑 DIP 检查全屏、窗口化、最大化；不把 4K 通过当作 1080p 通过。
- 页面级滚动只承载有限测量内容；DataGrid/ListBox 保留 236 DIP 最小视口、内部滚动和虚拟化；堆叠 Inspector 下限 160 DIP。
- 动态下拉框必须显示逻辑默认值（如“全部”）；TaskCenter 游戏/类型筛选通过 `TaskFilterOptionsSync` 增量同步，`全部` 稳定保留在 index 0，不再 Clear/Replace 集合。
- GamePicker 新用户默认筛选为“已安装”，已有明确配置值必须保留；Dashboard 打开时运行中游戏优先，否则恢复上次选择，普通刷新不得抢回用户手动选择。

## 已完成的大型重构 / 优化

- UI-001～UI-205、SKILL-001、QA-001～005：页面 Workspace 化、响应式断点、滚动分工、Inspector 下限、筛选默认值、离屏渲染 QA。
- UI-207（2026-08-12）：设置页 Header 不裁剪与分类栏滚动（920 DIP 断点）、运行中游戏自动定位、上次选择持久化复用、GamePicker 新用户默认“已安装”、当前游戏真实 Playnite Icon（事件驱动，无轮询/无网络，LRU 48）。
- `scripts/render-qa.ps1` + `tests/GameSaveCenter.RenderHarness`：7 页面 × 5 常用窗口离屏渲染回归，含自动失败门禁。
- PERF-001：`BatchObservableCollection` 批量 Reset。
- PERF-002/003：Task 筛选与 GamePicker 平台指纹短路。
- PERF-004（旧编号）：GamePickerItem 缓存复用（新任务编号体系中 PERF-004 是性能基线设施，不要混淆）。
- PERF-004/005/006（新编号）：`[PERF]` 基线日志、Snapshot 无变化 0 Reset、Task/Media 搜索防抖。
- PERF-007：媒体缩略图异步化（`AsyncThumbnailLoader` Task.Run 后台解码 + 3 并发 + LRU + Freeze + `[PERF]` 埋点，`AsyncThumbnailImage` 占位加载并 Unloaded 取消）。
- PERF-009/010：任务事件合并 TaskId 索引 O(1) 更新；命令状态刷新 Dispatcher 合帧。
- GAME-TOOL-001/002：自定义启动项正式支持 EXE/LNK/BAT/CMD/PS1，外部路径引用不复制文件；Session 级 PID 追踪与 CloseOnGameExit 安全关闭。
- UI-204/205：TaskCenter 与 GamePicker 下拉框默认值恢复（含真实 Playnite 异步物化重试）。
- UI-206（含回滚）：DataGrid 滚动几何修复。初版 `Pixel ScrollUnit` 经真实 Playnite A/B 验证会严重恶化空白，已撤回；最终采用 `Item` + `GscStableDataGridRow` 稳定行样式 + geometry probe（60 行 × 非整行高度，gap ≤4 DIP、末行完整、无跳变、Recycling 保持）；诊断摘要取消外层裁剪并由页面滚动负责可达性。

## 当前技术债

- `DashboardViewModel` 仍很大，包含命令、筛选、导入、诊断、设备状态等职责；只有性能实现被严重阻碍或 GAME-TOOL 无法接入时才拆（独立 `ARCH-xxx` 任务）。
- `DashboardView.xaml.cs` 仍承担部分响应式协调。
- 媒体列表/详情缩略图已异步化；真实大量截图滚动下的帧率仍需真机验证。
- 真实 Playnite 宿主、主题切换、DPI 真机、连续缩放流畅性尚未完整人工验收（UI-QA-REAL-001 仅完成冒烟）。

## 当前开发优先级

- P0：性能基础设施与真实热点优化（PERF-004～007、009/010 已完成）。
- P0：自定义游戏启动项（已完成，GAME-TOOL-001/002）。
- P1：媒体性能（PERF-007 异步缩略图，已完成）。
- P1：真实 Playnite / DPI / 大型游戏库 QA（UI-QA-REAL-001 冒烟已完成，完整人工验收待用户）。
- P2：架构进一步拆分（不主动做）。
- PERF-008：已评估收口，维持现状。详情已按激活 Workspace 分支加载，全量快照仅用于全局摘要且后台有 1 分钟 TTL；2000 规模合成 profiling 无 O(n^2)，待真实大库渲染 profiling 证明瓶颈后再评估。

## 2026-08-12 可靠性阶段补充

- `RELIABILITY-RESTORE-001` 已实现：备份历史版本支持非破坏性的恢复可用性检查，结果持久化在 `backup_versions.restore_readiness_json`，检查过程只在应用数据目录隔离提取，不接触真实存档目录。
- Ludusavi 备份版本的 `backupPath + backup ID` 已持久化为 `backup_versions.archive_path`；Simple 归档、缺失/损坏 ZIP、路径穿越、超大展开量、不一致统计与不支持压缩方式必须返回明确状态。
- 恢复可用性入口位于现有 Save Center 历史 Inspector，不能新建页面或改变 `SaveHistoryGrid` 的滚动/虚拟化骨架；新增内容必须留在 `SaveHistoryActionsScrollViewer` 内，并继续通过 `render-qa` 验证 1040×700 等窗口。
- 当前测试基线为 Core 13、Worker 58、Playnite 197；本阶段源码门禁、WPF 静态门禁、隔离渲染 QA 和 Release 构建均已通过。真实 Playnite 宿主、主题/DPI 人工验收仍待用户环境确认。
- 下一项按附件顺序为 `RELIABILITY-HEALTH-001`；继续采用小阶段、独立测试、更新记忆/工作日志、独立 commit，并 push 到 `origin/main`。

已完成：见 WORKLOG.md 与 Git log；不要重复实现已完成的 UI/性能工作。

## 已知坑

- WPF `ICollectionView.Refresh()` 昂贵；不要在每个按键或每次快照都调用。
- `ObservableCollection` Reset 仍会触发 CollectionView 重建；数据没变时应跳过（PERF-005）。
- 动态 ComboBox Items 重建会清空 SelectedItem；要显式恢复逻辑默认值。
- 大库启动不要同步全量匹配/扫描；先渲染 SQLite 缓存。
- Worker 是独立进程：Playnite 启动早期 IPC 可能超时，要用失败快速降级 + 后台重试。
- 修改器/CT/自定义工具启动一律走 Worker；禁止在 Playnite UI 进程直接 Process.Start 外部程序。
- CloseOnGameExit 只能关闭本 Session 由 GameSaveCenter 启动且能确认 PID/StartTime 的进程；脚本（BAT/CMD/PS1）与系统默认程序打开的文件不可靠，UI 对这类入口禁用开关。
- 自定义启动项支持 EXE/LNK/BAT/CMD/PS1/普通文件：EXE 与导入/重定位时已解析并持久化的 LNK→EXE 目标可跟踪；未解析的 LNK、脚本和系统默认程序启动时 Trackable=false。
- 磁盘 IO、图片解码不要放 UI 线程；图片解码要限制并发并 freeze。
- 表格/列表虚拟化很容易被外层 ScrollViewer 或 DataGrid 嵌套破坏，改 XAML 后必须跑 render-qa。
- DataGrid 不要写死运行时 `Height`，用 `MinHeight/MaxHeight` 保持有限 viewport；`Pixel ScrollUnit` 已在真实 Playnite 验证会回归（轻微滚动即大空白），当前必须保持 `Item` + 稳定行样式，禁止重新改回 Pixel。
- `git push` 前确认没有 bin/obj、用户本地配置、密钥、测试临时文件和大压缩包（如 `GameSaveCenter.7z` 不要提交）。

## 文档导航

- `docs/DEVELOPMENT_HANDOFF.md`：跨电脑/跨模型交接入口，包含每轮 UI 基线。
- `docs/PROJECT_MEMORY.md`：长期不可丢失约束与 UI 决策历史（大文件，按章节检索）。
- `docs/DEVELOPMENT_PROGRESS.md`：按 UI 编号的实施历史与下一步线索。
- `docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md`、`docs/design/UI_CHANGE_GATE.md`：UI 方向与门禁。
- `.codex/skills/wpf-apple-desktop-ui/SKILL.md`：WPF/Playnite UI 专项技能。
- `docs/ai/WORKLOG.md`：每阶段开发流水记录。
- `docs/ai/PERFORMANCE_BASELINE.md`：性能基线与测量方法。
