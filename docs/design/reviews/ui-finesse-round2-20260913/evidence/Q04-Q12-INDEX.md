# Q04–Q12 共享控件与表面受控证据

采集日期：2026-09-15（Asia/Shanghai）。本索引记录 Q04–Q12 对共享表面、按钮、图标、输入、选择器、选择控件、导航/页签和表格的专项复核。当前代码基线为提交 `624ece6`（补充危险对话框焦点复核）；夹具仍是 STA、96 DPI、DpiScale=1.00 的开发专用 WPF 离屏窗口，不替代真实 Playnite、Popup/IME、物理 DPI 或屏幕读屏验收。

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

## 2026-09-15 Q05-05 生产忙态按钮复核

- 提交 `4947539` 为生产按钮忙态阶段基线：`Controls.Button.IsBusy` 由真实 `DashboardViewModel.IsBusy` 驱动，顶部刷新、全部备份、媒体同步按钮共享 `WpfUiProduction` 的 `BusyIndicatorHost`。忙态用 indeterminate 指示层叠加在按钮底部，保留原 ContentPresenter 与测量槽，不通过等待动画延迟命令。
- 使用 `GameSaveCenter.RenderHarness.exe buttonbusyprobe <output>` 在 STA、96 DPI、DpiScale=1.00 的生产资源夹具中运行 Light/Dark 两主题；报告记录两主题均 `normalWidth=180`、`busyWidth=180`、`widthStable=True`、`indicatorVisible=True`、`indeterminate=True`、`contentStable=True`。原始报告：[button-busy-probe-report.txt](q04-q12/button-busy-20260915/button-busy-probe-report.txt)。
- 代表截图：[浅色正常](q04-q12/button-busy-20260915/button-busy-light-normal.png)、[浅色忙态](q04-q12/button-busy-20260915/button-busy-light-busy.png)、[深色正常](q04-q12/button-busy-20260915/button-busy-dark-normal.png)、[深色忙态](q04-q12/button-busy-20260915/button-busy-dark-busy.png)。截图中按钮文字保持“全部备份”，忙态不改变宽度。
- 本证据升级 Q05-05 的共享生产模板与双主题视觉列；真实 Playnite 命令耗时、宿主绑定更新时序、实际输入和屏幕 DPI 仍保留宿主边界。

## 2026-09-15 Q05-06 危险确认按钮组复核

- 提交 `624ece6` 增加源契约断言：危险确认仍将 `DialogConfirmButton` 设为 `GscErrorBrush`，并调用 `OpenDialog(request.IsDangerous ? DialogCancelButton : DialogConfirmButton)`；`OpenDialog` 的实际焦点 API 仍聚焦传入的取消按钮。
- 使用 `GameSaveCenter.RenderHarness.exe dangerdialogprobe <output>` 直接加载生产 `GscRedesignFeedbackDialogCard`、`GscButtonBase` 和动态主题资源，在 STA、96 DPI、DpiScale=1.00 的 Light/Dark 窗口中渲染“取消 / 删除存档”按钮组。报告两主题均 `cancelFocused=True`、`dialogWidth=560`、`gap=8`、`dangerFirstFocus=false`；原始报告：[danger-dialog-probe-report.txt](q04-q12/danger-dialog-20260915/danger-dialog-probe-report.txt)。
- 代表截图：[浅色危险确认](q04-q12/danger-dialog-20260915/danger-dialog-light.png)、[深色危险确认](q04-q12/danger-dialog-20260915/danger-dialog-dark.png)。截图复核确认危险色、取消优先顺序、按钮间距与标题/说明在两主题均可读。
- 本证据升级 Q05-06 的受控资源布局与视觉列；真实业务确认完成/取消结果、实际 Playnite 宿主焦点和鼠标/键盘输入仍保留宿主边界。

## 2026-09-15 Q06-02 卸载状态清理门禁

- 提交 `7804431` 为 Q06-02 生命周期修复：`GameSaveCenter.Playnite.Controls.Button` 订阅真实 `Unloaded` 路由事件，在模板仍可用时清除 `ButtonChrome` 的 Opacity/Scale 动画，并将 `HoverOverlay`、`PressedOverlay`、`FocusOverlay` 的活动动画与 Opacity 归零；不会触发或延迟任何业务命令。
- 源契约测试覆盖 `Unloaded` 订阅、ScaleX/ScaleY 归一和三个交互覆盖层；本阶段 `UiFinesseRound2ControlSourceTests=19/19`、RenderHarness Release `0 warning/0 error`、源码/XAML 门禁通过。
- 提交 `132e6d5` 修复了真实 RenderHarness 卸载路径暴露的冻结 `ScaleTransform` 回归：无活动动画的模板实例可能被冻结，清理逻辑现在只对未冻结变换取消动画并写回 `ScaleX/ScaleY=1`；clean-tree RenderHarness 报告绑定 `132e6d5`，完整 `render-qa OK`。
- 该阶段只升级 Q06-02 的生命周期实现与自动门禁，不把卸载路径静态复核写成真实按下→移出→失焦→禁用→卸载输入截图；真实宿主输入序列仍保留视觉/宿主边界。

## 2026-09-15 Q12-07 排序箭头双状态复核

