# GameSaveCenter 性能基线

> 维护时间：2026-09-05
> 本文件记录性能测量方法、当前基线数字与待真机验证项。不要伪造数字；没有实测的写“待验证”。

## E01 隔离规模基线（2026-09-05）

- 入口为 `scripts/e01-scale-baseline.ps1`。full 使用 2,000 游戏、20,000 备份、10,000 任务、5,000 媒体、500 工具，seed `111193 ms`、20 轮 Worker 读/事件/原子写/锁模拟 `523 ms`。
- stress 使用 10,000 游戏、20,000 备份、10,000 任务、50,000 媒体、500 工具，seed `256888 ms`、模拟 `1762 ms`。
- 两档均通过 `SoakDataScaleTests.DataScaleSoakRemainsBounded`；报告记录 managed memory 保留增长 `0 MiB`、句柄/线程增长 `+0/+0`、订阅者残留 `0`、原子写临时文件残留 `0`。输出只保留在本地 `.tmp`，不进入 Git。
- 该夹具覆盖 Worker/SQLite 数据量、查询/事件/原子写/操作锁循环和资源增长，不覆盖真实 Playnite 冷/热首屏、UI 分配、帧间隔、DPI 或用户目录性能；这些继续为 `MANUAL QA REQUIRED`。

## Phase 7 测量记录（2026-08-26）

- Worker 全量数据规模 harness：`GSC_SOAK_DATA_SCALE=1`，2,000 游戏、20,000 备份、10,000 任务、30,000 媒体、500 工具，20 个读/事件/原子写/锁循环；`SoakDataScaleTests.DataScaleSoakRemainsBounded` 通过 `1/1`，耗时约 `3m03s`，记录 `managedGrowth=0 MiB, handles+0, threads+0`。这是 Worker/SQLite 压力证据，不是 Playnite UI 滚动帧率。
- 2,000 游戏合成集合基准（`GamePicker2000_Benchmark_WritesMeasuredTimings`）通过 `1/1`：首次 `SetItems=55ms`、未变化 `2ms`、单项变化 `15ms`、搜索刷新 `215ms`、清空搜索 `196ms`、任务首次/未变化 `ReplaceAll=1/0ms`。原始文件为 `.tmp/phase7-performance/ui-qa/benchmarks/large-library.txt`。
- 当前离屏 Render QA 报告 `.tmp/phase6-responsive-coordinator-render/render-qa-report.txt`：253 个 `render_ms` 样本，范围 `20–3032ms`，平均 `219.01ms`，`render-qa OK`；Light/Dark、四个尺寸组和 Resize 恢复通过。该数字包含 WPF 离屏布局/PNG 输出，不等于真实宿主首屏或页面切换耗时。
- Blur 配置回归现在覆盖 `20→20 DIP`、`78→78 DIP`、`100→100 DIP`，差值分别为 `58 DIP` 与 `22 DIP`，并保持 `RenderingBias.Performance`；本阶段没有修改默认 Blur、动画时长、曲线或新增 BlurEffect。
- 首次打开、真实工作区切换、侧栏动画 UI 线程耗时、主题切换、背景开关内存、DPI 帧率以及 2,000/10,000/30,000 数据在 Playnite 真实页面中的滚动刷新仍为 `MANUAL QA REQUIRED`，因为用户要求跳过 Phase 4，当前环境没有可审计的 Playnite 宿主。不能用上述离屏或 SQLite 数字替代这些证据。

## Phase 0 当前基线（2026-08-26）

- Release RenderHarness 报告：253 张 PNG、11 个窗口尺寸、`render-qa OK`、无 `PROBLEM`；253 个 `render_ms` 样本范围 16–3072ms，平均 238.82ms。报告为 `.tmp/phase0-render-qa/render-qa-report.txt`。
- 当前 2000 游戏合成基准：首次 `SetItems=21ms`、未变化 `SetItems=0ms`、单项变化 `SetItems=26ms`、搜索刷新 `203ms`、清空搜索 `195ms`、任务首次/未变化 `ReplaceAll=0ms`。详细结果为 `.tmp/phase0-baseline-build/ui-qa/benchmarks/large-library.txt`。
- 这些数字来自隔离 WPF/单元测试夹具，不包含真实 Playnite 宿主 IPC、磁盘、DPI、窗口合成和大库可视滚动；不能用来宣称真实帧率或宿主性能已验收。

