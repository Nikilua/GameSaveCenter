# R13-03 手动重试范围证据

日期：2026-09-19  
分支：`codex/ui-finesse-round2`  
代码提交：`680ea83a`（`明确云端手动重试范围`）

## 完成范围

- 先复用现有 `CloudTransferStatusDto.State`、维护页选中行、`RetryCloudUploadCommand`、任务中心单项/批量重试和 IPC replay ledger；没有另建云端上传服务、队列或请求身份系统。
- `CloudTransferStatusDto.CanManuallyRetry` 将手动重试限定为选中行的 `Failed` 或 `RetryScheduled`；`Transferring`、`Uploaded`、`RemoteVerified` 等状态不会显示为可重试。
- 维护页详情明确“仅当前选中项；只重试云端上传，不重新执行本地备份”；按钮仍由 `!IsBusy` 和选中项状态共同门控，传输中或已完成项显示不会重复提交。
- 既有任务中心批量重试仍只取当前已加载且符合筛选的失败/取消任务，按任务类型与游戏 ID 去重；云端上传路径复用云端重试，不重新创建本地备份。维护页单项入口和任务中心批量入口的作用域没有混用。
- `RetryCloudUpload` 与 `RetryMediaCloudUpload` 已在 `IpcRequestSemantics` 中要求 replay protection；请求超时/管道丢失时 `WorkerIpcClient` 复用同一 RequestId，SQLite ledger 对相同身份只认领一次，冲突 payload 被拒绝。

## 行为与构建证据

- Playnite `R13CloudTransferStageBehaviorTests`：`8/8` 通过。实际构造 DTO 验证失败/排队可重试、传输中/已上传/已校验不可重试，并用真实 `RelayCommand` 的 `CanExecute` 门控验证忙态第二次点击不会增加提交次数。
- Core `UiDisplayMappingTests`：`29/29` 通过。
- Worker `CloudTransferStateTests` 与 `BackupResultLayerTests`：`19/19` 通过，覆盖备份/媒体队列分离、状态持久化以及本地成功与云端失败的部分成功显示/补救边界。
- Worker `IpcRequestLedgerTests`：`6/6` 通过，覆盖相同 RequestId 的一次认领、完成结果重放、类型/协议/负载冲突拒绝和 Worker 重启中断语义。
- 外部隔离 Debug solution 构建：`0 warning / 0 error`；XAML `24/24`；`python scripts/validate-source.py` 与 `git diff --check` 通过。
- Playnite `WorkerIpcClientBehaviorTests`：`1 passed / 6 skipped / 0 failed`。6 个真实 named-pipe 时序用例因当前宿主测试条件跳过，未计入真实 IPC 客户端通过；ledger 行为仍由隔离 SQLite 实测覆盖。

## 边界

- 构建使用当前 linked worktree 的外部隔离源码副本和独立输出根，完成后已清理；没有连接真实云端、rclone、存档、媒体目录或用户诊断数据。
- 未运行真实 Playnite/package-host、真实远端、物理 DPI/跨屏、UIA/IME、最终 presented frame、ETW 或宿主性能；离屏命中区/命令门控不代表真实设备输入或宿主管道时序。Demo 原目录不可用，沿用恢复生产基线。
- `.tmp/r12-07-build-final` 仍是旧阶段遗留，关闭构建服务器后逐项精确删除仍 Access denied；本阶段没有强杀未知进程，也没有把该目录作为证据引用。
- main 工作树仍有用户 R08 文件与 `src.zip`，本阶段未触碰、未合并。

下一可执行任务：`R13-04 暂停与允许时段`，先核对现有 `CloudUploadQueuePaused`、允许时段策略、持久化状态和进行中上传的边界。
