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
- 收益：以合成的部分写入和磁盘满错误验证恢复安全链仍能回滚并保留原状态；不把该测试说成真实卷耗尽验收。

## A11：提交前证据路径检查

- 当前入口：`scripts/archive-diagnostics.ps1` 和 `diagnostic-retention.ps1` 已做归档所有权、SHA-256 与原件保留检查。
- 缺口：原有流程没有检查本次暂存证据在 GitHub Windows checkout 下的投影路径长度，也没有阻止再次提交相同的大型证据 blob。Round5 U06-02 的旧 TRX 嵌套路径曾导致 run #1451（`41681642551`）在 checkout 失败；证据见[既有记录](../../../ui-polish-round5-20261027/evidence/U06-02-3aa-20261104/BASELINE.md)。
- 实现：[`check-evidence-paths.ps1`](../../../../../../scripts/check-evidence-paths.ps1) 仅检查暂存证据路径，默认投影到 `GITHUB_WORKSPACE`（本地回退至 Windows runner 路径），上限 240 字符；对大于等于 1 MiB 的暂存证据 blob 与索引内其他证据比对 Git object ID。历史未改动文件不作为阻断项，未改变 workflow、required checks 或触发器。提交前执行 `pwsh -NoProfile -File scripts/check-evidence-paths.ps1`。
- 验证：[`Test-EvidencePathPreflight.ps1`](../../../../../../scripts/tests/Test-EvidencePathPreflight.ps1) 4/4：短路径通过、305 字符投影路径拒绝、重复 1,048,577-byte blob 拒绝、不同内容的大文件通过。UTF-8 BOM 修正后的最终报告含脚本 SHA、清理清单与 D 盘空间前后值：[测试报告](preflight-tests-attempt-05.json)。前三次报告和日志保留，记录进程查询权限/过宽快照导致的清理误判；attempt-04 是加 BOM 前的相同夹具复跑。所有 `.tmp` 夹具均经路径、链接、索引锁和独占文件检查后清理。[清理复核](cleanup-recovery.json)。
- 收益：在提交前挡住会让 Windows runner 无法 checkout 的新证据路径和重复大证据，同时保留有诊断价值的历史记录。

## 冻结候选验证：`52a57db9a4c8dacb1678fbf6fca4cac9b00e87e0`

本批代码在 `52a57db9a4c8dacb1678fbf6fca4cac9b00e87e0` 已提交并冻结。GitHub run #1545（`43161957758`）的基线 job `130995009638`（15/15步）与 SDK 10.0.401 job `130995009279`（8/8步）均成功；job、artifact ID/digest 与 required-checks 403 状态见[精确CI记录](ci-run-43161957758.json)。两个已运行 job 成功不等于 required-check/ruleset 名单已经验证。

本机 `GameSaveCenter-一键构建安装运行.cmd` → `GameSaveCenter-Run.cmd` 使用 SDK `8.0.423`，构建、测试、Worker发布、身份校验和打包阶段成功；160份TRX共2,042项，1,974通过、0失败、68跳过（跳过不计通过）。zip/pext identity为 `0.6.73+52a57db9…`，二者均45,740,287 bytes、SHA-256 `F0C8157CEF44048AD06B71369E9DBB0427E3C830EA191484D3AD79C0C474BBAF`。安装阶段向Playnite扩展目录写入被拒，根入口exit 1；候选未安装，候选Playnite启动未到达。安装失败阶段、旧安装DLL身份和原始日志/摘要SHA见[root入口记录](root-entry-result.json)。没有权限提升或重试。

随后单独运行 `scripts/build.cmd`（Release，默认测试开启），同样使用SDK `8.0.423`，exit 1。151份TRX共1,829项，1,800通过、0失败、29跳过；WPF隔离类只完成148/157。`UiDiagnosticsExporterTests` 因“Could not inspect the input desktop”使编排停止，剩余9类不计通过。摘要与原始console诊断见[独立构建记录](scripts-build-cmd-result.json)。没有桌面绕过或重试。

完整入口的160份TRX和独立build的151份TRX均有逐文件相对路径、字节数与SHA-256索引：[TRX清单](candidate-trx-index-52a57db9.json)。原始TRX仍留在本机两份build-diagnostics目录，没有复制进Git。

因此，P1本地可处理的代码/测试收口与精确SHA CI已完成，完整宿主验收仍未完成：没有验证候选安装身份或候选Playnite启动；运行后Playnite与Worker均未启动，也没有重启旧安装以免启动非候选版本。A03/U09-04与A04仍等待真实宿主条件；P2/P3保持未开始。

旧 `artifacts/gsc-b` 输出被诊断日志或既有证据路径引用，当前候选构建、TRX/失败诊断、安装尝试记录和安装包均需保留；详细清理预检和前后空间快照见[清理复核](cleanup-review-20261112.json)。既有用户TRX、`TestResults/`、配置、存档以及外部Playnite安装目录没有清理。
