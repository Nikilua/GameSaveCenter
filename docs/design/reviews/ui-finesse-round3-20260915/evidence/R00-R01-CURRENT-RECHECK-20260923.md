# R00/R01 当前复核与证据校正（2026-09-23）

## main 身份与当前结论

本次受控审计采样的源码身份为 main 提交 f55dce61adba84fec96c3e5434e5a8c1e3fa7132。其后的 c46c5b99 只更新复核文档，没有改审计源码；分支仍为 main。已将 R01-03 与 R01-06 的 evidence baseline sourceCommit 绑定到该审计身份。

R00/R01 已有代码修正随合并进入 main：RenderHarness 将图标按钮标记为“无文字样本”，不再误报控件缺失；半透明 hover+pressed+focus 组合态增加了有效文字颜色与对比度的行为断言。没有覆盖 main 生产实现或改写命令/业务语义。

## main 构建与行为回归

main 身份的隔离 Release solution build 已通过：XAML 24/24，0 errors，Playnite target 为 net462；保留 2 条既有 MediaCenterView.xaml.cs:706 CS8602 warning。

R00/R01 相关类在该 Release 测试程序集中的联合结果为 179 passed、39 skipped、0 failed，共 218 项。计数按类为：

| 测试类 | 通过 | 跳过 |
| --- | ---: | ---: |
| GamePickerKeyboardBehaviorTests | 6 | 0 |
| LargeLibraryPerformanceTests | 5 | 0 |
| NumericCellReadabilityTests | 2 | 0 |
| RepositoryIdentityTests | 2 | 0 |
| UiAuditSourceTests | 6 | 0 |
| UiDiagnosticsExporterTests | 11 | 0 |
| UiFinesseFoundationTests | 9 | 0 |
| UiNegativeFixtureRegistryTests | 1 | 0 |
| WpfUiResourceDictionaryTests | 137 | 39 |
| **合计** | **179** | **39** |

39 项均有明确的 xUnit skip 记录，原因是这些断言针对已撤销的“今日工作台”UI 架构，与恢复后的 AcrylicFork 生产页面不再适用；不把它们计为通过。本次没有失败项。历史 5fbfc869 的 36/36 和 38d5b7b2 的身份/对比度 5/5 仍只对应各自测试身份，不冒充为 main 上的同一计数。

## 当前 RenderHarness 审计

main 源码身份 f55dce61 的完整受控 WPF audit 报告现已归档至 [R01-06 main 审计](R01-06-controlled-audit-20260923/README.md)。RenderHarness 使用 synthetic DTO、隔离 WPF 窗口和逻辑 DPI 1.0；窗口尺寸覆盖 maximized、2k、wide、standard、compact、narrow-1100、narrow。

- 静态清点：10 Views、33 Tabs、297 Button/ToggleButton、16 DataGrid、38 ScrollViewer、292 conditional UI。
- 运行时输出：168 snapshots、110 warnings、0 Fidelity warnings、0 failed routes。
- 当前真实审计发现：7 HIGH TRUE_PARENT_CHILD_SCROLL_CONFLICT、4 MEDIUM TOOLBAR_VERTICAL_EXPANSION。索引通过不表示这些发现已经修复或清零。
- evidence index 抽样 E01–E20；对持久归档运行 validate-ui-evidence-index.ps1 的结果为 rows=20、references=20/20、identities=20/20、samples=20/20、boundaries=20/20。
- 归档保留 6 份报告 Markdown、便携 metadata 与 6 张代表图；完整 JSON、raw tree、全量截图、日志和临时 zip 未入库。

## freshness

R01-03、R01-06 的 sourceCommit 均绑定到 f55dce61 完整 SHA。以当前文档提交之前的 main HEAD c46c5b99894cfeaaf502a43fba30bb1bb1ed345a 运行 freshness，14 records 为 fresh、0 stale；scripts/test-ui-evidence-freshness.ps1 的 docs-only、shared-control、package-identity 用例通过。package identity 为 not-provided，不能据此宣称已重装或运行真实 package-host。

## 边界与下一项

审计截图是受控 WPF 离屏 logical DIP 样本，不证明真实 Playnite Dashboard、物理 DPI/跨屏、UIA/读屏、Windows IME、DWM presented frame、ETW 或宿主性能。保留的 7 HIGH / 4 MEDIUM 是本轮真实发现，后续按具体任务处理。没有访问真实存档或媒体、写用户云端或外发诊断；Demo 原目录不可用，沿用已恢复的生产基线。

下一可执行小批量：R23-02 生产资源状态矩阵，优先补齐 AcrylicNavItem、设置派生 Tab 和各页 DataGrid 的实际 Light/Dark WPF 状态观察；无法在受控窗口证明的宿主边界继续明示。