## 测量设施（PERF-004）

统一使用 `[PERF]` 前缀的 Debug 日志：

- Worker `DashboardService.GetAsync`：`[PERF] DashboardSnapshot fetch=...ms games=... tasks=... findings=... audit=...`
- Playnite `DashboardViewModel.RefreshDashboardAsync`：`[PERF] DashboardSnapshot fetch=...ms apply=...ms games=... tasks=... findings=...`
- Playnite `DashboardViewModel` Task/Media 搜索：`[PERF] TaskSearch refresh=...ms tasks=...` / `[PERF] MediaSearch refresh=...ms media=...`
- Playnite `GamePickerViewModel`：`[PERF] GamePicker setItems=...ms games=...` / `[PERF] GamePicker refresh=...ms filtered=... games=...`
- Playnite `DashboardView.OnNavigationChecked`：`[PERF] WorkspaceSwitch workspace=... layout=...ms`
- Playnite `MediaThumbnailConverter`（兼容路径）与 `AsyncThumbnailLoader`（主路径）：`[PERF] Thumbnail decode=...ms width=... path=...`

规则：

- 只输出 Debug 级别，Release 正常运行不刷屏；需要时通过日志级别/诊断开关查看。
- 每个日志都带数量与耗时，便于对照“数据量 × 耗时”。
- Worker 与 Playnite 各自记录生成/传输/应用三段，便于定位瓶颈在 IPC 还是 UI 应用。

## 待测操作清单

| 编号 | 操作 | 当前状态 |
| --- | --- | --- |
| A | Dashboard 初次加载 | 有 `[PERF] DashboardSnapshot` 日志；真机待验证 |
| B | Dashboard Snapshot 获取 | Worker `[PERF] DashboardSnapshot fetch` 已埋点 |
| C | Snapshot 应用到 UI Collection | Playnite `[PERF] DashboardSnapshot apply` 已埋点 |
| D | 切换各 Workspace | `[PERF] WorkspaceSwitch layout` 已埋点 |
| E | GamePicker 打开/首次加载/输入/筛选/排序 | `[PERF] GamePicker setItems/refresh` 已埋点 |
| F | Task Search Refresh | `[PERF] TaskSearch refresh` 已埋点 |
| G | Media Search Refresh | `[PERF] MediaSearch refresh` 已埋点 |
| H | Media Detail Loading | `[PERF] MediaDetails load/apply` 已埋点 |
| I | Thumbnail Decode | `AsyncThumbnailLoader` 主路径 `[PERF] Thumbnail decode` 已埋点 |

## 已落地的性能优化

- PERF-005：Snapshot 内容未变化时 0 次 CollectionChanged。`BatchObservableCollection.ReplaceAll` + `SnapshotComparers` 内容比较已覆盖 Games/Tasks/Findings/Audit/Backups/SaveCandidates/Media/MediaSources/GameTools/ProcessMappings/DeviceComparisons；GamePicker 相同内容跳过重建。测试：Playnite 156/156，render-qa 全绿。
- PERF-006：Task/Media 搜索 180ms 防抖已实现（`DebouncedRefresh`），连续输入只执行约 1 次最终 Refresh，清空立即刷新，卸载时取消。测试：Playnite 160/160。
- PERF-007：媒体缩略图异步化已实现（`AsyncThumbnailLoader` 用 `Task.Run` 强制 File IO/Decode 离开调用线程、3 并发、LRU 96、Freeze、`[PERF]` 埋点；`AsyncThumbnailImage` 占位加载并在 Unloaded 时取消）。测试：Playnite 171/171，render-qa 全绿。
- PERF-009/010：任务事件合并改为 TaskId 索引 O(1) 更新；命令状态刷新改为 Dispatcher 合帧（一次业务操作内约 1 次 `RaiseCanExecuteChanged`）。测试：Playnite 167/167。
- 下一项：UI-QA-REAL-001 真机回归。

