# Media 视频预览 fallback 空引用保护

日期：2026-09-26

实现提交：`f1b746d51b3621e1e4f24ee7ca90809d2116aea4`

## 发现与修复

`MediaCenterView.ResetSelectedVideoPreview` 在路径缺失或格式不支持时先显示 fallback，再无条件访问 XAML 命名的 `MediaSelectedVideo`。该字段可能尚未生成/可用，Release 因此在此处分出 `CS8602`。现仅在 `MediaSelectedVideo != null` 时折叠视频元素；fallback 仍按原逻辑显示，正常已加载视图行为不变。`R14ClassificationSelectionTests` 增加了该保护的回归契约。

## 验证

- 当前提交 `f1b746d5`，Release Playnite 测试项目 `--no-restore` build：`0 warnings / 0 errors`。
- `R14ClassificationSelectionTests`：`4 passed / 0 failed / 0 skipped`。
- `scripts/validate-source.py` 通过；WPF 静态审查为 `0 errors / 28 warnings / 177 info`（其余既有 XAML 提示未在本批改动）；`git diff --check` 通过。
- 未启动 Playnite、未触碰用户媒体或 profile；真实 Playnite 视频播放/MediaFailed 事件仍未验证。本批不改 XAML、布局或 R 账本；192 项唯一 ID 与 `106/83/1/1/1` 计数不变。
