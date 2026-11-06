# U09-05 桌面黑窗修订与事件监测

## 已确认的入口行为

`GameSaveCenter-Run.cmd` 原先会在成功后以及缺文件、脚本过期和安装失败分支调用 `pause`。由该入口启动的命令控制台会一直留在屏幕，直到按键才退出。当前候选已删除这些等待、显式成功返回 0，并在启动安装脚本时给 PowerShell 子进程设置 `-WindowStyle Hidden`。独立 `scripts/build.cmd` 同样隐藏 PowerShell 子进程，仍通过 `%*` 转交参数。图中历史窗口没有 HWND/PID，故只能说找到了会造成残留黑窗的确定入口行为，不能把历史截图的窗口身份追溯为该控制台。

## 正确桌面上的聚焦行为验证

`62a363d3a34ea9b9678aee3e8424113416778eae`、SDK 8.0.423、Release、test assembly SHA `CBCB249C1A5CCA7C2637D625D32B7B260C05E93AE45F6267D2BC0136BAE91682`。Q10 生产 Toggle 行为类在真实输入桌面 `WinSta0\Default` 使用 `CreateProcess` 的 `CREATE_NO_WINDOW` 启动，exit 0，1 passed、0 failed、0 skipped；源 TRX SHA `2BA6AA88A1616D2825CB8E9793076CB40D5D0AAED5B71A2B5080848CEF916502`，已脱敏副本和摘要见 [`q10-focused.trx`](q10-focused.trx)、[`active-desktop-window-monitor.json`](active-desktop-window-monitor.json)，控制台日志见 [`q10-focused.log`](q10-focused.log)。

WinEvent `EVENT_OBJECT_SHOW` 覆盖该用例全过程，记录 8 次窗口显示事件，全部来自无标题的 `testhost.net472.exe` `HwndWrapper[TestHostAppDomain]`。未观察到 `ConsoleWindowClass`、命令控制台或 Playnite 窗口。对应测试夹具将窗口设为 `WindowStyle=None`、`ShowInTaskbar=false`、`ShowActivated=false`、`Opacity=0.01`。本监测可证明该精确用例没有创建可见控制台，不识别此前截图里的窗口，也不替代完整根入口检查。

## 被纠正的非活动桌面尝试

两个隔离桌面完整 `scripts/build.cmd` 尝试分别在 R08 和 Q10 动画类失败，失败时计数和 SHA 见 [`sandbox-desktop-failures.json`](sandbox-desktop-failures.json)，脱敏 TRX 为 [`sandbox-desktop-r08-failure.trx`](sandbox-desktop-r08-failure.trx) 与 [`sandbox-desktop-q10-failure.trx`](sandbox-desktop-q10-failure.trx)。聚焦 Q10 在非活动桌面也无法推进动画时钟。原因是先前所谓 `Default` 实际属于 CodexSandbox 窗口站；WPF 动画时钟在该非活动桌面上没有可靠推进。保留所有失败，不修改断言、不扩大 skip，后续 WPF 动效验收使用真实 `WinSta0\Default` 并隐藏控制台。

## 尚未完成

批处理修订候选仍需独立 `scripts/build.cmd` 完整运行、中文根入口完整构建/测试/发布/打包/安装身份核验/启动，以及覆盖整个运行过程的 WinEvent 检查。CI #1483 只验证前一 SHA 的测试修复，不能代表本批入口修改。原截图窗口仍无可回溯的 HWND/PID；U09-05 保持进行中。
