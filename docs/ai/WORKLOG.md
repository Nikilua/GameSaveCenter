# GameSaveCenter AI 开发工作日志

> 每完成一个有意义的阶段追加一条；只记录对未来开发有帮助的信息。

## 2026-08-12 UI-207-FOLLOWUP 设置滚动所有权与当前游戏上下文收口

**做了什么：**

- 设置页把页面级滚动从 UserControl 根节点收回到共享 `GscRedesignSettingsTabControl` 模板：宽屏分类栏固定 232 DIP、有限高度下垂直 `Auto`，紧凑布局切换为顶部水平 `Auto`；选中项仍自动 `BringIntoView()`，内容区拥有独立 `SettingsScroller`，5 个分类和所有表单字段均可达。
- 新增共享 `GscSelectedGameIconControl`，当前真实 Playnite Icon 只在 Dashboard 顶部/选中头部、Overview、Save、Trainer、Media 的当前游戏上下文表面加载；GamePicker 虚拟化列表保持 initials，Provider 仍为本地优先、48 DIP 解码、OnLoad/Freeze、LRU 48、失败 glyph fallback、无网络。
- GamePicker 默认“已安装”与已有筛选/排序/搜索保持不变；当前选择即使被筛选隐藏也不被静默替换，显示恢复提示并提供“清除搜索 / 显示当前游戏”命令。事件驱动的 `PlayniteGameStarted` 自动定位、持久化选择和停止后保持选择逻辑不变。
- 没有修改任何 DataGrid、命令、Binding、Worker、IPC 或 Playnite 业务数据流；性能基准改为写入临时/显式 `GSC_TEST_ARTIFACT_ROOT`，避免污染测试 bin 输出树。

**测试结果：**

- `python scripts/validate-source.py` 通过；`python .codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .` 0 errors（仓库既有 33 warnings）。
- Playnite Release 构建隔离输出 0 警告/0 错误；Core 13/13、Worker 51/51、Playnite 197/197 通过；`git diff --check` 通过。
- `scripts/render-qa.ps1 -Configuration Release` 全绿；Settings 760/880/920/1100/1400 × 560/700/900 共 15 组探针，5 个分类可见，宽屏导航最小宽度 232 DIP，内容滚动超限时 `Auto` 可滚动。
- 未运行真实 Playnite 宿主内的主题切换、DPI、键盘和连续缩放人工验收；这些仍标记为 `BLOCKED_ENVIRONMENT`。

## 2026-08-12 UI-207 设置布局响应式与当前游戏上下文

**做了什么：**

- Settings 设置页：`SettingsHeader` 显式 `ClipToBounds=False`；宽/窄布局断点统一为 920 DIP；宽布局 5 个分类 `MinHeight=72`、窄布局 `MinHeight=44`；`GscRedesignSettingsTabControl` 分类栏改为 ScrollViewer（宽时垂直 Auto、窄时水平 Auto），选中 Tab 自动 `BringIntoView()`，5 个分类在常用尺寸下完整可达；设置项与保存行为不变。
- 当前游戏自动定位：新增 `GameSelectionResolver`，打开 Dashboard 时按“运行中优先（持久化 id → 最近 GameStarted → 最近活动）→ 上次选择 → 首个已安装 → 空库 null”选择；`GameSaveCenterPlugin` 新增 `PlayniteGameStarted` 事件，`DashboardViewModel` 只在首次打开或新 GameStarted 时切换，普通刷新不抢回用户手动选择；游戏停止后保持当前选择并继续复用 `GamePickerSelectedGameId` 持久化。
- GamePicker 新用户默认筛选改为“已安装”，空值/未知值归一到“已安装”，已有明确配置值保留。
- 当前选中游戏显示真实 Playnite Icon：新增 UI-only `PlayniteGameIconProvider`（48 max decode、OnLoad、Freeze、LRU 48、远程/缺失/异常 fallback 到手柄 glyph），Dashboard 顶部选择器、展开头部与 Overview 当前游戏卡复用同一 `SelectedGameIcon`；GamePicker 列表不加载真实图标。
- 不新增 Timer/进程扫描/IPC/网络 IO；未触碰 DataGrid 滚动相关代码。

**测试结果：**

