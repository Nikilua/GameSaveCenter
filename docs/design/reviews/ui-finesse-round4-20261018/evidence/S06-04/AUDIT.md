# S06-04 续接上下文能力审计

日期：2026-10-21
审计基线：`43b869d13e7b62a5af319aa9c3cfdaf33a81dfc9`

## 计划验收

重启后能从已保存清单回到对应页面对象并带入必要筛选；不会恢复过期对话框或自动重放命令；对象消失时给出诊断并允许移除。

## 现有能力核对

- `GameSaveCenterSettings.Worklists` 和 `ActiveWorklistId` 持久化清单、条目及当前清单选择；`WorklistDefinition` 只记录对象种类、稳定 ID、备注和人工完成状态。
- `DashboardViewModel.WorklistBrowser` 提供打开/关闭、清单选择、搜索、分组、重排、解析状态刷新和移除命令，没有“打开所选引用”或返回目标对象的导航命令。
- `WorklistBrowserEntry` 没有页面目标、筛选状态或对话框数据。现有条目可在重启后继续显示，但点击/执行没有从清单返回业务对象的路径。
- `TaskSourceNavigationResolver` 按精确任务/游戏/备份 ID 解析对象；`WorkspaceNavigationState` 与 `DashboardViewModel.Navigation` 保存当前进程内的返回状态，不持久化为清单上下文，也不由工作清单条目驱动。
- S06-03 的 Worker 查询只给出索引引用状态，不提供业务页路由或详情 DTO；它不能替代导航恢复。

## 结论与准入范围

未发现等价能力；S06-03 已验收，稳定对象 ID 及保守失效状态已具备，S06-04 可进入 READY。

实施应为选中条目提供显式续接动作，按对象种类与稳定 ID 导航至对应页面并应用完成定位所需的最小筛选；不得按名称/路径猜测，不恢复旧对话框或执行命令。对象无法解析时保留在清单页，展示具体失效/无法检查原因，并允许用户移除该引用；其余条目继续可浏览。对话框、恢复、备份、重试、校验等业务操作仍须用户重新发起。

## 证据位置

- `src/GameSaveCenter.Playnite/Settings/GameSaveCenterSettings.cs`：持久化的工作清单与活动清单 ID。
- `src/GameSaveCenter.Playnite/Settings/WorklistDefinition.cs`：清单引用字段及有界归一化。
- `src/GameSaveCenter.Playnite/ViewModels/DashboardViewModel.WorklistBrowser.cs`：当前浏览器命令及条目解析。
- `src/GameSaveCenter.Playnite/ViewModels/TaskSourceNavigationResolver.cs`：现有精确 ID 解析能力。
- `src/GameSaveCenter.Playnite/ViewModels/WorkspaceNavigationState.cs`：会话内页面返回状态。

审计只读取代码，不改产品行为、不触发构建。下一小批为 S06-04 续接上下文。
