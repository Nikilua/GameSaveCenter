# R06-06 当前 main 行内进度复核

日期：2026-10-13。此复核只补当前 main 证据，没有修改生产代码或测试源码。

## 复核对象与本机结果

- 源码身份：main `73d0f6e6f94cd87b4b575a3663b76e4989ee8fcd`。测试中的 `TestRepositoryContext.AssertAssemblyMatchesSource()` 通过，当前输出程序集版本为 `0.6.73+73d0f6e6f94cd87b4b575a3663b76e4989ee8fcd`。
- 构建诊断的 `workingTreeClean=false` 来自构建前已有的 R04/R05/R07 未跟踪证据文件；没有把它们加入本阶段提交。当前本机行为测试随后按类隔离运行，避免非隔离混跑的 WPF fixture 相互影响。
- 本机 SDK `8.0.423`；官方 Release solution build `0 warning / 0 error`、XAML `24/24`。插件目标 `net462`，Playnite 测试目标 `net472`。构建诊断 `testsRequested=false`，测试由独立隔离 runner 随后运行。CI SDK 另行记录；本机结果不替代 `10.0.401`。
- Playnite 隔离测试 `34/34`：R06TaskProgress `4/4`、TaskIndexedCollection `4/4`、TaskEventUiBatcher `3/3`、BatchObservableCollection `3/3`、R06SelectionState `2/2`、R03NumericAlignment `10/10`、R15TaskProgressMetrics `1/1`、R06Sorting `7/7`。
- Worker `TaskQueryPersistenceTests` `9/9`，使用隔离 SQLite 与合成任务。上述共 `43 passed / 0 failed / 0 skipped`。

## 行为证据

- 真实 WPF `DataGrid` 装入 80 个合成任务并滚动到第 42 行。按稳定 `TaskId` 更新该行后，集合事件恰为一个 `Replace`，无 `Reset`；行数、选中索引、选中任务 ID 和逻辑滚动偏移保持，进度值更新为 67。视口/选择断言实际运行，不以源码包含断言替代。
- `TaskStatusDto` 边界验证排队 0 和未知负值显示 `—`，有效 0 显示 `0%`，超界 120 钳制到 `100%`；取消与成功文案不同，快照比较将其视为不同状态。
- 在生产 `TaskCenterView` 的 STA WPF 窗口中，取消提示随 `IsCancellingTask` 从折叠切换为可见，文本为“正在取消…”，保留现有 HelpText。取消命令本身和 Worker 取消/收尾语义未变。
- 现有 XAML 接线检查只作为补充：Task 行/详情绑定 `ProgressValue`/`ProgressDisplay`，总数绑定服务端 `TaskSummary.TotalCount`。Worker 合成 SQLite 测试验证分页条目数与服务端总数/状态摘要分离，搜索过滤、半开日期范围和未知/过期进度速率、ETA 的负例保持。
- 有一次未隔离的混合 WPF VSTest 诊断为 `22/23`：失败测试在 `FindScrollViewer(grid)` 的 `Assert.NotNull` 处得到 null，尚未执行后续 Replace/选择/滚动断言。它不是上述逐类隔离结果的一部分；其来源和 fixture 交互未进一步确定，不作为成功或生产缺陷证据。

## 构建身份

| 文件 | SHA-256 | MVID |
| --- | --- | --- |
| `GameSaveCenter.Playnite.dll` | `1255485869c469c6f036928b7c051e5ec65573bcf4deeef85a742b416782f52c` | `83afd246-7e24-456d-aff1-a275a68fa1b3` |
| `GameSaveCenter.Playnite.Tests.dll` | `65d5d08e4455867e8f4f6071ec30ff9406d7886f271b73e206edc9b2ded82216` | `9acfeb5f-20a8-4860-a3ca-a3601fcb4f31` |
| `GameSaveCenter.Worker.Tests.dll` | `963785967786801155e0e068b64cf14fcb1ac03d5abe4a90ee32c82bbd8582dc` | `eccd4fc1-af25-4690-b408-8ce7c3f886d3` |

关键源码 SHA-256：

