# GameSaveCenter Post-Typography Geometry Closure Report

## 基线

- 开始：`978e437`（`main`，与 `origin/main` 同步）
- 分支：`main`
- 远程：`github.com/Nikilua/GameSaveCenter.git`
- 工作区：提交后 clean

## 已确认问题

Maintenance 诊断 → 问题列表“等级”列：

```text
old = 72 DIP
new = GscSeverityColumnWidth（92 DIP，DataGridLength token）
```

根因：字体统一后中文真实度量变大，72 DIP 减去共享 DataGridCell Padding 后，Pill 内容区不足以容纳“提示/警告/错误”等两字状态词。异常审计同款 Pill 的 92 DIP 列已验证可读，因此采用共享 92 DIP token，而不是缩小字体或全局增宽。

## 全仓 Geometry Audit

扫描范围：所有 `Width/MinWidth <= 120` 且承载文本的 DataGrid 列、Pill/Badge、Button、Tab、ComboBox/TextBox、Inspector action、Settings category。

修改：

- Diagnostics 等级：72 → `GscSeverityColumnWidth` 92。
- Audit Findings 等级：92 → 同一 `GscSeverityColumnWidth` token（数值不变）。

核对后保持不变：

- Task 状态列 110、Save 历史状态列 96、Save 历史大小列 90、Media 待归类类型列 72、Audit Log 分类列 100、Process 操作列 90。
- Overview 各按钮 `MinWidth` 72-92 为下限，按钮按内容自动增长，不裁切。
- 全局行高/表头高/按钮高未放大；DataGrid virtualization、`Item` scroll unit、stable row 与既有 geometry probe 未改动。

## QA 改进

- `UiLayoutAnalyzer` 新增 Text-Fit 检测：用 `FormattedText` 计算无约束文本宽度，与 TextBlock `ActualWidth` 对比。
- 排除 `TextWrapping=Wrap` 与 `TextTrimming != None` 的 intentional trimming；路径、长游戏名、详情描述不误报。
- `TEXT_FIT` 以 MEDIUM 记录，`UiAuditRunner` 在存在任何 `TEXT_FIT` 时返回失败码，防止回到 72 DIP 式问题。
- visual-tree exporter 修复：离屏 host 未连接 PresentationSource 时 `IsVisible` 恒为 false，改为 `Visibility == Visible` + 非零尺寸判断；当前 175 个 visual-tree JSON 全部包含真实节点（TextBlock/ContentPresenter/Border/Button/DataGrid cell 等）。

## Responsive Regression

- Settings：render-qa 760×560 正文 viewport 300 DIP、880×560 285 DIP、1040×700 201 DIP，均不低于 180 DIP。
- Save Compare：1040×700 主比较 viewport 234 DIP（>= 220），保留策略 246 DIP。
- Compact Inspector：Save/Trainer/Media/Task 详情按钮仍为独立 `Grid.Row=1` 操作行，无 overlay。
- Media 待归类底栏：`Margin="12,10,12,0"` 与窄窗换行保持不变。

## Tests

- Release 构建：0 warning / 0 error。
- Core：59/59。
- Worker：190/190。
- Playnite：268/268（含 2 条新增回归）。
- `validate-source.py`、`check-xaml.ps1`、WPF 技能静态审查：通过。
- render-qa：11 档窗口 + 56 主题场景 + 7 Resize 全绿。

## UI Audit

```text
HIGH: 0
MEDIUM: 0
failed routes: 0
text-fit: 0
unexpected clipping: 0
```

运行时警告 65，均为预期滚动类 INFO（`EXPECTED_INTERNAL_SCROLL` / `EXPECTED_SIBLING_SCROLL`）。最终 Audit ZIP：`artifacts/GameSaveCenter-ui-audit.zip`。

## 剩余真实 Playnite 风险

- Audit 与 render-qa 的 `DpiScale=1.0`，125%/150% DPI 未自动仿真。
- 真实 Playnite 宿主、第三方主题、键盘焦点与连续缩放仍为 `MANUAL QA REQUIRED`，需人工检查：Maintenance 诊断/审计等级词、Save History、Media Inbox、Task Center、Settings category scroll-into-view、GamePicker 中英文长名。
