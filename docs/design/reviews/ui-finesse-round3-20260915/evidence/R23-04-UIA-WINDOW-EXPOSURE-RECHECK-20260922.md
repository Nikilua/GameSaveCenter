# R23-04 UIA 宿主窗口暴露边界复测

日期：2026-09-22
工作区：`D:\workplace\github\GameSaveCenter`
分支：`codex/ui-finesse-round2`
复测身份：`eba374c5c89770db2c4ff37ec18102cf6f55decb`（窗口探测：`c6d65b08`，审计开关：`2d327d4b`）

## 当前复测结论（eba374c5）

本批把真实宿主窗口暴露结果从“源码/人工核对”收口为 `host-window-exposure.json`：审计脚本用进程快照和 Win32 `EnumWindows` 记录 Playnite PID 的 `MainWindowHandle`、顶层窗口标题、类名和可见性。它仍保留原 UIA 60 秒探测、summary 等待和 `[PARTIAL]` 语义，不会猜测句柄或把窗口存在升级成 UIA 通过。

当前隔离审计实际观察到 Playnite PID `31920`、路径 `D:\software\Playnite\Playnite.DesktopApp.exe`、`MainWindowHandle=27199668`（`0x19F08B4`），但可见窗口标题为 `Startup Error`，顶层窗口共 `5` 个（其余为隐藏辅助窗口），`SidebarAutomationFound=false`，分类为 `top-level-window-observed-ui-automation-not-confirmed`。没有生成 `summary.json`，runner 按设计以 `[PARTIAL]` 结束。

隔离 `cef.log` 记录 `platform_channel.cc ... 拒绝访问 (0x5)`；这解释了当前宿主启动错误的外部边界，但不证明所有 Playnite 会话均不可见，也不证明 GameSaveCenter 侧栏实现错误。该结果仍不构成 UIA、键盘、读屏、Controlled host、真实 presented frame 或宿主性能通过。

## 当前执行与证据

使用的隔离命令为：

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/real-host-audit.ps1 -Configuration Release -Output "D:\workplace\github\GameSaveCenter\artifacts\ui-host-audit-r23-04-profile-bootstrap-20260922" -UserDataDir "D:\workplace\github\GameSaveCenter\.tmp\r23-04-profile-bootstrap-user-20260922" -PlayniteExecutable "D:\software\Playnite\Playnite.DesktopApp.exe" -TestTempRoot "D:\workplace\github\GameSaveCenter\.tmp\r23-04-profile-bootstrap-test-20260922" -SkipInstallTests
```

- `-SkipInstallTests` 是本批新增的显式审计开关，只透传现有 `build.ps1 -SkipTests`；隔离 profile 自举阶段记录 `IsolatedProfileConfig=restored-from-isolated-backup`。当前提交 Release 构建仍为 XAML `24/24`、`0` errors、既有 `MediaCenterView.xaml.cs:699` 两条 `CS8602` warning，六份程序集构建身份一致。
- 当前编译程序集直接运行 `DiagnosticsEvidenceSourceTests` 为 `2/2`；它覆盖窗口探测、跳过开关和隔离配置自举契约。没有把未执行的 101 个 WPF 隔离类写成通过。
- 全新隔离 profile 由 runner 自行启动 Playnite 生成 `Backup/config.json`，再只在同一隔离目录恢复根配置；没有手工复制步骤，也没有读取或修改真实用户 profile、存档、媒体、云端或诊断外发。
- 输出目录为 [`artifacts/ui-host-audit-r23-04-profile-bootstrap-20260922`](../../../../../artifacts/ui-host-audit-r23-04-profile-bootstrap-20260922)，其中 `host-window-exposure.json` 的 `EvidenceSource=RealPlaynite`、`MainWindowHandleNonZero=1`、`TopLevelWindowCount=5`、`CountsAsVisualPass=false`；runner metadata 记录 `InstallTests=skipped-by-explicit-audit-switch`、`IsolatedProfileConfig=restored-from-isolated-backup`、当前完整 SHA。
- 本机只有一个显示器，Q24-03 仍为 `blocked-single-display`；没有绕过 ETW/WPR/系统跟踪权限，也没有把代理/离屏截图写成物理跨屏或真实帧率。Demo 原目录不可用，继续参考恢复生产基线。

## 当前收口与下一步

- 已结束本次隔离 Playnite PID `31920`；保留上述 JSON 和 runner metadata 审计目录，读档后已清理未被证据引用的隔离日志、profile、构建和测试临时目录。
- R23-04 继续保持“已满足，待宿主环境验证”，窗口探测能力已补齐，但 UIA/Controlled host 仍未通过；下一可执行项是找可稳定提供正常 Playnite 主窗体的隔离桌面会话。
- 若同一外部权限边界继续存在，则转做依赖已满足的 R23-05 独立几何小批量；仍不能把离屏或代理数据写成真实 presented-frame 性能。

## 历史复测结论（0157ace6）

本批没有生产代码变更。当前 `real-host-audit.ps1` 在 D 盘隔离 profile 中重新构建、安装并启动真实 `D:\software\Playnite\Playnite.DesktopApp.exe`；插件日志确认 GameSaveCenter `0.6.73` 已加载，Playnite 也记录了主窗体创建事件。但该会话没有向当前 Windows 桌面暴露可枚举的 Playnite 主窗体：进程 `MainWindowHandle=0`，Win32 顶层窗口枚举为空，UI Automation 按进程查找也为空。

因此 runner 无法合法定位并 Invoke 左侧 `GameSaveCenter` 项，约 90 秒后没有 `summary.json`，按设计以 partial 结束。本批将“窗口未暴露给自动化”与“产品侧栏缺失”分开记录，不把失败写成 UIA/键盘/读屏通过，也不修改 runner 去猜测句柄。

## 历史执行范围

使用的命令为：

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/real-host-audit.ps1 -Configuration Release -Output "D:\workplace\github\GameSaveCenter\artifacts\ui-host-audit-r23-04-uia-debug-20260922" -UserDataDir "D:\workplace\github\GameSaveCenter\.tmp\r23-04-uia-debug-20260922" -PlayniteExecutable "D:\software\Playnite\Playnite.DesktopApp.exe" -TestTempRoot "D:\workplace\github\GameSaveCenter\.tmp\r23-04-uia-debug-test-20260922"
```

