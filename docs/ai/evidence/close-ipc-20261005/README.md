# CLOSE-IPC-01：真实管道取消竞态与夹具释放

2026-10-05；开发基线 `226bce6bf8b76c216cd7c5c4092adc74c59d20da`，SDK `8.0.423`，版本 `0.6.73`。本目录的 DLL 是提交前工作树构建，ProductVersion 保留基线提交；不能称为 clean HEAD 包。最终两个源码文件 SHA-256、插件/测试 DLL SHA-256 和 MVID 见 [身份记录](binary-identity.json)。

## 根因与受控复现

初始审阅的两个失败见 [原审阅证据](../completion-review-20261005/README.md)。原夹具全类共用一个管道名，连接使用占用线程池的同步等待，服务用 300/450/1000 ms 固定延时退出；断言失败又会跳过正常的服务等待。它允许服务关闭先于取消，并允许残留实例影响下一条测试。

先只修夹具：每条 xUnit 实例使用独立管道名，真正异步连接，接收请求/首字节后握手，服务保持打开直到取消断言结束；IAsyncLifetime 清理在异常路径也释放管道并等待服务退出。新增未连接和已连接的受控场景失败清理测试，随后用同一管道名的单实例服务证明资源已释放。

夹具修复后仍在连续专项第 5 轮复现生产失败（[TRX](ipc-5.trx)、[控制台](ipc-5-console.txt)）：调用方已经 Cancel，服务仍等待释放，却报告 WorkerRequestException。取消注册会关闭客户端管道；IO 操作可能先于取消 Task 被 WhenAny 观察到，从而以 IOException 或 EOF 完成。只处理 IOException/ObjectDisposedException 后，第 4 轮仍经 EOF → WorkerRequestException 路径失败（[TRX](final-ipc-4.trx)、[控制台](final-ipc-4-console.txt)）。这两次失败均保留，没有用单次绿灯覆盖。

最终仅在 linked token 已取消的管道故障/IOException/ObjectDisposedException 路径按既有 host → caller → timeout 优先级分类。请求 ID 和 MayHaveBeenAccepted 不变，不增加重放次数，不把取消解释为业务回滚。新增服务先关闭、等待故障返回后才取消的负例，仍精确要求 PipeDisconnected 和未知提交标记；重放正例和取消重放均核对两次请求与异常使用相同 RequestId。真实服务拒绝不被取消分类覆盖。

## 当前验证

- 最终修复 DLL 的 20 次独立专项：每次 `10/10`，累计 `200 passed / 0 failed / 0 skipped`。完整逐轮 counters 见 [ipc-repetitions.json](ipc-repetitions.json)，最后一轮 [TRX](verified-ipc-20.trx)。
- 完整 Playnite source：112 类，`470 passed / 0 failed / 18 skipped`，488 total，exit 0；[TRX](source.trx)、[控制台](source-console.txt)。18 条是原有 skip，未新增或将失败转 skip。
- Release solution restore/build、XAML `24/24`，编译 `0 warnings / 0 errors`；[控制台](verified-build-console.txt)。Core 在同一最终输出的全量入口 `125/125`。
- Worker 全量 `357/357`，exit 0，运行约 16 分钟；[TRX](worker.trx)、[逐条控制台](worker-console.txt)。此前两次旧修复身份以及一次无逐条诊断的 Worker 运行已停止，不能列为通过；仅终止命令行确认属于本阶段 `.tmp/close-ipc-20261005` 的 testhost，不操作 Playnite 或用户 Worker。
- WPF 原脚本计划 113 类：前 23 类通过，第 24 类 Q14ToolbarAlignmentBehaviorTests 失败，后 89 类未执行，exit 1；[原脚本控制台](playnite-console.txt)。随后尝试的重复直接全量入口主动停止，不能记为通过；直接 VSTest 同 DLL Q14 复核仍 `0 passed / 1 failed`，[TRX](q14-failure-recheck.trx)、[控制台](q14-console.txt)。具体为 Light / 654 DIP / 单行的七个预设控件仍有 20 DIP bottom margin，期望恢复作者 0 DIP。独立登记 CLOSE-WRAP-01，不把 IPC 完成升级为全量门禁通过。
- source validator、git diff --check 通过；git fsck --full exit 0，仅既有 dangling 对象，无损坏。源码结构结果见 [source-validation.txt](source-validation.txt)。
- 提交前 package.ps1 正确拒绝 dirty working tree；正式打包需要提交后的 clean 身份，不能绕过门禁。

控制台/TRX 已替换仓库和用户目录，不提交二进制、临时库或 dump。测试均使用本机隔离管道及可丢弃业务夹具，没有连接生产固定管道、安装用户扩展、操作真实存档/媒体/云端。

## 独立 CI 诊断

本轮 `gh run view --log-failed` 成功取得最新 [run 37329446647](https://github.com/Nikilua/GameSaveCenter/actions/runs/37329446647)，源码 `226bce6b`。这是新证据，不表示旧 run 36677346317 的 403 记录不真实。

[脱敏完整失败步骤日志](ci-log-37329446647.txt) 确认实际 SDK `10.0.401`，Core 125、Worker 357 通过；失败发生于 ProductionShellChromeSourceTests 的 `ReducedMotionCancelsAnActiveSidebarTransitionWithoutLateClockWrites`（234 行）和 `SidebarTransitionReleasesClocksOnCompletionAndUnloadInAnActualWpfWindow`（319 行），断言均为活动动画应存在。COM 清理异常出现在这些明确断言之后，不替代根因定位。该问题与 IPC 分类竞态分账，后续 CLOSE-CI-01/CLOSE-SDK-01 继续处理。

真实 Playnite、物理 DPI、系统动画设置切换、Media Inbox 同进程滚动与用户 Q06/R08 原故障未在本任务验收；当前候选 CI、证据 freshness 和正式发布矩阵均不能从这些 IPC 成绩推断通过。
