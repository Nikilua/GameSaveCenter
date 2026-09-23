# R18-04 当前 main 复核与 23 项测试组成（2026-09-24）

## 当前构建身份与结果

- 当前 main HEAD / 隔离 Release test assembly：`abd7927b`。Release solution build 成功，XAML `24/24`、0 errors；保留两条既有 `MediaCenterView.xaml.cs:703 CS8602` warning。Playnite 为 `net462`，测试为 `net472`。
- R18-04 专测 `R18TableContainerBudgetTests.ProductionTablesStayViewportBoundedAcrossLargeSyntheticDatasets`：`1/1`，0 failed/skipped，VSTest exit `0`；程序集身份门禁通过。
- 四个关联类按类独立串行运行：MediaPageAccumulator `6/6`、MediaWindowAnchorContract `10/10`、MediaInboxGeometry `3/3`、R07SelectionAnchor `4/4`，合计 `23/23`，0 failed/skipped；四个 VSTest 进程均 exit `0`。
- 测试程序集的源码身份门禁与本轮 source 相符。TRX 持久保存于本文末尾列出的五个文件。

## 23 项精确组成与覆盖边界

| 测试类 | 数量 | 精确用例与理论参数 | 覆盖类型 |
| --- | ---: | --- | --- |
| `MediaPageAccumulatorTests` | 6 | `Appending250PagesKeepsBoundedWindowAndSelectedItem`；`AppendingOverlappingPageUpdatesByIdWithoutDuplicating`；`CrossingTheEleventhPageMakesEvictedSelectionExplicitButKeepsPinnedSelection`；`BackendScaleKeepsTheMediaWindowBounded(200, 200)`、`(2000, 2000)`、`(10000, 2000)` | 纯数据行为：2000 项有界累积、ID 去重更新、淘汰/固定选择、三档后端规模；250 页用例还包含 collection change 与 Stopwatch 上限断言，不是 WPF 屏幕性能测量 |
| `MediaWindowAnchorContractTests` | 10 | `LoadMoreSurfacesCaptureAndRestoreTheMediaAnchor`；`EvictedWindowHasAnExplicitReloadRouteAndVisibleSelectionSemantics`；`PurposeNavigationUsesDedicatedMediaAndSaveTabState`；`GridScrollTemplateReservesTheRealContentViewport`；`AnchorDiagnosticsRecordExecutionAndSkipReasonsWithoutChangingScrollSemantics`；`AnchorUsesTheRowsPresenterScrollViewerWhenTemplatesExposeMultipleViewers`；`AnchorViewerSelectionPrefersRowsPresenterOverALargerOuterViewer`；`CurrentMediaCardsUseTheBoundedVirtualizingPanel`；`StaleRestoreCallbackCannotSurfaceEvictedAnchorAfterContextInvalidation`；`EvictedAnchorNoticeReleasesSelectionRestoreGuard` | 7 项读取生产 XAML/C# 并检查模板、绑定/状态、锚点保护与虚拟化契约；3 项创建隔离 STA Window/真实 WPF 控件并验证滚动查看器选择、失效回调负例、淘汰提示和选择保护释放 |
| `MediaInboxGeometryTests` | 3 | `ReadableFloorUsesTableChromeAndFrameChromeIndependently`；`ProductionInboxKeepsFourRowsOrExposesThePageFallback`；`NarrowInboxKeepsThePageScrollChannelWhenFooterWraps` | 1 项验证几何计算（grid `266 DIP`、frame `292 DIP`）；2 项创建 `MediaCenterView` 与隔离 STA Window，检查四行可读预算/外层页级回退、窄窗口 footer 换行后的滚动可达 |
| `R07SelectionAnchorBehaviorTests` | 4 | `StableIdentityWinsOverChangedRowPosition`；`MissingIdentityClampsToNeighborInsteadOfFirstRow`；`SaveCandidateRestoreUsesNeighborWhenStablePathWasRemoved`；`DataGridSelectionUsesResolvedNeighborAfterRefresh` | 前 3 项验证纯选择解析/保存路径身份行为；最后一项创建 STA Window 和真实 WPF DataGrid，删除选中行后检查邻项恢复 |
| **合计** | **23** | **10 项纯数据/几何/选择行为 + 7 项源码契约 + 6 项隔离 STA WPF 行为** | **不是 23 项真实 Playnite 宿主交互** |

R18 专测的 `1/1` 独立于上表。它检查当前测试程序集与 source identity、生产虚拟化设置及 2k/10k/20k 合成源中的有限容器和滚动视口。关联行为用例补分页、锚点、窄窗几何和选择恢复；它们不是 23 个 DataGrid 容器计时样本。

## 当前 R18-04 专测样本

每档执行 8 次 `ScrollToVerticalOffset` 并 `UpdateLayout`；当前窗口由夹具固定为 Task `1100×640 DIP`、Media Inbox `1280×720 DIP`。最大已实现行数和最大完整可视行数均来自实际 `DataGridRow`，不是源行数估算。

