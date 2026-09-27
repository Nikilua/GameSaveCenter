# R08 动效反向测试时序抖动修复

日期：2026-09-27；代码身份：`b382fb02cde41f65d8daff5166934161eb956e2b`

## 诊断与修复

- 在修复前的当前身份 `2d3eaf42` 上，R08 定向重复运行第 3/5 次复现 `SidebarRapidReversalUsesLatestTargetAndReleasesOldClock` 失败：固定泵送 1000 ms 后侧栏宽度为 `269.6 DIP`、透明度为 `1`，但 WPF 动画 `Completed` 回调尚未清除 `SidebarTransitionRunningForAudit`，旧断言因此失败。不是反向目标或最终布局错误，而是 `DispatcherTimer` 到期和动画时钟完成回调没有同步。
- 将测试改为轮询动画自己的完成状态并设 2 秒超时。Translate 反向测试延长合成时长并等待真实的进行中取样；反向完成条件同时要求 X/Y 动画时钟释放、X/Y 到达最新目标，避免 WPF 首个时钟 tick 前 `IsAnimated=false` 被误当成完成。`PumpDispatcherUntil` 的截止时间改用单调 `Stopwatch`。
- 没有改生产动画实现、XAML、UI、命令或业务行为。测试继续验证反向期间的渲染值连续性、朝新目标推进、最终几何、透明度、完成标记与时钟释放；不以放宽断言消除失败。

## 验证

- `b382fb02` 身份 Playnite.Tests Release 项目构建：`0 warnings / 0 errors`；随后完整 Release solution `--no-restore` 构建：`0 warnings / 0 errors`；XAML 结构检查 `24/24`。
- `scripts/run-playnite-tests-isolated.ps1 -Configuration Release` 在该身份 clean exit `0`，输出 `All Playnite tests passed with WPF classes isolated by process.`；涵盖 111 个 source 类组及全部 105 个 WPF 隔离类，包括 R08 reverse 和 ReportedWorkspace lifecycle。R08 类另连续 5 次单独进程复跑，每次 `2/2`、退出码 `0`。
- 早期修复尝试只等到“动画时钟未标记 active”曾产生过 false positive：测试立即读取到旧值 `0.816` 而非目标 `-8`。最终断言要求时钟释放且值精确收敛后才算完成，未保留该 false positive。

## 边界

本证据只证明自动化 WPF/隔离 testhost 行为，不代表真实 Playnite、物理 DPI、UI Automation、呈现帧或宿主性能通过。没有启动 Playnite；本机仍为单显示器 `\\.\DISPLAY21`，`Win32_Process.CommandLine` CIM 查询仍拒绝访问。R 台账未更改，仍为 192 个唯一 ID、`106/83/1/1/1`；backlog 没有 READY/IN_PROGRESS 产品项。本阶段未创建需要保留的 `.tmp`/`artifacts` 证据目录。
