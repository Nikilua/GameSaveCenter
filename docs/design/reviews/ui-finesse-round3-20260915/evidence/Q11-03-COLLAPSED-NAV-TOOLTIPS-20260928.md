# Q11-03 收起导航 Tooltip 与图标状态（2026-09-28）

## 当前结果

生产 `AcrylicProductionShellView` 已在收起状态折叠每个导航文字，保留图标；各项具有完整 Tooltip 文本和 UI Automation 名称。本阶段只扩展现有真实 WPF shell 回归，没有修改生产 XAML、样式、命令或 Tooltip 配置。

在 `CollapsedSidebarPreservesSelectedWorkspaceAndAccessibleNavigation` 生产视图 STA `Window` 场景中，触发收起后对首页、存档、修改器、媒体、任务、维护、设置七项逐项断言：文字 `Visibility.Collapsed`；图标仍 Visible 且有非零实际宽度；导航仍启用并是 TabStop；Tooltip 完整匹配各自页面名；Automation Name 匹配其可操作说明。原有选中页保留、收起/展开宽度和边界按钮辅助名称断言继续通过。

本行为测试没有实际打开 WPF Tooltip Popup，也未驱动 OS 鼠标悬停；因此不证明 Tooltip 的真实延迟、屏幕边缘翻转/定位或 Playnite 宿主外观。Q11-03 仍未最终完成。

## 构建与测试

- 测试提交：`50865efcd67e3e17bfe32445ef7eab85b298d871`。
- Release `GameSaveCenter.Playnite.Tests` 项目构建成功，未输出编译 warning/error。
- `ProductionShellChromeSourceTests`：`13/13` passed、0 failed/skipped，VSTest exit `0`；TRX：`artifacts/q11-03-nav-tooltip-20260928/q11-03-production-shell-50865efc.trx`。
- TRX 含一条 WPF `TextServicesHost.OnUnregisterTextStore InvalidComObjectException` 清理噪声；测试结果仍为全通过、进程 exit `0`，根因未知。

