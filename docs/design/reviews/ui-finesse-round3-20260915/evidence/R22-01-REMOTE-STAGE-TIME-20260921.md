# R22-01 远端隔离有效期

日期：2026-09-21  
代码提交：`f39b8ef1`（`统一远端隔离有效期显示`）

## 本批范围

先核对 `RemoteBackupStageResultDto`、`DashboardViewModel.StagedRemoteBackupStatus` 和 Maintenance 设备状态页的实际绑定，确认远端隔离有效期是用户可见的安全状态。本批只调整有效期的时间投影与提示，不改变远端下载、校验、取消、隔离区清理、PreRestore 快照、恢复命令门控或“当前存档不会被覆盖”语义。

复用 Contracts 中已有 `TimeDisplayFormatter`：

- `RemoteBackupStageResultDto` 新增 Staged/Expires 的相对、完整、原始 UTC 投影。
- `StagedRemoteBackupStatus` 正文使用相对有效期；`StagedRemoteBackupStatusFullDisplay` 提供完整本地时区与 round-trip UTC。
- Maintenance 实际隔离状态 TextBlock 的 Tooltip 与 `AutomationProperties.HelpText` 均绑定完整状态；未下载状态和下载中/取消/失败状态保持原文，不伪造有效期。

## 行为证据

`R22RemoteStageTimeBehaviorTests` 覆盖：

- 合成结果的 Staged/Expires 三种时间投影和默认时间未知负例。
- 生产 `DashboardViewModel.BuildStagedRemoteBackupStatus` 的相对正文、完整提示和未下载状态。
- 实际 STA WPF `MaintenanceView` 设备状态 TextBlock：正文读取相对有效期，Tooltip 与 HelpText 读取完整本地/UTC 证据。

## 验证结果

- `R22RemoteStageTimeBehaviorTests`：`3/3`
- 相邻 `R13CloudTransferStageBehaviorTests`（含理论用例）：`13/13`
- `DeviceConflictStateSourceTests`：`1/1`
- 定向合计：`17/17`
- Release 隔离构建：XAML `24/24`，Contracts/Playnite net462、Tests net472、Worker `0 errors`；构建输出仍只涉及既有 `MediaCenterView.xaml.cs:671 CS8602` 基线。
- `python scripts/validate-source.py`：通过。
- WPF 静态审查：`0 errors / 27 warnings / 177 info`。
- `git diff --check`：通过。

## 未验边界

证据只来自合成 `RemoteBackupStageResultDto`、fake DataContext、隔离 STA WPF、隔离 Release 构建和隔离测试目录；没有真实远端下载、Worker、真实隔离目录、恢复、存档、媒体、云端或诊断写入。未宣称真实 Playnite/package-host、Windows UIA/读屏、系统时钟跳变、DPI/物理跨屏、最终呈现帧、ETW 或宿主性能已验；Demo 原目录不可用，参考已恢复生产基线。恢复确认框、校验有效期及其他残余用户可见旧 `ToLocalTime` 入口仍需逐项核对。

