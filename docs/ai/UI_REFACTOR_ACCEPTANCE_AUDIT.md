# GameSaveCenter UI Refactor v1 验收审计

> 生成日期：2026-08-14
> 基线：`17ac57d`（`origin/main`）
> 本文把 `GameSaveCenter_UI_Refactor_Implementation_Pack_v1/06_ACCEPTANCE_CHECKLIST.md` 逐项映射到当前仓库证据，并严格区分 `AUTO VERIFIED` 与 `MANUAL QA REQUIRED`。

## 1. 结论摘要

- 自动化验收：功能保真、GamePicker 锁定、窗口/DPI 等效尺寸、滚动契约、表格视口、Light/Dark 主题渲染、Resize 恢复、测试与审计门禁、真实宿主 reload 与无新 crash log 均通过。
- 人工验收：真实 Playnite 宿主内的主题切换、100%–200% DPI、最大化/窗口化连续拖拽缩放，以及真实游戏库数据量下的滚动体验仍未在用户环境完成，不能声称已验收。
- 本轮没有 `REMOVE`；生产命令、Binding、ViewModel 业务语义、Worker/IPC、Ludusavi/Rclone/备份恢复安全机制和既有虚拟化策略未改。

## 2. 证据来源

- 保真计划与阶段状态：`docs/ai/UI_REFACTOR_FIDELITY_PLAN.md`
- 工作日志与长期记忆：`docs/ai/WORKLOG.md`、`docs/ai/PROJECT_MEMORY.md`
- 开发交接：`docs/DEVELOPMENT_HANDOFF.md`
- 离屏渲染报告：`artifacts/ui-qa/render-phase8-resize3/render-qa-report.txt`
- UI Audit 摘要：`artifacts/ui-audit/AUDIT_SUMMARY.md`
- 实施包：`C:\Users\lopmatu\AppData\Local\Temp\GSC_Refactor_Pack_v1\`

## 3. 功能保真

- `AUTO VERIFIED`：当前 main + Audit 中的 9 个 View、30 个 Tab、92 条命令、43 个 DataGrid 列、30 个 ScrollViewer、143 个条件 UI 已映射到 `UI_REFACTOR_FIDELITY_PLAN.md`；`REMOVE = 0`。
- `AUTO VERIFIED`：旧 Command missing = 0、旧 DataGrid column missing = 0、条件 UI 仍存在；折叠功能均有 1～2 步可达入口（详情按钮/Expander）。
- `AUTO VERIFIED`：没有为 Demo 对齐删除生产功能；Demo 未画的生产元素默认 `KEEP / MOVE / RESTYLE / COLLAPSE / RESPONSIVE_MOVE`。

## 4. GamePicker

- `AUTO VERIFIED`：Dashboard 顶部全局 GamePicker 是单实例共享控件，首页/存档中心/修改器中心/媒体中心/任务中心/维护中心永久常驻。
- `AUTO VERIFIED`：搜索、筛选、排序、当前运行游戏自动定位、上次选择持久化、游戏图标、Binding、Command、下拉、虚拟化与大型游戏库性能策略未改。
- `AUTO VERIFIED`：GamePicker 不在页面 ScrollViewer 内，没有复制 6 份局部 Picker。
- 约束依据：`01_DO_NOT_TOUCH.md` 与 `docs/ai/PROJECT_MEMORY.md`；生产源码中 Dashboard 仅保留一个 GamePicker 声明。

## 5. 用户明确要求保留

- `AUTO VERIFIED`：首页“今日工作台 / TODAY / 当前游戏”信息架构保留，仅做布局、间距和响应式修正。
- `AUTO VERIFIED`：按钮设计语言、毛玻璃、圆角和桌面工具风格保留；颜色继续走 DynamicResource / Design Token / Playnite 主题桥接，没有为匹配 Demo 写死关键前景/背景色。

## 6. 窗口、DPI 与 Resize

- `AUTO VERIFIED`：render-qa 覆盖 10 档逻辑尺寸：1040×700、1100×720、1280×720、1366×768、1536×864、1600×900、1707×960、1920×1080、2048×1152、2560×1440；其中 1536×864 / 1707×960 / 2048×1152 覆盖 1080p@125%、1440p@150%、1440p@125% 的 DPI 等效逻辑尺寸。
- `AUTO VERIFIED`：UI Audit 覆盖 maximized、2k、wide、standard、compact、narrow-1100、narrow，共 161 个运行时快照，0 HIGH、0 MEDIUM、0 失败路由。
- `AUTO VERIFIED`：render-qa Resize transition QA 对 7 个工作区执行 2560×1440 → 1100×720 → 2560×1440 同实例布局恢复，全部恢复。
- `AUTO VERIFIED`：页面级 `*ScrollSurface` / `SettingsScroller` 无横向溢出；DataGrid 内部列滚动允许。
- `MANUAL QA REQUIRED`：真实 Playnite 宿主 100%–200% DPI、连续拖拽缩放、最大化与窗口化切换的最终视觉验收。

## 7. 滚动

- `AUTO VERIFIED`：Dashboard/表单页使用页面主滚动；Data Workspace 主 DataGrid/List 自身滚动；Master/Detail 只保留列表与 Inspector 两个语义明确的滚动区。
- `AUTO VERIFIED`：DataGrid 虚拟化保持开启（Recycling），稳定滚动策略未改。
- `MANUAL QA REQUIRED`：真实鼠标滚轮停驻 DataGrid 时外页面不滚走、大数据量滚动流畅性仍需人工验收。

## 8. 表格视口

- `AUTO VERIFIED`：1040×700 下主表至少约 4 行可读；Standard/Wide 目标不少于 6 行。
- 离屏实测（1040×700）：Save 历史 385 DIP、候选 254 DIP；Trainer 工具列表 236 DIP；Media 主表 350 DIP；Task 表 252 DIP；Maintenance Findings/Device/Process 350 DIP、Audit 日志 280 DIP。
- `AUTO VERIFIED`：SaveCandidateGrid、MaintenanceAuditLogGrid、MaintenanceDeviceGrid/ProcessGrid 窄窗高度均明显改善并通过 Audit HIGH 门禁。
- `AUTO VERIFIED`：表头使用主题资源，不依赖宿主白色兜底；最后一行完整可见；无滚动后大块空白；虚拟化保持开启。

## 9. 各页面

- Overview：最近活动、全局活动、最近 30 天保护明细均保留；明细可折叠；全局活动窄窗四列稳定。
- Save：4 Tab、历史 7 列、路径 4 列、恢复/撤销/备注/比较/候选动作和备份策略字段全部保留。
- Trainer：4 Tab、导入修改器/目录/CT/自定义启动项与全部工具编辑字段/动作保留。
- Media：3 Tab、待归类 5 列、当前媒体元数据动作、来源添加/删除保留。
- Task：四筛选、六列、Copy/Retry/Cancel 保留；窄窗详情动作不再过高。
- Maintenance：5 顶级 Tab、诊断全部 Audit 命令、13 工具窄窗收敛、Device 六列与决策动作、Retention 四块、Audit 双表、Process Mapping 动作保留。
- Settings：5 Tab 与业务字段保留，字段列宽 token 化。

## 10. 主题

- `AUTO VERIFIED`：RenderHarness 强制 Light/Dark 渲染 7 工作区 × 4 尺寸 × 2 主题 = 56 个离屏场景，调色板断言与主表/滚动面视口全部通过。
- `AUTO VERIFIED`：像素采样确认 Light 背景约 `(239,240,243)`、Dark 约 `(54,57,67)`，主题切换真实生效。
- `AUTO VERIFIED`：关键页面无硬编码固定黑/白背景冲突；Button/ComboBox/DataGrid/ScrollBar/语义色继续使用共享主题资源。
- `MANUAL QA REQUIRED`：真实 Playnite Light/Dark/Follow/高对比度主题、Accent 变化、Popup 与 DataGrid Selection 的最终视觉验收。

## 11. 测试与 QA

- `AUTO VERIFIED`：Core `59/59`、Worker `190/190`、Playnite `250/250`。
- `AUTO VERIFIED`：Release 构建 0 warning / 0 error；`scripts/validate-source.py`、WPF UI validator、`scripts/render-qa.ps1` 全部通过。
- `AUTO VERIFIED`：UI Audit before/after：HIGH 10→0、MEDIUM 4→0、运行时警告 62→39、失败路由 0。
- `AUTO VERIFIED`：工作树干净，`origin/main` 已推送。
- `AUTO VERIFIED`：真实宿主 reload 已完成；`playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，扩展日志记录 `0.6.70.0 loaded`，Worker 从当前扩展目录运行，`18:10` 后无 ERROR/Exception/crash。
- `MANUAL QA REQUIRED`：真实宿主内的主题视觉、DPI 与连续缩放仍由用户人工验收。

