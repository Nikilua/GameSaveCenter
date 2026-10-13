# R06-07 当前 main 行为复核

日期：2026-10-13

任务：`R06-07 | 空表保留结构`

当前代码结论：现有实现满足本任务的状态区分、表头保留和可达重试要求；本机隔离行为复核通过。对应 Actions #962 已终态 Failure，但完整日志暂时不可读取，因此 CI 门禁仍未收口。

## 基线与复核方式

- 当前 main HEAD：`87d27592086bbef355b9426a00614b5bb9856e8c`；生产实现及测试程序集在 `73d0f6e6f94cd87b4b575a3663b76e4989ee8fcd` 构建。执行 `git diff --exit-code 73d0f6e6f94cd87b4b575a3663b76e4989ee8fcd HEAD -- src tests` 无差异，HEAD 前进仅包含文档。因此复用该 Release/net472 程序集覆盖当前 main 源码，不把较早二进制冒称为新编译。
- SDK：本机隔离 SDK `8.0.423`；VSTest `17.11.1`；目标 `net472`。这次没有重新编译，复用 R06-06 已完成的同一源码提交 Release 构建：`0 warning / 0 error`、XAML `24/24`。本机结果不替代 CI SDK。
- 为避免旧程序集读取不同 checkout 的源码，定向执行仅选取不调用 `TestRepositoryContext` 的实际 WPF/状态行为用例。源码身份用上述 `src tests` 零差异检查约束；源契约类未从旧程序集执行。

## 行为验证

| 隔离套件 | 结果 | 覆盖 |
|---|---:|---|
| `R06EmptyStateBehaviorTests.SaveTablesKeepHeadersAndSeparateLoadingEmptyCompletedAndFailureStates` | 1/1 | 实例化生产 `SaveCenterView` 和 DataGrid；确认历史 7 列、候选 4 列持续存在；实际绑定在 Loading、首次 Empty、候选处理完成、Error 间切换；失败优先显示 presenter、折叠空文案；执行真实绑定的 `LoadDetailsCommand` 一次。 |
| `WorkspaceStatePresenterBehaviorTests` | 8/8 | 失败重试按钮命中测试与命令绑定；Loading 覆盖层阻断底层输入并隐藏重试；Enter/Space 与框架 Button 点击实际派发；`CanExecute=false` 时键盘和点击均不执行。 |
| `MediaWorkspaceStateCacheTests` | 7/7 | 同一上下文失败降级并保留成功时间；新上下文不会借用旧缓存；取消不伪造 Ready；缓存时间文案和 Tooltip 有边界覆盖。 |

总计 `16 passed / 0 failed / 0 skipped`。原实现与历史跨页面空表渲染记录仍见 [初始 R06-07 证据](../R06-07-EMPTY-TABLE-STRUCTURE-20260918.md)。本轮重验补的是当前 main 的真实 WPF 状态与输入行为，不以 `Assert.Contains` 源码断言代替交互。

脱敏 TRX SHA-256：

- `R06EmptyStateBehaviorTests-source-73d-current-code.trx`：`6A6689A895AE97B5875E2438C295B8E30B98A734994040F178A4D490E536E1EC`
- `WorkspaceStatePresenterBehaviorTests-source-73d-current-code.trx`：`443E9101C501E4946683080F5884A8EC136739A668C8E79A9507080A51127B45`
- `MediaWorkspaceStateCacheTests-source-73d-current-code.trx`：`519E4FB2F538607C9242807C90971E9E8E004AD43EDB75DEA8CAF9EB92513D4E`

## 视觉与未验证边界

- 当前 checkout 不含原始 Demo `DesignShellView.xaml` 与 `Pages/` 路径；没有引入新的视觉体系，沿用恢复生产基线。
- 旧 `.tmp/r06-07-emptytables` 报告和 PNG 已不存在；历史报告绑定 `5046bf8f`，而后续 XAML/状态代码发生过变化。本轮没有重跑全量 RenderHarness，也没有把旧图写成当前 main 视觉证据。当前行为测试在 STA 中显示真实 WPF 窗口并核对表格结构、状态显隐和命令，但未保存截图。
- 合成状态与隔离窗口没有启动 Playnite/Worker；未验真实宿主 UIA/读屏、OS 输入/IME、物理 DPI/跨屏、最终呈现帧、ETW、宿主性能、真实服务失败时序或用户实际点击。未读取/修改真实存档、媒体、云端或诊断数据。

## 远端门禁与下一步

- 用户随后提供 #962 诊断包。该 run 的 Release 构建成功，CI SDK `8.0.423`、VSTest `17.11.1`；失败位于无关的 `R08NumericChangeBehaviorTests.CounterChangeUsesRealRenderPulseAndSuppressesHighFrequencyBounce`：反馈计数实际 `0`、期望 `1`。摘要和原始测试输出已核对，详见 [R08-06 CI 门控复核](../R08-06-CURRENT-MAIN-RECHECK-20261013/README.md)。这不是 R06-07 失败。
- 下一步：等待 R08-06 修正提交的 Actions 终态，再继续 `R07-01 滚动所有权` 当前 main 复核。R06-07 本机行为结论不变；真实 Playnite、物理 DPI/跨屏、UIA/读屏等边界仍未验证。
