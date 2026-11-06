# U09-05 隔离宿主启动窗口复核

## 问题与范围

用户在上一次完整验收后仍看到黑色窗口闪现。此前只记录了启动完成后的前台窗口快照，这不能证明完整运行期间没有瞬时窗口。本批只调整验收启动器如何创建 Playnite 进程，不更改产品页面、命令、数据、安全语义或盘符。

`scripts/dev-install-run.ps1` 的启动代码从 `Start-Process` 改为 `System.Diagnostics.ProcessStartInfo`，明确设置 `UseShellExecute = $false`，让 GUI 子进程继承调用线程所在桌面；入口校验修订号同步更新为 `DEV-INSTALL-010`。`Test-LauncherExitCodes.ps1` 增加此启动方式的防回归检查。

受控桌面探针在非活动桌面 `GscGate_14ca4d1c4a2a4c10baa8f4c5fae2d5c6` 启动两种隐藏的 PowerShell 子进程。直接 `ProcessStartInfo` 与隐藏的 `Start-Process` 子进程都位于该隔离桌面，输入桌面始终是 `Default`，两者退出码均为 0。该结果验证了当前的进程桌面继承，但不能单独确定用户所见黑窗的唯一来源；完整中文根链仍需核对 Playnite 实际窗口所属桌面并检查运行期间的用户桌面状态。见 [直接子进程探针](desktop-inheritance.json) 与 [两种启动方式探针](desktop-launch-methods.json)。

## 本批已完成检查

- 两个修改后的 PowerShell 文件语法解析通过；`scripts/validate-source.py` 与 `git diff --check` 通过。
- 三个实际入口的退出码矩阵 9/9 通过（0、7、23 均正确传播），并覆盖不经 Shell 启动 Playnite 的源码守卫。见 [入口结果](launcher-exit-results.json)。
- 桌面继承探针两种方式均退出 0；输入桌面为 `Default`，未切换桌面。
- 一次直接 `dotnet test` 探针因仓库锁定 SDK 8.0.423、调用环境先选择 SDK 9.0.302 而未启动；按 `sdk-baseline.ps1` 选择 8.0.423 后，测试项目编译完成，但直接调用未携带 `GscBuildCommit`，TRX 中 4/8 项因测试身份守卫失败。原始 TRX 保留在 [未提交状态的定向尝试](uncommitted-focused-attempt.trx)，不计通过。没有更改身份门禁或测试断言；当前候选提交后由仓库入口带精确源码身份重跑。

本文件编写时，代码候选尚未提交；`U09-05` 继续进行中。尚未运行本候选的中文根完整链、独立 `scripts/build.cmd` 或对应 GitHub CI，也未作真实页面、键盘/IME、外部 UIA 或物理 DPI 验收。最终结论须以提交后的完整构建、安装身份、宿主窗口桌面及 CI 结果为准。

## 本阶段临时输出

[阶段起始空间与目录](stage-start.json)、[本阶段输出清单](stage-output-inventory.json) 和[清理记录](cleanup.json)记录本阶段自有 `.tmp` 输出。TRX 与探针 JSON 已校验哈希并归档；五个一次性目录经规范化路径、仓库 `.tmp` 边界、重解析点、独占文件打开和运行中 dotnet/testhost 检查后按字面路径清理，净释放 D 盘 954,368 bytes。测试尚未完成部分不计成功。
