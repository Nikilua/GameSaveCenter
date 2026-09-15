# Q00 受控证据索引

采集日期：2026-09-15（Asia/Shanghai）。本索引只收录 Q00 的受控 WPF 证据；它不替代真实 Playnite 宿主、物理 DPI、读屏或屏幕呈现帧证据。

## 2026-09-15 深色设置校验标题回归

当前提交 `0648689a1ac74f43ec918e051f80037a4ff2d20d` 的 clean-tree RenderHarness 发现并修复了一个真实暗色漏检：设置页 `SettingsValidationDetails` 的 Expander 标题没有显式主题前景，虽然页面根节点已有前景，模板仍可能回落为暗色默认黑字。修复在该共享节点直接使用 `DynamicResource GscPrimaryTextBrush`，并由 `SettingsValidationSourceTests` 锁定同节点属性。

- [深色设置页 1040×700](q00/Q00-settings-dark-1040x700.png)：错误摘要、定位按钮和“查看错误详情”标题在暗底上保持可读；这是修复后的当前提交截图。
- [浅色无效状态 1040×700](q00/Q00-settings-invalid-light-1040x700.png)：同一错误摘要状态的浅色对照。
- [完整 RenderHarness 报告](q00/Q00-render-qa-20260915.txt)：`Commit=0648689a1ac74f43ec918e051f80037a4ff2d20d`、`WorkingTreeClean=True`、`DpiScale=1.00`；Light/Dark 的 Settings 1040×700、1100×720、1366×768、2560×1440 均为 `OK`，并包含 normal/dirty/invalid 设置状态夹具。

这组新截图证明的是离屏 DIP 视觉修复，不是 Playnite 实机窗口、物理缩放、读屏或打开态交互证明；宿主边界仍按本索引末尾记录。

## 2026-09-15 审计门禁统计假阳性修复

`f1ea52a` 修正 `RealHostUiAuditService` 对 `gates/*.json` 的统计：`overflow-classification.json` 是诊断分类报告，不再计为阻断门禁；`UiAuditTruthfulnessTests.OverflowClassificationReportIsNotCountedAsBlockingGate` 在当前 Release 二进制下 `1/1` 通过。既有真实宿主产物的 `gates/` 目录只有该文件，因此按修复规则阻断门禁数为 `0`，但旧 `summary.json` 的 `HighGateCount=1` 原始值保持不变。

最新 `cc63523` 宿主重跑因 Playnite CEF 初始化访问拒绝未生成 Dashboard/Settings，不替代历史 `69e1f84` 的真实宿主像素，也不能把未捕获的宿主启动写成 Q00 最终视觉通过；详见 [`REAL_HOST_AUDIT-FP-FIX-20260915.md`](q13-q25/REAL_HOST_AUDIT-FP-FIX-20260915.md)。

## 运行身份

- 来源：`tests/GameSaveCenter.RenderHarness` 的 `finesseprobe`，ProductionResourceDictionary/`UiFrameworkProbeView`，STA 离屏布局。
- 窗体：1120×980 DIP；RenderTargetBitmap 96 DPI，DpiScale=1.00；数据为合成中英文、数字、路径、诊断、状态与 4 行表格。
- 代码基线：提交 `87a40c8`（父提交 `f501476`）；包含 Q00 共享前景修复、有效视觉树对比门禁、四行裁剪负例和 8 位隔离构建 token 修复。
- 夹具源码身份：`tests/GameSaveCenter.RenderHarness` 与本索引所引用的报告同属提交 `87a40c8`；本轮后续文档/测试变更不回写或冒充该历史截图、报告的代码来源。
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
