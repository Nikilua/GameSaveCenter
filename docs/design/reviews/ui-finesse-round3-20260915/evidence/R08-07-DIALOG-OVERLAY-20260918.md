# R08-07 对话框遮罩同步证据

日期：2026-09-18  
实现提交：`afa845a6`（`收口对话框遮罩生命周期`）

## 范围

本项只覆盖 Dashboard 内嵌确认/选择对话框的生命周期与遮罩同步。验证使用合成 DTO、fake 请求和隔离 STA WPF 窗口，不写真实存档、媒体、云端或用户 Playnite 数据。

## 实现事实

- `DialogLifecycleStateMachine` 将 Opening/Open/Closing/Closed 与一次性完成声明分离；关闭期间仍视为 active，重复命令、Esc 和完成回调不能再次进入或重复完成。
- `DialogOverlayMotion` 统一持有遮罩、卡片透明度和 `TranslateTransform` 的入场/退场时钟；退场以卡片位移动画作为唯一完成时钟，完成后才折叠遮罩，并以同持续时间的 Dispatcher watchdog 处理 WPF 时钟不回调的终态边界。
- 现有 `FocusManager.IsFocusScope="True"`、`KeyboardNavigation.TabNavigation="Cycle"` 与 `DirectionalNavigation="Contained"` 保留；关闭完成后才把焦点返还调用前元素。
- 取消、错误、卸载和减动效路径均清理动画、回调和遮罩终态；没有修改命令绑定、取消语义、错误语义、游戏选框或现有滚动条系统。

## 验证结果

验证源基于 `9c4b0a7e` 的隔离临时源目录叠加本提交文件，避免 C: worktree 的 WPF 临时项目路径权限干扰：

- Release solution：XAML `24/24`，构建 `0 warning / 0 error`，包含 Playnite `net462`。
- R08 测试按类隔离进程运行：`16/16` 通过（BusinessFeedback `4/4`、DialogOverlay `3/3`、MotionHotChange `1/1`、MotionReverse `2/2`、NumericChange `3/3`、OffscreenMotion `1/1`、PageSwitch `2/2`）。
- Core：`83/83`；Worker：`310/311`，`1` 项既有环境跳过，无失败。
- `python scripts/validate-source.py`、`git diff --check` 通过。
- R08 合并到一个 testhost 时出现 `6 failed / 10 passed`；逐类单进程全部通过，故将其记录为同一 WPF testhost 的 Dispatcher/视觉资源污染边界，不把合并运行结果写成产品回归，也不把它计为本项通过证据。

## 未验边界

测试是隔离 STA/offscreen logical DIP 与合成数据；没有运行真实 Playnite 安装呈现、物理 DPI/跨屏、真实键盘/IME、UI Automation/读屏、presented frame、ETW 或宿主性能采集。C: 分支工作树直接构建曾因 WPF 生成的 `_wpftmp.csproj` 路径 `Access denied` 失败，D: 隔离源目录的同身份构建用于区分该环境限制。Demo 原始目录仍不可用，本阶段沿用恢复的生产资源基线。

下一项：R08-08 变换所有权，先核对 `GscMotion` 共用可变/冻结 `Freezable` 与其他变换实例的所有权边界。
