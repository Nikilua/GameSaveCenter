# R08-04 业务完成节奏

## 结论

R08-04 在当前可控验收范围内已满足，生产实现提交为 `3bf90a00`，兼容性说明修正为 `43141399`。先核对已有能力：`BusyOperationCoordinator` 已等待真实 prepare/action 结束后才清除 `IsBusy`；失败和取消分别走独立反馈；Dashboard 的任务通知只对 `Succeeded/Failed/Cancelled` 终态发出，并按同一个 `TaskStatusDto.State` 生成标题、摘要和严重级别。没有用固定延迟把慢请求伪装成成功。

本阶段只补共享视觉与可访问反馈：

- `GameSaveCenter.Playnite.Controls.Button` 保留即时 `IsBusy` 作为命令门禁，新增只读的 `IsBusyIndicatorVisible`。忙碌持续超过 `120ms` 才显示共享 `BusyIndicatorHost`；快速完成会停止计时器并保持折叠，卸载时清理计时器和状态。生产模板改为触发视觉状态，不改变命令、绑定、取消、失败、恢复保护或 `ProgressBar.IsIndeterminate` 业务语义。
- 任务 Toast 使用 net462 可用的 `FeedbackToast` AutomationPeer，设置最终标题/摘要的 `AutomationProperties.Name` 和帮助文本，并通过 Name 属性变更向 UI Automation 暴露非抢焦点反馈。目标框架没有 `LiveRegionChanged`/`LiveSetting` API，因此没有伪造更高框架属性；本证据不把该信号写成 Narrator/真实读屏已验。

## 行为与构建证据

最终重跑显式绑定 `GscBuildCommit=43141399`；隔离源码根为 `r08-04-source-3bf90a00`，输出根为 `r08-04-build-3bf90a00`。完整 solution Release 构建成功：`0 error`，Playnite 目标为 `net462`；8 条 `NU1900` 只表示沙箱无法访问 NuGet 漏洞索引，不影响本地包还原和编译。

当前身份的定向行为测试为 `8/8`：

- `R02BusyStateTests 4/4`：原有真实失败/取消/重复忙态、慢任务忙碌指示器可见且宽度/内容/焦点保持，以及快速完成的负例（等待 `150ms` 仍为 `Collapsed`）。
- `R08BusinessFeedbackBehaviorTests 4/4`：成功/失败/取消三种终态 DTO 均保持不可取消、状态文案与详情一致；实际 `FeedbackToast` AutomationPeer 暴露最终状态文案和 Custom 控件语义。

同一提交身份的 `python scripts/validate-source.py` 通过，`scripts/check-xaml.ps1` 为 `24/24`；`git diff --check` 无输出。测试使用合成 DTO、fake/隔离 WPF Window、STA Dispatcher、offscreen logical DIP 和隔离输出目录，没有写真实存档、媒体、云端或诊断数据。

## 验收边界

已验证慢任务不会提前隐藏忙态，快速任务不会出现一帧 spinner；任务列表/Toast 使用同一终态 DTO。没有启动真实 Playnite 宿主，也没有宣称真实屏幕阅读器、物理 DPI/跨屏、呈现帧、ETW、宿主性能或真实 Worker 长请求时序。Demo 原始目录不可用，沿用当前恢复生产基线；游戏选框、滚动条系统、有限列表虚拟化、命令绑定、取消/错误语义和恢复保护均未改动。

下一可执行任务为 R08-05 页面切换轻量化。