- 使用空的合成库 `library/database.json`（`Version=4`），profile、Worker 数据目录和 IPC 均为本轮隔离范围；没有读取或修改真实存档、媒体、云端数据。
- 当前流水线先通过 XAML `24/24`、Core `125/125`、Worker `355/355`、Playnite source `111` 类、WPF `101/101`；Release `0 error`，保留既有 `MediaCenterView.xaml.cs:699` 的两条 `CS8602` warning。
- 首次尝试中的一个既有 IPC 取消时序测试出现抖动；未改弱测试，单独复跑为 `1/1`，随后第二次完整 runner 未重现该失败。
- Playnite profile 日志记录 `ExtensionFactory:Loaded plugin: GameSaveCenter, version 0.6.73` 和 `WindowFactory:Show window Playnite.DesktopApp.Windows.MainWindowFactory`；同一运行期间 Playnite 进程 ID 为 `39900`，`MainWindowHandle=0`。

## 历史自动化窗口边界

在 runner 等待期间对同一隔离 PID 做了只读核对：

| 检查 | 结果 | 含义 |
| --- | --- | --- |
| `Get-Process Playnite.DesktopApp` | PID `39900`，`MainWindowHandle=0` | 没有可直接交互的主窗体句柄 |
| Win32 `EnumWindows` + `GetWindowThreadProcessId` | 没有该 PID 的顶层窗口 | 不是侧栏名称匹配失败，而是当前会话没有暴露窗口 |
| Windows UI Automation `RootElement.FindAll` 按 PID | `0` 个元素 | 无法取得侧栏、焦点、键盘或读屏树 |
| `Invoke-GameSaveCenterSidebar` | 未定位侧栏，等待约 90 秒超时 | 没有生成 `summary.json`，runner 输出 `[PARTIAL]` |

这组结果只能说明当前隔离宿主/桌面会话的窗口自动化入口不可用；不能据此断言 Playnite 产品窗口在所有宿主环境都不可见，也不能据此断言 GameSaveCenter 的视觉内容错误。

## 历史已取得与未取得的证据

当前输出目录为 [`artifacts/ui-host-audit-r23-04-uia-debug-20260922`](../../../../../artifacts/ui-host-audit-r23-04-uia-debug-20260922)。它包含绑定 `0157ace6` 的 `metadata.json`、`runner-metadata.json`、视觉树、资源快照、样式指纹、overflow 分类、嵌入式 Dashboard 视图和滚动回放。metadata 明确为 `CaptureOrigin=EmbeddedPlaynite`、`Mode=embeddedcurrent`、DPI `1.5`、Dashboard `1313.33 × 898 DIP`。

这些输出可以证明当前包进入了 Playnite 的嵌入承载路径，并用于核对受控结构/布局；不能替代 UIA/键盘/读屏、专用 Controlled host、最终 presented frame、ETW 或宿主真实性能证据。没有 `summary.json`，本批不补写任何 UIA 成功结果。

本机只有一个显示器，Q24-03 仍为 `blocked-single-display`；没有绕过 ETW/WPR/系统跟踪权限，也没有把代理/离屏截图写成物理跨屏或真实帧率。Demo 原目录不可用，继续参考恢复生产基线。

## 历史收口与下一步

- 隔离 Playnite 已用 `--shutdown --userdatadir` 正常关闭；本轮 profile、测试临时目录和未被本报告引用的构建缓存已清理，当前审计输出因被本报告引用而保留。
- R23-04 继续保持“已满足，待宿主环境验证”，不改成 UIA/Controlled host 已通过。
- 下一可执行项是取得一个稳定暴露 Playnite 主窗体给 UIA/受控交互的隔离桌面会话；若该外部条件仍不可用，转做依赖已满足的 R23-05 独立几何小批量。仍不能把离屏或代理数据写成真实 presented frame 性能。
