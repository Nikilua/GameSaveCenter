# R08-04 当前 main 业务完成节奏复核

日期：2026-10-14
测试源码：本轮只改进测试，生产逻辑复用当前 main。构建时 checkout HEAD 为 `37bd3c97`；新测试源已通过本轮文件 SHA 固定，详见 `source-hashes.txt`。
本机 SDK：`9.0.302`；MSBuild `17.14.13`；VSTest `17.14.1`；.NET Framework `4.8.9181.0`。仓库 `global.json` 锁定 `8.0.423`，本机未安装；本结果不代替 SDK8 基线或 CI SDK `10.0.401`。

## 当前实现与补充行为

复用 `BusyOperationCoordinator`、共享生产按钮 `IsBusy`、`FeedbackToast` AutomationPeer、`DashboardView.ApplyToastAutomation` 和 `TaskStatusDto`。未改生产命令、绑定、业务状态、取消/错误语义或按钮模板。

补充的正向行为断言以未完成的 `TaskCompletionSource` 驱动成功请求：action 未结束时 operation 仍未完成、忙态保持、结束回调为零次；放行 action 后忙态只切换一次回普通状态，结束回调恰好一次，失败/取消回调均未触发。终态 DTO 测试改为精确比较成功、失败（错误码和错误详情覆盖阶段消息）及取消的 `StateDisplay` 与 `DetailMessage`。

当前测试还覆盖：重复操作不启动第二次；失败与取消在 action 完成后恢复忙态并只完成一次；生产按钮慢任务期间宽度、内容和键盘焦点不变，超过 spinner 延迟后才显示指示器；快速结束后等待 150ms 指示器仍折叠；反馈 AutomationPeer 提供最终状态 Name/HelpText 和 Custom 控件语义。

## 本机结果

- 完整 Release solution build（隔离输出）成功：`0 warning / 0 error`；Playnite `net462`，测试 `net472`。程序集 SHA/MVID/ProductVersion 见 `tests/assemblies.json`。
- `R02BusyStateTests`：`5/5`，0 failed/skipped。
- `R08BusinessFeedbackBehaviorTests`：`4/4`，0 failed/skipped。合计 `9/9`。
- `scripts/validate-source.py` 通过；`git diff --check` 通过。
- WPF 技能静态检查：`0 errors / 29 warnings / 177 info`。未修改 XAML；扫描同时发现 `.tmp/r12-04-stage` 内一条与本阶段无关的历史布局 warning，未据此改动其他任务文件。

第一次无 `GSC_BUILD_COMMIT` 的隔离构建虽成功，但一个源码契约测试按 `TestRepositoryContext` 设计拒绝读取无身份程序集；设置当前 HEAD 元数据后重新构建，两个完整定向类均通过。该初次运行不是产品行为失败，也不计入 `9/9`。

## 证据边界与 CI

测试使用合成 DTO、`TaskCompletionSource`、fake 状态和隔离 STA WPF 窗口；按钮几何是逻辑 DIP。没有运行真实 Playnite host/Worker 长任务、Narrator 或外部 UI Automation 客户端，也没有验证自动通知的真实宿主时序、物理 DPI、屏幕呈现帧、ETW 或宿主性能。未访问真实存档、媒体、云端或诊断数据。

截至记录时，公开 [Actions 列表](https://github.com/Nikilua/GameSaveCenter/actions) 仍只显示到 #979 queued，没有显示 `31d64e42`、`37bd3c97` 或本轮提交的 run；#971/run `38393317198` 页面仍为 In progress，#978/run `38509928015` 为 Failure，完整日志需要 GitHub 登录。此页面不能给出当前提交的 SDK8/SDK10 终态；本机 SDK9 结果不可替代 CI。推送本轮提交后继续查两个 SDK lane，CI 终态前不报阶段完全收口。

旧复核：[2026-09-24 R08-04 证据](../R08-04-CURRENT-MAIN-RECHECK-20260924.md)。下一项：R08-05 页面切换轻量化。
