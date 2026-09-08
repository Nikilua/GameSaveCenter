# 2026-09-08 独立质量复核证据

基线 `97131f0`，运行 `scripts/render-qa.ps1 -Configuration Release`。图片为原始未编辑 WPF 离屏输出，不是真实 Playnite 截图。

- `Shell-Maintenance-Process-1040x700-closed.png`：紧凑进程映射，详情默认收起。
- `Shell-Tasks-1040x700-closed.png`：紧凑任务主表，详情默认收起。
- `Shell-Media-1040x700.png`：媒体收件箱间距与按钮布局。
- `Settings-1040x700-tab0.png`：审计环境缺 Worker 的设置校验失败态，不代表用户实际安装缺失。
- `render-qa-report.txt`：完整主题、尺寸、resize、Shell 几何与离屏性能报告。

Shell 图片使用实际生产 Shell 类和 Fake 数据，PageHost 为 715×577；Fake 页头仍显示首页，不作为生产导航证据。独立 Settings 图片画布为 744×460。新增运维记录与三组状态夹具仍待补齐，不把 render-qa OK 等同于这些分支已完成视觉/交互验收。

参见 [评估与任务包](../../../ai/QUALITY_REVIEW_2026-09-08.md)。
