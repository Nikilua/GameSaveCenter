# R00/R01 当前复核与证据校正（2026-09-23）

## 本批修正

在 `tests/GameSaveCenter.RenderHarness/Program.cs` 中，纯图标次要按钮没有 `TextBlock` 时，审计原本输出 `<composite>: missing`。查看 Light 探针截图并核对实际控件后，确认按钮已呈现，只是没有可测文字。现改为 `<icon-only>: no text label; text contrast not applicable`，避免把无文字样本记成控件缺失。提交 `5fbfc869` 不改生产 UI、命令或绑定。

核对 R00-01 原计算断言时，又发现 `opacity=0.5` 的数值测试仅验证 pressed 状态；hover+pressed+focus 虽出现在另一个 88 项矩阵，但没有与半透明 chrome 同时验证。已在 `UiDiagnosticsExporterTests.GradientContrastModelsWholeChromeOpacityAndNonUniformStops` 增加组合态行为断言：直接核验灰色合成背景、黑色有效文字与 `4.5` 对比度门槛。提交 `38d5b7b2`，不是字符串存在性检查。

同时补充 R01-07 的源路径映射：R00-01/02 记录此前只观察主题/控件源码，没有跟踪 `UiDiagnosticsExporterTests.cs` 和 `UiFinesseFoundationTests.cs`；R00-05 缺 `WpfUiResourceDictionaryTests.cs`；R00-06 缺 `MediaInboxGeometryTests.cs`。新规则可在这些行为夹具变化时使对应证据进入待复验状态。

## 当前身份构建与行为结果

- 最终代码/测试 checkout：`38d5b7b2`，`codex/ui-finesse-round2`。
- Release solution 构建及 XAML 校验：XAML `24/24`，`0 errors`，两条既有 `CS8602` warning 均位于 `MediaCenterView.xaml.cs:706`（WPF 临时项目与正式 Playnite 项目各一条）。Playnite 仍编译为 `net462`。
- 最终构建身份下，`RepositoryIdentityTests 2/2`；R00-01 灰底/半透明组合态 + Light/Dark 状态矩阵 `3/3`。
- `5fbfc869` clean 构建的 R00/R01 选定行为类 `36/36`：`UiFinesseFoundationTests 9`、R00-01 contrast `3`、R00-05 disabled chrome `2`、`GamePickerKeyboardBehaviorTests 6`、`LargeLibraryPerformanceTests 5`、`RepositoryIdentityTests 2`、`NumericCellReadabilityTests 2`、`UiAuditSourceTests 6`、`UiNegativeFixtureRegistryTests 1`。失败 `0`、跳过 `0`。
- 同一 clean 身份的 R18 专测 `1/1`，相关媒体/选择回归 `23/23`；精确 23 项方法与理论用例已列在 [R18-04 报告](R18-04-TABLE-CONTAINER-BUDGET-RECHECK-20260923.md)。其中 media page theory 按 200、2,000、10,000 后端规模展开成 3 个测试。
- 完整 Release 构建、TRX 文件和逐类 testhost 输出位于 `.tmp/r00-r01-audit-20260923`；均为可再生临时内容，不引用为长期 artifact。

## 视觉/行为探针与审计

- `5fbfc869` clean-tree Light/Dark `finesseprobe` 均 exit `0`：每主题 88 个渐变按钮状态/stop 样本，0 violations；数字可读 `4/4`；窄列、黑字深底和裁切负例 `must-fail=passed`；图标按钮明确报告“无文字样本”。
- R00-03/R01-04 的 `UiFinesseFoundationTests 9/9` 与 Light/Dark motion probe 结果按受控 STA/Dispatcher 记录；它们不等价真实 Playnite 输入或 presented frame。
- `5fbfc869` clean-tree 完整 WPF 离屏审计：10 Views、33 Tabs、297 Button/ToggleButton、16 DataGrid、38 ScrollViewer、292 conditional UI；168 runtime snapshots、110 runtime warnings、0 Fidelity、0 failed routes。当前仍有 7 HIGH `TRUE_PARENT_CHILD_SCROLL_CONFLICT` 与 4 MEDIUM `TOOLBAR_VERTICAL_EXPANSION`，明确保留为待处理发现。
- `validate-ui-evidence-index.ps1` 输出 `rows=20, references=20/20, identities=20/20, samples=20/20, boundaries=20/20`。六张代表图、报告与便携 metadata 已归档到 [R01-06 当前审计](R01-06-controlled-audit-20260923/README.md)。
- WPF testhost TRX 中，`R18TableContainerBudgetTests` 与 `MediaWindowAnchorContractTests` 的关闭阶段记录 `TextServicesHost.OnUnregisterTextStore InvalidComObjectException`；xUnit/VSTest 计数明确通过（分别 `1/1`、`10/10`），进程 exit `0`。根因未查明，未把它改写为失败或忽略。

## freshness 和验收边界

更新后的 `UI_EVIDENCE_BASELINE.json` 已在完整代码身份 `38d5b7b2d0488dc5e7234d77ff1435c9d4e521c0` 上运行 freshness：`14 fresh/0 stale`；docs-only、shared-control 和 package-identity 行为样例通过。报告分别列源身份和包身份；包身份仍为 `not-provided`。没有启动真实 Playnite/package-host，也没有验证真实宿主像素、UIA/读屏、物理 DPI/跨屏、IME 候选 UI、presented frame、ETW 或宿主性能。Demo 原目录不可用，使用已恢复的生产基线；仅使用 synthetic DTO、fake 状态和隔离目录。

没有访问真实存档或媒体，也没有云端写入/诊断外发。当前游戏选框、滚动条、命令绑定、取消/错误语义、恢复保护、有限列表和 Playnite `net462` 兼容保持不变。

下一可执行任务：R02-01 当前来源/行为小批。先按任务组条件检查已有动作优先级实现、真实命令和安全语义，再挑对应受控行为测试；若当前实现与证据已满足条件，只补当前身份证据并记“已满足”。真实 Playnite/package-host、物理呈现/输入、ETW/宿主性能仍是未验边界。
