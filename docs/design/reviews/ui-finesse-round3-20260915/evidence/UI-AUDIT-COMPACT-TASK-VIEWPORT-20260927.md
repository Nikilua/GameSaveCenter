# UI Audit 紧凑 Task 视口误报修复

日期：2026-09-27；代码身份：`a829521c752adcecd62e7a8c9bfc76149a40c49d`

## 结果

- 复核 GSC-058 后续 UI Audit 中 3 条 `TABLE_VIEWPORT_TOO_SHORT` HIGH。TaskGrid 实际高度为 200 DIP，表头 41.6 DIP，实测行高 36 DIP；门禁要求四行时所需高度为 `41.6 + 4 × 36 = 185.6 DIP`，实测估算可见 4.4 行。
- 根因是 RenderHarness analyzer 同时套用 `visibleRows < 4` 与固定 `grid.ActualHeight < 236`。236 DIP 是历史桌面密度下的常量，与当前紧凑行高冲突，造成 200 DIP 紧凑视口误报。
- `UiLayoutAnalyzer` 现在以实测表头和行高推导四行最小视口，并留 0.5 DIP 取整容差；`UiAuditSourceTests` 固定这一契约并拒绝旧的 236-DIP 常量。没有改 Task 页面、生产 XAML 或真实业务行为。
- 完整 UI Audit 绑定代码提交 `a829521c`：168 个运行时快照、85 条 INFO 提示、HIGH 0、MEDIUM 0、Fidelity 0、失败路由 0；compact TaskGrid 仍为 200 DIP / 4.4 行。审计报告的运行身份与代码提交一致。
- Release RenderHarness build `0 warnings / 0 errors`；Playnite.Tests 的 `UiAuditSourceTests` 定向测试 `6/6`。不是完整 Playnite.Tests 或项目级 `render-qa` 结果。

## 范围与边界

本阶段是审计器规则修正，不是 Task UI 修复，也不把离屏 WPF logical DIP 扩大成 Playnite 宿主或物理 DPI 结论。没有启动 Playnite，没有更改 192 项 R 台账（仍为 `106/83/1/1/1`）。原始完整报告和 ZIP 写入 `.tmp/ui-audit-a829521c/` 与 `.tmp/GameSaveCenter-ui-audit.zip`，仅用于本次核验，记录后清理、不入 Git。
