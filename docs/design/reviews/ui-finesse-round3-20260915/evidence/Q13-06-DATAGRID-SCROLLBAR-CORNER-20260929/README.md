# Q13-06 DataGrid 滚动条交角与末行末列（2026-09-29）

## 现有实现与测试缺口

生产共享 `GscRedesignDataGridTemplate` 已用 3×3 Grid 为表头、内容视口、横向条和纵向条分配独立区域：表头在第 0 行，`PART_ScrollContentPresenter` 在第 1 行/第 1 列，纵条在第 1 行/第 2 列，横条在第 2 行/第 1 列。无需改生产模板。

旧 `MediaInboxScrollBehaviorTests` 实测 2,000 条记录、虚拟化、首行位置、滚动到底时末行完整和页尾操作可达，但没有量测两条滚动条的交角，也没有确认最右单元格完整落在视口内。此提交只为这个行为缺口补了生产 WPF 回归。

## 当前身份行为验证

- 源码身份：`b98ea5a2`，分支 `main`。
- Release solution 构建成功，0 warning / 0 error；XAML 结构 `24/24`。本阶段没有生产 XAML/C# 修改。
- `MediaInboxScrollBehaviorTests` `4/4`，0 failed/skipped，VSTest exit `0`；新增 `InboxScrollBarsReserveTheCornerAndKeepTheFinalRowAndRightmostCellInsideViewport`。`Q13ScrollBarThumbBehaviorTests` `1/1`，0 failed/skipped，exit `0`，保留共享滚动条纵横向、Light/Dark Hover 与端点检查。
- Playnite 程序集 SHA-256：`CAED633A61CFE0B9CCFEE74D3AAD43A6A0F739AD844F1ED459B15FD8C85AF1A2`；Playnite 测试程序集 SHA-256：`64CEC2B52C197F9A421FBE7308C9CB495E8187D30BBD90A186C6BE344DD8F0C9`。
- 生产 `MediaCenterView` / `MediaInboxGrid` 装入 2,000 条合成媒体项；有限网格视口为 `620×400 DIP`，横纵条均可见，行虚拟化、`CanContentScroll=True`、`ScrollUnit.Item` 和 `VirtualizationMode.Standard` 保持。
- 顶部、横纵中段和右下端三处都测得：列头 `y=0..82`、内容视口 `x=0..608,y=82..388`、横条 `x=0..608,y=388..400`、竖条 `x=608..620,y=82..388 DIP`。条与视口/彼此只共边，没有超过 `0.5 DIP` 的重叠面积。三个位置的滚动偏移分别覆盖横向 `0/12/24 of 24` 与纵向 `0/997.5/1995 of 1995`。
- 右下端最后一行是索引 `1999`，完整位于视口；该行五个单元格均已实现且含文本，最后一列滚至最右后完整落在视口内。2,000 条有限列表仍通过容器数上限。
- WPF 夹具的 `VisualTreeHelper.GetDpi` 报告 `1.5×1.5`。它是本次受控窗口报告的有效 DPI；这不是 125%/150% 多显示器切换、真实 Playnite 窗口或显示器像素呈现证明。既有 `outputScale=1/1.25/1.5` 样本是 RenderTransform，未作为物理 DPI 证据。

TRX：[Media Inbox 交角/末行末列](media-inbox-scroll-corner-b98ea5a2.trx)、[共享 Thumb 双主题行为](q13-scrollbar-thumb-b98ea5a2.trx)。Media Inbox 测试退出还打印了 WPF `TextServicesHost.InvalidComObjectException` 清理噪声；TRX 为 `4/4`、进程 exit `0`，原因未知。

## 结论与边界

Q13-06 的自动行为条件已有生产 WPF 测量支持：视口与横纵条分区，最右下单元格完整，有限列表/虚拟化保持；没有发现需要修改生产模板的缺陷。Round2 任务表维持 `代码完成 / 自动检查通过 / 宿主像素边界待验 / 最终未完成`。没有启动 Playnite，也没有真实屏幕鼠标/像素测试。
