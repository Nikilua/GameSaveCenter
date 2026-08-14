# GameSaveCenter 持续维护交接与开发入口

> 这是 GameSaveCenter 的跨电脑、跨模型持续维护入口。任何新的 agent、模型或开发者接手前，先完整读取本文件，再读取项目记忆、开发进度和 UI 规则。不要只依赖聊天记录。

## 2026-08-14 Final Code Gap Closure 与最终 Epic 状态

- 当前基线：Core `59/59`、Worker `190/190`、Playnite `235/235`；Release 构建 0 warnings / 0 errors；source、XAML、WPF 静态门禁和 `render-qa` 通过；fault-injection 与 1000 轮 soak 通过。
- 已交付：多设备强内容指纹/保守分歧判断、可取消的流式 Restore Readiness、按实际目录分盘检查、重复 Manifest 防护、结构化且集中脱敏的诊断包、`ImportantOnly/Summary/Verbose` 通知级别、带启动失败自动请求与恢复正常按钮的安全模式、覆盖孤儿归档/Manifest/磁盘空间/四态结果的完整性自检、旧版数据库 Fixture 升级 Harness、自身元数据灾备 ZIP 与安全恢复流程、带只读预览与确认的备份仓库索引重建、带预览/目标缺失策略/自动灾备的批量路径迁移、按 Worker 会话与任务类型分类的中断任务协调、带显式兼容矩阵的单游戏操作锁、带 AppVersion 与能力列表的 IPC protocol handshake、覆盖设置/媒体/元数据恢复/启动计数且旧文件完整保留的共享原子写入器、数据规模与资源监控 Soak Harness、覆盖 ZIP/数据库/原子写/外部进程/任务/广播/锁的 15 类故障注入 Harness、通知级别正式收口与同 Session 去重测试、未分类自定义启动项反作弊授权语义、Onboarding 测试备份真实生产链路审计、只读备份存储分析、全局保留策略模拟器、第二本地镜像、首页全局活动时间线、Playnite 游戏右键快捷操作、修改器中心拖拽导入、合理 UI 状态持久化、键盘快捷搜索与自动化名称、统一页面状态控件、设置即时验证摘要、用户可读维护健康报告（复制/导出 TXT/Markdown）、本轮四项缺口（Repository Rebuild 空库灾难恢复、元数据灾备含 Playnite 插件设置、Workspace 状态控件覆盖存档/工具/媒体/维护、Local Mirror SHA256 内容校验），以及 Metadata 跨进程原子回滚与状态控件 TargetType 崩溃修复。
- 真实开发安装已通过：Playnite 扩展 `0.6.70` 加载，Worker 从当前扩展目录启动并记录 `Application started`。安装器仍默认不请求管理员权限；沙箱无法写入 Playnite 扩展目录时，需在正常用户环境或获授权外部环境运行安装器。
- 当前阶段 commit：`cbd0cdb`、`c6bfda2`、通知级别/安全模式/完整性自检/迁移 Harness/元数据灾备/索引重建/路径迁移/任务协调/操作锁/握手提交见紧随其后的 commit；原子写入提交为 `f1eda41`，稳定性测试提交为 `8132419`，故障注入提交为 `f91992e`，A-HARDEN 提交为 `fbbdb90/8d6c36c/5666944`，B01+B02 升级提交为 `17e88b6`，B03 提交为 `a064108`，B04 提交为 `55f532f`，B05 提交为 `97b06b5`，B06 提交为 `dff0cf4`，B07 提交为 `bddcfdd`，B08 提交为 `47685bd`，B09 提交为 `7dba09c`，B10 提交为 `7ee738d`，B11 提交为 `eed946c`，B12 提交为 `c9e053a`，B13 提交为 `738f339`，Layer B Audit 提交为 `25b191f`，C01 提交为 `2ef8f13`，C02 提交为 `a27f430`，C03 提交为 `387149f`，C04 提交为 `7f38b15`，C05 提交为 `15ab1d7`，C06 提交为 `5d113c3`，C07 提交为 `e4513cb`，C08 提交为 `c907983`，C09 提交为 `f21cba4`，C10 提交为 `4614bb8`，C11 提交为 `9d465d2`；Final Code Gap Closure 提交为 `e58714c`、`0bcdce2`、`84f74fc`、`5a12d1e`；崩溃修复与 Metadata 原子回滚提交为 `a417b7c`、`13f21a5`。Layer B 13 项与 Layer C 11 项已全部交付；`PRODUCT_HARDENING_LAYER_C_AUDIT.md` 与 `PRODUCT_HARDENING_EPIC_FINAL_AUDIT.md` 已生成。
- 整体 Epic 状态：`PARTIALLY COMPLETED / MANUAL QA REQUIRED`；按 `docs/ai/FINAL_MANUAL_QA_CHECKLIST.md` 执行真实 Restore/Undo、Rclone、双设备、外置镜像、启动项、反作弊、主题/DPI/连续缩放与 1000+ 游戏库人工验收。
- `AUTO VERIFIED` 与 `MANUAL QA REQUIRED` 必须分开记录。真实 Restore/Undo、Rclone 断网、双设备、EXE/LNK/BAT/PS1、1000+ 游戏库、主题/DPI/连续缩放仍需人工验收。

## 2026-08-13 UI-QA-REAL-006 设置分类 Tab 实际裁切修复

- 上一轮分类栏底部安全区只改变了外层 extent，未改变 `TabItem` Chrome 贴住 `TabPanel` 布局槽的问题；真实截图中的每张分类卡底边仍像被一条水平边界切平。
- 共享 `GscRedesignSettingsTabItem` 现使用不裁切的 `TabItemRoot` + 独立圆角 Chrome，Chrome 顶部对齐并留出 2 DIP 底部安全距离，移除 Chrome 的 `ClipToBounds=True`。
- `GscRedesignSettingsTabControl` 现用 `SettingsHeaderBottomSafetyZone` 真实占位元素增加分类滚动内容 extent；顶部横向模式折叠占位元素。RenderHarness 会检查最后 Tab 与 Chrome 的底部几何及安全距离。
- 已验证：XAML/source 门禁、Playnite `210/210`、设置页五种窗口 `render-qa OK`；仍需真实 Playnite 宿主主题/DPI/连续缩放人工验收。

## 接手时的最短指令

以后可以直接对新的 agent 说：

