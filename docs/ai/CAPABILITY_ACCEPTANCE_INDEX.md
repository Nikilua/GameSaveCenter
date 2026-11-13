# 核心能力验收索引

本页只提供“能力—入口—风险测试—最后验证源码—未验条件”的轻量入口，不复制TRX、日志或历史账本。当前产品验证身份统一引用 `b667e22445fbf57aa591f39f3d968f5c166c7cb6`；后续产品源码变化时，应先更新精确门禁证据再改本表。

| 能力 | 当前用户/服务入口 | 关键风险测试入口 | 最后验证源码与证据 | 仍未验条件 |
| --- | --- | --- | --- | --- |
| 备份 | Save Center `BackupSelectedCommand` / `PreviewBackupCommand`；Worker `BackupOrchestrator` | `BackupPreviewBehaviorTests`、`R11BackupPreviewBehaviorTests`、请求账本与原子写边界 | `b667e224`；[A09/A10入口审查](../design/reviews/project-assessment-2026-11-06/evidence/A09-A10-audit-20261113/README.md)、[P1本地收口](../design/reviews/project-assessment-2026-11-06/evidence/P1-close-b667e224-20261113/README.md) | 真实游戏规则、真实存档写入和同会话首次使用闭环未执行 |
| 恢复 | Save Center `ValidateRestoreReadinessCommand` / `RestoreCommand` / `UndoRestoreCommand`；Worker `RestoreReadinessService` / `RestoreOrchestrator` | `RestoreReadinessTests`、`RestoreOrchestratorTests`、`R12RestoreWorkflowBehaviorTests`、磁盘满与PreRestore恢复边界 | `b667e224`；[A09/A10入口审查](../design/reviews/project-assessment-2026-11-06/evidence/A09-A10-audit-20261113/README.md)、[A05/A06证据](../design/reviews/project-assessment-2026-11-06/evidence/A05-A06-20261112/README.md) | 未授权真实恢复；真实进程占用、反病毒和物理磁盘故障仍需专用环境 |
| 云传输 | Overview/Maintenance云队列、验证与重试命令；Worker `CloudTransferStateService` | `CloudTransferStateTests`、`CloudRetryPolicyTests`、`CloudRetryPersistenceTests`、`R13CloudTransferStageBehaviorTests` | `b667e224`；[P1本地收口](../design/reviews/project-assessment-2026-11-06/evidence/P1-close-b667e224-20261113/README.md) | 本轮禁止云端写入；真实rclone目标、断网恢复和凭据边界未执行 |
| 媒体 | Media Center同步、分页、收件箱、归类与比较命令；Worker `MediaSyncService` | `MediaSyncServiceTests`、`MediaQueryPersistenceTests`、`MediaInboxScrollBehaviorTests`、`R18ThumbnailBudgetTests` | `b667e224`；[A09/A10规模审查](../design/reviews/project-assessment-2026-11-06/evidence/A09-A10-audit-20261113/README.md)、[P1本地收口](../design/reviews/project-assessment-2026-11-06/evidence/P1-close-b667e224-20261113/README.md) | 真实大媒体库的宿主滚动、缩略图解码和云同步未验 |
| 游戏工具 | Trainer Center导入、保存、启动、目录、删除、迁移与目录同步命令；Worker `GameToolService` | `GameToolServiceImportTests`、`GameToolLauncherTests`、`GameToolPolicyPersistenceTests`、`FlingTrainerCatalogSourceTests` | `b667e224`；[P1本地收口](../design/reviews/project-assessment-2026-11-06/evidence/P1-close-b667e224-20261113/README.md) | 未启动真实修改器或下载目录；杀毒拦截、提权和第三方站点变化未验 |

索引边界：自动测试通过不等于真实Playnite可用性；skip不计通过；required-checks名单仍受既有403限制。原始run/job/artifact、SDK、TRX计数和hash以链接证据为准。
