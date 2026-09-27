# GSC-058 首页待处理与最近任务视觉回归

日期：2026-09-27；代码提交：`d9b08aa22f5c30354644f591d0d49f64dae36e1e`
RenderHarness 说明修正：`1860cdb7460e0588427162eb5426d33b6133e875`

## 范围与结果

- 首页保留刷新、全部备份、同步媒体真实命令；把单张“待处理”优先卡扩为全宽并前置，随后显示最近任务与风险/关注详情。当前游戏摘要、六项重复统计、最近访问和全局活动卡在首页折叠；没有删除 DTO、命令、绑定或独立工作区入口。
- 媒体生产 XAML 未在本阶段改动。复核确认 Inbox 与当前游戏媒体、重复识别、来源规则各在独立 TabItem；三个局部页签继续使用局部导航。收件箱类型/来源绑定 `KindDisplay` / `SourceDisplay`，来源规则显示 `SourceKindDisplay`，未展示 DTO 的原始枚举名。
- Demo Design 原目录在本机 checkout 不存在，因此不声称按 Demo 原文件完成像素级比对。本次依据 GSC-058 明确验收、当前生产共享样式与 RenderHarness 证据核对。

## 验证

- 当前提交身份 Release Playnite.Tests 与 RenderHarness 隔离构建均 `0 warnings / 0 errors`；XAML 结构检查 `24/24`，`git diff --check` 通过。普通 NuGet restore 受 `%AppData%\NuGet\NuGet.Config` ACL 拒绝；复用仓库现有 restore assets，在 `.tmp` 隔离输出中以 `--no-restore` 构建，未改配置/权限。Python/`py` 不在 PATH，`validate-source.py` 未运行。
- 当前提交身份定向回归 `6/6`：首页源码/布局/动作优先级 `3/3`；媒体工作区、收件箱表格与来源规则 `3/3`。Overview 边界夹具双主题、1040×700 与 1600×900、空任务/多风险/离线共 `12/12`，四个主要表面均可测量，水平溢出 `0`，真实优先动作可达。
- 完整 UI Audit：`168` 个运行时快照、`88` 条扫描警告、Fidelity `0`、失败路由 `0`；剩余 `3` 条 HIGH 全为 Task 窄视口 200 DIP 估算 4.4 行，不属于本次首页改动。Overview 路由未报 HIGH/MEDIUM/Fidelity 失败。
- 完整 RenderHarness `render-qa` 未通过：报告仅有 SettingsState 的 4 条夹具断言（normal/dirty 状态的保存提示与 summary visibility）；本阶段未改 Settings 源码或该夹具，故未扩范围修理。相关过滤测试另发现两条既有静态断言过期：`SaveCurrentRuleStatusIsOneLineBadgeWithAlignedActions` 仍要求按钮直接使用 `GscIconOnlyButtonBase`，而生产控件使用继承该基样式的 `GscIconOnlyToolbarButton`；`ViewsUseUnifiedDisclosureCardChrome` 禁止 `GscExpander`，但当前 Settings 已按共享样式使用它。该次范围测试结果 `34 passed / 11 skipped / 2 failed`，不能记作全套测试通过。
- `OverviewPrioritySummaryHidesDuplicateStatusPills`、全局活动和统计条 legacy 测试按项目标记跳过；新首页信息层级测试与新 WPF 布局契约测试均通过。没有启动 Playnite；离屏 WPF 的逻辑 DIP 不是宿主、UIA、物理 DPI 或最终呈现证明。
- UI Audit/RenderHarness 截图和 ZIP 位于本机 `.tmp/gsc058-release-20260927/`，仅供本次核验，记录完成后按仓库规则清理，不进入 Git。

## 后续复核

2026-09-27 在当前产品提交 `d5f0e8c2aa134d76255408c822f63348439e56d9` 使用现有 restore assets 重新构建并完整运行 RenderHarness，结果为 `render-qa OK`、无 PROBLEM；Settings normal/dirty/invalid 三态全部通过。此前提及的四条 SettingsState PROBLEM 未复现，原因未知，因此没有据此改动 Settings 产品代码或测试夹具。对应结果和两条共享样式测试断言校准详见 [UI-REGRESSION-CONTRACTS-20260927.md](UI-REGRESSION-CONTRACTS-20260927.md)。

## 结论

GSC-058 首页重构已完成当前机器的源代码、绑定契约、边界状态与离屏视觉回归；不宣称真实 Playnite 宿主验收完成。R 总基线保持 `192` 项，状态 `106/83/1/1/1`，本批没有增删或重计账本项。
