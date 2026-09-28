# R08 one-click `1/2` 失败报告复核：`8e4c32a7`

日期：2026-09-29。用户再次报告一键构建中 `R08MotionReverseBehaviorTests` 为 `1 passed / 1 failed / 2 total`。其消息未包含失败测试名、断言消息、堆栈或 TRX。本机 `artifacts/one-click-install.log` 最后修改时间是 2026-09-24，属于另一轮宿主审计，不能作为该次失败日志。

## 本机精确身份复跑

- 代码身份：`8e4c32a7ffc65538f28ac7411ef88b286c4ef94e`。
- Release solution 构建成功：`0 warning / 0 error`，XAML 检查 `24/24`。Playnite 生产 DLL `0.6.73+8e4c32a7ffc65538f28ac7411ef88b286c4ef94e`，SHA-256 `444150691B412C6B6F0CD671DEC7D887EA33342760AB3667F555678EA3FB4DDC`；Playnite 测试程序集 SHA-256 `8DDAEE378AEF7A46ED0F68DC8C5A61E2F2625DA7218FDA534FA0E4B2BF8555C6`。
- 在该身份的隔离 Release 输出中，连续启动 12 个互相独立的串行 VSTest 进程；每个进程只运行 `R08MotionReverseBehaviorTests`，使用 `--no-build --no-restore` 并分别保存详细 console 与 TRX。12 轮均为 `2 passed / 0 failed / 0 skipped`，所有进程 exit `0`，总计 `24/24`。每轮 TRX：[`01`](R08-01.trx)、[`02`](R08-02.trx)、[`03`](R08-03.trx)、[`04`](R08-04.trx)、[`05`](R08-05.trx)、[`06`](R08-06.trx)、[`07`](R08-07.trx)、[`08`](R08-08.trx)、[`09`](R08-09.trx)、[`10`](R08-10.trx)、[`11`](R08-11.trx)、[`12`](R08-12.trx)。
- 原始 console log：[`01`](R08-01-console.log)、[`02`](R08-02-console.log)、[`03`](R08-03-console.log)、[`04`](R08-04-console.log)、[`05`](R08-05-console.log)、[`06`](R08-06-console.log)、[`07`](R08-07-console.log)、[`08`](R08-08-console.log)、[`09`](R08-09-console.log)、[`10`](R08-10-console.log)、[`11`](R08-11-console.log)、[`12`](R08-12-console.log)。
- 每轮 console 都在 xUnit 完成后打印 `TextServicesHost.OnUnregisterTextStore InvalidComObjectException` 清理异常；同一日志随后明确显示 `测试运行成功`，TRX 也都是 `2/2`。异常根因未知，记录为 testhost 清理噪声，不把它解释成动画失败或修复。
- 用例分别是 `TranslateReversalStartsAtRenderedValueAndFinishesAtLatestTarget` 与 `SidebarRapidReversalUsesLatestTargetAndReleasesOldClock`。本阶段没有修改动画生产代码或 R08 测试，也没有放宽断言。

## 结论与待补证

本机在当前精确身份上未复现用户报告。12 轮通过只能说明该构建在这台机器的独立 testhost 下连续通过，不能说明另一台机器的失败已解决，也无法从计数推断失败断言。R08 报告保持打开。

要继续定位，需要失败机器对应的 `artifacts/one-click-install.log` 完整 R08 测试段或该轮 TRX，至少包含具体失败方法、错误消息/堆栈以及构建身份。真实 Playnite 未启动，本复核不是宿主动画或呈现验收。
