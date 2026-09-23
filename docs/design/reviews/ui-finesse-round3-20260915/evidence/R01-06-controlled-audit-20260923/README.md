# R01-06 受控 UI 审计归档（2026-09-23）

## 身份与结果

- 采样源码身份：f55dce61adba84fec96c3e5434e5a8c1e3fa7132（main 合并提交）；audit-metadata.json 保留采样时间、运行环境与相对输出路径。
- 运行：RenderHarness 的 WPF 离屏 audit，逻辑 DPI 1.0；七种受控窗口尺寸。
- 静态清点：10 Views、33 Tabs、297 Button/ToggleButton、16 DataGrid、38 ScrollViewer、292 conditional UI。
- 运行时：168 snapshots、110 warnings、0 Fidelity warnings、0 route failures。
- 本轮实际发现仍有 7 HIGH TRUE_PARENT_CHILD_SCROLL_CONFLICT 与 4 MEDIUM TOOLBAR_VERTICAL_EXPANSION。审计没有把它们归为清零。
- 证据索引抽样 20 项；validate-ui-evidence-index.ps1 校验 20/20 references、identities、samples、boundaries。

## 文件

保留聚合摘要、索引、布局报告、Fidelity 矩阵、静态 manifest 和 route map 的 Markdown 报告，以及 6 张可代表页面状态的受控离屏图。完整 JSON/raw tree/log 和临时 zip 可由 RenderHarness 重建；不归档机器绝对路径或本机临时文件夹。

- overview-standard.png、overview-narrow.png
- save-history-standard.png
- media-inbox-standard.png
- task-center-standard.png
- maintenance-diagnostics-standard.png

截图来自 synthetic DTO 和隔离 WPF audit，不是 Playnite 最终呈现、物理 DPI/跨屏、UIA/读屏、DWM presented frame、ETW 或宿主性能证据。它们只用于本报告标注的受控布局观察。

## 重建

在目标源码 checkout 执行 Release build，再用其 GameSaveCenter.RenderHarness.exe audit <output-directory> 生成完整输出。生成过程中设置 GSC_SOURCE_ROOT、GSC_BUILD_COMMIT 与 GSC_UI_AUDIT_COMMIT 为该 checkout 的绝对路径和完整 SHA，可避免短 SHA/机器路径误标。之后使用 scripts/validate-ui-evidence-index.ps1 -AuditRoot <output-directory> -MinimumRows 20 校验索引。