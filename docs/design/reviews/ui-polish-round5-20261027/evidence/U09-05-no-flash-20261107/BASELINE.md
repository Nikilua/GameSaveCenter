# U09-05 隔离宿主启动窗口复核与最终验收

## 问题与范围

用户在上一次完整验收后仍看到黑色窗口闪现。此前只记录了启动完成后的前台窗口快照，这不能证明完整运行期间没有瞬时窗口。本批只调整验收启动器如何创建 Playnite 进程，不更改产品页面、命令、数据、安全语义或盘符。

`scripts/dev-install-run.ps1` 的启动代码从 `Start-Process` 改为 `System.Diagnostics.ProcessStartInfo`，明确设置 `UseShellExecute = $false`，让 GUI 子进程继承调用线程所在桌面；入口校验修订号同步更新为 `DEV-INSTALL-010`。`Test-LauncherExitCodes.ps1` 增加此启动方式的防回归检查。

受控桌面探针在非活动桌面 `GscGate_14ca4d1c4a2a4c10baa8f4c5fae2d5c6` 启动两种隐藏的 PowerShell 子进程。直接 `ProcessStartInfo` 与隐藏的 `Start-Process` 子进程都位于该隔离桌面，输入桌面始终是 `Default`，两者退出码均为 0。该结果验证了当前的进程桌面继承，但不能单独确定用户所见黑窗的唯一来源；完整中文根链仍需核对 Playnite 实际窗口所属桌面并检查运行期间的用户桌面状态。见 [直接子进程探针](desktop-inheritance.json) 与 [两种启动方式探针](desktop-launch-methods.json)。

## 本批已完成检查

- 两个修改后的 PowerShell 文件语法解析通过；`scripts/validate-source.py` 与 `git diff --check` 通过。
- 三个实际入口的退出码矩阵 9/9 通过（0、7、23 均正确传播），并覆盖不经 Shell 启动 Playnite 的源码守卫。见 [入口结果](launcher-exit-results.json)。
- 桌面继承探针两种方式均退出 0；输入桌面为 `Default`，未切换桌面。
- 一次直接 `dotnet test` 探针因仓库锁定 SDK 8.0.423、调用环境先选择 SDK 9.0.302 而未启动；按 `sdk-baseline.ps1` 选择 8.0.423 后，测试项目编译完成，但直接调用未携带 `GscBuildCommit`，TRX 中 4/8 项因测试身份守卫失败。原始 TRX 保留在 [未提交状态的定向尝试](uncommitted-focused-attempt.trx)，不计通过。没有更改身份门禁或测试断言；当前候选提交后由仓库入口带精确源码身份重跑。

## 冻结源码后的验收

候选 `76e5670f02a3bf85432c312a27d1ae51fc379678` 已提交并推送。标准中文入口映射为 `GameSaveCenter-一键构建安装运行.cmd` → `GameSaveCenter-Run.cmd` → `scripts/dev-install-run.ps1`。普通权限尝试在写入用户 Playnite 扩展目录时被系统拒绝，完整失败事实脱敏记录在 [sandbox-install-denial.txt](sandbox-install-denial.txt)；随后按既有授权提升权限，重跑完整中文链成功，入口/根链最终退出码均为 0。只读运行记录见仓库本地 `artifacts/build-diagnostics/20261106T184403270Z-818963d1/summary.json`、`artifacts/last-dev-install.txt` 和桌面探针 JSON；测试汇总 160 份 TRX、2030 项、1972 通过、0 失败、58 跳过，157 个隔离 WPF 类。58 个跳过由 57 个已撤销“今日工作台”架构断言与 1 个默认 opt-in 鼠标指针用例构成，均没有计入通过。

独立 `scripts/build.cmd` 按其构建职责单独退出 0，SDK 8.0.423 / Release：160 份 TRX、2030 项、1962 通过、0 失败、68 跳过，157 个隔离 WPF 类。68 个跳过为相同 57 个旧架构断言、9 个受限环境下的 Named Pipe 客户端用例、1 个受限环境下的 Worker Named Pipe 重启用例、1 个默认 opt-in 鼠标指针用例。摘要和 TRX 位于保留的 `artifacts/build-diagnostics/20261106T191851252Z-884a1cd6/`；此入口没有替代完整安装启动链。

### 运行期间的桌面观察

