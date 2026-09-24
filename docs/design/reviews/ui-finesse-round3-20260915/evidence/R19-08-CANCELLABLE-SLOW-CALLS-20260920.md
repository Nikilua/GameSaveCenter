# R19-08 慢调用可取消证据

日期：2026-09-20  
分支：`codex/ui-finesse-round2`  
结论：已满足，待环境验证

## 1. 任务条件

R19-08 要求为网络与外部工具等待设置可观察的超时和取消边界；取消必须解除 UI 等待，不可确认的写操作结果必须保持未知，不能伪装成功或盲目以新请求重试。

本项先核对最新实现，没有重建第二套等待或写入服务：

- `ExternalProcessRunner` 对 Ludusavi/Rclone 使用显式 `CancellationToken` 和每次调用 `TimeSpan`；超时返回稳定的 `PROCESS_TIMED_OUT` 结果，取消会终止整个外部进程树并观察 stdout/stderr，避免后台读取任务残留。输出按流限制为 4 MiB，并保留错误码和诊断文本。
- `LudusaviClient` 为版本查询、编辑、查找、备份/恢复分别传入 15 秒、2 分钟或 15 分钟边界；`RcloneClient` 为版本、读取、探测、校验、下载和复制传入 15 秒至 2 小时边界，所有调用继续走无 Shell 的安全 runner 和只读/单向命令白名单。
- `WorkerIpcClient` 将调用者取消、宿主退出、管道断开和超时区分；取消会先关闭管道以解除挂起的读写。写请求在可能已经送达时保留 `RequestId` 与 `MayHaveBeenAccepted`，只允许用同 envelope 做有限复核；取消不会创建新 ID 自动重放。`REQUEST_IN_PROGRESS`、`REQUEST_INTERRUPTED` 和可能已接收均由调用方获得明确的未知结果语义。
- `BusyOperationCoordinator` 使用原子运行门，并在成功、异常和取消路径的 `finally` 中释放 `IsBusy`；云端上传只有真实 `ProcessResult.Success` 才写入 `Uploaded`，失败/中断保留本地副本和队列诊断，不把超时写成成功。

## 2. 实际证据

- Worker `ExternalProcessRunnerTests`、`CloudTransferStateTests`、`CloudRetryPersistenceTests`、`IpcRequestLedgerTests` 合计 `36/36` 通过，覆盖长进程取消、外部工具超时稳定码、输出上限、云队列取消/校验恢复、重启中断状态、有限退避和 RequestId ledger。
- Worker `RcloneClientSourceTests` `1/1` 通过，确认 Rclone 没有把工作目录伪装成标准输入，所有调用仍经过 `RunSafeAsync(..., timeout, token)`。
- Playnite 可执行子集 `7 passed / 6 skipped / 0 failed / 13 total`：`BusyOperationCoordinator` 取消后恢复、`LatestRequestCoordinator` 旧请求失效、取消展示状态和生产请求协调逻辑通过。6 条 Named Pipe 时序测试因当前环境禁止创建本地 Named Pipe 客户端跳过。
- 同一 Playnite 定向组曾运行 `17 total`，其中 2 条旧源码测试在复用的 net472 产物缺少 `GscBuildCommit` 身份时退出；这是构建身份门失败，不计为本项行为失败，也没有通过改写身份绕过。
- `scripts/validate-source.py`、`scripts/check-xaml.ps1`（`24/24`）和 `git diff --check` 通过。本阶段没有生产代码变更，沿用 `ab32bc9f` 前的 Release 生产基线（`0 errors / 2 条既有 MediaCenter nullable warnings`）。

## 3. 未确认边界与安全语义

- 真实 Named Pipe 客户端、真实 Playnite/Worker 断管和网络延迟注入仍未取得运行时样本；没有绕过系统拒绝的管道权限。
- 真实 Rclone/Ludusavi、真实远端和真实云端写入未执行。现有代码只在外部命令明确成功时标记上传成功；超时/取消不会伪装成功，写请求可能已送达时不会以新 RequestId 盲目重试。远端是否已收到某一部分内容仍须通过既有只读校验或人工确认，不能由本地超时结果推断。
- 合成请求、fake/隔离 SQLite、隔离目录和本地已有 Release 测试产物均未接触真实存档、媒体、用户云端或外部诊断；未执行删除、恢复或自动改路径操作。Demo 原目录不可用，沿用恢复的生产基线。
- 未宣称真实宿主 UI 呈现、物理 DPI/跨屏、UI Automation/读屏/IME、presented frame、ETW 或宿主性能结果。

