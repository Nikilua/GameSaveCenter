# GameSaveCenter UI Fidelity Closure（Audit10）报告

## A. Git 基线

```text
start commit: 4f8e37f
end commit:   （见 git log）
branch:       main
remote:       github.com/Nikilua/GameSaveCenter.git
working tree: clean
```

## B. Maintenance header 根因

- MaintenanceView 曾声明局部 implicit `Style TargetType="DataGridColumnHeader"`（BasedOn `{StaticResource {x:Type DataGridColumnHeader}}`），并覆盖 `OverridesDefaultStyle=True` 与主题色。
- 首列/末列使用显式 `MaintenanceFirstColumnHeader / MaintenanceLastColumnHeader`，模板仍能解析；中间真实列走 `GscDataGridColumnHeaderStyle`，在该局部作用域下模板解析断裂，出现“metadata 有值、视觉子树为空”。
- 修法：移除 Maintenance 局部 implicit header style。真实列继续统一使用共享 `GscDataGridColumnHeaderStyle`；filler header 由生产 `DataGridColumnHeadersPresenter` 背景与 `ColumnHeaderStyle` 保持插件主题。
- 修复后 visual-tree 中 游戏/标题/详情/分类/目标游戏 等 header 均重新出现 TextBlock 子树，Sort/Hover/Focus 行为沿用共享模板。

## C. 缺失 header 清单（全部恢复）

```text
Diagnostics: 游戏 / 标题 / 详情
Device: 游戏 / 其他设备 / 原因 / 人工决策
Audit Findings: 游戏 / 标题
Audit Log: 分类
Process Mapping: 目标游戏
```

## D. Media Search

```text
old ActualWidth ≈ 54.67 DIP
new geometry   = Grid Auto / * (MinWidth=160) / Auto / 150，TextBox Grid.Column=1 + MinWidth=160 + Stretch
```

narrow 档 render-qa 中搜索框内容宽度约 390 DIP；Standard/Compact/Narrow 均通过 `CONTROL_USABILITY_GEOMETRY`。

## E. Settings scroll

- `OnSettingsTabSelectionChanged` 同步调用 `ScrollSelectedCategoryIntoView()` 并安排 Loaded 级重试；`ApplyResponsiveLayout` 末尾也同步调用。
- 保留 `selected.BringIntoView()` 安全回退，随后按当前 scroll 坐标计算增量 delta，最多 3 轮收敛，避免双重偏移与持续抖动。
- 程序化 route/tab 切换（UI Audit harness）已验证；narrow/compact 下 `ACTIVE_TAB_VISIBILITY=0`，最后分类 `HorizontalOffset=359.33` 完整可见。

## F. Narrow essential columns

- Save History：narrow（<1100 DIP）收起备注 summary 列（宽度 0、MinWidth 0），完整备注仍在版本详情 Inspector；状态列留在视口内，`ESSENTIAL_COLUMN_VISIBILITY=0`，未开启横向滚动。
- 其他 DataGrid：SaveCandidate 长文本保持 ellipsis + Inspector 全文；Task 核心字段可达；Maintenance Device 详情由 Inspector 承载；均未全局开启横向滚动。

## G. Audit 新能力

```text
HEADER_CONTENT_FIDELITY        metadata 非空但无渲染内容 → MEDIUM + 失败门禁
ACTIVE_TAB_VISIBILITY          selected 横向分类不在 viewport → MEDIUM + 失败门禁
CONTROL_USABILITY_GEOMETRY     媒体搜索框 < 160 DIP → MEDIUM + 失败门禁
ESSENTIAL_COLUMN_VISIBILITY    SaveHistory 状态列越界 → MEDIUM + 失败门禁
```

修复前审计可复现 92 个失败（Header 77、搜索框 7、状态列 2、分类 6），修复后全部归零。

## H. Tests

- Release 构建：0 warning / 0 error
- Core：59/59
- Worker：190/190
- Playnite：273/273（含 5 条新增回归）
- `validate-source.py`、`check-xaml.ps1`、WPF 技能静态审查：通过
- render-qa：11 档窗口 + 56 主题场景 + 7 Resize 全绿

## I. UI Audit

```text
HIGH: 0
MEDIUM: 0
Text-Fit: 0
Header Fidelity: 0
Active Tab Visibility: 0
Geometry: 0
failed routes: 0
unexpected clipping: 0
```

运行时警告 65，均为预期滚动类 INFO。最终 Audit ZIP：`artifacts/GameSaveCenter-ui-audit.zip`。

## J. Manual QA remaining

- 自动 harness `DpiScale=1.0`，125%/150% DPI 未自动仿真。
- 真实 Playnite 宿主、第三方主题、键盘焦点、连续缩放仍需人工验收；重点页面：Maintenance 表头、Media 搜索框、Settings 分类滚动、Save History 状态列、GamePicker 中英文长名。
