# GameSaveCenter UI Table and Chip Closure Plan v6.2

> 来源：`GameSaveCenter_UI_Table_and_Chip_Fix_Pack_v6_2.zip`
> 原则：不推翻 v6/v6.1 大方向；只做表格细节、chip、时间列、进度条与 4K 撑满收口；`REMOVE=0`；不新增 GamePicker；不引入大型重构。

## 1. Chip 样式：胶囊 pill → 圆角矩形 chip

- 当前证据：`artifacts/ui-qa/v6-shots/v6-overview-activity-wide.png` 中 “维护 / 信息 / 成功 / 失败” 标签为 `GscRedesignContextPill`（CornerRadius=12、Padding=10,5），视觉接近胶囊。
- 当前样式位置：`src/GameSaveCenter.Playnite/Themes/Redesign.xaml` → `GscRedesignContextPill`（约 210-218）、`GscRedesignTableStatusPill`（约 250-257）。
- 根因：共享 pill 样式两端圆角过大，且 height/padding 与“状态 chip”语义不匹配。
- 精确修改：
  - `GscRedesignContextPill`：CornerRadius `12` → `7`，Padding `10,5` → `11,5`，增加 `MinHeight="26"`。
  - `GscRedesignTableStatusPill`：CornerRadius `9` → `7`，Padding `8,3` → `10,3`。
  - 保留 Background/BorderBrush/BorderThickness 的 DynamicResource 语义；Kind chip 保持低饱和分类色，Result chip 保持状态色（成功绿/警告黄/失败红/信息蓝/中性）。
- 保留：所有 Binding、Command、DataTrigger 颜色切换逻辑。
- 尺寸：1040×700 与 2K/4K 均保持 24~28 DIP 高度。
- 主题：Dark/Light 均沿用现有 token，不写死前景/背景。
- 验收：截图对比 chip 圆角；`OvernightClosureV6Tests` 增加 CornerRadius=7 断言。

## 2. 时间列右内边距

- 当前证据：`v6-overview-activity-wide.png` 时间列文本贴右边缘。
- 当前控件位置：`OverviewView.xaml` → `OverviewActivityTimelineList` 行模板 `ActivityTimeColumn`（Width=112，无右 padding）。
- 根因：自定义 Grid 模板没有为最后一列保留右侧安全边距；共享 DataGrid 最后一列也仅靠 `DataGridCell` 的 `Padding=12,8`。
- 精确修改：
  - `OverviewView.xaml` 时间列 TextBlock 增加 `Margin="12,0,20,0"`。
  - 共享 `DataGridCell` Padding 从 `12,8` 调整为 `12,8,20,8`，保证所有表格最后一列与 frame 右侧保持 ≥20 DIP。
  - 检查 Task/Media/Maintenance 最后一列 ElementStyle 不再额外压缩。
- 保留：时间绑定 `CreatedDisplay`、DataGrid 列宽与滚动。
- 尺寸：1040 窄窗与 4K 宽窗均不贴边；时间列仍完整显示 `MM-dd HH:mm`。
- 主题：无新增颜色。
- 验收：render-qa/截图检查时间列右间距；静态断言 `Margin="12,0,20,0"` 与 DataGridCell Padding。

## 3. 表格列间距与列宽协调

- 当前证据：Global Activity 六列中 Summary 与 Kind/Result/Time 之间仍偶有空洞。
- 当前控件位置：`OverviewView.xaml` 活动行模板列：`40 | 150 | * | 88 | 76 | 112`。
- 根因：Kind/Result 固定列偏窄、Time 无右 padding，导致视觉“左一坨、右一坨”。
- 精确修改：
  - 列模型调整为 `40 | 150 | * | 96 | 84 | 112`；Kind/Result chip 使用 `HorizontalAlignment="Left"` + 固定槽位。
  - Summary 保持 `*` 主伸缩；行高保持 52 DIP。
  - 窄窗继续隐藏 header、chips 下移，字段不丢。
