# Product Hardening Layer B 最终审计

> 审计日期：2026-08-14
> 范围：Layer B 13 项
> 原则：逐项给出生产代码、测试、自动验证与人工验收边界；B13 完成后不再宣称 Layer A 状态可代表整个 Epic。

## 汇总

| Task | Status | 主要生产代码 | 主要测试 | AUTO VERIFIED | MANUAL QA REQUIRED | Commit |
|---|---|---|---|---|---|---|
| DIAGNOSTICS-001 | COMPLETED | `DiagnosticsPackageService.cs`、`DiagnosticRedactor.cs` | `DiagnosticsPackageServiceTests.cs` | 是 | 真实 ZIP 人工检查脱敏与字段 | `17e88b6` |
| SAFE-MODE-001 | COMPLETED | `WorkerOptions.cs`、`WorkerInitializationService.cs`、设置/维护 UI | `SafeModeStartupTests.cs` | 是 | 连续失败 3 次提示与恢复正常模式 | `17e88b6` |
| INTEGRITY-001 | COMPLETED | `IntegrityCheckService.cs`、`SqliteStateStore.Integrity.cs` | `IntegrityCheckServiceTests.cs` | 是 | 真实孤儿 ZIP/损坏 Manifest/磁盘不足 | `a064108` |
| DB-MIGRATION-001 | COMPLETED | `DatabaseMigrationHarness.cs` | `DatabaseMigrationHarnessTests.cs` | 是 | 真实历史用户库升级回归 | `55f532f` |
| METADATA-BACKUP-001 | COMPLETED | `MetadataBackupService.cs`、`AtomicFileWriter.cs` | `MetadataBackupServiceTests.cs` | 是 | 真实恢复与失败回滚 | `97b06b5` |
| REPOSITORY-REBUILD-001 | COMPLETED | `RepositoryRebuildService.cs` | `RepositoryRebuildServiceTests.cs` | 是 | 真实仓库预览/确认/幂等 | `dff0cf4` |
| PATH-REMAP-001 | COMPLETED | `PathRemapService.cs`、`SqliteStateStore.PathRemap.cs` | `PathRemapServiceTests.cs` | 是 | 真实目录迁移与缺失目标策略 | `bddcfdd` |
| TASK-RECONCILE-001 | COMPLETED | `TaskReconcileService.cs`、`TaskCoordinator.cs`、`WorkerOptions.cs` | `TaskReconcileServiceTests.cs` | 是 | 真实中断后核对分类与 PreRestore | `47685bd` |
| GAME-OP-LOCK-001 | COMPLETED | `GameOperationLock.cs`、`BackupOrchestrator.cs`、`RestoreOrchestrator.cs`、`MediaSyncService.cs` | `GameOperationLockTests.cs` | 是 | 真实长备份期间并发操作 | `7dba09c` |
| IPC-COMPAT-001 | COMPLETED | `WorkerDtos.cs`、`IpcRequestDispatcher.cs` | `ProtocolCompatibilityTests.cs` | 是 | 旧 Worker/新插件拒绝与替换 | `7ee738d` |
| ATOMIC-IO-001 | COMPLETED | `AtomicFileWriter.cs`、`WorkerOptions.cs`、`MediaSyncService.cs` | `AtomicFileWriterTests.cs` | 是 | 真实写入中途终止/断电 | `eed946c` |
| SOAK-001 | COMPLETED | `SoakDataScaleHarness.cs`（测试 Harness） | `SoakDataScaleTests.cs` | 是 | 真实 24/48 小时宿主与 2000 游戏库 | `c9e053a` |
| FAULT-INJECTION-001 | COMPLETED | `FaultInjectionHarness.cs`（测试 Harness） | `FaultInjectionTests.cs` | 是 | 真实磁盘满/权限/进程强杀 | `738f339` |

## 逐项验收

### DIAGNOSTICS-001

- 生产证据：维护中心可导出有上限、只读、集中脱敏的结构化 ZIP；包含 `system/worker/dependencies/database/recent-tasks/health/settings` JSON、审计与受限日志，不包含数据库、存档、媒体或凭据。
- 自动验证：`DiagnosticsPackageServiceTests` 覆盖敏感字段与大小边界；真实开发安装后 Playnite/Worker 正常加载。
- 人工验收：真实导出 ZIP 后检查 JSON 字段与脱敏结果。

### SAFE-MODE-001

- 生产证据：连续 3 次启动失败自动请求安全模式，Playnite 询问确认；设置页与维护中心提供手动开关和“恢复正常模式”；安全模式暂停自动任务。
- 自动验证：`SafeModeStartupTests` 覆盖三次失败请求、成功清计数、请求清除、设置持久化与 UI 绑定。
- 人工验收：真实连续启动失败后确认提示与安全模式效果。

### INTEGRITY-001

- 生产证据：自检覆盖 SQLite、备份记录↔归档、孤儿归档、Manifest 缺失/无效/重复路径、GameTool、媒体引用、依赖和分卷磁盘空间；结果四态且只读。
- 自动验证：`IntegrityCheckServiceTests` 覆盖孤儿、Manifest、低磁盘和依赖状态。
- 人工验收：真实磁盘不足、孤儿 ZIP、损坏 Manifest 场景复核。

### DB-MIGRATION-001

