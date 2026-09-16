# R01-08 跳过测试说明证据

## 结论

R01-08 已满足。当前隔离 Release 全流程把通过、失败和跳过分开记录，并按能力说明 gated 用例。任务表所称的 63 项可对应 Playnite 测试侧的 57 条 legacy UI 基线和 6 条 NamedPipe IPC gated；本机 Named Pipe 可用，因此这 6 条实际通过，实际 Playnite skip 为 57。Worker 项目另有 1 条 WorkerProcessFact gated，本机也通过。没有把环境限制改写成测试通过。

## 当前 Release 结果

执行：

~~~powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\build.ps1 -Configuration Release -OutputRoot .tmp\r01-08-skip-build
~~~

| 测试项目 | 通过 | 失败 | 跳过 | 总计 |
| --- | ---: | ---: | ---: | ---: |
| Core | 83 | 0 | 0 | 83 |
| Worker | 311 | 0 | 0 | 311 |
| Playnite | 523 | 0 | 57 | 580 |
| 合计 | 917 | 0 | 57 | 974 |

XAML 结构检查为 24/24，解决方案构建为 0 warning / 0 error。

## Skip / gated 分类

### LEGACY_UI：57 条，当前必然跳过

属性：LegacyProductionUiBaselineFact。原因是这些断言针对已撤销的今日工作台架构，当前生产页面恢复 AcrylicFork 基线后不再适用；它们不是当前 UI 缺陷的失败报告，也不应为了消除 skip 强行迁移成另一套设计。

| 文件 | 数量 | 影响范围 |
| --- | ---: | --- |
| OvernightClosureV6Tests.cs | 3 | 旧 UI 结构/间距/页面滚动 |
| OvernightV4SaveFormTests.cs | 1 | 旧存档表单结构 |
| SettingsAndAutoSelectSourceTests.cs | 2 | 旧设置与自动选择入口 |
| UiLayoutRegressionTests.cs | 11 | 旧布局/响应式宿主判断 |
| WorkspaceStateSourceTests.cs | 1 | 旧共享工作区状态架构 |
| WpfUiResourceDictionaryTests.cs | 39 | 旧资源、页面结构、响应式布局和命令可达性 |
| 合计 | 57 | 旧 UI 基线，不代表当前生产页面失败 |

### IPC_CANCELLATION_RECOVERY：6 条，NamedPipe 可用时可执行

属性：NamedPipeFact。若当前环境禁止创建本地 Named Pipe，则这 6 条应保持 skip，并在完整 Windows/Playnite 环境补测；本轮隔离 Windows 环境实际通过，不能把它们计入当前 skip。

- CallerCancellationBeforeConnectDoesNotOpenARequest：取消发生在连接前，不应打开请求。
- CallerCancellationDuringReadClosesThePipeAndReturnsPromptly：读期间调用方取消，管道关闭且快速返回。
- HostShutdownDuringReadIsDistinctFromCallerCancellation：宿主关闭与调用方取消保持不同语义。
- LostWriteResponseIsRecoveredWithTheSameRequestId：丢失写响应时用同一 request id 恢复。
- CallerCancellationDuringReplayWaitStopsWithAmbiguousOutcome：重放等待取消时保留 ambiguous 结果。
- CancellationDuringLargeWriteIsReportedAsAmbiguousAndIsNotRetried：大写入取消标记 ambiguous，不自动重试。

这组直接影响 IPC、取消和恢复保护语义；当前 6/6 随 Playnite 全量通过。

### WORKER_RESTART_RECOVERY：1 条，NamedPipe 可用时可执行

属性：WorkerProcessFact。HardRestartReconcilesDurableIncompleteTask 启动隔离 Worker，硬停止后由第二进程回收未完成 durable task；Named Pipe 不可用时才 skip。本机实际通过，当前 Worker 为 311/311。

这项直接影响 Worker 重启恢复、持久任务状态和宿主/Worker 协调，不等价真实用户数据目录或运行中 Playnite 宿主。

## 可执行补测与限制

当前可执行的 IPC/Worker gated 测试已在隔离目录通过；下次环境若 Named Pipe 探针失败，应保留失败数为 0、跳过数增加，并记录具体原因，不调整业务断言。LEGACY_UI 57 条只有在决定迁移到当前 AcrylicFork 契约时才应另立任务，不能用开启旧架构断言替代当前 UI 证据。

本轮没有启动真实 Playnite Dashboard、写入真实存档/媒体/云端、删除真实文件或外发诊断，也没有宣称物理 DPI、OS 输入/IME、presented frame、ETW 或宿主性能。当前游戏选框、滚动条、命令/绑定、取消/错误、恢复保护和有限列表契约未改。

## 下一步

R01-08 已满足；下一可执行小批量为 R02-01“动作优先级”，先核对主页、详情和批量栏现有 Primary/Danger/次要语义，再用真实控件级聚合验证避免重复高亮。
