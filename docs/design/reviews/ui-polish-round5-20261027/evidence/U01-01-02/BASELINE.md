# U01-01 / U01-02 混排基线与标题层级

日期：2026-10-27。工作树基于 `0447237a582eb20c22320657c981245cdfe624a6`，本批源码提交以随后记录的候选提交为准。

## 本轮增量

- U01-01：在隔离的 UI Framework Probe 中，用生产主按钮样式展示 `校验 Verify (9)`，在生产 DataGrid 样式中展示 `存档 2077 / Save 9 (DLC)`。RenderHarness 读取实际控件的字号/字重，再用 WPF TextFormatter 捕获 GlyphRun 并测量混排基线。按钮为 12 DIP SemiBold，表格为 14 DIP Normal；两者 baseline spread 均为 0，样本稳定。未增加固定偏移或捆绑字体。
- U01-02：生产 shell 页标题改用 `GscPageTitleStyle`；媒体、通知、任务和工具页同级分组标题统一引用现有 `GscSectionTitleStyle` DynamicResource。保留各页面既有文字、命令、绑定和布局密度。存档页上一候选的垂直留白导致历史表高度回归，现恢复紧凑垂直参数；横向节奏保留。
- 测量到的字号来自按钮与 DataGrid 实际控件；字体覆盖结果是当前机器已安装字体的 WPF 布局结果，不代表可选字体缺失场景。

## 视觉与行为证据

- [截图索引及 SHA-256](screenshots.json)：按钮/表格混排探针浅深主题截图，以及存档、任务、媒体、工具、通知页和 shell 的双主题 1040×700 DIP 截图。
- [浅色混排探针报告](reports/finesse-light-report.txt) 与 [深色报告](reports/finesse-dark-report.txt)：实际控件字号/字重、GlyphRun、字形覆盖和基线测量。报告中截图路径已规范到本目录的截图副本；原报告 SHA-256 保存在 `reports.json` 的 `sourceSha256` 字段。
- [render-qa 报告](reports/render-qa-report.txt)：当前批全页双主题/多尺寸矩阵通过，末尾为 `render-qa OK`。快速侧栏切换记录的帧间隔是离屏代理指标，不是显示器帧率或 Playnite 性能保证。
- [侧栏中断接续报告](reports/motion-reentry-probe-report.txt)：受控窗口浅深主题均从当前渲染宽度接续；该报告属于补充探针，不代替真实宿主输入验收。
- [定向测试 TRX](tests/U01-01-02-final-targeted.trx)：TypographyDiagnostics、生产 shell 标题和 R07 状态横幅相关测试共 37 passed、0 failed、0 skipped。另行运行的 R07 类测试 4/4 passed、0 skipped，原始 TRX 位于本地 `tests/GameSaveCenter.Playnite.Tests/TestResults/`，该目录是共享测试输出，未复制或清理。

## 修复的回归与验证

提交 `0447237a` 的间距候选使 `R07StatusBannerBudgetBehaviorTests.SaveStaleBannerKeepsRetryAndHistoryTableFloor` 实测表格高度降到 260 DIP，低于 274 DIP 门槛 14 DIP。工作流 run `40490978543` 的 SDK 8 与 SDK 10 job 都在该断言失败；原因是摘要区纵向扩张，不是测试基线或业务状态变化。当前只恢复存档摘要卡片垂直 Padding、底部 Margin 和动作区顶部 Margin 为紧凑值，没有修改测试阈值或生产行为。

RenderHarness 快速双击探针原用 Background DispatcherTimer，在渲染负载下可能晚于原有 1.5 秒观察期限。计时器切到 Input 优先级，60 ms 间隔、1.5 秒期限和最终状态断言均不变；当前 render-qa 得到 `secondClick=True`、最终展开宽度 270 DIP、状态稳定。此项是探针调度修复，不是降低门槛。

本批门禁：source validation 通过；XAML 检查 25/25；WPF 静态审查 31 个 XAML、0 errors、26 warnings、179 info（与既有基线一致）；RenderHarness Release 编译 0 warnings/0 errors；全矩阵 render-qa 通过；定向测试 37/37 且无 skip。具体命令和 SDK 记录在 `validation.json`。

## 未验边界

没有验证可选 Inter/Noto 字体未安装的回退组合、真实 Playnite 宿主、物理 125/150/200% DPI、UIA/键盘/IME 状态，也没有执行存档恢复、媒体删除或云端写入。当前源码候选的新 GitHub 双 SDK 工作流仍待提交后查询。离屏逻辑 DIP 不代表物理屏幕 DPI；render-qa 的帧统计仅作探针代理。
