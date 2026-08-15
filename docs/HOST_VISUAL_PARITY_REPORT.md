# Host Visual Parity Report

维护时间：2026-08-15

## Audit 分层

### Tier A — Offscreen Regression Audit

命令：`scripts/capture-ui-audit.ps1`

- 独立 WPF Application + RenderHarness + FakeDashboardData；
- 直接实例化 Workspace，不进入真实 `DashboardView` 外壳；
- 不调用 `ApplyAdaptiveTheme()`，因此展示的是 DesignTokens/ResourceDictionary fallback palette；
- DPI 固定 96（metadata `DpiScale=1.0`）；
- 尺寸由 synthetic `ContentSize` 公式模拟。

用途：deterministic geometry/clipping/scroll/virtualization/Text-Fit/header fidelity 回归。
定位：**不是 production pixel truth**。

### Tier B — Real Playnite Host Fidelity Audit

命令：`scripts/real-host-audit.ps1`

- 真实 Playnite Desktop 进程加载插件；
- 从真实已加载 `DashboardView` 捕获；
- `ApplyAdaptiveTheme()` 已执行，runtime palette 已写入 Resources；
- 记录真实 DPI（`VisualTreeHelper.GetDpi`）、Dashboard/DetailsTabControl bounds、ThemeMode/Glass/HighContrast；
- 导出 resource snapshot、style fingerprints、visual tree、screenshots、zip；
- 只切换 workspace/tab，不触发备份、恢复、删除、下载、上传等业务命令。

定位：**production visual source of truth**。

## 已知差异（解释“离屏截图更好看”）

1. Palette：离屏使用静态 DesignTokens fallback；真实宿主使用 `AdaptiveThemePaletteFactory` 生成的 runtime palette（随 Playnite theme/glass/high-contrast 变化）。
2. Host：离屏没有真实 Dashboard shell、Playnite theme implicit styles、MainWindow 祖先资源链。
3. DPI：离屏固定 1.0；真实宿主按显示器缩放。
4. Data：离屏是 FakeDashboardData；真实宿主是用户真实数据/状态。
5. Rendering：ClearType、设备渲染、动画帧、transparency 效果不同。

## 当前状态

- Tier A 保留并继续作为回归门禁。
- Tier B 基础设施已加入插件（`GSC_REAL_HOST_AUDIT` / sentinel 触发），输出结构见 `REAL_HOST_OUTPUT_REQUIREMENTS.md` 与 `artifacts/ui-host-audit/`。
- 本机已成功生成一组真实宿主证据（`artifacts/ui-host-audit/` 与 `artifacts/GameSaveCenter-ui-host-audit.zip`）。
- 尚未发现需要修改 production palette 的证据；只有 Tier B 输出确认“surface hierarchy/border/text contrast 被宿主压平”后才允许改 `AdaptiveThemePaletteFactory`。

## 本机实测（2026-08-15，真实 Playnite）

metadata.json：

```text
DpiScaleX/Y = 1.5
Dashboard = 1264 x 868
DetailsTabControl = 965.33 x 663.33
ThemeMode = FollowPlaynite
GlassEnabled = true / GlassStrength = 100
HighContrast = false
Playnite SDK = 6.16.0.0
```

resource-snapshot.json 关键 runtime palette：

```text
GscBackdropBrush      = #42080D14（alpha 0.26）
GscControlFillBrush   = #E61B1F26（alpha 0.90）
GscGlassFillBrush     = LinearGradient #DB22262D -> #C71B1F26（约 0.78-0.86 alpha）
GscGlassStrongBrush   = LinearGradient #F0262A31 -> #E01B1F26（约 0.88-0.94 alpha）
GscControlStrokeBrush = #26FFFFFF（alpha 0.15）
GscPrimaryTextBrush   = #FFFFFFFF
GscSecondaryTextBrush = #BDFFFFFF（alpha 0.74）
GscMutedTextBrush     = #8FFFFFFF（alpha 0.56）
GscAccentBrush        = #FF0379FF（Playnite 宿主 accent）
GscPrimaryButtonBrush = #FF0379FF -> #FF0366D6
GscSurfaceEffect      = DropShadowEffect（glass 开启）
```

对照：离屏 Tier A 使用 DesignTokens.xaml 的静态 fallback（例如 accent `#7897FF`、Backdrop 不透明、Glass 默认 alpha），因此“离屏更精致”主要来自：

1. 真实宿主运行了 `AdaptiveThemePaletteFactory`，palette 随 Playnite 主题/accent 变化；
2. 真实宿主 DPI 为 1.5，尺寸与离屏 synthetic ContentSize 不同；
3. 真实宿主包含 Dashboard 外壳与 Playnite 隐式样式链；
4. 真实数据/状态与 FakeDashboardData 不同。

这属于“预期环境差异 + runtime palette 差异”，当前证据未显示 GSC surface hierarchy 被宿主压平，因此本轮不改 palette。
