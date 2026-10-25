# b29c39a5 标准根入口完整链

源码身份 b29c39a541ba92461e167f14f64917d889293161 的默认命令 cmd.exe /d /c "GameSaveCenter-Run.cmd < nul" 退出码为 0。构建诊断 run 20261025T152449622Z-e85af693，安装 run 20261025T152447580Z-d50774fa。SDK 8.0.423、Release；155 个阶段无失败，solution 0 警告、0 错误。执行环境为 Windows 10.0.22000、PowerShell Core 7.6.2。

测试共 1968 项：1911 通过、57 跳过、0 失败/错误/超时/中止。149 份 TRX 的文件计数与汇总一致，0 解析错误、0 计数差异；146 个 WPF 隔离类全部结束。Core 157/157，Worker 400/400，Playnite source 562 项中544通过/18跳过，WPF隔离849项中810通过/39跳过。57个跳过项均明确标为已撤销的今日工作台UI架构断言，见各TRX的NotExecuted理由。

Worker win-x64发布、ZIP/PEXT打包与安装身份验证成功。ZIP/PEXT SHA-256 均为 E6F5F836E2FA59ACBC6BE5D6EBC9351F79A72C95B4EDCFBFE4422C36E6213C77。安装清单版本0.6.73、DLL版本0.6.73.0；六个已安装程序集的 ProductVersion 均含完整 b29c39a5 源码身份，SHA见 install-identities.txt。

Playnite PID 53100 于 2026-10-25 23:52:27+08:00 启动；同次 extensions.log 在23:52:34.784确认 GameSaveCenter 从实际安装目录加载并观察到3个游戏。Worker PID 52832 从该安装目录启动。当前宿主加载路径证据不表示已验收真实页面的浅色/窄短窗、物理输入或DPI矩阵。

用户最新外机附件记录的构建 run 为 20261025T040600037Z-ec604b91，安装前置 run 为 20261025T040558033Z-0b75781f；Q06 Light hover 失败并在打包安装前停止。附件没有同次 HEAD/sourceCommit，控制台显示的合成探针依赖 MouseDevice.ChangeMouseOver。该日志不能证明当前 b29c39a5 主线的结果；当前主线同类Q06在本次完整链通过。外机快进至当前主线后的完整根入口仍需实跑并回报，不能用本机结果代替。

构建 summary 的 workingTreeClean=false 仅因运行前已有的未跟踪用户Round3 TRX/TestResults目录；跟踪产品源码、测试和脚本未变。完整149份原始TRX、summary.json与全部控制台日志保留在本机 artifacts/build-diagnostics/20261025T152449622Z-e85af693；隔离构建输出、包和安装报告保留在 artifacts。提交证据不包含 artifacts/ 生成物。

同一日较早的受限沙箱根入口曾在安装用户目录时遇到 Access denied，并有11项因沙箱禁止本地Named Pipe或系统鼠标访问而额外跳过。未把那次受限结果算作成功；随后获准的同一个默认根入口命令完成上述完整链。安装器确认替换并验证六个程序集后启动宿主。

核心机器可读结果见 verification.json；安装程序集身份见 install-identities.txt；当前主线Q06与R05 Tooltip原始TRX随本记录归档。用户最新外机旧失败的边界见 external-q06-20261025.json。