```text
请先读取 GameSaveCenter 项目的 docs/DEVELOPMENT_HANDOFF.md，按照其中的读取顺序、不可丢失约束和验证要求恢复项目上下文。当前代码侧已收口：除非用户提供新的真实问题、日志或明确新需求，否则不要主动开启新的 UI 重构或性能优化；不要重置或覆盖已有改动，先检查 `git status`，完成后更新项目记忆与工作日志并提交 commit。
```

## 2026-08-13 DEV-INSTALL-007：兼容未发现 Playnite 路径

- 另一台机器的一键安装日志显示，失败发生在构建前：Playnite 使用自定义/便携安装路径时没有发现任何 `Playnite.DesktopApp.exe`，空候选数组绑定到 `TrustedPlayniteExecutables` 后被 PowerShell 拒绝；不是编译失败。
- 当前安装器增加 App Paths、PATH 和规范化路径发现，并允许可信 Playnite 候选为空。没有运行中的 Playnite 时继续构建、打包和安装；安装完成后只提示无法自动启动 Playnite。可使用 `-PlayniteExecutable "D:\\实际路径\\Playnite.DesktopApp.exe"` 支持自定义路径。
- 如果 Playnite 仍在运行但路径不可确认，安装器仍安全停止并要求手动退出或显式指定路径，不按进程名强杀未知进程。
- 根目录入口修订号为 `DEV-INSTALL-007`；真实另一台机器的便携版、自定义路径和完全未启动 Playnite 场景仍需用户手工验证。

## 2026-08-13 UI-QA-REAL-005：首页与设置页几何兼容性修复

- 首页宽屏右侧 `今日概览` 已在 XAML 与响应式代码双重设为顶端对齐；离屏几何探针在 1600/1920 DIP 宽屏下 `OverviewSecondaryTopDelta=0`。
- Hero/当前游戏宽屏比例调整为 `1.1* + 0.9*`，离屏报告当前游戏卡相对 Hero 约 `0.82`，改善 4K/大窗口下按钮与指标的拥挤；没有改变业务命令、Binding 或窄屏堆叠阈值。
- 设置分类共享模板增加 `SettingsHeaderItemsHost` 底部安全区，且分类 Chrome 使用像素对齐和布局取整；末项滚动到底部后仍完整落在 viewport 内，五个分类都保持可见/可滚动。
- RenderHarness 在捕获设置图前将 `SettingsShell.Opacity` 设为 1，避免入口动画未由真实宿主触发时生成空白图；新增三类几何门禁。
- 当前验证基线：Release 隔离构建 0 warnings/0 errors；Core `42/42`、Worker `117/117`、Playnite `210/210`；五种常用窗口的 `render-qa` 全绿。真实 Playnite 宿主 Light/Dark/Follow、高对比度、100%–200% DPI 和连续缩放仍需人工验收。

## 2026-08-13 DEV-INSTALL-006：无窗口 Playnite 残留回收

- 真实复现 PID 48188：Playnite 已无主窗口但进程残留，`CloseMainWindow()` 无法请求退出，旧安装器等待 20 秒后中止。这不是管理员权限问题。
- 当前安装器仍先请求正常退出；超时后仅当进程属于当前会话、路径与本次发现的 Playnite 可执行文件完全一致且 `MainWindowHandle=0` 时，才结束该残留实例。路径不可确认、跨会话或仍有主窗口时继续拒绝强制结束；PID 自然退出竞态不再误报。
- `DEV-INSTALL-006` 已在原失败场景完整通过：Release 0 警告/0 错误，Core 42/42、Worker 117/117、Playnite 203/203，普通用户安装并启动；当前安装 DLL 标识 `0.6.70+4125a5448b1d903c1122d6ba596b8ca31597a714`。Playnite 11:14:33 日志确认插件加载，Worker 11:14:38 日志确认初始化并启动。

## 2026-08-13 DEV-INSTALL-005：构建隔离与真实宿主验证

- 用户日志中的 Contracts 编译成功；失败发生在后续测试覆盖标准 `bin\Release` 时，原因是旧 `dotnet/testhost` 或 Worker 文件锁。当前一键入口 `GameSaveCenter-Run.cmd` 对应 `DEV-INSTALL-005`，每次构建使用唯一 `artifacts\dev-build\<Configuration>\<guid>`，不清理标准输出，也不默认请求 UAC。
- `scripts/dev-install-run.ps1 -Configuration Release -NoStart -SkipClean` 已在正常桌面文件权限下完整通过；当前受限 Codex 沙箱对 Playnite 用户扩展目录的写入失败不代表脚本失败。遇到旧残留时应先正常关闭 Playnite，让插件回收其所属 Worker；不要按进程名强杀，也不要把管理员提权作为默认修复。
- 真实启动证据：`C:\Users\lopmatu\AppData\Roaming\Playnite\playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`；插件自身日志记录 `GameSaveCenter.Playnite 0.6.70.0`；`C:\Users\lopmatu\AppData\Local\GameSaveCenter\Logs\worker-launch.log` 记录 Worker 存储初始化后进入 `Application started`。安装报告为 `artifacts\last-dev-install.txt`。
- 当前自动化基线：Core `37/37`、Worker `81/81`、Playnite `202/202`，Release 构建 0 warnings / 0 errors。真实主题/DPI/键盘、Rclone/多设备、游戏恢复/撤销、EXE/LNK/BAT/PS1 和 900+ 游戏库仍需人工 QA。

## 必须读取的资料

按以下顺序读取：

1. `AGENTS.md`
2. `docs/DEVELOPMENT_HANDOFF.md`（本文件）
3. `docs/PROJECT_MEMORY.md`：长期不可丢失约束、已完成 UI 决策和性能边界
4. `docs/DEVELOPMENT_PROGRESS.md`：按 UI 编号排列的实施历史和下一步线索
5. `docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md`：总体设计方向
6. `docs/design/UI_CHANGE_GATE.md`：每次 UI 变更的门禁与验收标准
7. `.codex/skills/wpf-apple-desktop-ui/SKILL.md`：WPF/Playnite UI 专项技能，随仓库提交；本机同时安装于 `%USERPROFILE%\.codex\skills\wpf-apple-desktop-ui`。做任何 WPF/XAML 改动前先完整读取，并按任务需要读取 `references/` 中的对应文档
8. `C:\Users\lopmatuse\.codex\attachments\1b6b382f-30ed-44c7-a9ce-6c580fefbe83\pasted-text.txt`：用户提供的完整任务提示词附件；如果新电脑不存在该路径，以本文件和仓库内文档为准
9. `D:\workplace\Github\GameSaveCenter.WpfUiDemo.v3.1`：WPF Demo 模板，比较布局层级、节奏、控件尺寸和交互表面，不复制 Demo 假数据或业务实现；当前本机实际可用副本为 `D:\workplace\VSCode\GameSaveCenter.WpfUiDemo.v3.1`

