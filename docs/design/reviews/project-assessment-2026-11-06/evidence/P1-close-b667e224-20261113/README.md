# P1 本地可执行范围收口：b667e224（2026-11-13）

## 边界

- 本记录更新综合评估计划的当前事实，不把抽查报告的 16 项候选改写为已确认缺陷。
- A01、A05、A06、A07、A08、A11 的本地可执行范围已由精确源码 `b667e22445fbf57aa591f39f3d968f5c166c7cb6` 的完整入口和独立构建覆盖。
- A03 仍与 Round5 U09-04 合并，A04 仍需真实可见 Playnite、物理输入和呈现帧数据。本轮不占用用户桌面，也不以离屏代理关闭这两项。
- 没有读取真实 Playnite profile、配置、游戏库或存档，没有执行恢复、删除或云端写入。

## 中文完整入口

- 入口：`GameSaveCenter-一键构建安装运行.cmd` → `GameSaveCenter-Run.cmd`。
- SDK `8.0.423`，Release；源码、安装清单及六个程序集的信息版本均对应完整 SHA `b667e22445fbf57aa591f39f3d968f5c166c7cb6`，扩展版本 `0.6.73`。
- 完成 build、test、publish、package、隔离安装和 Playnite/Worker 启动，控制台进入 `[OK] Build, verification, installation and Playnite startup completed.` 成功分支。外层 `Start-Process -Wait` 因继承句柄没有返回数值，本记录不把包装器状态写成独立捕获的 cmd 退出码。
- 160 份 TRX：2,043 项，1,985 通过、0 失败、58 skipped；skip 不计通过。157/157 个 WPF 隔离类完成。`R02BusyStateTests` 6/6。
- Playnite PID 34680 与 Worker PID 12536 均使用一次性 profile/安装内容，核验后已关闭。启动身份不等于修复后的真实宿主视觉验收。

## 独立构建入口

- `scripts/build.cmd` 使用同一 SHA、SDK `8.0.423`、Release，实际退出码 `0`。
- 160 份 TRX：2,043 项，1,985 通过、0 失败、58 skipped；157/157 个 WPF 隔离类完成，`R02BusyStateTests` 6/6。
- 此成功结果更新 `c2fcc804` 上输入桌面探针失败的当前状态；旧失败日志和 TRX 继续保留，不被成功记录覆盖。

## 原始本地证据

| 文件 | bytes | SHA-256 |
| --- | ---: | --- |
| `artifacts/build-diagnostics/20261113T060254846Z-d6b80c0d/summary.json` | 568,129 | `ACA8D7E34C8746E7A2FFD96E7F02EC0E5877C761D02019BE2E58D08A62D6EC1B` |
| `artifacts/build-diagnostics/20261113T060254846Z-d6b80c0d/tests/wpf.R02BusyStateTests/wpf.R02BusyStateTests.trx` | 10,566 | `6956B6C294CE789E703285DCD606319D5F041532292D678B0D89F4560D9E2577` |
| `artifacts/busy-b667e224/console.out.log` | 122,381 | `8876A716A6DC36C3DF6A1B52C67B66790AF88B3C90F288F68BCBA9265109C4CC` |
| `artifacts/busy-b667e224/console.err.log` | 388 | `F72F5F87D99D221B79F8F075DA045946A530D1BA418306110F3824F9E2CBFC2D` |
| `artifacts/busy-b667e224/install-report.txt` | 2,789 | `C31AEED832F6188FB85F17E7EF96F2448D6DFE304337C974D800BF2E8C863D51` |
| `artifacts/busy-b667e224/scripts-build/20261113T063842849Z-ba2d067c/summary.json` | 992,591 | `A470F260F496033294AE307034D4F3F4A6117F3CBA996F3734C72F3B075EA5B6` |
| `artifacts/busy-b667e224/scripts-build/20261113T063842849Z-ba2d067c/tests/wpf.R02BusyStateTests/wpf.R02BusyStateTests.trx` | 10,638 | `3B4ED2E151622EB4CEB4DBA7A3C8893CBF7F0AE9DEBF8DE05B101484BC85C293` |
| `artifacts/busy-b667e224/scripts-build-console.log` | 117,626 | `9FDFB9939BE852A497083457CA213F01E3B8E1BC59844B3C6DFCD413A734B228` |

`console.err.log` 保留两条 Playnite CEF 读取 Chrome 扩展注册表值的告警；完整入口仍进入成功分支。

## GitHub 与远端边界

- 精确源码 run `43375661710`：基线 job `131721452745` 的全部 15 个步骤 success；SDK `10.0.401` job `131721453027` 的全部 8 个步骤 success。
- GitHub commit API 返回的 SHA、标题及两文件 diff 与本地提交一致。
- required-checks/ruleset 名单仍沿用既有 HTTP 403；权限未变化，未重复请求。已运行 job 成功与 required checks 名单未验证仍是两个事实。

## 清理

本批拟清理的 `.tmp/busy-b667e224-tests`（32 文件，12,278,656 bytes）和 `.tmp/busy-b667e224-scripttests`（15 文件，6,451,200 bytes）已核对归属、规范化根内路径、重解析点、进程占用和文档引用。指定的 PowerShell `Remove-Item -LiteralPath -Recurse -Force` 被执行策略以 `blocked by policy` 拒绝，因此两目录仍在，没有绕过删除。用户 TRX/TestResults、配置、真实存档和未知文件未触碰。

## 下一步

P1 的本地代码/测试范围以本记录收口；最终交付证据仍缺中文根入口最外层cmd的直接数值退出码。日志成功分支只能支持流程完成，不能补造进程退出记录。后续实际完整链须在启动前安排独立进程退出采集，避免等待Playnite继承句柄；不得用SkipTests或旧提交的退出码补齐。

A03/A04 保持真实宿主条件待满足；不重复离屏跑数，不因计划记录解除宿主或权限边界。任何真实恢复、删除或云端写入仍需另行授权。

### 交付证据复核（2026-11-13）

在文档HEAD `80b5fe6c`复核，`git diff --name-only b667e224..HEAD`仅列出10个文档/证据索引文件，产品源码未变。现场重算完整入口日志、安装报告与根链summary三项SHA-256，均与上表一致；日志末尾仍保留安装成功与启动成功分支，六个程序集的信息版本仍在安装报告中绑定完整b667 SHA。本次未运行构建、未启动宿主、未删除文件。

尚不能据此宣称整体目标完成：根入口直接退出码缺失；A03/A04真实输入与呈现帧、A09同会话宿主闭环、A10宿主代表规模仍未验；required-checks名单仍为既有403；三处本阶段临时测试目录的指定删除被策略拒绝。各项当前成功与历史失败证据继续保留。
