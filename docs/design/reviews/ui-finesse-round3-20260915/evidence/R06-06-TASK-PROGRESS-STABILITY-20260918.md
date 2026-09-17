# R06-06 行内进度稳定证据

日期：2026-09-18  
任务：`R06-06 | 行内进度稳定`  
结论：在当前可控范围内已满足；代码与行为测试已提交并推送到 `codex/ui-finesse-round2`。

## 1. 现有能力核对

- `TaskStatusDto` 已有 `ProgressPercent`、`ProgressValue` 和 `ProgressDisplay` 显示层：进度条值限制在 0–100，负值和排队状态的默认 0 显示为“—”，不把未知总量伪装成 0%。`StateDisplay` 将取消与成功分开表达。
- `SnapshotComparers.Task` 已比较进度、状态、时间、错误字段；相同快照跳过集合通知。实时事件经 `TaskIndexedCollection.Merge` 按 `TaskId` O(1) 找到已有行并使用索引器替换，产生行级 `Replace`，不是全表 `Reset`；事件路径随后按 ID 回写 `SelectedTask`。
- 任务历史分页的总数和完成汇总来自 `TaskPageDto.Summary`/服务端聚合，不从当前可见页 `Tasks.Count` 反推；因此加载更多、筛选和进度事件不会把“已加载数”误当任务总数。取消请求期间已有 `IsCancellingTask` 防重入语义，本阶段补上任务详情中的独立“正在取消…”提示。
- 原始 Demo `DesignShellView.xaml`/`Pages` 目录在当前 checkout 仍不存在；沿用恢复生产基线，没有把 main 的旧实现覆盖到当前分支。

## 2. 实现

生产代码提交：`72fd6d9aeb74890b86ec11b2ef8e4d3954546803`（`补强任务进度刷新状态`）。行为测试补强：`fd9326d7`（`补充任务取消状态测试`）。

- `TaskCenterView.xaml` 在复制/重试/取消动作之间加入 `TaskCancellationStatusText`。它默认折叠，只在既有 `DashboardViewModel.IsCancellingTask` 为 `True` 时显示“正在取消…”，并提供兼容 net462 的 `AutomationProperties.HelpText`；没有改变取消命令、确认、Worker 请求、最终状态或安全收尾语义。
- 没有改写 `TaskStatusDto`、进度计算、任务分页协议或列表滚动模型；行进度继续绑定 `ProgressValue`/`ProgressDisplay`，详情继续绑定 `SelectedTask` 的同一显示层。
- `R06TaskProgressBehaviorTests` 使用合成任务和隔离 STA WPF：真实 `DataGrid` 在 80 行中滚动到第 42 行后，进度更新只产生一个 `Replace`、无 `Reset`，按 ID 回写选择后 `SelectedIndex`、任务 ID、逻辑滚动偏移和行数保持；生产 `TaskCenterView` 实例实际验证取消提示由折叠变为可见、文本与 HelpText 正确。

## 3. 自动验证

- `R06TaskProgressBehaviorTests`：`4/4`。
  - 进度替换不重置选择/滚动，集合事件为 `Replace` 而非 `Reset`。
  - 排队 0、运行中负值、有效 0、超界 120 和取消/成功状态边界符合现有显示契约。
  - TaskCenter 行/详情均使用 `ProgressValue`/`ProgressDisplay`，没有直接把原始 `ProgressPercent` 当 UI 百分比；总数仍绑定服务端 `TaskSummary.TotalCount`。
  - 取消中提示在真实生产视图中按 `IsCancellingTask` 变化。
- 相邻回归：`TaskIndexedCollectionTests 4/4`、`BatchObservableCollectionTests 3/3`、`R03NumericAlignmentTests 10/10`、`R06SelectionStateBehaviorTests 2/2`、`R06SortingBehaviorTests 4/4`。
- `scripts/check-xaml.ps1`：`24/24`；`python scripts/validate-source.py`、`git diff --check` 通过。
- 标准 Release 构建：`0 warning / 0 error`；Release/net472 `R06TaskProgressBehaviorTests`：`4/4`。

## 4. 视觉与布局证据

渲染证据：`.tmp/r06-06-render-final/render-qa-report.txt`。

- 报告绑定生产代码提交 `72fd6d9aeb74890b86ec11b2ef8e4d3954546803`，`WorkingTreeClean=True`，Light/Dark 双主题共 357 张 PNG，结果为 `render-qa OK`。
- 覆盖任务页 1040×700、1100×720、1366×768、2560×1440 DIP，50/400/2000/4468 数据量、纵向/横向滚动、虚拟化和 `2560×1440 → 1100×720 → 2560×1440` resize；任务表在代表尺寸保持至少 4 行可读，任务工作区的有限滚动和详情区未被提示挤坏。
- 已人工抽查 `theme/light/Task-1040x700.png`、`theme/dark/Task-1040x700.png`：进度条与百分比列、成功/取消/失败徽章、错误行和紧凑详情入口均可读，浅深主题层级保持。
- `DpiScale=1.00` 仅代表离屏 logical DIP；截图不证明真实 presented frame、物理 DPI 或跨屏行为。

## 5. 未验边界与下一步

本阶段未启动真实 Playnite 宿主，也未验真实 Worker 长任务、取消请求与终态竞争、宿主 UIA/读屏、OS 输入/IME、物理 DPI/跨屏、presented frame、ETW、宿主性能或实际帧率。测试使用合成任务、隔离 STA WPF 和离屏 logical DIP；没有把 `TaskSummary` 的服务端聚合验证写成真实数据库/宿主运行证明。未写真实存档、媒体、云端或诊断数据。

`.tmp/r06-06-render-final` 被本证据引用并保留；本阶段没有其他需保留的一次性 artifacts/.tmp 产物。

下一可执行任务：`R06-07 空表保留结构`，先盘点 Task/Save/Media/Maintenance 各类首次空、筛选空、全部处理完和读取失败状态的现有 presenter 与恢复命令。
