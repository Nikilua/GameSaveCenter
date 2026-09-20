# R19-04 取消关闭顺序证据

日期：2026-09-20  
分支：`codex/ui-finesse-round2`  
结论：已满足，待环境验证

## 1. 任务条件

R19-04 要求窗口关闭、请求取消、Worker 断开同时发生时有确定收尾；不能出现 UI 线程异常或永久 `IsBusy`；下一次打开要能从真实持久化任务状态恢复。

本项先审查当前生产链，确认已有实现覆盖该顺序，没有新增生命周期服务：

- `DashboardView.OnUnloaded` 停止 refresh timer、游戏事件和任务事件订阅，调用 `CancelDeferredUiWork`，停止大库通知轮询，并解除 UI 事件；`CancelDeferredUiWork` 统一取消 debounce、分页/详情/媒体/云端请求、初始同步、generation 和 persistence token。
- `DashboardViewModel` 的 `LatestRequestCoordinator`、分页/媒体 generation 和 `ApplyOnUi` dispatcher 门阻止卸载后的迟到结果回写；`StartTaskEventSubscription` 对取消任务观察 `OperationCanceledException`，`StopTaskEventSubscription` 先释放 batcher 再取消/释放 token。
- `GameSaveCenterPlugin.OnApplicationStopped` 先取消 lifetime，再停通知轮询、清 deferred 状态，最后只停止本插件拥有的 Worker；`RequestAsync`/`RequestWithTrackingAsync` 在 shutdown 后直接返回 `HostShutdown`，不会重新提交操作。
- Worker 初始化调用 `MarkInterruptedTasksAsync`，将旧 Worker 留下的 Queued/Running 任务标为 Failed，并按任务类型给出 `WORKER_RESTARTED_RETRYABLE`、`WORKER_RESTARTED` 或 `MANUAL_INTERVENTION_REQUIRED`；任务页重新读取 durable SQLite 快照，重复协调是幂等的，不伪装完成。

## 2. 实际证据

- `LatestRequestCoordinatorTests` 与 `R02BusyStateTests.BusyCoordinatorRejectsDuplicateAndRestoresAfterFailureAndCancellation`：`5/5` 通过。取消会让当前 scope 失效，旧上下文不能提交；重复 UI 操作只执行一次，失败/取消后 `IsBusy` 恢复，完成回调只收尾一次。
- `TaskReconcileServiceTests.ReconcileMarksInterruptedTasksAndIsIdempotent`：`1/1` 通过。Queued/Running 任务被标为失败并保留可读重试/人工介入边界，第二次协调返回 `0`，Succeeded 任务保持完成。
- Worker 取消/重启相邻组合（`TaskReconcileServiceTests`、`CloudRetryPersistenceTests`、`WorkerProcessRestartTests`）：`13 passed / 1 skipped / 14 total`。持久化任务状态、云端恢复和中断任务分类通过；硬重启独立进程项因当前环境禁止本地 Named Pipe 跳过。
- XAML 静态门禁在本阶段沿用已通过的 `24/24`；`validate-source.py` 与 `git diff --check` 已通过。R19-04 没有生产代码变更，仅补证据与交接事实。

## 3. 负例与环境边界

- `WpfUiResourceDictionaryTests.PlayniteShutdownStopsQueuedCallbacksBeforeTheyTouchWorkerOrUi` 已尝试运行，但当前复用的 net472 本地产物缺少 `GscBuildCommit`，在测试仓库身份校验处退出；这不是源码断言失败，也不能写成运行时通过。需在可注入构建身份的干净 Playnite 构建中复跑。
- 真实 Playnite 宿主关闭事件、真实 Worker 管道断开与同一时刻 Dispatcher 关闭、下一次打开后的最终 presented frame、DPI/UIA/读屏/IME、ETW 和宿主性能仍未验；没有把离屏/代理回归扩写成这些证据。
- 测试使用合成任务、隔离 SQLite/fake、已有本地产物；未写真实存档、媒体、用户云端或外部诊断，也未执行删除或自动恢复。Demo 原目录不可用，沿用恢复生产基线。

## 4. 下一步

下一可执行任务为 `R19-05 Worker 重启恢复`：在可创建 Named Pipe 且能注入真实 Worker 进程的环境复跑硬重启、进度订阅和重连去重，再核对断线后任务页与云端队列的持久化事实。
