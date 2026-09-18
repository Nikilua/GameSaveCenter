# R07-05 详情断点稳定

## 结论

R07-05 在当前分支 `codex/ui-finesse-round2` 已满足可控验收条件，代码提交为 `0daca6f0921d3a562c461f194bc6260d09580b69`，已推送到同名远端分支。实现没有覆盖 `main` 的旧实现。

## 实现事实

- 在 `ResponsiveLayoutCoordinator` 增加详情断点滞回：首次内容预算断点为 `980 DIP`；宽布局进入紧凑布局需小于 `972 DIP`，紧凑布局回到宽布局需达到 `988 DIP`。
- Save、Media、Task、Maintenance 四个生产页面复用同一 latch 语义。临界宽度的重复测量不反复切换侧栏/下方详情；真正切换时保留已有详情控件、选择对象和原有 `ScrollViewer`，不新建滚动系统。
- Maintenance 不再在进入紧凑布局时无条件清除诊断/进程详情状态；仅按已选对象和原详情可见性恢复详情打开状态。游戏选框、滚动条、命令/Binding、取消/错误、安全与恢复语义、有限列表和 net462 未改。

## 行为证据

隔离 Release 构建输出：`.tmp\\r07-05-build-0daca6f0`，构建身份与已提交代码一致。

- XAML 结构检查：`24/24`。
- Solution Release：`0 warning / 0 error`；Playnite 目标仍为 `net462`。
- `ResponsiveLayoutCoordinatorTests`：`5/5`。新增测试实际检查 `980→979→981→973→987→980` 不切换，`971` 进入紧凑，`988` 回到宽布局，`TransitionCount=2`。
- `R07DetailsBreakpointBehaviorTests`：`1/1`。真实生产 `TaskCenterView`、合成失败 DTO、隔离 STA WPF Window 中，序列 `1000→979→981→971→979→988` 保持同一选中对象、详情焦点和垂直 offset；紧凑态详情仍可见并移动到 `Grid` 第 4 行第 0 列。
- 相邻回归：`R07ScrollOwnershipBehaviorTests 2/2`、`TaskCenterViewResponsiveTests 7/7`、`R06DetailsBudgetBehaviorTests 2/2`，共 `11/11`。
- 当前提交身份下合并运行上述五个测试类：`17/17`，`0` 失败、`0` 跳过。
- `python scripts/validate-source.py` 与 `git diff --check` 通过。

## 验收边界

测试使用合成 DTO、fake/隔离窗口和 offscreen logical DIP；没有访问真实存档、媒体、云端或诊断输出。没有启动真实 Playnite，也没有把离屏结果写成物理 DPI、跨屏呈现、OS 触控板/鼠标、UIA/读屏、ETW、宿主帧率或性能结论。第一次聚焦测试的 WPF 进程退出阶段曾打印 TextServicesContext 的 COM 清理诊断，但 vstest 退出码为 0；相邻回归复跑为 `11/11` 且退出码为 0。

Demo 原始目录仍不可用，本项沿用已恢复的生产基线；没有自行换用新的设计体系。

下一项可执行任务：R07-06 状态横幅预算。R07-05 尚未覆盖真实 Playnite 宿主拖拽、物理跨屏或 ETW/宿主性能。
