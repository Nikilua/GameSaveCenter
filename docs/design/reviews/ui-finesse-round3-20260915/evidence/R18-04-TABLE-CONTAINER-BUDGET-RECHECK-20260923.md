# R18-04 表格容器预算定向复核（2026-09-23）

## 此前复核身份（f175c57d）

- 仓库：`D:\workplace\github\GameSaveCenter`
- 分支：`codex/ui-finesse-round2`
- 构建身份：`f175c57dda95f6eee9031f0363349189c4e47bb9`
- 本批没有改生产代码。复用 `64843642` 已完成的有限视口修复、生产 Task/Media DataGrid、`MediaPageAccumulator` 与当前共享模板；没有从 `main` 复制或覆盖实现。
- R18-04 专测在隔离 Release testhost 中通过 `1/1`。相关媒体分页、锚点、几何和稳定选择行为回归合计 `23/23`，精确组成见下表。

## f175c57d 采样

探针使用生产 `TaskCenterView` 与 `MediaCenterView`、合成 DTO、隔离 STA WPF Window。Task 窗口为 `1100×640 DIP`，Media Inbox 窗口为 `1280×720 DIP`；响应式有限布局在首次 measure 前应用。每个场景执行 8 次滚动并测量实际 DataGridRow 容器、完整可见行和滚动更新耗时。

| 页面 | 后端数据 | UI 窗口项数 | 虚拟化 | 视口行 | 最大已实现行容器 | 最大可见行 | 滚动 p95 / 最大 (ms) |
| --- | ---: | ---: | --- | ---: | ---: | ---: | ---: |
| Task | 2,000 | 2,000 | Recycling / Item，列虚拟化开启 | 7 | 9 | 7 | 60.790 / 60.790 |
| Task | 10,000 | 10,000 | Recycling / Item，列虚拟化开启 | 7 | 9 | 7 | 30.387 / 30.387 |
| Task | 20,000 | 20,000 | Recycling / Item，列虚拟化开启 | 7 | 9 | 7 | 38.595 / 38.595 |
| Media Inbox | 2,000 | 2,000 | Standard / Item，列虚拟化关闭 | 14 | 14 | 14 | 0.026 / 0.026 |
| Media Inbox | 10,000 | 2,000 | Standard / Item，列虚拟化关闭 | 14 | 14 | 14 | 0.027 / 0.027 |
| Media Inbox | 20,000 | 2,000 | Standard / Item，列虚拟化关闭 | 14 | 14 | 14 | 0.031 / 0.031 |

Task 的 8 个滚动样本原序列（毫秒）：

```text
2k:  3.446, 60.790, 29.471, 17.186, 14.369, 22.132, 22.388, 21.943
10k: 0.481, 19.327, 30.387, 17.524, 16.999, 27.988, 16.881, 22.097
20k: 0.738, 18.601, 23.234, 17.802, 14.813, 38.595, 18.008, 30.396
```

Media Inbox 的 8 个滚动样本原序列（毫秒）：

```text
2k:  0.026, 0.021, 0.020, 0.019, 0.019, 0.019, 0.025, 0.023
10k: 0.027, 0.020, 0.025, 0.025, 0.022, 0.026, 0.019, 0.019
20k: 0.031, 0.031, 0.020, 0.019, 0.022, 0.023, 0.022, 0.024
```

当前共享模板和受控窗口的有效内容高度给 Media Inbox 14 行视口，所以实际容器/可见行是 `14/14`。这与旧证据中的 9 行测量属于不同基线；本报告将当前值作为最新采样，不把差异写成虚拟化退化。后端规模增至 10k/20k 时，Media UI 窗口仍由既有 `MediaPageAccumulator` 限制为 2,000 项。Media 的 `Standard / Item / EnableColumnVirtualization=False` 例外保持；Task 继续使用 Recycling 和列虚拟化。

## 23 项相关行为回归组成

四个类按隔离 testhost 分别执行，全部通过；R18 专测不计入这 23 项：

| 测试类 | 数量 | 覆盖内容 |
| --- | ---: | --- |
| `MediaPageAccumulatorTests` | 6 | 250 页有界累积、重叠页按 ID 合并、淘汰与固定选择，以及 200/2,000/10,000 后端规模的 3 个理论用例 |
| `MediaWindowAnchorContractTests` | 10 | 加载更多后滚动锚点、淘汰恢复、滚动模板视口、真实 RowsPresenter 选择、失效回调保护、详情虚拟化和导航状态 |
| `MediaInboxGeometryTests` | 3 | 表格/外框高度预算、四行或页级回退、窄窗页级滚动可达 |
| `R07SelectionAnchorBehaviorTests` | 4 | 稳定身份优先、身份缺失邻项回退、候选路径移除和 DataGrid 刷新后的选中恢复 |
| **合计** | **23** | 另有 `R18TableContainerBudgetTests 1/1` 专测 |

