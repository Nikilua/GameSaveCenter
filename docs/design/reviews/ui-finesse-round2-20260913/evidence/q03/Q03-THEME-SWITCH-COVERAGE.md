# Q03-07 主题切换覆盖证据

采集日期：2026-09-15（Asia/Shanghai）。源码基线：`a1f3cae`（补充设置主题切换开放态夹具）。本证据覆盖共享资源链、Light/Dark 资源替换以及受控 WPF 窗口中的打开态 Popup/ToolTip；不把受控窗口升级为真实 Playnite 宿主或物理 DPI 验收。

## 受控资源链

- `DashboardView.ApplyAdaptiveTheme` 将同一套运行时语义资源写入 Dashboard、生产壳层和已创建的生产工作区；主题变化不会只刷新隐藏兼容树。
- Popup 模板通过 `DynamicResource` 读取 `GscPopupAllowsTransparency`、`GscPopupAnimation`、`GscPopupBrush` 与 `GscPopupEffect`；Tooltip 使用动态前景、浮层背景和描边；Redesign 对话框/浮层使用动态 `GscDialogEffect`/`GscPopupEffect`。
- 动画、阴影、透明度和状态画刷由 `ApplyRuntimeThemeResources` 重新写入同一视图资源字典；不改 Playnite 全局资源。

## 自动门禁

- `UiDiagnosticsExporterTests.ThemeResourceSwitchReplacesStateBrushesWithoutLeavingStaticFallbacks` 现在真实比较 Light/Dark 的选中前景、按钮渐变颜色、画刷实例和渐变长度，修复了原先“浅色序列与自身比较”的假阳性。
- `UiFinesseRound2ControlSourceTests.TransientSurfacesKeepThemeSensitiveResourcesDynamic` 锁定 Popup/Tooltip/Dialog 的动态资源入口及生产壳层向 `ProductionShellView`、`WorkspaceViews` 广播主题资源。
- 相关定向测试 `UiDiagnosticsExporterTests` 与 `UiFinesseRound2ControlSourceTests` 共 14/14 通过；源校验和 Release WPF 构建通过（现有 Contracts/Core NU1900 网络漏洞源告警仍单独记录）。

## 打开态运行时夹具

`a1f3cae` 的 Release RenderHarness 在 STA 线程创建隐藏 `Window`，加载真实 `GameSaveCenterSettingsView`，进入“外观与可访问性”，打开生产 ComboBox 的 `PART_Popup` 和设置保存提示 `ToolTip`，再在两者保持打开时把主题从 Light 切换到 Dark。`settingsthemeprobe` 输出 `popupOpen=True`、`tooltipOpen=True`、主文字资源从 `#F21B1F27` 切换为 `#FFF2F4F8`，并生成 6 张截图；同一提交的完整 `render-qa` 为 `WorkingTreeClean=True`、`render-qa OK`。

- [浅色设置打开态](Q03-settings-theme-switch-light-open-1040x700.png)
- [深色设置打开态](Q03-settings-theme-switch-dark-open-1040x700.png)
- [浅色 Popup](Q03-settings-theme-switch-light-popup.png) / [深色 Popup](Q03-settings-theme-switch-dark-popup.png)
- [浅色 ToolTip](Q03-settings-theme-switch-light-tooltip.png) / [深色 ToolTip](Q03-settings-theme-switch-dark-tooltip.png)

夹具为避免未激活隐藏窗口因生产 `StaysOpen=False` 自动收回 Popup，仅在审计期间把已绑定的 Popup 临时保持打开；生产模板的点外部关闭契约仍由源门禁覆盖。逻辑尺寸为 `1040×700 DIP`，离屏 DPI 为 `1.00`。

## 未覆盖边界

受控夹具已覆盖 Popup/ToolTip 的打开态资源替换，但未覆盖真实 Playnite 宿主的 Dialog/Inspector、闪白屏幕帧、Popup 跨屏定位、宿主主题跟随、物理 DPI 和真实输入；这些仍是 Q03-07 的宿主边界。
