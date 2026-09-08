# 2026-09-09 回归 Skip 账本

## 当前 Release 全量结果

命令：

    dotnet test GameSaveCenter.sln -c Release --no-build --no-restore -m:1 -nodeReuse:false -p:NuGetAudit=false -p:MSBuildEnableWorkloadResolver=false

结果：

| 项目 | 通过 | 跳过 | 总数 | 说明 |
|---|---:|---:|---:|---|
| Core | 76 | 0 | 76 | 全部执行 |
| Worker | 310 | 1 | 311 | 1 项真实 Worker 进程重启测试受当前沙箱 Named Pipe 条件限制 |
| Playnite | 420 | 63 | 483 | 57 项旧架构断言，6 项 Named Pipe 行为测试 |

没有失败。跳过项不计入通过，也不以跳过替代真实宿主验收。

## Playnite 的 63 项

### 57 项：旧“今日工作台”架构断言

这些测试使用 `LegacyProductionUiBaselineFactAttribute`，属性本身明确说明它们断言的是已撤销的生产 UI 架构；当前生产页以 AcrylicFork/Demo-first 基线为准，因此不在默认回归中运行：

| 测试文件 | 跳过数 |
|---|---:|
| `OvernightClosureV6Tests.cs` | 3 |
| `OvernightV4SaveFormTests.cs` | 1 |
| `SettingsAndAutoSelectSourceTests.cs` | 2 |
| `UiLayoutRegressionTests.cs` | 11 |
| `WorkspaceStateSourceTests.cs` | 1 |
| `WpfUiResourceDictionaryTests.cs` | 39 |
| 合计 | 57 |

替代证据：当前未标记该属性的生产源契约、`RestoredAcrylicForkBaselineTests`、可访问性/键盘/状态/布局活动测试，以及可运行时的 RenderHarness/STA 测试。它们只证明当前架构的对应事实，不证明旧断言仍然适合恢复。残余风险是这些历史测试没有逐条一对一迁移；恢复前必须先按当前 Demo-first 结构重写断言。

### 6 项：本地 Named Pipe 行为

`WorkerIpcClientBehaviorTests.cs` 的 6 个 `[NamedPipeFact]` 在当前执行环境全部跳过，原因是 `NamedPipeTestSupport.IsAvailable` 探测不到允许创建本地 Named Pipe 客户端。它们覆盖连接前取消、读响应取消、宿主关闭、同 RequestId 复核、复核等待取消和大写入取消；不能用普通源契约或 Worker 单元测试冒充这些进程间时序证据。

替代证据：`WorkerIpcClientBehaviorTests` 的源码契约、Worker `IpcRequestLedgerTests` `6/6`、当前 IPC/取消相关单元测试。真实 Named Pipe 行为仍需在完整 Windows/Playnite 环境重新执行。

## Worker 的 1 项

`WorkerProcessRestartTests.HardRestartReconcilesDurableIncompleteTask` 使用 `[WorkerProcessFact]`，当前沙箱无法创建本地 Named Pipe 因而跳过。L26 曾在完整 Windows 环境用隔离 Data/Saves/Media 和临时进程完成 `1/1` 真实硬停止→重启恢复证据；本次全量结果仍按 `0 通过 / 1 跳过` 记录，不把历史环境证据改写成本次通过。

## E01 行为矩阵

修正 `scripts/e01-behavior-matrix.ps1`：传入 `-SkipBuild` 时不再向 `dotnet test` 注入新的 `GscBuildOutputRoot`。此前会出现空日志和 `0/0` 汇总，退出码仍为 0，不能作为测试证据。

修复后的 `-SkipBuild -OutputRoot .tmp/l29-behavior` 矩阵实际执行 `151` 项：

| 分类 | 通过 | 跳过 | 总数 |
|---|---:|---:|---:|
| business | 74 | 0 | 74 |
| ipc | 25 | 0 | 25 |
| wpf-sta | 42 | 6 | 48 |
| fault-soak | 3 | 1 | 4 |
| 合计 | 144 | 7 | 151 |

E01 同样保留 `MANUAL QA REQUIRED`：真实 Playnite、实际 Worker 中断恢复、双选择器、主题/DPI、睡眠唤醒、退出重启和原视频操作不由离线测试代替。`.tmp/l29-behavior` 已在读取汇总后清理，未纳入 Git。
