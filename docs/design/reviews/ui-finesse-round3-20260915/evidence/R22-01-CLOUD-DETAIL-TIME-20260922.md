# R22-01 维护云端状态详情时间合同（2026-09-22）

## 本批范围

审计确认 `MaintenanceView.xaml` 的云端队列表格“状态详情”和选中记录详情都真实绑定 `CloudTransferStatusDto.DetailDisplay`。该兼容投影在有 `NextAttemptUtc` 时直接显示本地 `MM-dd HH:mm`，不符合相对正文、完整时间提示和 UTC 证据的统一合同。

本批复用已有 `NextAttemptUtc`、`RetryTimingRelativeDisplay` 和 `TimeDisplayFormatter`：

- `DetailRelativeDisplay`：表格状态详情和选中详情正文使用相对重试时间，保留“立即重试/约 N 分钟后”等已有状态语义。
- `DetailFullDisplay`：表格 TextBlock 的 Tooltip/Automation HelpText，以及选中详情的 Tooltip/Automation HelpText 提供完整本地时区、偏移和 round-trip UTC。
- 旧 `DetailDisplay` 保留给 Worker 维护报告等稳定完整文本路径；没有改变队列命令、筛选、分页、选中、上传/校验、取消或错误语义。

## 可复现证据

- 代码提交：`db2ba3c0`（`统一云端状态详情时间显示`）。
- `R22TimeDisplayBehaviorTests`：`29/29`；覆盖有计划重试、未知计划负例、XAML DataGrid/选中详情绑定及旧绑定移除。
- 提交后隔离 Release 构建：XAML `24/24`；Playnite `net462`、Tests `net472`、Contracts/Core/Worker 均构建成功，`0` error；保留仓库既有 `MediaCenterView.xaml.cs:699` 两条 `CS8602` warning。
- `python scripts/validate-source.py`：通过；`git diff --check`：通过。
- `scripts/validate_wpf_ui.py` 在当前 D 盘仓库不存在；本批是绑定/Tooltip 微批量，不新增 WPF 静态审查或 render-qa 通过声明。

## 边界

测试使用合成 `CloudTransferStatusDto` 和隔离构建/测试目录，没有执行真实上传、远端校验、重试或云端写入，也未读写真实存档/媒体/诊断。真实 Playnite/package-host、Windows UIA/读屏、OS 输入/IME、DPI/物理跨屏、最终呈现帧、ETW 和宿主性能仍未验；Demo 原目录不可用，继续参考已恢复生产基线。

下一步继续盘点 `DashboardViewModel`/Contracts 其余真实 stale/缓存绑定；报告、复制列和日志保持稳定完整时间语义，不由本批代签。