| 文件 | SHA-256 |
| --- | --- |
| `TaskIndexedCollection.cs` | `7d1ba598a770a57880f83270d69e9ddd6ddf263db77eb71437fbc69df9ce187c` |
| `DashboardViewModel.cs` | `d285103396ca66d4b7b986807039f5895a861ce3a07a61df39686722fe1b118f` |
| `TaskCenterView.xaml` | `f61cfb7d45f8d0e8d0ee008657d5b19e8212762db7d938db538b68b76ab7a47b` |
| `OperationDtos.cs` | `8b45047c63fca5da5bd17003ed4b51eaa2cfb8b31817e9887b136820af1896a1` |
| `R06TaskProgressBehaviorTests.cs` | `a570b15411f4c7f7a02dca064e5667051245aef293fa4b2e83295a2b8a4c0ed5` |

## 可复查 TRX

归档 TRX 已统一将测试运行名、账号、机器名和部署根替换为本地中性值；计数、测试名、断言和堆栈保留。

| 文件 | 结果 | SHA-256 |
| --- | --- | --- |
| `R06TaskProgressBehaviorTests.trx` | `4/4` | `4fc28467d4f19e798ec5bf05b4129683717fb253c806184a86deee5828f48202` |
| `TaskIndexedCollectionTests.trx` | `4/4` | `97ccf98579c142b5c64f4b52875811b06602ac82f8df0ebd902521e47c9d0d42` |
| `TaskEventUiBatcherTests.trx` | `3/3` | `b79fd708cf69d2aa221a62b170045ec633e0b8af5f8dd7503eced16d2b776f5a` |
| `BatchObservableCollectionTests.trx` | `3/3` | `20b67c95dd1febfeb8dab5a3654226b8b81fbff4e0495c2be5e1c098d4c87be4` |
| `R06SelectionStateBehaviorTests.trx` | `2/2` | `0553789a331f99ab1364643b238f6cc39210dc3a7ccc03e4e18450095ce0a0a3` |
| `R03NumericAlignmentTests.trx` | `10/10` | `c061bb4fe69abe1d4b35d5be77a7e9c00937ee7cc4902fd96adb5fa79a38c0b1` |
| `R15TaskProgressMetricsTests.trx` | `1/1` | `56a39aef4d702037d5b39e5161d969dcc417d15ee558742954023bee57b69fbc` |
| `R06SortingBehaviorTests.trx` | `7/7` | `34400d660b3daa8abaefe7a9f114724af3face861d3582e65ae718721ce0e253` |
| `TaskQueryPersistenceTests.trx` | `9/9` | `65d2a9ba7295d9893ffd96a5d8365e49c617718f5c3f6d6b1d9fc951bdd140ec` |
| `unisolated-mixed-WPF-run-22-of-23.trx` | `22/23; 1 failed` | `2162cb9eff67b40f89f7584e2f343876b9b4d955d1cbbf1b5006b98d24c4fe79` |

## 视觉与未验边界

此复核没有生成当前身份截图。此前 R06-06 的 Light/Dark RenderHarness 图仍绑定生产实现提交 `72fd6d9a`，只能作为该历史身份的离屏 logical DIP 证据，不冒充当前 main 截图或 Playnite 呈现。真实 Playnite、真实长任务/取消竞争、宿主 UIA/Narrator/IME、物理 DPI/跨屏、最终呈现帧、ETW 与宿主性能仍待验；没有读写真实存档、媒体、云端或诊断数据。

本机 SDK `8.0.423` 不替代当前 main 推送所触发的 Actions。#961（commit `73d0f6e6`）最近一次公开检查仍为 `In progress`；相应 CI gate 未收口。旧检查点 #919/#920 的 SDK `10.0.401` 完整日志已在[CI 归档](../CI-R07-08-RESIZE-FAILURE-20261013/README.md)并按 SHA 校验；失败是旧源码侧栏 transition 行为断言，不能替代当前代码的 CI。

下一可执行项：待 #961 到终态并完成本证据提交的 Actions 检查后，复核已实现的 `R06-07` 当前 main 证据；若实现和覆盖仍满足，补身份/行为证据并沿用，继续 `R06-08` 与 R07 账本。
