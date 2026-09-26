# ENV-001 隔离 Playnite runner 安全收口（2026-09-26）

## 范围与实现

本阶段只加固审计/安装脚本，不改产品业务、版本或 192 项 R 账本：

- real-host audit 必须收到显式 `.tmp` 用户数据目录和 `Playnite.DesktopApp.exe`；启动前拒绝已运行的 Playnite/Worker，并确认当前调用环境能查询进程命令行。检查失败时不创建 profile marker、audit output 或启动宿主。
- profile 仅在空目录写入仓库绑定 marker；复用 marker 前递归拒绝目录内任何 reparse point，未标记的非空目录原样保留。配置数据库路径必须是 profile 子目录且不能经过 reparse point。
- audit 输出只能位于 `artifacts/` 且不得覆盖已有路径。isolated install 分支显式要求 `-NoStart` 与仓库 `.tmp` 扩展根；确认没有 Playnite/Worker 后跳过停止进程逻辑。
- host runner 不查询用户 AppData 主题或默认日志位置。内置主题只从 Playnite 安装目录（可执行文件目录及其父目录）按 `theme.yaml` ID 查询；日志诊断必须带显式隔离数据根。
- 启动记录包含传入的 `--userdatadir` 参数、请求执行文件和 PID 观察结果；实际 Playnite/UIA/呈现结果仍须由宿主运行获得，脚本记录本身不构成真机通过。

## 环境事实与边界

- 本机 Playnite/Worker 当前未运行。已找到并检查 `D:\software\Playnite\Playnite\Playnite.DesktopApp.exe`（签名有效，文件修改时间 2026-09-11）；这纠正了此前只检查标准安装目录的遗漏。
- 当前 Codex 执行身份对 `Win32_Process` 的当前 PID 命令行查询返回“拒绝访问”。用该 Playnite 路径调用审计入口时，新 preflight 返回非零；独立 `.tmp` profile、marker 和 `artifacts` output 均不存在，宿主未启动。
- 已有 [2026-09-24 Playnite bootstrap 记录](R23-04-BOOTSTRAP-SAFE-START-ROOT-CAUSE-20260924-42884321.md)明确包含 fresh profile 的 CEF `platform_channel 0x5` / `拒绝访问`，并要求同状态不重试。本阶段没有改变该 OS/Cef 条件，因此没有再次启动，也没有下载或安装 Playnite。
- 2026-09-26 只读核对了 [Playnite 官方命令行参数文档](https://api.playnite.link/docs/manual/advanced/cmdlineArguments.html)：文档将 `--userdatadir` 定义为重定向数据目录，没有列出启动独立并行实例的参数；`--shutdown` 会关闭已有实例，不能用于隔离启动。结合 2026-08-01 `--userdatadir` 实测仍输出 `Application already running, shutting down.`，目前没有可安全采用的官方多实例启动替代方案；不猜测隐藏参数、不重试相同 CEF 条件。
- 所以尚无测试 PID/扩展加载/UIA/页面截图，也没有通过宿主文件跟踪证明 AppData/现有库未读写。ENV-001 状态保持 `BLOCKED_ENVIRONMENT`。

## 自动验证

- `scripts/tests/Test-PlayniteHostIsolation.ps1`：profile marker、空/未标记目录、路径/数据库范围、用户进程拒绝、输出不覆盖、reparse point 和空格路径测试通过。
- 无 Playnite 可执行文件和 WMI 命令行权限时的入口负向测试均在副作用前拒绝，profile/output 未创建。
- `scripts/validate-source.py`、PowerShell AST、XAML structural `24/24`、`git diff --check` 均通过。
- Release solution `--no-restore` build 与 Playnite test project build：`0 warnings / 0 errors`。Core `125/125`、Worker `357/357`。Playnite 非 WPF source group：`465 passed / 18 skipped / 483 total`（既有显式 skip，包含撤销 UI 基线断言与当前权限下不可运行的 Named Pipe 客户端用例）。105 个 WPF 类以独立进程运行，最终 `105/105` 类通过；R02 hit-area `2/2`、R06 clipboard `4/4`、R08 motion `2/2`、R09 pixel stroke `2/2` 复测通过。当前 DPI 的半物理像素布局与抗锯齿输出已由测试容差覆盖，没有改生产 XAML/业务代码。
- `scripts/validate-source.py`、4 个目标 PowerShell 文件的 AST、XAML structural `24/24`、runner helper tests 和 `git diff --check` 均通过。当前沙箱对用户级 NuGet.Config 的访问拒绝时，使用已有 restore assets 执行 `--no-restore` 构建。

本文件的自动化证据不替代 Playnite 真机验收。R 表 192 项唯一任务基线及既有状态计数未改变。