- `python scripts/validate-source.py`、`git diff --check`、`git fsck --full` 通过。
- Core 13/13、Worker 51/51、Playnite 195/195 通过；解决方案 Release 构建 0 警告/0 错误。
- render-qa 全绿，含 15 组 SettingsLayout 探针（760/880/920/1100/1400 × 560/700/900 DIP，5 个分类全部可见可测）。
- `scripts/dev-install-run.ps1 -Configuration Release -NoStart` 一键构建安装成功，安装到 `%APPDATA%\Playnite\Extensions\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec`（0.6.70）。
- 真实 Playnite 冒烟：启动后 GameSaveCenter 0.6.70.0 正常加载，无异常日志；设置页视觉、自动定位场景、真实 Icon、1080p/4K 与 DPI 人工验收标记 `BLOCKED_ENVIRONMENT`。

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

## 2026-08-12 UI-QA-REAL-001 真机冒烟与最终收尾

**做了什么：**

- 一键构建安装最新扩展（Release 0 警告/0 错误，Core 13、Worker 45、Playnite 163 全过，extension.yaml/DLL 0.6.70）。
- 启动隔离 Playnite 真机冒烟：插件成功加载，Worker 0.6.70.0 自动启动并完成存储初始化，Playnite 进程稳定运行，无新增错误。
- 保存真实宿主截图：`artifacts/ui-qa/real/playnite-real.png`（本会话无法直接预览，已留给用户复核）。
- 离屏 render-qa（7 页面 × 5 常用窗口）与技能静态审查保持全绿；TaskCenter 状态/游戏/类型筛选在 harness 中均显示“全部”默认值。

**为什么这样做：**

真机回归用于确认本轮性能与自定义启动项改动没有破坏 Playnite 宿主加载链路；无法自动判断的画面细节（主题/DPI/键盘/缩放流畅性）写入待验证清单。

**测试结果：**

- 一键构建安装成功；Playnite 真机启动、扩展加载、Worker IPC 自动启动成功。

**仍需人工验证：**

- Playnite 内 Light/Dark/Follow/高对比度主题。
- 100%/125%/150%/175%/200% DPI 与窗口化/最大化/连续缩放。
- 首页/存档/修改器/媒体/任务/维护/设置各页首屏与底部滚动。
- 自定义启动项导入 LNK/BAT/CMD/PS1、随游戏自动启动、延迟与退出关闭真机流程。
- 大量截图滚动的帧率与缩略图异步加载效果。

**下一步：**

NEXT: 按真实 Playnite 使用反馈决定 PERF-008/009/010 是否值得继续；当前无真实 profiling 证据，不为了编号做无收益优化。

## 2026-08-12 PERF-009 / PERF-010 代码级热点优化

**做了什么：**

- PERF-009：新增 `TaskIndexedCollection`，用 TaskId→index 字典替代每次进度事件 `Tasks.ToList().FindIndex(...)` 的 O(n) 拷贝扫描；快照替换后重建索引，事件更新 O(1)，列表仍保持 200 行上限。
- PERF-010：`RaiseCommandStates()` 改为 Dispatcher 合帧刷新（DataBind 优先级），同一 UI 帧内多次属性变化只执行一次全部命令的 `RaiseCanExecuteChanged`；Dispatcher 关闭时回退为立即刷新。

**为什么这样做：**

任务进度是高频更新路径（每次 Progress/Message/State 变化都会到达 Dashboard），列表拷贝 + 线性查找在 200 行规模下也是每次事件的浪费；命令状态在一次业务操作里可能被触发数十次，合并后不会影响 CanExecute 正确性（WPF 在命令执行时仍会查询）。

**修改文件：**

- 新增 `src/GameSaveCenter.Playnite/Infrastructure/TaskIndexedCollection.cs`
- `src/GameSaveCenter.Playnite/ViewModels/DashboardViewModel.cs`
- 新增测试 `tests/GameSaveCenter.Playnite.Tests/TaskIndexedCollectionTests.cs`

**测试结果：**

- Playnite 167/167（新增 4 个任务索引测试）通过；Release 编译通过。

**性能变化：**

- 任务事件合并：O(n) 拷贝+查找 → O(1) 索引更新。
- 命令状态刷新：一次业务操作内 N 次触发 → 约 1 次合帧刷新。

**仍需验证内容：**

真实 Playnite 下高频任务进度事件的 UI 帧率。

**下一步：**

NEXT: 仅剩 PERF-008（按 Workspace 按需刷新）未实施；目标文件要求先有真实 profiling 证据，当前保留为待定，不做无收益重构。

## 2026-08-12 大库 2000 规模回归测试

**做了什么：**

