# GameSaveCenter AI/Codex 长期项目记忆

> 维护时间：2026-08-14
> 本文件面向新的 AI/Codex 会话，目标是在几分钟内恢复项目状态，避免重复实现已完成的工作。

## 2026-08-14 UI-REFACTOR-V1（实施包 v1）当前事实

- 本轮是严格受控 WPF UI 重构，不是业务重写。事实来源优先级：当前生产 main > UI Audit（commit `4ab44fe`）> 实施包 v1 锁定/范围 > WPF Demo v6.1 > 旧布局。
- 完整功能保真计划在 `docs/ai/UI_REFACTOR_FIDELITY_PLAN.md`：覆盖 92 条命令、43 个 DataGrid 列、30 个 ScrollViewer、143 个条件 UI；默认禁止 `REMOVE`，只允许 `KEEP/MOVE/RESTYLE/COLLAPSE/RESPONSIVE_MOVE`。
- Dashboard 顶部全局 GamePicker 绝对锁定，必须是 Dashboard 单实例共享控件，在六个工作区永久常驻；首页“今日工作台 / TODAY / 当前游戏”只做布局、间距和响应式修正。
- 已知必须修复的 Audit 症状：SaveCandidateGrid 约 3.7 行、MaintenanceAuditLogGrid 约 1.6～1.9 行、MaintenanceDeviceGrid/ProcessGrid narrow 约 3.7 行、诊断 13 工具 narrow 138 DIP 按钮墙、多处 Page Scroll + DataGrid/List Scroll 嵌套。
- Phase 0 基线：Release 构建 0 警告/0 错误，Core `59/59`、Worker `190/190`、Playnite `238/238`，source/XAML/WPF/render-qa 全绿。后续按 Phase 1～8 分阶段独立提交并 push。
- Phase 1（共享布局基础）已交付：`Redesign.xaml` 新增 `GscInternalTabControl`、`GscInternalTabItem`、`GscToolbarActionRow`、`GscToolbarOverflowButton`，并由 `WpfUiResourceDictionaryTests` 锁定；未改任何 View 页面与业务。
- Phase 2（首页 Overview）已交付：六项 Snapshot 指标改为响应式紧凑 Summary Strip（6/3/2 列），最近 30 天保护明细默认折叠到共享 Expander，全局活动改为稳定四列；`OverviewStatStrip` 响应式列数、保护明细可达性、全局活动四列由新回归测试锁定。GamePicker 与首页锁定结构未改。
- Phase 3（存档中心）已交付：历史/候选窄窗 Inspector 默认收起为“查看详情”按钮，主表高度在 1040×700 分别提升到约 385/254 DIP；候选页头部压成单行；策略模板区默认折叠但全部命令可达。新增窄窗 Inspector 切换回归测试。
- Phase 4a（修改器中心 Trainer）已交付：已绑定工具页窄窗默认收起工具设置 Inspector 为详情按钮，1040×700 工具列表视口 236 DIP；新增 Trainer 窄窗切换回归测试。FLiNG/可下载版本/导入流程未改。
- Phase 4b（媒体中心 Media）已交付：当前媒体窄窗 Inspector 默认收起为详情按钮，来源规则添加表单默认折叠但字段可达；新增 Media 窄窗切换与来源表单折叠回归测试。待归类 DataGrid 与媒体异步缩略图未改。
- Phase 5（任务中心 Task）已交付：游戏筛选在 compact 进入“更多筛选”Expander、wide 回到主行；任务详情 Inspector 窄窗默认收起为详情按钮；操作行保持横向；任务表 1040×700 视口 252 DIP。新增 Task 窄窗切换与更多筛选移动回归测试。
- Phase 6（维护中心 Maintenance）已交付：诊断常用按钮收敛为主行 5 个，低频命令进入共享 Expander；审计日志表视口提升到 280 DIP（约 6 行）；设备/进程/Findings 主表保持 350 DIP。保留策略与全部维护命令未改。
- Phase 7（设置轻量统一）已交付：设置字段标签列宽 token 化为 `GscSettingsFieldColumnWidth`；五个设置分区与保存语义未改。
- Phase 8（最终回归）已交付：Audit HIGH 从 10 清零、MEDIUM 从 4 降到 0，失败路由 0；最终测试基线 Core 59/Worker 190/Playnite 247；真实宿主主题/DPI/连续缩放仍为 MANUAL QA REQUIRED。
- Phase 8 收口：`OverviewView.xaml` 把“当前游戏”卡片 3 个操作按钮底部边距从 8 收到 4 DIP，消除最后一个 Audit MEDIUM（未命名 WrapPanel 92 DIP）；顶部工作台工具栏使用 `Padding="14,10"`，1040×700 下由 91 降到 79 DIP。无 REMOVE，GamePicker 与 Dashboard 锁定区域未改。
- 扩档验证：render-qa 覆盖 10 档逻辑尺寸（1040×700 / 1100×720 / 1280×720 / 1366×768 / 1536×864 / 1600×900 / 1707×960 / 1920×1080 / 2048×1152 / 2560×1440）；UI Audit 新增 2K 与 1100×720 尺寸，快照 161，HIGH/MEDIUM 均 0，运行时警告 39。Audit 工作区高度已改为窗口高度，与生产 Dashboard 和 render-qa 一致。
- 主题 QA：RenderHarness 对 7 个工作区 × 4 尺寸 × Light/Dark 共 56 个离屏场景渲染并校验调色板与视口，全部通过；像素采样确认 Light/Dark 背景确实切换。真实 Playnite 宿主主题仍为 MANUAL QA REQUIRED。

## 当前事实覆盖（2026-08-14 Layer A 收口、Layer B 13 项与 Layer C 11 项）

