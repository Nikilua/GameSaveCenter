# GameSaveCenter UI Visual Rework Plan v3

> 来源：`GameSaveCenter_UI_Design_and_Prompt_Pack_v3.zip`
> 生成日期：2026-08-14
> 原则：最新 main / 生产功能 > 用户 5 张截图 > v3 文档 > 旧 audit/refactor 包 > 旧 demo；`REMOVE = 0`；GamePicker HARD LOCK；不直接改，先按本计划实施。

## 图一：首页当前游戏卡 —— 三按钮强制一排

- 用户问题：`立即备份 / 刷新详情 / 查看需关注项` 被拆成两行；三按钮几何不统一；卡片层级一般。
- 受影响：`OverviewView.xaml` → `OverviewCurrentGameCard` 内 `WrapPanel`（约 line 175）。
- 当前结构：三个 `ui:Button` 使用 `GscWpfUiPrimaryActionButton` / `GscWpfUiActionButton`，`MinWidth` 92/92/116，1040×700 下会换行。
- 拟改结构：
  - 三个按钮统一使用紧凑几何：`MinHeight=38`、`MinWidth=100`、间距 6、同圆角。
  - 第一个按钮保留 Primary 视觉（仅 `Appearance=Primary` 或 `GscWpfUiCompactButton` + Primary 外观），其余 Secondary。
  - 标准宽度（当前游戏卡内容宽度足够）强制同一行；只有窄窗 fallback 允许 2+1 换行，不形成三行按钮墙。
- 操作：`RESTYLE / RESPONSIVE_MOVE`
- 保留：`BackupSelectedCommand`、`LoadDetailsCommand`、`OpenAttentionCenterCommand`、AutomationProperties。
- 预期标准态：三按钮同一行、等高、等宽、右对齐。
- 预期窄窗态：允许 `[立即备份][刷新详情]` + `[查看需关注项]` 两行 fallback。
- 验收：render-qa 1040/1100/1366/2560 记录三按钮 Y 坐标与高度差；截图人工确认。

## 图二：最近 30 天玩过的游戏 —— 动作/统计/折叠三层

- 用户问题：动作按钮与统计气泡视觉混淆；折叠控件像输入框；Chevron 与文字不对齐；尾部多余 `>`。
- 受影响：`OverviewView.xaml` 风险卡内 `RecentProtection` 区域（约 line 415-453），共享 `GscDisclosureCardExpander`。
- 当前结构：动作按钮与统计 chips 都接近按钮外观；统计 chips 使用 `GscRedesignContextPill`；Disclosure header 尾部有 `>`。
- 拟改结构：
  - 主动作按钮：`查看需要处理的游戏` / `启用所选自动保护` 统一为同一组操作按钮规格。
  - 统计 chips：`未识别存档 N` 使用中性蓝灰；`需要处理 N` 使用警告橙/黄强调，明确非按钮。
  - 所有 Disclosure/Expander 统一到新 `GscDisclosureCard`：Chevron 独立图标区、垂直居中、无尾部 `>`、12~14 圆角、Hover/Pressed/Expanded 状态明确。
  - 适用范围：最近保护明细、首次环境检查、更多维护操作、其他项目同类折叠。
- 操作：`RESTYLE`
- 保留：`OpenProtectionGamesCommand`、`ApplyRecommendedProtectionCommand`、`RecentProtection.Items`、Disclosure 内容。
- 预期标准/窄窗：折叠态 `▸ 展开最近游戏保护明细`，展开态 `▾ 收起最近游戏保护明细` + 分隔线。
- 验收：截图折叠/展开两态；检查 Chevron 与文字基线。

## 图三：全局活动 —— 轻量表格式活动列表

- 用户问题：列间距过大、列宽不自适应、日期不完整、与 DataGrid 视觉不搭。
- 受影响：`OverviewView.xaml` → `OverviewActivityTimelineList` ItemTemplate（约 line 306 起）。
- 当前结构：四列 `38 | * MinWidth=120 | 110 | 96`，Meta 列是两行文本，Time 列窄。
- 拟改结构：
  - 行高约 64~72 DIP，带轻分隔线与 hover 高亮。
  - Time 固定宽度约 112 DIP，保证 `08-14 20:40` 完整显示。
  - Meta 改为分类 chips（维护/信息/云端/媒体等），不再用两行裸文本。
  - Main 列 ellipsis + ToolTip；窄窗将 chips 移到摘要下方（沿用 Tag 响应式机制）。
- 操作：`RESTYLE / RESPONSIVE_MOVE`
- 保留：Glyph、GameName、Summary、KindDisplay、ResultDisplay、CreatedDisplay。
- 预期标准态：Icon | Main | Chips | Time 一行，行高 64~72。
- 预期窄窗态：Icon | Main | Time 一行，Chips 在摘要下方。
- 验收：render-qa 记录行宽/时间列宽；截图标准/窄窗。

## 图四：存档中心当前存档规则 —— 状态一行 Badge + 按钮对齐

