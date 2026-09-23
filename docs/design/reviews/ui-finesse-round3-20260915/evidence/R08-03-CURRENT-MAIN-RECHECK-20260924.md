# R08-03 当前 main 复核（2026-09-24）

## 结论

R08-03 既有生产实现仍满足当前可控行为条件；本次在当前 main 补齐了 `IsIndeterminate` 业务状态不变的直接行为断言，没有修改生产代码。原 R08-03 测试已实际取样循环动画暂停与恢复，但没有直接断言任务所要求的忙碌业务状态保持不变；本次在隐藏、最小化、暂停等待和恢复节点逐一检查这一负例。

## 当前身份与测试组成

- 当前生产源码/初始 checkout identity：`378ceb13`。完整 Release solution 构建 XAML `24/24`、0 errors，保留两条既有 `MediaCenterView.xaml.cs:703 CS8602` warnings；Playnite 目标为 `net462`、测试目标为 `net472`。
- 加入上述断言后，Playnite 测试项目重建为 0 warnings / 0 errors。五类测试按独立 VSTest 进程串行执行，合计 `25/25`、0 failed / skipped，所有进程明确 exit `0`：R08 Offscreen `1/1`、HotChange `1/1`、Reverse `2/2`、ProductionShellChrome `12/12`、UiFinesseFoundation `9/9`。
- `R08-03-motion-foundation` 首次筛选器使用了错误的类名，VSTest 显示没有匹配用例；查明实际类名为 `UiFinesseFoundationTests` 后，以该类重跑并记录上述 `9/9`，首次空筛选不计入测试总数。

## 离屏动画行为

`R08OffscreenMotionBehaviorTests.IndeterminateProgressPausesForHiddenTabAndMinimizedWindowThenResumes` 使用真实共享 `ProgressBar` 模板、`IndeterminateProgressBehavior` 和 storyboard，在隔离 STA WPF Window 内观察实际 `PART_Indicator` 的 `TranslateTransform`：

- 可见时实际位移超过 `0.5 DIP`。
- 切至隐藏 Tab 后行为状态变为 paused；等待 `360ms`，位移保持至小数后三位；返回原 Tab 后恢复位移。
- 窗口最小化后行为状态变为 paused；等待 `360ms`，位移保持至小数后三位；恢复窗口后动画继续。
- 每个隐藏、暂停等待与恢复节点均直接断言 `progress.IsIndeterminate == true`，确认暂停视觉时钟没有将业务忙碌状态伪装成完成。

相邻热切换、反向动效、生产壳层 Chrome 和动效基础行为 `24/24` 通过，未改写这些结果。

## 退出诊断与边界

HotChange TRX 有 8 段、Reverse 有 1 段、ShellChrome 有 1 段 `TextServicesHost.OnUnregisterTextStore InvalidComObjectException` 清理输出；Offscreen 与 Foundation TRX 未见该异常。每组 xUnit 均成功，VSTest exit `0`。清理噪声根因未明，不将它改写为产品通过或测试失败。

验证只使用合成 Tab/ProgressBar、隔离 STA WPF Window 和逻辑 DIP。没有证明真实 Playnite 页面切换或嵌入窗口最小化、物理 DPI/跨屏、UIA/读屏、实际呈现帧、ETW 或宿主性能。未访问真实存档、媒体、云端或诊断。Demo 原始目录不可用，沿用恢复生产基线。

历史资源字典宽回归 `136 passed / 39 skipped / 1 failed` 中的 Save 时间列源码字符串期望失败已在原始证据中记录；不归入本次五类当前身份复核。

## 原始结果

- [Offscreen 1/1](R08-03-OFFSCREEN-CURRENT-MAIN-20260924.trx)
- [HotChange 1/1](R08-03-HOT-CHANGE-CURRENT-MAIN-20260924.trx)
- [Reverse 2/2](R08-03-REVERSE-CURRENT-MAIN-20260924.trx)
- [ProductionShellChrome 12/12](R08-03-SHELL-CHROME-CURRENT-MAIN-20260924.trx)
- [UiFinesseFoundation 9/9](R08-03-MOTION-FOUNDATION-CURRENT-MAIN-20260924.trx)

下一可执行任务：R08-04 业务完成节奏。
