# R23-02 生产资源状态矩阵

日期：2026-09-22  
状态：已实现，待环境验证  
审计基线：`4de27bd0`（R23-01 交付审计；本项无新增资源代码）

## 资源链与实例范围

- 生产壳层从 `AcrylicProductionShellView.xaml` 的 `UserControl.Resources` 合并 `AcrylicProductionResources.xaml`；该资源继续接入 `DesignTokens.xaml`、`WpfUiProduction.xaml`、`Redesign.xaml` 以及壳层参考控件资源。
- 各真实页面分别合并 `DesignTokens.xaml`、`WpfUiProduction.xaml`、`Redesign.xaml`，并由 `AdaptiveThemePalette` 在运行时替换浅色/深色主题相关画刷。矩阵按资源 key 和页面 `Style="{Static|Dynamic}Resource ...}"` 的直接实例引用统计，不把只在资源文件中声明、没有页面实例的 Demo/Lab 样式当作生产实例。
- 当前直接实例引用包括：`GscWpfUiActionButton` 35、`GscWpfUiSecondaryButton` 35、`GscWpfUiTextBox` 28、`GscWpfUiComboBox` 26、`GscWpfUiToolbarButton` 48、`GscWpfUiToggleSwitch` 38、`GscWpfUiContextButton` 17、`GscWpfUiPrimaryActionButton` 23、`GscWpfUiFilterComboBox` 10、`GscWpfUiPathDetailTextBox` 7、`GscStableDataGridRow` 6、`GscPageScrollViewer` 13、`GscInspectorScrollViewer` 7。
- 页面派生实例单独保留：`MaintenanceDataGrid`/`MaintenanceFirstColumnHeader`/`MaintenanceLastColumnHeader`、`MediaDataGrid`/`MediaFirstColumnHeader`/`MediaMiddleColumnHeader`/`MediaLastColumnHeader`、`SaveDataGrid`/`SaveFirstHeader`/`SaveLastHeader`、`TaskDataGrid`/`TaskFirstColumnHeader`/`TaskLastColumnHeader`、`TrainerTableFrame`/`TrainerTabControl`、`OverviewActivityRowButton` 等，不能由通用 `DataGrid` 或按钮样例代替。

## 状态矩阵

符号说明：`✓` 为资源模板/触发器明确声明，`→` 为真实派生实例继承并保留基类状态，`—` 为该控件没有独立的该状态，`待验` 为当前只有隔离 WPF/源码证据，仍需真实页面或宿主观察。

