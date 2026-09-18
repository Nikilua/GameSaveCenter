# R01-08 跳过测试说明证据

## 结论

R01-08 已满足当前可执行条件。当前 HEAD 72be494d 的隔离构建把通过、失败和跳过分开记录，并按能力说明 gated 用例。任务表所称的 63 项可对应 Playnite 测试侧的 57 条 legacy UI 基线和 6 条 NamedPipe IPC gated；本机 Named Pipe 可用，因此这 6 条实际通过，legacy skip 仍为 57。Worker 项目另有 1 条 WorkerProcessFact gated，本机也通过。没有把环境限制改写成测试通过。

## 当前 Release 结果

构建身份与输出目录：

~~~text
sourceCommit=72be494d（完整身份以当前 checkout HEAD 为准）
.tmp\r01-08-current-72be494d
~~~

执行：

~~~powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\build.ps1 -Configuration Release -OutputRoot .tmp\r01-08-current-72be494d
~~~

| 当前可执行分类/测试项目 | 通过 | 失败 | 跳过 | 总计 |
| --- | ---: | ---: | ---: | ---: |
| Core 全量当前程序集 | 83 | 0 | 0 | 83 |
| Playnite WorkerIpcClientBehaviorTests（6 NamedPipe gated + 1 隔离管道契约） | 7 | 0 | 0 | 7 |
| Worker WorkerProcessRestartTests | 1 | 0 | 0 | 1 |
| Legacy UI 分类清单 | 0 | 0 | 57 | 57 |

XAML 结构检查为 24/24；当前隔离输出已产出 Core、Worker、Playnite 及测试程序集。上表是 R01-08 的当前可执行/分类统计，不冒充一次全量 Playnite 绿色结果。

当前直接运行 Playnite 全量程序集还观察到若干非 R01-08 的 WPF 资源树、动画、布局和现有 R02/R06/R07 行为失败；这些失败没有被重分类为 skip，也没有用本项 gated 结果覆盖。相关阶段证据继续保留各自的失败/环境边界，后续按独立任务处理。

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

这组直接影响 IPC、取消和恢复保护语义；当前定向 WorkerIpcClientBehaviorTests 中 6/6 NamedPipe gated 通过，另有 1 条隔离管道契约通过。

### WORKER_RESTART_RECOVERY：1 条，NamedPipe 可用时可执行

属性：WorkerProcessFact。HardRestartReconcilesDurableIncompleteTask 启动隔离 Worker，硬停止后由第二进程回收未完成 durable task；Named Pipe 不可用时才 skip。本机定向测试实际为 1/1。

这项直接影响 Worker 重启恢复、持久任务状态和宿主/Worker 协调，不等价真实用户数据目录或运行中 Playnite 宿主。

## 可执行补测与限制

当前可执行的 IPC/Worker gated 测试已在隔离目录通过；下次环境若 Named Pipe 探针失败，应保留失败数为 0、跳过数增加，并记录具体原因，不调整业务断言。LEGACY_UI 57 条只有在决定迁移到当前 AcrylicFork 契约时才应另立任务，不能用开启旧架构断言替代当前 UI 证据。

本轮没有启动真实 Playnite Dashboard、写入真实存档/媒体/云端、删除真实文件或外发诊断，也没有宣称物理 DPI、OS 输入/IME、presented frame、ETW 或宿主性能。当前游戏选框、滚动条、命令/绑定、取消/错误、恢复保护和有限列表契约未改。

## 下一步

R01-08 已满足；R02-01～R02-05 已有独立证据，R02-06 仍受 Playnite 宿主菜单 visual tree 外部边界阻塞。下一可执行小批量为 R07-05“详情断点稳定”，先核对现有详情面断点实现和 Q/R 对应能力。