如果附件路径发生变化，先在当前对话的附件中找到同一份完整提示词；不能因为附件不可用而跳过仓库内的规则和约束。

## Codex 2026-08-11/12 阶段补充（性能与自定义启动项）

## Codex 2026-08-13 阶段补充（环境准备与 GameTool 安全策略）

ONBOARDING-001 已在既有 Maintenance 诊断页加入首次环境准备入口；GAME-TOOL-003/004 已在既有 TrainerCenter Inspector 加入 CustomExecutable 的已有实例策略和风险分类。当前测试基线为 Worker 74/74、Playnite 200/200；Worker/Playnite Release 构建均为 0 警告、0 错误，WPF 静态校验与离屏 render QA 通过。

GameTool 的安全边界不可丢失：不能按进程名直接关闭程序；只在完整 EXE 路径一致且重新确认 PID 路径后执行 Restart；无法读取路径时保守拒绝。反作弊风险游戏中只允许 `GeneralUtility` 自定义工具自动启动，Unknown/Trainer/CT/GameModification 必须阻止并审计。真实 Playnite 覆盖安装、扩展扫描和 Worker/IPC 加载仍受 PID 3896 文件锁限制，清理 PID 后需要重新执行一键安装并检查 `playnite.log` 与 `extensions.log`。

## Codex 2026-08-13 阶段补充（SMART-PROTECT-001/002）

SMART-PROTECT 已完成第一版智能保护闭环：游戏停止请求等待现有存档识别，识别到候选/接受候选/Ludusavi 匹配后在既有 Dashboard 对话框显示“启用推荐策略 / 以后再说 / 不再提醒”三选一；没有识别到存档只写审计，不打扰用户。提示状态与最近识别结果保存在 SQLite，`Deferred` 冷却 7 天，`Enabled` 和 `Dismissed` 不再重复提示。因为识别最长约 2 分钟，Playnite 的 `session.stopped` 请求使用 3 分钟专用超时。

Overview 既有最近保护卡已扩展为最近窗口游戏状态列表：`已保护`、`未匹配`、`存档未保护`、`风险`。已保护项不可选，其余项可多选，通过 `protection.recommended.apply` 批量启用退出后与游玩中自动保护，并写入审计后刷新快照。不要把这个能力移到新主导航页，也不能把“启用推荐”实现成自动恢复或删除操作。

本阶段验证：Core 29/29、Worker 76/76、Playnite 202/202；Worker/Playnite Release 构建 0 警告、0 错误；`validate-source.py`、`check-xaml.ps1`、WPF UI 契约测试与 `render-qa.ps1` 全部通过。测试使用 `artifacts/smart-test/final-*` 隔离输出；标准测试目录仍被早先宿主锁住时，不要删除或强杀不属于本次任务的进程。

验证边界不变：当前会话无法完成真实最终包的 Playnite 扩展扫描；真实宿主日志仍需用户复核。开发安装器不默认请求管理员权限，而是先正常关闭 Playnite、等待插件回收其 Worker，再按扩展路径处理残留。隔离启动/离屏渲染只能证明应用层启动和布局，不得写成“真实宿主加载成功”。

## Codex 2026-08-13 阶段补充（NOTIFY-001 / MULTI-DEVICE-001 / RCLONE-RELIABILITY-001）

退出任务的 `TaskStatusDto.SessionId` 是通知聚合的唯一业务关联；备份与媒体任务必须使用退出会话的 SessionId，Playnite 只在该会话预期任务全部进入终态后发一条摘要。摘要不能另造成功判断，必须复用任务中心终态；云端失败时保留“本地备份完成”事实并给出重试提示。

多设备摘要的 `ParentBackupId` 用于识别共同基线：同父版本的两个不同子版本标记 `DivergedFromCommonBase`，线性父子关系不标记冲突。`PreferLocal`、`PreferRemote`、`KeepBoth` 只持久化用户决定；不得自动合并、自动选择、删除远端或绕过 PreRestore → Restore → Validate → Rollback。

Rclone 可靠性仍是单向安全适配器，只允许 `copy`、`check`、`lsf`、`cat`、`version`。`RcloneFailureClassifier` 将认证、权限、远端不存在、网络和不完整传输转换为稳定错误码；只有网络/不完整传输进入有限退避，凭据/权限类错误不能无限重试。本地历史不得因远端缺失而删除。

当前自动验证基线：Core 35/35、Worker 81/81、Playnite 202/202；三者 Release 构建均 0 警告/0 错误；source、XAML、WPF 静态门禁通过。真实 Rclone、两台设备、真实恢复和最终 Playnite 宿主加载仍为 `MANUAL QA REQUIRED`。开发安装的 Playnite/Worker 回收链路已改为普通权限正常关闭优先。

新的 AI/Codex 长期记忆入口已建立：先读 `docs/ai/PROJECT_MEMORY.md` 与 `docs/ai/WORKLOG.md`，再读本文件。

本轮已完成并推送：

- PERF-004：`[PERF]` 性能基线日志 + `docs/ai/PERFORMANCE_BASELINE.md`。
- PERF-005：Snapshot 无变化 0 CollectionChanged（`SnapshotComparers` + `BatchObservableCollection.ReplaceAll`）。
- PERF-006：Task/Media 搜索 180ms 防抖（`DebouncedRefresh`）。
- GAME-TOOL-001/002：自定义启动项 EXE/LNK/BAT/CMD/PS1，外部路径引用不复制文件；`GameToolLauncher` 按类型启动；`GameToolSessionTracker` 只按 Session/PID/StartTime 关闭本会话进程。
- PERF-007：媒体缩略图异步化（`AsyncThumbnailLoader` 3 并发 + LRU + Freeze，`AsyncThumbnailImage`）。
- UI-QA-REAL-001：隔离 Playnite 真机冒烟通过，截图在 `artifacts/ui-qa/real/playnite-real.png`；主题/DPI/键盘/缩放与自定义启动项真机流程仍待用户复核。
- UI-206 初始方案（SUPERSEDED）：提交 `962a6b0` 曾把共享与关键 DataGrid Style 改为 `VirtualizingPanel.ScrollUnit=Pixel`，并把 Maintenance/Task 表格由强制 `Height` 改为 `Height=double.NaN + MaxHeight`；该 Pixel 方案已由真实 Playnite A/B 验证为回归并撤回，仅保留历史记录，不作为当前方案。
- DataGrid 最终结论（`d9cd82f`/`0ce3388`/`4564c8f`）：Pixel ScrollUnit 经真实 Playnite A/B 验证会回归，已撤回；当前采用 `Item` + `GscStableDataGridRow` 稳定行样式 + geometry probe（gap ≤4 DIP、末行完整、Recycling 保持）。不要重新改回 Pixel。

