# GameSaveCenter AI 开发工作日志

> 每完成一个有意义的阶段追加一条；只记录对未来开发有帮助的信息。

## 2026-08-11 DEV-AUDIT-001 重新审计当前仓库

**做了什么：**

- 重新阅读交接文档、设计门禁、AGENTS.md、最新 Git 历史，并核对当前源码。
- 建立 `docs/ai/` 长期记忆机制（本文件 + `PROJECT_MEMORY.md`），供后续任意 Codex 会话快速恢复上下文。
- 确认当前分支 `main`、版本 `0.6.70-development-preview`、工作区仅有未跟踪的 `GameSaveCenter.7z`（用户生成，不提交）。
- 核对六个 Workspace、Settings、GamePicker、DataGrid/ScrollViewer 分工、Theme、离屏 QA、UI-201～205 与 PERF-001～004（旧编号）实际实现。
- 确认 `GameToolType.CustomExecutable` 与 GameTool DTO 字段已存在，但 Worker 导入/启动仍只完整支持 Trainer/CT：自定义启动项尚未正式实现。
- 确认 `MediaThumbnailConverter` 仍是 UI 线程同步解码；Task/Media 搜索仍每次按键刷新；Snapshot 未变化时多数集合仍会 Reset。

**为什么这样做：**

目标文件要求先审计再开发，禁止根据历史聊天或旧 ZIP 猜测当前实现；同时为多端持续开发建立本地长期记忆。

**修改文件：**

- 新增 `docs/ai/PROJECT_MEMORY.md`
- 新增 `docs/ai/WORKLOG.md`

**测试结果：**

文档阶段无需编译；源码未改动。

**仍需验证内容：**

真实 Playnite 宿主、主题/DPI 真机与连续缩放流畅性仍待 UI-QA-REAL-001。

**下一步：**

NEXT: PERF-004 性能基线设施（`[PERF]` 日志 + `docs/ai/PERFORMANCE_BASELINE.md`）。

## 2026-08-11 PERF-004 性能基线设施

**做了什么：**

- 新增统一 `[PERF]` Debug 日志：Worker 快照生成（fetch）、Playnite 快照应用（apply）、Workspace 切换布局、GamePicker setItems/refresh、Task/Media 搜索刷新、Media 详情加载、缩略图解码。
- 新增 `docs/ai/PERFORMANCE_BASELINE.md`，记录测量清单、离屏 render-qa 基线数字与待真机验证项。
- Worker `DashboardService` 注入 `ILogger<DashboardService>`，只输出 Debug，Release 正常运行不刷屏。

**为什么这样做：**

性能优化前先建立可重复的测量设施，避免“凭感觉优化”；日志带数据量便于对照大库规模。

**修改文件：**

- `src/GameSaveCenter.Worker/Services/DashboardService.cs`
- `src/GameSaveCenter.Playnite/ViewModels/DashboardViewModel.cs`
- `src/GameSaveCenter.Playnite/ViewModels/GamePickerViewModel.cs`
- `src/GameSaveCenter.Playnite/Views/DashboardView.xaml.cs`
- `src/GameSaveCenter.Playnite/Converters/MediaThumbnailConverter.cs`
- 新增 `docs/ai/PERFORMANCE_BASELINE.md`

**测试结果：**

- Release 编译通过（Contracts/Core/Worker/Playnite 四个生产项目；测试输出目录有外部残留进程锁，改用隔离 `--output` 目录跑测试）。
- Core 13/13、Worker 23/23、Playnite 152/152 通过。

**性能变化：**

- 离屏渲染基线已记录（1040×700 到 1920×1080 的 render_ms），见 `docs/ai/PERFORMANCE_BASELINE.md`。

**仍需验证内容：**

真实 Playnite 宿主下 `[PERF]` 日志的实际耗时、1000+ 游戏库搜索/切页、缩略图滚动。

**下一步：**

NEXT: PERF-005 Snapshot 无变化 0 Reset。

## 2026-08-11 PERF-005 Snapshot 无变化 0 Reset

**做了什么：**

- `BatchObservableCollection.ReplaceAll` 支持内容比较器（`Func<T,T,bool>`）并返回是否实际替换；内容未变化时不再发任何 `CollectionChanged`（从“1 次 Reset”降到“0 次”）。
- 新增 `SnapshotComparers`：为 Games/Tasks/Findings/Audit/Backups/SaveCandidates/Media/MediaSources/GameTools/ProcessMappings/DeviceComparisons 提供覆盖 UI 字段的内容比较，Task 覆盖 Progress/State/Message/Error 等高频变化字段。
- `DashboardViewModel.Replace` 透传比较器；Media 只在集合变化时才 `MediaView.Refresh()`；GamePicker `SetItems` 在内容未变化时跳过 Clear/Add/Refresh，避免整表重建。