R18 专测会先验证测试程序集与源码身份，再在 2k/10k/20k 下检查容器受限、内部滚动可用、Media 2,000 项窗口上限及 Task/Media 各自的虚拟化配置。它是受控 WPF 行为测量，不是字符串断言签收。

## 构建与运行记录

- 使用 `.\scripts\build.ps1 -Configuration Release -SkipTests -OutputRoot .tmp\r18-04-recheck` 生成当前 checkout 的隔离程序集；XAML `24/24`，Release solution `0 errors / 2 warnings`。两条 warning 都是既有 `MediaCenterView.xaml.cs:706 CS8602`。
- R18 专测与四个回归类分别运行；专测 `1/1`，回归类 `6/6 + 10/10 + 3/3 + 4/4 = 23/23`。每类测试进程返回退出码 `0`。
- `R18TableContainerBudgetTests` 和 `MediaWindowAnchorContractTests` 的进程清理阶段观察到 WPF `TextServicesHost.OnUnregisterTextStore` 的 `InvalidComObjectException` 调试输出；xUnit 均明确报告测试成功，VSTest 总数/通过数一致且进程返回 `0`。本次不将其记为测试失败，也未查明该 WPF testhost 清理噪声的根因。
- 使用的隔离输出和逐类原始日志位于 `.tmp/r18-04-recheck`，不作为长期证据引用，提交前清理。

## 验收边界

此结果来自合成 DTO、生产 WPF 视图、固定逻辑 DIP 窗口和隔离 STA testhost。它证明容器/窗口上限、有限滚动与当前受控窗口中的滚动更新时间；不证明真实 Playnite/package-host 首次 Loaded 时序、物理 DPI/跨屏、UIA/读屏、IME、实际呈现帧、ETW 或宿主性能。Demo 原目录不可用；本批没有访问真实存档、媒体或云端。

下一可执行任务：按用户指定优先复核并校正 R00/R01 当前证据身份与仍未满足的边界；之后继续依赖已满足的 Q/R 小批量，R18-05 保持后续可执行项。

## 2026-09-23 clean commit 复采与 23 项精确组成

用户要求核对相关行为测试的精确组成。为此，在当前 clean commit `5fbfc869ecddec852440ac82b3b0cc94343f3d60` 的隔离 Release 输出中重新执行 R18 专测和四个相关类，并导出独立 TRX。专测 `1/1`；相关行为测试 `23/23`，0 失败、0 跳过。R18 专测仍观察到 `TextServicesHost.OnUnregisterTextStore` 的 WPF `InvalidComObjectException` 清理输出；TRX `Counters` 为 `total=1, passed=1, failed=0`，VSTest 进程 exit `0`。根因未查明。

| 测试类 | TRX 数量 | 精确用例组成 |
| --- | ---: | --- |
| `MediaPageAccumulatorTests` | 6 | `Appending250PagesKeepsBoundedWindowAndSelectedItem`；`AppendingOverlappingPageUpdatesByIdWithoutDuplicating`；`CrossingTheEleventhPageMakesEvictedSelectionExplicitButKeepsPinnedSelection`；`BackendScaleKeepsTheMediaWindowBounded` × 3（backend `200/200`、`2000/2000`、`10000/2000`） |
| `MediaWindowAnchorContractTests` | 10 | `LoadMoreSurfacesCaptureAndRestoreTheMediaAnchor`；`EvictedWindowHasAnExplicitReloadRouteAndVisibleSelectionSemantics`；`PurposeNavigationUsesDedicatedMediaAndSaveTabState`；`GridScrollTemplateReservesTheRealContentViewport`；`AnchorDiagnosticsRecordExecutionAndSkipReasonsWithoutChangingScrollSemantics`；`AnchorUsesTheRowsPresenterScrollViewerWhenTemplatesExposeMultipleViewers`；`AnchorViewerSelectionPrefersRowsPresenterOverALargerOuterViewer`；`CurrentMediaCardsUseTheBoundedVirtualizingPanel`；`StaleRestoreCallbackCannotSurfaceEvictedAnchorAfterContextInvalidation`；`EvictedAnchorNoticeReleasesSelectionRestoreGuard` |
| `MediaInboxGeometryTests` | 3 | `ReadableFloorUsesTableChromeAndFrameChromeIndependently`；`ProductionInboxKeepsFourRowsOrExposesThePageFallback`；`NarrowInboxKeepsThePageScrollChannelWhenFooterWraps` |
| `R07SelectionAnchorBehaviorTests` | 4 | `StableIdentityWinsOverChangedRowPosition`；`MissingIdentityClampsToNeighborInsteadOfFirstRow`；`SaveCandidateRestoreUsesNeighborWhenStablePathWasRemoved`；`DataGridSelectionUsesResolvedNeighborAfterRefresh` |
| **相关行为合计** | **23** | 以上四类；R18 专测不计入 |

