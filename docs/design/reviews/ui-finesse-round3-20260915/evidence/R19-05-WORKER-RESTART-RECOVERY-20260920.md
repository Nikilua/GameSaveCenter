# R19-05 Worker 重启恢复证据

日期：2026-09-20  
分支：`codex/ui-finesse-round2`  
结论：已满足，待环境验证

## 1. 任务条件

R19-05 要求扩展已有重启回归，覆盖进行中任务、进度订阅和队列恢复；中断任务必须按真实持久化状态显示，不能伪装完成，重连不能重复订阅。

本项审查当前实现后确认已有能力满足方向，没有重建 Worker 恢复体系：

- `WorkerInitializationService` 启动先初始化 SQLite、执行 `MarkInterruptedTasksAsync`，再恢复云传输状态、快照和整库备份；旧 Worker 的 Queued/Running 任务会按任务类型变成 Failed，并保留 retryable 或 manual-intervention 错误码。
- `BackupOrchestrator.ResumePendingAsync` 从 durable full-library job 继续未完成工作，并用原子 in-flight 门避免重复恢复；`CloudRetryService` 与 `CloudTransferStateService.RecoverInterruptedAsync` 从隔离队列恢复云端重试，使用有限批次和已有退避策略。
- `TaskEventBroadcaster` 每订阅者独立 bounded channel（容量 `128`），慢订阅者优先淘汰进度、保留终态；`TaskEventPipeServerService` 每个当前连接创建并释放一个订阅，断开不会留下订阅者。
- Playnite `WorkerIpcClient.ListenForTaskEventsAsync` 只维护一个当前事件连接，断管以有界指数退避重连；`DashboardViewModel` 的 event token 与 batcher 在卸载释放，`lastTaskEventSequence` 配合 `GetTaskChanges` 和 SQLite 任务页做 durable 修复，不把瞬时事件流当作唯一来源。

## 2. 实际证据

- Worker `TaskEventBroadcasterTests`、`TaskReconcileServiceTests`、`CloudRetryPersistenceTests` 合计 `18/18` 通过：独立订阅 fan-out、释放无 residue、128 容量与终态优先、中断任务可见失败、重复协调幂等、云端队列跨 store 重建和中断恢复均通过。
- Playnite `TaskEventUiBatcherTests`：`3/3` 通过；UI 端按 TaskId 合并进度、终态优先和批量投递边界保持稳定。
- 独立 Worker 进程硬重启 `WorkerProcessRestartTests.HardRestartReconcilesDurableIncompleteTask` 已尝试，但当前执行环境禁止创建本地 Named Pipe，结果为 `1 skipped`；不能写成真实进程重启通过。
- `validate-source.py`、XAML `24/24`、`git diff --check` 已通过（本阶段文档同步无生产代码变更）。

## 3. 负例与环境边界

- 真实 Worker 进程被硬停止后再次启动、真实事件管道断线重连和 Playnite 宿主重开尚未取得运行时样本；Named Pipe 权限被拒绝时没有绕过系统权限。
- Playnite 一组相邻 subscription 测试中的一条因旧 net472 本地产物缺少 `GscBuildCommit` 在源码身份门退出；事件 batcher 的独立 `3/3` 通过不扩大解释为完整 Playnite build/test 全绿。
- 未验真实 Playnite/Worker package-host、物理 DPI/跨屏、UIA/读屏/IME、presented frame、ETW 或宿主性能；合成事件、隔离 SQLite/fake 和离屏 batcher 结果不等于真实呈现。
- 未写真实存档、媒体、用户云端或外部诊断；Demo 原目录不可用，沿用恢复生产基线。

## 4. 下一步

下一可执行任务为 `R19-06 分页快照变化`：先核对 durable cursor、快照重载、末页新增/删除、重复/漏项保护和稳定选择 ID，再补合成变化夹具。
