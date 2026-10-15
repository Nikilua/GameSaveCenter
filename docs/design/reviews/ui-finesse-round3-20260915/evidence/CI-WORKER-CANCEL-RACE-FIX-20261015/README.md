# Worker 取消竞态修复本机证据（2026-10-15）

- 基线提交：`8e6513856500505d4608f76ef098236c64e1b5b4`；工作树仅本任务修改 `src/GameSaveCenter.Worker/Services/TaskCoordinator.cs`，本轮构建使用该文件的未提交改动。
- 本机 SDK：8.0.423。`scripts/build.ps1` Release 解决方案构建成功，0 warning、0 error。完整本机 Playnite 全类扫描因耗时主动中止，已启动的 122 个隔离类中只完成前 6 个；不能视为 Playnite 全量通过。
- Core：125/125 通过；TRX：`tests/core-sdk8423.trx`。
- Worker：356 通过，1 个环境跳过（`WorkerProcessRestartTests.HardRestartReconcilesDurableIncompleteTask`；本机环境禁止 Named Pipes），0 失败；TRX：`tests/worker-sdk8423.trx`。
- 取消幂等回归：`TaskCoordinatorFailureTests.CancellationIsIdempotentAndPublishesSafeFinalizationBeforeCancelled` 修复后 1/1 通过；TRX：`tests/worker-cancel-targeted-sdk8423.trx`。
- 相关高对比焦点 WPF 类：3/3 通过；TRX：`tests/R09-high-contrast-sdk8423.trx`。
- CI #1026（SDK 8 与 SDK 10.0.401）在旧提交 `8e651385` 均失败。SDK10 完整日志显示 `CancelAsync` 对已释放的 `SemaphoreSlim` 调用 `Release`；代码改为保留运行时门闩供已捕获运行时引用的并发取消调用收尾，并保持重复取消返回成功。SDK8 的 `R09HighContrastBehaviorTests` 有一条焦点断言失败；本机相同类在当前构建 3/3 通过，原因仍待新 CI 与必要时的完整日志确认。
- 边界：新提交的 GitHub Actions 尚未运行；Playnite 全量本机回归未完成；SDK10.0.401 尚无本机复现。

## 文件哈希

- src/GameSaveCenter.Worker/Services/TaskCoordinator.cs（修复后工作树）：SHA-256 $(Microsoft.PowerShell.Commands.FileHashInfo.Hash)。
- `tests/core-sdk8423.trx`：`AB243A3E8285FE33DB0F96D7C3ABC99000E0457661C08558A140A6DE718B0BDC`。
- `tests/worker-sdk8423.trx`：`0BA26E77FDED28D96B3330D4313DC53CAAD228DD62B6C5877569F41316A93AC1`。
- `tests/worker-cancel-targeted-sdk8423.trx`：`D7AC740FE256CD576FD5524C50DDF2781E1FFC3C78A5F16E754BE7C6981A9171`。
- `tests/R09-high-contrast-sdk8423.trx`：`99E34FD371B880DD383B4917AFD601CB99E620BACC3D88169A20AC41416EF4B4`。
- 为减少临时空间，未归档 build 的 DLL/PDB；这些 TRX 记录本机 SDK8.0.423 的执行结果，不作为程序集身份或 CI 产物身份声明。
