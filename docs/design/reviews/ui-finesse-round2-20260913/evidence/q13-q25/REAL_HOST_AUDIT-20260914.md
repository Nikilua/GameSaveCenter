# 2026-09-14 隔离 Playnite 真实宿主审计

## 运行身份

- 采集代码基线：`6bae7c14cb93cfad992c230aedd5a458a10c0f34`（`6bae7c1`）。审计启动前工作树干净。
- 审计脚本：`scripts/real-host-audit.ps1 -Configuration Release`，使用 `D:\software\Playnite\Playnite.DesktopApp.exe`。
- 用户数据模式：`isolated-user-data`，目录为 `.tmp/ui-host-userdata-20260914`；仅复制原有 `config.json` 到隔离目录并创建空 `library`，未修改用户 Playnite 数据。
- 真实宿主日志确认 Playnite 启动并加载 `GameSaveCenter 0.6.73`；审计结束后已停止本次隔离 Playnite 与 Worker 进程。

## 已取得事实

- Release 构建：0 warning / 0 error。
- Worker 测试：311/311，通过且无跳过。
- Playnite 测试：474/531，通过，57 skipped，0 failed。
- 当前包已完成打包、隔离安装和 Playnite 启动；程序集身份为 `0.6.73+6bae7c14cb93cfad992c230aedd5a458a10c0f34`。
- 宿主 metadata：Playnite SDK `6.16.0.0`、Windows `10.0.22000.0`、`DpiScaleX/Y=1.5`、`PixelsPerDip=1.5`、Dashboard `1706.67×912 DIP`。
- Settings 截图的来源为 `EmbeddedPlaynite`，`EmbeddedSettingsCaptured=true`；截图和元数据位于 `artifacts/ui-host-audit-current-20260914/settings/embedded-current/`。
- 受控审计窗口生成了 Light/Dark、多尺寸、各页面和滚动面的截图；其 metadata 明确为 `CaptureOrigin=DedicatedAuditWindow`，不是 Playnite 嵌入 Dashboard。

## 未取得事实与结论

- UI Automation 未能定位 Playnite 左侧 `GameSaveCenter` 项；脚本等待 90 秒后仍未触发真实 `DashboardView.Loaded`。
- `summary.json` 明确记录：`EmbeddedDashboardCaptured=false`、`ControlledDashboardCaptured=true`、`ProductionVisualSourceOfTruthAvailable=false`、`HighGateCount=3`。
- 真实宿主日志同时记录 `REAL_EMBEDDED_DASHBOARD_NOT_CAPTURED`。因此本轮不能签收 Dashboard 的宿主像素、侧栏/页面交互、物理跨屏 Popup、键盘/IME/读屏或 Q25-02～Q25-05 性能证据。
- `EmbeddedSettingsCaptured=true` 只证明 Settings 在 Playnite 宿主路径中的一次截图可达，不代表六页 Dashboard 或其他宿主交互已通过。受控 Dashboard 截图仅用于保留当前构建的可视化诊断，不作为生产视觉真值。

原始输出：`artifacts/ui-host-audit-current-20260914/summary.json`、`metadata.json`、`runner-metadata.json`、`capture-manifest.json`、`settings/embedded-current/viewport/settings.png` 及隔离宿主日志。

## 主题复制修复后的第二次隔离复核（ed42868）

