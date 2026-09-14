# Q09-06 / Q15-07 / Q24-04～Q24-06 游戏选框键盘、焦点与自动化证据

采集日期：2026-09-15（Asia/Shanghai）。源码与测试已落地于提交 `dea74f7`。本记录只签收当前生产壳层可以由 WPF 测试和源码直接证明的部分，不把离屏审计当成真实 Playnite 键盘、读屏或输入法验收。

## 实际修复

- `AcrylicProductionShellView` 的生产游戏选框在 `PickerOverlay` 这一共享边界处理 `PreviewKeyDown`：Esc 关闭；已有选中游戏时 Enter 关闭；打开时把焦点放入搜索框；选择游戏、点击遮罩和再次点击当前游戏按钮均通过同一关闭辅助方法收口。
- 关闭辅助方法把键盘焦点返回 `GameContextButton`，避免选框消失后焦点落到宿主窗口或不可见内容；原有 `GamePicker.SelectedItem`、`DropDownClosed` 写回和虚拟化 ListBox 保持不变。
- 为生产壳层的导航滚动面、Header 媒体/备份动作、游戏选框筛选器与列表、footer 状态区补充稳定 `AutomationProperties.Name`；Overview 优先事项标题和当前工作台动作也补充自动化名称及长标题 Tooltip。

## 证据与验证

- `KeyboardFocusSourceTests.ProductionGamePickerClosesWithEscapeOrEnterAndReturnsFocus` 锁定壳层事件入口、Esc/Enter 分支、焦点返回和鼠标关闭/选择路径。
- `KeyboardFocusSourceTests.ProductionGamePickerEscapeActuallyReturnsFocusToContextButton` 在真实 WPF STA `Window` 中向可见 `PickerOverlay` 路由 PreviewKeyDown，断言选框变为 `Collapsed`、事件已处理且 `Keyboard.FocusedElement` 为当前游戏按钮。
- `KeyboardFocusSourceTests.ProductionHeaderAndPickerActionsHaveStableAutomationNames` 锁定导航、Header、筛选器、列表、footer 和 Overview 主要动作的自动化名称。
- 定向 WPF 回归：`KeyboardFocusSourceTests`、`AccessibilitySourceTests`、`OverviewInteractionTests`、`ProductionShellChromeSourceTests` 合计 `17/17` 通过。
- 完整 `GameSaveCenter.Playnite.Tests` 回归：`472 passed / 63 skipped / 0 failed`，进程退出码为 0；本次新增的 3 个测试已包含在总数中。
- `scripts/render-qa.ps1 -Configuration Release -Output .tmp/ui-qa-picker-clean-20260915`：干净 HEAD `e6ddd5c278873013286303db3fa5a142b9296172` 的 RenderHarness 构建 `0 warning / 0 error`，`WorkingTreeClean: True`；双主题、多尺寸、壳层紧凑 Header、页面滚动和 Resize 场景均输出 `render-qa OK`，报告无 `PROBLEM`。
- `python .codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .`：扫描 326 个 XAML，`0 errors`；151 warnings / 548 info 来自宿主 FusionX/历史主题资源及既有共享资源提示，未出现本次改动引入的 error。`python scripts/validate-source.py` 与 `git diff --check` 通过。

## 尚未签收

- Q09-06 的 ComboBox 原生 Alt+Down、F4、方向键和真实 Popup 边缘定位仍需真实宿主输入序列；本记录只补齐生产选框的 Esc/Enter/点外部/选择关闭链。
- Q24-04～Q24-06 的完整六页 Tab/Shift+Tab 走查、Playnite 宿主 UI Automation Pattern、中文 IME 组合和真实读屏仍待宿主条件；本轮 WPF STA 测试不替代这些边界。
