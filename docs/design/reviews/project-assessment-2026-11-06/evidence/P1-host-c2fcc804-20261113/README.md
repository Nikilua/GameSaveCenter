# P1 隔离宿主验收：c2fcc804（2026-11-13）

## 项目与状态

- 状态：隔离安装身份与 Playnite 进程启动验证完成；独立 `scripts/build.cmd` 仍受 Windows 输入桌面探针阻塞。没有把首次启动向导等同于插件运行态验收。
- 计划去重：本候选是验收基础设施，不是抽查报告中16项已确认缺陷。A05/A06/A07/A08/A11原有范围按此前证据复用；本轮没有扩大为全面重构。
- 当前入口：`GameSaveCenter-一键构建安装运行.cmd` → `GameSaveCenter-Run.cmd` → `scripts/dev-install-run.ps1`。默认行为指向现有Playnite目录；隔离入口把扩展、用户数据、Worker数据和IPC限制到仓库`.tmp/`的新profile。
- 实现候选：`c2fcc80477b936bafbaf46e983c266365480fb5c`（提交：`隔离一键安装启动验收环境`）；候选入口/缺口/收益在实现前记录于 `56d3f30e17d8d5924710ed8fc62c7c31fd3c4cfd`。

## 本机中文根入口

- SDK `8.0.423`，Release；源码SHA与安装程序集身份均为 `c2fcc80477b936bafbaf46e983c266365480fb5c`。
- `GameSaveCenter-一键构建安装运行.cmd` → `GameSaveCenter-Run.cmd` 成功分支退出码 `0`，launcher输出 `[OK] Build, verification, installation and Playnite startup completed.`。外层日志采集器仍等待已启动Playnite继承的输出句柄关闭，未生成自己的exit JSON；这不改变根入口成功分支的退出码证据。
- 根入口诊断的160份TRX共2,042项：1,984通过、58个 `NotExecuted`、0失败；58项不计为通过。157/157个WPF隔离测试类运行完成。诊断汇总：`artifacts/build-diagnostics/20261113T031639919Z-ad059a2a/summary.json`。
- 扩展版本`0.6.73`；安装包zip/pext各45,740,284 bytes，SHA-256均为`CDB9FC558EFB1E673AB497D46C39DBC8B181E2752017B6685BA5DF1AF4AAF4A2`。包位于本地`artifacts/p1-candidate-c2fcc804-20261113/`，避免替换被历史文档引用的`52a57db`包。
- 安装目录为隔离profile下的`Extensions/GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec`。Playnite进程PID `33116` 的参数含 `--userdatadir .tmp/p1-host-playnite-c2fcc804`；Worker数据路径和IPC名称均为隔离值，子进程环境显式设 `GameSaveCenter__EnableCloudUpload=false`。
- Playnite显示首次配置向导，日志之后记录应用启动；没有观察到GameSaveCenter Worker进程，因此插件运行态、真实游戏库、UI Automation和业务命令未验。没有读取真实配置/数据库/存档，也没有执行恢复、删除或云端写入。Playnite更新检查为只读网络请求并超时。
- 本机安装报告：`artifacts/p1-host-root-c2fcc804.txt`；Transcript当前实际写入`artifacts/one-click-install.log`；launcher输出`artifacts/p1-host-root-c2fcc804-launcher-console.log`。定向测试原始日志已移到`artifacts/p1-candidate-c2fcc804-20261113/targeted-tests/`。

## 独立 `scripts/build.cmd`

- 同一冻结SHA、SDK `8.0.423`、Release；没有设置`SkipTests`。解决方案编译0 warning / 0 error。
- 命令退出码`1`，失败不是测试断言：WPF隔离到第41/157类后，`run-playnite-tests-isolated.ps1:282` 的 `InputDesktop()` 抛出 `Could not inspect the input desktop.`。
- 44份已生成TRX共1,351项：1,333通过、18个 `NotExecuted`、0失败；Core 157/157、Worker 407/407、Playnite source 557通过/18未执行、WPF已运行41类/212项全通过。其余WPF类未运行，不以已完成类替代完整脚本验证。
- 原始失败与TRX保留在`artifacts/p1-scripts-build-c2fcc804-console.log`、`artifacts/p1-scripts-build-c2fcc804-exit.json`及`artifacts/build-diagnostics/p1-scripts-build-c2fcc804/20261113T045021023Z-7e121647/`。不关闭桌面探针，不重跑追绿；待Windows输入桌面可被读取后再单独复验。

## GitHub与后续

- 精确源码提交run #1551 / `43356307416`：基线SDK job `131659060201` 15/15 success；SDK `10.0.401` job `131659060720` 8/8 success。artifact ID、大小、digest见[`ci-run-43356307416.json`](ci-run-43356307416.json)。此前功能候选记录提交run #1550 / `43355362103` 两job也均success，见[`ci-run-43355362103.json`](ci-run-43355362103.json)。
- required-checks/ruleset名单此前HTTP 403；权限未变化，本轮未重复请求。已运行job通过与required-check名单未验证是两个事实。
- 下一步：保留隔离profile作为本次启动证据；输入桌面可读取后，仅复跑独立 `scripts/build.cmd`。不启动真实profile，不做真实恢复、删除或云端写入。

## 清理记录

本轮23份定向通过日志已逐文件SHA核对后从`.tmp/`移入`artifacts/p1-candidate-c2fcc804-20261113/`；此前23/20份小型测试与启动尝试目录通过同卷`Move-Item`整体保留在`prior-attempts/`。历史`52a57db`包和`last-dev-install.txt`恢复到原canonical路径；c2包单独保留在候选目录，未复制大包。

构建/测试临时目录已核对仓库边界、当前用户归属、无重解析点、无文件锁、无运行测试进程及文档引用，并请求使用指定PowerShell命令清理；执行策略拒绝`Remove-Item -LiteralPath -Recurse -Force`（`blocked by policy`），因此未删除这些目录，也没有换用其他删除方式。完整路径、字节数、拒绝结果和前后C/D空间见[`cleanup.json`](cleanup.json)。当前临时Playnite profile保留为启动审计证据；其中生成配置未读取或清理。
