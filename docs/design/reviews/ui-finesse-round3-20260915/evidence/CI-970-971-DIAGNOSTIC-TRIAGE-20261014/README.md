# #970 / #971 CI 诊断与当前 main 定向复核

## 用户提供的诊断包

附件是此前已审 ZIP 的逐字节副本，SHA-256 与原包一致；未把 ZIP 原件复制进仓库。#970 SDK10 包提供了此前缺少的失败测试堆栈，#971 附件只有 SDK8 诊断，没有 #971 SDK10 lane 日志。

| 来源 | commit / SDK | 诊断结果 | ZIP SHA-256 |
| --- | --- | --- | --- |
| #970 SDK10（[run 38386326043](https://github.com/Nikilua/GameSaveCenter/actions/runs/38386326043)） | `c9f9179d9b8d38e506dbb1402ae7030eafebea6c` / `10.0.401` | Build、Core 通过；Worker 357 项中 1 项失败：`TaskCoordinatorFailureTests.CancellationIsIdempotentAndPublishesSafeFinalizationBeforeCancelled`，断言期望回调数 `1`、实际 `0`，测试行 128。 | `155612D3A083821531E04D07F57CCAE2CF403647741996395B335FD98AE43EAE` |
| #970 SDK8 | `c9f9179d9b8d38e506dbb1402ae7030eafebea6c` / `8.0.423` | 两次完整 solution-build summary 均通过。诊断脚本中的 controlled failure / missing command 属于预期负例，不是 solution-build 失败。 | `598C2CA619F0CAC37E3E6877B5CBC0EE45AEFC3371DAAACA0F122AE0BC64A8B6` |
| #971 所附 SDK8 诊断（[run 38393317198](https://github.com/Nikilua/GameSaveCenter/actions/runs/38393317198)） | `5a6647cbf8b40b30bc11bf8c6c32015545896967` / `8.0.423` | 所附 solution-build summary 的唯一失败为 Q14 工具栏几何测试。Light `980×640 DIP` 报告的 preset text centers 为 `6,18,18,18,18,18`。这份 SDK8 summary 不能单独确定整个 #971 workflow 的终态。 | `6AC19F269A3B2FDF3618BF9153C313245AE742EADB6E389A5DACDAEE486ADA4C` |

截至 2026-10-14 的公开 run 页面：#970 为 Failure；#971 仍显示 In progress，页面没有列出 artifacts。不能把附件内 SDK8 重跑状态当作 #971 的 SDK10 或整个 workflow 终态。#972 的双 SDK 成功对应更早的 `8546aca`，不覆盖当前 `a78c46b5`。

## 失败原因与已有修正

两个失败都来自测试前提，而不是生产实现：

- #970 的测试先发出 `started` 信号、之后才注册取消 token callback；测试线程可以在注册前取消任务，因此期望回调数为 1 的断言存在竞态。`599d2fd38dd5c9af523a0890a33e73fc78ac0bfa` 将信号移到 callback 注册之后。`TaskCoordinator` 生产代码没有改动。
- #971 的 Q14 fixture 用字符串替换了生产 `TaskFilterPreset` 对象集合，打乱了绑定的选中显示内容；测量代码又可能选到隐藏的零尺寸 presenter。`599d2fd3` 改为选中真实默认预设对象，并只读取可见、有尺寸且文本匹配的选择内容。生产 XAML、样式和布局没有改动。

这两份测试源在当前 main `a78c46b59836bb4dfb408e4bdd467b6e1597e1e4` 与 `599d2fd3` 完全相同；未发现后续回归差异。

## 当前 main 定向重跑

当前执行环境没有仓库 `global.json` 要求的 SDK `8.0.423`，只提供 SDK `9.0.302`。为得到当前源码的独立交叉检查，从仓库父目录启动 `dotnet`，没有编辑 `global.json`。SDK9 结果不替代 SDK8 或 CI SDK10.0.401。

| 定向测试 | 结果 | 证据 |
| --- | --- | --- |
| `TaskCoordinatorFailureTests.CancellationIsIdempotentAndPublishesSafeFinalizationBeforeCancelled` | `1/1` 通过 | [脱敏 TRX](tests/WorkerCancellation-current-main-sdk9.trx)，SHA-256 `9FDE0C88C54F539D4D21081D385CC901F10E8B8D9492DCA0739AC9329537A5BE` |
| `Q14ToolbarAlignmentBehaviorTests.ProductionTaskToolbarsKeepMixedControlGeometryAndTextCenters` | `1/1` 通过 | [脱敏 TRX](tests/Q14ToolbarAlignment-current-main-sdk9.trx)，SHA-256 `5D0553F50F22E28A12B0BFFE5978CED445D6005E7D8D6AFADBEB5E8EBE953464` |

Q14 测试运行结束后 runner 仍输出 WPF `TextServicesContext` / `TextStore` 的 `InvalidComObjectException` 清理噪声；TRX 为 Passed，进程退出码为 0。该现象此前也见于受控 WPF runner；本次没有把它归因于产品或当作通过条件，也没有进一步调查其关闭期来源。

本轮仅复核测试，没有改产品代码；没有启动 Playnite，也未接触真实存档、媒体、用户配置或云端。需继续查新提交对应的 SDK8 与 SDK10.0.401 Actions 到终态；CI 仍运行时不得签收阶段。
