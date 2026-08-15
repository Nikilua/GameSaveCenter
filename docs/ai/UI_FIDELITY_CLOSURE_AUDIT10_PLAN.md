# GameSaveCenter UI Fidelity Closure（Audit10）计划

来源：`GameSaveCenter_UI_Fidelity_Closure_Audit10_Prompt.zip`（2026-08-15）
基线：`4f8e37f`（`main` 与 `origin/main` 已同步）
范围：Rendered Content Fidelity Closure。不回退字体，不重做页面，不改业务逻辑。

## 实施项

1. P0：Maintenance 中间列表头未渲染
   - 根因候选：Maintenance 局部 implicit `DataGridColumnHeader` style 干扰共享 header template resolution，中间列依赖 `GscDataGridColumnHeaderStyle` 时丢失视觉模板。
   - 方案：移除 Maintenance 局部 implicit header style，真实列继续走共享 `GscDataGridColumnHeaderStyle`，filler header 由生产 `DataGridColumnHeadersPresenter` + `ColumnHeaderStyle` 主题化。
   - 新增 `HEADER_CONTENT_FIDELITY`：metadata 非空但视觉子树无 ContentPresenter/TextBlock 即失败，filler header 排除。

2. P1：MediaSearchTextBox 54.67 DIP
   - 根因：搜索标签与 TextBox 放进横向 StackPanel，TextBox 无伸展列。
   - 方案：改为 `Auto / * (MinWidth=160) / Auto / 150` 的 Grid，TextBox `Grid.Column=1` + `MinWidth=160` + Stretch。
   - 新增 `CONTROL_USABILITY_GEOMETRY`：媒体搜索框 < 160 DIP 即失败。

3. P1：Settings 选中分类未 scroll-into-view
   - 方案：`OnSettingsTabSelectionChanged` 与 `ApplyResponsiveLayout` 末尾同步调用自定义滚动；`BringIntoView()` 保留作为安全回退，再用增量 delta 循环收敛到 viewport 内，避免与滚动后坐标双重偏移。
   - 新增 `ACTIVE_TAB_VISIBILITY`：横向可滚分类条中选中 TabItem 必须完整位于 viewport。

4. P2：Save History narrow 状态列不可达
   - 方案：narrow（宽度 < 1100 DIP）将备注 summary 列收起为 0 宽，完整备注保留在版本详情 Inspector；状态列留在 viewport 内，不开启横向滚动。
   - 新增 `ESSENTIAL_COLUMN_VISIBILITY`：SaveHistory 状态列右缘不得超出视口。

5. 验证与交付
   - 全量 build/tests/validator/render-qa/UI Audit。
   - 更新 WORKLOG / PROJECT_MEMORY / DEVELOPMENT_HANDOFF。
   - commit + push `main`。
