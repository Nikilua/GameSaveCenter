# R21-04 异步完成播报证据

日期：2026-09-21。行为夹具提交：`f6c9619c`。实现复用：`a67d371e`、`882638ea`。

## 实施范围

- 先复用现有 `TaskEventUiBatcher`、`TaskNotificationDeduper`、`SessionNotificationAccumulator`、`NotificationLevelPolicy`、Dashboard `FeedbackToast` 和 Task Center 的 `TaskPageStatusSummary`，没有新建通知服务、DTO、任务历史或第二条 Worker 数据源。
- 运行中进度由 `TaskEventUiBatcher` 按任务合并并限制为最多 128 条待处理变化、每批最多 32 条；终态变化即时旁路，且会移除同任务的旧进度，完成/失败不会被后续进度挤掉。
- 任务完成、失败和取消只对终态领取通知键；相同任务与相同失败证据不重复播报，不同错误码/未知错误证据仍可见。会话任务在预计数量齐全后只发一次摘要，任务中心历史仍保留可再次读取的详情。
- Toast 是不抢键盘焦点的页面本地 `FeedbackToast`，Automation Name/HelpText 携带最终状态与“以任务中心记录为准”的回读入口；任务页加载结束由绑定的 `TaskPageStatusSummary` 更新为最近更新时间，加载失败保留旧数据并保留重试入口。

## 行为与构建证据

- 新增 `R21AsyncCompletionAnnouncementBehaviorTests` `2/2`：实际 WPF 窗口中验证终态 Toast 的 Automation 文本、非焦点属性和播报前后键盘焦点不变；实际 `TaskCenterView` 验证加载态文本更新为最近更新时间。
- 相邻运行时行为合计 `22/22`：`TaskEventUiBatcher` 进度合并/容量边界/终态旁路/卸载取消，`TaskNotificationDeduper` 重复终态与不同失败证据，`SessionNotificationAccumulator` 会话去重，以及既有 R08 Toast peer、R07 任务失败保留旧数据边界均通过。
- Core `NotificationLevelPolicyTests` `5/5`；Worker `TaskEventBroadcasterTests` `5/5`。D 盘隔离 Release solution（Playnite `net462` / Tests `net472`）`0 warning / 0 error`；XAML `24/24`、`validate-source.py`、`git diff --check` 通过；WPF 静态审查 `0 errors / 27 warnings / 162 info`，提示均为既有 XAML/资源启发式项。
- 合并相邻旧测试时仍捕获既有 `TaskCenterViewResponsiveTests.FailedTaskDetailsPutUserReasonBeforeCollapsedTechnicalDetails`：`28 passed / 1 failed`，失败是旧源码断言期待 `SelectedTask.ErrorMessage` 直接绑定；本批未改它、未将它记为通过。R21-04 自身及上述定向门禁未受该失败影响。

## 边界与下一步

- 证据使用合成 DTO、fake/隔离 testhost、隔离目录和 STA WPF；未运行真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终 presented frame、ETW 或宿主性能。没有写真实存档、媒体、云端或外发诊断。Demo 原目录不可用，沿用恢复生产基线。
- main 上的用户改动、`src.zip` 和 R08 未提交文件未触碰、未合并；链接工作树 `_wpftmp.csproj` 的 `Access denied` 未绕过。

本项在受控范围内“已满足，待环境验证”。下一可执行小批量为 `R21-05 禁用与隐藏区别`：先核对真实命令的禁用/折叠/解释映射与键盘、读屏可达性。
