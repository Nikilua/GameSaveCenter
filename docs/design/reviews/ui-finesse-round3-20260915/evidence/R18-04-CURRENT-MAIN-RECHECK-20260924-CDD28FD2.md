# R18-04 当前 main 复测：容器预算与 23 项组成（2026-09-24）

## 构建与测试身份

- 在 main 代码身份 `cdd28fd2` 的 Release test assembly 上复测；复用当前生产 `TaskCenterView`、`MediaCenterView` 与排序/分页/锚点实现，没有从旧分支复制实现。
- R18 专测 `R18TableContainerBudgetTests.ProductionTablesStayViewportBoundedAcrossLargeSyntheticDatasets`：`1/1`，0 失败/跳过，VSTest exit `0`。
- 四类关联行为测试分别串行运行：`MediaPageAccumulatorTests 6/6`、`MediaWindowAnchorContractTests 10/10`、`MediaInboxGeometryTests 3/3`、`R07SelectionAnchorBehaviorTests 4/4`，共 `23/23`，0 失败/跳过，四个 VSTest exit 均为 `0`。R18 专测不计入 23 项。
- Release solution 在该身份构建成功，XAML `24/24`、0 errors；保留两条既有 `MediaCenterView.xaml.cs:703 CS8602` warnings。

## 23 项精确组成

| 测试类 | 数量 | 精确用例与理论参数 | 覆盖类型 |
| --- | ---: | --- | --- |
| `MediaPageAccumulatorTests` | 6 | `Appending250PagesKeepsBoundedWindowAndSelectedItem`；`AppendingOverlappingPageUpdatesByIdWithoutDuplicating`；`CrossingTheEleventhPageMakesEvictedSelectionExplicitButKeepsPinnedSelection`；`BackendScaleKeepsTheMediaWindowBounded(200, 200)`、`(2000, 2000)`、`(10000, 2000)` | 纯数据行为：有界累积、按 ID 更新、淘汰/固定选择与三档规模；Stopwatch 是累积代理，不是呈现时间 |
| `MediaWindowAnchorContractTests` | 10 | `LoadMoreSurfacesCaptureAndRestoreTheMediaAnchor`；`EvictedWindowHasAnExplicitReloadRouteAndVisibleSelectionSemantics`；`PurposeNavigationUsesDedicatedMediaAndSaveTabState`；`GridScrollTemplateReservesTheRealContentViewport`；`AnchorDiagnosticsRecordExecutionAndSkipReasonsWithoutChangingScrollSemantics`；`AnchorUsesTheRowsPresenterScrollViewerWhenTemplatesExposeMultipleViewers`；`AnchorViewerSelectionPrefersRowsPresenterOverALargerOuterViewer`；`CurrentMediaCardsUseTheBoundedVirtualizingPanel`；`StaleRestoreCallbackCannotSurfaceEvictedAnchorAfterContextInvalidation`；`EvictedAnchorNoticeReleasesSelectionRestoreGuard` | 7 项生产源码契约；3 项隔离 STA WPF 查看器选择、失效回调负例、淘汰提示及保护释放行为 |
| `MediaInboxGeometryTests` | 3 | `ReadableFloorUsesTableChromeAndFrameChromeIndependently`；`ProductionInboxKeepsFourRowsOrExposesThePageFallback`；`NarrowInboxKeepsThePageScrollChannelWhenFooterWraps` | 1 项独立几何计算；2 项隔离 STA 生产页行为，验证四行预算/回退及窄窗滚动可达 |
| `R07SelectionAnchorBehaviorTests` | 4 | `StableIdentityWinsOverChangedRowPosition`；`MissingIdentityClampsToNeighborInsteadOfFirstRow`；`SaveCandidateRestoreUsesNeighborWhenStablePathWasRemoved`；`DataGridSelectionUsesResolvedNeighborAfterRefresh` | 前 3 项纯选择行为；最后一项为隔离 STA 真实 DataGrid 删除选中项后的邻项恢复 |
| **合计** | **23** | **10 项纯数据/几何/选择行为 + 7 项源码契约 + 6 项隔离 STA WPF 行为** | **不代表 23 项 Playnite 宿主交互** |

## 当前固定窗口复测样本

