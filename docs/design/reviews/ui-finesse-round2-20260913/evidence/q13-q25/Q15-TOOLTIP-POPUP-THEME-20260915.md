# Q15-03 / Q15-07 / Q15-08 Tooltip、Popup 与浮层主题证据

采集日期：2026-09-15（Asia/Shanghai）。代码与测试提交：`86ac336`（`收紧提示时序与浮层关闭契约`）。本记录只签收共享资源、窗口级主题资源和自动门禁；不把源码契约或受控 STA 测试写成真实 Playnite 鼠标、屏幕边缘、子菜单或跨屏验收。

## 已落地的共享约束

- Q15-03：`DashboardView`、`AcrylicProductionShellView` 和独立设置页统一声明 `InitialShowDelay=350 ms`、`BetweenShowDelay=100 ms`。Dashboard 保留已有 `ShowDuration=18000 ms`；共享 Tooltip 样式继续使用动态字体/正文资源、`MaxWidth=420` 和受控内边距。导航、游戏选框和关键动作继续提供 `ToolTip` 与 `AutomationProperties.Name`，键盘路径不依赖鼠标悬停才能理解操作。
- Q15-07：`DesignTokens.xaml` 与 `WpfUiProduction.xaml` 的生产 Combo Popup 均显式使用 `Placement=Bottom`、`Focusable=False`、`StaysOpen=False`，因此共享模板拥有点外部关闭契约；Popup 表面、阴影、透明度和动画继续走动态资源。DesignTokens 模板将高度限制为 `320 DIP`，WPF-UI 模板绑定 `MaxDropDownHeight`，两者均保留自动垂直滚动和 `KeyboardNavigation.DirectionalNavigation=Contained`。
- Q15-08：Dashboard 将完整运行时调色板分别应用到自身、生产壳层和每个 workspace 的局部 `ResourceDictionary`；独立设置窗口只更新自己的 `Resources`，随后应用设置页材质。没有把 Popup/Tooltip 资源写入 `Application.Current.Resources` 或 Playnite 全局主题字典。

## 自动验证

`scripts/build.ps1 -Configuration Release -OutputRoot .tmp/q15-build`：

- XAML structural validation：24/24；
- Release：0 warning / 0 error；
- Core：83/83；Worker：310/311（1 skip）；Playnite：481/544（63 skip，0 fail）；
- 新增 `FloatingShellsUseOneTooltipDelayContractForQuickPointerMoves`、`ComboPopupClosesOutsideAndKeepsItsReadingSurfaceBounded`；
- 新增 `FloatingThemeResourcesStayLocalToDashboardAndSettingsOwners`：在 WPF STA 中为浅色 Dashboard 与深色 Settings 创建独立调色板/资源字典，验证 Popup 颜色随 Owner 变化、Settings 材质只存在于 Settings 字典，且两个 owner 本身没有被写入 Popup 资源。

另外运行：

- `python .codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .`：0 error、151 warning、548 info。警告主要来自 `.tmp` 中复制的 FusionX 宿主资源与既有共享资源审查提示，不是本阶段新增错误；
- `git diff --check`：通过。

## 尚未签收的宿主边界

- 未运行真实 Playnite 输入序列，因此 Combo Popup 在屏幕边缘的实际翻转/裁剪、鼠标进入子菜单、Esc 与点外部的现场时序仍待宿主；`StaysOpen=False` 是共享模板门禁，不是可见窗口操作录像。
- 未在独立 Settings 窗口和 Dashboard 同时打开浮层并热切换 Playnite 主题；STA 测试只证明局部资源隔离和浅/深色资源差异，不证明宿主实时重绘、系统 DPI、读屏或跨物理屏 Popup。
- 因此 Q15-03、Q15-07、Q15-08 的“自动验证”更新为通过；视觉与宿主列继续保持待验/外部阻塞，最终结论仍未完成。