- 在提交 `2f3d17b8a34780546039aa6b7b07ecbb1c6a2ec6` 的 clean tree 上运行 `finesseprobe <output> dark sorted` 与 `light sorted`；报告身份均为 `WorkingTreeClean=True`、`DpiScale=1.00`，并记录 `SortFixture: ascending="名称" visible=True width=14; descending="数值" visible=True width=14 angle=180`。
- [当前深色排序夹具截图](q04-q12/sort-20260915/ui-finesse-fixture-dark.png) / [报告](q04-q12/sort-20260915/ui-finesse-fixture-dark-report.txt)；[当前浅色排序夹具截图](q04-q12/sort-20260915/ui-finesse-fixture-light.png) / [报告](q04-q12/sort-20260915/ui-finesse-fixture-light-report.txt)。截图确认升序/降序箭头各自保留 22 DIP 槽位，不压缩表头文字，深浅主题均清晰可辨。
- 本证据只签收共享表头的双方向可见性和旋转状态；真实业务排序点击、排序键/结果和宿主输入序列仍保留为 Q12-07 的宿主边界。

## 逐组边界

- Q04：卡片/输入/弹层/表格使用共享圆角、描边和阴影资源；截图检查嵌套表面、表格壳层和无玻璃视觉层级。滚动行不新增逐行 Effect，真实窗口接缝与关闭玻璃后的宿主组合仍待验。
- Q05–Q06：按钮高度、padding、文本模板、复合内容、忙态占位、危险确认布局、卸载清理和禁用/按压/焦点共享模板已被夹具与源码测试覆盖；Q05-05/Q05-06 另有 Light/Dark 证据。命令单次执行、键盘与鼠标序列及真实宿主状态序列仍需继续验收。
- Q07：`ThemeAwareIcon` 的 Path stroke/fill 绑定控件最终 Foreground，状态图标和复制图标在双主题截图中可见；分数 DPI 的实际线宽、完整图标包去重和读屏命名仍不能由离屏 PNG 宣称完成。
- Q08：TextBox 内容视口、CaretBrush、SelectionBrush、只读/禁用资源和长路径 Tooltip 已专项记录；中文 IME 组合、候选确认、撤销和粘贴原值属于宿主输入验收。
- Q09：ComboBox 选中内容、Chevron、3 项 Popup 与有限滚动模板已专项记录；Popup 真定位、键盘关闭不写回、游戏选框 DropDownClosed 同步和移屏主题切换待真实窗口验收。
- Q10：CheckBox 勾形/半选、ToggleSwitch、Slider 的共享几何与实际边界已记录；本仓库没有额外 RadioButton 业务组，导航 RadioButton 继续沿用 `GscNavItem`/`AcrylicNavItem` 的真实导航入口，不新增控件。绑定拒绝、连续切换和键盘步进仍需宿主行为验收。
- Q11：当前导航 RadioButton、TabControl/TabItem 的共享入口已核对来源；本夹具只对 ListBox 选中/焦点节奏做受控检查，不把离屏截图冒充六页导航状态保持或真实页签溢出验收。
- Q12：DataGrid 表头、行、状态胶囊、数字/路径列、排序槽、4 行端点和双主题业务空表在截图/报告中复核；完整名称 Tooltip、最坏列宽、排序点击和真实 Worker/Playnite 数据生命周期仍需各工作区宿主回归。

## 2026-09-15 Q12-08 业务空表双主题复核

- 在 clean-tree `77f4dc5d7766751f007621d2db15b66dff2afbd2` 上，使用开发专用 `FakeDashboardData(18, WorkspaceFixtureState.Empty)` 清空所有生产表/列表数据，覆盖 Light/Dark 与 `1040×700`、`1600×900` 逻辑窗口；production views/XAML 的 direct-reference runner 报告为 `emptytables OK`。
- 报告逐页记录 Save 历史/候选、Task、Trainer 工具/在线库/版本、Media 收件箱/当前媒体/来源规则、Maintenance 诊断/云队列/设备/保留分析/审计/进程映射均为 0 项，并记录相应空态文案。代表截图和完整报告见 [Q12-08 空表证据](q04-q12/Q12-08-EMPTY-TABLES-20260915.md)。
- 本证据只升级 Q12-08 的离屏视觉列；真实 Playnite/Worker 空结果、Popup/Tooltip、物理 DPI、读屏和键盘操作仍保持宿主边界。

## 证据边界

## 2026-09-15 Q06-05 选中悬停优先级

- [选中悬停优先级证据](q04-q12/nav-priority-20260915.md) 对应提交 `513ac5f`：共享 `AcrylicNavItem` 追加 `Selected+Hover` 最后 `MultiTrigger`，避免普通悬停 tint 覆盖当前页强选中层级。
- 定向 `UiFinesseRound2ControlSourceTests=20/20`、XAML `24/24`、source validation 和 clean-tree 双主题 RenderHarness 均通过；真实宿主组合输入仍待验。

报告区分“共享模板声明/受控实现”和“实际输入/宿主表现”。未宣称 IME、Popup 跨屏、真实 Playnite 主题 Owner、物理 100/125/150/175/200% DPI、读屏或屏幕帧率已通过。
