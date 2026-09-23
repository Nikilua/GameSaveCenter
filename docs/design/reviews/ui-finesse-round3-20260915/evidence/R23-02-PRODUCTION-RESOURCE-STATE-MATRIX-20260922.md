# R23-02 生产资源状态矩阵

日期：2026-09-23
状态：受控资源与行为门禁已满足，真实宿主环境待验
审计基线：`4de27bd0`（R23-01 交付审计）；本次生产样式/行为测试提交：`a18cb43eb3a7a623aeefa1546be5b2407f30173a`

## 资源链与实例范围

- 生产壳层从 `AcrylicProductionShellView.xaml` 的 `UserControl.Resources` 合并 `AcrylicProductionResources.xaml`；该资源继续接入 `DesignTokens.xaml`、`WpfUiProduction.xaml`、`Redesign.xaml` 以及壳层参考控件资源。
- 各真实页面分别合并 `DesignTokens.xaml`、`WpfUiProduction.xaml`、`Redesign.xaml`，并由 `AdaptiveThemePalette` 在运行时替换浅色/深色主题相关画刷。矩阵按资源 key 和页面 `Style="{Static|Dynamic}Resource ...}"` 的直接实例引用统计，不把只在资源文件中声明、没有页面实例的 Demo/Lab 样式当作生产实例。
- 当前直接实例引用包括：`GscWpfUiActionButton` 35、`GscWpfUiSecondaryButton` 35、`GscWpfUiTextBox` 28、`GscWpfUiComboBox` 26、`GscWpfUiToolbarButton` 48、`GscWpfUiToggleSwitch` 38、`GscWpfUiContextButton` 17、`GscWpfUiPrimaryActionButton` 23、`GscWpfUiFilterComboBox` 10、`GscWpfUiPathDetailTextBox` 7、`GscStableDataGridRow` 6、`GscPageScrollViewer` 13、`GscInspectorScrollViewer` 7。
- 页面派生实例单独保留：`MaintenanceDataGrid`/`MaintenanceFirstColumnHeader`/`MaintenanceLastColumnHeader`、`MediaDataGrid`/`MediaFirstColumnHeader`/`MediaMiddleColumnHeader`/`MediaLastColumnHeader`、`SaveDataGrid`/`SaveFirstHeader`/`SaveLastHeader`、`TaskDataGrid`/`TaskFirstColumnHeader`/`TaskLastColumnHeader`、`TrainerTableFrame`/`TrainerTabControl`、`OverviewActivityRowButton` 等，不能由通用 `DataGrid` 或按钮样例代替。

## 状态矩阵

符号说明：`✓` 表示资源模板/触发器明确声明，不自动代表本轮已用输入事件实测；`→` 表示派生实例继承基类状态；`—` 表示控件没有独立状态；`待验` 表示仍需真实页面或宿主观察。`R23ProductionResourceStateBehaviorTests` 只把明确列出的 STA WPF 观察标作行为实测。悬停触发器本轮未驱动真实鼠标输入；无独立按压视觉状态的控件以“不适用”记录。