- 新增 `LargeLibraryPerformanceTests`：2000 游戏相同 Snapshot 时 GamePicker 0 次二次集合通知；单游戏状态变化时只发 1 次 Reset 且无逐项 Add；2000 任务相同 Snapshot 时 0 次二次 CollectionChanged。

**为什么这样做：**

目标文件的性能验收要求 1000+ 游戏不冻结，并明确要求用 Unit Test / Instrumentation 证明“相同 Snapshot 从 1 次 Reset 降到 0 次”；补上 2000 规模证据，避免只在小样本上证明。

**修改文件：**

- 新增 `tests/GameSaveCenter.Playnite.Tests/LargeLibraryPerformanceTests.cs`

**测试结果：**

- Playnite 170/170（新增 3 个大库规模测试）通过。

**仍需验证内容：**

真实 Playnite 1000+ 游戏库的 IPC 与渲染帧率。

**下一步：**

NEXT: PERF-008 仍待真实 profiling；当前按 Workspace 按需刷新已有基础（详情只在激活 Workspace 加载），暂不追加重构。

## 2026-08-12 2000 规模合成 profiling 基准

**做了什么：**

- `LargeLibraryPerformanceTests` 增加合成 profiling：测量 2000 游戏 GamePicker 首次/未变化/单变化 SetItems、搜索输入到防抖刷新完成，以及 2000 任务首次/未变化 ReplaceAll；结果写入 `artifacts/ui-qa/benchmarks/large-library.txt`。

**实测结果（本机）：**

- 首次 SetItems 27ms；未变化 0ms；单游戏变化 25ms；搜索端到端 402ms（含 180ms 防抖）；任务 ReplaceAll 均 <1ms。

**结论：**

- 大库集合路径没有明显 O(n^2)；PERF-008 不因 GamePicker/任务集合压力而紧迫，保留按真实 Playnite 渲染 profiling 再评估。

**修改文件：**

- `tests/GameSaveCenter.Playnite.Tests/LargeLibraryPerformanceTests.cs`
- `docs/ai/PERFORMANCE_BASELINE.md`

**测试结果：**

- Playnite 171/171 通过。

**仍需验证内容：**

真实 Playnite 1000+ 游戏库的 IPC/渲染帧率。

**下一步：**

NEXT: 待用户提供真实 Playnite 大库反馈或人工验收结果。

## 2026-08-12 PERF-008 评估收口

**做了什么：**

- 基于 2000 规模合成 profiling 与源码审计，正式评估 PERF-008（按 Workspace 按需刷新）：
  - 详情数据已经在 `RefreshCoreAsync` / `RequestWorkspaceLoad` / `LoadDetailsAsync` 中按当前 Workspace 分支加载（Saves/Trainers/Media 只在激活时请求，Tasks/Maintenance 只加载各自独立数据）。
  - 全量 `DashboardSnapshot` 仅用于全局摘要与任务轮询，后台轮询已有 1 分钟全量 TTL，任务变化走 `GetTaskChanges` 增量。
  - 2000 规模合成 profiling 显示 GamePicker/Tasks 集合路径无 O(n^2)（首次 27ms、未变化 0ms、任务 ReplaceAll <1ms）。

**结论：**

PERF-008 维持现状，不追加按 Workspace 重构；等真实 Playnite 大库渲染 profiling 显示全量快照成为瓶颈时再单独评估。

**修改文件：**

- `docs/ai/WORKLOG.md`
- `docs/ai/PROJECT_MEMORY.md`

**仍需验证内容：**

真实 Playnite 大库 IPC 与渲染帧率（外部环境）。

**下一步：**

NEXT: 全部可自主完成项已收口；剩余为真实 Playnite 人工验收。

## 2026-08-12 验收缺口修复（评审反馈）

按评审意见修复以下语义问题，全部先改实现再跑测试：

### PID 身份安全

- 启动后立即记录 `Process.StartTime`（真实值），不再用 `DateTime.UtcNow` 近似。
- 关闭时要求 PID 相同且实际 StartTime 与记录值在 5 秒容差内双向匹配（`ProcessIdentityGuard`），避免 PID 被复用后误杀新进程。
- 新增真实双进程测试：Session A 关闭、Session B 存活；新增 PID 复用拒绝测试。

### 缩略图真正后台化

