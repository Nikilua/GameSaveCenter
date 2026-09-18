# R00-03 动效完成态与生命周期证据

- 既有生产实现：`4414f05` 已为完成事件写回 opacity/位移终态并增加清理；本阶段 `cda168c` 增加真实 Dispatcher 行为复核，不重复实现 Completed。
- clean-tree 探针基线：`cda168ca410bee0a4b0416664452010c9246db96`，报告均为 `WorkingTreeClean=True`、Release 受控 WPF Window、`DpiScale=1.00` offscreen logical DIP。

## 行为测试

- `UiFinesseFoundationTests.MotionAnimationsReleaseClocksAtTheirFinalValues`：完成后 opacity=`1`、Y=`0`、X/Y 时钟均清除。
- `UiFinesseFoundationTests.EntranceMotionReentryKeepsTheCurrentDispatcherValue`：中途采样后立即重入，Y 与 opacity 起点差异分别控制在 `0.8` 与 `0.08` 内；重入完成后无活动时钟且回到 `Opacity=1/Y=0`。
- `ProductionShellChromeSourceTests.ReducedMotionCancelsAnActiveSidebarTransitionWithoutLateClockWrites`：活动侧栏过渡中切换 reduced motion，立即归一到 `72/Opacity=1/X=0`，等待原 1 秒时长后仍不改变终态；既有完成及卸载测试继续通过。
- Release 单节点构建 `0 warning/0 error`；上述 5 项定向测试 `5/5` 通过。

## 生产壳层受控探针

- `motionreentryprobe`：Light `interrupted=139.25 → immediate=139.33 DIP`，Dark `133.26 → 133.33 DIP`；两主题中间态继续到约 `224 DIP`，最终 `270 DIP`、`X=0`、`finalAnimated=False`。
- `motionhotprobe`：两主题均在活动中间态确认 `duringAnimated=True`；关闭动效后 `disabledFinalWidth=72`、`disabledOpacity=1`、`disabledReentryWidth=270`、`disabledX=0`、`disabledReentryAnimated=False`。
- 两个探针 exit `0` 并报告 `MotionReentryProbe OK` / `MotionHotChangeProbe OK`。这是同一生产壳层的受控窗口与审计覆盖，不等价真实 Playnite 输入、Windows 偏好通知、ETW 或物理屏幕呈现帧。

## 2026-09-18 当前分支复核

- 当前 HEAD `e5a12ffaea138c3a3efccaeb82c58d339ae7d494` 的实际 WPF Dispatcher 行为测试：完成态时钟释放、重入保持当前渲染值、Reduced Motion 中途取消共 `3/3` 通过；没有把源码字符串测试当作本批次唯一依据。
- 当前隔离 Release RenderHarness `motionreentryprobe`：Light `103.40 → 103.33 → 168.91 → 270 DIP`、Dark `127.06 → 127.33 → 181.38 → 270 DIP`，最终 `X=0/finalAnimated=False`；`motionhotprobe` 两主题均在中间态 `duringAnimated=True`，热切换后 `disabledFinalWidth=72`、`disabledOpacity=1`、禁用重入立即到 `270/X=0`。两个探针均 `OK`，报告 `WorkingTreeClean=True`、offscreen `DpiScale=1.00`，原始报告目录为 `.tmp/r00-03-04-probes/`。
- 因此 R00-03 的当前可控完成态、取消/卸载清理、重入和热关闭证据已满足；真实 Playnite Loaded/Unloaded 耐久、快速输入、Windows 偏好通知、ETW、物理呈现帧仍保持边界。

## 未验边界与下一步

- 未把受控 Window 结果升级为真实宿主 Loaded/Unloaded 耐久、鼠标/键盘快速输入、ETW 生命周期或物理 DPI/呈现帧签收。
- 下一可执行任务：R00-04 修正 2000 项 GamePicker 基准的每次输入变化与独立超时计时器，并补有限时间负例。