- `UI-AUDIT-001` 已交付（提交见 `git log -1`）：开发专用 UI 自动审计工具由 `scripts/capture-ui-audit.ps1` / `GameSaveCenter-UI-Audit.cmd` 启动，复用 RenderHarness 渲染真实生产视图；自动扫描 XAML 生成路由/Manifest/保真矩阵，输出视觉树与布局 JSON；页面级滚动容器直接渲染完整内容，DataGrid/ListBox 逐段滚动拼接 `-scroll-*.png`；覆盖 maximized/2k/wide/standard/compact/narrow-1100/narrow，最终 ZIP 在 `artifacts/GameSaveCenter-ui-audit.zip`。后续新增页面只要放入 Dashboard 或 `Views` 目录并保持无参构造，静态盘点与运行时路由会自动纳入。
- 用户日志中的“编译解决方案”失败根因是旧 `dotnet/testhost` 或 Worker 锁住标准 `bin\Release` 输出，随后测试项目无法覆盖 DLL/PDB/XML；不是 `GameSaveCenter.Contracts` 编译失败。
- 一键开发安装器现在默认不请求管理员权限。`scripts/build.ps1`、`scripts/package.ps1` 和 `scripts/dev-install-run.ps1` 支持按运行生成 `artifacts\dev-build\<Configuration>\<guid>` 隔离的 bin/obj、Worker 发布和安装暂存目录，入口修订号为 `DEV-INSTALL-007`。Playnite 发现增加运行中进程、常见目录、卸载信息、App Paths 和 PATH；未发现 Playnite 且没有运行中的 Playnite 时允许继续构建/安装并提示无法自动启动。Playnite 正常退出超时后，仅当进程属于当前会话、可执行文件路径与本次发现结果完全一致且已经没有主窗口时，才结束该无窗口残留；路径不可确认、跨会话或仍有主窗口时继续停止安装。
- 真实宿主已验证：安装报告为 0.6.70 / DLL 0.6.70.0；Playnite `playnite.log` 记录插件加载，插件日志记录 0.6.70.0，`worker-launch.log` 记录存储初始化、过期任务整理和 `Application started`。不要再用 2026-08-12 的 PID 3896 历史日志判断当前安装器行为。
- 当前自动化基线为 Core `59/59`、Worker `190/190`、Playnite `247/247`，Release 构建 0 warnings / 0 errors；source、XAML、WPF 静态门禁与 10 档 `render-qa` 通过；扩档 UI Audit 161 快照、0 HIGH/0 MEDIUM/0 失败路由。真实开发安装已成功，Playnite 与 Worker 启动日志正常。
- `ATOMIC-IO-001` 已交付：新增共享 `AtomicFileWriter`，Worker 设置持久化与媒体复制统一使用“目标同目录临时文件 + 原子 Move”，失败自动清理 `.tmp/.partial` 后再抛出；`WorkerOptions.Persist()` 与 `MediaSyncService` 私有复制逻辑已委托给共享实现。
- `SOAK-001` 已交付：`SoakStabilityHarness` 加速压测任务协调、事件扇出、单游戏锁、原子写入和 SQLite 探针；`TaskEventBroadcaster.SubscriberCount` 与 `GameOperationLock.TrackedGameCount` 提供只读稳定性计数，`scripts/soak-test.ps1` 支持用 `GSC_SOAK_ITERATIONS` 扩展到最多 5000 轮长跑。
- `FAULT-INJECTION-001` 已交付：`FaultInjectionHarness` 注入原子写、外部进程、任务协调、事件广播、操作锁、损坏 ZIP 与损坏 SQLite 共 15 类边界故障，断言无残留、稳定终态、原始文件不被失败注入删除，且锁/订阅全部回收；`scripts/fault-injection-test.ps1` 可独立运行。
- `A-HARDEN-001` 通知级别主体已收口：`NotificationLevel` 持久化默认 `Summary`，`NotificationLevelPolicy` 控制仅重要事件/退出摘要/详细任务；`SessionNotificationAccumulator` 已抽出并覆盖同 Session 单次 final、期望任务数、重复投递等测试。非任务型重要事件（健康风险/冲突/完整性严重）仍由 Dashboard Findings 承载，未单独 toast。
- `A-HARDEN-002` 已交付：未分类 `CustomExecutable` 在普通游戏下按 AutoStart 正常启动；反作弊游戏下必须持久化 `AllowUnknownToolWithAntiCheat` 授权后才允许，Trainer/CT/GameModification 继续禁止；`game_tools` 新增授权列并纳入旧库升级测试。
- `A-HARDEN-003` 已审计收口：首次使用“测试备份”按钮复用真实 `MessageTypes.BackupGame` 生产链路，无独立假服务；无可用测试游戏时显示“可稍后在存档中心手动执行备份”，并有回归测试锁定命令链路。
- `DIAGNOSTICS-001` 已升级：诊断包包含 `system/worker/dependencies/database/recent-tasks/health/settings` JSON、审计与受限日志；`DiagnosticRedactor` 集中脱敏密码、Token、API Key、Authorization、URL query、UNC 凭据、邮箱和用户路径。
- `SAFE-MODE-001` 已升级：Worker 连续 3 次启动失败后请求安全模式，Playnite 询问确认；设置页支持“下次以安全模式启动”，维护中心安全模式提示条提供“恢复正常模式”。
- `INTEGRITY-001` 已补齐：自检覆盖孤儿归档、Manifest 无效/重复路径、磁盘剩余空间和未配置依赖状态；结果使用 `Healthy/Warning/Error/Skipped`，仍只读不自动修复。
- `DB-MIGRATION-001` 已补齐：两代旧库 Fixture 覆盖策略、模板、会话、设备决策、GameTool 与备份历史，并使用 `ReadScalar` 验证真实数据值而非仅检查表存在。
- `METADATA-BACKUP-001` 已补齐恢复流程：预览校验 manifest/哈希/路径越界，确认后备份当前元数据、原子替换数据库与设置、完整性校验并在失败时回滚；维护中心提供“恢复元数据灾备”入口。
- `REPOSITORY-REBUILD-001` 已补齐：只读扫描预览统计已确认/未归属/部分缺失/损坏归档，执行重建必须用户确认，未确认不写库。
- `PATH-REMAP-001` 已补齐：只读预览按类型列出受影响路径和目标存在状态；目标缺失默认跳过，可显式授权仍应用；执行前自动创建元数据灾备。
- `TASK-RECONCILE-001` 已补齐：任务持久化 `WorkerSessionId`，启动协调只处理旧 Worker 会话遗留任务；Backup/Media/Cloud 标记可重试中断，Integrity 标记普通中断，Restore 标记人工介入且不自动重试。
- `GAME-OP-LOCK-001` 已补齐：`GameOperationKind` 与显式兼容矩阵写入代码，备份/恢复/媒体/云端使用类型化锁；Restore 不与其他操作并发，同游戏双 Backup 禁止。
- `IPC-COMPAT-001` 已补齐：握手返回 `AppVersion` 与能力列表，协议版本独立于应用版本，能力包括 RestoreReadiness/MetadataBackup/RepositoryRebuild/PathRemap/TaskReconcile/GameOperationLock/AtomicIo。
- `ATOMIC-IO-001` 已审计补齐：共享原子写入覆盖设置/媒体/元数据恢复/启动失败计数，取消写入或替换失败时旧文件保持完整且无残留。
- `SOAK-001` 已补齐：DataScale Soak 默认小规模、`GSC_SOAK_DATA_SCALE=1` 全量规模；监控 Managed Memory/句柄/线程/订阅/临时文件并断言有界增长。
- `STORAGE-001` 已交付：维护中心“保留策略”页新增只读备份存储分析卡；显示卷剩余/总容量、目录实测与索引体积、版本数、7/30/90 天增长趋势、Top 5 游戏占用排行，并给出标注“估算”的简单容量耗尽预测；新增 IPC `storage.analysis`、Worker 服务与取消支持。
- `RETENTION-SIM-001` 已交付：维护中心“保留策略”页新增全局保留策略模拟器；按每游戏策略复用 `RetentionPlanner` 计算现有/保留/候选清理/预计释放、用户锁定/健康保护/PreRestore 计数与候选明细；`retention.simulation.apply` 要求二次确认，只删除备份根目录下的 ZIP 候选并同步移除 SQLite 索引，锁定/PreRestore/健康恢复点永不进入候选。
- `LOCAL-MIRROR-001` 已交付：设置页新增“启用第二本地镜像”与镜像目录；维护中心“保留策略”页新增镜像状态与“同步镜像”入口。Worker `LocalMirrorService` 只复制和按大小校验，绝不删除镜像中多余文件；外置硬盘未连接时状态为 `Unavailable` 而不是系统错误；同步完成后写入镜像标记文件。
- `ACTIVITY-001` 已交付：首页新增“全局活动”时间线，由 `ActivityTimelineMapper` 把最近 100 条审计记录映射为备份/恢复/云端/媒体/工具/健康/冲突/完整性/仓库修复等业务事件；只展示时间、游戏、分类、结果与摘要，不暴露原始日志，UI 最多显示 12 条并保持有限视口与虚拟化。
- `PLAYNITE-QUICK-001` 已交付：`GetGameMenuItems` 为游戏右键菜单提供“立即备份 / 查看备份历史 / 验证最新恢复点 / 游戏工具 / 打开设置”五个快捷操作，全部绑定当前所选游戏 ID，并复用 Worker 生产 IPC 链路。
- `DRAGDROP-001` 已交付：修改器中心支持单文件/目录拖拽导入，`.ct` 自动按 CheatTable，`.lnk/.bat/.cmd/.ps1` 按自定义启动项，`.exe` 弹出“修改器/普通启动项”二选一，`.zip`/目录进入既有主程序选择流程；未选择游戏时拒绝导入并提示。
- `UI-STATE-001` 已交付：设置持久化上次 Workspace、任务状态/游戏/类型筛选、任务搜索、媒体筛选与媒体搜索；VM 启动时恢复，变更经 500ms 防抖保存；运行中游戏优先与上次选择恢复继续复用既有 GamePicker 持久化，不保存 Loading/Busy/Error 等瞬态。
- `ACCESSIBILITY-001` 已交付：`Ctrl+F` 按当前 Workspace 聚焦游戏/任务/媒体/FLiNG/进程映射搜索框并全选；任务、媒体、FLiNG 与游戏搜索框补充 `AutomationProperties.Name`；共享 `GscSharedFocusVisual` 与高对比度降级继续生效。
- `UI-STATES-001` 已交付：新增共享 `WorkspaceStatePresenter`，统一 Loading/Empty/Error/Degraded/Offline/Disabled 六种状态的图标、标题、说明与可选重试按钮；Overview 全局活动与 Task 空状态已接入共享控件，其余页面继续复用 `GscEmptyStateText`。
- `SETTINGS-VALIDATION-001` 已交付：设置页在标题区显示即时验证摘要，文本框、下拉框与复选框变化时复用 `VerifySettings` 校验并内联展示最多 4 条错误；验证错误不再只等 Playnite 保存时出现。
- `MAINTENANCE-REPORT-001` 已交付：新增 IPC `maintenance.report.get` 与 Worker `MaintenanceReportService`，从 SQLite 计数、完整性自检、存储分析与本地镜像状态聚合用户可读健康报告；维护中心诊断操作带新增“复制健康报告/导出健康报告”，支持 TXT/Markdown；报告不含日志、原始数据库或凭据，与开发者诊断 ZIP 明确区分。
- 最终代码缺口已闭合：`RepositoryRebuildService` 现在可从空/新 SQLite 按磁盘 ZIP 与 Manifest 重建历史，按 Ludusavi 目录名创建 `recovered-*` 占位游戏，不猜 Parent，二次重建幂等；`MetadataBackupService` 灾备包新增 `settings/plugin-settings.json`，恢复后由 Playnite 侧导入插件设置并回滚；`WorkspaceStatePresenter` 已覆盖存档历史 Loading、修改器工具 Loading/Empty、媒体 Worker Offline、维护云端 Degraded；`LocalMirrorService` 同步改为 SHA256 内容校验，同大小但内容不同会重新复制。
- 崩溃修复：`GscWorkspaceStatePresenter` 模板内重试按钮从普通 `Button` 改为 `ui:Button`，修复真实 Playnite 切换存档页时 `“Button”TargetType 与元素“Button”的类型不匹配` 的 XamlParseException；已增加源码回归断言并在真实宿主复测。
- Metadata 原子回滚：恢复前用 `VACUUM INTO` 生成一致性 DB 快照（不再直接复制可能缺 WAL 的活库）；Worker 新增 `metadata.restore.rollback`，Playnite 侧新增 `MetadataRestoreCoordinator`，Plugin 设置导入/保存/应用任一步失败时先恢复旧插件设置，再调用 Worker 从 PreRestorePath 回滚 DB 与 Worker 设置，失败才进入人工介入。
- 本轮已修复 Layer A 审计缺口：多设备只有 Manifest 内容指纹相同才可判定等价；仅文件数/总大小相同改为保守的未知分歧；Restore Readiness 使用可取消的流式解压与增量 Hash；环境检查分别验证数据、存档和媒体所在磁盘；Manifest 重复路径不会抛异常或产生强指纹。
- `DIAGNOSTICS-001` 已完成：维护中心可导出有上限、只读、脱敏的 ZIP 诊断包；包含环境/任务/审计/Worker 日志摘要，不包含数据库、存档、媒体或凭据；新增 IPC 请求和 Worker 测试覆盖敏感字段与大小边界。
- Layer A 14 项、本轮审计补缺、A-HARDEN-001/002/003、Layer B 13 项（DIAGNOSTICS/SAFE-MODE/INTEGRITY/DB-MIGRATION/METADATA-BACKUP/REPOSITORY-REBUILD/PATH-REMAP/TASK-RECONCILE/GAME-OP-LOCK/IPC-COMPAT/ATOMIC-IO/SOAK/FAULT-INJECTION）与 Layer C 11 项已交付；逐项验收见 `docs/ai/PRODUCT_HARDENING_LAYER_B_AUDIT.md` 与 `docs/ai/PRODUCT_HARDENING_LAYER_C_AUDIT.md`，最终逐项审计见 `docs/ai/PRODUCT_HARDENING_EPIC_FINAL_AUDIT.md`，人工验收清单见 `docs/ai/FINAL_MANUAL_QA_CHECKLIST.md`。由于真实场景人工验收未全部完成，整体 Epic 状态为 `PARTIALLY COMPLETED / MANUAL QA REQUIRED`，不能宣称全部任务完成。
- 通知级别已收口：`ImportantOnly` 只显示失败/取消任务与警告/失败摘要，`Summary` 保持一次退出摘要，`Verbose` 在最终摘要外逐任务显示；设置页新增通知级别选择，旧设置缺省归一为 `Summary`。
- 安全模式已交付：全局开关持久化到插件与 Worker 设置；开启后暂停自动退出/定时备份、自动媒体同步、自动工具启动、会话存档快照与保护提示、云端自动上传与自动重试，手动操作和恢复仍可用。维护中心诊断页与诊断摘要会显示当前状态。
- 完整性自检已交付：维护中心“完整性自检”通过 IPC 检查 SQLite 完整性/外键/表结构、目录可写性、配置程序存在性和索引文件引用；只报告不修复，数据库问题为 Critical，文件缺失为 Warning。
- 数据库迁移 Harness 已交付：`DatabaseMigrationHarness` 在临时目录创建旧版 Fixture 后执行当前 `SqliteStateStore.InitializeAsync`，覆盖旧库升级、全新库创建、重复初始化和失败报告；只操作临时数据库，不触碰用户数据。
- 元数据灾备已交付：维护中心“导出元数据灾备”生成 SQLite `VACUUM INTO` 一致性快照、脱敏 Worker 设置和版本清单 ZIP；不包含存档、媒体或凭据，超过 512 MiB 安全上限时失败并清理。
- 备份索引重建已交付：维护中心“重建备份索引”按 Ludusavi 磁盘列表重建 SQLite 版本索引，单游戏失败不中断，只读归档并保留失败游戏原索引。
- 批量路径迁移已交付：维护中心“批量路径迁移”按旧根/新根前缀批量改写 SQLite 与 Worker 设置中的已索引路径；只改引用不移动文件，服务端强制确认。
- 中断任务协调已交付：维护中心“协调中断任务”把 Worker 重启遗留的排队/运行中任务幂等标记为 `WORKER_RESTARTED`，启动时仍自动执行同一逻辑。
- 单游戏操作锁已交付：同一游戏的备份、云端重试、媒体同步、恢复预览/执行互斥，超时返回 `GAME_OPERATION_BUSY`；不同游戏并行不受影响。
- IPC handshake 已交付：`system.handshake` 返回协议版本、最低支持版本与 Worker 版本；客户端握手不兼容即拒绝，旧 Worker 回退 Ping 探测。
- Restore 在实际写入开始后的失败、异常或后校验失败必须尝试恢复锁定的 PreRestore；回滚本身失败才进入 `ManualInterventionRequired`。灾难演练现覆盖 A/B/Undo、部分写入、写后异常、权限、只读、目录缺失和回滚失败。
- 多设备云目录使用持久化 32 位不透明 `DeviceId`，机器名只用于显示与旧 sidecar 兼容；便携设置导入不得复制设备身份。远端恢复继续要求隔离下载、Rclone check、Ludusavi 版本确认和既有 PreRestore 恢复链。
- 每游戏策略新增 `BackupAnomalyProtectionLevel`（Off/Normal/Strict）；重要游戏模板默认 Strict。Manifest 大量删除参与异常检测，最后健康恢复点与用户 Lock 都不能成为 retention 候选。
- Rclone 每次执行都经过命令白名单 `copy/check/lsf/cat/version`，禁止 `sync/move/delete/purge`；外部进程日志不再记录完整参数。Worker 重启会把未完成任务转为 `WORKER_RESTARTED`，取消会终止子进程。
- 真实 Rclone 断网、真实两台设备、真实游戏 Restore/Undo、真实 EXE/LNK/BAT/PS1、1000+ 游戏库和完整主题/DPI 连续缩放仍为 `MANUAL QA REQUIRED`，不得由自动化结果冒充。

