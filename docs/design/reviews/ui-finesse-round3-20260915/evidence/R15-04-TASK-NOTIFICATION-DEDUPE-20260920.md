# R15-04 重复通知归并证据

日期：2026-09-20  
代码提交：`a67d371e 收口重复任务通知`  
分支：`codex/ui-finesse-round2`

## 实际完成

- 先核对现有 `BoundedTaskIdSet`、`SessionNotificationAccumulator`、`NotificationLevelPolicy`、`GameSessionSummaryBuilder`、Task Center 历史和 Dashboard Toast；没有另建通知服务、任务历史或新的数据源。
- `TaskNotificationDeduper` 只为有稳定任务 ID 的终态领取通知键。Queued/Running 等进度事件不领取键；相同任务、相同终态和相同失败证据只领取一次；同一任务出现不同错误码或不同未知错误证据时保留新的重要失败通知。既有任务 ID 账本继续保留，兼容源码门禁和历史监测语义。
- 会话任务继续由 `SessionNotificationAccumulator` 合并并在预计任务齐全后发一次会话摘要；同一会话摘要已发出后，后续新的失败/取消不再被会话分支静音。已发会话 ID 也使用有界集合，不增长为无界缓存。
- Task Center/任务变更历史仍保存全部任务错误与事件；Toast 只是节制的摘要反馈，不替代历史详情。命令绑定、取消/错误/恢复保护、游戏选框、滚动条、有限列表和 net462 路径保持。

## 证据

- `python scripts/validate-source.py`：通过。
- `scripts/check-xaml.ps1 -ProjectRoot (Get-Location)`：24/24 通过。
- `git diff --check`：通过。
- WPF 技能静态审查：0 errors / 28 warnings / 177 info；本批未改 XAML，提示为现有 XAML/资源启发式提示。
- 外部源码副本（带当前 linked-worktree `.git` 身份）构建 Playnite Tests 项目，包含 Playnite `net462`：0 errors，保留 `MediaCenterView.xaml.cs:664` 的 2 条既有 `CS8602` warning。
- 通知去重、会话累加、通知反馈、任务时间线、R13 云端相邻夹具定向测试：28/28 通过，0 failed，0 skipped。行为负例包含进度更新不领取通知、相同失败证据不重复、无任务 ID 不领取；正例包含同任务不同错误证据和摘要后的新重要失败仍可见。
- 本批 `r15-04-source`、`r15-04-playnite-tests`、`r15-04-playnite-tests-copy`、`r15-04-playnite-tests-final` 四个隔离目录已清理。

## 边界与未验

- linked worktree 直接触发 WPF `_wpftmp.csproj` 时仍遇 `Access denied`；按已有流程改用当前分支的外部源码副本完成项目级构建，没有把该阻塞写成完整 solution 通过，也没有绕过系统权限。
- 本批没有改 Worker 项目，因此未重复构建 Worker；没有宣称真实 Playnite/package-host、真实任务长时序、最终 presented frame、物理 DPI/跨屏、UIA/读屏、IME、ETW 或宿主性能通过。
- 测试只使用合成 DTO、fake/内存状态和隔离构建目录；未读取/写入真实存档、媒体、用户云端或诊断，未发送外部诊断。Demo 原目录不可用，沿用恢复生产基线。
- main 的 `DashboardView.xaml.cs`、对话框基础设施和 `src.zip` 未触碰、未合并。

下一可执行任务：`R15-05 任务来源定位`，先查现有任务详情、稳定游戏/版本/媒体批次/云队列身份和删除对象负例。