- `Default` 桌面只记录目标窗口的出现/消失事件，采样时段为 `2026-11-06T18:05:55.628Z` 至 `19:52:36.302Z`，间隔 25 ms。覆盖普通权限根链、授权提升的成功根链和独立 `scripts/build.cmd`；记录只有探针开始与停止，目标进程窗口事件数为 0。筛选进程包括 Playnite / GameSaveCenter、`cmd`、`powershell`、`pwsh`、`dotnet` 与 `testhost`。逐样本证据已汇总到 [desktop-launch-audit.json](desktop-launch-audit.json)。
- 成功根链启动的 Playnite 主窗口 class 为 `HwndWrapper[Playnite.DesktopApp.exe;…]` 且 `IsWindowVisible=true`；其线程属于非活动桌面 `GscR5Elev_e8bce0044f4a4413bbcb73faa69d1c31`，输入桌面持续为 `Default`。本轮关闭了该验收进程；当前窗口盘点不再有 Playnite 运行实例。
- 上述结果说明这几次构建/安装启动没有观察到目标窗口出现在用户桌面。25 ms 轮询无法排除更短的瞬时帧；它也没有回溯捕获截图中的 HWND。因此不把采样外推成“黑窗根因已完全定位”或对未来手动启动的绝对保证。之后的验收仍须把 WPF 测试与宿主放在非活动桌面。

### 包、安装与远端 CI 身份

完整链生成的 ZIP 和 PEXT 各 45,739,053 bytes，SHA-256 均为 `99A86E7CF42CCE127A0EB0B76E0E8E674207F6D4E37F53359FC21036E88C56D0`。安装清单版本 0.6.73、程序集版本 0.6.73.0，六个安装 DLL 的 SHA-256 与包内文件一致：

| 已安装程序集 | SHA-256 |
| --- | --- |
| GameSaveCenter.Playnite.dll | `75B464DB8176C49371134F6D519E8707F29AA8A4B5C951D46A9351F536555342` |
| GameSaveCenter.Contracts.dll | `CBC0DEC7E3E67890F70A8A99BC1C550F91D770FDEB25401B8DB9EB358EDF36BE` |
| GameSaveCenter.Core.dll | `1D096B92773E4208E5663B636EC781E273AC915029A57D3989257A772EFC8316` |
| GameSaveCenter.Worker.Contracts.dll | `B50C48740BE11FE53218EE1BB09DAD8D6CA1DF20F4AA91CB225DE36BAC97CD64` |
| GameSaveCenter.Worker.Core.dll | `227EEEFA159DCBEE7DD07FC2E63DB2AB6426B2BACB468694111002829CC3D782` |
| GameSaveCenter.Worker.dll | `7A850054C435D534C7EB620B2CB280BBA037E32955D526B1E509FB4D79D32D4E` |

精确源码 GitHub run [#1481](https://github.com/Nikilua/GameSaveCenter/actions/runs/42265616989) completed/success；baseline job `128056627907` 与 SDK 10.0.401 job `128056628466` 全部适用步骤完成。三个 artifact 的 ID、大小和 digest 见 [ci-run-1481.json](ci-run-1481.json)。required-check / ruleset 读取仍沿用先前 403，不能据此认定分支保护条件完整，U09-06 继续“远端待验”。

本次没有重新验证插件嵌入页面、外部 UIA、实体键盘/IME、物理 DPI/跨屏，也未执行真实存档恢复、媒体删除或云端写入。之前没有 `GscBuildCommit` 的定向测试失败仍作为历史拒绝证据，不计为通过，身份门禁未放宽。源码启动修正和入口测试见提交 `76e5670f`；此前测试不完整的时间线仍保留在本文上半部分。

## 本阶段临时输出

[阶段起始空间与目录](stage-start.json)、[本阶段输出清单](stage-output-inventory.json) 和[清理记录](cleanup.json)记录本阶段自有 `.tmp` 输出。TRX 与探针 JSON 已校验哈希并归档；五个一次性目录经规范化路径、仓库 `.tmp` 边界、重解析点、独占文件打开和运行中 dotnet/testhost 检查后按字面路径清理，净释放 D 盘 954,368 bytes。测试尚未完成部分不计成功。

冻结候选验收后，`.tmp/r5-gate-76e5670f` 起始有 10 个自有文件、359,912 bytes；归档并核对证据后清理 9 个文件、238,178 bytes，D 盘可用空间增加 245,760 bytes，C 盘变化为 0。唯一保留成功根链控制台日志：独占打开检查发现外部句柄，未终止未识别的持有进程。路径边界、无重解析点、无文档引用、运行中构建/宿主进程检查与哈希清单见[本批清理记录](cleanup-final.json)；包、安装、当前诊断/TRX 与用户及其它任务的 TestResults 均保留。
