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

## 当时下一步（截至 b667 本地证据阶段）

P1 的本地代码/测试范围以本记录收口；最终交付证据仍缺中文根入口最外层cmd的直接数值退出码。日志成功分支只能支持流程完成，不能补造进程退出记录。后续实际完整链须在启动前安排独立进程退出采集，避免等待Playnite继承句柄；不得用SkipTests或旧提交的退出码补齐。

A03/A04 保持真实宿主条件待满足；不重复离屏跑数，不因计划记录解除宿主或权限边界。任何真实恢复、删除或云端写入仍需另行授权。

## 最终冻结候选完整链补充（2026-11-13）

最终候选提交为 `008b3d5dde4440534fe35a563422c60817681058`。它相对忙碌提示修复提交 `b667e22445fbf57aa591f39f3d968f5c166c7cb6` 只新增文档，`src/`、`tests/`、`scripts/` 与资源没有差异；Busy 提示修复仍由 b667 实现。

- SDK `8.0.423`、Release、`SkipTests=false`。实际执行 `GameSaveCenter-一键构建安装运行.cmd` → `GameSaveCenter-Run.cmd`；直接等待 cmd 进程 PID `36560` 后捕获退出码 `0`，不是根据包装器或日志推断。时间为 `2026-11-13T08:17:55Z`–`08:46:19Z`。
- build、test、publish、package、隔离安装和 Playnite/Worker 启动分支均完成。160 份 TRX 共 2,043 项：1,985 通过、0 失败、58 跳过；跳过项不计通过。157/157 个 WPF 隔离类完成，`R02BusyStateTests` 为 6/6。
- `0.6.73` 安装目录位于一次性 profile `.tmp/e008-profile`；六个程序集信息版本均绑定完整 SHA `008b3d5dde4440534fe35a563422c60817681058`。Playnite 使用隔离 profile 启动；后续进程快照显示 PID `10092` 已退出。该启动未读取用户 profile/配置/存档，未执行恢复、删除或云端写入。
- 私有桌面进程 PID `18580` 退出码 `0`，WinSta0 桌面为 `GscFinalExit008b`，输入桌面前后均为 `Default`。这不证明真实用户桌面的画面或输入已验收。

当前必要原始证据及 SHA-256：

| 文件 | bytes | SHA-256 |
| --- | ---: | --- |
| `artifacts/exit-008b3d5d/root-exit.json` | 279 | `5A313FDDFBF3C1525BECF246CD76208EE8FDAC71BA6FB3A416960EB6D4E69748` |
| `artifacts/exit-008b3d5d/desktop-exit.json` | 279 | `B8DC615465BB4FAFB467AD94CC24E93EABA8E152E2AFA86DFE6B41F3F40ADED9` |
| `artifacts/build-diagnostics/20261113T081754370Z-d14bef2d/summary.json` | 566,556 | `7A6363052255E119AF604990323D2AE3B886FFAADA74568B1F8EC015F7C2B5AB` |
| `artifacts/build-diagnostics/20261113T081754370Z-d14bef2d/tests/wpf.R02BusyStateTests/wpf.R02BusyStateTests.trx` | 10,458 | `A5EF573F0E8DA9B0AB07B9D2A97B8E9DF3A6BDAA08F3762AE4D2A6E5C5A06EBD` |
| `artifacts/exit-008b3d5d/console.out.log` | 122,218 | `5006BD4129C69B19E0D6D61CE23B42C036B646C3AF3FEDDC00DAE0B0780B0DAC` |
| `artifacts/exit-008b3d5d/console.err.log` | 388 | `0E34CE8DF7CCD5237FA7415ED7856DDADBB74BB030695F23382C51C255BE7244` |
| `artifacts/exit-008b3d5d/install-report.txt` | 2,699 | `249B386A59839C34B54C19D98907968C50F35343C2B38A0996CD26DCD4830D69` |
| `artifacts/GameSaveCenter-0.6.73-playnite.zip` / `.pext` | 45,740,149 each | `732B67B8220C058FB07270AC82819D86DB44AD45BA6A7FE191DCF8D537F5BAA8` |

`console.err.log` retains two Playnite CEF Chrome-extension registry warnings. They did not alter the command exit code or success branch. Isolated startup and source-level busy-state tests do not establish that the screenshot's original runtime loaded this assembly; that runtime identity remains unknown.

独立 `scripts/build.cmd` 与精确 SHA GitHub 基线 job 在首次写入本节时尚未终态；后续终态结果如下：

### 独立 `scripts/build.cmd`

- 实际入口退出码 `0`；诊断摘要 `outcome=passed`，源码 SHA 为 `008b3d5dde4440534fe35a563422c60817681058`。
- 实际构建使用 SDK `8.0.423`、Release。诊断 `summary.json.sdk` 与 `console.out.log` 的 SDK 选择、Base Path 和 `dotnet --info` 均为 8.0.423。辅助退出记录 `exit.json` 另含 `sdkVersion=9.0.302`，它是在外层进程读取到的系统 PATH SDK，不是本次构建所选 SDK；保留原始值，不将其当作构建事实。
- 160 份 TRX 共 2,043 项：1,985 通过、0 失败、58 跳过；跳过项不计通过。157/157 个 WPF 隔离类完成，`R02BusyStateTests` 6/6。
- 控制台错误日志为空。此命令未安装或启动 Playnite；根入口中的隔离启动证据独立保留。

`scripts/build.cmd` 原始结果：

