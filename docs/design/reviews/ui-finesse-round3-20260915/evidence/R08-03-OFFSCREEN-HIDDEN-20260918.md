# R08-03 离屏与隐藏停机

## 结论

R08-03 在当前分支已满足当前可控验收条件，代码提交为 `4a18fe0c`。先核对已有能力：共享 `ProgressBar` 模板的不确定进度条只在 `IsIndeterminate` 变化时启停，实际循环入口是生产壳层按钮忙碌指示器和 Dashboard 后台刷新提示；原实现没有把页面可见性、Tab 隐藏或宿主窗口最小化纳入循环动效控制。

本阶段新增 `IndeterminateProgressBehavior`：按真实 `ProgressBar` 追踪 `Loaded/Unloaded`、有效 `IsVisible` 和宿主 `Window.StateChanged/IsVisibleChanged`，只把不可呈现状态映射为模板的 `PauseStoryboard`，恢复时 `ResumeStoryboard`。没有把 `IsIndeterminate` 改成业务假完成，也没有改变忙碌绑定、命令/DTO、游戏选框、滚动条、取消/错误、恢复保护或有限列表语义。

## 行为与构建证据

当前提交身份的隔离源码根为 `r08-03-source-hiddenstop`；完整 solution Release 构建 `0 warning / 0 error`，Playnite 目标为 `net462`，XAML 结构检查 `24/24`。构建和测试均显式绑定 `GscBuildCommit=4a18fe0c` 与该隔离源码根，避免旧输出或其他 checkout 被当作证据。

焦点测试按类串行执行，合计 `22/22`：`R08OffscreenMotionBehaviorTests 1/1`、`R08MotionHotChangeBehaviorTests 1/1`、`R08MotionReverseBehaviorTests 2/2`、`ProductionShellChromeSourceTests 10/10`、`UiFinesseFoundationTests 8/8`。

`R08OffscreenMotionBehaviorTests` 使用真实共享 `ProgressBar` 模板和实际 storyboard 取样：

- 可见状态下进度条变换在两个 180ms 样本间移动超过 `0.5 DIP`。
- 切换到第二个 Tab 后，行为状态为 paused；等待 `360ms`，指示块位置保持到小数后三位不变。切回原 Tab 后恢复运动。
- 将承载窗口设为 `WindowState.Minimized` 后，行为状态为 paused；等待 `360ms`，位置同样保持到小数后三位不变。恢复 `Normal` 后继续运动。

这组断言验证了循环时钟实际暂停/恢复和隐藏/最小化负例，没有只检查 XAML 文本。相邻 R08-01、R08-02、生产壳层和基础动效回归也在同一提交身份下通过。

资源字典类的较宽相邻运行结果为 `136 passed / 39 skipped / 1 failed`；唯一失败是既有 Save 表格源码期望 `Header="时间" Binding="{Binding CreatedLocal...` 与当前页面字符串不一致，未触及本阶段文件，也未改写为通过，故不归入 R08-03 验收。

## 验收边界

验证使用合成 Tab/ProgressBar、隔离 STA WPF Window、隔离源码/输出目录和 offscreen logical DIP；没有真实存档、媒体、云端或诊断写入。未验真实 Playnite 宿主页面隐藏、实际用户 Tab/最小化链路、物理 DPI/跨屏、呈现帧、UIA/读屏、ETW 或宿主性能；受控 `WindowState.Minimized` 不等价真实 Playnite 嵌入窗口。Demo 原始目录不可用，沿用恢复生产基线。

下一可执行任务为 R08-04 业务完成节奏。
