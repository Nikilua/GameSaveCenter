# GameSaveCenter UI Final Polish Plan v7.1

> 来源：`D:\Download\Brave\GameSaveCenter_UI_Final_Polish_Pack_v7_1.zip`
> 依据：当前生产 main（`3b5b046`）、audit7 实际截图/JSON、用户最新反馈截图。
> 事实优先级：当前生产源码 > 用户最新反馈截图 > audit7 > 本包 > 旧 prompt 包。

## 0. 范围与锁定

- 本轮是最后一轮可见问题收尾，不是重新设计产品。
- `REMOVE=0`；不改 ViewModel 业务语义、Worker、IPC、Ludusavi、Rclone、备份恢复、安全机制。
- GamePicker 策略、页面会话/默认页、Overview 大卡与当前游戏卡、Save Policy 表单结构、Maintenance 主/子 Tab 信息架构保持不变。
- 不新增内部 ScrollViewer；不用分辨率 if/else 硬编码；不做纯视觉 patch 而不查根因。

## 1. 首页活动行模板重构（Phase 1，最高优先级）

### Issue
用户最新截图（`ReferenceScreenshots/01_user_activity_row_issue.png`）显示：
- chip（“维护 / 信息”）与 scope（“全局”）在同一个竖向区块上下堆叠；
- 左侧拥挤、中间空旷、时间贴右边框；
- chip 形态/对齐不统一。

### 当前实现
- `src/GameSaveCenter.Playnite/Views/OverviewView.xaml` → `OverviewActivityTimelineList` 行模板。
- 当前六列：`40 | 150 | * | 96 | 84 | 112`。
- 窄窗 Compact 触发器把 Kind/Result 列折叠，并把 `ActivityMetaCompact`（两个 chip）塞回 `ActivityScopeColumn`（Scope 下方），形成“scope 下面堆 chip”。

### 根因
1. 宽模板 chip 虽然独立成列，但窄模板又退化回 scope 下堆叠；
2. Scope 固定 150、Message 虽然 `*` 但没有足够 MinWidth，视觉重心偏左；
3. Time 列 112 且只有 `Margin="12,0,20,0"`，仍被用户认为贴边；
4. 两个 chip 分占两列，中间空隙不稳定。

### 精确修改
- 行模板改为五列语义：`Icon | Scope | Message(*) | MetaChip(Auto) | Time`。
- 宽模板列模型：`40 | 150 | * (MinWidth 140) | 132 | 112`；MetaChip 用一个横向 StackPanel 放 Kind/Result 两个 chip，垂直中心对齐。
- 窄模板（Tag=Compact）只调整列宽：`36 | 130 | * (MinWidth 100) | 112 | 96`，chip 仍在独立 MetaChip 列横向排列，不再放回 Scope 下方；删除 `ActivityMetaCompact` 的 scope 下堆叠路径。
- Time TextBlock 保持 `HorizontalAlignment=Right`，增加 `Margin="8,0,20,0"`，保证右边框留白 ≥20 DIP。
- chip 统一继续使用 `GscRedesignContextPill`（圆角矩形 7、MinHeight 26），Kind/Result 文本保持三向居中。
- Header 行同步五列模型，避免表头与行错位。

### 保留
- `Glyph`、`GameName`、`Summary`、`KindDisplay`、`ResultDisplay`、`CreatedDisplay` 全部绑定；
- hover/选中、ToolTip、TextTrimming、虚拟化/Recycling；
- Overview 主结构、今日工作台、当前游戏卡不重排。

### 验收
- standard / wide / 2K：chip 与 scope 不再同列上下堆叠；Message 为主伸缩列；time 右留白 20 DIP。
- narrow：chip 独立横向列，不退回 scope 下堆叠；time 不裁切。
- 回归断言更新：`UiLayoutRegressionTests` 活动行列模型改为五列语义，并断言 chip 不在 Scope 列内。

## 2. Maintenance 小 clipping / spacing 清扫（Phase 2）

### Issue
audit7 持续出现 INFO `POSSIBLE_CLIPPING`，期望宽度普遍比实际宽 4~16 DIP：

| 路由 | 期望 | 实际 |
| --- | ---: | ---: |
| 诊断概览 | 181 | 169 |
| 设备状态 | 100 | 96 |
| 保留策略 | 71 | 55 |
| 发现的问题 | 96 | 92 |
| 审计记录 | 48 | 44 |
| 进程映射 | 69 | 65 |

### 根因
- 当前 `UiLayoutAnalyzer.AnalyzeClipping` 的警告消息没有元素名，无法直接定位；先给消息补 `element.Name / Text`。
- 常见根因候选：按钮/文本/badge 的 MinWidth 不足、UniformGrid 列被压缩、固定列宽比内容 DesiredWidth 小、`GscRedesignSectionCard` padding 与列宽不匹配。

### 精确修改
- 先增强 Audit：`POSSIBLE_CLIPPING` 消息输出被裁切元素的 `Name / Text / Type`，重新跑 audit7 定位到具体控件。
- 逐项按“MinWidth / Padding / ColumnDefinition / HorizontalAlignment”修正：
  - 诊断概览：检查环境检查卡、指标卡与说明文本的最小宽度。
  - 设备状态：检查摘要卡、状态列与按钮行。
  - 保留策略：检查指标卡 3/4 列与说明文本。
  - 发现的问题 / 审计记录 / 进程映射：检查 DataGrid header/单元格与按钮。
- 不无脑放大所有控件；只给被裁切的元素最小必要宽度/留白。
- 共享 chip / badge / row style 改动后回归 Overview / Save / Task / Media。

### 保留
- Maintenance 全部命令、绑定、Tab 结构、Inspector、虚拟化。
- 4K stretch、列 Fill、表头主题等 v7 成果不回归。

### 验收
- standard / wide / 2K / narrow 下 `POSSIBLE_CLIPPING` 显著减少或清零；
- 新 Audit 警告消息带元素名，可复核。

## 3. 回归与交付（Phase 3）

1. Playnite 测试 `263/263` 基线保持/更新。
2. render-qa 11 档 + 56 主题 + 7 Resize 全绿。
3. `capture-ui-audit.ps1` 重建 `artifacts/GameSaveCenter-ui-audit.zip`（v7.1 最终）。
4. `docs/ai/UI_FINAL_POLISH_REPORT_V7_1.md` 输出 Fixed / Not changed / before-after / tests / manual QA / commit SHA。
5. 每阶段独立 commit。

## 4. 禁止

- 不重排 Overview 大结构、不新增 GamePicker、不动 Save Policy；
- 不删除功能/列/页面块；
- 不为对齐引入新的嵌套滚动；
- 不写死主题色，全部走 DynamicResource/Design Token。
