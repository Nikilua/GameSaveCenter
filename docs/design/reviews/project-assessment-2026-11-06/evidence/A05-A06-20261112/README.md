# A05/A06 隔离验证

本证据对应综合评估计划首个源码批次。所有文件路径均由测试夹具在系统临时目录内创建；没有读取或写入真实存档、用户配置或云端数据。最终源码提交SHA及该提交CI身份在下一次账本更新回填。

## 改动

- `AtomicFileWriter` 按规范化目标路径串行化同一路径的写入、复制和替换，锁按引用计数回收；不同目标路径仍可并发处理。
- 写入临时文件后、原子移动前重新检查取消令牌，取消发生在提交点前时清除临时文件并保留旧目标。
- Worker 意外重启时，将持久化 `RestoreReport` 的结果更新为 `ManualIntervention`，写入失败码并标记需要人工检查，同时保留原阶段、目标备份和 PreRestore ID。

## 验证

SDK：8.0.423（当前用户缓存）；Release；`GameSaveCenter.Worker.Tests`，目标框架 `net8.0-windows10.0.19041.0`；VSTest 17.11.1。输出隔离在本阶段专用本地临时目录中，完整 TRX、日志、退出码已在本目录归档；本轮临时输出清理清单与空间快照见[cleanup.json](cleanup.json)。

首轮定向运行exit 1：12项中10通过、2失败、0跳过。失败分别是同目标并发覆盖抛出 `UnauthorizedAccessException`，以及临时文件已创建后取消仍继续替换目标。该诊断没有被覆盖或删除：[原始日志](failure-initial-test.log)、[原始TRX](failure-initial.trx)、[退出码](failure-initial-exitcode.txt)。

修正后定向运行exit 0：12/12通过、0失败、0跳过：[日志](targeted-pass-test.log)、[TRX](targeted-pass.trx)、[退出码](targeted-pass-exitcode.txt)。随后运行整个Worker测试项目exit 0：406项，405通过、0失败、1跳过。跳过项为 `WorkerProcessRestartTests.HardRestartReconcilesDurableIncompleteTask`，原因是当前执行环境禁止创建本地Named Pipe；此skip不计为通过：[日志](worker-full-test.log)、[TRX](worker-full.trx)、[退出码](worker-full-exitcode.txt)。

TRX原件校验值：首轮失败 23,634 bytes / SHA-256 `DF2199FF31847DD6ED828C208BE130467E2A4A488F3F7762AA43875B7340E63E`；定向通过 20,262 bytes / `8F1F93D4E6C48F769F3C7B76DFD116B72186738B9FC6351E659D9CE1CED0AD65`；Worker全套 645,283 bytes / `A54F7FF31FF039736EBB78F7C851DEA5DB7FDF4F569AE67CBD5092D8C7DCA649`。

`python scripts/validate-source.py` exit 0；`git diff --check` exit 0。未运行全仓完整安装启动链、Playnite宿主或GitHub工作流；这些由最终冻结候选门禁核验。

## 失败与成功源码身份

首轮失败运行基于提交 `6768878a4c9df55ba024033481d9972d2dbb75e3`，当时工作树包含本批测试与SQLite恢复报告变更；尚未加入目标路径锁和移动前取消检查。该版本 `AtomicFileWriter.cs` 的Git blob为 `805bc4482c7764c7ddcd43feea59ec69f3e81907`。TRX及日志保留首轮测试失败诊断。

修正后定向与Worker全套运行使用相同源码工作树：

| 文件 | Git blob |
| --- | --- |
| `src/GameSaveCenter.Worker/Infrastructure/AtomicFileWriter.cs` | `5effcaa2156723d748835321d665d7469d0f7ffe` |
| `src/GameSaveCenter.Worker/Persistence/SqliteStateStore.cs` | `3c5696056825e0a9810d009b1c99047bc2a09599` |
| `tests/GameSaveCenter.Worker.Tests/AtomicFileWriterTests.cs` | `bd793f293564d11ae6454689bc5e4f3a859fd532` |
| `tests/GameSaveCenter.Worker.Tests/TaskReconcileServiceTests.cs` | `81392f151fa17e9a727cbfd1021dd2ff8e9d7be3` |

运行时工作树基于上述父提交且包含本批改动；精确提交身份和双SDK CI job 后续记录，不把本机工作树测试冒充提交级完整门禁。