## UI-QA-REAL-006 设置分类 Tab 实际裁切修复（2026-08-13）

- 上一轮仅在 `TabPanel` 外增加底部留白没有解决用户截图中的直线底边。实际根因是 `GscRedesignSettingsTabItem` 让圆角 Border 直接充满 `TabItem` 模板布局槽，并开启 `ClipToBounds=True`；`TabPanel`/宿主布局取整后会把 Chrome 的底部圆角贴槽裁平。
- 当前共享模板使用不裁切的 `TabItemRoot` 包裹独立 Chrome；Chrome `VerticalAlignment=Top`、`Margin=0,0,0,2`，因此始终保留底部安全距离并移除 Chrome 的 `ClipToBounds=True`。
- 分类滚动内容使用真实 `SettingsHeaderBottomSafetyZone` 元素放在 `TabPanel` 后面形成内容 extent；顶部横向模式折叠该元素。RenderHarness 同时检查最后一项 `TabItem`、Chrome 的底部位置和 `chromeSafety >= 1`。
- 当前验证：5 种窗口渲染图通过，设置几何探针和 Playnite `210/210` 通过；真实 Playnite 主机的 DPI/主题/连续缩放依旧只能由人工验收确认。

## 2026-08-13 UI-QA-REAL-005 首页顶端对齐、当前游戏空间与设置圆角回归

