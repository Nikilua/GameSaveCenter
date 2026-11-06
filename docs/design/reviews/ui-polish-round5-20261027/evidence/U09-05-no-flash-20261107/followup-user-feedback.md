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
