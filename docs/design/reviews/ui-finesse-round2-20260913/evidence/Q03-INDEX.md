# Q03 语义色与主题受控证据

采集日期：2026-09-14（Asia/Shanghai）。本索引记录共享 `AdaptiveThemePalette`、`ContrastGuard` 与生产模板的受控 WPF 证据；截图仍为 DIP/离屏结果，不替代真实 Playnite 宿主、Popup/Dialog/Tooltip 热切换或物理 DPI。

## 运行身份

- 代码基线：本阶段提交 SHA 在提交后补入；夹具为 `tests/GameSaveCenter.RenderHarness` 的 `finesseprobe`，STA、1120×980 DIP、96 DPI、DpiScale=1.00。
- 双主题截图：[dark/ui-finesse-fixture.png](q03/dark/ui-finesse-fixture.png)、[light/ui-finesse-fixture.png](q03/light/ui-finesse-fixture.png)。原始报告：[dark/ui-finesse-fixture-report.txt](q03/dark/ui-finesse-fixture-report.txt)、[light/ui-finesse-fixture-report.txt](q03/light/ui-finesse-fixture-report.txt)。
- 测量先保留原始 alpha，按层合成后再计算对比度；报告不把格式化后的三位小数当作门禁输入。

## 结果

- `SemanticButtonContrast`：每个主题 33 个样本，覆盖渐变 11 个位置 × normal/hover/pressed 三态，0 violation。主按钮 CTA 渐变 stops 改为不受环境透明度影响的 opaque accent；按压父层透明度收敛到 0.96，保留 pressed overlay/scale 的状态差异。
- Hover/Pressed overlay 不再固定使用同一白/黑方向，而是依据实时 `OnAccent` 极性选择远离文字前景的 wash；危险按钮新增 `GscOnDangerTextBrush`，按错误色实际对比度选择白/黑前景。
- `SemanticLayerContrast`：选中前景、输入正文、placeholder、危险按钮各 1 个样本，双主题均 0 violation。输入资源仍明确提供 `Foreground`、`CaretBrush`、`SelectionBrush`，placeholder 单独使用 Muted 层级。
- `ComplexBackdropContrast`：对白、黑、高频红、饱和蓝四种合成背景逐项经过 `SurfaceTop + ControlFill` 壳层，双主题 4/4 通过；深色选中/输入等读面不靠黑字默认值。
- 主题资源切换测试确认 Light/Dark 各自重新写入 selection、button gradient 和 state brushes，未沿用上一主题画刷实例。

## 证据边界

- 报告中的交互行仍写明 normal 已捕获、Hover/Pressed/Disabled/Focus 由共享模板声明；真实鼠标、键盘焦点、Popup、Tooltip、Dialog 开闭中切换和 Playnite 宿主状态序列尚未宣称完成。
- 复杂背景是合成负例/边界夹具，不是用户真实游戏背景截图；真实窗口 DPI、多屏移位和高对比主题仍需外部验收。