- 首页宽屏 `OverviewSecondaryScrollViewer` 与其内容面显式使用 `VerticalAlignment/VerticalContentAlignment=Top`，并在响应式代码中重复设定，避免 Playnite 宿主模板刷新后“今日概览”落到工作区中部。
- 首页 Hero/当前游戏宽屏列由 `1.25* + 0.75*` 调整为 `1.1* + 0.9*`；离屏报告中的当前游戏/Hero 宽度比约 `0.82`，原约 `0.60`，没有改变 Hero/当前游戏的堆叠断点、命令或绑定。
- 设置共享分类栏模板在 `TabPanel` 外增加命名的底部安全 host，并设置顶部内容对齐、像素对齐和布局取整；滚动到末端时最后一个分类的底部仍落在 viewport 内，避免圆角被横向直线裁掉。
- RenderHarness 现在在截图前显式解除设置页入口动画的 `Opacity=0`，并检查 Overview 右栏 top delta、当前游戏宽度比和 Settings 最后一张 Tab 的底部几何，避免“空白 PNG/只测到布局没有测到可见性”。
- 验证：`python scripts/validate-source.py`、WPF 静态门禁、`git diff --check`、五种窗口尺寸 `render-qa` 全绿；Core `42/42`、Worker `117/117`、Playnite `210/210` 通过。真实 Playnite 主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## AI/Codex 启动协议

开始 GameSaveCenter 开发前，请依次阅读：

1. `docs/ai/PROJECT_MEMORY.md`（本文件）
2. `docs/ai/WORKLOG.md`
3. `docs/DEVELOPMENT_HANDOFF.md`
4. `docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md`（如果存在）
5. `docs/design/UI_CHANGE_GATE.md`（如果存在）
6. `git log` 最近 15～30 个 commit
7. `git status`

然后才开始修改代码。不要仅凭历史对话假设当前项目状态；代码、文档和 Git 历史是唯一事实来源。

## 项目定位

- GameSaveCenter 是 Playnite 的 GenericPlugin，提供存档备份/恢复/校验、媒体同步、任务中心、维护中心、修改器与 CT 管理，以及新增的自定义游戏启动项能力。
- Playnite 是唯一主要 UI（WPF），后台 Worker 是独立 .NET 8 进程，两者通过 Named Pipe IPC 通信。
- `GameSaveCenter.Contracts`：Playnite/Worker 共享的 DTO、枚举、消息类型，netstandard2.0。
- `GameSaveCenter.Core`：Playnite 侧可复用逻辑（目前主要是启动/包装与少量辅助）。
- `GameSaveCenter.Worker`：持久化、Ludusavi、Rclone、媒体索引、任务编排、游戏 Session、GameTool 导入/启动/追踪。
- `GameSaveCenter.Playnite`：WPF Dashboard 外壳 + 六个 Workspace 页面 + 设置页。
- 数据持久化：SQLite（`SqliteStateStore`）+ 文件系统（存档、媒体归档、GameTools 目录）。
- 模块关系：Ludusavi 负责存档底层；Rclone 只允许 copy/check，不使用 sync/delete/purge；媒体为增量同步；GameTool 绑定在游戏级。

## 当前主要架构

