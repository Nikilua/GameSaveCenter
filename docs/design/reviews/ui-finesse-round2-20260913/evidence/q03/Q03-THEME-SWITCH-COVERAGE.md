# Q03-07 主题切换覆盖证据

采集日期：2026-09-14（Asia/Shanghai）。源码基线：`46ece40`（修正主题切换资源测试）。本证据覆盖共享资源链与 Light/Dark 资源替换，不把离屏夹具升级为真实 Playnite Popup、Tooltip、Dialog 或物理 DPI 验收。

## 受控资源链

- `DashboardView.ApplyAdaptiveTheme` 将同一套运行时语义资源写入 Dashboard、生产壳层和已创建的生产工作区；主题变化不会只刷新隐藏兼容树。
- Popup 模板通过 `DynamicResource` 读取 `GscPopupAllowsTransparency`、`GscPopupAnimation`、`GscPopupBrush` 与 `GscPopupEffect`；Tooltip 使用动态前景、浮层背景和描边；Redesign 对话框/浮层使用动态 `GscDialogEffect`/`GscPopupEffect`。
- 动画、阴影、透明度和状态画刷由 `ApplyRuntimeThemeResources` 重新写入同一视图资源字典；不改 Playnite 全局资源。

## 自动门禁

- `UiDiagnosticsExporterTests.ThemeResourceSwitchReplacesStateBrushesWithoutLeavingStaticFallbacks` 现在真实比较 Light/Dark 的选中前景、按钮渐变颜色、画刷实例和渐变长度，修复了原先“浅色序列与自身比较”的假阳性。
- `UiFinesseRound2ControlSourceTests.TransientSurfacesKeepThemeSensitiveResourcesDynamic` 锁定 Popup/Tooltip/Dialog 的动态资源入口及生产壳层向 `ProductionShellView`、`WorkspaceViews` 广播主题资源。
- 相关定向测试 `UiDiagnosticsExporterTests` 与 `UiFinesseRound2ControlSourceTests` 共 14/14 通过；源校验和 Release WPF 构建通过（现有 Contracts/Core NU1900 网络漏洞源告警仍单独记录）。

## 未覆盖边界

受控测试未打开真实 Popup/Tooltip/Dialog，也未采集切换发生在打开态时的屏幕帧，因此闪白、旧动画画刷、Popup 跨屏定位、宿主主题跟随和真实 Playnite 输入仍为 Q03-07 的视觉/宿主待验项。
