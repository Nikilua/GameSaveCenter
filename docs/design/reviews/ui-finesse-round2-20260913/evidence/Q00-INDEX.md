# Q00 受控证据索引

采集日期：2026-09-14（Asia/Shanghai）。本索引只收录 Q00 的受控 WPF 证据；它不替代真实 Playnite 宿主、物理 DPI、读屏或屏幕呈现帧证据。

## 运行身份

- 来源：`tests/GameSaveCenter.RenderHarness` 的 `finesseprobe`，ProductionResourceDictionary/`UiFrameworkProbeView`，STA 离屏布局。
- 窗体：1120×980 DIP；RenderTargetBitmap 96 DPI，DpiScale=1.00；数据为合成中英文、数字、路径、诊断、状态与 4 行表格。
- 代码基线：提交 `87a40c8`（父提交 `f501476`）；包含 Q00 共享前景修复、有效视觉树对比门禁、四行裁剪负例和 8 位隔离构建 token 修复。
- 采样方式：从已排列的 WPF 视觉树读取最终 `TextBlock.Foreground`，累乘祖先 `Opacity`，以渲染像素四角采样表面，先做 alpha 合成，再以未舍入值计算对比度。
- 负例：固定注入 `#000000` 文字到 `#252A34` 暗底，必须得到 1 个低于 4.5:1 的 violation。

## Q00-01～Q00-05

| 主题 | 深色 | 浅色 |
| --- | --- | --- |
| 受控截图 | [q00-after-dark.png](q00-after-dark.png) | [q00-after-light.png](q00-after-light.png) |
| 受控报告 | [q00-after-dark.txt](q00-after-dark.txt) | [q00-after-light.txt](q00-after-light.txt) |
| 有效文本样本 | 12；0 violation | 12；0 violation |
| 黑字负例 | 1 violation，门禁通过 | 1 violation，门禁通过 |
| 实际字体证据 | `FontCandidate` 仅为候选覆盖；`FontActualGlyphRun=unknown` | 同左 |

共享实现位于 `Themes/Typography.xaml`、`Themes/WpfUiProduction.xaml`、`Themes/Redesign.xaml`、`Infrastructure/AdaptiveThemePalette.cs` 与 `Infrastructure/AdaptiveThemePaletteContrastGuard.cs`。修复包括 Numeric/按钮/Toggle/状态胶囊的动态前景、按钮派生样式前景、Toggle 内容前景绑定、禁用态合成对比，以及不带布局副作用的主按钮渐变端点调整。

## Q00-04

`RowsEffective` 在两个主题均为 `realized=4 completeInsideGrid=4`，表格有效区域为 250 DIP；故意把视口底部压缩 4 DIP 后为 `completeInsideGrid=3`，`RowsNegativeFixture` 门禁通过。该结果证明了行容器与裁剪交集检查，不把 `Items.Count=4` 当作完整可读行证明。

## Q00-05

`FontHasGlyph` 的输出已改名为 `FontCandidate`。它只表示候选 Typeface 的字符覆盖，不能代表 WPF 实际 GlyphRun 的回退结果；本离屏探针未抓取 GlyphRun，因此报告显式保留 `unknown`，罕见扩展 CJK 不被假报为已解决。

## 尚未取得的证据

- 当前未运行成功的真实 Playnite 宿主仍为 `MainWindowHandle=0`，不能宣称宿主呈现或真实窗口 DPI 通过。
- 截图为 DIP/离屏结果，不是 125/150/175/200% 物理 DPI 证据；Hover、Pressed、Keyboard Focus 的输入序列仍需独立行为探针。
