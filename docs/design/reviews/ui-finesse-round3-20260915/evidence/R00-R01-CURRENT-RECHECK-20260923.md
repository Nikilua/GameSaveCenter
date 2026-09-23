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

## 当前 main `3a1dadd8` 与用户截图问题复核

源码身份为 `3a1dadd80bae6152dce9c3f684d5d2c745f02bc8`。当前 Release 构建 Playnite `net462`、XAML `24/24`、`0 errors`，保留两条已有 `MediaCenterView.xaml.cs:703 CS8602` warning。`ReportedWorkspaceLayoutBehaviorTests` 两主题 `8/8`；R18 专测 `1/1`，分页/锚点/几何/选择关联行为 `23/23`，各类按独立 testhost 运行。

- Light/Dark `finesseprobe` 当前 SHA 均为 palette `11` checks、`0` violations、numeric readability `4/4`、窄列负例通过；`WorkingTreeClean=False` 是当时文档改动导致，不影响完整源码/build SHA 一致。
- RenderHarness 完整审计：`168` snapshots、`118` warnings、`0` Fidelity、`0` 失败路由、`7 HIGH / 4 MEDIUM`；索引 `20/20`。用户四张布局截图、精确 DIP 和 R18 原始样本见 [用户截图布局复核](USER-REPORTED-LAYOUT-20260923.md)。
- 全局 `render-qa` 为 `40 PROBLEM`：OverviewRecentAccessList `20`、SaveHistoryGrid `8`、TaskGrid `4`、其他 SettingsLayout 断点 `8`。定向四页行为回归通过不等于 render-qa 门禁全绿。
- WPF 技能静态审查 `0 errors / 30 warnings / 177 info`。清单包含既有 StackPanel/滚动控件和模板建议项；本轮未以静态 warning 代替 WPF 布局行为结果。
- 不把离屏逻辑 DIP/合成数据描述为真实 Playnite package-host、物理 DPI/UIA/IME、DWM presented frame、ETW 或宿主性能证据；没有碰真实存档、媒体、云端或诊断数据。Demo 原目录不可用，继续使用恢复的生产基线。本节记录的下一项 freshness 校正已在后续 `8846d712` 复核中完成，见文末。

## 当前 main `8846d712` 范围 freshness 复核（2026-09-23）

- `check-ui-evidence-freshness.ps1` 在当前 main `8846d712af8fc388cb18cb732d94321d95034f11` 输出 14 条 R00/R01 记录；逐条 `needsRerun=false`、`matchedSourcePaths=0`。R00-01/02 与 R00-05 仍以 `3a1dadd80bae6152dce9c3f684d5d2c745f02bc8` 为证据源码身份；本次没有重写历史 baseline，也没有把旧测试冒称为在 8846 上重跑。
- `scripts/test-ui-evidence-freshness.ps1` exit `0`，覆盖 docs-only、shared-control 变更失效、package identity mismatch 三个门禁负例。完整 freshness JSON 已更新到 [当前 freshness 报告](R01-07-freshness-report-20260923-current.json)。报告全局 `documentationOnlyChange=false`，因为从旧证据提交到当前 HEAD 的变更还包括源码；结论仅是每条记录声明的 sourcePaths 均未匹配到变化。package identity 为 `not-provided`，不表示已安装或运行当前包。
- R01-03 当前链接改为本复核页，不再错误指向用户截图布局报告；R01-07 账本记录本次 `8846d712` fresh-scope 结果。RenderHarness 的 `168 / 118 / 0 Fidelity / 0 failed routes`、`7 HIGH / 4 MEDIUM` 仍是既有受控审计数据，没有在本次重新运行。

## 新增设置宿主截图差异与下一项

用户新附的设置窗口图仍显示搜索输入框远离标题左边界，顶部图标明显下移。当前 `main` 的 `GameSaveCenterSettingsView.xaml` 已显式左对齐搜索框，并在 `3a1dadd8` 修正布局；`ReportedWorkspaceLayoutBehaviorTests` 的 Light/Dark `8/8` 覆盖生产设置视图几何和路径编辑控件，但测试窗口为隔离 STA、DPI 1.0。用户图未包含可核对的 package/build identity，之前正常 Playnite 宿主复核也受 CEF `platform_channel` `0x5` 阻挡，因此不能把源代码和离屏行为证据写成用户当前窗口已修复。

下一项先以实际用户窗口边界复核设置视图：沿用合成设置对象与隔离宿主，覆盖截图尺寸换算出的逻辑尺寸、当前 `SettingsShell` 实际宽度、搜索框到标题左边界、图标顶部、保存状态及路径控件对齐；同时确认 `ApplyResponsiveLayout` 在宿主 `SizeChanged` 时执行。若 current checkout 可复现错位，再改共享布局并验证正/负场景；若不能复现，保留 package/host identity 与系统 DPI 为待核事实，不把问题关闭。随后继续依赖已满足的 Q/R 小批量。
