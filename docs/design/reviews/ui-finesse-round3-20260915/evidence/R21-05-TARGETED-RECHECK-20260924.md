# R21-05 禁用与隐藏区别定向复核

日期：2026-09-24  
实现提交：`380234e2`（已有实现）  
复测提交：`1f69b803`（`codex/ui-finesse-round2`）

## 结论

R21-05 的实现已在当前 D 盘工作区历史中存在，本批没有重建生产代码或命令契约，只用当前提交重新生成隔离产物并复跑行为门禁。当前受控条件下仍为“已满足，待环境验证”。

## 当前行为证据

- `R21DisabledHiddenBehaviorTests`：`2/2` 通过。
- `SaveAvailabilitySurfaceSeparatesHiddenHandoffFromReadableReason` 在实际 `SaveCenterView`、生产资源字典和隔离 `WindowHost` 中验证：前置条件不满足时维护跳转按钮为 `Visibility.Collapsed`、不可见且不能取得焦点；说明文本仍可聚焦，并由 Automation Name/HelpText 读回不可用原因。切换到满足条件的合成上下文后，同一按钮恢复可见，原命令可执行且只产生一次合成副作用。
- `DisabledContextActionStaysVisibleAndAutomationInvokeIsRejected` 在实际共享上下文按钮和 WPF AutomationPeer 中验证：动作保持可见但禁用，说明文本保留原因；对 `IInvokeProvider.Invoke()` 的调用抛出 `ElementNotEnabledException`，命令副作用保持为零。
- 夹具先核对真实控件行为，再以生产 XAML 的绑定接线作补充检查；没有把 `Assert.Contains`、`Visibility=Collapsed` 或 HelpText 单独当成交互签收。

## 构建与边界

- D 盘隔离 Release 构建成功：XAML `24/24`，Playnite 目标仍为 `net462`，`0` error；保留 `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs:706` 的两条既有 `CS8602` warning。
- 测试只使用合成可用性上下文、生产视图/资源和隔离 STA WindowHost；没有读取或写入真实存档、媒体、云端、用户诊断，也没有修改当前游戏选框、滚动条、命令绑定、取消/错误/恢复保护或有限列表策略。
- 未运行真实 Playnite/package-host、Windows UIA/读屏、OS 输入/IME、物理 DPI/跨屏、最终 presented frame、ETW 或宿主性能；Demo 原目录不可用，视觉依据仍为已恢复生产基线。隔离 AutomationPeer 不替代上述宿主边界。

下一可执行任务：保留 R23-04 正常可枚举 Playnite 主窗体/UIA 会话作为 P0；若 CEF/窗口暴露条件仍阻塞，则继续账本中依赖已满足且可独立复核的 Q/R 小批量，不重复 R21-05/R22-01，也不绕过系统跟踪权限。
