# R05-01 当前 main 焦点边界复核

日期：2026-10-13

状态：已满足（插件可控焦点交互）；真实 Playnite 与完整 Dashboard XAML 加载仍未验。

## 当前身份与验证

- 当前源码/测试 commit：`1c39e909856faa736b32224ee196ef0bac6ca460`。精确提交身份下重建并运行；此前未提交候选运行保留为对照，不替代本次 post-commit 证据。
- 使用隔离 .NET SDK `8.0.423` 构建插件 `net462` 和 Playnite 测试 `net472`；测试宿主为 .NET Framework `4.8.9181.0`。
- 合并定向命令过滤 `R05FocusBoundaryBehaviorTests`、`R05OptionVirtualizationBehaviorTests` 与 `R21FocusVisualRegressionBehaviorTests`，结果 `11 passed / 0 failed / 0 skipped`。提交后脱敏 TRX：[R05-01-02-dashboard-postcommit.trx](tests/R05-01-02-dashboard-postcommit.trx)，SHA-256 `DE45C3F9462F44665BDCD2F068E00630377C413D8669B54F4944F3E988312554`。提交前候选 TRX：[R05-01-02-dashboard-precommit.trx](tests/R05-01-02-dashboard-precommit.trx)，SHA-256 `A8F4E8A68177017A1782B5F60814BC6865073A2190AA4D79762DB9C289333004`；原基线复跑：[R05-01-current-main.trx](tests/R05-01-current-main.trx)，`5/5`。
- 精确提交测试程序集 SHA-256 `9A509F12BBA20BD826ECC3D900DFD21752F0AAA498B9E10A7BE1CF683C52F063`，MVID `8c8895de-7c96-4fe3-bdcb-fc0455c400e4`；插件程序集 SHA-256 `8EB2BCE95E6959F3F4F794B26E1CE565CD59ADEF86F4A5B4EEB29133615E7F33`，MVID `0477b734-38ce-4897-8ea4-6bb904b4d2ec`。ProductVersion 均为 `0.6.73+1c39e909856faa736b32224ee196ef0bac6ca460`。插件 `net462`、测试 `net472` 均在该 Release 测试命令中构建成功。

## 已实测的焦点行为

- 在生产 `AcrylicProductionShellView` 的 STA `Window` 中实际打开游戏选框：搜索框取得键盘焦点；`Tab` 和 `Shift+Tab` 在 `PickerOverlay` 后代之间循环，访问至少两个元素且不落到遮罩后的上下文按钮；`Escape` 实际关闭遮罩并把焦点还给 `GameContextButton`。
- 640 DIP 正常高度和 320 DIP 紧凑高度都运行上述输入序列；紧凑模式下隐藏的状态筛选器不可见、不能获焦，焦点保持在搜索框。
- 实际生产 ComboBox 打开选项弹层；生成的每个 `ComboBoxItem` 都不是 Tab 停靠点，方向键由原生 ComboBox 选项处理，`Tab` 回到 `PickerOverlay` 内表单且关闭下拉，不把焦点留在选项容器。
- `R21FocusVisualRegressionBehaviorTests` 另以 2000 项合成游戏列表验证当前项虚拟化/滚动期间焦点仍可见；切换浅/深主题后搜索框焦点不丢，关闭后上下文按钮可见并重新取得键盘焦点。
- Dashboard 最小 STA WPF fixture 调用生产 `OnUiConfirmationRequested`，再以 routed `Click` 实际调用生产取消处理器。验证初始焦点进入取消按钮、Tab/Shift+Tab 在 overlay 内循环；关闭动效关闭时立即收起并回焦，启用 240ms 资源动效时 overlay 在关闭动画期间仍可见、焦点仍在层内，完成后才收起并把焦点还给触发器；confirmation completion 为取消。XML 结构检查另外读取生产 `DashboardView.xaml` 并核对 Cancel/Confirm `Click` 属性。

## 限制

- Dashboard fixture 使用人工构造的最小 WPF 控件树，事件处理器来自生产 `DashboardView`；它不加载完整生产 Dashboard XAML，按钮绑定由单独 XML 接线检查覆盖。因此真实 XAML 初始化/资源/宿主接合仍未验证。
- 本次是 STA WPF 合成输入和逻辑 DIP，不代表真实 Playnite 嵌入、物理键盘/OS 输入、IME、UI Automation/Narrator、物理 DPI/跨屏、最终呈现帧、ETW 或宿主性能；未重跑 RenderHarness/截图。`validate-source.py` 与 `git diff --check` 通过；VSTest exit0 后打印三条 WPF TextServicesHost/TextStore `InvalidComObjectException` shutdown cleanup 消息，原因未确认。
- 使用合成游戏 DTO 和隔离 WPF 窗口；没有访问真实存档、媒体、云端或发送诊断。

原始实现证据见 [R05-01-FOCUS-BOUNDARY-20260917.md](../R05-01-FOCUS-BOUNDARY-20260917.md)。R05-02 当前 main 复核见 [R05-02 复核包](../R05-02-CURRENT-MAIN-RECHECK-20261013/README.md)。提交 `1c39e909` 已推送；Actions #941 仍运行，待其到终态后推进 R05-03 弹层边缘适配当前 main 复核。