### 程序集与入口
- Solution：`GameSaveCenter.sln`，版本 `0.6.70-development-preview`（`Directory.Build.props` 0.6.70）。
- 插件入口：`src/GameSaveCenter.Playnite/GameSaveCenterPlugin.cs`，扩展 ID `66e9f2d7-67bb-43ef-b62a-b8e60734fcec`。
- Worker 入口：`src/GameSaveCenter.Worker`，IPC dispatcher 为 `IpcRequestDispatcher`。
- 测试：Core 42、Worker 117、Playnite 203（2026-08-13 当前基线；优先使用 `scripts/build.ps1 -OutputRoot <目录>`，避免本机旧 Worker/测试宿主锁住标准输出）。
- ONBOARDING-001（2026-08-13）新增 `environment.check`：检查服务驻留 Worker，使用临时 SQLite 表和目录临时文件做可逆探针；Rclone 未配置为 `Skipped`，不把可选云端能力误计为基础失败。当前基线为 Worker 70、Playnite 198；UI 仍复用 Maintenance 诊断页的单一外层滚动与有限表格视口。
- GAME-TOOL-003/004（2026-08-13）新增 `GameToolIfAlreadyRunning` 与 `GameToolRiskCategory` 持久化列。CustomExecutable 的已有实例策略只允许按解析后的 EXE 完整路径匹配；Skip 为默认，Restart 只重启再次确认过的同路径 PID，路径读取不完整时必须保守停止。反作弊游戏仅允许已分类为 `GeneralUtility` 的自定义工具自动启动；Unknown 与 `GameModification` 自动启动必须阻止并写审计，用户需在 TrainerCenter Inspector 明确分类后保存。
- SMART-PROTECT-001/002（2026-08-13）：完整游戏停止请求等待存档识别并以持久化提示状态驱动三选一保护提示；只在识别到候选/匹配存档时提示，未识别时写审计并等待后续识别。`Deferred` 有 7 天冷却，`Enabled`/`Dismissed` 不再弹出；停止 IPC 使用 3 分钟专用超时。Overview 最近游戏列表显示已保护、未匹配、存档未保护和风险，已保护项不可选，其余项可批量启用游戏中/退出后推荐保护并写审计。不要新增主导航页或绕过既有恢复安全边界。
- NOTIFY-001 / MULTI-DEVICE-001 / RCLONE-RELIABILITY-001（2026-08-13）：退出备份与媒体任务使用同一 SessionId，Playnite 依据 Task Center 的终态任务聚合为一条退出摘要；本地备份成功但云端失败时必须同时显示本地成功和云端可重试失败。设备摘要携带 `ParentBackupId`，同一父版本分叉只标记冲突并要求人工决策，禁止自动合并/覆盖/删除；下载远端仍必须进入隔离 staging、校验、归档检查后才能走既有安全恢复链。Rclone 仅允许 copy/check/lsf/cat/version；网络或不完整传输有限重试，凭据/权限/远端不存在明确失败并停止自动重试。

### Dashboard / Workspace
- `DashboardViewModel` 是大型聚合 ViewModel（技术债，暂不拆分），持有所有 Workspace 数据与命令。
- 六个 Workspace：Overview（首页）、Saves（存档中心）、Trainers（修改器中心）、Media（媒体中心）、Tasks（任务中心）、Maintenance（维护中心）；另有 Settings 页面。
- 工作区页面位于 `Views/`：DashboardView + 各 CenterView；共享资源在 `Themes/DesignTokens.xaml`、`Themes/WpfUiProduction.xaml`、`Themes/Redesign.xaml`。
- Dashboard 视图有响应式 code-behind 协调（`DashboardView.xaml.cs`），页面级滚动面 + 主表/主列表有限视口 + 内部虚拟化滚动。

### UI-207 当前约束（2026-08-12）

- Settings 的 `SettingsScroller` 位于共享 `GscRedesignSettingsTabControl` 模板内容区；`SettingsHeaderScroller` 是分类导航区。宽屏分类栏为 232 DIP 左侧有限滚动，紧凑布局为顶部横向 `Auto`，不能把根 UserControl 再包回第二个页面滚动器。
- `GscSelectedGameIconControl` 只用于当前游戏上下文表面（Dashboard、Overview、Save、Trainer、Media），GamePicker 虚拟化列表不得加载真实 Icon。
- GamePicker 选择可被当前筛选隐藏但不能静默丢失；必须保留 `SelectedItem`、显示恢复语义并保持 `GamePickerSelectedGameId` 持久化。默认筛选只对新用户/未知值归一为“已安装”。
- 事件驱动的 `PlayniteGameStarted` 自动定位优先于普通刷新；游戏停止不改变当前选择。不得为此新增轮询、进程扫描、IPC 或网络请求，也不得改动 DataGrid 滚动/虚拟化契约。
- 当前自动化结果：本阶段 Worker 相关 Release 构建 0 警告/0 错误，Worker 67/67 通过；上一阶段 Core 27/27、Playnite 197/197、render-qa 通过。真实 Playnite 宿主/DPI/主题人工验证仍待环境。

### 数据流
- Playnite → Worker：Named Pipe 请求（`GameSaveCenter.Playnite/Ipc`、`GameSaveCenter.Worker/Ipc`）。
- 任务状态：Worker `TaskCoordinator` 持久化 + `TaskEventBroadcaster` 事件流 + Dashboard 轮询兜底。
- 快照：`MessageTypes.GetDashboard` 返回 `DashboardSnapshotDto`；大库先渲染 SQLite 缓存，后台再同步。

### GameTool 模型
- `GameToolType`：Trainer / CheatTable / CustomExecutable（自定义启动项）。
- `GameToolDto` + `GameToolVersionDto`：DisplayName、Enabled、AutoStart、LaunchTiming、LaunchDelaySeconds、CloseOnGameExit、RequiresAdmin、ActiveVersionId、EntryPath、WorkingDirectory、Arguments、ResolvedTargetPath 等；`game_tool_versions` 已补 `resolved_target_path` 兼容列。
- Worker `GameToolService`：导入（Trainer/CT 复制进 GameTools 目录；自定义启动项默认保留外部路径引用）、更新、删除、启动、随游戏自动启动/延迟/关闭追踪。
- Session 追踪：`GameToolSessionTracker`（SessionId → PID + 实际 StartTime + CloseOnExit），关闭时要求 PID 与实际 StartTime 双向匹配，禁止按进程名杀。

### 任务系统
- `TaskCoordinator` 统一编排；`TaskStatusDto` 有 Progress/Message/ErrorCode/ErrorMessage/State/时间戳。
- Dashboard `TaskIndexedCollection` 按 TaskId 索引增量合并；`knownTaskStates` 去重通知。

### 媒体系统
- `MediaItemDto` 由 Worker 索引；列表与详情预览已改为 `AsyncThumbnailImage` 异步加载（`Task.Run` 强制后台、3 并发、LRU 96、Freeze 后回 UI、Unloaded 取消）；`MediaThumbnailConverter` 保留为兼容实现。
- Media 列表使用 ListBox + Recycling 虚拟化；页面滚动面与列表滚动分工明确。

### 缓存与性能机制
- `BatchObservableCollection<T>`：批量 Replace 只发一次 Reset（默认引用相等比较；PERF-005 起支持内容比较器跳过未变化）。
- GamePicker 有 180ms 搜索防抖、按 PlayniteId 缓存 `GamePickerItem`、平台指纹短路。
- Task 筛选指纹短路（`ComputeTaskFilterFingerprint`）、平台指纹短路（`ComputePlatformFingerprint`）。
- Dashboard 大库 cache-first + 延迟后台同步；`[PERF]` 日志设施见 `docs/ai/PERFORMANCE_BASELINE.md`。

## UI 设计原则

- 目标是 Apple-inspired 的原生 WPF 桌面工具：清晰层级、克制毛玻璃、圆角、统一设计令牌、自然微动效、深浅色、跟随 Playnite、高对比度、DPI 适配、响应式布局、不使用突兀的原生控件视觉。
- 所有 UI 修改必须先读 `docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md` 与 `docs/design/UI_CHANGE_GATE.md`，并遵循 `.codex/skills/wpf-apple-desktop-ui/SKILL.md`。
- 常用窗口下限 1040×700 DIP；1080p/2K/4K 必须按 DPI 换算后的逻辑 DIP 检查全屏、窗口化、最大化；不把 4K 通过当作 1080p 通过。
- 页面级滚动只承载有限测量内容；DataGrid/ListBox 保留 236 DIP 最小视口、内部滚动和虚拟化；堆叠 Inspector 下限 160 DIP。
- 动态下拉框必须显示逻辑默认值（如“全部”）；TaskCenter 游戏/类型筛选通过 `TaskFilterOptionsSync` 增量同步，`全部` 稳定保留在 index 0，不再 Clear/Replace 集合。
- GamePicker 新用户默认筛选为“已安装”，已有明确配置值必须保留；Dashboard 打开时运行中游戏优先，否则恢复上次选择，普通刷新不得抢回用户手动选择。