| 文件 | bytes | SHA-256 |
| --- | ---: | --- |
| `artifacts/scripts-build-008b3d5d/exit.json` | 221 | `88102F351621D9A55A064DD35F83492725BE576203937BCFAAAB2147F903CFDB` |
| `artifacts/scripts-build-008b3d5d/diagnostics/20261113T085220211Z-d9cf8c58/summary.json` | 993,464 | `BB4AC262F722F2352AC587F3BE09C1547CB298EC92856CEC6D989B92136A9A5B` |
| `artifacts/scripts-build-008b3d5d/diagnostics/20261113T085220211Z-d9cf8c58/tests/wpf.R02BusyStateTests/wpf.R02BusyStateTests.trx` | 10,626 | `8E0F1977D323F84B22E9C27C19DB6CBC62EC3389F59C3D341DE273FB4559DA96` |
| `artifacts/scripts-build-008b3d5d/console.out.log` | 117,508 | `F0305F306E9F0C98560DC027963B462498FDB10A59A660084805163528170B0A` |
| `artifacts/scripts-build-008b3d5d/console.err.log` | 0 | `E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855` |

### 精确 SHA GitHub run

- Run #1564 / `43391873169`，event `push`，head SHA `008b3d5dde4440534fe35a563422c60817681058`。
- 基线 job `131773490954`（`编译、测试与打包`）全部步骤 completed/success；SDK 10.0.401 job `131773490438`（`.NET SDK 10.0.401 编译与测试复核`）全部步骤 completed/success。
- artifact `GameSaveCenter-windows-preview`：ID `14024746736`，90,796,020 bytes，digest `sha256:481bfc1d97991f5a4cac6f575ae53c665bcc282b2f77104799acc9c7ee9591c7`。
- artifact `GameSaveCenter-build-diagnostics`：ID `14024666949`，2,533,522 bytes，digest `sha256:9c3e0fc66b7c0f2d9fba822327b3c9f97b103daccdea85fd12a54cda22556d0a`。
- artifact `GameSaveCenter-sdk10-build-diagnostics`：ID `14022865985`，1,283,826 bytes，digest `sha256:ea5e51001bc6170ddca7bc07ca52478c539bcb4a5a3913f06c40b80dfe0d0fa2`。
- required-checks/ruleset 名单仍是既有 HTTP 403，权限未变，没有重复请求；两个 workflow job 成功不等于已验证 required-checks 名单。

当前两入口原始结果仍保存在 `artifacts/exit-008b3d5d/`、`artifacts/build-diagnostics/20261113T081754370Z-d14bef2d/`、`artifacts/scripts-build-008b3d5d/`；本地成功包保留在 `artifacts/GameSaveCenter-0.6.73-playnite.zip` 和 `.pext`。当前用户 TRX、`TestResults/`、配置、真实存档与未知文件未触碰。

### 本阶段 `.tmp` 清理复核

已完成构建后拟清理的目标都规范化在仓库 `.tmp/` 内，所有者为当前用户 `Sult\lopmatu`，递归检查均为0个重解析点；精确目标在跟踪文档中没有路径引用，构建/Playnite进程已退出。清单如下：

| 目标 | 文件数 | 逻辑 bytes | 状态 |
| --- | ---: | ---: | --- |
| `.tmp/scripts-build-008b-build` | 711 | 162,910,383 | `Remove-Item -LiteralPath -Recurse -Force` 被自动审查以 `blocked by policy` 拒绝；仍保留 |
| `.tmp/scripts-build-008b-tests` | 32 | 12,278,656 | 同一批清理命令被拒绝；仍保留 |
| `.tmp/run-build-exit-008b.ps1` | 1 | 1,304 | 同一批清理命令被拒绝；仍保留 |
| `.tmp/exit-008b-inner.ps1` | 1 | 1,514 | 同一批清理命令被拒绝；仍保留 |
| `.tmp/exit-008b-outer.ps1` | 1 | 516 | 同一批清理命令被拒绝；仍保留 |

清理尝试前 C/D 可用空间为 `5,118,636,032` / `15,957,209,088` bytes；拒绝后复测为 `5,117,407,232` / `15,957,209,088` bytes。命令在执行前被拒绝，没有文件删除，因此空间变化不归因于清理；未使用其他方式绕过。保留 `.tmp/e008-profile`（隔离profile配置/安装核验）、`.tmp/e008-build`（安装报告记载的构建根）、当前包与全部审计/TRX。用户及其他任务的 `TestResults/`、TRX、配置、真实存档和未知文件均未操作。

### 历史交付证据复核（2026-11-13；HEAD 80b5fe6c，早于 008 补验）

在文档HEAD `80b5fe6c`复核，`git diff --name-only b667e224..HEAD`仅列出10个文档/证据索引文件，产品源码未变。现场重算完整入口日志、安装报告与根链summary三项SHA-256，均与上表一致；日志末尾仍保留安装成功与启动成功分支，六个程序集的信息版本仍在安装报告中绑定完整b667 SHA。本次未运行构建、未启动宿主、未删除文件。

尚不能据此宣称整体目标完成：在该历史HEAD上根入口直接退出码仍缺失；A03/A04真实输入与呈现帧、A09同会话宿主闭环、A10宿主代表规模仍未验；required-checks名单仍为既有403；三处本阶段临时测试目录的指定删除被策略拒绝。随后008候选的根入口退出码已由上方“最终冻结候选完整链补充”记录解决。各项当前成功与历史失败证据继续保留。
