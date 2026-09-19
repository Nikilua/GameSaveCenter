# R12-08 恢复结果报告证据

日期：2026-09-19
分支：`codex/ui-finesse-round2`
代码提交：`fd4756ea`（`补齐恢复结果报告链路`）

## 完成范围

- 复用既有 `RestoreOrchestrator`、`TaskCoordinator`、`TaskStatusDto`、PreRestore 和任务中心详情滚动容器；新增 `RestoreReportDto` 作为无凭据的结果摘要。
- 报告记录执行游戏/PlayniteId、目标 `BackupId`、预览确认的文件数与大小、PreRestore 保护快照、当前阶段、完成/回滚/人工介入/取消/失败结果和稳定任务 ID。
- Worker 将报告随任务状态持久化到 SQLite；既有最近任务、活动任务和分页任务查询均回读报告。查询回读使用与写入相同的 Web JSON 选项，避免属性名策略导致保护快照丢失。
- TaskCenter 在既有任务详情 `ScrollViewer` 内显示结果卡，明确目标版本、任务 ID、文件范围、保护备份和失败阶段；`CopyTaskErrorCommand` 在有报告时复制 `ToRedactedText()`，不复制路径、诊断详情或凭据。
- `Completed` 与 `RolledBack`/`ManualIntervention`/`Failed`/`Cancelled` 分开投影；回滚失败保留人工介入和错误阶段，不冒充全部完成。

## 行为与构建证据

- Playnite R12 定向测试：`FullyQualifiedName~R12Restore`，`15/15` 通过。
- Worker 定向测试：`RestoreReadinessTests|RestoreOrchestratorTests|TaskQueryPersistenceTests`，`34/34` 通过；覆盖成功、回滚、报告持久化最近/分页回读和原恢复就绪/编排回归。
- 最终隔离 Debug solution 构建：`0 warning / 0 error`；目标包含 Contracts、Core、Worker、Playnite `net462` 及测试程序集。
- XAML 结构/资源检查：`24/24`；`python scripts/validate-source.py` 通过；`git diff --check` 通过。
- 测试先捕获到一个真实持久化缺口：分页查询使用默认 JSON 选项，导致 `PreRestoreBackupId` 回读为空；改为复用 `SqliteStateStore` 的 Web JSON 选项后，Worker 定向集恢复为 `34/34`。

## 构建与验证边界

- 当前 linked worktree 直接构建 WPF 临时项目时仍遇到 `_wpftmp.csproj` 的 Access denied；最终验证使用当前分支源码的外部隔离副本和独立 `GscBuildOutputRoot`，不是 main 源码，也不是旧实现覆盖。
- 没有运行真实 Playnite/package-host、安装包、最终 presented frame、物理 DPI/跨屏、UIA/IME 或宿主性能验证；未使用被拒绝的 ETW/系统跟踪权限，也没有把离屏结果写成真实呈现/跨屏证据。
- Demo 原目录不可用，本阶段继续使用已恢复的生产资源基线；没有改变现有游戏选框、滚动条系统、命令绑定、取消/错误语义、恢复保护、有限列表性能或 Playnite/net462 兼容。
- 证据只使用合成 DTO、fake Worker、隔离 SQLite/目录和隔离构建输出；没有读取或修改真实存档、媒体、云端数据或外发诊断。
- `.tmp/r12-07-build-final` 的精确清理在关闭 MSBuild/VB/C# 编译器服务器后仍逐项返回 Access denied；未强杀未知进程，下一启动继续重试。外部隔离源码/输出为本阶段临时产物，已在验证后清理，不作为文档引用证据。

下一可执行任务：`R13-01 队列阶段展示`，先核对 `CloudTransferCoordinator`/维护页现有阶段投影及 Q22 依赖，再补阶段与负例证据。
