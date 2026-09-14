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
