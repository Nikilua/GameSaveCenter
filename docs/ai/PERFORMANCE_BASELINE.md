# GameSaveCenter 性能基线

> 维护时间：2026-08-12
> 本文件记录性能测量方法、当前基线数字与待真机验证项。不要伪造数字；没有实测的写“待验证”。

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