本机最近两个本地提交（未推送）：`e86e461 docs: record UI-207 revalidation baseline`、`d45f65c feat: harden restore readiness and recovery drills`。UI-207 的设置布局、运行中游戏自动定位、上次选择恢复、GamePicker 新用户默认“已安装”和当前游戏真实 Icon 已由 `d2662e3` 收口；Restore Readiness 的严格 Manifest 校验与恢复灾难演练已由 `d45f65c` 收口。

当前测试基线：Core 27、Worker 67、Playnite 197；本阶段 Worker Release 构建 0 警告/0 错误，Restore Readiness/恢复演练 67/67；上一阶段 render-qa 全绿，源码验证与技能静态审查通过。下一阶段为 POLICY-001；不要重复打开已完成的 UI-207、RELIABILITY-RESTORE-001、HEALTH-001、PROTECTION-001、PERF-004～010 与 GAME-TOOL-001/002。

## 项目目标

这是已有的 Playnite 插件项目 GameSaveCenter 的持续 UI 重构，不是新建功能，也不是只改某一个 `Margin` 或颜色。

生产 UI 使用现有的 WPF/C# 技术栈，最终视觉和信息层级应接近 `GameSaveCenter.WpfUiDemo.v3.1`。Demo 只是 UI 模板；生产页面必须继续使用真实数据、真实命令和真实状态。

必须长期保持：

- 不改插件 ID、业务服务、Worker、IPC、数据库、持久化和任务协议，除非用户明确要求并单独批准。
- 保留现有 ViewModel、命令、Binding、`x:Name`、真实状态流、错误反馈和 Playnite 生命周期。
- 修复共享资源、样式、ControlTemplate 和页面结构，不用局部补丁掩盖共享控件问题。
- 保留大型列表的有限测量、内部滚动、键盘访问、UI Automation 和虚拟化。
- 不使用 HTML、WebView、Electron、Avalonia、WinUI 或截图替代原生 WPF。
- 不使用 Demo 假数据，不用 `Task.Delay` 模拟业务成功，不把视觉状态伪造成业务结果。

## 用户明确提出的 UI 细节

这些细节不是可选的视觉偏好，而是持续验收标准：

- 同类按钮的宽度/高度、文字方向、文字垂直和水平位置必须一致。
- 文本不能因为共享模板硬编码对齐而偏离调用方意图；需要修复共享模板的 `TemplateBinding`。
- 下拉框初始必须显示明确的默认值。例如初始筛选是“全部”，控件就必须显示“全部”。
- 对于依赖真实上下文的动态下拉框，空值可能代表“等待真实上下文”，不能为了视觉而强行选择第一项。
- 页面在 100%/125%/150%/175%/200% DPI、不同窗口尺寸、Light/Dark/Follow Playnite/高对比度下不能出现重叠、裁切、文字不可读或操作入口消失。

## 每次开发的固定流程

1. 先执行 `git status`、`git branch --show-current` 和最近提交检查；不得 `reset --hard`、`checkout --` 或覆盖别的电脑留下的未提交改动。
2. 读取本文件、`PROJECT_MEMORY.md`、`DEVELOPMENT_PROGRESS.md` 中与目标页面相关的最新条目。
3. 搜索目标控件的全部共享样式、模板、资源和调用点，先判断根因属于信息架构、布局测量、模板状态、可读性、可访问性、性能还是宿主兼容性。
4. 按 UI Change Gate 实施小范围、可验证的 UI 改动；优先共享资源和结构修复，保持业务合同不变。
5. 至少运行适用的静态校验（含 `python .codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .`）、`git diff --check`、源码验证、WPF 结构测试、Debug/Release 构建和相关单元测试。
6. 真实 Playnite、主题、DPI、键盘或宿主渲染没有实际运行时，必须明确写“尚未验证”，不能声称已经验证。
7. 完成一轮后同步更新：
   - `docs/PROJECT_MEMORY.md`：新增不可丢失的结构/行为约束
   - `docs/DEVELOPMENT_PROGRESS.md`：记录 UI 编号、修改文件、保留的命令/绑定、验证结果和未完成的宿主验证
   - 本文件的“当前交接基线”和“下一步方向”
8. 每次有实际开发改动都必须创建一个清晰的 Git commit。提交前确认工作区没有意外文件。

## 合并后当前交接基线（2026-08-11）

