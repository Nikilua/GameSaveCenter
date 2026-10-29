# U09-01：U08-05/06 双主题配对证据

## 配对范围

U08-05 对比 `d6d940e70c24d87a742882cbd052c45cd9926c8f` → `d19570fad7f056e44b2f0d5aee2eca3260d04ae7`；U08-06 对比 `b9bdd2d3a09c9b0634729b72ebb81fcc3042c10c` → `e81c17ce7df7dc46f84bdc9a163aac360b89f842`。每组固定样本时间为 `2026-10-28T09:00:00Z`，覆盖浅/深主题与 1040×700、1366×768 DIP。RenderHarness 仅在隔离的 `git archive` 临时副本中改动：固定样本时钟，保持相同维护动作种子，并给 FakeDashboardData 增加 `GSC_RENDER_TOOL_BUSY=1` 测试状态；生产源码没有为取图改写。

存档快照没有 `.git`，因此 Render QA 报告里的 Commit 字段来自承载本轮的 `5040ac555c3d202b144c4ca9730a87978d139252`，不代表历史截图源码。准确来源以本页列出的 Git commit 与 archive SHA-256 为准，完整快照元数据保存在 `metadata/`。`origin-preflight.json`记载了真实入口`GameSaveCenter-Run.cmd`、`GameSaveCenter-一键构建安装运行.cmd`、`scripts/build.cmd`均存在，以及本地SDK 8.0.423；未新建或虚构入口。

## U08-05：启动进行中状态

改前与改后快照均使用 SDK 8.0.423 / Release：Worker 与 RenderHarness 构建退出码均为 0，完整 Render QA 退出码 0，问题数为 0，主报告各有 334 张 PNG。改后在 1366×768 的浅色和深色页面中出现“正在启动所选工具…”文字，原启动按钮仍在原位；1040×700 响应布局隐藏右侧工具检查器，仍保留列表与导入入口。截图在 `screenshots/`，完整报告、构建日志与快照信息在 `reports/`、`logs/`、`metadata/`。

## U08-06：维护动作分组与诊断摘要

改前与改后使用相同的三个 fixture 项：`inspection-1`、`transfer-1`、`quarantine-1`。改后按“需要人工处理”“等待自动重试”“例行巡检”分组，列表预览最多三项并保留“更多维护操作”展开入口；完整诊断摘要仍可向下滚动读取。历史完整 Render QA 改前为 26 个问题、改后为 25 个问题，均来自既有 Media 视口/侧栏快速切换检查；更改目标页的浅/深主题 × 1040×700、1366×768 共 8 行都为 `OK`。改前人工整理的 `snapshot.json` 中 `targetThemeRowsOk` 字段为空（解析缺陷），因此8/8结论直接从保留的 focused-theme 与完整报告行复核，见 `reports/target-theme-status.txt`。完整报告和所有 `PROBLEM` 行均保留，未把全局失败算成通过。

## 当前主线行为与静态检查

当前冻结源码 `5040ac555c3d202b144c4ca9730a87978d139252` 使用 SDK 8.0.423 / Release 定向执行 5 项相关行为测试，5 通过、0 失败、0 skip。覆盖忙碌状态/UIA 帮助文字、静默态恢复、维护短窗可达性、摘要复制绑定和相关共享样式。源码结构校验退出码 0；WPF 静态审查扫描 26 个 XAML，0 error、26 warning、164 info。

第一次直接测试压缩快照时遗漏 `GSC_BUILD_COMMIT`，1 项身份守卫测试失败（另 2 项通过，0 skip）；这次失败日志和 TRX 已保留且没有计入通过数。随后在真实 Git checkout 中设置准确 `GSC_BUILD_COMMIT` 和 `GSC_SOURCE_ROOT`，对冻结提交 5040ac5 重跑相关 5 项，全部通过。

## 文档校验

更新后的源码结构校验退出码 0；本轮六份 Markdown 文档共检查 809 个本地链接、33 个外链、0 个缺失链接；git diff --check退出码 0。原始链接扫描明细见 `logs/relative-link-check.json`，源码校验日志在 `logs/final-source-validation.log`。阶段临时目录精确清理18,652个文件/1,412,199,397字节，C盘空闲增加1,464,045,568字节、D盘不变；清单和前后空间见 `cleanup.json`。

## 证据边界

本批只验证离屏 WPF 和假数据状态，不启动真实游戏工具；不执行真实存档恢复、媒体删除或云端写入。未验证 Playnite 实际宿主渲染、外部 UIA/Narrator、实体键盘/IME、物理 DPI 或多屏。U08-05 与 U08-06 状态仍为“代码完成待验收”；U09-01 继续盘点其余已列配对批次。

文件身份与 SHA-256 清单见 `pair.json` 与 `SHA256SUMS.txt`；目标主题行见 `reports/target-theme-status.txt`。24张截图及32个复制文件均逐项SHA-256核对一致，记录见 `logs/file-copy-verification.json`。
