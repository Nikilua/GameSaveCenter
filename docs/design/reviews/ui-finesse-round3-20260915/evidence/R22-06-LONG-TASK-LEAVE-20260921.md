# R22-06 长任务离页提示（2026-09-21）

## 结论

本批按“已满足，待环境验证”收口。实现提交为 `ef671288`（`补充任务离页提示`），已推送到 `origin/codex/ui-finesse-round2`。

## 现状核对与实现

- 先核对现有真实执行链：后台任务由 Worker 执行，工作区切换只改变生产页面宿主；`DashboardViewModel` 的任务事件连接是可选实时通道，任务快照/历史轮询仍是断线、Worker 重启和页面重新打开后的事实来源。显式取消仍只通过 `CancelTaskCommand`/Worker 取消协议触发。
- Dashboard 卸载时会停止事件订阅、取消 UI 延迟加载和隐藏监听，但没有调用取消任务命令；重新打开时重新订阅事件，并通过任务页面加载恢复任务状态、真实阶段与进度。生产 Shell 缓存 `TaskCenterView`，工作区切换不会重建页面，任务选中项和表格滚动保持原语义。
- 在任务页筛选栏增加一条 Demo 基线内的轻量说明：“离开本页不会取消后台任务；返回任务中心后会从任务记录恢复阶段和进度。” 同步设置 Automation Name/HelpText；没有新增计时器、模拟进度或自动取消路径。
- 保留现有终态 Toast、失败/取消详情、重试、取消等待状态、虚拟化 DataGrid 和滚动条；离页提示不能替代真实任务终态通知。

## 验证证据

- `R22LongTaskLeavePageBehaviorTests 2/2`：隔离 STA WPF 实际读取提示文本、可见性、Automation Name/HelpText；源码负例确认 Dashboard 卸载只停止展示/事件监听而不调用 `CancelTask`，任务订阅仍连接事件并保留正常轮询回退，任务页仍绑定真实 `StageDisplay`、`ProgressDisplay` 和显式 `CancelTaskCommand`。
- 相邻 `R21AsyncCompletionAnnouncementBehaviorTests 2/2`：终态 Toast 不抢焦点，任务页加载完成后替换可读状态面。生产工作区缓存切换的 R08 第一场景通过，验证 TaskCenter 页面实例、选择和滚动在离开/返回后保留。
- 使用提交身份 `ef671288` 的隔离 Release 编译：Playnite `net462`、Tests `net472` 无错误；保留既有 `MediaCenterView.xaml.cs:699 CS8602` 两条 warning。`validate-source.py`、`git diff --check` 通过；WPF 检查为 `0 errors / 27 warnings / 177 info`。R21 WPF testhost 退出时输出既有 TextServices COM 清理异常文本，但测试结果为进程成功、`2/2`，未将该清理输出写成业务失败。

## 未验边界

没有启动真实 Playnite/package-host、真实 Worker 长任务或实机离页/返回；没有把隔离 Window、缓存页面和 testhost 状态写成最终呈现、真实 UIA/读屏、键盘、DPI/物理跨屏、系统休眠/重启恢复、ETW 或宿主性能证据。Demo 原目录不可用，视觉继续沿用已恢复生产基线。main 用户改动未碰、未合并，未访问真实存档、媒体、云端或外发诊断。

R08 的另一个旧基线测试仍因生产 Shell 文本契约漂移而失败（要求旧 `PageHost.Content` 字符串），本批不改写；它不影响本项的独立测试结论。

## 下一步

下一可执行项为 `R22-07` 确认框信息结构：先盘点真实危险动作确认内容、按钮名称、取消默认和 Enter 行为，再选一个边界明确的小批量补负例，不改变恢复保护和取消语义。
