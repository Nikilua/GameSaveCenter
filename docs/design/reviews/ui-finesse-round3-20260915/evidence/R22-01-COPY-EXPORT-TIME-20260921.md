# R22-01 复制列与报告/日志时间证据

日期：2026-09-21

## 本子批范围

本批核对 SaveHistory DataGrid 的复制行/复制单元格，以及已有维护报告和诊断/Worker 日志中的机器可读时间。复制、导出和内部日志不改成相对时间；相对时间只用于用户可见正文，避免破坏稳定脚本/审计格式。

## 已核对与结论

- `DataGridClipboardFormatter` 的 SaveHistory 行和“时间”单元格继续输出本地 `yyyy-MM-dd HH:mm:ss`，按稳定 `BackupId` 去重，保留 TSV 分隔、凭据脱敏和完整值复制；这不是用户可见相对时间缺口，按“已满足”记录。
- Worker `MaintenanceReportService` 生成时间继续输出完整本地秒级时间；Dashboard 诊断摘要、失败任务报告和 `WorkerLauncher` 日志继续使用完整本地时间/时区格式；没有改写内部日志或报告字段。
- SaveHistory 的游戏选框、虚拟化/滚动、复制命令和不写入真实用户数据语义保持；报告复制/导出仍由既有命令和隔离目录控制。

## 证据

- `R06ClipboardBehaviorTests 4/4`：实际调用 SaveHistory 复制行/单元格格式化，验证完整本地秒级时间、类型字段、复制顺序、稳定去重和脱敏负例。
- `R22TimeDisplayBehaviorTests 23/23`；Worker `MaintenanceReportServiceTests 2/2` 实际验证报告生成时间包含完整本地秒级值。Playnite/Worker 隔离 Release 构建：XAML `24/24`，Playnite `net462`、Tests `net472` 和 Worker `net8.0` `0 errors`，仅既有 `MediaCenterView.xaml.cs:671` 2 条 CS8602 warning。
- `scripts/validate-source.py`、`git diff --check` 通过；WPF 静态基线为 `0 errors / 27 warnings / 177 info`。

## 边界

验证使用合成 DTO、fake/隔离 testhost 和隔离构建目录，没有真实剪贴板、真实报告文件、真实日志目录、存档、媒体、云端或诊断外发证据；Demo 原目录不可用。未声称真实 Playnite/package-host、Windows UIA/读屏、系统时钟跳变、DPI/跨屏、presented frame、ETW 或宿主性能已验证。R22-01 仍有其他残余 `ToLocalTime` 入口需按实际绑定逐项核对，例如校验有效期、任务页状态、快照更新时间和报告内部字段；下一步先选择依赖满足的 Q/R 项并保留这些未验边界。