- 保留：Glyph/GameName/Summary/KindDisplay/ResultDisplay/CreatedDisplay 绑定。
- 尺寸：1040 与 4K 均协调。
- 主题：无新增。
- 验收：`UiLayoutRegressionTests.OverviewGlobalActivityUsesStableFourColumnRow` 更新为 `40|150|*|96|84|112`。

## 4. Progress bar：真实可视进度条 + 数值

- 当前证据：TaskCenter 进度列/详情已有真实 ProgressBar；SaveCandidate “可信度”仍是纯 `StringFormat=P0` 文本。
- 当前控件位置：`SaveCenterView.xaml` → `SaveCandidateGrid` 第一列（`Score`）。
- 根因：可信度是 0~1 的百分比语义，但只用文本，5% 与 100% 视觉无差别。
- 精确修改：
  - `SaveCandidateGrid` 第一列改为 `DataGridTemplateColumn`：左侧小型 ProgressBar（Height 8、MinWidth 60）+ 右侧百分比文本。
  - 不改变 `Score` 绑定与候选业务逻辑。
  - 全项目扫描确认 Task/Overview 已有真实进度条；Settings 数值不是业务进度，不改。
- 保留：`Score`、`StatusDisplay`、`Path`、`ReasonsDisplay` 等全部列与命令。
- 尺寸：1040 下进度列保持可读，不增加行高。
- 主题：Fill 使用 `GscAccentBrush`，Dark/Light 均可读。
- 验收：v6.2 截图“含 progress bar 的表格页”；静态断言模板含 ProgressBar + Score。

## 5. Maintenance 2K/4K 表格撑满

- 当前证据：`MaintenanceView.xaml.cs` 仍对 `FindingsGrid`、`MaintenanceDeviceGrid`、`MaintenanceAuditFindingsGrid`、`MaintenanceProcessGrid` 设置 `MaxHeight = tableViewportHeight`（上限约 460 DIP），4K 下主表不会随工作区伸展。
- 根因：旧有限视口策略为了防挤压，但现在 v6.1 要求主表占满 `*` 工作区。
- 精确修改：
  - Diagnostics 问题列表、Device、Audit、Process 主表取消 `MaxHeight` 上限（`double.PositiveInfinity`），保留 `MinHeight` 与 `Height=NaN`。
  - 保留 inspector 堆叠与内部滚动；页面滚动只属于明确允许的概览页。
  - 增加 render 探针记录 `DataWorkspaceActualHeight / DataGridActualHeight / FillRatio / TopExternalGap / BottomExternalGap`。
- 保留：全部命令、绑定、虚拟化、Inspector。
- 尺寸：1040×700 主表仍 ≥252 DIP；2K/4K FillRatio ≥0.92（有 inspector 时合理接近）。
- 主题：无新增。
- 验收：UI Audit/render-qa 输出 FillRatio；截图 Diagnostics/Device/Audit。

## 6. 交付物

- `docs/ai/UI_TABLE_AND_CHIP_CLOSURE_REPORT_V6_2.md`
- v6.2 截图：Global Activity 标准/窄、含 progress 表格、Maintenance Diagnostics/Device/Audit（2K/4K）
- 新 UI Audit
- commit SHA

## 7. 完成状态（2026-08-15）

- Chip 圆角矩形：已完成，共享样式 `GscRedesignContextPill` / `GscRedesignTableStatusPill`。
- 时间列右留白：已完成，共享 `DataGridCell` 右 Padding 20 DIP，Overview 时间列 `Margin=12,0,20,0`。
- 列宽协调：已完成，Overview 六列 `40|150|*|96|84|112`。
- 可视进度条：已完成，SaveCandidate 可信度列 ProgressBar + `P0` 文本。
- Maintenance 2K/4K 撑满：已完成，取消 460 DIP 上限，Device/Process 布局改 Stretch；2K/4K fill ratio 见报告第 5 节。
- 验证：Playnite `263/263`；render-qa 11 档 + 主题 + Resize 全绿；UI Audit 0 HIGH/0 MEDIUM；截图 `artifacts/ui-qa/v6-2-shots/`。
- 实施提交：`c58b359`、`6a68a59`；报告 `docs/ai/UI_TABLE_AND_CHIP_CLOSURE_REPORT_V6_2.md`。