| 真实生产家族 | 浅色/深色资源 | 正常 | 悬停 | 按压/打开 | 键盘焦点 | 禁用 | 证据与边界 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `GscWpfUiButton` 及 `Secondary/Primary/Danger/Action/Context/Toolbar/IconOnly` 派生 | `GscButtonGlass*`、`GscPrimaryButton*`、`GscError*` 由运行时调色板替换 | ✓ | ✓ | ✓，含 overlay/轻微缩放 | ✓，共享 `GscSharedFocusVisual` + focus overlay | ✓，共享 chrome opacity；文案不靠整控件透明度 | Light/Dark finesse fixture 已实际实例化普通/禁用按钮；状态组合和对比度代理已有 R00-01/02 证据。各派生角色当前未在真实 Playnite 逐一呈现，保留 R23-03/R23-04 边界 |
| `AcrylicNavItem`、`AcrylicSidebarBoundaryButton` | 壳层 `GscAccentTint*`、`GscPrimaryTextBrush` 等动态资源 | ✓ | ✓ | BoundaryButton ✓；NavItem 无独立 pressed | BoundaryButton ✓；NavItem 仅依赖默认焦点路径，待验 | BoundaryButton ✓；NavItem 未显式定义 | 壳层实际引用 7 个导航项和 1 个边界按钮；不能拿 `GscWpfUiButton` 样例覆盖 NavItem 的缺省状态，待真实壳层键盘/禁用复测 |
| `GscWpfUiTextBox` 及 `Path/Technical/Numeric` 派生 | `GscControl*`、`GscAccent*`、`GscError*` | ✓ | ✓ | —（文本框没有独立按压 chrome） | ✓，focus border/fill | ✓，chrome opacity | 生产模板含内容滚动、焦点、校验错误和禁用；数字可读/窄列负例已有 R01-02，真实主题字体和 IME 仍待宿主 |
| `GscWpfUiComboBox`、`Filter`、`PickerFilter` 与 `GscComboBox` | `GscControl*`、`GscPopupBrush`、`GscAccent*` | ✓ | ✓ | ✓，打开态由 toggle/check 与 chevron 表达 | ✓ | ✓ | 页面直接实例化了两套兼容链；弹层、键盘导航和真实 Playnite popup 仍不能用离屏模板宣称通过 |
| `GscWpfUiToggleSwitch`、`GscCheckBox`、`GscDataGridCheckBox`、`GscSlider` | `GscControl*`、`GscAccentBrush`、共享焦点资源 | ✓ | ✓ | Toggle/Check/Slider ✓ | Toggle/Check ✓；Slider 使用共享焦点资源 | Toggle ✓；其余按模板/宿主默认路径待验 | 浅/深 fixture 实例化 toggle、checkbox、slider，并保留实际选择状态；未宣称 OS 键盘/读屏或高 DPI |
| `GscDataGridColumnHeaderStyle`、共享 `DataGridRow/Cell`、`GscStableDataGridRow` 及五页派生表格 | `GscTable*`、`GscRowHoverBrush`、`GscAccent*`、错误状态资源 | ✓ | ✓，行/表头分开 | — | ✓，row/cell 共享焦点；选中/非活动选中分开 | Row ✓；header/cell 依赖宿主可用性，待验 | R00-06、R00-07、R01-02 和既有四行/表格几何证据覆盖真实生产模板；每个页面表格 key 仍需 R23-03 代表页终审 |
| `GscRedesignWorkspaceTabItem`、`GscInternalTabItem`、`GscRedesignSettingsTabItem`、`GscSettingsSectionTabs` | `GscControlFillBrush`、`GscAccentTint*`、共享焦点资源 | ✓ | ✓ | 打开/选中 ✓ | ✓ | Workspace TabItem ✓；设置派生项待验 | 维护、媒体、存档、训练和设置页有独立 tab 实例；不能只引用一个 `TabItem` 基类截图 |
| `GscPageScrollViewer`、`GscInspectorScrollViewer`、`GscScrollThumb` | `GscScrollTrackBrush`、`GscScrollThumbBrush`、`GscScrollThumbHoverBrush`、`GscAccentBrush` | ✓ | ✓ | 拖拽 ✓ | —（Thumb 明确不可聚焦） | —（由滚动容器/宿主控制） | 保留当前滚动条系统；已有 R00-06、R00-07 和页面滚动证据，未改成新滚动模型；真实鼠标/触摸、物理 DPI 和 Playnite 宿主仍待验 |
| `GscRedesignSectionCard/SubCard/ReadingCard`、状态/计数/表格 pill | `GscGlass*`、`GscControl*`、Success/Warning/Error/Info tint | ✓ | — | — | — | — | 这些是信息表面而非可操作控件；状态颜色与 `StatusGlyphConverter` 的文本线索由 R22-08 复核，不把颜色样例当作按钮交互证据 |

## 已有行为证据与未验边界

- Light/Dark `finesse-fixture` 均报告 `ControlSurfaceCounts: textboxes=1 combos=1 buttons=4 toggles=1 checkboxes=2 sliders=1 listboxes=1`、普通/禁用按钮已实例化、共享模板声明悬停/按压/焦点/禁用；R00-01/02 对按钮状态组合做了 88 样本对比度代理，R01-02、R00-06/R00-07 分别覆盖数字可读和表格/滚动几何。
- WPF 质量检查当前为 `0 errors / 27 warnings / 177 info`；警告为既有 Canvas/StackPanel 容器与滚动审阅项，未因本次报告改写。`python scripts/validate-source.py` 与 `git diff --check` 作为文档门禁通过。
- 本矩阵没有把基类 fixture 写成所有派生样式已呈现；`AcrylicNavItem` 的焦点/禁用缺少显式触发器、设置派生 Tab 和各页 DataGrid 派生状态仍标记待验。未启动真实 Playnite/package-host、未做 Windows UIA/读屏、OS 输入/IME、物理 DPI/跨屏、最终呈现帧、ETW 或宿主性能验证。Demo 原目录不可用，沿用恢复生产基线；未读写真实存档/媒体/云端或外发诊断。

## 下一步

进入 `R23-03` 代表页面终审，优先覆盖壳层、概览、存档、媒体、工具、任务、维护、设置的实际空/错/加载和高风险状态；对本矩阵标为“待验”的派生样式逐页给出可审阅结果。