**为什么这样做：**

Snapshot 每次返回全新 DTO，即使内容相同，旧的引用比较也会触发 Reset 并让 WPF 重建 CollectionView/ItemContainer；这是大库高频刷新的主要浪费。

**修改文件：**

- 新增 `src/GameSaveCenter.Playnite/Infrastructure/SnapshotComparers.cs`
- `src/GameSaveCenter.Playnite/Infrastructure/BatchObservableCollection.cs`
- `src/GameSaveCenter.Playnite/ViewModels/DashboardViewModel.cs`
- `src/GameSaveCenter.Playnite/ViewModels/GamePickerViewModel.cs`
- 新增测试 `tests/GameSaveCenter.Playnite.Tests/BatchObservableCollectionTests.cs`
- `tests/GameSaveCenter.Playnite.Tests/GamePickerViewModelTests.cs`

**测试结果：**

- Core 13/13、Worker 23/23、Playnite 156/156（新增 4 个 0 通知/变化检测测试）通过。
- `scripts/render-qa.ps1` 全绿（7 页面 × 5 尺寸，无 `PROBLEM`）。

**性能变化：**

- 内容相同的 Snapshot：Games/Tasks/Findings/Media/GameTools 等集合 0 次 CollectionChanged（测试覆盖）。
- GamePicker：相同内容跳过重建，不再每次快照发 Reset。

**仍需验证内容：**

真实 Playnite 大库下高频刷新/任务进度事件的实际 UI 卡顿改善。

**下一步：**

NEXT: PERF-006 Task/Media 搜索防抖。

## 2026-08-11 PERF-006 Task/Media 搜索防抖

**做了什么：**

- 新增轻量 `DebouncedRefresh`（180ms）：快速连续输入只保留最后一次 Refresh；清空搜索框立即刷新；`Cancel()` 在页面/ViewModel 卸载时取消，避免 Timer 泄漏。
- `TaskSearchText` / `MediaSearchText` 改用防抖；原来的 `[PERF] TaskSearch/MediaSearch refresh` 日志保留在最终刷新动作中。
- 新增 4 个防抖单元测试：空查询立即刷新、快速输入只刷一次、取消阻止待执行刷新、清空会取消待执行刷新并立即刷新。

**为什么这样做：**

每次按键都执行 `ICollectionView.Refresh()` 会让大列表在输入过程中反复重新过滤/布局；防抖后连续输入 `abcdef` 只执行约 1 次最终 Refresh。

**修改文件：**

- 新增 `src/GameSaveCenter.Playnite/Infrastructure/DebouncedRefresh.cs`
- `src/GameSaveCenter.Playnite/ViewModels/DashboardViewModel.cs`
- 新增测试 `tests/GameSaveCenter.Playnite.Tests/DebouncedRefreshTests.cs`

**测试结果：**

- Core 13/13、Worker 23/23、Playnite 160/160 通过。

**性能变化：**

- 连续输入场景：Task/Media 搜索从 N 次 Refresh 降为约 1 次（测试覆盖）。

**仍需验证内容：**

真实 Playnite 大列表输入体验与滚动流畅性。

**下一步：**

NEXT: GAME-TOOL-001 自定义游戏启动项（EXE/LNK/BAT/CMD/PS1）。

## 2026-08-11 GAME-TOOL-001 自定义游戏启动项基础能力

**做了什么：**

- 新增 `GameToolLaunchKind` 与 `GameToolLaunchKinds`：按扩展名分类 EXE / LNK / BAT / CMD / PS1 / 系统默认程序，并明确只有 EXE（含可解析快捷方式目标）可安全跟踪进程。
- 新增 `GameToolLauncher`：EXE 支持 Arguments / WorkingDirectory / RunAsAdministrator；LNK 通过 WScript.Shell 解析 TargetPath/Arguments/WorkingDirectory 后优先运行真实目标；BAT/CMD 通过 `cmd.exe /d /s /c`，PS1 通过 `powershell.exe -NoProfile -ExecutionPolicy Bypass -File`；其他文件用 `UseShellExecute` 交给系统默认程序。
- 修改器中心新增“+ 添加启动项”入口，文件选择器覆盖 EXE/LNK/脚本/所有文件；导入默认 `CopyIntoLibrary=false`，保留外部路径引用，不复制文件。
- 工具卡片类型显示改为“自定义启动项”，Inspector 显示外部引用提示；`CloseOnGameExit` 对不可跟踪类型禁用。

