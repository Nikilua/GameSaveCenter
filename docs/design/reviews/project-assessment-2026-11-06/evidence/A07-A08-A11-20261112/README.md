# A07 / A08 / A11 P1 批次

本批按入口和已有证据去重，不把评估报告当作已确认缺陷。

## A07：IPC 断连、重启与任务状态

- 当前入口：Playnite `WorkerIpcClient` 的写请求与固定 `RequestId` 重放；Worker SQLite request ledger、任务查询和重启协调；任务页阶段呈现与长任务页面切换。
- 缺口核对：已有隔离覆盖证明完成响应可重放、同一 RequestId 可恢复丢失写响应、冲突 payload 会被拒绝、Worker 重启不会盲目重放进行中的写操作，取消/断连会保留“可能已接受”的歧义；任务协调器把可重试任务与恢复人工介入任务分开。当前没有足以支持新增产品实现的本地缺陷证据。真实 Playnite 中的断连后页面状态仍属宿主验收边界。
- 复用测试：`IpcRequestLedgerTests.SameRequestIdIsClaimedOnceAndCompletedResponseCanBeReplayed`、`WorkerRestartMarksInFlightWriteAsInterruptedInsteadOfReplayingIt`、`SameRequestIdWithDifferentTypePayloadOrProtocolIsRejectedAsConflict`、`TaskQueryCanFindTheTaskSubmittedByAnIpcRequest`；`WorkerIpcClientBehaviorTests.LostWriteResponseIsRecoveredWithTheSameRequestId`、`CallerCancellationDuringReplayWaitStopsWithAmbiguousOutcome`、`CancellationDuringLargeWriteIsReportedAsAmbiguousAndIsNotRetried`、`ReplayDisconnectBeforeCallerCancellationRemainsAnAmbiguousTransportFailure`；`TaskReconcileServiceTests.ReconcileMarksInterruptedTasksAndIsIdempotent`、`R15TaskStageTests`、`R22LongTaskLeavePageBehaviorTests`。
- 收益：避免未知写入结果被当作失败后盲目重复执行，保持任务状态可解释。
- 结论：去重为既有隔离覆盖充分；不新增代码。真实宿主页面复核未宣称完成。

## A08：预检通过后的受控磁盘满错误

- 当前入口：`RestoreOrchestrator.ExecuteAsync` 在强制创建并锁定 PreRestore 快照后执行恢复；写入失败走已有自动回滚。`scripts/space-budget.ps1` 的构建/打包预算预检不模拟真实卷耗尽。
- 缺口：已有部分写入、只读和异常回滚测试，但没有 Windows 磁盘满错误码 `0x80070070` 在部分写入后的显式用例。
- 实现：只给 `RestoreOrchestratorTests` 的合成 `FakeRestoreClient` 增加磁盘满注入。它先写入合成 `PARTIAL` 内容，再抛出带磁盘满 HResult 的 `IOException`；断言任务失败且已回滚为 B、PreRestore 快照仍保存 B 且锁定、审计状态为 `RolledBack`。没有填充真实卷或操作用户存档。
- 验证：SDK `8.0.423`，Release，离线使用已缓存 NuGet 包，`-m:1 -nodeReuse:false`；精确用例 `1 passed / 0 failed / 0 skipped`，退出码 0。TRX 大小 3,238 bytes，SHA-256 `A67D37813BD2DEC73A8B3586CCDEEB8BFCCC6BD8F09123370121F084FF866A65`：[TRX](trx/A08-disk-full-rollback-after-analyzer-fix.trx)，[最终测试日志](A08-offline-targeted-test-attempt-03.log)。测试源 blob SHA-1 `829eab0a806d5444690b243a823a9bd16a7d5be7`；生成测试程序集 SHA-256 `6569A28956B76477840115C73BB26B2ABF421FD7D9FEE709E88865835ABE3EE8`。隔离 SDK 输出已验证后清理，详见[清理记录](cleanup-recovery.json)。
- 首轮普通还原因本机并行子进程/网络等待没有形成测试结果；详细诊断、SDK 选择修正、离线串行还原日志均保留在本目录。未把这些失败尝试算作测试通过。
- 收益：证明预检之后发生真实写入空间错误时，现有恢复安全链仍会保留原状态。

## A11：提交前证据路径检查

- 当前入口：`scripts/archive-diagnostics.ps1` 和 `diagnostic-retention.ps1` 已做归档所有权、SHA-256 与原件保留检查。
- 缺口：原有流程没有检查本次暂存证据在 GitHub Windows checkout 下的投影路径长度，也没有阻止再次提交相同的大型证据 blob。Round5 U06-02 的旧 TRX 嵌套路径曾导致 run #1451（`41681642551`）在 checkout 失败；证据见[既有记录](../../../ui-polish-round5-20261027/evidence/U06-02-3aa-20261104/BASELINE.md)。
- 实现：[`check-evidence-paths.ps1`](../../../../../../scripts/check-evidence-paths.ps1) 仅检查暂存证据路径，默认投影到 `GITHUB_WORKSPACE`（本地回退至 Windows runner 路径），上限 240 字符；对大于等于 1 MiB 的暂存证据 blob 与索引内其他证据比对 Git object ID。历史未改动文件不作为阻断项，未改变 workflow、required checks 或触发器。提交前执行 `pwsh -NoProfile -File scripts/check-evidence-paths.ps1`。
- 验证：[`Test-EvidencePathPreflight.ps1`](../../../../../../scripts/tests/Test-EvidencePathPreflight.ps1) 4/4：短路径通过、305 字符投影路径拒绝、重复 1,048,577-byte blob 拒绝、不同内容的大文件通过。UTF-8 BOM 修正后的最终报告含脚本 SHA、清理清单与 D 盘空间前后值：[测试报告](preflight-tests-attempt-05.json)。前三次报告和日志保留，记录进程查询权限/过宽快照导致的清理误判；attempt-04 是加 BOM 前的相同夹具复跑。所有 `.tmp` 夹具均经路径、链接、索引锁和独占文件检查后清理。[清理复核](cleanup-recovery.json)。
- 收益：在提交前挡住会让 Windows runner 无法 checkout 的新证据路径和重复大证据，同时保留有诊断价值的历史记录。

## 相关精确提交 CI

A05/A06 当前源提交 `06b88bf95e7d3646ad905a4333938369891fca1b` 的 GitHub run #1544（`43150017597`）已终态成功：基线 job `130956727575` 15/15 步成功，SDK 10.0.401 job `130956727381` 8/8 步成功。run artifacts 的 ID、SHA-256 digest、大小和链接见 [CI 记录](../A05-A06-20261112/ci-run-43150017597.json)。这是该 SHA 的已运行 job 事实；required-checks/ruleset 名单此前 HTTP 403，仍未确认。

本批代码当前针对 `06b88bf95e7d3646ad905a4333938369891fca1b` 的本地工作树验证。冻结后的本批提交仍须查询基线 SDK 与 SDK10 精确 SHA jobs；P1 本地范围收口后，按用户指定顺序执行完整中文入口链，再单独验证 `scripts/build.cmd`。A03/U09-04 与 A04 仍等待真实宿主测量，不因本批关闭。
