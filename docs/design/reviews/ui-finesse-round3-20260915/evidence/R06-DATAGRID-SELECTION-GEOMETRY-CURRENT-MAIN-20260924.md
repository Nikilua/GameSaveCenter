# R06 DataGrid 选中/焦点轮廓布局复核

日期：2026-09-24  
构建 checkout：`main`，实现期间 HEAD `f8a82469`；Release 隔离输出 `.tmp/rowchrome`

## 修改

共享 `GscRoundedDataGridRowTemplate` 原来把 `SelectiveScrollingGrid` 放在 `RowChrome` Border 内，selected/focus trigger 改 Border Margin 时会连 DataGrid 内容一起重新测量和缩进。现在模板以同级元素分别承载 `RowBackground`、`SelectiveScrollingGrid` 和透明、不可命中的 `RowChrome`；选择和键盘焦点只改变背景/描边层，不参与 cell 布局。保留原圆角、边框、右侧滚动条安全间距、`SelectiveScrollingGrid` 行细节滚动，以及行容器虚拟化。

拆分后，disabled 透明度移到 `DataGridRow` 本身，继续淡化整行文字、徽章和内容；描边层保持自身 opacity=1，避免仅把内容移出旧 Border 后丢失禁用态。

## 行为与回归证据

五份同一隔离 Release 测试程序集的 TRX 全部通过：

- `R06SelectionStateBehaviorTests`：`2/2`。实际 WPF DataGrid 两列中先记录未选行每个 cell 与 TextBlock 相对表格的 x/y/width/height，再切换到失焦选中和键盘焦点选中；各值差异均由 `0.25 DIP` 容差门禁约束。另验证失焦选择配色、聚焦描边、失败状态仍在单元格中。
- `R23ProductionResourceStateBehaviorTests.EachProductionPageGridKeepsSelectedFocusAndDisabledRowStatesAcrossThemes`：`1/1`。同一行为覆盖 Save、Task、Media Inbox、Maintenance 四个实际生产表格，在 Light/Dark 两主题分别比较未选、失焦选中、键盘焦点选中几何；同时验证禁用行 opacity=0.42、描边层 opacity=1 和 cell 不可用。
- `ReportedWorkspaceLayoutBehaviorTests`：`8/8`，原四页布局回归未受共享模板调整影响。
- `WpfUiResourceDictionaryTests.SharedDataGridSelectionUsesTrainerRoundedCardChrome`：`1/1`，核对生产模板中滚动内容与行 chrome 为同一 Grid 的独立兄弟，且 chrome 不参与命中测试。
- `R06SortingBehaviorTests`：`7/7`，确认实际列头排序及 detached-view 崩溃修复未回退。

总计 `19/19`，0 失败/跳过。TRX：

- [DataGrid 选择/焦点几何](R06-DATAGRID-SELECTION-GEOMETRY-20260924.trx)
- [四页生产表格双主题状态与几何](R23-PRODUCTION-ROW-STATE-GEOMETRY-20260924.trx)
- [模板结构门禁](R06-ROW-CHROME-TEMPLATE-CONTRACT-20260924.trx)
- [用户四页布局回归](USER-LAYOUT-ROW-CHROME-20260924.trx)
- [排序回归](R06-SORTING-REGRESSION-AFTER-ROW-CHROME-20260924.trx)

Release solution 编译成功：Playnite `net462`、Tests `net472`，XAML `24/24`，0 errors；两条既有 `MediaCenterView.xaml.cs:703 CS8602` warning。source validator 与 `git diff --check` 通过。

## 边界与后续

这里证明的是隔离 STA WPF 窗口中的生产模板布局与状态，不等于真实 Playnite/package-host 的最终呈现帧、鼠标/键盘物理输入或用户安装包问题已经关闭；正常隔离宿主仍受 CEF `platform_channel 0x5` 阻挡。未改表格数据/命令、滚动系统、业务错误/取消/恢复语义，也未做宿主性能基准。

这是用户所报选中行列内容缩进的定点修复，不签收整个 R06-03。下一可执行小批量：移除 Media Inbox 单独的目标游戏选择，复用顶部全局 `SelectedGame`，同时核实单项/批量命令的空目标、确认目标快照、取消和失败行为。
