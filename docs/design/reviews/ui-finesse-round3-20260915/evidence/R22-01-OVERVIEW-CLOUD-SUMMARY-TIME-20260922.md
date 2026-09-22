# R22-01 概览云端队列时间合同（2026-09-22）

## 本批范围

只处理审计中仍有真实生产绑定的概览云端队列摘要。`OverviewView.xaml` 原先把 `Snapshot.CloudTransfers.SummaryDisplay` 同时作为上下文提示和云端队列卡片 Tooltip；该兼容属性在有下次尝试时间时直接拼接本地短日期，不能提供相对主文案、时区和可复制 UTC 证据。

本批复用 `CloudTransferSummaryDto.NextAttemptUtc` 和已有 `TimeDisplayFormatter`，新增：

- `SummaryRelativeDisplay`：概览摘要中的下次尝试使用相对时间，立即可重试和无计划状态沿用已有语义。
- `SummaryFullDisplay`：概览上下文提示和云端队列卡片 Tooltip/Automation HelpText 提供完整本地时间、时区偏移和 round-trip UTC。
- 旧 `SummaryDisplay` 保留为兼容投影，报告、复制列、日志及云端队列命令、筛选、分页、选中和错误/取消语义未改。

## 可复现证据

- 代码提交：`74d3dcdc`（`统一概览云端队列时间显示`）。
- `R22TimeDisplayBehaviorTests`：`28/28`，包含已安排、未知/无计划摘要的正负行为，以及旧 `SummaryDisplay` XAML 绑定移除、Tooltip 和 HelpText 绑定。
- 提交后隔离 Release 构建：XAML `24/24`；Playnite `net462`、Tests `net472`、Contracts/Core/Worker 均构建成功，`0` error；保留仓库既有 `MediaCenterView.xaml.cs:699` 两条 `CS8602` warning。
- `python scripts/validate-source.py`：通过；`git diff --check`：通过。
- `scripts/validate_wpf_ui.py` 在当前 D 盘仓库不存在，因此本批不写 WPF 静态审查通过；本批是绑定/Tooltip 微批量，不新增 render-qa 声明。

## 边界

测试使用合成 `CloudTransferSummaryDto`、固定结构绑定和隔离构建/测试目录；未读写真实存档、媒体、云端或诊断，也未执行真实上传/重试。真实 Playnite/package-host、Windows UIA/读屏、OS 输入/IME、DPI/物理跨屏、最终呈现帧、ETW 和宿主性能仍未验；Demo 原目录不可用，继续参考已恢复生产基线。

下一步继续按实际绑定核对 `DashboardViewModel`/Contracts 其他 stale/缓存时间入口；报告、复制列和日志保持稳定完整时间语义，不由本批代签。
