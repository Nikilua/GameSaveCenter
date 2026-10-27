# 标准入口完整验收：90bf0405

2026-10-27 从仓库根目录原样运行 `cmd.exe /d /c "GameSaveCenter-一键构建安装运行.cmd < nul"`，外层退出码 **0**。本轮产品源码固定为 `90bf040567f013f3a8d9ed8667028a4edb2ec524`；build run `20261027T010319835Z-7b052459`、install run `20261027T010317740Z-96e7b91b`、package run `20261027T012406316Z-c7e3272f`。

## 阶段结果

- global.json 指定的 SDK `8.0.423`，Release 解决方案编译 0 warning/0 error，XAML 25/25。
- Core 157/157；Worker 400/400；Playnite 源码组 544 通过、18 跳过；WPF 隔离组 809 通过、40 跳过。总计 1968 项，1910 通过、58 跳过、0 失败/错误/超时/中止；149 份 TRX 计数全部一致，146 个 WPF 类完成。
- 58 个跳过用例名称与 b05e5696 的完整链相同：一个真实系统指针用例在发现期默认跳过，57 个既有环境/架构 skip 保持原状态，均未算作通过。
- Worker `win-x64` 发布成功。ZIP 与 PEXT 都是 45,728,328 字节，SHA-256 为 `D5A4970893497134188308978E111897FA3AB8E0F9AB074EA451B26CB4A48F6A`。
- 安装目录版本为 `0.6.73` / DLL `0.6.73.0`；六个程序集的完整 `ProductVersion` 和 SHA-256 见 [installed-assemblies.json](installed-assemblies.json)。包身份摘要见 [package-ownership.json](package-ownership.json)。
- Playnite PID 39152 在 09:24:11 启动，Worker PID 42800 在 09:24:21 从安装扩展目录启动。09:25 的 `extensions.log` 记录插件从安装目录加载，`ProductVersion` 与提交身份一致。没有触发备份、还原、媒体或云端业务操作。

## Worker 清理失败及复核

同一源码身份第一次完整根入口在 Worker 399/400 处退出 1；原始失败位于 `NotificationRecordPersistenceTests.Dispose()` 删除该测试的 SQLite 临时目录时，Windows 报告 `gamesavecenter.db` 正被使用。原始结果保存在 [失败运行摘要](initial-failed-run-summary.json)、[失败 TRX](tests/initial-worker-cleanup-failure.trx)、[根构建日志](initial-worker-cleanup-build-console.txt) 和 [Worker 日志](initial-worker-cleanup-worker-console.txt)。

我用同一 SDK、源码身份和预编译测试程序集复跑：这个用例单测 1/1，通过；完整 Worker 项目 400/400，通过；随后最终标准根入口整链再次 0 退出，Worker 400/400。失败未被忽略或改写，复测 TRX 分别为 [定向复测](tests/targeted-worker-recheck.trx)、[完整 Worker 复测](tests/full-worker-recheck.trx) 和 [最终整链 Worker 结果](tests/standard-root-worker.trx)。源码没有在失败与成功运行之间更改，因此没有加入无证据的产品/测试改动。

## 早期验证尝试

RenderHarness首轮报告只在无关侧栏快速切换settle探针报错；[首轮报告](render-qa-first-attempt-failure.txt)和成功重跑均保留。早期一次隔离测试使用了错误的 `BaseOutputPath`，另一次直接调用漏传仓库身份变量并运行到当时的候选表增长断言；完整尝试顺序与失败原因记于[阶段归属/分诊记录](stage-ownership-and-triage.txt)，原始TRX见[早期复核](tests/early-identity-guard-and-candidate-growth-recheck.trx)。该候选表断言已从代码撤回，后续验证经仓库标准入口身份保护运行；这些无效/失败尝试不计入最终通过结果。

## 保留的原始输出

- 本证据目录内Markdown、JSON、TXT和TRX已统一为LF后计算SHA-256，逐文件清单见 [archive-sha256.json](archive-sha256.json)。截图保持PNG原样。
- 标准 run 摘要： [standard-run-summary.json](standard-run-summary.json)
- 受影响 WPF 类的最终 TRX： [CompactWorkspaceTableViewportBehaviorTests](tests/standard-root-compact-viewport.trx)
- 最终标准构建的 149 份 TRX、完整构建摘要及安装/启动日志仍保留在本机 `artifacts/build-diagnostics/20261027T010319835Z-7b052459` 与 `artifacts/one-click-install.log`；当前构建输出留在 `artifacts/gsc-b/48144df5`。
- 4K RenderHarness 的完整输出和代表截图见同层 [render-qa-report.txt](render-qa-report.txt) 与 [Save-3840x2160-tab0.png](Save-3840x2160-tab0.png)。这是合成内容、逻辑 DPI 的离屏结果；另一台物理 4K 电脑尚未复核。
- 首轮 RenderHarness 完整报告保留于 [首轮失败报告](render-qa-first-attempt-failure.txt)：仅侧栏快速切换 settle 探针失败；重跑报告无 `PROBLEM`。
- 入口退出码转交测试为 9/9：三个入口均原样返回注入的 0、7、23。代码未改动；见 [T00-02 results.json](../../../build-ui-reliability-20261022/evidence/T00-02/results.json)。
- 本轮自有临时目录/首轮失败输出的删除清单、归属/边界检查和前后卷空间见 [cleanup.json](cleanup.json)。

状态：本机候选完整标准链通过；用户另一台 4K 电脑上的物理 DPI / Playnite 实机确认仍待用户回传。不要将本证据解读成外机实测。
