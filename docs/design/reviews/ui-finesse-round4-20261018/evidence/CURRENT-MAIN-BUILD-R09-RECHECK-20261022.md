# 2026-10-22 当前 main 构建与 R09 失败日志复核

## 最新标准入口复验（2026-10-22）

- 当前源码提交：`162170a6dc41d7a1148e0f3a1ea2fbae49c369e6`；SDK `8.0.423`、Release、Windows x64。直接从仓库根目录执行 `cmd.exe /d /c "GameSaveCenter-Run.cmd < NUL"`，工具返回退出码 `0`。诊断摘要：`artifacts/build-diagnostics/20261021T225909667Z-dc4f8e95/summary.json`；完整标准入口输出：`artifacts/one-click-install.log`。
- XAML `25/25`，Release solution `0 warning / 0 error`；Core `157/157`，Worker `400/400`，Playnite source `567` 项（`549` pass、`18` skip），WPF 隔离 `780` 项（`741` pass、`39` skip；`140/140` 类完成）。全量 `143` 份 TRX 合计 `1,904` 项（`1,847` pass、`57` skip、`0` fail/error/timeout/abort），报告计数一致，`counterMismatchCount=0`。57 个 skip 都带有同一明确原因：断言针对已撤销的“今日工作台”UI 架构，当前生产页已恢复 AcrylicFork 基线；没有环境权限 skip。R09 `2/2` 通过，TRX SHA-256：`2F15A331DCC66E828023C9AA5B61809389FF464BF740729600270B9BA37402DB`。
- Worker 发布、二进制构建身份核对、PEXT/ZIP 打包、扩展安装验证与 Playnite 启动均通过。安装清单 `0.6.73`、DLL `0.6.73.0`；ZIP 与 PEXT SHA-256 均为 `2478E354F4029F635DE84DA8C9B6713487CD253FAED90D041F986C7005F9D486`；构建 DLL 与已安装 DLL SHA-256 均为 `D26EC254B8E8173F97101FB1E2E1BA2E1A86885CDBB135DAA95D4C83110353F7`。Playnite PID `26040` 正在响应。
- 第 16 个 WPF 类 `MediaThumbnailConverterTests` 的 TRX 完整记录 `1/1` pass；console 同时出现 WPF `TextServicesContext` 关闭异常，测试宿主随后保持存活，导致输出管道暂未关闭。核对父进程已退出且 TRX 所有测试完成后，终止了这个孤立 testhost；隔离器随即记录该步骤退出码 `0` 并继续。没有跳过、取消或中断测试；汇总仍为 `140/140` 类和 `0` 失败。该清理事实保留在完整 console/摘要中。
- 摘要的 `workingTreeClean=false` 来自先前用户保留的 Round3 未跟踪 TRX 与 `tests/GameSaveCenter.Playnite.Tests/TestResults/`；本次没有修改、暂存或清理这些内容。所有已跟踪源码均属于提交 `162170a6`。结束后可用空间约为 D: `17,503,531,008` bytes、C: `1,176,997,888` bytes；本次没有手动删除 artifacts/.tmp 输出，当前诊断、隔离构建目录与安装包均保留。

以下小节记录附件旧失败及较早的 `fcf787cc` 沙箱安装尝试；它们已由上述当前提交完整标准入口复验补充，不代表最新结果。

## 历史身份：`fcf787cc` 复核

- 当时的复核提交：`fcf787ccad480db4ace5962abf6a28ec00dcb9fa`（`main` / `origin/main`）。跟踪文件干净；既有 Round3 未跟踪 TRX 与 `tests/GameSaveCenter.Playnite.Tests/TestResults/` 保留。
- SDK `8.0.423`，Release，Windows x64。
- 完整构建/测试诊断：`artifacts/build-diagnostics/20261021T210446110Z-2eb6b36c/summary.json`；隔离输出：`artifacts/gsc-b/f7f3fdaf`。

## 用户所附失败日志

附件 run `20261021T135554848Z-c2927c39` 报告 WPF 类 `[65/116] R09FocusOutlineBehaviorTests`，并在旧版测试源码第 151 行断言描边厚度 `2`、实际为 `1`。其诊断目录当前不可用，无法恢复对应 checkout、测试 DLL 或 PDB 身份。

该次提交的全量测试清单有 140 个 WPF 隔离类；第 151 行检查错误态边框颜色，厚度断言位于后续行。该次构建中的 R09 位于 `[77/140]`，TRX `2/2` 通过。故附件错误不能归属于当时 main 的测试源码；由于旧二进制身份缺失，不推断其具体来自哪个 checkout。

## 历史 run：`fcf787cc` 构建与测试

从根目录执行 `GameSaveCenter-Run.cmd`，XAML `25/25`，Release solution `0 warning / 0 error`。诊断摘要记录 source commit 为 `fcf787cc`，SDK `8.0.423`，143 份 TRX 共 `1,904` 项：`1,836` 通过、`68` 跳过、`0` 失败/错误/超时/中止；所有 TRX 计数一致，140 个 WPF 隔离类全部完成。R09 `2/2` 通过。

源码编译、测试、Worker 发布、程序集身份校验、PEXT/ZIP 打包均通过。该次根目录命令最终退出码为 `1`，原因是沙箱拒绝写入默认 `%APPDATA%\Playnite\Extensions` 安装目录（`Access denied`）；不是编译或测试失败。该次不能记录为标准一键命令完整退出码 `0`。

## 历史安装与启动复核

在同一 source commit 上使用 PowerShell 7 提权运行 `scripts/dev-install-run.ps1 -Configuration Release -SkipTests`。测试没有在此重跑；上面的完整测试已在同一提交完成。此次 solution 编译 `0 warning / 0 error`，Worker 发布、ZIP/PEXT 打包和安装验证通过，清单版本 `0.6.73`，插件 DLL 版本 `0.6.73.0`。本次构建 DLL、打包目录 DLL 与已安装 DLL 的 SHA-256 均为 `5683CD242FC1EA1CBFEE15509D6A6F9C27A70BB965E892470FDA118D844DE81F`；Playnite PID `63560` 正在响应。

安装阶段诊断：`artifacts/build-diagnostics/20261021T225251016Z-d115411c/summary.json`；隔离输出：`artifacts/gsc-b/816b3f43`。另一次提权的全流程重跑在 R10 类 TRX 已记 `5/5` 后未能结束测试宿主，随后被中断；不把它当作通过或失败测试结果，也不取代上方完成的全量测试。

## 历史边界

旧 `fcf787cc` 的沙箱尝试在安装写入处退出 `1`；最新 `162170a6` 根目录标准入口已完成同一全链并退出 `0`。附件所示的 `2→1` 描边失败没有在当前 R09 复现；旧 DLL/PDB 输入身份与根因仍未知。