- 在提交 `ed428687f5cdedc64c753e22b85abfa411405be2` 上重新运行同一隔离流程，输出为 `artifacts/ui-host-audit-theme-20260914`，隔离用户数据为 `.tmp/ui-host-userdata-theme-20260914`；本次没有修改原 Playnite 用户数据。
- `runner-metadata.json` 记录原配置主题 `FusionX_54244ec8-29ec-418e-bce7-415250c8d67b`，并记录 `ConfiguredDesktopThemeCopied=true`。Playnite 日志确认插件 `GameSaveCenter 0.6.73` 加载、`WindowFactory:Show window` 出现，且不再出现主题缺失错误。
- 本次 Release 构建为 0 warning / 0 error，Worker 测试 `311/311`，Playnite 测试 `474/531`（57 skipped，0 failed）；程序集身份为 `0.6.73+ed428687f5cdedc64c753e22b85abfa411405be2`。metadata 继续记录 `DpiScaleX/Y=1.5`、`PixelsPerDip=1.5`、Dashboard `1706.67×912 DIP`。
- 第二次 `summary.json` 仍明确记录 `EmbeddedDashboardCaptured=false`、`EmbeddedSettingsCaptured=true`、`ControlledDashboardCaptured=true`、`ProductionVisualSourceOfTruthAvailable=false`、`EmbeddedDashboardOrigin=None`、`EmbeddedSettingsOrigin=EmbeddedPlaynite`、`HighGateCount=3`。Settings 的一次嵌入截图仍不代表 Dashboard 已嵌入。
- 对有效 Playnite 窗口句柄执行刷新后，UI Automation 仍只得到 `EmptyWindowAutomationPeer` 根节点，未得到任何后代节点，也未定位 `GameSaveCenter` 侧栏项；审计服务最终记录 `REAL_EMBEDDED_DASHBOARD_NOT_CAPTURED`。窗口取样仍不能提供可签收的生产 Dashboard 像素，因此受控窗口截图继续标记为 `DedicatedAuditWindow`。

本次复核排除了“隔离宿主缺少用户主题目录”这一脚本缺陷，但没有改变真实 Dashboard 未捕获的结论。Q00-08/Q25-07 的主题复制、构建、打包、隔离安装和插件启动事实已更新；真实 Dashboard 页面、物理跨屏、IME/读屏以及 Q25-02～Q25-05 的 ETW/调用栈/30 分钟耐久/低 Tier 证据仍未完成。

最新输出：`artifacts/ui-host-audit-theme-20260914/summary.json`、`metadata.json`、`runner-metadata.json`、`capture-manifest.json`、`settings/embedded-current/viewport/settings.png`、`.tmp/ui-host-userdata-theme-20260914/playnite.log` 与 `extensions.log`。

## 物理显示器前置检查

- 在同一台实际 Windows 主机上通过 `System.Windows.Forms.Screen.AllScreens` 枚举显示器，结果只有主屏 `\\.\\DISPLAY1`，边界 `2560×1440`、工作区 `2560×1368`；没有第二个物理显示器或可迁移的跨屏目标。
- 因此本轮没有执行“迁移宿主窗口并在打开态 Popup 中跨屏”的操作，也没有生成跨屏通过结论。Q24-03 继续保留为未完成/待宿主条件，不以单屏枚举替代多屏行为证据。

## 窗口句柄刷新修复后的第三次复核（a04a824）

- 在提交 `a04a8249a2411c78f0580d67ff195ef27afaa40d` 上运行干净隔离流程，输出为 `artifacts/ui-host-audit-refresh-final-20260914`，隔离用户数据为 `.tmp/ui-host-userdata-refresh-final-20260914`。本轮在侧栏 UIA 探测前对 Playnite 进程调用 `Process.Refresh()`，用来排除启动早期缓存 `MainWindowHandle=0` 的脚本误判。
- 干净流程完成 XAML `24/24`、Release 构建 `0 warning / 0 error`、Core `82/82`、Worker `311/311`、Playnite `474/531`（57 skipped，0 failed）；打包、程序集身份核对和隔离安装均成功，身份为 `0.6.73+a04a8249a2411c78f0580d67ff195ef27afaa40d`。主题元数据仍为 `ConfiguredDesktopThemeCopied=true`，150% DPI 与 Dashboard `1706.67×912 DIP` 可追溯。
- Playnite 日志确认 `GameSaveCenter 0.6.73` 加载并出现 `WindowFactory:Show window`；插件审计日志确认先等待真实侧栏，之后进入受控专用窗口 fallback。侧栏探测在刷新句柄后仍未找到 `GameSaveCenter`，但这次延迟生成的 `summary.json` 已完整记录：`EmbeddedDashboardCaptured=false`、`EmbeddedSettingsCaptured=true`、`ControlledDashboardCaptured=true`、`ProductionVisualSourceOfTruthAvailable=false`、`EmbeddedDashboardOrigin=None`、`EmbeddedSettingsOrigin=EmbeddedPlaynite`、`HighGateCount=3`。
- 本轮 Playnite 还记录了联网更新清单/Addon blacklist 的 TLS 失败；这没有阻止插件加载或审计服务运行，也不被记作插件代码错误。审计结束后已停止本轮隔离 Playnite 与 Worker，原用户数据未修改。

