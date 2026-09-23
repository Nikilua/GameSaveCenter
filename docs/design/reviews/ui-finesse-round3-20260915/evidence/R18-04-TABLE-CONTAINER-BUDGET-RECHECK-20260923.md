# R18-04 表格容器预算定向复核（2026-09-23）

## 身份与范围

- 仓库：`D:\workplace\github\GameSaveCenter`
- 分支：`codex/ui-finesse-round2`
- 构建身份：`f175c57dda95f6eee9031f0363349189c4e47bb9`
- 本批没有改生产代码。复用 `64843642` 已完成的有限视口修复、生产 Task/Media DataGrid、`MediaPageAccumulator` 与当前共享模板；没有从 `main` 复制或覆盖实现。
- R18-04 专测在隔离 Release testhost 中通过 `1/1`。相关媒体分页、锚点、几何和稳定选择行为回归合计 `23/23`，精确组成见下表。

## 当前采样

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

Media 10k 这一轮含单个 `0.989 ms` 样本，所以该轮最大值不能继续简写成约 `0.03 ms`；其余七次在 `0.025–0.048 ms`。这类隔离 STA Stopwatch 仅报告观察样本，不把排程抖动解释为真实宿主帧时延，也不以重复采样筛除慢值。前一组样本仍保留在本报告“当前采样”表中，供对照测量波动。

构建身份由 `GSC_BUILD_COMMIT` 绑定到 `5fbfc869...`；Release solution 为 `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning，XAML `24/24`。TRX 目录在 `.tmp/r00-r01-audit-20260923/focused-tests/trx-final-5fbfc869`，仅为本机可再生输出，不纳入 Git。
