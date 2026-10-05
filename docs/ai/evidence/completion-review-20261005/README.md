# 2026-10-04/05 完成度审阅证据

审阅基线：`77de7f450431d267026ce2169aa30525b18bf528`，main，0.6.73。本轮为文档/任务/工程状态审阅，不修改生产代码或版本，不安装用户扩展，不启动真实 Playnite。

## 实际执行结果

| 检查 | 结果 | 证据与限制 |
| --- | --- | --- |
| Git | fetch 后 main 与 origin/main 在审阅基线一致，初始工作树干净 | 后续文档提交不改变 src/tests/scripts/.github/global.json 的内容 |
| 本机 SDK | 8.0.423 | 不沿用历史本机9.0.302或runner10.0.401的身份 |
| XAML / Release | 24/24；0 warnings / 0 errors | `build-console.txt`，构建时尚未修改文档 |
| Core | 125 passed / 0 failed / 0 skipped | 完整 build.ps1 执行 |
| Worker | 357 passed / 0 failed / 0 skipped | 完整 build.ps1 执行；本轮没有因环境跳过进程测试 |
| Playnite source | 465 passed / 2 failed / 18 skipped，485 total | 全脚本退出1，后续113类WPF阶段未执行，不称全量通过 |
| 同DLL IPC专项 | 7 passed / 0 failed / 0 skipped | `ipc-recheck.trx`、`ipc-recheck-console.txt`；一次复跑通过不能关闭首次失败 |
| R00/R01 freshness | 9 fresh / 5 stale，package not-provided | `freshness.json`；不重写baseline |
| 最新已核查CI | run36677346317 / 77de7f45，编译与测试失败 | `ci-run-36677346317.json`；失败后render/static-WPF/package/upload均skipped；日志下载403 |
| source结构校验 | 审阅前通过；编辑后结果记录于`source-validation.txt` | 静态检查不替代真实WPF或宿主 |

执行入口：`scripts/build.ps1 -Configuration Release -OutputRoot .tmp/completion-review-20261004/build`。首个沙箱运行因常规NuGet.Config读取拒绝在restore前停止；按批准的权限重跑后restore和编译成功，随后发生上表测试失败。NuGet权限与测试断言是不同问题。

同DLL复核使用 `dotnet vstest <该隔离输出的net472测试DLL> /TestCaseFilter:FullyQualifiedName~WorkerIpcClientBehaviorTests /Logger:trx;LogFileName=ipc-recheck.trx`。二进制SHA-256、MVID和ProductVersion见 [binary-identity.json](binary-identity.json)。没有更换DLL、修改测试或将失败改成skip。

## 本轮新发现的两条失败

1. `WorkerIpcClientBehaviorTests.CallerCancellationDuringReplayWaitStopsWithAmbiguousOutcome`：预期 `WorkerIpcCancellationException`，实际 `WorkerRequestException`，消息为“Worker 管道已断开；请求可能已提交，不能据此判断业务回滚。”栈指向测试203行、`WorkerIpcClient.RequestWithTrackingAsync` 241行。
2. `WorkerIpcClientBehaviorTests.CancellationDuringLargeWriteIsReportedAsAmbiguousAndIsNotRetried`：`System.IO.IOException`“所有的管道范例都在使用中”，栈指向测试夹具 `RunServerAsync` 278行、等待信号251行和测试228行。

源码观察：该类使用同一测试管道名；取消测试包含定时关闭服务和异步信号；前一断言失败后可能跳过正常等待清理。**这是需验证的夹具/时序假设，不是已确认根因。** 同DLL专项7/7说明本轮尚未稳定复现；`CLOSE-IPC-01`保持READY。不能把这两条失败直接归因于SDK8或当作远端SDK10 CI根因。

## 完成度统计与证据新鲜度

[ledger-snapshot.json](ledger-snapshot.json) 保存审阅前R/Q文件哈希、原始状态计数、R逐ID分类与四份历史快照的原始字节哈希。统计只取正式任务表，排除表前三列R13/R14摘要；R22-01/R23-06的历史行少一列，仍按前3列ID/任务/状态纳入，不丢失任务。

- Round3：105受控满足、83环境待验、1用户失败开放、1外部阻塞、1部分满足、1不适用，共192。旧106受控分组包含R08-01，本次单列，**没有改变原行状态或声称新增验收**。
- Round2：208唯一ID，最终5已验收/203未完成。详细分列计数在JSON和完成度评估；Q/R不相加为产品百分比。
- freshness：R00-04/06/07/08、R01-05受9月30日shell/Media/Trainer源变更影响，仍stale。其他9项fresh只代表登记路径未变化，不代表重新执行或真实宿主通过。

`freshness.json` 为脚本报告的精简投影：保留14条记录的来源身份、命中路径和全部判定，把大量未命中路径列表替换为数量，并记录原报告SHA-256；没有删除失败条目或改变判定。复核命令为 `scripts/check-ui-evidence-freshness.ps1 -HeadCommit 77de7f450431d267026ce2169aa30525b18bf528`。

## 文档与任务变更范围

重写完成度评估；CURRENT_STATE/PROJECT_MEMORY/HANDOFF改为短入口并保留同目录完整历史快照；WORKLOG记录本轮；活动队列增加具体收口任务；旧治理规则的push禁令按当前AGENTS修正；Q/R只增加统计口径说明和修复Markdown列格式，不签收任务。

历史文件在复制时与原文件字节一致，Git规范化后的内容也应保持一致。历史原文可能已有过期链接或旧“下一项”，本轮不批量改写历史证据。新入口/本轮新增链接单独校验；不将历史全仓链接问题冒充本轮已全部解决。

本轮文档校验结果见 [documentation-validation.json](documentation-validation.json)：四份归档完整、R/Q唯一ID和原状态计数保持、任务表列数正确、新增本地链接可达、TRX/JSON可解析且机器身份已脱敏。`git diff --check`通过。临时构建及一次性审阅脚本在证据归档后清理，旧的非本轮产物未盲删。

## 后续边界

真实Playnite/物理DPI/跨屏/OS输入/读屏/presented frame未在本轮执行，Media Inbox用户问题和Q06/R08原始失败仍未解决；没有进行新候选安装、真实恢复或云端写入。当前归档用于说明完成度与下一步，不能替代发布验收。
