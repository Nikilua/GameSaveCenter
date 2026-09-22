# R22-01 时间显示统一审计收口（2026-09-22）

## 结论

R22-01 的当前代码与受控证据已满足“相对时间、完整时间、时区提示、原始 UTC 可复制值、稳定边界、UTC 排序和单调任务时长”合同，账本状态改为“已满足，待环境验证”。本批没有重建时间服务或替换 DTO；只补了生产 XAML 入口审计夹具，确认先前各子批次的实际绑定没有被兼容属性重新带回。

## 本批实际审计

- `ed03c49e` 新增 `R22TimeDisplayBehaviorTests.ProductionViewsDoNotBindLegacyLocalTimeProjections`，读取当前 `src/GameSaveCenter.Playnite/Views/*.xaml`，负向检查 `CreatedDisplay`、`ComparisonDisplay`、`LastSyncDisplay`、`PublishedDisplay`、`LatestBackupDisplay`、`TaskPageLastUpdatedDisplay`、`SelectedGameLastBackupDisplay`、`LastAccessDisplay`、`CheckedLocalDisplay`、`GeneratedDisplay`、`LastAttemptDisplay`、`LastSuccessfulVerificationDisplay`、`RetryTimingDisplay` 及云端 `DetailDisplay` 旧绑定均不在生产视图中。
- 同一夹具正向确认生产视图仍有 `SelectedGameLastBackupRelativeDisplay`/`FullDisplay`、`TaskPageStatusSummaryFullDisplay`、`ComparisonRelativeDisplay`/`FullDisplay` 和 `DetailRelativeDisplay`/`FullDisplay`，不是只用负向字符串断言签收交互。
- Dashboard/Contracts 余下直接 `ToLocalTime` 入口逐项复核后分为三类：DTO 兼容投影；报告/导出/复制列需要稳定完整本地时间；Worker 日志和诊断文件需要稳定生成时间。`DataGridClipboardBehavior`、Dashboard 报告生成、`MaintenanceReportService` 和 `WorkerLauncher` 未被误改成相对时间，也没有把这些稳定输出当作页面绑定缺陷。

## 验证

- 开发分支 `R22TimeDisplayBehaviorTests`：`30/30`；主分支合并后同一组：`30/30`。
- 当前提交身份下 `R22TaskDurationBehaviorTests`：`2/2`；覆盖计数器差值、倒退/无效频率负例和终态耗时持久化。
- 开发分支 Worker.Tests Release 构建：`0 warning / 0 error`；主分支合并提交 `002c2c63` 的完整 solution Release 构建：`0 error / 2` 条既有 `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs:699` `CS8602` warning。
- `python scripts/validate-source.py`、`git diff --check` 通过；完整 solution 仍以 Playnite `net462`、Tests `net472` 和 Worker 目标成功生成。

## 保留边界

本批使用合成 DTO、源代码审计和隔离 testhost，没有访问真实存档、媒体、云端、用户诊断或真实 Playnite 数据。Demo 原目录不可用，继续以已恢复生产基线和 Demo-first 资源链为准。真实 Playnite/package-host 当前呈现、Windows UIA/读屏、OS 输入/IME、DPI/物理跨屏、最终 presented frame、ETW/WPR 和宿主性能仍未验；`scripts/validate_wpf_ui.py` 当前仓库不存在，未将缺失脚本写成 WPF 静态通过。

下一可执行任务：依照账本进入依赖已满足的 R23-04 UIA/Controlled host 收口；若宿主可达性仍受环境限制，则执行 R23-05/相关残余的独立受控验证，不回头重做已满足的 R22-01 时间实现。
