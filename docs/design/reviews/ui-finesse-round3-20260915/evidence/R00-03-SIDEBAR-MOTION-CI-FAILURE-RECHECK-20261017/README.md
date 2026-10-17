# R00-03 历史 SDK 10 动效失败日志复核

更新：2026-10-17。只复核历史 GitHub Actions 日志及当前已提交修正，没有改生产动效策略。

## 历史失败输入

| Run | 提交 | Job | Runner / SDK | 结果 |
| --- | --- | --- | --- | --- |
| #919 / 36675309812 | a6b6dfc9430662e3cc5b7fe00862bce95ac5ebb5 | 109758888214 | Windows Server 2025，SDK 10.0.401 | ProductionShellChromeSourceTests 13 项中 11 通过、2 失败 |
| #920 / 36677346317 | 77de7f450431d267026ce2169aa30525b18bf528 | 109765067482 | Windows Server 2025，SDK 10.0.401 | 同一类 13 项中 11 通过、2 失败 |

两份完整 job log 均已由 GitHub 连接读取。失败分别是：

- ReducedMotionCancelsAnActiveSidebarTransitionWithoutLateClockWrites：测试在 `ProductionShellChromeSourceTests.cs:234` 期待过渡时钟已运行，实得 false。
- SidebarTransitionReleasesClocksOnCompletionAndUnloadInAnActualWpfWindow：测试在 `ProductionShellChromeSourceTests.cs:319` 同样期待活动过渡，实得 false。

包装层的 Assert.Null 只是传递 STA 内部异常；两处内层 Assert.True 才是失败断言。后续 TextServices COM shutdown 异常发生在测试失败后的清理阶段，不解释动画时钟未启动。源码结构检查已成功，失败发生在 SDK 10.0.401 的编译/测试步骤，后续 render、静态审查和打包被跳过。

## 根因与修正

当时测试只用 MotionEnabledProvider 固定应用设置为启用，却没有固定 Windows 系统动效偏好。生产侧栏策略同时要求应用设置启用且 GscMotion.IsEnabled(true) 接受当前系统高对比与 ClientAreaAnimation 状态；CI runner 的系统策略可以合法地禁用动画，于是依赖“动画正在运行”的测试前提不成立。

提交 `48b78b3beb0465083f420b1df23cd4c973a06ea8`（2026-10-07）增加了实例级 `SystemMotionEnabledProviderForAudit` 测试输入。需要检查活动动画的夹具显式固定该输入为 true；独立矩阵测试覆盖应用开关与系统偏好的组合，默认策略测试确认生产仍读取真实 `SystemParameters`。该 seam 只作用于测试实例，未绕过或改变生产系统偏好。

`48b78b3` 是当前 main 的祖先。修正后的当前源码已通过 Actions #1056 / run `38962119739`：SDK8.0.423 job `117179503173` 与 SDK10.0.401 job `117179503250` 均完成 Release build、Core/Worker 与 125 个隔离 WPF 测试类；Playnite 全组通过。文档提交 #1057 / run `38969111355` 也已成功。历史 #919/#920 仍记录为真实失败，不因后续修正而改写为成功。

当前 main 的后续确认：HEAD `4cde34ce465a99787ba3ef969a57bacf9e606550` 对应 #1084/run `39070369704` 已整体终态成功。SDK8.0.423 job `117506536858` 与 SDK10.0.401 job `117506537093` 都在 `ProductionShellChromeSourceTests`（第 `17/125` 类）之后继续跑完全部 125 类。两 lane 的 Release build/test 及 Core/Worker/WPF 全组均通过；SDK8 Render QA、XAML 静态审查和扩展包也成功。产品测试源码与 `48b78b3` 修正后的系统偏好输入一致。#1084 验证的是 GitHub runner 上当前 main，不代表外机加载了相同 checkout 或程序集。

## 结论边界

- R00-03 的受控动画结束、清钟和当前双 SDK CI 证据可继续记为已满足；真实 Playnite 生命周期、物理屏幕呈现、OS UIA/IME、ETW 与宿主性能没有因此验收。
- 用户另一台机器的 R09 失败是另一件事。该附件 SHA-256 `F3DDDC6445E0BB1A5E4CD44EEEEC2C2C98834FC8C81482A67E456C5DB5864514` 与先前文本相同；其 `Expected Thickness(2) / Actual Thickness(1)` 第 151 行对应历史 `3f7d0b30` 断言，而当前 main 在 `f13b9e05` 已改为 Chrome 1 DIP 和独立 ValidationStroke 2 DIP。重复文本没有提供新运行的 HEAD、TRX 或程序集身份，不能据此断言当前 main 在该机器仍失败，也不能当作机器已通过。
- 如果该机在当前 main 上产生新的失败，定位需要该次诊断目录的 `summary.json`、R09 console/TRX、test DLL/PDB 与插件 DLL 身份；无需为了补证重跑真实存档或 Playnite 用户数据。