| 真实生产家族 | 浅色/深色资源 | 正常 | 悬停 | 按压/打开 | 键盘焦点 | 禁用 | 证据与边界 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `GscWpfUiButton` 及 `Secondary/Primary/Danger/Action/Context/Toolbar/IconOnly` 派生 | `GscButtonGlass*`、`GscPrimaryButton*`、`GscError*` 由运行时调色板替换 | ✓ | ✓ | ✓，含 overlay/轻微缩放 | ✓，共享 `GscSharedFocusVisual` + focus overlay | ✓，共享 chrome opacity；文案不靠整控件透明度 | Light/Dark finesse fixture 已实际实例化普通/禁用按钮；状态组合和对比度代理已有 R00-01/02 证据。各派生角色当前未在真实 Playnite 逐一呈现，保留 R23-03/R23-04 边界 |
| `AcrylicNavItem`、`AcrylicSidebarBoundaryButton` | 壳层 `GscAccentTint*`、`GscPrimaryTextBrush` 等动态资源 | ✓ | ✓（模板声明；本轮未驱动鼠标） | BoundaryButton 有按钮态；NavItem 无独立 pressed | NavItem `GscSharedFocusVisual` + 2 DIP 强调边框；BoundaryButton 依既有模板 | NavItem 禁用 opacity `0.46`；BoundaryButton 依既有模板 | 壳层实际引用 7 个导航项和 1 个边界按钮。NavItem 用实际生产 Style 的 RadioButton 在 Light/Dark 隔离 STA Window 验证正常/选中/失选、焦点/失焦和禁用拒焦；边界按钮未由本项测试替代或签收。真实壳层和鼠标输入仍待宿主 |
| `GscWpfUiTextBox` 及 `Path/Technical/Numeric` 派生 | `GscControl*`、`GscAccent*`、`GscError*` | ✓ | ✓ | —（文本框没有独立按压 chrome） | ✓，focus border/fill | ✓，chrome opacity | 生产模板含内容滚动、焦点、校验错误和禁用；数字可读/窄列负例已有 R01-02，真实主题字体和 IME 仍待宿主 |
| `GscWpfUiComboBox`、`Filter`、`PickerFilter` 与 `GscComboBox` | `GscControl*`、`GscPopupBrush`、`GscAccent*` | ✓ | ✓ | ✓，打开态由 toggle/check 与 chevron 表达 | ✓ | ✓ | 页面直接实例化了两套兼容链；弹层、键盘导航和真实 Playnite popup 仍不能用离屏模板宣称通过 |
| `GscWpfUiToggleSwitch`、`GscCheckBox`、`GscDataGridCheckBox`、`GscSlider` | `GscControl*`、`GscAccentBrush`、共享焦点资源 | ✓ | ✓ | Toggle/Check/Slider ✓ | Toggle/Check ✓；Slider 使用共享焦点资源 | Toggle ✓；其余按模板/宿主默认路径待验 | 浅/深 fixture 实例化 toggle、checkbox、slider，并保留实际选择状态；未宣称 OS 键盘/读屏或高 DPI |
| `GscDataGridColumnHeaderStyle`、共享 `DataGridRow/Cell`、`GscStableDataGridRow` 与页面派生表格 | `GscTable*`、`GscRowHoverBrush`、`GscAccent*`、错误状态资源 | ✓ | ✓（行/表头模板声明；本轮未驱动鼠标） | —（行按选择态表达） | ✓，共享焦点 ring；选中/非活动选中分开 | Row ✓；本轮四页网格验证禁用传播与 opacity；header/cell 另待宿主 | 实例化 `TaskCenterView.TaskGrid/TaskDataGrid`、`MediaCenterView.MediaInboxGrid/MediaDataGrid`、`SaveCenterView.SaveHistoryGrid/SaveDataGrid`、`MaintenanceView.FindingsGrid/MaintenanceDataGrid`，每个在 Light/Dark 各一遍；包含实际行/单元格焦点和禁用负例。没有用 `DataGrid` 基类样例代替派生 key；其他同 key 网格实例仍在 R23-03 页面终审核对 |
| `GscRedesignWorkspaceTabItem`、`GscInternalTabItem`、`GscRedesignSettingsTabItem`、`GscSettingsSectionTabs` | `GscControlFillBrush`、`GscAccentTint*`、共享焦点资源 | ✓ | ✓（模板声明；本轮未驱动鼠标） | 选中态 ✓；无独立 pressed chrome | `GscSharedFocusVisual` | `GscSettingsSectionTabItem` opacity `0.45` + disabled text；其他派生项保留各自模板定义 | 本轮实例化真实 `GscSettingsSectionTabs` ListBox 及自动生成的 `GscSettingsSectionTabItem`，Light/Dark 验证选中、焦点/失焦、禁用后不改变选择且不能重新获焦；`Workspace/Internal/Redesign` 其他派生 Tab 未被该样例代替，留给 R23-03 逐页核对 |
| `GscPageScrollViewer`、`GscInspectorScrollViewer`、`GscScrollThumb` | `GscScrollTrackBrush`、`GscScrollThumbBrush`、`GscScrollThumbHoverBrush`、`GscAccentBrush` | ✓ | ✓ | 拖拽 ✓ | —（Thumb 明确不可聚焦） | —（由滚动容器/宿主控制） | 保留当前滚动条系统；已有 R00-06、R00-07 和页面滚动证据，未改成新滚动模型；真实鼠标/触摸、物理 DPI 和 Playnite 宿主仍待验 |
| `GscRedesignSectionCard/SubCard/ReadingCard`、状态/计数/表格 pill | `GscGlass*`、`GscControl*`、Success/Warning/Error/Info tint | ✓ | — | — | — | — | 这些是信息表面而非可操作控件；状态颜色与 `StatusGlyphConverter` 的文本线索由 R22-08 复核，不把颜色样例当作按钮交互证据 |

## 已有行为证据与未验边界