clean commit 的 TRX 同时保存了实际 WPF 测量。几何与容器数量和前次一致：Task 仍为 viewport/最大实现/可见 `7/9/7`；Media Inbox 仍为 `14/14/14`，后端到 10k/20k 时 UI 缓存仍为 2,000 项。该次 8 次滚动样本如下：

| 页面 | 后端 / UI 项数 | 滚动 p95 / 最大 (ms) | 原始 8 样本 (ms) |
| --- | --- | ---: | --- |
| Task | 2k / 2k | 51.172 / 51.172 | `3.015, 51.172, 27.597, 14.638, 11.299, 25.428, 12.445, 16.140` |
| Media Inbox | 2k / 2k | 0.031 / 0.031 | `0.028, 0.030, 0.020, 0.019, 0.019, 0.031, 0.019, 0.018` |
| Task | 10k / 10k | 25.651 / 25.651 | `0.498, 16.343, 21.623, 14.160, 12.373, 25.651, 16.768, 18.258` |
| Media Inbox | 10k / 2k | 0.989 / 0.989 | `0.027, 0.034, 0.026, 0.025, 0.026, 0.989, 0.048, 0.029` |
| Task | 20k / 20k | 20.681 / 20.681 | `0.431, 14.061, 17.945, 13.401, 11.387, 20.681, 17.502, 16.820` |
| Media Inbox | 20k / 2k | 0.023 / 0.023 | `0.023, 0.019, 0.019, 0.019, 0.019, 0.019, 0.021, 0.021` |

Media 10k 这一轮含单个 `0.989 ms` 样本，所以该轮最大值不能继续简写成约 `0.03 ms`；其余七次在 `0.025–0.048 ms`。这类隔离 STA Stopwatch 仅报告观察样本，不把排程抖动解释为真实宿主帧时延，也不以重复采样筛除慢值。前一组样本保留在本报告 f175c57d 历史采样表中，供对照测量波动。