- 分支：`main`
- 当前 UI 交接基线：本轮本地提交 `界面：完善设置布局与当前游戏上下文`（hash 以 `git log -1` 为准；本任务不 push）；生产 UI 最近相关提交 `0ce3388`（DataGrid 稳定行样式 + geometry probe）与 `4564c8f`（诊断摘要非裁剪）。DataGrid 最终采用 `Item + GscStableDataGridRow`，Pixel 已真机验证并撤回。
- UI-207（本轮）：设置页 Header 不裁剪、分类栏宽/窄滚动与选中 BringIntoView；打开 Dashboard 自动定位运行中游戏（否则恢复上次选择/首个已安装），GameStarted 事件驱动、普通刷新不抢回；GamePicker 新用户默认“已安装”；当前游戏显示真实 Playnite Icon（UI-only provider，LRU 48，失败 fallback 手柄 glyph）。当前复验基线为 Core 27 / Worker 59 / Playnite 197、render-qa 全绿；真实设置页/自动定位/Icon 与 1080p/4K/DPI 人工验收标记 BLOCKED_ENVIRONMENT。
- 上一合并提交：`e87e2af`（`merge: reconcile local and cross-machine UI migration`）
- 合并共同基线：`9cdd975`；本机 UI-173～UI-181 与 `origin/main` 的 UI-181～UI-183、交接文档线均已保留，没有删除任一方共同基线后的提交。
- 本机额外 WIP 已先由 `e61d0fc` 固化后纳入合并；本机的长期约束已追加到 `docs/PROJECT_MEMORY.md` 的 `MERGE-001`，远端既有记忆条目保持原文。
- UI-184 已将 Overview 的 Demo 层级落地为“今日工作台动作卡 → Hero/当前游戏双列（受限宽度堆叠）→ 六项指标 → 最近活动”，具体约束见 `docs/PROJECT_MEMORY.md` 与 `docs/DEVELOPMENT_PROGRESS.md`。
- UI-185 已将 SaveCenter 的候选路径页补齐为“当前规则与校验 → 候选表/Inspector”，页签改为“路径与校验”；共享 Dashboard 游戏上下文不重复渲染，真实 `SelectedGame` 状态和扫描/校验/刷新命令保持绑定，具体约束见 `docs/PROJECT_MEMORY.md` 与 `docs/DEVELOPMENT_PROGRESS.md`。
- UI-186 已将 TaskCenter 摘要卡改为 Demo 的“任务总数 / 运行中 / 需要重试 / 已完成”四项真实任务状态计数，宽屏四列、窄屏按两列/单列收缩；任务筛选、全局视角、详情 Inspector 和恢复命令未改变。
- UI-187 已将 Maintenance 诊断页顶部摘要改为 Demo 的六项真实健康卡（Worker、Ludusavi、Rclone、数据与备份目录、媒体目录、设备状态），并将响应式列数收口为宽屏 3 列、中屏 2 列、窄屏 1 列；诊断操作、表格、Inspector、审计、完整摘要和原有命令/绑定均保留。
- UI-188 已将 TaskCenter 任务队列补齐 Demo 的搜索输入框，真实搜索任务 ID、类型、游戏、详情和错误，并与状态/游戏/类型筛选叠加；未新增 Worker/IPC 请求，任务计数、Inspector、恢复命令和虚拟化保持不变。
- UI-189 已将 TaskCenter 顶部四项任务摘要改为 Demo 的“标题 → 30px 数值 → 副文案”三行阅读卡，仍绑定真实任务计数并保持四列/两列/单列响应式逻辑；搜索、筛选、Inspector、命令和虚拟化未改变。
- UI-190 已修复常用窗口尺寸下 Overview 底部内容被截断、Media 表格只剩一行和 Maintenance 诊断下方内容不可达的问题：Overview 只滚动上方工作台内容并让最近活动保持有限 Grid/ListBox 视口；Media 与 Maintenance 使用明确命名的页面滚动面承载下方内容，主表/主列表由 code-behind 保持 236–460 DIP 有限高度和内部虚拟化滚动。真实命令、Binding、Inspector 和业务层未改变。
- UI-191 已修复 SaveCenter/TrainerCenter 窄宽度或低高度堆叠 Inspector 时主列表被挤成一行的问题：历史版本/候选路径 DataGrid、已安装工具/FLiNG 搜索结果/可下载版本 ListBox 区域保持 236 DIP 最小视口；Inspector 高度按实际布局剩余空间计算并继续使用自身滚动。真实命令、Binding、选中项、导入确认、虚拟化和 Recycling 未改变。
- UI-192 已修复 TaskCenter 窄宽度或低高度堆叠详情 Inspector 时任务主表被挤压的问题：任务队列保留 236 DIP 最小视口，详情高度按摘要区、筛选区和主表剩余空间计算，搜索/筛选、真实计数、取消/重试/复制命令和虚拟化未改变。
- UI-193 已修复 Maintenance 设备状态、异常审计和进程映射页窄宽度/低高度堆叠 Inspector 时主表被挤压的问题：诊断、设备、审计发现和进程映射表统一保留 236 DIP 最小视口；设备/审计 Inspector 按实际布局剩余空间限高并继续内部滚动，进程映射表接入显式结构标识和共享表头主题加载，命令、Binding、选中项、审计日志和虚拟化未改变。
- UI-194 已将共享页面级滚动契约与 Demo 对齐：`GscPageScrollViewer` 默认垂直 `Auto`，设置页、存档策略、维护保留策略、侧栏导航和 Overview 辅助页面不再用 `Hidden` 掩盖溢出；Overview 宽屏/堆叠的有限内部滚动仍由 code-behind 分工控制，表格/列表虚拟化未改变。
- UI-195 已补齐 Maintenance“设备状态 / 异常与审计 / 进程映射”三个 Tab 的命名页面滚动面；对应三张主表由 code-behind 使用 236–460 DIP 有限 Height，继续保留 DataGrid 内部滚动、虚拟化、Inspector 滚动、真实命令和 Binding，源码门禁同步识别该结构。
- UI-196 已补齐 TaskCenter 的命名页面滚动面 `TaskPageScrollSurface`；任务表由 code-behind 使用 236–460 DIP 有限 Height，堆叠 Inspector 按页面实际视口计算剩余高度，摘要、筛选、任务表、详情和底部恢复操作在短高度常用窗口下均可通过明确滚动访问，真实搜索/筛选/计数/Binding/命令/虚拟化未改变。
- UI-197 已修复截图暴露的常用窗口可见性问题：TaskCenter 动态游戏/类型筛选的 WPF 集合刷新空选中在 DataBind 优先级恢复为 `全部`，MediaGrid 通过共享 ListBox 顶部内容契约避免少量媒体卡沉到有限视口底部，Overview 工作台按钮在窄主列第二行横向自动换行，宽屏右侧摘要/风险列保留有限 Auto 滚动以确保“打开维护中心”可达；真实命令、Binding、Worker、IPC、数据库、持久化、虚拟化、键盘访问和 Automation 未改变。
- UI-198 已修复 Overview 在常用窗口和窄窗口下的主工作区被 sibling 行挤压问题：工作台、Hero/当前游戏、六项指标和最近活动统一进入 `OverviewPrimaryScrollSurface`，窄布局再由 `OverviewStackScrollSurface` 统一承载主列与右侧摘要，避免 980 DIP 下主列高度变成 0；宽布局仍保留主列/摘要列独立有限滚动。`OverviewActivityList` 继续使用有限高度、ListBox Recycling 和自身滚动，真实命令、Binding、SelectedTask、键盘访问和 Automation 未改变。已按 1600/1366/1280/1100/980 DIP 与 900/768/720/700/640 DIP 运行隔离生产离屏渲染，源码验证通过，生产插件 Release 构建 0 警告/0 错误，隔离测试 149/149 通过；未运行真实 Playnite 宿主、主题切换、DPI 真机和连续缩放流畅性验证。
- UI-199（代码提交 `5cbd512`）已修复工作区由程序化导航、恢复状态或离屏渲染直接切换时顶栏仍显示“首页”的语义不同步：`DashboardView.UpdateWorkspacePresentation()` 与侧栏点击共同调用 `UpdateWorkspaceHeader`，媒体/维护/任务等页面标题和副标题始终跟随当前可见工作区。MediaCenter 的摘要卡响应式断点改为逻辑 DIP 的 `>=760` 四列、`>=520` 两列、其余单列，使 Dashboard 在常用 1080p/2K/4K 窗口下保持 Demo 四卡横排并为主表保留可见行；表格有限视口、内部滚动、虚拟化、Inspector、真实命令和 Binding 未改变。源码验证通过；生产插件 Release 构建 0 警告/0 错误；隔离 WPF 测试 150/150 通过；生产离屏渲染覆盖 1600/1366/1280/1100/980 DIP 与 900/768/720/700/640 DIP 并返回 `render-prod OK`。Render harness 自身仍有 3 个 FakeApi 未使用事件警告；真实 Playnite 宿主、主题切换、DPI 真机和连续缩放流畅性尚未验证。
- UI-200（代码提交 `f11e9b7`）已将 Demo 的 `MinWidth=1040`、`MinHeight=700` DIP 固化为生产外壳的常用最小窗口：Dashboard `>=1040` 保留带文字侧栏和单行顶栏，低于该值才进入图标紧凑壳；同时按外壳扣除侧栏后的约 700 DIP 页面宽度校准 Media `>=700` 四列、Task `>=900` 四列/`>=680` 两列、Maintenance `>=980` 三列/`>=680` 两列。1040×700 离屏结果为 Media 四卡并显示两行表格、Task 2×2 摘要并显示队列、Maintenance 两列健康卡；1366×768 仍为完整多列，页面级滚动、表格/列表有限视口、内部滚动、虚拟化、真实命令/Binding 和业务层未改。源码验证通过；生产插件 Release 构建 0 警告/0 错误；隔离 WPF 测试 151/151 通过；生产离屏渲染覆盖 1600/1366/1280/1100/1040/980 DIP 与 900/768/720/700/640 DIP 并返回 `render-prod OK`。Render harness 自身仍有 3 个 FakeApi 未使用事件警告；真实 Playnite 宿主、主题切换、DPI 真机和连续缩放流畅性尚未验证。
- SKILL-001（本轮）：`wpf-apple-desktop-ui` 技能已随仓库提交到 `.codex/skills/wpf-apple-desktop-ui/`，并安装到本机 `%USERPROFILE%\.codex\skills\wpf-apple-desktop-ui`；AGENTS.md、DEVELOPMENT_HANDOFF.md、UI_CHANGE_GATE.md、PROJECT_MEMORY.md 与 DEVELOPMENT_PROGRESS.md 已同步仓库内技能路径，UI 门禁新增 `python .codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .` 静态审查。
- QA-001（本轮）：新增可复用的离屏渲染 QA：`tests/GameSaveCenter.RenderHarness`（假数据，不启动 Worker/IPC）与 `scripts/render-qa.ps1`，覆盖 1040×700、1280×720、1366×768、1600×900、1920×1080 逻辑窗口，输出 PNG 与 `artifacts/ui-qa/render/render-qa-report.txt`（页面滚动面、DataGrid/ListBox 有限视口尺寸、可滚动性）。1040×700 复核结果：Media 待归类/当前游戏媒体主表 350 DIP 高、6 行；Task 队列 350 DIP 高、8 行；Maintenance 各主表 350 DIP 高、8 行；所有页面滚动面为 `Auto` 且内容超限时 `scrollable=True`；Overview 堆叠模式由页面滚动承载，风险区内容完整（496 DIP）。本机系统 SDK 9.0.302 在多节点构建时会因 SDK locator 目录缺失在 `GetTargetFrameworks` 静默失败，`render-qa.ps1` 已固化 `-m:1 -nodeReuse:false -p:NuGetAudit=false`。本机 C 盘空间耗尽时，需先把 `TEMP/TMP` 指到仓库 `.tmp/qa-temp` 再运行脚本。真实 Playnite 宿主、主题、DPI 和连续缩放流畅性仍未验证。
- UI-201（本轮）：TaskCenter 堆叠详情 Inspector 的最小高度从 96 提高到 160 DIP，解决 1040×700/1280×720/1366×768 常用窗口下详情条带过矮、基本无法阅读的问题；`TaskGrid` 的 236–460 DIP 有限视口、内部滚动、虚拟化、真实搜索/筛选/计数/命令/Binding 均未改变。离屏 QA 复核：1040×700/1280×720/1366×768 下 `TaskDetailScrollViewer` 均为 160 DIP 且内部 `Auto` 滚动，Task 队列仍为 350/360/384 DIP 高、8 行；1600×900/1920×1080 仍保持 Demo 式右栏 Inspector（360 宽）。源码验证通过；真实 Playnite 宿主、主题、DPI 和连续缩放流畅性仍未验证。
- UI-202（本轮）：Overview 首页按 Demo `HomeView` 的单列阅读流收口：Dashboard 在内容区 `<1200` DIP（1280×720、1366×768 等常用窗口化逻辑宽度）时把 Overview 切为堆叠单列页面流，右侧“今日概览/风险与提醒”下移到主内容之后，避免主工作区只剩 550–600 DIP 宽导致 Hero 与“当前游戏”被挤成上下堆叠；Hero/当前游戏同行的堆叠阈值同步从 760 降到 700，使 1040×700 最小窗口也保持 Demo 的“Hero + 当前游戏”并排。离屏 QA 复核：1040×700 Hero 444px、当前游戏 266px 同行；1280×720 Hero 576px、当前游戏 346px 同行；1366×768 Hero 630px、当前游戏 378px 同行；所有尺寸均无重叠，最近活动由 `OverviewStackScrollSurface` Auto 滚动可达；1600×900/1920×1080 继续使用宽屏双列。`OverviewActivityList` 有限视口、ListBox Recycling、真实命令/Binding 和页面滚动分工未改变；`render-qa.ps1` 的 Overview 堆叠阈值同步为 1200。源码验证通过；真实 Playnite 宿主、主题、DPI 和连续缩放流畅性仍未验证。
- UI-203（本轮）：存档中心与修改器中心的堆叠 Inspector 最小高度统一从 96 提高到 160 DIP（`SaveHistoryActionsScrollViewer`、`SaveCandidateInspectorScrollViewer`、`TrainerToolsSettingsScrollViewer`、`TrainerReleaseInfoScrollViewer`），与 UI-201 的 Task 规则一致；`SaveHistoryGrid`/`SaveCandidateGrid`、修改器主表与可下载版本面板仍保持 236 DIP 最小视口、内部滚动和虚拟化。离屏 QA 已扩展覆盖 SaveCenterView/TrainerCenterView（含假数据）：1040×700/1280×720/1366×768 下四个堆叠 Inspector 均为 160 DIP 且内部 Auto 滚动，主表 236 DIP 高、8 行；1600×900/1920×1080 保持右栏 360 宽。源码验证通过；真实 Playnite 宿主、主题、DPI 和连续缩放流畅性仍未验证。
- QA-002（本轮）：离屏渲染 QA 覆盖补齐全部工作区与设置页：Overview、Save、Trainer、Media、Maintenance、Task、Settings 均在 1040×700/1280×720/1366×768/1600×900/1920×1080 渲染并输出 PNG/报告。设置页依赖 Playnite 宿主 `BaseTextBlockStyle`，harness 在 Application.Resources 预置中性 fallback 后可在无宿主环境解析；1040×700 下 Settings 四个 Tab 的 `SettingsScroller` 均为 Auto 且内容超限时 `scrollable=True`。
- QA-003（本轮）：`render-qa.ps1` 增加自动失败门禁：任何命名工作区主表/主列表（排除 `MaintenanceAuditLogGrid` 审计条带与 `OverviewActivityList` 最近活动）在任一常用窗口下的有限视口 `<236` DIP，或命名页面滚动面（`*ScrollSurface`/`SettingsScroller`）内容超限却使用 Hidden 滚动条，render-qa 将以退出码 1 失败并在报告中列出 `PROBLEM`。当前 7 页面 × 5 尺寸全绿。
- QA-004（本轮）：在 C 盘恢复可用空间并重定向 `TEMP/TMP` 到 `.tmp/qa-temp`、测试输出隔离到 `artifacts/ui-qa/*-tests` 后，完整隔离测试重跑通过：Core 13/13、Worker 23/23、Playnite 151/151。Playnite 源码结构断言同步更新为 UI-201/202/203 的新阈值（`Math.Max(160, ...)`、`workspaceContentWidth < 1200`、`primaryWidth < 700`）。
- QA-005（本轮）：`scripts/dev-install-run.ps1 -Configuration Release -NoStart` 一键构建安装成功：解决方案 Release 构建 0 警告/0 错误，Core 13/13、Worker 23/23、Playnite 151/151 全部通过，已打包并安装到 `C:\Users\lopmatu\AppData\Roaming\Playnite\Extensions\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec`（extension.yaml 0.6.70、DLL 0.6.70.0），未自动启动 Playnite。真实 Playnite 宿主内的 Light/Dark/Follow/高对比度、DPI、键盘与连续缩放仍需用户手工验收。
- UI-207-FOLLOWUP（2026-08-12）：Settings 的页面滚动所有权已收口到共享 TabControl 模板：宽屏 `SettingsHeaderScroller` 为 232 DIP 左侧导航，紧凑布局为顶部横向 Auto，`SettingsScroller` 仅承载当前内容；5 个分类在 760/880/920/1100/1400 × 560/700/900 探针中可见。当前游戏 Icon 统一由共享样式复用到 Dashboard/Overview/Save/Trainer/Media，GamePicker 列表不加载 Icon；筛选隐藏当前选择时保留选择并提供恢复操作。隔离 Release 构建 0 警告/0 错误，Core 13/13、Worker 51/51、Playnite 197/197，render-qa 全绿；真实 Playnite 宿主主题/DPI/键盘/连续缩放仍未运行。
- UI-204（本轮）：修复真实 Playnite 中任务中心筛选下拉框在异步集合重建后显示为空的问题。新增共享 `UiFilterSelection.RestoreDefault`（`src/GameSaveCenter.Playnite/Infrastructure/UiFilterSelection.cs`），TaskCenter 的 状态/游戏/类型 三个筛选下拉框在 `Loaded`、`DataContextChanged`、游戏/类型选项集合 `CollectionChanged` 时恢复逻辑默认值（全部）；Dashboard GamePicker 的 状态/平台/排序 三个下拉框在打开选择器与平台选项重建时同样恢复，且只在 `SelectedItem == null` 时恢复，不覆盖用户真实选择。离屏 QA 记录命名 ComboBox 的 `selected/index/items`，空选择会触发 `PROBLEM`；render-qa 全绿，Playnite 151/151 通过。
- UI-205（本轮）：针对真实 Playnite 中动态下拉框 Items 物化晚于 DataBind 导致游戏/类型筛选仍为空的问题，TaskCenter 增加 200ms `DispatcherTimer` 短周期重试（最多 25 次），在集合重建、加载、DataContext 变化后持续恢复默认选中直到三个下拉框都有值；GamePicker 平台选项重建与面板打开时增加 `DispatcherPriority.Loaded` 二次恢复。render-qa 与 Playnite 152/152 通过。
- PERF-001（本轮）：性能优化第一刀——大列表集合批量通知。新增共享 `BatchObservableCollection<T>`（`src/GameSaveCenter.Playnite/Infrastructure/BatchObservableCollection.cs`），`ReplaceAll` 只在内容真正变化时发一次 `Reset`，避免 `Clear()+Add()` 对 Games/Tasks/Media/Findings/Backups/GameTools/TrainerCatalogResults 等大集合逐条触发 WPF 布局；`DashboardViewModel.Replace` 自动路由到 `ReplaceAll`。render-qa 新增每页 `render_ms` 计时基线；render-qa 全绿，Playnite 152/152 通过。
- PERF-002（本轮）：任务中心每次完整快照都会重建 游戏/类型 筛选选项（O(n log n)）。新增 `ComputeTaskFilterFingerprint`，当 `Tasks` 的任务 ID 顺序与数量指纹未变化时直接跳过 `RebuildTaskFilters`，用户筛选仍由属性 setter 直接 `TasksView.Refresh()`；render-qa 全绿，Playnite 152/152 通过。
- PERF-003（本轮）：GamePicker 每次快照都会 `Distinct+OrderBy` 重建平台筛选选项。新增 `ComputePlatformFingerprint`（平台名顺序 + 数量），`Items` 未变化时跳过 `RebuildPlatformOptions`；render-qa 全绿，Playnite 152/152 通过。
- PERF-004（本轮）：GamePicker 每次快照都为整库新建 `GamePickerItem`，大库下分配多且选中引用会漂移。改为按 `PlayniteId` 缓存复用 `GamePickerItem`，快照时只 `UpdateGame` 更新内部 `GameStatusDto` 引用（缓存超过 `max(1024, 2*游戏数+100)` 才清空重建）；减少大库分配并让选中对象身份跨快照稳定。render-qa 全绿，Playnite 152/152 通过。
- 新增的响应式门禁要求：1080p、2K、4K 不能只按物理分辨率判断，必须按 DPI 换算后的逻辑 DIP 尺寸检查全屏、窗口化和最大化；常用窗口下首屏下方真实内容不得被页脚或工作区边界遮住，主表/主列表应保留约四行可读视口，页面滚动与列表内部虚拟化滚动必须分工明确。具体门禁见 `docs/design/UI_CHANGE_GATE.md`。
- 本轮工作区：`0c6f143` 提交后干净；后续 agent 仍须先运行 `git status`、`git log -5 --oneline --decorate` 和 `git branch --show-current`。
- 验证：`python scripts/validate-source.py` 通过；`scripts/render-qa.ps1` 覆盖 Overview/Save/Trainer/Media/Maintenance/Task/Settings × 1040×700/1280×720/1366×768/1600×900/1920×1080 全部通过（含自动失败门禁）；技能静态审查 0 error；`scripts/dev-install-run.ps1 -NoStart` 一键构建安装成功（解决方案 Release 0 警告/0 错误，Core 13/13、Worker 23/23、Playnite 151/151），最新扩展已安装到本机 Playnite Extensions，未自动启动。真实 Playnite 宿主、主题、DPI、窗口化截图和连续缩放运行时渲染仍需用户在 Playnite 内手工验收。