- Light/Dark `finesse-fixture` 均报告 `ControlSurfaceCounts: textboxes=1 combos=1 buttons=4 toggles=1 checkboxes=2 sliders=1 listboxes=1`、普通/禁用按钮已实例化、共享模板声明悬停/按压/焦点/禁用；R00-01/02 对按钮状态组合做了 88 样本对比度代理，R01-02、R00-06/R00-07 分别覆盖数字可读和表格/滚动几何。
- WPF 技能静态检查扫描 31 个 XAML：`0 errors / 30 warnings / 177 info`；其中可见既有 Canvas/StackPanel 容器审阅项，另有两条来自仓库中预先存在的 `.tmp/r12-04-stage` XAML 副本。`python scripts/validate-source.py` 通过；本报告最终编辑后的 `git diff --check` 仍需提交前复核。
- 本轮关闭的三项缺口为：`AcrylicNavItem` 显式共享焦点 ring/2 DIP 强调边框与禁用 opacity；生产设置生成 TabItem 的禁用外观；四种页面派生 DataGrid row 的焦点/选择/禁用行为证据。隔离 Light/Dark 夹具强制关闭 High Contrast override，不代表 OS 高对比度验收。实际被测的是控件模板和逻辑布局，不是像素截图或 Playnite 宿主。
- 未测试悬停鼠标注入/指针事件；悬停只可确认生产模板声明。NavItem、设置 Tab、DataGrid 行没有单独 pressed chrome，分别用选中/按钮自身交互语义表达。未实测 `AcrylicSidebarBoundaryButton` 或所有 `GscRedesignWorkspaceTabItem`、`GscInternalTabItem`、`GscRedesignSettingsTabItem` 实例；这些仍按各自模板/真实页面验收，不由测试样例推断。未启动真实 Playnite/package-host、Windows UIA/读屏、OS 输入/IME、物理 DPI/跨屏、最终呈现帧、ETW 或宿主性能验证。Demo 原目录不可用，沿用恢复生产基线；未读写真实存档/媒体/云端或外发诊断。

## 2026-09-23 R23-02 生产派生状态复测

- 生产变化：`AcrylicNavItem` 增加 `GscSharedFocusVisual`、键盘焦点边框和 disabled opacity；`GscSettingsSectionTabItem` 增加禁用 opacity/文案色。命令、绑定、业务 DTO 和当前滚动条系统未改。
- 测试：`R23ProductionResourceStateBehaviorTests` `3/3`，零失败/跳过。`AcrylicNavigationExposesFocusAndDisabledStatesAfterSelectionClearsInBothThemes` 检查选中/失选、共享 focus visual、焦点进入/离开、禁用 opacity 和不能重新获焦；`SettingsSectionTabsUseTheirGeneratedStyleForSelectionFocusAndDisabledNegativeInBothThemes` 检查生成样式、选中不变量、焦点/失焦、禁用文字/opacity 与拒焦；`EachProductionPageGridKeepsSelectedFocusAndDisabledRowStatesAcrossThemes` 分别实际加载 Task、Media Inbox、Save History、Maintenance 四个派生网格，验证样式 key、选中行、DataGridCell 焦点的 row ring、禁用传播和拒绝重新聚焦。每项都在 Light/Dark 两主题 fixture 上运行。
- 构建：提交 `a18cb43eb3a7a623aeefa1546be5b2407f30173a` 上的隔离 Release solution 构建成功，XAML `24/24`、`0 errors`；Playnite 为 `net462`、Playnite.Tests 为 `net472`。干净构建报告 `MediaCenterView.xaml.cs:706` 的 `CS8602` 两条既有 warning。该提交重跑目标类 VSTest/TRX `3 passed / 0 failed / 0 skipped`、进程 exit `0`。
- 关联 freshness 复核：UI freshness 脚本结果 `12 fresh / 2 stale`，仅 `R00-01-02` 与 `R00-05` 因共享主题 XAML 路径变更待更新源证据身份，package identity 为 `not-provided`。在 a18 上额外复跑 R00-01/02 指定方法 `5/5` 和 R00-05 ContextButton 双主题测试 `2/2`；未重跑 R00-01/02 的 Light/Dark RenderHarness 探针，因此没有把两条旧记录改标 fresh。
- 边界：此处是受控 STA WPF Window、生产资源字典/页面控件和合成行数据；只实测上文列明的焦点、选择和禁用。悬停没有真实输入驱动，静态 trigger 声明不记成交互通过；未验实际 Playnite 包窗口、UI Automation/读屏、操作系统输入/IME、物理 DPI/跨屏、最终呈现帧、ETW 或宿主性能。测试/TRX 为临时产物，已在提交证据前清理，数值和用例名已记入本报告。

## 下一步

进入 `R23-03` 代表页面终审，先复核其既有证据与当前生产 XAML 身份，优先覆盖壳层、概览、存档、媒体、工具、任务、维护、设置的实际空/错/加载和高风险状态；逐页核对本矩阵未直接实例化的派生 tab/button/grid。正常可枚举 Playnite 会话仍受 R23-04 CEF `0x5` 边界限制。
