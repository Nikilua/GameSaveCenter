# R08-03 当前 main 隐藏/最小化动效停机复核

日期：2026-10-14
源码提交：`31d64e42c2048fe0b7c77b66238b3a15ecf8de39`
本机测试 SDK：`9.0.302`；MSBuild `17.14.13`；VSTest `17.14.1`；.NET Framework `4.8.9181.0`
测试：`R08OffscreenMotionBehaviorTests.IndeterminateProgressPausesForHiddenTabAndMinimizedWindowThenResumes`，`1/1` 通过，进程退出码 `0`

## 行为与能力复用

当前 main 已有 `IndeterminateProgressBehavior`、共享 `ProgressBar` 模板与动画 storyboard；本项未重建暂停服务或改生产代码。此前的 R08-03 源码、模板已在 `0243c108` 等后续提交中变化，故不直接沿用 9 月证据，而是在当前 main 上重新执行实际行为测试。

测试在隔离 STA Window 中使用生产 `DesignTokens.xaml` 的 `PART_Indicator`，通过实际 `TranslateTransform.X` 采样验证：

- 可见时 spinner 位移超过 `0.5 DIP`；
- 切到另一 Tab 后，暂停标记生效，等待 `360 ms` 位移保持到小数后三位，且 `IsIndeterminate` 仍为 `true`；
- 切回后动画再次移动；
- 最小化窗口后，同样暂停 `360 ms`、位移不变且业务忙态保持；
- 还原窗口后动画继续移动。

## 构建与程序集身份

R08-02 阶段的隔离 Release 构建在提交前对与 `31d64e42` 字节内容一致的源码完成，构建结果 `0 warning / 0 error`；随后在 `31d64e42` 上用该构建的测试程序集执行上述测试。构建发生时 Git `HEAD` 仍为父提交 `d3cb0b2d`，所以 ProductVersion 是 `0.6.73+d3cb0b2d`；DLL SHA/MVID、源码 SHA 和脱敏 TRX SHA 分列于 `tests/`，没有把该 ProductVersion 误记为 `31d64e42`。

提交后另做全新隔离构建尝试，MSBuild 在 `_GenerateRestoreProjectPathWalk` / `_GetProjectReferenceTargetFrameworkProperties` 阶段以 exit `1` 退出，但完整诊断只有目标失败、`0 warning / 0 error`，没有 `MSB`/`NU` 根因。该尝试不计作构建通过，也未据此改源码；本项复用了前一阶段成功编译、源内容哈希可核对的程序集。SDK9 本机结果不替代 SDK8 或 CI SDK `10.0.401`。

## 验证边界

窗口与 Tab 为合成 WPF 行为夹具，逻辑 DIP 与时钟/依赖属性采样不代表真实 Playnite 中的呈现帧、宿主 CPU/电量或真实物理最小化输入。真实 Playnite、Narrator/UIA、物理 DPI/跨屏、ETW 与宿主性能仍待验。无视觉改动，未生成截图，也未访问真实存档、媒体或用户配置。`31d64e42` 的 SDK8/SDK10 CI 公开 run 尚未可核实；不得以本机 SDK9 或该定向测试代替 CI 终态。