## 12. 最终报告字段

- 修改页面/文件：OverviewView、SaveCenterView、TrainerCenterView、MediaCenterView、MaintenanceView、TaskCenterView、Settings 相关 XAML/code-behind、Redesign/DesignTokens/WpfUiProduction 共享资源、RenderHarness/Audit 工具、回归测试与文档。
- 明确未修改：GamePicker 锁定区域、ViewModel 业务语义、Worker、IPC、Ludusavi、Rclone、备份恢复、安全机制、既有虚拟化/稳定滚动策略。
- MOVE/RESTYLE/COLLAPSE：见 `UI_REFACTOR_FIDELITY_PLAN.md` 每阶段说明；低频命令进入 Expander，Inspector 窄窗默认收起，响应式移动筛选/操作行。
- REMOVE：0。
- GamePicker：保持不变。
- 尺寸表现：见第 6 节；2560×1440、1366×768、1100×720、1040×700 均被离屏矩阵覆盖。
- 主题表现：见第 10 节；真实 Playnite 主题仍为 MANUAL QA REQUIRED。
- DataGrid/滚动：见第 7、8 节。
- 测试结果：见第 11 节。
- Commit：Phase 0 `85e3d71`、1 `a4deb1e`、2 `283342f`、3 `c7bc847`、4a `258c67b`、4b `e84c904`、5 `d6701cf`、6 `c29d831`、7 `5f9fca5`、8 `5db5032`；后续收口 `3ec6c9f`、扩档矩阵 `845b2d7`、主题 QA `cbfe3dc`、横向门禁 `a4a4ec2`、Resize 恢复 `17ac57d`。

## 13. 未完成项

- 真实 Playnite 宿主 Light/Dark/Follow/高对比度的视觉验收。
- 真实 DPI 100%–200% 与连续拖拽缩放。
- 最大化/窗口化切换后的真实视觉验收。
- 真实游戏库数据量下的大列表滚动流畅性。