## 已完成的大型重构 / 优化

- UI-001～UI-205、SKILL-001、QA-001～005：页面 Workspace 化、响应式断点、滚动分工、Inspector 下限、筛选默认值、离屏渲染 QA。
- UI-207（2026-08-12）：设置页 Header 不裁剪与分类栏滚动（920 DIP 断点）、运行中游戏自动定位、上次选择持久化复用、GamePicker 新用户默认“已安装”、当前游戏真实 Playnite Icon（事件驱动，无轮询/无网络，LRU 48）。
- `scripts/render-qa.ps1` + `tests/GameSaveCenter.RenderHarness`：7 页面 × 5 常用窗口离屏渲染回归，含自动失败门禁。
- PERF-001：`BatchObservableCollection` 批量 Reset。
- PERF-002/003：Task 筛选与 GamePicker 平台指纹短路。
- PERF-004（旧编号）：GamePickerItem 缓存复用（新任务编号体系中 PERF-004 是性能基线设施，不要混淆）。
- PERF-004/005/006（新编号）：`[PERF]` 基线日志、Snapshot 无变化 0 Reset、Task/Media 搜索防抖。
- PERF-007：媒体缩略图异步化（`AsyncThumbnailLoader` Task.Run 后台解码 + 3 并发 + LRU + Freeze + `[PERF]` 埋点，`AsyncThumbnailImage` 占位加载并 Unloaded 取消）。
- PERF-009/010：任务事件合并 TaskId 索引 O(1) 更新；命令状态刷新 Dispatcher 合帧。
- GAME-TOOL-001/002：自定义启动项正式支持 EXE/LNK/BAT/CMD/PS1，外部路径引用不复制文件；Session 级 PID 追踪与 CloseOnGameExit 安全关闭。
- UI-204/205：TaskCenter 与 GamePicker 下拉框默认值恢复（含真实 Playnite 异步物化重试）。
- UI-206（含回滚）：DataGrid 滚动几何修复。初版 `Pixel ScrollUnit` 经真实 Playnite A/B 验证会严重恶化空白，已撤回；最终采用 `Item` + `GscStableDataGridRow` 稳定行样式 + geometry probe（60 行 × 非整行高度，gap ≤4 DIP、末行完整、无跳变、Recycling 保持）；诊断摘要取消外层裁剪并由页面滚动负责可达性。

## 当前技术债

- `DashboardViewModel` 仍很大，包含命令、筛选、导入、诊断、设备状态等职责；只有性能实现被严重阻碍或 GAME-TOOL 无法接入时才拆（独立 `ARCH-xxx` 任务）。
- `DashboardView.xaml.cs` 仍承担部分响应式协调。
- 媒体列表/详情缩略图已异步化；真实大量截图滚动下的帧率仍需真机验证。
- 真实 Playnite 宿主、主题切换、DPI 真机、连续缩放流畅性尚未完整人工验收（UI-QA-REAL-001 仅完成冒烟）。

## 当前开发优先级

- P0：性能基础设施与真实热点优化（PERF-004～007、009/010 已完成）。
- P0：自定义游戏启动项（已完成，GAME-TOOL-001/002）。
- P1：媒体性能（PERF-007 异步缩略图，已完成）。
- P1：真实 Playnite / DPI / 大型游戏库 QA（UI-QA-REAL-001 冒烟已完成，完整人工验收待用户）。
- P2：架构进一步拆分（不主动做）。
- PERF-008：已评估收口，维持现状。详情已按激活 Workspace 分支加载，全量快照仅用于全局摘要且后台有 1 分钟 TTL；2000 规模合成 profiling 无 O(n^2)，待真实大库渲染 profiling 证明瓶颈后再评估。

## 2026-08-12 可靠性阶段补充

- `RELIABILITY-RESTORE-001` 已实现：备份历史版本支持非破坏性的恢复可用性检查，结果持久化在 `backup_versions.restore_readiness_json`，检查过程只在应用数据目录隔离提取，不接触真实存档目录。
- `d45f65c` 已补齐恢复校验安全闭环：Manifest 非法、重复/越界路径、Manifest 缺失文件现在不能得到 `Ready`；逐文件路径集合、大小、可用 Hash 和提取结果均纳入判定，验证目录创建失败返回 `Failed`，取消仍由调用方观察。
- `d45f65c` 为恢复编排增加窄接口测试边界，并用临时 SQLite + 内存假 Ludusavi 完成 A→PreRestore→B→失败回滚、成功恢复→Undo、运行中拒绝恢复等灾难演练；未启动 Playnite、未调用真实 Ludusavi、未接触真实存档。
- Ludusavi 备份版本的 `backupPath + backup ID` 已持久化为 `backup_versions.archive_path`；Simple 归档、缺失/损坏 ZIP、路径穿越、超大展开量、不一致统计与不支持压缩方式必须返回明确状态。
- 恢复可用性入口位于现有 Save Center 历史 Inspector，不能新建页面或改变 `SaveHistoryGrid` 的滚动/虚拟化骨架；新增内容必须留在 `SaveHistoryActionsScrollViewer` 内，并继续通过 `render-qa` 验证 1040×700 等窗口。
- 初始实现的历史基线为 Core 13、Worker 58、Playnite 197；当前阶段增量基线为 Worker 67/67，生产 Worker Release 构建 0 警告/0 错误。真实 Playnite 宿主、主题/DPI 人工验收仍待用户环境确认。
- 该恢复可用性阶段的下一项已在后续 `HEALTH-001` 完成；历史记录保留原阶段编号，当前开发顺序见下方 HEALTH-001 补充。

## 2026-08-12 HEALTH-001 阶段补充

- 每游戏健康状态已统一为四态：`Healthy`（健康）、`Attention`（注意）、`Risk`（风险）、`Unknown`（未知）。旧 `Ready / Warning / LudusaviUnavailable` 仅作为 UI/历史缓存兼容输入保留；新 Dashboard 快照输出四态。
- `GameHealthAssessmentService` 是 Core 纯计算服务，证据包括最近游玩、备份版本/时间、最近 30 天失败任务数、最近任务状态、最新 `RestoreReadinessStatus`、未解决 finding 严重度、按游戏策略启用的云端状态；不做磁盘、ZIP、网络或数据库访问。
- Worker 的 `GetDashboardGameRecordsAsync` 一次聚合最新备份可用性、任务失败、finding 和媒体/策略数据；`DashboardService` 只在内存中计算四态和理由，并把 `WarningGames = AttentionGames + RiskGames`，`UnknownGames` 不误计入需处理数。
- UI 改动只复用首页统计卡、Dashboard 游戏列表/选中头部和 Save Center 校验区；四态在有限宽度下不新增列或固定宽度，理由使用已有 Tooltip，旧 `Ready` 夹具继续显示绿色。Snapshot comparer 已比较健康摘要与理由列表。
- 当前测试基线为 Core 19、Worker 59、Playnite 197；源码门禁、XAML 门禁、WPF 静态门禁、隔离 Release 构建和 render-qa 已通过。真实 Playnite 宿主、主题/DPI 人工验收仍待用户环境确认。
- 当前已完成 Restore Readiness、Health、Protection 三项；下一项按附件顺序为 `POLICY-001`，不要重做上述功能，不新增主页面，继续采用小阶段、独立 commit、文档和 push。