- 用户问题：按钮不对齐、校验状态两行、卡片偏高、挤占表格。
- 受影响：`SaveCenterView.xaml` → `SaveCurrentRuleCard`（约 line 158 起）。
- 当前结构：状态区是两个 TextBlock 上下两行；按钮在 compact 时移到第二行。
- 拟改结构：
  - 状态改为一行 badge：`校验状态：风险` / `正常` / `待确认` / `未扫描`，按状态着色（风险=橙黄、正常=绿、待确认=蓝灰、未扫描=中性灰），全部走 DynamicResource。
  - 三个按钮统一 `MinHeight=38`、`MinWidth=100`、间距 8、同基线、同圆角；Primary 仅改底色。
  - 卡片进一步压缩 padding，标准态约 100~120 DIP 自然高。
- 操作：`RESTYLE`
- 保留：`DetectPathsCommand`、`ValidateCommand`、`LoadDetailsCommand`、`SaveCandidateGrid` 4 列。
- 预期标准/窄窗：状态一行 badge + 三按钮同一行；窄窗按钮行允许 2+1 fallback。
- 验收：SAVE-001/002 断言 + 截图。

## 图五：维护中心诊断 —— 初始尽量无滚动，主表首屏

- 用户问题：折叠卡简陋；页面初始有滚动条；表格被压成“只剩两列”视觉。
- 受影响：`MaintenanceView.xaml` → `EnvironmentCheckCard`、`MaintenanceDiagnosticsActionCard`、`MaintenanceDiagnosticsLayout`、`FindingsGrid`。
- 当前结构：Diagnostics 已是 Grid 无外层页滚；环境/低频/运行状态收进 Disclosure；FindingsGrid 5 列 min widths 合计约 932 DIP，1040×700 需要横向滚动。
- 拟改结构：
  - 保持“初始无页面滚动条”现状；所有折叠内容展开后内部使用有限滚动，页面本身不新增外层 ScrollViewer。
  - FindingsGrid 列最小宽度下调：等级 72、游戏 120、标题 160、详情 `*` MinWidth=180、建议处理 140，合计约 672 DIP，1040 下五列表头完整可见。
  - 环境摘要卡压缩为一行/两行 Summary Strip；首次环境检查、更多维护操作统一使用 `GscDisclosureCard`。
  - 详情列 ellipsis；建议处理列在窄态保留表头与列，不整列隐藏。
- 操作：`RESTYLE / RESPONSIVE_MOVE`
- 保留：FindingsGrid 5 列、全部诊断命令、环境检查项、元数据灾备/路径迁移字段。
- 预期标准/窄窗：首屏可见 FindingsGrid Header + 至少 4 行；展开折叠后内容可滚动。
- 验收：MAINT-001/002/003 + render-qa 截图；1040 下五列表头完整。

## 颜色系统补强

- 风险状态 badge、校验状态 badge、最近 30 天统计 chips、全局活动 chips、诊断等级标签统一分层：
  - 正常=绿、信息=蓝、警告=黄/橙、错误=红、中性=灰蓝。
- 全部使用现有 DynamicResource / Design Token，不写死纯色。
- 涉及：`OverviewView.xaml`、`SaveCenterView.xaml`、`MaintenanceView.xaml` 局部样式。

## 验收与交付

- 每张截图对应问题：图一~图五逐项关闭。
- 截图路径：`artifacts/ui-qa/v3-*`。
- UI Audit：0 HIGH / 0 MEDIUM。
- 功能保真：REMOVE=0、old command missing=0、old grid missing=0、old column missing=0。
- 提交：每个功能块独立 commit 并 push。

## 完成状态（2026-08-14）

- 图一~图五均已实施并关闭：Overview 三按钮同排同几何、保护动作/统计/折叠三层分离、活动轻量表四列；Save 状态一行 Badge 与三按钮对齐；Maintenance 环境摘要卡 + `GscDisclosureCard` + FindingsGrid 五列收敛。
- `GscDisclosureCard` 已在 `DesignTokens.xaml` 统一；旧 `GscDisclosureCardExpander` 保留为别名，不再用于页面；Media/Save/Task 的旧 `GscExpander` 引用也已替换，页面不再引用旧样式。
- Maintenance 两个主 Disclosure（首次环境检查 / 更多维护操作）内容使用内部有限滚动，展开后滚动条出现在折叠卡内部，FindingsGrid 首屏和主表视口不被挤掉。
- render-qa 新增三按钮对齐探针；为修复 1536×864 下“查看需关注项”换行，Overview Hero/当前游戏列改为 1:1，10 档尺寸均同排同高。
- 功能保真：REMOVE=0；`DetectPathsCommand`、`ValidateCommand`、`LoadDetailsCommand`、FindingsGrid 5 列、EnvironmentCheckItems 与全部维护命令保留；GamePicker HARD LOCK 未改。
- 验证：Release 0 warning/0 error；Core `59/59`、Worker `190/190`、Playnite `253/253`；`check-xaml.ps1`、`validate-source.py`、WPF 静态审查（0 error）通过；render-qa 10 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/32 INFO/0 失败路由。
- 截图与证据：`artifacts/ui-qa/v3-shots/`（10 张，脚本 `scripts/capture-v3-shots.ps1`）、`artifacts/ui-qa/v3-final/`；`artifacts/ui-audit/v3-final/AUDIT_SUMMARY.md`。
- 提交：`5c3bdae`（计划）、`9ee3660`（Overview）、`e8b8c31`（Save/Maintenance）及最终补强/文档提交，均已 push 到 `origin/main`。
- 未验证项：真实 Playnite 宿主主题、DPI 与连续缩放的最终视觉仍为 `MANUAL QA REQUIRED`。
