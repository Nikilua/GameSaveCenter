# Host Audit Truthfulness Fix — 实施报告

来源：`GameSaveCenter_HostAudit_Truthfulness_Fix_Prompt.zip`
范围：只修 Real Host Audit 基础设施（origin、scroll semantics、manifest、success state），未改 production UI/字体/布局/业务。

## 1. 为什么上一版 Dashboard 全是 DedicatedAuditWindow

- 插件启动时调用 `SidebarItem.Activated`，但该项是 `SiderbarItemType.View`；Playnite 对 View 只在用户点击时调用 `Opened`，`Activated` 不会打开 View。
- 因此真实 embedded Dashboard 从未加载，6 秒后必然走 `EnsureDashboardCaptured` 专用窗口，产出全部是 `DedicatedAuditWindow`。

## 2. 为什么调用 SidebarItem.Activated 对 View 无效

- Playnite SDK：`Button` 用 `Activated`；`View` 由 Playnite 在用户激活后调用 `Opened` 并把返回 Control 加载为 sidebar view。
- 本轮已删除 `auditSidebarItem?.Activated?.Invoke()`，改为通知用户手动打开侧栏，并等待真实 `DashboardView.OnLoaded`。

## 3. 为什么 Settings 能 embedded、Dashboard 不能

- Settings 通过 `OpenPluginSettings` 由 Playnite 真实承载设置窗口，`GameSaveCenterSettingsView.OnLoaded` 成功触发捕获。
- Dashboard 依赖 sidebar View 打开；自动打开路径错误导致从未 embedded 加载。
- 本轮把 Dashboard 与 Settings 的捕获状态分开记录（`EmbeddedDashboardCaptured` / `EmbeddedSettingsCaptured`）。

## 4. 为什么 SaveHistory DG ScrollSurfaceFull 出现黑块和重复 Chip

- `CanContentScroll=true` + `ScrollUnit=Item` 时，ScrollViewer 的 offset/viewport/extent 是逻辑 item 单位，不是像素。
- 旧 `SaveScrollViewerFull` 把这些值直接当像素裁剪，产生错误切片。

## 5. DataGrid ScrollUnit=Item 与 pixel stitch 的单位冲突

- 逻辑滚动单位下 `ViewportHeight=4`、`ExtentHeight=4472` 表示 4/4472 个 item，不是像素高度。
- 像素 stitch 需要 DIP 单位；两者混用必然错位。

## 6. 为什么 settings manifest 有 475 条、实际 Settings 只有 10 条

- 旧 `CaptureManifest` 是 static 全局 List，Settings 写 manifest 时把 Dashboard 的 465 条一起写进去。
- 本轮改为 `AuditCaptureSession`：EmbeddedDashboard / ControlledDashboard / Settings 三个独立列表，每次输出根独立 session，重复运行不串数据。

## 7. 新 session/manifest 如何隔离

- `AuditCaptureSession` 按 output root 创建；`TryCaptureDashboard` 每次新请求重置 session。
- `settings/capture-manifest.json` 只含 Settings entries；`controlled/capture-manifest.json` 只含 Controlled Dashboard；`embedded-current/dashboard/capture-manifest.json` 只含 Embedded Dashboard；根 `capture-manifest.json` 是 aggregate。

## 8. Controlled screenshot 内部 child overflow 如何与 screenshot clipping 区分

- 保留 root bitmap 尺寸 gate（`CAPTURE_VIEWPORT_CLIPPED`）。
- 新增 `CHILD_LAYOUT_OVERFLOW`：对具名 child 计算相对 Dashboard 的 bounds，若超出 root 尺寸则写 MEDIUM gate。
- 本轮最终包中仅出现 `CHILD_LAYOUT_OVERFLOW`（Settings 内嵌窗口确有子元素溢出），没有 `CAPTURE_VIEWPORT_CLIPPED`。

## 9. 新 ZIP 是否真的有 Embedded Dashboard

- 本机 headless 会话无人工点击，`summary.json` 如实写 `EmbeddedDashboardCaptured=false`、`VisualSourceOfTruthAvailable=false`。
- Settings 为真实 embedded（`EmbeddedSettingsCaptured=true`），Controlled Dashboard 已生成。
- 有真实用户点击侧栏后，Dashboard `OnLoaded` 会以 `AuditHostKind=EmbeddedPlaynite` 独立完成 embedded 捕获，不再被 controlled 抢占。

## 10. 哪个目录现在才是 production visual source of truth

- 只有 `embedded-current/dashboard/viewport/` 是 production visual source of truth。
- `controlled/*` 是响应式回归证据；`scroll-surfaces/*` 只代表单个滚动区域，都不是真实宿主视口。

## 本轮实现摘要

- `GameSaveCenterPlugin`：删除 View 的 `Activated` 调用；审计就绪时发 Playnite 通知并等待用户打开；fallback 专用窗口创建的 Dashboard 显式标记 `AuditHostKind.ControlledAuditWindow`。
- `DashboardView`：新增 `AuditHostKind`（默认 `EmbeddedPlaynite`），origin 不再靠 static null 猜测。
- `RealHostUiAuditService`：`CompletedRoots` 按 kind 拆分；`AuditCaptureSession` 隔离 manifest；`summary.json` 硬门禁；`ScrollSurfaceStatus`（CapturedAndValidated/CapturedUnvalidated/SkippedVirtualized/SkippedTooLarge/Failed）；虚拟化 DataGrid 逻辑滚动器不生成像素长图；`DG_ScrollViewer`/`PART_ContentHost` 排除；滚动面写入真实 OutputWidth/Height、viewport/extent/segment、Reason；`CHILD_LAYOUT_OVERFLOW` gate。
- `real-host-audit.ps1`：打印 WAITING/PARTIAL/OK；Embedded 未捕获时退出码 2，绝不宣称成功。

## 最终证据（本机无人工点击）

- `artifacts/ui-host-audit/summary.json`：`EmbeddedDashboardCaptured=false`、`EmbeddedSettingsCaptured=true`、`ControlledDashboardCaptured=true`、`VisualSourceOfTruthAvailable=false`。
- `settings/capture-manifest.json`：10 条，Origin=EmbeddedPlaynite。
- `controlled/capture-manifest.json`：363 条，Origin=DedicatedAuditWindow。
- Scroll surfaces：86 条 `CapturedAndValidated`、15 条 `CapturedUnvalidated`（含 Reason），真实 OutputWidth/Height。
- 无 `DG_ScrollViewer` PNG；gates 仅 `CHILD_LAYOUT_OVERFLOW`。
- ZIP：`artifacts/GameSaveCenter-ui-host-audit.zip`（约 132MB）。

## 回归

- Playnite 294/294、Worker 191/191、Core 59/59；Release 0 warning/0 error；`validate-source.py`、`check-xaml.ps1` 通过。