以下原有的远端交接基线保留为历史记录，便于追溯另一台机器的 UI-183 上下文：

## 原远端交接基线（2026-08-10）

- 分支：`main`
- 当前 UI 基线：`2db4336`（`重构：维护中心诊断操作区对齐 Demo`，UI-183）
- 版本：`0.6.70-development-preview`
- 当前工作区：本轮交接文档提交完成后应干净
- 相对 `origin/main`：请运行 `git rev-list --count origin/main..HEAD` 实时确认；UI-183 实现提交为 `2db4336`，后续交接文档提交不改变生产 UI
- 最近已完成的重点：
  - UI-177：修改器中心无选中工具时释放空 Inspector 和固定右栏
  - UI-178：媒体中心无选中媒体时释放空 Inspector 和堆叠行
  - UI-179：设置页窄屏标题提示换行，以及 ZIP/Zstandard/跟随 Playnite 默认值显示
  - UI-180：首页阅读顺序调整为“工作台/今日状态 → 当前游戏 → 六项指标 → 最近活动”
  - UI-181：维护中心五张 DataGrid 的首列显式使用 `MaintenanceFirstColumnHeader`，末列继续使用显式维护表头主题
  - UI-182：维护中心进程映射编辑器宽屏使用 EXE `*`、目标游戏 240 DIP、绑定按钮的 Demo 对齐 Grid，窄于 720 DIP 时目标和按钮换到第二行
  - UI-183：维护中心诊断页顶部改为 Demo 式“诊断操作”阅读卡，刷新诊断提升为主操作，其余五个只读入口保留在第二行操作带
