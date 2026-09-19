# R12-04 恢复保护备份证据

日期：2026-09-19。实现分支：`codex/ui-finesse-round2`。实现提交：`e4e42f40`。

## 实施范围

- 先核对并复用现有 `RestoreOrchestrator`、`TaskCoordinator`、`GameOperationLock`、PreRestore 回滚和 `RestoreWorkflowProgress`，没有重建恢复服务、DTO、IPC、游戏选框或滚动体系。
- PreRestore 保护快照返回失败、没有可确认的新增版本、无法锁定或本地保护索引保存失败时，统一返回稳定错误码 `RESTORE_PRERESTORE_FAILED`。这些分支发生在目标版本预览/写入之前，危险恢复不会继续。
- 执行摘要明确显示“保护备份”子阶段；保护成功后任务完成消息包含已创建并锁定，保护失败时显示中止原因、错误码和任务详情。取消和已有回滚/人工介入错误语义保持。

## 行为与构建证据

- Worker `RestoreOrchestratorTests`：`12/12`。新增 fake 负例先让 PreRestore 失败，确认任务返回 `RESTORE_PRERESTORE_FAILED`、当前存档不变、没有目标恢复/锁定调用；随后把当前状态变为 `A-latest` 重试，确认重新使用同游戏锁和最新状态创建锁定快照并完成目标版本恢复。
- Playnite `R12RestoreWorkflowBehaviorTests`：`7/7`。除既有四阶段、目标失败、回滚失败和成功状态外，新增保护备份失败状态投影，确认执行阶段为 Failed、不可标记为完成、摘要保留执行结果入口和中止原因。
- `WpfUiResourceDictionaryTests`：`137 passed / 39 skipped / 0 failed`，总计 `176`；与 SaveCenter 资源字典/命令可达性相邻回归无失败。
- 完整隔离 `scripts/build.ps1 -Configuration Release -OutputRoot .tmp/r12-04-full-build`：XAML `24/24`；Release solution `0 warning / 0 error`；Core `85/85`；Worker `326/326`；Playnite source `68` 类、WPF `84` 类逐类隔离进程全部返回 `0`，最终输出 `All Playnite tests passed with WPF classes isolated by process.`
- `python scripts/validate-source.py`、`scripts/check-xaml.ps1` 和 `git diff --check` 通过；定向 Release 构建为 `0 warning / 0 error`。

## 边界与 main 集成事实

- 验证只使用合成任务状态、fake restore client、隔离目录/SQLite 和 STA WPF；没有读写真实存档、媒体、用户云端或外发诊断。重试时“最新状态”是 fake 当前状态的可追溯行为，不等价真实 Ludusavi/Playnite 长时操作。
- 本轮没有把离屏/隔离进程结果写成真实 Playnite/package-host 呈现、物理 DPI/跨屏、presented frame、UIA/IME、ETW 或宿主性能通过；Demo 原目录不可用，沿用恢复生产基线。
- 用户在 main 上运行 `GameSaveCenter-一键构建安装运行.cmd` 的最新 `DEV-INSTALL-008` 日志仍在安装前失败：源码组 `273 passed / 18 skipped / 1 failed / 292 total`，失败为 `UiFinesseRound2ControlSourceTests.DangerousConfirmationKeepsCancelAsTheInitialFocusTarget`。main dirty `DashboardView.xaml.cs` 已包含 `!dialogLifecycle.IsClosing`，但 main 跟踪测试仍期待旧源码字符串；续接分支测试已通过。
- main 的 R08 未提交/未跟踪用户文件、`src.zip` 均未覆盖或清理。本轮只推送续接分支，没有宣称 main 一键构建安装成功；需待 main 用户 R08 变更形成可审阅提交后再安全集成。

## 下一步

下一可执行小批量为 `R12-05 远端下载进度`：先核对现有 remote staging、校验和任务进度 DTO，再补下载/校验/准备覆盖的真实状态投影和取消残留负例。继续保留 main dirty 集成边界。