## 离屏渲染基线（2026-08-11，render-qa 报告）

环境：本机，隔离生产插件，假数据，无 Worker/IPC；数字来自 `artifacts/ui-qa/render/render-qa-report.txt` 的 `render_ms`（PNG 保存 + 单帧渲染，包含布局成本）。

### 1040×700

- Overview 111ms；Save 14–24ms；Trainer 12–30ms；Media 15–20ms；Maintenance 16–21ms；Task 19ms；Settings 91–103ms。

### 1280×720

- Overview 32ms；Save 19–22ms；Trainer 15–21ms；Media 18–23ms；Maintenance 20–35ms；Task 28ms；Settings 101–116ms。

### 1366×768

- Overview 34ms；Save 23–32ms；Trainer 19–25ms；Media 22–32ms；Maintenance 25–29ms；Task 28ms；Settings 101–119ms。

### 1600×900

- Overview 47ms；Save 31–39ms；Trainer 30–45ms；Media 36–52ms；Maintenance 37–48ms；Task 51ms；Settings 119–137ms。

### 1920×1080

- Overview 77ms；Save 46–67ms；Trainer 42–62ms；Media 48–66ms；Maintenance 49–63ms；Task 60ms；Settings 126–140ms。

说明：离屏数字只做布局回归参考；真实 Playnite 宿主下的 IPC、磁盘、大库虚拟化和连续操作流畅性仍待 UI-QA-REAL-001。

## 大型游戏库目标

- 100 / 500 / 1000 / 2000 游戏规模下，打开 GamePicker、搜索、排序、切页不应出现明显长时间冻结。
- Snapshot 数据未变化时，目标为 0 次 CollectionChanged（PERF-005）。
- 连续输入 `abcdef` 时 Task/Media 搜索目标约 1 次最终刷新（PERF-006）。

已实现证据（Playnite 171/171）：

- 2000 游戏相同 Snapshot：GamePicker 第二次 SetItems 0 次集合通知。
- 2000 游戏中单游戏状态变化：1 次 Reset、0 次逐项 Add。
- 2000 任务相同 Snapshot：`BatchObservableCollection` 第二次 ReplaceAll 0 次 CollectionChanged。

自动回归门槛：

- `LargeLibraryPerformanceTests.GamePicker2000_Benchmark_WritesMeasuredTimings` 对 2000 条 GamePicker 首次/单项变化更新和任务首次更新设置 5 秒宽松上限；未变化更新设置 1 秒上限。
- 这些上限用于拦截数量级退化或明显 UI 卡死，不替代本机 profiling 数字，也不把真实 Playnite 帧率写成离线测试结论。

## 2000 规模合成 profiling（本机，2026-08-12）

由 `LargeLibraryPerformanceTests.GamePicker2000_Benchmark_WritesMeasuredTimings` 输出（Playnite 171/171 通过，文件在 `artifacts/ui-qa/fixup-tests/playnite/artifacts/ui-qa/benchmarks/large-library.txt`）：

- 2000 游戏首次 `SetItems`：30ms
- 2000 游戏未变化 `SetItems`：0ms
- 2000 游戏单游戏变化 `SetItems`：23ms
- 2000 游戏搜索输入到防抖刷新完成（轮询 FilteredCount 等待实际刷新，含 180ms 防抖）：208ms
- 2000 游戏清空搜索到刷新完成：199ms
- 2000 任务首次 `ReplaceAll`：<1ms（0ms）
- 2000 任务未变化 `ReplaceAll`：<1ms（0ms）

说明：这是合成数据 + 本机无渲染负载的数字，只用于证明大库集合路径没有明显 O(n^2)；真实 Playnite 渲染帧率仍待 UI-QA-REAL-001。

## 待真机验证

- 真实 Playnite 宿主下 Dashboard 初次加载与各 Workspace 切换耗时。
- 1000+ 游戏库 GamePicker 搜索/筛选/排序耗时。
- 大量截图滚动时 Media 页面缩略图解码耗时与 UI 线程占用。
- 100%/125%/150%/175%/200% DPI 下的渲染耗时与流畅性。