- 生产证据：`DatabaseMigrationHarness` 在临时目录创建两代旧库 Fixture 后执行当前 `SqliteStateStore.InitializeAsync`，覆盖策略、模板、会话、设备决策、GameTool、备份历史与版本表。
- 自动验证：`DatabaseMigrationHarnessTests` 使用 `ReadScalar` 验证真实数据值、重复迁移幂等且不触碰用户数据。
- 人工验收：真实历史用户库升级后的功能回归。

### METADATA-BACKUP-001

- 生产证据：导出 SQLite `VACUUM INTO` 快照、脱敏 Worker 设置与版本清单；恢复流程预览校验 manifest/哈希/越界，执行前备份当前元数据、原子替换、完整性校验并在失败时回滚。
- 自动验证：`MetadataBackupServiceTests` 覆盖导出、预览、确认、替换、回滚和回滚失败。
- 人工验收：真实选择元数据包执行恢复并核对恢复前副本。

### REPOSITORY-REBUILD-001

- 生产证据：只读扫描预览统计已确认/未归属/部分缺失/损坏归档；写库必须确认；失败游戏保留原索引且重复重建幂等。
- 自动验证：`RepositoryRebuildServiceTests` 覆盖预览、未确认拒绝、确认执行与幂等。
- 人工验收：真实 Ludusavi 仓库预览后执行重建并核对版本数量。

### PATH-REMAP-001

- 生产证据：只读预览按类型列出受影响旧/新路径与目标存在状态；目标缺失默认跳过，可显式授权仍应用；执行前自动创建元数据灾备；只替换严格前缀匹配。
- 自动验证：`PathRemapServiceTests` 覆盖预览、缺失目标策略、严格前缀与自动灾备。
- 人工验收：真实移动目录后预览执行并核对数据库、设置与灾备包。

### TASK-RECONCILE-001

- 生产证据：任务持久化 `WorkerSessionId`；启动只协调旧 Worker 会话遗留任务；Backup/Media/Cloud 可重试，Integrity 普通中断，Restore 人工介入且不自动重试。
- 自动验证：`TaskReconcileServiceTests` 覆盖分类、错误信息与手动协调复用。
- 人工验收：真实中断恢复任务后核对 `MANUAL_INTERVENTION_REQUIRED` 与 PreRestore。

### GAME-OP-LOCK-001

- 生产证据：`GameOperationKind` 与显式兼容矩阵写入代码；备份/恢复/媒体/云端使用类型化锁；Restore 不与其他操作并发，同游戏双 Backup 禁止，不同游戏并行。
- 自动验证：`GameOperationLockTests` 覆盖兼容矩阵、超时、释放与并行。
- 人工验收：真实长备份期间对同一游戏发起恢复/媒体同步。

### IPC-COMPAT-001

- 生产证据：`WorkerHandshakeDto` 返回 `AppVersion` 与 `Capabilities`；协议版本独立于应用版本，能力覆盖 Layer B 新功能。
- 自动验证：`ProtocolCompatibilityTests` 覆盖能力列表与版本独立语义。
- 人工验收：保留旧 Worker 后启动新插件，确认握手拒绝并替换。

### ATOMIC-IO-001

- 生产证据：共享 `AtomicFileWriter` 覆盖 Worker 设置、媒体复制、元数据恢复替换与启动失败计数；临时文件同目录、失败清理、旧文件完整保留。
- 自动验证：`AtomicFileWriterTests` 覆盖取消写入、替换失败、旧文件保留与无残留。
- 人工验收：真实进程写入中途被终止/断电后确认无半成品。

### SOAK-001

- 生产证据：`SoakDataScaleHarness` 默认小规模、`GSC_SOAK_DATA_SCALE=1` 全量规模；循环模拟快照、事件扇出、原子写入和操作锁，并监控内存/句柄/线程/订阅/临时文件。
- 自动验证：`SoakDataScaleTests` 做有界增长断言并记录错误。
- 人工验收：真实 24/48 小时宿主持续运行与大库挂机趋势观察。

### FAULT-INJECTION-001

- 生产证据：`FaultInjectionHarness` 注入原子写、外部进程、任务协调、事件广播、操作锁、损坏 ZIP 与损坏 SQLite 共 15 类故障；断言无残留、稳定终态、原始文件不被删除、锁/订阅回收。
- 自动验证：`FaultInjectionTests` 要求至少 15 类故障全部恢复；`scripts/fault-injection-test.ps1` 可独立运行。
- 人工验收：真实磁盘满、权限拒绝、进程强杀或断电场景复核。

## Layer B 最终门禁

- Release 解决方案构建：0 warnings / 0 errors。
- Core：55/55。
- Worker：170/170。
- Playnite：217/217。
- `scripts/validate-source.py`：通过。
- XAML 结构门禁：通过；涉及 XAML 的提交均通过 `render-qa`；B11 之后为测试-only 阶段，未重跑 `render-qa`。
- 每个任务提交后均执行 `scripts/dev-install-run.ps1 -Configuration Release` 真实开发安装；B13 安装后 `playnite.log` 01:28:48 记录插件加载，`worker-launch.log` 01:28:54 记录 `Application started`。

## 结论

Layer B 13/13 已完成并有真实代码、测试、文档与提交证据。Layer C 11 项尚未开始，整个 38 项 Epic 不能标记为全部完成。
