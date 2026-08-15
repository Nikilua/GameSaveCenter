# GameSaveCenter Audit11 Residual UI Closure 报告

## A. Git

```text
start commit: dc5aa7b
end commit:   （见 git log）
branch:       main
working tree: clean
```

## B. SaveHistory Size

```text
old: 列宽 90 DIP，SaveLongText（CharacterEllipsis），25.56 MiB 被显示为 25.56...
new: 列宽 116 DIP，SaveSizeValue（TextTrimming=None，Tag=SaveHistorySize）
```

`SHORT_SEMANTIC_VALUE_TRIMMING=0`；narrow 下备注列仍为 0，状态列 96 DIP 完整可见；未开启横向滚动。标准/紧凑/narrow 三档均通过。

## C. Device Inspector

```text
compact old viewport ≈ 90 / extent 508
narrow old viewport  ≈ 90 / extent 508
```

新交互：Compact/Narrow 默认收起 Inspector，主表保留 252–280 DIP；表格下方独立“查看设备详情 ›”按钮；点击后 Inspector viewport >= 180 DIP（可用高度不足时动态计算），表格保留约 header + 2 行（MinHeight 150）。Wide 保持右栏 Inspector。`INTERACTIVE_INSPECTOR_USABILITY=0`。

## D. Settings P2

- 对选中分类滚动目标做整数取整，减少滚动边缘的半像素文字碎片。
- 不改变 `ACTIVE_TAB_VISIBILITY` 行为：选中分类仍完整可见，最后分类 HorizontalOffset 滚到末端。
- 横向 strip 在末端左侧出现上一分类少量文字属“还有更多”的滚动提示；未加 fade/复杂 shader，避免影响键盘焦点与 Automation。

## E. Audit

```text
SHORT_SEMANTIC_VALUE_TRIMMING: 0
INTERACTIVE_INSPECTOR_USABILITY: 0
```

修复前可复现问题均被对应检测覆盖；修复后 Fidelity 总数为 0。

## F. Regression

- Maintenance header：未回退，`HEADER_CONTENT_FIDELITY=0`。
- Media search：未回退，`CONTROL_USABILITY_GEOMETRY=0`。
- Settings active tab：未回退，`ACTIVE_TAB_VISIBILITY=0`。
- SaveHistory status：未回退，`ESSENTIAL_COLUMN_VISIBILITY=0`。
- virtualization / `Item` scroll / stable row：未改动，既有 probe 全绿。

## G. Tests

- Release 构建：0 warning / 0 error
- Core：59/59
- Worker：190/190
- Playnite：276/276（新增 3 条回归）
- `validate-source.py`、`check-xaml.ps1`、WPF 技能静态审查：通过
- render-qa：11 档 + 56 主题 + 7 Resize 全绿
- UI Audit：HIGH=0、MEDIUM=0、Fidelity=0、failed routes=0

注：标准 `artifacts/GameSaveCenter-ui-audit.zip` 被其他进程锁定，本轮审计包输出到 `artifacts/audit11-final/GameSaveCenter-ui-audit.zip`。

## H. Manual QA remaining

- 真实 Playnite 宿主、第三方主题、键盘焦点、连续缩放仍需人工验收。
- 自动 harness 为 `DpiScale=1.0`，125%/150% DPI 未自动仿真；重点检查 SaveHistory size、Device inspector、Settings category strip。
