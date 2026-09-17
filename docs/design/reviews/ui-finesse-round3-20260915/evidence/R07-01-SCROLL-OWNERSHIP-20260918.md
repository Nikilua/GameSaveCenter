# R07-01 滚动所有权验收证据

日期：2026-09-18  
分支：`codex/ui-finesse-round2`  
代码提交：`9c878b9cb9a8cd07075ede332c9534e1d957f6b3`

## 结论

R07-01 在当前可控范围内已满足。页面主滚动、表格内部滚动和详情滚动沿用现有滚动条系统；新增统一边界行为后，内层表面在自身还能滚动时保留原生处理，到方向边界才转给最近可滚动外层，不吞掉可继续向外传递的滚轮。

## 现有所有权核对

- 页面级：`GscPageScrollViewer` 是垂直主滚动通道，水平关闭；Overview、Media Inbox、Save Compare、Settings 等页面按既有命名使用。侧栏 `SidebarNavigationScrollViewer` 和顶部动作 `TopActionsScroller` 仍是各自局部通道。
- 表格级：`GscRedesignWorkspaceDataGrid` 的模板内部 `DG_ScrollViewer` 继续拥有表格行/列滚动，`CanContentScroll=True`、`VirtualizingPanel.ScrollUnit=Item`、行虚拟化和列虚拟化保持；Save、Task、Media、Maintenance 的页面样式继续从该共享契约派生。
- 详情级：`GscInspectorScrollViewer` 及既有命名 Inspector 保持垂直 Auto、水平 Disabled 和独立高度预算；长诊断、路径、预览说明不抬高所有列表行。
- 发现的缺口是这些层级没有统一的滚轮边界转发契约；旧 Dashboard 私有处理器未接线且固定滚动三行，已移除，避免成为错误实现入口。

## 实现与行为证据

- 新增 `ScrollBoundaryRoutingBehavior`，接入共享 `GscPageScrollViewer`、`GscInspectorScrollViewer` 和 `GscRedesignWorkspaceDataGrid`。它只处理垂直滚轮：内层方向上仍有空间时让 WPF 原生处理；到边界且最近外层可滚动时按滚轮增量调用外层 `LineUp/LineDown` 并标记当前事件，外层也到边界或不存在时不伪造移动。
- DataGrid 路径从事件源解析模板内最近 `ScrollViewer`，因此不会把表格边界误当成页面本身；普通嵌套详情路径按视觉/逻辑树寻找最近外层。未改 ScrollBar 样式、`CanContentScroll`、虚拟化、游戏选框或命令/Binding。
- `R07ScrollOwnershipBehaviorTests 2/2` 是实际 STA WPF 行为夹具：普通嵌套 ScrollViewer 覆盖内层底部向下、内层顶部向上两种传递；实际 DataGrid 覆盖内表格到边界后页面 offset 前进、表格 offset 不跳和可见行仍存在。
- 相邻回归筛选通过 `14/14`，包括 `TaskCenterViewResponsiveTests`、`MediaInboxGeometryTests`、`DetailsDisclosureSourceTests` 和 `R06DetailsBudgetBehaviorTests`。
- 最终提交重建后 Release XAML `24/24`、生产 net462/测试 net472 编译 `0 warning / 0 error`；`validate-source.py` 已将门禁从旧固定滚动调用迁移为共享行为的真实结构/接线检查，`git diff --check` 通过。

## 离屏渲染与视觉核验

报告：`.tmp/r07-01-scroll-final/render-qa-report.txt`

- 报告绑定 `9c878b9cb9a8cd07075ede332c9534e1d957f6b3`，`WorkingTreeClean: True`，Light/Dark、多尺寸（1040×700、1100×720、1366×768、2560×1440 等）和 `2560×1440 → 1100×720 → 2560×1440` resize 均 `render-qa OK`。
- 1040×700 最小窗口仍报告 Save `4/4`、Media `6/4`、Maintenance Findings `5/4`、Task `5/4` 可读行；1366×768 Task 最窄布局为 `4/4`，resize 步骤 Task `5/4`。报告同时保留各表的 `DG_ScrollViewer` 和页面/详情 ScrollViewer 的 Auto/Disabled 方向记录。
- PageHost 几何 QA 记录 Media 1040/1100/1366 的页面 ScrollViewer 及表格位置、Maintenance 紧凑 Inspector 的打开/关闭状态、Task 紧凑 160 DIP 详情与宽布局 `360×516` 侧栏详情；已查看 `Task-1040x700.png`、`Media-1040x700-tab0.png`、`Maintenance-1040x700-tab0.png`。

## 边界与下一步

行为证据使用合成内容、隔离 STA WPF Window 和 offscreen logical DIP（`DpiScale=1.00`）。没有启动真实 Playnite/Worker，也没有真实鼠标/触控板设备轨迹；未验 OS 输入/IME、UIA/读屏、物理 DPI/跨屏、presented frame、ETW、宿主性能或外层主题对 routed event 的额外改写。未写真实存档、媒体、云端或诊断数据。Demo 原始 `DesignShellView.xaml`/`Pages` 当前 checkout 仍不存在，继续沿用恢复生产基线。

下一可执行任务为 R07-02「锚点删除回退」：先核对刷新、删除、筛选、加载更多的现有稳定 ID 和视口恢复路径，再补对象消失时邻近项回退的实际行为/负例，不能跳首行或错选同索引对象。