该复核排除了“启动早期句柄缓存导致未探测”的审计脚本缺口，但没有改变真实 Dashboard 未捕获的结论；受控窗口仍标记 `DedicatedAuditWindow`，不升级为生产视觉真值。Q24-03 的单屏条件边界以及 Q25-02～Q25-05 的实际呈现帧、调用栈、30 分钟耐久和低性能 Tier 证据仍未完成。

最新输出：`artifacts/ui-host-audit-refresh-final-20260914/summary.json`、`metadata.json`、`runner-metadata.json`、`capture-manifest.json`、`settings/embedded-current/viewport/settings.png`、`.tmp/ui-host-userdata-refresh-final-20260914/playnite.log` 与 `extensions.log`。

## 宿主原生侧栏命令修复后的第四次复核（2250719）

- 在提交 `2250719b728f6ddee32233f9ff456b8ea1b8fc7d` 上运行干净隔离流程，输出为 `artifacts/ui-host-audit-reflection-final-20260914`，隔离用户数据为 `.tmp/ui-host-userdata-reflection-final-20260914`。审计入口在真实宿主启动后通过 Playnite 自身的 `SelectSidebarViewCommand` 选择 `GameSaveCenter`，没有创建专用 Dashboard 窗口或把 fallback 截图升级为嵌入证据。
- 干净流程完成 XAML `24/24`、Release 构建 `0 warning / 0 error`、Core `82/82`、Worker `311/311`、Playnite `474/531`（57 skipped，0 failed）；打包、程序集身份核对、隔离安装和真实 Playnite 启动均成功，身份为 `0.6.73+2250719b728f6ddee32233f9ff456b8ea1b8fc7d`。`runner-metadata.json` 记录 `ConfiguredDesktopThemeCopied=true`，宿主 metadata 记录 150% DPI（`DpiScaleX/Y=1.5`、`PixelsPerDip=1.5`）。
- Playnite/插件日志形成闭环：插件加载后先记录等待侧栏，随后记录 `Real host audit invoking Playnite's own SelectSidebarViewCommand for GameSaveCenter`，再记录 `EmbeddedPlaynite Dashboard capture` 开始；`summary.json` 明确为 `EmbeddedDashboardCaptured=true`、`EmbeddedSettingsCaptured=true`、`ControlledDashboardCaptured=false`、`ProductionVisualSourceOfTruthAvailable=true`、`EmbeddedDashboardOrigin=EmbeddedPlaynite`、`EmbeddedSettingsOrigin=EmbeddedPlaynite`、`HighGateCount=1`。
- `capture-manifest.json` 记录 27 个 Dashboard 视口（六个工作区及其 Tab）、2 个 Dashboard 完整滚动面和 1 个 Settings 视口；Dashboard 视口为 `1298.67×900 DIP`、`1948×1350 px`、150% 渲染，滚动面均为 `CapturedAndValidated`，Settings 为 `1278×762 DIP`、`1917×1143 px`。当前证据首次提供了真实 Playnite 嵌入 Dashboard 的生产像素来源，覆盖当前深色宿主状态。
- 运行器仍打印“UI Automation 未找到 GameSaveCenter 侧栏项”的旧探测警告；该警告不是本次最终判定依据，因为宿主原生命令调用和插件日志均成功，且捕获元数据由 `Window.GetWindow` 真实性判断标记为 `EmbeddedPlaynite`。这次没有取得 Hover/Focus、键盘/IME、读屏、多屏 Popup、浅色/高对比或低性能 Tier 的宿主操作证据。
- 审计结束后已明确停止隔离 Playnite 与 Worker，进程复查无残留；原用户 Playnite 数据未修改。Q24-03 仍受当前仅有 `DISPLAY1` 单屏限制，Q25-02～Q25-05 仍受 ETW/呈现帧、调用栈、30 分钟时间序列和低 Tier 实测边界限制。

最新输出：`artifacts/ui-host-audit-reflection-final-20260914/summary.json`、`metadata.json`、`runner-metadata.json`、`capture-manifest.json`、`embedded-current/dashboard/viewport/`、`embedded-current/dashboard/scroll-surfaces/`、`settings/embedded-current/viewport/settings.png`、`.tmp/ui-host-userdata-reflection-final-20260914/playnite.log` 与 `extensions.log`。
