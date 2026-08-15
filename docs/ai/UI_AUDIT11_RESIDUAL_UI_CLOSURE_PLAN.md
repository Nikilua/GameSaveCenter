# GameSaveCenter Audit11 Residual UI Closure 计划

来源：`GameSaveCenter_Audit11_Residual_UI_Closure_Prompt.zip`（2026-08-15）
基线：`dc5aa7b`（`main` 与 `origin/main` 已同步）
范围：Residual UI Usability + Semantic Trimming Closure。不回退前几轮成果，不改业务逻辑。

## 实施项

1. SaveHistory 大小列
   - 新增 `SaveSizeValue` 样式：`TextTrimming=None` + `Tag=SaveHistorySize`。
   - 列宽 90 → 116 DIP，覆盖 `25.56 MiB / 999.99 GiB / 1.23 TiB` 一类短结构化值。
   - narrow 仍收起备注列，状态列保持可见（总宽 674 < 741）。
   - 新增 `SHORT_SEMANTIC_VALUE_TRIMMING` 门禁：对带 `SaveHistorySize` Tag 的单元格用无约束文本宽度对比 content box。

2. Maintenance Device Inspector
   - Compact/Narrow 改为详情切换模式：默认收起，独立“查看设备详情 ›”按钮；展开后 viewport >= 180 DIP，表格保留 header + 2 行。
   - 新增 `INTERACTIVE_INSPECTOR_USABILITY` 门禁：可见且含交互控件的 Device inspector 若 viewport < 150 或比例 < 0.3 即失败。

3. Settings P2
   - 滚动目标取整（integer snap），减少半像素文字碎片；仍只在 SelectionChanged / ApplyResponsiveLayout 时滚动，不抢用户手动滚动。

4. 验证与交付
   - build/tests/validator/render-qa/UI Audit 全绿；更新记忆文档；commit + push。
