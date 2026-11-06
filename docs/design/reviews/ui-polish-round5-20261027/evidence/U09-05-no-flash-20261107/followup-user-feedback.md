# U09-05 黑窗反馈复核

## 反馈范围

2026-11-07 用户再次提供构建期间出现黑窗的截图。图中黑色区域覆盖 GameSaveCenter/Playnite 界面，但没有 HWND、进程 ID、窗口类或桌面名。仅凭截图无法判定窗口来自 Playnite、WPF 测试宿主还是控制台，也不能映射到先前某次构建进程。

先前的 25 ms `Default` 桌面轮询只检查了指定进程集和受控运行。它证明该样本周期没有观察到匹配窗口进入 `Default`，但不能排除短于采样间隔的窗口；它也没有捕获这张截图对应的窗口。因此该反馈使 U09-05 重新进入“进行中”，不撤销历史运行记录，但撤回它对当前问题的结项效力。

## 本轮自动化运行

本地定向测试通过 Win32 `CreateDesktop` 在非活动桌面启动；启动器确认运行桌面与输入桌面不同，测试前后输入桌面都为 `Default`。SDK 8.0.423 / Release 编译退出码为 0。目标 Named Pipe 测试在本地运行环境因客户端权限限制跳过，结果为 1 项跳过、0 项通过、0 项失败；不计作行为通过。测试源文件的请求同步修改仍需由 GitHub Windows 测试环境实际执行确认。

后续本机 WPF 测试、Render QA 和 Playnite 启动均从非活动桌面启动。当前证据不能证明用户截图中的黑窗已消失；需要在取得目标窗口的 HWND/PID/桌面归属，或在窗口事件监测覆盖下复测后，才能重新评估 U09-05。真实插件页面、实体输入和外部 UIA 仍属于各自的宿主验收边界。

聚焦命令的源码身份、桌面身份和跳过计数见 [本地运行摘要](followup-local-run.json)。

## 2026-11-07 桌面身份纠正与根入口修订

此前非活动桌面运行记录中的 `Default` 实为 CodexSandbox 窗口站的同名桌面，不是用户输入桌面 `WinSta0\Default`。R08 与 Q10 动画行为组在非活动桌面分别未能推进 WPF 动画时钟；失败 TRX 已脱敏归档，不作为产品行为结论。精确 Q10 类改在 `WinSta0\Default` 用 `CreateProcess` 的 `CREATE_NO_WINDOW` 重跑，exit 0、1/1、0 skip。事件监测记录到 8 个空标题 `HwndWrapper[TestHostAppDomain]`，进程均为 `testhost.net472.exe`；没有控制台或 Playnite 窗口出现。Q10 夹具窗口在源码中设为无边框、无任务栏、无激活、1% 透明。事件和 TRX 哈希见 [`followup-window-20261107/README.md`](followup-window-20261107/README.md)。

源码核查发现 `GameSaveCenter-Run.cmd` 在成功结束和几个失败分支都执行 `pause`，命令窗口会因此留在桌面。本候选移除这些等待，明确成功返回 0，并将安装脚本 PowerShell 子进程隐藏；独立 `scripts/build.cmd` 也隐藏其 PowerShell 子进程且保留 `%*` 参数转交。`scripts/build.cmd -__gsc_invalid_parameter_probe__` 仍以 1 退出并报告未知参数，source validation 与 `git diff --check` 通过。

精确 SHA `62a363d3a34ea9b9678aee3e8424113416778eae` 的 GitHub run #1483 已完成：SDK 8.0.423 baseline job `128148639481` 与 SDK 10.0.401 job `128148639830` 均 success；Artifact 身份、大小与 digest 见 [`ci-run-1483.json`](ci-run-1483.json)。这些结果验证的是前一提交的 Worker 测试修复，尚非当前批处理修订候选。

本次修订尚未完成其独立 `scripts/build.cmd` 以及中文入口完整安装启动链，也尚未覆盖其全过程窗口事件。因此仍不声称用户截图中的黑窗已被识别或修复；原截图无 HWND/PID 的限制继续有效。完整链将使用 `WinSta0\Default` 和隐藏控制台，WPF 测试仍在可推进动画时钟的桌面执行；仅接受测试宿主的 1% 透明非激活窗以及最终稳定的 Playnite 启动窗。

## 2026-11-07 再次反馈：父命令窗口仍可见

用户指出上述修订后仍闪屏。进一步核对后，中文入口 `GameSaveCenter-一键构建安装运行.cmd` 仍以同步 `call` 执行 `GameSaveCenter-Run.cmd`；这两个批处理共享并保留外层 `cmd.exe` 窗口直到整个构建完成。移除末尾 `pause` 只能防止完成后等待，隐藏 PowerShell 子进程不能隐藏这个父窗口。独立 `scripts/build.cmd` 也存在相同的入口控制台问题。这是明确的源码缺陷，但截图仍没有 HWND/PID，不能追溯认领截图中的历史窗口。

修订候选新增 `scripts/hide-console.ps1`，并从中文入口、英文入口及 `scripts/build.cmd` 在开始构建前调用。脚本验证当前 PowerShell 的父链是 Explorer→`cmd.exe` 后才隐藏，已打开的终端保持可见；如果控制台隐藏不成功则入口退出 90。WinSta0 非活动 desktop 上的原生控制台 probe 读取到 `beforeVisible=true`、`afterVisible=false`，exit 0；不切换输入 desktop。Source Validation、`git diff --check`、隐藏脚本无控制台 smoke exit 0、build.cmd 未知参数探针 exit 1 且 `NamedParameterNotFound` 均通过。新增 PowerShell 文件已按 Windows PowerShell 5.1 约束加 UTF‑8 BOM。

该证据只证明隐藏函数对 Explorer 自有控制台的核心隐藏调用有效；全链/独立 build 的新提交验收仍待完成。所有 WPF 测试与 Playnite 启动会放在 WinSta0 非活动 desktop 上，另在输入 `WinSta0\Default` 监控窗口事件，防止测试宿主覆盖用户桌面。用户截图窗口身份仍未知，故完整链门禁和后续用户验收前 U09-05 不结项。

## 2026-11-07 用户再次指出仍然闪屏

新截图显示黑色窗口遮挡 Playnite/GameSaveCenter，但没有 HWND、PID 或窗口类；当前没有 Playnite 进程，无法追溯识别截图中的窗口。控制台隐藏改动未能解释截图中的完整 UI 内容，也不能据此声称已经解决。

复核本机验证手法后发现此前 Q10 动效聚焦测试在 WinSta0 输入桌面运行。WinEvent 在测试全过程记录 8 个 testhost.net472.exe 无标题 HwndWrapper 显示事件，未记录控制台或 Playnite；对应 WPF 夹具设置 Opacity=0.01。这确认本轮测试会把极暗的 UI 测试窗显示到用户桌面，可能造成截图所见的闪现，但缺少截图 HWND/PID，不能把二者等同。后续测试改在 WinSta0 的非活动 desktop，测试期间监测输入 Default，不再在用户前台启动 WPF 测试。

入口行为探针在更新夹具以包含新隐藏脚本后通过三入口×三退出码 9/9，Source Validation、git diff --check 通过。此前原生控制台 true→false 结果早于祖先进程保护的加入；当前保护分支没有经过 Explorer→cmd→PowerShell 的完整行为探针，故不作已验收声明。完整 build.cmd、中文根链、输入桌面 WinEvent 和当前 SHA CI 均待验，U09-05 保持进行中。