R18 测试夹具固定 Task 为 `1100×640 DIP`、Media Inbox 为 `1280×720 DIP`。每档滚动 8 次；最大容器与可见行来自实际 `DataGridRow`，不是源数据行数推算。

| 页面 | 后端 / UI 项数 | 视口 / 最大实现 / 最大可视 | 虚拟化 | 8 次滚动原始样本 (ms) | p95 / 最大 (ms) |
| --- | ---: | ---: | --- | --- | ---: |
| Task | 2k / 2k | 7 / 9 / 7 | Recycling / Item / column=True | `3.643, 80.839, 18.925, 19.003, 20.211, 39.268, 18.775, 21.416` | `80.839 / 80.839` |
| Media Inbox | 2k / 2k | 7 / 7 / 7 | Standard / Item / column=False | `0.040, 0.024, 0.036, 0.034, 0.028, 0.027, 0.023, 0.023` | `0.040 / 0.040` |
| Task | 10k / 10k | 7 / 9 / 7 | Recycling / Item / column=True | `0.618, 17.483, 20.712, 33.607, 13.780, 20.089, 17.568, 23.392` | `33.607 / 33.607` |
| Media Inbox | 10k / 2k | 7 / 7 / 7 | Standard / Item / column=False | `0.025, 0.020, 0.052, 0.021, 0.019, 0.018, 0.019, 0.018` | `0.052 / 0.052` |
| Task | 20k / 20k | 7 / 9 / 7 | Recycling / Item / column=True | `0.459, 16.419, 28.492, 17.962, 15.171, 27.287, 17.676, 24.213` | `28.492 / 28.492` |
| Media Inbox | 20k / 2k | 7 / 7 / 7 | Standard / Item / column=False | `0.025, 0.020, 0.019, 0.019, 0.018, 0.018, 0.018, 0.018` | `0.025 / 0.025` |

测试夹具里的 Media UI 项数在 10k/20k 后端场景仍限制为 `2,000`。Task 的本轮最大代理时间为 `80.839 ms`；Media 最大为 `0.052 ms`。Stopwatch 包含 STA Dispatcher 与 `UpdateLayout`，不是 Playnite 呈现帧或真实滚轮延迟。

用户同时补充了另一窗口尺度的实测摘要：Task 仍为最大实现/可见 `9/7`，Media Inbox 为 `14` 行，Media UI 为 `2,000` 条，滚动最大约 `0.03 ms`。这与本文件固定 `1280×720 DIP` 的 Media `7/7/7` 不是同一窗口采样；该摘要没有附原始 TRX/窗口尺寸，故保留为用户报告，不混入本次可复现表。容器数随视口变化不等于缓存上限变化。

R18 专测 TRX 的 WPF TextServices 清理输出包含 `InvalidComObjectException` 6 次；`MediaWindowAnchorContractTests` 有 2 次。对应 xUnit 结果仍分别为 `1/1` 和 `10/10` Passed，VSTest exit `0`；根因未知。其它三份关联 TRX 未含该异常文本。

## 未验边界与原始结果

- 全部数据使用合成 DTO、fake 分页、隔离 STA WPF；未访问真实存档、媒体、云端或诊断。当前没有真实 Playnite/package-host、物理 DPI/跨屏、UIA/读屏、IME、presented frame、ETW 或宿主性能结论。隔离 STA Stopwatch 不冒充呈现性能。
- 23 项方法、理论展开参数与当次 TRX 如上。`R18TableContainerBudgetTests` 单独的 `1/1` 不算进 23。

原始 TRX：

- [R18 专测](R18-04-current-main-20260924/R18TableContainerBudgetTests.trx)
- [MediaPageAccumulatorTests](R18-04-current-main-20260924/MediaPageAccumulatorTests.trx)
- [MediaWindowAnchorContractTests](R18-04-current-main-20260924/MediaWindowAnchorContractTests.trx)
- [MediaInboxGeometryTests](R18-04-current-main-20260924/MediaInboxGeometryTests.trx)
- [R07SelectionAnchorBehaviorTests](R18-04-current-main-20260924/R07SelectionAnchorBehaviorTests.trx)

下一可执行小批量：修复 crash.zip 指向的 `DataGridStableSortController.OnSorting` 空引用，再补实际列头点击行为；共享 DataGrid 行选中几何及 Media Inbox 复用全局游戏选择随后一并验证。