- `AsyncThumbnailLoader.LoadAsync` 用 `Task.Run` 强制 File.Exists/FileInfo/FileStream/Decode 全部离开调用线程，即使 Semaphore 立即放行也不会在 UI 线程同步执行。
- `AsyncThumbnailImage` 注册 `Unloaded` 取消待处理加载，`Loaded` 时若占位为空自动重新加载。
- 主路径 `AsyncThumbnailLoader.Decode` 增加 `[PERF] Thumbnail decode` 埋点。

### 自定义启动项管理

- Inspector 新增名称、工作目录、启动参数编辑框；保存时把 `DisplayName/WorkingDirectory/Arguments` 一起提交 Worker。
- 新增“重新定位”命令（仅外部启动项显示），Worker 新增 `tools.relocate` 消息与存储更新。
- LNK 导入/重定位时解析并持久化 `ResolvedTargetPath`；UI 的 `CanTrackProcess/LaunchKindDisplay` 改用解析后目标，LNK→EXE 不再被 UI 误判为不可追踪。
- `game_tool_versions` 增加兼容列 `resolved_target_path`。

### Benchmark 与记忆清理

- 搜索 benchmark 改为轮询 `FilteredCount` 等待真实刷新完成，不再固定睡 400ms；新增清空搜索测量。
- 修正 `PROJECT_MEMORY.md` 里 PERF-005/006 的过期技术债、测试数量与优先级描述。

**测试结果：**

- Worker 49/49、Playnite 171/171 通过；新 benchmark：搜索到刷新完成 208ms、清空 199ms。

**修改文件：**

- `GameToolSessionTracker.cs`、`GameToolService.cs`
- `AsyncThumbnailLoader.cs`、`AsyncThumbnailImage.cs`
- Contracts/Store/Dispatcher/`DashboardViewModel.cs`/`TrainerCenterView.xaml`
- `GameToolSessionTrackerTests.cs`、`GameToolServiceImportTests.cs`、`LargeLibraryPerformanceTests.cs`
- `docs/ai/PROJECT_MEMORY.md`、`docs/ai/PERFORMANCE_BASELINE.md`

**仍需验证内容：**

真实 Playnite 人工验收（主题/DPI/窗口化/自定义启动项真机流程/大库渲染帧率）。

## 2026-08-12 UI-206 DataGrid 滚动几何修复（SUPERSEDED）

> 历史记录保留。该条目的 Pixel 方案已由真实 Playnite A/B 验证为严重回归并撤回；当前最终方案是 `Item` + `GscStableDataGridRow` + geometry probe，见后续条目与 PROJECT_MEMORY。

**做了什么：**

- 共享 `DataGrid` Style 与 `SaveDataGrid` / `TaskDataGrid` / `MaintenanceDataGrid` 显式设置 `VirtualizingPanel.ScrollUnit=Pixel`，保留 `CanContentScroll=True`、虚拟化与 Recycling，不关闭任何性能机制。
- Maintenance/Task 响应式代码不再给 DataGrid 写死运行时 `Height`，改为 `Height=double.NaN` + 有限 `MaxHeight`（保留 MinHeight）。
- 完整诊断摘要：移除外层 `ClipToBounds=True`，删除 code-behind 对外层 Border 的 MinHeight/MaxHeight 动态限制；由页面滚动面负责短窗口可达性，TextBox 保留自身 160 DIP 上限与内部滚动。
- 新增 offscreen 滚动回归探针：`SaveHistoryGrid` / `TaskGrid` / `FindingsGrid` / `MaintenanceAuditFindingsGrid` / `MaintenanceAuditLogGrid`，60 行假数据 × 287/311/337/353/419 DIP 非整行高度，滚到底后断言 `VerticalOffset ≈ ScrollableHeight`，不越界。
- 更新 `WpfUiResourceDictionaryTests`：断言 Pixel ScrollUnit、MaxHeight + Height=NaN、诊断摘要不再被外层裁剪。

**为什么这样做：**

真实 Playnite 2K 窗口化下，DataGrid 按 Item 逻辑滚动与固定 Height 在非整行/高 DPI viewport 上产生“表头大空白 + 末行只露一点”的滚动几何错误；Pixel 滚动 + 有限 MaxHeight 让滚到底的偏移与 ScrollableHeight 一致。

**修改文件：**

