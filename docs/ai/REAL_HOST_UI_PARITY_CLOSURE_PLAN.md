# GameSaveCenter Real Host UI Parity Closure 计划

来源：`GameSaveCenter_RealHost_UI_Parity_Audit_Prompt.zip`（2026-08-15）
基线：`85902df`（`main` 与 `origin/main` 已同步）
范围：建立 Tier A/Tier B 双审计链路；明确离屏截图不是真实宿主视觉事实来源；落实“完成后自己 commit + push”项目协定。

## 实施项

1. 保留 Tier A Offscreen Regression Audit，新增 Tier B Real Playnite Host Fidelity Audit。
2. 插件内实现真实宿主诊断服务：
   - `GSC_REAL_HOST_AUDIT` 环境变量或 sentinel 触发；
   - 从真实已加载 `DashboardView` 捕获截图、visual tree、resource snapshot、style fingerprint、DPI、实际 bounds；
   - 安全遍历 workspace/tab，不触发业务命令；
   - Dashboard 捕获后自动打开 Settings 并捕获设置页；
   - 输出 `artifacts/ui-host-audit/` 与 `GameSaveCenter-ui-host-audit.zip`。
3. 新增 `AdaptiveThemePaletteContrastGuard` 与导出器单测，防止 palette 层级被宿主压平。
4. 产出 `HOST_STYLE_DEPENDENCY_REPORT.md` 与 `HOST_VISUAL_PARITY_REPORT.md`，更正 README/审计文档定位。
5. 把“完成后自己 commit 并 push”写入 AGENTS.md 与 DEVELOPMENT_HANDOFF。
6. 跑完整验证并提交 push。
