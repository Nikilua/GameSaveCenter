# Q01–Q02 字体、数字与路径受控证据

采集日期：2026-09-14（Asia/Shanghai）。证据来自 `tests/GameSaveCenter.RenderHarness` 的 STA 离屏 ProductionResourceDictionary 夹具；它证明共享资源与 DIP 布局，不替代真实 Playnite 宿主、物理 DPI 或 IME 证据。

## 运行身份

- 夹具窗口：1120×980 DIP，RenderTargetBitmap 96 DPI，DpiScale=1.00；数据为合成中英文、扩展 CJK、组合字符、代理对、路径、数值、状态与四行表格。
- 代码基线：`db43230`（Q01/Q02 字体与数字排版审计）。
- 双主题截图：[dark/ui-finesse-fixture.png](q01/dark/ui-finesse-fixture.png)、[light/ui-finesse-fixture.png](q01/light/ui-finesse-fixture.png)。原始报告：[dark/ui-finesse-fixture-report.txt](q01/dark/ui-finesse-fixture-report.txt)、[light/ui-finesse-fixture-report.txt](q01/light/ui-finesse-fixture-report.txt)。

## 结果

- 字体入口现在显式包含 `Inter`、`Segoe UI Variable`、`Segoe UI`、`Noto Sans SC`、`Noto Sans CJK SC`、`Microsoft YaHei UI` 与 `Microsoft YaHei`；报告区分 `FontCandidate` 覆盖证据与实际 WPF `GlyphRun`。本机候选结果为：中文 `Noto Sans SC`，Latin/数字/箭头 `Segoe UI Variable Text`，扩展 CJK 与指南针 emoji 为 `unresolved`，没有把缺字误报为已显示。
- 中文 Normal/Medium/SemiBold 均有候选权重记录；当前 Noto Sans SC 的 SemiBold 请求映射到其 Bold 字形，已记录为实际结果，不假称存在独立 SemiBold 文件。
- 正文共享行高为 20 DIP，说明文字为 18 DIP；报告包含中英混排、下伸部 `g/j/y`、全角标点、重音、组合字符、代理对、扩展 CJK 和 emoji 的实测宽高/基线。
- 数字样式启用 `Typography.NumeralAlignment=Tabular`；报告中 `1` 与 `8`、`00:09` 与 `12:59` 宽度一致，并覆盖 `0`、破折号、秒/分钟单位样本。
- 生产存档候选表仍保留原始路径字符串、CharacterEllipsis 与 Tooltip 取值；候选路径列已挂接 `SavePathText` 代码字体样式，其他路径入口的全面迁移和真实复制/Tooltip 宿主验收仍单独保留为未完成项。

## 当前源码门禁

- `TypographyDiagnosticsTests.ControlledReportsKeepPunctuationAndWeightEvidenceTruthful` 对深色/浅色报告同时锁定全角引号、书名号、破折号、省略号原文保留、无未配对代理项，以及 `SemiBold → Bold` 的真实候选字重映射；`FontActualGlyphRun` 继续明确为 `unknown`。
- `TypographyDiagnosticsTests.ProductionColumnsKeepNumericAndPathSemantics` 锁定存档大小列的固定宽度与 `SaveSizeValue`、媒体拍摄时间列、未知值破折号、维护数量单位和媒体原始路径 Tooltip 契约。它不替代完整八入口盘点，也不把离屏报告升级成宿主列布局通过。
- `TypographyDiagnosticsTests.ImportantTrainerDiagnosticsUseReadableSharedStyles` 锁定 Trainer 设置 Inspector 的重要标签、工具路径和风险提示使用共享可读样式，不再由显式 10pt 覆盖；普通状态密度文本与真实宿主小窗口观感仍需单独复核。
- 存档历史的文件数/大小列现在分别使用 `SaveCountValue`/`SaveSizeValue`（共享 Tabular 数字、右锚点、禁止数值截断）；时间、状态、备注列保持各自文本语义。该列级事实由 `ProductionColumnsKeepNumericAndPathSemantics` 锁定。
- `UiDisplayMappingTests` 现在锁定真实零值（`0 B`、`文件 0/0`）、未知大小（`未知大小`）、未检查时间和路径/产品名混排分隔符；本次 Core 定向测试 16/16 通过，不改变排序键，也不改写用户路径。

## 证据边界

- 截图和报告是受控 DIP/离屏证据，不能证明 125/150/175/200% 物理 DPI、真实宿主字体安装差异、IME 组合过程或最终 GlyphRun。
- 缺失字记录是合格的负向证据：扩展 CJK/emoji 在当前候选链没有候选覆盖；下一阶段应在真实宿主确认系统回退或明确产品缺字策略，不应捆绑未经许可的字体资产。
