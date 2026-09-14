# Q04–Q12 共享控件与表面受控证据

采集日期：2026-09-14（Asia/Shanghai）。本索引记录 Q04–Q12 对共享表面、按钮、图标、输入、选择器、选择控件、导航/页签和表格的专项复核。代码基线在阶段提交后补入；夹具仍是 STA、1120×980 DIP、96 DPI、DpiScale=1.00 的开发专用 WPF 离屏窗口，不替代真实 Playnite、Popup/IME、物理 DPI 或屏幕读屏验收。

## 运行身份与产物

- 夹具命令：`GameSaveCenter.RenderHarness.exe finesseprobe <output> <dark|light>`；数据为 4 行合成中英/数字/路径/状态样本，不读写真实存档。
- 双主题截图：[dark/ui-finesse-fixture.png](q04-q12/dark/ui-finesse-fixture.png)、[light/ui-finesse-fixture.png](q04-q12/light/ui-finesse-fixture.png)。原始报告：[dark/ui-finesse-fixture-report.txt](q04-q12/dark/ui-finesse-fixture-report.txt)、[light/ui-finesse-fixture-report.txt](q04-q12/light/ui-finesse-fixture-report.txt)。
- 共享入口：`DesignTokens.xaml`、`WpfUiProduction.xaml`、`GscIconPack.xaml`、`Redesign.xaml`、`DashboardView.xaml`；夹具通过 `AcrylicProductionResources.xaml` 使用生产资源链。

## 结果摘要

- `ControlSurfaceCounts` 双主题均为 `textboxes=1 combos=1 buttons=4 toggles=1 checkboxes=2 sliders=1 listboxes=1`；输入框、选择器、按钮、Toggle、半选 CheckBox、Slider、ListBox 和 DataGrid 均实际实现并测得 DIP 边界。
- `TextInputContract` 记录了 `Padding=12,3,12,3`、`HorizontalContentAlignment=Left`、`VerticalContentAlignment=Center`、主题 CaretBrush/SelectionBrush；TextBox 模板保持 `PART_ContentHost` 零外层重复 padding。
- `ComboContract` 记录 3 个选项、选中索引、320 DIP 最大弹层高度与 Popup 模板声明；真实弹层定位、Esc/Alt+Down 和跨屏热切换保留宿主验收边界。
- `ButtonGeometry` 记录 36 DIP 常规命中高度、统一 14,7 padding，以及正常/禁用实际控件；共享模板声明 Hover/Pressed/Focus/Disabled，未在夹具中触发业务命令。
- 新增半选状态：`GscCheckBox` 与 `GscDataGridCheckBox` 都拥有独立 `IndeterminateMark`，`IsChecked=null` 时显示 accent 背景与短横线；报告为 `indeterminate=True mark=visible`。
- `ListContract` 记录 3 个选项、选中索引 0 与虚拟化开关；DataGrid 报告保持 4/4 行完整，压缩 4 DIP 视口负例为 3/4，并与 Q00/Q03 的裁剪门禁一致。
- 双主题有效文本对比均为 12 个样本 0 violation；按钮 33 个渐变/状态样本、语义层 4 个样本、复杂背景 4 个样本均 0 violation。截图复核确认暗/浅主题下没有黑字、方角漏裁、状态胶囊或半选标记缺失。

## 逐组边界

- Q04：卡片/输入/弹层/表格使用共享圆角、描边和阴影资源；截图检查嵌套表面、表格壳层和无玻璃视觉层级。滚动行不新增逐行 Effect，真实窗口接缝与关闭玻璃后的宿主组合仍待验。
- Q05–Q06：按钮高度、padding、文本模板、复合内容和禁用/按压/焦点共享模板已被夹具与源码测试覆盖；命令单次执行、键盘与鼠标序列、卸载清理需在真实输入/宿主窗口继续验收。
- Q07：`ThemeAwareIcon` 的 Path stroke/fill 绑定控件最终 Foreground，状态图标和复制图标在双主题截图中可见；分数 DPI 的实际线宽、完整图标包去重和读屏命名仍不能由离屏 PNG 宣称完成。
- Q08：TextBox 内容视口、CaretBrush、SelectionBrush、只读/禁用资源和长路径 Tooltip 已专项记录；中文 IME 组合、候选确认、撤销和粘贴原值属于宿主输入验收。
- Q09：ComboBox 选中内容、Chevron、3 项 Popup 与有限滚动模板已专项记录；Popup 真定位、键盘关闭不写回、游戏选框 DropDownClosed 同步和移屏主题切换待真实窗口验收。
- Q10：CheckBox 勾形/半选、ToggleSwitch、Slider 的共享几何与实际边界已记录；本仓库没有额外 RadioButton 业务组，导航 RadioButton 继续沿用 `GscNavItem`/`AcrylicNavItem` 的真实导航入口，不新增控件。绑定拒绝、连续切换和键盘步进仍需宿主行为验收。
- Q11：当前导航 RadioButton、TabControl/TabItem 的共享入口已核对来源；本夹具只对 ListBox 选中/焦点节奏做受控检查，不把离屏截图冒充六页导航状态保持或真实页签溢出验收。
- Q12：DataGrid 表头、行、状态胶囊、数字/路径列、排序槽和 4 行端点在截图/报告中复核；完整名称 Tooltip、最坏列宽、排序点击和空表业务数据仍需各工作区宿主回归。

## 证据边界

报告区分“共享模板声明/受控实现”和“实际输入/宿主表现”。未宣称 IME、Popup 跨屏、真实 Playnite 主题 Owner、物理 100/125/150/175/200% DPI、读屏或屏幕帧率已通过。