## 2026-08-12 PROTECTION-001 阶段补充

- `RecentProtectionAssessmentService` 已在 Core 实现为无副作用纯计算：以 `GameStatusDto.LastPlayedUtc` 过滤最近 7/30/90 天，按未识别存档、从未备份、恢复点不可用、自动保护关闭、云同步异常、游玩后备份过旧和备份健康异常分类；每个游戏只显示一条最高优先级原因。
- `GameStatusDto` 现在带有 `LatestRestoreReadinessStatus`，由 Worker Dashboard 从已有聚合记录投影；Playnite 不增加 IPC、扫描或数据库查询，Overview 只从现有快照计算摘要。
- UI 复用现有 Overview 风险滚动面与 Settings 自动化分类。保护摘要最多展示 6 条；选择条目只改变当前游戏选择并提示用户确认，绝不因筛选/选择自动备份或恢复；没有新增页面，也没有修改 DataGrid、虚拟化或滚动骨架。
- 最近保护窗口设置默认 30 天，接受 7/30/90，便携设置导入会校验非法值，旧 JSON 缺少字段时保持默认值。
- 本阶段验证基线为 Core 27、Worker 59、Playnite 197；Worker/Playnite Release 构建、源码/XAML/WPF 门禁和 render-qa 均通过。真实 Playnite 主题/DPI/键盘/连续缩放验收仍待用户环境。
- 下一项按附件顺序为 `POLICY-001`；不要重做 `HEALTH-001` 或本阶段保护摘要。

## 2026-08-12 POLICY-001 阶段补充

- 策略模板复用 `BackupPolicyDto`，内置模板 ID 固定为 `default`、`important`、`high-frequency`、`exit-only`、`manual-only`；用户模板 ID 必须以 `custom-` 开头。模板应用是一次性复制，不建立继承关系。
- `BackupPolicyTemplateCatalog.ClonePolicy` 是模板的安全边界：周期间隔限制在 1–1440 分钟，保留值不小于 0，所有模板都强制关闭自动恢复。内置模板由 Worker 初始化幂等播种，禁止通过 IPC 修改/删除。
- Save Center 的模板区位于既有策略页滚动内容内，未新增页面、未改变 DataGrid/虚拟化骨架；创建副本时先保存当前选择再清空选择，避免名称丢失。
- Playnite 包必须同时包含 `GameSaveCenter.Core.dll` 与 Worker 的 self-contained Windows runtime；`scripts/package.ps1` 会验证 Core、hostfxr/hostpolicy/coreclr/System.Private.CoreLib 和 `includedFrameworks`，Worker 项目保持 `RuntimeIdentifiers=win-x64`，发布使用单节点/无 node reuse 参数。
- 当前自动验证：Core 29/29、Worker 69/69、Playnite 197/197；Worker/Playnite Release 隔离构建 0 警告/0 错误；source/XAML/WPF 门禁与 render-qa 通过；最终 `.pext` 打包成功。真实 Playnite 日志曾确认插件加载，但旧 Worker PID 3896 仍锁住用户安装目录，完整 Worker/IPC/UI 仍标记为 `MANUAL QA REQUIRED`，不能以隔离首启未进入扩展阶段冒充真实宿主通过。
- 以后每个代码阶段的验收顺序固定为：`dotnet test/build` → 源码/XAML/WPF/render-qa → `scripts/package.ps1` → 安装包内容断言 → 启动 Playnite 并检查 `ExtensionFactory`/扩展日志；若宿主被单实例或权限环境阻断，必须记录为人工验收，不得宣称加载成功。
- 本阶段完成后的下一项为 `ONBOARDING-001`；不要重做 Restore Readiness、Health、Protection 或本阶段策略模板。

## 一键安装器进程停止与权限约束

- `DEV-INSTALL-007` 允许可信 Playnite 候选为空，避免 PowerShell 将空数组绑定到停止函数时直接失败。没有运行中的 Playnite 时，安装器仍使用 `%APPDATA%\Playnite\Extensions`（或显式 `-PlayniteExtensionsPath`）完成安装；以后需要自动启动时应通过 `-PlayniteExecutable` 指定便携版/自定义目录中的 `Playnite.DesktopApp.exe`。
- `scripts/dev-install-run.ps1` 的 `Stop-PlayniteAndOwnedWorkerReliably` 必须先允许 Playnite 正常退出并等待插件回收 Worker，再处理残留；不能把 `Get-Process` 与停止之间的退出竞态误报为失败。
- 安装器不应默认请求管理员权限，也不应按进程名广泛终止 Worker。`DEV-INSTALL-004` 先调用 Playnite 的正常窗口关闭，让插件既有 `OnApplicationStopped`/`WorkerLauncher.StopOwnedWorker()` 回收自己创建的 Worker；只有 Playnite 已退出后仍存在、且路径明确属于当前扩展目录的残留 Worker 才可处理。
- 路径不可读取或残留 Worker 属于其他扩展时必须停止安装并要求用户手动处理，不能为了自动化验证提权或误杀其他用户进程。根目录入口同步检查 `DEV-INSTALL-004`，避免旧副本继续运行已经废弃的提权逻辑。
- `DEV-INSTALL-006` 补齐 Playnite 自身的无窗口残留：先等待 20 秒正常退出；仅对当前会话、精确可信路径且 `MainWindowHandle=0` 的实例执行强制结束，并把 `Refresh` 与停止之间的自然退出视为成功。不得退化为按进程名批量终止。

## 2026-08-12 Worker 生命周期清理补充

- Playnite 插件退出必须调用 `WorkerLauncher.StopOwnedWorker()`；Launcher 只允许停止当前实例记录的 `runningWorker`，不能按名称终止任意 `GameSaveCenter.Worker`。`shutdownRequested` 防止退出竞态重新启动子进程。
- 本阶段 `3f05e16 fix: stop owned worker on Playnite shutdown` 已通过 Playnite Release 全量 198/198、源码校验、Release 编译和 Release self-contained 包验证。
- 隔离 Playnite 的 `--userdatadir` 首次启动会停在 `FirstTimeStartupWindowFactory`，不能据此宣称扩展加载；真实宿主验证仍必须看 `ExtensionFactory:Loaded plugin: GameSaveCenter` 与扩展日志，并记录 `MANUAL QA REQUIRED` 直到用户环境实际通过。

已完成：见 WORKLOG.md 与 Git log；不要重复实现已完成的 UI/性能工作。

## 已知坑

