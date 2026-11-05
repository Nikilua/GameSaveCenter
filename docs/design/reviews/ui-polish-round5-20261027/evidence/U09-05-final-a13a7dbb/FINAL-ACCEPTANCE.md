# Round5 U09-05 本机最终验收：a13a7dbb

日期：2026-11-06。冻结源码候选为 `a13a7dbbe27f4cb90d37057163e1481aa8abf355`，版本 `0.6.73`。本轮真实入口与测试证据、源码身份、包/安装身份和 CI 身份均绑定该提交。

## 本机入口与测试

- 实际存在的中文入口 `GameSaveCenter-一键构建安装运行.cmd` 转交 `GameSaveCenter-Run.cmd`，再运行 `scripts/dev-install-run.ps1 -Configuration Release`。根入口进程与外层命令的真实退出码均为 **0**；[`root-entry-result.json`](root-entry-result.json) 记录入口、SHA、候选 SHA、进程窗口句柄与退出码。SDK 为 **8.0.423**，MSBuild 为 **17.11.48**。
- 独立 `scripts/build.cmd` 按其解决方案构建和测试职责运行，退出码 **0**。160 份 TRX 共 2030 项：1962 通过、0 失败、68 跳过。独立入口跳过为57条已撤销的“今日工作台”架构断言、10条当前环境不能运行的 Named Pipe 测试、1条 opt-in 系统鼠标测试；跳过不计通过。
- 中文根入口完成 Release 编译、核心/Worker/WPF 测试、Worker `win-x64` 发布、ZIP/PEXT 打包、安装身份核验及 Playnite 启动。160 份 TRX 共 2030 项：1972 通过、0 失败、58 跳过，157/157 个 WPF 隔离类组完成。58条跳过为57条已撤销的“今日工作台”断言和1条 opt-in 系统鼠标测试；跳过不计通过。
- ZIP 与 PEXT 各45,739,077 bytes，SHA-256 均为 `DD9156E61EC1132B65A9FC906A9D3E58B731C50232B47625DFEE2AF19F450045`。安装清单 `0.6.73`、DLL 文件版本 `0.6.73.0`；6个安装程序集的 SHA 和源码身份见 [`local-gates.json`](local-gates.json)。
- Source Validation 与 XAML 结构审查通过；WPF 静态审查 26 个 XAML、0 error、26 warning、164 info。精确候选 Render QA 退出0，双主题多尺寸394张 offscreen 图、0 `PROBLEM`；它采用逻辑 DPI 1.00，不能代替真实宿主 DPI。报告见 [`render-qa-report.txt`](render-qa-report.txt)。
- 两入口的320份TRX原文件按相对路径和 SHA-256 收录在 [`final-trx-a13a7dbb.zip`](final-trx-a13a7dbb.zip)，逐文件清单在 [`trx-manifest.json`](trx-manifest.json)。完整标准输出的脱敏副本和本机原始日志 SHA 见同目录；原始日志与完整独立 build 诊断仍保存在本机 `artifacts/`、`.tmp/` 引用路径。

## Playnite 宿主边界

本次由根入口启动的 Playnite 进程加载了 `GameSaveCenter 0.6.73.0`，日志记录观察到3个 Playnite 游戏。CMD、PowerShell 与 Playnite 进程的主窗口句柄均为0，未把窗口带到用户桌面；启动进程没有显示实际插件页面，因此**实际页面视觉、外部 UIA、实体键盘/IME、物理 DPI/跨屏尚未验收**。根入口完成启动后，按仓库 `Stop-PlayniteAndOwnedWorkerReliably` 的所有权与路径规则，仅结束了本次启动且窗口句柄为0的 Playnite 实例及其当前扩展目录 Worker；身份见 [`playnite-shutdown.json`](playnite-shutdown.json) 与 [`worker-shutdown.json`](worker-shutdown.json)。

本次没有操作 `subst`、磁盘管理或任何盘符映射；C/D/R 映射未被本轮验收改动。没有执行真实存档恢复、媒体删除或云端写入。

## GitHub CI

精确候选的 [`windows-build.yml` run #1461](https://github.com/Nikilua/GameSaveCenter/actions/runs/42110142797) 为 `completed/success`：SDK 8.0.423 baseline job `127535228072` 15/15 步成功；SDK 10.0.401 job `127535227511` 8/8 步成功。预览包与两份诊断 artifact 的 ID、大小、SHA-256 见 [`windows-ci.json`](windows-ci.json)。

`main` required checks/ruleset 的只读读取此前返回403，本次没有重复探测；U09-06 仍为“远端待验”。由有权限的维护者只读执行 `gh api repos/Nikilua/GameSaveCenter/branches/main/protection/required_status_checks`，并将 required 名单与上述两个 job 核对。

## 状态

U09-05 本机最终门禁通过；U09-08 已完成状态清点，但没有把未验项标为结项。整轮仍为7项已满足、71项代码完成待验收、U09-04进行中、U09-06远端待验。U09-04 的 U00 同条件实际帧时间和累计分配对照缺失；真实宿主页面、UIA、实体键盘/IME与物理 DPI 仍须按各项状态等待验收。故障、取消、恢复安全语义与现有游戏选框/滚动条系统均未为视觉简化而删改。
