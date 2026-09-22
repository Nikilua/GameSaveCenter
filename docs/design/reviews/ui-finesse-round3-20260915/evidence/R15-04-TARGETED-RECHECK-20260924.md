# R15-04 重复通知归并定向复核

日期：2026-09-24  
工作区：`D:\\workplace\\github\\GameSaveCenter`  
分支：`codex/ui-finesse-round2`  
当前复核代码身份：`6510ccf6`  
实现提交：`a67d371e`；本批行为边界补测：`6510ccf6`

## 结论

R15-04 已按当前实现和隔离行为结果收口为“已满足，待环境验证”。已有通知去重、会话摘要、通知等级策略和任务历史链路均可复用；本批没有生产代码变更，只补未知失败证据的行为覆盖。

## 已核对的现有能力

- `TaskNotificationDeduper` 只为有稳定任务 ID 的终态领取通知键；Queued/Running 进度不会领取键；相同任务、终态和失败证据只领取一次，新的错误码或新的未知详情仍可领取。
- `SessionNotificationAccumulator` 按稳定任务 ID 替换重复快照，只有达到预期任务数才完成，并用一次性标记防止重复会话摘要；已发会话后的新失败/取消由现有路由分支保留为重要反馈。
- Task Center/任务变更历史继续保存完整失败与事件；Toast/Playnite 通知只是有界摘要，不替代历史详情。命令绑定、取消/错误/恢复保护、游戏选框、滚动条、有限列表和 `net462` 路径未改变。

## 受控验证

- Playnite 核心通知行为：`TaskNotificationDeduperTests 6/6` + `SessionNotificationAccumulatorTests 4/4`，合计 `10/10`；覆盖进度不通知、无任务 ID、相同失败去重、不同错误证据保留、成功/取消独立去重、未知详情变化和会话重复/缺项边界。
- Playnite 相邻回归：`NotificationFeedbackSourceTests`、`R15TaskTimelineTests`、`TaskEventUiBatcherTests`、`R13CloudTransferStageBehaviorTests` 合计与核心套件共 `31/31`；其中 `NotificationFeedbackSourceTests` 是源码契约检查，不作真实 Toast 交互证据。
- Core `NotificationLevelPolicyTests` + `GameSessionSummaryBuilderTests`：`7/7` 通过。
- 当前隔离 Release 构建：XAML `24/24`，solution `0 error / 2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning，Playnite 目标 `net462`；Worker 也在 solution 构建中完成编译。
- `python scripts/validate-source.py`、`scripts/check-xaml.ps1 -ProjectRoot (Get-Location)` 与 `git diff --check` 通过；WPF 静态检查（`src/GameSaveCenter.Playnite`）为 `0 errors / 28 warnings / 162 info`，为仓库既有 StackPanel/Canvas/主题资源启发式提示。

## 视觉与边界

- 本批没有可证明 Toast 已在真实 Playnite 窗口出现的屏幕帧；不把离屏截图、源码断言或通知事件回调写成真实 Toast 排列、读屏播报或最终 presented frame 证据。
- 测试只使用合成 `TaskStatusDto`、fake/内存累加器和隔离 testhost/构建输出，没有写真实存档、媒体、用户云端或诊断，也没有发送外部诊断。Demo 原目录不可用，沿用恢复生产基线。
- 未验真实 Playnite/package-host 的 Toast/系统通知时序、长任务连续失败、UIA/读屏/IME、物理 DPI/跨屏、最终呈现帧、ETW 或宿主性能；完整真实历史仍以 Task Center 为后续核查入口。

下一可执行小批量：`R15-05 任务来源定位`，先核对任务详情已有来源卡片、游戏/版本/媒体批次/云队列稳定身份，以及对象删除或同名对象负例。