| 页面 | 后端 / UI 项数 | 视口行 / 最大已实现 / 最大可视 | 模式 / 单位 / 列虚拟化 | 8 次滚动原始样本 (ms) | p95 / 最大 (ms) |
| --- | ---: | ---: | --- | --- | ---: |
| Task | 2k / 2k | 7 / 9 / 7 | Recycling / Item / True | `3.116, 59.837, 18.026, 26.183, 11.069, 13.662, 23.744, 16.794` | `59.837 / 59.837` |
| Media Inbox | 2k / 2k | 7 / 7 / 7 | Standard / Item / False | `0.027, 0.023, 0.024, 0.032, 0.028, 0.089, 0.038, 0.040` | `0.089 / 0.089` |
| Task | 10k / 10k | 7 / 9 / 7 | Recycling / Item / True | `0.547, 19.427, 14.938, 18.804, 26.136, 18.014, 13.514, 17.575` | `26.136 / 26.136` |
| Media Inbox | 10k / 2k | 7 / 7 / 7 | Standard / Item / False | `0.031, 0.027, 0.024, 0.025, 0.024, 0.024, 0.028, 0.024` | `0.031 / 0.031` |
| Task | 20k / 20k | 7 / 9 / 7 | Recycling / Item / True | `0.433, 13.907, 14.271, 24.464, 12.486, 15.788, 22.701, 17.004` | `24.464 / 24.464` |
| Media Inbox | 20k / 2k | 7 / 7 / 7 | Standard / Item / False | `0.026, 0.019, 0.018, 0.018, 0.017, 0.018, 0.017, 0.020` | `0.026 / 0.026` |

每个样本均报告 `rowsPanelVirtualizing=True` 与 `CanContentScroll=True`。Media 后端扩大到 10k/20k 时 UI 页缓存仍保持 2,000 项；Media 的 `Standard / Item / EnableColumnVirtualization=False` 和 Task 的 `Recycling / Item / EnableColumnVirtualization=True` 均保留。8 个样本的最近秩 p95 与最大值相同。Task 的最高实测约 `59.8 ms`；Media 2k 有一个 `0.089 ms` 样本，10k/20k 最大分别 `0.031/0.026 ms`。Stopwatch 包含受控 STA Dispatcher 和 `UpdateLayout`，不代表宿主帧延迟、屏幕刷新率或真实滚轮体验。

此前 main `922501e7` 的 Media Inbox 同夹具曾测为 `14/14/14`。源码之后出现的布局修正在当前 `abd7927b` 已生效：`b5c7a6d4` 因表格下方 footer 增加筛选预设、可用性、摘要、次操作和失败行，将非表格高度预算从旧 `220 DIP` 改为 `360 DIP`；`da91bd68` 又令 `MediaInboxGrid.Height/MaxHeight` 在包括短窗口回退的所有布局中都保持有限，页级 ScrollViewer 承担窄窗溢出。当前夹具因此测得 `7/7/7`。`14` 与 `7` 是不同源版本/布局预算下的实测值；当前 main 最新事实是 `7/7/7`，不能把旧值覆盖进新样本，也不能据隔离窗口宣布用户 Playnite 截图已经修复。

## 清理日志与未验边界

- R18 专测 TRX 在 WPF TextServicesHost 收尾记录 `InvalidComObjectException` 6 次；MediaWindowAnchor TRX 有 2 次；其他三个相关类没有该异常文本。五份测试结果全部明确通过、VSTest exit `0`；清理噪声根因未知。
- 本轮只使用合成 DTO、fake/有界分页、生产 WPF 页面、隔离 STA Window 和逻辑 DIP。没有访问真实存档、媒体、用户云端或诊断数据。尚未验证真实 Playnite/package-host 下首次 Loaded 顺序、物理 DPI/跨屏、UIA/读屏、IME、屏幕呈现帧、ETW 或宿主性能；没有把 Stopwatch 代理写成物理性能结论。
- `python scripts/validate-source.py`、`scripts/check-xaml.ps1`（24 files）、`git diff --check` 通过。

## 原始 TRX

- [R18TableContainerBudgetTests](R18-04-CURRENT-MAIN-abd7927b-R18TableContainerBudgetTests.trx)
- [MediaPageAccumulatorTests](R18-04-CURRENT-MAIN-abd7927b-MediaPageAccumulatorTests.trx)
- [MediaWindowAnchorContractTests](R18-04-CURRENT-MAIN-abd7927b-MediaWindowAnchorContractTests.trx)
- [MediaInboxGeometryTests](R18-04-CURRENT-MAIN-abd7927b-MediaInboxGeometryTests.trx)
- [R07SelectionAnchorBehaviorTests](R18-04-CURRENT-MAIN-abd7927b-R07SelectionAnchorBehaviorTests.trx)

