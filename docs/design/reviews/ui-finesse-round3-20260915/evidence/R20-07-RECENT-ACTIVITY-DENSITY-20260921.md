# R20-07 最近活动密度证据

日期：2026-09-21  
状态：已满足，待真实宿主与外部时序验证  
本阶段：复用现有实现，无生产代码新增  
验证身份：`ee327e80`

## 已有链路核对

R20-07 要求重复进度合并为易扫描的活动摘要，失败与完成仍可追溯详情，首页不被动画或高频事件刷屏，计数来自真实列表状态。本阶段先查最新实现，确认以下能力已经存在：

- Worker `DashboardService` 读取活动审计与近期任务；活动通过 `ActivityTimelineMapper` 映射为有限字段的 `ActivityEntryDto`，按业务类别/结果分层，长消息裁剪到 180 字符并不暴露原始诊断载荷。近期任务合并 active 与 recent 的同一 `TaskId`，按创建时间稳定排序。
- Playnite `TaskEventUiBatcher` 按 `TaskId` 合并高频进度，待处理项上限为 128、单次 UI 批次上限为 32；成功/失败/取消终态绕过进度队列立即落地，迟到的旧进度不会覆盖终态。`DashboardViewModel` 更新 `OverviewTasks` 时只保留最近 8 项，首页 `ListBox` 使用 Recycling 虚拟化和本地滚动，计数绑定实际 `OverviewTasks.Count`。
- 首页任务行保留 `SelectedTask` 绑定，不用高频进度动画刷屏；TaskCenter 选中项提供状态/进度/失败摘要/错误码、可展开技术详情和复制/重试入口。`TaskTimelineBuilder` 只展示已收到的开始、阶段、取消和结束事件，时间线有 220 DIP 的独立滚动上限，不虚构缺失中间事件或重试。

## 验证结果

- Core `ActivityTimelineMapperTests`：`4/4`，覆盖成功备份、云端失败、冲突/仓库修复类别和长消息裁剪。
- Playnite 定向汇总：`TaskEventUiBatcherTests 3/3`、`R15TaskTimelineTests 3/3`、`OverviewInteractionTests 1/1`、`R06SelectionStateBehaviorTests 2/2`、`R06DetailsBudgetBehaviorTests 2/2`，合计 `11/11`。首次把这些 WPF 类放在同一 testhost 时，详情选择项出现 `task-a` 保留的时序失败；单独隔离该类立即 `2/2`，其余类不变，故将它记录为现有 WPF testhost 时序边界，未修改产品实现。
- 同一当前分支 source-copy Release 构建已产出 Playnite `net462` 与 Playnite.Tests `net472`：`0 errors / 2 warnings`，两条均为既有 `MediaCenterView.xaml.cs:671` `CS8602`。`validate-source.py`、XAML `24/24`、`git diff --check` 通过。

## 证据边界

证据使用合成 DTO、fake/隔离 testhost、隔离目录和当前生产 WPF 视图；没有写真实存档、媒体、用户云端或诊断，也没有把“最近 8 项”当作全历史任务计数。未运行真实 Playnite/package-host、Worker/Named Pipe 实时流、真实外部工具、60fps/呈现帧、物理 DPI/跨屏、UIA/读屏、OS 输入/IME、ETW 或宿主性能验证。Demo 原目录不可用，沿用已恢复生产基线；main 用户改动未碰、未合并。

阶段临时目录在文档提交前按精确路径清理；下一可执行任务为 `R20-08 状态语气统一`，先盘点加载/失败/空/完成/需操作的现有文案来源与可复用模板，补正向和负例证据。
