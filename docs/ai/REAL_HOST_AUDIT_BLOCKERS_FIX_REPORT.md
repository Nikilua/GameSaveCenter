# Real Host Audit Blockers — 实施报告

来源：`GameSaveCenter_RealHost_Audit_Blockers_Prompt.zip`
范围：只修 Audit 可信度（Git/revision、Embedded 捕获握手、命名、overflow gate、resize 稳定、manifest scope、内部滚动器过滤），未改 production UI。

## Git / Revision

- start commit：`c6c790e`；end commit：`d7948c9`；branch：`main`；remote：`github.com/Nikilua/GameSaveCenter.git`。
- `real-host-audit.ps1` 现在启动前写入 `GSC_UI_AUDIT_COMMIT = git rev-parse HEAD`。
- 插件 metadata 优先级：`GSC_UI_AUDIT_COMMIT` → AssemblyInformationalVersion → `unknown`；unknown 时写 `AUDIT_SOURCE_REVISION_MISSING` HIGH gate。
- 本次产物 summary：`CommitSha = d7948c95451e9dd48be72dfa781d7ef283a49d38`（与 git HEAD 一致）。

## Embedded

- `DashboardView.AuditHostKind` 显式标记创建路径（sidebar=EmbeddedPlaynite、fallback=ControlledAuditWindow）。
- `IsGenuinelyEmbeddedDashboard`：要求 `IsLoaded`、`PresentationSource.FromVisual != null`、`Window.GetWindow != null` 且不是 audit fallback 窗口；不再用 static null 猜 origin。
- 本机 headless 会话无人点击侧栏：`EmbeddedDashboardCaptured=false`、`EmbeddedSettingsCaptured=true`、`ControlledDashboardCaptured=true`、`ProductionVisualSourceOfTruthAvailable=false`，并写 `REAL_EMBEDDED_DASHBOARD_NOT_CAPTURED` HIGH gate。
- 有用户点击侧栏后，`DashboardView.OnLoaded` 会以 EmbeddedPlaynite 独立完成 embedded capture，不再被 controlled 抢占（CompletedRoots 按 kind 拆分）。

## SafeFileName

- 根因：`string.Join("-", chars)` 会在每个字符间插入 `-`。
- 修复：仅把非法文件名字符替换为 `-`，连续 `-` 折叠，trim 首尾 `-`/`.`/空白，保留中文。
- 实测产物：`tab-保留策略.png`、`workspace-overview` 风格文件名恢复正常；新增中英文用例测试。

## Overflow gate 分类

- `ClassifyOverflow`：`RealFixedLayoutOverflow` / `IntentionalScrollableOverflow` / `DecorativeOverflow` / `AuditFalsePositive`。
- ScrollViewer/ItemsPresenter/DataGrid 内容越界 → scrollable，不报 HIGH；Blur/DropShadow/IsHitTestVisible=false → decorative，不报 HIGH；固定 Card/Button 越界 → `CHILD_LAYOUT_OVERFLOW` HIGH。
- 本次分类统计（settings 内嵌窗口）：real=0、scroll=1、decorative=0、false-positive=0；分类明细写入 `gates/overflow-classification.json`。

## Resize 稳定

- `StabilizeControlledLayoutAsync`：每次切尺寸后最多 3 pass，每 pass 等待 DataBind/Loaded/Render/Idle 并重排；连续两次关键几何差 ≤0.5 DIP 才截图。
- 记录 `layout/controlled/<size>/<theme>/responsive-stable.json`（ResponsivePassCount/ResponsiveStable/关键宽度）。

## Scroll surfaces / manifest

- 默认排除 `DG_ScrollViewer`、`PART_ContentHost`、TextBox/ComboBox 内部 scroller（`IsInternalTemplateScroller`）。
- ScrollSurface manifest 写真实 OutputWidthPx/OutputHeightPx/Viewport/Extent/Segment/CaptureStatus/Reason；CompletenessValidated 只在真实校验后为 true。
- Manifest 作用域：`controlled/capture-manifest.json`（Dashboard 363 条）、`settings/capture-manifest.json`（Settings 10 条）、根 `capture-manifest.json` aggregate；每条含 `Scope`。

## 测试

- 新增 `UiAuditBlockerTests`（8 项）：SafeFileName、fallback 窗口显式比较、fixed overflow、scroll overflow、decorative overflow、TextBox 内部 scroller、manifest/gates scope、embedded identity。
- Playnite 302/302、Worker 191/191、Core 59/59；Release 0 warning/0 error；`validate-source.py`、`check-xaml.ps1` 通过。

## 最终证据

- ZIP：`artifacts/GameSaveCenter-ui-host-audit.zip`（约 132MB，08:16 生成）。
- `summary.json`：CommitSha=d7948c9、EmbeddedDashboardCaptured=false（本机无人点击）、EmbeddedSettingsCaptured=true、ControlledDashboardCaptured=true、ProductionVisualSourceOfTruthAvailable=false、HighGateCount=4。
- 有真实用户点击后重跑 `scripts/real-host-audit.ps1`，脚本会等待 embedded 并输出 `[OK]`；未捕获时输出 `[PARTIAL]` 并以退出码 2 结束。
