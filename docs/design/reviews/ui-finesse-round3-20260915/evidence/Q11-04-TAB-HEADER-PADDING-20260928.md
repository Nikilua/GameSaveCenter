# Q11-04 页签内边距与长文本几何（2026-09-28）

## 当前结果

审阅共享 `GscRedesignWorkspaceTabItem` 后确认已有 `14×7 DIP` 内边距、`11 DIP` 圆角和独立 `8 DIP` 页签间距；本阶段没有改生产 XAML/样式。新增真实 WPF TabControl/TabItem 几何行为用例，使用短中文“历史版本”、长英文“Validation and restore history”及“待归类 + 200”组合标签，在 Light/Dark 主题各测一遍。

每项实际布局中检查：

- 文本完整按自然宽度呈现，未启用字符裁切或自动折行。
- Header 内容保持在 BorderThickness 与 Padding 所界定的 `15 DIP` 圆角安全内缩区域内；圆角为 `11 DIP` 且 `ClipToBounds=False`。
- 计数徽章不与中文标签重叠，计数文本完整落在徽章内部。
- 分别选中短、长、计数页签时，`PART_SelectedContentHost` 的实际宽度变化不超过 `0.25 DIP`；不同主题页签槽宽变化不超过 `0.25 DIP`。
- 所有 TabItem 高度至少 `36 DIP`，长文本或计数内容没有压缩选中页内容视口。

这是合成 Header 内容、STA WPF 窗口中的生产共享资源验证；不是当前各业务页的真实 Playnite 截图，也未验证 OS 鼠标/键盘、Playnite Follow Host 主题、物理 DPI 或实际屏幕边缘滚动行为。Q11-04 保持最终未完成。

## 构建与测试

- 最终测试身份：`9804f494`（完整提交 `9804f494c77f97c6a38a81e56c861f9918a4b9ea`）。
- Release `GameSaveCenter.Playnite.Tests` 项目构建成功。
- `R23ProductionResourceStateBehaviorTests`：`5/5` passed、0 failed/skipped，VSTest exit `0`；TRX：`artifacts/q11-04-tab-padding-20260928/q11-04-r23-resource-state-9804f494.trx`。
- 此最终 TRX 无 `InvalidComObjectException` 清理噪声。