构建身份由 `GSC_BUILD_COMMIT` 绑定到 `5fbfc869...`；Release solution 为 `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning，XAML `24/24`。TRX 仅为本机可再生输出，具体用例名和计数已转录到本文件，不纳入 Git。

## main 合并后复核（2026-09-23）

当前分支为 main，源码身份 f55dce61adba84fec96c3e5434e5a8c1e3fa7132。使用该提交隔离 Release 输出运行 R18 专测和相关四类行为回归；测试程序集身份检查通过。R18TableContainerBudgetTests.ProductionTablesStayViewportBoundedAcrossLargeSyntheticDatasets 为 1/1；四类行为测试 TRX 为 23/23，0 失败、0 跳过、VSTest exit 0。测试输出发生顺序不固定，精确用例名和理论展开数仍以本文件“23 项相关行为回归组成”表为准：Accumulator 6（其中 3 个 backend-scale theory 实例）、Anchor 10、Inbox geometry 3、selection anchor 4。

本次 main 采样确认容器和有限页边界：Task 在 2k/10k/20k 下最大已实现容器均为 9、最大可见行为 7；Media Inbox 视口/已实现/可见行为 14/14/14，后端规模 10k/20k 时 UI 项仍为 2,000。Task 保持 Recycling/Item/列虚拟化；Media 保留 Standard/Item/禁列虚拟化例外。

| 页面 | 后端 / UI 项数 | 视口 / 最大容器 / 最大可见 | 8 次滚动原始样本 (ms) | 最大 / 本测试 p95 (ms) |
| --- | --- | --- | --- | ---: |
| Task | 2k / 2k | 7 / 9 / 7 | 3.536, 64.610, 16.537, 16.953, 14.055, 31.025, 16.568, 20.791 | 64.610 |
| Media Inbox | 2k / 2k | 14 / 14 / 14 | 0.027, 0.020, 0.019, 0.019, 0.019, 0.019, 0.021, 0.020 | 0.027 |
| Task | 10k / 10k | 7 / 9 / 7 | 0.656, 17.932, 27.330, 16.420, 16.202, 25.212, 31.580, 21.808 | 31.580 |
| Media Inbox | 10k / 2k | 14 / 14 / 14 | 0.025, 0.024, 0.020, 0.020, 0.019, 0.019, 0.022, 0.019 | 0.025 |
| Task | 20k / 20k | 7 / 9 / 7 | 0.859, 19.233, 26.482, 17.281, 15.436, 24.117, 18.091, 21.208 | 26.482 |
| Media Inbox | 20k / 2k | 14 / 14 / 14 | 0.652, 0.159, 0.039, 0.030, 0.029, 0.029, 0.030, 0.037 | 0.652 |

Media 20k 最新一轮包含 0.652 ms 单样本，故本轮最大值不能写成约 0.03 ms；其余 7 次为 0.029–0.159 ms，其中后 6 次为 0.029–0.039 ms。此前 f175c57d 采样的 Media 最大值为 0.026/0.027/0.031 ms，保留作历史样本，不以重复采样筛掉 main 的慢值。测试内 8 点最近秩 p95 等于最大值；Stopwatch/STA 布局刷新受调度影响，只能表述为这轮受控样本，不能外推为真实 Playnite 帧延迟或宿主性能。

相关测试的原始 TRX 只作本地核对，测试名和计数已转录至本文件，临时输出随后清理；其用例名与真实断言覆盖范围由上表逐项列出，不把 Assert.Contains 源码契约单独当成交互/性能证明。此前 R18/锚点隔离 testhost 收尾时出现过 TextServicesHost.OnUnregisterTextStore InvalidComObjectException；xUnit/VSTest 计数仍成功、进程 exit 0，根因未明。本次 main 两次 VSTest 的 TRX 均为通过，控制台未重现该噪声。

本次仍只使用合成 DTO、生产 WPF 视图、隔离 STA Window 和逻辑 DIP。没有真实 Playnite/package-host 呈现、物理 DPI/跨屏、UIA/读屏、IME、DWM presented frame、ETW 或宿主性能证据；未触碰真实存档、媒体、用户云端或诊断目录。Demo 原目录不可用，沿用恢复的生产基线。

## 当前 main HEAD `922501e7` 精确复核（2026-09-23）

在 main `922501e71c9b77f5c7d227edb4aa42ebe9308d78` 新建隔离 Release 输出后，重新执行 R18 专测及四个关联类，每类使用独立 VSTest 进程并保存本机 TRX。XAML 检查 `24/24`；Release solution `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning；Playnite 目标仍为 `net462`。R18 专测中的程序集身份检查通过。

| 测试类 | TRX total / passed / failed / skipped | 精确展开 |
| --- | ---: | --- |
| `R18TableContainerBudgetTests`（专测，不计入 23 项） | `1 / 1 / 0 / 0` | `ProductionTablesStayViewportBoundedAcrossLargeSyntheticDatasets` |
| `MediaPageAccumulatorTests` | `6 / 6 / 0 / 0` | `Appending250PagesKeepsBoundedWindowAndSelectedItem`；`AppendingOverlappingPageUpdatesByIdWithoutDuplicating`；`CrossingTheEleventhPageMakesEvictedSelectionExplicitButKeepsPinnedSelection`；`BackendScaleKeepsTheMediaWindowBounded(backendCount: 200, expectedCount: 200)`；同一 theory 的 `(2000, 2000)`、`(10000, 2000)` |
| `MediaWindowAnchorContractTests` | `10 / 10 / 0 / 0` | `LoadMoreSurfacesCaptureAndRestoreTheMediaAnchor`；`EvictedWindowHasAnExplicitReloadRouteAndVisibleSelectionSemantics`；`PurposeNavigationUsesDedicatedMediaAndSaveTabState`；`GridScrollTemplateReservesTheRealContentViewport`；`AnchorDiagnosticsRecordExecutionAndSkipReasonsWithoutChangingScrollSemantics`；`AnchorUsesTheRowsPresenterScrollViewerWhenTemplatesExposeMultipleViewers`；`AnchorViewerSelectionPrefersRowsPresenterOverALargerOuterViewer`；`CurrentMediaCardsUseTheBoundedVirtualizingPanel`；`StaleRestoreCallbackCannotSurfaceEvictedAnchorAfterContextInvalidation`；`EvictedAnchorNoticeReleasesSelectionRestoreGuard` |
| `MediaInboxGeometryTests` | `3 / 3 / 0 / 0` | `ReadableFloorUsesTableChromeAndFrameChromeIndependently`；`ProductionInboxKeepsFourRowsOrExposesThePageFallback`；`NarrowInboxKeepsThePageScrollChannelWhenFooterWraps` |
| `R07SelectionAnchorBehaviorTests` | `4 / 4 / 0 / 0` | `StableIdentityWinsOverChangedRowPosition`；`MissingIdentityClampsToNeighborInsteadOfFirstRow`；`SaveCandidateRestoreUsesNeighborWhenStablePathWasRemoved`；`DataGridSelectionUsesResolvedNeighborAfterRefresh` |
| **关联行为测试合计** | **`23 / 23 / 0 / 0`** | **`6 + 10 + 3 + 4`；R18 专测单独计数** |

