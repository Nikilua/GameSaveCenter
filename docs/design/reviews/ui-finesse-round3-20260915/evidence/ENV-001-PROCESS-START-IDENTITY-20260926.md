# ENV-001 启动进程身份复核（2026-09-26）

## 本轮发现与修正

复核隔离 runner 的启动取证时发现三处误接受风险：旧路径判断用子串匹配，profile `...-sibling` 可匹配期望 profile；命令行含多个 `--userdatadir` 时没有拒绝歧义；Playnite 第二实例可向现有实例转发并以正常退出结束，runner 曾把取证前退出表示成普通状态而非启动失败。Playnite 的单实例转发/退出行为见[官方应用源码](https://github.com/JosefNemec/Playnite/blob/master/source/Playnite/App/PlayniteApplication.cs)；`--userdatadir` 的文档含义是数据目录重定向，不等同于多实例隔离，见[官方命令行参数文档](https://api.playnite.link/docs/manual/advanced/cmdlineArguments.html)。

`Get-GscPlayniteProcessStartEvidence` 现要求命令行恰有一个带引号的 `--userdatadir`，规范化后与请求路径完整相等（不区分大小写并处理尾部分隔符）；记录 `ObservedUserDataDir`。它还要求进程在首次快照后持续存活 500 ms，并由第二次 CIM 快照确认 PID、可执行文件路径和完整命令行未变。进程在首次快照前或稳定窗口内退出、消失或身份变化均抛错，不作为隔离成功。此稳定窗口针对短暂单实例转发，不构成操作系统级原子隔离证明。

helper 回归新增/覆盖了正确 profile、相似前缀兄弟 profile、重复参数、快照前退出和首次 CIM 快照后退出。后一负例会在返回第一次模拟快照后终止本测试自己启动的子进程，确认稳定性检查拒绝该启动。

## 验证

- Release solution：设置 `GSC_BUILD_COMMIT` / `GSC_SOURCE_ROOT` 后 `dotnet build GameSaveCenter.sln --no-restore -c Release -m:1`，0 warning、0 error。正式 `scripts/build.ps1` 已先通过 XAML 结构检查 `24/24`，随后因当前身份无法读取 `%AppData%\NuGet\NuGet.Config` 而在 restore 阶段停止；使用已还原资产完成 `--no-restore` 构建与测试。
- Core `125/125`；Worker `357/357`；Playnite `DiagnosticsEvidenceSourceTests 8/8`；`scripts/tests/Test-PlayniteHostIsolation.ps1` 通过；4 个相关 PowerShell 文件 AST 解析通过；`git diff --check` 通过。
- Playnite 隔离 runner 的源码组（111 个测试类）通过后进入 105 个 WPF 类逐进程阶段。本阶段没有 UI 改动；全量 WPF 阶段在前两个类后停止，故不声称 105 类全通过。
- 首次裸跑 solution 全量 `dotnet test` 未使用仓库 build identity/隔离 runner，导致源码 identity 门拒绝和多个 WPF testhost 并行运行；已停止由该命令遗留且时间戳确认归属本轮的测试宿主，再按项目方式串行重建、测试。该裸跑不是最终验证结果。

## 环境边界与账本

本轮没有启动 Playnite。当前 WMI `Win32_Process.CommandLine` 查询仍拒绝访问；同机 2026-09-24 CEF `platform_channel 0x5` 阻断状态未变化，不重复启动或绕过权限。没有用户 AppData、扩展目录、库、存档、媒体或云端读写证据，也没有 UIA/最终呈现证据；ENV-001 继续 `BLOCKED_ENVIRONMENT`。

启动前检查与启动后复核之间仍存在极窄的 TOCTOU 窗口；进程命令行匹配及短暂存活只提供本地 fail-closed 证据，不证明内核级实例隔离或无文件副作用。只有权限/CEF 条件变化后才可继续真实宿主验收。

R 正式任务表仍是 192 个唯一 ID，状态计数 `106/83/1/1/1` 不变；本轮不新增、重分类或签收任何 R 项。
