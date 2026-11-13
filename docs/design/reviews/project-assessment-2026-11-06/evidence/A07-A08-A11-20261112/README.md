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

## 2026-11-13 启动宿主安全边界复核

Codex文件系统权限在前次安装写入失败后变为完全访问，但没有据此重跑真实入口。只读源码复核确认 `GameSaveCenterPlugin.OnApplicationStarted` 可启动Worker；`WorkerInitializationService.StartAsync` 会恢复待处理隔离项、清理过期检测快照并续跑未完成的整库备份；`CloudRetryService` 每30秒检查一次队列，用户设置允许且队列有任务时可能执行云端上传。相关入口见 [`GameSaveCenterPlugin.cs`](../../../../../../src/GameSaveCenter.Playnite/GameSaveCenterPlugin.cs)、[`WorkerInitializationService.cs`](../../../../../../src/GameSaveCenter.Worker/Services/WorkerInitializationService.cs)、[`CloudRetryService.cs`](../../../../../../src/GameSaveCenter.Worker/Services/CloudRetryService.cs)、[`RetentionSimulationService.cs`](../../../../../../src/GameSaveCenter.Worker/Services/RetentionSimulationService.cs)。

当前真实Playnite配置与Worker待处理状态未知。重跑 `GameSaveCenter-一键构建安装运行.cmd` 会启动现有用户配置，可能触发恢复/删除或云端写入；本轮明确排除这些真实副作用，因此没有再次启动宿主，也没有读取或改动配置、数据库、存档、安装目录。此复核未产生新的根链退出码、TRX或安装身份；此前失败和对应原始证据继续有效，不得写成已安装/已启动。

下一步：在默认Playnite profile与Worker数据均为一次性测试状态、且没有真实存档和云端写权限的Windows验收环境执行完整入口；独立 `scripts/build.cmd` 仍需可访问的输入桌面。现有CI只代表冻结源码SHA `52a57db9a4c8dacb1678fbf6fca4cac9b00e87e0`，文档提交SHA不改变该产品源码身份。没有清理 `artifacts/`、`.tmp/` 或用户数据。

## 候选：一键入口的一次性 Playnite profile 验收

- 状态：记录为验证基础设施候选；不属于抽查报告的16项确认缺陷，也不改变产品功能结论。
- 当前入口：`GameSaveCenter-一键构建安装运行.cmd` 调用 `GameSaveCenter-Run.cmd`，后者执行 `scripts/dev-install-run.ps1`。默认扩展安装目标和启动 profile 来自当前机器发现的 Playnite 安装/用户目录。
- 缺口：当真实 Playnite/Worker 配置未知时，完整入口会启动现有 profile；Worker 启动可能处理待恢复隔离项、过期检测快照、未完成整库任务或到期云队列。现有入口没有显式的一次性 profile 参数，也没有保证 Playnite 子进程仅接收隔离的 Worker 路径和 IPC 名称。52a57db 冻结候选在安装阶段写入被拒，安装身份/启动仍未验证。
- 预期收益：让完整入口可在仓库 `.tmp/` 的新标记 profile、独立 Worker 数据目录和私有 IPC 上运行，从而检查准确候选的安装身份与启动状态，避开真实 Playnite 配置、数据库、存档以及云端操作。
- 实现范围：仅为启动验收增加可选隔离 profile 路径，校验 profile 是新建且位于仓库 `.tmp/`，将扩展目录/Worker 数据/IPC 限定在该 profile，并从 Playnite 子进程环境移除继承的 GameSaveCenter、Rclone 和审计变量。默认入口行为保持兼容。此候选不授权访问真实存档、恢复/删除或云端写入。
- 验收：脚本/测试改动提交并冻结后，先跑隔离与安装事务定向测试；随后在无真实游戏数据的临时 profile 执行中文根入口完整链，并单独验证 `scripts/build.cmd`。记录准确源码 SHA、SDK、所有退出码、TRX 和安装身份；失败不计通过。

### 2026-11-13 验收结果

候选实现提交`c2fcc80477b936bafbaf46e983c266365480fb5c`已推送。SDK8.0.423 / Release中文根链在标记的`.tmp` Playnite profile上成功build/test/publish/package/install/start，160份TRX共2,042项中1,984通过、58 `NotExecuted`、0失败；58项不计通过。安装程序集身份与源码SHA一致，启动进程参数指向隔离profile，子进程云上传开关强制false。首次配置向导之后日志记录应用启动；没有观察到Worker进程，因此插件运行态仍未验。完整身份、包hash、进程与安全边界见[精确本机验收账本](../P1-host-c2fcc804-20261113/README.md)。

独立`scripts/build.cmd`在相同SHA与SDK上编译0 warning/0 error；Core 157/157、Worker 407/407、Playnite source 557通过/18未执行、WPF 41/157类后因`InputDesktop()`抛`Could not inspect the input desktop.`退出1。累计1,351项中1,333通过、18未执行、0失败；剩余WPF类不计通过。原始TRX、完整脚本输出和诊断汇总保留在工作区`artifacts/build-diagnostics/p1-scripts-build-c2fcc804/`。不绕过探针、不无限重跑。

精确源码run #1551（`43356307416`）的基线15/15与SDK10.0.401 8/8 jobs均success，artifact身份见[CI结果](../P1-host-c2fcc804-20261113/ci-run-43356307416.json)。此前候选记录提交run #1550（`43355362103`）两job也均success，身份见[CI记录](../P1-host-c2fcc804-20261113/ci-run-43355362103.json)。required-checks/ruleset名单的HTTP403仍是未验证，不等同于已运行job的成功。没有访问真实配置/数据库/存档，也没有执行恢复、删除或云端写入。
