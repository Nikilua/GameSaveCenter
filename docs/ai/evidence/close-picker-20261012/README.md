# CLOSE-PICKER-01 与全量 Release 复验（2026-10-12）

本证据记录了选框刷新线程修复、R09 错误态描边测试校正和完整本机 Release 复验。运行使用合成游戏列表、隔离临时目录与 STA WPF 窗口，没有访问真实存档、媒体、云端或 Playnite 用户目录。

## 选框失败与修复

此前全量运行在删除所选游戏后，回退断言期望首个可见游戏 `Game 0000`，实际得到 `Game 0020`；当时独立的三项测试没有覆盖异步刷新线程。本阶段新增的确定性夹具在 STA Dispatcher 上清空 `SynchronizationContext`：旧实现的 20 ms 延迟刷新运行在 ThreadPool（负例 TRX 记录 owner thread `22`、实际 thread `18`），证实 WPF `ItemsView` 会在所属 Dispatcher 之外刷新。

`GamePickerViewModel` 现在在 STA 创建时捕获所属 Dispatcher；有 `SynchronizationContext` 时仍使用原上下文，缺少上下文时将延迟刷新以 `DataBind` 优先级投递回 Dispatcher。`RefreshNow`、游戏状态刷新和释放路径会取消并释放待处理 debounce，防止旧回调覆盖即时状态。20 ms 输入合并、过滤、隐藏选择、删除后首项回退、键盘选择、虚拟化和有限列表性能语义保留。

- [旧实现的 Dispatcher 线程负例](targeted/DebouncedRefreshWithoutSynchronizationContextRunsOnOwningDispatcher-red.trx)：失败证明刷新发生在线程池。
- [旧测试集独立通过](targeted/R05OptionVirtualizationBehaviorTests.trx)：3/3；说明原用例没有检测线程归属，不用于否定全量失败。
- [修复后 picker 定向测试](targeted/R05OptionVirtualizationBehaviorTests-fixed.trx)：5/5；包括线程归属、立即刷新取消 debounce、删除回退和 2,000 项虚拟化/键盘路径。
- [完整运行中的 picker 类 TRX](full-release/tests/wpf.R05OptionVirtualizationBehaviorTests/wpf.R05OptionVirtualizationBehaviorTests.trx)：最终全量运行通过。

## R09 错误描边测试校正

第一次全量复跑发现 R09 测试仍要求错误态 `Chrome.BorderThickness` 为 2 DIP，但当前生产模板已在 `e12119b` 将两 DIP 错误描边放到独立 `ValidationStroke` 覆盖层，以避免错误状态改变输入内容布局。测试断言与模板架构不一致；生产样式没有在本阶段修改。

测试现改为检查错误态 `Chrome` 保持 1 DIP、`ValidationStroke` 可见且为 2 DIP/不截获命中；值恢复后检查错误层透明度回到 0、Chrome 恢复焦点边框。第一次失败保留在[复现 TRX](targeted/r09-focus-repro.trx)，校正后的最终[全量 R09 TRX](full-release/tests/wpf.R09FocusOutlineBehaviorTests/wpf.R09FocusOutlineBehaviorTests.trx)为 2/2。

## 完整本机 Release 结果

完整脚本 `scripts/build.ps1 -Configuration Release` 于 2026-10-12 07:10:32–08:00:03 UTC 执行，终态 exit `0`。XAML 结构检查 24/24；Release solution 编译 0 warnings / 0 errors；117 份 TRX 合计 1,550 passed、67 skipped、0 failed。随后 `python scripts/validate-source.py` 通过。117 份逐项结果和完整命令/退出码都在[原始诊断摘要](full-release/summary.json)及随附诊断目录中。

| 测试组 | 结果 |
| --- | --- |
| Core | 125/125 passed |
| Worker | 356 passed、1 skipped、0 failed；独立 Worker Named Pipe 用例受当前环境限制 |
| Playnite 源码组（112 类） | 461 passed、27 skipped、0 failed |
| Playnite/WPF 隔离组（114 类） | 608 passed、39 skipped、0 failed；逐类结果见 [CSV 索引](wpf-class-results.csv) |

完整运行中的 Core、Worker、Playnite source 和关键 picker/R09 原始 TRX：

- [Core TRX](full-release/tests/core/core.trx)
- [Worker TRX](full-release/tests/worker/worker.trx) 与 [Worker 控制台日志](full-release/worker-console.txt)
- [Playnite 源码 TRX](full-release/tests/playnite-source/playnite-source.trx) 与 [控制台日志](full-release/playnite-source-console.txt)
- [Release 编译日志](full-release/build-console.txt)

## 源码与构建身份

完整运行时 `HEAD=e12119bffc6d66faee44cfc89e555461fe89205b`，工作树包含本阶段三个源码/测试改动，诊断摘要如实标记 `workingTreeClean=false`。三个改动文件的 SHA-256：

| 文件 | SHA-256 |
| --- | --- |
| `src/GameSaveCenter.Playnite/ViewModels/GamePickerViewModel.cs` | `FFB9DDB390490C67E6DFE210F8C496FF55F02947D63BA5DF3C5A1111DEA4548B` |
| `tests/GameSaveCenter.Playnite.Tests/R05OptionVirtualizationBehaviorTests.cs` | `912AA78C3CE1E12E8AED3EC35E7D5653025E21656788CCFB18E932B2C684CF8D` |
| `tests/GameSaveCenter.Playnite.Tests/R09FocusOutlineBehaviorTests.cs` | `A5AAB9E90C340F93301D8424FF1E30D98B160D39C1265172AEE7A6B01B2CBC98` |

本机 SDK 为仓库锁定的 `8.0.423`；构建程序集的 `ProductVersion` 因构建发生在提交前仍带 `+e12119b...`。完整运行的 Playnite DLL SHA-256 `D7CF98D7366F0E3543FEC86FD4A9C96456F0026462DDFDDAD3DB240306F80FC7`、MVID `19a3e2b3-4060-4e15-a2ad-beffc49f5118`；测试 DLL SHA-256 `7E4B53F945E88008FF4B0B06354264B8ACE5C228EA5C65DF5355208749185133`、MVID `e4533956-3fa0-4cbd-b22c-1777ece876c5`。这些身份属于 e12119b 加上述 dirty 源码的本机候选，不冒称 clean post-commit 身份。

## 未验证边界与下一步

- 此处只证明本机 SDK 8.0.423 的自动化构建/测试。GitHub Actions 使用 SDK 10.0.401 的结果必须单独核对。
- 历史 Actions run `36677346317` 在本次恢复时无法从 GitHub 页/API读取（页面 fetch 无内容，应用内浏览器查询超时）；run `36675309812` 的完整失败日志仍因仓库权限返回 403。两者状态/根因不推断。当前提交推送后须检查新 Actions run，直到终态明确。
- 没有启动真实 Playnite，也没有验证真实宿主呈现、物理 DPI/跨屏、OS 键盘/IME、Narrator/UIA、ETW 或最终 presented frame。原 Demo 目录当前不可用，视觉基准仍是恢复的生产基线。
- 下一可执行任务为 CLOSE-EVID-01：在这次修复提交后的 `main` 上重算 freshness，逐项补 R00/R01 的 10 条 stale 证据；仅在对应行为/负例重验后更新 baseline。CI 无法读取时记录具体限制并继续这组本地证据工作。
