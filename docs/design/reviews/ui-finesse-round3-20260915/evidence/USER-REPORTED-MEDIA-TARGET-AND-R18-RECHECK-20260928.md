# 当前 main 媒体全局目标绑定与 R18-04 复核

日期：2026-09-28

验证起点：`331a57137f755897f309664064a8f8e1adbbd58f`（fetch 后的 `main`/`origin/main`；本记录补充一条测试，不修改生产代码）

## 当前结果

- Release solution 构建 `0 warnings / 0 errors`；XAML 结构检查 `24/24`；`scripts/validate-source.py` 与 `git diff --check` 通过。
- `ReportedWorkspaceLayoutBehaviorTests` 在隔离 WPF testhost 中 `10/10` 通过并正常退出。双主题的媒体 Inbox 布局用例真实创建 WPF 视图，并将全局游戏依次切换到第二个合成游戏、清空、恢复；工具栏摘要和右侧详情都验证了名称、身份 Tooltip、Automation HelpText 的更新。空目标时两处均显示“未选择游戏”。
- `R14ClassificationSelectionTests` `4/4` 通过。
- R18-04 专测 `1/1` 通过；以下四个关联类分别以独立 testhost 串行复跑，合计 `23/23`，无失败、无跳过。

## 23 项的精确组成

| 测试类 | 计数 | 方法与参数 | 覆盖形态 |
| --- | ---: | --- | --- |
| `MediaPageAccumulatorTests` | 6 | `Appending250PagesKeepsBoundedWindowAndSelectedItem`；`AppendingOverlappingPageUpdatesByIdWithoutDuplicating`；`CrossingTheEleventhPageMakesEvictedSelectionExplicitButKeepsPinnedSelection`；`BackendScaleKeepsTheMediaWindowBounded(200, 200)`、`(2000, 2000)`、`(10000, 2000)` | 合成分页、去重、淘汰/固定选择、有界缓存；250 页用例含 collection change 与 Stopwatch 预算，不是屏幕呈现性能 |
| `MediaWindowAnchorContractTests` | 10 | `LoadMoreSurfacesCaptureAndRestoreTheMediaAnchor`；`EvictedWindowHasAnExplicitReloadRouteAndVisibleSelectionSemantics`；`PurposeNavigationUsesDedicatedMediaAndSaveTabState`；`GridScrollTemplateReservesTheRealContentViewport`；`AnchorDiagnosticsRecordExecutionAndSkipReasonsWithoutChangingScrollSemantics`；`AnchorUsesTheRowsPresenterScrollViewerWhenTemplatesExposeMultipleViewers`；`AnchorViewerSelectionPrefersRowsPresenterOverALargerOuterViewer`；`CurrentMediaCardsUseTheBoundedVirtualizingPanel`；`StaleRestoreCallbackCannotSurfaceEvictedAnchorAfterContextInvalidation`；`EvictedAnchorNoticeReleasesSelectionRestoreGuard` | 7 项源码结构/绑定/状态契约，3 项隔离 STA WPF 视图行为（滚动查看器选择、过期回调负例、淘汰提示释放选择保护） |
| `MediaInboxGeometryTests` | 3 | `ReadableFloorUsesTableChromeAndFrameChromeIndependently`；`ProductionInboxKeepsFourRowsOrExposesThePageFallback`；`NarrowInboxKeepsThePageScrollChannelWhenFooterWraps` | 1 项几何计算，2 项隔离 STA 窗口检查四行预算、页级滚动回退与 footer 换行后的可达性 |
| `R07SelectionAnchorBehaviorTests` | 4 | `StableIdentityWinsOverChangedRowPosition`；`MissingIdentityClampsToNeighborInsteadOfFirstRow`；`SaveCandidateRestoreUsesNeighborWhenStablePathWasRemoved`；`DataGridSelectionUsesResolvedNeighborAfterRefresh` | 3 项纯选择解析/路径身份行为；1 项隔离 STA WPF DataGrid 删除选中行后恢复邻项 |
| **合计** | **23** | **10 项纯数据/几何/选择行为 + 7 项源码契约 + 6 项隔离 STA WPF 行为** | **不等于 23 项真实 Playnite 宿主交互** |

