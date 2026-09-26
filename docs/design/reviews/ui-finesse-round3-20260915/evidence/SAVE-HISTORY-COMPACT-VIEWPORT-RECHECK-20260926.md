# Save 历史窄窗视口与当前 UI gate 复核

日期：2026-09-26
起始代码基线：`0e340c08`（本地 `main`，复核开始时与 `origin/main` 一致）
任务账本：R `192/192` 个唯一 ID，状态计数 `106/83/1/1/1`；本批不新增或改写 R 行

## 结论

复核当前 main 后，Save Center 历史版本页在窄窗中原有可读行数不足问题仍可复现。已修正页面的垂直预算与详情入口：`1040×700 DIP` 窗口（内容 `744×460 DIP`）下，历史表视口为 `260 DIP`，四行完整可读；动作按钮使用共享 `GscIconOnlyToolbarButton` 的 `36 DIP` 尺寸。compact 布局把恢复可用性说明从底部占行区域移至 Inspector，并为详情入口提供 tooltip/Automation HelpText；宽布局仍显示行内说明。命令、绑定与恢复安全语义未变。

RenderHarness 对当前工作树产生 168 个运行时快照：`HIGH=0`、`MEDIUM=0`、Fidelity warning `0`、失败路由 `0`，另有 87 条 INFO。审计器现在只有在内部列表实际纵向溢出且没有显式有限视口时才报告嵌套滚动 HIGH；本轮确认媒体子列表均有有限高度，不再作为误报。

完整 `render-qa.ps1` 项目级 gate 仍未通过：报告共有 33 条 PROBLEM，均位于本批目标之外的 Overview、Task、Settings：

- Overview 最近访问列表视口为 `2 DIP`（最小期望 `236 DIP`），跨多个布局/主题重复。
- Task 表在部分尺寸仅有 `3/4` 数据行完整可读；`1100×720` compact 步骤中 Inspector 超出 Task surface。
- Settings 在较矮尺寸有 `2–3 DIP` 类别滚动视口或 `128–144 DIP` 主体视口不足。
- 报告没有 Save 或 Media 的 PROBLEM。

因此本证据只关闭 Save 历史窄窗这一项，不代表整个 UI gate 通过，也不调整 R23-08 的 192 项计数。全局 Render QA 中其余三页问题现为明确的后续实现范围，应逐页修复并独立验收。

## 验证

- RenderHarness Release build：`0 warning / 0 error`。
- 定向测试 `SaveAndTrainerStackedInspectorsReserveAReadableListViewport`、`AuditHarnessContainsStaticRuntimeAndFullScrollCapabilities`、`SaveCenterCompactModeKeepsRestoreAvailabilityAccessibleWithoutTakingTableRows`：`3/3` 通过。
- `scripts/validate-source.py`：通过。
- `scripts/check-xaml.ps1 -ProjectRoot .`：`24/24` 通过。
- WPF 静态检查：`0 errors / 28 warnings / 177 info`（既有检查提示）。
- `git diff --check`：通过。
- 完整 Render QA 报告：`render-qa FAILED`，33 项上述非目标页面问题；Save history 在 `1040×700` 为 `readableRows=4/4`，候选表同尺寸 `4/4`，SaveCurrentRuleButtons 为 `36,36,36`。

RenderHarness 使用离屏 WPF 与逻辑 DIP；它不代表物理 DPI、Playnite 父容器或真实宿主最终呈现。本轮未安装或启动 Playnite。既有 WMI `Win32_Process.CommandLine` Access Denied 与 CEF `platform_channel 0x5` 阻塞没有重试或绕过。
