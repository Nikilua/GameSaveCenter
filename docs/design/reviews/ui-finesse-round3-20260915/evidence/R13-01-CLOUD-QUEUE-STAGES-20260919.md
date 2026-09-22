# R13-01 队列阶段展示证据

日期：2026-09-19
分支：`codex/ui-finesse-round2`
代码提交：`803470b8`（`补齐云端队列阶段展示`）

## 完成范围

- 复用 `CloudTransferStatusDto.State`、`CloudTransferSummaryDto.QueueControlDisplay`、`CloudTransferStateService` 和现有维护页队列；没有改写 Worker 状态机、重试调度、远端校验或命令绑定。
- 新增 `QueuePhaseDisplay` 派生显示：待处理显示“等待队列”，网络/不完整传输退避显示“等待网络”，其他重试显示“等待重试”，活动上传显示“上传中”，远端检查显示“验证中”，已上传但未检查显示“等待验证”，远端检查成功显示“已验证”。
- 维护页表格和详情复用同一阶段投影；详情同时保留 `GuaranteeLevelDisplay`，所以“已上传”不会被显示成“已验证”。既有队列摘要继续显示“自动队列已暂停/当前不在允许时段/自动队列运行中”。
- 负例固定为认证失败重试不冒充“等待网络”，而是“等待重试”；认证仍由既有维护提示和命令门禁处理。

## 行为与构建证据

- Core `UiDisplayMappingTests` 阶段映射与等待时段验证：`27/27` 通过。
- Playnite `R13CloudTransferStageBehaviorTests|MaintenanceCloudTransferResolverTests`：`7/7` 通过；验证维护页阶段和保证级别绑定共存。
- Worker `CloudTransferStateTests`：`11/11` 通过，确认既有持久化队列状态、重试和远端校验语义没有被阶段投影改变。
- 外部隔离 Debug solution 构建：`0 warning / 0 error`；XAML `24/24`；`python scripts/validate-source.py` 和 `git diff --check` 通过。

## 边界

- 构建使用当前分支源码的外部隔离副本和独立输出根；Playnite 源码型测试通过显式 `GIT_DIR` 绑定当前 linked worktree 的 `e516a3d1`，没有把 main HEAD 当作测试源。外部源码/输出已清理。
- 未运行真实 Playnite/package-host、真实远端、物理 DPI/跨屏、UIA/IME、presented frame、ETW 或宿主性能；没有写用户云端、真实存档、媒体或诊断数据。Demo 原目录不可用，沿用恢复生产基线。
- `.tmp/r12-07-build-final` 仍是旧阶段遗留且精确删除逐项 Access denied；本阶段未扩大清理范围或强杀未知进程。

下一可执行任务：`R13-02 下次重试时间`，先核对既有 `NextAttemptUtc/NextAttemptLocal`、时钟变化和页面生命周期，再补无负倒计时与无常驻计时器证据。