R18-04 专测单独计数，不包括在以上 23 项内。方法/参数也可在既有 [R18-04 复核记录](R18-04-CURRENT-MAIN-RECHECK-20260924.md)中查看；本次 TRX 对应当前拉取后的主线。

## 本次 R18-04 实际采样

| 表格 | 后端项 / UI 项 | 最大实现容器 / 最大可见行 | 本次 8 次滚动样本中的最大耗时 |
| --- | ---: | ---: | ---: |
| Task | 2,000 / 2,000 | 9 / 7 | 63.899 ms |
| Task | 10,000 / 10,000 | 9 / 7 | 32.968 ms |
| Task | 20,000 / 20,000 | 9 / 7 | 27.796 ms |
| Media Inbox | 2,000 / 2,000 | 7 / 7 | 0.027 ms |
| Media Inbox | 10,000 / 2,000 | 7 / 7 | 0.217 ms |
| Media Inbox | 20,000 / 2,000 | 7 / 7 | 0.463 ms |

Task 保持 Recycling/Item 和列虚拟化；Media Inbox 保持 Standard/Item、2,000 项窗口缓存。TRX 中的滚动时间是隔离 STA 测试调用 `ScrollToVerticalOffset` 并同步布局后的时长，不是用户输入延迟、DWM presented frame 或真实宿主帧耗时。此前提供的另一共享模板/窗口样本中 Media Inbox 最大实现容器为 14、最大约 0.03 ms；本次 R18 专测直接创建 `MediaCenterView` 的 1,280×720 测试窗口，读数为 7 和 0.027/0.217/0.463 ms。两组保留为不同测试上下文的样本，不相互覆盖，也不据它们推导真实滚动性能。

R18 testhost 的 TRX 记录了 6 次 WPF `TextServicesHost.OnUnregisterTextStore` / `InvalidComObjectException` 清理文本；`MediaWindowAnchorContractTests` TRX 另有 2 次。对应 xUnit 结果分别为 `1/1` 和 `10/10`，两个 `dotnet test` 进程均退出 `0`。清理文本和通过结果都如实保留，根因未知。

## 证据与边界

原始结果：

- [R18-04 专测 1/1](R18-04-CURRENT-MAIN-331A-20260928.trx)
- [MediaPageAccumulatorTests 6/6](R18-04-23-MEDIA-PAGE-ACCUMULATOR-331A-20260928.trx)
- [MediaWindowAnchorContractTests 10/10](R18-04-23-MEDIA-WINDOW-ANCHOR-331A-20260928.trx)
- [MediaInboxGeometryTests 3/3](R18-04-23-MEDIA-INBOX-GEOMETRY-331A-20260928.trx)
- [R07SelectionAnchorBehaviorTests 4/4](R18-04-23-SELECTION-ANCHOR-331A-20260928.trx)
- [双主题媒体全局目标 WPF 绑定/布局 10/10](USER-REPORTED-MEDIA-TARGET-WPF-BEHAVIOR-331A-20260928.trx)
- [R14 分类选择回归 4/4](R14-MEDIA-CLASSIFICATION-331A-20260928.trx)

远端当前 main 已包含媒体 Inbox 改用顶部全局 `SelectedGame`、移除重复局部选框的实现；本次只补真实 WPF 绑定行为/清空负例与可访问身份文本验证。此处不改 R 行或 `106/83/1/1/1` 统计。没有启动 Playnite、安装候选包或访问真实存档/媒体/云端；隔离窗口、合成游戏数据和离屏布局不能证明当前用户安装包、Playnite 嵌入宿主、物理 DPI、UI Automation 实机输入、呈现帧或 ETW 性能。后续可执行产品项仍按 R23-08 准入条件等待新复现或环境变化。
