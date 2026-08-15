# GameSaveCenter UI Final Polish Report v7.1

> 来源：`GameSaveCenter_UI_Final_Polish_Pack_v7_1.zip`
> 范围：最后一轮可见问题收尾；`REMOVE=0`；不改 ViewModel/Worker/IPC/备份恢复/安全机制；GamePicker 与 Overview/Save Policy 锁定结构未动。

## Fixed

### 1. 首页活动行五列重构
- 行模板由六列（Icon/Scope/Message/Kind/Result/Time）收敛为五列语义：`Icon | Scope | Message(*) | MetaChip | Time`。
- chip（Kind/Result）永远放在独立横向 MetaChip 列，不再在窄窗退化回 Scope 下方上下堆叠；删除 `ActivityMetaCompact` 路径。
- Message 是唯一主伸缩列（`*` MinWidth 140，窄窗 100）；Time 固定列，右留白 `Margin="8,0,20,0"`（≥20 DIP）。
- 宽模型 `40|150|*|132|112`；窄模型 `36|130|*|112|96`（通过 Compact DataTrigger 只调列宽）。
- chip 继续使用 `GscRedesignContextPill`（圆角矩形 7、MinHeight 26），文本三向居中。

### 2. POSSIBLE_CLIPPING 清扫
- 修复 Maintenance 六个指定 Tab 的重复 clipping：诊断概览“最近检查”、设备状态“一致或仅单端存在”、保留策略“12 个版本/超出保留窗口或桶位”、发现的问题“Baldur's Gate 3”、审计记录“Backup”、进程映射“skse64.exe”。
- 修复方式：给被裁切 TextBlock 补 `TextTrimming + ToolTip`，给 DataGrid 列补共享 `MaintenanceLongText` ElementStyle，保留策略/存储分析绑定文本补 trimming。
- 同步清扫其它页面同类问题：Save History 时间/文件数/大小/设备、Save Candidate 状态、Task 本地时间/任务、Media Inbox 时间/类型、Trainer 标题/拖拽提示/启动延迟、Settings 毛玻璃强度百分比。
- Audit 增强：`POSSIBLE_CLIPPING` 消息现在包含元素名、父元素与文本片段；比较时扣除元素左右 Margin，消除 `DesiredSize` 把 Margin 计入的误报。
- 最终结果：`POSSIBLE_CLIPPING=0`。

## Not changed intentionally

- Overview TODAY 大卡、当前游戏卡、顶部按钮、指标小卡整体结构。
- Save Backup Policy 表单结构（label + input + unit + helper）。
- Maintenance 主/子 Tab 信息架构与 4K stretch 策略。
- GamePicker、页面会话/默认页逻辑。
- 所有命令/绑定/业务字段；未新增内部 ScrollViewer；未用分辨率 if/else 硬编码。

## Before / After

- 活动行：chip 从 scope 下堆叠 → 独立 MetaChip 横向列；Message 主伸缩；Time 右留白 20 DIP。
- Maintenance clipping：六个指定 Tab 的 INFO 裁切全部清零。
- 全量 clipping：audit7 约 100+ 条重复 INFO → v7.1 `POSSIBLE_CLIPPING=0`。
- 截图证据：`artifacts/ui-audit/v7-1-final/screenshots/`（2k / standard / narrow 等），参考包截图 `ReferenceScreenshots/01_user_activity_row_issue.png`。

## Tests / Audit

- Playnite：`263/263`。
- render-qa：11 档窗口（含 3840×2160）+ 56 主题场景 + 7 Resize 全绿。
- 最终 UI Audit：0 HIGH / 0 MEDIUM / 0 失败路由；`POSSIBLE_CLIPPING=0`；列/纵向/表头/TextBox/父子滚动指标保持 0。
- check-xaml / validate-source / validate_wpf_ui：通过（0 error）。

## Remaining manual QA

- 真实 Playnite 宿主下 Light / Dark / Follow 主题与 125/150% DPI、真实 4K Windows scaling：`MANUAL_REQUIRED`，不假 PASS。

## Commit SHA

- `037eb17` docs: add ui final polish plan v7.1
- `702b0d5` feat: v7.1 overview activity row five-column redesign
- `f6f17a8` feat: v7.1 clipping and spacing sweep

最终 Audit ZIP：`artifacts/GameSaveCenter-ui-audit.zip`（`artifacts/ui-audit/v7-1-final/`，Commit `f6f17a8`）。
