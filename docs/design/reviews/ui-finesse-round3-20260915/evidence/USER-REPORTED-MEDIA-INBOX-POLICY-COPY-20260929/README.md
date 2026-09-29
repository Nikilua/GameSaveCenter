# Media Inbox 数据态说明精简与 R00-06 freshness 复核

日期：2026-09-29  
源码提交：`1e64347d7d36e7a83b898f8b0c0c4c613e174d1f`。此项只改“待归类”策略说明的可见状态和文案，不改归类服务、命令、虚拟化、滚动锚点、页级滚动或主题色板。

## 行为与布局

- `MediaInboxInfoDescription` 只在 `MediaInboxMode == 待归类` 且列表 `Count == 0` 时显示；列表有数据即折叠；进入“已忽略”视图也折叠。短显示文案为“归属不明的媒体会留在待归类中，不会自动猜测。”
- 完整策略仍通过持续可见的 `MediaInboxTitleText` Tooltip 和 UI Automation HelpText 提供：“只保留无法唯一判断所属游戏的公共截图和录像，不会静默猜测。”因此折叠说明行时，辅助技术仍可读取该安全语义。
- 浅/深主题生产 WPF 行为测试实测说明行 `Collapsed/0 DIP → Visible/18 DIP → Collapsed/0 DIP`，列表重新有数据后再次折叠；无数据的“已忽略”页不显示待归类策略。说明行折叠时，右侧数量胶囊仍决定信息带最小高度，信息带整体仅 `67.33 → 68 DIP`（空列表更高）。证据不将其表述成整张色带大幅收缩。
- 代表截图为 RenderHarness `1040×700 DIP`，数据态有收件行时不出现这段说明；待归类批量目标仍只是全局选框对应的只读摘要，不是第二个选择器。全局目标的独立证据见 [Media Inbox 全局目标核对](../USER-REPORTED-MEDIA-INBOX-GLOBAL-TARGET-20260929/README.md)。

## 构建、测试与来源身份

- 提交后 Release 隔离构建：解决方案 `0 warnings / 0 errors`，XAML `24/24`。
- 媒体相关 WPF 测试 `15/15`：新策略状态行为 Light/Dark `2/2`，`MediaInboxGeometryTests 3/3`，`MediaWindowAnchorContractTests 10/10`。因此当前 R00-06 四行下限、短窗页级 fallback 和锚点恢复仍由关联回归守护。
- RenderHarness 全量双主题/多尺寸报告身份为源码提交 `1e64347d`，`WorkingTreeClean=True`、逻辑离屏 DPI `1.00`、`PROBLEM_COUNT=0`、`render-qa OK`；卷积范围的媒体网格 fixture 同时覆盖 `50/400/2000/4468` 行样本。
- 测试 DLL ProductVersion `0.6.73+1e64347d7d36e7a83b898f8b0c0c4c613e174d1f`；SHA-256 `5A16A88B3F9702CDD5A3DD06848DBD7C124ABD0E5093B6EBE4F74DE7F7FFFC17`；MVID `6e49705e-4279-4fba-aede-07ecf54895f3`。
- Playnite 插件 DLL ProductVersion 同上；SHA-256 `24C74068FE95DB9A16D7207DEAD11EABA9B0ACC81FFFFA6DA7528200DC25C96A`；MVID `f06300a5-edb1-4b91-911c-aa157f654497`。
- `scripts/validate-source.py` 和 `git diff --check` 通过。freshness 基线把 R00-06 `sourceCommit` 更新到本提交后为 `14/14 fresh`；独立 package identity 未提供。

## 清理噪声与未验证边界

VSTest 明确为 `15/15`、失败 `0`、跳过 `0`、退出码 `0`。TRX 仍记录两条 WPF 文本服务关闭噪声：`TextServicesHost.OnUnregisterTextStore` 抛 `InvalidComObjectException`；它出现在 xUnit 全部完成之后，根因未知，本证据不将其说成测试失败或已修复。

RenderHarness 和受控 STA WPF 是合成/离屏逻辑 DIP 验证，没有启动真实 Playnite，没有验证物理 DPI、OS 输入或宿主最终呈现。用户截图运行实例的 DLL/MVID 未提供。Media Inbox 在真实 Playnite 中滚动后行偏移问题仍未解决，仍须同一安全宿主进程的 DLL 身份、窗口 DIP/DPI/主题及滚动前后 `[GSC-GRID-DIAGNOSTIC]`；本项不触碰它。原 Demo 目录在当前 checkout 不可用，按恢复的生产基线做局部文案整理，没有另换设计体系。

## 记录

- [`media-inbox-copy-behavior.trx`](media-inbox-copy-behavior.trx)
- [`media-inbox-1040x700.png`](media-inbox-1040x700.png)
- [`render-qa-media-inbox-1040.txt`](render-qa-media-inbox-1040.txt)
- 既有行距证据：[Media Inbox 次级动作行距](../USER-REPORTED-MEDIA-SECONDARY-ROW-GAP-20260929/README.md)
- 既有四行/锚点证据：[R00-06 Media Inbox 四行门禁](../R00-06-MEDIA-FOUR-ROWS-20260916.md)