- `Themes/WpfUiProduction.xaml`
- `Views/SaveCenterView.xaml`、`Views/TaskCenterView.xaml`、`Views/MaintenanceView.xaml`
- `Views/MaintenanceView.xaml.cs`、`Views/TaskCenterView.xaml.cs`
- `tests/GameSaveCenter.Playnite.Tests/WpfUiResourceDictionaryTests.cs`
- `tests/GameSaveCenter.RenderHarness/Program.cs`、`tests/GameSaveCenter.RenderHarness/FakeDashboardData.cs`

**测试结果：**

- Playnite 172/172、render-qa 全绿（含新增滚动探针，offset==scrollable）、源码验证与技能静态审查通过。

**仍需验证内容：**

真实 Playnite 2K 窗口化/最大化的视觉确认；1080p/4K 环境标记 BLOCKED_ENVIRONMENT（本机暂无法验证）。

## 2026-08-12 QA-HARDENING 收口清理

**做了什么：**

- 修复 `RelocateAsync` WorkingDirectory 语义：WorkingDirectory 如果只是导入时自动等于旧 EntryPath 父目录，重新定位时跟随新文件目录；用户显式自定义的目录保留；旧目录已不存在时回退新文件目录。
- 自定义外部文件导入成功后清理无意义空 GameTools 目录。
- `LargeLibraryPerformanceTests` 等待 helper 超时后现在直接 `Assert.Equal(expected, FilteredCount)`，搜索坏了会测试失败，不再把 5 秒当成“耗时”放行。
- GitHub Actions Windows CI 新增 `scripts/render-qa.ps1` 与 WPF UI validator 步骤，离屏几何 QA 真正进入 CI 门禁。
- 同步 `PROJECT_MEMORY / WORKLOG / README` 到最终状态；删除“DataGrid 必须 Pixel”的过期结论，明确记录 Pixel 经真机验证会回归，当前采用 `Item + GscStableDataGridRow + geometry probe`。
- 测试方法改名：`DataGridsUsePixelScrollUnit...` → `DataGridsUseItemScrollUnitAndStableRowWithoutDiagnosticClip`。

**为什么这样做：**

收口评审指出长期记忆与源码已经漂移（Pixel/Item 相反），继续让未来 Codex 接力会把刚修好的 UI bug 改回去；同时把最有价值的离屏几何 QA 放进 CI，避免本地忘记跑。

**修改文件：**

- `src/GameSaveCenter.Worker/Services/GameToolService.cs`
- `tests/GameSaveCenter.Playnite.Tests/LargeLibraryPerformanceTests.cs`
- `tests/GameSaveCenter.Playnite.Tests/WpfUiResourceDictionaryTests.cs`
- `.github/workflows/windows-build.yml`
- `README.md`、`docs/ai/PROJECT_MEMORY.md`、`docs/ai/WORKLOG.md`

**测试结果：**

- 本地 Playnite 179/179、render-qa 全绿；源码验证与技能静态审查通过。

**最终结论：**

PERF-004～010 与 GAME-TOOL-001/002 主体完成；最近 DataGrid/UI 问题在源码层已经经历回滚与再修复，当前方案为 Item + 稳定行样式 + geometry probe。剩余完整主题/DPI/大库/启动项真机验收仍需用户真实使用反馈，不宣称已完成。

## 2026-08-12 QA-HARDENING-2 文档与边缘清理

**做了什么：**

- `PROJECT_MEMORY.md` 测试基线更新为 Worker 51；TaskFilter 描述改为 `TaskFilterOptionsSync`（不再提 200ms timer）；LNK 可追踪描述与“脚本/普通文件不可靠”分开写清。
- `DEVELOPMENT_HANDOFF.md` 顶部当前基线更新到 `0c6f143`，测试基线更新为 Core 13/Worker 51/Playnite 179；删除“下一步 PERF-008/009/010”过期指示。
- WORKLOG 旧 UI-206 条目标注 SUPERSEDED，避免未来 Codex 误以为 Pixel 是当前方案。
- 外部 CustomExecutable 导入不再预先创建 GameTools root（只有 ZIP/目录/复制需要存储时才创建），失败路径也不会留下空目录；补充“导入失败不留下空目录”测试。

**修改文件：**

- `src/GameSaveCenter.Worker/Services/GameToolService.cs`
- `tests/GameSaveCenter.Worker.Tests/GameToolServiceImportTests.cs`
- `docs/ai/PROJECT_MEMORY.md`、`docs/ai/WORKLOG.md`、`docs/DEVELOPMENT_HANDOFF.md`

**测试结果：**

- Worker 51/51、Playnite 179/179；源码验证通过；远端 `main` 已推送（`bc83562`）。
