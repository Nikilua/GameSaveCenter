# GameSaveCenter Post-Typography Geometry Closure Plan

来源：`GameSaveCenter_PostTypography_Geometry_Audit_Fix_Prompt.zip`（2026-08-15）
基线：`978e437`（`main` 与 `origin/main` 已同步）
范围：Post-Typography Geometry & Text-Fit Closure。保留字体修复，不重构页面，不改业务逻辑。

## 实施项

1. P0：Maintenance 诊断“等级”列
   - 新增共享 `GscSeverityColumnWidth`（DataGridLength 92 DIP）。
   - 诊断 FindingsGrid 与异常审计 FindingsGrid 统一引用该 token，移除 `Width="72"`。
   - 保持 `GscRedesignTableStatusPill` 内容自适应，不全局增宽。

2. 全仓固定几何扫描
   - 扫描所有 `Width/MinWidth <= 120` 且承载文本的列、Pill、Button、Tab、Input。
   - 仅修复确认挤压的列；其余列按当前字体度量核对后保持不变。

3. UI Audit Text-Fit 检测
   - 在 `UiLayoutAnalyzer` 增加基于 `FormattedText` 的无约束文本宽度与 `ActualWidth` 对比。
   - `TextWrapping=Wrap` 与 `TextTrimming != None` 的文本视为 intentional trimming，不误报。
   - `TEXT_FIT` 为 MEDIUM，`UiAuditRunner` 对任何 `TEXT_FIT` 直接失败退出。

4. visual-tree exporter 修复
   - 根因：离屏 host 未连接 PresentationSource，`IsVisible` 恒为 false，导致 JSON 空数组。
   - 改为 `Visibility == Visible` + 非零尺寸判断，恢复真实视觉树导出。

5. 回归锁定
   - 新增 severity 列 token 静态测试。
   - 新增 Audit Text-Fit / visual-tree 基础设施静态测试。
   - 保留 Settings、Save Compare、Compact Inspector、Media Inbox 既有响应式成果。

## 验收

- Core/Worker/Playnite 测试全绿
- render-qa 全绿（11 档 + 56 主题 + 7 Resize）
- UI Audit：0 HIGH / 0 MEDIUM / 0 failed routes / 0 TEXT-FIT
- visual-tree 175 个 JSON 非空
- 更新 WORKLOG / PROJECT_MEMORY / DEVELOPMENT_HANDOFF
- commit + push `main`