覆盖形态需与用例名一起理解：`MediaPageAccumulatorTests` 是合成分页/容量/身份行为；`MediaWindowAnchorContractTests` 中 7 项是源码结构/契约断言，另 3 项实际创建 STA WPF 视图/模板验证负责滚动查看器选择、过期回调不显示锚点提示、淘汰提示释放选择恢复保护；`MediaInboxGeometryTests` 包含 1 项几何计算和 2 项实际 STA 窗口布局/页级滚动检查；`R07SelectionAnchorBehaviorTests` 包含 3 项选择解析行为和 1 项实际 DataGrid 刷新选中检查。计数不是 23 项都经过真实宿主交互的主张。

本次专测在 2k/10k/20k 后端规模下得到以下实际容器与滚动结果。8 点测试内 p95 使用最近秩规则，因此与最大值相同；原始值完整保留：

| 页面 | 后端 / UI 项数 | 视口 / 最大已实现 / 最大可见 | 8 次滚动原始样本 (ms) | p95 / 最大 (ms) |
| --- | --- | --- | --- | ---: |
| Task | 2k / 2k | 7 / 9 / 7 | `3.496, 69.227, 14.481, 14.564, 12.013, 31.256, 15.350, 19.450` | `69.227 / 69.227` |
| Media Inbox | 2k / 2k | 14 / 14 / 14 | `0.027, 0.020, 0.022, 0.023, 0.019, 0.023, 0.023, 0.022` | `0.027 / 0.027` |
| Task | 10k / 10k | 7 / 9 / 7 | `0.714, 23.961, 25.567, 16.130, 14.225, 34.083, 14.680, 19.301` | `34.083 / 34.083` |
| Media Inbox | 10k / 2k | 14 / 14 / 14 | `0.026, 0.024, 0.019, 0.020, 0.022, 0.022, 0.019, 0.019` | `0.026 / 0.026` |
| Task | 20k / 20k | 7 / 9 / 7 | `1.198, 17.374, 36.431, 16.628, 13.598, 25.935, 13.828, 17.820` | `36.431 / 36.431` |
| Media Inbox | 20k / 2k | 14 / 14 / 14 | `0.032, 0.023, 0.020, 0.019, 0.024, 0.023, 0.019, 0.023` | `0.032 / 0.032` |

Media UI 窗口在 10k/20k 后端下仍为 2,000 项；Media 继续使用 Standard/Item、关闭列虚拟化，Task 继续使用 Recycling/Item、开启列虚拟化。与较早 main 采样相比，当前三档 Media 最大值均约 `0.03 ms`；前次 20k 的 `0.652 ms` 是另一次保留的样本，不覆盖本次值，也不据此声称真实宿主滚动帧性能。

清理输出在 TRX 中有记录：R18 专测 testhost 输出 `TextServicesHost.OnUnregisterTextStore` 的 `InvalidComObjectException` 文本 6 次，`MediaWindowAnchorContractTests` testhost 输出 2 次。两进程的 TRX 计数均全通过，五个 `dotnet test` 进程均 exit `0`；本次没有把该输出改判为测试失败，也未查明其清理阶段根因。原始 TRX 位于 `.tmp/r18-04-current-main-20260923/trx`，完成证据同步后清理；此处保留用例名、计数、采样及边界作为持久记录。
