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

## 校正提交的完整 CI 收口

修正提交 `e30a29767a903cfe42d16885dc5ed15c3885f05c` 对应 Actions [#1046 / run `38902753483`](https://github.com/Nikilua/GameSaveCenter/actions/runs/38902753483) 已终态成功：

- SDK `8.0.423` job `116978745271` 成功。完整 Release 编译/测试显示 Core `125/125`、Worker `362/362`；Playnite 隔离入口 `source classes 107 / WPF classes 125`，全部类通过，其中 R09 焦点行为类位于第 `68/125` 类。离屏 `render-qa OK`、WPF 静态审查、Playnite 扩展包生成、自包含 Worker 发布、上传与诊断归档步骤均成功。
- SDK `10.0.401` job `116978744884` 成功。Core `125/125`、Worker `362/362`；Playnite 同样为 `107` 个 source 类与 `125` 个 WPF 类，日志确认所有 Playnite 测试类通过。
- SDK8 打包脚本会再次构建并执行测试；这次打包阶段的复建/测试也通过，最终生成 `GameSaveCenter-0.6.73-playnite.zip` 与 `GameSaveCenter-0.6.73.pext`。这些 CI 包在 GitHub runner 上生成，不是用户机安装验证。

这两条 CI lane 关闭了校正后源码测试的 SDK 门禁，也证明当前提交的 R09 测试在两种 CI SDK 下通过；它们不等于另一台机器的 R09 失败已定位或消失。

## 边界和下一步

用户另一台机器的日志确认 R09 `1/2`、总流程 exit `1`。line 151 映射到 `Thickness(2/1)`，而当前测试源 line 151 是 Brush 颜色比较、历史 `3f7d0b30` line 151 才是 Chrome 厚度断言；因此外机测试代码或 PDB 行映射与当前源不匹配，具体是旧测试 DLL、PDB、checkout 还是脚本输入待同次 summary/TRX/程序集身份确认。本机和 #1046 父提交 CI 通过不覆盖外机失败。R13-08 真实生产 MaintenanceView 的已知/未知帮助与诊断展开行为补证见[R13-08 复核包](R13-08-CURRENT-MAIN-FAILURE-HELP-RECHECK-20261016/README.md)；提交推送后必须监控两条新 SDK Actions 到终态。真实 Playnite/package-host、OS UIA/IME、物理 DPI/跨屏、最终呈现帧、ETW 和宿主性能仍待验。
