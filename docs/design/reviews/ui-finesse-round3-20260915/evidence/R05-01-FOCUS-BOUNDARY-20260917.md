# R05-01 弹层焦点范围（2026-09-17）

## 结论

R05-01 在当前可控生产范围内已满足。现有生产 Shell 的游戏选框打开后进入 `GameSearchTextBox`；`Tab`/`Shift+Tab` 在 `PickerOverlay` 内循环，不会落到被遮挡的主页面；关闭路径仍复用已有选择、遮罩、`Enter`、`Escape` 和命令语义，并回焦 `GameContextButton`。共享 ComboBox 选项改为方向键拥有选项导航，`ComboBoxItem` 不再成为表单 `Tab` 停靠点。Dashboard 的详情/确认层声明本地焦点范围，并捕获、恢复打开触发器。

没有改动 `GamePickerViewModel`、游戏选框筛选/选择契约、滚动条、命令绑定、取消/错误/恢复保护或有限列表行为；没有引入新的视觉体系。

## 现状核对与负例

此前实际 STA WPF 行为测试打开生产 Shell 游戏选框后，第一次 `Tab` 会把焦点移到遮挡层后的 `RadioButton`，观测为 `Focus=RadioButton, inside=False`。原 `PickerOverlay` 没有容器级 Tab 边界；共享 ComboBox 也没有明确的选项 `IsTabStop` 契约；Dashboard 对话层关闭时没有统一的打开触发器回焦。

## 实现与证据

- 代码提交：`0004999507d0cf27f483ea1234a7a60b4ad7f9b7`，已推送到 `origin/codex/ui-finesse-round2`。
- `PickerOverlay` 和 `DialogOverlay` 使用 `FocusManager.IsFocusScope=True`、`TabNavigation=Cycle`、`DirectionalNavigation=Contained`；`OpenDialog` 保存非弹层后代的当前焦点，`CloseDialog` 在关闭后恢复可见、启用且可聚焦的触发器。
- `ComboBoxItem` 的共享样式明确 `KeyboardNavigation.IsTabStop=False`，保留原生 Alt+Down/F4、方向键、Enter/Escape 与弹层滚动行为。
- `R05FocusBoundaryBehaviorTests` 使用实际生产 Shell 和隔离 STA WPF Window 验证：选框打开即聚焦搜索框、前进/后退焦点不越过遮罩、循环存在至少两个元素、Shift+Tab 可回到最后访问选项、Escape 关闭并回焦上下文按钮；实际生产 Shell ComboBox 选项均为非 Tab 停靠点。定向结果 `3/3`，包含焦点越界负例的修复回归。
- clean Release 构建：XAML `24/24`，构建 `0` 警告、`0` 错误；官方 artifact 定向测试 `3/3`。
- clean RenderHarness：`.tmp/r05-01-render-clean`，绑定完整 SHA，`WorkingTreeClean=True`，Light/Dark，297 张 PNG，`render-qa OK`；`Settings-state-normal`、`Settings-state-dirty` 和 `Overview-1040x700` 已人工抽查。

## 边界

Dashboard `DialogOverlay` 的真实关闭事件顺序因需要 Playnite 插件/宿主而未启动真实宿主，本阶段以生产代码契约和焦点行为源审查覆盖；没有把它写成真实宿主模态自动化证据。ComboBox 测试是隔离 STA WPF 生产资源，不等价真实 OS IME、物理键盘、屏幕阅读器或 Playnite 嵌入输入链。RenderHarness 是离屏 logical DIP（`DpiScale=1.00`），不代表物理 DPI/跨屏、presented frame、ETW 或宿主帧率/性能。没有写真实存档、媒体、云端或发送诊断。

下一可执行任务：R05-02 选项虚拟化焦点。
