# L31 真实宿主矩阵阻塞记录（2026-09-09）

## 结论

本轮没有取得真实 Playnite/FusionX 的可审计 UI 证据，不能声称表格滚动异常已解决，也不能把候选包检查替代视频复测。

当前源码交接基线为 `580a70f`。该基线的离屏 RenderHarness 报告已为 `render-qa OK`，但这不改变本记录的真实宿主阻塞结论。

## 已核对事实

- `scripts/real-host-audit.ps1` 会调用 `scripts/dev-install-run.ps1`，停止现有 Playnite、安装/启动开发扩展并等待真实 Dashboard capture；本轮未执行，避免中断用户当前宿主或改变其扩展安装状态。
- PowerShell 只读进程检查确认 `Playnite.DesktopApp.exe` 路径为 `D:\software\Playnite\Playnite.DesktopApp.exe`；本次继续核查时该进程已退出。已安装扩展目录仍为 `0.6.73`，但没有由本轮启动。
- Windows Computer Use 的当前应用清单仍返回 `apps: []`，没有可绑定的 Playnite 窗口；因此无法安全执行截图、拖动滑块、滚轮、Ctrl+End、键盘焦点或 DPI/主题矩阵。该检查未启动或停止任何宿主进程。
- 本轮尝试调用 `scripts/real-host-audit.ps1` 时被安全门禁拒绝：该流程会替换用户扩展目录并启动宿主，而当前没有明确的替换授权。未通过变通方式绕过；未写入用户目录、未启动 Playnite，外部状态保持不变。
- 未生成真实宿主 `summary.json`、滚动诊断日志、录屏或 FusionX 模板链证据；Computer Use 仍返回 `apps: []`，已有离线 RenderHarness/标准模板结果不具备真实宿主证明力。

## 下次执行条件

1. 用户确认可以停止并重新启动当前 Playnite，且提供可绑定的 Windows UI 控制入口。
2. 使用 L30 候选包或同一源码身份安装到隔离扩展目录；先保存完整 Playnite 配置、扩展目录和插件状态库副本。
3. 记录真实宿主的窗口尺寸、DPI、主题、FusionX、Worker 身份、表格内部 ScrollViewer/IScrollInfo 和视频操作结果。
4. 按任务表和媒体表分别完成顶部/底部往返拖动、滚轮、PageUp/PageDown、Ctrl+End、最后一项定位、水平滚动条显示/隐藏、50/400/2000 条数据及刷新/加载更多后的回归。

在上述条件满足前，L31 状态为“外部阻塞”，L30 候选包仍是未安装真实宿主的候选版本。
