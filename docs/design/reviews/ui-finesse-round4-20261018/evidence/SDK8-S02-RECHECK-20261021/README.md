# S02-04 至 S02-08 SDK8 当前源码复核

日期：2026-10-21
源码 checkout HEAD：`5a0ca52dd218b080b052073e3f655942773a1044`；构建时工作树含本轮改动。程序集、TRX 与修改文件 SHA-256 见 [验证身份](verification-identity.json)。

## 构建

- 官方入口 `scripts/build.ps1 -Configuration Release -SkipTests` 使用 SDK `8.0.423`，完成 NuGet restore、25 个 XAML 结构检查和完整 Release solution build：`0 warnings / 0 errors`。日志见 [构建](build/build-console.txt) 和 [还原](build/restore-console.txt)，诊断 ID `20261021T030909128Z-cd5facd2`。
- 该命令明确跳过全量测试。以下为按 WPF 类隔离方式运行的 SDK8 定向测试；不代表完整 WPF 测试循环、一键安装或真实 Playnite 已通过。

## S02 定向测试

| 计划项 | SDK8 结果 | TRX |
| --- | --- | --- |
| S02-04 上下文跳转 | 通知视图 2/2、R10 游戏/任务上下文 6/6、Worker 精确 task ID 查询 10/10、R09 焦点 2/2 | [通知视图](tests/notification-inbox-view.trx)、[R10](tests/r10-context-game.trx)、[精确查询](tests/exact-task-query.trx)、[R09](tests/r09-focus-outline.trx) |
| S02-05 临时静音与时段 | 弹窗策略 22/22；便携设置 10/10、设置默认/重启往返 4/4、时段 UI 1/1；R09 2/2 | [策略](tests/notification-popup-policy.trx)、[便携设置](tests/portable-settings.trx)、[设置默认](tests/notification-settings-defaults.trx)、[时段 UI](tests/notification-schedule-ui.trx) |
| S02-06 重要事件例外 | 警告/错误/未知级别策略 22/22；Worker 通知持久化 12/12；R09 2/2 | [策略](tests/notification-popup-policy.trx)、[Worker 持久化](tests/notification-record-persistence.trx) |
| S02-07 重复归并摘要 | Playnite 通知归并、筛选、已读、视图、重试来源及 R09 共 19/19；Worker 通知持久化 12/12 | [归并](tests/notification-aggregation.trx)、[筛选](tests/notification-filter.trx)、[已读操作](tests/notification-read-actions.trx)、[通知视图](tests/notification-inbox-view.trx)、[重试来源](tests/task-retry-source.trx)、[Worker 持久化](tests/notification-record-persistence.trx) |
| S02-08 通知体验 | 计划覆盖子集 20/20：收件箱已读/视图、缓存页返回、受控高对比、任务进度 UIA 与通知去重；另外 R21 UIA/语义值完整行为类 24/24；Worker 12/12 | [已读操作](tests/notification-read-actions.trx)、[通知视图](tests/notification-inbox-view.trx)、[缓存页返回](tests/cached-page-switch.trx)、[受控高对比](tests/controlled-high-contrast.trx)、[UIA/语义值](tests/uia-and-semantic-values.trx)、[任务通知去重](tests/task-notification-deduper.trx)、[Worker 持久化](tests/notification-record-persistence.trx) |

同一份 TRX 会被多个计划项引用，因为 S02-05 至 S02-08 共享策略、持久化和 R09 回归；不是重复触发 CI。

## 宿主边界

当前 SDK8 的 R05 Tooltip 类为 2 项通过、1 项按设计跳过。跳过原因是本 Windows 会话不允许读取系统鼠标位置；用例要求可交互桌面。原始结果在 [R05 TRX](tests/r05-desktop-boundary.trx)。没有绕过输入权限，也没有将离屏测试当成真实屏幕、高对比系统设置或读屏器验收。真实 Playnite 安装、宿主页面呈现和完整默认测试/安装链仍待相应环境验证。