- WPF `ICollectionView.Refresh()` 昂贵；不要在每个按键或每次快照都调用。
- `ObservableCollection` Reset 仍会触发 CollectionView 重建；数据没变时应跳过（PERF-005）。
- 动态 ComboBox Items 重建会清空 SelectedItem；要显式恢复逻辑默认值。
- 大库启动不要同步全量匹配/扫描；先渲染 SQLite 缓存。
- Worker 是独立进程：Playnite 启动早期 IPC 可能超时，要用失败快速降级 + 后台重试。
- 修改器/CT/自定义工具启动一律走 Worker；禁止在 Playnite UI 进程直接 Process.Start 外部程序。
- CloseOnGameExit 只能关闭本 Session 由 GameSaveCenter 启动且能确认 PID/StartTime 的进程；脚本（BAT/CMD/PS1）与系统默认程序打开的文件不可靠，UI 对这类入口禁用开关。
- 自定义启动项支持 EXE/LNK/BAT/CMD/PS1/普通文件：EXE 与导入/重定位时已解析并持久化的 LNK→EXE 目标可跟踪；未解析的 LNK、脚本和系统默认程序启动时 Trackable=false。
- 磁盘 IO、图片解码不要放 UI 线程；图片解码要限制并发并 freeze。
- 表格/列表虚拟化很容易被外层 ScrollViewer 或 DataGrid 嵌套破坏，改 XAML 后必须跑 render-qa。
- DataGrid 不要写死运行时 `Height`，用 `MinHeight/MaxHeight` 保持有限 viewport；`Pixel ScrollUnit` 已在真实 Playnite 验证会回归（轻微滚动即大空白），当前必须保持 `Item` + 稳定行样式，禁止重新改回 Pixel。
- `git push` 前确认没有 bin/obj、用户本地配置、密钥、测试临时文件和大压缩包（如 `GameSaveCenter.7z` 不要提交）。

### ONBOARDING-001 不可丢失约束

- 首次使用状态由 Playnite `GameSaveCenterSettings.OnboardingCompleted` 持久化；未完成时 Dashboard 首次打开定位 Maintenance，用户可“跳过首次检查”，之后仍可手动重新运行环境检查。
- 环境检查只允许读取、创建/删除自身临时探针和只读远端列举；禁止自动备份、上传、同步、删除或覆盖真实存档。测试备份必须由用户明确点击，并且要求当前游戏已匹配且 Ludusavi 可用。
- 真实宿主验收必须看到 `ExtensionFactory:Loaded plugin: GameSaveCenter` 与扩展日志；隔离 Playnite 只能证明进程启动，不可替代真实安装验证。当前安装器不再默认请求 UAC；用户桌面应确认普通双击入口即可完成“关闭 Playnite → 回收 Worker → 构建安装 → 启动 Playnite”链路。

## 文档导航

- `docs/DEVELOPMENT_HANDOFF.md`：跨电脑/跨模型交接入口，包含每轮 UI 基线。
- `docs/PROJECT_MEMORY.md`：长期不可丢失约束与 UI 决策历史（大文件，按章节检索）。
- `docs/DEVELOPMENT_PROGRESS.md`：按 UI 编号的实施历史与下一步线索。
- `docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md`、`docs/design/UI_CHANGE_GATE.md`：UI 方向与门禁。
- `.codex/skills/wpf-apple-desktop-ui/SKILL.md`：WPF/Playnite UI 专项技能。
- `docs/ai/WORKLOG.md`：每阶段开发流水记录。
- `docs/ai/PERFORMANCE_BASELINE.md`：性能基线与测量方法。

## 2026-08-13 UI-QA-REAL-002 当前事实

- 首页“今日工作台”状态区已改为标题下方全宽第二行，避免 2K 最大化时原窄列 `WrapPanel` 将状态胶囊挤成不可见/竖向圆点；`OverviewView.xaml.cs` 会根据实际或估算宽度调整英雄卡内边距。
- 维护中心首次使用环境检查复用现有检查项，采用响应式 `UniformGrid`：可用宽度 ≥ 900 DIP 为 3 列，620–899 DIP 为 2 列，更窄为 1 列；检查卡统一拉伸并设置最小高度，避免胶囊乱序和不规则空洞。
- 设置页共享文本框内容宿主已垂直居中，默认高度 42 DIP；设置分类卡片的共享模板增加底部安全间距并使用一致圆角，避免窗口底部裁切。
- 本阶段没有新增主导航页面，没有改动业务命令、绑定、DataGrid 虚拟化或 Worker/恢复体系；渲染夹具仅补充首次使用卡片的演示数据。
- 当前自动化基线为 Core 42/42、Worker 117/117、Playnite 206/206；Release 构建无警告/错误，五种窗口尺寸的 `render-qa` 通过。
- 2026-08-13 真实开发安装完成，Playnite 扩展日志确认 `GameSaveCenter 0.6.70.0` 加载，Worker 正常运行。真实宿主日志/进程证明已记录；用户实际 2K 最大化、主题/DPI、Settings 连续缩放仍标记 `MANUAL QA REQUIRED`。
- 本阶段只完成用户反馈的三类 UI 修复，不引入第 15 个功能；下一步等待人工视觉反馈。

## 2026-08-13 UI-QA-REAL-003 当前事实

- 首页“最近 30 天玩过的游戏”风险卡片的两个操作按钮已经移到摘要下方的独立响应式行，避免 2K 或右侧窄栏中按钮与标题、统计文本挤压。
- 修改器中心启动延迟编辑器现在明确显示“启动延迟”和“秒”，继续绑定 `SelectedGameTool.LaunchDelaySeconds`，输入框高度收敛为 34 DIP。
- 媒体中心 `MediaGrid` 显式使用顶部对齐的虚拟化面板和内容对齐，已修复表头/筛选区下方到首条媒体记录之间的大段空白；未修改列表的虚拟化滚动模型。
- 当前自动化基线为 Core 42/42、Worker 117/117、Playnite 209/209；Release 构建无警告/错误，五种窗口尺寸的 `render-qa` 通过。
- 2026-08-13 一键开发安装完成，Playnite 扩展日志确认 `GameSaveCenter 0.6.70.0` 加载，Worker 进程从当前扩展目录运行。
- `AUTO VERIFIED` 仅覆盖自动化、渲染、安装和真实宿主日志；用户实际 2K 最大化、主题/DPI、连续缩放及真实媒体数据滚动仍为 `MANUAL QA REQUIRED`。
- 本阶段只补充用户反馈的三个布局问题，不新增主导航页面，不改变业务绑定或 Worker/恢复体系；下一步等待人工反馈。

## 2026-08-13 UI-QA-REAL-004 当前事实

- 设置页左侧分类卡的“底部/边缘圆角被削掉”根因已确认是 `SettingsHeaderScroller` 的滚动条占用内容宽度，固定 232 DIP 的 `TabItem` 被 viewport 裁切，不是 CornerRadius 数值失效。
- `SettingsHeaderScroller` 已扩展到 248 DIP，分类 `TabItem` 仍保持 232 DIP 内容宽度；滚动条出现时为卡片边缘预留安全区，分类卡继续使用 14 DIP 圆角并开启自身边界裁剪。
- 设置页在可用高度低于 760 DIP 时使用更紧凑的 60 DIP 分类卡和 8 DIP 间距；左右设置滚动面保留底部安全留白，避免最后一项在宿主 viewport 边缘被直接截断。
- 当前自动化基线为 Core 42/42、Worker 117/117、Playnite 210/210；Release 构建无警告/错误，五种窗口尺寸的 `render-qa` 通过。
- 2026-08-13 一键开发安装完成，真实 Playnite 扩展日志确认 `GameSaveCenter 0.6.70.0` 加载，Worker 进程从当前扩展目录运行。
- `AUTO VERIFIED` 仅覆盖自动化、渲染、安装和真实宿主日志；用户实际 2K/DPI 设置页最终视觉仍为 `MANUAL QA REQUIRED`。
- 本阶段只修复设置页现有分类卡和滚动 viewport 的裁切，不新增页面、不改变设置字段、绑定、保存语义或 Worker/恢复体系；下一步等待人工反馈。