## 4. 下一步

下一可执行任务为 `R20-01 概览下一步`：先核对 OverviewPriorityResolver、现有概览状态与真实导航入口，覆盖无游戏、未匹配、可备份和失败待处理四种合成状态；本项的真实 Playnite/Worker 管道和真实外部工具边界继续作为待验项。

## 2026-09-24 当前 main 定向复核

- 当前 main/test identity `6618de22`。完整 Release solution：XAML `24/24`、0 errors、两条既有 `MediaCenterView.xaml.cs:703 CS8602`；`validate-source.py` 与 `git diff --check` 通过。Plugin 为 `net462`，Playnite tests `net472`，Worker `net8.0-windows`。
- Worker 慢调用组 `37/37`：`ExternalProcessRunnerTests` `5/5`（本机 ping loopback 取消、PowerShell UTF-8、双流输出上限、非零退出诊断、稳定超时码）；`CloudTransferStateTests` `13/13`（取消/失败验证不丢失既有 Uploaded 保证、旧代际不覆盖新状态）；`CloudRetryPersistenceTests` `12/12`（有限退避、恢复队列、重启中断状态）；`IpcRequestLedgerTests` `6/6`（RequestId 同 envelope ledger、冲突拒绝、重启中的写状态改为 Interrupted）；`RcloneClientSourceTests` `1/1`。
- IPC 边界组 `IpcMessageBoundaryTests` `10/10`，其中阻塞 `MemoryStream` 确实等到 read 开始后再发出取消并观察到 `OperationCanceledException`，其余边界门保持有限消息/客户端槽，不以断言代替行为。Playnite `LatestRequestCoordinatorTests` `4/4`、`R02BusyStateTests` `4/4`、`R08BusinessFeedbackBehaviorTests` `4/4`：覆盖旧请求失效、重复操作被拒绝、失败/取消后 IsBusy 在 finally 复位、快速/慢速按钮 busy 状态与取消结果反馈。
- `WorkerIpcClientBehaviorTests` `1 passed / 6 skipped / 7 total`：唯一不依赖 pipe 的隔离 pipe-name 测试通过；六条客户端连接/读取消/Host 退出/写响应丢失后同 RequestId 复核/模糊写取消用例因 `NamedPipeTestSupport` 探测到当前环境禁止创建本地 Named Pipe 客户端而跳过。未绕过限制。最终六份 TRX 均无 `InvalidComObjectException`。
- 总计 `60 passed / 6 skipped / 0 failed / 66 total`。Loopback ping 和 PowerShell 的可执行文件均存在；它们只用于本地进程边界测试，没有运行真实 Ludusavi/Rclone、访问网络远端或写云端。测试仍不能证明 Named Pipe/Playnite/Worker 实际 IPC 时序或真实云端写入状态。

当前结论仍为“已满足，待环境验证”。TRX：`R19-08-WORKER-SLOW-CANCEL-MAIN-6618DE22.trx`、`R19-08-IPC-MESSAGE-BOUNDARY-MAIN-6618DE22.trx`、`R19-08-PLAYNITE-IPC-CANCELLATION-MAIN-6618DE22.trx`、`R19-08-PLAYNITE-BUSY-CANCELLATION-MAIN-6618DE22.trx`、`R19-08-REQUEST-COORDINATOR-MAIN-6618DE22.trx`、`R19-08-CANCELLATION-FEEDBACK-MAIN-6618DE22.trx`。

下一可执行任务：`R20-01 概览下一步`。先核对现有 `OverviewPriorityResolver`、首屏状态 DTO 与游戏选框真实导航；四态仅用合成游戏/任务验证。当前设置截图的用户包身份与真实 host 呈现仍保持独立待验，不把 STA WPF 结果升级成实机通过。