**为什么这样做：**

复用现有 GameTool 模型与字段，不新建第二套数据库；不同文件类型必须采用不同启动策略，不能统一 `Process.Start(path)`。

**修改文件：**

- `src/GameSaveCenter.Contracts/Enums.cs`
- `src/GameSaveCenter.Contracts/TrainerDtos.cs`
- 新增 `src/GameSaveCenter.Worker/Services/GameToolLauncher.cs`
- `src/GameSaveCenter.Playnite/ViewModels/DashboardViewModel.cs`
- `src/GameSaveCenter.Playnite/Views/TrainerCenterView.xaml`
- 新增测试 `GameToolLaunchKindsTests.cs`、`GameToolLauncherTests.cs`

**测试结果：**

- 启动策略单测覆盖 EXE/LNK→EXE/LNK→文档/BAT/CMD/PS1/普通文件/缺失文件/管理员参数。

## 2026-08-11 GAME-TOOL-002 自定义启动项生命周期与安全关闭

**做了什么：**

- 新增 `GameToolSessionTracker`：按 SessionId 记录 GameSaveCenter 自己启动的 PID + 启动时间 + CloseOnExit；游戏退出时只关闭本 Session 且 CloseOnExit 的进程，不按进程名杀、不跨 Session。
- `GameToolService` 自动启动流程接入 tracker：延迟取消、启动记录、退出关闭；只有可跟踪类型且用户开启时才实际关闭。
- 自定义导入支持单文件/任意类型并保持外部路径；`SaveSelectedGameToolAsync` 保存时自动纠正“不可跟踪类型开启退出后关闭”与“系统默认程序开启管理员”。

**为什么这样做：**

CloseOnGameExit 的安全边界必须按 PID + StartTime + Session 确认，避免误杀用户游戏前已打开的同名工具。

**修改文件：**

- 新增 `src/GameSaveCenter.Worker/Services/GameToolSessionTracker.cs`
- `src/GameSaveCenter.Worker/Services/GameToolService.cs`
- 新增测试 `GameToolServiceImportTests.cs`、`GameToolSessionTrackerTests.cs`

**测试结果：**

- Worker 45/45、Playnite 160/160、Core 13/13 通过；render-qa 全绿；技能静态审查 0 error。

**仍需验证内容：**

真实 Playnite 中导入 LNK/BAT/PS1、自动随游戏启动、延迟与退出关闭的真机流程。

**下一步：**

NEXT: PERF-007 媒体缩略图异步化。

## 2026-08-11 PERF-007 媒体缩略图异步化

**做了什么：**

- 新增 `AsyncThumbnailLoader`：File IO + BitmapImage 解码全部移到后台线程，最多 3 个并发，解码后 Freeze，LRU 缓存 96 项，缓存 key 覆盖路径/宽度/文件长度/最后修改时间。
- 新增 `AsyncThumbnailImage` 控件：先显示空占位，后台解码完成后回 UI 只更新对应 item；路径被替换或控件卸载时取消过期加载。
- 媒体列表缩略图（96px）与选中媒体大图预览（480px）都改用异步控件；原 `MediaThumbnailConverter` 保留兼容。
- `scripts/validate-source.py` 门禁同步支持 `PreviewWidth="96"` 异步缩略图写法，并校验异步加载器存在。

**为什么这样做：**

原绑定转换器在 UI 线程同步做 File.Exists/FileInfo/FileStream/Decode，大量截图滚动时会卡 UI；异步化后滚动只触发可见项加载，且并发有界。

**修改文件：**

- 新增 `src/GameSaveCenter.Playnite/Converters/AsyncThumbnailLoader.cs`
- 新增 `src/GameSaveCenter.Playnite/Controls/AsyncThumbnailImage.cs`
- `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml`
- `scripts/validate-source.py`
- 新增测试 `tests/GameSaveCenter.Playnite.Tests/AsyncThumbnailLoaderTests.cs`

**测试结果：**

- Playnite 163/163（新增 3 个异步加载测试）通过；render-qa 全绿；技能静态审查 0 error；`scripts/validate-source.py` 通过。

**性能变化：**

- 缩略图解码不再占用 UI 线程；并发限制 3，避免 200 张图同时 200 个 Task。

**仍需验证内容：**

真实 Playnite 下大量截图滚动的帧率与解码耗时（`[PERF]` 日志 + UI-QA-REAL-001）。

**下一步：**

NEXT: UI-QA-REAL-001 真机回归与最终收尾。
