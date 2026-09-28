# R08 反向动效失败报告后的当前身份复核（2026-09-29）

## 用户提供的失败

用户报告一键构建中 `R08MotionReverseBehaviorTests` 为 `1 passed / 1 failed / 2 total`，整类耗时约 9 秒。消息没有失败方法、断言堆栈、TRX 或失败构建身份。本 checkout 的 `artifacts/one-click-install.log` 最后修改于 2026-09-24，内容是另一轮真实宿主审计，没有这次测试输出，因此不能用它推断失败原因。

## 当前 main 复核

- 源码身份：`c50de56ae9d54995e5bc4834c6bf6d664097bb07`。
- 命令：`scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot .tmp/r08-motion-failure-20260929/build`；随后从该隔离输出串行启动 8 个新的 VSTest 进程，每次只运行 `R08MotionReverseBehaviorTests`，附 `console;verbosity=normal` 和独立 TRX。构建身份由仓库脚本从当前 HEAD 注入；测试均 `--no-build --no-restore`，没有并行共用 WPF testhost。
- Release solution/XAML 构建退出 `0`，XAML `24/24`，solution `0 warning / 0 error`。隔离测试程序集 SHA-256：`39FF75E6744AEC3748BAD7243B0F9D56DA9D8D9250140141655D77F170320811`。
- 8 个独立进程均为 `2 passed / 0 failed / 0 skipped`，共 `16/16` 测试执行、全部进程退出 `0`。每轮 TRX：[`01`](R08-01.trx)、[`02`](R08-02.trx)、[`03`](R08-03.trx)、[`04`](R08-04.trx)、[`05`](R08-05.trx)、[`06`](R08-06.trx)、[`07`](R08-07.trx)、[`08`](R08-08.trx)。
- 每轮 console 都出现 WPF `TextServicesHost.OnUnregisterTextStore InvalidComObjectException` 清理输出；对应 xUnit/VSTest 结果仍明确 `2/2`，exit `0`。异常根因未知，单独记作退出噪声，不归为测试失败或动画通过证据。

## 解释与边界

此次本机复跑没有重现用户失败，也没有失败断言可用于进一步定位。既有修正只调整反向动画测试的有界 dispatcher 采样，没有修改生产动画；当前的 8 次通过不能证明用户那次失败一定是同一采样问题。要核实该失败具体原因仍需那台机器完整的 `artifacts/one-click-install.log` 或对应 TRX。

此处测试使用隔离 STA WPF testhost、合成点击和逻辑 DIP，不代表真实 Playnite 动画输入或屏幕呈现。未启动 Playnite，也未操作用户数据。

## 后续一键构建报告与最新身份复核

用户再次提供 `R08MotionReverseBehaviorTests` 为 `1 passed / 1 failed / 2 total` 的一键构建摘要，但没有失败方法、断言、堆栈或 TRX。本 checkout 的 `artifacts/one-click-install.log` 修改时间仍为 2026-09-24，内容属于另一轮宿主审计，不能定位本次失败。

- 在提交 `256a40f2` 的隔离 Release solution build 后，单独运行 `R08MotionReverseBehaviorTests`：`2/2`、0 failed/skipped、VSTest exit `0`；TRX 为 [`R08-256a40f2.trx`](R08-256a40f2.trx)。
- 该次退出仍输出 `TextServicesHost.OnUnregisterTextStore InvalidComObjectException`；xUnit/VSTest 明确成功，根因未知。此文本仅记录为清理噪声，不视为业务失败，也不代表已解决用户的间歇失败。
- 用户失败仍未复现，失败方法与该提交身份未能在用户机器上核对。不能据本地单轮通过宣称已定位或根治；如再次发生，需要完整失败段或相应 TRX 来确定是哪一个动画断言。
