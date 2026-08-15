# Real Host Capture Completeness Fix — 实施报告

来源：`GameSaveCenter_RealHost_Capture_Completeness_Fix_Prompt.zip`
基线：远端 `main`（最新已同步），本轮仅改 UI Audit / Real Host Capture，不改 production UI、DataGrid 虚拟化、Typography。

## A. 为什么旧截图右/底被切

- 旧 `CaptureDashboardAtSizeAsync` 同时设置 `Window.Width/Height = profile` 和 `Dashboard.Width/Height = profile`。
- WPF `Window.Width/Height` 是 outer size，client content 一定更小；Dashboard 又被迫等于 outer，必然超出 client 被父容器 clip。
- 真实 embedded 场景中父容器是 Playnite 自己，改 Dashboard 尺寸既不能放大宿主，也会造成 clip。

## B. 为什么旧 full-page 没有 sidebar/header

- 旧 `SaveFullPage` 只做一件事：找 `ExtentHeight` 最大的一个 `ScrollViewer`，再调用 `SaveScrollViewerFull`。
- 因此所谓 full page 只是单个 ScrollViewer 的内容；左 sidebar、Dashboard header、同页其它 scroller 都不会出现。
- 已删除该函数，不再用“最大 scroller”代表整页。

## C. 新截图类型

1. `embedded-current/viewport/*.png` — EmbeddedCurrentViewport：真实 Playnite 当前 Dashboard 一屏完整可见内容，包含 shell/sidebar/header/右边界/底部当前 viewport；不修改 Dashboard 尺寸、主题、宿主窗口。
2. `controlled/<profile>/<theme>/viewport/*.png` — ControlledViewport：无边框审计窗口，profile 即 Dashboard client size，Dashboard Stretch；用于 1024x768/1280x720/1366x768/1600x1000/maximized × light/dark 响应式回归。
3. `scroll-surfaces/<route>__<name>.png` — ScrollSurfaceFull：单个具名/自动编号 ScrollViewer 的完整 extent，不代表整个 Dashboard。

## D. 哪个是真实视觉 Source of Truth

- 只有 `embedded-current/viewport` 才是 production visual source of truth。
- 本次运行如果使用 Dedicated Audit Window fallback（headless 会话），输出为 `controlled-host-window`，metadata 会写 `REAL EMBEDDED CAPTURE NOT AVAILABLE` 状态，绝不把 controlled window 冒充 embedded。

## E. 截图完整性如何自动证明

- `SaveViewport` 校验输出 PNG 尺寸 == `ceil(Actual * DpiScale)`（±2px），否则写 `CAPTURE_VIEWPORT_CLIPPED` gate。
- Controlled host 校验 `Dashboard.Actual ≈ profile`（±2px），否则写 `CAPTURE_PROFILE_SIZE_MISMATCH` gate。
- 右下边界 sentinel 几何通过 `IsBoundsWithinViewport` 断言（bounds 必须落在 viewport 内）。
- `CaptureScrollSurfaces` 枚举所有 meaningful ScrollViewer（visible、>=60px、可滚动），逐个输出并记录 viewport/extent/segment 数到 manifest。
- `capture-manifest.json` 记录每张图的 CaptureType/Origin/Route/DIP/DPI/RenderScale/输出像素/ScrollerName/Extent/Segment/CompletenessValidated。

## 本轮证据

- 回归测试：`UiAuditCaptureContractTests` 6 项；Playnite 287/287、Worker 191/191、Core 59/59。
- 端到端产物：`artifacts/ui-host-audit/`，共 603 个文件，zip `artifacts/GameSaveCenter-ui-host-audit.zip`（约 140MB）。
- 最终运行无 `CAPTURE_VIEWPORT_CLIPPED`、`CAPTURE_PROFILE_SIZE_MISMATCH` gate（无 gates 目录）。
- Controlled 尺寸实测：1024x768、1280x720、1366x768 全部精确命中；1600x1000 受工作区高度限制为 1600x912、maximized 1706.67x912；viewport 像素 = DIP × 1.5（如 1024x768 → 1536x1152，maximized → 2560x1368），metadata `ProfileSizeApplied=True`。
- 滚动面：`scroll-surfaces/*__<name>.png` 共 197 张，覆盖各工作区/Tab 的具名或自动编号 ScrollViewer，manifest 记录 viewport/extent/segment/completeness；超过 60 段的超大虚拟化表诚实标记 `CompletenessValidated=false` 且不阻塞整轮。
- Settings 托管区分：本次设置页由 Playnite 托管，metadata `mode=embedded-current-settings`、`origin=EmbeddedPlaynite`、`ThemeOverrideApplied=false`，不再误报 controlled profile 断言。
- 环境说明：Dashboard 本轮使用 Dedicated Audit Window fallback（headless 会话），因此 Dashboard 输出为 `controlled-host-window`；真实 embedded-current 仅当 Playnite 直接托管 Dashboard 时生成，metadata 会如实区分，绝不冒充。
