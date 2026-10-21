# 2026-10-22 当前 main 构建与 R09 失败日志复核

## 身份

- 当前提交：`fcf787ccad480db4ace5962abf6a28ec00dcb9fa`（`main` / `origin/main`）。跟踪文件干净；既有 Round3 未跟踪 TRX 与 `tests/GameSaveCenter.Playnite.Tests/TestResults/` 保留。
- SDK `8.0.423`，Release，Windows x64。
- 完整构建/测试诊断：`artifacts/build-diagnostics/20261021T210446110Z-2eb6b36c/summary.json`；隔离输出：`artifacts/gsc-b/f7f3fdaf`。

## 用户所附失败日志

附件 run `20261021T135554848Z-c2927c39` 报告 WPF 类 `[65/116] R09FocusOutlineBehaviorTests`，并在旧版测试源码第 151 行断言描边厚度 `2`、实际为 `1`。其诊断目录当前不可用，无法恢复对应 checkout、测试 DLL 或 PDB 身份。

当前提交的全量测试清单有 140 个 WPF 隔离类；第 151 行现在检查错误态边框颜色，厚度断言位于后续行。当前构建中的 R09 位于 `[77/140]`，TRX `2/2` 通过。故附件错误不能归属于当前 main 的测试源码；由于旧二进制身份缺失，不推断其具体来自哪个 checkout。

## 当前提交完整构建与测试

从根目录执行 `GameSaveCenter-Run.cmd`，XAML `25/25`，Release solution `0 warning / 0 error`。诊断摘要记录 source commit 为当前 `fcf787cc`，SDK `8.0.423`，143 份 TRX 共 `1,904` 项：`1,836` 通过、`68` 跳过、`0` 失败/错误/超时/中止；所有 TRX 计数一致，140 个 WPF 隔离类全部完成。R09 `2/2` 通过。

源码编译、测试、Worker 发布、程序集身份校验、PEXT/ZIP 打包均通过。该次根目录命令最终退出码为 `1`，原因是沙箱拒绝写入默认 `%APPDATA%\Playnite\Extensions` 安装目录（`Access denied`）；不是编译或测试失败。该次不能记录为标准一键命令完整退出码 `0`。

## 安装与启动复核

在同一 source commit 上使用 PowerShell 7 提权运行 `scripts/dev-install-run.ps1 -Configuration Release -SkipTests`。测试没有在此重跑；上面的完整测试已在同一提交完成。此次 solution 编译 `0 warning / 0 error`，Worker 发布、ZIP/PEXT 打包和安装验证通过，清单版本 `0.6.73`，插件 DLL 版本 `0.6.73.0`。本次构建 DLL、打包目录 DLL 与已安装 DLL 的 SHA-256 均为 `5683CD242FC1EA1CBFEE15509D6A6F9C27A70BB965E892470FDA118D844DE81F`；Playnite PID `63560` 正在响应。

安装阶段诊断：`artifacts/build-diagnostics/20261021T225251016Z-d115411c/summary.json`；隔离输出：`artifacts/gsc-b/816b3f43`。另一次提权的全流程重跑在 R10 类 TRX 已记 `5/5` 后未能结束测试宿主，随后被中断；不把它当作通过或失败测试结果，也不取代上方完成的全量测试。

## 边界

未修改产品代码，也未更改或跳过完整测试运行中的任何用例。提权安装复核与完整测试是同一提交上的两个阶段；当前沙箱中没有取得单次根目录 `GameSaveCenter-Run.cmd` 的退出码 `0`，因为普通进程不能写入 Playnite 安装目录。
