# R13-07 CI 源码测试断言校正

日期：2026-10-16。输入代码提交：`33b9d23f73b05d18b703959a5106b34b2fb662f9`，Actions run [#1045](https://github.com/Nikilua/GameSaveCenter/actions/runs/38897153757)。

## CI 失败事实

- SDK `10.0.401` job `116960249686` 与仓库锁定 SDK `8.0.423` job `116960249845` 均终态失败。两份完整 job log 已通过 GitHub 插件取得；不是网络/包还原失败。
- 两条 lane 都在 Playnite `source test group` 的 `WorkspaceStateSourceTests.ConsistentPagedViewsPreserveSelectionAndStopAfterRepeatedResets` 失败；两边均为 462 项、443 通过、1 失败、18 跳过。`Assert.Contains` 找不到旧文本 `var shouldLoadPending = restored == null`。
- R13-07 将云队列页选择恢复提取为生产路径实际调用的 `ResolveCloudTransferPageSelection` helper。失败项是旧的源码形状断言，不是该行为的反例。R13-07 的 `R13CloudTransferStageBehaviorTests` 已对同一生产 helper 执行稳定选择恢复、后续页 pending、删除后回退正反行为。

## 校正

本轮只改 `WorkspaceStateSourceTests`：将名称改为 `PagedViewsRetainSelectionRecoverySourceHooks`，把云队列断言改为确认生产调用 `ResolveCloudTransferPageSelection`。该用例仍是源码接线检查，不作为交互验收；具体分页/选择行为由 R13-07 WPF 行为测试负责。未改生产模板、业务状态或断言阈值。

## 本机验证边界

本机只安装 SDK `9.0.302` / VSTest `17.14.1`，仓库 `global.json` 要求 SDK `8.0.423` 且禁用 roll-forward。首次从仓库外尝试定向运行时，MSBuild 在项目引用阶段终止且没有测试结果；诊断包含 MSB4276，报告默认解析器找不到 `Microsoft.NET.SDK.WorkloadAutoImportPropsLocator` 与 `Microsoft.NET.SDK.WorkloadManifestTargetsLocator` 目录。随后参考仓库 `build.ps1` 的配置，显式关闭 `MSBuildEnableWorkloadResolver`，使用全新隔离目录 `.tmp/r13-07-source-fix-sdk9-20261016` 执行 Release `dotnet test`，成功编译相关引用项目和 Playnite 测试项目；`WorkspaceStateSourceTests` 为 `9 passed / 1 skipped / 0 failed`，VSTest `17.14.1`、testhost `.NET Framework 4.8.9181.0`。这只是 SDK9 定向项目验证，不是完整 solution build，也不替代 SDK8/10 CI。TRX 已归档为 [`WorkspaceStateSourceTests-sdk9302-current-source-fix.trx`](tests/WorkspaceStateSourceTests-sdk9302-current-source-fix.trx)，SHA-256 `24FC93F63D1494AA8612415384291078E45C02F4B77550F7A80703E5399876E2`。测试源 SHA-256 `6108E82BD515C77952ED83E7B2BF491A89A8878BB3FF46A580933AD6C2719035`；测试 DLL SHA-256 `5128343D795C5F2BF766BD3C77D068D20C1336D26F13438B93DB23F936752197`、PDB SHA-256 `523D550A509011CFC8F631EC32600013234DC6BFE39A8E51621EB835CBB6323D`，ProductVersion `0.6.73+33b9d23f73b05d18b703959a5106b34b2fb662f9`；Playnite DLL SHA-256 `074F0C7F6A4606C0C3AB393FF8BCDC2769D31B14C2B64001D70AA1AA6515FDED`。首次失败目录 `.tmp/r13-07-sdk9-ci-failure-repro-20261016` 与本次成功构建目录均已清理。

本轮修正后的 `python scripts/validate-source.py` 通过，`git diff --check` 通过。对修改的 `WorkspaceStateSourceTests.cs` 运行 R00/R01 freshness 后为 `14/14 FRESH / 0 STALE / 0 matched source paths`，`packageIdentityStatus=not-provided`。本机结果只证明该源码接线测试在 SDK9 定向执行成功，不覆盖完整 Playnite source group、WPF behavior suite 或 SDK8/10；推送提交的两个 SDK lane 必须到终态后才能签收，如再失败先取得完整日志再定位。

## 边界和下一步

仍未取得用户另一台机器的 R09 同次 HEAD/status、测试源、DLL/PDB 与 plugin DLL 身份；该失败的真实输入根因未确认。本轮 CI 的源码测试失败与那台机器的 R09 失败是不同问题。下一步提交此断言校正并监控新 Actions；R13-07 只有新 run 两条 lane 均成功后才收口。之后按账本核对已满足的 R13-08 证据，推进下一个未完成 Q/R 项。
