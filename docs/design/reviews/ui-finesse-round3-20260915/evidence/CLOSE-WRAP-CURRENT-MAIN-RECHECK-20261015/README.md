# CLOSE-WRAP current main 复核

日期：2026-10-15

代码基线：`main` / `b04660e0af2d17daae8c7dec426598e90de738b2`
本阶段生产 UI 未修改；唯一 tracked 改动是 `WorkspaceTreeGovernanceTests` 的过期源码契约。

## 已确认并修正的门禁断言

`DashboardCompatibilityCodeUsesProductionWorkspaceRegistry` 仍要求 Dashboard 直接调用 Trainer/Media 的旧泛型 getter，但当前焦点路由通过 `workspace => ProductionShellView.GetWorkspaceView(workspace)` 访问已注册的页面。测试改为断言通用回调和 Trainer/Media 路由分支。真实 PageHost 创建、导航、页面缓存与焦点路由由 WPF 行为测试覆盖；没有以源码字符串断言代替交互验证。

修改后 Playnite 测试项目 Release 重建为 `0 warning / 0 error`；[build log](tests/playnite-test-project-release-build.txt)。`WorkspaceTreeGovernanceTests`、`R08PageSwitchBehaviorTests` 和 `R10SearchShortcutFocusBehaviorTests` 定向合计 `8/8`，对应 [TRX](tests/workspace-registry-page-switch-current-main.trx)。测试源码 SHA-256：`4A2BCA8E041409DB30CAC38D76B8E607E17147EA9AC413B055BE8B0E9584360F`。

## CLOSE-WRAP / 654 DIP 行为

当前 main 的 `Q14ToolbarAlignmentBehaviorTests` 为 `1/1`，覆盖 Light/Dark `654 DIP` 单行无额外行距、`620 DIP` 折为两行、`660 DIP` 单行及往返布局。它直接验证共享 `WrapPanelRowGapController` 的实际 STA WPF 几何；[TRX](tests/Q14ToolbarAlignmentBehaviorTests-current-main.trx) 通过。因此旧 backlog 中“当前 SDK8 仍复现 654 DIP 单行 margin”的描述已过期，生产共享实现没有在本阶段重建。

## 本机 Release 验证

- 隔离 SDK：`.NET SDK 8.0.423`、MSBuild `17.11.48`、VSTest `17.11.1`。Playnite 插件目标 `net462`，测试目标 `net472`。
- 当前基线的完整 Release solution build：`0 warning / 0 error`；[build log](tests/solution-release-build-b046.txt)；XAML 检查 `24/24`。之后只修改测试源码，重新构建 Playnite 测试项目仍为 `0 warning / 0 error`。
- Core：`125/125`；Worker：`356 passed / 1 skipped / 0 failed`。
- Playnite 源码组：110 个类，`455 passed / 27 skipped / 0 failed`；完整源码组 TRX 见 [playnite-source-current-main.trx](tests/playnite-source-current-main.trx)。
- WPF：122 个类均已执行，`648 passed / 39 skipped / 1 failed`；121 个类无失败。分类汇总见 [wpf-class-results.json](tests/wpf-class-results.json)。隔离脚本在失败处停止后，剩余 78 个类另按相同的单类进程方式执行，`481 passed / 39 skipped / 0 failed`。
- 本机源码结构验证通过；`git diff --check` 通过。

关键文件 SHA-256：source 组 TRX `F977FE8EB42B0A1E7EB1F42E53D75244C585C04234F5B84363BACA844169A06E`；Q14 TRX `A8E4D15FF37AD5C934B9DB502D372996EE2D977DA0A4391504F6DC59C036AEF3`；R05 系统指针 TRX `23BFCAAF72DDD30317291E1C636709D0E1CCB1BEFC3F52991DA72E52236986DD`；WPF class manifest `01F0147DAE576D0805D5993320AE6FDE4245DA1469AC72E99C9336249321A717`。Release 插件 DLL SHA-256 `3CEC3811B0E99D48B21070F2866D997566ECCB860A4375C70DF96B91265CBF1B`；测试 DLL SHA-256 `1A14E23A7EFF9F8DC7D2B0F857C373A3E7CF8A4B06EF2B5FE1779CFE5805B22C`。build identity 使用基线 commit `b04660e0`；修改后的测试源另以文件哈希标识，工作树当时非 clean。

唯一失败为 `R05TooltipTimingBehaviorTests.QuickMousePassesCancelTooltipAndAStationaryDwellOpensIt`。测试已实例化 Window，但在调用 `window.Show()` 前执行 `GetCursorPos`；API 返回 `false`，保存系统指针位置的前置断言失败，该类其余 `2/2` 通过。完整 [TRX](tests/R05TooltipTimingBehaviorTests-current-main.trx) 记录了实际失败。当前环境下系统指针输入不可用，未把此项记为通过，也没有将其改成跳过。全套计数合计 `1584 passed / 67 skipped / 1 failed`，所以本机测试门禁不是全绿。

## CI 附件与边界

用户提供的 #962/#963、#965、#967、#970/#971 diagnostics ZIP 与已有归档的 SHA、提交及 SDK 记录一致；其已知测试问题和修正见 [R08-06 CI 复核](../R08-06-CURRENT-MAIN-RECHECK-20261013/README.md)、[#965 布局校正](../CI-965-LAYOUT-HOST-CURRENT-MAIN-20261013/README.md)、[#967 SDK10 复核](../CI-SDK10-BUILD-VERIFY-20261013/README.md) 和 [#970/#971 诊断分流](../CI-970-971-DIAGNOSTIC-TRIAGE-20261014/README.md)。这些旧 run 不代表本次提交的 Actions 状态；SDK8 本机结果不替代 SDK10.0.401。

本阶段没有启动真实 Playnite，也没有验证真实系统指针、物理 DPI/跨屏、呈现帧或 ETW。推送后须分别查询当前提交的 SDK8 与 SDK10.0.401 Actions 到明确终态。其后按用户顺序重新核对当前 main 的 R00/R01 freshness，再推进依赖已满足的 Q/R 项。
