# R23-04 UIA 宿主窗口暴露边界复测

日期：2026-09-22
工作区：`D:\workplace\github\GameSaveCenter`
分支：`codex/ui-finesse-round2`
复测身份：`0157ace6f9c13ecc8544e9d801a2d2d2f9a62018`

## 结论

本批没有生产代码变更。当前 `real-host-audit.ps1` 在 D 盘隔离 profile 中重新构建、安装并启动真实 `D:\software\Playnite\Playnite.DesktopApp.exe`；插件日志确认 GameSaveCenter `0.6.73` 已加载，Playnite 也记录了主窗体创建事件。但该会话没有向当前 Windows 桌面暴露可枚举的 Playnite 主窗体：进程 `MainWindowHandle=0`，Win32 顶层窗口枚举为空，UI Automation 按进程查找也为空。

因此 runner 无法合法定位并 Invoke 左侧 `GameSaveCenter` 项，约 90 秒后没有 `summary.json`，按设计以 partial 结束。本批将“窗口未暴露给自动化”与“产品侧栏缺失”分开记录，不把失败写成 UIA/键盘/读屏通过，也不修改 runner 去猜测句柄。

## 执行范围

使用的命令为：

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/real-host-audit.ps1 -Configuration Release -Output "D:\workplace\github\GameSaveCenter\artifacts\ui-host-audit-r23-04-uia-debug-20260922" -UserDataDir "D:\workplace\github\GameSaveCenter\.tmp\r23-04-uia-debug-20260922" -PlayniteExecutable "D:\software\Playnite\Playnite.DesktopApp.exe" -TestTempRoot "D:\workplace\github\GameSaveCenter\.tmp\r23-04-uia-debug-test-20260922"
```

- 使用空的合成库 `library/database.json`（`Version=4`），profile、Worker 数据目录和 IPC 均为本轮隔离范围；没有读取或修改真实存档、媒体、云端数据。
- 当前流水线先通过 XAML `24/24`、Core `125/125`、Worker `355/355`、Playnite source `111` 类、WPF `101/101`；Release `0 error`，保留既有 `MediaCenterView.xaml.cs:699` 的两条 `CS8602` warning。
- 首次尝试中的一个既有 IPC 取消时序测试出现抖动；未改弱测试，单独复跑为 `1/1`，随后第二次完整 runner 未重现该失败。
- Playnite profile 日志记录 `ExtensionFactory:Loaded plugin: GameSaveCenter, version 0.6.73` 和 `WindowFactory:Show window Playnite.DesktopApp.Windows.MainWindowFactory`；同一运行期间 Playnite 进程 ID 为 `39900`，`MainWindowHandle=0`。

## 自动化窗口边界

在 runner 等待期间对同一隔离 PID 做了只读核对：

| 检查 | 结果 | 含义 |
| --- | --- | --- |
| `Get-Process Playnite.DesktopApp` | PID `39900`，`MainWindowHandle=0` | 没有可直接交互的主窗体句柄 |
| Win32 `EnumWindows` + `GetWindowThreadProcessId` | 没有该 PID 的顶层窗口 | 不是侧栏名称匹配失败，而是当前会话没有暴露窗口 |
| Windows UI Automation `RootElement.FindAll` 按 PID | `0` 个元素 | 无法取得侧栏、焦点、键盘或读屏树 |
| `Invoke-GameSaveCenterSidebar` | 未定位侧栏，等待约 90 秒超时 | 没有生成 `summary.json`，runner 输出 `[PARTIAL]` |

这组结果只能说明当前隔离宿主/桌面会话的窗口自动化入口不可用；不能据此断言 Playnite 产品窗口在所有宿主环境都不可见，也不能据此断言 GameSaveCenter 的视觉内容错误。

## 已取得与未取得的证据

当前输出目录为 [`artifacts/ui-host-audit-r23-04-uia-debug-20260922`](../../../../../artifacts/ui-host-audit-r23-04-uia-debug-20260922)。它包含绑定 `0157ace6` 的 `metadata.json`、`runner-metadata.json`、视觉树、资源快照、样式指纹、overflow 分类、嵌入式 Dashboard 视图和滚动回放。metadata 明确为 `CaptureOrigin=EmbeddedPlaynite`、`Mode=embeddedcurrent`、DPI `1.5`、Dashboard `1313.33 × 898 DIP`。

这些输出可以证明当前包进入了 Playnite 的嵌入承载路径，并用于核对受控结构/布局；不能替代 UIA/键盘/读屏、专用 Controlled host、最终 presented frame、ETW 或宿主真实性能证据。没有 `summary.json`，本批不补写任何 UIA 成功结果。

本机只有一个显示器，Q24-03 仍为 `blocked-single-display`；没有绕过 ETW/WPR/系统跟踪权限，也没有把代理/离屏截图写成物理跨屏或真实帧率。Demo 原目录不可用，继续参考恢复生产基线。

## 收口与下一步

- 隔离 Playnite 已用 `--shutdown --userdatadir` 正常关闭；本轮 profile、测试临时目录和未被本报告引用的构建缓存已清理，当前审计输出因被本报告引用而保留。
- R23-04 继续保持“已满足，待宿主环境验证”，不改成 UIA/Controlled host 已通过。
- 下一可执行项是取得一个稳定暴露 Playnite 主窗体给 UIA/受控交互的隔离桌面会话；若该外部条件仍不可用，转做依赖已满足的 R23-05 独立几何小批量。仍不能把离屏或代理数据写成真实 presented frame 性能。
