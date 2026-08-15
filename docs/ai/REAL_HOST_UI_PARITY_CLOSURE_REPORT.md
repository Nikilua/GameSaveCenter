# GameSaveCenter Real Host UI Parity Closure 报告

## A. Git

```text
start commit: 85902df
end commit:   （见 git log）
branch:       main
working tree: clean
```

## B. Tier A / Tier B

- Tier A：`scripts/capture-ui-audit.ps1` 保留，作为 deterministic regression（geometry/clipping/scroll/virtualization/Text-Fit/header fidelity）。
- Tier B：`scripts/real-host-audit.ps1` 新增；插件在 `GSC_REAL_HOST_AUDIT` 或 sentinel 请求下从真实 Dashboard 捕获证据。

离屏截图已明确标注为“Offscreen Regression Audit”，不是 production pixel truth；真实宿主截图才是视觉事实来源。

## C. 真实宿主证据（本机）

- 输出：`artifacts/ui-host-audit/`，ZIP：`artifacts/GameSaveCenter-ui-host-audit.zip`
- DPI：1.5（`DpiScaleX/Y=1.5`）
- Dashboard：1264×868；DetailsTabControl：965.33×663.33
- ThemeMode：FollowPlaynite；Glass：true/100；HighContrast：false
- SDK：6.16.0.0
- 已导出：metadata、resource-snapshot、style-fingerprints、visual-tree、6 个 workspace 截图/视觉树/layout

关键 runtime palette（与离屏 fallback 的差异）：

```text
Backdrop      #42080D14（alpha 0.26）
ControlFill   #E61B1F26（alpha 0.90）
GlassFill     #DB22262D -> #C71B1F26
GlassStrong   #F0262A31 -> #E01B1F26
ControlStroke #26FFFFFF
Accent        #FF0379FF（Playnite 宿主 accent）
```

结论：离屏截图“更漂亮”的主因是离屏使用 DesignTokens fallback palette，真实宿主使用 `AdaptiveThemePaletteFactory` 生成的 runtime palette，加上真实 DPI、真实 bounds、真实 host implicit style 链与真实数据。当前证据未显示 GSC surface hierarchy 被宿主压平，因此本轮不改 palette。

## D. 新增能力与测试

- `RealHostUiAuditService`：真实宿主捕获（Dashboard + Settings），不触发业务命令。
- `UiDiagnosticsExporters`：resource snapshot / style fingerprint / visual tree / PNG 导出。
- `AdaptiveThemePaletteContrastGuard`：palette 对比守卫（Light/Dark/FollowPlaynite 通过）。
- 单测：Playnite 281/281（新增 exporter/palette 测试）。

## E. 文档与协定

- README：区分 Offscreen Audit（Tier A）与 Real Host Audit（Tier B）。
- `docs/HOST_STYLE_DEPENDENCY_REPORT.md`：BasedOn `{x:Type}` 清单与分类。
- `docs/HOST_VISUAL_PARITY_REPORT.md`：paired evidence 与差异解释。
- AGENTS.md / DEVELOPMENT_HANDOFF：新增“每轮完成后由 Agent 自己 commit 并 push”项目协定。

## F. 验证

- Release 构建：0 warning / 0 error
- Core 59/59、Worker 190/190、Playnite 281/281
- validators：通过
- render-qa：11 档 + 56 主题 + 7 Resize 全绿
- Offscreen UI Audit：0 HIGH / 0 MEDIUM / 0 fidelity / 0 failed routes
- Real host capture：成功生成（见 C）

## G. 剩余风险

- 本机 Playnite 主窗口 UIA 树为空，脚本的侧栏自动点击在部分环境不可用；Dashboard 需要在侧栏打开后才会创建。
- Settings 自动捕获已接入代码，但本机最后一次重跑未自动打开 Dashboard；Settings paired evidence 需在下次真实运行中由脚本补充。
- 真实 DPI 125%/150%/175%/200%、第三方主题、高对比度、连续缩放仍需用户实际环境人工确认。