- 当前已完成的自动化基线：Core 13、Worker 23、Playnite 142 测试通过；Release 构建 0 警告/0 错误；XAML 结构检查 13/13；WPF 源码验证通过。静态测试不等同于真实 Playnite 宿主、主题和 DPI 渲染验证。

## 下一步开发方向

继续以 Demo 对齐为目标，对生产页面做页面级收口和真实宿主验收，优先顺序如下：

1. 以 `scripts/render-qa.ps1`（7 页面 × 5 种常用逻辑窗口）为离屏回归基线；继续检查 Overview、SaveCenter、TrainerCenter、TaskCenter、Maintenance、MediaCenter、Settings 的层级、按钮尺寸、文字对齐、默认选择和空状态。
2. 检查共享 `Button`、`ComboBox`、`TextBox`、`ListBox`、`DataGrid`、Tab 和 Inspector 资源，发现同类问题时修共享模板。
3. 每次页面级改动后运行 `scripts/render-qa.ps1`（C 盘满时先设 `TEMP/TMP` 到仓库 `.tmp/qa-temp`）；按 980/1040/1100/1280/1366/1600 DIP 宽度、640/720/900/1080 DIP 高，以及 1080p/2K/4K 在 100%/125%/150%/175%/200% DPI 下的常用窗口化逻辑尺寸，复核窄屏堆叠、首屏内容可见性、有限滚动和长文本；不把 4K 通过当作 1080p 通过。
4. 在可用环境中运行 Playnite 宿主，验证 Light/Dark/Follow Playnite/高对比度、键盘焦点、真实数据加载和窗口关闭生命周期；若环境不可用，保留明确的手工验收清单。
5. 发现问题后继续使用新的 UI 编号记录，不要删除历史记录或把未验证事项标成完成。
6. 在可用的干净环境中重跑完整 Release 构建与隔离测试；本机曾因测试输出 DLL 残留句柄占用和 C 盘 0 可用空间而无法在本次重跑。

## 跨电脑、跨模型规则

代码和文档是交接的真实来源，模型记忆不是。切换电脑前应先把当前提交推送到远端：

```powershell
git push origin main
```

切换后应先拉取同一分支，再执行：

```powershell
git status
git log -5 --oneline --decorate
git branch --show-current
```

如果另一台电脑有未 push 的提交或未提交改动，先比较 `git status`、分支和提交历史，再合并；禁止直接覆盖。DeepSeek、Claude 或其他模型可以根据本文件、源码和测试继续工作，但不会自动继承原聊天中的隐含上下文、工具状态或审批状态，因此必须先读取本文件并按流程重新建立上下文。

## 用户原话（必须保留）

> 在继续之前你最好是能够搞一个文件，能够指引去哪里读取获得开发方向等等。这样我直接说你读取xx文件就可以了，他就知道后续怎么开发了。连我这段话你也要放进去，省得我每次都说了（这样每次开发他们都会维护这个项目）。

这句话代表长期维护要求：后续每次开发都必须继续维护本项目，并同步维护本交接文件、项目记忆、开发进度和 Git commit。
