# R08-02 热关闭动画

## 结论

R08-02 在当前分支已满足当前可控验收条件，提交为 `459de0de`。先核对已有实现：Dashboard 已监听应用视觉设置与 `SystemParameters.StaticPropertyChanged`，侧栏已有关闭动效归一化；Settings 也已有系统参数监听和动画开关事件。本项补齐的缺口是设置页自身入场时钟和通用 hover/入场时钟没有统一的安全收口。

实现使用按 WPF `Dispatcher` 隔离的弱引用动效登记表；关闭动画时递增 generation、清理 opacity/translate 时钟并写回中性值。`AnimateEntrance` 的旧完成回调也受 generation 保护。没有改变服务、DTO、命令/Binding、游戏选框、滚动条、取消/错误/恢复语义。

## 行为与构建证据

当前提交身份隔离输出为 `r08-02-build-459de0de`；完整 solution 输出为 `r08-02-solution-build-459de0de`。两者均为 `0 warning / 0 error`，Playnite 目标为 `net462`；XAML 结构检查 `24/24`，`python scripts/validate-source.py` 和 `git diff --check` 通过。

焦点测试按测试类串行执行以避免多个 STA WPF 窗口互相影响：`R08MotionHotChangeBehaviorTests 1/1`、`R08MotionReverseBehaviorTests 2/2`、`ProductionShellChromeSourceTests 10/10`、`UiFinesseFoundationTests 8/8`，合计 `21/21`。

真实 WPF 行为断言包括：

- Settings 真实 `GameSaveCenterSettingsView` 入场时先确认 `SettingsShell` 的 opacity/Y 时钟运行；切换“启用界面动画”为关闭后，设置值为 `false`、两个时钟均释放、opacity 为 `1`、Y 为 `0`。
- 重新打开动画开关后等待原动画时长，`SettingsShell` 不重新启动旧入场动画，仍保持 opacity `1`、Y `0` 且没有活动时钟。
- 现有真实生产 shell 的 reduced-motion / unload 行为继续通过：侧栏落在 `72` DIP、内容 opacity `1`、translate `0`，transition 和旧完成回调不会再次写回。

可复现性说明：一次并行的 21 条合跑出现 3 条 Foundation STA 中点/完成值时序抖动（`18/21`）；将同一身份按测试类串行重跑后稳定为 `21/21`。因此本项不把并行合跑结果写成通过，也不把 WPF 测试退出阶段的 `TextServicesContext.InvalidComObjectException` 当成通过依据；该轮 vstest 退出码为 `0`。

## 验收边界

证据使用合成设置、隔离目录、真实生产 WPF 控件和隔离 STA Window/offscreen logical DIP；没有真实存档、媒体、云端或诊断写入。代码监听路径已验证，但没有修改用户 Windows 动画偏好来做系统级切换实验。未验真实 Playnite 宿主、物理 DPI/跨屏、物理输入、UIA/读屏、呈现帧、ETW、宿主性能或 OS reduced-motion 设备行为。Demo 原始目录不可用，沿用恢复生产基线。

下一可执行任务为 R08-03 离屏与隐藏停机。
