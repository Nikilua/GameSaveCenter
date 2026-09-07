# UI3 完成质量复核证据

基线 `b0aa85a`，2026-09-07 从 Git 归档建立隔离源码，运行 `scripts/render-qa.ps1 -Configuration Release`。图片为未编辑的原始 RenderHarness 输出，不是真实 Playnite 截图。

- `Overview-1040x700.png`：首页单一优先行动。
- `Task-1040x700.png`：紧凑任务页三个完整数据行。
- `Maintenance-1040x700-tab0.png`：诊断详情挤占列表。
- `Maintenance-1040x700-tab5.png`：紧凑进程映射列表只剩表头。
- `Maintenance-1366x768-tab5.png`：较宽画布进程映射双栏。
- `Shell-Media-1040x700.png`：生产 Shell 构造的媒体页面几何；Fake 数据，页头/导航状态不用于生产路由验收。
- `render-qa-report.txt`：完整本轮报告，含主题、resize、生产 Shell 媒体几何和离屏性能采样。

独立 1040×700 场景的页面画布实际为 744×460；Shell 媒体场景 PageHost 为 715×577。场景名不可直接当成画布尺寸。[评估与实施计划](../../../ai/QUALITY_REVIEW_2026-09-07.md) 区分了静态确认、离屏复现和未验证范围。
